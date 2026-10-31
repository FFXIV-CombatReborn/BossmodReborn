using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using RebornMaterial;
using System.IO;

namespace BossMod.RebornUI;

internal sealed partial class SettingsWindow : RebornFoldWindow
{
    public const string HomePageId = "home";
    public const string CommandsPage = "commands";
    public const string FightsPage = "fights";
    public const string PresetsPage = "presets";

    private const string SectionGeneral = "General";
    private const string SectionBossModules = "Boss modules";
    private const string SectionAutomation = "Automation";
    private const string SectionTools = "Tools";
    private const string SectionOther = "Other";

    private const int MaxSearchResults = 40;

    private static readonly Vector2 DefaultSize = new(980f, 680f);
    private static readonly WindowSizeConstraints DefaultSizeConstraints = new()
    {
        MinimumSize = new(420f, 360f),
        MaximumSize = new(5000f, 5000f),
    };

    private static readonly M3WindowAction[] _windowActions =
    [
        new("##window_discord", FontAwesomeIcon.Comments, "Combat Reborn Discord"),
        new("##window_github", FontAwesomeIcon.CodeBranch, "BossMod Reborn on GitHub"),
        new("##window_wiki", FontAwesomeIcon.Book, "BossMod wiki"),
    ];

    private sealed class UINode(ConfigNode? node)
    {
        public ConfigNode? Node = node;
        public string Name = "";
        public int Order;
        public FontAwesomeIcon Icon;
        public string? Description;
        public ModuleViewer.SupportedFightSortKey? SupportedFightOrder;
        public UINode? Parent;
        public List<UINode> Children = [];
        public string[] Tags = [];
        public List<BossModuleRegistry.Info> PrePullHintModules = [];
        public string PathText = "";
    }

    private sealed class Page(string id, string label, FontAwesomeIcon icon, string section, string? description, Action draw, UINode? node = null, bool scrolls = true)
    {
        public readonly string Id = id;
        public readonly string Label = label;
        public readonly FontAwesomeIcon Icon = icon;
        public readonly string Section = section;
        public readonly string? Description = description;
        public readonly Action Draw = draw;
        public readonly UINode? Node = node;
        public readonly bool Scrolls = scrolls;
    }

    private sealed class SearchResult(UINode node, bool wholeNode)
    {
        public readonly UINode Node = node;
        public readonly bool WholeNode = wholeNode;
        public readonly HashSet<string> Fields = [];
        public readonly List<BossModuleRegistry.Info> Hints = [];
    }

    private readonly List<UINode> _roots = [];
    private readonly UITree _tree = new();
    private readonly HomePage _home;
    private readonly ModuleViewer _mv;
    private readonly ConfigRoot _root;
    private readonly WorldState _ws;
    private readonly UIPresetDatabaseEditor? _presets;
    private readonly List<Page> _pages = [];
    private readonly List<M3NavItem> _navItems = [];
    private readonly Dictionary<UINode, bool> _expanded = [];
    private readonly string _version = $"v{typeof(SettingsWindow).Assembly.GetName().Version}";
    private Page _page;
    private string _loggedPageError = "";
    private float _collapsedCardStep;

    private string _searchText = "";
    private readonly List<SearchResult> _searchResults = [];
    private bool _railSearch;
    private bool _focusSearch;

    private static float Scale => M3.Scale;

    private bool Searching => _searchText.Length > 0;

    private bool RailSearchOpen => _railSearch || Searching;

