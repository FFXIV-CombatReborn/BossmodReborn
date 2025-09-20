namespace BossMod.Heavensward.Dungeon.D15Xelphatol.D150XelphatolSwiftbeak;

public enum OID : uint
{
    Boss = 0x17BA, // R0.9
    XelphatolFatecaller = 0x17B4, // R1.08
    XelphatolStrongbeak = 0x17B3, // R1.08
    XelphatolBravewing = 0x17B2, // R1.08
    XelphatolWhirltalon = 0x17B1, // R1.08
    XelphatolWindtalon = 0x17B9, // R0.9
    XelphatolFogcaller = 0x17BB, // R0.9
    XelphatolWatchwolf = 0x17B6, // R1.65
    AbalathianHornbill = 0x17B7, // R1.8
    BoneKey = 0x1EA165, // R1.8
    Airstone1 = 0x1EA167,
    Airstone2 = 0x1EA166
}

public enum AID : uint
{
    AutoAttack1 = 871, // XelphatolStrongbeak/XelphatolSwiftbeak/AbalathianHornbill->player, no cast, single-target
    AutoAttack2 = 870, // XelphatolWhirltalon/XelphatolWindtalon/XelphatolBravewing/XelphatolWatchwolf->player, no cast, single-target

    ComingStorm = 423, // XelphatolWhirltalon/XelphatolSwiftbeak/XelphatolStrongbeak/XelphatolWindtalon->self, 2.5s cast, single-target
    Aero = 969, // Boss/XelphatolFogcaller->player, 1.0s cast, single-target
    TrueThrust = 722, // XelphatolStrongbeak/XelphatolSwiftbeak->player, no cast, single-target
    FoulBite = 510, // XelphatolWatchwolf->player, no cast, single-target
    FastBlade = 717, // XelphatolWhirltalon/XelphatolWindtalon->player, no cast, single-target
    Overpower = 720, // XelphatolBravewing->self, 2.5s cast, range 6+R 90-degree cone
    Gust = 6627, // AbalathianHornbill->location, 3.0s cast, range 5 circle
    Breakbeak = 6626 // AbalathianHornbill->player, no cast, single-target
}

sealed class Overpower(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Overpower, new AOEShapeCone(7.08f, 45f.Degrees()));
sealed class Gust(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Gust, 5f);

sealed class D150XelphatolSwiftbeakStates : StateMachineBuilder
{
    public D150XelphatolSwiftbeakStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Overpower>()
            .ActivateOnEnter<Gust>()
            .Raw.Update = () => AllDeadOrDestroyedInBounds(D150XelphatolSwiftbeak.Trash) || AnyTargetable(D150XelphatolSwiftbeak.Keys);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 182, NameID = 5261, SortOrder = 4)]
public sealed class D150XelphatolSwiftbeak : BossModule
{
    public D150XelphatolSwiftbeak(WorldState ws, Actor primary) : this(ws, primary, primary.Position is var pos && pos.X < 200f && pos.Z > -200f ? BuildArena1() : pos.Z < -200f ? BuildArena2() : BuildArena3()) { }

