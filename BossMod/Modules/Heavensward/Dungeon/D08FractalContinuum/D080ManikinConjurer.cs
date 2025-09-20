namespace BossMod.Heavensward.Dungeon.D08FractalContinuum.D080ManikinConjurer;

public enum OID : uint
{
    Boss = 0x101D, // R0.5
    ImmortalizedClockworkKnight = 0x101F, // R1.225
    ManikinMarauder = 0x101B, // R0.5
    ManikinPugilist = 0x101C, // R0.5
    ImmortalizedClockworkSoldier = 0x101E // R1.225
}

public enum AID : uint
{
    AutoAttack1 = 872, // ImmortalizedClockworkSoldier/ManikinPugilist/ImmortalizedClockworkKnight->player, no cast, single-target
    AutoAttack2 = 870, // ManikinMarauder->player, no cast, single-target

    Rive = 1135, // ManikinMarauder->self, 2.5s cast, range 30+R width 2 rect
    Stone = 970, // Boss->player, 1.0s cast, single-target
    Overpower = 720 // ManikinMarauder->self, 2.5s cast, range 6+R 90-degree cone
}

class Overpower(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Overpower, new AOEShapeCone(6.5f, 45f.Degrees()));
class Rive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Rive, new AOEShapeRect(30.5f, 1f));

class D080ManikinConjurerStates : StateMachineBuilder
{
    public D080ManikinConjurerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Overpower>()
            .ActivateOnEnter<Rive>()
            .Raw.Update = () => AllDeadOrDestroyed(D080ManikinConjurer.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 35, NameID = 3413, SortOrder = 2)]
