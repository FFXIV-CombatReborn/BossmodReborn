namespace BossMod.Dawntrail.Quantum.Q1FinalVerse;

sealed class Q1FinalVerseStates : StateMachineBuilder
{
    private readonly Q1FinalVerse _module;

    public Q1FinalVerseStates(Q1FinalVerse module) : base(module)
    {
        _module = module;
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<Quantumlevel>()
            .ActivateOnEnter<TerrorEyeVoidTrapBallOfFire>()
            .ActivateOnEnter<BladeOfFirstLight>()
            .ActivateOnEnter<ChainsOfCondemnation>()
            .ActivateOnEnter<BallOfFireBait>()
            .ActivateOnEnter<Eruption>()
            .ActivateOnEnter<UnholyDarkness>()
            .ActivateOnEnter<SelfDestruct>()
            .ActivateOnEnter<SearingChain>()
            .ActivateOnEnter<SearingChainCross>()
            .ActivateOnEnter<Spinelash>()
            .ActivateOnEnter<Vodoriga>()
            .ActivateOnEnter<ShackleSpreadHint>()
            .ActivateOnEnter<ShackleHint>()
            .ActivateOnEnter<ArcaneFont>()
            .ActivateOnEnter<HellishEarthPull>()
            .ActivateOnEnter<ManifoldLashingsTower>()
            .ActivateOnEnter<ManifoldLashingsTail>()
            .ActivateOnEnter<Flameborn>()
            .ActivateOnEnter<FlamebornAura>()
            .ActivateOnEnter<CrimeAndPunishmentHint>()
            .ActivateOnEnter<SinBearer>()
            .ActivateOnEnter<Doom>();
    }

    enum QuantumLevel
    {
        Level15,
        Level20,
        Level25,
        Level30,
        Level35,
        Level40
    }

    private void SinglePhase(uint id)
    {
        var dispatch = new Dictionary<QuantumLevel, (uint, Action<uint>)>
        {
            [QuantumLevel.Level15] = (1u, SinglePhaseRest),
            [QuantumLevel.Level20] = (2u, SinglePhaseRest),
            [QuantumLevel.Level25] = (3u, SinglePhaseRest),
            [QuantumLevel.Level30] = (4u, SinglePhaseRest),
            [QuantumLevel.Level35] = (5u, SinglePhaseRest),
            [QuantumLevel.Level40] = (6u, SinglePhaseQ40),
        };

        ConditionFork(id, default, () => _module.FindComponent<Quantumlevel>()!.QuantumLevel >= 15u, SelectCase, dispatch, "Select quantum level")
            .DeactivateOnExit<Quantumlevel>();
    }

    private QuantumLevel SelectCase()
    {
        return _module.FindComponent<Quantumlevel>()!.QuantumLevel switch
        {
            >= 35u and < 40u => QuantumLevel.Level35,
            >= 30u and < 35u => QuantumLevel.Level30,
            >= 25u and < 30u => QuantumLevel.Level25,
            >= 20u and < 25u => QuantumLevel.Level20,
            < 20u => QuantumLevel.Level15,
            _ => QuantumLevel.Level40
        };
    }

    private void SinglePhaseQ40(uint id)
    {
        BoundsOfSinScourgingBlaze(id, 12.1f);
        SimpleState(id + 0xFF0000u, 10000f, "???");
    }

    // private void SinglePhaseQ15(uint id)
    // {
    //     BoundsOfSinScourgingBlaze(id, 12.1f);
    //     SimpleState(id + 0xFF0000u, 10000f, "???");
    // }

    private void SinglePhaseRest(uint id)
    {
        SimpleState(id + 0xFF0000u, 10000f, "Unsupported quantum level (upload replays!)");
    }

    private void BoundsOfSinScourgingBlaze(uint id, float delay)
    {
        ComponentCondition<ScourgingBlaze>(id, delay, static comp => comp.Direction != 0, "Exaflare direction 1")
            .ActivateOnEnter<ScourgingBlaze>();
        ComponentCondition<ScourgingBlaze>(id + 0x10u, 8f, static comp => comp.Direction == 2, "Exaflare direction 2");
        ComponentCondition<BoundsOfSinPull>(id + 0x20u, 13.1f, static comp => comp.NumCasts != 0, "Pull into mid")
            .ActivateOnEnter<BoundsOfSinPull>()
            .ActivateOnEnter<BoundsOfSinTowers>()
            .DeactivateOnExit<BoundsOfSinPull>()
            .ActivateOnEnter<BoundsOfSinEnd>();
        ComponentCondition<BoundsOfSinSmallAOE>(id + 0x30u, 4f, static comp => comp.NumCasts != 0, "Small AOEs start")
            .ActivateOnEnter<LightDarkNeutralize>()
            .ActivateOnEnter<BoundsOfSinSmallAOE>();
        ComponentCondition<BoundsOfSinSmallAOE>(id + 0x40u, 1.6f, static comp => comp.NumCasts == 12, "Small AOEs end")
            .DeactivateOnExit<BoundsOfSinSmallAOE>();
        ComponentCondition<BoundsOfSinEnd>(id + 0x50u, 1.4f, static comp => comp.NumCasts != 0 && comp.Pillars.Count == 0, "In/Out AOE + arena restored")
            .DeactivateOnExit<BoundsOfSinEnd>();
        ComponentCondition<LightDarkNeutralize>(id + 0x60u, 1.2f, static comp => comp.NumCasts != 0, "Stacks resolve")
            .DeactivateOnExit<LightDarkNeutralize>();
        ComponentCondition<BoundsOfSinTowers>(id + 0x70u, 5.1f, static comp => comp.NumCasts != 0, "Towers resolve + raidwide") // some variation with timings here, either 4.1s or 5.1s
            .SetHint(StateMachine.StateHint.Raidwide)
            .ExecOnExit<ScourgingBlaze>(static comp => comp.ShowAOEs())
            .DeactivateOnExit<BoundsOfSinTowers>();
        ComponentCondition<ScourgingBlaze>(id + 0x80u, 6.9f, static comp => comp.NumCasts != 0, "Exaflares start");
        ComponentCondition<ScourgingBlaze>(id + 0x90u, 7.8f, static comp => comp.Lines.Count == 0, "Exaflares end")
            .DeactivateOnExit<ScourgingBlaze>();
    }
}
