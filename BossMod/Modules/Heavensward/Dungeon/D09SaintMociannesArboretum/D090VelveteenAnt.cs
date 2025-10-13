namespace BossMod.Heavensward.Dungeon.D09SaintMociannesArboretum.D090VelveteenAnt;

public enum OID : uint
{
    Boss = 0x1423, // R1.95
    WorkerHawk = 0x146E, // R1.08
    ArboretumCrawler = 0x1425, // R1.5
    ThavnairianTortoise = 0x1422, // R5.0
    GardenBeeCloud = 0x1426, // R0.400, x?
    SoldierHawk = 0x1424, // R0.54
    OrnHornet = 0x1427, // R0.4
    Honeycomb1 = 0x1428, // R1.5
    Honeycomb2 = 0x1429, // R1.5
    WallController1 = 0x1E9DFA, // R2.0
    WallController2 = 0x1E9DFB // R2.0
}

public enum AID : uint
{
    AutoAttack1 = 872, // ThavnairianTortoise/ArboretumCrawler/GardenBeeCloud->player, no cast, single-target
    AutoAttack2 = 871, // WorkerHawk/SoldierHawk/OrnHornet->player, no cast, single-target
    AutoAttack = 870, // Boss->player, no cast, single-target

    TrapJaws = 523, // Boss->player, no cast, single-target
    StickyThread = 5369, // ArboretumCrawler->self, no cast, range 6+R 60-degree cone
    PoisonBreath = 5370, // ArboretumCrawler->self, no cast, range 6+R 120-degree cone
    TortoiseStomp = 417, // ThavnairianTortoise->self, 4.0s cast, range 6+R circle
    SpinalTap = 4525, // WorkerHawk->player, no cast, single-target
    UnfinalSting = 4526, // WorkerHawk->self, 3.0s cast, range 8+R width 3 rect
    FinalSting = 2482 // OrnHornet->player, 4.0s cast, single-target
}

sealed class WallRemoval(BossModule module) : BossComponent(module)
{
    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008u)
        {
            if (actor.OID == (uint)OID.WallController1)
            {
                SetArena(D090VelveteenAnt.Arena1B);
            }
            else if (actor.OID == (uint)OID.WallController2)
            {
                SetArena(D090VelveteenAnt.Arena2B);
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var pos = pc.PosRot;
        if (pos.X > -127f)
        {
            var pZ = pos.Z;
            var bounds = Arena.Bounds;
            if (bounds == D090VelveteenAnt.Arena1B && pZ < -186f)
            {
                SetArena(D090VelveteenAnt.Arena2);
            }
            else if (bounds == D090VelveteenAnt.Arena2 && pZ > -186f)
            {
                SetArena(D090VelveteenAnt.Arena1B);
            }
        }
    }

    private void SetArena(ArenaBoundsCustom bounds)
    {
        Arena.Bounds = bounds;
        Arena.Center = bounds.Center;
    }
}

sealed class PoisonBreathStickyThread(BossModule module) : Components.Cleave(module, (uint)AID.PoisonBreath, new AOEShapeCone(7.5f, 60f.Degrees()), [(uint)OID.ArboretumCrawler]);
sealed class TortoiseStomp(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TortoiseStomp, 11f);
sealed class UnfinalSting(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UnfinalSting, new AOEShapeRect(9.08f, 1.5f));
sealed class FinalSting(BossModule module) : Components.SingleTargetCast(module, (uint)AID.FinalSting);

sealed class D090VelveteenAntStates : StateMachineBuilder
{
    public D090VelveteenAntStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TortoiseStomp>()
            .ActivateOnEnter<UnfinalSting>()
            .ActivateOnEnter<FinalSting>()
            .ActivateOnEnter<PoisonBreathStickyThread>()
            .ActivateOnEnter<WallRemoval>()
            .Raw.Update = () => AllDeadOrDestroyed(D090VelveteenAnt.All) || D092QueenHawk.D092QueenHawk.ArenaBounds.Contains(Module.Raid.Player()!.Position - D092QueenHawk.D092QueenHawk.ArenaBounds.Center);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 41, NameID = 4641, SortOrder = 3)]
