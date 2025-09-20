namespace BossMod.Dawntrail.Foray.FATE.KingOfTheCrescent;

public enum OID : uint
{
    Boss = 0x46D7, // R5.75
    GaleSphere = 0x46D8, // R1.0
    Helper = 0x46D9
}

public enum AID : uint
{
    AutoAttack = 42899, // Boss->player, no cast, single-target
    Teleport1 = 41384, // Boss->location, no cast, single-target
    Teleport2 = 41392, // Boss->location, no cast, single-target

    GlidingSwoop = 41387, // Boss->location, 6.0s cast, range 50 circle, proximity AOE, optimal range around 25
    BitingScratch = 41388, // Boss->self, 6.0s cast, range 40 90-degree cone
    AeroIV = 41391, // Boss->self, 5.0s cast, range 60 circle

    WindSphere = 41385, // Boss->self, 3.0s cast, single-target
    Airburst = 41386, // GaleSphere->self, 1.0s cast, range 11 circle
    FeatherRainVisual = 41389, // Boss->self, 4.0s cast, single-target
    FeatherRain = 41390 // Helper->location, 4.0s cast, range 11 circle
}

sealed class GlidingSwoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GlidingSwoop, 25f);
sealed class BitingScratch(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BitingScratch, new AOEShapeCone(40f, 45f.Degrees()));
sealed class AeroIV(BossModule module) : Components.RaidwideCast(module, (uint)AID.AeroIV);
sealed class FeatherRain(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FeatherRain, 11f);

sealed class Airburst(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(8);
    private static readonly AOEShapeCircle circle = new(11f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.GaleSphere)
        {
            _aoes.Add(new(circle, actor.Position.Quantized(), default, WorldState.FutureTime(9.8d), actorID: actor.InstanceID));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Airburst)
        {
            var count = _aoes.Count;
            var id = caster.InstanceID;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                if (aoes[i].ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

sealed class KingOfTheCrescentStates : StateMachineBuilder
{
    public KingOfTheCrescentStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Airburst>()
            .ActivateOnEnter<FeatherRain>()
            .ActivateOnEnter<AeroIV>()
            .ActivateOnEnter<BitingScratch>()
            .ActivateOnEnter<GlidingSwoop>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.ForayFATE, GroupID = 1018, NameID = 1964)]
public sealed class KingOfTheCrescent(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
