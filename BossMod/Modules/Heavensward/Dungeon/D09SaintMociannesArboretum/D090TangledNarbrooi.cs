namespace BossMod.Heavensward.Dungeon.D09SaintMociannesArboretum.D090TangledNarbrooi;

public enum OID : uint
{
    Boss = 0x142A, // R0.75
    Biloko = 0x142C, // R2.85
    GrizzlyHost = 0x142B, // R1.7
    WitheredMelia = 0x142E, // R1.8
    RoyalOak = 0x142D, // R3.0
    WallController = 0x1E9DFC // R2.0
}

public enum AID : uint
{
    AutoAttack1 = 872, // Biloko/Boss/WitheredMelia/RoyalOak->player, no cast, single-target
    AutoAttack2 = 870, // GrizzlyHost->player, no cast, single-target

    AdventitiousLash = 4502, // Boss->player, no cast, single-target
    TheWoodRemembers = 5353, // Biloko->self, 3.0s cast, range 5+R 120-degree cone
    MindOfItsOwn = 5350, // GrizzlyHost->self, 3.0s cast, range 6+R 120-degree cone
    Champ = 5351, // GrizzlyHost->player, no cast, single-target
    Canopy = 4701, // RoyalOak->self, 3.0s cast, range 6+R 120-degree cone
    Paralyze = 1118, // Biloko->player, 4.0s cast, single-target
    Hurl = 5352 // Biloko->location, 3.0s cast, range 6 circle
}

class WallRemoval(BossModule module) : BossComponent(module)
{
    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008 && actor.OID == (uint)OID.WallController)
            SetArena(D090TangledNarbrooi.Arena1B);
    }

    public override void Update()
    {
        var pos = Module.Raid.Player()!.Position;
        if (pos.Z > 12f)
        {
            var pX = pos.X;
            if (Arena.Bounds == D090TangledNarbrooi.Arena1B && pX > -101f)
                SetArena(D090TangledNarbrooi.Arena2);
            else if (Arena.Bounds == D090TangledNarbrooi.Arena2 && pX < -101f)
                SetArena(D090TangledNarbrooi.Arena1B);
        }
    }

    private void SetArena(ArenaBoundsCustom bounds)
    {
        Arena.Bounds = bounds;
        Arena.Center = bounds.Center;
    }
}

class TheWoodRemembers(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheWoodRemembers, new AOEShapeCone(7.85f, 60f.Degrees()));
class MindOfItsOwn(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MindOfItsOwn, new AOEShapeCone(9f, 60f.Degrees()));
class Canopy(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Canopy, new AOEShapeCone(7.7f, 60f.Degrees()));
class Hurl(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hurl, 6f);

class D090TangledNarbrooiStates : StateMachineBuilder
{
    public D090TangledNarbrooiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheWoodRemembers>()
            .ActivateOnEnter<MindOfItsOwn>()
            .ActivateOnEnter<Canopy>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<WallRemoval>()
            .Raw.Update = () => AllDeadOrDestroyed(D090TangledNarbrooi.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 41, NameID = 4647, SortOrder = 5)]
