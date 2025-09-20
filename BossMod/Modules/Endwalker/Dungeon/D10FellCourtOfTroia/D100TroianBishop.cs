namespace BossMod.Endwalker.Dungeon.D10FellCourtOfTroia.D100TroianBishop;

public enum OID : uint
{
    Boss = 0x3983, // R2.52,
    TroianKnight = 0x3982, // R3.15
    TroianHound = 0x3980 // R3.06
}

public enum AID : uint
{
    AutoAttack = 6497, // TroianHound/TroianKnight/Boss->player, no cast, single-target

    HallOfSorrow = 30216, // TroianKnight->location, 4.0s cast, range 8 circle
    JestersReap = 30217 // Boss->self, 4.0s cast, range 12 120-degree cone
}

sealed class JestersReap(BossModule module) : Components.SimpleAOEs(module, (uint)AID.JestersReap, new AOEShapeCone(12f, 60f.Degrees()));
sealed class HallOfSorrow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HallOfSorrow, 8f);

sealed class D100TroianBishopStates : StateMachineBuilder
{
    public D100TroianBishopStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HallOfSorrow>()
            .ActivateOnEnter<JestersReap>()
            .Raw.Update = () => AllDeadOrDestroyedInBounds(D100TroianBishop.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 869, NameID = 11364, SortOrder = 4)]
