namespace BossMod.Heavensward.Dungeon.D09SaintMociannesArboretum.D090MalboroTerra;

public enum OID : uint
{
    Boss = 0x141F, // R3.5
    BloomingOchu = 0x1421, // R1.6
    SouthernSeasColibri = 0x141E, // R0.6
    Korpokkur = 0x1420, // R1.0
    WallController = 0x1E9DF8 // R2.0
}

public enum AID : uint
{
    AutoAttack1 = 871, // SouthernSeasColibri->player, no cast, single-target
    AutoAttack2 = 872, // Boss/Korpokkur/BloomingOchu->player, no cast, single-target

    Loop = 923, // SouthernSeasColibri->player, no cast, single-target
    OffalBreath = 4532, // Boss->location, 4.0s cast, range 6 circle
    BullersDrop = 5354, // Korpokkur->player, no cast, single-target
    Spiritus = 5355 // Korpokkur->self, 3.0s cast, range 5+R 60-degree cone
}

class WallRemoval(BossModule module) : BossComponent(module)
{
    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008 && actor.OID == (uint)OID.WallController)
        {
            SetArena(D090MalboroTerra.Arena1B);
        }
    }

    public override void Update()
    {
        var pZ = Module.Raid.Player()!.PosRot.Z;
        if (Arena.Bounds == D090MalboroTerra.Arena1B && pZ < 16f)
            SetArena(D090MalboroTerra.Arena2);
        else if (Arena.Bounds == D090MalboroTerra.Arena2 && pZ > 16f)
            SetArena(D090MalboroTerra.Arena1B);
    }

    private void SetArena(ArenaBoundsCustom bounds)
    {
        Arena.Bounds = bounds;
        Arena.Center = bounds.Center;
    }
}

class OffalBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OffalBreath, 6f);
class Spiritus(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spiritus, new AOEShapeCone(6f, 30f.Degrees()));

class D090MalboroTerraStates : StateMachineBuilder
{
    public D090MalboroTerraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WallRemoval>()
            .ActivateOnEnter<OffalBreath>()
            .ActivateOnEnter<Spiritus>()
            .Raw.Update = () => AllDeadOrDestroyed(D090MalboroTerra.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 41, NameID = 4637, SortOrder = 1)]
