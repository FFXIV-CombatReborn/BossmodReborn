namespace BossMod.Shadowbringers.Hunt.RankA.Sugaar;

public enum OID : uint
{
    Boss = 0x2875 // R5.500, x1
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    BodySlam = 18018, // Boss->self, 5.0s cast, range 11 circle

    NumbingNoise = 18015, // Boss->self (front), 5.0s cast, range 13, 120 degree cone
    NumbingNoiseRotation = 18100, // Boss->self, 5.0s cast, range 30 circle, pulls player in from 30 by max 25 units between hitboxes, 3 hits of NumbingNoiseRotationAOE
    NumbingNoiseRotationAOE = 18098, // Boss->self, 0.5s cast, 13 range, 120 degree cone

    TailSnap = 18016, // Boss->self (behind), 5.0s cast, range 18, 120 degree cone
    TailSnapRotation = 18101, // Boss->self, 5.0s cast, range 30 circle, pulls player in from 30 by max 25 units between hitboxes, 3 hits of TailSnapRotationAOE
    TailSnapRotationAOE = 18099 // Boss->self (behind), 0.5 cast, range 18, 120 degree cone
}

class BodySlam(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BodySlam, 11f);
class NumbingNoise(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NumbingNoise, NumbingNoiseTailSnapRotating.ShapeNumbingNoise);
class TailSnap(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TailSnap, NumbingNoiseTailSnapRotating.ShapeTailSnap);

class NumbingNoiseTailSnapRotating(BossModule module) : Components.GenericRotatingAOE(module)
{
    public static readonly AOEShapeCone ShapeNumbingNoise = new(13f, 60f.Degrees());
    public static readonly AOEShapeCone ShapeTailSnap = new(18f, 60f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.NumbingNoiseRotation: // NN always seems to go CCW
                AddSequence(ShapeNumbingNoise, default, 120f.Degrees());
                break;
            case (uint)AID.TailSnapRotation: // TS always seems to go CW
                AddSequence(ShapeTailSnap, 180f.Degrees(), -120f.Degrees());
                break;
        }
        void AddSequence(AOEShapeCone shape, Angle offset, Angle increment) => Sequences.Add(new(shape, spell.LocXZ, spell.Rotation + offset, increment, Module.CastFinishAt(spell, 1.1f), 2.7f, 3));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NumbingNoiseRotationAOE or (uint)AID.TailSnapRotationAOE)
            AdvanceSequence(0, WorldState.CurrentTime);
    }
}

class NumbingNoiseTailSnapAttract(BossModule module) : Components.GenericKnockback(module)
{
    private readonly NumbingNoiseTailSnapRotating _rotating = module.FindComponent<NumbingNoiseTailSnapRotating>()!;
    private DateTime _activation;

    private static readonly AOEShapeCircle _shape = new(30f);

    private readonly Knockback[] _oneKnockback = new Knockback[1];
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_activation != default)
        {
            _oneKnockback[0] = new(Module.PrimaryActor.Position, 25f, _activation, _shape, default, Kind.TowardsOrigin, Module.PrimaryActor.HitboxRadius + actor.HitboxRadius);
            return _oneKnockback;
        }
        else
            return [];
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _rotating.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return false;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NumbingNoiseRotation or (uint)AID.TailSnapRotation)
            _activation = Module.CastFinishAt(spell);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NumbingNoiseRotation or (uint)AID.TailSnapRotation)
            _activation = default;
    }
}

class SugaarStates : StateMachineBuilder
{
    public SugaarStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BodySlam>()
            .ActivateOnEnter<NumbingNoise>()
            .ActivateOnEnter<TailSnap>()
            .ActivateOnEnter<NumbingNoiseTailSnapRotating>()
            .ActivateOnEnter<NumbingNoiseTailSnapAttract>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 8902)]
public class Sugaar(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