public sealed class D100TroianBishop(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(6.22f, -91.79f), new(6.05f, -91.29f), new(6.02f, -89.05f), new(6.22f, -88.57f), new(8.33f, -88.52f),
    new(8.54f, -87.92f), new(8.54f, -87.33f), new(8.63f, -86.75f), new(10.74f, -86.49f), new(11.33f, -86.38f),
    new(11.91f, -86.44f), new(12.21f, -86.91f), new(12.21f, -89.02f), new(12.93f, -89.1f), new(18.11f, -89.08f),
    new(18.47f, -88.55f), new(18.95f, -85.64f), new(18.95f, -80.39f), new(18.4f, -79.91f), new(18.22f, -77.46f),
    new(18.46f, -77.02f), new(19.1f, -77.02f), new(19.36f, -74.61f), new(18.84f, -74.48f), new(18.3f, -74.31f),
    new(18.22f, -71.93f), new(18.68f, -71.51f), new(19.37f, -71.39f), new(19.37f, -70.86f), new(19.53f, -70.37f),
    new(21.35f, -70.28f), new(21.66f, -69.71f), new(21.71f, -69.09f), new(21.61f, -68.6f), new(21.01f, -68.31f),
    new(20.43f, -68.06f), new(20.07f, -67.64f), new(20.18f, -67.1f), new(20.49f, -66.55f), new(20.62f, -66.05f),
    new(20.16f, -65.84f), new(19.53f, -65.61f), new(19.16f, -65.22f), new(19.1f, -64.66f), new(19.09f, -64f),
    new(18.97f, -63.34f), new(17.86f, -62.55f), new(17.53f, -62.11f), new(17.86f, -61.63f), new(17.91f, -60.97f),
    new(17.88f, -60.34f), new(17.58f, -59.75f), new(17.29f, -59.14f), new(16.98f, -58.71f), new(16.32f, -58.9f),
    new(12.12f, -59.9f), new(7.44f, -60.66f), new(6.81f, -60.73f), new(6.26f, -61.02f), new(5.63f, -61.2f),
    new(5.01f, -60.99f), new(4.36f, -60.8f), new(3.69f, -61.06f), new(3.12f, -61.36f), new(2.53f, -61.28f),
    new(1.94f, -61.03f), new(1.29f, -60.8f), new(0.71f, -61.2f), new(0.28f, -61.76f), new(-0.24f, -61.97f),
    new(-0.73f, -61.73f), new(-1.13f, -61.25f), new(-1.62f, -60.69f), new(-2.56f, -61.48f), new(-3.08f, -61.64f),
    new(-3.62f, -61.4f), new(-4.18f, -61.07f), new(-4.85f, -61.01f), new(-5.49f, -61.2f), new(-6.01f, -61.09f),
    new(-6.61f, -60.76f), new(-7.26f, -60.68f), new(-10.31f, -60.24f), new(-14.95f, -59.26f), new(-16.3f, -58.9f),
    new(-17.63f, -58.52f), new(-22.17f, -56.95f), new(-23.42f, -56.45f), new(-24.64f, -55.93f), new(-27.51f, -54.57f),
    new(-31.45f, -52.4f), new(-32.55f, -51.72f), new(-35.18f, -49.97f), new(-37.63f, -48.15f), new(-38.63f, -47.35f),
    new(-41.1f, -45.22f), new(-42.04f, -44.35f), new(-44.22f, -42.17f), new(-45.11f, -41.22f), new(-45.72f, -40.87f),
    new(-46.2f, -41.31f), new(-46.72f, -41.28f), new(-47.14f, -40.83f), new(-47.56f, -40.35f), new(-47.36f, -39.79f),
    new(-46.82f, -39.28f), new(-49.08f, -36.42f), new(-50.88f, -33.87f), new(-51.55f, -32.82f), new(-53.2f, -30.07f),
    new(-53.84f, -28.92f), new(-55.87f, -24.77f), new(-56.39f, -23.57f), new(-57.57f, -20.51f), new(-58f, -19.27f),
    new(-58.93f, -16.18f), new(-59.26f, -14.93f), new(-59.96f, -11.81f), new(-60.2f, -10.54f), new(-60.67f, -7.31f),
    new(-60.81f, -6.04f), new(-61.04f, -2.92f), new(-61.09f, -1.7f), new(-61.03f, 3.04f), new(-60.99f, 3.71f),
    new(-60.98f, 4.39f), new(-60.93f, 5.03f), new(-61.12f, 5.69f), new(-64.73f, 5.69f), new(-65.36f, 5.54f),
    new(-65.97f, 5.53f), new(-66.57f, 5.59f), new(-72.65f, 5.69f), new(-74.21f, 5.66f), new(-74.41f, 5f),
    new(-74.17f, 4.36f), new(-73.92f, 3.78f), new(-73.99f, 3.15f), new(-73.98f, 2.43f), new(-74.15f, 1.9f),
    new(-77.33f, 1.82f), new(-77.8f, 2.27f), new(-78.4f, 2.54f), new(-79.12f, 2.63f), new(-79.82f, 2.68f),
    new(-80.35f, 2.9f), new(-80.42f, 4.1f), new(-80.21f, 4.56f), new(-79.33f, 4.62f), new(-79.19f, 5.11f),
    new(-78.61f, 5.46f), new(-78.61f, 10.51f), new(-79.05f, 10.76f), new(-79.36f, 12.73f), new(-79.06f, 13.23f),
    new(-78.61f, 18.38f), new(-79.1f, 18.79f), new(-79.33f, 19.37f), new(-84.48f, 19.37f), new(-84.85f, 18.82f),
    new(-86.7f, 18.72f), new(-87.77f, 18.58f), new(-91.38f, 18.57f), new(-92f, 18.6f), new(-92.64f, 18.58f),
    new(-93.22f, 18.71f), new(-94.63f, 18.71f), new(-95.24f, 18.58f), new(-98.55f, 18.57f),
    new(-96.56f, -4f), new(-96.77f, -4.57f), new(-97f, -5.12f), new(-97.35f, -5.67f), new(-97.6f, -6.2f),
    new(-98.05f, -6.64f), new(-98.32f, -7.18f), new(-99.05f, -7.37f), new(-99.34f, -7.99f), new(-99.58f, -8.56f),
    new(-99.79f, -9.14f), new(-100.18f, -10.34f), new(-100.39f, -11.01f), new(-100.57f, -11.73f), new(-100.73f, -12.44f),
    new(-100.86f, -13.17f), new(-100.96f, -13.86f), new(-101.09f, -15.09f), new(-101.14f, -15.72f), new(-101.17f, -16.38f),
    new(-101.15f, -17.03f), new(-101.07f, -17.68f), new(-100.94f, -18.33f), new(-100.75f, -18.99f), new(-100.18f, -19.28f),
    new(-95.76f, -19.28f), new(-95.19f, -18.87f), new(-94.64f, -18.73f), new(-94.01f, -18.72f), new(-93.41f, -18.62f),
    new(-92.82f, -18.86f), new(-87.56f, -19.27f), new(-87.11f, -18.81f), new(-85.29f, -18.68f), new(-84.77f, -18.94f),
    new(-81.58f, -19.39f), new(-81.33f, -16.66f), new(-81.32f, -16.01f), new(-81.33f, -15.36f), new(-81.54f, -14.67f),
    new(-81.8f, -14.02f), new(-81.62f, -13.42f), new(-81.34f, -12.76f), new(-80.93f, -12.43f), new(-80.29f, -12.43f),
    new(-79.64f, -12.41f), new(-79.38f, -11.98f), new(-79.4f, -11.28f), new(-79.09f, -10.8f), new(-78.6f, -8.99f),
    new(-78.61f, -8.26f), new(-78.64f, -7.73f), new(-79.17f, -7.24f), new(-79.4f, -6.68f), new(-79.57f, -5.95f),
    new(-80.09f, -5.66f), new(-80.47f, -5.23f), new(-80.67f, -4.68f), new(-80.82f, -4.04f), new(-80.57f, -3.44f),
    new(-79.77f, -3.29f), new(-79.21f, -2.91f), new(-78.68f, -2.69f), new(-78.1f, -2.53f), new(-77.73f, -2.02f),
    new(-74.57f, -1.82f), new(-74.06f, -1.99f), new(-73.98f, -3.04f), new(-73.87f, -3.78f), new(-73.34f, -3.76f),
    new(-72.71f, -3.83f), new(-72.51f, -4.33f), new(-72.32f, -7.53f), new(-72.56f, -8.04f), new(-73.17f, -8.13f),
    new(-73.83f, -8.17f), new(-74.07f, -8.8f), new(-73.92f, -9.3f), new(-73.66f, -9.89f), new(-73.34f, -10.45f),
    new(-73.19f, -11.01f), new(-73.37f, -11.6f), new(-73.53f, -12.33f), new(-73.04f, -12.41f), new(-72.44f, -12.64f),
    new(-71.95f, -13.03f), new(-71.56f, -13.58f), new(-71.31f, -14.19f), new(-71.29f, -14.77f), new(-71.42f, -15.39f),
    new(-71.66f, -15.93f), new(-71.6f, -16.6f), new(-71.1f, -17.11f), new(-70.73f, -17.62f), new(-70.2f, -17.94f),
    new(-69.92f, -18.35f), new(-69.64f, -20.31f), new(-69.4f, -20.76f), new(-69.35f, -21.37f), new(-69.53f, -22.06f),
    new(-69.48f, -22.72f), new(-69.26f, -23.4f), new(-69.05f, -23.99f), new(-69.2f, -24.68f), new(-69.61f, -25.13f),
    new(-69.72f, -25.75f), new(-69.59f, -26.42f), new(-68.91f, -26.45f), new(-68.31f, -26.64f), new(-67.78f, -27.02f),
    new(-67.37f, -27.51f), new(-67.11f, -28.06f), new(-67f, -28.68f), new(-67.1f, -29.34f), new(-66.73f, -29.69f),
    new(-66.47f, -31.31f), new(-66.12f, -31.88f), new(-65.74f, -32.41f), new(-65.48f, -32.92f), new(-65.79f, -33.33f),
    new(-66.31f, -33.72f), new(-66.2f, -34.4f), new(-65.02f, -36.58f), new(-64.68f, -37.14f), new(-64.16f, -37.54f),
    new(-63.55f, -37.83f), new(-62.98f, -37.7f), new(-62.39f, -37.98f), new(-61.81f, -38.3f), new(-61.72f, -38.94f),
    new(-61.45f, -39.56f), new(-60.91f, -39.94f), new(-60.49f, -40.39f), new(-60.2f, -40.97f), new(-60.08f, -41.54f),
    new(-60.14f, -42.13f), new(-60.33f, -42.71f), new(-60.76f, -43.32f), new(-60.26f, -43.58f), new(-59.66f, -43.84f),
    new(-59.31f, -44.29f), new(-59.25f, -44.93f), new(-58.85f, -45.41f), new(-58.69f, -46.04f), new(-56.52f, -48.68f),
    new(-56.09f, -49.17f), new(-55.48f, -49.05f), new(-54.99f, -48.62f), new(-54.46f, -48.39f), new(-54.01f, -48.76f),
    new(-53.56f, -49.29f), new(-53.44f, -49.78f), new(-53.86f, -50.21f), new(-54.4f, -50.66f), new(-54.17f, -51.31f),
    new(-53.8f, -51.91f), new(-52.43f, -51.84f), new(-51.93f, -52.05f), new(-51.79f, -52.59f), new(-51.81f, -53.75f),
    new(-50.61f, -53.56f), new(-50.06f, -53.65f), new(-49.71f, -54.12f), new(-49.08f, -55.24f), new(-48.37f, -55.32f),
    new(-47.67f, -55.28f), new(-47.17f, -55.46f), new(-47.02f, -56f), new(-47.16f, -56.61f), new(-47.24f, -57.23f),
    new(-47.02f, -57.85f), new(-46.54f, -58.29f), new(-46.05f, -58.69f), new(-45.41f, -58.88f), new(-44.9f, -59.24f),
    new(-44.26f, -59.32f), new(-43.83f, -59.68f), new(-43.67f, -60.25f), new(-43.37f, -60.68f), new(-42.92f, -60.45f),
    new(-42.35f, -60.18f), new(-41.77f, -60.09f), new(-41.16f, -60.15f), new(-40.6f, -60.36f), new(-40.09f, -60.73f),
    new(-39.7f, -61.22f), new(-39.48f, -61.77f), new(-39.38f, -62.4f), new(-39.5f, -63.05f), new(-39.08f, -63.32f),
    new(-38.45f, -63.33f), new(-37.93f, -63.53f), new(-37.6f, -64.03f), new(-37.28f, -64.61f), new(-36.71f, -64.94f),
    new(-33.24f, -66.78f), new(-32.57f, -66.88f), new(-31.96f, -66.8f), new(-31.43f, -67.03f), new(-31.1f, -67.49f),
    new(-30.58f, -68.02f), new(-30.25f, -67.63f), new(-29.73f, -67.28f), new(-29.16f, -67.05f), new(-28.56f, -67.03f),
    new(-27.94f, -67.15f), new(-27.39f, -67.42f), new(-26.95f, -67.85f), new(-26.64f, -68.36f), new(-26.46f, -68.95f),
    new(-26.44f, -69.6f), new(-25.75f, -69.72f), new(-25.11f, -69.58f), new(-24.53f, -69.91f), new(-24.28f, -70.44f),
    new(-23.68f, -70.75f), new(-23.07f, -70.95f), new(-22.38f, -70.9f), new(-22.16f, -70.37f), new(-19.91f, -70.3f),
    new(-19.4f, -70.53f), new(-18.9f, -71.04f), new(-18.4f, -71.56f), new(-18.22f, -72.76f), new(-17.98f, -73.35f),
    new(-18.11f, -74.03f), new(-18.41f, -74.44f), new(-18.98f, -74.48f), new(-19.37f, -76.71f), new(-18.75f, -77.02f),
    new(-18.25f, -77.25f), new(-18.22f, -77.84f), new(-18.22f, -79.39f), new(-18.37f, -79.9f), new(-19.01f, -79.99f),
    new(-19f, -87.57f), new(-18.96f, -88.46f), new(-18.45f, -88.56f), new(-18.07f, -89.09f), new(-12.3f, -89.09f),
    new(-12.2f, -88.55f), new(-12.29f, -87.81f), new(-12.24f, -87.09f), new(-12f, -86.56f), new(-10.91f, -86.51f),
    new(-10.33f, -86.31f), new(-8.92f, -86.52f), new(-8.59f, -86.94f), new(-8.59f, -88.23f), new(-7.98f, -88.42f),
    new(-7.46f, -88.27f), new(-6.29f, -88.55f), new(-6.02f, -90.98f), new(-6.31f, -91.49f), new(6.22f, -91.79f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.TroianKnight, (uint)OID.TroianHound];

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
