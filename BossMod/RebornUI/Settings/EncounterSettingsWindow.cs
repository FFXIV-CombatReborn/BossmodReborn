using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using RebornMaterial;

namespace BossMod.RebornUI;

internal sealed class EncounterSettingsWindow(BossModuleRegistry.Info info, WorldState ws) : RebornWindow($"{info.ModuleType.Name} config", true, new(900, 700))
{
    private static readonly M3Tab[] _tabs = [new("Encounter", FontAwesomeIcon.SlidersH), new("Party roles", FontAwesomeIcon.Users)];

    private readonly BossModuleRegistry.Info _info = info;
    private readonly ConfigNode? _node = info.ConfigType != null ? Service.Config.Get<ConfigNode>(info.ConfigType) : null;
    private readonly PartyRolesConfig _prc = Service.Config.Get<PartyRolesConfig>();
    private readonly WorldState _ws = ws;
    private readonly UITree _tree = new();
    private int _tab;

    protected override M3Density Density => SettingsDensity;

    public override void Draw()
    {
        var clicked = M3Navigation.Tabs("##tabs", _tabs, _tab);
        if (clicked >= 0)
        {
            _tab = clicked;
        }
        ImGui.Dummy(new(0f, M3.Space2));

        using var child = ImRaii.Child("##content", -Vector2.One);
        if (!child)
        {
            return;
        }

        if (_tab == 0)
        {
            DrawEncounterTab();
        }
        else
        {
            DrawPartyRolesTab();
        }
    }

    private void DrawEncounterTab()
    {
        if (_info.HasPrePullHints)
        {
            using var card = M3Card.Begin("hints", "Pre-fight hints", FontAwesomeIcon.CommentDots, M3.Scheme.Secondary);
            SettingsRenderer.DrawPrePullHintSetting(_info);
        }

        if (_node != null)
        {
            SettingsRenderer.DrawNodeCards(_node, Service.Config, _tree, _ws, null, accentOffset: _info.HasPrePullHints ? 1 : 0);
        }
        else if (!_info.HasPrePullHints)
        {
            M3Widgets.EmptyState("##nosettings", "No settings", "This module does not expose any additional configuration.", FontAwesomeIcon.SlidersH);
        }
    }

    private void DrawPartyRolesTab()
    {
        if (_ws.Party.Player() != null)
        {
            SettingsRenderer.DrawNodeCards(_prc, Service.Config, _tree, _ws, null, FontAwesomeIcon.Users);
        }
        else
        {
            M3Widgets.EmptyState("##noplayer", "Not logged in", "Party roles can be assigned once you're logged in.", FontAwesomeIcon.Users);
        }
    }
}
