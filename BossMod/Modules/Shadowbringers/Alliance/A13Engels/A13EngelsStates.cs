namespace BossMod.Shadowbringers.Alliance.A13Engels;

sealed class A13MarxEngelsStates : StateMachineBuilder
{
    public A13MarxEngelsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DemolishStructureArenaChange>()
            .ActivateOnEnter<IncendiarySaturationBombingArenaChange>()
            .ActivateOnEnter<MarxSmashLR>()
            .ActivateOnEnter<MarxSmashOutside>()
            .ActivateOnEnter<MarxSmashInside>()
            .ActivateOnEnter<MarxCrush>()
            .ActivateOnEnter<MarxThrust>()
            .ActivateOnEnter<IncendiarySaturationBombing>()
            .ActivateOnEnter<CrushingWheel>()
            .ActivateOnEnter<PrecisionGuidedMissile>()
            .ActivateOnEnter<IncendiaryBombing>()
            .ActivateOnEnter<IncendiaryBombingVoidzone>()
            .ActivateOnEnter<DiffuseLaser>()
            .ActivateOnEnter<SurfaceMissile>()
            .ActivateOnEnter<GuidedMissile>()
            .ActivateOnEnter<LaserSight>()
            .ActivateOnEnter<EnergyDispersal>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<ArmLaser>()
            .ActivateOnEnter<ReverseJointedGoliathCleave>()
            .ActivateOnEnter<AddsArms>()
            .ActivateOnEnter<WideAngleDiffuseLaser>()
            .ActivateOnEnter<RadiateHeat>();
    }
}
