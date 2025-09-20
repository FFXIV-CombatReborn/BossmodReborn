namespace BossMod.Endwalker.Dungeon.D12Aetherfont.D120HaamOtodus;

public enum OID : uint
{
    Boss = 0x3EC7, // R1.3
    HaamFrostbeast = 0x3EC6, // R5.4
    HaamOtodus2 = 0x3F5E, // R1.3
    HaamOtodus3 = 0x3F5F, // R1.3
    HaamAurelia = 0x3EC8 // R1.25
}

public enum AID : uint
{
    AutoAttack1 = 872, // Boss/HaamOtodus2/HaamOtodus3/HaamOtodus1->player, no cast, single-target
    AutoAttack2 = 871, // HaamAurelia->player, no cast, single-target
    Jump = 33983, // HaamOtodus3/HaamOtodus2/HaamOtodus1->location, no cast, ???

    AquaticLance = 33992, // HaamOtodus2/HaamOtodus3->location, 3.0s cast, range 8 circle
    Icestorm = 33991, // Boss->self, 3.0s cast, range 16 90-degree cone
}

class AquaticLance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AquaticLance, 8);
class Icestorm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Icestorm, new AOEShapeCone(16, 45.Degrees()));

class D120HaamOtodusStates : StateMachineBuilder
{
    public D120HaamOtodusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AquaticLance>()
            .ActivateOnEnter<Icestorm>()
            .Raw.Update = () => AllDeadOrDestroyedInBounds(D120HaamOtodus.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 822, NameID = 12339, SortOrder = 1)]
