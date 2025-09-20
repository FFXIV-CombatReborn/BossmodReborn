namespace BossMod.Endwalker.Dungeon.D12Aetherfont.D120HaamTroll;

public enum OID : uint
{
    Boss = 0x3ECB, // R3.0
    HaamYakow = 0x3ECC, // R4.6
    HaamAuk = 0x3ECA // R1.25
}

public enum AID : uint
{
    AutoAttack1 = 872, // Boss->player, no cast, single-target
    AutoAttack2 = 870, // HaamAuk->player, no cast, single-target
    AutoAttack3 = 871, // HaamYakow->player, no cast, single-target

    Uppercut = 33994 // Boss->self, 3.0s cast, range 11 120-degree cone
}

class Uppercut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Uppercut, new AOEShapeCone(11, 60.Degrees()));

class D120HaamTrollStates : StateMachineBuilder
{
    public D120HaamTrollStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Uppercut>()
            .Raw.Update = () => AllDeadOrDestroyedInBounds(D120HaamTroll.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 822, NameID = 12343, SortOrder = 4)]
public class D120HaamTroll(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(233.1f, -28.18f), new(233.61f, -28.08f), new(234.11f, -27.92f), new(234.53f, -27.63f), new(234.76f, -27.11f),
    new(236.6f, -21.16f), new(236.79f, -20.69f), new(239.55f, -17.01f), new(239.96f, -16.64f), new(240.4f, -16.4f),
    new(240.88f, -16.24f), new(241.4f, -16.24f), new(241.91f, -16.28f), new(242.83f, -16.76f), new(243.33f, -16.97f),
    new(243.84f, -17.09f), new(244.38f, -17.12f), new(244.91f, -17.08f), new(245.44f, -17.12f), new(245.97f, -17.03f),
    new(246.39f, -16.75f), new(246.65f, -16.29f), new(247.17f, -16.09f), new(247.68f, -15.95f), new(248.19f, -15.98f),
    new(251.94f, -16.53f), new(252.4f, -16.76f), new(256.33f, -19.39f), new(256.84f, -19.49f), new(263.13f, -18.26f),
    new(263.53f, -17.96f), new(266.53f, -13.61f), new(266.69f, -13.13f), new(266.33f, -12.13f), new(266.59f, -11.7f),
    new(266.97f, -11.34f), new(267.41f, -11.08f), new(267.9f, -10.87f), new(268.95f, -10.94f), new(269.8f, -11.5f),
    new(271.05f, -11.73f), new(271.81f, -11.72f), new(273.1f, -10.8f), new(273.41f, -10.39f), new(273.82f, -8.93f),
    new(273.68f, -8.42f), new(273.32f, -7.46f), new(273.01f, -7.04f), new(272.13f, -6.44f), new(271.71f, -5.45f),
    new(271.83f, -4.42f), new(272.3f, -4.19f), new(272.74f, -3.92f), new(273.3f, -3.78f), new(273.71f, -3.47f),
    new(274.7f, -2.18f), new(277.22f, 1.7f), new(277.12f, 2.2f), new(267.54f, 9.69f), new(265.87f, 11.69f),
    new(265.5f, 12.03f), new(265.22f, 12.49f), new(264.21f, 13.7f), new(263.93f, 14.14f), new(264.12f, 14.61f),
    new(266.36f, 18.17f), new(266.67f, 18.57f), new(270.62f, 21.31f), new(271.04f, 21.68f), new(271.37f, 22.12f),
    new(271.9f, 22.3f), new(272.43f, 22.31f), new(279.77f, 17.07f), new(280.27f, 17.22f), new(280.4f, 17.73f),
    new(279.97f, 22.94f), new(279.89f, 23.44f), new(279.81f, 24.45f), new(279.48f, 24.84f), new(277.42f, 24.38f),
    new(274.24f, 24.6f), new(273.75f, 24.72f), new(271.95f, 25.77f), new(271.55f, 26.09f), new(269.23f, 30.37f),
    new(269.08f, 30.85f), new(269.11f, 31.36f), new(268.73f, 31.69f), new(267.3f, 31.14f), new(266.77f, 31.09f),
    new(255.31f, 31.77f), new(254.78f, 31.86f), new(254.27f, 32), new(253.18f, 32.1f), new(252.78f, 32.47f),
    new(252.6f, 32.96f), new(252.53f, 36.12f), new(252.76f, 37.66f), new(252.25f, 37.69f), new(247.53f, 37.42f),
    new(245.97f, 37.74f), new(245.54f, 38.07f), new(245.39f, 38.55f), new(244.88f, 38.59f), new(241.86f, 38.07f),
    new(241.36f, 38.22f), new(239.56f, 40.02f), new(239.03f, 40.12f), new(236.55f, 38.1f), new(236.42f, 37.57f),
    new(235.99f, 35.04f), new(235.83f, 34.55f), new(232, 28.39f), new(231.92f, 27.88f), new(230.62f, 17.42f),
    new(228.74f, 13.74f), new(228.31f, 13.46f), new(219.74f, 11.63f), new(219.22f, 11.46f), new(215.47f, 7.08f),
    new(215.17f, 6.63f), new(214.45f, 4.65f), new(214.3f, 4.11f), new(215.11f, -1.15f), new(214.56f, -3.23f),
    new(213.87f, -4.59f), new(213.59f, -5), new(210.16f, -8.84f), new(209.64f, -8.87f), new(207.15f, -8.26f),
    new(205.02f, -8.42f), new(204.54f, -8.22f), new(203.47f, -7.09f), new(203.16f, -6.66f), new(202.67f, -5.72f),
    new(202.29f, -5.37f), new(201.32f, -4.87f), new(197.24f, -5.23f), new(196.71f, -5.15f), new(195.21f, -4.57f),
    new(194.68f, -4.66f), new(193.68f, -4.97f), new(191.57f, -4.68f), new(191.06f, -4.85f), new(189.65f, -5.54f),
    new(189.22f, -5.83f), new(187.32f, -7.55f), new(186.83f, -7.73f), new(183.82f, -8.55f), new(183.3f, -8.65f),
    new(182.84f, -8.41f), new(179.87f, -6.35f), new(179.36f, -6.27f), new(174.69f, -6.78f), new(169.81f, -8.6f),
    new(169.31f, -8.72f), new(168.82f, -8.96f), new(166.97f, -17.99f), new(167.32f, -18.35f), new(167.76f, -18.62f),
    new(170.78f, -19.8f), new(173.42f, -19.68f), new(177.01f, -21.81f), new(179.36f, -20.64f), new(179.85f, -20.51f),
    new(187.24f, -19.82f), new(187.77f, -19.81f), new(189.73f, -21.69f), new(192.17f, -22.78f), new(192.66f, -22.86f),
    new(200.27f, -22.21f), new(200.82f, -22.11f), new(204.2f, -20.75f), new(204.65f, -20.5f), new(206.78f, -19.03f),
    new(207.14f, -18.68f), new(207.88f, -16.66f), new(210.03f, -15.08f), new(210.46f, -14.83f), new(215.98f, -13.07f),
    new(216.51f, -12.98f), new(219.13f, -13.13f), new(219.53f, -13.48f), new(220.41f, -15.45f), new(220.72f, -15.86f),
    new(222.11f, -17.34f), new(222.38f, -17.78f), new(223.33f, -21.28f), new(223.72f, -21.62f), new(224.19f, -21.91f),
    new(224.52f, -22.31f), new(226.67f, -25.82f), new(227.09f, -26.13f), new(229.46f, -27.45f), new(229.94f, -27.64f),
    new(233.1f, -28.18f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.HaamYakow, (uint)OID.HaamAuk];

    protected override bool CheckPull() => IsAnyActorInBoundsInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Trash);
    }
}
