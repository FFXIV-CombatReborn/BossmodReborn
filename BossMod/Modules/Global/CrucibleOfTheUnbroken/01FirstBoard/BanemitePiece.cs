namespace BossMod.Global.CrucibleOfTheUnbroken.FirstBoard.BanemitePiece;

public enum OID : uint
{
    BanemitePiece = 0x4B8B, // R3.000, x?
    MitelingPiece = 0x4B8C, // R1.800, x?
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 50398, // BanemitePiece->player, no cast, single-target
    AutoAttackAdd = 49682, // MitelingPiece->player, no cast, single-target

    BedrockUplift = 46901, // BanemitePiece->self, 4.0s cast, single-target
    BedrockUplift1 = 46902, // 233C->self, 5.0s cast, range 6 circle
    BedrockUplift2 = 46903, // 233C->self, 7.0s cast, range 5-12 donut
    BedrockUplift3 = 46904, // 233C->self, 9.0s cast, range 20-18 donut
    BedrockUplift4 = 46905, // 233C->self, 11.0s cast, range 36-24 donut
    DeadlyThrust = 46906, // BanemitePiece->player, 5.0s cast, single-target
    VenomWeb = 46907, // BanemitePiece->self, 3.0s cast, single-target
    VenomWeb1 = 46908, // 233C->location, 6.0s cast, range 9 circle

    Silkscreen = 46909, // MitelingPiece->self, 5.0s cast, range 40 width 4 rect
}

// Concentric rings after an initial circle aoe.
sealed class BedrockUplift(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(6f), new AOEShapeDonut(6f, 12f),
        new AOEShapeDonut(12f, 18f), new AOEShapeDonut(18f, 24f)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BedrockUplift1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell, 1d), spell.Rotation);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count > 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.BedrockUplift1 => 0,
                (uint)AID.BedrockUplift2 => 1,
                (uint)AID.BedrockUplift3 => 2,
                (uint)AID.BedrockUplift4 => 3,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(1.3d), spell.Rotation);
        }
    }
}

//single-target piercing thrust : Using tankbuster hint to signal to use a shield or other mitigation
sealed class DeadlyThrust(BossModule module) : Components.SingleTargetCast(module, (uint)AID.DeadlyThrust, "Tankbuster + poison");

// Aoe circles that drop on center then around the outside and explode one after another
sealed class VenomWeb(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VenomWeb1, 9f, 6);

sealed class MitelingAdds(BossModule module) : Components.Adds(module, (uint)OID.MitelingPiece);

//Silkscreen = 46909, // MitelingPiece->self, 5.0s cast, range 40 width 4 rect
sealed class Silkscreen(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Silkscreen, new AOEShapeRect(40f, 2f));

sealed class BanemitePieceStates : StateMachineBuilder
{
    public BanemitePieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BedrockUplift>()
            .ActivateOnEnter<DeadlyThrust>()
            .ActivateOnEnter<VenomWeb>()
            .ActivateOnEnter<MitelingAdds>()
            .ActivateOnEnter<Silkscreen>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    PrimaryActorOID = (uint)OID.BanemitePiece,
    Contributors = "wen",
    GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken,
    GroupID = 1088u,
    NameID = 14536u,
    SortOrder = 4)]

[PreferredKinship(BST.Kinship.Ash, "Cleanse Poison from Deadly Thrust and Venom Web")]
public sealed class BanemitePiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f));
