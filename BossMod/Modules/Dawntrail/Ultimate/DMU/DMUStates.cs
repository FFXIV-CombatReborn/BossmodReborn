namespace BossMod.Dawntrail.Ultimate.DMU;

sealed class DMUStates : StateMachineBuilder
{
    private readonly DMU _module;

    public DMUStates(DMU module) : base(module)
    {
        _module = module;

        SimplePhase(0u, Phase1, "P1")
            .ActivateOnEnter<RevoltingRuinIIIFirst>()
            .ActivateOnEnter<RevoltingRuinIIISecond>()
            .Raw.Update = () => !Module.PrimaryActor.IsTargetable;
        SimplePhase(1u, Phase2, "P2")
            .SetHint(StateMachine.PhaseHint.StartWithDowntime)
            .Raw.Update = () => _module.BossP2() is { IsTargetable: false, HPRatio: < 1 };
        SimplePhase(2u, Phase3, "P3")
            .SetHint(StateMachine.PhaseHint.StartWithDowntime)
            .Raw.Update = () => _module.ChaosP3()?.IsDeadOrDestroyed == true && _module.ExdeathP3()?.IsDeadOrDestroyed == true;
        SimplePhase(3u, Phase4, "P4")
            .SetHint(StateMachine.PhaseHint.StartWithDowntime)
            .Raw.Update = () => _module.KefkaP4()?.IsDeadOrDestroyed == true;
        SimplePhase(4u, Phase5, "P5")
            .SetHint(StateMachine.PhaseHint.StartWithDowntime)
            .Raw.Update = () => _module.KefkaP5()?.IsDeadOrDestroyed == true;
    }

    private void Phase1(uint id)
    {
        Phase1RevoltingRuinIII(id, 10.1f);
        Phase1GravenImage1(id + 0x1000u, 7.8f);
        Phase1LightOfJudgement(id + 0x2000u, 4.1f);
        Phase1Gravitas(id + 0x3000u, 4.0f);
        Phase1LightOfJudgement(id + 0x4000u, 6.2f);
        Phase1TeleTrouncing(id + 0x5000u, 6.9f);
    }

    void Phase1RevoltingRuinIII(uint id, float delay)
    {
        CastStart(id, AID.RevoltingRuinIIIFirstHit, delay, "RevoltingRuinIII")
            .ExecOnEnter<RevoltingRuinIIIFirst>(static o => o.EnableHints = true);
        ComponentCondition<RevoltingRuinIIIFirst>(id + 0x10, 5.1f, static o => o.NumCasts > 0, "1st Tankbuster Resolve")
            .DeactivateOnExit<RevoltingRuinIIIFirst>()
            .ExecOnExit<RevoltingRuinIIISecond>(static o => o.EnableHints = true);
        ComponentCondition<RevoltingRuinIIISecond>(id + 0x20, 3.2f, static o => o.NumCasts > 0, "2nd Tankbuster Resolve")
            .DeactivateOnExit<RevoltingRuinIIISecond>();
    }

    void Phase1GravenImage1(uint id, float delay)
    {
        Cast(id + 0x10, AID.GravenImage, delay, 3.0f, "Graven Image 1")
            .ActivateOnEnter<PulseWave>();
        CastStart(id + 0x20, AID.MysteryMagic, 3.2f, "MysteryMagic")
            .ActivateOnEnter<BlizzardIIIBlowoutGravenImage1>()
            .ActivateOnEnter<FlagrantFire>();
        ComponentCondition<PulseWave>(id + 0x30, 2.6f, static o => o.NumCasts >= 4, "PulseWave Knockbacks")
            .DeactivateOnExit<PulseWave>()
            .ExecOnExit<BlizzardIIIBlowoutGravenImage1>(o => o.Risky = true);
        ComponentCondition<BlizzardIIIBlowoutGravenImage1>(id + 0x40, 2.4f, static o => o.NumCasts > 0, "BlizzardIII Blowout")
            .DeactivateOnExit<BlizzardIIIBlowoutGravenImage1>();
        ComponentCondition<FlagrantFire>(id + 0x50, 0.8f, static o => !o.Active, "Stack / Spread")
            .DeactivateOnExit<FlagrantFire>()
            .ActivateOnExit<WaveCannon>();
        CastStart(id + 0x60, AID.DoubleTroubleTrap, 3.4f, "Double Trouble Cast");
        ComponentCondition<WaveCannon>(id + 0x70, 0.8f, static o => o.ActiveBaits.Count == 0, "Wave Cannon Spreads")
            .DeactivateOnExit<WaveCannon>()
            .ActivateOnExit<WaveCannonTowers>();
        ComponentCondition<WaveCannonTowers>(id + 0x80, 3.6f, static o => o.NumCasts > 0, "Towers resolve")
            .ActivateOnEnter<DoubleTroubleTrapStacks>()
            .ActivateOnEnter<DoubleTroubleTrapKnockback>()
            .DeactivateOnExit<WaveCannonTowers>()
            .ExecOnExit<DoubleTroubleTrapStacks>(static o => o.resolving = true);
        CastStart(id + 0x90u, AID.MysteryMagic, 2.7f)
            .ActivateOnEnter<LightningSafeSpots>()
            .ActivateOnEnter<BlizzardIIIBlowout>();
        ComponentCondition<DoubleTroubleTrapStacks>(id + 0x100, 1.0f, static o => o.NumCasts == 2, "Stacks + Knockbacks")
            .ExecOnExit<DoubleTroubleTrapStacks>(static o => o.resolving = false)
            .ExecOnExit<BlizzardIIIBlowout>(static o => o.Risky = true)
            .ExecOnExit<LightningSafeSpots>(static o => o.Risky = true);
        ComponentCondition<BlizzardIIIBlowout>(id + 0x110, 4.0f, static o => o.NumCasts > 0, "Blizzard + Lightning safe spots")
            .DeactivateOnExit<DoubleTroubleTrapStacks>() // Has to be deactivated later since it contains AI movement
            .DeactivateOnExit<DoubleTroubleTrapKnockback>() // Has to be deactivated later since it contains AI movement
            .DeactivateOnExit<LightningSafeSpots>()
            .DeactivateOnExit<BlizzardIIIBlowout>()
            .ActivateOnExit<HyperDrive>(); // Activated early for pre-position movement
    }

    void Phase1LightOfJudgement(uint id, float delay)
    {
        Cast(id + 0x10u, AID.P1LightOfJudgment, delay, 5.0f, "LightOfJudgment")
            .ActivateOnEnter<LightOfJudgment>()
            .DeactivateOnExit<LightOfJudgment>();
        ComponentCondition<HyperDrive>(id + 0x20, 3.2f, static o => o.NumCasts > 0, "1st Tankbuster");
        ComponentCondition<HyperDrive>(id + 0x30, 2.1f, static o => o.NumCasts > 1, "2nd Tankbuster");
        ComponentCondition<HyperDrive>(id + 0x40, 2.1f, static o => o.NumCasts > 2, "3rd Tankbuster")
            .DeactivateOnExit<HyperDrive>();
    }

