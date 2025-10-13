namespace BossMod.Shadowbringers.TreasureHunt.DungeonsOfLyheGhiah;

public abstract class DungeonsOfLyheGhiahRoom5 : BossModule
{
    public DungeonsOfLyheGhiahRoom5(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private DungeonsOfLyheGhiahRoom5(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        WPos[] vertices = [new(14.82f, -416.43f), new(15.44f, -416.52f), new(15.93f, -416.35f), new(16.07f, -415.68f), new(16.5f, -415.19f),
        new(16.96f, -414.83f), new(17.51f, -414.6f), new(18.1f, -414.65f), new(18.68f, -414.76f), new(19.2f, -415.17f),
        new(19.58f, -415.61f), new(20.2f, -415.48f), new(25.7f, -409.9f), new(25.42f, -409.38f), new(24.96f, -408.85f),
        new(24.7f, -408.28f), new(24.59f, -407.65f), new(24.77f, -407.03f), new(25.06f, -406.47f), new(25.53f, -406.09f),
        new(26.12f, -405.86f), new(26.31f, -405.2f), new(26.42f, -404.55f), new(26.82f, -403.14f), new(26.89f, -402.41f),
        new(26.23f, -402.21f), new(25.56f, -402.16f), new(24.96f, -402.45f), new(23.87f, -403.13f), new(23.28f, -403.4f),
        new(22.6f, -403.55f), new(21.94f, -403.64f), new(20.7f, -403.52f), new(20.06f, -403.35f), new(19.47f, -403.15f),
        new(18.28f, -402.6f), new(16.62f, -401.59f), new(16.22f, -401.18f), new(16.43f, -400.63f), new(17.95f, -397.79f),
        new(18.19f, -397.2f), new(19.1f, -394.21f), new(19.24f, -393.55f), new(19.57f, -390.25f), new(19.22f, -386.22f),
        new(18.08f, -382.45f), new(16.2f, -379.01f), new(13.69f, -376.02f), new(11.09f, -373.88f), new(10.54f, -373.53f),
        new(4.02f, -370.55f), new(-3.7f, -370.53f), new(-4.26f, -370.63f), new(-7.46f, -371.88f), new(-10.85f, -373.69f),
        new(-13.99f, -376.31f), new(-16.19f, -378.99f), new(-18.17f, -382.71f), new(-19.22f, -386.21f), new(-19.58f, -390.05f),
        new(-19.18f, -393.95f), new(-18.2f, -397.19f), new(-17.93f, -397.82f), new(-16.9f, -399.73f), new(-16.4f, -400.11f),
        new(-19.26f, -403.06f), new(-19.84f, -403.32f), new(-20.51f, -403.49f), new(-21.14f, -403.59f), new(-21.81f, -403.63f),
        new(-22.45f, -403.58f), new(-23.15f, -403.43f), new(-23.74f, -403.23f), new(-25.37f, -402.2f), new(-26.07f, -402.17f),
        new(-26.73f, -402.35f), new(-26.88f, -402.99f), new(-26.35f, -404.91f), new(-26.43f, -405.51f), new(-25.94f, -405.9f),
        new(-25.4f, -406.21f), new(-24.91f, -406.65f), new(-24.69f, -407.24f), new(-24.6f, -407.86f), new(-24.71f, -408.52f),
        new(-25.06f, -409), new(-25.63f, -410), new(-19.96f, -415.7f), new(-19.46f, -415.43f), new(-19.01f, -415.03f),
        new(-18.48f, -414.74f), new(-17.82f, -414.61f), new(-17.22f, -414.76f), new(-16.66f, -414.99f), new(-16.28f, -415.44f),
        new(-16, -415.95f), new(-15.89f, -416.49f), new(-15.38f, -416.25f), new(-14.79f, -416.26f), new(-12.91f, -416.85f),
        new(-12.48f, -416.31f), new(-12.34f, -415.66f), new(-12.57f, -415.01f), new(-13.56f, -413.38f), new(-13.85f, -412.06f),
        new(-13.75f, -410.77f), new(-13.4f, -409.53f), new(-12.85f, -408.34f), new(-12.16f, -407.17f), new(-11.35f, -406.27f),
        new(-10.81f, -406.33f), new(-7.5f, -408.1f), new(-3.67f, -409.23f), new(0.15f, -409.58f), new(3.94f, -409.18f),
        new(7.16f, -408.2f), new(7.77f, -407.95f), new(10.71f, -406.38f), new(11.27f, -406.26f), new(11.72f, -406.65f),
        new(12.16f, -407.15f), new(12.85f, -408.31f), new(13.38f, -409.48f), new(13.72f, -410.7f), new(13.83f, -411.98f),
        new(13.6f, -413.3f), new(12.62f, -414.9f), new(12.36f, -415.55f), new(12.47f, -416.28f), new(12.81f, -416.95f)];
        var arena = new ArenaBoundsCustom([new PolygonCustom(vertices)]);
        return (arena.Center, arena);
    }
}