public class D080ManikinConjurer(WorldState ws, Actor primary) : BossModule(ws, primary, arena1.Center, arena1)
{
    private static readonly WPos[] vertices = [new(218.43f, 3.56f), new(223.97f, 5.82f), new(224.57f, 6.18f), new(229.26f, 9.84f), new(231.64f, 12.95f),
    new(231.83f, 13.58f), new(231.6f, 14.12f), new(231.69f, 14.78f), new(231.86f, 15.32f), new(232.39f, 15.73f),
    new(232.93f, 16.02f), new(233.43f, 16.43f), new(235.61f, 26.55f), new(235.27f, 32.01f), new(235.15f, 32.71f),
    new(233.62f, 36.3f), new(233.12f, 36.74f), new(232.57f, 36.87f), new(232.05f, 37.28f), new(231.7f, 37.72f),
    new(231.58f, 38.32f), new(231.66f, 38.93f), new(231.03f, 39.06f), new(229.74f, 38.42f), new(229.24f, 38.45f),
    new(228f, 40.02f), new(227.79f, 40.54f), new(228.13f, 40.92f), new(229.66f, 42.12f), new(229.15f, 41.97f),
    new(228.59f, 42.2f), new(227.98f, 42.58f), new(227.46f, 43.03f), new(227.19f, 43.64f), new(227.21f, 44.31f),
    new(227.44f, 44.85f), new(228.04f, 45.33f), new(220.89f, 48.26f), new(220.28f, 47.89f), new(219.7f, 47.71f),
    new(219.04f, 47.76f), new(218.49f, 48.17f), new(218.1f, 49.17f), new(217.91f, 48.59f), new(217.81f, 47.86f),
    new(217.48f, 47.35f), new(216.88f, 46.91f), new(216.33f, 46.67f), new(214.48f, 46.93f), new(213.96f, 47.16f),
    new(213.93f, 47.71f), new(214.21f, 49.73f), new(213.95f, 49.29f), new(212.65f, 48.73f), new(212.07f, 48.8f),
    new(211.45f, 49.07f), new(211.05f, 49.57f), new(207.33f, 48.72f), new(207.56f, 48.09f), new(208.6f, 47.24f),
    new(208.83f, 46.68f), new(208.97f, 46.04f), new(209.36f, 44.8f), new(209.36f, 44.15f), new(209.75f, 43.58f),
    new(210.29f, 43.16f), new(212.52f, 41.83f), new(212.74f, 40.69f), new(212.8f, 40.02f), new(212.76f, 36.77f),
    new(212.6f, 36.26f), new(212.28f, 35.71f), new(212.22f, 32.46f), new(212.63f, 32.16f), new(213.93f, 31.91f),
    new(214.6f, 31.91f), new(214.98f, 31.55f), new(216.56f, 30.52f), new(217.6f, 28.96f), new(217.97f, 28.58f),
    new(217.95f, 27.94f), new(218.2f, 26.71f), new(218.25f, 26.14f), new(218.01f, 24.93f), new(217.97f, 24.29f),
    new(216.6f, 22.26f), new(215.58f, 21.57f), new(214.63f, 20.83f), new(213.97f, 20.84f), new(212.64f, 20.57f),
    new(212.05f, 20.57f), new(211.4f, 20.7f), new(210.85f, 20.38f), new(210.36f, 20.59f), new(209.96f, 21.05f),
    new(209.44f, 21.48f), new(208.39f, 22.18f), new(208.01f, 22.65f), new(207.07f, 23.31f), new(206.94f, 23.88f),
    new(206.98f, 24.44f), new(206.58f, 26.37f), new(206.69f, 26.95f), new(206.45f, 27.57f), new(205.99f, 28f),
    new(204.04f, 30.5f), new(200.44f, 33.95f), new(200.11f, 34.46f), new(199.4f, 36.26f), new(198.85f, 36.71f),
    new(196.84f, 37.29f), new(193.57f, 39.67f), new(193.08f, 39.46f), new(193.39f, 38.85f), new(193.4f, 38.34f),
    new(193.21f, 37.73f), new(192.85f, 37.21f), new(192.32f, 36.84f), new(191.65f, 36.71f), new(192.11f, 36.35f),
    new(193.47f, 35.78f), new(193.81f, 35.38f), new(192.86f, 33.09f), new(192.18f, 33.14f), new(191f, 33.63f),
    new(190.58f, 33.31f), new(190.15f, 32.83f), new(190.39f, 32.37f), new(190.58f, 31.79f), new(190.54f, 31.13f),
    new(190.24f, 30.61f), new(189.77f, 30.26f), new(189.13f, 30.13f), new(188.51f, 30.28f), new(188f, 30.68f),
    new(187.54f, 30.9f), new(184.19f, 30.25f), new(183.8f, 29.75f), new(183.29f, 29.53f), new(182.89f, 29.01f),
    new(183.64f, 22.21f), new(183.86f, 21.51f), new(184.47f, 21.44f), new(184.99f, 20.99f), new(186.44f, 21.21f),
    new(187.05f, 21.54f), new(188.11f, 22.25f), new(188.68f, 22.52f), new(189.26f, 22.6f), new(189.85f, 22.43f),
    new(190.3f, 22.02f), new(190.59f, 21.46f), new(190.59f, 20.86f), new(190.33f, 20.29f), new(189.87f, 19.77f),
    new(190.24f, 19.08f), new(191.85f, 19.81f), new(192.43f, 20.01f), new(192.88f, 19.71f), new(193.61f, 17.95f),
    new(193.71f, 17.4f), new(193.28f, 17.11f), new(191.35f, 16.24f), new(192.43f, 15.8f), new(192.96f, 15.38f),
    new(193.23f, 14.85f), new(193.31f, 14.17f), new(193.11f, 13.63f), new(193.48f, 13.05f), new(200.62f, 6.52f),
    new(201.2f, 6.15f), new(206.07f, 3.73f), new(206.71f, 3.53f), new(210.47f, 3.06f), new(210.97f, 3.11f),
    new(211.36f, 3.65f), new(211.99f, 3.9f), new(212.55f, 4.05f), new(213.72f, 3.54f), new(214.11f, 2.99f)];
    private static readonly ArenaBoundsCustom arena1 = new([new PolygonCustom(vertices)]);
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.ImmortalizedClockworkSoldier, (uint)OID.ImmortalizedClockworkKnight, (uint)OID.ManikinMarauder,
    (uint)OID.ManikinPugilist];

    protected override bool CheckPull() => IsAnyActorInBoundsInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Trash);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
            hints.PotentialTargets[i].Priority = 0;
    }
}
