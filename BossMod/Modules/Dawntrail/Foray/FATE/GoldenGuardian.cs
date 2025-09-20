namespace BossMod.Dawntrail.Foray.FATE.GoldenGuardian;

public enum OID : uint
{
    Boss = 0x471A, // R4.5
    Helper = 0x471B
}

public enum AID : uint
{
    AutoAttack = 41788, // Boss->player, no cast, single-target
    Teleport = 41789, // Boss->location, no cast, single-target

    ErosiveEyeVisual = 41791, // Boss->self, 4.0s cast, single-target
    ErosiveEye1 = 41792, // Helper->location, 5.0s cast, range 50 circle, gaze
    ErosiveEye2 = 41793, // Helper->location, 8.0s cast, range 50 circle, gaze
    ErosiveEye3 = 41794, // Helper->location, 5.0s cast, range 50 circle, gaze inverted
    ErosiveEye4 = 41795, // Helper->location, 8.0s cast, range 50 circle, gaze inverted

    FlamingEpigraphVisual = 41802, // Boss->self, 5.6+0,4s cast, single-target
    FlamingEpigraph = 41803, // Helper->location, 6.0s cast, range 60 60-degree cone

    Epigraph = 41790, // Boss->self, 4.0s cast, range 45 width 5 rect
    EpigraphicFireIIVisual = 41804, // Boss->self, 2.6+0,4s cast, single-target
    EpigraphicFireII = 41805, // Helper->location, 3.0s cast, range 5 circle

    WideningTwinflame = 41796, // Boss->self, 4.6+0,4s cast, single-target
    NarrowingTwinflame = 41799, // Boss->self, 4.6+0,4s cast, single-target

    TongueOfFlame1 = 41797, // Helper->location, 5.0s cast, range 10 circle
    TongueOfFlame2 = 41798, // Helper->location, 7.0s cast, range 10 circle
    LickOfFlame1 = 41801, // Helper->location, 7.0s cast, range 10-40 donut
    LickOfFlame2 = 41800, // Helper->location, 5.0s cast, range 10-40 donut
    FlaringEpigraphVisual = 41808, // Boss->self, 4.6+0,4s cast, single-target
    FlaringEpigraph = 41809 // Helper->location, 5.0s cast, range 60 circle
}

sealed class FlamingEpigraph : Components.SimpleAOEs
{
    public FlamingEpigraph(BossModule module) : base(module, (uint)AID.FlamingEpigraph, new AOEShapeCone(60f, 30f.Degrees()), 4)
    {
        MaxDangerColor = 2;
    }
}
sealed class FlaringEpigraph(BossModule module) : Components.RaidwideCast(module, (uint)AID.FlaringEpigraph);
sealed class EpigraphicFireII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EpigraphicFireII, 5f);
sealed class Epigraph(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Epigraph, new AOEShapeRect(45f, 2.5f));

sealed class ErosiveEye(BossModule module) : Components.GenericGaze(module)
{
    private readonly List<Eye> _eyes = new(4);

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor) => _eyes.Count != 0 ? CollectionsMarshal.AsSpan(_eyes)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        bool? inverted = spell.Action.ID switch
        {
            (uint)AID.ErosiveEye1 or (uint)AID.ErosiveEye2 => false,
            (uint)AID.ErosiveEye3 or (uint)AID.ErosiveEye4 => true,
            _ => null
        };
        if (inverted is bool inv)
        {
            _eyes.Add(new(spell.LocXZ, Module.CastFinishAt(spell), default, 50f, inv));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_eyes.Count != 0 && spell.Action.ID is (uint)AID.ErosiveEye1 or (uint)AID.ErosiveEye2 or (uint)AID.ErosiveEye3 or (uint)AID.ErosiveEye4)
        {
            _eyes.RemoveAt(0);
        }
    }
}

sealed class TongueLickOfFlameOutIn(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 40f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TongueOfFlame1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.TongueOfFlame1 => 0,
                (uint)AID.LickOfFlame1 => 1,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

sealed class TongueLickOfFlameInOut(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeDonut(10f, 40f), new AOEShapeCircle(10f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LickOfFlame2)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.LickOfFlame2 => 0,
                (uint)AID.TongueOfFlame2 => 1,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

sealed class GoldenGuardianStates : StateMachineBuilder
{
    public GoldenGuardianStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FlamingEpigraph>()
            .ActivateOnEnter<FlaringEpigraph>()
            .ActivateOnEnter<Epigraph>()
            .ActivateOnEnter<EpigraphicFireII>()
            .ActivateOnEnter<ErosiveEye>()
            .ActivateOnEnter<TongueLickOfFlameInOut>()
            .ActivateOnEnter<TongueLickOfFlameOutIn>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.ForayFATE, GroupID = 1018, NameID = 1963)]
public sealed class GoldenGuardian(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
