using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using System.Globalization;

namespace BossMod;

public sealed partial class ModuleViewer(PlanDatabase? planDB, WorldState ws) : IDisposable
{
    private readonly struct ModuleInfo
    {
        public readonly BossModuleRegistry.Info Info;
        public readonly int SortOrder;
        public readonly string DisplayName;
        public readonly string EnableID;
        public readonly string ConfigID;
        public readonly string PlansID;
        public readonly string PopupID;
        public readonly string HelpText;

        public ModuleInfo(BossModuleRegistry.Info info, string name, int sortOrder)
        {
            Info = info;
            SortOrder = sortOrder;

            var typeName = info.ModuleType.FullName ?? info.ModuleType.Name;
            DisplayName = $"{name} [{info.ModuleType.Name}]";
            EnableID = $"##enable-module-{info.PrimaryActorOID:X8}";
            ConfigID = $"{typeName}_cfg";
            PlansID = $"{typeName}_plans";
            PopupID = $"{typeName}_popup";
            HelpText = BuildModuleHelpText(info);
        }
    }

    private readonly struct ModuleGroupInfo(string name, uint id, uint sortOrder, uint icon = default) : IEquatable<ModuleGroupInfo>
    {
        public readonly string Name = name;
        public readonly uint Id = id;
        public readonly uint SortOrder = sortOrder;
        public readonly uint Icon = icon;

        public static bool operator ==(ModuleGroupInfo left, ModuleGroupInfo right) => left.Id == right.Id;
        public static bool operator !=(ModuleGroupInfo left, ModuleGroupInfo right) => left.Id != right.Id;

        public readonly bool Equals(ModuleGroupInfo other) => this == other;
        public override readonly bool Equals(object? obj) => obj is ModuleGroupInfo other && Equals(other);
        public override readonly int GetHashCode() => (Name, Id, SortOrder, Icon).GetHashCode();
    }

    private sealed class ModuleGroup(ModuleGroupInfo info, int expansion, int category)
    {
        public readonly ModuleGroupInfo Info = info;
        public readonly List<BossModuleRegistry.Info> ModuleInfos = [];
        public readonly List<uint> ModuleOIDs = [];
        public readonly List<uint> NonDummyModuleOIDs = [];
        public readonly string EnableID = $"##enable-group-{expansion}-{category}-{info.Id:X8}";
        public readonly string NodeLabel = $"{info.Name}###{expansion}/{category}/{info.Id}";
        public List<ModuleInfo>? Modules;
    }

    private readonly struct GroupClassificationKey(BossModuleInfo.Expansion expansion, BossModuleInfo.Category category, BossModuleInfo.GroupType groupType, uint groupID)
    {
        public readonly BossModuleInfo.Expansion Expansion = expansion;
        public readonly BossModuleInfo.Category Category = category;
        public readonly BossModuleInfo.GroupType GroupType = groupType;
        public readonly uint GroupID = groupID;
    }

    private readonly PlanDatabase? _planDB = planDB;
    private readonly WorldState _ws = ws; // TODO: reconsider...
    private readonly BossModuleConfig _moduleConfig = Service.Config.Get<BossModuleConfig>();

    private BitMask _filterExpansions;
    private BitMask _filterCategories;

    private (string name, uint icon)[]? _expansions;
    private (string name, uint icon)[]? _categories;
    private uint _iconFATE;
    private uint _iconHunt;
    private List<ModuleGroup>?[,]? _groups;
    private bool _initialized;
    private readonly Vector2 _iconSize = new(30f, 30f);

    private (string name, uint icon)[] Expansions => _expansions!;
    private (string name, uint icon)[] Categories => _categories!;
    private List<ModuleGroup>?[,] Groups => _groups!;

    private string _searchText = "";

    private void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        _expansions = new (string, uint)[(int)BossModuleInfo.Expansion.Count];
        _categories = new (string, uint)[(int)BossModuleInfo.Category.Count];
        const uint defaultIcon = 61762u;
        var expansionNames = GeneratedEnumMetadata.Names<BossModuleInfo.Expansion>();
        for (var i = 0; i < (int)BossModuleInfo.Expansion.Count; ++i)
        {
            Expansions[i] = (expansionNames[i], defaultIcon);
        }

        var categoryNames = GeneratedEnumMetadata.Names<BossModuleInfo.Category>();
        for (var i = 0; i < (int)BossModuleInfo.Category.Count; ++i)
        {
            Categories[i] = (categoryNames[i], defaultIcon);
        }

