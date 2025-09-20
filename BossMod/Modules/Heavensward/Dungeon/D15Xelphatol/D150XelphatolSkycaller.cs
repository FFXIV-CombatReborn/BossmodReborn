namespace BossMod.Heavensward.Dungeon.D15Xelphatol.D150XelphatolSkycaller;

public enum OID : uint
{
    Boss = 0x17B8, // R1.8
    AbalathianHornbill = 0x17B7 // R1.8
}

public enum AID : uint
{
    AutoAttack = 871, // AbalathianHornbill->player, no cast, single-target

    Aero = 969, // Boss->player, 1.0s cast, single-target
    Gust = 6627, // AbalathianHornbill->location, 3.0s cast, range 5 circle
    Tornado = 6413, // Boss->player, 5.0s cast, range 6 circle
    IxaliAeroIII = 6629, // Boss->self, 3.0s cast, range 30+R circle
    IxaliAeroII = 6628 // Boss->self, 3.0s cast, range 40+R width 8 rect
}

class Tornado(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Tornado, 6f);
class TornadoHint(BossModule module) : Components.CastInterruptHint(module, (uint)AID.Tornado, true, true, showNameInHint: true);
class IxaliAeroIII(BossModule module) : Components.RaidwideCast(module, (uint)AID.IxaliAeroIII);
class IxaliAeroIIIHint(BossModule module) : Components.CastInterruptHint(module, (uint)AID.IxaliAeroIII, true, true, showNameInHint: true);
class IxaliAeroII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IxaliAeroII, new AOEShapeRect(41.8f, 4f));
class Gust(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Gust, 5f);

class D150XelphatolSkycallerStates : StateMachineBuilder
{
    public D150XelphatolSkycallerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Tornado>()
            .ActivateOnEnter<TornadoHint>()
            .ActivateOnEnter<IxaliAeroIII>()
            .ActivateOnEnter<IxaliAeroIIIHint>()
            .ActivateOnEnter<IxaliAeroII>()
            .ActivateOnEnter<Gust>()
            .Raw.Update = () => AllDeadOrDestroyedInBounds(D150XelphatolSkycaller.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 182, NameID = 5264, SortOrder = 5)]
public sealed class D150XelphatolSkycaller : BossModule
{
    public D150XelphatolSkycaller(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D150XelphatolSkycaller(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        WPos[] vertices = [new(363.26f, -414.76f), new(365.28f, -414.44f), new(366.58f, -414.09f), new(368.51f, -413.30f), new(370.19f, -412.29f),
        new(371.67f, -411.09f), new(372.58f, -410.18f), new(373.79f, -408.64f), new(374.75f, -406.99f), new(375.48f, -405.23f),
        new(375.95f, -403.36f), new(376.16f, -401.34f), new(376.08f, -400.81f), new(376.16f, -400.25f), new(376.49f, -399.83f),
        new(377.07f, -399.65f), new(377.24f, -398.97f), new(377, -396.94f), new(375.64f, -392.92f), new(375.18f, -393.24f),
        new(374.64f, -393.43f), new(374.11f, -393.21f), new(373.64f, -392.77f), new(372.82f, -391.72f), new(371.39f, -390.26f),
        new(369.78f, -389.07f), new(368.12f, -388.15f), new(366.92f, -387.66f), new(365.1f, -387.14f), new(363.28f, -386.88f),
        new(361.21f, -386.87f), new(359.31f, -387.14f), new(357.49f, -387.66f), new(354.6f, -389.1f), new(353.05f, -390.3f),
        new(351.57f, -391.81f), new(350.7f, -392.96f), new(349.73f, -394.6f), new(348.97f, -396.41f), new(348.48f, -398.39f),
        new(348.28f, -399.75f), new(348.25f, -400.41f), new(348.31f, -400.98f), new(349.44f, -400.61f), new(350.02f, -400.64f),
        new(350.62f, -400.85f), new(351.19f, -401), new(351.4f, -401.57f), new(351.28f, -402.08f), new(350.76f, -402.5f),
        new(350.69f, -403.09f), new(350.14f, -403.35f), new(350.07f, -403.86f), new(350.87f, -406.19f), new(351.38f, -406.03f),
        new(352.53f, -406.46f), new(353.12f, -406.38f), new(353.53f, -406.74f), new(353.48f, -407.25f), new(353.19f, -407.79f),
        new(352.69f, -408.28f), new(352.36f, -408.67f), new(351.78f, -408.87f), new(351.05f, -409.21f), new(351.78f, -410.13f),
        new(353.19f, -411.45f), new(356.06f, -413.34f), new(357.81f, -414.06f), new(359.74f, -414.53f)];
        var arena = new ArenaBoundsCustom([new PolygonCustom(vertices)]);
        return (arena.Center, arena);
    }

    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.AbalathianHornbill];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.ActorsInBounds(this, Trash);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            hints.PotentialTargets[i].Priority = 0;
        }
    }
}
