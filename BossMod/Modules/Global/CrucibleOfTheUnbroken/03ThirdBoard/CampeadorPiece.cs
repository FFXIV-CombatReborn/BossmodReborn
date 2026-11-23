namespace BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.CampeadorPiece;

public enum OID : uint
{
    CampeadorPiece = 0x4CA5,
    SabotenderPiece = 0x4CA6, // R0.800, x38
    FlowertenderPiece = 0x4CA7, // R1.600, x2
    GuardiaPiece = 0x4CA9, // R2.400, x1
    SoldadoPiece = 0x4CA8, // R2.000, x4
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 49682, // 4CA6/4CA7/4CA8/4CA9/CampeadorPiece->player, no cast, single-target
    EmergencyOrder = 48578, // CampeadorPiece->self, 5.0s cast, single-target
    HealingWater = 48579, // 4CA7->4CA6, 3.0s cast, range 6 circle - deals no damage
    NeedlesVisual = 48580, // 4CA8->self, 8.0s cast, single-target
    Needles = 48581, // Helper->self, 8.0s cast, range 15 circle
    Cactguard = 48582, // 4CA9->CampeadorPiece, no cast, single-target
    SeedingNeedlesRaidwide = 48589, // CampeadorPiece->self, 5.0s cast, range 100 circle
    SeedingNeedlesCircleVisual = 48585, // CampeadorPiece->self, 5.3+0.7s cast, single-target
    SeedingNeedlesCircle = 48586, // Helper->self, 6.0s cast, range 9 circle
    NeedleGnarlTeleport = 48583, // CampeadorPiece->location, 3.0+1.0s cast, single-target
    NeedleGnarl = 48584, // Helper->location, 4.0s cast, range 8 circle
    SeedingNeedlesDonutVisual = 48587, // CampeadorPiece->self, 5.3+0.7s cast, single-target
    SeedingNeedlesDonut = 48588, // Helper->self, 6.0s cast, range 5-40 donut
}

public enum SID : uint
{
    Cover = 2412, // 4CA9->4CA9, extra=0x14
    Covered = 2413, // 4CA9->CampeadorPiece, extra=0x0
}

public enum IconID : uint
{
    OrbNumber1 = 336, // 4CA8->self
    OrbNumber2 = 337, // 4CA8->self
    OrbNumber3 = 338, // 4CA8->self
    OrbNumber4 = 339, // 4CA8->self
}

sealed class HealingWater(BossModule module) : Components.Adds(module, (uint)OID.FlowertenderPiece, 4)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveActorsCount == 0)
        {
            return;
        }

        hints.Add("Kill the Flowertender Piece adds");
    }
}
sealed class Needles(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Needles, 15f, maxCasts: 3);
sealed class SeedingNeedlesRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.SeedingNeedlesRaidwide);
sealed class SeedingNeedlesCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SeedingNeedlesCircle, 9f);
sealed class NeedleGnarl(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NeedleGnarl, 8f);
sealed class SeedingNeedlesDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SeedingNeedlesDonut, new AOEShapeDonut(5f, 40f));

sealed class CampeadorPieceStates : StateMachineBuilder
{
    public CampeadorPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HealingWater>()
            .ActivateOnEnter<Needles>()
            .ActivateOnEnter<SeedingNeedlesRaidwide>()
            .ActivateOnEnter<SeedingNeedlesCircle>()
            .ActivateOnEnter<NeedleGnarl>()
            .ActivateOnEnter<SeedingNeedlesDonut>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.CampeadorPiece, Contributors = "Equilius",
    GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1090u, NameID = 14587u, SortOrder = 7)]
public sealed class CampeadorPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.FlowertenderPiece => 3,
                (uint)OID.SoldadoPiece => 2,
                (uint)OID.SabotenderPiece => 1,
                (uint)OID.GuardiaPiece => 1,
                (uint)OID.CampeadorPiece => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.FlowertenderPiece), Colors.Vulnerable);
        Arena.Actors(Enemies((uint)OID.SoldadoPiece), Colors.Vulnerable);
        Arena.Actors(Enemies((uint)OID.SabotenderPiece));
        Arena.Actors(Enemies((uint)OID.GuardiaPiece));
    }

    private readonly string[] _prePullHints = [
        "Fight kill order priority: Flowertender Piece/Soldado Piece (purple) -> Sabotender Piece/Guardia Piece/Campeador Piece (red)"
    ];

    public override string[] PrePullHints => _prePullHints;
}
