namespace BossMod.Heavensward.Dungeon.D15Xelphatol.D150ChasmHarpeia;

public enum OID : uint
{
    Boss = 0x17AF, // R1.2
    SwiftwaterHakulaq = 0x17AE, // R2.16
    AbalathianSlug = 0x17AB, // R0.8
    ChasmCobra = 0x17AC, // R1.44
    VelodynaToad = 0x17AD, // R4.32
    FallenRock = 0xD25 // R0.5
}

public enum AID : uint
{
    AutoAttack1 = 872, // AbalathianSlug->player, no cast, single-target
    AutoAttack2 = 870, // SwiftwaterHakulaq/ChasmCobra->player, no cast, single-target
    AutoAttack3 = 871, // Boss->player, no cast, single-target

    LaboredLeap = 1006, // VelodynaToad->self, 4.0s cast, range 6+R circle
    Lap = 976, // VelodynaToad->player, no cast, single-target

    FallingRock = 6852, // FallenRock->location, 5.0s cast, range 4 circle
    FlashFlood = 1656, // SwiftwaterHakulaq->self, 2.5s cast, range 6+R 120-degree cone
    DrippingFang = 409, // ChasmCobra->player, no cast, single-target
    WingsOfWoe = 2478, // Boss->location, 2.5s cast, range 6 circle
    WildRattle = 408, // ChasmCobra->player, no cast, single-target
}

class FlashFlood(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlashFlood, new AOEShapeCone(8.16f, 60f.Degrees()));
class LaboredLeap(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LaboredLeap, 10.32f);
class FallenRock(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FallingRock, 4f);
class WingsOfWoe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WingsOfWoe, 6f);

class D150ChasmHarpeiaStates : StateMachineBuilder
{
    public D150ChasmHarpeiaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FlashFlood>()
            .ActivateOnEnter<FallenRock>()
            .ActivateOnEnter<WingsOfWoe>()
            .ActivateOnEnter<LaboredLeap>()
            .Raw.Update = () => AllDeadOrDestroyedInBounds(D150ChasmHarpeia.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 182, NameID = 5253, SortOrder = 1)]
public sealed class D150ChasmHarpeia : BossModule
{
    public D150ChasmHarpeia(WorldState ws, Actor primary) : this(ws, primary, primary.PosRot.X < -130f ? BuildArena1() : BuildArena2()) { }

