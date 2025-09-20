namespace BossMod.Endwalker.Dungeon.D12Aetherfont.D120HaamHalonid;

public enum OID : uint
{
    Boss = 0x3EC9, // R1.5
    HaamAurelia = 0x3EC8, // R1.25
    HaamFrostbeast = 0x3EC6 // R5.4
}

public enum AID : uint
{
    AutoAttack1 = 872, // Boss/HaamFrostbeast->player, no cast, single-target
    AutoAttack2 = 871, // HaamAurelia->player, no cast, single-target

    Icestorm = 33991 // HaamFrostbeast->self, 3.0s cast, range 16 90-degree cone
}

class Icestorm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Icestorm, new AOEShapeCone(16, 45.Degrees()));

class D120HaamHalonidStates : StateMachineBuilder
{
    public D120HaamHalonidStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Icestorm>()
            .Raw.Update = () => AllDeadOrDestroyedInBounds(D120HaamHalonid.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 822, NameID = 12341, SortOrder = 2)]
public class D120HaamHalonid(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(-325.70f, 228.19f), new(-324.46f, 228.61f), new(-323.65f, 229.52f), new(-323.07f, 229.82f), new(-322.55f, 230.31f),
    new(-321.99f, 230.55f), new(-319.57f, 231.20f), new(-319.07f, 230.91f), new(-318.03f, 230.09f), new(-317.43f, 229.75f),
    new(-316.12f, 229.46f), new(-315.51f, 229.75f), new(-307.57f, 237.76f), new(-308.04f, 238.09f), new(-309.20f, 238.33f),
    new(-309.60f, 238.88f), new(-310.27f, 238.75f), new(-310.92f, 238.97f), new(-311.55f, 238.72f), new(-312.15f, 238.76f),
    new(-313.40f, 239.05f), new(-314.12f, 239.88f), new(-316.80f, 239.43f), new(-317.23f, 239.70f), new(-318.29f, 242.06f),
    new(-318.81f, 243.93f), new(-319.65f, 244.97f), new(-320.33f, 245.07f), new(-321.63f, 244.86f), new(-322.21f, 244.92f),
    new(-322.84f, 245.20f), new(-323.48f, 245.41f), new(-326.02f, 244.98f), new(-326.54f, 245.11f), new(-327.05f, 245.53f),
    new(-327.72f, 245.54f), new(-328.06f, 246.08f), new(-327.77f, 247.31f), new(-328.01f, 247.78f), new(-330.82f, 249.55f),
    new(-332.13f, 251.13f), new(-332.47f, 251.63f), new(-332.27f, 252.17f), new(-331.93f, 252.77f), new(-332.03f, 253.28f),
    new(-332.48f, 253.70f), new(-332.73f, 254.35f), new(-333.22f, 254.79f), new(-333.33f, 255.41f), new(-333.81f, 255.89f),
    new(-334.43f, 256.15f), new(-334.89f, 256.53f), new(-335.18f, 257.11f), new(-335.40f, 257.68f), new(-335.85f, 258.11f),
    new(-336.41f, 258.33f), new(-336.98f, 258.72f), new(-337.61f, 258.75f), new(-338.16f, 259.08f), new(-338.87f, 259.00f),
    new(-339.45f, 259.11f), new(-339.81f, 259.57f), new(-340.44f, 259.75f), new(-341.06f, 259.75f), new(-341.49f, 260.18f),
    new(-342.16f, 260.26f), new(-342.71f, 260.36f), new(-343.30f, 260.55f), new(-343.88f, 260.32f), new(-345.13f, 260.18f),
    new(-345.72f, 260.54f), new(-346.46f, 261.68f), new(-346.47f, 262.20f), new(-346.00f, 262.61f), new(-345.44f, 263.79f),
    new(-345.22f, 264.42f), new(-344.96f, 264.97f), new(-344.73f, 265.65f), new(-345.08f, 267.71f), new(-345.01f, 268.36f),
    new(-345.18f, 268.83f), new(-345.21f, 269.44f), new(-343.84f, 270.74f), new(-343.52f, 271.34f), new(-343.57f, 272.01f),
    new(-343.26f, 272.55f), new(-343.02f, 273.19f), new(-343.90f, 274.74f), new(-345.34f, 276.90f), new(-345.25f, 277.44f),
    new(-345.31f, 278.13f), new(-345.60f, 278.75f), new(-345.92f, 279.27f), new(-346.44f, 280.61f), new(-346.95f, 280.48f),
    new(-347.51f, 280.17f), new(-348.12f, 279.99f), new(-348.77f, 279.96f), new(-349.39f, 280.15f), new(-349.85f, 280.51f),
    new(-350.42f, 281.69f), new(-350.94f, 282.17f), new(-351.40f, 282.38f), new(-352.76f, 282.34f), new(-353.32f, 282.52f),
    new(-353.92f, 282.82f), new(-354.53f, 282.96f), new(-354.94f, 283.41f), new(-355.36f, 283.99f), new(-356.31f, 284.88f),
    new(-356.84f, 285.27f), new(-357.22f, 285.72f), new(-357.31f, 286.38f), new(-358.20f, 287.41f), new(-358.76f, 287.83f),
    new(-359.97f, 288.24f), new(-360.64f, 288.25f), new(-361.76f, 287.74f), new(-365.72f, 287.67f), new(-366.23f, 287.98f),
    new(-366.88f, 288.23f), new(-368.06f, 287.66f), new(-368.64f, 287.44f), new(-369.21f, 287.70f), new(-369.86f, 287.62f),
    new(-370.45f, 287.42f), new(-371.04f, 287.29f), new(-371.60f, 287.56f), new(-372.18f, 287.90f), new(-373.43f, 288.05f),
    new(-373.90f, 288.51f), new(-374.58f, 288.51f), new(-375.19f, 288.55f), new(-375.53f, 289.02f), new(-376.11f, 289.36f),
    new(-376.73f, 289.41f), new(-377.20f, 289.80f), new(-379.04f, 290.25f), new(-379.62f, 290.09f), new(-380.22f, 290.22f),
    new(-380.82f, 290.15f), new(-382.66f, 291.19f), new(-385.88f, 292.51f), new(-387.64f, 292.93f), new(-388.17f, 293.14f),
    new(-389.72f, 294.52f), new(-390.35f, 294.74f), new(-391.03f, 294.73f), new(-392.31f, 294.50f), new(-392.74f, 294.83f),
    new(-393.19f, 295.33f), new(-393.90f, 295.29f), new(-394.52f, 295.39f), new(-395.15f, 295.63f), new(-395.81f, 295.41f),
    new(-396.39f, 295.14f), new(-396.93f, 294.68f), new(-397.04f, 294.07f), new(-397.36f, 293.66f), new(-397.96f, 293.53f),
    new(-398.58f, 293.30f), new(-399.14f, 292.94f), new(-399.67f, 292.85f), new(-400.04f, 293.30f), new(-402.53f, 297.12f),
    new(-402.89f, 298.38f), new(-404.61f, 302.62f), new(-404.60f, 302.07f), new(-404.90f, 301.62f), new(-414.12f, 290.50f),
    new(-414.15f, 289.81f), new(-413.99f, 289.10f), new(-413.55f, 288.61f), new(-413.11f, 288.18f), new(-412.65f, 286.33f),
    new(-412.29f, 285.77f), new(-411.90f, 285.26f), new(-411.26f, 284.04f), new(-410.89f, 282.07f), new(-409.52f, 280.64f),
    new(-408.85f, 280.48f), new(-407.62f, 280.69f), new(-406.26f, 280.44f), new(-405.61f, 280.59f), new(-404.98f, 280.82f),
    new(-404.51f, 281.34f), new(-404.17f, 281.96f), new(-403.75f, 282.29f), new(-402.00f, 283.06f), new(-401.44f, 283.12f),
    new(-399.40f, 282.97f), new(-398.84f, 282.69f), new(-397.00f, 282.22f), new(-396.78f, 281.55f), new(-396.81f, 280.30f),
    new(-396.51f, 279.72f), new(-394.16f, 278.77f), new(-393.66f, 278.39f), new(-393.18f, 277.93f), new(-391.40f, 277.02f),
    new(-390.28f, 276.14f), new(-389.61f, 276.00f), new(-387.27f, 277.06f), new(-386.74f, 277.16f), new(-381.52f, 273.29f),
    new(-380.87f, 273.16f), new(-380.23f, 273.14f), new(-379.66f, 273.01f), new(-379.21f, 272.53f), new(-377.89f, 272.47f),
    new(-377.31f, 272.35f), new(-376.69f, 272.05f), new(-375.55f, 272.71f), new(-373.65f, 272.25f), new(-373.25f, 271.79f),
    new(-372.75f, 271.31f), new(-371.02f, 270.43f), new(-370.76f, 269.95f), new(-370.55f, 269.37f), new(-370.20f, 268.78f),
    new(-369.10f, 268.12f), new(-367.98f, 266.36f), new(-367.81f, 265.78f), new(-367.70f, 265.15f), new(-367.48f, 264.53f),
    new(-367.17f, 263.99f), new(-367.05f, 263.36f), new(-366.84f, 262.72f), new(-365.91f, 261.60f), new(-365.33f, 261.21f),
    new(-364.16f, 260.64f), new(-363.70f, 260.45f), new(-362.39f, 260.83f), new(-361.78f, 261.13f), new(-361.53f, 261.77f),
    new(-361.21f, 262.32f), new(-360.65f, 262.67f), new(-360.30f, 263.26f), new(-360.10f, 263.93f), new(-359.68f, 264.40f),
    new(-359.24f, 264.72f), new(-358.64f, 264.64f), new(-358.11f, 264.23f), new(-356.24f, 262.24f), new(-355.66f, 261.82f),
    new(-355.07f, 261.51f), new(-354.60f, 261.16f), new(-354.66f, 260.59f), new(-355.28f, 257.99f), new(-355.51f, 257.44f),
    new(-357.25f, 254.62f), new(-357.32f, 253.94f), new(-357.29f, 253.27f), new(-357.11f, 252.63f), new(-355.47f, 248.99f),
    new(-354.95f, 248.51f), new(-354.45f, 248.13f), new(-354.02f, 247.66f), new(-353.62f, 247.13f), new(-352.97f, 246.87f),
    new(-351.59f, 246.91f), new(-350.98f, 246.85f), new(-345.56f, 244.51f), new(-345.25f, 244.07f), new(-341.81f, 236.91f),
    new(-341.29f, 234.92f), new(-340.89f, 234.41f), new(-339.55f, 232.89f), new(-338.73f, 231.16f), new(-338.58f, 230.54f),
    new(-338.31f, 229.87f), new(-337.19f, 229.56f), new(-336.68f, 229.66f), new(-336.08f, 230.08f), new(-335.51f, 230.17f),
    new(-333.80f, 229.17f), new(-331.28f, 229.78f), new(-330.72f, 230.16f), new(-330.05f, 230.26f), new(-329.44f, 230.63f),
    new(-328.76f, 230.39f), new(-328.10f, 230.35f), new(-327.49f, 230.19f), new(-327.20f, 229.67f), new(-327.33f, 229.07f),
    new(-326.86f, 228.75f), new(-326.30f, 228.44f), new(-325.70f, 228.19f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.HaamFrostbeast, (uint)OID.HaamAurelia];

    protected override bool CheckPull() => IsAnyActorInBoundsInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Trash);
    }
}
