﻿namespace BossMod;

// utility for building state machines for boss modules
// conventions for id:
// - high word (mask 0xFFFF0000) is used for high level groups - states sharing high word are grouped together
// - high byte (mask 0xFF000000) is used for independent subsequences (e.g. forks and phases) - states sharing high byte belong to same subsequence
// - fourth nibble (mask 0x0000F0000) is used for independent large-scale mechanics that are still parts of the same logical group (typically having single name)
// - first nibble (mask 0x0000000F) is used for smallest possible states (e.g. cast-start + cast-end)
// - second and third nibble can be used by modules needing more hierarchy levels
// this is all done to provide ids that are relatively stable across refactorings (these are used e.g. for cooldown planning)
public class StateMachineBuilder(BossModule module)
{
    // wrapper that simplifies building phases
    public class Phase(StateMachine.Phase raw, BossModule module)
    {
        public StateMachine.Phase Raw => raw;

        public Phase OnEnter(Action action, bool condition = true)
        {
            if (condition)
                Raw.Enter += action;
            return this;
        }

        public Phase OnExit(Action action, bool condition = true)
        {
            if (condition)
                Raw.Exit += action;
            return this;
        }

        // note: usually components are deactivated automatically on phase change - manual deactivate is needed only for components that opt out of this (useful for components that need to maintain state across multiple phases)
        public Phase ActivateOnEnter<C>(bool condition = true) where C : BossComponent => OnEnter(module.ActivateComponent<C>, condition);
        public Phase DeactivateOnEnter<C>(bool condition = true) where C : BossComponent => OnEnter(module.DeactivateComponent<C>, condition); // TODO: reconsider...
        public Phase DeactivateOnExit<C>(bool condition = true) where C : BossComponent => OnExit(module.DeactivateComponent<C>, condition);

        public Phase SetHint(StateMachine.PhaseHint h, bool condition = true)
        {
            if (condition)
                Raw.Hint |= h;
            return this;
        }
    }

    // wrapper that simplifies building states
    public class State(StateMachine.State raw, BossModule module)
    {
        public StateMachine.State Raw => raw;

        public State OnEnter(Action action, bool condition = true)
        {
            if (condition)
                Raw.Enter += action;
            return this;
        }

        public State OnExit(Action action, bool condition = true)
        {
            if (condition)
                Raw.Exit += action;
            return this;
        }

        public State ActivateOnEnter<C>(bool condition = true) where C : BossComponent => OnEnter(module.ActivateComponent<C>, condition);
        public State ActivateOnExit<C>(bool condition = true) where C : BossComponent => OnExit(module.ActivateComponent<C>, condition);
        public State DeactivateOnEnter<C>(bool condition = true) where C : BossComponent => OnEnter(module.DeactivateComponent<C>, condition);
        public State DeactivateOnExit<C>(bool condition = true) where C : BossComponent => OnExit(module.DeactivateComponent<C>, condition);
        public State ExecOnEnter<C>(Action<C> fn, bool condition = true) where C : BossComponent => OnEnter(ExecForComponent(fn), condition);
        public State ExecOnExit<C>(Action<C> fn, bool condition = true) where C : BossComponent => OnExit(ExecForComponent(fn), condition);
        public State ResetComp<C>(bool condition = true) where C : BossComponent
        {
            OnExit(module.DeactivateComponent<C>, condition);
            return OnExit(module.ActivateComponent<C>, condition);
        }

        public State SetHint(StateMachine.StateHint h, bool condition = true)
        {
            if (condition)
                Raw.EndHint |= h;
            return this;
        }

        public State ClearHint(StateMachine.StateHint h, bool condition = true)
        {
            if (condition)
                Raw.EndHint &= ~h;
            return this;
        }

        private Action ExecForComponent<C>(Action<C> fn) where C : BossComponent => () =>
        {
            var c = module.FindComponent<C>();
            if (c != null)
                fn(c);
            else
                module.ReportError(null, $"Component {typeof(C)} needed for state {Raw.ID:X} was not found");
        };
    }

    protected BossModule Module = module;
    private readonly List<StateMachine.Phase> _phases = [];
    private StateMachine.State? _curInitial;
    private StateMachine.State? _lastState;
    private readonly Dictionary<uint, StateMachine.State> _states = [];