        var exVersion = Service.LuminaSheet<ExVersion>()!;
        Customize(BossModuleInfo.Expansion.RealmReborn, 61875u, exVersion.GetRow(0u).Name);
        Customize(BossModuleInfo.Expansion.Heavensward, 61876u, exVersion.GetRow(1u).Name);
        Customize(BossModuleInfo.Expansion.Stormblood, 61877u, exVersion.GetRow(2u).Name);
        Customize(BossModuleInfo.Expansion.Shadowbringers, 61878u, exVersion.GetRow(3u).Name);
        Customize(BossModuleInfo.Expansion.Endwalker, 61879u, exVersion.GetRow(4u).Name);
        Customize(BossModuleInfo.Expansion.Dawntrail, 61880u, exVersion.GetRow(5u).Name);

        var contentType = Service.LuminaSheet<ContentType>()!;
        Customize(BossModuleInfo.Category.Dungeon, contentType.GetRow(2u));
        Customize(BossModuleInfo.Category.Trial, contentType.GetRow(4u));
        Customize(BossModuleInfo.Category.Raid, contentType.GetRow(5u));
        Customize(BossModuleInfo.Category.Chaotic, contentType.GetRow(37u));
        Customize(BossModuleInfo.Category.PVP, contentType.GetRow(6u));
        Customize(BossModuleInfo.Category.Quest, contentType.GetRow(7u));
        Customize(BossModuleInfo.Category.FATE, contentType.GetRow(8u));
        Customize(BossModuleInfo.Category.TreasureHunt, contentType.GetRow(9u));
        Customize(BossModuleInfo.Category.GoldSaucer, contentType.GetRow(19u));
        Customize(BossModuleInfo.Category.DeepDungeon, contentType.GetRow(21u));
        Customize(BossModuleInfo.Category.Quantum, contentType.GetRow(21u), "Quantum");
        Customize(BossModuleInfo.Category.Ultimate, contentType.GetRow(28u));
        Customize(BossModuleInfo.Category.VariantCriterion, contentType.GetRow(30u));
        Customize(BossModuleInfo.Category.HallOfTheNovice, contentType.GetRow(20u), "Hall of the Novice");
        Customize(BossModuleInfo.Category.CrucibleOfTheUnbroken, contentType.GetRow(40u), "Beastmaster");

        var playStyle = Service.LuminaSheet<CharaCardPlayStyle>()!;
        Customize(BossModuleInfo.Category.Foray, playStyle.GetRow(6u));
        Customize(BossModuleInfo.Category.MaskedCarnivale, playStyle.GetRow(8u));
        Customize(BossModuleInfo.Category.Hunt, playStyle.GetRow(10u));

        Categories[(int)BossModuleInfo.Category.Extreme].icon = Categories[(int)BossModuleInfo.Category.Trial].icon;
        Categories[(int)BossModuleInfo.Category.Unreal].icon = Categories[(int)BossModuleInfo.Category.Trial].icon;
        Categories[(int)BossModuleInfo.Category.Savage].icon = Categories[(int)BossModuleInfo.Category.Raid].icon;
        Categories[(int)BossModuleInfo.Category.Alliance].icon = Categories[(int)BossModuleInfo.Category.Raid].icon;
        //Categories[(int)BossModuleInfo.Category.Event].icon = GetIcon(61757);

        _iconFATE = contentType.GetRow(8u).Icon;
        _iconHunt = (uint)playStyle.GetRow(10u).Icon;

        _groups = new List<ModuleGroup>?[(int)BossModuleInfo.Expansion.Count, (int)BossModuleInfo.Category.Count];
        var groupInfoCache = new Dictionary<GroupClassificationKey, ModuleGroupInfo>();
        var groupCache = new Dictionary<(int Expansion, int Category, uint ID), ModuleGroup>();

        foreach (var info in BossModuleRegistry.RegisteredModules.Values)
        {
            var expansion = (int)info.Expansion;
            var category = (int)info.Category;
            var classificationKey = GroupKey(info);
            if (!groupInfoCache.TryGetValue(classificationKey, out var groupInfo))
            {
                groupInfo = ClassifyGroup(info);
                groupInfoCache.Add(classificationKey, groupInfo);
            }

            var groupKey = (expansion, category, groupInfo.Id);
            if (!groupCache.TryGetValue(groupKey, out var group))
            {
                group = new(groupInfo, expansion, category);
                (Groups[expansion, category] ??= []).Add(group);
                groupCache.Add(groupKey, group);
            }
            else if (group.Info != groupInfo)
            {
                Service.Log($"[ModuleViewer] Group properties mismatch between {groupInfo} and {group.Info}");
            }

            group.ModuleInfos.Add(info);
            group.ModuleOIDs.Add(info.PrimaryActorOID);
            if (info.Maturity != BossModuleInfo.Maturity.Dummy)
            {
                group.NonDummyModuleOIDs.Add(info.PrimaryActorOID);
            }
        }

