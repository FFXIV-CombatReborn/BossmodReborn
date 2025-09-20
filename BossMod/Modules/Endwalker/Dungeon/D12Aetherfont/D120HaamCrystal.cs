namespace BossMod.Endwalker.Dungeon.D12Aetherfont.D120HaamCrystal;

public enum OID : uint
{
    Boss = 0x3ECD, // R5.8
    HaamAuk = 0x3ECA, // R1.25
    HaamGolem = 0x3ECE // R2.2
}

public enum AID : uint
{
    AutoAttack1 = 872, // HaamGolem/Boss->player, no cast, single-target
    AutoAttack2 = 870, // HaamAuk->player, no cast, single-target

    EarthenHeart = 33993, // HaamGolem->location, 3.0s cast, range 6 circle
    HardHead = 33995, // Boss->self, 3.0s cast, range 12 120-degree cone
}

class HardHead(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HardHead, new AOEShapeCone(12f, 60f.Degrees()));
class EarthenHeart(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EarthenHeart, 6f);

class D120HaamCrystalStates : StateMachineBuilder
{
    public D120HaamCrystalStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HardHead>()
            .ActivateOnEnter<EarthenHeart>()
            .Raw.Update = () => AllDeadOrDestroyedInBounds(D120HaamCrystal.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 822, NameID = 12345, SortOrder = 5)]
public class D120HaamCrystal(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(386.06f, -159.30f), new(393.24f, -152.38f), new(393.58f, -151.98f), new(393.76f, -147.86f), new(393.42f, -147.44f),
    new(392.94f, -147.21f), new(392.53f, -146.89f), new(392.01f, -145.94f), new(392.01f, -145.39f), new(392.24f, -144.89f),
    new(392.66f, -144.57f), new(393.20f, -144.69f), new(393.68f, -144.86f), new(394.17f, -145.08f), new(394.56f, -145.43f),
    new(394.73f, -145.91f), new(395.04f, -146.34f), new(396.21f, -147.40f), new(396.69f, -147.60f), new(398.26f, -147.75f),
    new(399.52f, -146.85f), new(399.82f, -146.44f), new(400.38f, -144.42f), new(400.26f, -143.93f), new(400.43f, -143.43f),
    new(400.72f, -142.99f), new(401.13f, -142.68f), new(401.65f, -142.55f), new(402.11f, -142.84f), new(402.51f, -143.14f),
    new(402.67f, -143.64f), new(402.74f, -144.16f), new(402.55f, -144.64f), new(402.78f, -145.13f), new(403.73f, -146.44f),
    new(404.06f, -146.82f), new(404.54f, -146.60f), new(408.12f, -142.83f), new(412.89f, -133.97f), new(412.12f, -132.07f),
    new(411.99f, -131.55f), new(412.08f, -128.36f), new(411.91f, -127.88f), new(411.57f, -127.47f), new(410.74f, -126.08f),
    new(410.76f, -125.57f), new(410.99f, -125.12f), new(413.18f, -122.21f), new(413.25f, -121.67f), new(412.43f, -119.73f),
    new(412.38f, -119.23f), new(412.40f, -118.17f), new(411.60f, -115.64f), new(411.19f, -113.16f), new(411.20f, -111.06f),
    new(411.15f, -110.54f), new(411.01f, -110.04f), new(410.28f, -109.28f), new(409.93f, -108.84f), new(409.99f, -107.33f),
    new(409.91f, -106.79f), new(408.91f, -105.53f), new(408.15f, -104.11f), new(408.16f, -103.59f), new(408.40f, -102.57f),
    new(408.58f, -102.11f), new(408.82f, -101.65f), new(409.00f, -101.17f), new(408.92f, -100.11f), new(408.82f, -99.56f),
    new(407.77f, -98.40f), new(407.52f, -97.35f), new(407.64f, -96.85f), new(408.12f, -95.93f), new(409.29f, -94.23f),
    new(410.21f, -93.73f), new(410.33f, -92.69f), new(410.87f, -91.82f), new(412.35f, -83.18f), new(412.22f, -82.67f),
    new(411.73f, -82.45f), new(411.56f, -81.96f), new(411.64f, -81.44f), new(411.55f, -80.94f), new(411.86f, -79.96f),
    new(412.08f, -79.48f), new(412.58f, -79.44f), new(412.93f, -79.06f), new(413.18f, -78.62f), new(413.57f, -76.07f),
    new(413.55f, -75.57f), new(412.97f, -74.63f), new(412.79f, -74.14f), new(412.86f, -73.63f), new(412.61f, -73.17f),
    new(412.77f, -72.66f), new(412.97f, -72.15f), new(413.22f, -71.66f), new(413.71f, -71.52f), new(414.21f, -71.60f),
    new(414.81f, -66.43f), new(414.30f, -66.41f), new(412.73f, -66.77f), new(412.24f, -66.92f), new(410.29f, -67.69f),
    new(408.27f, -68.14f), new(407.74f, -68.11f), new(406.25f, -67.85f), new(404.69f, -67.77f), new(404.15f, -67.67f),
    new(402.64f, -67.25f), new(399.34f, -65.72f), new(398.82f, -65.69f), new(377.37f, -72.80f), new(350.62f, -81.87f),
    new(350.82f, -82.34f), new(352.24f, -83.82f), new(352.65f, -84.15f), new(354.01f, -85.67f), new(357.57f, -92.37f),
    new(357.73f, -92.88f), new(358.25f, -95.57f), new(358.29f, -96.09f), new(358.23f, -99.21f), new(358.31f, -99.72f),
    new(359.04f, -102.90f), new(363.28f, -112.69f), new(363.42f, -113.21f), new(364.97f, -123.01f), new(365.01f, -123.52f),
    new(364.16f, -135.36f), new(365.73f, -142.49f), new(365.94f, -142.94f), new(367.31f, -145.36f), new(368.34f, -148.33f),
    new(368.58f, -148.79f), new(370.45f, -150.68f), new(370.88f, -151.01f), new(374.69f, -152.67f), new(375.14f, -152.94f),
    new(378.38f, -156.37f), new(380.83f, -158.27f), new(381.28f, -158.55f), new(381.69f, -158.87f), new(382.20f, -158.91f),
    new(383.75f, -158.84f), new(385.77f, -159.32f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.HaamGolem, (uint)OID.HaamAuk];

    protected override bool CheckPull() => IsAnyActorInBoundsInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Trash);
    }
}