    public StateMachine Build() => new(_phases);

    // create a simple phase; buildState is called to fill out phase states, argument is seqID << 24
    // note that on exit, by default all components are removed (except those that opt out of this explicitly), since generally phase transition can happen at any time
    public Phase SimplePhase(uint seqID, Action<uint> buildState, string name, float dur = -1)
    {
        if (_curInitial != null)
            throw new InvalidOperationException($"Trying to create phase '{name}' while inside another phase");
        buildState(seqID << 24);
        if (_curInitial == null)
            throw new InvalidOperationException($"Phase '{name}' has no states");
        var phase = new StateMachine.Phase(_curInitial, name, dur);
        phase.Exit += () => Module.ClearComponents(comp => !comp.KeepOnPhaseChange);
        _phases.Add(phase);
        _curInitial = _lastState = null;
        return new(phase, Module);
    }

    // create a phase for typical single-phase fight (triggered by primary actor dying)
    public Phase DeathPhase(uint seqID, Action<uint> buildState)
    {
        var phase = SimplePhase(seqID, buildState, "Boss death");
        phase.Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed || Module.PrimaryActor.HPMP.CurHP == 0;
        return phase;
    }

    // create a single-state phase; useful for modules with trivial state machines
    public Phase TrivialPhase(uint seqID = default, float enrage = 10000f) => DeathPhase(seqID, id => SimpleState(id, enrage, "Enrage"));

    // create a simple state without any actions
    public State SimpleState(uint id, float duration, string name)
    {
        while (_states.ContainsKey(id))
        {
            Service.Log($"Duplicate state id {id:X}, incrementing by 1");
            ++id;
        }

        var state = _states[id] = new() { ID = id, Duration = duration, Name = name };
        if (_lastState != null)
        {
            if (_lastState.NextStates != null)
                throw new InvalidOperationException($"Previous state {_lastState.ID} is already linked while adding new state {id}");

            _lastState.NextStates = [state];
            if ((_lastState.ID & 0xFFFF0000) == (id & 0xFFFF0000))
                _lastState.EndHint |= StateMachine.StateHint.GroupWithNext;
        }
        else
        {
            _curInitial = _curInitial == null ? state : throw new InvalidOperationException($"Failed to link new state {id}");
        }

        _lastState = state;
        return new(state, Module);
    }

    // create a state triggered by timeout
    public State Timeout(uint id, float duration, string name = "")
    {
        var state = SimpleState(id, duration, name);
        state.Raw.Comment = "Timeout";
        state.Raw.Update = timeSinceTransition => timeSinceTransition >= state.Raw.Duration ? 0 : -1;
        return state;
    }

    // create a state triggered by custom condition, or if it doesn't happen, by timeout
    public State Condition(uint id, float expected, Func<bool> condition, string name = "", float maxOverdue = 1, float checkDelay = 0)
    {
        var state = SimpleState(id, expected, name);
        state.Raw.Comment = "Generic condition";
        state.Raw.Update = timeSinceTransition =>
        {
            if (timeSinceTransition < checkDelay)
                return -1; // too early to check for condition

            if (condition())
                return 0;

            if (timeSinceTransition < expected + maxOverdue)
                return -1;

            Module.ReportError(null, $"State {id:X}: transition triggered because of overdue");
            return 0;
        };
        return state;
    }

    // create a fork state that checks passed condition; when it returns non-null, next state is one built by corresponding action in dispatch map
    public State ConditionFork<Key>(uint id, float expected, Func<bool> condition, Func<Key> select, Dictionary<Key, (uint seqID, Action<uint> buildState)> dispatch, string name = "")
        where Key : notnull
    {
        Dictionary<Key, int> stateDispatch = [];

        var state = SimpleState(id, expected, name);
        state.Raw.Comment = $"Fork: [{string.Join(", ", dispatch.Keys)}]";
        state.Raw.NextStates = new StateMachine.State[dispatch.Count];
        state.Raw.Update = _ =>
        {
            if (!condition())
                return -1;

            var key = select();
            var fork = stateDispatch.GetValueOrDefault(key, -1);
            if (fork < 0)
                Module.ReportError(null, $"State {id:X}: unexpected fork condition result: got {key}");
            return fork;
        };

        int nextIndex = 0;
        var prevInit = _curInitial;
        foreach (var (key, action) in dispatch)
        {
            _lastState = _curInitial = null;
            action.buildState(action.seqID << 24);
            if (_curInitial == null)
                throw new InvalidOperationException($"Fork #{nextIndex} didn't create any states");
            state.Raw.NextStates[nextIndex] = _curInitial;
            stateDispatch[key] = nextIndex++;
        }
        _curInitial = prevInit;
        _lastState = null;

        return state;
    }

