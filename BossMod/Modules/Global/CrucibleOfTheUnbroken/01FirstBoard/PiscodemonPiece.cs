namespace BossMod.Global.CrucibleOfTheUnbroken.FirstBoard.PiscodemonPiece;

public enum OID : uint
{
    PiscodemonPiece = 0x4B8A, // R2.340, x?
    Helper = 0x233C // R0.500, x?, Helper type
}

public enum AID : uint
{
    Thunder = 50745, // 4B8A->player, no cast, single-target
    VoidBlizzardIIIAnimation = 46887, // 4B8A->self, 2.5+0.5s cast, single-target
    VoidBlizzardIIICircle = 49886, // 233C->location, 4.5s cast, range 5 circle
    VoidBlizzardIII = 46888, // 233C->location, 3.0s cast, single-target
    VoidBlizzardIII2Animation = 49887, // 4B8A->self, no cast, single-target
    VoidBlizzardIIIFirst = 46889, // 233C->self, 6.0s cast, range 5 circle
    VoidBlizzardIIIRest = 46890, // 233C->self, no cast, range 5 circle
    ClearMind = 46898, // 4B8A->self, 4.0s cast, single-target
    ArcaneBlast = 46899, // 4B8A->self, 8.0s cast, range 100 circle
    VoidFlareStarAnimation = 46893, // 4B8A->self, 2.5+0.5s cast, single-target
    VoidFlareStar = 46894, // 233C->location, 3.0s cast, single-target
    VoidFlareStar1 = 46895, // 233C->self, 6.0s cast, range 100 circle
    VoidThunderIIIAnimation = 46891, // 4B8A->self, 3.0s cast, single-target
    VoidThunderIII = 46892, // 233C->self, 5.0s cast, range 50 width 10 cross
    VoidAeroIIIAnimation = 46896, // 4B8A->self, 5.2+0.8s cast, single-target
    VoidAeroIII = 46897, // 233C->self, 6.0s cast, range 5-60 donut
}

public enum SID : uint
{
    DamageUp = 1225 // PiscodemonPiece->PiscodemonPiece, extra=0x0
}

// This is the initial aoe when the ice is thrown out to mark where exaflares start.
sealed class VoidBlizzardIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidBlizzardIIICircle, new AOEShapeCircle(5f));
// The ice bursts travel across the arena in a straight line. Distance and timing are a bit of an estimate here.
// Looks like it is showing one space further than what is blowing up. It does keep the player out of explosions though.
sealed class VoidBlizzardIIIOne(BossModule module) : Components.SimpleExaflare(module, 5f, (uint)AID.VoidBlizzardIIIFirst, (uint)AID.VoidBlizzardIIIRest, 6f, 1.1d, 6, 2, true);

sealed class ArcaneBlast(BossModule module) : Components.RaidwideCast(module, (uint)AID.ArcaneBlast);
sealed class ClearMind(BossModule module) : Components.Dispel(module, (uint)SID.DamageUp, (uint)AID.ClearMind);

//Proximity aoe. Initial safe distance is an estimate to be adjusted as we get more data.
sealed class VoidFlareStar(BossModule module) : Components.ProximityAOEs(module, (uint)AID.VoidFlareStar1, 28f);

sealed class VoidThunderIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidThunderIII, new AOEShapeCross(50f, 5f));

sealed class VoidAeroIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidAeroIII, new AOEShapeDonut(5f, 60f));

sealed class PiscodemonPieceStates : StateMachineBuilder
{
    public PiscodemonPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ClearMind>()
            .ActivateOnEnter<VoidBlizzardIIIOne>()
            .ActivateOnEnter<VoidBlizzardIII>()
            .ActivateOnEnter<ArcaneBlast>()
            .ActivateOnEnter<VoidFlareStar>()
            .ActivateOnEnter<VoidThunderIII>()
            .ActivateOnEnter<VoidAeroIII>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.PiscodemonPiece, Contributors = "wen", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1088u, NameID = 14535u, SortOrder = 3)]

[PreferredKinship(BST.Kinship.Wave, "Remove Damage Up from Clear Mind")]
public sealed class PiscodemonPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, 0f), new ArenaBoundsSquare(20f));
