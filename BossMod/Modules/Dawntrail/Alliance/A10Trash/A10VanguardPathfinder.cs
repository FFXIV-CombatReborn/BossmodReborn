namespace BossMod.Dawntrail.Alliance.A10Trash.VanguardPathfinder;

public enum OID : uint
{
    Boss = 0x4689, // R1.05
    VanguardsSlime1 = 0x470E, // R1.0
    VanguardsSlime2 = 0x4688, // R1.0
    GoblinReplica = 0x4687 // R3.3
}

public enum AID : uint
{
    AutoAttack1 = 872, // GoblinReplica/VanguardsSlime1/VanguardsSlime2->player, no cast, single-target
    AutoAttack2 = 870, // Boss->player, no cast, single-target

    Seismostomp = 41652, // GoblinReplica->location, 3.0s cast, range 5 circle
    BombToss = 41655, // Boss->location, 2.5s cast, range 3 circle
    GoblinRush = 41654 // Boss->players, no cast, single-target
}

sealed class BombToss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BombToss, 3f);
sealed class Seismostomp(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Seismostomp, 5f);

public sealed class A10VanguardPathfinderStates : StateMachineBuilder
{
    public A10VanguardPathfinderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Seismostomp>()
            .ActivateOnEnter<BombToss>()
            .Raw.Update = () => AllDeadOrDestroyed(A10VanguardPathfinder.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1015, NameID = 13599, SortOrder = 1)]
public sealed class A10VanguardPathfinder(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(802.28f, 621.41f), new(807.98f, 621.43f), new(808.32f, 621.98f), new(808.56f, 622.62f), new(808.64f, 623.2f),
    new(809.14f, 623.53f), new(810.36f, 624.09f), new(810.09f, 646.5f), new(810, 647.29f), new(808.77f, 647.07f),
    new(808.08f, 647.17f), new(807.4f, 647.06f), new(806.74f, 647.02f), new(806.27f, 647.33f), new(806.01f, 647.89f),
    new(806.11f, 648.49f), new(806.03f, 649.7f), new(806.09f, 650.38f), new(806.01f, 651.1f), new(806.2f, 651.76f),
    new(806.13f, 652.28f), new(806.1f, 654.22f), new(806.01f, 654.95f), new(806.17f, 655.57f), new(806.07f, 656.1f),
    new(806.04f, 656.77f), new(806.14f, 657.43f), new(806, 658.08f), new(806.21f, 658.65f), new(806.74f, 658.99f),
    new(807.37f, 659.25f), new(810.04f, 663.57f), new(810.94f, 665.41f), new(810.98f, 666.15f), new(810.72f, 666.58f),
    new(810.24f, 667.14f), new(809.71f, 667.65f), new(809.29f, 667.93f), new(808.83f, 668.14f), new(808.33f, 668.23f),
    new(792.56f, 668.1f), new(791.87f, 668.18f), new(791.4f, 667.68f), new(791.2f, 666.38f), new(791.52f, 658.97f),
    new(791.58f, 655.73f), new(791.53f, 655.03f), new(791.53f, 650.9f), new(791.59f, 647.85f), new(791.5f, 647.25f),
    new(791.14f, 646.8f), new(790.64f, 646.9f), new(790.1f, 644.4f), new(790.06f, 643.72f), new(790.36f, 638.9f),
    new(790.51f, 638.16f), new(791, 638.04f), new(793.05f, 637.93f), new(793.69f, 637.85f), new(793.91f, 636.67f),
    new(793.78f, 636.05f), new(793.89f, 635.54f), new(793.99f, 634.85f), new(793.86f, 634.16f), new(794, 633.46f),
    new(793.88f, 632.76f), new(793.99f, 632.05f), new(793.84f, 631.42f), new(793.32f, 631.02f), new(792.9f, 630.47f),
    new(792.26f, 630.45f), new(791.17f, 631.07f), new(790.72f, 630.82f), new(790.39f, 630.15f), new(790.12f, 629.47f),
    new(789.76f, 628.14f), new(789.53f, 627.49f), new(788.77f, 626.62f), new(788.84f, 625.36f), new(789.15f, 624.94f),
    new(790.4f, 624.3f), new(790.66f, 623.66f), new(790.86f, 623), new(790.61f, 622.49f), new(791.1f, 622.42f),
    new(791.62f, 622.18f), new(791.71f, 621.53f), new(802.28f, 621.41f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.VanguardsSlime1, (uint)OID.VanguardsSlime2, (uint)OID.GoblinReplica];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Trash);
    }
}
