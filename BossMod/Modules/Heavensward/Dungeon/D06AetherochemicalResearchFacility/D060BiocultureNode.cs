namespace BossMod.Heavensward.Dungeon.D06AetherochemicalResearchFacility.D060BiocultureNode;

public enum OID : uint
{
    Boss = 0xF4D, // R2.3
    Boss1Hole = 0x1E9934, // R2.0
    Boss2Hole = 0x1E9935, // R2.0
    PreculturedBiomass = 0xF4E, // R1.0
    CulturedNaga = 0x10B0, // R0.9
    CulturedEmpuse = 0xF50, // R1.5
    CulturedCobra = 0x10B1, // R1.2
    CulturedDancer = 0xF4F, // R1.0
    CulturedBowyer = 0x10AF, // R1.0
    BiocultureNode = 0xF4D, // R2.3
    CulturedReptoid = 0xF51, // R3.0
    Biohazard = 0xF53, // R1.8
    CulturedMirrorknight = 0x10B2, // R1.7
    CulturedChimera = 0x10B3 // R3.7
}

public enum AID : uint
{
    AutoAttack1 = 872, // Biohazard/CulturedChimera/PreculturedBiomass/CulturedNaga->player, no cast, single-target
    AutoAttack2 = 870, // CulturedEmpuse/CulturedCobra/CulturedDancer->player, no cast, single-target
    AutoAttack3 = 871, // CulturedReptoid/CulturedMirrorknight->player, no cast, single-target

    Electrify = 4824, // BiocultureNode->self, no cast, range 30+R circle
    Shot = 4704, // CulturedBowyer->player, no cast, single-target
    Puncture = 4465, // CulturedEmpuse->self, 3.0s cast, range 4+R width 3 rect
    TailSlap = 4703, // CulturedDancer->self, 3.0s cast, range 6+R 120-degree cone
    CalcifyingMist = 4666, // CulturedNaga->self, 3.0s cast, range 6+R 90-degree cone, gaze
    StandingChine = 939, // CulturedEmpuse->player, no cast, single-target
    EerieSoundwave = 4464, // CulturedEmpuse->self, 3.0s cast, range 6+R circle

    HardThrust = 4518, // CulturedReptoid->player, no cast, single-target
    Gust = 917, // CulturedMirrorknight->location, 2.5s cast, range 3 circle
    Sideswipe = 4519, // CulturedReptoid->self, 3.0s cast, range 6+R 90-degree cone
    Bearclaw = 915, // CulturedMirrorknight->player, no cast, single-target
    MarrowDrain1 = 3342, // CulturedChimera->self, 3.0s cast, range 6+R 120-degree cone
    MarrowDrain2 = 3341, // CulturedChimera->self, 3.0s cast, range 6+R 120-degree cone
    MarrowDrain3 = 3340, // CulturedChimera->self, 3.0s cast, range 6+R 120-degree cone
    TheRamsVoice = 3343 // CulturedChimera->self, 3.0s cast, range 6+R circle
}

class Hole(BossModule module) : BossComponent(module)
{
    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008)
        {
            if (actor.OID == (uint)OID.Boss1Hole)
                Arena.Bounds = D060BiocultureNode.Arena1b;
            else if (actor.OID == (uint)OID.Boss2Hole)
                Arena.Bounds = D060BiocultureNode.Arena2b;
        }
    }
}

class Puncture(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Puncture, new AOEShapeRect(5.5f, 1.5f));
class TailSlap(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TailSlap, new AOEShapeCone(7f, 60f.Degrees()));
class CalcifyingMist(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CalcifyingMist, new AOEShapeCone(6.9f, 45f.Degrees()));
class EerieSoundwave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EerieSoundwave, 7.5f);

abstract class MarrowDrain(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(9.7f, 60f.Degrees()));
class MarrowDrain1(BossModule module) : MarrowDrain(module, (uint)AID.MarrowDrain1);
class MarrowDrain2(BossModule module) : MarrowDrain(module, (uint)AID.MarrowDrain2);
class MarrowDrain3(BossModule module) : MarrowDrain(module, (uint)AID.MarrowDrain3);

class TheRamsVoice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheRamsVoice, 9.7f);
class Sideswipe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Sideswipe, new AOEShapeCone(9f, 45f.Degrees()));
class Gust(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Gust, 3f);

