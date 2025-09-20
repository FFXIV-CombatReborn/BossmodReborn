namespace BossMod.Heavensward.Dungeon.D13SohrKhai.D130CloudGardener;

public enum OID : uint
{
    Boss = 0x1604, // R1.2
    EnchantedFan = 0x1605, // R3.0
    GardenCloudtrap = 0x1607, // R0.8
    SanctuarySkipper = 0x1609, // R0.9
    GardenSankchinni = 0x1608, // R0.75
    GardenMelia = 0x1606, // R3.0
}

public enum AID : uint
{
    AutoAttack = 872, // EnchantedFan/SanctuarySkipper/GardenSankchinni/GardenCloudtrap/GardenMelia/Boss->player, no cast, single-target

    Seedvolley = 344, // GardenCloudtrap->player, no cast, single-target
    RiseAndFall = 4700, // GardenMelia->self, 3.0s cast, range 6+R 270-degree cone
    AdventitiousLash = 4502, // GardenSankchinni->player, no cast, single-target
    TightTornado = 6237, // EnchantedFan->self, 3.0s cast, range 15+R width 4 rect
    Venom = 1911, // SanctuarySkipper->player, no cast, range 10+R 120-degree cone
    CursedSphere = 1912, // SanctuarySkipper->players, no cast, range 3 circle, targets seemingly random players
    DarkBlizzardIII = 6236, // Boss->location, 3.0s cast, range 5 circle
}

class RiseAndFall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RiseAndFall, new AOEShapeCone(9f, 135f.Degrees()));
class TightTornado(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TightTornado, new AOEShapeRect(18f, 2f));
class Venom(BossModule module) : Components.Cleave(module, (uint)AID.Venom, new AOEShapeCone(10.9f, 60f.Degrees()), [(uint)OID.SanctuarySkipper]);
class DarkBlizzardIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkBlizzardIII, 5f);

class D130CloudGardenerStates : StateMachineBuilder
{
    public D130CloudGardenerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RiseAndFall>()
            .ActivateOnEnter<TightTornado>()
            .ActivateOnEnter<DarkBlizzardIII>()
            .ActivateOnEnter<Venom>()
            .Raw.Update = () => AllDeadOrDestroyedInBounds(D130CloudGardener.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 171, NameID = 4928, SortOrder = 1)]
