namespace BossMod.Heavensward.Dungeon.D06AetherochemicalResearchFacility.D060SixthLegionMagitekVanguardH1;

public enum OID : uint
{
    Boss = 0x1350, // R2.8
    SixthLegionOptio = 0xF4A, // R0.5
    SixthLegionMedicus = 0xF4C, // R0.5
    SixthLegionTesserarius = 0xF4B // R0.5
}

public enum AID : uint
{
    AutoAttack1 = 871, // Boss/SixthLegionTesserarius->player, no cast, single-target
    AutoAttack2 = 870, // SixthLegionOptio->player, no cast, single-target

    Aero = 969, // SixthLegionMedicus->player, 1.0s cast, single-target
    Celeris = 404, // SixthLegionOptio->self, 2.5s cast, single-target
    Fortis = 403, // SixthLegionMedicus/SixthLegionTesserarius->self, 2.5s cast, single-target
    TrueThrust = 722, // SixthLegionTesserarius->player, no cast, single-target
    CermetDrill = 526, // Boss->self, 2.5s cast, range 6+R width 5 rect
    FastBlade = 717, // SixthLegionOptio->player, no cast, single-target
    Stoneskin = 316, // SixthLegionMedicus->self, 2.5s cast, single-target
    Rampart = 10, // SixthLegionOptio->self, no cast, single-target
    Heartstopper = 866, // SixthLegionTesserarius->self, 2.5s cast, range 3+R width 3 rect
    Feint = 76, // SixthLegionTesserarius->player, no cast, single-target
    ShieldBash = 718, // SixthLegionOptio->player, no cast, single-target
    Cure = 120, // SixthLegionMedicus->self, 1.5s cast, single-target
    FightOrFlight = 20 // SixthLegionOptio->self, no cast, single-target
}

class CermetDrill(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CermetDrill, new AOEShapeRect(8.8f, 2.5f));
class Heartstopper(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Heartstopper, new AOEShapeRect(3.5f, 1.5f));
class Stoneskin(BossModule module) : Components.CastInterruptHint(module, (uint)AID.Stoneskin, canBeStunned: true, showNameInHint: true);

class D060SixthLegionMagitekVanguardH1States : StateMachineBuilder
{
    public D060SixthLegionMagitekVanguardH1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CermetDrill>()
            .ActivateOnEnter<Heartstopper>()
            .ActivateOnEnter<Stoneskin>()
            .Raw.Update = () => AllDeadOrDestroyed(D060SixthLegionMagitekVanguardH1.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 38, NameID = 4341, SortOrder = 3)]
public class D060SixthLegionMagitekVanguardH1(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(-168.92f, -310.29f), new(-168.68f, -309.78f), new(-164.6f, -305.63f), new(-162.22f, -303.37f), new(-161.69f, -302.98f),
    new(-161.64f, -301.71f), new(-161.53f, -301.12f), new(-159.39f, -300.13f), new(-158.74f, -300.31f), new(-158.74f, -291.19f),
    new(-158.97f, -290.71f), new(-159.61f, -290.99f), new(-160.25f, -291.07f), new(-160.88f, -291.13f), new(-161.47f, -291.04f),
    new(-162.21f, -288.69f), new(-161.91f, -288.19f), new(-162.36f, -287.96f), new(-165.75f, -284.37f), new(-168.27f, -281.82f),
    new(-168.74f, -281.31f), new(-168.8f, -268.16f), new(-169.41f, -267.78f), new(-171.51f, -267.35f), new(-172.17f, -267.39f),
    new(-176, -267.69f), new(-177.9f, -267.75f), new(-178.55f, -267.76f), new(-181.11f, -266.96f), new(-181.78f, -267.05f),
    new(-183.14f, -267.29f), new(-183.35f, -281.21f), new(-183.73f, -281.67f), new(-195.59f, -293.6f), new(-196.02f, -294.06f),
    new(-196.96f, -295.01f), new(-197.43f, -295.57f), new(-183.31f, -309.76f), new(-183.05f, -322.95f), new(-183.05f, -329.17f),
    new(-182.5f, -329.47f), new(-174.49f, -329.12f), new(-173.76f, -329.12f), new(-170.86f, -329.22f), new(-170.15f, -329.34f),
    new(-169.46f, -329.55f), new(-168.92f, -329.66f), new(-168.92f, -310.29f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.SixthLegionTesserarius, (uint)OID.SixthLegionOptio, (uint)OID.SixthLegionMedicus];

    protected override bool CheckPull() => IsAnyActorInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Trash);
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
