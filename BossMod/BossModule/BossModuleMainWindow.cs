using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace BossMod;

public sealed class BossModuleMainWindow : UIWindow
{
    private readonly BossModuleManager _mgr;
    private readonly ZoneModuleManager _zmm;
    private bool _shouldRecenter;

    private const string _windowID = "###Boss module";

    public BossModuleMainWindow(BossModuleManager mgr, ZoneModuleManager zmm) : base(_windowID, false, new(400f, 400f))
    {
        _mgr = mgr;
        _zmm = zmm;
        RespectCloseHotkey = false;
        TitleBarButtons.Add(new() { Icon = FontAwesomeIcon.Cog, IconOffset = new(1), Click = _ => OpenModuleConfig() });
    }

    public override void PreOpenCheck()
    {
        var showZoneModule = ShowZoneModule();
        IsOpen = BossModuleManager.Config.EnableRadar && (_mgr.LoadedModules.Count > 0 || showZoneModule);
        ShowCloseButton = _mgr.ActiveModule != null && !showZoneModule;
        WindowName = (showZoneModule ? $"Zone module ({_zmm.ActiveModule?.GetType().Name})" : _mgr.ActiveModule != null ? $"Boss module ({_mgr.ActiveModule.GetType().Name})" : "Loaded boss modules") + _windowID;
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        if (BossModuleManager.Config.TrishaMode)
        {
            Flags |= ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground;
        }

        if (BossModuleManager.Config.Lock)
        {
            Flags |= ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoInputs;
        }

        ForceMainWindow = BossModuleManager.Config.TrishaMode; // NoBackground flag without ForceMainWindow works incorrectly for whatever reason

        if (BossModuleManager.Config.ShowWorldArrows && _mgr.ActiveModule != null && _mgr.WorldState.Party[PartyState.PlayerSlot] is var pc && pc != null)
        {
            DrawMovementHints(_mgr.ActiveModule.CalculateMovementHintsForRaidMember(PartyState.PlayerSlot, pc), pc.PosRot.Y);
        }

        if (!BossModuleManager.Config.EnableRadar && BossModuleManager.Config.ProjectRadarInto3DWorld)
        {
            var module = _mgr.ActiveModule;
            if (module == null)
            {
                return;
            }

            module.DrawWorldProjection(_mgr.WorldState.Client.CameraAzimuth, PartyState.PlayerSlot);
        }
    }

    public override void OnOpen() => Service.Log($"[BMM] Opening main window; there are {_mgr.LoadedModules.Count} loaded modules, active is {_mgr.ActiveModule?.GetType().FullName ?? "<n/a>"}; zone module is {_zmm.ActiveModule?.GetType().FullName ?? "<n/a>"}");

    public override void OnClose() => Service.Log($"[BMM] Closing main window; there are {_mgr.LoadedModules.Count} loaded modules, active is {_mgr.ActiveModule?.GetType().FullName ?? "<n/a>"}; zone module is {_zmm.ActiveModule?.GetType().FullName ?? "<n/a>"}");

    public override void PreDraw()
    {
        if (_shouldRecenter)
        {
            var viewport = ImGui.GetMainViewport();
            var windowSize = Size ?? new Vector2(400f, 400f);
            var center = viewport.Pos + viewport.Size * 0.5f;
            var newPos = center - windowSize * 0.5f;
            ImGui.SetNextWindowPos(newPos, ImGuiCond.Always);
            _shouldRecenter = false;
        }
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
                var primary = m.PrimaryActor;
                var oid = primary.OID;
                var oidType = BossModuleRegistry.FindByOID(oid)?.ObjectIDType;
                var oidName = oidType?.GeneratedEnumName(oid);
                if (ImGui.Button($"{m.GetType()} ({primary.InstanceID:X} '{primary.Name}' {oidName})"))
                {
                    _mgr.ActiveModule = m;
                }
            }
        }
    }

    private void DrawMovementHints(BossComponent.MovementHints? arrows, float y)
    {
        if (arrows == null || Camera.Instance == null)
        {
            return;
        }
        var count = arrows.Count;
        for (var i = 0; i < count; ++i)
        {
            var a = arrows[i];
            Camera.Instance.DrawWorldArrow(a.Item1.ToVec3(y), a.Item2.ToVec3(y), a.Item3, shaftWidth: 0.25f, headLength: 0.8f, headWidth: 0.7f, projectionHeight: 2.5f);
        }
    }

    private void OpenModuleConfig()
    {
        if (_mgr.ActiveModule?.Info != null)
        {
            BossModuleConfigWindow.Open(_mgr.ActiveModule.Info, _mgr.WorldState);
        }
    }

    public void RecenterWindow()
    {
        _shouldRecenter = true;
        MiniArena.Config.RadarResize = true;
    }

    private bool ShowZoneModule() => BossModuleManager.Config.ShowGlobalHints && !BossModuleManager.Config.HintsInSeparateWindow && _mgr.ActiveModule?.StateMachine.ActivePhase == null && (_zmm.ActiveModule?.WantDrawHints() ?? false);
}