public class D130CloudGardener(WorldState ws, Actor primary) : BossModule(ws, primary, IsArena1(primary) ? arena1.Center : arena2.Center, IsArena1(primary) ? arena1 : arena2)
{
    private static bool IsArena1(Actor primary) => primary.PosRot.Z > 80f;
    private static readonly WPos[] vertices1 = [new(-397.21f, 80.53f), new(-396.7f, 80.92f), new(-395.75f, 81.87f), new(-395.65f, 83.98f), new(-395.55f, 84.54f),
    new(-393.56f, 84.66f), new(-392.7f, 84.66f), new(-392.24f, 84.41f), new(-392.2f, 82.13f), new(-384.32f, 81.86f),
    new(-383.38f, 82.76f), new(-383.32f, 83.44f), new(-383.42f, 84.07f), new(-383.54f, 84.69f), new(-383.68f, 85.35f),
    new(-383.84f, 86.01f), new(-384.03f, 86.68f), new(-384.23f, 87.31f), new(-384.44f, 87.96f), new(-384.67f, 88.59f),
    new(-385.15f, 89.73f), new(-385.42f, 90.3f), new(-385.7f, 90.86f), new(-386.01f, 91.45f), new(-386.33f, 92.01f),
    new(-386.67f, 92.58f), new(-387.04f, 93.16f), new(-387.39f, 93.66f), new(-387.75f, 94.16f), new(-388.15f, 94.69f),
    new(-388.57f, 95.21f), new(-389.02f, 95.73f), new(-389.47f, 96.23f), new(-389.9f, 96.68f), new(-390.36f, 97.13f),
    new(-390.83f, 97.57f), new(-391.29f, 97.99f), new(-391.8f, 98.41f), new(-392.31f, 98.82f), new(-392.54f, 98.26f),
    new(-392.7f, 97.68f), new(-393.07f, 97.25f), new(-393.61f, 96.99f), new(-394.2f, 96.97f), new(-394.73f, 97.27f),
    new(-395.1f, 97.73f), new(-395.2f, 98.32f), new(-395.05f, 98.87f), new(-394.65f, 99.36f), new(-395.03f, 99.72f),
    new(-405.02f, 99.84f), new(-405.23f, 99.29f), new(-404.89f, 98.76f), new(-404.81f, 98.18f), new(-404.97f, 97.64f),
    new(-405.39f, 97.2f), new(-405.93f, 96.98f), new(-406.5f, 97.02f), new(-407.03f, 97.27f), new(-407.37f, 97.78f),
    new(-407.46f, 98.35f), new(-407.3f, 98.93f), new(-407.82f, 98.71f), new(-408.34f, 98.29f), new(-408.86f, 97.84f),
    new(-409.37f, 97.38f), new(-409.86f, 96.91f), new(-410.33f, 96.44f), new(-410.79f, 95.94f), new(-411.25f, 95.42f),
    new(-411.69f, 94.88f), new(-412.44f, 93.86f), new(-412.8f, 93.34f), new(-413.15f, 92.82f), new(-413.49f, 92.28f),
    new(-414.1f, 91.2f), new(-414.41f, 90.61f), new(-414.7f, 90.01f), new(-414.98f, 89.38f), new(-415.24f, 88.76f),
    new(-415.46f, 88.15f), new(-415.69f, 87.51f), new(-415.9f, 86.86f), new(-416.23f, 85.64f), new(-416.37f, 85.04f),
    new(-416.49f, 84.43f), new(-416.61f, 83.79f), new(-416.7f, 83.15f), new(-416.38f, 82.56f), new(-416.01f, 82.12f),
    new(-408.07f, 81.86f), new(-407.8f, 84.12f), new(-407.6f, 84.59f), new(-406.62f, 84.65f), new(-405.06f, 84.66f),
    new(-404.5f, 84.57f), new(-404.33f, 82.1f), new(-403.93f, 81.53f), new(-403.45f, 81.06f), new(-403.01f, 80.53f),
    new(-397.21f, 80.53f)];
    private static readonly WPos[] vertices2 = [new(-397.01f, -79.49f), new(-396.61f, -78.99f), new(-395.72f, -78.1f), new(-395.65f, -76.2f), new(-395.6f, -75.54f),
    new(-392.75f, -75.34f), new(-392.26f, -75.54f), new(-392.19f, -77.83f), new(-384.16f, -78.14f), new(-383.75f, -77.59f),
    new(-383.24f, -77.15f), new(-383.41f, -76), new(-383.53f, -75.36f), new(-383.68f, -74.68f), new(-383.84f, -74.01f),
    new(-384.02f, -73.35f), new(-384.21f, -72.74f), new(-384.43f, -72.08f), new(-384.66f, -71.44f), new(-384.92f, -70.82f),
    new(-385.19f, -70.19f), new(-385.49f, -69.55f), new(-385.79f, -68.97f), new(-386.09f, -68.4f), new(-386.41f, -67.84f),
    new(-387.11f, -66.74f), new(-387.48f, -66.2f), new(-387.89f, -65.66f), new(-388.75f, -64.61f), new(-389.21f, -64.09f),
    new(-389.68f, -63.58f), new(-390.16f, -63.08f), new(-390.65f, -62.61f), new(-391.15f, -62.16f), new(-391.67f, -61.72f),
    new(-392.21f, -61.28f), new(-392.8f, -60.96f), new(-393.22f, -60.57f), new(-393.42f, -54.31f), new(-393.14f, -53.82f),
    new(-392.9f, -35.36f), new(-392.64f, -34.9f), new(-390.73f, -34.44f), new(-390.17f, -34.14f), new(-392.21f, -32.74f),
    new(-392.67f, -32.36f), new(-393.06f, -31.13f), new(-393.26f, -30.65f), new(-393.76f, -30.15f), new(-394.12f, -29.65f),
    new(-393.97f, -29.07f), new(-393.73f, -28.48f), new(-393.39f, -27.86f), new(-393.07f, -27.35f), new(-393.19f, -26.68f),
    new(-393.6f, -26.14f), new(-393.92f, -25.59f), new(-394.01f, -24.98f), new(-393.92f, -24.41f), new(-393.23f, -23.77f),
    new(-393.77f, -23.69f), new(-394.37f, -23.43f), new(-394.91f, -23.09f), new(-395.26f, -22.56f), new(-395.31f, -21.96f),
    new(-395.07f, -21.37f), new(-394.89f, -12.92f), new(-394.92f, -11.62f), new(-395.22f, -11), new(-395.3f, -10.4f),
    new(-395.12f, -9.85f), new(-394.67f, -9.39f), new(-394.1f, -9.2f), new(-393.44f, -9.07f), new(-392.95f, -8.69f),
    new(-392.47f, -8.27f), new(-392, -7.82f), new(-391.54f, -7.32f), new(-391.16f, -6.68f), new(-390.9f, -5.99f),
    new(-390.6f, -5.33f), new(-390.14f, -4.87f), new(-389.57f, -4.68f), new(-388.96f, -4.81f), new(-388.36f, -5.11f),
    new(-384.79f, -5.08f), new(-384.17f, -4.95f), new(-383.55f, -4.7f), new(-383.25f, -4.03f), new(-383.1f, -3.3f),
    new(-383.01f, -2.72f), new(-383.22f, -2.06f), new(-383.55f, -1.44f), new(-383.9f, -0.89f), new(-384.17f, -0.29f),
    new(-384.66f, -0.13f), new(-385.37f, -0.12f), new(-386.08f, -0.1f), new(-386.65f, 0.08f), new(-386.96f, 0.66f),
    new(-387.35f, 1.21f), new(-387.58f, 1.79f), new(-387.77f, 2.44f), new(-387.83f, 3.05f), new(-387.36f, 3.36f),
    new(-386.19f, 3.89f), new(-385.5f, 4.17f), new(-385.88f, 4.61f), new(-386.48f, 4.88f), new(-388.03f, 5.11f),
    new(-388.52f, 4.97f), new(-389.16f, 4.76f), new(-389.76f, 4.74f), new(-390.3f, 4.98f), new(-390.69f, 5.46f),
    new(-390.83f, 6.03f), new(-391, 6.67f), new(-391.41f, 7.19f), new(-391.91f, 7.73f), new(-392.43f, 8.24f),
    new(-392.96f, 8.7f), new(-393.52f, 9.06f), new(-394.22f, 9.21f), new(-394.82f, 9.49f), new(-395.21f, 9.95f),
    new(-395.33f, 10.59f), new(-395.09f, 11.18f), new(-394.91f, 15.18f), new(-395.04f, 15.82f), new(-395.29f, 16.43f),
    new(-395.88f, 16.71f), new(-396.48f, 16.89f), new(-397.14f, 16.89f), new(-397.81f, 16.86f), new(-398.44f, 16.54f),
    new(-399.05f, 16.15f), new(-399.63f, 15.85f), new(-400.23f, 15.44f), new(-400.12f, 14.95f), new(-400.05f, 14.27f),
    new(-399.93f, 13.6f), new(-400.33f, 13.23f), new(-400.9f, 12.87f), new(-401.46f, 12.47f), new(-402.04f, 12.09f),
    new(-402.73f, 11.94f), new(-403.2f, 12.3f), new(-403.77f, 13.54f), new(-404.01f, 14.16f), new(-404.61f, 14.1f),
    new(-404.9f, 13.47f), new(-405.11f, 12.37f), new(-404.88f, 11.68f), new(-404.7f, 10.99f), new(-404.73f, 10.27f),
    new(-404.97f, 9.7f), new(-405.45f, 9.3f), new(-406.06f, 9.19f), new(-406.71f, 8.94f), new(-407.27f, 8.5f),
    new(-407.77f, 8.04f), new(-408.23f, 7.56f), new(-408.69f, 7.03f), new(-409.09f, 6.46f), new(-409.23f, 5.74f),
    new(-409.51f, 5.15f), new(-410.01f, 4.78f), new(-410.64f, 4.72f), new(-411.21f, 4.88f), new(-414.94f, 5.11f),
    new(-421.03f, 5.08f), new(-421.65f, 4.74f), new(-422.31f, 4.71f), new(-422.92f, 4.96f), new(-423.35f, 5.44f),
    new(-423.49f, 6.04f), new(-423.35f, 6.61f), new(-422.95f, 7.04f), new(-422.57f, 7.58f), new(-422.37f, 8.18f),
    new(-422.13f, 8.78f), new(-421.88f, 9.39f), new(-421.61f, 10.01f), new(-421.31f, 10.61f), new(-421, 11.22f),
    new(-420.67f, 11.83f), new(-420.31f, 12.42f), new(-419.94f, 13.01f), new(-419.54f, 13.6f), new(-418.73f, 14.72f),
    new(-418.29f, 15.26f), new(-417.84f, 15.78f), new(-417.39f, 16.28f), new(-416.91f, 16.78f), new(-416.42f, 17.25f),
    new(-415.92f, 17.71f), new(-415.4f, 18.17f), new(-414.88f, 18.59f), new(-414.35f, 19.01f), new(-413.81f, 19.4f),
    new(-413.27f, 19.77f), new(-412.71f, 20.14f), new(-412.15f, 20.48f), new(-411.59f, 20.8f), new(-411.02f, 21.11f),
    new(-409.88f, 21.68f), new(-409.31f, 21.93f), new(-408.16f, 22.39f), new(-407.46f, 22.63f), new(-407.03f, 22.94f),
    new(-406.52f, 23.35f), new(-405.91f, 23.44f), new(-405.32f, 23.33f), new(-404.89f, 22.81f), new(-404.68f, 22.21f),
    new(-404.73f, 21.65f), new(-405.08f, 21.08f), new(-405.08f, 19.47f), new(-404.54f, 19.46f), new(-403.98f, 19.24f),
    new(-403.3f, 19.2f), new(-402.76f, 19.52f), new(-402.22f, 19.87f), new(-401.62f, 20.15f), new(-401.03f, 20.45f),
    new(-400.47f, 20.72f), new(-399.83f, 20.88f), new(-399.16f, 20.92f), new(-398.37f, 20.89f), new(-398.09f, 21.3f),
    new(-397.83f, 21.92f), new(-397.29f, 22.11f), new(-396.67f, 22.02f), new(-396.13f, 21.66f), new(-395.19f, 21.37f),
    new(-395.34f, 21.99f), new(-395.22f, 22.65f), new(-394.87f, 23.12f), new(-394.36f, 23.4f), new(-393.98f, 24.02f),
    new(-391.4f, 33.67f), new(-391.28f, 34.3f), new(-391.74f, 34.74f), new(-392.37f, 34.78f), new(-392.86f, 35.15f),
    new(-392.93f, 53.56f), new(-393.26f, 53.96f), new(-393.41f, 59.84f), new(-393.63f, 60.32f), new(-394.34f, 60.41f),
    new(-395.02f, 60.48f), new(-397.11f, 60.58f), new(-399.24f, 60.59f), new(-402.13f, 60.48f), new(-402.85f, 60.47f),
    new(-403.56f, 60.5f), new(-404.99f, 60.58f), new(-405.7f, 60.59f), new(-406.42f, 60.56f), new(-406.59f, 60.09f),
    new(-406.59f, 54.73f), new(-406.6f, 54.09f), new(-407.05f, 53.65f), new(-407.1f, 35.39f), new(-407.35f, 34.95f),
    new(-409.2f, 34.51f), new(-410.57f, 34.11f), new(-411.95f, 33.66f), new(-413.73f, 32.97f), new(-415.49f, 32.18f),
    new(-416.74f, 31.55f), new(-417.39f, 31.19f), new(-418, 30.85f), new(-419.07f, 30.2f), new(-419.59f, 29.86f),
    new(-420.62f, 29.16f), new(-422.17f, 28), new(-423.71f, 26.71f), new(-424.21f, 26.25f), new(-425.18f, 25.33f),
    new(-425.63f, 24.87f), new(-426.49f, 23.95f), new(-427.71f, 22.54f), new(-428.49f, 21.54f), new(-428.9f, 20.98f),
    new(-429.74f, 19.77f), new(-430.15f, 19.14f), new(-430.54f, 18.51f), new(-430.87f, 17.95f), new(-431.51f, 16.81f),
    new(-431.81f, 16.23f), new(-432.14f, 15.56f), new(-432.44f, 14.93f), new(-432.69f, 14.37f), new(-433.17f, 13.24f),
    new(-433.82f, 11.47f), new(-434.4f, 9.61f), new(-434.57f, 8.97f), new(-434.88f, 7.67f), new(-435.02f, 7.01f),
    new(-435.26f, 5.65f), new(-435.36f, 4.98f), new(-435.45f, 4.32f), new(-435.58f, 3), new(-435.69f, 1.04f),
    new(-435.7f, -0.89f), new(-435.64f, -2.16f), new(-435.6f, -2.79f), new(-435.47f, -4.1f), new(-435.39f, -4.76f),
    new(-435.19f, -6.05f), new(-435.08f, -6.67f), new(-434.82f, -7.92f), new(-434.67f, -8.55f), new(-434.33f, -9.81f),
    new(-434.13f, -10.51f), new(-433.82f, -11.32f), new(-433.63f, -10.84f), new(-432.95f, -9.7f), new(-432.58f, -9.17f),
    new(-432.16f, -8.6f), new(-431.77f, -8.07f), new(-431.35f, -7.56f), new(-430.76f, -7.26f), new(-430.21f, -6.86f),
    new(-429.72f, -6.44f), new(-429.14f, -6.27f), new(-428.47f, -6.41f), new(-427.9f, -6.1f), new(-427.33f, -6.07f),
    new(-426.79f, -6.32f), new(-426.2f, -6.66f), new(-425.53f, -6.66f), new(-424.93f, -6.78f), new(-424.3f, -7.03f),
    new(-423.87f, -6.51f), new(-423.7f, -5.86f), new(-423.41f, -5.28f), new(-422.91f, -4.88f), new(-422.27f, -4.69f),
    new(-421.66f, -4.75f), new(-421.13f, -5.11f), new(-411.75f, -5.1f), new(-411.11f, -4.84f), new(-410.47f, -4.7f),
    new(-409.87f, -4.87f), new(-409.41f, -5.3f), new(-409.21f, -5.88f), new(-409.09f, -6.53f), new(-408.67f, -7.08f),
    new(-408.22f, -7.59f), new(-407.75f, -8.07f), new(-407.29f, -8.5f), new(-406.8f, -8.88f), new(-406.14f, -9.17f),
    new(-405.48f, -9.29f), new(-405.01f, -9.67f), new(-404.75f, -10.23f), new(-404.75f, -10.85f), new(-405.05f, -11.42f),
    new(-405.11f, -12.84f), new(-405.11f, -18.46f), new(-405.08f, -20.08f), new(-404.82f, -20.73f), new(-404.66f, -21.4f),
    new(-404.66f, -22.09f), new(-404.83f, -22.73f), new(-405.21f, -23.18f), new(-405.76f, -23.41f), new(-406.43f, -23.37f),
    new(-406.95f, -23.06f), new(-407.49f, -22.64f), new(-408.1f, -22.43f), new(-408.43f, -23.02f), new(-408.44f, -23.67f),
    new(-408.68f, -24.31f), new(-412.16f, -32.69f), new(-412.37f, -33.31f), new(-411.97f, -33.65f), new(-410.03f, -34.28f),
    new(-409.36f, -34.47f), new(-407.31f, -34.96f), new(-407.1f, -53.41f), new(-406.7f, -54.01f), new(-406.58f, -60.02f),
    new(-406.92f, -60.6f), new(-407.46f, -61.01f), new(-407.96f, -61.39f), new(-408.47f, -61.81f), new(-409, -62.27f),
    new(-409.47f, -62.7f), new(-409.91f, -63.12f), new(-410.35f, -63.58f), new(-410.79f, -64.05f), new(-411.22f, -64.54f),
    new(-411.65f, -65.07f), new(-412.06f, -65.58f), new(-412.44f, -66.1f), new(-413.15f, -67.14f), new(-413.51f, -67.71f),
    new(-413.85f, -68.3f), new(-414.19f, -68.92f), new(-414.51f, -69.55f), new(-414.82f, -70.21f), new(-415.11f, -70.87f),
    new(-415.36f, -71.5f), new(-415.58f, -72.12f), new(-415.79f, -72.73f), new(-415.99f, -73.38f), new(-416.17f, -74.03f),
    new(-416.32f, -74.66f), new(-416.47f, -75.35f), new(-416.55f, -76), new(-416.65f, -76.65f), new(-416.54f, -77.29f),
    new(-416.1f, -77.74f), new(-407.93f, -78.13f), new(-407.75f, -75.56f), new(-406, -75.33f), new(-405.44f, -75.17f),
    new(-404.84f, -75.31f), new(-404.37f, -75.59f), new(-404.33f, -78.08f), new(-402.92f, -79.49f), new(-397.01f, -79.49f)];

    private static readonly ArenaBoundsCustom arena1 = new([new PolygonCustom(vertices1)]);
    private static readonly ArenaBoundsCustom arena2 = new([new PolygonCustom(vertices2)]);
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.EnchantedFan, (uint)OID.GardenCloudtrap, (uint)OID.SanctuarySkipper, (uint)OID.GardenSankchinni, (uint)OID.GardenMelia];

    protected override bool CheckPull() => Arena.InBounds(Raid.Player()!.Position);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.ActorsInBounds(this, Trash);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
            hints.PotentialTargets[i].Priority = 0;
    }
}
