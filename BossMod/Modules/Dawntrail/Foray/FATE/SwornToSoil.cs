namespace BossMod.Dawntrail.Foray.FATE.SwornToSoil;

public enum OID : uint
{
    Boss = 0x46BD, // R3.6
    CommonMud = 0x46BE, // R2.5-8.125
    CommonCompost = 0x46BF, // R4.0
    Helper = 0x46C0
}

public enum AID : uint
{
    AutoAttack = 39461, // Boss->player, no cast, single-target

    RoyalFlushDonut = 29827, // Boss->self, 5.0s cast, range 10-20 donut
    RoyalFlushCircle = 29826, // Boss->self, 5.0s cast, range 8 circle
    FromMud1 = 30706, // Boss->self, 3.0s cast, single-target
    FromMud2 = 29809, // Boss->self, 3.0s cast, single-target
    UnshowerVisual = 29806, // Boss->self, 3.0s cast, single-target
    Unshower = 29807, // Helper->location, 3.0s cast, range 6 circle
    Rupture = 29808, // CommonMud->self, 2.0s cast, range 16 circle
    FeculentFlood = 29825, // Boss->self, 3.0s cast, range 40 60-degree cone
    RockyRoll = 29810 // CommonCompost->self, 5.0s cast, range 60 width 6 rect
}

public enum SID : uint
{
    Growth = 4221 // Helper->CommonMud, extra=0x1/0x2/0x3
}

sealed class Rupture(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeCircle circle = new(16f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Growth && status.Extra == 0x02)
        {
            _aoes.Add(new(circle, actor.Position.Quantized(), default, WorldState.FutureTime(7.1d), actorID: actor.InstanceID));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.Rupture)
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

sealed class RoyalFlushCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RoyalFlushCircle, 8f);
sealed class RoyalFlushDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RoyalFlushDonut, new AOEShapeDonut(10f, 20f));
sealed class Unshower(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Unshower, 6f);
sealed class FeculentFlood(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FeculentFlood, new AOEShapeCone(40f, 30f.Degrees()));
sealed class RockyRoll(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RockyRoll, new AOEShapeRect(60f, 3f));

sealed class MadMudarchStates : StateMachineBuilder
{
    public MadMudarchStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Rupture>()
            .ActivateOnEnter<RoyalFlushCircle>()
            .ActivateOnEnter<RoyalFlushDonut>()
            .ActivateOnEnter<Unshower>()
            .ActivateOnEnter<FeculentFlood>()
            .ActivateOnEnter<RockyRoll>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.ForayFATE, GroupID = 1018, NameID = 1969)]
public sealed class MadMudarch(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