class D060BiocultureNodeStates : StateMachineBuilder
{
    public D060BiocultureNodeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hole>()
            .ActivateOnEnter<Puncture>()
            .ActivateOnEnter<TailSlap>()
            .ActivateOnEnter<CalcifyingMist>()
            .ActivateOnEnter<EerieSoundwave>()
            .ActivateOnEnter<MarrowDrain1>()
            .ActivateOnEnter<MarrowDrain2>()
            .ActivateOnEnter<MarrowDrain3>()
            .ActivateOnEnter<TheRamsVoice>()
            .ActivateOnEnter<Sideswipe>()
            .ActivateOnEnter<Gust>()
            .Raw.Update = () => AllDeadOrDestroyed(module.Bounds == D060BiocultureNode.Arena1 || module.Bounds == D060BiocultureNode.Arena1b ? D060BiocultureNode.Trash1 : D060BiocultureNode.Trash2);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 38, NameID = 3830, SortOrder = 5)]
public class D060BiocultureNode(WorldState ws, Actor primary) : BossModule(ws, primary, IsArena1(primary) ? Arena1.Center : arena2.Center, IsArena1(primary) ? Arena1 : arena2)
{
    public static bool IsArena1(Actor primary) => primary.PosRot.Z < 250f;

    private static readonly WPos[] vertices1 = [new(26.19f, 163.9f), new(29.99f, 164.07f), new(30.59f, 164.12f), new(31.3f, 164.26f), new(31.59f, 164.85f),
    new(31.81f, 165.42f), new(32, 166.01f), new(32.24f, 166.6f), new(34.76f, 168.55f), new(34.99f, 169.17f),
    new(35.98f, 170.85f), new(36.08f, 193.26f), new(36.16f, 193.81f), new(40.19f, 197.85f), new(40.6f, 198.35f),
    new(40.94f, 198.88f), new(41.12f, 199.36f), new(41.23f, 200.02f), new(41.62f, 200.51f), new(43.27f, 202.4f),
    new(43.77f, 202.77f), new(45.92f, 205.07f), new(46.41f, 205.39f), new(49.02f, 208.15f), new(49.53f, 208.52f),
    new(51.69f, 210.84f), new(52.2f, 211.19f), new(54.38f, 213.51f), new(54.87f, 213.92f), new(57.07f, 216.22f),
    new(57.55f, 216.67f), new(58.04f, 217.1f), new(60.25f, 219.38f), new(60.79f, 219.82f), new(60.6f, 220.29f),
    new(60.15f, 220.72f), new(59.71f, 221.13f), new(59.38f, 221.68f), new(57.08f, 223.79f), new(56.72f, 224.31f),
    new(54.4f, 226.46f), new(53.98f, 226.87f), new(53.62f, 227.39f), new(51.31f, 229.56f), new(50.87f, 230.09f),
    new(48.55f, 232.3f), new(48.28f, 232.83f), new(45.86f, 235.02f), new(45.45f, 235.48f), new(45, 235.97f),
    new(42.68f, 238.14f), new(42.29f, 238.66f), new(40.94f, 240.01f), new(40.5f, 240.48f), new(40.09f, 240.97f),
    new(40.02f, 241.5f), new(38.84f, 243.09f), new(38.44f, 243.61f), new(38.02f, 244.11f), new(36.3f, 246.04f),
    new(36.08f, 246.79f), new(36.06f, 264.86f), new(20.21f, 264.97f), new(19.92f, 246.65f), new(19.8f, 246.14f),
    new(15.65f, 241.99f), new(14.94f, 240.92f), new(15.02f, 240.28f), new(14.65f, 239.81f), new(14.25f, 239.32f),
    new(13.01f, 237.9f), new(12.56f, 237.47f), new(12.05f, 237.04f), new(9.9f, 234.74f), new(9.35f, 234.37f),
    new(7.18f, 232.05f), new(6.68f, 231.7f), new(2.77f, 227.51f), new(-1.34f, 223.5f), new(-1.84f, 223.11f),
    new(-2.28f, 222.66f), new(-3.74f, 221.09f), new(-4.21f, 220.64f), new(-4.84f, 220.68f), new(-5.37f, 220.98f),
    new(-5.14f, 220.44f), new(1.82f, 213.35f), new(2.04f, 212.83f), new(2.51f, 212.38f), new(2.96f, 211.96f),
    new(4.78f, 210.36f), new(5.08f, 209.8f), new(7.38f, 207.77f), new(7.77f, 207.32f), new(8.69f, 206.48f),
    new(9.13f, 206.05f), new(10.41f, 204.73f), new(10.39f, 204.2f), new(10.68f, 203.6f), new(11.97f, 202.08f),
    new(12.38f, 201.63f), new(12.91f, 201.22f), new(13.41f, 201.18f), new(15.82f, 199.07f), new(16.04f, 198.53f),
    new(16.36f, 198.09f), new(16.4f, 197.47f), new(16.77f, 196.88f), new(19.79f, 193.86f), new(19.95f, 167.03f),
    new(20.8f, 166.14f), new(20.85f, 164.66f), new(21.34f, 164.33f), new(25.24f, 164.32f), new(25.73f, 164.04f)];
    private static readonly WPos[] vertices2 = [new(119.47f, 228.45f), new(119.98f, 228.57f), new(119.98f, 245.6f), new(120.27f, 246.05f), new(125.43f, 251.12f),
    new(125.92f, 251.71f), new(124.05f, 253.58f), new(123.61f, 254.06f), new(123.35f, 254.57f), new(123.53f, 255.12f),
    new(123.93f, 255.62f), new(124.41f, 256.05f), new(125.88f, 257.35f), new(126.39f, 257.74f), new(126.92f, 258.12f),
    new(127.43f, 258.55f), new(127.82f, 259.09f), new(127.95f, 259.75f), new(127.97f, 260.28f), new(124.69f, 262.5f),
    new(124.26f, 262.91f), new(124.31f, 263.56f), new(124.11f, 264.21f), new(123.92f, 264.76f), new(124.35f, 265.16f),
    new(124.88f, 265.4f), new(125.56f, 265.41f), new(136.78f, 265.41f), new(137.21f, 265.12f), new(137.39f, 264.49f),
    new(137.5f, 263.91f), new(137.43f, 263.31f), new(137.16f, 262.78f), new(136.65f, 262.6f), new(134.43f, 262.6f),
    new(133.83f, 262.61f), new(133.18f, 262.66f), new(132.49f, 262.79f), new(131.88f, 262.59f), new(131.45f, 262.23f),
    new(139.71f, 253.95f), new(140.15f, 253.53f), new(142.37f, 251.45f), new(142.79f, 251.02f), new(143.22f, 250.58f),
    new(144.87f, 248.79f), new(146.17f, 247.5f), new(154.88f, 238.78f), new(155.37f, 238.33f), new(155.84f, 237.86f),
    new(156.34f, 238.01f), new(157.31f, 238.99f), new(157.72f, 239.59f), new(157.25f, 240.09f), new(155.91f, 241.41f),
    new(155.48f, 241.89f), new(155.35f, 242.44f), new(155.74f, 242.88f), new(156.18f, 243.3f), new(159.31f, 246.46f),
    new(160.21f, 247.35f), new(160.81f, 247.64f), new(164.01f, 250.87f), new(164.5f, 251.29f), new(168.2f, 255.04f),
    new(168.7f, 255.43f), new(169.29f, 255.76f), new(169.76f, 256.22f), new(171.24f, 257.54f), new(171.72f, 258f),
    new(172.65f, 258.95f), new(173.15f, 259.33f), new(173.69f, 259.77f), new(176.39f, 262.67f), new(176.84f, 263.15f),
    new(177.31f, 263.53f), new(177.83f, 263.44f), new(178.29f, 263.02f), new(179.62f, 261.69f), new(180.23f, 261.89f),
    new(182.18f, 263.84f), new(202.04f, 263.92f), new(209.08f, 279.77f), new(208.68f, 280.07f), new(182.47f, 280.07f),
    new(181.97f, 280.37f), new(180.63f, 281.71f), new(180.17f, 281.52f), new(177.97f, 279.31f), new(177.53f, 278.89f),
    new(177.04f, 278.56f), new(176.44f, 278.95f), new(175.99f, 279.42f), new(172.23f, 283.16f), new(171.83f, 283.65f),
    new(171.85f, 284.3f), new(170.55f, 285.59f), new(168.59f, 287.39f), new(168.12f, 287.87f), new(167.67f, 288.36f),
    new(166.69f, 289.25f), new(163.89f, 292.04f), new(163.6f, 292.51f), new(163.71f, 293.14f), new(161.48f, 295.39f),
    new(160.06f, 296.77f), new(159.67f, 297.26f), new(159.29f, 297.84f), new(158.82f, 298.29f), new(158.33f, 298.73f),
    new(155.93f, 300.84f), new(155.5f, 301.27f), new(155.4f, 301.83f), new(155.71f, 302.44f), new(154.08f, 304.28f),
    new(153.62f, 303.97f), new(130.76f, 281.41f), new(131.31f, 281.08f), new(133.19f, 281.34f), new(133.81f, 281.39f),
    new(136.69f, 281.4f), new(137.16f, 281.22f), new(137.24f, 279.01f), new(136.77f, 278.59f), new(125.01f, 278.59f),
    new(124.46f, 278.69f), new(124.35f, 279.48f), new(124.2f, 280.12f), new(124.22f, 280.77f), new(124.46f, 281.33f),
    new(127.64f, 281.43f), new(128.05f, 281.73f), new(128.48f, 282.25f), new(128.89f, 282.79f), new(129.31f, 283.38f),
    new(129.05f, 283.82f), new(124.47f, 288.62f), new(124.06f, 289.09f), new(123.68f, 289.59f), new(123.55f, 290.15f),
    new(123.87f, 290.62f), new(125.26f, 292.01f), new(125.65f, 292.62f), new(120.19f, 297.99f), new(119.98f, 315.39f),
    new(105.88f, 315.48f), new(104.09f, 315.45f), new(103.83f, 298.43f), new(103.6f, 297.96f), new(100.27f, 294.62f),
    new(99.85f, 294.15f), new(100, 293.58f), new(101.46f, 292.12f), new(102.05f, 291.77f), new(102.03f, 291.14f),
    new(101.61f, 290.68f), new(101.13f, 290.28f), new(98.12f, 286.99f), new(97.66f, 286.56f), new(97.19f, 286.18f),
    new(96.61f, 286.09f), new(96.13f, 286.47f), new(94.78f, 287.82f), new(94.29f, 288.25f), new(93.64f, 287.97f),
    new(87.55f, 281.88f), new(87.29f, 281.26f), new(86.52f, 278.24f), new(85.79f, 276.54f), new(85.41f, 276.19f),
    new(83.53f, 276.03f), new(82.9f, 275.88f), new(82.28f, 275.82f), new(75.1f, 278.11f), new(73.53f, 273.24f),
    new(73.51f, 272.74f), new(74.46f, 271.85f), new(75.01f, 271.49f), new(75.63f, 271.27f), new(76.29f, 271.09f),
    new(77.51f, 270.97f), new(78.15f, 270.98f), new(78.74f, 270.92f), new(91.57f, 258.12f), new(92.08f, 257.66f),
    new(92.72f, 257.66f), new(95.36f, 260.29f), new(95.81f, 260.65f), new(96.33f, 260.56f), new(96.78f, 260.21f),
    new(99.38f, 257.43f), new(99.83f, 256.97f), new(100.31f, 256.55f), new(100.76f, 256.12f), new(101.19f, 255.7f),
    new(101.16f, 255.18f), new(100.78f, 254.71f), new(98.07f, 252), new(98.2f, 251.49f), new(98.72f, 250.97f),
    new(99.2f, 250.46f), new(103.7f, 245.95f), new(103.85f, 228.55f), new(119.47f, 228.45f)];
    public static readonly ArenaBoundsCustom Arena1 = new([new PolygonCustom(vertices1)], [new Polygon(new(28.122f, 220.05f), 2.5f, 16)]);
    public static readonly ArenaBoundsCustom Arena1b = new([new PolygonCustom(vertices1)]);
    private static readonly ArenaBoundsCustom arena2 = new([new PolygonCustom(vertices2)], [new Polygon(new(111.925f, 271.931f), 2.5f, 16)]);
    public static readonly ArenaBoundsCustom Arena2b = new([new PolygonCustom(vertices2)]);

    public static readonly uint[] Trash1 = [(uint)OID.Boss, (uint)OID.PreculturedBiomass, (uint)OID.CulturedCobra, (uint)OID.CulturedNaga,
    (uint)OID.CulturedBowyer, (uint)OID.CulturedEmpuse, (uint)OID.CulturedDancer];
    public static readonly uint[] Trash2 = [(uint)OID.Boss, (uint)OID.CulturedChimera, (uint)OID.BiocultureNode, (uint)OID.CulturedReptoid, (uint)OID.CulturedMirrorknight,
    (uint)OID.Biohazard];

    protected override bool CheckPull() => Arena.InBounds(Raid.Player()!.Position);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        if (Bounds == Arena1 || Bounds == Arena1b)
        {
            Arena.Actors(this, Trash1);
        }
        else
        {
            Arena.Actors(this, Trash2);
        }
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
            hints.PotentialTargets[i].Priority = 0;
    }
}