public class D090MalboroTerra(WorldState ws, Actor primary) : BossModule(ws, primary, Arena1.Center, Arena1)
{
    private static readonly WPos[] vertices1 = [new(118.79f, 15.9f), new(119.08f, 16.5f), new(119.06f, 17.86f), new(119.03f, 18.51f), new(118.63f, 21.23f),
    new(117.9f, 23.79f), new(117.65f, 24.44f), new(116.83f, 26.31f), new(116.53f, 26.92f), new(115.25f, 29.11f),
    new(113.73f, 31.17f), new(113.33f, 31.68f), new(111.55f, 33.67f), new(109.64f, 35.54f), new(109.14f, 36f),
    new(107.04f, 37.81f), new(104.81f, 39.58f), new(84.42f, 54.32f), new(82.08f, 56.34f), new(80.27f, 58.08f),
    new(79.83f, 58.52f), new(78.12f, 60.35f), new(76.49f, 62.33f), new(76.11f, 62.85f), new(74.99f, 64.44f),
    new(74.65f, 64.98f), new(73.68f, 66.6f), new(73.35f, 67.22f), new(72.53f, 68.88f), new(71.72f, 70.9f),
    new(71.46f, 71.57f), new(70.83f, 73.63f), new(70.66f, 74.29f), new(70.24f, 76.14f), new(70.13f, 76.76f),
    new(69.73f, 79.22f), new(69.49f, 81.64f), new(69.44f, 82.33f), new(69.29f, 85.25f), new(69.24f, 92.53f),
    new(68.95f, 93.16f), new(68.69f, 93.8f), new(68.58f, 94.44f), new(68.74f, 95f), new(69.2f, 95.38f),
    new(69.75f, 95.45f), new(70.4f, 95.7f), new(76.1f, 101.4f), new(76.08f, 102.05f), new(76.29f, 102.57f),
    new(76.75f, 103.06f), new(76.71f, 111.34f), new(76.24f, 111.81f), new(76.03f, 112.42f), new(75.91f, 113.06f),
    new(70.25f, 118.71f), new(69.58f, 118.8f), new(69.03f, 119.01f), new(68.55f, 119.49f), new(60.43f, 119.49f),
    new(59.85f, 119.09f), new(59.31f, 118.82f), new(58.67f, 118.86f), new(58.13f, 118.35f), new(54.54f, 114.76f),
    new(53.6f, 113.77f), new(53.11f, 113.29f), new(53.15f, 112.69f), new(52.82f, 112.17f), new(52.43f, 111.69f),
    new(51.95f, 111.37f), new(51.36f, 111.36f), new(50.81f, 111.64f), new(48.2f, 112f),
    new(47.63f, 112f), new(46.76f, 112f), new(46.53f, 111.56f), new(46.47f, 104.94f),
    new(46.49f, 104.3f), new(46.54f, 103.65f), new(46.66f, 102.32f), new(50.27f, 102.27f), new(50.87f, 102.64f),
    new(51.45f, 102.92f), new(52.01f, 102.87f), new(52.5f, 102.51f), new(52.74f, 101.94f), new(52.73f, 101.3f),
    new(56.58f, 97.45f), new(57.09f, 96.97f), new(58.04f, 96.03f), new(58.65f, 95.75f), new(59.26f, 95.52f),
    new(59.82f, 95.17f), new(60.14f, 94.68f), new(60.14f, 94.08f), new(59.86f, 93.54f), new(59.52f, 92.95f),
    new(59.62f, 84.76f), new(59.79f, 81.45f), new(60.11f, 78.2f), new(60.53f, 75.45f), new(60.65f, 74.75f),
    new(61.28f, 71.89f), new(61.45f, 71.26f), new(62.28f, 68.52f), new(63.35f, 65.77f), new(63.54f, 65.31f),
    new(65.01f, 62.29f), new(66.61f, 59.59f), new(68.43f, 56.97f), new(68.82f, 56.45f), new(70.5f, 54.39f),
    new(70.94f, 53.88f), new(72.9f, 51.76f), new(75.31f, 49.43f), new(78.11f, 46.98f), new(80.63f, 44.95f),
    new(83.26f, 42.97f), new(96.38f, 33.78f), new(98.52f, 32.2f), new(100.58f, 30.59f), new(101.08f, 30.18f),
    new(102.55f, 28.92f), new(103.02f, 28.49f), new(104.38f, 27.18f), new(105.8f, 25.61f), new(107.1f, 23.89f),
    new(107.86f, 22.65f), new(108.18f, 22.01f), new(108.48f, 21.35f), new(108.75f, 20.69f), new(108.96f, 20.01f),
    new(109.15f, 19.34f), new(109.37f, 18.02f), new(109.44f, 17.43f), new(109.46f, 16.72f), new(109.45f, 16.04f),
    new(118.79f, 15.9f)];
    private static readonly WPos[] vertices2 = [new(101.95f, -22.44f), new(101.87f, -21.79f), new(101.81f, -21.12f), new(102.17f, -19.27f), new(102.31f, -18.65f),
    new(102.88f, -16.86f), new(103.81f, -14.38f), new(104.96f, -11.96f), new(105.27f, -11.4f), new(106.31f, -9.66f),
    new(106.66f, -9.09f), new(107.85f, -7.4f), new(108.24f, -6.87f), new(109.05f, -5.81f), new(109.57f, -5.34f),
    new(110.11f, -4.86f), new(110.7f, -4.61f), new(111.3f, -4.39f), new(111.97f, -4.21f), new(112.6f, -3.09f),
    new(112.56f, -2.42f), new(112.69f, -1.82f), new(112.84f, -1.24f), new(113.33f, -0.86f), new(113.86f, -0.49f),
    new(114.31f, 0.07f), new(114.63f, 0.66f), new(114.92f, 1.27f), new(116.09f, 3.77f), new(117.18f, 6.52f),
    new(117.39f, 7.15f), new(118.14f, 9.52f), new(118.7f, 12.08f), new(118.82f, 12.76f), new(119.04f, 14.8f),
    new(119.06f, 15.51f), new(119.05f, 16.19f), new(119.07f, 16.84f), new(119.05f, 18.1f), new(118.67f, 18.64f),
    new(109.82f, 20.52f), new(109.13f, 20.7f), new(108.96f, 20.1f), new(109.16f, 19.44f), new(109.39f, 18.09f),
    new(109.43f, 17.42f), new(109.46f, 16.7f), new(109.45f, 16.01f), new(109.33f, 14.68f), new(109.21f, 13.99f),
    new(108.95f, 12.69f), new(108.78f, 12.06f), new(108.21f, 10.19f), new(107.49f, 8.35f), new(107.23f, 7.72f),
    new(106.34f, 5.79f), new(106.04f, 5.17f), new(105.82f, 4.5f), new(105.86f, 3.85f), new(105.78f, 3.19f),
    new(105.58f, 2.66f), new(105.09f, 2.28f), new(104.56f, 1.93f), new(104.07f, 1.46f), new(99.24f, -6.99f),
    new(98.9f, -7.63f), new(93.23f, -17.44f), new(93.01f, -18.16f), new(93.49f, -18.38f), new(94.07f, -18.74f),
    new(94.69f, -19.05f), new(95.19f, -19.52f), new(96.64f, -21.06f), new(97.17f, -21.54f), new(97.74f, -21.98f),
    new(98.33f, -22.38f), new(99.01f, -22.58f), new(99.72f, -22.61f), new(100.42f, -22.62f), new(101f, -22.76f),
    new(101.49f, -23.18f), new(102.04f, -23.42f)];
    private static readonly WPos[] vertices3 = [new(80.68f, -76.27f), new(81.39f, -76.22f), new(82.03f, -76.09f), new(82.67f, -75.89f), new(83.27f, -75.63f),
    new(83.83f, -75.32f), new(84.36f, -74.95f), new(84.88f, -74.49f), new(85.3f, -74.01f), new(85.7f, -73.47f),
    new(86.03f, -72.88f), new(86.29f, -72.3f), new(86.5f, -71.64f), new(86.62f, -71.02f), new(86.68f, -70.36f),
    new(86.66f, -69.69f), new(86.58f, -69.05f), new(86.43f, -68.41f), new(86.23f, -67.8f), new(85.93f, -67.2f),
    new(85.57f, -66.62f), new(85.13f, -66.07f), new(84.66f, -65.59f), new(84.14f, -65.17f), new(83.56f, -64.8f),
    new(82.97f, -64.51f), new(82.27f, -64.35f), new(81.58f, -64.25f), new(81.01f, -63.84f), new(80.55f, -63.38f),
    new(80.68f, -62.83f), new(81.22f, -62.47f), new(81.73f, -62.04f), new(82.17f, -61.57f), new(82.55f, -61.05f),
    new(82.88f, -60.47f), new(83.11f, -59.85f), new(83.29f, -59.17f), new(83.38f, -58.5f), new(83.39f, -57.79f),
    new(83.3f, -57.12f), new(83.13f, -56.46f), new(82.75f, -56.09f), new(82.23f, -55.65f), new(82.49f, -55.01f),
    new(82.69f, -54.54f), new(83.09f, -54.09f), new(83.73f, -54.19f), new(84.4f, -54.13f), new(85.04f, -53.86f),
    new(85.59f, -53.4f), new(85.95f, -52.82f), new(86.12f, -52.17f), new(86.09f, -51.47f), new(85.93f, -50.99f),
    new(86.14f, -50.5f), new(86.77f, -50.25f), new(87.37f, -50.12f), new(88.03f, -50.25f), new(88.68f, -50.3f),
    new(89.35f, -50.26f), new(90.02f, -50.14f), new(90.63f, -49.95f), new(91.22f, -49.68f), new(91.8f, -49.33f),
    new(92.34f, -48.9f), new(92.84f, -48.39f), new(93.23f, -47.86f), new(93.57f, -47.25f), new(93.82f, -46.65f),
    new(93.99f, -46.02f), new(94.08f, -45.37f), new(94.1f, -44.73f), new(94.04f, -44.08f), new(93.9f, -43.46f),
    new(93.69f, -42.85f), new(93.42f, -42.29f), new(93.06f, -41.74f), new(92.64f, -41.25f), new(92.17f, -40.81f),
    new(91.64f, -40.42f), new(91.24f, -40.03f), new(91.37f, -39.48f), new(91.66f, -39.01f), new(92.22f, -38.79f),
    new(92.76f, -38.42f), new(93.17f, -37.91f), new(93.44f, -37.3f), new(93.53f, -36.65f), new(93.43f, -35.98f),
    new(93.17f, -35.39f), new(92.79f, -34.9f), new(92.88f, -34.23f), new(93.19f, -33.75f), new(93.73f, -33.67f),
    new(94.35f, -33.62f), new(94.97f, -33.5f), new(95.6f, -33.32f), new(96.21f, -33.09f), new(96.77f, -32.81f),
    new(97.3f, -32.47f), new(97.83f, -32.08f), new(98.29f, -31.67f), new(98.72f, -31.19f), new(99.12f, -30.66f),
    new(99.48f, -30.07f), new(99.78f, -29.46f), new(99.99f, -28.87f), new(100.15f, -28.25f), new(100.27f, -27.63f),
    new(100.31f, -26.98f), new(100.3f, -26.34f), new(100.24f, -25.71f), new(100.11f, -25.09f), new(99.93f, -24.49f),
    new(99.7f, -23.9f), new(99.42f, -23.34f), new(99.43f, -22.81f), new(100f, -22.68f), new(100.56f, -22.51f),
    new(100.25f, -22.02f), new(99.77f, -21.56f), new(99.27f, -21.15f), new(98.74f, -20.78f), new(98.18f, -20.45f),
    new(97.6f, -20.16f), new(94.69f, -18.82f), new(94.16f, -18.72f), new(93.91f, -19.37f), new(93.57f, -19.84f),
    new(92.99f, -19.88f), new(92.38f, -19.94f), new(91.76f, -20.06f), new(91.15f, -20.25f), new(90.56f, -20.49f),
    new(89.98f, -20.77f), new(89.42f, -21.15f), new(88.9f, -21.55f), new(88.41f, -22.02f), new(87.97f, -22.52f),
    new(87.61f, -23.05f), new(87.29f, -23.6f), new(87f, -24.21f), new(86.77f, -24.9f), new(86.62f, -25.56f),
    new(86.53f, -26.26f), new(86.52f, -26.95f), new(86.56f, -27.61f), new(86.69f, -28.29f), new(86.68f, -28.89f),
    new(86.14f, -29.15f), new(85.53f, -29.31f), new(85f, -29.25f), new(84.4f, -28.96f), new(83.78f, -28.75f),
    new(83.14f, -28.59f), new(82.46f, -28.5f), new(81.77f, -28.49f), new(81.11f, -28.56f), new(80.49f, -28.68f),
    new(79.84f, -28.89f), new(79.2f, -29.18f), new(78.61f, -29.53f), new(78.09f, -29.92f), new(77.59f, -30.39f),
    new(77.14f, -30.92f), new(76.76f, -31.47f), new(76.45f, -32.08f), new(76.22f, -32.69f), new(76.04f, -33.34f),
    new(76.27f, -33.99f), new(76.08f, -34.58f), new(75.53f, -35.09f), new(75f, -35.57f), new(74.49f, -35.45f),
    new(73.93f, -35.14f), new(73.26f, -34.97f), new(72.58f, -34.97f), new(71.97f, -35.14f), new(71.41f, -35.43f),
    new(70.91f, -35.87f), new(70.54f, -36.42f), new(70.3f, -37.06f), new(70.23f, -37.74f), new(70.33f, -38.4f),
    new(70.61f, -39.04f), new(71.02f, -39.58f), new(71.57f, -40f), new(72.27f, -40.16f), new(72.96f, -40.34f),
    new(73.35f, -40.78f), new(73.3f, -41.44f), new(72.83f, -41.85f), new(72.4f, -42.29f), new(71.99f, -42.8f),
    new(71.64f, -43.35f), new(71.35f, -43.93f), new(71.14f, -44.54f), new(70.99f, -45.22f), new(70.91f, -45.88f),
    new(70.91f, -46.55f), new(70.98f, -47.21f), new(71.13f, -47.85f), new(71.34f, -48.46f), new(71.64f, -49.07f),
    new(72f, -49.65f), new(72.46f, -50.2f), new(72.96f, -50.69f), new(73.49f, -51.1f), new(74.06f, -51.43f),
    new(74.75f, -51.61f), new(75.35f, -51.9f), new(75.59f, -52.91f), new(75.36f, -53.48f), new(74.78f, -53.87f),
    new(74.3f, -54.3f), new(73.85f, -54.8f), new(73.46f, -55.37f), new(73.17f, -55.93f), new(72.95f, -56.52f),
    new(72.78f, -57.21f), new(72.7f, -57.86f), new(72.72f, -58.59f), new(72.86f, -59.28f), new(73.05f, -59.93f),
    new(73.35f, -60.61f), new(74.05f, -60.8f), new(74.2f, -61.45f), new(74.02f, -61.94f), new(73.73f, -62.54f),
    new(73.15f, -62.51f), new(72.45f, -62.51f), new(71.8f, -62.7f), new(71.25f, -63.05f), new(70.78f, -63.57f),
    new(70.5f, -64.15f), new(70.38f, -64.83f), new(70.46f, -65.53f), new(70.73f, -66.16f), new(71.09f, -66.81f),
    new(73.45f, -70.47f), new(73.85f, -71.04f), new(75.67f, -73.84f), new(76.11f, -74.35f), new(76.61f, -74.82f),
    new(77.13f, -75.21f), new(77.71f, -75.56f), new(78.3f, -75.84f), new(78.92f, -76.05f), new(79.55f, -76.18f),
    new(80.21f, -76.25f)];
    public static readonly ArenaBoundsCustom Arena1 = new([new PolygonCustom(vertices1)]);
    public static readonly ArenaBoundsCustom Arena1B = new([new PolygonCustom(vertices1), new PolygonCustom(vertices2), new PolygonCustom(vertices3)]);
    public static readonly ArenaBoundsCustom Arena2 = new([new PolygonCustom(vertices2), new PolygonCustom(vertices3)]);
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.BloomingOchu, (uint)OID.SouthernSeasColibri, (uint)OID.Korpokkur];

    protected override bool CheckPull() => IsAnyActorInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Trash);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
            hints.PotentialTargets[i].Priority = 0;
    }
}
