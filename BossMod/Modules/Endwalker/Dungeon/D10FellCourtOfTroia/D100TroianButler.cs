namespace BossMod.Endwalker.Dungeon.D10FellCourtOfTroia.D100TroianButler;

public enum OID : uint
{
    Boss = 0x3986, // R2.0
    TroianTrapper = 0x3984, // R2.5
    TroianSteward = 0x3987, // R2.34
    TroianEquerry = 0x3985 // R2.6
}

public enum AID : uint
{
    AutoAttack1 = 6497, // Boss/TroianEquerry->player, no cast, single-target
    AutoAttack2 = 6499, // TroianTrapper/TroianSteward->player, no cast, single-target

    ArachneWeb = 30223, // TroianTrapper->location, 3.0s cast, range 6 circle
    Swoop = 30221, // TroianEquerry->self, 3.0s cast, range 6 circle
    UnholyDarkness = 30220, // TroianSteward->self, 4.0s cast, range 8 circle
    Karma = 30219, // TroianSteward->self, 5.0s cast, range 30 90-degree cone
    Dark = 30222 // Boss->self, 3.0s cast, range 6 120-degree cone
}

sealed class Karma(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Karma, new AOEShapeCone(30f, 45f.Degrees()));
sealed class Dark(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Dark, new AOEShapeCone(6f, 60f.Degrees()));
sealed class ArachneWeb(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArachneWeb, 6f);
sealed class Swoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Swoop, 6f);
sealed class UnholyDarkness(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UnholyDarkness, 8f);

sealed class D100TroianButlerStates : StateMachineBuilder
{
    public D100TroianButlerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Karma>()
            .ActivateOnEnter<Dark>()
            .ActivateOnEnter<ArachneWeb>()
            .ActivateOnEnter<Swoop>()
            .ActivateOnEnter<UnholyDarkness>()
            .Raw.Update = () => AllDeadOrDestroyedInBounds(D100TroianButler.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 869, NameID = 11367, SortOrder = 6)]