    public SettingsWindow(ConfigRoot config, WorldState ws, DirectoryInfo? replayDir, RotationDatabase? rotationDB) : base("BossMod Reborn###BMRConfig", "BossMod Reborn", DefaultSize, DefaultSizeConstraints)
    {
        _root = config;
        _ws = ws;
        _home = new(replayDir, ShowPage);
        _mv = new(rotationDB?.Plans, ws);
        _presets = rotationDB != null ? new(rotationDB) : null;

        Dictionary<Type, UINode> nodes = [];

        foreach (var n in _root._nodes.Values)
        {
            nodes[n.GetType()] = new(n);
        }

        foreach (var (t, n) in nodes)
        {
            var props = GeneratedConfigMetadata.Get(t).Display;
            var layout = SettingsLayout.For(t);
            n.Name = layout?.Name ?? props?.Name ?? GenerateNodeName(t);
            n.Order = props?.Order ?? 0;
            n.Icon = layout?.Icon ?? FontAwesomeIcon.None;
            n.Description = layout?.Description;
            n.Parent = props?.Parent != null ? nodes.GetValueOrDefault(props.Parent) : null;
            n.Tags = [.. props?.Tags ?? [], .. layout?.Tags ?? []];

            var parentNodes = n.Parent?.Children ?? _roots;
            parentNodes.Add(n);
        }

        foreach (var info in BossModuleRegistry.RegisteredModules.Values)
        {
            if (!info.HasPrePullHints)
            {
                continue;
            }

            if (info.ConfigType != null && nodes.TryGetValue(info.ConfigType, out var configNode))
            {
                configNode.PrePullHintModules.Add(info);
                continue;
            }

            var hintNode = new UINode(null)
            {
                Name = GenerateNodeName(info.ModuleType),
                Order = 0x100000,
                SupportedFightOrder = ModuleViewer.GetSupportedFightSortKey(info),
                Parent = nodes.GetValueOrDefault(ExpansionConfigType(info.Expansion)) ?? nodes.GetValueOrDefault(typeof(ModuleConfig))
            };
            hintNode.PrePullHintModules.Add(info);
            (hintNode.Parent?.Children ?? _roots).Add(hintNode);
        }

        SortByOrder(_roots);
        ResolvePaths(_roots, "");

        BuildPages();
        _page = _pages[0];
    }

    protected override void Dispose(bool disposing)
    {
        _mv.Dispose();
        base.Dispose(disposing);
    }

    public static string PageForClassicTab(string tab) => tab switch
    {
        "About" => HomePageId,
        "Supported fights" => FightsPage,
        "Autorotation presets" => PresetsPage,
        "Slash commands" => CommandsPage,
        _ => tab,
    };

    public void Open(string page = "")
    {
        ShowPage(page);
        Restore();
        OpenAndBringToFront();
    }

    public void ShowPage(string id)
    {
        foreach (var p in _pages)
        {
            if (p.Id == id)
            {
                _page = p;
                ClearSearch();
                return;
            }
        }
    }

    public override void OnClose()
    {
        M3Snackbar.Clear();
        base.OnClose();
    }

    protected override M3Density Density => SettingsDensity;

    protected override M3WindowAction[] WindowActions => _windowActions;

    protected override void OnWindowAction(int index)
    {
        var error = index switch
        {
            0 => HomePage.OpenLink(HomePage.DiscordUrl),
            1 => HomePage.OpenLink(HomePage.GitHubUrl),
            2 => HomePage.OpenLink(HomePage.WikiUrl),
            _ => ""
        };
        if (error.Length > 0)
        {
            M3Snackbar.Show(error, severity: M3Severity.Error);
        }
    }

    private void BuildPages()
    {
        AddPage(new(HomePageId, "Home", FontAwesomeIcon.Home, SectionGeneral, "What BossMod Reborn does, and where to get help", _home.Draw));
        AddNodePage(typeof(InterfaceConfig), SectionGeneral);
        AddNodePage(typeof(ActionTweaksConfig), SectionGeneral);
        AddPage(new(CommandsPage, "Slash commands", FontAwesomeIcon.Terminal, SectionGeneral, "Chat commands for controlling BossMod Reborn", DrawCommands));

        AddNodePage(typeof(BossModuleConfig), SectionBossModules);
        AddPage(new(FightsPage, "Supported fights", FontAwesomeIcon.Dungeon, SectionBossModules, "Every encounter with a module; turn modules on or off, open their settings and cooldown plans", () => _mv.DrawMaterial(_ws), scrolls: false));
        AddNodePage(typeof(ModuleConfig), SectionBossModules);
        AddNodePage(typeof(PartyRolesConfig), SectionBossModules);
        AddNodePage(typeof(ColorConfig), SectionBossModules);

        AddNodePage(typeof(AI.AIConfig), SectionAutomation);
        AddNodePage(typeof(AutorotationConfig), SectionAutomation);
        if (_presets != null)
        {
            AddPage(new(PresetsPage, "Autorotation presets", FontAwesomeIcon.LayerGroup, SectionAutomation, "Presets choose rotation modules and configure their strategies", _presets.Draw));
        }
        AddNodePage(typeof(ZoneModuleConfig), SectionAutomation);

        AddNodePage(typeof(ReplayManagementConfig), SectionTools);
        AddNodePage(typeof(Pathfinding.DeveloperConfig), SectionTools);

        foreach (var n in _roots)
        {
            if (!_pages.Exists(p => p.Node == n))
            {
                AddNodePage(n, SectionOther);
            }
        }
    }

