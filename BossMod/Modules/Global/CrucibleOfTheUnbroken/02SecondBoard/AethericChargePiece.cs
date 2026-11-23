namespace BossMod.Global.CrucibleOfTheUnbroken.SecondBoard.AethericChargePiece;

public enum OID : uint
{
    AethericCharge = 0x4C64, // R6.000, x1
    DemonPiece = 0x4C61, // R1.100, x4
    EvilWeaponPiece = 0x4C62, // R2.200, x0 (spawn during fight)
    DeviletPiece = 0x4C63, // R0.840, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint
{
    DemonAuto = 50396, // DemonPiece->player, no cast, single-target
    WeaponAuto = 49680, // EvilWeaponPiece->player, no cast, single-target

    SmiteOfExcessiveRageRect = 48218, // EvilWeaponPiece->self, 7.0s cast, range 40 width 10 rect
    SmiteOfExcessiveRageDonut = 48219, // EvilWeaponPiece->self, 7.0s cast, range 4-30 donut
    VoidFireIII = 48222, // DeviletPiece->self, 10.0s cast, range 60 circle
    DarkEruption = 48217, // Helper->location, 3.0s cast, range 6 circle
    Condemnation = 48223, // DemonPiece->self, 3.0s cast, range 12 90-degree cone
    VoidParalyze = 48247 // DemonPiece->none, 5.0s cast, single-target
}

public enum SID : uint
{
    Recharge = 5406, // none->DemonPiece/EvilWeaponPiece/DeviletPiece, extra=0x1/0x2/0x3/0x4
    DamageUp = 5409 // none->EvilWeaponPiece/DeviletPiece/DemonPiece, extra=0x0
}

public enum TetherID : uint
{
    Recharge = 205 // DemonPiece/EvilWeaponPiece/DeviletPiece->AethericCharge
}

// pieces that are recharging the boss or buffed should be killed first
sealed class Adds(BossModule module) : Components.AddsMulti(module, [(uint)OID.DemonPiece, (uint)OID.EvilWeaponPiece, (uint)OID.DeviletPiece])
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            if (e.Actor.OID is not ((uint)OID.DemonPiece or (uint)OID.EvilWeaponPiece or (uint)OID.DeviletPiece))
                continue;
            e.Priority = e.Actor.FindStatus((uint)SID.Recharge) != null || e.Actor.FindStatus((uint)SID.DamageUp) != null ? 1 : 0;
        }
    }
}

sealed class SmiteOfExcessiveRageRect(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SmiteOfExcessiveRageRect, new AOEShapeRect(40f, 5f));
sealed class SmiteOfExcessiveRageDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SmiteOfExcessiveRageDonut, new AOEShapeDonut(4f, 30f));
sealed class VoidFireIII(BossModule module) : Components.RaidwideCast(module, (uint)AID.VoidFireIII);
sealed class DarkEruption(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkEruption, 6f);
sealed class Condemnation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Condemnation, new AOEShapeCone(12f, 45f.Degrees()));
sealed class VoidParalyze(BossModule module) : Components.CastInterruptHint(module, (uint)AID.VoidParalyze, showNameInHint: true);

sealed class AethericChargePieceStates : StateMachineBuilder
{
    public AethericChargePieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<SmiteOfExcessiveRageRect>()
            .ActivateOnEnter<SmiteOfExcessiveRageDonut>()
            .ActivateOnEnter<VoidFireIII>()
            .ActivateOnEnter<DarkEruption>()
            .ActivateOnEnter<Condemnation>()
            .ActivateOnEnter<VoidParalyze>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.AethericCharge, Contributors = "Ported from upstream by LTS", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1089u, NameID = 14560u, SortOrder = 6)]
[PreferredKinship(BST.Kinship.Soul, "Interrupt Demon Piece's Void Paralyze")]
[PreferredKinship(BST.Kinship.Ash, "Cleanse Paralysis from Void Paralyze")]
public sealed class AethericChargePiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f))
{
    protected override bool CheckPull() => PrimaryActor.InCombat;
}
