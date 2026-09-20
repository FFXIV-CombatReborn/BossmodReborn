namespace BossMod.Endwalker.Raid.P1NErichthonios;

sealed class P1NStates : StateMachineBuilder
{
    public P1NStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GaolersFlail>()
            .ActivateOnEnter<PitilessFlail>()
            .ActivateOnEnter<TrueHoly>()
            .ActivateOnEnter<HeavyHand>()
            .ActivateOnEnter<Raidwides>()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<AetherExplosion>()
            .ActivateOnEnter<Intemperance>();
    }
}