    private void AddPage(Page page)
    {
        _pages.Add(page);
        _navItems.Add(new(page.Id, page.Label, page.Icon, false, page.Description, Section: page.Section));
    }

    private void AddNodePage(Type type, string section)
    {
        var node = _roots.Find(n => n.Node?.GetType() == type);
        if (node != null)
        {
            AddNodePage(node, section);
        }
    }

    private void AddNodePage(UINode node, string section)
        => AddPage(new(node.Node?.GetType().Name ?? node.Name, node.Name, node.Icon != FontAwesomeIcon.None ? node.Icon : FontAwesomeIcon.Cog, section, node.Description, () => DrawNodePage(node), node));

    private void ResolvePaths(List<UINode> nodes, string parent)
    {
        foreach (var n in nodes)
        {
            n.PathText = parent.Length > 0 ? $"{parent} › {n.Name}" : n.Name;
            ResolvePaths(n.Children, n.PathText);
        }
    }

    protected override void DrawBackdrop(float rounding) => M3Draw.WindowBackdrop(rounding);

    protected override void DrawOpen()
    {
        var available = MathF.Max(1f, ImGui.GetContentRegionAvail().X);
        var spacing = ImGui.GetStyle().ItemSpacing.X;
        var expanded = available >= 600f * Scale;
        var sidebarWidth = expanded ? MathF.Min(DrawerWidth(), available * 0.4f) : MathF.Min(84f * Scale, available * 0.3f);

        var origin = ImGui.GetCursorScreenPos();
        var paneHeight = MathF.Max(1f, ImGui.GetContentRegionAvail().Y);
        var dividerX = origin.X + sidebarWidth + spacing * 0.5f;
        ImGui.GetWindowDrawList().AddLine(new(dividerX, origin.Y), new(dividerX, origin.Y + paneHeight), M3.U32(M3.Scheme.OutlineVariant, 0.45f), 1f * Scale);

        DrawNavigation(sidebarWidth, expanded);
        ImGui.SameLine(0f, spacing);
        DrawBody(expanded);

        M3Snackbar.Draw(new(origin.X + sidebarWidth + spacing, origin.Y), new(origin.X + available, origin.Y + paneHeight));
    }

    private float DrawerWidth()
    {
        var widest = 0f;
        foreach (var p in _pages)
        {
            widest = MathF.Max(widest, ImGui.CalcTextSize(p.Label).X);
        }
        return MathF.Max(190f * Scale, widest + 24f * Scale + M3Style.Spacing(16f, 12f) + M3Style.Spacing(14f, 10f) + 20f * Scale);
    }

    private void DrawNavigation(float width, bool expanded)
    {
        using var child = ImRaii.Child("##bmr_nav", new(width, -1f), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
        if (!child)
        {
            return;
        }

        var inner = ImGui.GetContentRegionAvail().X;
        DrawBrand(inner, expanded);

        ImGui.Dummy(new(0f, M3.Space1));
        if (expanded)
        {
            DrawSearchField(inner);
        }
        else
        {
            // The rail is too narrow for the field, so this shows one above the page instead.
            var button = M3Widgets.IconButtonSize;
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + MathF.Max(0f, (inner - button) * 0.5f));
            var open = RailSearchOpen;
            if (M3Widgets.IconToggle("##bmr_search_toggle", FontAwesomeIcon.Search, ref open, open ? "Close search" : "Search settings"))
            {
                if (open)
                {
                    _railSearch = _focusSearch = true;
                }
                else
                {
                    ClearSearch();
                }
            }
        }
        ImGui.Dummy(new(0f, M3.Space1));

        using var list = ImRaii.Child("##bmr_nav_list", new(-1f, -1f), false);
        if (!list)
        {
            return;
        }

        var tools = M3.Scheme.Tertiary;
        for (var i = 0; i < _navItems.Count; ++i)
        {
            _navItems[i] = _navItems[i] with { Selected = !Searching && _pages[i] == _page, Accent = _pages[i].Section == SectionTools ? tools : null };
        }

        if (M3Navigation.Draw("bmr_nav", _navItems, expanded) is { } clicked)
        {
            ShowPage(clicked);
        }
    }