    void Phase1Gravitas(uint id, float delay)
    {
        CastStart(id + 0x10, AID.GravenImage, 7.3f, "Graven Image 2")
            .ActivateOnEnter<BlizzardIIIBlowout>()
            .ExecOnEnter<BlizzardIIIBlowout>(static o => o.Risky = true)
            .ActivateOnEnter<Gravitas>()
            .ExecOnEnter<Gravitas>(static o => o.side = P1GravitasData.Side.NORTH);
        ComponentCondition<BlizzardIIIBlowout>(id + 0x20, 10.1f, static o => o.NumCasts > 0, "Blizzard safe spots")
            .DeactivateOnExit<BlizzardIIIBlowout>();
        ComponentCondition<Gravitas>(id + 0x30, 0.1f, static o => o.ActiveStacks.Count == 0, "Stacks")
            .ActivateOnEnter<GravitasPuddles>();
        ComponentCondition<Gravitas>(id + 0x40, 4.1f, static o => !o.Active, "Spreads")
            .DeactivateOnExit<Gravitas>()
            .ActivateOnExit<RevoltingRuinIIIFirst>() // Activated early for pre-position movement
            .ActivateOnExit<RevoltingRuinIIISecond>(); // Activated early for pre-position movement

        CastStart(id + 0x50, AID.RevoltingRuinIIIFirstHit, 0.7f, "RevoltingRuinIII")
            .ExecOnEnter<RevoltingRuinIIIFirst>(static o => o.EnableHints = true)
            .ActivateOnExit<GravitationalWave>()
            .ExecOnExit<GravitationalWave>(static o => o.Risky = true);
        ComponentCondition<RevoltingRuinIIIFirst>(id + 0x60, 5.1f, static o => o.NumCasts > 0, "1st Tankbuster Resolve")
            .DeactivateOnExit<RevoltingRuinIIIFirst>()
            .ExecOnExit<RevoltingRuinIIISecond>(static o => o.EnableHints = true);
        ComponentCondition<RevoltingRuinIIISecond>(id + 0x70, 3.2f, static o => o.NumCasts > 0, "2nd Tankbuster Resolve")
            .DeactivateOnExit<RevoltingRuinIIISecond>()
            .ActivateOnExit<Gravitas>()
            .ExecOnExit<Gravitas>(static o => o.side = P1GravitasData.Side.SOUTH);
        ComponentCondition<GravitationalWave>(id + 0x80, 0.80f, static o => o.NumCasts > 0, "Left/Right Cleave")
            .DeactivateOnExit<GravitationalWave>();

        ComponentCondition<Gravitas>(id + 0x90, 4.6f, static o => o.ActiveStacks.Count == 0, "Stacks");
        ComponentCondition<Gravitas>(id + 0x100, 4.0f, static o => !o.Active, "Spreads")
            .ActivateOnEnter<GravitationalWave>()
            .DeactivateOnExit<Gravitas>()
            .ExecOnExit<GravitationalWave>(static o => o.Risky = true);

        ComponentCondition<GravitationalWave>(id + 0x110, 4.5f, static o => o.NumCasts > 0, "Left/Right Cleave")
            .ActivateOnEnter<DoubleTroubleTrapStacksGravitas>()
            .ActivateOnEnter<DoubleTroubleTrapKnockback>()
            .ExecOnEnter<DoubleTroubleTrapStacksGravitas>(static o => o.resolving = true)
            .DeactivateOnExit<GravitationalWave>();

        ComponentCondition<DoubleTroubleTrapKnockback>(id + 0x120, 3.8f, static o => o.NumCasts == 2, "Stacks + Knockbacks")
            .ExecOnExit<DoubleTroubleTrapStacks>(static o => o.resolving = false);

        ComponentCondition<GravitasPuddles>(id + 0x130, 3.0f, static o => o.Puddles.Count == 0, "Puddle explosions")
            .DeactivateOnExit<DoubleTroubleTrapStacks>() // Has to be deactivated later since it contains AI movement
            .DeactivateOnExit<DoubleTroubleTrapKnockback>() // Has to be deactivated later since it contains AI movement
            .DeactivateOnExit<GravitasPuddles>()
            .ActivateOnExit<HyperDrive>(); // Activated early for pre-position movement
    }

    void Phase1TeleTrouncing(uint id, float delay)
    {
        Cast(id + 0x10u, AID.TeleTrouncing, delay, 5.0f, "TeleTrouncing")
            .ActivateOnEnter<TeleTrouncing>();
        ComponentCondition<TeleTrouncing>(id + 0x20, 7.8f, static o => o.NumCasts > 0, "First Arrows");
        CastStart(id + 0x30, AID.GravenImage, 1.3f, "Graven Image 3");
        ComponentCondition<TeleTrouncing>(id + 0x40, 1.7f, static o => o.NumCasts > 8, "Second Arrows")
            .ActivateOnExit<DoubleTroubleTrapStacksTeleTrouncing>()
            .ActivateOnExit<DoubleTroubleTrapKnockback>()
            .ExecOnExit<DoubleTroubleTrapStacksTeleTrouncing>(static o => o.resolving = true);
        ComponentCondition<DoubleTroubleTrapKnockback>(id + 0x50u, 5.4f, static o => o.NumCasts == 2, "Stacks + Knockbacks")
            .ExecOnExit<DoubleTroubleTrapStacks>(static o => o.resolving = false)
            .ActivateOnExit<TeleTrouncingTethers>();

        ComponentCondition<TeleTrouncingTethers>(id + 0x60u, 5.7f, static comp => !comp.Active, "Sleeps + Confusion Spreads - Start")
            .DeactivateOnExit<DoubleTroubleTrapStacksTeleTrouncing>() // Has to be deactivated later since it contains AI movement
            .DeactivateOnExit<DoubleTroubleTrapKnockback>() // Has to be deactivated later since it contains AI movement
            .ActivateOnExit<P1Gaze>();

        ComponentCondition<TeleTrouncingTethers>(id + 0x70u, 6.6f, static comp => comp.debuffActive && comp.debuffPlayers.None(), "Sleeps + Confusion Spreads - End")
            .ActivateOnExit<LightningSafeSpots>()
            .ActivateOnExit<GravenImage3FlagrantFire>()
            .ExecOnExit<LightningSafeSpots>(static o => o.Risky = true)
            .DeactivateOnExit<TeleTrouncingTethers>();

        CastStart(id + 0x80u, AID.MysteryMagic, 1.3f)
            .DeactivateOnExit<TeleTrouncing>();

        Condition(id + 0x90u, 5.1f, () => Module.FindComponent<LightningSafeSpots>()!.NumCasts > 0 && Module.FindComponent<P1Gaze>()!.NumCasts > 0, "Lightning + Gaze")
            .DeactivateOnExit<LightningSafeSpots>()
            .DeactivateOnExit<P1Gaze>();

        ComponentCondition<FlagrantFire>(id + 0x100, 0.6f, static o => !o.Active, "Stack / Spread")
            .DeactivateOnExit<GravenImage3FlagrantFire>();

        Targetable(id + 0x1000u, false, 10.4f, "Boss disappears");
    }