    private D150XelphatolSwiftbeak(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena1()
    {
        WPos[] vertices = [new(152.49f, -71.52f), new(150.27f, -69.29f), new(149.68f, -68.8f), new(147.76f, -70.72f), new(147.27f, -71.05f),
        new(146.79f, -70.79f), new(146.28f, -70.33f), new(145.76f, -70.31f), new(145.3f, -69.98f), new(143.97f, -68.28f),
        new(143.63f, -67.66f), new(144.19f, -67.53f), new(145.39f, -68.3f), new(145.89f, -68.72f), new(146.5f, -69.06f),
        new(149.91f, -65.65f), new(152.35f, -62.06f), new(152.74f, -61.62f), new(153.26f, -61.56f), new(153.85f, -61.7f),
        new(154.35f, -61.97f), new(159.95f, -67.57f), new(160.34f, -67.18f), new(160.7f, -66.57f), new(161.26f, -66.2f),
        new(162.47f, -65.89f), new(164.88f, -63.55f), new(164.81f, -62.38f), new(164.64f, -61.73f), new(164.91f, -61.26f),
        new(165.38f, -60.99f), new(165.98f, -61.15f), new(166.64f, -61.08f), new(167.12f, -61.39f), new(167.75f, -61.26f),
        new(169.19f, -59.76f), new(169.14f, -59.07f), new(169.03f, -58.52f), new(168.95f, -57.24f), new(169.38f, -56.81f),
        new(169.94f, -56.81f), new(170.57f, -57.01f), new(171.25f, -56.81f), new(171.89f, -56.45f), new(172.42f, -56),
        new(173.84f, -54.58f), new(174.09f, -53.44f), new(174.98f, -52.51f), new(175.31f, -51.9f), new(158.57f, -35.16f),
        new(158.14f, -34.88f), new(155.17f, -37.89f), new(154.78f, -38.48f), new(155.14f, -38.86f), new(155.17f, -39.47f),
        new(155.05f, -40.14f), new(154.66f, -40.57f), new(154.04f, -40.79f), new(153.59f, -40.42f), new(153.45f, -39.48f),
        new(153.14f, -39.9f), new(152.46f, -39.78f), new(151.91f, -39.51f), new(123.63f, -11.23f), new(123.65f, -10.19f),
        new(122.66f, -9.44f), new(122.22f, -9), new(122.04f, -8.43f), new(122.38f, -7.94f), new(122.78f, -7.6f),
        new(123.28f, -7.83f), new(123.8f, -8.22f), new(124.06f, -8.72f), new(124.53f, -9.02f), new(129.44f, -4.09f),
        new(129.9f, -3.78f), new(130.4f, -4.08f), new(144.24f, -17.92f), new(144.73f, -17.74f), new(150.94f, -11.52f),
        new(151.27f, -10.89f), new(147.57f, -7.19f), new(146.88f, -6.96f), new(145.51f, -6.86f), new(145.28f, -6.4f),
        new(145.14f, -5.74f), new(145.13f, -5.01f), new(145.22f, -4.45f), new(146.19f, -3.62f), new(146.67f, -3.44f),
        new(148.54f, -5.25f), new(149.17f, -5.59f), new(151.44f, -3.3f), new(151.72f, -2.85f), new(145.31f, 3.56f),
        new(144.7f, 3.95f), new(144.3f, 3.6f), new(142.37f, 1.63f), new(142.13f, 1.16f), new(143.98f, -0.69f),
        new(144.31f, -1.17f), new(144.23f, -2.46f), new(141.94f, -3.9f), new(141.44f, -3.82f), new(141.49f, -3.27f),
        new(141.8f, -2.73f), new(142.22f, -2.29f), new(142.04f, -1.69f), new(136.81f, 3.55f), new(136.15f, 3.94f),
        new(135.98f, 3.41f), new(135.5f, 3.05f), new(134.89f, 2.84f), new(134.31f, 3.03f), new(133.87f, 3.39f),
        new(133.63f, 4f), new(133.79f, 4.55f), new(134.27f, 5.05f), new(134.88f, 5.5f), new(132.13f, 8.26f),
        new(131.67f, 7.94f), new(123.71f, 0.04f), new(123.21f, 0.2f), new(121.82f, 1.58f), new(121.42f, 1.93f),
        new(117.11f, -2.41f), new(117.09f, -2.93f), new(116.83f, -3.39f), new(116.23f, -3.58f), new(115.52f, -3.64f),
        new(115.02f, -3.41f), new(114.59f, -2.81f), new(114.14f, -2.36f), new(97.01f, 2.27f), new(91.6f, 0.34f),
        new(88.26f, -1.92f), new(87.89f, -2.28f), new(88.29f, -2.67f), new(92.38f, -5.24f), new(92.88f, -5.38f),
        new(102.76f, -2.94f), new(108.8f, -5.14f), new(109.41f, -5.47f), new(110.12f, -5.63f), new(110.77f, -5.9f),
        new(111.35f, -6.29f), new(111.78f, -6.82f), new(111.85f, -7.35f), new(111.52f, -7.97f), new(109.03f, -10.51f),
        new(106.15f, -13.34f), new(106.35f, -13.96f), new(107.7f, -15.31f), new(107.86f, -15.82f), new(107.48f, -16.28f),
        new(99.82f, -23.94f), new(99.91f, -24.46f), new(102.28f, -26.84f), new(102.67f, -26.28f), new(103.27f, -25.88f),
        new(103.85f, -25.73f), new(104.48f, -25.95f), new(104.85f, -26.39f), new(105.02f, -27.01f), new(104.79f, -27.49f),
        new(104.36f, -27.94f), new(103.79f, -28.39f), new(105.79f, -30.36f), new(107.44f, -31.8f), new(107.89f, -32.3f),
        new(107.84f, -32.86f), new(104.66f, -36.06f), new(104.39f, -36.68f), new(110.73f, -43.03f), new(111.3f, -43.47f),
        new(115.04f, -39.71f), new(115.35f, -39.12f), new(115.26f, -38.43f), new(115.25f, -37.76f), new(115.66f, -37.37f),
        new(117.68f, -37.35f), new(118.27f, -37.43f), new(118.58f, -37.82f), new(118.81f, -38.47f), new(118.71f, -38.96f),
        new(117.36f, -40.32f), new(116.89f, -40.86f), new(117.26f, -41.85f), new(119.14f, -43.68f), new(119.94f, -43.31f),
        new(125.96f, -37.24f), new(126.29f, -36.81f), new(123.91f, -34.45f), new(123.44f, -34.24f), new(121.6f, -36.07f),
        new(121.15f, -36.4f), new(120.67f, -36.19f), new(120.18f, -35.7f), new(119.68f, -35.61f), new(118.96f, -35.57f),
        new(118.51f, -35.21f), new(118.14f, -34.65f), new(117.83f, -34.09f), new(118.47f, -33.76f), new(119.1f, -33.62f),
        new(119.58f, -33.89f), new(120.06f, -34.37f), new(120.59f, -34.17f), new(122.08f, -32.61f), new(111.86f, -22.43f),
        new(111.7f, -21.91f), new(112.06f, -21.47f), new(115.95f, -17.58f), new(116.41f, -17.03f), new(116.17f, -16.43f),
        new(116f, -15.86f), new(115.96f, -15.25f), new(116.28f, -14.78f), new(116.79f, -14.58f), new(117.36f, -14.78f),
        new(117.82f, -15.25f), new(119.11f, -15.74f), new(147.2f, -43.79f), new(147.6f, -44.28f), new(147.75f, -44.97f),
        new(147.98f, -45.51f), new(148.1f, -46.11f), new(147.77f, -46.57f), new(147.32f, -46.94f), new(146.74f, -47.23f),
        new(146.09f, -47.47f), new(145.4f, -47.64f), new(142.51f, -50.56f), new(142.14f, -51.12f), new(141.71f, -51.63f),
        new(141.13f, -51.6f), new(137.4f, -47.93f), new(136.97f, -47.57f), new(132.04f, -52.51f), new(132.09f, -53.05f),
        new(131.82f, -53.57f), new(130.88f, -54.62f), new(130.32f, -55.07f), new(129.78f, -54.98f), new(129.21f, -54.7f),
        new(128.72f, -54.39f), new(128.54f, -53.84f), new(128.06f, -53.39f), new(127.43f, -53.4f), new(126.98f, -53.89f),
        new(126.89f, -54.4f), new(127.25f, -54.95f), new(129.7f, -57.25f), new(129.54f, -57.74f), new(129, -57.94f),
        new(128.47f, -57.74f), new(127.64f, -56.93f), new(127.26f, -57.31f), new(127.13f, -57.94f), new(127.16f, -58.54f),
        new(127.13f, -59.17f), new(129.4f, -61.42f), new(129.91f, -61.77f), new(130.78f, -62.57f), new(130.78f, -63.16f),
        new(127.34f, -66.63f), new(127.1f, -67.12f), new(128.18f, -67.39f), new(128.97f, -68.43f), new(128.92f, -69.11f),
        new(128.68f, -69.67f), new(128.38f, -70.2f), new(127.51f, -71.06f), new(126.93f, -71.06f), new(126.39f, -70.86f),
        new(124.4f, -68.9f), new(123.93f, -69.09f), new(122.03f, -71), new(121.61f, -71.66f), new(128.23f, -78.23f),
        new(128.89f, -78.2f), new(130.78f, -76.28f), new(131.2f, -75.66f), new(129.56f, -74.52f), new(129.04f, -74.09f),
        new(129f, -73.4f), new(129.26f, -72.85f), new(129.73f, -72.38f), new(129.96f, -71.93f), new(130.5f, -70.54f),
        new(130.8f, -70.11f), new(131.38f, -70.01f), new(131.74f, -70.44f), new(131.89f, -71.05f), new(131.45f, -71.84f),
        new(132.12f, -71.93f), new(132.8f, -72.2f), new(133.36f, -72.58f), new(133.81f, -72.32f), new(136.85f, -69.29f),
        new(137.38f, -69.4f), new(139.79f, -71.82f), new(140.43f, -72.12f), new(141.1f, -72.26f), new(141.49f, -72.61f),
        new(141.86f, -72.27f), new(143.06f, -71.57f), new(143.65f, -71.37f), new(144.29f, -71.67f), new(144.85f, -72.05f),
        new(144.97f, -72.75f), new(145f, -73.4f), new(144.61f, -73.89f), new(143.22f, -75.27f), new(142.79f, -75.87f),
        new(145.52f, -78.54f)];
        var arena = new ArenaBoundsCustom([new PolygonCustom(vertices)]);
        return (arena.Center, arena);
    }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena2()
    {
        WPos[] vertices = [new(424.15f, -281.72f), new(425.91f, -280.7f), new(426.84f, -279.74f), new(427.41f, -279.61f), new(428.02f, -279.57f),
        new(428.68f, -279.45f), new(430.08f, -278.06f), new(430.61f, -278.11f), new(431.83f, -278.47f), new(432.46f, -278.33f),
        new(433.89f, -276.3f), new(434.46f, -276.19f), new(436.98f, -275.94f), new(443.23f, -277.15f), new(442.81f, -276.87f),
        new(442.67f, -276.35f), new(442.88f, -275.7f), new(443.19f, -275.18f), new(443.63f, -274.71f), new(444.16f, -274.62f),
        new(444.75f, -274.8f), new(445.24f, -275.12f), new(445.38f, -275.73f), new(445.34f, -276.33f), new(445.22f, -276.97f),
        new(445.54f, -277.54f), new(446.18f, -277.73f), new(448.77f, -278.22f), new(450.58f, -268.77f), new(450.12f, -268.37f),
        new(447.58f, -267.92f), new(447.11f, -268.36f), new(446.58f, -269.62f), new(446.22f, -270.05f), new(445.64f, -270.09f),
        new(445.02f, -269.99f), new(444.49f, -269.77f), new(444.31f, -269.2f), new(444.25f, -268.6f), new(443.97f, -267.96f),
        new(443.45f, -267.49f), new(438.03f, -266.4f), new(437.38f, -266.19f), new(436.8f, -265.83f), new(436.7f, -265.32f),
        new(437.11f, -264.95f), new(437.78f, -264.75f), new(438.39f, -264.76f), new(439.01f, -264.84f), new(438.14f, -262.98f),
        new(438.27f, -262.31f), new(438.63f, -261.81f), new(438.86f, -261.17f), new(438.81f, -260.46f), new(435.89f, -254.52f),
        new(435.44f, -253.98f), new(433.8f, -252.73f), new(431.91f, -252.15f), new(431.38f, -251.84f), new(429.92f, -250.38f),
        new(427.96f, -249.81f), new(427.56f, -249.46f), new(426.75f, -247.02f), new(426.58f, -244.48f), new(426.19f, -243.21f),
        new(425.3f, -242.25f), new(424.96f, -241.77f), new(425.16f, -241.23f), new(425.8f, -240.01f), new(424.49f, -237.92f),
        new(424.29f, -237.36f), new(424.14f, -236.73f), new(423.9f, -236.12f), new(420.21f, -233.59f), new(419.49f, -232.57f),
        new(419.01f, -232.07f), new(417.74f, -231.73f), new(415.19f, -229.65f), new(413.84f, -228.3f), new(413.45f, -227.85f),
        new(413.16f, -226.55f), new(412.68f, -226.82f), new(406.34f, -232.53f), new(405.89f, -233.08f), new(405.51f, -233.61f),
        new(405.18f, -234.22f), new(405f, -234.84f), new(405.28f, -235.45f), new(405.8f, -235.82f), new(406.38f, -236.17f),
        new(407.72f, -236.09f), new(409.72f, -237.73f), new(410.14f, -238.16f), new(410.84f, -239.23f), new(410.89f, -239.85f),
        new(410.87f, -240.46f), new(411.18f, -241.03f), new(411.61f, -241.46f), new(411.95f, -241.93f), new(412.15f, -242.43f),
        new(412.07f, -242.99f), new(411.39f, -243.96f), new(411.68f, -244.4f), new(412.2f, -244.84f), new(412.92f, -244.89f),
        new(413.43f, -245.07f), new(413.9f, -245.57f), new(414.2f, -246.04f), new(414.01f, -246.6f), new(413.46f, -246.98f),
        new(413.52f, -247.57f), new(414.13f, -247.85f), new(414.62f, -248.23f), new(415.01f, -248.64f), new(414.84f, -249.15f),
        new(414.33f, -249.53f), new(413.39f, -250.45f), new(413.48f, -250.96f), new(413.72f, -251.55f), new(413.8f, -252.16f),
        new(413.69f, -252.72f), new(410.03f, -254.04f), new(409.58f, -254.55f), new(409f, -254.52f), new(406.5f, -254.04f),
        new(405.85f, -253.97f), new(403.3f, -253.44f), new(402.79f, -253.41f), new(402.27f, -254.66f), new(401.96f, -255.15f),
        new(401.52f, -255.6f), new(400.32f, -255.46f), new(399.89f, -255.08f), new(399.73f, -254.43f), new(399.88f, -253.89f),
        new(400.29f, -253.37f), new(400.14f, -252.87f), new(399.45f, -252.68f), new(396.37f, -252.08f), new(394.57f, -261.32f),
        new(394.54f, -261.84f), new(397.04f, -262.31f), new(397.79f, -262.4f), new(398.24f, -261.23f), new(398.59f, -260.65f),
        new(399.07f, -260.21f), new(400.36f, -260.26f), new(400.74f, -260.73f), new(400.91f, -261.38f), new(401.17f, -262.07f),
        new(401.94f, -263.13f), new(402.41f, -263.37f), new(405.2f, -263.92f), new(406.48f, -263.95f), new(407.09f, -264.04f),
        new(407.63f, -264.27f), new(407.59f, -264.95f), new(407.24f, -265.45f), new(406.67f, -265.77f), new(405.51f, -266.3f),
        new(404.9f, -266.65f), new(404.36f, -267.06f), new(404.05f, -267.64f), new(403.73f, -268.9f), new(403.88f, -269.59f),
        new(404.11f, -270.2f), new(404.68f, -270.62f), new(406.62f, -271.35f), new(406.95f, -271.79f), new(407.08f, -273.02f),
        new(407.34f, -273.64f), new(407.85f, -274.13f), new(409.21f, -274.58f), new(409.71f, -274.82f), new(409.88f, -275.4f),
        new(410.21f, -276.01f), new(411.4f, -276.57f), new(411.93f, -276.89f), new(412.75f, -278.54f), new(413.24f, -278.36f),
        new(414.29f, -277.67f), new(418.52f, -278.64f), new(418.68f, -279.2f), new(418.61f, -279.82f), new(418.67f, -280.5f),
        new(419.3f, -280.72f), new(419.95f, -280.81f), new(421.2f, -280.81f), new(423.68f, -281.65f)];
        var arena = new ArenaBoundsCustom([new PolygonCustom(vertices)]);
        return (arena.Center, arena);
    }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena3()
    {
        WPos[] vertices = [new(302.73f, -161.99f), new(302.61f, -161.34f), new(302.15f, -160.81f), new(301.76f, -160.27f), new(301.49f, -159.68f),
        new(301.72f, -159.14f), new(302.11f, -158.57f), new(302.6f, -158.31f), new(303.27f, -158.45f), new(304.24f, -159.21f),
        new(304.75f, -159.2f), new(306.85f, -156.59f), new(307.13f, -155.99f), new(307.64f, -155.48f), new(308.16f, -155.03f),
        new(308.75f, -154.68f), new(308.83f, -153.97f), new(308.59f, -153.25f), new(308.56f, -152.71f), new(309.04f, -152.36f),
        new(309.59f, -152.37f), new(309.75f, -152.94f), new(310.04f, -154.92f), new(309.84f, -156.95f), new(309.96f, -158.15f),
        new(310.72f, -159.25f), new(311.42f, -159.33f), new(312.07f, -159.33f), new(312.7f, -159.08f), new(314.34f, -157.83f),
        new(314.92f, -157.76f), new(316.86f, -157.94f), new(317.52f, -157.65f), new(318.14f, -157.27f), new(318.67f, -156.82f),
        new(319.22f, -156.47f), new(319.82f, -156.53f), new(320.53f, -156.38f), new(321.72f, -155.61f), new(322.33f, -155.48f),
        new(323.71f, -155.58f), new(324.37f, -155.3f), new(325.38f, -154.31f), new(325.61f, -153.85f), new(326.17f, -151.95f),
        new(326.14f, -151.28f), new(326.35f, -150.68f), new(326.72f, -150.27f), new(327.29f, -150.43f), new(327.81f, -150.82f),
        new(328.45f, -150.68f), new(331.62f, -146.53f), new(331.83f, -146.07f), new(331.35f, -143.4f), new(331.49f, -142.83f),
        new(331.8f, -142.22f), new(331.7f, -141.53f), new(331.32f, -140.23f), new(331.22f, -139.61f), new(332.49f, -137.38f),
        new(332.23f, -136.73f), new(331.51f, -135.55f), new(331.73f, -134.92f), new(332.03f, -134.33f), new(332.06f, -133.63f),
        new(332.2f, -132.97f), new(332.54f, -132.34f), new(339.24f, -123.51f), new(339.45f, -122.84f), new(331.97f, -117.15f),
        new(331.35f, -117.47f), new(325.74f, -124.85f), new(325.59f, -125.51f), new(325.4f, -126.11f), new(325.08f, -126.77f),
        new(324.68f, -127.37f), new(324.19f, -127.85f), new(323.62f, -128.16f), new(323.03f, -128.15f), new(322.75f, -127.55f),
        new(322.64f, -126.96f), new(322.89f, -126.4f), new(323.27f, -125.68f), new(321.29f, -125.78f), new(320.82f, -125.57f),
        new(319.98f, -124.59f), new(319.3f, -124.45f), new(317.9f, -124.58f), new(317.2f, -124.59f), new(316.51f, -124.48f),
        new(315.83f, -124.29f), new(315.17f, -124.04f), new(314.69f, -123.64f), new(314.7f, -121.7f), new(314.53f, -121.02f),
        new(313.61f, -118.49f), new(314.35f, -116.04f), new(312.59f, -111.89f), new(312.18f, -111.33f), new(307.85f, -107.41f),
        new(307.42f, -106.91f), new(305.03f, -103.72f), new(304.75f, -103.19f), new(304.67f, -102.54f), new(304.04f, -102.45f),
        new(302.13f, -103.34f), new(301.7f, -103.59f), new(295.22f, -108.33f), new(295.48f, -108.79f), new(295.95f, -109.27f),
        new(297.15f, -109.62f), new(297.67f, -109.86f), new(299.36f, -111.79f), new(299.66f, -112.33f), new(300.11f, -113.51f),
        new(300f, -114.07f), new(299.83f, -114.67f), new(299.93f, -115.34f), new(300.82f, -116.26f), new(300.93f, -116.76f),
        new(300.33f, -117.87f), new(300.45f, -118.52f), new(301.44f, -119.45f), new(302.2f, -120.51f), new(302.03f, -121.02f),
        new(301.74f, -121.63f), new(301.74f, -122.31f), new(302.13f, -122.89f), new(302.42f, -123.44f), new(302.17f, -123.89f),
        new(301.23f, -124.96f), new(300.79f, -125.62f), new(301.72f, -126.25f), new(304.12f, -128.51f), new(304.07f, -129.17f),
        new(302.29f, -130.96f), new(299.44f, -134.59f), new(296.95f, -136.83f), new(296.82f, -137.5f), new(296.94f, -138.8f),
        new(297.35f, -139.3f), new(299.5f, -140.68f), new(299.55f, -141.21f), new(299.22f, -142.45f), new(299.55f, -142.85f),
        new(301.13f, -144.1f), new(301.12f, -144.67f), new(300.86f, -146.6f), new(301.01f, -147.85f), new(296.9f, -153.28f),
        new(297.14f, -153.73f), new(297.68f, -154.12f), new(298.14f, -154.56f), new(298.4f, -155.12f), new(298.28f, -155.68f),
        new(298.26f, -156.33f), new(297.75f, -156.65f), new(297.15f, -156.84f), new(296.68f, -156.64f), new(296.25f, -156.18f),
        new(295.69f, -155.83f), new(295.06f, -156.04f), new(294.51f, -156.41f), new(293.27f, -158.04f), new(293.17f, -158.54f),
        new(300.31f, -163.97f), new(300.93f, -164.37f)];
        var arena = new ArenaBoundsCustom([new PolygonCustom(vertices)]);
        return (arena.Center, arena);
    }

    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.XelphatolWindtalon, (uint)OID.XelphatolWhirltalon, (uint)OID.XelphatolWatchwolf, (uint)OID.XelphatolFogcaller,
    (uint)OID.XelphatolBravewing, (uint)OID.XelphatolFatecaller, (uint)OID.XelphatolStrongbeak, (uint)OID.AbalathianHornbill];
    public static readonly uint[] Keys = [(uint)OID.BoneKey, (uint)OID.Airstone1, (uint)OID.Airstone2];

    protected override bool CheckPull() => IsAnyActorInBoundsInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.ActorsInBounds(this, Trash);
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