public class D090TangledNarbrooi(WorldState ws, Actor primary) : BossModule(ws, primary, Arena1.Center, Arena1)
{
    private static readonly WPos[] vertices1 = [new(-124.78f, -10.09f), new(-123.51f, -8.38f), new(-123.01f, -8.12f), new(-122.39f, -7.85f), new(-121.80f, -7.73f),
    new(-121.15f, -7.65f), new(-120.56f, -7.70f), new(-119.91f, -7.84f), new(-119.39f, -7.43f), new(-119.41f, -6.74f),
    new(-119.70f, -5.41f), new(-119.53f, -4.87f), new(-113.84f, 3.38f), new(-113.38f, 3.77f), new(-112.80f, 4.05f),
    new(-112.22f, 4.35f), new(-111.07f, 4.91f), new(-109.84f, 7.97f), new(-109.69f, 8.47f), new(-111.32f, 9.39f),
    new(-111.81f, 9.77f), new(-112.18f, 11.60f), new(-112.63f, 12.09f), new(-113.95f, 13.45f), new(-114.57f, 13.75f),
    new(-115.89f, 13.79f), new(-116.31f, 14.25f), new(-116.69f, 14.83f), new(-116.98f, 15.33f), new(-116.75f, 15.84f),
    new(-116.43f, 16.40f), new(-116.36f, 17.09f), new(-117.41f, 19.53f), new(-117.81f, 20.06f), new(-118.46f, 20.33f),
    new(-119.07f, 20.62f), new(-119.33f, 21.11f), new(-119.70f, 21.63f), new(-120.19f, 22.02f), new(-120.55f, 22.59f),
    new(-120.40f, 23.15f), new(-119.93f, 23.63f), new(-120.49f, 23.99f), new(-121.18f, 23.79f), new(-121.84f, 23.58f),
    new(-122.53f, 23.41f), new(-123.22f, 23.51f), new(-123.84f, 23.85f), new(-124.28f, 24.36f), new(-124.34f, 24.93f),
    new(-123.91f, 27.42f), new(-123.82f, 28.08f), new(-123.45f, 28.47f), new(-122.91f, 28.40f), new(-122.76f, 29.02f),
    new(-122.63f, 29.53f), new(-122.61f, 30.23f), new(-122.09f, 29.87f), new(-122.04f, 29.29f), new(-122.08f, 28.68f),
    new(-121.67f, 28.19f), new(-120.38f, 27.98f), new(-119.83f, 28.00f), new(-119.60f, 29.21f), new(-119.48f, 29.80f),
    new(-118.87f, 30.00f), new(-118.20f, 29.85f), new(-117.71f, 30.36f), new(-116.61f, 33.09f), new(-116.55f, 33.76f),
    new(-116.83f, 35.10f), new(-116.76f, 35.67f), new(-116.00f, 36.82f), new(-115.52f, 37.09f), new(-114.31f, 37.28f),
    new(-113.68f, 37.50f), new(-112.72f, 38.43f), new(-112.27f, 38.93f), new(-111.78f, 40.93f), new(-111.37f, 41.30f),
    new(-110.83f, 41.68f), new(-110.27f, 42.01f), new(-109.64f, 41.97f), new(-109.12f, 42.46f), new(-108.62f, 42.81f),
    new(-108.06f, 42.67f), new(-107.47f, 42.37f), new(-107.16f, 41.97f), new(-105.49f, 42.66f), new(-105.16f, 43.05f),
    new(-105.51f, 43.70f), new(-105.83f, 44.14f), new(-106.04f, 45.35f), new(-106.01f, 45.94f), new(-105.58f, 46.32f),
    new(-104.12f, 47.44f), new(-103.60f, 47.91f), new(-103.84f, 48.55f), new(-103.97f, 49.12f), new(-103.61f, 49.57f),
    new(-100.52f, 52.76f), new(-100.02f, 52.93f), new(-98.80f, 52.95f), new(-98.60f, 53.57f), new(-99.92f, 59.42f),
    new(-100.20f, 60.01f), new(-100.57f, 60.50f), new(-100.92f, 61.00f), new(-101.22f, 61.52f), new(-101.43f, 62.08f),
    new(-101.53f, 62.66f), new(-101.53f, 63.26f), new(-101.46f, 63.85f), new(-101.25f, 64.40f), new(-100.84f, 64.88f),
    new(-100.29f, 65.17f), new(-99.70f, 65.25f), new(-99.14f, 65.11f), new(-98.63f, 65.12f), new(-98.54f, 65.67f),
    new(-98.50f, 66.37f), new(-99.03f, 66.69f), new(-100.60f, 66.69f), new(-101.29f, 66.66f), new(-105.93f, 66.15f),
    new(-106.58f, 66.03f), new(-107.60f, 65.25f), new(-108.09f, 65.04f), new(-109.36f, 65.42f), new(-110.02f, 65.56f),
    new(-113.31f, 64.61f), new(-113.96f, 64.39f), new(-116.92f, 63.04f), new(-118.01f, 61.51f), new(-118.39f, 60.96f),
    new(-118.74f, 60.46f), new(-119.18f, 60.01f), new(-120.22f, 59.17f), new(-120.71f, 58.74f), new(-121.25f, 58.59f),
    new(-123.82f, 59.00f), new(-125.13f, 59.17f), new(-125.83f, 59.14f), new(-126.89f, 56.83f), new(-127.23f, 56.30f),
    new(-127.62f, 55.78f), new(-128.10f, 55.38f), new(-129.41f, 54.01f), new(-129.74f, 53.42f), new(-129.49f, 52.14f),
    new(-129.05f, 50.24f), new(-128.74f, 49.75f), new(-128.65f, 48.43f), new(-128.95f, 47.89f), new(-129.30f, 47.34f),
    new(-129.56f, 46.74f), new(-129.97f, 44.91f), new(-130.13f, 44.29f), new(-130.62f, 43.99f), new(-132.95f, 42.87f),
    new(-133.56f, 42.55f), new(-134.62f, 41.67f), new(-135.16f, 41.51f), new(-136.45f, 41.46f), new(-137.02f, 41.58f),
    new(-137.61f, 41.81f), new(-138.26f, 41.80f), new(-139.83f, 40.57f), new(-140.36f, 40.30f), new(-141.01f, 40.12f),
    new(-141.65f, 39.92f), new(-142.28f, 39.70f), new(-143.62f, 34.67f), new(-143.78f, 34.01f), new(-144.29f, 30.17f),
    new(-144.33f, 29.43f), new(-141.86f, 29.62f), new(-141.24f, 29.80f), new(-138.77f, 30.75f), new(-138.17f, 30.88f),
    new(-135.19f, 28.42f), new(-135.00f, 27.87f), new(-134.93f, 27.16f), new(-134.90f, 26.48f), new(-134.97f, 25.80f),
    new(-135.11f, 25.12f), new(-135.46f, 24.73f), new(-139.33f, 24.71f), new(-140.01f, 24.63f), new(-140.60f, 24.34f),
    new(-141.43f, 23.43f), new(-141.74f, 22.85f), new(-141.29f, 19.75f), new(-141.03f, 19.14f), new(-139.92f, 18.52f),
    new(-139.65f, 17.91f), new(-139.60f, 17.36f), new(-140.34f, 16.23f), new(-140.72f, 15.70f), new(-141.28f, 15.53f),
    new(-141.89f, 15.38f), new(-142.47f, 14.99f), new(-141.66f, 12.35f), new(-141.47f, 11.68f), new(-141.25f, 11.04f),
    new(-141.06f, 10.42f), new(-140.54f, 9.93f), new(-139.96f, 9.56f), new(-139.58f, 9.11f), new(-139.96f, 8.63f),
    new(-140.46f, 8.13f), new(-141.22f, 7.90f), new(-141.92f, 7.90f), new(-142.19f, 4.64f), new(-142.19f, 3.93f),
    new(-142.40f, 3.41f), new(-142.51f, -2.41f), new(-142.49f, -3.14f), new(-142.41f, -4.60f), new(-142.19f, -5.39f),
    new(-142.19f, -6.31f),  new(-141.53f, -6.19f), new(-140.88f, -6.29f), new(-139.55f, -6.28f),
    new(-138.95f, -6.33f), new(-136.27f, -8.18f), new(-133.12f, -8.85f), new(-132.44f, -8.97f), new(-128.46f, -8.25f),
    new(-127.95f, -8.38f), new(-125.23f, -10.21f)];
    private static readonly WPos[] vertices2 = [new(-61.37f, -73.30f), new(-58.15f, -71.25f), new(-57.71f, -70.78f), new(-57.01f, -69.74f), new(-56.68f, -69.19f),
    new(-55.64f, -67.64f), new(-55.26f, -67.15f), new(-51.43f, -63.76f), new(-50.97f, -63.33f), new(-50.79f, -62.68f),
    new(-51.53f, -60.99f), new(-51.76f, -60.39f), new(-52.19f, -59.16f), new(-52.02f, -58.61f), new(-51.51f, -57.49f),
    new(-51.43f, -56.81f), new(-51.71f, -55.36f), new(-51.92f, -54.71f), new(-53.26f, -53.36f), new(-53.25f, -52.81f),
    new(-53.16f, -52.15f), new(-53.02f, -51.55f), new(-52.14f, -50.65f), new(-51.79f, -50.05f), new(-51.36f, -48.04f),
    new(-51.33f, -47.39f), new(-51.76f, -46.21f), new(-51.84f, -45.69f), new(-51.42f, -44.49f), new(-51.00f, -44.21f),
    new(-50.35f, -44.08f), new(-49.70f, -43.92f), new(-48.28f, -41.73f), new(-48.04f, -41.10f), new(-47.87f, -39.82f),
    new(-47.61f, -39.32f), new(-47.16f, -38.90f), new(-46.72f, -38.50f), new(-46.15f, -38.56f), new(-45.44f, -38.67f),
    new(-45.68f, -38.14f), new(-51.56f, -29.75f), new(-52.01f, -29.25f), new(-52.69f, -29.24f), new(-53.27f, -29.02f),
    new(-53.78f, -28.67f), new(-54.23f, -28.17f), new(-54.49f, -27.59f), new(-54.57f, -27.00f), new(-54.57f, -26.34f),
    new(-55.11f, -25.90f), new(-60.73f, -22.24f), new(-61.42f, -21.96f), new(-61.67f, -22.47f), new(-61.92f, -23.16f),
    new(-62.16f, -23.74f), new(-62.68f, -24.03f), new(-63.30f, -24.25f), new(-63.89f, -24.56f), new(-64.47f, -24.91f),
    new(-64.90f, -25.45f), new(-67.73f, -29.19f), new(-68.11f, -29.76f), new(-70.71f, -34.52f), new(-70.92f, -35.16f),
    new(-70.86f, -36.51f), new(-70.93f, -37.12f), new(-72.45f, -38.28f), new(-73.01f, -38.35f), new(-74.24f, -38.27f),
    new(-74.89f, -38.39f), new(-76.03f, -39.12f), new(-76.63f, -39.38f), new(-77.91f, -39.64f), new(-78.53f, -39.73f),
    new(-80.63f, -39.92f), new(-81.31f, -39.92f), new(-84.29f, -38.98f), new(-84.88f, -38.71f), new(-87.19f, -37.36f),
    new(-87.69f, -36.96f), new(-89.60f, -35.22f), new(-91.97f, -32.82f), new(-92.23f, -32.25f), new(-93.19f, -29.16f),
    new(-93.50f, -25.98f), new(-93.61f, -25.32f), new(-94.00f, -23.42f), new(-94.05f, -22.81f), new(-93.62f, -20.96f),
    new(-93.31f, -20.47f), new(-92.43f, -19.60f), new(-92.21f, -18.95f), new(-91.99f, -17.64f), new(-92.02f, -16.98f),
    new(-92.20f, -15.75f), new(-92.17f, -15.16f), new(-92.02f, -14.48f), new(-92.07f, -13.90f), new(-91.19f, -11.99f),
    new(-90.85f, -11.45f), new(-87.88f, -10.78f), new(-87.54f, -10.39f), new(-87.38f, -9.75f), new(-87.61f, -9.16f),
    new(-87.96f, -8.78f), new(-88.08f, -8.19f), new(-88.17f, -7.60f), new(-88.23f, -7.00f), new(-88.20f, -6.37f),
    new(-88.29f, -2.89f), new(-88.54f, -2.24f), new(-88.97f, -1.88f), new(-88.89f, -1.33f), new(-88.65f, -0.78f),
    new(-88.28f, -0.31f), new(-87.68f, -0.04f), new(-87.12f, 0.11f), new(-86.53f, 0.04f), new(-85.99f, -0.24f),
    new(-85.89f, -0.79f), new(-85.38f, -1.24f), new(-84.85f, -1.57f), new(-84.45f, -2.10f), new(-84.29f, -2.69f),
    new(-84.37f, -3.36f), new(-84.69f, -3.93f), new(-85.11f, -4.33f), new(-85.73f, -4.51f), new(-85.78f, -5.23f),
    new(-85.75f, -6.59f), new(-85.25f, -6.79f), new(-79.76f, -4.65f), new(-79.24f, -4.24f), new(-78.01f, -2.67f),
    new(-77.61f, -2.29f), new(-75.23f, -1.56f), new(-71.14f, 1.70f), new(-70.65f, 2.14f), new(-66.94f, 5.73f),
    new(-66.67f, 6.38f), new(-66.18f, 8.22f), new(-65.81f, 8.60f), new(-64.68f, 9.21f), new(-64.13f, 9.61f),
    new(-62.98f, 11.92f), new(-63.39f, 12.47f), new(-63.71f, 13.01f), new(-63.45f, 13.58f), new(-63.08f, 14.07f),
    new(-62.51f, 14.12f), new(-61.83f, 14.20f), new(-61.60f, 14.65f), new(-61.34f, 15.24f), new(-59.68f, 19.40f),
    new(-59.26f, 20.62f), new(-59.58f, 22.65f), new(-60.09f, 23.11f), new(-66.15f, 24.92f), new(-66.64f, 25.27f),
    new(-68.37f, 27.43f), new(-68.44f, 27.95f), new(-67.55f, 31.21f), new(-67.76f, 31.86f), new(-68.95f, 34.22f),
    new(-69.49f, 35.38f), new(-69.69f, 35.98f), new(-70.45f, 39.31f), new(-70.65f, 39.96f), new(-71.12f, 41.17f),
    new(-71.32f, 41.76f), new(-71.30f, 43.72f), new(-72.64f, 51.81f), new(-72.98f, 52.36f), new(-74.74f, 54.44f),
    new(-75.18f, 54.93f), new(-78.73f, 57.65f), new(-79.27f, 58.04f), new(-84.16f, 61.30f), new(-84.79f, 61.58f),
    new(-88.07f, 62.07f), new(-88.75f, 62.16f), new(-89.97f, 62.14f), new(-90.58f, 62.11f), new(-91.19f, 62.11f),
    new(-91.84f, 62.14f), new(-96.03f, 62.53f), new(-96.68f, 62.51f), new(-97.96f, 62.20f), new(-98.57f, 62.04f),
    new(-99.16f, 61.92f), new(-99.82f, 61.88f), new(-100.41f, 62.00f), new(-100.94f, 62.31f), new(-101.57f, 62.45f),
    new(-101.46f, 63.14f), new(-101.60f, 63.79f), new(-102.24f, 63.97f),
    new(-102.84f, 64.11f), new(-104.66f, 64.47f), new(-105.20f, 64.53f), new(-105.41f, 64.06f), new(-106.78f, 60.03f),
    new(-106.99f, 59.35f), new(-107.35f, 57.97f), new(-108.99f, 49.49f), new(-109.02f, 48.96f), new(-108.51f, 48.91f),
    new(-104.64f, 48.66f), new(-104.00f, 48.88f), new(-103.72f, 49.44f), new(-100.46f, 52.81f), new(-99.90f, 52.91f),
    new(-93.92f, 53.02f), new(-93.32f, 52.91f), new(-91.50f, 52.20f), new(-90.85f, 52.20f), new(-87.58f, 52.36f),
    new(-87.11f, 52.09f), new(-84.93f, 49.90f), new(-83.37f, 47.92f), new(-83.01f, 47.40f), new(-82.19f, 45.58f),
    new(-81.89f, 44.98f), new(-80.93f, 43.27f), new(-79.15f, 40.53f), new(-78.91f, 40.00f), new(-78.44f, 36.71f),
    new(-78.81f, 36.30f), new(-79.34f, 36.00f), new(-79.85f, 35.52f), new(-79.40f, 33.58f), new(-79.19f, 32.95f),
    new(-77.77f, 30.72f), new(-77.44f, 30.21f), new(-77.54f, 28.14f), new(-77.59f, 27.47f), new(-78.02f, 27.18f),
    new(-80.52f, 26.69f), new(-81.60f, 25.98f), new(-82.10f, 25.55f), new(-82.96f, 23.67f), new(-82.93f, 23.01f),
    new(-82.74f, 21.73f), new(-82.85f, 21.18f), new(-83.64f, 20.13f), new(-84.20f, 19.98f), new(-85.50f, 19.79f),
    new(-86.10f, 19.43f), new(-87.55f, 18.03f), new(-87.76f, 17.35f), new(-87.91f, 16.66f), new(-88.09f, 16.01f),
    new(-88.56f, 15.67f), new(-89.11f, 15.35f), new(-89.66f, 15.12f), new(-91.55f, 15.61f), new(-92.24f, 15.36f),
    new(-93.54f, 14.84f), new(-94.02f, 14.32f), new(-94.85f, 13.28f), new(-95.32f, 12.98f), new(-96.59f, 12.79f),
    new(-97.04f, 13.03f), new(-98.01f, 13.77f), new(-98.59f, 14.07f), new(-101.17f, 14.13f), new(-101.78f, 13.81f),
    new(-102.33f, 13.40f), new(-102.86f, 13.26f), new(-104.61f, 13.58f), new(-105.08f, 10.48f), new(-104.69f, 10.08f),
    new(-103.50f, 9.73f), new(-103.09f, 9.37f), new(-102.87f, 8.77f), new(-102.81f, 8.05f), new(-102.91f, 7.44f),
    new(-103.43f, 7.23f), new(-103.74f, 6.83f), new(-103.97f, 6.17f), new(-104.37f, 5.70f), new(-105.50f, 5.10f),
    new(-108.23f, -0.23f), new(-108.41f, -0.70f), new(-107.40f, -1.68f), new(-106.86f, -2.13f), new(-106.94f, -2.69f),
    new(-107.35f, -3.13f), new(-107.78f, -3.61f), new(-108.32f, -3.75f), new(-109.23f, -2.88f), new(-109.76f, -3.25f),
    new(-110.40f, -4.48f), new(-110.04f, -4.86f), new(-107.16f, -7.66f), new(-106.98f, -8.15f), new(-107.02f, -10.87f),
    new(-106.98f, -11.41f), new(-103.80f, -11.82f), new(-103.11f, -11.86f), new(-102.45f, -11.98f), new(-102.06f, -12.35f),
    new(-102.45f, -16.17f), new(-102.54f, -16.80f), new(-102.98f, -19.35f), new(-102.91f, -21.23f), new(-102.83f, -21.89f),
    new(-102.42f, -23.88f), new(-102.34f, -24.48f), new(-102.90f, -28.89f), new(-101.65f, -33.35f), new(-101.29f, -33.92f),
    new(-99.52f, -36.55f), new(-99.12f, -37.04f), new(-95.24f, -41.13f), new(-94.71f, -41.55f), new(-92.11f, -43.51f),
    new(-91.48f, -43.78f), new(-89.50f, -44.21f), new(-88.97f, -44.44f), new(-85.02f, -47.86f), new(-84.49f, -48.23f),
    new(-80.08f, -51.02f), new(-79.44f, -51.19f), new(-76.23f, -50.36f), new(-75.74f, -50.53f), new(-75.26f, -51.02f),
    new(-74.83f, -51.41f), new(-74.39f, -51.93f), new(-74.24f, -52.55f), new(-74.99f, -53.61f), new(-75.12f, -54.27f),
    new(-75.00f, -54.94f), new(-74.28f, -60.16f), new(-74.14f, -60.81f), new(-72.72f, -66.60f), new(-71.68f, -67.51f),
    new(-71.44f, -67.99f), new(-71.39f, -69.27f), new(-71.13f, -69.89f), new(-70.37f, -70.83f), new(-69.93f, -71.29f),
    new(-69.32f, -71.51f), new(-68.81f, -71.81f), new(-67.66f, -72.35f), new(-67.04f, -72.61f), new(-62.03f, -73.32f),
    new(-61.37f, -73.30f)];
    private static readonly Polygon[] hole = [new Polygon(new(-112.773f, 8.201f), 1.25f, 6)];
    public static readonly ArenaBoundsCustom Arena1 = new([new PolygonCustom(vertices1)], hole);
    public static readonly ArenaBoundsCustom Arena1B = new([new PolygonCustom(vertices1), new PolygonCustom(vertices2)], hole);
    public static readonly ArenaBoundsCustom Arena2 = new([new PolygonCustom(vertices2)]);

    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.Biloko, (uint)OID.GrizzlyHost, (uint)OID.WitheredMelia,
    (uint)OID.RoyalOak];

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