    private void Phase5(uint id)
    {
        ActorTargetable(id, _module.KefkaP5, true, 0.1f, "Boss appears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        ActorCast(id + 0x10u, _module.KefkaP5, AID.UltimaRepeaterCast, 3.0f, 5.0f, true, "Ultima Repeater")
            .ActivateOnEnter<UltimaRepeater>()
            .DeactivateOnExit<UltimaRepeater>()
            .ActivateOnEnter<FellForces>()
            .ExecOnEnter<FellForces>(static comp => comp.active = true);

        ComponentCondition<FellForces>(id + 0x20u, 6.0f, static comp => comp.NumCasts > 0, "1st Auto Attack Stack");
        ComponentCondition<FellForces>(id + 0x25u, 3.1f, static comp => comp.NumCasts > 3, "2nd Auto Attack Stack");
        ComponentCondition<FellForces>(id + 0x30u, 3.1f, static comp => comp.NumCasts > 6, "3rd Auto Attack Stack")
            .DeactivateOnExit<FellForces>();

        ActorCast(id + 0x40u, _module.KefkaP5, AID.ChaoticFlood, 0.3f, 5.0f, true, "Chaotic Flood")
            .ActivateOnEnter<ChaoticFlood>()
            .ActivateOnEnter<ChaoticFloodStack>();

        ComponentCondition<ChaoticFloodStack>(id + 0x45u, 1.0f, static comp => comp.NumCasts > 0, "1st Stack");
        ComponentCondition<ChaoticFlood>(id + 0x50u, 0.1f, static comp => comp.NumCasts > 0, "1st Flood");
        ComponentCondition<ChaoticFloodStack>(id + 0x55, 1.0f, static comp => comp.NumCasts > 1, "2nd Stack");
        ComponentCondition<ChaoticFlood>(id + 0x60u, 0.1f, static comp => comp.NumCasts > 2, "2nd Flood");
        ComponentCondition<ChaoticFloodStack>(id + 0x65, 1.0f, static comp => comp.NumCasts > 2, "3rd Stack");
        ComponentCondition<ChaoticFlood>(id + 0x70u, 0.1f, static comp => comp.NumCasts > 4, "3rd Flood");
        ComponentCondition<ChaoticFloodStack>(id + 0x75, 1.0f, static comp => comp.NumCasts > 3, "4th Stack");
        ComponentCondition<ChaoticFlood>(id + 0x80u, 0.1f, static comp => comp.NumCasts > 6, "4th Flood")
            .DeactivateOnExit<ChaoticFloodStack>()
            .DeactivateOnExit<ChaoticFlood>()
            .ActivateOnExit<MaddeningOrchestra>();

        ComponentCondition<MaddeningOrchestra>(id + 0x90u, 9.9f, static comp => comp.NumCasts > 0, "1st Baits Resolve")
            .ActivateOnExit<ChaoticFlareTB>()
            .ExecOnExit<ChaoticFlareTB>(static comp => comp.active = true);

        ComponentCondition<MaddeningOrchestra>(id + 0x100u, 3.2f, static comp => comp.NumCasts > 5, "2nd Baits + TB Resolve")
            .DeactivateOnExit<ChaoticFlareTB>()
            .DeactivateOnExit<MaddeningOrchestra>()
            .ActivateOnExit<ChaoticHolyFlareDiffusion>();

        ComponentCondition<ChaoticHolyFlareDiffusion>(id + 0x110u, 3.5f, static comp => comp.NumCasts >= 2, "Tank Baits Resolve")
            .DeactivateOnExit<ChaoticHolyFlareDiffusion>()
            .ActivateOnEnter<FellForces>()
            .ExecOnExit<FellForces>(static comp => comp.active = true)
            .ExecOnExit<FellForces>(static comp => comp.expectedCasts = 6);

        ComponentCondition<FellForces>(id + 0x120u, 4.6f, static comp => comp.NumCasts > 0, "1st Auto Attack Stack");
        ComponentCondition<FellForces>(id + 0x130u, 3.1f, static comp => comp.NumCasts > 3, "2nd Auto Attack Stack")
            .DeactivateOnExit<FellForces>();

        ActorCast(id + 0x140u, _module.KefkaP5, AID.Celestriad, 1.4f, 5.0f, true, "Celestraid")
            .ActivateOnEnter<Celestriad>()
            .ActivateOnEnter<CatastrophicChoice>();

        ComponentCondition<Celestriad>(id + 0x150u, 9.1f, static comp => comp.NumCasts > 0, "1st Tower Set");
        ComponentCondition<CatastrophicChoice>(id + 0x155u, 0.2f, static comp => comp.NumCasts > 0, "In/Out");
        ComponentCondition<Celestriad>(id + 0x160u, 5.8f, static comp => comp.NumCasts > 4, "2nd Tower Set");
        ComponentCondition<Celestriad>(id + 0x170u, 6.0f, static comp => comp.NumCasts > 8, "3rd Tower Set");
        ComponentCondition<CatastrophicChoice>(id + 0x175u, 0.2f, static comp => comp.NumCasts > 1, "2nd In/Out");

        ActorCast(id + 0x180u, _module.KefkaP5, AID.UltimaRepeaterCast, 4.0f, 4.0f, true, "Ultima Repeater")
            .ActivateOnEnter<UltimaRepeater>()
            .DeactivateOnExit<UltimaRepeater>()
            .ActivateOnEnter<FellForces>()
            .ExecOnEnter<FellForces>(static comp => comp.active = true)
            .ExecOnEnter<FellForces>(static comp => comp.expectedCasts = 6);

        ComponentCondition<FellForces>(id + 0x190, 6.0f, static comp => comp.NumCasts > 0, "1st Auto Attack Stack");
        ComponentCondition<FellForces>(id + 0x200, 3.1f, static comp => comp.NumCasts > 3, "2nd Auto Attack Stack")
            .DeactivateOnExit<FellForces>();

        ActorCast(id + 0x210u, _module.KefkaP5, AID.StrayApocalypseCast, 1.5f, 5.0f, true, "Exa-flares start")
            .ActivateOnEnter<StrayApocalypse>()
            .ActivateOnEnter<StrayEntropy>();

        ComponentCondition<StrayApocalypse>(id + 0x220u, 15.8f, static comp => comp.NumCasts >= 84, "Exa-Flares end");
        ComponentCondition<StrayEntropy>(id + 0x230u, 2.3f, static o => !o.Active, "Spreads")
            .DeactivateOnExit<StrayApocalypse>()
            .DeactivateOnExit<StrayEntropy>();

        ActorCast(id + 0x240u, _module.KefkaP5, AID.MaddeningOrchestra, 3.3f, 5.0f, true, "Maddening Orchestra")
            .ActivateOnEnter<MaddeningOrchestra>();

        ComponentCondition<MaddeningOrchestra>(id + 0x250u, 0.9f, static comp => comp.NumCasts > 0, "1st Baits Resolve")
            .ActivateOnExit<ChaoticFlareTB>()
            .ExecOnExit<ChaoticFlareTB>(comp => comp.active = true);

        ComponentCondition<MaddeningOrchestra>(id + 0x260u, 3.2f, static comp => comp.NumCasts > 5, "2nd Baits + TB Resolve")
            .DeactivateOnExit<ChaoticFlareTB>()
            .DeactivateOnExit<MaddeningOrchestra>()
            .ActivateOnExit<ChaoticHolyFlareDiffusion>();

        ComponentCondition<ChaoticHolyFlareDiffusion>(id + 0x270u, 3.5f, static comp => comp.NumCasts > 0, "Tank Baits Resolve")
            .DeactivateOnExit<ChaoticHolyFlareDiffusion>()
            .ActivateOnExit<FellForces>()
            .ExecOnExit<FellForces>(comp => comp.active = true);

        ComponentCondition<FellForces>(id + 0x280u, 4.7f, static comp => comp.NumCasts > 0, "1st Auto Attack Stack");
        ComponentCondition<FellForces>(id + 0x290u, 3.1f, static comp => comp.NumCasts > 3, "2nd Auto Attack Stack");
        ComponentCondition<FellForces>(id + 0x300u, 3.1f, static comp => comp.NumCasts > 6, "3rd Auto Attack Stack")
            .DeactivateOnExit<FellForces>()
            .ActivateOnEnter<P5ForsakenBait>()
            .ActivateOnEnter<P5ForsakenRaidWide>()
            .ActivateOnEnter<P5ForsakenGround>()
            .ActivateOnEnter<P5ForsakenStack>();

        Timeout(id + 0x500000u, 30.0f, "P5 Unknown");
    }

    // TODO update raidwides to actually use actors instead since it should now be fixed and able to find the boss correctly
    private void Phase4(uint id)
    {
        ActorTargetable(id, _module.KefkaP4, true, 2.1f, "Boss appears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        ActorCast(id + 0x05u, _module.KefkaP4, AID.KefkaSays, 5.2f, 5.0f, true, "Other bosses spawn")
            .ActivateOnEnter<GrandCrossOrder>()
            .ActivateOnEnter<TsunamiInfernoOrder>();

        ActorCastStart(id + 0x10, _module.KefkaP4, AID.MysteryMagic, 4.6f, true, "Mystery Magic")
            .ActivateOnEnter<BlizzardIIIBlowout>()
            .ActivateOnEnter<LightningSafeSpots>()
            .ExecOnEnter<BlizzardIIIBlowout>(static o => o.Risky = true)
            .ExecOnEnter<LightningSafeSpots>(static o => o.Risky = true);

        ComponentCondition<BlizzardIIIBlowout>(id + 0x20, 5.0f, static o => o.NumCasts > 0, "Blizzard + Lightning safe spots")
            .DeactivateOnExit<LightningSafeSpots>()
            .DeactivateOnExit<BlizzardIIIBlowout>();

        ComponentCondition<GrandCrossOrder>(id + 0x30u, 4.4f, static comp => comp.currentCast > 0, "Raidwide (1st Grand Cross)")
            .ActivateOnEnter<GrandCrossRaidwide>()
            .DeactivateOnExit<GrandCrossRaidwide>();
        ComponentCondition<TsunamiInfernoOrder>(id + 0x40u, 5.1f, static comp => comp.currentCast > 0, "Raidwide (1st Tsunami Inferno)")
            .ActivateOnEnter<TsunamiRaidwide>()
            .ActivateOnEnter<InfernoRaidwide>()
            .DeactivateOnExit<TsunamiRaidwide>()
            .DeactivateOnExit<InfernoRaidwide>();

        ActorCastStart(id + 0x50, _module.KefkaP4, AID.MysteryMagic, 0.5f, true, "Mystery Magic")
            .ActivateOnEnter<BlizzardIIIBlowout>()
            .ActivateOnEnter<LightningSafeSpots>()
            .ExecOnEnter<BlizzardIIIBlowout>(static o => o.Risky = true)
            .ExecOnEnter<LightningSafeSpots>(static o => o.Risky = true);

        ComponentCondition<BlizzardIIIBlowout>(id + 0x60, 5.0f, static o => o.NumCasts > 0, "Blizzard + Lightning safe spots")
            .DeactivateOnExit<LightningSafeSpots>()
            .DeactivateOnExit<BlizzardIIIBlowout>();

        ComponentCondition<GrandCrossOrder>(id + 0x70u, 4.2f, static comp => comp.currentCast > 1, "Raidwide (2nd Grand Cross)")
            .ActivateOnEnter<GrandCrossRaidwide>()
            .DeactivateOnExit<GrandCrossRaidwide>();
        ComponentCondition<TsunamiInfernoOrder>(id + 0x80u, 5.2f, static comp => comp.currentCast > 1, "Raidwide (2nd Tsunami Inferno)")
            .ActivateOnEnter<TsunamiRaidwide>()
            .ActivateOnEnter<InfernoRaidwide>()
            .DeactivateOnExit<TsunamiRaidwide>()
            .DeactivateOnExit<InfernoRaidwide>();

        ActorCastStart(id + 0x90, _module.KefkaP4, AID.MysteryMagic, 0.7f, true, "Mystery Magic")
            .ActivateOnEnter<BlizzardIIIBlowout>()
            .ActivateOnEnter<LightningSafeSpots>()
            .ExecOnEnter<BlizzardIIIBlowout>(static o => o.Risky = true)
            .ExecOnEnter<LightningSafeSpots>(static o => o.Risky = true);

        ComponentCondition<BlizzardIIIBlowout>(id + 0x100, 5.0f, static o => o.NumCasts > 0, "Blizzard + Lightning safe spots")
            .DeactivateOnExit<LightningSafeSpots>()
            .DeactivateOnExit<BlizzardIIIBlowout>();

        ComponentCondition<GrandCrossOrder>(id + 0x110u, 3.9f, static comp => comp.currentCast > 2, "Raidwide (3rd Grand Cross)")
            .ActivateOnEnter<GrandCrossRaidwide>()
            .DeactivateOnExit<GrandCrossRaidwide>()
            .ActivateOnExit<AntiLight>()
            .ActivateOnExit<EdgeOfDeath>();

        ComponentCondition<AntiLight>(id + 0x120u, 12.6f, static comp => comp.NumCasts >= 4, "Antilight + Edge of Death")
            .DeactivateOnExit<AntiLight>()
            .DeactivateOnExit<EdgeOfDeath>()
            .ActivateOnExit<ForkedWater>()
            .ActivateOnExit<AccelerationBomb>();

        Condition(id + 0x130u, 8.4f, () => Module.FindComponent<AccelerationBomb>()!.NumCasts >= 4 &&
                                          Module.FindComponent<ForkedWater>()!.NumCasts >= 4,
            "Spreads + Stacks + Acceleration Bombs Resolve")
            .DeactivateOnExit<ForkedWater>()
            .DeactivateOnExit<AccelerationBomb>()
            .ActivateOnEnter<LightningSafeSpots>()
            .ExecOnEnter<LightningSafeSpots>(static o => o.Risky = true)
            .ActivateOnExit<CursedShriek>()
            .ActivateOnExit<KefkaOrder>();

        ComponentCondition<LightningSafeSpots>(id + 0x140u, 8.1f, static comp => comp.NumCasts > 0, "Lightning Safe Spots");

        ComponentCondition<CursedShriek>(id + 0x150u, 0.8f, static comp => comp.NumCasts > 0, "Gazes Resolve")
            .DeactivateOnExit<CursedShriek>()
            .DeactivateOnExit<LightningSafeSpots>()
            .ActivateOnExit<Inferno>()
            .ActivateOnExit<InfernoBaits>();

        ActorCastStart(id + 0x160u, _module.KefkaP4, AID.UltimaUpsurge, 4.3f, true, "Ultima Upsurge")
            .ActivateOnEnter<UltimaUpsurge>()
            .ActivateOnEnter<ForkedWater>()
            .ActivateOnEnter<AccelerationBomb>()
            .ExecOnEnter<ForkedWater>(static comp => comp.active = false);

        ComponentCondition<Inferno>(id + 0x165u, 2.5f, static comp => comp.active, "Baits dropped")
            .ExecOnExit<ForkedWater>(static comp => comp.active = true);

        ComponentCondition<UltimaUpsurge>(id + 0x166u, 2.5f, static comp => comp.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<UltimaUpsurge>();

        ComponentCondition<InfernoBaits>(id + 0x170u, 2.5f, static comp => comp.NumCasts >= 8, "Inferno Baits")
            .DeactivateOnExit<Inferno>()
            .DeactivateOnExit<InfernoBaits>();

        Condition(id + 0x180u, 4.2f, () => Module.FindComponent<AccelerationBomb>()!.NumCasts >= 4 &&
                                          Module.FindComponent<ForkedWater>()!.NumCasts >= 4,
                "Spreads + Stacks + Acceleration Bombs Resolve")
            .DeactivateOnExit<ForkedWater>()
            .DeactivateOnExit<AccelerationBomb>()
            .ActivateOnEnter<BlizzardIIIBlowout>()
            .ExecOnEnter<BlizzardIIIBlowout>(static o => o.Risky = true);

        ComponentCondition<BlizzardIIIBlowout>(id + 0x190, 1.5f, static o => o.NumCasts > 0, "Blizzard Safe spots")
            .DeactivateOnExit<BlizzardIIIBlowout>()
            .ActivateOnExit<CursedShriek>();

        ComponentCondition<CursedShriek>(id + 0x200u, 7.1f, static comp => comp.NumCasts > 0, "Gazes Resolve")
            .DeactivateOnExit<CursedShriek>()
            .ActivateOnExit<Tsunami>()
            .ActivateOnExit<TsunamiBaits>();

        ComponentCondition<TsunamiBaits>(id + 0x210u, 10.7f, static comp => comp.NumCasts >= 8, "Tsunami Baits")
            .DeactivateOnExit<Tsunami>()
            .DeactivateOnExit<TsunamiBaits>()
            .ActivateOnEnter<BlizzardIIIBlowout>()
            .ActivateOnEnter<LightningSafeSpots>()
            .ExecOnEnter<BlizzardIIIBlowout>(static o => o.Risky = true)
            .ExecOnEnter<LightningSafeSpots>(static o => o.Risky = true);

        ComponentCondition<LightningSafeSpots>(id + 0x220, 0.5f, static o => o.NumCasts > 0, "Blizzard + Lightning Safe Spots")
            .DeactivateOnExit<LightningSafeSpots>()
            .DeactivateOnExit<BlizzardIIIBlowout>()
            .DeactivateOnExit<GrandCrossOrder>()
            .DeactivateOnExit<TsunamiInfernoOrder>()
            .DeactivateOnExit<KefkaOrder>();

        ActorCast(id + 0x40000u, _module.KefkaP4, AID.UltimaUpsurge, 4.2f, 5.0f, true, "Enrage")
            .ActivateOnEnter<UltimaUpsurge>()
            .DeactivateOnExit<UltimaUpsurge>();

        Timeout(id + 0x40010u, 31.0f, "Downtime");
    }

    private void Phase3(uint id)
    {
        ActorCast(id, _module.BossP3, AID.AeroIIIAssault, 2.4f, 3, false, "Knockback")
            .ActivateOnEnter<AeroIIIAssault>()
            .DeactivateOnExit<AeroIIIAssault>();

        ActorCast(id + 0x10u, _module.BossP3, AID.DefinitionOfInsanity, 33.7f, 4);
        ActorTargetable(id + 0x20u, _module.ExdeathP3, true, 3.1f, "Bosses appear")
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        ActorCast(id + 0x30u, _module.ChaosP3, AID.TheDecisiveBattle, 0.2f, 3.0f, true)
            .ActivateOnEnter<TheDecisiveBattle>()
            .ActivateOnEnter<P3Firewall>() // stays active for the whole phase, driven by statuses
            .DeactivateOnExit<TheDecisiveBattle>();

        ActorCast(id + 0x40u, _module.ChaosP3, AID.BowelsOfAgony, 14.4f, 5.0f, true, "Raidwide")
            .ActivateOnEnter<BowelsOfAgony>()
            .DeactivateOnExit<BowelsOfAgony>()
            .ActivateOnExit<Crystals>();

        var state1 = ActorCast(id + 0x50u, _module.ExdeathP3, AID.ThunderIII, 12.3f, 7.0f, true, "Thunder + element 1")
            .ActivateOnEnter<ThunderIII>()
            .DeactivateOnExit<ThunderIII>();
        ActivateCrystalElement(state1);
        var state2 = Condition(id + 0x60u, 0.8f, () => Module.FindComponent<FireCrystal>()?.NumCasts > 0 || Module.FindComponent<WaterCrystal>()?.NumCasts > 0, "1st Crystal");
        DeactivateCrystalElement(state2);

        ActorCast(id + 0x70u, _module.ExdeathP3, AID.ThunderIII, 3.4f, 5.0f, true, "Tankbuster cast")
            .ActivateOnEnter<ThunderIIITB>();
        ComponentCondition<ThunderIIITB>(id + 0x75u, 0.1f, static comp => comp.NumCasts > 0, "Tankbuster 1st hit");
        ComponentCondition<ThunderIIITB>(id + 0x80u, 3.0f, static comp => comp.NumCasts > 1, "Tankbuster 2nd hit")
            .DeactivateOnExit<ThunderIIITB>();

        var state3 = ActorCastStartMulti(id + 0x90u, _module.ChaosP3, [AID.LongitudinalImplosion, AID.LatitudinalImplosion], 4.8f, true)
            .ActivateOnEnter<LongitudinalLatitudinalImplosion>();
        ActivateCrystalElement(state3);
        ComponentCondition<LongitudinalLatitudinalImplosion>(id + 0x100u, 5.6f, static comp => comp.NumCasts > 0, "Front/sides 1st");
        ComponentCondition<LongitudinalLatitudinalImplosion>(id + 0x110u, 2.0f, static comp => comp.NumCasts > 2, "Front/sides 2nd")
            .DeactivateOnExit<LongitudinalLatitudinalImplosion>();

        var state4 = Condition(id + 0x120u, 3.1f,
                () => Module.FindComponent<WaterCrystal>()?.NumCasts > 0 ||
                      Module.FindComponent<FireCrystal>()?.NumCasts > 0, "2nd Crystal")
            .ActivateOnExit<UmbraSmash>()
            .ActivateOnEnter<UltimaBlaster>();
        DeactivateCrystalElement(state4);
        State ActivateCrystalElement(State state) => state.ExecOnEnter<Crystals>(comp =>
                                                                {
                                                                    var next = comp.nextElement;
                                                                    if (next == Crystals.Element.Fire)
                                                                    {
                                                                        Module.ActivateComponent<FireCrystal>();
                                                                    }
                                                                    else if (next == Crystals.Element.Water)
                                                                    {
                                                                        Module.ActivateComponent<WaterCrystal>();
                                                                    }
                                                                });
        State DeactivateCrystalElement(State state) => state.ExecOnExit<Crystals>(comp =>
                                                                {
                                                                    var next = comp.nextElement;
                                                                    if (next == Crystals.Element.Fire)
                                                                    {
                                                                        Module.DeactivateComponent<FireCrystal>();
                                                                    }
                                                                    if (next == Crystals.Element.Water)
                                                                    {
                                                                        Module.DeactivateComponent<WaterCrystal>();
                                                                    }
                                                                });
        ActorCastStart(id + 0x130u, _module.ChaosP3, AID.UmbraSmash, 9.3f, true, "UmbraSmash bait")
            .ActivateOnEnter<UltimaBlasterLimitCut>();

        ComponentCondition<UltimaBlaster>(id + 0x140u, 0.8f, static comp => comp.NumCasts > 0, "Raidwides start");

        ComponentCondition<UmbraSmash>(id + 0x150u, 4.1f, static comp => comp.NumCasts > 0, "UmbraSmash bait resolves")
            .ActivateOnEnter<HeadTailWind>()
            .ActivateOnEnter<Cyclone>()
            .DeactivateOnExit<UmbraSmash>();

        ComponentCondition<HeadTailWind>(id + 0x160u, 3.1f, static comp => comp.NumCasts > 0, "Knockback");
        ComponentCondition<Cyclone>(id + 0x170u, 3.9f, static comp => comp.NumCasts == 8, "Wind stacks")
            .DeactivateOnExit<HeadTailWind>()
            .DeactivateOnExit<Cyclone>()
            .DeactivateOnExit<Crystals>();

        ComponentCondition<UltimaBlasterLimitCut>(id + 0x180u, 11.0f, static comp => comp.NumCasts > 0, "Limit cut starts");
        ComponentCondition<UltimaBlasterLimitCut>(id + 0x190u, 1.6f, static comp => comp.NumCasts == 8, "Limit cut ends")
            .DeactivateOnExit<UltimaBlaster>()
            .DeactivateOnExit<UltimaBlasterLimitCut>();

        ActorCast(id + 0x200u, _module.ExdeathP3, AID.ThunderIII, 1.0f, 5.0f, true, "Tankbuster cast")
            .ActivateOnEnter<ThunderIIITB>();
        ComponentCondition<ThunderIIITB>(id + 0x205u, 0.1f, static comp => comp.NumCasts > 0, "Tankbuster 1st hit");
        ComponentCondition<ThunderIIITB>(id + 0x210u, 3.1f, static comp => comp.NumCasts > 1, "Tankbuster 2nd hit")
            .DeactivateOnExit<ThunderIIITB>();

        ActorCast(id + 0x220u, _module.ChaosP3, AID.TheDecisiveBattle, 1.9f, 3.0f, true)
            .ActivateOnEnter<TheDecisiveBattle>()
            .DeactivateOnExit<TheDecisiveBattle>()
            .ActivateOnExit<ThunderIIITB>();

        ComponentCondition<ThunderIIITB>(id + 0x235u, 9.2f, static comp => comp.NumCasts > 0, "Tankbuster 1st hit");
        ComponentCondition<ThunderIIITB>(id + 0x240u, 3.1f, static comp => comp.NumCasts > 1, "Tankbuster 2nd hit")
            .DeactivateOnExit<ThunderIIITB>()
            .ActivateOnEnter<EarthquakeRaidwide>();

        ComponentCondition<EarthquakeRaidwide>(id + 0x250u, 1.8f, static comp => comp.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<EarthquakeRaidwide>()
            .ActivateOnEnter<KefkaMax>()
            .ActivateOnExit<SlapHappy>()
            .ActivateOnExit<SlapHappyBaits>()
            .ActivateOnExit<BlackHole>()
            .ActivateOnExit<BlackHoleActors>();

        ComponentCondition<SlapHappy>(id + 0x260u, 18.8f, static comp => comp.NumCasts == 4, "SlapHappy AOEs resolve + Baits")
            .DeactivateOnExit<SlapHappy>()
            .DeactivateOnExit<SlapHappyBaits>();

        ComponentCondition<BlackHole>(id + 0x270u, 7.4f, static comp => comp.NumCasts == 1, "Tethers set 1-1");

        ActorCastStart(id + 0x280u, _module.ExdeathP3, AID.ThunderIII, 5.3f, true, "Tankbuster cast")
            .ActivateOnEnter<ThunderIIITB>();
        ComponentCondition<BlackHole>(id + 0x290u, 1.8f, static comp => comp.NumCasts > 1, "Tethers set 1-2");
        ComponentCondition<ThunderIIITB>(id + 0x295u, 3.2f, static comp => comp.NumCasts > 0, "Tankbuster 1st hit");
        ComponentCondition<ThunderIIITB>(id + 0x300u, 3.1f, static comp => comp.NumCasts > 1, "Tankbuster 2nd hit")
            .DeactivateOnExit<ThunderIIITB>()
            .ActivateOnEnter<DamningEdict>();

        ComponentCondition<DamningEdict>(id + 0x310u, 5.0f, static comp => comp.NumCasts > 0, "Frontal")
            .DeactivateOnExit<DamningEdict>()
            .ActivateOnEnter<SlapHappy>()
            .ActivateOnEnter<SlapHappyBaits>();

        ComponentCondition<SlapHappy>(id + 0x320u, 4.7f, static comp => comp.NumCasts == 4, "SlapHappy AOEs resolve + Baits")
            .DeactivateOnExit<SlapHappy>()
            .DeactivateOnExit<SlapHappyBaits>();

        ComponentCondition<BlackHole>(id + 0x340u, 7.5f, static comp => comp.NumCasts > 3, "Tethers set 2-1");
        ComponentCondition<BlackHole>(id + 0x350u, 5.0f, static comp => comp.NumCasts > 6, "Tethers set 2-2");
        ComponentCondition<BlackHole>(id + 0x360u, 5.1f, static comp => comp.NumCasts > 9, "Tethers set 2-3")
            .ActivateOnEnter<DamningEdict>();

        ComponentCondition<DamningEdict>(id + 0x370u, 4.9f, static comp => comp.NumCasts > 0, "Frontal")
            .DeactivateOnExit<DamningEdict>()
            .ActivateOnEnter<LookUponMeAndDespairAOE>();

        ComponentCondition<LookUponMeAndDespairAOE>(id + 0x380u, 1.4f, static comp => comp.NumCasts > 0, "Middle line AOE")
            .DeactivateOnExit<LookUponMeAndDespairAOE>()
            .ActivateOnEnter<ThunderIIITB>();

        ComponentCondition<ThunderIIITB>(id + 0x395u, 4.5f, static comp => comp.NumCasts > 0, "Tankbuster 1st hit");
        ComponentCondition<ThunderIIITB>(id + 0x400u, 3.1f, static comp => comp.NumCasts > 1, "Tankbuster 2nd hit")
            .DeactivateOnExit<ThunderIIITB>();

        ComponentCondition<BlackHole>(id + 0x410u, 10.1f, static comp => comp.NumCasts > 12, "Tethers set 3-1");
        ComponentCondition<BlackHole>(id + 0x420u, 5.0f, static comp => comp.NumCasts > 15, "Tethers set 3-2");
        ComponentCondition<BlackHole>(id + 0x430u, 5.1f, static comp => comp.NumCasts > 18, "Tethers set 3-3")
            .ActivateOnEnter<WhiteHole>();

        ComponentCondition<WhiteHole>(id + 0x440u, 11.0f, static comp => comp.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<WhiteHole>()
            .ActivateOnEnter<LongitudinalLatitudinalImplosion>()
            .ActivateOnEnter<SlapHappy>()
            .ActivateOnEnter<SlapHappyBaits>();

        ComponentCondition<LongitudinalLatitudinalImplosion>(id + 0x450u, 0.7f, static comp => comp.NumCasts > 0, "Front/sides 1st");
        ComponentCondition<LongitudinalLatitudinalImplosion>(id + 0x460u, 2.0f, static comp => comp.NumCasts > 2, "Front/sides 2nd")
            .DeactivateOnExit<LongitudinalLatitudinalImplosion>();

        ComponentCondition<SlapHappy>(id + 0x470u, 2.2f, static comp => comp.NumCasts == 4, "SlapHappy AOEs resolve + Baits")
            .DeactivateOnExit<SlapHappy>()
            .DeactivateOnExit<SlapHappyBaits>();

        ComponentCondition<BlackHole>(id + 0x480u, 7.3f, static comp => comp.NumCasts > 21, "Tethers set 4-1")
            .ActivateOnEnter<LookUponMeAndDespairAOE>();
        ComponentCondition<BlackHole>(id + 0x490u, 7.0f, static comp => comp.NumCasts > 23, "Tethers set 4-2 + Middle line AOE")
            .DeactivateOnExit<LookUponMeAndDespairAOE>()
            .ActivateOnExit<P3Blizzard>();

        ActorCast(id + 0x500u, _module.ExdeathP3, AID.BlizzardIIICast, 5.3f, 3.0f, true, "1st Blizzard Baits")
            .DeactivateOnEnter<KefkaMax>()
            .DeactivateOnEnter<BlackHoleActors>()
            .DeactivateOnEnter<BlackHole>()
            .ActivateOnEnter<P3BlizzardBaits>()
            .ActivateOnEnter<KnockDown>()
            .ActivateOnEnter<StompAMole>();

        ComponentCondition<P3Blizzard>(id + 0x510u, 0.1f, static comp => comp.NumCasts > 0, "1st Blizzard Baits");
        ComponentCondition<P3BlizzardBaits>(id + 0x520u, 3.0f, static comp => comp.NumCasts > 0, "1st Blizzard Baits Resolve");
        ComponentCondition<P3Blizzard>(id + 0x530u, 0.1f, static comp => comp.NumCasts > 8, "2nd Blizzard Baits");
        ComponentCondition<StompAMole>(id + 0x540u, 2.2f, static comp => comp.NumCasts > 0, "1st Tower");
        ComponentCondition<P3BlizzardBaits>(id + 0x550, 0.8f, static comp => comp.NumCasts > 8, "2nd Blizzard Baits Resolve")
            .DeactivateOnExit<P3Blizzard>()
            .DeactivateOnExit<P3BlizzardBaits>();
        ComponentCondition<StompAMole>(id + 0x570u, 0.5f, static comp => comp.NumCasts > 1, "2nd Tower");
        ComponentCondition<StompAMole>(id + 0x580u, 1.3f, static comp => comp.NumCasts > 2, "3rd Tower");
        ComponentCondition<StompAMole>(id + 0x590u, 1.3f, static comp => comp.NumCasts > 3, "4th Tower");
        ComponentCondition<KnockDown>(id + 0x600u, 1.8f, static o => !o.Active, "2nd Stack")
            .DeactivateOnExit<StompAMole>()
            .ActivateOnEnter<BigBang>()
            .ActivateOnEnter<P3BlizzardMove>();
        ComponentCondition<P3BlizzardMove>(id + 0x610u, 3.5f, static comp => comp.NumCasts > 0, "Blizzard Raidwide")
            .DeactivateOnExit<P3BlizzardMove>();
        ComponentCondition<BigBang>(id + 0x620u, 1.2f, static comp => comp.NumCasts > 1, "Stack AOEs resolve")
            .DeactivateOnExit<BigBang>()
            .DeactivateOnExit<KnockDown>()
            .ActivateOnEnter<P3Enrage>();
        ComponentCondition<P3Enrage>(id + 0x640u, 17.6f, static comp => !comp.enrage, "Enrage");
    }

    private void Phase2(uint id)
    {
        ActorTargetable(id, _module.BossP2, true, 10.3f, "Boss appears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ActorCast(id + 0x10u, _module.BossP2, AID.UltimateEmbrace, 7.2f, 5.0f, true, "Tankbuster")
            .ActivateOnEnter<UltimateEmbrace>()
            .DeactivateOnExit<UltimateEmbrace>();
        ActorCast(id + 0x20u, _module.BossP2, AID.Forsaken, 8.2f, 7.0f, true, "Raidwide")
            .ActivateOnEnter<Forsaken>()
            .DeactivateOnExit<Forsaken>()
            .ActivateOnEnter<ForsakenShapes>()
            .ActivateOnEnter<PathOfLight>()
            .ActivateOnEnter<ForsakenBaitsSpreadStacks>()
            .ActivateOnExit<ForsakenBaitsBossClones>()
            .ActivateOnEnter<ForsakenBaitsCone>()
            .ActivateOnEnter<ForsakenSolverSet1>();

        // Tower set 1
        ComponentCondition<ForsakenShapes>(id + 0x30u, 13.2f, static comp => comp.currentTowerSet > 1, "1st Tower Set")
            .DeactivateOnExit<ForsakenSolverSet1>()
            .ActivateOnExit<ForsakenSolverSet2>();

        // Tower set 2
        ComponentCondition<ForsakenShapes>(id + 0x40u, 10.0f, static comp => comp.currentTowerSet > 2, "2nd Tower Set")
            .DeactivateOnExit<ForsakenSolverSet2>()
            .ActivateOnExit<ForsakenSolverSet1>()
            .ExecOnExit<ForsakenSolverSet1>(static comp => comp.colourCircle = Colors.AOE)
            .ActivateOnEnter<AllThingsEnding>()
            .ActivateOnEnter<AllThingsEndingCasts>()
            .ExecOnExit<AllThingsEnding>(static comp => comp.aoesLocked = false);

        // Clones baits
        ComponentCondition<AllThingsEnding>(id + 0x50u, 5.7f, static comp => comp.aoesLocked, "Boss/Clones baits")
            .ExecOnExit<ForsakenSolverSet1>(static comp => comp.colourCircle = Colors.Safe);

        // Tower set 3
        Condition(id + 0x60u, 5.4f, () => Module.FindComponent<AllThingsEndingCasts>()!.NumCasts >= 4 &&
                                         Module.FindComponent<ForsakenShapes>()!.currentTowerSet > 3, "3rd Tower Set + baits")
            .DeactivateOnExit<AllThingsEnding>()
            .DeactivateOnExit<AllThingsEndingCasts>()
            .DeactivateOnExit<ForsakenSolverSet1>()
            .ActivateOnExit<ForsakenSolverSet2>();

        // Tower set 4
        ComponentCondition<ForsakenShapes>(id + 0x70u, 10.0f, static comp => comp.currentTowerSet > 4, "4th Tower Set")
            .DeactivateOnExit<ForsakenSolverSet2>()
            .ActivateOnExit<ForsakenSolverSet1>()
            .ExecOnExit<ForsakenSolverSet1>(static comp => comp.colourCircle = Colors.AOE)
            .ActivateOnEnter<AllThingsEnding>()
            .ActivateOnEnter<AllThingsEndingCasts>()
            .ExecOnExit<AllThingsEnding>(static comp => comp.aoesLocked = false);

        // Clones baits
        ComponentCondition<AllThingsEnding>(id + 0x80u, 5.7f, comp => comp.aoesLocked, "Boss/Clones baits")
            .ExecOnExit<ForsakenSolverSet1>(static comp => comp.colourCircle = Colors.Safe);

        // Tower set 5
        Condition(id + 0x90u, 5.4f, () => Module.FindComponent<AllThingsEndingCasts>()!.NumCasts >= 4 &&
                                         Module.FindComponent<ForsakenShapes>()!.currentTowerSet > 5, "5th Tower Set + baits")
            .DeactivateOnExit<AllThingsEnding>()
            .DeactivateOnExit<AllThingsEndingCasts>()
            .DeactivateOnExit<ForsakenSolverSet1>()
            .ActivateOnExit<ForsakenSolverSet2>();

        // Tower set 6
        ComponentCondition<ForsakenShapes>(id + 0x100u, 10.0f, comp => comp.currentTowerSet > 6, "6th Tower Set")
            .DeactivateOnExit<ForsakenSolverSet2>()
            .ActivateOnExit<ForsakenSolverSet1>()
            .ExecOnExit<ForsakenSolverSet1>(static comp => comp.colourCircle = Colors.AOE)
            .ActivateOnEnter<AllThingsEnding>()
            .ActivateOnEnter<AllThingsEndingCasts>()
            .ExecOnExit<AllThingsEnding>(static comp => comp.aoesLocked = false);

        // Clones baits
        ComponentCondition<AllThingsEnding>(id + 0x110u, 5.7f, comp => comp.aoesLocked, "Boss/Clones baits")
            .ExecOnExit<ForsakenSolverSet1>(static comp => comp.colourCircle = Colors.Safe);

        // Tower set 7
        Condition(id + 0x120u, 5.4f, () => Module.FindComponent<AllThingsEndingCasts>()!.NumCasts >= 4 &&
                                         Module.FindComponent<ForsakenShapes>()!.currentTowerSet > 7, "7th Tower Set + baits")
            .DeactivateOnExit<AllThingsEnding>()
            .DeactivateOnExit<AllThingsEndingCasts>()
            .DeactivateOnExit<ForsakenSolverSet1>()
            .ActivateOnExit<ForsakenSolverSet2>();

        // Tower set 8
        ComponentCondition<ForsakenShapes>(id + 0x130u, 10.0f, comp => comp.currentTowerSet > 8, "8th Tower Set")
            .DeactivateOnExit<ForsakenSolverSet2>()
            .ActivateOnEnter<AllThingsEnding>()
            .ActivateOnEnter<AllThingsEndingCasts>()
            .ExecOnEnter<AllThingsEnding>(static comp => comp.aoesLocked = false);

        // Clones baits
        ComponentCondition<AllThingsEnding>(id + 0x140u, 5.4f, comp => comp.aoesLocked, "Boss/Clones baits");

        ComponentCondition<AllThingsEndingCasts>(id + 0x145u, 5.2f, comp => comp.NumCasts >= 4, "Boss/Clones baits Resolve")
            .DeactivateOnExit<ForsakenShapes>()
            .DeactivateOnExit<ForsakenBaitsSpreadStacks>()
            .DeactivateOnExit<ForsakenBaitsCone>()
            .DeactivateOnExit<ForsakenBaitsBossClones>()
            .DeactivateOnExit<AllThingsEnding>()
            .DeactivateOnExit<AllThingsEndingCasts>();

        ActorCast(id + 0x150u, _module.BossP2, AID.LightOfJudgmentP2, 4.1f, 5.0f, true, "Raidwide")
            .ActivateOnEnter<LightOfJudgmentP2>()
            .DeactivateOnExit<LightOfJudgmentP2>();

        ActorCast(id + 0x160u, _module.BossP2, AID.Trine, 8.2f, 3.0f, true, "Trine")
            .ActivateOnEnter<Trine>();

        ActorCastMulti(id + 0x170u, _module.BossP2, [AID.WingsOfDestructionLeft, AID.WingsOfDestructionRight], 3.1f, 4.0f, true, "Left / Right")
            .ActivateOnEnter<WingsOfDestructionLeftRight>()
            .DeactivateOnExit<WingsOfDestructionLeftRight>();
        ComponentCondition<Trine>(id + 0x180u, 5.7f, static comp => comp.NumCasts == 10, "Trine 1 Explosions");
        ActorCastStart(id + 0x190u, _module.BossP2, AID.WingsOfDestructionTB, 0.6f, true)
            .ActivateOnEnter<WingsOfDestructionTB>();
        ComponentCondition<Trine>(id + 0x200u, 1.5f, static comp => comp.NumCasts == 13, "Trine 2 Explosions");
        ComponentCondition<Trine>(id + 0x210u, 2.0f, static comp => comp.NumCasts == 22, "Trine 3 Explosions")
            .DeactivateOnExit<Trine>();
        ComponentCondition<WingsOfDestructionTB>(id + 0x220u, 0.6f, comp => comp.NumCasts > 0, "Tankbuster")
            .DeactivateOnExit<WingsOfDestructionTB>();
        ActorCast(id + 0x220u, _module.BossP2, AID.UltimateEmbrace, 2.1f, 5.0f, true, "Tankbuster")
            .ActivateOnEnter<UltimateEmbrace>()
            .DeactivateOnExit<UltimateEmbrace>();

        ActorTargetable(id + 0x30000u, _module.BossP2, false, 4.1f, "Boss disappears")
            .SetHint(StateMachine.StateHint.DowntimeStart);
    }
}