public sealed class D090VelveteenAnt(WorldState ws, Actor primary) : BossModule(ws, primary, Arena1.Center, Arena1)
{
    private static readonly WPos[] vertices1 = [new(-123.8f, -191.08f), new(-117.35f, -190.85f), new(-116.85f, -190.4f), new(-115.74f, -188.76f), new(-113.76f, -186.84f),
    new(-113.28f, -186.35f), new(-109.9f, -181.68f), new(-107.49f, -177.8f), new(-107.27f, -177.15f), new(-107.7f, -175.92f),
    new(-108.04f, -175.36f), new(-108.47f, -174.9f), new(-108.86f, -174.35f), new(-108.57f, -173.75f), new(-107.6f, -172.92f),
    new(-107.06f, -172.51f), new(-106.53f, -172.06f), new(-105.54f, -171.29f), new(-105.29f, -170.84f), new(-105.11f, -170.18f),
    new(-104.93f, -169.49f), new(-104.43f, -167.33f), new(-104.22f, -166.71f), new(-103.77f, -166.24f), new(-103.35f, -165.76f),
    new(-102.95f, -165.28f), new(-102.52f, -164.82f), new(-101.64f, -164.11f), new(-101.97f, -163.68f), new(-102.37f, -163.19f),
    new(-102.76f, -162.65f), new(-102.87f, -162f), new(-102.95f, -161.37f), new(-103.33f, -160.82f), new(-103.71f, -160.35f),
    new(-103.88f, -159.84f), new(-102.39f, -154.82f), new(-100.99f, -148.86f), new(-99.94f, -142.54f), new(-99.32f, -136.14f),
    new(-99.13f, -130.25f), new(-99.13f, -129.6f), new(-99.32f, -123.77f), new(-99.9f, -117.87f), new(-99.98f, -117.21f),
    new(-100.96f, -111.29f), new(-102.31f, -105.49f), new(-102.48f, -104.85f), new(-103.85f, -100.29f), new(-103.81f, -99.76f),
    new(-99.87f, -96.74f), new(-99.44f, -96.27f), new(-96.68f, -92.67f), new(-96.4f, -92.07f), new(-94.84f, -88.32f),
    new(-94.64f, -87.65f), new(-94.03f, -83.03f), new(-94.08f, -82.39f), new(-94.6f, -78.42f), new(-94.51f, -77.87f),
    new(-93.39f, -77.22f), new(-75.87f, -68.88f), new(-75.46f, -69.25f), new(-74.94f, -69.64f), new(-73.25f, -70.77f),
    new(-72.69f, -71.08f), new(-71.06f, -71.89f), new(-70.44f, -72.15f), new(-68.51f, -72.8f), new(-67.89f, -72.95f),
    new(-65.88f, -73.35f), new(-63.34f, -73.53f), new(-60.74f, -73.37f), new(-58.26f, -72.88f), new(-56.26f, -72.21f),
    new(-55.6f, -71.95f), new(-53.67f, -71.01f), new(-53.12f, -70.67f), new(-52.08f, -69.98f), new(-51.56f, -69.75f),
    new(-51.14f, -70.06f), new(-50.73f, -70.52f), new(-50.14f, -70.87f), new(-48.85f, -71.43f), new(-48.34f, -70.98f),
    new(-48.1f, -70.35f), new(-47.63f, -69.09f), new(-47.91f, -68.49f), new(-48.42f, -68.08f), new(-48.8f, -67.66f),
    new(-48.81f, -67.12f), new(-47.89f, -66.08f), new(-47.48f, -65.52f), new(-46.39f, -63.89f), new(-46.05f, -63.26f),
    new(-45.11f, -61.33f), new(-44.41f, -59.26f), new(-44.25f, -58.62f), new(-43.88f, -56.73f), new(-43.8f, -56.06f),
    new(-43.69f, -54.35f), new(-43.68f, -53.68f), new(-43.8f, -51.84f), new(-43.86f, -51.21f), new(-44.35f, -48.81f),
    new(-44.95f, -47.02f), new(-45.19f, -46.36f), new(-46.26f, -44.19f), new(-47.7f, -42.03f), new(-49.09f, -40.43f),
    new(-49.61f, -39.9f), new(-50.16f, -39.42f), new(-50.68f, -38.99f), new(-51.17f, -38.56f), new(-51.72f, -38.15f),
    new(-53.33f, -37.07f), new(-53.89f, -36.77f), new(-55.56f, -35.95f), new(-56.14f, -35.72f), new(-57.39f, -35.3f),
    new(-58.06f, -35.11f), new(-66.68f, -36.18f), new(-68.62f, -36.58f), new(-70.54f, -36.91f), new(-71.17f, -37.09f),
    new(-71.82f, -37.35f), new(-71.82f, -38.04f), new(-71.85f, -38.61f), new(-71.99f, -39.27f), new(-72.18f, -39.89f),
    new(-72.99f, -40.86f), new(-73.51f, -40.85f), new(-75.3f, -40.32f), new(-75.95f, -40.25f), new(-76.57f, -40.23f),
    new(-77.26f, -40.25f), new(-78.5f, -41.67f), new(-78.92f, -42.2f), new(-80.01f, -43.82f), new(-80.36f, -44.41f),
    new(-81.23f, -46.18f), new(-81.49f, -46.83f), new(-82.13f, -48.72f), new(-82.29f, -49.41f), new(-82.57f, -50.79f),
    new(-82.69f, -51.51f), new(-82.81f, -53.4f), new(-82.84f, -54.06f), new(-82.67f, -56.57f), new(-82.27f, -58.6f),
    new(-82.39f, -59.1f), new(-96.29f, -67.13f), new(-96.87f, -67.51f), new(-97.47f, -67.94f), new(-98.08f, -68.35f),
    new(-98.67f, -68.66f), new(-99.26f, -68.9f), new(-99.84f, -69.03f), new(-100.33f, -68.69f), new(-102.39f, -67.1f),
    new(-102.94f, -66.72f), new(-103.44f, -66.78f), new(-104.01f, -66.87f), new(-105.34f, -66.72f), new(-105.96f, -66.61f),
    new(-106.62f, -66.36f), new(-106.95f, -65.92f), new(-107.26f, -64.62f), new(-107.89f, -64.36f), new(-108.5f, -64.14f),
    new(-112.94f, -63.55f), new(-113.58f, -63.52f), new(-118.02f, -64.1f), new(-118.67f, -64.26f), new(-123.15f, -66.11f),
    new(-123.78f, -65.84f), new(-127.28f, -62.13f), new(-131.7f, -57.99f), new(-136.69f, -53.9f), new(-141.55f, -50.41f),
    new(-142.12f, -50.04f), new(-147.23f, -46.93f), new(-147.83f, -46.59f), new(-148.27f, -46.86f), new(-148.74f, -47.3f),
    new(-149.21f, -47.64f), new(-149.72f, -47.66f), new(-150.14f, -47.27f), new(-150.55f, -46.75f), new(-150.73f, -46.18f),
    new(-150.45f, -45.64f), new(-150.85f, -45.09f), new(-151.43f, -44.72f), new(-151.86f, -44.46f), new(-152.49f, -44.11f),
    new(-152.99f, -44.08f), new(-153.04f, -44.8f), new(-153f, -45.48f), new(-152.67f, -47.29f), new(-152.79f, -47.91f),
    new(-153.85f, -51f), new(-154.31f, -51.33f), new(-154.69f, -51.88f), new(-155.1f, -52.36f), new(-155.57f, -52.82f),
    new(-156.08f, -53.26f), new(-156.49f, -53.64f), new(-152.48f, -55.78f), new(-151.32f, -56.45f), new(-149.62f, -57.51f),
    new(-148.97f, -57.36f), new(-148.39f, -57.17f), new(-147.81f, -57.24f), new(-146.48f, -57.59f), new(-144.42f, -58.08f),
    new(-143.92f, -58.45f), new(-142.44f, -59.76f), new(-141.93f, -60.17f), new(-141.3f, -60.29f), new(-140.59f, -60.41f),
    new(-139.97f, -60.58f), new(-138.74f, -61.12f), new(-138.28f, -61.47f), new(-137.85f, -61.92f), new(-136.56f, -63.47f),
    new(-136.08f, -63.93f), new(-135.55f, -64.36f), new(-135f, -64.69f), new(-134.6f, -65.21f), new(-134.18f, -65.82f),
    new(-133.89f, -66.39f), new(-134.01f, -67.66f), new(-134.13f, -68.37f), new(-133.7f, -68.65f), new(-131.2f, -69.58f),
    new(-130.64f, -69.9f), new(-130.15f, -70.41f), new(-129.45f, -71.62f), new(-129.15f, -72.26f), new(-128.59f, -73.58f),
    new(-128.35f, -74.23f), new(-128.19f, -74.92f), new(-128.08f, -75.6f), new(-127.43f, -76.62f), new(-127.49f, -77.14f),
    new(-127.91f, -78.36f), new(-127.7f, -79.01f), new(-125.21f, -81.09f), new(-124.36f, -82.21f), new(-124.09f, -82.77f),
    new(-123.99f, -83.43f), new(-123.56f, -83.99f), new(-122.94f, -84.37f), new(-122.46f, -84.79f), new(-121.98f, -85.27f),
    new(-121.55f, -85.74f), new(-121.44f, -86.33f), new(-121.35f, -86.96f), new(-121.3f, -87.59f), new(-121.23f, -88.26f),
    new(-121.1f, -88.86f), new(-121.24f, -89.52f), new(-121.9f, -90.62f), new(-122.39f, -91.08f), new(-121.82f, -92.22f),
    new(-121.86f, -92.79f), new(-122.98f, -96.56f), new(-123.81f, -99.03f), new(-123.97f, -99.71f), new(-123.41f, -100.14f),
    new(-118.83f, -102.05f), new(-118.16f, -102.21f), new(-114.65f, -102.69f), new(-113.98f, -102.81f), new(-113.74f, -103.32f),
    new(-112.34f, -107.99f), new(-111.12f, -113.2f), new(-110.21f, -118.67f), new(-109.66f, -124.17f), new(-109.62f, -124.78f),
    new(-109.46f, -129.7f), new(-109.46f, -130.31f), new(-109.62f, -135.19f), new(-109.65f, -135.79f), new(-110.21f, -141.32f),
    new(-111.02f, -146.21f), new(-111.13f, -146.82f), new(-112.35f, -152.05f), new(-112.94f, -154.03f), new(-113f, -154.75f),
    new(-112.34f, -154.89f), new(-111.78f, -155.05f), new(-111.5f, -155.55f), new(-111.24f, -156.15f), new(-111.17f, -156.77f),
    new(-111.26f, -157.43f), new(-111.42f, -157.95f), new(-111.97f, -158.18f), new(-112.57f, -158.38f), new(-113.17f, -158.5f),
    new(-113.72f, -158.27f), new(-114.16f, -157.81f), new(-114.67f, -157.33f), new(-117.97f, -157.77f), new(-118.66f, -157.89f),
    new(-123.26f, -159.79f), new(-123.8f, -160.18f), new(-126.02f, -161.88f), new(-126.56f, -162.28f), new(-127.58f, -163.13f),
    new(-127.85f, -163.59f), new(-127.32f, -164.03f), new(-126.77f, -164.39f), new(-126.39f, -164.82f), new(-126.05f, -165.33f),
    new(-125.78f, -165.89f), new(-125.66f, -166.49f), new(-125.65f, -167.07f), new(-125.76f, -167.67f), new(-125.91f, -168.32f),
    new(-126.05f, -168.81f), new(-126.51f, -169.22f), new(-127.01f, -169.59f), new(-127.56f, -169.96f), new(-128.14f, -170.15f),
    new(-128.78f, -170.28f), new(-129.39f, -170.24f), new(-130.02f, -170.12f), new(-130.61f, -170.43f), new(-131.55f, -173.02f),
    new(-131.93f, -174.34f), new(-132.09f, -175f), new(-132.36f, -175.57f), new(-132.65f, -176.15f), new(-132.6f, -176.81f),
    new(-129.59f, -183.98f), new(-129.12f, -184.52f), new(-128.37f, -185.75f), new(-128.35f, -186.28f), new(-128.46f, -186.88f),
    new(-128.51f, -187.55f), new(-125.27f, -190.68f), new(-124.68f, -190.95f), new(-124.04f, -191.11f)];
    private static readonly WPos[] vertices2 = [new(-209.62f, -227.66f), new(-208.3f, -227.35f), new(-207.71f, -226.23f), new(-207.36f, -225.67f), new(-206.79f, -225.67f),
    new(-204.9f, -226.08f), new(-204.28f, -225.82f), new(-203.78f, -225.4f), new(-203.26f, -225.05f), new(-202.65f, -225.17f),
    new(-201.98f, -225.05f), new(-201.46f, -225.27f), new(-201.05f, -225.77f), new(-200.61f, -226.26f), new(-200.2f, -226.86f),
    new(-199.76f, -226.55f), new(-197.59f, -224.85f), new(-197.06f, -224.47f), new(-194.12f, -223.17f), new(-193.51f, -222.96f),
    new(-190.84f, -222.36f), new(-190.17f, -222.23f), new(-187.43f, -221.99f), new(-186.77f, -221.97f), new(-183.44f, -222.02f),
    new(-182.82f, -222.11f), new(-179.01f, -222.94f), new(-178.43f, -223.11f), new(-174.99f, -224.53f), new(-174.48f, -224.46f),
    new(-173.15f, -223.81f), new(-172.5f, -223.61f), new(-171.29f, -223.35f), new(-170.77f, -223.58f), new(-170.19f, -223.95f),
    new(-168.89f, -223.82f), new(-168.38f, -224f), new(-167.87f, -224.38f), new(-167.59f, -224.89f), new(-167.26f, -226.1f),
    new(-166.9f, -226.67f), new(-166.38f, -227.11f), new(-165.68f, -227.28f), new(-165.43f, -226.71f), new(-165.49f, -226.1f),
    new(-165.37f, -225.49f), new(-164.75f, -223.51f), new(-164.36f, -223.11f), new(-163.28f, -222.33f), new(-162.72f, -222f),
    new(-160.96f, -221.46f), new(-160.44f, -221.69f), new(-158.78f, -222.71f), new(-157.5f, -223.24f), new(-157.06f, -223.62f),
    new(-156.72f, -224.14f), new(-156.48f, -224.72f), new(-155.82f, -226.52f), new(-155.23f, -226.74f), new(-154.56f, -226.57f),
    new(-153.94f, -226.46f), new(-153.35f, -226.11f), new(-152.78f, -224.84f), new(-152.35f, -224.37f), new(-151.79f, -224.02f),
    new(-151.24f, -223.7f), new(-150.01f, -223.23f), new(-149.47f, -223.27f), new(-148.23f, -223.8f), new(-147.72f, -224.14f),
    new(-147.31f, -224.64f), new(-146.79f, -225.16f), new(-145.87f, -224.42f), new(-145.36f, -223.95f), new(-145.58f, -223.39f),
    new(-145.98f, -222.98f), new(-146.05f, -222.44f), new(-145.79f, -221.82f), new(-145.69f, -221.21f), new(-145.25f, -220.87f),
    new(-144.62f, -220.53f), new(-144.1f, -220.44f), new(-143.54f, -220.73f), new(-143.22f, -221.21f), new(-143f, -221.8f),
    new(-142.37f, -221.96f), new(-141.71f, -221.76f), new(-141.13f, -221.43f), new(-140.55f, -221.08f), new(-139.46f, -220.44f),
    new(-139.3f, -219.96f), new(-139.12f, -219.3f), new(-138.36f, -218.1f), new(-138f, -217.62f), new(-135.84f, -215.81f),
    new(-135.3f, -215.66f), new(-134.02f, -215.82f), new(-133.34f, -215.76f), new(-133.66f, -215.18f), new(-134.27f, -214.82f),
    new(-135.32f, -213.14f), new(-135.64f, -212.56f), new(-134.77f, -209.42f), new(-134.56f, -208.85f), new(-133.95f, -208.61f),
    new(-131.37f, -207.8f), new(-130.8f, -207.96f), new(-129.01f, -208.93f), new(-128.42f, -209.28f), new(-128.12f, -209.85f),
    new(-127.68f, -210.53f), new(-122.78f, -204.54f), new(-122.37f, -203.98f), new(-122.72f, -203.38f), new(-123.09f, -202.83f),
    new(-123.91f, -201.76f), new(-124.27f, -201.24f), new(-124.08f, -200.74f), new(-123.18f, -199.84f), new(-123.31f, -199.35f),
    new(-123.91f, -198.24f), new(-124.15f, -197.63f), new(-124.4f, -196.96f), new(-124.94f, -196.52f), new(-125.01f, -195.9f),
    new(-124.7f, -195.33f), new(-125.12f, -191.29f), new(-124.61f, -190.97f), new(-123.96f, -191.11f), new(-120.53f, -190.98f),
    new(-121.04f, -190.61f), new(-121.33f, -189.97f), new(-121.71f, -189.33f), new(-123.41f, -186.93f), new(-123.87f, -186.36f),
    new(-124.36f, -185.82f), new(-124.91f, -185.31f), new(-125.41f, -184.93f), new(-125.94f, -184.61f), new(-128.66f, -183.06f),
    new(-129.19f, -182.68f), new(-129.69f, -182.61f), new(-129.88f, -183.3f), new(-129.59f, -183.98f), new(-129.17f, -184.46f),
    new(-128.44f, -185.65f), new(-128.33f, -186.24f), new(-128.58f, -187.57f), new(-128.76f, -188.15f), new(-129.33f, -188.39f),
    new(-131.08f, -188.97f), new(-131.58f, -188.81f), new(-132.23f, -188.86f), new(-133.48f, -189.09f), new(-134.09f, -189.07f),
    new(-134.66f, -188.94f), new(-135.17f, -188.52f), new(-135.62f, -188.27f), new(-136.54f, -189.28f), new(-137.01f, -189.71f),
    new(-138.27f, -190.04f), new(-138.92f, -189.97f), new(-139.61f, -189.99f), new(-140.11f, -190.47f), new(-140.57f, -190.98f),
    new(-140.42f, -191.66f), new(-140.34f, -192.3f), new(-140.11f, -193.53f), new(-140.17f, -194.14f), new(-140.56f, -194.58f),
    new(-142.91f, -196.88f), new(-143.45f, -197.21f), new(-145.98f, -198.14f), new(-146.6f, -198.17f), new(-147.21f, -198.18f),
    new(-147.91f, -198.24f), new(-149.07f, -199.03f), new(-150.9f, -199.31f), new(-151.48f, -199.28f), new(-152.02f, -198.94f),
    new(-152.64f, -199.25f), new(-153.65f, -200f), new(-154.18f, -200.36f), new(-156.45f, -201.85f), new(-157.05f, -202.2f),
    new(-158.34f, -202.79f), new(-158.91f, -202.97f), new(-160.13f, -203.32f), new(-162.03f, -203.75f), new(-162.55f, -204.23f),
    new(-163.47f, -205.11f), new(-163.97f, -205.51f), new(-165.02f, -206.3f), new(-165.52f, -206.72f), new(-166.73f, -207.24f),
    new(-167.33f, -207.48f), new(-168.66f, -207.72f), new(-169.19f, -208.12f), new(-170.1f, -208.98f), new(-170.63f, -209.31f),
    new(-171.75f, -209.96f), new(-173.4f, -210.84f), new(-174.02f, -211.02f), new(-175.35f, -211.23f), new(-177.23f, -211.69f),
    new(-178.53f, -211.87f), new(-179.15f, -211.92f), new(-180.43f, -211.91f), new(-181.03f, -211.86f), new(-182.25f, -211.65f),
    new(-182.92f, -211.47f), new(-184.1f, -211.06f), new(-184.71f, -210.78f), new(-189.23f, -208.56f), new(-189.85f, -208.3f),
    new(-191.79f, -207.6f), new(-192.42f, -207.39f), new(-193.03f, -207.15f), new(-193.46f, -206.82f), new(-194.01f, -205.55f),
    new(-194.52f, -205.48f), new(-195.11f, -205.76f), new(-195.68f, -206.02f), new(-196.26f, -205.93f), new(-197.56f, -205.63f),
    new(-198.13f, -205.41f), new(-199.28f, -204.81f), new(-199.82f, -204.5f), new(-200.02f, -203.93f), new(-200.5f, -202.1f),
    new(-200.64f, -201.46f), new(-200.85f, -200.82f), new(-201.14f, -200.16f), new(-201.46f, -199.56f), new(-201.8f, -199.06f),
    new(-201.74f, -198.47f), new(-201.62f, -197.85f), new(-201.55f, -197.14f), new(-201.79f, -196.54f), new(-202.01f, -195.96f),
    new(-202.15f, -195.34f), new(-202.06f, -194.77f), new(-201.77f, -194.22f), new(-201.22f, -193.97f), new(-200.75f, -193.73f),
    new(-200.97f, -193.19f), new(-201.27f, -192.6f), new(-201.89f, -192.52f), new(-203.1f, -192.93f), new(-203.65f, -192.57f),
    new(-204f, -192.02f), new(-204.42f, -191.49f), new(-205.03f, -191.28f), new(-205.56f, -190.92f), new(-206.02f, -190.48f),
    new(-206.39f, -189.95f), new(-206.71f, -189.4f), new(-206.82f, -188.83f), new(-206.45f, -188.33f), new(-206.22f, -187.7f),
    new(-205.74f, -187.35f), new(-205.47f, -186.68f), new(-205.81f, -186.31f), new(-206.24f, -186.05f), new(-206.86f, -185.69f),
    new(-207.41f, -185.35f), new(-208.11f, -185.08f), new(-208.51f, -186.13f), new(-208.88f, -186.71f), new(-209.66f, -187.78f),
    new(-210.21f, -188.12f), new(-210.65f, -188.57f), new(-211.29f, -188.72f), new(-211.91f, -188.91f), new(-212.49f, -189.24f),
    new(-213.06f, -189.41f), new(-215.39f, -188.17f), new(-215.91f, -187.76f), new(-217.16f, -187.13f), new(-217.51f, -186.73f),
    new(-217.81f, -185.35f), new(-217.76f, -184.7f), new(-217.6f, -184.01f), new(-217.4f, -183.39f), new(-217.09f, -182.81f),
    new(-216.73f, -182.09f), new(-217.37f, -182.27f), new(-218.68f, -182.16f), new(-219.27f, -181.99f), new(-219.78f, -181.58f),
    new(-220.17f, -180.8f), new(-220.47f, -181.26f), new(-220.96f, -181.63f), new(-221.57f, -181.51f), new(-222.02f, -181.17f),
    new(-222.52f, -180.69f), new(-223.8f, -180.55f), new(-224.42f, -180.42f), new(-224.89f, -179.97f), new(-225.34f, -179.47f),
    new(-225.75f, -179f), new(-225.85f, -178.35f), new(-226.41f, -178.24f), new(-227.03f, -178.41f), new(-227.63f, -178.69f),
    new(-228.79f, -180.25f), new(-230.65f, -180.79f), new(-231.19f, -180.61f), new(-231.75f, -180.33f), new(-232.41f, -180.21f),
    new(-236.33f, -182.57f), new(-236.59f, -183.12f), new(-238.98f, -186.32f), new(-239.49f, -186.58f), new(-240.77f, -186.95f),
    new(-240.95f, -187.44f), new(-240.89f, -188.05f), new(-240.89f, -188.64f), new(-241.14f, -189.31f), new(-240.35f, -191.1f),
    new(-240.39f, -191.71f), new(-240.38f, -192.42f), new(-239.8f, -192.75f), new(-239.44f, -193.22f), new(-238.97f, -193.62f),
    new(-238.62f, -194.07f), new(-238.3f, -194.63f), new(-238.18f, -195.22f), new(-238.13f, -197.14f), new(-238.42f, -197.67f),
    new(-238.87f, -198.18f), new(-239.29f, -198.72f), new(-239.35f, -199.4f), new(-239.65f, -199.96f), new(-240.64f, -201.01f),
    new(-241.13f, -201.36f), new(-241.84f, -201.51f), new(-241.9f, -202.04f), new(-242.18f, -203.95f), new(-242.22f, -204.61f),
    new(-242.32f, -206.64f), new(-241.77f, -207.09f), new(-241.24f, -207.43f), new(-239.05f, -209.06f), new(-238.56f, -209.46f),
    new(-236.28f, -211.71f), new(-233.94f, -214.54f), new(-233.59f, -215.12f), new(-231.93f, -218.2f), new(-231.64f, -218.81f),
    new(-230.03f, -223.73f), new(-229.79f, -224.35f), new(-229.36f, -224.88f), new(-228.79f, -225.18f), new(-228.28f, -225.58f),
    new(-227.86f, -226f), new(-227.39f, -224.88f), new(-227.08f, -224.28f), new(-226.43f, -223.14f), new(-226.17f, -222.54f),
    new(-226.02f, -221.9f), new(-225.83f, -221.36f), new(-225.27f, -221.1f), new(-224.76f, -220.82f), new(-221.52f, -219.81f),
    new(-220.93f, -219.82f), new(-217.57f, -220.65f), new(-217.14f, -221.09f), new(-215.51f, -224.17f), new(-215.4f, -224.75f),
    new(-215.33f, -225.37f), new(-215.13f, -226.07f), new(-212.82f, -225.22f), new(-212.3f, -225.24f), new(-211.91f, -225.71f),
    new(-211.14f, -226.73f), new(-210.65f, -227.23f), new(-210.09f, -227.58f)];
    private static readonly WPos[] vertices3 = [new(-237.07f, -188.28f), new(-236.44f, -188.22f), new(-235.81f, -188.08f), new(-235.23f, -187.88f), new(-234.66f, -187.65f),
    new(-234.1f, -187.39f), new(-233.56f, -187.12f), new(-233.02f, -186.84f), new(-232.48f, -186.53f), new(-231.97f, -186.17f),
    new(-230.49f, -185.03f), new(-230.02f, -184.59f), new(-229.65f, -184.05f), new(-228.34f, -181.76f), new(-228.05f, -181.11f),
    new(-227.85f, -180.42f), new(-227.66f, -179.73f), new(-228.05f, -179.39f), new(-228.42f, -179.73f), new(-228.81f, -180.23f),
    new(-229.46f, -180.45f), new(-230.15f, -180.66f), new(-230.71f, -180.8f), new(-231.27f, -180.56f), new(-232.44f, -180f),
    new(-232.81f, -179.6f), new(-232.93f, -177.61f), new(-232.87f, -177.03f), new(-231.36f, -175.95f), new(-230.94f, -175.38f),
    new(-230.59f, -174.85f), new(-230.94f, -172.88f), new(-231.09f, -172.19f), new(-231.62f, -172.21f), new(-232.77f, -172.71f),
    new(-233.37f, -172.87f), new(-233.89f, -172.59f), new(-233.98f, -172.08f), new(-233.99f, -171.48f), new(-234.4f, -170.94f),
    new(-235.06f, -170.74f), new(-235.75f, -170.66f), new(-236.17f, -170.35f), new(-236.5f, -169.78f), new(-236.86f, -169.35f),
    new(-236.74f, -168.75f), new(-236.47f, -168.22f), new(-236.16f, -167.7f), new(-235.67f, -167.29f), new(-234.84f, -166.25f),
    new(-235.4f, -165.09f), new(-236.07f, -164.03f), new(-236.43f, -163.5f), new(-236.96f, -163.08f), new(-237.89f, -163.08f),
    new(-238.46f, -162.97f), new(-239.39f, -162.02f), new(-239.76f, -161.48f), new(-240.41f, -161.53f), new(-246.96f, -165.51f),
    new(-247.51f, -165.89f), new(-249.08f, -167.13f), new(-249.46f, -167.69f), new(-249.5f, -168.36f), new(-249.43f, -169.04f),
    new(-249.28f, -169.67f), new(-248.78f, -170.91f), new(-247.87f, -173.36f), new(-247.62f, -174.01f), new(-247.01f, -175.16f),
    new(-245.37f, -177.45f), new(-245.13f, -177.98f), new(-245.06f, -179.29f), new(-244.59f, -179.75f), new(-244.09f, -180.1f),
    new(-243.22f, -180.99f), new(-242.98f, -181.55f), new(-242.64f, -182.15f), new(-241.96f, -182.38f), new(-241.53f, -182.11f),
    new(-240.98f, -181.8f), new(-239.16f, -182.08f), new(-238.49f, -182.06f), new(-237.88f, -182.05f), new(-237.28f, -182.25f),
    new(-236.7f, -182.51f), new(-236.49f, -182.99f), new(-238.86f, -186.15f), new(-239.33f, -186.53f), new(-239.95f, -186.72f),
    new(-240.28f, -187.27f), new(-239.66f, -187.44f), new(-239.15f, -187.85f), new(-238.55f, -188.12f), new(-237.92f, -188.25f),
    new(-237.28f, -188.29f)];
    public static readonly ArenaBoundsCustom Arena1 = new([new PolygonCustom(vertices1)]);
    public static readonly ArenaBoundsCustom Arena1B = new([new PolygonCustom(vertices1), new PolygonCustom(vertices2)]);
    public static readonly ArenaBoundsCustom Arena2 = new([new PolygonCustom(vertices2)]);
    public static readonly ArenaBoundsCustom Arena2B = new([new PolygonCustom(vertices2), new PolygonCustom(vertices3)]);

    private static readonly uint[] trash = [(uint)OID.Boss, (uint)OID.GardenBeeCloud, (uint)OID.WorkerHawk, (uint)OID.ArboretumCrawler,
    (uint)OID.SoldierHawk, (uint)OID.ThavnairianTortoise, (uint)OID.OrnHornet];
    private static readonly uint[] combs = [(uint)OID.Honeycomb1, (uint)OID.Honeycomb2];
    public static readonly uint[] All = [.. trash, .. combs];

    protected override bool CheckPull() => IsAnyActorInCombat(All);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        var m = this;
        Arena.Actors(m, trash);
        Arena.Actors(m, combs, Colors.Object);
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
