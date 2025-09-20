namespace BossMod.Endwalker.Dungeon.D12Aetherfont.D120HaamForlorn;

public enum OID : uint
{
    Boss = 0x3ED1, // R2.4
    HaamPolycrystal = 0x3ED0, // R1.0
    HaamMyrrlith = 0x3ECF, // R3.75
    HaamDhruva = 0x3ED2 // R1.8
}

public enum AID : uint
{
    AutoAttack = 872, // HaamMyrrlith/HaamPolycrystal->player, no cast, single-target

    Scathe = 972, // Boss/HaamDhruva->player, no cast, single-target
    AetherialSpark = 33996, // Boss->self, 3.0s cast, range 12 width 4 rect
}

class AetherialSpark(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AetherialSpark, new AOEShapeRect(12f, 2f));

class D120HaamForlornStates : StateMachineBuilder
{
    public D120HaamForlornStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AetherialSpark>()
            .Raw.Update = () => AllDeadOrDestroyedInBounds(D120HaamForlorn.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 822, NameID = 12349, SortOrder = 7)]
public class D120HaamForlorn(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(-368.13f, -144.43f), new(-367.6f, -144.38f), new(-367.07f, -144.24f), new(-366.64f, -143.99f), new(-366.25f, -143.62f),
    new(-365.58f, -142.81f), new(-365.28f, -142.38f), new(-364.31f, -140.03f), new(-363.31f, -138.84f), new(-362.29f, -138.61f),
    new(-361.35f, -136.82f), new(-361.05f, -136.42f), new(-360.64f, -136.05f), new(-360.11f, -135.99f), new(-359.66f, -136.24f),
    new(-359.27f, -136.56f), new(-358.95f, -136.97f), new(-359, -137.47f), new(-359.19f, -137.98f), new(-360.53f, -139.58f),
    new(-360.02f, -139.72f), new(-355.43f, -139.43f), new(-354.96f, -139.67f), new(-354.54f, -139.94f), new(-354.07f, -140.16f),
    new(-353.56f, -140), new(-353.06f, -139.93f), new(-351.01f, -140.37f), new(-350.47f, -140.37f), new(-346.24f, -137.18f),
    new(-346.05f, -136.7f), new(-346.81f, -133.68f), new(-346.7f, -133.15f), new(-345.47f, -130.76f), new(-345.15f, -130.33f),
    new(-345.35f, -129.85f), new(-345.65f, -129.43f), new(-346, -129.06f), new(-346.27f, -128.6f), new(-346.23f, -127.08f),
    new(-346.07f, -126.57f), new(-344.68f, -125.06f), new(-344.43f, -124.58f), new(-344.11f, -124.15f), new(-343.63f, -123.86f),
    new(-342.88f, -123.13f), new(-342.53f, -122.72f), new(-341.91f, -120.24f), new(-341.74f, -119.75f), new(-340.97f, -119.04f),
    new(-340.67f, -117.99f), new(-340.13f, -117.08f), new(-339.74f, -116.75f), new(-339.46f, -116.34f), new(-338.94f, -116.15f),
    new(-337.95f, -115.95f), new(-337.59f, -115.59f), new(-335.9f, -112.48f), new(-335.49f, -112.18f), new(-333.62f, -111.18f),
    new(-333.27f, -110.77f), new(-333.01f, -110.29f), new(-332.53f, -110.1f), new(-329.89f, -109.31f), new(-329.45f, -109),
    new(-329.24f, -108.54f), new(-328.89f, -107.55f), new(-328.53f, -107.14f), new(-326.62f, -106.21f), new(-326.29f, -105.8f),
    new(-326.17f, -105.32f), new(-326.38f, -104.79f), new(-326.87f, -103.88f), new(-326.99f, -103.36f), new(-326.79f, -102.28f),
    new(-326.48f, -101.88f), new(-326.37f, -101.37f), new(-325.65f, -100.57f), new(-325.54f, -100.04f), new(-325.23f, -96.77f),
    new(-325.58f, -96.37f), new(-325.85f, -95.89f), new(-325.62f, -95.44f), new(-325.34f, -94.99f), new(-325.19f, -94.48f),
    new(-324.58f, -93), new(-324.24f, -92.61f), new(-323.25f, -92.2f), new(-322.83f, -91.88f), new(-322.21f, -90.4f),
    new(-321.93f, -89.97f), new(-318.42f, -88.72f), new(-317.95f, -88.44f), new(-317.56f, -88.13f), new(-317.18f, -87.74f),
    new(-316.86f, -87.3f), new(-316.58f, -86.84f), new(-316.38f, -86.37f), new(-316.25f, -84.82f), new(-316.73f, -84.64f),
    new(-317.14f, -84.34f), new(-318.13f, -83.13f), new(-320.03f, -81.39f), new(-320.38f, -81.02f), new(-322.42f, -76.25f),
    new(-322.95f, -76.27f), new(-324.33f, -76.97f), new(-326.88f, -77.54f), new(-328.46f, -77.39f), new(-328.99f, -77.49f),
    new(-329.49f, -77.53f), new(-330.43f, -77.13f), new(-330.85f, -76.8f), new(-331.81f, -76.37f), new(-332.17f, -75.97f),
    new(-332.44f, -75.54f), new(-332.83f, -75.22f), new(-333.6f, -73.28f), new(-335.37f, -71.3f), new(-336.97f, -69.99f),
    new(-337.93f, -70.39f), new(-339.47f, -70.44f), new(-339.86f, -70.81f), new(-340.54f, -72.79f), new(-340.86f, -73.19f),
    new(-341.24f, -73.56f), new(-341.7f, -73.79f), new(-342.11f, -73.47f), new(-343.56f, -72.05f), new(-344.06f, -71.88f),
    new(-344.6f, -71.87f), new(-345.07f, -71.67f), new(-345.81f, -70.94f), new(-346.08f, -70.5f), new(-346.15f, -69.43f),
    new(-347.96f, -67.64f), new(-348.45f, -67.48f), new(-352.91f, -71.28f), new(-353.24f, -71.71f), new(-353.54f, -72.16f),
    new(-353.76f, -72.67f), new(-353.85f, -73.2f), new(-353.8f, -73.72f), new(-353.6f, -74.19f), new(-352.46f, -75.32f),
    new(-352.03f, -75.58f), new(-350.98f, -75.65f), new(-349.86f, -76.74f), new(-349.79f, -77.25f), new(-349.8f, -77.78f),
    new(-349.55f, -78.24f), new(-348.42f, -79.37f), new(-347.99f, -79.71f), new(-347.95f, -80.26f), new(-348.72f, -81.06f),
    new(-349.23f, -81.08f), new(-351.09f, -79.23f), new(-353.23f, -81.54f), new(-353.01f, -82.02f), new(-351.16f, -84.55f),
    new(-350.95f, -85.01f), new(-350.12f, -89.11f), new(-349.91f, -89.6f), new(-346.42f, -91.84f), new(-344.52f, -92.76f),
    new(-344.08f, -93.04f), new(-343.09f, -94.2f), new(-343, -94.7f), new(-343.19f, -95.2f), new(-343.09f, -95.73f),
    new(-342.89f, -96.25f), new(-342.79f, -96.78f), new(-342.78f, -97.32f), new(-342.88f, -98.42f), new(-343.22f, -99.97f),
    new(-343.56f, -100.35f), new(-344.7f, -101.4f), new(-344.88f, -101.87f), new(-344.68f, -102.38f), new(-344.77f, -102.89f),
    new(-345.01f, -103.38f), new(-345.16f, -103.89f), new(-345.07f, -104.93f), new(-345.06f, -105.47f), new(-345.11f, -106.02f),
    new(-345.49f, -106.37f), new(-345.99f, -106.52f), new(-347.02f, -106.6f), new(-347.53f, -106.59f), new(-349.42f, -104.1f),
    new(-349.77f, -103.72f), new(-350.21f, -103.36f), new(-351.19f, -103.7f), new(-351.69f, -103.82f), new(-352.16f, -104.05f),
    new(-356.82f, -107.36f), new(-357.26f, -107.6f), new(-357.76f, -107.73f), new(-358.27f, -107.78f), new(-358.64f, -107.44f),
    new(-359.14f, -107.52f), new(-359.28f, -108.03f), new(-359.54f, -108.48f), new(-359.92f, -108.87f), new(-360.37f, -109.11f),
    new(-362.28f, -109.87f), new(-362.79f, -110), new(-364.33f, -110.24f), new(-364.83f, -110.42f), new(-365.8f, -111.62f),
    new(-365.96f, -112.15f), new(-365.86f, -113.21f), new(-365.97f, -113.73f), new(-367.04f, -116.67f), new(-367.08f, -117.17f),
    new(-366.81f, -118.24f), new(-366.4f, -118.57f), new(-366.04f, -118.98f), new(-365.79f, -119.96f), new(-365.58f, -120.48f),
    new(-364.69f, -121.01f), new(-364.34f, -121.38f), new(-364.22f, -121.87f), new(-364.18f, -122.4f), new(-364.22f, -122.93f),
    new(-364.55f, -123.34f), new(-364.99f, -123.65f), new(-365.51f, -123.45f), new(-366.48f, -122.94f), new(-366.91f, -122.65f),
    new(-367.14f, -122.21f), new(-369.27f, -121.9f), new(-369.78f, -121.86f), new(-371.89f, -121.88f), new(-372.47f, -121.82f),
    new(-373.41f, -124.85f), new(-373.54f, -125.88f), new(-373.4f, -126.37f), new(-372.69f, -127.12f), new(-372.17f, -127.27f),
    new(-368.62f, -127.26f), new(-368.25f, -127.61f), new(-367.18f, -128.72f), new(-366.96f, -129.17f), new(-366.99f, -130.7f),
    new(-367.09f, -131.2f), new(-367.41f, -131.59f), new(-367.7f, -132), new(-367.33f, -132.39f), new(-367.12f, -132.89f),
    new(-367.23f, -133.41f), new(-367.49f, -133.89f), new(-367.83f, -134.29f), new(-368.24f, -134.64f), new(-369.64f, -135.52f),
    new(-370.64f, -135.79f), new(-371.05f, -136.11f), new(-374.54f, -140.07f), new(-374.85f, -140.49f), new(-374.78f, -141.02f),
    new(-371.75f, -142.94f), new(-371.33f, -143.24f), new(-369.52f, -144.17f), new(-369.01f, -144.33f), new(-368.49f, -144.42f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.HaamMyrrlith, (uint)OID.HaamDhruva, (uint)OID.HaamPolycrystal];

    protected override bool CheckPull() => IsAnyActorInBoundsInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.ActorsInBounds(this, Trash);
    }
}
