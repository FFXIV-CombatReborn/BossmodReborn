namespace BossMod.Heavensward.Dungeon.D13SohrKhai.D130Kargas;

public enum OID : uint
{
    Boss = 0x160E, // R2.875
    SanctuaryTsanahale = 0x160F, // R1.2
    SohrKhaiAnzu = 0x160C, // R3.6
    SohrKhaiCockerel = 0x160A // R0.4
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 871, // SohrKhaiAnzu/SanctuaryTsanahale/SohrKhaiCockerel->player, no cast, single-target

    BreathWing = 6107, // SohrKhaiAnzu->self, 4.0s cast, range 50 circle
    GoldenTalons = 4690, // Boss->player, no cast, single-target
    WingsOfWoe = 2478 // SanctuaryTsanahale->location, 2.5s cast, range 6 circle
}

class BreathWing(BossModule module) : Components.RaidwideCast(module, (uint)AID.BreathWing);
class WingsOfWoe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WingsOfWoe, 6f);

class D130KargasStates : StateMachineBuilder
{
    public D130KargasStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BreathWing>()
            .ActivateOnEnter<WingsOfWoe>()
            .Raw.Update = () => AllDeadOrDestroyed(D130Kargas.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 171, NameID = 4939, SortOrder = 3)]
public class D130Kargas(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(402.46f, 156), new(403.14f, 156.09f), new(403.76f, 156.32f), new(404.33f, 156.69f), new(404.79f, 157.18f),
    new(405.13f, 157.79f), new(405.31f, 158.42f), new(405.38f, 159.12f), new(404.98f, 159.52f), new(404.92f, 160.13f),
    new(405.31f, 160.84f), new(405.18f, 161.47f), new(405.05f, 162.18f), new(405.03f, 162.79f), new(405.65f, 162.93f),
    new(406.53f, 168.8f), new(406.41f, 169.32f), new(406.26f, 170.05f), new(406.13f, 170.78f), new(406.06f, 171.47f),
    new(406.54f, 171.97f), new(411.74f, 172.26f), new(414.11f, 172.52f), new(414.09f, 248.26f), new(407.4f, 248.48f),
    new(406.8f, 248.53f), new(406.59f, 276.15f), new(406.34f, 276.78f), new(406.07f, 277.35f), new(405.84f, 277.94f),
    new(405.84f, 279.49f), new(405.8f, 280.04f), new(406.17f, 280.42f), new(408.62f, 280.42f), new(409.18f, 280.49f),
    new(409.24f, 281.05f), new(409.37f, 281.76f), new(409.51f, 282.45f), new(409.69f, 283.12f), new(409.89f, 283.75f),
    new(410.11f, 284.37f), new(410.35f, 284.94f), new(410.64f, 285.56f), new(410.96f, 286.18f), new(412.3f, 288.33f),
    new(412.63f, 288.88f), new(413.28f, 290.04f), new(413.6f, 290.63f), new(414.47f, 292.51f), new(414.74f, 293.14f),
    new(415.24f, 294.44f), new(415.46f, 295.08f), new(415.86f, 296.35f), new(416.03f, 296.94f), new(416.34f, 298.17f),
    new(416.48f, 298.79f), new(416.72f, 300.06f), new(416.82f, 300.7f), new(416.92f, 301.36f), new(417.06f, 302.7f),
    new(417.16f, 304.08f), new(417.19f, 304.78f), new(417.2f, 305.48f), new(417.19f, 306.17f), new(417.17f, 306.86f),
    new(417.14f, 307.55f), new(417.09f, 308.24f), new(417.03f, 308.95f), new(416.95f, 309.67f), new(416.86f, 310.38f),
    new(416.74f, 311.11f), new(416.61f, 311.84f), new(416.47f, 312.56f), new(416.32f, 313.24f), new(415.79f, 315.2f),
    new(415.15f, 317.1f), new(414.66f, 318.36f), new(414.39f, 318.98f), new(414.12f, 319.58f), new(413.83f, 320.18f),
    new(413.53f, 320.79f), new(413.22f, 321.38f), new(412.87f, 321.94f), new(412.55f, 322.5f), new(412.23f, 323.03f),
    new(411.56f, 324.08f), new(410.44f, 325.66f), new(410.03f, 326.18f), new(409.17f, 327.23f), new(408.72f, 327.74f),
    new(408.25f, 328.25f), new(407.77f, 328.76f), new(407.28f, 329.25f), new(406.77f, 329.73f), new(405.76f, 330.66f),
    new(405.24f, 331.09f), new(404.17f, 331.94f), new(403.63f, 332.35f), new(401.93f, 333.52f), new(401.32f, 333.9f),
    new(400.7f, 334.27f), new(400.07f, 334.63f), new(399.42f, 334.98f), new(398.8f, 335.29f), new(398.12f, 335.62f),
    new(397.53f, 335.89f), new(396.29f, 336.44f), new(395.66f, 336.74f), new(395.05f, 337.07f), new(394.53f, 337.38f),
    new(394.03f, 337.72f), new(393.5f, 338.1f), new(392.92f, 338.55f), new(392.38f, 339.04f), new(391.88f, 339.52f),
    new(391.41f, 340.01f), new(390.99f, 340.47f), new(390.6f, 340.96f), new(390.2f, 341.51f), new(389.84f, 342.06f),
    new(389.49f, 342.62f), new(389.21f, 343.15f), new(388.94f, 343.7f), new(388.68f, 344.29f), new(388.46f, 344.88f),
    new(388.01f, 345.35f), new(383.45f, 345.35f), new(382.8f, 345.37f), new(370.29f, 344.97f), new(369.75f, 344.9f),
    new(369.84f, 344.36f), new(370.08f, 343.07f), new(370.23f, 342.43f), new(370.56f, 341.13f), new(370.75f, 340.48f),
    new(370.96f, 339.82f), new(371.37f, 338.6f), new(371.85f, 337.39f), new(372.12f, 336.74f), new(372.96f, 334.96f),
    new(373.9f, 333.25f), new(374.94f, 331.59f), new(375.33f, 331.03f), new(375.73f, 330.47f), new(376.14f, 329.92f),
    new(376.56f, 329.38f), new(377, 328.84f), new(377.89f, 327.81f), new(379.25f, 326.39f), new(380.7f, 325.05f),
    new(381.23f, 324.6f), new(382.3f, 323.74f), new(382.79f, 323.37f), new(383.82f, 322.64f), new(384.35f, 322.28f),
    new(385.47f, 321.59f), new(386.05f, 321.25f), new(386.66f, 320.91f), new(387.28f, 320.58f), new(387.92f, 320.26f),
    new(388.58f, 319.95f), new(389.95f, 319.35f), new(391.23f, 318.75f), new(391.84f, 318.39f), new(392.42f, 318.02f),
    new(392.98f, 317.63f), new(393.51f, 317.21f), new(394.01f, 316.79f), new(394.49f, 316.34f), new(394.94f, 315.87f),
    new(395.39f, 315.39f), new(395.82f, 314.86f), new(396.22f, 314.32f), new(396.61f, 313.75f), new(396.98f, 313.15f),
    new(397.31f, 312.52f), new(397.62f, 311.92f), new(397.89f, 311.29f), new(398.15f, 310.6f), new(398.36f, 309.98f),
    new(398.53f, 309.34f), new(398.67f, 308.75f), new(398.79f, 308.14f), new(398.87f, 307.55f), new(398.95f, 306.9f),
    new(398.99f, 306.17f), new(398.99f, 305.44f), new(398.98f, 304.75f), new(398.9f, 304.02f), new(398.81f, 303.34f),
    new(398.71f, 302.75f), new(398.56f, 302.12f), new(398.4f, 301.49f), new(398.18f, 300.8f), new(397.92f, 300.12f),
    new(397.65f, 299.47f), new(397.35f, 298.86f), new(397.03f, 298.28f), new(396.69f, 297.71f), new(396, 296.64f),
    new(395.32f, 295.51f), new(395, 294.93f), new(394.36f, 293.72f), new(394.06f, 293.11f), new(393.78f, 292.47f),
    new(393.5f, 291.83f), new(393.24f, 291.19f), new(392.98f, 290.54f), new(392.74f, 289.86f), new(392.51f, 289.18f),
    new(392.29f, 288.49f), new(392.09f, 287.77f), new(391.63f, 285.87f), new(391.39f, 284.67f), new(391.3f, 284.07f),
    new(391.2f, 283.42f), new(391.11f, 282.68f), new(391.04f, 281.97f), new(390.94f, 280.59f), new(392.06f, 280.41f),
    new(392.72f, 280.54f), new(393.34f, 280.51f), new(393.94f, 280.37f), new(394.16f, 278.35f), new(394.13f, 277.81f),
    new(393.4f, 277.54f), new(393.39f, 249.03f), new(393.2f, 248.54f), new(386.18f, 248.48f), new(385.91f, 172.54f),
    new(391.29f, 172.28f), new(391.91f, 172.36f), new(392.53f, 172.34f), new(393.13f, 172.24f), new(393.43f, 161.97f),
    new(393.78f, 161.59f), new(394.38f, 161.24f), new(394.7f, 158.34f), new(394.91f, 157.69f), new(395.26f, 157.12f),
    new(395.73f, 156.65f), new(396.27f, 156.3f), new(396.89f, 156.08f), new(397.56f, 156), new(402.46f, 156)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);

    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.SohrKhaiAnzu, (uint)OID.SohrKhaiCockerel, (uint)OID.SanctuaryTsanahale];

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
