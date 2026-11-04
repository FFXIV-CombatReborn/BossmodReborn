using Dalamud.Bindings.ImGui;

namespace BossMod;

public sealed class BossModuleConfigWindow : UIWindow
{
    private readonly BossModuleRegistry.Info _info;
    private readonly ConfigNode? _node;
    private readonly PartyRolesConfig _prc = Service.Config.Get<PartyRolesConfig>();
    private readonly WorldState _ws;
    private readonly UITree _tree = new();
    private readonly UITabs _tabs = new("ConfigTabs");

    public BossModuleConfigWindow(BossModuleRegistry.Info info, WorldState ws) : base($"{info.ModuleType.Name} config", true, new(1200, 800))
    {
        _info = info;
        _node = info.ConfigType != null ? Service.Config.Get<ConfigNode>(info.ConfigType) : null;
        _ws = ws;
        _tabs.Add("Encounter-specific config", DrawEncounterTab);
        _tabs.Add("Party roles assignment", DrawPartyRolesAssignmentsTab);
    }

    public static void Open(BossModuleRegistry.Info info, WorldState ws)
    {
        if (!RebornUI.MaterialUI.OpenEncounterSettings(info, ws))
        {
            _ = new BossModuleConfigWindow(info, ws);
        }
    }

    public override void Draw() => _tabs.Draw();

    private void DrawEncounterTab()
    {
        if (_info.HasPrePullHints)
        {
            ConfigUI.DrawPrePullHintSetting(_info);
            if (_node != null)
                ImGui.Separator();
        }

        if (_node != null)
            ConfigUI.DrawNode(_node, Service.Config, _tree, _ws);
        else if (!_info.HasPrePullHints)
            ImGui.TextUnformatted("This module does not expose any additional configuration");
    }

    private void DrawPartyRolesAssignmentsTab()
    {
        if (_ws.Party.Player() != null)
        {
            ConfigUI.DrawNode(_prc, Service.Config, _tree, _ws);
        }
    }
}
