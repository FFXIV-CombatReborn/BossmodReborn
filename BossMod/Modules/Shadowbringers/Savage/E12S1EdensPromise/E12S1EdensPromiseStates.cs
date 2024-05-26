namespace BossMod.Shadowbringers.Savage.E12S1EdensPromise;

class E12S1EdensPromiseStates : StateMachineBuilder
{
    public E12S1EdensPromiseStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IceFloe>()
            .ActivateOnEnter<IcicleImpact>()
            .ActivateOnEnter<JudgmentJolt>()
            .ActivateOnEnter<PlungingIce>()
            .ActivateOnEnter<PulseOfTheLand>()
            .ActivateOnEnter<TemporaryCurrent>()
            .ActivateOnEnter<RapturousReach1>()
            .ActivateOnEnter<RapturousReach2>()
            .ActivateOnEnter<Ferostorm>()
            .ActivateOnEnter<ConflagStrike>()
            .ActivateOnEnter<Maleficium>();
    }
}
