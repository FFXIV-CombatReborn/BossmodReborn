namespace BossMod.Dawntrail.Dungeon.D04Vanguard.D040VanguardSentryR7;

public enum OID : uint
{
    Boss = 0x4479, //R=2.34
    SentryR7 = 0x41D8, //R=2.34
}

public enum AID : uint
{
    AutoAttack = 36403, // SentryR7/Boss->player, no cast, single-target

    Swoop = 38051, // SentryR7/Boss->location, 4.0s cast, width 5 rect charge
    FloaterTurn = 38451, // SentryR7/Boss->self, 4.0s cast, range 4-10 donut
    SpinningAxle = 39018, // Boss->self, 4.0s cast, range 6 circle
}

sealed class Swoop(BossModule module) : Components.ChargeAOEs(module, (uint)AID.Swoop, 2.5f);
sealed class FloaterTurn(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FloaterTurn, new AOEShapeDonut(4f, 10f));
sealed class SpinningAxle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpinningAxle, 6f);

sealed class D040VanguardSentryR7States : StateMachineBuilder
{
    public D040VanguardSentryR7States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Swoop>()
            .ActivateOnEnter<FloaterTurn>()
            .ActivateOnEnter<SpinningAxle>()
            .Raw.Update = () => AllDeadOrDestroyed(D040VanguardSentryR7.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 831, NameID = 12778, SortOrder = 2)]
public sealed class D040VanguardSentryR7(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(-93.53f, 262.87f), new(-91.94f, 262.9f), new(-90.82f, 263.02f), new(-90.31f, 263.21f), new(-90.22f, 263.86f),
    new(-89.91f, 264.43f), new(-89.93f, 278.76f), new(-90.12f, 279.4f), new(-89.96f, 280.02f), new(-89.56f, 280.42f),
    new(-80.64f, 280.42f), new(-80.54f, 280.95f), new(-80.54f, 282.18f), new(-80.99f, 282.73f), new(-81.42f, 283.16f),
    new(-81.47f, 286.97f), new(-82.16f, 288.07f), new(-82.23f, 288.79f), new(-86.3f, 288.76f), new(-86.91f, 288.47f),
    new(-87.48f, 288.38f), new(-88.13f, 288.55f), new(-88.76f, 288.8f), new(-89.07f, 300.14f), new(-88.81f, 300.58f),
    new(-88.42f, 301.15f), new(-88.41f, 301.86f), new(-88.78f, 302.89f), new(-89.6f, 304.8f), new(-89.6f, 306.87f),
    new(-89.45f, 307.43f), new(-85.06f, 307.55f), new(-84.74f, 308.18f), new(-84.53f, 308.73f), new(-84.13f, 309.09f),
    new(-83.54f, 309.09f), new(-82.86f, 309.39f), new(-82.61f, 310.95f), new(-82.65f, 311.67f), new(-74.3f, 313.14f),
    new(-74.5f, 313.62f), new(-74.56f, 318.22f), new(-74.29f, 318.66f), new(-55.8f, 318.66f), new(-55.37f, 318.34f),
    new(-55.34f, 317.21f), new(-54.91f, 316.85f), new(-54.43f, 317.03f), new(-53.85f, 317.14f), new(-52.72f, 316.71f),
    new(-52.08f, 316.77f), new(-51.61f, 316.95f), new(-51, 316.72f), new(-50.42f, 317.09f), new(-50.03f, 317.74f),
    new(-49.32f, 321.89f), new(-49.3f, 322.4f), new(-50.59f, 331.97f), new(-50.63f, 332.58f), new(-50.08f, 332.56f),
    new(-50.32f, 333.66f), new(-50.8f, 333.84f), new(-52.18f, 333.79f), new(-53.48f, 333.55f), new(-54.13f, 333.57f),
    new(-54.76f, 333.66f), new(-56.83f, 333.69f), new(-57.56f, 333.59f), new(-58.25f, 333.56f), new(-58.94f, 333.59f),
    new(-59.62f, 333.74f), new(-61.74f, 333.85f), new(-62.37f, 334.09f), new(-63.1f, 334.12f), new(-63.62f, 334.32f),
    new(-63.71f, 337.46f), new(-64.16f, 337.69f), new(-64.88f, 337.86f), new(-66.28f, 337.76f), new(-66.94f, 337.83f),
    new(-68.31f, 337.72f), new(-70.32f, 337.8f), new(-70.92f, 337.87f), new(-71.61f, 337.81f), new(-72.24f, 338.11f),
    new(-72.51f, 339.51f), new(-74.39f, 339.68f), new(-82.01f, 339.66f), new(-82.7f, 339.51f), new(-85.92f, 336.18f),
    new(-96.32f, 335.99f), new(-96.94f, 335.95f), new(-97.81f, 335.96f), new(-98f, 333.99f), new(-97.9f, 333.37f),
    new(-97.88f, 332.78f), new(-97.94f, 332.15f), new(-98.06f, 331.57f), new(-105.24f, 331.38f), new(-105.63f, 331.06f),
    new(-108.96f, 326.19f), new(-109.2f, 311.78f), new(-109.26f, 311.13f), new(-110.74f, 310.93f), new(-110.74f, 310.1f),
    new(-110.51f, 309.02f), new(-110.79f, 308.4f), new(-111.16f, 308.03f), new(-110.61f, 306.2f), new(-110.59f, 295.17f),
    new(-110.81f, 294.6f), new(-110.9f, 287.95f), new(-111.2f, 287.52f), new(-118.89f, 287.52f), new(-118.86f, 272.73f),
    new(-118.67f, 273.19f), new(-118.12f, 273.45f), new(-111.03f, 273.48f), new(-110.41f, 273.75f), new(-109.86f, 273.6f),
    new(-109.54f, 273.09f), new(-109.52f, 263.18f), new(-109.01f, 263.02f), new(-107.66f, 262.88f), new(-93.53f, 262.87f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.SentryR7];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Trash);
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