        for (var i = 0; i < (int)BossModuleInfo.Expansion.Count; ++i)
        {
            for (var j = 0; j < (int)BossModuleInfo.Category.Count; ++j)
            {
                var groups = Groups[i, j];
                if (groups == null)
                {
                    continue;
                }

                groups.Sort(static (a, b) => a.Info.SortOrder.CompareTo(b.Info.SortOrder));
                var groupsSpan = CollectionsMarshal.AsSpan(groups);
                var count = groups.Count;
                var countAdj = count - 1;
                for (var g = 0; g < countAdj; ++g)
                {
                    var g1 = groupsSpan[g];
                    var g2 = groupsSpan[g + 1];
                    if (g1.Info.SortOrder == g2.Info.SortOrder)
                    {
                        Service.Log($"[ModuleViewer] Same sort order between groups {g1.Info} and {g2.Info}");
                    }
                }

                for (var g = 0; g < count; ++g)
                {
                    var group = groupsSpan[g];
                    var moduleInfos = group.ModuleInfos;
                    moduleInfos.Sort(static (a, b) => a.SortOrder.CompareTo(b.SortOrder));

                    var moduleCount = moduleInfos.Count;
                    var moduleInfosSpan = CollectionsMarshal.AsSpan(moduleInfos);
                    for (var m = 0; m + 1 < moduleCount; ++m)
                    {
                        ref readonly var m1 = ref moduleInfosSpan[m];
                        ref readonly var m2 = ref moduleInfosSpan[m + 1];
                        if (m1.SortOrder == m2.SortOrder)
                        {
                            Service.Log($"[ModuleViewer] Same sort order between modules {m1.ModuleType.FullName} and {m2.ModuleType.FullName}");
                        }
                    }
                }
            }
        }

