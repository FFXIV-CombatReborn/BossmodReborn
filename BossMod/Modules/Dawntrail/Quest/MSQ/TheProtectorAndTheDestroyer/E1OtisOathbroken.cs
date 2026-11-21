namespace BossMod.Dawntrail.Quest.MSQ.TheProtectorAndTheDestroyer.Otis;

public enum OID : uint
{
    Boss = 0x4342, // R3.0    
    EverkeepAerostat = 0x4344, // R2.3
    EverkeepAerostat2 = 0x4345, // R2.3
    EverkeepTurret = 0x4346, // R0.6
    EverkeepSentryG10 = 0x4343, // R0.9
    EverkeepSentryR10 = 0x4347, // R1.999
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->tank, no cast, single-target
    AutoAttack2 = 872, // EverkeepAerostat2->tank, no cast, single-target
    AutoAttack3 = 28538, // EverkeepTurret->tank, no cast, single-target
    AutoAttack4 = 36403, // EverkeepSentryR10->tank, no cast, single-target
    AutoAttack5 = 873, // EverkeepSentryG10->tank, no cast, single-target

    Teleport = 38193, // Boss->location, no cast, single-target
    FormationAlpha = 38194, // Boss->self, 5.0s cast, single-target
    ThrownFlames = 38205, // EverkeepAerostat2->self, 6.0s cast, range 8 circle
    BastionBreaker = 38198, // Helper->all, 6.0s cast, range 6 circle, spread
    SearingSlash = 38197, // Boss->self, 6.0s cast, range 8 circle
    StormlitShockwave = 38202, // Boss->self, 5.0s cast, range 40 circle
    SelfDestruct = 38206, // EverkeepAerostat2->self, 8.0s cast, range 40 circle
    FormationBeta = 38195, // Boss->self, 5.0s cast, single-target
    Electrobeam = 38207, // EverkeepTurret->self, 6.0s cast, range 40 width 4 rect
    HolyBlade = 38199, // Helper->Alisaie, 6.0s cast, range 6 circle, stack
    SteadfastWill = 38201, // Boss->tank, 5.0s cast, single-target
    SelfDestruct2 = 38208, // EverkeepTurret->self, 8.0s cast, range 40 circle
    FormationGamma = 38196, // Boss->self, 5.0s cast, single-target
    Rush = 38209, // EverkeepSentryR10->location, 5.0s cast, width 5 rect charge
    SelfDestruct23 = 38210, // EverkeepSentryR10->self, 8.0s cast, range 40 circle
    ValorousAscension = 38203, // Boss->self, 8.0s cast, range 40 circle
    RendPower = 38200, // Helper->self, 4.5s cast, range 40 30-degree cone
    ModelChange = 38204 // Boss->self, no cast, single-target
}

sealed class StormlitShockwave(BossModule module) : Components.RaidwideCast(module, (uint)AID.StormlitShockwave);
sealed class ValorousAscension(BossModule module) : Components.RaidwideCast(module, (uint)AID.ValorousAscension);
sealed class RendPower(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RendPower, new AOEShapeCone(40f, 15f.Degrees()), 6);

sealed class BastionBreaker(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.BastionBreaker, 6f);
sealed class HolyBlade(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.HolyBlade, 6f);

sealed class SearingSlashThrownFlames(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.SearingSlash, (uint)AID.ThrownFlames], 8f);

sealed class Electrobeam(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Electrobeam, new AOEShapeRect(40f, 2f));

sealed class Rush : Components.SimpleChargeAOEGroups
{
    public Rush(BossModule module) : base(module, [(uint)AID.Rush], 2.5f, 4)
    {
        MaxDangerColor = 2;
        MaxRisky = 2;
    }
}
sealed class SteadfastWill(BossModule module) : Components.SingleTargetCast(module, (uint)AID.SteadfastWill);

sealed class OtisOathbrokenStates : StateMachineBuilder
{
    public OtisOathbrokenStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StormlitShockwave>()
            .ActivateOnEnter<ValorousAscension>()
            .ActivateOnEnter<RendPower>()
            .ActivateOnEnter<SearingSlashThrownFlames>()
            .ActivateOnEnter<BastionBreaker>()
            .ActivateOnEnter<Electrobeam>()
            .ActivateOnEnter<SteadfastWill>()
            .ActivateOnEnter<HolyBlade>()
            .ActivateOnEnter<Rush>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70478, NameID = 13168)]
public sealed class OtisOathbroken(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, ArenaBounds)
{
    public static readonly WPos ArenaCenter = new(349f, -14f);
    public static readonly ArenaBoundsCustom ArenaBounds = new([new Polygon(ArenaCenter, 19.5f, 20)]);

    protected override bool CheckPull() => Raid.Player() is Actor { InCombat: true };

    private static readonly uint[] all = [(uint)OID.Boss, (uint)OID.EverkeepTurret, (uint)OID.EverkeepAerostat, (uint)OID.EverkeepAerostat2, (uint)OID.EverkeepSentryG10,
    (uint)OID.EverkeepSentryR10];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, all);
    }
}