    private void DrawBrand(float width, bool expanded)
    {
        var s = M3.Scheme;
        var size = 40f * Scale;
        var origin = ImGui.GetCursorScreenPos();
        var height = size + 2f * M3.Space1;

        var pressed = ImGui.InvisibleButton("##bmr_brand", new(width, height));
        var hovered = ImGui.IsItemHovered();
        var drawList = ImGui.GetWindowDrawList();
        if (hovered)
        {
            drawList.AddRectFilled(origin, origin + new Vector2(width, height), M3.U32(s.OnSurface, M3.StateHover), M3.ShapeMedium);
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            M3Tooltip.Show("Home");
        }

        var tileMin = new Vector2(expanded ? origin.X + M3.Space2 : origin.X + (width - size) * 0.5f, origin.Y + M3.Space1);
        DrawLogo(drawList, tileMin, size);

        if (expanded)
        {
            var textX = tileMin.X + size + M3.Space2;
            var textWidth = MathF.Max(1f, origin.X + width - textX - M3.Space1);
            float titleHeight, versionHeight;
            string title, version;
            using (ImRaii.PushFont(M3.TitleMedium))
            {
                title = M3Navigation.Truncate("BossMod Reborn", textWidth);
                titleHeight = ImGui.CalcTextSize(title).Y;
            }
            using (ImRaii.PushFont(M3.LabelSmall))
            {
                version = M3Navigation.Truncate(_version, textWidth);
                versionHeight = ImGui.CalcTextSize(version).Y;
            }

            var top = tileMin.Y + (size - titleHeight - versionHeight - 2f * Scale) * 0.5f;
            using (ImRaii.PushFont(M3.TitleMedium))
            {
                drawList.AddText(new(textX, top), M3.U32(s.OnSurface), title);
            }
            using (ImRaii.PushFont(M3.LabelSmall))
            {
                drawList.AddText(new(textX, top + titleHeight + 2f * Scale), M3.U32(s.OnSurfaceVariant, 0.9f), version);
            }
        }

        if (pressed)
        {
            ShowPage(HomePageId);
        }
    }

    private void DrawSearchField(float width)
    {
        if (M3Widgets.SearchField("##bmr_search", "Search settings", ref _searchText, width, busy: Searching))
        {
            UpdateSearch();
        }
    }