public sealed class D100TroianButler(WorldState ws, Actor primary) : BossModule(ws, primary, IsArena1(primary) ? arena1.Center : arena2.Center, IsArena1(primary) ? arena1 : arena2)
{
    private static bool IsArena1(Actor primary) => primary.PosRot.Z > -138f;
    private static readonly WPos[] vertices1 = [new(2.32f, -133.04f), new(2.68f, -132.67f), new(3.2f, -132.4f), new(3.84f, -132.15f), new(10.5f, -128.57f),
    new(11.08f, -128.13f), new(11.63f, -127.91f), new(12.22f, -127.75f), new(12.87f, -127.46f), new(14.47f, -125.46f),
    new(15.98f, -122.63f), new(16.17f, -122.01f), new(16.13f, -121.35f), new(16.2f, -120.76f), new(16.6f, -120.31f),
    new(16.88f, -119.71f), new(17.01f, -119.05f), new(17.3f, -115.85f), new(17.16f, -114.47f), new(17.06f, -113.83f),
    new(17f, -113.19f), new(16.76f, -112.59f), new(16.46f, -112.09f), new(16.49f, -111.51f), new(16.44f, -110.83f),
    new(16.08f, -109.65f), new(15.82f, -109.04f), new(14.54f, -106.65f), new(14.17f, -106.11f), new(13.35f, -105.11f),
    new(12.86f, -104.7f), new(12.27f, -104.52f), new(11.92f, -104.05f), new(11.82f, -103.44f), new(9.85f, -101.82f),
    new(9.29f, -101.45f), new(7.01f, -100.23f), new(6.35f, -99.95f), new(4.96f, -99.96f), new(4.43f, -99.77f),
    new(4.08f, -99.25f), new(4.14f, -98.65f), new(4.26f, -97.97f), new(4.1f, -72.05f), new(4.22f, -71.41f),
    new(4.61f, -70.04f), new(4.99f, -69.66f), new(7.17f, -68.46f), new(8.67f, -67.23f), new(9.15f, -66.77f),
    new(10.37f, -65.28f), new(10.74f, -64.72f), new(11.7f, -62.92f), new(12.47f, -60.31f), new(12.67f, -58.3f),
    new(12.66f, -57.63f), new(12.47f, -55.67f), new(11.76f, -53.23f), new(10.5f, -50.88f), new(9.17f, -49.26f),
    new(8.68f, -48.78f), new(7.05f, -47.44f), new(4.8f, -46.24f), new(4.57f, -45.73f), new(4.51f, -45.06f),
    new(4.34f, -44.37f), new(3.95f, -43.88f), new(3.94f, -43.26f), new(4.25f, -42.78f), new(4.78f, -42.56f),
    new(5.4f, -42.59f), new(9.45f, -41.98f), new(14.38f, -40.75f), new(15.58f, -40.3f), new(16.12f, -39.95f),
    new(16.65f, -39.77f), new(17.87f, -39.49f), new(18.3f, -39.78f), new(18.9f, -40.01f), new(21.88f, -38.66f),
    new(22.08f, -38.01f), new(22.33f, -37.46f), new(22.78f, -37.16f), new(23.38f, -37.16f), new(23.91f, -37.49f),
    new(24.4f, -37.36f), new(27.41f, -35.54f), new(29.56f, -33.91f), new(30.06f, -33.42f), new(30.04f, -32.89f),
    new(30.2f, -32.26f), new(30.65f, -31.9f), new(31.21f, -31.82f), new(31.88f, -32.08f), new(34.79f, -29.41f),
    new(36.07f, -28f), new(36.48f, -27.49f), new(36.93f, -27f), new(36.81f, -25.78f), new(37.13f, -25.29f),
    new(37.64f, -25.04f), new(38.23f, -25.07f), new(38.81f, -24.72f), new(40.84f, -21.94f), new(42.46f, -19.23f),
    new(35.97f, -12.04f), new(35.92f, -12.58f), new(36.1f, -13.8f), new(35.95f, -14.28f), new(35.55f, -14.68f),
    new(34.94f, -14.76f), new(34.32f, -14.95f), new(33.99f, -15.5f), new(33.7f, -16.11f), new(33.39f, -18.07f),
    new(33.14f, -18.62f), new(31.44f, -20.89f), new(28.74f, -23.88f), new(26.28f, -26.11f), new(25.9f, -26.59f),
    new(25.32f, -26.9f), new(22.49f, -28.97f), new(15.37f, -32.72f), new(11.87f, -33.97f), new(8.17f, -34.9f),
    new(4.23f, -35.52f), new(0.16f, -35.74f), new(-4.27f, -35.51f), new(-8.09f, -34.93f), new(-11.7f, -34.02f),
    new(-15.21f, -32.79f), new(-17.66f, -31.68f), new(-18.29f, -31.44f), new(-18.68f, -31.79f), new(-21.4f, -37.07f),
    new(-21.57f, -37.79f), new(-17.97f, -39.4f), new(-13.64f, -40.94f), new(-9.2f, -42.03f), new(-6.73f, -42.4f),
    new(-5.43f, -42.27f), new(-4.81f, -42.32f), new(-4.28f, -42.62f), new(-3.96f, -43.12f), new(-3.84f, -43.64f),
    new(-4.08f, -44.21f), new(-4.47f, -44.73f), new(-4.67f, -46.11f), new(-5.15f, -46.43f), new(-6.96f, -47.4f),
    new(-8.59f, -48.7f), new(-9.08f, -49.17f), new(-9.51f, -49.7f), new(-9.77f, -50.32f), new(-9.57f, -50.95f),
    new(-9.63f, -51.54f), new(-10f, -52.08f), new(-10.5f, -52.38f), new(-11.05f, -52.45f), new(-11.57f, -52.84f),
    new(-11.83f, -53.48f), new(-12.42f, -55.43f), new(-12.51f, -56.09f), new(-12.35f, -56.75f), new(-11.87f, -57.15f),
    new(-11.63f, -57.69f), new(-11.63f, -58.31f), new(-11.88f, -58.85f), new(-12.33f, -59.24f), new(-12.51f, -59.89f),
    new(-12.43f, -60.57f), new(-11.85f, -62.46f), new(-11.6f, -63.04f), new(-11.22f, -63.62f), new(-10.52f, -63.68f),
    new(-10f, -63.96f), new(-9.62f, -64.41f), new(-9.49f, -65.05f), new(-9.62f, -65.62f), new(-9.54f, -66.29f),
    new(-9.1f, -66.82f), new(-8.61f, -67.27f), new(-7.08f, -68.53f), new(-4.88f, -69.71f), new(-4.56f, -70.15f),
    new(-4.22f, -71.36f), new(-4.1f, -71.95f), new(-4.23f, -98.55f), new(-4.1f, -99.1f), new(-4.3f, -99.68f),
    new(-4.84f, -99.93f), new(-5.42f, -99.84f), new(-6.07f, -99.86f), new(-6.67f, -100.03f), new(-9.6f, -101.63f),
    new(-11.51f, -103.2f), new(-11.77f, -103.83f), new(-12.1f, -104.32f), new(-12.63f, -104.56f), new(-13.18f, -104.9f),
    new(-14.4f, -106.39f), new(-15.93f, -109.25f), new(-16.12f, -109.92f), new(-16.14f, -110.59f), new(-16.05f, -111.14f),
    new(-16.25f, -111.68f), new(-16.79f, -111.93f), new(-37.9f, -111.91f), new(-38.16f, -120.04f), new(-16.64f, -120.12f),
    new(-16.2f, -120.44f), new(-16f, -121.03f), new(-16.22f, -121.65f), new(-16.1f, -122.14f), new(-15.89f, -122.83f),
    new(-14.59f, -125.26f), new(-14.19f, -125.88f), new(-13.3f, -126.96f), new(-12.88f, -127.4f), new(-11.8f, -128.1f),
    new(-11.53f, -128.61f), new(-11.15f, -129.13f), new(-9.69f, -130.33f), new(-6.89f, -131.86f), new(-6.25f, -132.11f),
    new(-5.57f, -132.29f), new(-4.92f, -132.17f), new(-4.42f, -132.35f), new(-3.94f, -132.8f), new(-3.26f, -132.98f),
    new(0.07f, -133.29f)];
    private static readonly WPos[] vertices2 = [new(-73.9f, -189.73f), new(-71.37f, -189.48f), new(-68.87f, -188.75f), new(-68.49f, -188.21f), new(-67.95f, -187.88f),
    new(-67.33f, -187.69f), new(-62f, -182.36f), new(-61.65f, -181.79f), new(-61.49f, -181.23f), new(-61.07f, -180.91f),
    new(-60.68f, -180.38f), new(-60.08f, -178.39f), new(-59.79f, -175.72f), new(-60.1f, -172.53f), new(-60.54f, -171.22f),
    new(-61.05f, -170.78f), new(-61.25f, -170.22f), new(-61.08f, -169.6f), new(-62.02f, -167.85f), new(-64.13f, -165.28f),
    new(-66.05f, -163.7f), new(-67.8f, -162.76f), new(-68.49f, -162.72f), new(-69.55f, -162.05f), new(-71.42f, -161.52f),
    new(-73.95f, -161.27f), new(-76.57f, -161.54f), new(-78.42f, -162.06f), new(-78.87f, -162.53f), new(-79.43f, -162.75f),
    new(-80.03f, -162.67f), new(-80.49f, -162.23f), new(-80.6f, -161.65f), new(-80.3f, -161.12f), new(-79.81f, -160.86f),
    new(-79.33f, -160.7f), new(-78.19f, -157f), new(-77.88f, -153.79f), new(-78.19f, -150.45f), new(-79.57f, -145.37f),
    new(-79.99f, -144.06f), new(-79.79f, -143.53f), new(-79.3f, -143.19f), new(-74.33f, -138.13f), new(-81.18f, -133.55f),
    new(-81.53f, -133.92f), new(-82.05f, -134.27f), new(-84.79f, -136.97f), new(-85.23f, -137.46f), new(-85.69f, -137.85f),
    new(-86.25f, -137.93f), new(-86.9f, -137.68f), new(-92.46f, -136.18f), new(-95.79f, -135.88f), new(-100.93f, -136.64f),
    new(-105.59f, -138.86f), new(-108.15f, -140.94f), new(-110.69f, -144.07f), new(-112.19f, -146.88f), new(-113.14f, -150.01f),
    new(-113.51f, -153.33f), new(-113.51f, -154.02f), new(-113.12f, -157.43f), new(-112.36f, -159.93f), new(-111.97f, -160.48f),
    new(-110.78f, -160.99f), new(-109.85f, -161.95f), new(-109.46f, -162.44f), new(-107.83f, -165.18f), new(-106.45f, -166.58f),
    new(-102.74f, -169.16f), new(-102.12f, -169.4f), new(-101.48f, -169.52f), new(-100.84f, -169.57f), new(-99.56f, -169.51f),
    new(-97.51f, -170.91f), new(-97.09f, -171.4f), new(-95.88f, -171.52f), new(-92.07f, -171.16f), new(-89.58f, -170.4f),
    new(-89f, -170.02f), new(-88.6f, -169.46f), new(-88.14f, -168.96f), new(-87.61f, -168.68f), new(-87.03f, -168.79f),
    new(-86.56f, -169.26f), new(-86.59f, -169.9f), new(-86.94f, -170.35f), new(-87.39f, -170.8f), new(-87.96f, -172.71f),
    new(-88.21f, -175.25f), new(-87.9f, -178.52f), new(-87.53f, -179.71f), new(-87.06f, -180.21f), new(-86.7f, -180.68f),
    new(-86.59f, -181.95f), new(-85.96f, -183.13f), new(-85.6f, -183.67f), new(-83.93f, -185.66f), new(-81.95f, -187.28f),
    new(-80.28f, -188.19f), new(-79.06f, -188.43f), new(-78.61f, -188.88f), new(-76.67f, -189.47f), new(-74.11f, -189.73f)];
    private static readonly ArenaBoundsCustom arena1 = new([new PolygonCustom(vertices1)], [new Polygon(new(default, -116f), 6.778f, 32)]);
    private static readonly ArenaBoundsCustom arena2 = new([new PolygonCustom(vertices2)]);
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.TroianEquerry, (uint)OID.TroianTrapper, (uint)OID.TroianSteward];

    protected override bool CheckPull() => IsAnyActorInCombat(Trash);

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
