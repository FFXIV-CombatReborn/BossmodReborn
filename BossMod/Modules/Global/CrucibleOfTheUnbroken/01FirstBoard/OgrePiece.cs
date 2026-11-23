namespace BossMod.Global.CrucibleOfTheUnbroken.FirstBoard.OgrePiece;

public enum OID : uint
{
    OgrePiece = 0x4B8D, // R2.080, x?
    MagmaPuddle = 0x1EC025, // R0.500, x?, EventObj type
    WispPiece = 0x4B8E, // R0.800, x?
    GreatWispPiece = 0x4DD4, // R1.600, x?
    BurningWardPuddle = 0x1E9927, // R0.500, x?, EventObj type
    BallOfFire = 0x4B8F, // R1.000, x?
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack_ = 49682, // OgrePiece->player, no cast, single-target
    Teleport = 46914, // OgrePiece->location, no cast, single-target

    ScorchingSmite = 46913, // OgrePiece->self, 5.0+1.0s cast, single-target
    ScorchingSmite1 = 46912, // Helper->self, 6.0s cast, range 40 120.000-degree cone
    Allfire = 46915, // OgrePiece->self, 4.0s cast, range 40 circle
    Magma = 46916, // Helper->location, 3.0s cast, range 3 circle
    Magma1 = 46917, // Helper->location, 3.0s cast, range 5 circle
    ScorchingSmite2 = 46910, // OgrePiece->self, 6.0s cast, single-target : Visual for teleport from puddle to puddle possibly.
    ScorchingSmite3 = 46911, // OgrePiece->self, no cast, single-target
    ScorchingSmite4 = 49688, // Helper->self, 9.3s cast, range 40 120.000-degree cone
    BurningWard = 46918, // OgrePiece->self, 3.0s cast, single-target : Invincibility shield for OgrePiece
    ScorchedEarth = 46919, // WispPiece->self, no cast, range 60 circle
    ScorchedEarth1 = 49728, // GreatWispPiece->self, no cast, range 60 circle
    FireCall = 46921, // OgrePiece->self, 4.0s cast, single-target : Summons BallOfFire and tethers to player
    ArmOfPurgatory = 46922, // BallOfFire->self, 1.0s cast, range 10 circle
    StraightPunch = 46920, // OgrePiece->player, 5.0s cast, single-target

}

public enum SID : uint
{
    BurningWard = 4175, // OgrePiece->OgrePiece, extra=0x0
}

public enum TetherID : uint
{
    FireTether = 17, // BallOfFire -> Player // TODO: When OgrePiece casts FireCall a tether and a floating ball of fire appear.
}

sealed class ScorchingSmite(BossModule module)
    : Components.SimpleAOEGroups(module, [(uint)AID.ScorchingSmite1, (uint)AID.ScorchingSmite4], new AOEShapeCone(40f, 60f.Degrees()));

sealed class StraightPunch(BossModule module) : Components.SingleTargetCast(module, (uint)AID.StraightPunch);

//Magma = 46916, // Helper->location, 3.0s cast, range 3 circle
sealed class Magma(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Magma, 3f);

//Magma1 = 46917, // Helper->location, 3.0s cast, range 5 circle
sealed class Magma1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Magma1, 5f);

sealed class MagmaPuddle(BossModule module) : Components.Voidzone(module, 5.0f, m => m.Enemies((uint)OID.MagmaPuddle).Where(z => z.EventState != 7));

//VoidZone under OgrePiece during BurningWard Invincible shield.
sealed class BurningWardPuddle(BossModule module) : Components.Voidzone(module, 5.0f, m => m.Enemies((uint)OID.BurningWardPuddle).Where(z => z.EventState != 7));

sealed class WispAdds(BossModule module)
    : Components.AddsMulti(module, [(uint)OID.WispPiece, (uint)OID.GreatWispPiece], 1)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var ogre = Module.PrimaryActor.Position;
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            if (e.Actor.OID is (uint)OID.WispPiece or (uint)OID.GreatWispPiece)
            {
                var distance = (e.Actor.Position - ogre).Length();
                e.Priority = Math.Clamp(1 + (int)((20f - distance) / 5f), 1, 5);
            }
        }
    }
}

sealed class RaidWideFires(BossModule module)
    : Components.RaidwideCasts(module, [(uint)AID.Allfire]);

// Ball of Fire chases the player and explodes into an r10 circle on contact or after 15s, so keep out of its full explosion radius.
sealed class BallOfFireVoidZone(BossModule module) : Components.Voidzone(module, 10.0f, m => m.Enemies((uint)OID.BallOfFire).Where(z => z.EventState != 7));

// When Ball of Fire explodes it is a large r10 circle.
sealed class ArmOfPurgatory(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArmOfPurgatory, 10f);

sealed class OgrePieceStates : StateMachineBuilder
{
    public OgrePieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StraightPunch>()
            .ActivateOnEnter<ScorchingSmite>()
            .ActivateOnEnter<RaidWideFires>()
            .ActivateOnEnter<Magma>()
            .ActivateOnEnter<Magma1>()
            .ActivateOnEnter<MagmaPuddle>()
            .ActivateOnEnter<BurningWardPuddle>()
            .ActivateOnEnter<WispAdds>()
            .ActivateOnEnter<BallOfFireVoidZone>()
            .ActivateOnEnter<ArmOfPurgatory>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    PrimaryActorOID = (uint)OID.OgrePiece,
    Contributors = "wen",
    GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken,
    GroupID = 1088u,
    NameID = 14538u,
    SortOrder = 5)]

public sealed class OgrePiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f));