    private void DrawBody(bool expanded)
    {
        using var child = ImRaii.Child("##bmr_body", -Vector2.One, false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
        if (!child)
        {
            return;
        }

        DrawTopAppBar();
        if (!expanded && RailSearchOpen)
        {
            _railSearch = true;
            if (_focusSearch)
            {
                _focusSearch = false;
                ImGui.SetKeyboardFocusHere();
            }
            DrawSearchField(ImGui.GetContentRegionAvail().X);
            ImGui.Dummy(new(0f, M3.Space1));
        }

        var scrolls = Searching || _page.Scrolls;
        using var page = ImRaii.Child($"##bmr_page_{(Searching ? "search" : _page.Id)}", -Vector2.One, false,
            scrolls ? ImGuiWindowFlags.None : ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
        if (!page)
        {
            return;
        }

        try
        {
            if (Searching)
            {
                DrawSearchResults();
            }
            else
            {
                _page.Draw();
            }
        }
        catch (Exception ex)
        {
            var name = Searching ? "search results" : _page.Id;
            var error = $"{name}: {ex.Message}";
            if (error != _loggedPageError)
            {
                _loggedPageError = error;
                Service.Logger.Error(ex, $"[RebornUI] Exception while drawing page {name}");
            }
            M3Widgets.Banner("##bmr_page_error", $"Something went wrong while drawing this page: {ex.Message}", M3Severity.Error, FontAwesomeIcon.Bug);
        }
    }

    private void DrawTopAppBar()
    {
        var title = Searching ? "Search results" : _page.Label;
        var subtitle = Searching
            ? _searchResults.Count == 1 ? "1 matching group of settings" : $"{_searchResults.Count} matching groups of settings"
            : _page.Description;

        var clearSize = M3Widgets.IconButtonSize;
        using var bar = M3TopAppBar.Begin(title, subtitle, _windowActions.Length, Brand, Searching ? clearSize + 4f * Scale : 0f);
        SetActionBar(bar.ShownActions, bar.BarTop);

        if (Searching)
        {
            ImGui.SetCursorScreenPos(bar.TrailingPosition(clearSize));
            if (M3Widgets.IconButton("##bmr_clear_search", FontAwesomeIcon.Times, "Clear search"))
            {
                ClearSearch();
            }
        }
    }

    private void DrawNodePage(UINode n)
    {
        if (n.Node != null && SettingsLayout.For(n.Node.GetType())?.Notice is { } notice)
        {
            M3Widgets.Banner($"##notice_{n.PathText}", notice.Text, notice.Severity, notice.Icon);
            ImGui.Dummy(new(0f, M3.Space2));
        }

        DrawHintSettings(n, n.PrePullHintModules);
        if (n.Node != null)
        {
            SettingsRenderer.DrawNodeCards(n.Node, _root, _tree, _ws, null);
        }
        DrawChildNodes(n.Children);
    }

    private void DrawChildNodes(List<UINode> nodes)
    {
        var index = 0;
        foreach (var c in nodes)
        {
            var hasSettings = c.PrePullHintModules.Count > 0 || c.Node != null && SettingsRenderer.NodeHasSettings(c.Node);
            if (!hasSettings)
            {
                if (c.Children.Count > 0)
                {
                    M3Widgets.SectionLabel(c.Name);
                    DrawChildNodes(c.Children);
                }
                continue;
            }

            var wasExpanded = _expanded.GetValueOrDefault(c);
            var accent = M3.AccentFor(index++);
            if (!wasExpanded && SkipHiddenCollapsedCard())
            {
                continue;
            }

            var expanded = wasExpanded;
            var top = ImGui.GetCursorScreenPos().Y;
            using (var card = M3ExpandableCard.Begin($"node_{c.PathText}", c.Name, ref expanded, c.Icon, accent))
            {
                if (card.Expanded)
                {
                    DrawHintSettings(c, c.PrePullHintModules);
                    if (c.Node != null)
                    {
                        SettingsRenderer.DrawNode(c.Node, _root, _tree, _ws);
                    }
                    DrawChildNodes(c.Children);
                }
            }

            if (expanded != wasExpanded)
            {
                _expanded[c] = expanded;
            }
            else if (!expanded)
            {
                _collapsedCardStep = ImGui.GetCursorScreenPos().Y - top;
            }
        }
    }

    private bool SkipHiddenCollapsedCard()
    {
        var step = _collapsedCardStep;
        if (step <= 0f)
        {
            return false;
        }

        var min = ImGui.GetCursorScreenPos();
        var width = MathF.Max(1f, ImGui.GetContentRegionAvail().X);
        if (ImGui.IsRectVisible(min, min + new Vector2(width, step)))
        {
            return false;
        }

        ImGui.Dummy(new(width, MathF.Max(0f, step - ImGui.GetStyle().ItemSpacing.Y)));
        return true;
    }

    private static void DrawHintSettings(UINode n, List<BossModuleRegistry.Info> infos)
    {
        foreach (var info in infos)
        {
            SettingsRenderer.DrawPrePullHintSetting(info, PrePullHintSettingLabel(info, n.PrePullHintModules.Count > 1));
        }
    }

    private void ClearSearch()
    {
        _searchText = "";
        _searchResults.Clear();
        _railSearch = false;
    }

    private void UpdateSearch()
    {
        _searchResults.Clear();
        if (_searchText.Length == 0)
        {
            return;
        }

        foreach (var n in _roots)
        {
            Search(n, false);
        }
    }

    private void Search(UINode n, bool parentMatched)
    {
        var whole = parentMatched || Utils.TextMatch(n.Name, _searchText) || TagsMatch(n.Tags);
        var hasSettings = n.PrePullHintModules.Count > 0 || n.Node != null && SettingsRenderer.NodeHasSettings(n.Node);
        if (hasSettings)
        {
            var result = new SearchResult(n, whole);
            var hintCount = n.PrePullHintModules.Count;
            foreach (var info in n.PrePullHintModules)
            {
                if (whole || Utils.TextMatch(PrePullHintSettingLabel(info, hintCount > 1), _searchText))
                {
                    result.Hints.Add(info);
                }
            }

            if (!whole && n.Node != null)
            {
                foreach (var field in SettingsRenderer.View(n.Node.GetType()).Fields)
                {
                    if (field.Section.Length > 0 && Utils.TextMatch(field.Section, _searchText) || Utils.TextMatch(field.Label, _searchText) || TagsMatch(field.Display.Tags))
                    {
                        result.Fields.Add(field.Meta.Name);
                    }
                }
            }

            if (whole || result.Fields.Count > 0 || result.Hints.Count > 0)
            {
                _searchResults.Add(result);
            }
        }

        foreach (var c in n.Children)
        {
            Search(c, whole);
        }
    }

    private bool TagsMatch(string[] tags)
    {
        foreach (var tag in tags)
        {
            if (Utils.TextMatch(tag, _searchText))
            {
                return true;
            }
        }
        return false;
    }

    private void DrawSearchResults()
    {
        if (_searchResults.Count == 0)
        {
            M3Widgets.EmptyState("##bmr_search_empty", "No settings found", $"Nothing matches \"{_searchText}\". Try a shorter or different word.", FontAwesomeIcon.Search);
            return;
        }

        var shown = 0;
        foreach (var r in _searchResults)
        {
            if (++shown > MaxSearchResults)
            {
                M3Text.Draw($"Showing the first {MaxSearchResults} of {_searchResults.Count} groups. Refine the search to see the rest.", M3TextStyle.Muted, true);
                break;
            }

            var root = RootOf(r.Node);
            using var card = M3Card.Begin($"search_{r.Node.PathText}", r.Node.PathText, root.Icon != FontAwesomeIcon.None ? root.Icon : FontAwesomeIcon.Cog, M3.AccentFor(shown - 1), M3CardStyle.Outlined);
            foreach (var info in r.Hints)
            {
                SettingsRenderer.DrawPrePullHintSetting(info, PrePullHintSettingLabel(info, r.Node.PrePullHintModules.Count > 1));
            }

            if (r.Node.Node != null)
            {
                SettingsRenderer.DrawNode(r.Node.Node, _root, _tree, _ws, r.WholeNode ? null : r.Fields);
            }

            var page = _pages.Find(p => p.Node == root);
            if (page != null && M3SettingRow.NavigationRow($"##jump_{r.Node.PathText}", $"Open {page.Label}", null, FontAwesomeIcon.ExternalLinkAlt, FontAwesomeIcon.ChevronRight, M3.Scheme.Tertiary))
            {
                for (var n = r.Node; n != root && n != null; n = n.Parent)
                {
                    _expanded[n] = true;
                }
                _page = page;
                ClearSearch();
                break;
            }
        }
    }

    private static UINode RootOf(UINode n)
    {
        while (n.Parent != null)
        {
            n = n.Parent;
        }
        return n;
    }

    private static string PrePullHintSettingLabel(BossModuleRegistry.Info info, bool disambiguate)
        => disambiguate ? $"Show pre-fight hint popup for {GenerateNodeName(info.ModuleType)}" : "Show pre-fight hint popup for this encounter";

    private static Type ExpansionConfigType(BossModuleInfo.Expansion expansion) => expansion switch
    {
        BossModuleInfo.Expansion.RealmReborn => typeof(RealmReborn.RealmRebornConfig),
        BossModuleInfo.Expansion.Heavensward => typeof(Heavensward.HeavenswardConfig),
        BossModuleInfo.Expansion.Stormblood => typeof(Stormblood.StormbloodConfig),
        BossModuleInfo.Expansion.Shadowbringers => typeof(Shadowbringers.ShadowbringersConfig),
        BossModuleInfo.Expansion.Endwalker => typeof(Endwalker.EndwalkerConfig),
        BossModuleInfo.Expansion.Dawntrail => typeof(Dawntrail.DawntrailConfig),
        BossModuleInfo.Expansion.Global => typeof(Global.GlobalConfig),
        _ => typeof(ModuleConfig)
    };

    private static string GenerateNodeName(Type t) => t.Name.EndsWith("Config", StringComparison.Ordinal) ? t.Name[..^"Config".Length] : t.Name;

    private static void SortByOrder(List<UINode> nodes)
    {
        nodes.Sort(static (a, b) =>
        {
            var order = a.Order.CompareTo(b.Order);
            if (order != 0)
                return order;

            if (a.SupportedFightOrder.HasValue != b.SupportedFightOrder.HasValue)
                return a.SupportedFightOrder.HasValue ? 1 : -1;

            if (a.SupportedFightOrder is { } aSupported && b.SupportedFightOrder is { } bSupported)
            {
                var supportedOrder = aSupported.CompareTo(bSupported);
                if (supportedOrder != 0)
                    return supportedOrder;
            }

            return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
        });
        foreach (var n in nodes)
        {
            SortByOrder(n.Children);
        }
    }
}
