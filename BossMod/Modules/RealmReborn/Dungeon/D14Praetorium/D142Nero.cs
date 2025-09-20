namespace BossMod.RealmReborn.Dungeon.D14Praetorium.D142Nero;

public enum OID : uint
{
    Boss = 0x3873, // R1.65
    MagitekDeathClaw = 0x3874, // R1.0
    Helper = 0x233
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 28475, // Boss->location, no cast, single-target

    IronUprising = 28482, // Boss->self, 3.0s cast, range 7 120-degree cone aoe (knockback 12)
    SpineShatter = 28483, // Boss->player, 5.0s cast, single-target tankbuster
    AugmentedSuffering = 28476, // Boss->self, 5.0s cast, knockback 12
    AugmentedShatter = 28477, // Boss->player, 5.0s cast, range 6 circle stack
    AugmentedUprising = 28478, // Boss->self, 7.0s cast, range 45 90-degree cone aoe
    Activate = 28479, // Boss->self, 3.0s cast, single-target, visual (spawn claw)
    TheHand = 28480, // MagitekDeathClaw->player, no cast, single-target (autoattack with knockback 20)
    WheelOfSuffering = 28481 // Boss->self, 3.5s cast, range 7 circle aoe (knockback 12)
}

class IronUprising(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IronUprising, new AOEShapeCone(7f, 60f.Degrees()));
class SpineShatter(BossModule module) : Components.SingleTargetCast(module, (uint)AID.SpineShatter);

class AugmentedSuffering(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.AugmentedSuffering, 12f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center, 8f), Casters.Ref(0).Activation);
        }
    }
}

class AugmentedShatter(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.AugmentedShatter, 6f, 4, 4);
class AugmentedUprising(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AugmentedUprising, new AOEShapeCone(45f, 45f.Degrees()));
class WheelOfSuffering(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WheelOfSuffering, 7f);

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(20f, 35f);
    private AOEInstance[] _aoe = [];
    private bool begin;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x00 && state == 0x00020001u)
        {
            Arena.Bounds = D142Nero.DefaultBounds;
            Arena.Center = D142Nero.DefaultBounds.Center;
            _aoe = [];
            begin = true;
        }
    }

    public override void Update()
    {
        if (!begin && _aoe.Length == 0)
        {
            _aoe = [new(donut, Arena.Center, default, WorldState.FutureTime(3.6d))];
        }
    }
}

class D142NeroStates : StateMachineBuilder
{
    public D142NeroStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<IronUprising>()
            .ActivateOnEnter<SpineShatter>()
            .ActivateOnEnter<AugmentedSuffering>()
            .ActivateOnEnter<AugmentedShatter>()
            .ActivateOnEnter<AugmentedUprising>()
            .ActivateOnEnter<WheelOfSuffering>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 16, NameID = 2135)]
public class D142Nero(WorldState ws, Actor primary) : BossModule(ws, primary, startingBounds.Center, startingBounds)
{
    private static readonly WPos[] vertices = [new(-158.27f, -29.04f), new(-156.76f, -28.7f), new(-155.3f, -28.29f), new(-151.86f, -26.99f), new(-151.47f, -25.48f),
    new(-151.27f, -25), new(-151.02f, -24.52f), new(-150.27f, -23.82f), new(-149.28f, -23.51f), new(-148.21f, -23.58f),
    new(-146.71f, -23.99f), new(-146.3f, -23.69f), new(-143.17f, -21), new(-142.44f, -20.24f), new(-142.05f, -19.86f),
    new(-138.63f, -15.25f), new(-137.84f, -13.83f), new(-136.14f, -9.99f), new(-135.65f, -8.51f), new(-135.24f, -6.99f),
    new(-135.23f, -5.44f), new(-135.02f, -3.03f), new(-135.02f, 5.01f), new(-135.21f, 5.48f), new(-135.23f, 6.75f),
    new(-135.3f, 7.25f), new(-135.59f, 8.27f), new(-136.19f, 10.1f), new(-136.64f, 11.25f), new(-137.33f, 12.82f),
    new(-137.79f, 13.74f), new(-138.22f, 14.49f), new(-138.46f, 14.95f), new(-139.28f, 16.26f), new(-143.03f, 20.88f),
    new(-144.14f, 21.95f), new(-145.29f, 22.94f), new(-146.52f, 23.88f), new(-147.03f, 23.91f), new(-148.03f, 23.64f),
    new(-149.05f, 23.46f), new(-150.04f, 23.74f), new(-150.49f, 24.03f), new(-150.86f, 24.38f), new(-151.21f, 24.77f),
    new(-151.41f, 25.27f), new(-151.82f, 26.76f), new(-152.2f, 27.12f), new(-152.69f, 27.35f), new(-158.17f, 29.02f),
    new(-159.72f, 29.29f), new(-163.96f, 29.6f), new(-165.52f, 29.56f), new(-171.11f, 28.73f), new(-172.65f, 28.31f),
    new(-176.05f, 27.03f), new(-176.24f, 26.55f), new(-176.51f, 25.56f), new(-176.74f, 25.1f), new(-176.88f, 24.62f),
    new(-177.64f, 23.9f), new(-178.12f, 23.7f), new(-178.6f, 23.55f), new(-179.11f, 23.49f), new(-179.66f, 23.55f),
    new(-181.16f, 23.95f), new(-181.64f, 23.77f), new(-182.85f, 22.81f), new(-185.85f, 19.96f), new(-186.89f, 18.76f),
    new(-190.05f, 14.05f), new(-190.77f, 12.63f), new(-192.29f, 8.69f), new(-192.71f, 7.2f), new(-192.78f, 5.42f),
    new(-192.98f, 4.88f), new(-192.98f, -5.06f), new(-192.78f, -5.53f), new(-192.76f, -6.86f), new(-191.36f, -11.23f),
    new(-190.74f, -12.63f), new(-189.82f, -14.46f), new(-187.83f, -17.56f), new(-186.86f, -18.79f), new(-182.78f, -22.87f),
    new(-181.55f, -23.83f), new(-181.03f, -23.92f), new(-179.56f, -23.52f), new(-179.06f, -23.44f), new(-178.55f, -23.56f),
    new(-178.06f, -23.71f), new(-177.57f, -23.95f), new(-176.84f, -24.65f), new(-176.48f, -25.61f), new(-176.22f, -26.59f),
    new(-175.97f, -27.03f), new(-174.56f, -27.62f), new(-174.07f, -27.83f), new(-172.59f, -28.32f), new(-171.11f, -28.73f),
    new(-166.99f, -29.45f), new(-165.38f, -29.56f)];

    private static readonly ArenaBoundsCustom startingBounds = new([new PolygonCustom(vertices)]);
    public static readonly ArenaBoundsCustom DefaultBounds = new([new Polygon(new(-164f, default), 20f, 48)]);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.MagitekDeathClaw => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.MagitekDeathClaw), Colors.Danger);
    }
}
