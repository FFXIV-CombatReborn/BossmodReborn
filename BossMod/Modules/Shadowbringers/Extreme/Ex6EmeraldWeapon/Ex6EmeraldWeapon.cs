namespace BossMod.Shadowbringers.Extreme.Ex6EmeraldWeapon;

public enum OID : uint
{
    Boss = 0x3041,
    BossP2 = 0x3154,
    Helper = 0x233C,
    BlackWolfsImage = 0x2DD1,
    CeruleumSphere = 0x304A,
    Nitrosphere = 0x304B,
    BitbladeA = 0x3164,
    BitbladeB = 0x3165
}

public enum AID : uint
{
    EmeraldBeam = 21873,
    PhotonLaserCircle = 21883,
    PhotonLaserDonut1 = 21885,
    PhotonLaserDonut2 = 21887,
    PhotonLaserDonut3 = 20504,
    PhotonLaserDonut4 = 20503,
    HeatRay = 23507,
    EmeraldCrusher1 = 21891,
    EmeraldCrusher2 = 21892,
    EmeraldCrusher3 = 21974,
    EmeraldCrusherKnockback = 21893,
    Sidescathe1 = 21894,
    Sidescathe2 = 21895,
    Sidescathe3 = 21896,
    Sidescathe4 = 21897,
    Sidescathe5 = 21972,
    Sidescathe6 = 21973,
    AireTamStorm1 = 21902,
    AireTamStorm2 = 21903,
    AireTamStorm3 = 21968,
    Expire1 = 21904,
    Expire2 = 21905,
    Expire3 = 21969,
    MagitekMagnetism1 = 21908,
    MagitekMagnetism2 = 21909,
    PulseLaser = 21914,
    AetheroplasmProduction1 = 21930,
    AetheroplasmProduction2 = 21931,
    ExtinctionImpact = 21933,
    EmeraldShot = 21936,
    OptimizedUltima1 = 21937,
    OptimizedUltima2 = 21938,
    Breakthrough = 21939,
    LegioPhantasmatis = 21940,
    UnluckyLot = 21941,
    ShotsFired = 21943,
    MagitekCannon = 21950,
    DivideEtImperaVisual = 21851,
    DivideEtImpera = 21977,
    PrimusTerminusEstVisual = 21955,
    SecundusTerminusEst = 21963,
    TertiusTerminusEstCircle = 21965,
    TertiusTerminusEst = 21966,
    DisruptionField = 23298
}

sealed class Raidwides(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.OptimizedUltima1, (uint)AID.OptimizedUltima2, (uint)AID.DisruptionField, (uint)AID.MagitekCannon]);
sealed class EmeraldShot(BossModule module) : Components.SingleTargetCast(module, (uint)AID.EmeraldShot);
sealed class PhotonLaserCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PhotonLaserCircle, 12f);
sealed class PhotonLaserDonut1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PhotonLaserDonut1, new AOEShapeDonut(10f, 17f));
sealed class PhotonLaserDonut2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PhotonLaserDonut2, new AOEShapeDonut(16f, 23f));
sealed class PhotonLaserDonut3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PhotonLaserDonut3, new AOEShapeDonut(15f, 20f));
sealed class PhotonLaserDonut4(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PhotonLaserDonut4, new AOEShapeDonut(20f, 25f));
sealed class HeatRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeatRay, new AOEShapeCone(25f, 30f.Degrees()));
sealed class EmeraldCrusher(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.EmeraldCrusher1, (uint)AID.EmeraldCrusher2, (uint)AID.EmeraldCrusher3], 6f);
sealed class EmeraldCrusherKnockback(BossModule module) : Components.SimpleKnockbackGroups(module, [(uint)AID.EmeraldCrusher1, (uint)AID.EmeraldCrusher2, (uint)AID.EmeraldCrusher3], 18f, stopAtWall: true);
sealed class Sidescathe(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Sidescathe1, (uint)AID.Sidescathe2, (uint)AID.Sidescathe3, (uint)AID.Sidescathe4, (uint)AID.Sidescathe5,
    (uint)AID.Sidescathe6], new AOEShapeRect(50f, 12.5f));