    private D150ChasmHarpeia(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena1()
    {
        WPos[] vertices = [new(-116.6f, 153.39f), new(-116.25f, 153.86f), new(-114.35f, 154.36f), new(-113.7f, 154.48f), new(-111.05f, 154.59f),
        new(-110.39f, 154.71f), new(-106.02f, 160.42f), new(-106.51f, 160.64f), new(-106.66f, 161.15f), new(-105.65f, 165.95f),
        new(-105.49f, 166.42f), new(-106.31f, 170.45f), new(-106.4f, 171.14f), new(-106.59f, 171.61f), new(-106.97f, 172.2f),
        new(-108.27f, 174.61f), new(-108.98f, 176.61f), new(-110.17f, 177.12f), new(-111.99f, 179.14f), new(-112.65f, 179.2f),
        new(-113.86f, 179.05f), new(-114.44f, 179.03f), new(-114.91f, 179.49f), new(-116.13f, 181.18f), new(-116.45f, 181.83f),
        new(-116.7f, 182.47f), new(-117.28f, 185.68f), new(-116.97f, 186.18f), new(-116.81f, 186.85f), new(-116.71f, 188.23f),
        new(-118.6f, 192.97f), new(-118.98f, 193.54f), new(-120.4f, 194.72f), new(-120.7f, 195.15f), new(-120.38f, 196.45f),
        new(-120.56f, 197.11f), new(-121.76f, 198.64f), new(-124.65f, 201.37f), new(-125.22f, 201.79f), new(-126.55f, 201.71f),
        new(-127.16f, 201.72f), new(-128.82f, 202.76f), new(-129.44f, 203.04f), new(-130.71f, 203.41f), new(-131.28f, 203.67f),
        new(-132.35f, 204.47f), new(-132.97f, 204.78f), new(-134.77f, 205.41f), new(-135.1f, 205.86f), new(-135.59f, 207.08f),
        new(-135.62f, 207.73f), new(-135.47f, 209.04f), new(-135.28f, 209.63f), new(-134.19f, 211.55f), new(-134.05f, 212.06f),
        new(-134.1f, 212.61f), new(-134.62f, 212.94f), new(-134.86f, 219.11f), new(-135.36f, 222.6f), new(-135.29f, 223.3f),
        new(-135.17f, 223.91f), new(-133.35f, 228.29f), new(-132.91f, 228.64f), new(-132.4f, 232.12f), new(-132.85f, 232.36f),
        new(-135.39f, 233.28f), new(-136.02f, 233.59f), new(-137.76f, 234.73f), new(-138.43f, 235.08f), new(-138.91f, 235.3f),
        new(-141.55f, 235.2f), new(-142.16f, 235.37f), new(-142.43f, 236), new(-142.83f, 237.41f), new(-143.11f, 238.12f),
        new(-143.62f, 238.12f), new(-144.93f, 238.01f), new(-145.56f, 237.87f), new(-147.99f, 236.86f), new(-148.47f, 236.4f),
        new(-148.95f, 236.24f), new(-149.59f, 236.2f), new(-150.74f, 235.72f), new(-151.33f, 235.81f), new(-151.95f, 235.97f),
        new(-152.64f, 235.96f), new(-154.06f, 234.75f), new(-154.63f, 234.8f), new(-157.62f, 235.69f), new(-158.26f, 235.57f),
        new(-158.86f, 235.39f), new(-160.13f, 235.38f), new(-160.75f, 235.16f), new(-162.09f, 233.8f), new(-162.59f, 233.64f),
        new(-163.16f, 233.33f), new(-163.46f, 232.76f), new(-163.77f, 232.26f), new(-168.69f, 228.97f), new(-169.27f, 228.91f),
        new(-171.21f, 229.05f), new(-172.32f, 229.42f), new(-172.91f, 229.09f), new(-173.49f, 228.93f), new(-174.16f, 229.12f),
        new(-174.83f, 229.22f), new(-175.41f, 228.93f), new(-175.98f, 228.86f), new(-177.91f, 229.02f), new(-178.46f, 229.12f),
        new(-178.66f, 229.62f), new(-178.69f, 230.34f), new(-178.54f, 231.73f), new(-178.52f, 233.24f), new(-178.58f, 233.9f),
        new(-179.08f, 235.18f), new(-179.11f, 235.85f), new(-180.48f, 237.31f), new(-180.98f, 237.73f), new(-181.52f, 238.02f),
        new(-181.92f, 238.5f), new(-182.25f, 239.08f), new(-182.48f, 239.63f), new(-183.07f, 239.67f), new(-184.21f, 239.02f),
        new(-187.36f, 238.15f), new(-188.62f, 238.12f), new(-189.23f, 238.44f), new(-190.09f, 239.32f), new(-190.82f, 239.4f),
        new(-190.98f, 238.91f), new(-191.04f, 238.3f), new(-191.33f, 237.82f), new(-191.78f, 237.34f), new(-193.03f, 235.64f),
        new(-193.2f, 235.03f), new(-193.74f, 233.81f), new(-194.27f, 233.52f), new(-194.74f, 233.35f), new(-195.35f, 233.24f),
        new(-196.54f, 233.94f), new(-197.11f, 234.34f), new(-197.5f, 234.8f), new(-199.69f, 236.37f), new(-201, 237.64f),
        new(-201.28f, 238.18f), new(-202.19f, 242.49f), new(-202.37f, 244.38f), new(-202.37f, 244.94f), new(-201.98f, 246.15f),
        new(-201.34f, 247.22f), new(-199.2f, 248.8f), new(-198.63f, 248.8f), new(-198.01f, 248.68f), new(-195.59f, 247.81f),
        new(-194.92f, 247.68f), new(-194.27f, 247.72f), new(-193.61f, 247.71f), new(-191.18f, 246.84f), new(-190.73f, 246.55f),
        new(-190.44f, 247.01f), new(-189.53f, 248.75f), new(-189.25f, 249.36f), new(-188.18f, 251.04f), new(-187.75f, 251.43f),
        new(-184.18f, 252.84f), new(-183.73f, 253.08f), new(-183.39f, 253.62f), new(-182.88f, 254.06f), new(-180.44f, 256.36f),
        new(-180.03f, 256.88f), new(-179.78f, 257.51f), new(-179.72f, 258.17f), new(-179.75f, 260.16f), new(-179.84f, 260.83f),
        new(-180.07f, 261.46f), new(-180.35f, 262.05f), new(-181.04f, 263.16f), new(-181.46f, 263.66f), new(-181.92f, 264.13f),
        new(-182.43f, 264.55f), new(-182.99f, 264.91f), new(-183.6f, 265.16f), new(-184.23f, 265.34f), new(-187.39f, 265.71f),
        new(-187.95f, 265.85f), new(-189.39f, 267.06f), new(-190.08f, 267.08f), new(-216.73f, 259.73f), new(-217.3f, 259.3f),
        new(-217.71f, 258.83f), new(-218.09f, 258.23f), new(-213.82f, 255.18f), new(-212.37f, 252.28f), new(-212.54f, 251.69f),
        new(-213.01f, 251.22f), new(-213.11f, 250.64f), new(-214.73f, 247.06f), new(-206.6f, 219.24f), new(-204.82f, 219.05f),
        new(-204.23f, 218.9f), new(-201.21f, 217.61f), new(-197.44f, 215.15f), new(-197.15f, 214.62f), new(-196.12f, 212.23f),
        new(-195.68f, 211.71f), new(-192.68f, 210.45f), new(-190.07f, 210.11f), new(-189.38f, 210.16f), new(-187.4f, 211.82f),
        new(-186.9f, 212.16f), new(-180.3f, 213.78f), new(-179.63f, 213.99f), new(-177.35f, 216.43f), new(-174.72f, 218.4f),
        new(-172.4f, 220.72f), new(-171.88f, 220.87f), new(-168.72f, 220.89f), new(-165.51f, 220.47f), new(-164.87f, 220.71f),
        new(-160.51f, 224.41f), new(-160.04f, 224.75f), new(-156.48f, 226.04f), new(-155.91f, 225.97f), new(-155.28f, 225.72f),
        new(-154.58f, 225.69f), new(-153.97f, 225.98f), new(-153.49f, 226.39f), new(-152.99f, 226.65f), new(-151.75f, 226.85f),
        new(-151.12f, 226.86f), new(-150.6f, 226.47f), new(-150.48f, 225.84f), new(-150.57f, 225.24f), new(-151.04f, 224.99f),
        new(-151.73f, 224.97f), new(-153.14f, 225), new(-153.82f, 224.74f), new(-153.8f, 224.1f), new(-154.43f, 222.99f),
        new(-154.86f, 219.86f), new(-155.95f, 218.12f), new(-155.82f, 217.48f), new(-155.19f, 216.42f), new(-155.22f, 215.88f),
        new(-155.7f, 214.73f), new(-155.57f, 214.08f), new(-154.7f, 213.13f), new(-154.54f, 212.59f), new(-155.17f, 211.45f),
        new(-155.36f, 210.74f), new(-153.08f, 209.48f), new(-152.71f, 209.09f), new(-152.42f, 207.22f), new(-152.21f, 206.53f),
        new(-149.77f, 206.06f), new(-149.59f, 205.55f), new(-149.46f, 204.27f), new(-149.2f, 203.62f), new(-148.79f, 203.1f),
        new(-148.23f, 202.64f), new(-147.6f, 202.94f), new(-147.05f, 203.01f), new(-146.52f, 202.73f), new(-146.1f, 202.23f),
        new(-145.62f, 201.82f), new(-145.07f, 201.42f), new(-144.44f, 201.1f), new(-141.9f, 200.52f), new(-141.54f, 200.16f),
        new(-141.67f, 199.58f), new(-142.11f, 198.37f), new(-141.62f, 197.86f), new(-140.52f, 197.06f), new(-139.84f, 196.93f),
        new(-138.56f, 197.13f), new(-138, 196.98f), new(-134.97f, 195.39f), new(-134.65f, 194.86f), new(-134.09f, 193.59f),
        new(-133.71f, 193.02f), new(-132.7f, 192.31f), new(-132.36f, 191.87f), new(-132.18f, 190.56f), new(-131.95f, 189.94f),
        new(-129.76f, 188.83f), new(-129.64f, 188.29f), new(-129.65f, 187.65f), new(-129.55f, 186.97f), new(-129.02f, 186.58f),
        new(-128.59f, 186.19f), new(-128.63f, 185.63f), new(-129.05f, 183.71f), new(-126.83f, 182.42f), new(-126.38f, 182.02f),
        new(-126.21f, 180), new(-126.2f, 179.31f), new(-126.33f, 177.29f), new(-126.46f, 176.8f), new(-130.99f, 174.6f),
        new(-130.93f, 173.91f), new(-130.63f, 172.12f), new(-130.94f, 171.68f), new(-133.41f, 170.6f), new(-133.53f, 169.91f),
        new(-133.53f, 168.66f), new(-135.31f, 166.64f), new(-135.64f, 166.22f), new(-134.17f, 164.02f), new(-134.25f, 163.46f),
        new(-135.28f, 160.93f), new(-135.24f, 160.27f), new(-134.93f, 158.3f), new(-134.45f, 158.12f), new(-130.68f, 157.55f),
        new(-130.38f, 157.97f), new(-130.1f, 158.57f), new(-129.81f, 159.1f), new(-129.2f, 159.17f), new(-128.61f, 159.14f),
        new(-128.21f, 158.71f), new(-127.66f, 158.29f), new(-117.08f, 153.3f)];
        var arena = new ArenaBoundsCustom([new PolygonCustom(vertices)]);
        return (arena.Center, arena);
    }
    private static (WPos center, ArenaBoundsCustom arena) BuildArena2()
    {
        WPos[] vertices = [new(-84.63f, -42.88f), new(-83.93f, -42.42f), new(-84.48f, -42.18f), new(-84.79f, -41.69f), new(-85.87f, -39.26f),
        new(-86.44f, -38.87f), new(-87.05f, -38.6f), new(-87.6f, -38.28f), new(-88.05f, -37.87f), new(-89.01f, -36.1f),
        new(-89.67f, -35.81f), new(-90.25f, -35.71f), new(-90.74f, -35.36f), new(-91.66f, -32.91f), new(-92.11f, -32.43f),
        new(-93.68f, -31.38f), new(-98.11f, -30.79f), new(-99.45f, -31.04f), new(-100.04f, -31.23f), new(-100.62f, -31.21f),
        new(-101.11f, -30.8f), new(-101.59f, -30.33f), new(-102.23f, -30.15f), new(-102.9f, -30.06f), new(-103.52f, -30.27f),
        new(-104.09f, -30.31f), new(-104.84f, -29.39f), new(-105.85f, -28.51f), new(-106.27f, -28.01f), new(-106.93f, -27.94f),
        new(-107.6f, -27.82f), new(-107.9f, -27.4f), new(-107.85f, -26.81f), new(-107.55f, -25.57f), new(-107.3f, -25.09f),
        new(-106.72f, -25.05f), new(-106.11f, -25.27f), new(-105.52f, -25.54f), new(-102.84f, -26.3f), new(-100.99f, -26.16f),
        new(-100.41f, -26.2f), new(-95.67f, -27.91f), new(-94.96f, -27.82f), new(-94.29f, -27.66f), new(-92.38f, -26.95f),
        new(-91.94f, -26.69f), new(-89.69f, -24.87f), new(-89.27f, -24.35f), new(-87, -20.87f), new(-86.49f, -20.49f),
        new(-85.39f, -19.79f), new(-84.94f, -19.2f), new(-84.99f, -18.49f), new(-85.14f, -17.85f), new(-85.81f, -15.86f),
        new(-87.62f, -13.23f), new(-87.39f, -11.31f), new(-87.79f, -10.75f), new(-88.79f, -9.73f), new(-89.16f, -9.24f),
        new(-89.41f, -8.57f), new(-90.99f, -7.22f), new(-91.63f, -7), new(-92.93f, -6.96f), new(-95.81f, -6.32f),
        new(-96.36f, -6.06f), new(-96.75f, -5.61f), new(-97.34f, -5.28f), new(-97.86f, -5.17f), new(-98.73f, -3.48f),
        new(-99.87f, -1.69f), new(-100.31f, -1.39f), new(-100.82f, -1.88f), new(-101.83f, -3.51f), new(-102.04f, -4.2f),
        new(-102.14f, -4.83f), new(-102.38f, -5.49f), new(-102.68f, -6.09f), new(-103.12f, -6.48f), new(-103.71f, -6.8f),
        new(-105.33f, -7.88f), new(-105.71f, -8.43f), new(-106.1f, -8.9f), new(-106.53f, -9.34f), new(-107, -9.58f),
        new(-107.65f, -9.74f), new(-108.16f, -9.61f), new(-108.63f, -9.18f), new(-109.43f, -8.25f), new(-109.77f, -7.69f),
        new(-110.05f, -7.05f), new(-111.19f, -5.51f), new(-111.53f, -4.99f), new(-112.36f, -3.28f), new(-112.36f, -2.63f),
        new(-111.87f, 1.4f), new(-111.72f, 2.02f), new(-111.62f, 2.65f), new(-111.47f, 3.27f), new(-111.25f, 3.83f),
        new(-110.52f, 4.81f), new(-110.08f, 5.23f), new(-109.61f, 5.61f), new(-107.82f, 6.27f), new(-107.23f, 6.42f),
        new(-106.66f, 6.39f), new(-106.09f, 6.08f), new(-103.38f, 4.05f), new(-102.68f, 3.85f), new(-102.02f, 3.62f),
        new(-100.97f, 2.8f), new(-100.32f, 2.47f), new(-98.52f, 6.38f), new(-98.23f, 6.9f), new(-97.78f, 7.14f),
        new(-94.17f, 5.7f), new(-93.46f, 5.69f), new(-92.93f, 6.19f), new(-92.41f, 6.53f), new(-90.46f, 7.14f),
        new(-90.51f, 7.67f), new(-91.38f, 11.59f), new(-91.36f, 12.2f), new(-91.18f, 12.84f), new(-89.64f, 16.71f),
        new(-89.31f, 17.31f), new(-88.84f, 20.85f), new(-88.92f, 21.46f), new(-88.48f, 21.91f), new(-87.91f, 21.91f),
        new(-87.27f, 21.82f), new(-86.7f, 21.45f), new(-86.28f, 21.79f), new(-84.11f, 24.25f), new(-82.47f, 27.13f),
        new(-82.59f, 27.8f), new(-83.8f, 29.46f), new(-86.95f, 30.74f), new(-87.42f, 31.12f), new(-88.92f, 32.55f),
        new(-90.62f, 33.79f), new(-91.08f, 34.21f), new(-92.88f, 37), new(-93.69f, 38.86f), new(-93.52f, 39.4f),
        new(-93.14f, 39.91f), new(-93.01f, 40.58f), new(-93.26f, 41.18f), new(-93.68f, 41.77f), new(-94.56f, 42.72f),
        new(-95.14f, 43.03f), new(-95.78f, 42.96f), new(-96.28f, 43.24f), new(-96.75f, 43.73f), new(-97.24f, 44.16f),
        new(-97.79f, 44.55f), new(-98.45f, 44.64f), new(-100.28f, 44.15f), new(-100.82f, 43.82f), new(-101.42f, 43.67f),
        new(-102.01f, 43.58f), new(-102.66f, 43.55f), new(-103.29f, 43.66f), new(-103.91f, 43.68f), new(-105.78f, 43.67f),
        new(-106.18f, 43.99f), new(-106.94f, 45.77f), new(-107.97f, 46.72f), new(-108.45f, 46.95f), new(-108.89f, 46.47f),
        new(-109.47f, 46.38f), new(-110.07f, 46.39f), new(-110.68f, 46.66f), new(-110.88f, 47.17f), new(-111.42f, 47.58f),
        new(-112.01f, 47.83f), new(-112.48f, 48.2f), new(-135.36f, 80.87f), new(-135.25f, 81.44f), new(-135.53f, 82.06f),
        new(-135.89f, 82.59f), new(-136.08f, 83.15f), new(-135.99f, 83.74f), new(-135.54f, 84.08f), new(-136.03f, 84.48f),
        new(-136.19f, 85.08f), new(-136.29f, 85.7f), new(-136.31f, 86.29f), new(-135.94f, 86.78f), new(-134.7f, 88.19f),
        new(-132.96f, 88.97f), new(-132.5f, 89.54f), new(-132.25f, 90.17f), new(-132.71f, 91.3f), new(-132.48f, 91.82f),
        new(-130.69f, 92.69f), new(-130.6f, 93.84f), new(-130.67f, 95.1f), new(-130.62f, 95.67f), new(-129.61f, 96.41f),
        new(-129.56f, 96.93f), new(-129.76f, 97.59f), new(-130.06f, 98.18f), new(-130.42f, 98.77f), new(-130.98f, 99.23f),
        new(-131.62f, 99.6f), new(-132.04f, 99.95f), new(-131.92f, 100.46f), new(-131.66f, 101.07f), new(-131.44f, 101.75f),
        new(-131.68f, 103.09f), new(-132.16f, 103.65f), new(-132.65f, 104.06f), new(-133.35f, 104.25f), new(-134.39f, 104.27f),
        new(-135.47f, 104.88f), new(-135.95f, 105.24f), new(-136.12f, 105.81f), new(-136.12f, 106.46f), new(-136.45f, 107.07f),
        new(-137.31f, 108.14f), new(-138.78f, 108.15f), new(-139.41f, 108.42f), new(-139.57f, 108.92f), new(-139.49f, 109.45f),
        new(-139.09f, 110), new(-139.36f, 110.53f), new(-139.28f, 111.15f), new(-129.07f, 133.18f), new(-129.09f, 133.8f),
        new(-131.66f, 135.65f), new(-132.22f, 135.96f), new(-132.83f, 136.12f), new(-133.45f, 136.22f), new(-133.97f, 136.26f),
        new(-134.62f, 136.01f), new(-135.04f, 135.46f), new(-145.02f, 114.04f), new(-145.49f, 113.86f), new(-145.95f, 113.43f),
        new(-146.21f, 112.87f), new(-146.53f, 112.33f), new(-147.08f, 111.96f), new(-147.62f, 110.84f), new(-148.14f, 110.64f),
        new(-148.83f, 110.64f), new(-149.53f, 110.7f), new(-152.8f, 110.68f), new(-153.41f, 110.77f), new(-154.01f, 111.05f),
        new(-159.84f, 109.45f), new(-161.17f, 109.32f), new(-161.82f, 109.06f), new(-162.31f, 108.62f), new(-162.68f, 108.02f),
        new(-162.81f, 107.31f), new(-162.58f, 106.66f), new(-161.71f, 104.83f), new(-161.87f, 104.33f), new(-162.62f, 103.3f),
        new(-162.76f, 102.62f), new(-162.31f, 101.36f), new(-162.18f, 100.78f), new(-162.51f, 100.3f), new(-163.81f, 98.68f),
        new(-164.34f, 97.49f), new(-164.37f, 96.82f), new(-164.12f, 96.23f), new(-164.18f, 95.68f), new(-164.93f, 94.58f),
        new(-164.76f, 94.09f), new(-163.25f, 92.46f), new(-163.11f, 91.91f), new(-163.49f, 91.48f), new(-163.97f, 91.03f),
        new(-164.21f, 90.41f), new(-164.23f, 89.72f), new(-163.45f, 88.57f), new(-162.78f, 88.23f), new(-162.17f, 88.38f),
        new(-161.53f, 88.02f), new(-161.13f, 87.62f), new(-161.11f, 87), new(-162.16f, 85.41f), new(-162.4f, 83.41f),
        new(-160.98f, 81.87f), new(-158.32f, 82.09f), new(-157.85f, 81.76f), new(-156.86f, 80.84f), new(-156.3f, 80.42f),
        new(-154.91f, 80.48f), new(-154.44f, 80.67f), new(-154.03f, 81.01f), new(-153.44f, 81.08f), new(-152.83f, 81.07f),
        new(-151.59f, 80.62f), new(-151.06f, 80.29f), new(-148.32f, 79.44f), new(-145.77f, 80.2f), new(-145.22f, 79.96f),
        new(-144.26f, 79.11f), new(-143.66f, 78.81f), new(-143.04f, 78.83f), new(-142.44f, 78.68f), new(-142.08f, 78.29f),
        new(-141.74f, 77.69f), new(-141.15f, 77.48f), new(-140.58f, 77.23f), new(-117.63f, 44.45f), new(-117.56f, 43.92f),
        new(-117.62f, 43.31f), new(-117.22f, 42.83f), new(-116.86f, 42.29f), new(-116.8f, 41.78f), new(-117.13f, 41.17f),
        new(-116.36f, 40.38f), new(-115.56f, 39.29f), new(-115.38f, 38.74f), new(-115.32f, 38.05f), new(-115.05f, 37.43f),
        new(-114.56f, 37.03f), new(-114.15f, 36.65f), new(-114.12f, 36.02f), new(-114.17f, 34.05f), new(-115.01f, 30.89f),
        new(-117.13f, 27.54f), new(-118.24f, 25.08f), new(-118.41f, 24.4f), new(-117.12f, 21.79f), new(-117.16f, 21.24f),
        new(-118.22f, 19.6f), new(-118.72f, 19.19f), new(-121.39f, 17.32f), new(-122.49f, 16.75f), new(-123.43f, 15.97f),
        new(-124.03f, 15.77f), new(-124.6f, 15.48f), new(-125.62f, 14.8f), new(-126.04f, 14.24f), new(-126.6f, 12.46f),
        new(-126.66f, 11.22f), new(-126.85f, 10.68f), new(-127.82f, 9.91f), new(-127.81f, 9.28f), new(-125.23f, 5.31f),
        new(-124.93f, 4.77f), new(-126.55f, 0.22f), new(-126.88f, -0.26f), new(-128.67f, -2.15f), new(-128.83f, -2.82f),
        new(-128.71f, -7.79f), new(-128.4f, -10.55f), new(-125.33f, -16.17f), new(-125.4f, -16.77f), new(-125.94f, -19.36f),
        new(-125.12f, -21.95f), new(-123.91f, -22.58f), new(-123.41f, -22.96f), new(-121.17f, -25.24f), new(-120.85f, -25.74f),
        new(-119.7f, -28.07f), new(-119.22f, -30.67f), new(-117.55f, -32), new(-116.36f, -33.56f), new(-115.88f, -34.08f),
        new(-114.19f, -35.23f), new(-113.74f, -35.63f), new(-112.28f, -38.55f), new(-110.93f, -38.84f), new(-110.23f, -38.88f),
        new(-108.96f, -38.56f), new(-108.4f, -38.55f), new(-106.51f, -39.25f), new(-105.83f, -39.25f), new(-104.69f, -38.66f),
        new(-101.53f, -39.13f), new(-99.03f, -40.18f), new(-98.54f, -40.58f), new(-93.73f, -46.65f), new(-93.18f, -47.1f)];
        var arena = new ArenaBoundsCustom([new PolygonCustom(vertices)]);
        return (arena.Center, arena);
    }

    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.ChasmCobra, (uint)OID.VelodynaToad, (uint)OID.AbalathianSlug, (uint)OID.SwiftwaterHakulaq];

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
