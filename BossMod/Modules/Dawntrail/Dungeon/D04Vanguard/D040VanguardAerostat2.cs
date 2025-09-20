namespace BossMod.Dawntrail.Dungeon.D04Vanguard.D040VanguardAerostat2;

public enum OID : uint
{
    Boss = 0x43D4, //R=2.3
    Turret = 0x41DB, //R=0.6
    SentryR7 = 0x41D6, //R=2.34
    SentryS7 = 0x4478, //R=2.34
}

public enum AID : uint
{
    AutoAttack1 = 871, // Boss->player, no cast, single-target
    AutoAttack2 = 39089, // Turret->player, no cast, single-target
    AutoAttack3 = 873, // SentryS7->player, no cast, single-target
    AutoAttack4 = 870, // SentryR7->player, no cast, single-target

    IncendiaryRing = 38452, // Boss->self, 4.8s cast, range 3-12 donut
    Electrobeam = 38060, // Turret->self, 4.0s cast, range 50 width 4 rect
    SpreadShot = 39017, // SentryS7->self, 4.0s cast, range 12 90-degree cone
}

sealed class IncendiaryRing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IncendiaryRing, new AOEShapeDonut(3f, 12f));
sealed class Electrobeam(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Electrobeam, new AOEShapeRect(50f, 2f));
sealed class SpreadShot(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpreadShot, new AOEShapeCone(12f, 45f.Degrees()));

sealed class D040VanguardAerostat2States : StateMachineBuilder
{
    public D040VanguardAerostat2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IncendiaryRing>()
            .ActivateOnEnter<Electrobeam>()
            .ActivateOnEnter<SpreadShot>()
            .Raw.Update = () => AllDeadOrDestroyed(D040VanguardAerostat2.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 831, NameID = 12780, SortOrder = 6)]
public sealed class D040VanguardAerostat2(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(92.36f, -331.66f), new(97.65f, -331.36f), new(98.25f, -331.02f), new(100.54f, -328.46f), new(100.83f, -312.14f),
    new(100.27f, -312.18f), new(99.64f, -312.33f), new(99.11f, -312.16f), new(98.67f, -311.78f), new(98.42f, -311.24f),
    new(98.21f, -310.57f), new(82.13f, -310.57f), new(81.67f, -310.79f), new(81.6f, -311.38f), new(81.21f, -311.9f),
    new(80.74f, -312.17f), new(80.08f, -312.17f), new(79.53f, -312.29f), new(78.93f, -312.07f), new(78.59f, -311.63f),
    new(78.4f, -311.07f), new(78.28f, -310.57f), new(61.86f, -310.57f), new(61.32f, -311.79f), new(60.88f, -312.17f),
    new(59.59f, -312.17f), new(58.99f, -312.1f), new(58.61f, -311.64f), new(58.4f, -311.08f), new(58.34f, -310.57f),
    new(41.43f, -310.58f), new(41.2f, -311.05f), new(41.2f, -315.39f), new(40.9f, -315.99f), new(40.9f, -317.96f),
    new(40.79f, -318.48f), new(39.81f, -318.6f), new(39.24f, -318.51f), new(39.1f, -317.1f), new(23.75f, -316.9f),
    new(23.8f, -317.44f), new(23.77f, -318.05f), new(23.57f, -318.54f), new(23, -318.6f), new(22.34f, -318.6f),
    new(21.94f, -318.29f), new(21.94f, -316.14f), new(21.69f, -315.46f), new(21.63f, -311.6f), new(21.17f, -311.4f),
    new(4.69f, -311.4f), new(4.19f, -311.15f), new(3.19f, -310.25f), new(3.09f, -302.77f), new(2.53f, -302.42f),
    new(2.33f, -301.46f), new(2.08f, -300.8f), new(1.92f, -300.12f), new(1.95f, -299.49f), new(2.25f, -298.26f),
    new(2.48f, -297.68f), new(3.09f, -297.47f), new(3.09f, -279.62f), new(2.49f, -279.35f), new(2.32f, -277),
    new(2.06f, -276.37f), new(2.05f, -275.85f), new(2.24f, -275.25f), new(2.5f, -274.68f), new(3.1f, -274.58f),
    new(3.07f, -267.54f), new(2.97f, -266.92f), new(2.59f, -266.58f), new(1.25f, -266.16f), new(0.75f, -266.1f),
    new(-2.76f, -266.38f), new(-3.02f, -267.01f), new(-3.1f, -267.7f), new(-3.07f, -274.25f), new(-2.55f, -274.62f),
    new(-2.34f, -277.56f), new(-2.14f, -278.67f), new(-2.46f, -279.28f), new(-3.09f, -279.55f), new(-3.09f, -297.13f),
    new(-2.62f, -297.59f), new(-2.34f, -298.03f), new(-2.34f, -300.8f), new(-2.2f, -301.46f), new(-2.28f, -302),
    new(-2.62f, -302.4f), new(-3.1f, -309.82f), new(-3.26f, -310.33f), new(-4.26f, -311.24f), new(-12.49f, -311.44f),
    new(-12.57f, -311.95f), new(-12.83f, -312.38f), new(-16.87f, -312.43f), new(-17.34f, -312.22f), new(-17.4f, -311.63f),
    new(-21.05f, -311.4f), new(-21.8f, -311.5f), new(-21.77f, -328.31f), new(-18.23f, -328.6f), new(-17.53f, -328.56f),
    new(-17.39f, -328.07f), new(-13.07f, -327.74f), new(-12.62f, -327.99f), new(-12.35f, -328.6f), new(12.41f, -328.6f),
    new(12.58f, -328.12f), new(12.95f, -327.75f), new(16.22f, -327.75f), new(16.89f, -327.67f), new(17.35f, -327.94f),
    new(17.54f, -328.59f), new(21.39f, -328.59f), new(21.64f, -327.48f), new(21.29f, -325.37f), new(21.85f, -324.27f),
    new(21.94f, -321.8f), new(22.26f, -321.38f), new(23.43f, -321.41f), new(23.76f, -321.79f), new(23.76f, -322.86f),
    new(38.79f, -323.09f), new(39.1f, -321.87f), new(39.51f, -321.4f), new(40.13f, -321.39f), new(40.74f, -321.48f),
    new(40.91f, -323.92f), new(41.18f, -324.57f), new(41.22f, -329.01f), new(41.52f, -329.43f), new(57.95f, -329.4f),
    new(58.39f, -328.86f), new(58.66f, -328.25f), new(59.56f, -327.34f), new(60.07f, -327.28f), new(61.18f, -328.06f),
    new(61.52f, -328.58f), new(61.6f, -329.2f), new(77.89f, -329.41f), new(78.4f, -329.07f), new(79.09f, -328.99f),
    new(79.64f, -329.19f), new(81.05f, -329.4f), new(81.55f, -329.7f), new(81.51f, -330.3f), new(81.03f, -331),
    new(92.36f, -331.66f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.SentryR7, (uint)OID.SentryS7, (uint)OID.Turret];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Trash);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            hints.PotentialTargets[i].Priority = 0;
        }
    }
}
