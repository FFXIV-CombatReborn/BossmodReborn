namespace BossMod.Dawntrail.Raid.M09NVampFatale;

sealed class M09NVampFataleStates : StateMachineBuilder
{
    public M09NVampFataleStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<KillerVoice>()
            .ActivateOnEnter<HalfMoon>()
            .ActivateOnEnter<VampStomp>()
            .ActivateOnEnter<Hardcore>()
            .ActivateOnEnter<Hardcore2>()
            .ActivateOnEnter<FlayingFry>()
            .ActivateOnEnter<CoffinFiller>()
            .ActivateOnEnter<PenetratingPitch>()
            .ActivateOnEnter<BlastBeat>()
            .ActivateOnEnter<DeadWake>()
            .ActivateOnEnter<BrutalRain>()
            .ActivateOnEnter<FinaleFatale>()
            .ActivateOnEnter<FinaleFataleRaidwide>()
            .ActivateOnEnter<BarbedBurst>()
            .ActivateOnEnter<CrowdKill>()
            .ActivateOnEnter<PulpingPulse>()
            .ActivateOnEnter<AetherlettingHit>()
            .ActivateOnEnter<AetherlettingCross>()
            .ActivateOnEnter<InsatiableThirst>()
            .ActivateOnEnter<Plummet>()
            .ActivateOnEnter<NeckBiter>()
            .ActivateOnEnter<CoffinMaker>();
    }
}
