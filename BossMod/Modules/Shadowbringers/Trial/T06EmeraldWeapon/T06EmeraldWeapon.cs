namespace BossMod.Shadowbringers.Trial.T06EmeraldWeapon;

public enum OID : uint
{
    Boss = 0x303A,
    BossP2 = 0x3153,
    Helper = 0x233C,
    BlackWolfsImage = 0x2DD0,
    MagneticMine = 0x303D,
    ClawBitA = 0x303B,
    ClawBitB = 0x303E,
    Energy = 0x303C,
    ImperialImage = 0x303F,
    BitbladeA = 0x3162,
    BitbladeB = 0x3163,
    EmeraldCloneA = 0x316D,
    EmeraldCloneB = 0x316E
}

public enum AID : uint
{
    EmeraldBeam1 = 21802,
    EmeraldBeam2 = 21808,
    PhotonLaserCircle = 21812,
    PhotonLaserDonut1 = 21814,
    PhotonLaserDonut2 = 21816,
    PhotonLaserDonut3 = 20504,
    PhotonLaserDonut4 = 20503,
    HeatRayDonut = 20380,
    HeatRay = 20381,
    DivideEtImperaVisual1 = 21813,
    DivideEtImperaVisual2 = 21817,
    Split1 = 21818,
    Split2 = 21819,
    EmeraldCrusher1 = 21820,
    EmeraldCrusher2 = 21821,
    EmeraldCrusherKnockback = 21822,
    Sidescathe1 = 21823,
    Sidescathe2 = 21824,
    Sidescathe3 = 21825,
    Sidescathe4 = 21826,
    PulseLaserVisual = 21831,
    PulseLaser = 21832,
    BitStorm = 21834,
    BitPlasma = 21839,
    Aetheroplasm = 21841,
    EmeraldShot = 21844,
    OptimizedUltima = 21845,
    LegioPhantasmatis = 21849,
    RankAndFile = 21850,
    FireAway = 21854,
    ShotsFired = 21855,
    Heirsbane = 21857,
    PrimusTerminusEstVisual = 21858,
    PrimusTerminusEstLine = 21862,
    SecundusTerminusEstVisual = 21863,
    SecundusTerminusEst = 21866,
    TertiusTerminusEstVisual = 21867,
    TertiusTerminusEstCircle = 21868,
    TertiusTerminusEst = 21869,
    DivideEtImpera = 21871,
    PrimusTerminusEstCircle = 19262,
    DisruptionField = 23298,
    MagitekMagnetism1 = 23301,
    MagitekMagnetism2 = 23302,
    Explosion = 23300,
    OptimizedUltimaP2 = 23311
}

sealed class Raidwides(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.OptimizedUltima, (uint)AID.OptimizedUltimaP2, (uint)AID.DisruptionField]);
sealed class EmeraldShot(BossModule module) : Components.SingleTargetCast(module, (uint)AID.EmeraldShot);
sealed class PhotonLaserCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PhotonLaserCircle, 12f);
sealed class PhotonLaserDonut1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PhotonLaserDonut1, new AOEShapeDonut(10f, 17f));
sealed class PhotonLaserDonut2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PhotonLaserDonut2, new AOEShapeDonut(16f, 23f));
sealed class PhotonLaserDonut3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PhotonLaserDonut3, new AOEShapeDonut(15f, 20f));
sealed class PhotonLaserDonut4(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PhotonLaserDonut4, new AOEShapeDonut(20f, 25f));
sealed class HeatRayDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeatRayDonut, new AOEShapeDonut(7f, 30f));
sealed class HeatRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeatRay, new AOEShapeCone(25f, 30f.Degrees()));
sealed class EmeraldCrusher(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.EmeraldCrusher1, (uint)AID.EmeraldCrusher2], 6f);
sealed class EmeraldCrusherKnockback(BossModule module) : Components.SimpleKnockbackGroups(module, [(uint)AID.EmeraldCrusher1, (uint)AID.EmeraldCrusher2], 18f, stopAtWall: true);
sealed class Sidescathe(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Sidescathe1, (uint)AID.Sidescathe2, (uint)AID.Sidescathe3, (uint)AID.Sidescathe4], new AOEShapeRect(50f, 12.5f));
sealed class PulseLaser(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PulseLaser, new AOEShapeRect(50f, 3f));
sealed class DivideEtImpera(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DivideEtImpera, new AOEShapeCone(40f, 15f.Degrees()));
sealed class ShotsFired(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ShotsFired, new AOEShapeRect(50f, 2.5f));
sealed class PrimusTerminusEstCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PrimusTerminusEstCircle, 5f);
sealed class TertiusTerminusEstCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TertiusTerminusEstCircle, 5f);
sealed class TerminusEstCrosses(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.SecundusTerminusEst, (uint)AID.TertiusTerminusEst], new AOEShapeCross(25f, 5f));
sealed class MagitekMagnetism(BossModule module) : Components.CastHints(module, [(uint)AID.MagitekMagnetism1, (uint)AID.MagitekMagnetism2], "Magnetism: watch your charge relative to the mines!");

sealed class MagneticMines(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _mines = module.Enemies((uint)OID.MagneticMine);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        for (var i = 0; i < _mines.Count; ++i)
        {
            var m = _mines[i];
            if (!m.IsDead)
                Arena.AddCircleUnfilled(m.Position, 12f, Colors.Danger);
        }
    }
}

sealed class T06EmeraldWeaponStates : StateMachineBuilder
{
    public T06EmeraldWeaponStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Raidwides>()
            .ActivateOnEnter<EmeraldShot>()
            .ActivateOnEnter<PhotonLaserCircle>()
            .ActivateOnEnter<PhotonLaserDonut1>()
            .ActivateOnEnter<PhotonLaserDonut2>()
            .ActivateOnEnter<PhotonLaserDonut3>()
            .ActivateOnEnter<PhotonLaserDonut4>()
            .ActivateOnEnter<HeatRayDonut>()
            .ActivateOnEnter<HeatRay>()
            .ActivateOnEnter<EmeraldCrusher>()
            .ActivateOnEnter<EmeraldCrusherKnockback>()
            .ActivateOnEnter<Sidescathe>()
            .ActivateOnEnter<PulseLaser>()
            .ActivateOnEnter<DivideEtImpera>()
            .ActivateOnEnter<ShotsFired>()
            .ActivateOnEnter<PrimusTerminusEstCircle>()
            .ActivateOnEnter<TertiusTerminusEstCircle>()
            .ActivateOnEnter<TerminusEstCrosses>()
            .ActivateOnEnter<MagitekMagnetism>()
            .ActivateOnEnter<MagneticMines>()
            .Raw.Update = () =>
            {
                var p2 = module.Enemies((uint)OID.BossP2);
                return p2.Count != 0 && p2[0].IsDead || module.PrimaryActor.IsDestroyed && p2.Count == 0;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 762, NameID = 9696)]
public sealed class T06EmeraldWeapon(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f))
{
    private static readonly uint[] _enemies = [(uint)OID.BossP2, (uint)OID.BlackWolfsImage];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, _enemies);
    }
}