        _initialized = true;
    }

    public void Dispose() { }

    public void Draw(UITree tree, WorldState ws)
    {
        EnsureInitialized();

        var availWidth = ImGui.GetContentRegionAvail().X;
        var filterWidth = 300f; // Fixed width for filter panel
        var moduleWidth = availWidth - filterWidth - ImGui.GetStyle().ItemSpacing.X;

        using (var child = ImRaii.Child("FiltersPanel", new Vector2(filterWidth, 0), true))
        {
            if (child)
            {
                DrawFilters();
            }
        }

        ImGui.SameLine();
        using (var child = ImRaii.Child("ModulesPanel", new Vector2(moduleWidth, 0), true))
        {
            if (child)
            {
                DrawModules(tree, ws);
            }
        }
    }

    private void DrawFilters()
    {
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted("Search:");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(-1f);
        DrawSearchBar();

        ImGui.Spacing();
        DrawExpansionFilters();

        ImGui.Spacing();
        DrawContentTypeFilters();
    }

    private void DrawSearchBar()
    {
        ImGui.InputTextWithHint("##search", "e.g. \"Ultimate\"", ref _searchText, 100, ImGuiInputTextFlags.CallbackCompletion);

        if (ImGui.IsItemHovered() && !ImGui.IsItemFocused())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Type here to search for any specific instance by its respective title.");
            ImGui.EndTooltip();
        }
    }

    private static float EnabledColumnWidth()
    {
        var style = ImGui.GetStyle();
        return ImGui.CalcTextSize("Enabled").X + style.CellPadding.X * 2f + style.FramePadding.X * 2f;
    }

    private static void CenterEnableCheckbox()
    {
        var available = ImGui.GetContentRegionAvail().X;
        var checkboxWidth = ImGui.GetFrameHeight();
        if (available > checkboxWidth)
        {
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (available - checkboxWidth) * 0.5f);
        }
    }

    private void DrawExpansionFilters()
    {
        using var table = ImRaii.Table("ExpansionFilters", 2, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp);
        if (!table)
        {
            return;
        }

        ImGui.TableSetupColumn("Enabled", ImGuiTableColumnFlags.WidthFixed, EnabledColumnWidth());
        ImGui.TableSetupColumn("##showExpac", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();

        for (var e = BossModuleInfo.Expansion.RealmReborn; e < BossModuleInfo.Expansion.Count; ++e)
        {
            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            var (anyEnabled, allEnabled) = _moduleConfig.ExpansionEnabledState(e);
            var enabled = allEnabled;
            var mixed = anyEnabled && !allEnabled;
            if (mixed)
            {
                ImGuiP.PushItemFlag(ImGuiItemFlags.MixedValue, true);
            }
            CenterEnableCheckbox();
            var changed = ImGui.Checkbox($"##enable-expansion-{e}", ref enabled);
            if (mixed)
            {
                ImGuiP.PopItemFlag();
            }
            if (changed)
            {
                _moduleConfig.SetExpansionEnabled(e, enabled);
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(anyEnabled != allEnabled ? "Some modules in this expansion are disabled. Click to enable all." : enabled ? "Disable all modules in this expansion." : "Enable all modules in this expansion.");
            }

            ImGui.TableNextColumn();
            ref var expansion = ref Expansions[(int)e];
            UIMisc.ImageToggleButton(Service.Texture?.GetFromGameIcon(expansion.icon), _iconSize, !_filterExpansions[(int)e], expansion.name);
            if (ImGui.IsItemClicked())
            {
                _filterExpansions.Toggle((int)e);
            }
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                _filterExpansions = ~_filterExpansions;
                _filterExpansions.Toggle((int)e);
            }
        }
    }

    private void DrawContentTypeFilters()
    {
        using var table = ImRaii.Table("ContentFilters", 2, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp);
        if (!table)
        {
            return;
        }

        ImGui.TableSetupColumn("Enabled", ImGuiTableColumnFlags.WidthFixed, EnabledColumnWidth());
        ImGui.TableSetupColumn("##showType", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();

        for (var c = BossModuleInfo.Category.Uncategorized; c < BossModuleInfo.Category.Count; ++c)
        {
            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            var (anyEnabled, allEnabled) = _moduleConfig.CategoryEnabledState(c);
            var enabled = allEnabled;
            var mixed = anyEnabled && !allEnabled;
            if (mixed)
            {
                ImGuiP.PushItemFlag(ImGuiItemFlags.MixedValue, true);
            }
            CenterEnableCheckbox();
            var changed = ImGui.Checkbox($"##enable-category-{c}", ref enabled);
            if (mixed)
            {
                ImGuiP.PopItemFlag();
            }
            if (changed)
            {
                _moduleConfig.SetCategoryEnabled(c, enabled);
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(anyEnabled != allEnabled ? "Some modules in this category are disabled. Click to enable all." : enabled ? "Disable all modules in this category." : "Enable all modules in this category.");
            }

            ImGui.TableNextColumn();
            ref var category = ref Categories[(int)c];
            UIMisc.ImageToggleButton(Service.Texture?.GetFromGameIcon(category.icon), _iconSize, !_filterCategories[(int)c], category.name);
            if (ImGui.IsItemClicked())
            {
                _filterCategories.Toggle((int)c);
            }
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                _filterCategories = ~_filterCategories;
                _filterCategories.Toggle((int)c);
            }
        }
    }

    private void DrawModules(UITree tree, WorldState ws)
    {
        using var table = ImRaii.Table("ModulesTable", 3, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp);
        if (!table)
        {
            return;
        }

        ImGui.TableSetupColumn("##type", ImGuiTableColumnFlags.WidthFixed, 80f);
        ImGui.TableSetupColumn("Enabled", ImGuiTableColumnFlags.WidthFixed, EnabledColumnWidth());
        ImGui.TableSetupColumn("##fight", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();

        for (var i = 0; i < (int)BossModuleInfo.Expansion.Count; ++i)
        {
            if (_filterExpansions[i])
            {
                continue;
            }

            for (var j = 0; j < (int)BossModuleInfo.Category.Count; ++j)
            {
                if (_filterCategories[j])
                {
                    continue;
                }

                var groupList = Groups[i, j];
                if (groupList == null)
                {
                    continue;
                }

                var groups = CollectionsMarshal.AsSpan(groupList);
                var countG = groups.Length;
                for (var k = 0; k < countG; ++k)
                {
                    var group = groups[k];
                    var groupModuleOIDs = _moduleConfig.MinMaturity == BossModuleInfo.Maturity.Dummy ? group.ModuleOIDs : group.NonDummyModuleOIDs;
                    if (groupModuleOIDs.Count == 0)
                    {
                        continue;
                    }

                    if (!_searchText.IsNullOrEmpty() && !group.Info.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    ImGui.TableNextRow();

                    ImGui.TableNextColumn();
                    UIMisc.Image(Service.Texture?.GetFromGameIcon(Expansions[i].icon), new(36f));
                    ImGui.SameLine();
                    UIMisc.Image(Service.Texture?.GetFromGameIcon(group.Info.Icon != 0u ? group.Info.Icon : Categories[j].icon), new(36f));

                    ImGui.TableNextColumn();
                    var (groupAnyEnabled, groupAllEnabled) = _moduleConfig.ModulesEnabledState(groupModuleOIDs);
                    var groupEnabled = groupAllEnabled;
                    var groupMixed = groupAnyEnabled && !groupAllEnabled;
                    if (groupMixed)
                    {
                        ImGuiP.PushItemFlag(ImGuiItemFlags.MixedValue, true);
                    }
                    CenterEnableCheckbox();
                    var groupChanged = ImGui.Checkbox(group.EnableID, ref groupEnabled);
                    if (groupMixed)
                    {
                        ImGuiP.PopItemFlag();
                    }
                    if (groupChanged)
                    {
                        _moduleConfig.SetModulesEnabled(groupModuleOIDs, groupEnabled);
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip(groupAnyEnabled != groupAllEnabled ? "Some modules in this group are disabled. Click to enable all." : groupEnabled ? "Disable all modules in this group." : "Enable all modules in this group.");
                    }

                    ImGui.TableNextColumn();
                    foreach (var ng in tree.Node(group.NodeLabel))
                    {
                        var modules = CollectionsMarshal.AsSpan(EnsureModules(group));
                        var len = modules.Length;
                        for (var l = 0; l < len; ++l)
                        {
                            ref readonly var mod = ref modules[l];
                            if (!_moduleConfig.IncludeInSupportedFightControls(mod.Info))
                            {
                                continue;
                            }

                            ImGui.TableNextRow();

                            ImGui.TableNextColumn();

                            ImGui.TableNextColumn();
                            var moduleEnabled = _moduleConfig.IsModuleEnabled(mod.Info.PrimaryActorOID);
                            CenterEnableCheckbox();
                            if (ImGui.Checkbox(mod.EnableID, ref moduleEnabled))
                            {
                                _moduleConfig.SetModuleEnabled(mod.Info.PrimaryActorOID, moduleEnabled);
                            }
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip(moduleEnabled ? "Disable this module." : "Enable this module.");
                            }

                            ImGui.TableNextColumn();
                            using (ImRaii.Disabled(mod.Info.ConfigType == null && !mod.Info.HasPrePullHints))
                            {
                                if (UIMisc.IconButton(FontAwesomeIcon.Cog, mod.ConfigID))
                                {
                                    BossModuleConfigWindow.Open(mod.Info, ws);
                                }
                            }

                            ImGui.SameLine();
                            using (ImRaii.Disabled(mod.Info.PlanLevel == 0))
                            {
                                if (UIMisc.IconButton(FontAwesomeIcon.ClipboardList, mod.PlansID))
                                {
                                    ImGui.OpenPopup(mod.PopupID);
                                }
                            }

                            ImGui.SameLine();
                            DrawModuleHelpMarker(mod.Info, mod.HelpText);
                            ImGui.SameLine();
                            var textColor = MaturityColor(mod.Info.Maturity);
                            using (ImRaii.PushColor(ImGuiCol.Text, textColor))
                            {
                                ImGui.TextUnformatted(mod.DisplayName);
                            }

                            using (var popup = ImRaii.Popup(mod.PopupID))
                            {
                                if (popup)
                                {
                                    ModulePlansPopup(mod.Info);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void Customize(BossModuleInfo.Expansion expansion, uint iconId, ReadOnlySeString name) => Expansions[(int)expansion] = (name.ToString(), iconId);
    private void Customize(BossModuleInfo.Category category, uint iconId, ReadOnlySeString name) => Categories[(int)category] = (name.ToString(), iconId);
    private void Customize(BossModuleInfo.Category category, ContentType ct, string? name = null) => Customize(category, ct.Icon, name ?? ct.Name);
    private void Customize(BossModuleInfo.Category category, CharaCardPlayStyle ps) => Customize(category, (uint)ps.Icon, ps.Name);

    //private static IDalamudTextureWrap? GetIcon(uint iconId) => iconId != 0 ? Service.Texture?.GetIcon(iconId, Dalamud.Plugin.Services.ITextureProvider.IconFlags.HiRes) : null;
    public static string FixCase(ReadOnlySeString str) => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(str.ToString());
    public static string BNpcName(uint id) => FixCase(Service.LuminaRow<BNpcName>(id)!.Value.Singular);

    private static GroupClassificationKey GroupKey(BossModuleRegistry.Info module)
    {
        var groupID = module.GroupType switch
        {
            BossModuleInfo.GroupType.CFC or
            BossModuleInfo.GroupType.MaskedCarnivale or
            BossModuleInfo.GroupType.CrucibleOfTheUnbroken or
            BossModuleInfo.GroupType.ForayFATE or
            BossModuleInfo.GroupType.Quest or
            BossModuleInfo.GroupType.Hunt or
            BossModuleInfo.GroupType.CriticalEngagement or
            BossModuleInfo.GroupType.BozjaDuel or
            BossModuleInfo.GroupType.EurekaNM => module.GroupID,
            _ => 0u
        };
        return new(module.Expansion, module.Category, module.GroupType, groupID);
    }

    private ModuleGroupInfo ClassifyGroup(BossModuleRegistry.Info module)
    {
        var groupId = (uint)module.GroupType << 24;
        switch (module.GroupType)
        {
            case BossModuleInfo.GroupType.CFC:
                groupId |= module.GroupID;
                var cfcRow = Service.LuminaRow<ContentFinderCondition>(module.GroupID)!.Value;
                var cfcSort = cfcRow.SortKey;
                return new(FixCase(cfcRow.Name), groupId, cfcSort != 0 ? cfcSort : groupId);
            case BossModuleInfo.GroupType.MaskedCarnivale:
                groupId |= module.GroupID;
                var mcRow = Service.LuminaRow<ContentFinderCondition>(module.GroupID)!.Value;
                var mcSort = uint.Parse(mcRow.ShortCode.ToString().AsSpan(3), CultureInfo.InvariantCulture); // 'aozNNN'
                return new($"Stage {mcSort}: {FixCase(mcRow.Name)}", groupId, mcSort);
            case BossModuleInfo.GroupType.CrucibleOfTheUnbroken:
                groupId |= module.GroupID;
                var bmRow = Service.LuminaRow<ContentFinderCondition>(module.GroupID)!.Value;
                var bmSort = uint.Parse(bmRow.ShortCode.ToString().AsSpan(3), CultureInfo.InvariantCulture);
                var bmName = $"Crucible of the Unbroken: {FixCase(bmRow.Name)}";
                const string suffix = " Of the Unbroken";
                if (bmName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    bmName = bmName[..^suffix.Length];
                }
                return new(bmName, groupId, bmSort);
            case BossModuleInfo.GroupType.RemovedUnreal:
                return new("Removed Content", groupId, groupId);
            case BossModuleInfo.GroupType.BaldesionArsenal:
                return new("Baldesion Arsenal", groupId, groupId);
            case BossModuleInfo.GroupType.CastrumLacusLitore:
                return new("Castrum Lacus Litore", groupId, groupId);
            case BossModuleInfo.GroupType.TheDalriada:
                return new("The Dalriada", groupId, groupId);
            case BossModuleInfo.GroupType.TheForkedTowerBlood:
                return new("The Forked Tower: Blood", groupId, groupId);
            case BossModuleInfo.GroupType.TheForkedTowerMagicNormal:
                return new("The Forked Tower: Magic (Normal)", groupId, groupId);
            case BossModuleInfo.GroupType.TheForkedTowerMagicExtreme:
                return new("The Forked Tower: Magic (Extreme)", groupId, groupId);
            case BossModuleInfo.GroupType.ForayFATE:
                groupId |= module.GroupID;
                return new($"{FixCase(Service.LuminaRow<ContentFinderCondition>(module.GroupID)!.Value.Name)} FATE", groupId, groupId);
            case BossModuleInfo.GroupType.Quest:
                var questRow = Service.LuminaRow<Quest>(module.GroupID)!.Value;
                groupId |= questRow.JournalGenre.RowId;
                return new(questRow.JournalGenre.ValueNullable?.Name.ToString() ?? "", groupId, groupId);
            case BossModuleInfo.GroupType.Fate:
                return new($"{module.Expansion.ShortName()} FATE", groupId, groupId, _iconFATE);
            case BossModuleInfo.GroupType.Hunt:
                groupId |= module.GroupID;
                return new($"{module.Expansion.ShortName()} Hunt {(BossModuleInfo.HuntRank)module.GroupID}", groupId, groupId, _iconHunt);
            case BossModuleInfo.GroupType.CriticalEngagement:
                groupId |= module.GroupID;
                return new($"{FixCase(Service.LuminaRow<ContentFinderCondition>(module.GroupID)!.Value.Name)} CE", groupId, groupId);
            case BossModuleInfo.GroupType.BozjaDuel:
                groupId |= module.GroupID;
                return new($"{FixCase(Service.LuminaRow<ContentFinderCondition>(module.GroupID)!.Value.Name)} Duel", groupId, groupId);
            case BossModuleInfo.GroupType.EurekaNM:
                groupId |= module.GroupID;
                return new(FixCase(Service.LuminaRow<ContentFinderCondition>(module.GroupID)!.Value.Name), groupId, groupId);
            case BossModuleInfo.GroupType.GoldSaucer:
                return new("Gold Saucer", groupId, groupId);
            default:
                return new("Uncategorized", groupId, groupId);
        }
    }

    private List<ModuleInfo> EnsureModules(ModuleGroup group)
    {
        if (group.Modules != null)
        {
            return group.Modules;
        }

        var infos = group.ModuleInfos;
        var count = infos.Count;
        var modules = new List<ModuleInfo>(count);
        CollectionsMarshal.SetCount(modules, count);
        var infosSpan = CollectionsMarshal.AsSpan(infos);
        var modulesSpan = CollectionsMarshal.AsSpan(modules);

        for (var i = 0; i < count; ++i)
        {
            modulesSpan[i] = BuildModuleInfo(infosSpan[i]);
        }
        group.Modules = modules;
        return modules;
    }

    private static ModuleInfo BuildModuleInfo(BossModuleRegistry.Info module)
    {
        var name = module.GroupType switch
        {
            BossModuleInfo.GroupType.ForayFATE => Service.LuminaRow<Fate>(module.NameID)!.Value.Name.ToString(),
            BossModuleInfo.GroupType.Quest => $"{Service.LuminaRow<Quest>(module.GroupID)!.Value.Name}: {BNpcName(module.NameID)}",
            BossModuleInfo.GroupType.Fate => $"{Service.LuminaRow<Fate>(module.GroupID)!.Value.Name}: {BNpcName(module.NameID)}",
            BossModuleInfo.GroupType.CriticalEngagement or BossModuleInfo.GroupType.BozjaDuel => Service.LuminaRow<DynamicEvent>(module.NameID)!.Value.Name.ToString(),
            BossModuleInfo.GroupType.EurekaNM => Service.LuminaRow<Fate>(module.NameID)!.Value.Name.ToString(),
            BossModuleInfo.GroupType.GoldSaucer => $"{Service.LuminaRow<GoldSaucerTextData>(module.GroupID)?.Text}: {BNpcName(module.NameID)}",
            _ => BNpcName(module.NameID)
        };
        return new(module, name, module.SortOrder);
    }

    internal readonly struct SupportedFightSortKey(bool dummy, BossModuleInfo.Expansion expansion, BossModuleInfo.Category category, uint groupOrder, int moduleOrder) : IComparable<SupportedFightSortKey>
    {
        private readonly bool Dummy = dummy;
        private readonly BossModuleInfo.Expansion Expansion = expansion;
        private readonly BossModuleInfo.Category Category = category;
        private readonly uint GroupOrder = groupOrder;
        private readonly int ModuleOrder = moduleOrder;

        public readonly int CompareTo(SupportedFightSortKey other)
        {
            var cmp = Dummy.CompareTo(other.Dummy);
            if (cmp != 0)
            {
                return cmp;
            }
            cmp = ((int)Expansion).CompareTo((int)other.Expansion);

            if (cmp != 0)
            {
                return cmp;
            }
            cmp = ((int)Category).CompareTo((int)other.Category);
            if (cmp != 0)
            {
                return cmp;
            }
            cmp = GroupOrder.CompareTo(other.GroupOrder);
            return cmp != 0 ? cmp : ModuleOrder.CompareTo(other.ModuleOrder);
        }
    }

    internal static SupportedFightSortKey GetSupportedFightSortKey(BossModuleRegistry.Info module)
        => new(module.Maturity == BossModuleInfo.Maturity.Dummy, module.Expansion, module.Category, SupportedGroupSortOrder(module), module.SortOrder);

    private static uint SupportedGroupSortOrder(BossModuleRegistry.Info module)
    {
        var groupId = (uint)module.GroupType << 24;
        switch (module.GroupType)
        {
            case BossModuleInfo.GroupType.CFC:
                groupId |= module.GroupID;
                var cfcSort = Service.LuminaRow<ContentFinderCondition>(module.GroupID)!.Value.SortKey;
                return cfcSort != 0 ? cfcSort : groupId;
            case BossModuleInfo.GroupType.MaskedCarnivale:
            case BossModuleInfo.GroupType.CrucibleOfTheUnbroken:
                var shortCode = Service.LuminaRow<ContentFinderCondition>(module.GroupID)!.Value.ShortCode.ToString();
                return uint.Parse(shortCode.AsSpan(3), CultureInfo.InvariantCulture);
            case BossModuleInfo.GroupType.Quest:
                return groupId | Service.LuminaRow<Quest>(module.GroupID)!.Value.JournalGenre.RowId;
            case BossModuleInfo.GroupType.ForayFATE:
            case BossModuleInfo.GroupType.Hunt:
            case BossModuleInfo.GroupType.CriticalEngagement:
            case BossModuleInfo.GroupType.BozjaDuel:
            case BossModuleInfo.GroupType.EurekaNM:
                return groupId | module.GroupID;
            default:
                return groupId;
        }
    }

    private static uint MaturityColor(BossModuleInfo.Maturity maturity) => maturity switch
    {
        BossModuleInfo.Maturity.WIP => Colors.TextColor3,
        BossModuleInfo.Maturity.Verified => Colors.TextColor4,
        BossModuleInfo.Maturity.AISupport => Colors.TextColor2,
        BossModuleInfo.Maturity.Dummy => Colors.TextColor17,
        _ => Colors.TextColor1
    };

    private static void DrawModuleHelpMarker(BossModuleRegistry.Info info, string helpText)
    {
        UIMisc.IconText(FontAwesomeIcon.InfoCircle);
        if (!ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            return;
        }

        using var tooltip = ImRaii.Tooltip();
        using var wrap = ImRaii.TextWrapPos(ImGui.GetFontSize() * 35f);
        var maturityDescription = GeneratedEnumMetadata.For(info.Maturity).Attribute<PropertyDisplayAttribute>()?.Label ?? info.Maturity.ToString();
        ImGui.TextUnformatted("Maturity: ");
        ImGui.SameLine(0f, 0f);
        using (ImRaii.PushColor(ImGuiCol.Text, MaturityColor(info.Maturity)))
        {
            ImGui.TextUnformatted(maturityDescription);
        }
        ImGui.TextUnformatted(helpText);
    }

    private static string BuildModuleHelpText(BossModuleRegistry.Info info)
    {
        var planning = info.PlanLevel > 0 ? $"L{info.PlanLevel}" : "not supported";
        return info.Contributors.Length > 0
            ? $"Cooldown planning: {planning}\nContributors: {info.Contributors}\n"
            : $"Cooldown planning: {planning}\n";
    }

    private void ModulePlansPopup(BossModuleRegistry.Info info)
    {
        if (_planDB == null)
        {
            return;
        }

        var mplans = _planDB.Plans.GetOrAdd(info.ModuleType);
        foreach (var (cls, plans) in mplans)
        {
            var plansL = CollectionsMarshal.AsSpan(plans.Plans);
            var count = plansL.Length;
            for (var i = 0; i < count; ++i)
            {
                var plan = plansL[i];
                if (ImGui.Selectable($"Edit {cls} '{plan.Name}' ({plan.Guid})"))
                {
                    UIPlanDatabaseEditor.StartPlanEditor(_planDB, plan);
                }
            }
        }

        var player = _ws.Party.Player();
        if (player != null)
        {
            if (ImGui.Selectable($"New plan for {player.Class}..."))
            {
                var plans = mplans.GetOrAdd(player.Class);
                var plan = new Plan($"New {plans.Plans.Count + 1}", info.ModuleType) { Guid = Guid.NewGuid().ToString(), Class = player.Class, Level = info.PlanLevel };
                _planDB.ModifyPlan(null, plan);
                UIPlanDatabaseEditor.StartPlanEditor(_planDB, plan);
            }
        }
    }
}
