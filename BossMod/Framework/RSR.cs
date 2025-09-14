using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
namespace BossMod;

public sealed class RotationSolverRebornModule(IDalamudPluginInterface pluginInterface)
{
    // IPC subscribers to RSR endpoints
    private readonly ICallGateSubscriber<SpecialCommandType, object> _triggerSpecialState = pluginInterface.GetIpcSubscriber<SpecialCommandType, object>("RotationSolverReborn.TriggerSpecialState");
    private readonly ICallGateSubscriber<StateCommandType, object> _changeOperatingMode = pluginInterface.GetIpcSubscriber<StateCommandType, object>("RotationSolverReborn.ChangeOperatingMode");
    private readonly ICallGateSubscriber<StateCommandType, TargetingType, object> _autodutyChangeOperatingMode = pluginInterface.GetIpcSubscriber<StateCommandType, TargetingType, object>("RotationSolverReborn.AutodutyChangeOperatingMode");
    private readonly ICallGateSubscriber<OtherCommandType, string, object> _otherCommand = pluginInterface.GetIpcSubscriber<OtherCommandType, string, object>("RotationSolverReborn.OtherCommand");
    private readonly ICallGateSubscriber<string, float, object> _actionCommand = pluginInterface.GetIpcSubscriber<string, float, object>("RotationSolverReborn.ActionCommand");

    private const string rsr = "Rotation Solver Reborn";

    public bool IsInstalled
    {
        get
        {
            var installedPlugins = pluginInterface.InstalledPlugins;
            foreach (var x in installedPlugins)
            {
                if (x.IsLoaded && x.Name.Equals(rsr, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }

    // Convenience wrappers
    public void TriggerSpecialState(SpecialCommandType cmd) => _triggerSpecialState.InvokeAction(cmd);
    public void PauseRSR() => TriggerSpecialState(SpecialCommandType.NoCasting);
    public void UnPauseRSR() => TriggerSpecialState(SpecialCommandType.EndSpecial);
    public void ChangeOperatingMode(StateCommandType mode) => _changeOperatingMode.InvokeAction(mode);
    public void AutodutyChangeOperatingMode(StateCommandType mode, TargetingType targeting) => _autodutyChangeOperatingMode.InvokeAction(mode, targeting);
    public void OtherCommand(OtherCommandType type, string arg) => _otherCommand.InvokeAction(type, arg);
    public void ActionCommand(string action, float timeWindowSeconds) => _actionCommand.InvokeAction(action, timeWindowSeconds);

    // Mirror RSR enums for IPC signatures
    public enum SpecialCommandType : byte
    {
        EndSpecial,
        HealArea,
        HealSingle,
        DefenseArea,
        DefenseSingle,
        DispelStancePositional,
        RaiseShirk,
        MoveForward,
        MoveBack,
        AntiKnockback,
        Burst,
        Speed,
        LimitBreak,
        NoCasting,
    }

    public enum StateCommandType : byte
    {
        Off,
        Auto,
        Manual,
        AutoDuty,
    }

    public enum TargetingType : byte
    {
        Big,
        Small,
        HighHP,
        LowHP,
        HighHPPercent,
        LowHPPercent,
        HighMaxHP,
        LowMaxHP,
        Nearest,
        Farthest,
        PvPHealers,
        PvPTanks,
        PvPDPS
    }

    public enum OtherCommandType : byte
    {
        Settings,
        Rotations,
        DutyRotations,
        DoActions,
        ToggleActions,
        NextAction,
        Cycle,
    }
}
