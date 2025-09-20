using Dalamud.Interface;
using Dalamud.Bindings.ImGui;

namespace BossMod;

[SkipLocalsInit]
public sealed class BossModuleMainWindow : UIWindow
{
    private readonly BossModuleManager _mgr;
    private readonly ZoneModuleManager _zmm;

    private const string _windowID = "###Boss module";

    public BossModuleMainWindow(BossModuleManager mgr, ZoneModuleManager zmm) : base(_windowID, false, new(400, 400))
    {
        _mgr = mgr;
        _zmm = zmm;
        RespectCloseHotkey = false;
        TitleBarButtons.Add(new() { Icon = FontAwesomeIcon.Cog, IconOffset = new(1), Click = _ => OpenModuleConfig() });
    }

    public override void PreOpenCheck()
    {
        var showZoneModule = ShowZoneModule();
        IsOpen = BossModuleManager.Config.Enable && (_mgr.LoadedModules.Count > 0 || showZoneModule);
        ShowCloseButton = _mgr.ActiveModule != null && !showZoneModule;
        WindowName = (showZoneModule ? $"Zone module ({_zmm.ActiveModule?.GetType().Name})" : _mgr.ActiveModule != null ? $"Boss module ({_mgr.ActiveModule.GetType().Name})" : "Loaded boss modules") + _windowID;
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        if (BossModuleManager.Config.TrishaMode)
            Flags |= ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground;
        if (BossModuleManager.Config.Lock)
            Flags |= ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoInputs;
        ForceMainWindow = BossModuleManager.Config.TrishaMode; // NoBackground flag without ForceMainWindow works incorrectly for whatever reason

        if (BossModuleManager.Config.ShowWorldArrows && _mgr.ActiveModule != null && _mgr.WorldState.Party[PartyState.PlayerSlot] is var pc && pc != null)
            DrawMovementHints(_mgr.ActiveModule.CalculateMovementHintsForRaidMember(PartyState.PlayerSlot, ref pc), pc.PosRot.Y);
    }

    public override void OnOpen()
    {
        Service.Log($"[BMM] Opening main window; there are {_mgr.LoadedModules.Count} loaded modules, active is {_mgr.ActiveModule?.GetType().FullName ?? "<n/a>"}; zone module is {_zmm.ActiveModule?.GetType().FullName ?? "<n/a>"}");
    }

    public override void OnClose()
    {
        Service.Log($"[BMM] Closing main window; there are {_mgr.LoadedModules.Count} loaded modules, active is {_mgr.ActiveModule?.GetType().FullName ?? "<n/a>"}; zone module is {_zmm.ActiveModule?.GetType().FullName ?? "<n/a>"}");
    }

    public override void PostDraw()
    {
        if (!IsOpen)
        {
            // user pressed close button - deactivate current module and show module list instead
            // show module list instead of boss module
            Service.Log("[BMM] Bossmod window closed by user, showing module list instead...");
            _mgr.ActiveModule = null;
            IsOpen = true;
        }
    }

    public override void Draw()
    {
        if (ShowZoneModule())
        {
            _zmm.ActiveModule?.DrawGlobalHints();
        }
        else if (_mgr.ActiveModule != null)
        {
            try
            {
                _mgr.ActiveModule.Draw(BossModuleManager.Config.RotateArena ? _mgr.WorldState.Client.CameraAzimuth : BossModuleManager.Config.FlipArena ? 180f.Degrees() : default, PartyState.PlayerSlot, !BossModuleManager.Config.HintsInSeparateWindow, true);
            }
            catch (Exception ex)
            {
                Service.Log($"Boss module draw crashed: {ex}");
                _mgr.ActiveModule = null;
            }
        }
        else
        {
            var count = _mgr.LoadedModules.Count;
            for (var i = 0; i < count; ++i)
            {
                var m = _mgr.LoadedModules[i];
                var oidType = BossModuleRegistry.FindByOID(m.PrimaryActor.OID)?.ObjectIDType;
                var oidName = oidType?.GetEnumName(m.PrimaryActor.OID);
                if (ImGui.Button($"{m.GetType()} ({m.PrimaryActor.InstanceID:X} '{m.PrimaryActor.Name}' {oidName})"))
                    _mgr.ActiveModule = m;
            }
        }
    }

    private void DrawMovementHints(BossComponent.MovementHints? arrows, float y)
    {
        if (arrows == null || arrows.Count == 0 || Camera.Instance == null)
            return;

        foreach ((var start, var end, var color) in arrows)
        {
            Vector3 start3 = start.ToVec3(y);
            Vector3 end3 = end.ToVec3(y);
            Camera.Instance.DrawWorldLine(start3, end3, color);
            var dir = Vector3.Normalize(end3 - start3);
            var arrowStart = end3 - 0.4f * dir;
            var offset = 0.07f * Vector3.Normalize(Vector3.Cross(Vector3.UnitY, dir));
            Camera.Instance.DrawWorldLine(arrowStart + offset, end3, color);
            Camera.Instance.DrawWorldLine(arrowStart - offset, end3, color);
        }
    }

    private void OpenModuleConfig()
    {
        if (_mgr.ActiveModule?.Info != null)
            _ = new BossModuleConfigWindow(_mgr.ActiveModule.Info, _mgr.WorldState);
    }

    private bool ShowZoneModule() => BossModuleManager.Config.ShowGlobalHints && !BossModuleManager.Config.HintsInSeparateWindow && _mgr.ActiveModule?.StateMachine.ActivePhase == null && (_zmm.ActiveModule?.WantDrawHints() ?? false);
}