    // create a state triggered by component condition (or timeout if it never happens); if component is not present, error is logged and transition is triggered immediately
    public State ComponentCondition<T>(uint id, float expected, Func<T, bool> condition, string name = "", float maxOverdue = 1, float checkDelay = 0) where T : BossComponent
    {
        var state = SimpleState(id, expected, name);
        state.Raw.Comment = $"Condition on {typeof(T).Name}";
        state.Raw.Update = (timeSinceTransition) =>
        {
            if (timeSinceTransition < checkDelay)
                return -1; // too early to check for condition

            var comp = Module.FindComponent<T>();
            if (comp == null)
            {
                Module.ReportError(null, $"State {id:X}: component {typeof(T)} needed for condition is missing");
                return 0;
            }

            if (condition(comp))
                return 0;

            if (timeSinceTransition < expected + maxOverdue)
                return -1;

            Module.ReportError(null, $"State {id:X}: transition triggered because of overdue");
            return 0;
        };
        return state;
    }

    // create a fork state triggered by component condition
    public State ComponentConditionFork<T, Key>(uint id, float expected, Func<T, bool> condition, Func<T, Key> select, Dictionary<Key, (uint seqID, Action<uint> buildState)> dispatch, string name = "")
        where T : BossComponent
        where Key : notnull
    {
        bool cond()
        {
            var comp = Module.FindComponent<T>();
            if (comp == null)
            {
                Module.ReportError(null, $"State {id:X}: component {typeof(T)} needed for condition is missing");
                return false;
            }
            return condition(comp);
        }
        return ConditionFork(id, expected, cond, () => select(Module.FindComponent<T>()!), dispatch, name);
    }

    // create a state triggered by expected cast start by arbitrary actor; unexpected casts still trigger a transition, but log error
    public State ActorCastStart(uint id, Func<Actor?> actorAcc, uint aid, float delay, bool isBoss = false, string name = "")
    {
        var state = SimpleState(id, delay, name).SetHint(StateMachine.StateHint.BossCastStart, isBoss);
        state.Raw.Comment = $"Cast start: {aid}";
        state.Raw.Update = _ =>
        {
            var castInfo = actorAcc()?.CastInfo;
            if (castInfo == null)
                return -1;
            if (castInfo.Action.ID != aid)
                Module.ReportError(null, $"State {id:X}: unexpected cast start: got {castInfo.Action}, expected {aid}");
            return 0;
        };
        return state;
    }

    // create a state triggered by expected cast start by a primary actor; unexpected casts still trigger a transition, but log error
    public State CastStart(uint id, uint aid, float delay, string name = "")
        => ActorCastStart(id, () => Module.PrimaryActor, aid, delay, true, name);

    // create a state triggered by one of a set of expected casts by arbitrary actor; unexpected casts still trigger a transition, but log error
    public State ActorCastStartMulti(uint id, Func<Actor?> actorAcc, uint[] aids, float delay, bool isBoss = false, string name = "")
    {
        var state = SimpleState(id, delay, name).SetHint(StateMachine.StateHint.BossCastStart, isBoss);
        state.Raw.Comment = $"Cast start: [{string.Join(", ", aids)}]";
        state.Raw.Update = _ =>
        {
            var castInfo = actorAcc()?.CastInfo;
            if (castInfo == null)
                return -1;
            if (!aids.Any(aid => aid == castInfo.Action.ID))
                Module.ReportError(null, $"State {id:X}: unexpected cast start: got {castInfo.Action}");
            return 0;
        };
        return state;
    }

    // create a state triggered by one of a set of expected casts by a primary actor; unexpected casts still trigger a transition, but log error
    public State CastStartMulti(uint id, uint[] aids, float delay, string name = "")
        => ActorCastStartMulti(id, () => Module.PrimaryActor, aids, delay, true, name);

