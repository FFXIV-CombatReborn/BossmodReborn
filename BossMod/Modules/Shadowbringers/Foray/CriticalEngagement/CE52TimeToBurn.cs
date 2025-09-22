namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE52TimeToBurn;

public enum OID : uint
{
    Boss = 0x31C8, // R9.0
    Clock = 0x1EB17A, // R0.5
    TimeBomb1 = 0x1EB17B, // R0.5
    TimeBomb2 = 0x1EB1D4, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target

    TimeEruption = 23953, // Boss->self, 3.0s cast, single-target, visual
    PauseTime = 23954, // Boss->self, 3.0s cast, single-target, visual
    StartTime = 23955, // Boss->self, 3.0s cast, single-target, visual
    TimeEruptionAOE = 23956, // Helper->location, no cast, range 20 width 20 rect aoe
    TimeBomb = 23957, // Boss->self, 3.0s cast, single-target, visual
    TimeBombAOE = 23958, // Helper->self, 1.0s cast, range 60 90-degree cone
    Reproduce = 24809, // Boss->self, 3.0s cast, single-target, visual
    CrimsonCyclone = 23959, // Boss->self, 10.0s cast, range 60 width 20 rect aoe
    Eruption = 23960, // Helper->location, 3.0s cast, range 8 circle aoe
    FireTankbuster = 23961, // Boss->player, 5.0s cast, single-target
    FireRaidwide = 23962, // Boss->self, 5.0s cast, single-target, visual
    FireRaidwideAOE = 23963 // Helper->self, no cast, ???
}

// these three main mechanics can overlap in a complex way, so we have a single component to handle them. Potential options:
// - eruption only (fast/slow clocks): visual cast -> 9 eobjanims -> pause cast -> start cast -> resolve
// - bombs only (cones): visual cast -> spawn bombs -> countdown eobjanims -> resolve
// - reproduce only (clone charges): visual cast -> resolve
// - bombs only (x3 instead of x2)
// - complex: eruption visual -> 9 eruption eobjanims -> pause cast -> bomb visual -> spawn bombs -> reproduce visual & bomb countdown -> cyclone cast start -> bomb resolve -> cyclone resolve -> start cast -> eruption resolve
// => rules: show bombs if active (activate by visual, deactivate by resolve, show for each object); otherwise show cyclone cast if active; otherwise show eruptions
sealed class TimeEruptionBombReproduce(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _bombsActivation;
    private DateTime _eruptionStart; // timestamp of StartTime cast start
    private readonly List<Actor> _bombs = module.Enemies((uint)OID.TimeBomb1); // either 1 or 2 works, dunno what's the difference
    private readonly List<AOEInstance> _cycloneAOEs = [];
    private readonly List<(WPos pos, double delay)> _clocks = [];
    private readonly List<WPos> _eruptionSafeSpots = [];

    private static readonly AOEShapeCone _shapeBomb = new(60f, 45f.Degrees());
    private static readonly AOEShapeRect _shapeCyclone = new(60f, 10f);
    private static readonly AOEShapeRect _shapeEruption = new(10f, 10f, 10f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_bombsActivation != default)
        {
            var countB = _bombs.Count;
            var aoesB = new AOEInstance[countB];
            for (var i = 0; i < countB; ++i)
            {
                var b = _bombs[i];
                aoesB[i] = new(_shapeBomb, b.Position, b.Rotation, _bombsActivation);
            }
            return aoesB;
        }
        else if (_cycloneAOEs.Count != 0)
        {
            return CollectionsMarshal.AsSpan(_cycloneAOEs);
        }
        else if (_eruptionStart != default)
        {
            var countC = _clocks.Count;
            var max = countC > 2 ? countC - 2 : countC;
            var aoesC = new AOEInstance[max];
            for (var i = 0; i < max; ++i)
            {
                var e = _clocks[i];
                aoesC[i] = new(_shapeEruption, e.pos, default, _eruptionStart.AddSeconds(e.delay));
            }
            return aoesC;
        }
        return [];
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var count = _eruptionSafeSpots.Count;
        for (var i = 0; i < count; ++i)
            _shapeEruption.Draw(Arena, _eruptionSafeSpots[i], default, Colors.SafeFromAOE);
        base.DrawArenaBackground(pcSlot, pc);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TimeBomb:
                if (caster == Module.PrimaryActor)
                    _bombsActivation = WorldState.FutureTime(23.2d);
                break;
            case (uint)AID.CrimsonCyclone:
                _cycloneAOEs.Add(new(_shapeCyclone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                break;
            case (uint)AID.StartTime:
                _eruptionStart = WorldState.CurrentTime;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TimeBombAOE:
                _bombsActivation = default;
                break;
            case (uint)AID.CrimsonCyclone:
                _cycloneAOEs.RemoveAt(0);
                break;
            case (uint)AID.TimeEruptionAOE:
                _eruptionSafeSpots.Clear();
                var count = _clocks.Count;
                for (var i = 0; i < count; ++i)
                {
                    if (_clocks[i].pos.AlmostEqual(caster.Position, 1f))
                    {
                        _clocks.RemoveAt(i);
                        break;
                    }
                }
                if (_clocks.Count == 0)
                    _eruptionStart = default;
                break;
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID != (uint)OID.Clock)
            return;

        var delay = state switch
        {
            0x00010002u => 9.7d,
            0x00010020u => 11.7d,
            _ => default
        };
        if (delay == default)
            return;

        _clocks.Add((actor.Position, delay));
        if (_clocks.Count == 9)
        {
            _clocks.Sort(static (a, b) => a.delay.CompareTo(b.delay));
            var count = _clocks.Count;
            if (count >= 2)
            {
                _eruptionSafeSpots.Add(_clocks[count - 2].pos);
                _eruptionSafeSpots.Add(_clocks[count - 1].pos);
            }
        }
    }
}

sealed class Eruption(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Eruption, 8f);
sealed class FireTankbuster(BossModule module) : Components.SingleTargetCast(module, (uint)AID.FireTankbuster);
sealed class FireRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.FireRaidwide);

sealed class CE52TimeToBurnStates : StateMachineBuilder
{
    public CE52TimeToBurnStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TimeEruptionBombReproduce>()
            .ActivateOnEnter<Eruption>()
            .ActivateOnEnter<FireTankbuster>()
            .ActivateOnEnter<FireRaidwide>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 778, NameID = 26)] // bnpcname=9930
public sealed class CE52TimeToBurn(WorldState ws, Actor primary) : BossModule(ws, primary, new(-550f, default), new ArenaBoundsSquare(29.5f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InSquare(Arena.Center, 30f);
}
