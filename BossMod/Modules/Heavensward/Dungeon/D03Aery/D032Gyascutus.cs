namespace BossMod.Heavensward.Dungeon.D03Aery.D032Gyascutus;

public enum OID : uint
{
    Boss = 0x3970, // R5.4
    InflammableFumes = 0x3972, // R1.2
    Helper = 0x233C
}

public enum AID : uint
{
    Attack = 872, // Boss->player, no cast, single-target

    InflammableFumesVisual = 31232, // InflammableFumes->location, no cast, single-target
    InflammableFumes = 30181, // Boss->self, 4.0s cast, single-target
    BurstVisual = 30183, // InflammableFumes->self, no cast, single-target
    Burst = 30184, // Helper->self, 10.0s cast, range 10 circle
    DeafeningBellow = 31233, // Boss->self, 4.0s cast, range 55 circle

    ProximityPyre = 30191, // Boss->self, 4.0s cast, range 12 circle
    AshenOuroboros = 30190, // Boss->self, 8.0s cast, range 11-20 donut
    BodySlam = 31234, // Boss->self, 4.0s cast, range 30 circle, knockback 10, away from source
    CripplingBlow = 30193 // Boss->player, 5.0s cast, single-target
}

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(19.9f, 27);
    private static readonly ArenaBoundsCircle defaultbounds = new(19.9f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.InflammableFumes && Arena.Bounds == D032Gyascutus.StartingBounds)
        {
            _aoe = [new(donut, D032Gyascutus.ArenaCenter, default, Module.CastFinishAt(spell, 0.8d))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00 && state == 0x00020001u)
        {
            Arena.Bounds = defaultbounds;
            _aoe = [];
        }
    }
}

class ProximityPyre(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ProximityPyre, 12f);
class Burst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Burst, 10f);
class CripplingBlow(BossModule module) : Components.SingleTargetCast(module, (uint)AID.CripplingBlow);
class DeafeningBellow(BossModule module) : Components.RaidwideCast(module, (uint)AID.DeafeningBellow);
class AshenOuroboros(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AshenOuroboros, new AOEShapeDonut(11f, 20f));
class BodySlam(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.BodySlam, 10f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
            hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center, 9f), Casters.Ref(0).Activation);
    }
}

class D032GyascutusStates : StateMachineBuilder
{
    public D032GyascutusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<ProximityPyre>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<DeafeningBellow>()
            .ActivateOnEnter<AshenOuroboros>()
            .ActivateOnEnter<BodySlam>()
            .ActivateOnEnter<CripplingBlow>();

    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 39, NameID = 3455)]
public class D032Gyascutus(WorldState ws, Actor primary) : BossModule(ws, primary, StartingBounds.Center, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(11.978f, 67.979f);
    public static readonly ArenaBoundsCustom StartingBounds = new([new Circle(ArenaCenter, 26.5f)], [new Rectangle(new(38.805f, 66.371f), 20f, 0.8f, -86.104f.Degrees()), new Rectangle(new(-11.441f, 56.27f), 20f, 0.9f, 64.554f.Degrees()), new Circle(new(-16.5f, 66.1f), 2.5f)]);
}
