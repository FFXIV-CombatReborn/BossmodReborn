namespace BossMod.Dawntrail.Dungeon.D04Vanguard.D040VanguardLeptocyon;

public enum OID : uint
{
    Boss = 0x447A, // R2.7
    VanguardLeptocyon = 0x41D9, // R2.7
    VanguardSentryS7 = 0x41D6, // R0.5
    VanguardSentryG7 = 0x4478 // R0.5
}

public enum AID : uint
{
    AutoAttack1 = 871, // Boss/VanguardLeptocyon->player, no cast, single-target
    AutoAttack2 = 873, // VanguardSentryG7->player, no cast, single-target
    AutoAttack3 = 870, // VanguardSentryS7->player, no cast, single-target

    Levinbite = 39019, // Boss->player, no cast, single-target
    SpreadShot = 39017, // VanguardSentryG7->self, 4.0s cast, range 12 90-degree cone
}

sealed class SpreadShot(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpreadShot, new AOEShapeCone(12f, 45f.Degrees()));

sealed class D040VanguardLeptocyonStates : StateMachineBuilder
{
    public D040VanguardLeptocyonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SpreadShot>()
            .Raw.Update = () => AllDeadOrDestroyed(D040VanguardLeptocyon.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 831, NameID = 12778, SortOrder = 1)]
