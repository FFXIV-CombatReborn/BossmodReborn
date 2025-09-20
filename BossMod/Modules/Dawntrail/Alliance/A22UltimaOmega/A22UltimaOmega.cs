namespace BossMod.Dawntrail.Alliance.A22UltimaOmega;

sealed class IonEffluxCitadelBusterHyperPulseChemicalBomb(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.IonEfflux, (uint)AID.CitadelBuster,
(uint)AID.HyperPulse, (uint)AID.ChemicalBomb]);
sealed class Antimatter(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Antimatter);
sealed class AntiPersonnelMissile(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.AntiPersonnelMissile, 6f);

sealed class ChemicalBombHyperPulse : Components.SimpleAOEGroups
{
    public ChemicalBombHyperPulse(BossModule module) : base(module, [(uint)AID.ChemicalBomb, (uint)AID.HyperPulse], 25f, 2)
    {
        MaxDangerColor = 1;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.UltimaTheFeared, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1058u, NameID = 14231u, Category = BossModuleInfo.Category.Alliance, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 4)]
public sealed class A22UltimaOmega(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter1, new ArenaBoundsRect(20f, 23.5f))
{
    public static readonly WPos ArenaCenter1 = new(800f, 800f);
    public static readonly WPos ArenaCenter2 = new(735f, 800f);
    public Actor? Omega;

    protected override void UpdateModule()
    {
        Omega ??= GetActor((uint)OID.OmegaTheOne);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(Omega);
    }
}
