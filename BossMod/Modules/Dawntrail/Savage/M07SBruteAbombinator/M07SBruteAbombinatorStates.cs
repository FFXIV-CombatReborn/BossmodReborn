namespace BossMod.Dawntrail.Savage.M07SBruteAbombinator;

sealed class M07SBruteAbombinatorStates : StateMachineBuilder
{
    private static readonly M07SBruteAbombinatorConfig _config = Service.Config.Get<M07SBruteAbombinatorConfig>();

    public M07SBruteAbombinatorStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        BrutalImpact(id, 5.2f, 5.6f, 6);
        StoneRingerBrutalSmash(id + 0x10000u, 5.1f);
        AddPhase1(id + 0x20000u, 6.2f);
        ExplosionsStoneringer(id + 0x30000u, 12f);
        PulpSmash(id + 0x40000u, 7.7f);
        NeoBombarianSpecial(id + 0x50000u);
        StoneRingerGlowerPower(id + 0x60000u, 20.2f);
        ThornyDeathmatch(id + 0x70000u, 8.2f);
        DemolitionDeathmatch(id + 0x80000u, 6.2f);
        BrutalImpact(id + 0x90000u, 24.5f, 6.7f, 7);
        StoneRinger2GlowerPower(id + 0xA0000u, 5.2f);
        Slaminator(id + 0xB0000u, 5.9f);
        BrutalImpact(id + 0xC0000u, 9.4f, 7.7f, 8);
        StoneRingerBrutalSmash(id + 0xD0000u, 3.1f);
        DebrisDeathmatch(id + 0xE0000u, 8.2f);
        PulpSmash(id + 0xF0000u, 3.4f);
        BrutalImpact(id + 0x100000u, 1.3f, 7.7f, 8);
        StoneRinger2Tendrils(id + 0x110000u, 8.2f);
        Slaminator(id + 0x120000u, 6.7f);
        StoneRingerBrutalSmash(id + 0x130000u, 4.2f);
        BrutalImpact(id + 0x140000u, 4.1f, 7.7f, 8);
        Targetable(id + 0x150000u, false, 19.7f, "Enrage");
    }

    private void BrutalImpact(uint id, float delay1, float delay2, int numcasts)
    {
        Cast(id, AID.BrutalImpactVisual, delay1, 5f, $"Raidwide x{numcasts}")
            .ActivateOnEnter<BrutalImpact>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<BrutalImpact>(id + 0x10u, delay2, comp => comp.NumCasts == numcasts, $"Raidwide hit {numcasts}")
            .DeactivateOnExit<BrutalImpact>();
    }

    private void Slaminator(uint id, float delay)
    {
        ComponentCondition<Slaminator>(id, delay, static comp => comp.NumCasts != 0, $"Tower resolves")
            .ActivateOnEnter<Slaminator>()
            .DeactivateOnExit<Slaminator>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void StoneRingerBrutalSmash(uint id, float delay)
    {
        CastMulti(id, [AID.Stoneringer1, AID.Stoneringer3, AID.Stoneringer4], delay, 2f, "Select AOE shape");
        CastMulti(id + 0x10u, [AID.SmashHere, AID.SmashThere], 5.9f, 3f, "Proximity tankbuster appears")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .ActivateOnEnter<BrutalSwing>()
            .ActivateOnEnter<BrutalSmash>();
        ComponentCondition<BrutalSwing>(id + 0x20u, 1f, static comp => comp.NumCasts != 0, "Stoneringer resolves")
            .DeactivateOnExit<BrutalSwing>();
        ComponentCondition<BrutalSmash>(id + 0x30u, 1.2f, static comp => comp.NumCasts != 0, "Tankbuster resolves")
            .DeactivateOnExit<BrutalSmash>();
    }

    private void AddPhase1(uint id, float delay)
    {
        Cast(id, AID.SporeSacVisual, delay, 3f, $"Add phase 1");
        ComponentCondition<SporeSac>(id + 0x10u, 5.1f, static comp => comp.NumCasts != 0, $"Circle AOEs 1")
            .ActivateOnEnter<SporeSac>()
            .ActivateOnExit<Pollen>()
            .DeactivateOnExit<SporeSac>();
        var pollen = ComponentCondition<Pollen>(id + 0x20u, 5.6f, static comp => comp.NumCasts != 0, $"Circle AOEs 2")
            .ActivateOnExit<SinisterSeedsAOE>()
            .ActivateOnExit<SinisterSeedsSpread>()
            .DeactivateOnExit<Pollen>();
        if (_config.EnableSeedPrediction)
        {
            pollen.ActivateOnExit<TendrilsOfTerrorPrediction>()
            .ActivateOnExit<TendrilsOfTerrorBait>();
        }
        ComponentCondition<SinisterSeedsAOE>(id + 0x30u, 2.8f, static comp => comp.NumCasts != 0, $"Circle AOEs 3");
        ComponentCondition<SinisterSeedsAOE>(id + 0x40u, 2f, static comp => comp.NumCasts > 4, $"Circle AOEs 4");
        var sinister = ComponentCondition<SinisterSeedsSpread>(id + 0x50u, 2f, static comp => comp.NumFinishedSpreads != 0, "Spreads resolve + circle AOEs 5")
            .DeactivateOnExit<SinisterSeedsSpread>()
            .ActivateOnEnter<Impact>();
        if (_config.EnableSeedPrediction)
        {
            sinister.DeactivateOnExit<TendrilsOfTerrorBait>();
        }
        var tendrils = ComponentCondition<TendrilsOfTerror>(id + 0x60u, 1.7f, static comp => comp.AOEs.Count != 0, $"Tendrils appear")
            .ActivateOnEnter<TendrilsOfTerror>();
        if (_config.EnableSeedPrediction)
        {
            tendrils.DeactivateOnExit<TendrilsOfTerrorPrediction>();
        }
        ComponentCondition<SinisterSeedsAOE>(id + 0x70u, 0.4f, static comp => comp.NumCasts > 12, $"Circle AOEs 6")
            .DeactivateOnExit<SinisterSeedsAOE>();
        ComponentCondition<Impact>(id + 0x80u, 2.5f, static comp => comp.NumCasts != 0, $"Stacks resolve")
            .DeactivateOnExit<Impact>();
        ComponentCondition<TendrilsOfTerror>(id + 0x90u, 0.1f, static comp => comp.AOEs.Count == 0, $"Tendrils resolve")
            .DeactivateOnExit<TendrilsOfTerror>();
        ComponentCondition<RootsOfEvil>(id + 0xA0u, 5.4f, static comp => comp.NumCasts != 0, $"Circle AOEs 7")
            .ActivateOnEnter<RootsOfEvil>()
            .ActivateOnExit<CrossingCrosswinds>()
            .ActivateOnExit<AddInterruptHint>()
            .ActivateOnExit<HurricaneForce>()
            .ActivateOnExit<WindingWildwinds>()
            .DeactivateOnExit<RootsOfEvil>();
        ComponentCondition<CrossingCrosswinds>(id + 0xB0u, 0.7f, static comp => comp.Casters.Count != 0, $"Interruptible add casts appear");
        ComponentCondition<QuarrySwamp>(id + 0xC0u, 21.9f, static comp => comp.NumCasts != 0, $"Line of sight AOE")
            .ActivateOnEnter<QuarrySwamp>()
            .DeactivateOnExit<CrossingCrosswinds>()
            .DeactivateOnExit<AddInterruptHint>()
            .DeactivateOnExit<WindingWildwinds>()
            .DeactivateOnExit<QuarrySwamp>();
    }

    private void ExplosionsStoneringer(uint id, float delay)
    {
        ComponentCondition<Explosion>(id, delay, static comp => comp.NumCasts != 0, "Proximity AOE 1")
            .SetHint(StateMachine.StateHint.Raidwide)
            .ActivateOnEnter<Explosion>();
        CastStartMulti(id + 0x10u, [AID.Stoneringer1, AID.Stoneringer3, AID.Stoneringer4], 1.2f, "Select AOE shape")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<Explosion>(id + 0x20u, 1.4f, static comp => comp.NumCasts == 2, "Proximity AOE 2")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Explosion>(id + 0x30u, 2.5f, static comp => comp.NumCasts == 3, "Proximity AOE 3")
            .DeactivateOnExit<Explosion>()
            .SetHint(StateMachine.StateHint.Raidwide);
        CastMulti(id + 0x40u, [AID.SmashHere, AID.SmashThere], 4f, 3f, "Proximity tankbuster appears")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .ActivateOnEnter<BrutalSwing>()
            .ActivateOnEnter<BrutalSmash>();
        ComponentCondition<BrutalSwing>(id + 0x50u, 1f, static comp => comp.NumCasts != 0, "Stoneringer resolves")
            .DeactivateOnExit<BrutalSwing>();
        ComponentCondition<BrutalSmash>(id + 0x60u, 1.2f, static comp => comp.NumCasts != 0, "Tankbuster resolves")
            .ActivateOnExit<PulpSmash>()
            .DeactivateOnExit<BrutalSmash>();
    }

    private void PulpSmash(uint id, float delay)
    {
        ComponentCondition<PulpSmash>(id, delay, static comp => comp.NumFinishedStacks != 0, "Stack resolves")
            .SetHint(StateMachine.StateHint.Raidwide)
            .ActivateOnEnter<TheUnpotted>()
            .ActivateOnEnter<ItCameFromTheDirt>()
            .DeactivateOnExit<PulpSmash>();
        ComponentCondition<TheUnpotted>(id + 0x10u, 2f, static comp => comp.NumCasts != 0, "Baited cones + circle AOE")
            .DeactivateOnExit<ItCameFromTheDirt>()
            .DeactivateOnExit<TheUnpotted>();
    }

    private void NeoBombarianSpecial(uint id)
    {
        Cast(id, AID.NeoBombarianSpecial, 10.8f, 8f, "Raidwide")
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<NeoBombarianSpecialKB>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void StoneRingerGlowerPower(uint id, float delay)
    {
        CastMulti(id, [AID.Stoneringer2, AID.Stoneringer4], delay, 2f, "Select AOE shape")
            .DeactivateOnExit<NeoBombarianSpecialKB>()
            .DeactivateOnEnter<ArenaChanges>()
            .ActivateOnExit<BrutalSwing>();
        ComponentCondition<BrutalSwing>(id + 0x10u, 14f, static comp => comp.NumCasts != 0, "Stoneringer resolves")
            .ActivateOnEnter<ElectrogeneticForce>()
            .ActivateOnEnter<GlowerPower>()
            .DeactivateOnExit<BrutalSwing>();
        ComponentCondition<ElectrogeneticForce>(id + 0x20u, 4.7f, static comp => comp.NumCasts != 0, "Spreads resolve")
            .DeactivateOnExit<ElectrogeneticForce>();
        ComponentCondition<GlowerPower>(id + 0x30u, 0.1f, static comp => comp.AOE.Length == 0, "Line AOE resolves")
            .DeactivateOnExit<GlowerPower>();
        Cast(id + 0x40, AID.RevengeOfTheVines1, 0.9f, 5f, "Raidwide")
            .ActivateOnEnter<RevengeOfTheVines1>()
            .DeactivateOnExit<RevengeOfTheVines1>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void ThornyDeathmatch(uint id, float delay)
    {
        Cast(id, AID.ThornyDeathmatch, delay, 3f, "Thorny Deathmatch")
            .ActivateOnExit<ThornsOfDeath>();
        CastMulti(id + 0x10u, [AID.Stoneringer2, AID.Stoneringer4], 2.1f, 2f, "Select AOE shape")
            .ActivateOnExit<AbominableBlink>();
        ComponentCondition<AbominableBlink>(id + 0x20u, 12.2f, static comp => comp.NumCasts != 0, "Flare resolves")
            .SetHint(StateMachine.StateHint.Raidwide)
            .ActivateOnExit<Sporesplosion>()
            .ActivateOnExit<BrutalSwing>()
            .DeactivateOnExit<AbominableBlink>();
        for (var i = 1; i <= 3; ++i)
        {
            var offset = id + 0x30u + (uint)((i - 1) * 0x10u);
            var time = i == 1 ? 15.3f : 2f;
            var desc = $"Circle AOEs {i}";
            var casts = i * 6;
            var cond = ComponentCondition<Sporesplosion>(offset, time, comp => comp.NumCasts == casts, desc);
            if (i == 3)
                cond.DeactivateOnExit<Sporesplosion>();
        }
        ComponentCondition<BrutalSwing>(id + 0x60u, 5.1f, static comp => comp.NumCasts != 0, "Stoneringer resolves")
            .ActivateOnEnter<ElectrogeneticForce>()
            .ActivateOnEnter<GlowerPower>()
            .DeactivateOnExit<BrutalSwing>();
        ComponentCondition<ElectrogeneticForce>(id + 0x70u, 4.8f, static comp => comp.NumCasts != 0, "Spreads resolve")
            .DeactivateOnExit<ElectrogeneticForce>();
        ComponentCondition<GlowerPower>(id + 0x80u, 0.2f, static comp => comp.AOE.Length == 0, "Line AOE resolves")
            .DeactivateOnExit<GlowerPower>();
        Cast(id + 0x90, AID.RevengeOfTheVines1, 0.8f, 5f, "Raidwide")
            .ActivateOnEnter<RevengeOfTheVines1>()
            .DeactivateOnExit<RevengeOfTheVines1>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DemolitionDeathmatch(uint id, float delay)
    {
        Cast(id, AID.DemolitionDeathmatch, delay, 3f, "Demolition Deathmatch")
            .ActivateOnExit<AbominableBlink>();
        ComponentCondition<AbominableBlink>(id + 0x10u, 14.5f, static comp => comp.NumCasts != 0, "Flare resolves")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<AbominableBlink>();
        var seeds = Cast(id + 0x20u, AID.StrangeSeedsVisual1, 3.1f, 4f)
            .ActivateOnEnter<StrangeSeeds>();
        if (_config.EnableSeedPrediction)
        {
            seeds.ActivateOnExit<TendrilsOfTerrorPrediction>()
            .ActivateOnEnter<TendrilsOfTerrorBait>();
        }
        CastStartMulti(id + 0x30u, [AID.Stoneringer2, AID.Stoneringer4], 5.2f, "Select AOE shape");
        ComponentCondition<StrangeSeeds>(id + 0x40u, 1f, static comp => comp.NumFinishedSpreads != 0, "Spreads 1 resolve");
        ComponentCondition<TendrilsOfTerror>(id + 0x50u, 1.6f, static comp => comp.AOEs.Count != 0, $"Tendrils 1 appear")
            .ActivateOnEnter<TendrilsOfTerror>();
        ComponentCondition<TendrilsOfTerror>(id + 0x60u, 3f, static comp => comp.AOEs.Count == 0, $"Tendrils 1 resolve");
        ComponentCondition<StrangeSeeds>(id + 0x70u, 0.5f, static comp => comp.NumFinishedSpreads > 2, "Spreads 2 resolve");
        ComponentCondition<TendrilsOfTerror>(id + 0x80u, 1.6f, static comp => comp.AOEs.Count != 0, $"Tendrils 2 appear");
        ComponentCondition<TendrilsOfTerror>(id + 0x90u, 3f, static comp => comp.AOEs.Count == 0, $"Tendrils 2 resolve");
        ComponentCondition<StrangeSeeds>(id + 0xA0u, 0.3f, static comp => comp.NumFinishedSpreads > 4, "Spreads 3 resolve");
        ComponentCondition<TendrilsOfTerror>(id + 0xB0u, 1.7f, static comp => comp.AOEs.Count != 0, $"Tendrils 3 appear");
        ComponentCondition<TendrilsOfTerror>(id + 0xC0u, 3f, static comp => comp.AOEs.Count == 0, $"Tendrils 3 resolve");
        ComponentCondition<StrangeSeeds>(id + 0xD0u, 0.3f, static comp => comp.NumFinishedSpreads > 6, "Spreads 4 resolve");
        ComponentCondition<TendrilsOfTerror>(id + 0xE0u, 1.7f, static comp => comp.AOEs.Count != 0, $"Tendrils 4 appear")
            .DeactivateOnExit<StrangeSeeds>();
        ComponentCondition<TendrilsOfTerror>(id + 0xF0u, 3f, static comp => comp.AOEs.Count == 0, $"Tendrils 4 resolve");
        var killer = ComponentCondition<KillerSeeds>(id + 0x100u, 5.3f, static comp => comp.NumFinishedStacks != 0, $"Stacks resolve")
            .ActivateOnEnter<KillerSeeds>()
            .DeactivateOnExit<KillerSeeds>();
        if (_config.EnableSeedPrediction)
        {
            killer.DeactivateOnExit<TendrilsOfTerrorBait>();
        }
        var tendrils = ComponentCondition<TendrilsOfTerror>(id + 0x110u, 1.6f, static comp => comp.AOEs.Count != 0, $"Tendrils 5 appear")
            .ActivateOnExit<BrutalSwing>();
        if (_config.EnableSeedPrediction)
        {
            tendrils.DeactivateOnExit<TendrilsOfTerrorPrediction>();
        }
        ComponentCondition<TendrilsOfTerror>(id + 0x120u, 3f, static comp => comp.AOEs.Count == 0, $"Tendrils 5 resolve")
            .DeactivateOnExit<TendrilsOfTerror>();
        ComponentCondition<BrutalSwing>(id + 0x130u, 5.6f, static comp => comp.NumCasts != 0, "Stoneringer resolves")
            .ActivateOnEnter<ElectrogeneticForce>()
            .ActivateOnEnter<GlowerPower>()
            .DeactivateOnExit<ThornsOfDeath>()
            .DeactivateOnExit<BrutalSwing>();
        ComponentCondition<ElectrogeneticForce>(id + 0x140u, 4.8f, static comp => comp.NumCasts != 0, "Spreads resolve")
            .DeactivateOnExit<ElectrogeneticForce>();
        ComponentCondition<GlowerPower>(id + 0x150u, 0.2f, static comp => comp.AOE.Length == 0, "Line AOE resolves")
            .DeactivateOnExit<GlowerPower>();
        Cast(id + 0x160, AID.RevengeOfTheVines1, 0.9f, 5f, "Raidwide")
            .ActivateOnEnter<RevengeOfTheVines1>()
            .DeactivateOnExit<RevengeOfTheVines1>()
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x170, AID.Powerslam, 6.2f, 6f, "Raidwide")
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<Powerslam>()
            .DeactivateOnExit<Powerslam>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void StoneRinger2GlowerPower(uint id, float delay)
    {
        CastMulti(id, [AID.Stoneringer2Stoneringers1, AID.Stoneringer2Stoneringers2], delay, 2f, "Select AOE shapes")
            .DeactivateOnEnter<ArenaChanges>()
            .ActivateOnExit<LashingLariat>()
            .ActivateOnExit<BrutalSwing>();
        ComponentCondition<BrutalSwing>(id + 0x10u, 12.6f, static comp => comp.NumCasts != 0, "Stoneringer 1 resolves")
            .ActivateOnExit<RevengeOfTheVines2>();
        ComponentCondition<RevengeOfTheVines2>(id + 0x20u, 1f, static comp => comp.NumCasts != 0, "Raidwide")
            .DeactivateOnExit<RevengeOfTheVines2>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<LashingLariat>(id + 0x30u, 6.1f, static comp => comp.NumCasts != 0, "Huge cleave resolves")
            .ActivateOnExit<GlowerPower>()
            .ActivateOnExit<ElectrogeneticForce>()
            .DeactivateOnExit<LashingLariat>();
        ComponentCondition<BrutalSwing>(id + 0x40u, 8.4f, static comp => comp.NumCasts == 2, "Stoneringer 2 resolves")
            .DeactivateOnExit<BrutalSwing>();
        ComponentCondition<ElectrogeneticForce>(id + 0x50u, 2.9f, static comp => comp.NumCasts != 0, "Spreads resolve")
            .DeactivateOnExit<ElectrogeneticForce>();
        ComponentCondition<GlowerPower>(id + 0x60u, 0.1f, static comp => comp.AOE.Length == 0, "Line AOE resolves")
            .DeactivateOnExit<GlowerPower>();
    }

    private void DebrisDeathmatch(uint id, float delay)
    {
        Cast(id, AID.DebrisDeathmatch, delay, 3f, "Demolition Deathmatch")
            .ActivateOnExit<ThornsOfDeath>();
        ComponentCondition<SporeSac>(id + 0x10u, 10.3f, static comp => comp.NumCasts != 0, "Circle AOEs 1")
            .ActivateOnEnter<SporeSac>()
            .DeactivateOnExit<SporeSac>();
        var pollen = ComponentCondition<Pollen>(id + 0x20u, 5.6f, static comp => comp.NumCasts != 0, "Circle AOEs 2")
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<KillerSeeds>()
            .DeactivateOnExit<Pollen>();
        if (_config.EnableSeedPrediction)
        {
            pollen.ActivateOnExit<TendrilsOfTerrorPrediction>()
            .ActivateOnEnter<TendrilsOfTerrorBait>();
        }
        ComponentCondition<KillerSeeds>(id + 0x30u, 3.5f, static comp => comp.NumFinishedStacks != 0, $"Stacks resolve")
            .DeactivateOnExit<KillerSeeds>();
        ComponentCondition<TendrilsOfTerror>(id + 0x40u, 1.7f, static comp => comp.AOEs.Count != 0, $"Tendrils 1 appear")
            .ActivateOnEnter<TendrilsOfTerror>();
        ComponentCondition<TendrilsOfTerror>(id + 0x50u, 3f, static comp => comp.AOEs.Count == 0, $"Tendrils 1 resolve");
        ComponentCondition<QuarrySwamp>(id + 0x60u, 12.3f, static comp => comp.NumCasts != 0, $"Line of sight AOE")
            .ActivateOnEnter<QuarrySwamp>()
            .DeactivateOnExit<QuarrySwamp>();
        ComponentCondition<SinisterSeedsAOE>(id + 0x70u, 15.4f, static comp => comp.NumCasts != 0, "Circle AOEs 3")
            .ActivateOnEnter<SinisterSeedsSpread>()
            .ActivateOnEnter<SinisterSeedsAOE>();
        ComponentCondition<SinisterSeedsAOE>(id + 0x80u, 2f, static comp => comp.NumCasts > 4, "Circle AOEs 4");
        var seeds = ComponentCondition<SinisterSeedsSpread>(id + 0x90u, 2f, static comp => comp.NumFinishedSpreads != 0, "Spreads resolve + circle AOEs 5")
            .DeactivateOnExit<SinisterSeedsSpread>();
        if (_config.EnableSeedPrediction)
        {
            seeds.DeactivateOnExit<TendrilsOfTerrorBait>();
        }
        ComponentCondition<SinisterSeedsAOE>(id + 0xA0u, 2f, static comp => comp.NumCasts > 12, "Circle AOEs 5")
            .DeactivateOnExit<SinisterSeedsAOE>();
        var tendrils = ComponentCondition<TendrilsOfTerror>(id + 0xB0u, 1.6f, static comp => comp.AOEs.Count != 0, $"Tendrils 2 appear")
            .DeactivateOnEnter<ThornsOfDeath>()
            .ActivateOnEnter<PulpSmash>()
            .ActivateOnEnter<RootsOfEvil>();
        if (_config.EnableSeedPrediction)
        {
            tendrils.DeactivateOnExit<TendrilsOfTerrorPrediction>();
        }
        ComponentCondition<TendrilsOfTerror>(id + 0xC0u, 3f, static comp => comp.AOEs.Count == 0, $"Tendrils 2 resolve")
            .DeactivateOnExit<TendrilsOfTerror>();
        ComponentCondition<RootsOfEvil>(id + 0xD0u, 0.2f, static comp => comp.NumCasts != 0, "Circle AOEs 6")
            .DeactivateOnExit<RootsOfEvil>();
    }

    private void StoneRinger2Tendrils(uint id, float delay)
    {
        var stoneringer = CastMulti(id, [AID.Stoneringer2Stoneringers1, AID.Stoneringer2Stoneringers2], delay, 2f, "Select AOE shapes")
            .ActivateOnEnter<StrangeSeeds>()
            .ActivateOnExit<LashingLariat>()
            .ActivateOnExit<BrutalSwing>();
        if (_config.EnableSeedPrediction)
        {
            stoneringer.ActivateOnExit<TendrilsOfTerrorPrediction>()
            .ActivateOnEnter<TendrilsOfTerrorBait>();
        }
        ComponentCondition<StrangeSeeds>(id + 0x10u, 18.1f, static comp => comp.NumFinishedSpreads != 0, "Spreads 1 resolve");
        ComponentCondition<TendrilsOfTerror>(id + 0x20u, 2.1f, static comp => comp.AOEs.Count != 0, $"Tendrils 1 appear")
            .ActivateOnEnter<TendrilsOfTerror>();
        ComponentCondition<BrutalSwing>(id + 0x30u, 2.8f, static comp => comp.NumCasts != 0, "Stoneringer 1 resolves");
        ComponentCondition<TendrilsOfTerror>(id + 0x40u, 0.2f, static comp => comp.AOEs.Count == 0, $"Tendrils 1 resolve");
        ComponentCondition<LashingLariat>(id + 0x50u, 4.9f, static comp => comp.NumCasts != 0, $"Huge cleave")
            .DeactivateOnExit<LashingLariat>();
        var seeds = ComponentCondition<StrangeSeeds>(id + 0x60u, 4.1f, static comp => comp.NumFinishedSpreads > 4, "Spreads 2 resolve")
            .DeactivateOnExit<StrangeSeeds>();
        if (_config.EnableSeedPrediction)
        {
            seeds.DeactivateOnExit<TendrilsOfTerrorBait>();
        }
        var tendrils = ComponentCondition<TendrilsOfTerror>(id + 0x70u, 2.2f, static comp => comp.AOEs.Count != 0, $"Tendrils 1 appear");
        if (_config.EnableSeedPrediction)
        {
            tendrils.DeactivateOnExit<TendrilsOfTerrorPrediction>();
        }
        ComponentCondition<TendrilsOfTerror>(id + 0x80u, 3f, static comp => comp.AOEs.Count == 0, $"Stoneringer 2 + Tendrils 2 resolve")
            .DeactivateOnExit<BrutalSwing>()
            .DeactivateOnExit<TendrilsOfTerror>();
    }
}
