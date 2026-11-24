namespace BossMod.Shadowbringers.Alliance.A11CommandModel;

sealed class A11CommandModelStates : StateMachineBuilder
{
    public A11CommandModelStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ClangingBlow>()
            .ActivateOnEnter<ForcefulImpact>()
            .ActivateOnEnter<EnergyBombardment>()
            .ActivateOnEnter<CentrifugalSpin>()
            .ActivateOnEnter<SidestrikingSpin>()
            .ActivateOnEnter<HighCaliberLaser>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<ShockwaveRaidwide>()
            .ActivateOnEnter<EnergyRing>()
            .ActivateOnEnter<EnergyAssault>()
            .ActivateOnEnter<EnergyBomb>()
            .ActivateOnEnter<HighPoweredLaser>()
            .ActivateOnEnter<AirToSurfaceEnergy>();
    }
}
