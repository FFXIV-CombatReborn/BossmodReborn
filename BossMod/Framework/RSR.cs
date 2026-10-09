using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
namespace BossMod;

public sealed class RotationSolverRebornModule : IDisposable
{
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly ICallGateSubscriber<SpecialCommandType, object> _changeOperationMode;
    private readonly ICallGateSubscriber<SpecialCommandType, float, object> _triggerSpecialStateWithDuration;
    private readonly ICallGateSubscriber<byte> _getDesiredPositional;
    private readonly ICallGateSubscriber<byte, object?> _desiredPositionalChanged;
    private readonly ICallGateSubscriber<bool> _autorotationActive;
    private const string rsr = "Rotation Solver Reborn";

    // current value of RSR's desired positional, kept up to date via the DesiredPositionalChanged event
    public Positional DesiredPositional { get; private set; } = Positional.Any;

    public event Action<Positional>? DesiredPositionalChanged;

    public RotationSolverRebornModule(IDalamudPluginInterface pluginInterface)
    {
        _pluginInterface = pluginInterface;
        _changeOperationMode = pluginInterface.GetIpcSubscriber<SpecialCommandType, object>("RotationSolverReborn.TriggerSpecialState");
        _triggerSpecialStateWithDuration = pluginInterface.GetIpcSubscriber<SpecialCommandType, float, object>("RotationSolverReborn.TriggerSpecialStateWithDuration");
        _getDesiredPositional = pluginInterface.GetIpcSubscriber<byte>("RotationSolverReborn.GetDesiredPositional");
        _desiredPositionalChanged = pluginInterface.GetIpcSubscriber<byte, object?>("RotationSolverReborn.ActionUpdater.DesiredPositionalChanged");
        _autorotationActive = pluginInterface.GetIpcSubscriber<bool>("RotationSolverReborn.AutorotationActive");
        try
        {
            _desiredPositionalChanged.Subscribe(OnDesiredPositionalChanged);
        }
        catch
        {
            // RSR not installed/loaded yet - ignore, we'll still be able to poll GetDesiredPositional() later
        }
        DesiredPositional = GetDesiredPositional();
    }

    public void Dispose()
    {
        try
        {
            _desiredPositionalChanged.Unsubscribe(OnDesiredPositionalChanged);
        }
        catch
        {
            // ignore
        }
    }

    public bool IsInstalled
    {
        get
        {
            var installedPlugins = _pluginInterface.InstalledPlugins;
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

    public void PauseRSR() => _changeOperationMode.InvokeAction(SpecialCommandType.NoCasting);

    public void UnPauseRSR() => _changeOperationMode.InvokeAction(SpecialCommandType.EndSpecial);

    public void TriggerSpecialStateWithDuration(SpecialCommandType specialCommand, float duration) => _triggerSpecialStateWithDuration.InvokeAction(specialCommand, duration);

    public bool IsAutorotationActive
    {
        get
        {
            try
            {
                return _autorotationActive.HasFunction && _autorotationActive.InvokeFunc();
            }
            catch
            {
                return false;
            }
        }
    }

    // polls RSR's current desired positional (0=None, 1=Rear, 2=Flank, 3=Front); returns Any if RSR is not installed/loaded
    public Positional GetDesiredPositional()
    {
        try
        {
            return MapPositional(_getDesiredPositional.InvokeFunc());
        }
        catch
        {
            return Positional.Any;
        }
    }

    private void OnDesiredPositionalChanged(byte value)
    {
        DesiredPositional = MapPositional(value);
        DesiredPositionalChanged?.Invoke(DesiredPositional);
    }

    //I cant be fucked to rework this on RSRs side, so we just map the values here
    private static Positional MapPositional(byte value) => value switch
    {
        1 => Positional.Rear,
        2 => Positional.Flank,
        3 => Positional.Front,
        _ => Positional.Any,
    };

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
}
