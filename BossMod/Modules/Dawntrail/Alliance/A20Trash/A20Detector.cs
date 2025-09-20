namespace BossMod.Dawntrail.Alliance.A20Detector;

public enum OID : uint
{
    Detector1 = 0x4880, // R1.61
    Detector2 = 0x4881 // R1.15
}

public enum AID : uint
{
    AutoAttack = 27865, // Detector1/Detector2->player, no cast, single-target

    Electray = 43576, // Detector2->self, 6.0s cast, range 40 width 5 rect
    Electroswipe = 43559 // Detector1->self, 6.0s cast, range 50 120-degree cone
}

sealed class Electray(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Electray, new AOEShapeRect(40f, 2.5f));
sealed class Electroswipe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Electroswipe, new AOEShapeCone(50f, 60f.Degrees()));
sealed class ElectroswipeHint(BossModule module) : Components.CastInterruptHint(module, (uint)AID.Electroswipe, showNameInHint: true);

sealed class A20DetectorStates : StateMachineBuilder
{
    public A20DetectorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Electray>()
            .ActivateOnEnter<Electroswipe>()
            .ActivateOnEnter<ElectroswipeHint>()
            .Raw.Update = () => AllDeadOrDestroyed(A20Detector.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.Detector1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1058u, NameID = 14072u, Category = BossModuleInfo.Category.Alliance, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 3)]
public sealed class A20Detector(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices1 = [new(638.32f, 409.79f), new(638.77f, 410.25f), new(639.13f, 410.76f), new(639.72f, 411.08f), new(640.4f, 411.15f),
    new(640.96f, 411.51f), new(641.45f, 411.63f), new(641.8f, 412.19f), new(641.96f, 412.66f), new(641.91f, 413.32f),
    new(641.49f, 413.8f), new(641.25f, 414.46f), new(640.84f, 414.97f), new(640.78f, 415.59f), new(640.29f, 416.05f),
    new(639.78f, 416.39f), new(639.12f, 416.33f), new(638.59f, 416.84f), new(638.33f, 417.45f), new(637.92f, 418.07f),
    new(637.41f, 418.42f), new(636.89f, 418.85f), new(635.41f, 419.62f), new(634.81f, 419.79f), new(634.35f, 419.36f),
    new(633.99f, 418.83f), new(633.39f, 417.69f), new(633.17f, 417.1f), new(633.02f, 416.48f), new(633.22f, 415.98f),
    new(634.28f, 415.21f), new(634.08f, 414.6f), new(634.2f, 414.09f), new(634.5f, 413.5f),
    new(635.6f, 411.88f), new(636.57f, 410.98f), new(637.16f, 410.15f), new(637.64f, 410f), new(638.16f, 409.75f)];
    private static readonly WPos[] vertices2 = [new(636.91f, 380.32f), new(637.32f, 380.69f), new(637.73f, 382f), new(638.41f, 382.04f), new(639.07f, 381.92f),
    new(640.92f, 383.71f), new(642.38f, 386.02f), new(642.54f, 386.61f), new(642.12f, 386.97f), new(640.93f, 387.65f),
    new(640.88f, 388.33f), new(640.49f, 388.81f), new(639.88f, 389.3f), new(639.48f, 389.71f), new(638.92f, 389.81f),
    new(638.31f, 389.59f), new(637.91f, 389.1f), new(637.44f, 388.85f), new(637.12f, 388.28f), new(636.56f, 388.28f),
    new(636.13f, 387.81f), new(635.85f, 387.36f), new(635.77f, 386.85f), new(635.93f, 386.35f), new(635.42f, 385.84f),
    new(634.92f, 385.5f), new(634.16f, 384.56f), new(633.99f, 383.91f), new(633.72f, 383.41f), new(633.36f, 382.25f),
    new(633.24f, 381.66f), new(633.65f, 381.19f), new(634.17f, 380.92f), new(636.07f, 380.35f), new(636.69f, 380.31f)];
    private static readonly WPos[] vertices3 = [new(647.26f, 378.78f), new(647.93f, 378.77f), new(648.61f, 378.66f), new(651.29f, 378.47f), new(651.9f, 378.63f),
    new(653.65f, 379.35f), new(654.36f, 379.57f), new(654.98f, 379.51f), new(655.54f, 379.62f), new(655.78f, 380.25f),
    new(655.97f, 380.92f), new(656.14f, 382.23f), new(656.16f, 382.88f), new(655.99f, 383.53f), new(655.5f, 383.75f),
    new(653.59f, 384f), new(652.98f, 384.23f), new(651.73f, 384.47f), new(650.5f, 384.87f), new(649.74f, 384.82f),
    new(649.1f, 384.64f), new(648.77f, 384.05f), new(648.15f, 384.02f), new(647.51f, 384.22f), new(646.31f, 384.31f),
    new(645.97f, 383.94f), new(645.28f, 383.79f), new(644.79f, 383.91f), new(644.17f, 383.78f), new(643.01f, 383.25f),
    new(642.61f, 382.63f), new(642.38f, 382.09f), new(642.62f, 381.5f), new(642.95f, 380.25f), new(643.48f, 379.1f),
    new(643.85f, 378.52f), new(644.1f, 377.91f), new(644.55f, 377.68f)];
    private static readonly WPos[] vertices4 = [
    new(665.82f, 406.41f), new(664.81f, 407.02f), new(664.11f, 406.86f), new(663.43f, 406.79f), new(662.97f, 407.07f),
    new(662.81f, 407.69f), new(662.72f, 408.34f), new(662.69f, 409f), new(662.35f, 409.6f), new(661.46f, 409.48f),
    new(660.84f, 409.22f), new(660.26f, 409.07f), new(659.16f, 409.34f), new(658.88f, 408.78f), new(658.36f, 408.72f),
    new(656.45f, 408.86f), new(655.98f, 408.45f), new(655.48f, 408.21f), new(654.95f, 408.14f), new(654.32f, 408.18f),
    new(653.11f, 408.59f), new(652.41f, 408.67f), new(651.85f, 408.87f), new(651.16f, 409.04f), new(649.27f, 409.29f),
    new(648.81f, 409.69f), new(648.61f, 410.15f), new(648.8f, 410.75f), new(649.04f, 412.11f), new(649.21f, 412.73f),
    new(649.43f, 413.31f), new(649.96f, 413.53f), new(651.28f, 413.65f), new(653.09f, 414.4f), new(653.72f, 414.55f),
    new(657.67f, 414.2f), new(660.11f, 415.21f), new(660.63f, 415.31f), new(661.04f, 414.84f), new(662.03f, 412.79f),
    new(662.39f, 412.44f), new(662.93f, 412.78f), new(663.46f, 412.93f), new(664.05f, 413f), new(663.32f, 414.83f),
    new(663.53f, 415.32f), new(664.54f, 416.05f), new(665.15f, 416.34f), new(666.43f, 416.66f), new(667.9f, 416.69f),
    new(668.62f, 408.53f), new(668.32f, 407.97f), new(668.34f, 407.3f), new(668.01f, 406.73f), new(667.46f, 406.43f)
    ];
    private static readonly ArenaBoundsCustom arena = new([new Rectangle(new(650f, 400f), 18.72f, 23.5f)], [new PolygonCustom(vertices1),
    new PolygonCustom(vertices2), new PolygonCustom(vertices3), new PolygonCustom(vertices4), new Rectangle(new(669.10431f, 421.1626f), 13f, 1.25f, 90.283f.Degrees())]);

    public static readonly uint[] Trash = [(uint)OID.Detector1, (uint)OID.Detector2];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Trash);
    }
}
