namespace BossMod.Dawntrail.Quest.Job.Viper.VengeanceOfTheViper;

public enum OID : uint
{
    Boss = 0x42A7, // R7.5
    RightWing = 0x4423, // R2.5
    LeftWing = 0x4424, // R2.5
    BallOfFire = 0x42A8, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player/Keshkwa, no cast, single-target
    Teleport = 38737, // Boss->location, no cast, single-target

    BitingScratch1 = 38721, // Boss->self, 6.0s cast, range 40 90-degree cone
    BitingScratch2 = 39552, // Boss->self, 11.0s cast, range 40 90-degree cone

    FervidImpactVisual = 38716, // Boss->self, no cast, single-target
    FrigidPulseVisual = 38718, // Boss->self, no cast, single-target
    FervidImpact = 38715, // Boss->self, 5.0s cast, range 12 circle
    FrigidPulse = 38717, // Boss->self, 5.0s cast, range 12-60 donut
    StormkissedFlamesIn = 38795, // Boss->self, 4.0s cast, range 12-60 donut
    StormkissedFlamesOut = 38719, // Helper->self, 6.4s cast, range 12 circle
    FirestormCycleOut = 38794, // Boss->self, 4.0s cast, range 12 circle
    FirestormCycleIn = 38720, // Helper->self, 6.4s cast, range 12-60 donut

    SwoopingFrenzy1 = 38714, // Boss->location, 5.0s cast, range 15 circle
    SwoopingFrenzy2 = 38728, // Boss->location, 8.0s cast, range 15 circle
    SwoopingFrenzyTelegraph = 38730, // Helper->self, 9.0s cast, range 15 circle
    SwoopingFrenzy3 = 38729, // Boss->location, no cast, range 15 circle

    FervidPulseFirstCW1 = 38723, // Boss->self, 5.0s cast, range 50 width 14 cross, rotation, 5 hits, step 22.5°
    FervidPulseFirstCW2 = 38724, // Boss->self, 8.0s cast, range 50 width 14 cross
    FervidPulseFirstCCW = 38725, // Boss->self, 8.0s cast, range 50 width 14 cross
    FervidPulseRest = 38726, // Boss->self, no cast, range 50 width 14 cross
    FervidPulseTelegraph = 38727, // Helper->self, 2.5s cast, range 50 width 14 cross

    GaleripperVisual = 38731, // Boss->self, 4.0+0,3s cast, single-target
    Galeripper = 38732, // Helper->self, 4.3s cast, range 60 90-degree cone

    CatchingChaos = 38733, // Boss->self, 12.0s cast, range 60 circle
    CatchingChaosEnrage = 38734, // Boss->self, 59.0s cast, range 60 circle

    GreatBallOfFire = 38722, // Boss->self, 3.0s cast, single-target
    Explosion = 38745, // BallOfFire->self, 16.0s cast, range 15 circle

    BrutalStroke = 38735, // Boss->location, 6.0s cast, range 45 circle, damage fall off AOE
    Razorwind = 38736 // Helper->Keshkwa, 5.0s cast, range 7 circle, stack
}

sealed class Razorwind(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Razorwind, 7f, 2, 2);
sealed class Explosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion, 15f);

sealed class SwoopingFrenzy1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SwoopingFrenzy1, 15f);

sealed class SwoopingFrenzy2(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circle = new(15);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 3 ? 3 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.SwoopingFrenzy2 or (uint)AID.SwoopingFrenzyTelegraph && !HasMatchingAOE(spell.LocXZ))
        {
            _aoes.Add(new(circle, spell.LocXZ, default, Module.CastFinishAt(spell, 1.5d)));
        }
    }

    private bool HasMatchingAOE(WPos location)
    {
        var count = _aoes.Count;
        for (var i = 0; i < count; ++i)
        {
            if (_aoes[i].Origin.AlmostEqual(location, 1f))
                return true;
        }
        return false;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.SwoopingFrenzy2 or (uint)AID.SwoopingFrenzy3)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class BrutalStroke(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BrutalStroke, 25f);
sealed class CatchingChaos(BossModule module) : Components.RaidwideCast(module, (uint)AID.CatchingChaos);
sealed class Galeripper(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Galeripper, new AOEShapeCone(60f, 45f.Degrees()));
sealed class BitingScratch(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BitingScratch1, (uint)AID.BitingScratch2], new AOEShapeCone(40f, 45f.Degrees()));
sealed class FervidImpact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FervidImpact, 12f);
sealed class FrigidPulse(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FrigidPulse, new AOEShapeDonut(12f, 60f));

sealed class FirestormCycle(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(12f), new AOEShapeDonut(12f, 60f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FirestormCycleOut)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.FirestormCycleOut => 0,
                (uint)AID.FirestormCycleIn => 1,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2.4d));
        }
    }
}

sealed class StormkissedFlames(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeDonut(12f, 60f), new AOEShapeCircle(12f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.StormkissedFlamesIn)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.StormkissedFlamesIn => 0,
                (uint)AID.StormkissedFlamesOut => 1,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2.4d));
        }
    }
}

sealed class FervidPulse(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCross cross = new(50f, 7f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FervidPulseFirstCCW:
                AddSequence(22.5f.Degrees());
                break;
            case (uint)AID.FervidPulseFirstCW1:
            case (uint)AID.FervidPulseFirstCW2:
                AddSequence(-22.5f.Degrees());
                break;
        }
        void AddSequence(Angle angle) => Sequences.Add(new(cross, spell.LocXZ, spell.Rotation, angle, Module.CastFinishAt(spell), 2.9f, 5));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.FervidPulseFirstCCW or (uint)AID.FervidPulseFirstCW1 or (uint)AID.FervidPulseFirstCW2 or (uint)AID.FervidPulseRest)
            AdvanceSequence(0, WorldState.CurrentTime);
    }
}

sealed class VengeanceOfTheViperStates : StateMachineBuilder
{
    public VengeanceOfTheViperStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BitingScratch>()
            .ActivateOnEnter<Razorwind>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<BrutalStroke>()
            .ActivateOnEnter<SwoopingFrenzy1>()
            .ActivateOnEnter<SwoopingFrenzy2>()
            .ActivateOnEnter<CatchingChaos>()
            .ActivateOnEnter<Galeripper>()
            .ActivateOnEnter<FrigidPulse>()
            .ActivateOnEnter<FervidImpact>()
            .ActivateOnEnter<StormkissedFlames>()
            .ActivateOnEnter<FirestormCycle>()
            .ActivateOnEnter<FervidPulse>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70389, NameID = 12829)]
public sealed class VengeanceOfTheViper(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(-402f, 738f), 19.5f, 20)]);
    private static readonly uint[] all = [(uint)OID.Boss, (uint)OID.RightWing, (uint)OID.LeftWing];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, all);
    }
}