sealed class AireTamStorm(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.AireTamStorm1, (uint)AID.AireTamStorm2, (uint)AID.AireTamStorm3], 6f);
sealed class Expire(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Expire1, (uint)AID.Expire2, (uint)AID.Expire3], 12f);
sealed class PulseLaser(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PulseLaser, new AOEShapeRect(50f, 3f));
sealed class Breakthrough(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Breakthrough, new AOEShapeRect(53f, 16f));
sealed class UnluckyLot(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UnluckyLot, 8f);
sealed class ShotsFired(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ShotsFired, new AOEShapeRect(50f, 2.5f));
sealed class TertiusTerminusEstCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TertiusTerminusEstCircle, 5f);
sealed class TerminusEstCrosses(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.SecundusTerminusEst, (uint)AID.TertiusTerminusEst], new AOEShapeCross(25f, 5f));
sealed class MagitekMagnetism(BossModule module) : Components.CastHints(module, [(uint)AID.MagitekMagnetism1, (uint)AID.MagitekMagnetism2], "Magnetism: watch your charge relative to the mines!");
sealed class AetheroplasmProduction(BossModule module) : Components.CastHints(module, [(uint)AID.AetheroplasmProduction1, (uint)AID.AetheroplasmProduction2], "Intercept the spheres before they meet!");

sealed class DivideEtImpera(BossModule module) : Components.GenericBaitAway(module, (uint)AID.DivideEtImpera)
{
    private static readonly AOEShapeCone _cone = new(40f, 15f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.DivideEtImperaVisual)
            return;
        var party = Raid.WithoutSlot();
        var activation = Module.CastFinishAt(spell, 1d);
        for (var i = 0; i < party.Length; ++i)
            CurrentBaits.Add(new(caster, party[i], _cone, activation));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}

sealed class Ex6EmeraldWeaponStates : StateMachineBuilder
{
    public Ex6EmeraldWeaponStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Raidwides>()
            .ActivateOnEnter<EmeraldShot>()
            .ActivateOnEnter<PhotonLaserCircle>()
            .ActivateOnEnter<PhotonLaserDonut1>()
            .ActivateOnEnter<PhotonLaserDonut2>()
            .ActivateOnEnter<PhotonLaserDonut3>()
            .ActivateOnEnter<PhotonLaserDonut4>()
            .ActivateOnEnter<HeatRay>()
            .ActivateOnEnter<EmeraldCrusher>()
            .ActivateOnEnter<EmeraldCrusherKnockback>()
            .ActivateOnEnter<Sidescathe>()
            .ActivateOnEnter<AireTamStorm>()
            .ActivateOnEnter<Expire>()
            .ActivateOnEnter<PulseLaser>()
            .ActivateOnEnter<Breakthrough>()
            .ActivateOnEnter<UnluckyLot>()
            .ActivateOnEnter<ShotsFired>()
            .ActivateOnEnter<TertiusTerminusEstCircle>()
            .ActivateOnEnter<TerminusEstCrosses>()
            .ActivateOnEnter<MagitekMagnetism>()
            .ActivateOnEnter<AetheroplasmProduction>()
            .ActivateOnEnter<DivideEtImpera>()
            .Raw.Update = () =>
            {
                var p2 = module.Enemies((uint)OID.BossP2);
                return p2.Count != 0 && p2[0].IsDead || module.PrimaryActor.IsDestroyed && p2.Count == 0;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 763, NameID = 9696, PlanLevel = 80)]
public sealed class Ex6EmeraldWeapon(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f))
{
    private static readonly uint[] _enemies = [(uint)OID.BossP2, (uint)OID.BlackWolfsImage, (uint)OID.CeruleumSphere, (uint)OID.Nitrosphere];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, _enemies);
    }
}
