namespace BossMod.Dawntrail.Raid.M10NDaringDevils;

sealed class M10NDaringDevilsStates : StateMachineBuilder
{
    private readonly M10NDaringDevils _module;

    public M10NDaringDevilsStates(M10NDaringDevils module) : base(module)
    {
        _module = module;

        TrivialPhase()
            .ActivateOnEnter<HotImpact>()
            .ActivateOnEnter<DeepImpact>()
            .ActivateOnEnter<DiversDare>()
            .ActivateOnEnter<DiversDareBlue>()
            .ActivateOnEnter<FiresnakingWatersnaking>()

            .ActivateOnEnter<CutbackBlazeBait>()
            .ActivateOnEnter<CutbackBlazePersistent>()

            .ActivateOnEnter<AlleyOopInfernoSpread>()
            .ActivateOnEnter<AlleyOopInfernoPuddles>()
            .ActivateOnEnter<AlleyOopMaelstromSequential>()
            .ActivateOnEnter<SteamBurst>()
            .ActivateOnEnter<HotAerialTowers>()
            .ActivateOnEnter<HotAerialFirePuddles>()

            .ActivateOnEnter<DeepVarialCone>()
            .ActivateOnEnter<SickestTakeOffLine>()
            .ActivateOnEnter<SickSwellKB>()

            .ActivateOnEnter<PyrotationStack>()
            .ActivateOnEnter<PyrotationPuddles>()
            .ActivateOnEnter<XtremeSpectacularRaidwide>()
            .ActivateOnEnter<XtremeSpectacularEdge>()

            .ActivateOnEnter<InsaneAirSnaps>()
            .ActivateOnEnter<BlastingSnapPersistent>()

            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && (_module.DeepBlue?.IsDeadOrDestroyed ?? true);
    }
}
