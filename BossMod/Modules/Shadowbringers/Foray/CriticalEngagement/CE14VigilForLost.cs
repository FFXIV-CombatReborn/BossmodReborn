namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE14VigilForLost;

public enum OID : uint
{
    Boss = 0x2DBD, // R8.000, x1
    MagitekBit = 0x2F58, // R0.660, spawn during fight
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target

    LightLeap = 20146, // Boss->location, 4.0s cast, range 10 circle aoe
    AreaBombardment = 20147, // Boss->self, 5.0s cast, single-target, visual (main mechanic start)
    ChemicalMissile = 20148, // Helper->self, 4.0s cast, range 12 circle aoe
    TailMissile = 20149, // Boss->self, 5.0s cast, single-target, visual (big aoe)
    TailMissileAOE = 20150, // Helper->self, 8.0s cast, range 30 circle aoe
    Shockwave = 20151, // Boss->self, 6.0s cast, range 16 circle aoe
    ExplosiveFlare = 20152, // Helper->self, 3.0s cast, range 10 circle aoe
    CripplingBlow = 20153, // Boss->player, 4.0s cast, single-target, tankbuster
    PlasmaField = 20154, // Boss->self, 4.0s cast, range 60 circle, raidwide
    Explosion = 21266, // Helper->self, 7.0s cast, range 6 circle tower
    MassiveExplosion = 21267, // Helper->self, no cast, range 60 circle, failed tower
    MagitekRay = 21268 // MagitekBit->self, 2.5s cast, range 50 width 4 rect
}

sealed class LightLeapExplosiveFlare(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LightLeap, (uint)AID.ExplosiveFlare], 10f);
sealed class ChemicalMissile : Components.SimpleAOEs
{
    public ChemicalMissile(BossModule module) : base(module, (uint)AID.ChemicalMissile, 12f)
    {
        MaxDangerColor = 2;
    }
}
sealed class TailMissile(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TailMissileAOE, 30f);
sealed class Shockwave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shockwave, 16f);
sealed class CripplingBlow(BossModule module) : Components.SingleTargetCast(module, (uint)AID.CripplingBlow);
sealed class PlasmaField(BossModule module) : Components.RaidwideCast(module, (uint)AID.PlasmaField);
sealed class Towers(BossModule module) : Components.CastTowersOpenWorld(module, (uint)AID.Explosion, 6f);
sealed class MagitekRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagitekRay, new AOEShapeRect(50f, 2f));

sealed class CE14VigilForLostStates : StateMachineBuilder
{
    public CE14VigilForLostStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LightLeapExplosiveFlare>()
            .ActivateOnEnter<ChemicalMissile>()
            .ActivateOnEnter<TailMissile>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<CripplingBlow>()
            .ActivateOnEnter<PlasmaField>()
            .ActivateOnEnter<Towers>()
            .ActivateOnEnter<MagitekRay>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 735, NameID = 3)] // bnpcname=9396
public sealed class CE14VigilForLost(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(451f, 830f), 29.5f, 32)]);

    protected override bool CheckPull() => base.CheckPull() && Raid.Player() is Actor player && player.Position.InCircle(Arena.Center, 30f);
}
