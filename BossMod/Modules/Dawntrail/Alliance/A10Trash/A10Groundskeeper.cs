namespace BossMod.Dawntrail.Alliance.A10Groundskeeper;

public enum OID : uint
{
    Boss = 0x4691, // R1.87
    Groundskeeper = 0x4712, // R1.87
    Sprinkler = 0x4690 // R2.5
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/Sprinkler/Groundskeeper->player, no cast, single-target

    IsleDrop = 41670, // Boss->location, 3.0s cast, range 6 circle
    MysteriousLight = 41667, // Sprinkler->self, 4.5s cast, range 20 circle, gaze
    DoubleRay = 41668 // Sprinkler->player, no cast, single-target
}

sealed class IsleDrop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IsleDrop, 6f);
sealed class MysteriousLight(BossModule module) : Components.CastGaze(module, (uint)AID.MysteriousLight);

public sealed class A10GroundskeeperStates : StateMachineBuilder
{
    public A10GroundskeeperStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IsleDrop>()
            .ActivateOnEnter<MysteriousLight>()
            .Raw.Update = () => AllDeadOrDestroyed(A10Groundskeeper.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1015, NameID = 13607, SortOrder = 5)]
public sealed class A10Groundskeeper(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(-544.28f, -642.15f), new(-532, -635.01f), new(-531.47f, -634.52f), new(-531.58f, -634), new(-547.79f, -595.87f),
    new(-547.82f, -595.32f), new(-547.35f, -594.98f), new(-540.15f, -590.75f), new(-539.65f, -590.89f), new(-539.18f, -591.4f),
    new(-538.53f, -591.64f), new(-537.86f, -591.57f), new(-537.24f, -591.23f), new(-536.73f, -590.77f), new(-536.21f, -590.77f),
    new(-535.82f, -591.17f), new(-534.22f, -593.87f), new(-533.85f, -594.25f), new(-527.45f, -590.48f), new(-527.2f, -590.03f),
    new(-536.88f, -573.6f), new(-537.56f, -573.69f), new(-542.02f, -576.35f), new(-542.42f, -576.67f), new(-542.22f, -577.99f),
    new(-542.21f, -579.29f), new(-542.15f, -579.95f), new(-541.99f, -580.59f), new(-541.93f, -581.16f), new(-542.37f, -581.51f),
    new(-542.93f, -581.83f), new(-543.46f, -582.21f), new(-543.83f, -582.79f), new(-543.9f, -583.51f), new(-543.68f, -584.12f),
    new(-544.01f, -584.68f), new(-551.02f, -588.81f), new(-551.57f, -589.06f), new(-552, -588.72f), new(-565.78f, -571.07f),
    new(-566.46f, -571.14f), new(-578.6f, -578.84f), new(-578.47f, -579.48f), new(-575.19f, -583.67f), new(-574.69f, -584.13f),
    new(-574.19f, -584.46f), new(-573.85f, -584.98f), new(-573.74f, -585.58f), new(-573.98f, -586.06f), new(-577.9f, -589.12f),
    new(-578.21f, -589.54f), new(-575.7f, -592.76f), new(-575.24f, -593.01f), new(-572.97f, -591.23f), new(-571.98f, -590.14f),
    new(-571.43f, -589.78f), new(-570.76f, -589.62f), new(-570.3f, -589.94f), new(-564.43f, -597.37f), new(-564.35f, -597.92f),
    new(-564.71f, -598.33f), new(-568.95f, -600.82f), new(-569.28f, -601.23f), new(-567.14f, -604.83f), new(-566.46f, -604.8f),
    new(-565.21f, -604.06f), new(-564.32f, -603.14f), new(-563.89f, -602.73f), new(-563.4f, -602.37f), new(-562.83f, -602.15f),
    new(-562.26f, -602.15f), new(-561.68f, -602.32f), new(-561.05f, -603.42f), new(-557.61f, -611.37f), new(-557.63f, -611.93f),
    new(-558.2f, -612.25f), new(-562.33f, -614f), new(-562.88f, -614.49f), new(-561.34f, -618.12f), new(-561.07f, -618.57f),
    new(-557.2f, -616.93f), new(-556.69f, -616.59f), new(-556.17f, -616.37f), new(-555.61f, -616.44f), new(-555.32f, -616.94f),
    new(-544.54f, -642.34f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.Groundskeeper, (uint)OID.Sprinkler];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Trash);
    }
}