    // create a state triggered by one of a set of expected casts by arbitrary actor, each of which forking to a separate subsequence
    // values in map are actions building state chains corresponding to each fork
    public State ActorCastStartFork(uint id, Func<Actor?> actorAcc, Dictionary<uint, (uint seqID, Action<uint> buildState)> dispatch, float delay, bool isBoss = false, string name = "")
        => ConditionFork(id, delay, () => actorAcc()?.CastInfo?.IsSpell() ?? false, () => actorAcc()!.CastInfo!.Action.ID, dispatch, name)
            .SetHint(StateMachine.StateHint.BossCastStart, isBoss);

    // create a state triggered by one of a set of expected casts by a primary actor, each of which forking to a separate subsequence
    // values in map are actions building state chains corresponding to each fork
    public State CastStartFork(uint id, Dictionary<uint, (uint seqID, Action<uint> buildState)> dispatch, float delay, string name = "")
        => ActorCastStartFork(id, () => Module.PrimaryActor, dispatch, delay, true, name);

    // create a state triggered by cast end by arbitrary actor
    public State ActorCastEnd(uint id, Func<Actor?> actorAcc, float castTime, bool isBoss = false, string name = "", bool interruptible = false)
    {
        var state = SimpleState(id, castTime, name).SetHint(StateMachine.StateHint.BossCastEnd, isBoss);
        state.Raw.Comment = interruptible ? "Interruptible cast end" : "Cast end";
        state.Raw.Update = timeSinceTransition => actorAcc()?.CastInfo == null && (!interruptible || timeSinceTransition >= state.Raw.Duration) ? 0 : -1;
        return state;
    }

    // create a state triggered by cast end by a primary actor
    public State CastEnd(uint id, float castTime, string name = "", bool interruptible = false)
        => ActorCastEnd(id, () => Module.PrimaryActor, castTime, true, name, interruptible);

    // create a chain of states: ActorCastStart -> ActorCastEnd; second state uses id+1
    public State ActorCast(uint id, Func<Actor?> actorAcc, uint aid, float delay, float castTime, bool isBoss = false, string name = "", bool interruptible = false)
    {
        ActorCastStart(id, actorAcc, aid, delay, isBoss, "");
        return ActorCastEnd(id + 1, actorAcc, castTime, isBoss, name, interruptible);
    }

    // create a chain of states: CastStart -> CastEnd; second state uses id+1
    public State Cast(uint id, uint aid, float delay, float castTime, string name = "", bool interruptible = false)
    {
        CastStart(id, aid, delay, "");
        return CastEnd(id + 1, castTime, name, interruptible);
    }

    // create a chain of states: ActorCastStartMulti -> ActorCastEnd; second state uses id+1
    public State ActorCastMulti(uint id, Func<Actor?> actorAcc, uint[] aids, float delay, float castTime, bool isBoss = false, string name = "", bool interruptible = false)
    {
        ActorCastStartMulti(id, actorAcc, aids, delay, isBoss, "");
        return ActorCastEnd(id + 1, actorAcc, castTime, isBoss, name, interruptible);
    }

    // create a chain of states: CastStartMulti -> CastEnd; second state uses id+1
    public State CastMulti(uint id, uint[] aids, float delay, float castTime, string name = "", bool interruptible = false)
    {
        CastStartMulti(id, aids, delay, "");
        return CastEnd(id + 1, castTime, name, interruptible);
    }

    // create a state triggered by arbitrary actor becoming (un)targetable
    public State ActorTargetable(uint id, Func<Actor?> actorAcc, bool targetable, float delay, string name = "", float checkDelay = 0)
    {
        var state = SimpleState(id, delay, name);
        state.Raw.Comment = targetable ? "Targetable" : "Untargetable";
        state.Raw.Update = timeSinceTransition => timeSinceTransition >= checkDelay && actorAcc()?.IsTargetable == targetable ? 0 : -1;
        return state;
    }

    // create a state triggered by a primary actor becoming (un)targetable; automatically sets downtime begin/end flag
    public State Targetable(uint id, bool targetable, float delay, string name = "", float checkDelay = 0)
        => ActorTargetable(id, () => Module.PrimaryActor, targetable, delay, name, checkDelay)
            .SetHint(targetable ? StateMachine.StateHint.DowntimeEnd : StateMachine.StateHint.DowntimeStart);
}
