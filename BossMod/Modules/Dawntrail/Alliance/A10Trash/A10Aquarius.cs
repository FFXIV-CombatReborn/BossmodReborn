namespace BossMod.Dawntrail.Alliance.A10Aquarius;

public enum OID : uint
{
    Boss = 0x468F, // R2.1
    ElderGobbue = 0x468D, // R2.28
    RobberCrab1 = 0x468E, // R0.7
    RobberCrab2 = 0x4711, // R0.7
    DeathCap = 0x468C, // R1.65
    BarkSpider1 = 0x468B, // R1.5
    BarkSpider2 = 0x4710, // R1.5
    Skimmer1 = 0x468A, // R1.5
    Skimmer2 = 0x470F // R0.6
}

public enum AID : uint
{
    AutoAttack1 = 872, // BarkSpider1/DeathCap/BarkSpider2/Skimmer1/Skimmer2->player, no cast, single-target
    AutoAttack2 = 870, // RobberCrab2/Boss/RobberCrab1/Aquarius->player, no cast, single-target

    FrogKick = 41660, // DeathCap->player, no cast, single-target
    BubbleShower = 41664, // RobberCrab1->self, 3.0s cast, range 6 60-degree cone
    Scoop = 41663, // Boss->self, 4.0s cast, range 15 120-degree cone
    CursedSphere = 41656, // Skimmer1->location, 3.0s cast, range 3 circle
    WaterIII = 41666, // Aquarius->location, 3.0s cast, range 7 circle
    Beatdown = 41662, // Boss->self, 2.0s cast, range 9 width 3 rect
    SpiderWeb = 41659, // BarkSpider1->self, 4.0s cast, range 6 circle
    HundredFists = 40648, // Aquarius->self, 6.0s cast, single-target, applies Hundred Fists status (ID 1594) to self
    Agaricus = 41661 // DeathCap->self, 3.0s cast, range 5 circle
}

sealed class CursedSphere(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CursedSphere, 3f);
sealed class WaterIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WaterIII, 7f);
sealed class BubbleShower(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BubbleShower, new AOEShapeCone(6f, 30f.Degrees()));
sealed class Scoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scoop, new AOEShapeCone(15f, 60f.Degrees()));
sealed class Agaricus(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Agaricus, 5f);
sealed class Beatdown(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Beatdown, new AOEShapeRect(9f, 1.5f));
sealed class SpiderWeb(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpiderWeb, 6f);
sealed class HundredFists(BossModule module) : Components.CastInterruptHint(module, (uint)AID.HundredFists, showNameInHint: true);

public sealed class A10AquariusStates : StateMachineBuilder
{
    public A10AquariusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WaterIII>()
            .ActivateOnEnter<CursedSphere>()
            .ActivateOnEnter<BubbleShower>()
            .ActivateOnEnter<Scoop>()
            .ActivateOnEnter<Beatdown>()
            .ActivateOnEnter<SpiderWeb>()
            .ActivateOnEnter<HundredFists>()
            .ActivateOnEnter<Agaricus>()
            .Raw.Update = () => AllDeadOrDestroyed(A10Aquarius.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1015, NameID = 13605, SortOrder = 3)]
