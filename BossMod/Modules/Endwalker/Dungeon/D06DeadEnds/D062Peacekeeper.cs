namespace BossMod.Endwalker.Dungeon.D06DeadEnds.D062Peacekeeper;

public enum OID : uint
{
    Boss = 0x34C6, // R=9.0
    PerpetualWarMachine = 0x384B, // R=0.9
    ElectricVoidzone = 0x1EB5F7,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 25977, // Boss->player, no cast, single-target

    Decimation = 25936, // Boss->self, 5.0s cast, range 40 circle
    DisengageHatch = 28356, // Boss->self, no cast, single-target
    EclipsingExhaust = 25931, // Boss->self, 5.0s cast, range 40 circle, knockback 11, away from source
    ElectromagneticRepellant = 28360, // Boss->self, 4.0s cast, range 9 circle, voidzone
    Elimination = 25935, // Boss->self/player, 5.0s cast, range 46 width 10 rect, tankbuster
    InfantryDeterrentVisual = 28358, // Boss->self, no cast, single-target
    InfantryDeterrent = 28359, // Helper->player, 5.0s cast, range 6 circle, spread
    NoFutureVisual = 25925, // Boss->self, 4.0s cast, single-target
    NoFutureAOE = 25927, // Helper->self, 4.0s cast, range 6 circle
    NoFutureSpread = 25928, // Helper->player, 5.0s cast, range 6 circle, spread
    OrderToFire = 28351, // Boss->self, 5.0s cast, single-target
    PeacefireVisual = 25933, // Boss->self, 3.0s cast, single-target
    Peacefire = 25934, // Helper->self, 7.0s cast, range 10 circle
    SmallBoreLaser = 28352, // PerpetualWarMachine->self, 5.0s cast, range 20 width 4 rect
    Teleport = 28350, // PerpetualWarMachine->location, no cast, single-target
    VisualModelChange1 = 28357, // Boss->self, no cast, single-target
    VisualModelChange2 = 25926 // Boss->self, no cast, single-target
}

class DecimationArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(16f, 20f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x17 && state == 0x00020001u)
        {
            Arena.Bounds = D062Peacekeeper.SmallerBounds;
            Arena.Center = D062Peacekeeper.ArenaCenter;
            _aoe = [];
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Decimation && Arena.Bounds.Radius > 16f)
        {
            _aoe = [new(donut, Arena.Center, default, Module.CastFinishAt(spell, 0.4d))];
        }
    }
}

class ElectromagneticRepellant(BossModule module) : Components.VoidzoneAtCastTarget(module, 9f, (uint)AID.ElectromagneticRepellant, GetVoidzone, 0.7d)
{
    private static Actor[] GetVoidzone(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.ElectricVoidzone);
        if (enemies.Count != 0 && enemies[0].EventState != 7)
            return [.. enemies];
        return [];
    }
}
class InfantryDeterrent(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.InfantryDeterrent, 6f);
class NoFutureSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.NoFutureSpread, 6f);

class NoFutureAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NoFutureAOE, 6f);
class Peacefire(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Peacefire, 10f);
class SmallBoreLaser(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SmallBoreLaser, new AOEShapeRect(20f, 2f));

class Elimination(BossModule module) : Components.BaitAwayCast(module, (uint)AID.Elimination, new AOEShapeRect(46f, 5f), endsOnCastEvent: true, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

class Decimation(BossModule module) : Components.RaidwideCast(module, (uint)AID.Decimation);
class EclipsingExhaust(BossModule module) : Components.RaidwideCast(module, (uint)AID.EclipsingExhaust);

class EclipsingExhaustKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.EclipsingExhaust, 11f)
{
    private static readonly Angle a36 = 36f.Degrees();
    private readonly Peacefire _aoe = module.FindComponent<Peacefire>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var component = _aoe.Casters;
            var count = component.Count;
            var forbidden = new ShapeDistance[count + 1];
            var center = Arena.Center;
            var aoes = CollectionsMarshal.AsSpan(component);
            for (var i = 0; i < count; ++i)
            {
                ref readonly var aoe = ref aoes[i];
                forbidden[i] = new SDCone(center, 16f, Angle.FromDirection(aoe.Origin - center), a36);
            }
            forbidden[count] = new SDInvertedCircle(center, 4f);
            hints.AddForbiddenZone(new SDUnion(forbidden), c.Activation);
        }
    }
}

class D062PeacekeeperStates : StateMachineBuilder
{
    public D062PeacekeeperStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DecimationArenaChange>()
            .ActivateOnEnter<ElectromagneticRepellant>()
            .ActivateOnEnter<InfantryDeterrent>()
            .ActivateOnEnter<NoFutureSpread>()
            .ActivateOnEnter<NoFutureAOE>()
            .ActivateOnEnter<Peacefire>()
            .ActivateOnEnter<SmallBoreLaser>()
            .ActivateOnEnter<Elimination>()
            .ActivateOnEnter<Decimation>()
            .ActivateOnEnter<EclipsingExhaust>()
            .ActivateOnEnter<EclipsingExhaustKnockback>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 792, NameID = 10315)]
public class D062Peacekeeper(WorldState ws, Actor primary) : BossModule(ws, primary, StartingBounds.Center, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(-105f, -210f);
    private static readonly Angle offset = 5.625f.Degrees();
    public static readonly ArenaBoundsCustom StartingBounds = new([new Polygon(ArenaCenter, 19.5f * CosPI.Pi32th, 32, offset)], [new Rectangle(new(-105f, -229f), 20, 0.78f),
    new Rectangle(new(-105f, -190f), 20f, 1.1f)]);
    public static readonly ArenaBoundsCustom SmallerBounds = new([new Polygon(ArenaCenter, 16f, 32, offset)]);
}
