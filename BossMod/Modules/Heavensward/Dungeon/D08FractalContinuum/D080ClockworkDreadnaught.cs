namespace BossMod.Heavensward.Dungeon.D08FractalContinuum.D080ClockworkDreadnaught;

public enum OID : uint
{
    Boss = 0x1020, // R3.0
    ImmortalizedClockworkSoldier = 0x101E, // R1.225
    ImmortalizedClockworkKnight = 0x101F // R1.225
}

public enum AID : uint
{
    AutoAttack = 872, // ImmortalizedClockworkSoldier/ImmortalizedClockworkKnight/Boss->player, no cast, single-target

    Rotoswipe = 4556 // Boss->self, 3.0s cast, range 8+R 120-degree cone
}

class Rotoswipe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Rotoswipe, new AOEShapeCone(11f, 60f.Degrees()));

class D080ClockworkDreadnaughtStates : StateMachineBuilder
{
    public D080ClockworkDreadnaughtStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Rotoswipe>()
            .Raw.Update = () => AllDeadOrDestroyedInBounds(D080ClockworkDreadnaught.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 35, NameID = 3788, SortOrder = 1)]
public class D080ClockworkDreadnaught(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(233f, 57.46f), new(233.37f, 58.04f), new(233.87f, 58.5f), new(236.17f, 67.23f), new(237.26f, 79.69f),
    new(236.37f, 90.45f), new(235.97f, 91.04f), new(235.4f, 94.18f), new(235.36f, 94.79f), new(235.59f, 95.38f),
    new(234.64f, 99.23f), new(234.28f, 99.75f), new(233.74f, 99.94f), new(233.39f, 100.38f), new(233.16f, 101.04f),
    new(233.21f, 101.66f), new(233.55f, 102.18f), new(234.05f, 102.54f), new(234.65f, 102.64f), new(235.24f, 102.55f),
    new(235.86f, 102.34f), new(236.25f, 102.66f), new(236.18f, 103.29f), new(234.95f, 105.04f), new(235.21f, 105.51f),
    new(235.74f, 105.9f), new(236.19f, 106.36f), new(236.78f, 106.68f), new(237.31f, 106.79f), new(237.69f, 106.37f),
    new(238.47f, 105.33f), new(238.89f, 105.02f), new(238.84f, 105.52f), new(239.34f, 106.6f), new(240.46f, 107.09f),
    new(241.06f, 106.91f), new(241.44f, 107.26f), new(243.77f, 110.3f), new(244.11f, 110.87f), new(246.29f, 116.18f),
    new(247.11f, 122.55f), new(246.61f, 126.34f), new(246.37f, 126.97f), new(245.77f, 127.2f), new(245.38f, 127.72f),
    new(245.07f, 128.24f), new(245.45f, 130.11f), new(243.37f, 133.91f), new(240.14f, 138.45f), new(239.72f, 138.93f),
    new(235.44f, 142.71f), new(231.22f, 144.49f), new(230.64f, 144.11f), new(230.06f, 143.98f), new(229.43f, 143.9f),
    new(228.93f, 144.17f), new(228.43f, 144.57f), new(228.2f, 145.11f), new(227.78f, 145.4f), new(223.89f, 145.91f),
    new(223.21f, 145.92f), new(217.55f, 145.15f), new(211.67f, 142.73f), new(208.47f, 140.28f), new(208.23f, 139.67f),
    new(208.27f, 139.09f), new(207.96f, 138.45f), new(207.56f, 138f), new(206.49f, 137.59f), new(206.86f, 137.17f),
    new(207.89f, 136.39f), new(208.06f, 135.89f), new(206.5f, 133.91f), new(206f, 133.96f), new(204.46f, 135.14f),
    new(203.96f, 135.24f), new(203.57f, 134.69f), new(203.87f, 133.42f), new(203.72f, 132.82f), new(203.38f, 132.34f),
    new(202.87f, 132.04f), new(202.26f, 131.97f), new(201.58f, 132.14f), new(199.27f, 124.81f), new(199.86f, 124.79f),
    new(200.42f, 124.6f), new(200.86f, 124.2f), new(201.11f, 123.63f), new(201.1f, 123.05f), new(200.86f, 122.5f),
    new(200.45f, 122.08f), new(200.06f, 121.59f), new(200.25f, 120.86f), new(202.25f, 121.08f), new(202.86f, 121.1f),
    new(203.14f, 120.64f), new(203.21f, 119.98f), new(203.37f, 119.4f), new(203.37f, 118.75f), new(203.22f, 118.2f),
    new(202.69f, 118.06f), new(200.94f, 117.86f), new(201.47f, 117.55f), new(201.8f, 116.99f), new(202.03f, 116.43f),
    new(201.94f, 115.78f), new(201.7f, 114.59f), new(202.32f, 114.68f), new(204.4f, 116.31f), new(208.61f, 118.24f),
    new(209.86f, 118.19f), new(210.52f, 118.38f), new(213.73f, 121.39f), new(214.21f, 121.68f), new(214.82f, 121.43f),
    new(215.38f, 121.45f), new(216.75f, 120.1f), new(217.37f, 120.25f), new(217.97f, 120.63f), new(217.7f, 122f),
    new(217.74f, 122.6f), new(218f, 123.9f), new(217.97f, 124.51f), new(218.42f, 124.91f), new(219.16f, 126.01f),
    new(219.55f, 126.45f), new(220.64f, 127.21f), new(221.09f, 127.62f), new(221.8f, 127.7f), new(223.67f, 128.02f),
    new(224.91f, 127.78f), new(225.58f, 127.74f), new(227.7f, 126.23f), new(228.81f, 124.67f), new(229.36f, 122.15f),
    new(229.11f, 120.92f), new(229.15f, 120.28f), new(228.88f, 119.76f), new(228.41f, 119.25f), new(227.69f, 118.16f),
    new(227.25f, 117.79f), new(226.16f, 117.06f), new(225.87f, 116.65f), new(225.23f, 116.69f), new(224.6f, 116.57f),
    new(224.22f, 116.01f), new(222.51f, 109.64f), new(222.42f, 109.03f), new(222.22f, 108.36f), new(221.97f, 107.75f),
    new(218.57f, 105.51f), new(218f, 105.28f), new(214.19f, 104.61f), new(213.63f, 104.21f), new(212.42f, 101.83f),
    new(212.83f, 101.5f), new(215.24f, 100.5f), new(215.88f, 100.49f), new(216.33f, 100.88f), new(216.93f, 100.89f),
    new(217.58f, 100.84f), new(218.14f, 100.59f), new(218.65f, 100.2f), new(219.04f, 100.67f), new(219.35f, 102.05f),
    new(219.84f, 102.2f), new(220.5f, 102.11f), new(221.2f, 102.26f), new(221.76f, 102.04f), new(222.27f, 101.65f),
    new(222.25f, 101.05f), new(222.08f, 99.73f), new(222.12f, 98.95f), new(223.92f, 99.82f), new(224.59f, 99.94f),
    new(225.31f, 99.85f), new(225.85f, 99.6f), new(226.12f, 99.03f), new(225.99f, 98.3f), new(225.76f, 97.66f),
    new(225.38f, 97.11f), new(226.1f, 94.65f), new(226.37f, 94.23f), new(227.28f, 93.16f), new(227.41f, 92.51f),
    new(228.18f, 90.47f), new(228.05f, 89.88f), new(227.72f, 89.32f), new(227.29f, 88.85f), new(228.05f, 80.15f),
    new(227.09f, 68.84f), new(225.69f, 63.23f), new(226.12f, 62.65f), new(226.17f, 62.11f), new(226.08f, 61.45f),
    new(225.86f, 60.92f), new(225.44f, 60.29f), new(225.86f, 59.98f), new(232.5f, 57.45f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.ImmortalizedClockworkSoldier, (uint)OID.ImmortalizedClockworkKnight];

    protected override bool CheckPull() => IsAnyActorInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.ActorsInBounds(this, Trash);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
            hints.PotentialTargets[i].Priority = 0;
    }
}
