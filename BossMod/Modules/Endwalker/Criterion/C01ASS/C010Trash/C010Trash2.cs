namespace BossMod.Endwalker.VariantCriterion.C01ASS.C010Trash2;

public enum OID : uint
{
    NDullahan = 0x3AD7, // R2.5
    SDullahan = 0x3AE0, // R2.5
    NArmor = 0x3AD8, // R2.5
    SArmor = 0x3AE1, // R2.5
}

public enum AID : uint
{
    // Armor
    AutoAttack1 = 31109, // NArmor/SArmor->player, no cast, single-target

    NDominionSlash = 31082, // NArmor->self, 3.5s cast, range 12 90-degree cone
    NInfernalWeight = 31083, // NArmor->self, 5.0s cast, raidwide
    NHellsNebula = 31084, // NArmor->self, 4.0s cast, raidwide set hp to 1
    SDominionSlash = 31106, // SArmor->self, 3.5s cast, range 12 90-degree cone
    SInfernalWeight = 31107, // SArmor->self, 5.0s cast, raidwide
    SHellsNebula = 31108, // SArmor->self, 4.0s cast, raidwide set hp to 1

    // Dullahan
    AutoAttack2 = 31318, // NDullahan/SDullahan->player, no cast, single-target

    NBlightedGloom = 31078, // NDullahan->self, 4.0s cast, range 10 circle
    NKingsWill = 31080, // NDullahan->self, 2.5s cast, single-target damage up
    NInfernalPain = 31081, // NDullahan->self, 5.0s cast, raidwide
    SBlightedGloom = 31102, // SDullahan->self, 4.0s cast, range 10 circle
    SKingsWill = 31104, // SDullahan->self, 2.5s cast, single-target damage up
    SInfernalPain = 31105 // SDullahan->self, 5.0s cast, raidwide
}

abstract class DominionSlash(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(12f, 45f.Degrees()));
sealed class NDominionSlash(BossModule module) : DominionSlash(module, (uint)AID.NDominionSlash);
sealed class SDominionSlash(BossModule module) : DominionSlash(module, (uint)AID.SDominionSlash);

abstract class InfernalWeight(BossModule module, uint aid) : Components.RaidwideCast(module, aid, "Raidwide with slow");
sealed class NInfernalWeight(BossModule module) : InfernalWeight(module, (uint)AID.NInfernalWeight);
sealed class SInfernalWeight(BossModule module) : InfernalWeight(module, (uint)AID.SInfernalWeight);

abstract class HellsNebula(BossModule module, uint aid) : Components.CastHint(module, aid, "Reduce hp to 1");
sealed class NHellsNebula(BossModule module) : HellsNebula(module, (uint)AID.NHellsNebula);
sealed class SHellsNebula(BossModule module) : HellsNebula(module, (uint)AID.SHellsNebula);

abstract class BlightedGloom(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 10f);
sealed class NBlightedGloom(BossModule module) : BlightedGloom(module, (uint)AID.NBlightedGloom);
sealed class SBlightedGloom(BossModule module) : BlightedGloom(module, (uint)AID.SBlightedGloom);

abstract class KingsWill(BossModule module, uint aid) : Components.CastHint(module, aid, "Damage increase buff");
sealed class NKingsWill(BossModule module) : KingsWill(module, (uint)AID.NKingsWill);
sealed class SKingsWill(BossModule module) : KingsWill(module, (uint)AID.SKingsWill);

abstract class InfernalPain(BossModule module, uint aid) : Components.RaidwideCast(module, aid);
sealed class NInfernalPain(BossModule module) : InfernalPain(module, (uint)AID.NInfernalPain);
sealed class SInfernalPain(BossModule module) : InfernalPain(module, (uint)AID.SInfernalPain);

abstract class C010DullahanStates : StateMachineBuilder
{
    public C010DullahanStates(BossModule module, bool savage) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<NDominionSlash>(!savage)
            .ActivateOnEnter<NInfernalWeight>(!savage)
            .ActivateOnEnter<NHellsNebula>(!savage)
            .ActivateOnEnter<SDominionSlash>(savage)
            .ActivateOnEnter<SInfernalWeight>(savage)
            .ActivateOnEnter<SHellsNebula>(savage)
            .ActivateOnEnter<NBlightedGloom>(!savage)
            .ActivateOnEnter<NKingsWill>(!savage)
            .ActivateOnEnter<NInfernalPain>(!savage)
            .ActivateOnEnter<SBlightedGloom>(savage)
            .ActivateOnEnter<SKingsWill>(savage)
            .ActivateOnEnter<SInfernalPain>(savage)
            .Raw.Update = () => AllDeadOrDestroyed(savage ? Trash2Arena.TrashSavage : Trash2Arena.TrashNormal);
    }
}

sealed class C010NTrash2States(BossModule module) : C010DullahanStates(module, false);
sealed class C010STrash2States(BossModule module) : C010DullahanStates(module, true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.NDullahan, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 878, NameID = 11506, SortOrder = 3)]
public sealed class C010NTrash2(WorldState ws, Actor primary) : Trash2Arena(ws, primary, false);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.SDullahan, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 879, NameID = 11506, SortOrder = 3)]
public sealed class C010STrash2(WorldState ws, Actor primary) : Trash2Arena(ws, primary, true);

public abstract class Trash2Arena(WorldState ws, Actor primary, bool savage) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Rectangle(new(-34.937f, -198.475f), 33.89f, 21.428f), new Rectangle(new(-35f, -219.2f), 5.261f, 1.122f)], [new Rectangle(new(-35.5f, -207f), 3f, 4.19f),
    new Rectangle(new(-35.5f, -189.9f), 3f, 4.19f), new Rectangle(new(-17.51f, -198.5f), 4.98f, 13f), new Rectangle(new(-52.5f, -198.5f), 4.98f, 13f),
    new Square(new(-69.3f, -185.3f), 1.5f), new Square(new(-69.3f, -211.801f), 1.5f), new Square(new(-52.5f, -220.201f), 1.5f), new Square(new(-17.4f, -220.201f), 1.5f),
    new Square(new(-0.8f, -211.777f), 1.5f), new Square(new(-0.791f, -185.378f), 1.5f), new Square(new(-17.5f, -176.801f), 1.5f), new Square(new(-35f, -176.801f), 1.5f),
    new Square(new(-52.4f, -176.801f), 1.5f), new Square(new(-43.2f, -220.2f), 1.942f), new Square(new(-27.1f, -220.2f), 1.942f), new Rectangle(new(-39.981f, -221.1f), 1.526f, 2.172f),
    new Rectangle(new(-30.02f, -221.1f), 1.526f, 2.172f)]);

    public static readonly uint[] TrashNormal = [(uint)OID.NDullahan, (uint)OID.NArmor];
    public static readonly uint[] TrashSavage = [(uint)OID.NArmor, (uint)OID.SArmor];

    protected override bool CheckPull() => IsAnyActorInCombat(savage ? TrashSavage : TrashNormal);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, savage ? TrashSavage : TrashNormal);
    }
}