public sealed class D040VanguardLeptocyon(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(0.34f, 358), new(2.53f, 358.04f), new(2.56f, 358.59f), new(2.29f, 359.02f), new(1.51f, 359.94f),
    new(1.27f, 360.52f), new(1.13f, 361.2f), new(0.91f, 361.83f), new(0.65f, 362.44f), new(-0.01f, 363.53f),
    new(-0.27f, 364.13f), new(-0.72f, 366.75f), new(-0.67f, 367.41f), new(-0.46f, 368.05f), new(0.12f, 369.18f),
    new(0.55f, 370.58f), new(0.75f, 371.56f), new(1.31f, 372.86f), new(1.33f, 373.89f), new(2.03f, 375.98f),
    new(2.32f, 376.52f), new(2.8f, 376.71f), new(2.09f, 379.25f), new(2.05f, 379.91f), new(2.77f, 382.59f),
    new(3.22f, 383.13f), new(2.65f, 384.65f), new(2.49f, 385.28f), new(2.82f, 385.75f), new(3.05f, 386.44f),
    new(2.93f, 388.34f), new(2.65f, 388.77f), new(2.33f, 389.15f), new(1.92f, 389.7f), new(1.85f, 390.36f),
    new(1.52f, 390.75f), new(1.23f, 391.38f), new(0.98f, 392.07f), new(0.85f, 392.68f), new(0.77f, 393.34f),
    new(0.91f, 394.02f), new(1.23f, 394.59f), new(1.42f, 395.08f), new(0.71f, 395.86f), new(-0.15f, 397.41f),
    new(-0.32f, 397.99f), new(0.09f, 398.38f), new(0.59f, 398.74f), new(1.16f, 399.21f), new(0.91f, 399.7f),
    new(0.71f, 400.3f), new(0.57f, 401.02f), new(0.23f, 401.58f), new(0.31f, 402.15f), new(0.59f, 402.73f),
    new(0.62f, 403.9f), new(1.07f, 404.35f), new(1.52f, 404.88f), new(1.82f, 405.53f), new(1.97f, 406.02f),
    new(2.66f, 409.98f), new(2.92f, 410.55f), new(3.25f, 411.1f), new(3.55f, 411.77f), new(3.56f, 412.45f),
    new(3.78f, 413.1f), new(4.09f, 414.61f), new(4.42f, 415.07f), new(4.64f, 415.54f), new(4.86f, 416.23f),
    new(5.13f, 416.83f), new(4.83f, 426.64f), new(4.89f, 427.23f), new(5.34f, 427.68f), new(5.78f, 428.19f),
    new(5.91f, 428.85f), new(6.57f, 430.15f), new(6.95f, 433.61f), new(7.09f, 434.24f), new(7.65f, 434.57f),
    new(7.98f, 435.03f), new(7.4f, 435.19f), new(6.66f, 435.25f), new(5.97f, 435.14f), new(5.43f, 435.4f),
    new(4.84f, 435.8f), new(4.39f, 435.52f), new(3.75f, 435.3f), new(3.04f, 435.28f), new(2.34f, 435.48f),
    new(0.99f, 436.18f), new(0.41f, 436.63f), new(-0.24f, 437.43f), new(-0.49f, 437.95f), new(0.57f, 439.62f),
    new(1.02f, 439.92f), new(1.73f, 439.92f), new(1.36f, 440.3f), new(0.96f, 440.89f), new(0.74f, 441.55f),
    new(0.65f, 442.2f), new(1.05f, 442.73f), new(1.38f, 443.31f), new(2.29f, 445.73f), new(2.34f, 446.45f),
    new(2.95f, 447.49f), new(3.46f, 447.78f), new(4.08f, 447.93f), new(4.54f, 448.17f), new(4.61f, 448.67f),
    new(4.53f, 449.36f), new(4.25f, 449.96f), new(3.95f, 450.94f), new(4.06f, 451.57f), new(4.83f, 454.32f),
    new(4.78f, 455.57f), new(4.89f, 456.61f), new(5.07f, 457.24f), new(5.63f, 458.58f), new(5.8f, 459.28f),
    new(5.47f, 459.73f), new(5.03f, 460), new(4.33f, 460.22f), new(-2.75f, 461.33f), new(-15.27f, 459.98f),
    new(-15.83f, 460.33f), new(-16.1f, 459.79f), new(-15.46f, 458.8f), new(-14.76f, 457.1f), new(-14.12f, 454.53f),
    new(-14.1f, 451.92f), new(-14.02f, 451.3f), new(-14.76f, 449.49f), new(-14.83f, 448.99f), new(-14.69f, 448.44f),
    new(-15.43f, 446.6f), new(-15.48f, 445.93f), new(-14.89f, 444.76f), new(-15, 444.25f), new(-15.49f, 443.05f),
    new(-15.77f, 442.5f), new(-16.2f, 442.14f), new(-15.72f, 441.75f), new(-15.2f, 440.5f), new(-15.13f, 439.97f),
    new(-16.04f, 438.21f), new(-16.56f, 437.79f), new(-16.72f, 436.45f), new(-16.93f, 435.92f), new(-17.19f, 435.47f),
    new(-17.08f, 434.14f), new(-16.22f, 433.15f), new(-16, 432.62f), new(-15.91f, 432.1f), new(-16.38f, 431.68f),
    new(-16.95f, 431.31f), new(-17.49f, 431.1f), new(-17.61f, 430.51f), new(-17.54f, 429.88f), new(-17.53f, 429.18f),
    new(-17.25f, 428.57f), new(-16.88f, 428.04f), new(-16.55f, 427.49f), new(-16.31f, 426.9f), new(-16.39f, 426.38f),
    new(-17.04f, 425.27f), new(-17.83f, 424.17f), new(-17.93f, 423.55f), new(-17.55f, 423.03f), new(-17.31f, 422.5f),
    new(-16.93f, 420.69f), new(-16.39f, 419.5f), new(-16, 417.08f), new(-15.99f, 416.46f), new(-16.12f, 415.78f),
    new(-15.29f, 414.15f), new(-14.22f, 413.35f), new(-13.94f, 412.76f), new(-13.5f, 412.34f), new(-13.27f, 411.74f),
    new(-12.48f, 410.58f), new(-12.62f, 410.01f), new(-13.14f, 409.59f), new(-13.08f, 409.08f), new(-12.41f, 407.79f),
    new(-12.36f, 407.23f), new(-12.46f, 406.68f), new(-12.92f, 406.28f), new(-12.91f, 405.57f), new(-12.58f, 404.21f),
    new(-11.68f, 402.48f), new(-10.92f, 400.48f), new(-10.78f, 399.1f), new(-10.43f, 398.71f), new(-10.36f, 398.07f),
    new(-10.43f, 397.32f), new(-10.59f, 396.66f), new(-10.63f, 397.38f), new(-11.34f, 394.85f), new(-11.69f, 394.22f),
    new(-11.79f, 393.15f), new(-11.93f, 392.5f), new(-12.07f, 391.08f), new(-12.19f, 390.46f), new(-12.77f, 389.18f),
    new(-13.46f, 389), new(-14.1f, 388.72f), new(-14.31f, 388.07f), new(-14.25f, 387.41f), new(-14.35f, 386.05f),
    new(-14.57f, 385.53f), new(-14.92f, 384.93f), new(-15.06f, 384.28f), new(-15.33f, 383.64f), new(-15.7f, 383.14f),
    new(-15.9f, 382.67f), new(-15.71f, 382.2f), new(-16.95f, 379.14f), new(-17.26f, 378.69f), new(-17.95f, 377.49f),
    new(-17.62f, 377.06f), new(-17.33f, 376.49f), new(-16.53f, 375.37f), new(-16.67f, 374.87f), new(-16.74f, 374.19f),
    new(-16.47f, 373.58f), new(-16.09f, 373.02f), new(-15.76f, 372.44f), new(-15.46f, 372.03f), new(-14.92f, 371.64f),
    new(-14.49f, 371.12f), new(-14.18f, 370.58f), new(-13.96f, 369.89f), new(-13.05f, 368.19f), new(-12.92f, 367.58f),
    new(-12.47f, 367.33f), new(-11.43f, 366.51f), new(-11.03f, 366.02f), new(-9.76f, 363.78f), new(-9.67f, 363.18f),
    new(-9.76f, 361.18f), new(-9.84f, 360.58f), new(-10.64f, 358.79f), new(-10.75f, 358), new(0.34f, 358)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.VanguardSentryS7, (uint)OID.VanguardSentryG7, (uint)OID.VanguardLeptocyon];

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
