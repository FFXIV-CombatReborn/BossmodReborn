namespace BossMod.Dawntrail.Alliance.A20JaggedyEaredJack;

public enum OID : uint
{
    JaggedyEaredJack = 0x4875, // R1.1
    ForestFunguar = 0x487A, // R1.43
    ScarabBeetle = 0x4879, // R1.43
    GoblinThug = 0x4877, // R0.91
    ForestHare = 0x4876, // R0.715
    OrcishGrappler = 0x4874, // R2.25
    GoblinFisher = 0x4878 // R1.12
}

public enum AID : uint
{
    AutoAttack1 = 872, // ForestFunguar/JaggedyEaredJack/ForestHare/ScarabBeetle->player, no cast, single-target
    AutoAttack2 = 870, // GoblinFisher/OrcishGrappler/GoblinThug->player, no cast, single-target

    FrogKick = 43552, // ForestFunguar->player, no cast, single-target
    GoblinRush = 43551, // GoblinFisher->players, no cast, single-target
    BattleDance = 43546, // OrcishGrappler->self, no cast, range 30 circle
    RhinoAttack = 43549, // ScarabBeetle->player, no cast, single-target
    DustCloud = 43554 // JaggedyEaredJack->self, 3.0s cast, range 10 120-degree cone
}

sealed class DustCloud(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DustCloud, new AOEShapeCone(10f, 60f.Degrees()));

sealed class A20JaggedyEaredJackStates : StateMachineBuilder
{
    public A20JaggedyEaredJackStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DustCloud>()
            .Raw.Update = () => AllDeadOrDestroyed(A20JaggedyEaredJack.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.JaggedyEaredJack, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1058u, NameID = 14080u, Category = BossModuleInfo.Category.Alliance, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 1)]
public sealed class A20JaggedyEaredJack(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom([new(591.94f, 772.92f), new(597.99f, 774.09f),
    new(604.11f, 774.13f), new(611.30f, 776.14f), new(611.85f, 776.58f),
    new(613.09f, 778.00f), new(613.61f, 778.30f), new(614.16f, 777.92f), new(614.35f, 777.33f), new(617.61f, 778.94f),
    new(618.31f, 779.10f), new(627.38f, 780.06f), new(628.10f, 780.20f), new(631.80f, 782.57f), new(632.36f, 783.03f),
    new(638.82f, 791.50f), new(638.56f, 799.03f), new(638.72f, 799.64f), new(644.63f, 814.25f), new(644.57f, 814.83f),
    new(627.77f, 835.70f), new(627.33f, 835.32f), new(622.72f, 827.89f), new(622.16f, 827.61f), new(613.56f, 825.97f),
    new(613.04f, 825.84f), new(598.63f, 819.85f), new(590.72f, 817.29f), new(590.10f, 816.88f), new(582.21f, 810.24f),
    new(579.59f, 804.38f), new(579.00f, 804.23f), new(578.28f, 804.12f), new(578.25f, 803.42f), new(591.49f, 773.11f)])]);

    public static readonly uint[] Trash = [(uint)OID.JaggedyEaredJack, (uint)OID.ForestFunguar, (uint)OID.ScarabBeetle, (uint)OID.GoblinThug,
    (uint)OID.ForestHare, (uint)OID.OrcishGrappler, (uint)OID.GoblinFisher];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Trash);
    }
}