public class D120HaamOtodus(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(-422.31f, 355.42f), new(-421.72f, 356.67f), new(-421.41f, 357.22f), new(-420.21f, 357.64f), new(-419.71f, 358.12f),
    new(-419.42f, 358.71f), new(-419.33f, 361.1f), new(-418.92f, 361.6f), new(-418.43f, 361.99f), new(-417.83f, 362.24f),
    new(-417.33f, 362.73f), new(-416.82f, 363.86f), new(-415.67f, 364.26f), new(-415.15f, 364.7f), new(-414.74f, 365.28f),
    new(-414.39f, 366.65f), new(-414.86f, 367.57f), new(-415.06f, 368.19f), new(-414.98f, 370.11f), new(-415.08f, 372.1f),
    new(-414.74f, 372.57f), new(-414.55f, 373.25f), new(-414.75f, 373.72f), new(-415.15f, 374.27f), new(-415.08f, 374.83f),
    new(-415.16f, 375.72f), new(-415.68f, 375.23f), new(-416.31f, 374.89f), new(-416.9f, 374.83f), new(-417.6f, 375.01f),
    new(-418.06f, 375.28f), new(-418.24f, 375.85f), new(-418.6f, 376.46f), new(-419.35f, 377.48f), new(-420.04f, 377.76f),
    new(-421.97f, 378.26f), new(-422.66f, 378.34f), new(-423.16f, 378.25f), new(-423.79f, 378.18f), new(-424.07f, 378.78f),
    new(-424.11f, 379.52f), new(-424.19f, 380.01f), new(-424.25f, 380.69f), new(-423.85f, 381.14f), new(-423.46f, 381.74f),
    new(-423.58f, 382.44f), new(-424.04f, 382.9f), new(-424.12f, 383.58f), new(-423.51f, 383.9f), new(-422.81f, 383.99f),
    new(-421.58f, 384.52f), new(-420.9f, 384.35f), new(-420.42f, 384.54f), new(-420.24f, 385.77f), new(-419.99f, 386.26f),
    new(-419.41f, 386.49f), new(-418.99f, 387.04f), new(-418.78f, 387.72f), new(-418.5f, 389.12f), new(-417.95f, 390.4f),
    new(-417.71f, 391.1f), new(-417.38f, 391.68f), new(-416.79f, 391.98f), new(-416.3f, 391.84f), new(-415.89f, 391.34f),
    new(-415.22f, 391.19f), new(-414.58f, 391.23f), new(-414.26f, 391.7f), new(-413.81f, 392.13f), new(-413.37f, 392.38f),
    new(-412.81f, 392.54f), new(-412.24f, 392.48f), new(-411.99f, 391.96f), new(-411.53f, 391.78f), new(-410.27f, 391.84f),
    new(-409.82f, 392.05f), new(-409.27f, 393.19f), new(-409.25f, 393.69f), new(-409.08f, 394.22f), new(-408.79f, 394.8f),
    new(-408.98f, 396.77f), new(-409.14f, 397.48f), new(-409.01f, 398.08f), new(-408.62f, 398.71f), new(-408.18f, 399.3f),
    new(-407.68f, 399.83f), new(-407.07f, 400.14f), new(-405.41f, 401.2f), new(-404.81f, 401.41f), new(-404.23f, 401.39f),
    new(-403.65f, 401.1f), new(-403.21f, 401.35f), new(-402.66f, 401.74f), new(-402.01f, 401.76f), new(-400.57f, 401.89f),
    new(-400.6f, 402.47f), new(-400.44f, 403.06f), new(-400.25f, 403.56f), new(-400.56f, 404.12f), new(-400.48f, 404.82f),
    new(-400.21f, 405.46f), new(-399.63f, 405.73f), new(-399.05f, 405.71f), new(-398.77f, 405.18f), new(-398.25f, 404.75f),
    new(-397.62f, 404.61f), new(-396.94f, 404.8f), new(-395.85f, 405.5f), new(-395.28f, 405.39f), new(-394.67f, 405.22f),
    new(-394.17f, 405.28f), new(-392.96f, 405.72f), new(-392.29f, 405.83f), new(-391.46f, 406.74f), new(-390.92f, 406.86f),
    new(-388.31f, 406.88f), new(-387.71f, 407.26f), new(-386.47f, 407.48f), new(-384.09f, 406.39f), new(-383.4f, 406.22f),
    new(-381.37f, 406.15f), new(-379.34f, 408.85f), new(-378.9f, 409.24f), new(-377.74f, 409.96f), new(-377.25f, 410.49f),
    new(-376.93f, 411.05f), new(-376.38f, 411.41f), new(-376.37f, 412.07f), new(-376.59f, 412.62f), new(-376.47f, 414.43f),
    new(-376.6f, 415.78f), new(-376.86f, 416.41f), new(-377.54f, 417.55f), new(-378.1f, 417.93f), new(-379.31f, 418.36f),
    new(-379.77f, 418.72f), new(-380.79f, 420.37f), new(-381.41f, 420.7f), new(-381.83f, 421.1f), new(-382.21f, 421.67f),
    new(-382.19f, 422.18f), new(-381.66f, 422.46f), new(-380.47f, 422.95f), new(-379.94f, 423.41f), new(-379.62f, 424.02f),
    new(-379.17f, 424.36f), new(-378.58f, 424.59f), new(-378.06f, 425.08f), new(-377.86f, 425.73f), new(-377.75f, 426.54f),
    new(-379.01f, 426.63f), new(-379.59f, 426.81f), new(-379.91f, 427.93f), new(-379.74f, 428.56f), new(-377.45f, 429.93f),
    new(-377.17f, 430.54f), new(-377.23f, 431.16f), new(-377.41f, 431.84f), new(-377.89f, 431.98f), new(-378.57f, 431.88f),
    new(-379.21f, 431.58f), new(-380.41f, 430.95f), new(-381.04f, 431.03f), new(-381.64f, 430.71f), new(-382.02f, 430.17f),
    new(-382.45f, 429.67f), new(-382.94f, 429.22f), new(-383.47f, 428.81f), new(-384.06f, 428.51f), new(-384.67f, 428.42f),
    new(-385.21f, 428.7f), new(-385.9f, 429.67f), new(-386.19f, 430.27f), new(-386.25f, 430.88f), new(-385.76f, 431.99f),
    new(-385.45f, 432.55f), new(-385.02f, 433.07f), new(-384.79f, 433.58f), new(-386.54f, 434.37f), new(-386.84f, 434.79f),
    new(-387, 436.09f), new(-386.79f, 437.98f), new(-386.62f, 438.56f), new(-386.58f, 439.34f), new(-386.89f, 439.93f),
    new(-387.26f, 440.49f), new(-387.95f, 440.64f), new(-396.53f, 440.83f), new(-397.2f, 440.75f), new(-397.84f, 440.6f),
    new(-398.46f, 440.4f), new(-399.07f, 440.15f), new(-399.66f, 439.85f), new(-400.2f, 439.44f), new(-400.6f, 438.91f),
    new(-400.86f, 438.3f), new(-401.22f, 437.91f), new(-402.32f, 437.67f), new(-402.58f, 436.37f), new(-402.82f, 435.87f),
    new(-403.43f, 435.78f), new(-403.92f, 435.25f), new(-404.26f, 434.64f), new(-404.51f, 433.42f), new(-404.68f, 432.81f),
    new(-405.95f, 431.32f), new(-407.54f, 430.19f), new(-407.93f, 429.65f), new(-407.9f, 427.74f), new(-407.98f, 427.15f),
    new(-408.91f, 426.31f), new(-410.2f, 424.81f), new(-411.73f, 423.67f), new(-412.06f, 423.05f), new(-412.37f, 421.78f),
    new(-412.08f, 421.21f), new(-411.9f, 420.67f), new(-411.9f, 420), new(-411.61f, 419.44f), new(-411.56f, 418.84f),
    new(-411.58f, 417.56f), new(-411.79f, 416.98f), new(-411.95f, 416.32f), new(-411.76f, 415.7f), new(-411.82f, 415.1f),
    new(-413.03f, 411.37f), new(-413.13f, 410.71f), new(-412.91f, 410.09f), new(-412.79f, 409.47f), new(-412.94f, 408.88f),
    new(-413.44f, 408.57f), new(-414.02f, 408.6f), new(-414.66f, 408.73f), new(-416, 408.64f), new(-416.65f, 408.64f),
    new(-417.99f, 408.77f), new(-418.62f, 408.62f), new(-419.27f, 408.65f), new(-419.89f, 408.37f), new(-420.4f, 408.05f),
    new(-421.05f, 407.8f), new(-421.92f, 406.72f), new(-422.14f, 406.09f), new(-422.55f, 405.61f), new(-423.09f, 405.17f),
    new(-423.6f, 404.7f), new(-424.23f, 403.46f), new(-424.46f, 402.76f), new(-424.64f, 402.08f), new(-424.9f, 401.54f),
    new(-424.94f, 400.86f), new(-423.9f, 400.16f), new(-423.78f, 399.55f), new(-423.71f, 398.91f), new(-423.76f, 398.31f),
    new(-424.27f, 398.05f), new(-424.92f, 397.81f), new(-425.39f, 397.59f), new(-425.83f, 397.06f), new(-426.29f, 396.64f),
    new(-427.32f, 395.92f), new(-427.96f, 395.84f), new(-428.63f, 395.83f), new(-429.28f, 396.04f), new(-429.9f, 395.74f),
    new(-430.49f, 395.53f), new(-431.05f, 395.73f), new(-431.76f, 395.73f), new(-432.42f, 395.66f), new(-433.06f, 395.41f),
    new(-433.61f, 395.05f), new(-433.96f, 394.47f), new(-433.98f, 393.85f), new(-435.01f, 392.29f), new(-435.41f, 391.81f),
    new(-435.88f, 391.35f), new(-436.18f, 390.91f), new(-436.78f, 390.67f), new(-437.12f, 390.3f), new(-436.93f, 389.64f),
    new(-436.6f, 386.99f), new(-436.72f, 385.78f), new(-436.87f, 385.21f), new(-437.19f, 384.63f), new(-437.19f, 383.31f),
    new(-437.49f, 382.79f), new(-438.25f, 381.71f), new(-438.47f, 381.05f), new(-438.59f, 380.43f), new(-438.8f, 379.86f),
    new(-439.26f, 379.4f), new(-439.79f, 379.34f), new(-440.39f, 379.08f), new(-442.27f, 377.24f), new(-443, 376.19f),
    new(-443.24f, 375.56f), new(-442.53f, 372.36f), new(-442.78f, 369.69f), new(-442.35f, 368.34f), new(-441.16f, 367.81f),
    new(-438.5f, 365.87f), new(-437.18f, 365.75f), new(-436.51f, 365.77f), new(-435.88f, 366.01f), new(-435.43f, 366.47f),
    new(-435.15f, 367.12f), new(-434.83f, 367.58f), new(-434.24f, 367.49f), new(-433.7f, 367.2f), new(-433.24f, 366.74f),
    new(-432.04f, 365.21f), new(-432.44f, 364.06f), new(-432.4f, 363.45f), new(-432.22f, 362.8f), new(-431.59f, 361.72f),
    new(-431.36f, 361.08f), new(-431.21f, 360.46f), new(-430.86f, 359.87f), new(-422.75f, 355.19f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.HaamFrostbeast, (uint)OID.HaamOtodus2, (uint)OID.HaamOtodus3, (uint)OID.HaamAurelia];

    protected override bool CheckPull() => IsAnyActorInBoundsInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Trash);
    }
}