public sealed class A10Aquarius(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(-500.62f, 686.9f), new(-488.18f, 686.93f), new(-487.84f, 687.31f), new(-486.66f, 692.24f), new(-486.41f, 692.91f),
    new(-486.06f, 693.39f), new(-485.44f, 693.68f), new(-484.78f, 693.9f), new(-484.23f, 693.88f), new(-483.59f, 693.9f),
    new(-480.66f, 695.16f), new(-480.22f, 695.71f), new(-479.84f, 696.3f), new(-479.5f, 696.92f), new(-479.12f, 697.34f),
    new(-478.2f, 697.35f), new(-477.91f, 697.76f), new(-477.59f, 698.37f), new(-476.07f, 700.72f), new(-475.56f, 700.98f),
    new(-464.2f, 704.92f), new(-463.56f, 705.24f), new(-458.68f, 709.03f), new(-458.32f, 709.38f), new(-457.22f, 713.11f),
    new(-457.24f, 713.61f), new(-457.07f, 714.32f), new(-456.68f, 714.95f), new(-456.19f, 715.26f), new(-454.18f, 714.85f),
    new(-453.68f, 714.95f), new(-448.72f, 717.64f), new(-448.27f, 717.94f), new(-444.88f, 722.85f), new(-444.73f, 723.49f),
    new(-443.43f, 730.36f), new(-444.69f, 733.44f), new(-444.82f, 734.02f), new(-444.15f, 735.16f), new(-443.76f, 735.55f),
    new(-443.1f, 735.49f), new(-442.39f, 735.61f), new(-442.13f, 736.93f), new(-442.16f, 737.62f), new(-442.11f, 738.17f),
    new(-441.69f, 738.71f), new(-441.24f, 739.2f), new(-440.67f, 739.3f), new(-439.24f, 739.22f), new(-438.54f, 739.33f),
    new(-437.9f, 739.51f), new(-437.42f, 739.71f), new(-433.91f, 742.8f), new(-434.46f, 743.21f), new(-435.71f, 745.24f),
    new(-435.96f, 745.8f), new(-436.01f, 746.41f), new(-435.86f, 746.91f), new(-435.83f, 747.6f), new(-435.72f, 748.18f),
    new(-435.13f, 748.4f), new(-434.46f, 748.58f), new(-433.86f, 748.5f), new(-433.33f, 748.05f), new(-433.42f, 748.68f),
    new(-432.75f, 750.65f), new(-432.29f, 751.12f), new(-430.76f, 752.48f), new(-430.36f, 753.07f), new(-430.21f, 753.73f),
    new(-430.14f, 754.4f), new(-430.09f, 755.74f), new(-429.92f, 756.36f), new(-429.66f, 756.92f), new(-429.09f, 757.09f),
    new(-425.81f, 757.17f), new(-425.3f, 757.29f), new(-424.32f, 758.13f), new(-423.78f, 758.47f), new(-423.17f, 758.54f),
    new(-421.4f, 757.99f), new(-420.84f, 757.7f), new(-420.62f, 757.01f), new(-419.25f, 756.78f), new(-418.77f, 757.05f),
    new(-416.59f, 758.49f), new(-415.96f, 758.7f), new(-415.33f, 758.8f), new(-414.81f, 758.35f), new(-414.45f, 757.76f),
    new(-414.6f, 757.12f), new(-414.96f, 756.62f), new(-415.24f, 756.1f), new(-411.42f, 755.46f), new(-410.77f, 755.64f),
    new(-410.14f, 755.88f), new(-409.01f, 756.23f), new(-409.3f, 759.38f), new(-409.32f, 760.07f), new(-407.32f, 764.18f),
    new(-407.06f, 764.83f), new(-405.69f, 770.83f), new(-402.62f, 778.61f), new(-402.54f, 779.34f), new(-404.95f, 792.13f),
    new(-404.88f, 792.77f), new(-403.92f, 794.71f), new(-404.14f, 810.99f), new(-403.93f, 815.01f), new(-412.73f, 817.93f),
    new(-413.45f, 818f), new(-427.66f, 817.49f), new(-428.17f, 817.69f), new(-429.68f, 822.64f), new(-429.97f, 823.22f),
    new(-431.56f, 825.26f), new(-432.02f, 825.68f), new(-432.24f, 825.13f), new(-439.69f, 821.71f), new(-440.34f, 821.33f),
    new(-440.26f, 820.77f), new(-440.05f, 820.31f), new(-439.57f, 819.75f), new(-438.55f, 818.8f), new(-436.36f, 816.11f),
    new(-436.1f, 815.58f), new(-437.12f, 802.89f), new(-443.22f, 794.68f), new(-443.57f, 786.54f), new(-441.28f, 772.63f),
    new(-441.09f, 772), new(-439.51f, 770.54f), new(-439.04f, 770.02f), new(-438.72f, 769.42f), new(-438.92f, 768.84f),
    new(-439.31f, 768.31f), new(-439.91f, 768.12f), new(-440.55f, 767.85f), new(-441.14f, 764.48f), new(-441.09f, 763.75f),
    new(-440.8f, 761.66f), new(-441.05f, 761.09f), new(-440.93f, 759.67f), new(-440.93f, 758.95f), new(-441.22f, 758.38f),
    new(-446.26f, 750.17f), new(-446.79f, 749.85f), new(-447.17f, 749.25f), new(-447.33f, 748.6f), new(-448.51f, 748.28f),
    new(-449.12f, 747.96f), new(-449.59f, 747.54f), new(-450.1f, 747.35f), new(-451.23f, 748.07f), new(-451.84f, 748.27f),
    new(-452.81f, 748.98f), new(-456.82f, 751.19f), new(-468.09f, 752.08f), new(-468.62f, 752.04f), new(-468.02f, 743.77f),
    new(-468.1f, 743.06f), new(-469.6f, 735.88f), new(-469.96f, 735.35f), new(-486.77f, 718.37f), new(-491.67f, 715.35f),
    new(-497.16f, 715.52f), new(-497.7f, 715.1f), new(-500.81f, 712.09f), new(-501.05f, 711.64f), new(-502.98f, 705.53f),
    new(-503.24f, 704.97f), new(-503.82f, 704.9f), new(-505.33f, 704.92f), new(-505.74f, 704.59f), new(-506.99f, 702.21f),
    new(-506.76f, 701.71f), new(-503.9f, 698.73f), new(-503.63f, 698.19f), new(-503.54f, 697.49f), new(-503.31f, 696.83f),
    new(-503.55f, 696.32f), new(-504.41f, 695.37f), new(-504.92f, 695.35f), new(-506.16f, 695.72f), new(-506.72f, 695.68f),
    new(-505.95f, 688.81f), new(-505.83f, 688.18f), new(-505.76f, 687.58f), new(-505.52f, 686.9f), new(-500.62f, 686.9f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.ElderGobbue, (uint)OID.RobberCrab1, (uint)OID.RobberCrab2, (uint)OID.DeathCap,
    (uint)OID.BarkSpider1, (uint)OID.BarkSpider2, (uint)OID.Skimmer1, (uint)OID.Skimmer2];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Trash);
    }
}
