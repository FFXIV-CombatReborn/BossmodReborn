namespace BossMod.Dawntrail.Foray.FATE.RoughWaters;

public enum OID : uint
{
    Boss = 0x4718, // R5.5
    Helper = 0x4719
}

public enum AID : uint
{
    AutoAttack = 41768, // Boss->player, no cast, single-target

    TidelineVisual = 41770, // Boss->self, 3.0s cast, single-target
    TidelineFirst = 41771, // Helper->location, 5.0s cast, range 50 width 10 rect
    TidelineRest = 41772, // Helper->location, 1.0s cast, range 50 width 5 rect

    RightTwinTentacle = 41781, // Boss->self, 5.0s cast, range 60 180-degree cone
    LeftTentacle = 41780, // Boss->self, no cast, range 60 180-degree cone
    LeftTwinTentacle = 41779, // Boss->self, 5.0s cast, range 60 180-degree cone
    RightTentacle = 41782, // Boss->self, no cast, range 60 180-degree cone

    VoidWaterIIIVisual = 41783, // Boss->self, 3.0s cast, single-target
    VoidWaterIII = 41784, // Helper->location, 3.0s cast, range 6 circle
    VoidWaterIVVisual = 41785, // Boss->self, 5.0s cast, single-target
    VoidWaterIV = 41786, // Helper->location, 5.0s cast, range 40 circle

    RecedingTwinTides = 41773, // Boss->self, 5.0s cast, single-target
    NearTide1 = 41774, // Helper->location, 5.0s cast, range 10 circle
    FarTide1 = 41778, // Helper->location, 7.0s cast, range 10-40 donut
    EncroachingTwinTides = 41776, // Boss->self, 5.0s cast, single-target
    FarTide2 = 41777, // Helper->location, 5.0s cast, range 10-40 donut
    NearTide2 = 41775 // Helper->location, 7.0s cast, range 10 circle
}

sealed class Tideline(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(9);
    private static readonly AOEShapeRect rect1 = new(50f, 5f);
    private static readonly AOEShapeRect rect2 = new(50f, 2.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count == 9 ? 3 : count > 3 ? 4 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes)[..max];
        var isFourAOEs = max == 4;
        var isThreeAOEs = max == 3;

        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];

            var shouldBeDanger = isFourAOEs && i < 2 || isThreeAOEs && i == 0;
            var shouldBeRisky = shouldBeDanger || max == 2 && i < 2;

            if (shouldBeDanger)
                aoe.Color = Colors.Danger;

            if (shouldBeRisky)
                aoe.Risky = true;
        }

        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TidelineFirst)
        {
            var rot = spell.Rotation;
            AddAOE(rect1, spell.LocXZ, rotation: rot);
            var initialpos1 = new WPos(169.5f, 701f);
            var initialpos2 = new WPos(154.5f, 651f);
            var dir = new WDir(5f, default);
            var a180 = 180f.Degrees();
            for (var i = 0; i < 4; ++i)
            {
                var act = 2f + 2 * i;
                AddAOE(rect2, initialpos1 + i * dir, act, a180);
                AddAOE(rect2, initialpos2 + -i * dir, act, rot);
            }
        }
        void AddAOE(AOEShapeRect shape, WPos position = default, float delay = default, Angle rotation = default) => _aoes.Add(new(shape, position.Quantized(), rotation, Module.CastFinishAt(spell, delay), risky: false));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.TidelineFirst or (uint)AID.TidelineRest)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class TwinTentacle(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeCone cone = new(60f, 90f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        aoes[0].Risky = true;
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.LeftTwinTentacle or (uint)AID.RightTwinTentacle)
        {
            AddAOE();
            AddAOE(false);
        }
        void AddAOE(bool first = true) => _aoes.Add(new(cone, spell.LocXZ, spell.Rotation + (first ? default : 180f.Degrees()), Module.CastFinishAt(spell, first ? default : 2.1d), first ? Colors.Danger : default, first));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.LeftTwinTentacle or (uint)AID.RightTwinTentacle or (uint)AID.LeftTentacle or (uint)AID.RightTentacle)
        {
            _aoes.RemoveAt(0);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_aoes.Count != 2)
        {
            return;
        }
        // make ai stay close to boss to ensure successfully dodging the combo
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        ref readonly var aoe = ref aoes[0];
        hints.AddForbiddenZone(new SDInvertedRect(Module.PrimaryActor.Position, aoe.Rotation, 2f, 2f, 40f), aoe.Activation);
    }
}

sealed class RecedingTwinTides(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 40f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.NearTide1)
        {
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.NearTide1 => 0,
                (uint)AID.FarTide1 => 1,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

sealed class EncroachingTwinTides(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeDonut(10f, 40f), new AOEShapeCircle(10f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FarTide2)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.FarTide2 => 0,
                (uint)AID.NearTide2 => 1,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

sealed class VoidWaterIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidWaterIII, 6f);
sealed class VoidWaterIV(BossModule module) : Components.RaidwideCast(module, (uint)AID.VoidWaterIV);

sealed class RoughWatersStates : StateMachineBuilder
{
    public RoughWatersStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TwinTentacle>()
            .ActivateOnEnter<EncroachingTwinTides>()
            .ActivateOnEnter<RecedingTwinTides>()
            .ActivateOnEnter<VoidWaterIII>()
            .ActivateOnEnter<VoidWaterIV>()
            .ActivateOnEnter<Tideline>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.ForayFATE, GroupID = 1018, NameID = 1962)]
public sealed class RoughWaters(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
