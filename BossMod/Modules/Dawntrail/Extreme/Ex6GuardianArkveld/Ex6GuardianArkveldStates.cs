namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

sealed class Ex6GuardianArkveldStates : StateMachineBuilder
{
    public Ex6GuardianArkveldStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Roar(id, 5.1f);
        WyvernsRadianceCleave(id + 0x10000u, 11f);
        GuardianWyvernsSiegeflight(id + 0x20000u, 11.4f);
        WyvernsRadianceExaflareGuardianWyvernsSiegeflight(id + 0x30000u, 1f);
        WyvernsRadianceCleave(id + 0x40000u, 8.9f);
        Roar(id + 0x50000u, 6.5f);
        WyvernsRadianceCharges(id + 0x60000u, 12.2f);
        SteeltailThrustChainbladeCharge1(id + 0x70000u, 7.6f);
        WyvernsRadianceCleave(id + 0x80000u, 9.9f);
        Roar(id + 0x90000u, 6.6f);
        GuardianResonanceTowers(id + 0xA0000u, 14.8f);
        WyvernsVengeanceForgedFury(id + 0xB0000u, 5.2f);
        Roar(id + 0xC0000u, 16f);
        ClamorousChase(id + 0xD0000u, 3.7f);
        Roar(id + 0xE0000u, 6.1f);
        GuardianResonanceTowers(id + 0xF0000u, 13.7f);
        WyvernsWeal(id + 0x100000u, 4.4f);
        Roar(id + 0x110000u, 12.5f);
        WyvernsRadianceExaflare2(id + 0x120000u, 9.6f);
        SteeltailThrustChainbladeCharge2(id + 0x130000u, 6f);
        Roar(id + 0x140000u, 6.6f);
        GuardianResonanceTowers(id + 0x150000u, 14.8f);
        WyvernsVengeance2(id + 0x160000u, 4.1f);
        GuardianWyvernsSiegeflight(id + 0x170000u, 16.9f);
        WyvernsRadianceExaflareGuardianWyvernsSiegeflight(id + 0x180000u, 1f, true);
        WyvernsRadianceCleave(id + 0x190000u, 7.9f);
        Roar(id + 0x1A0000u, 6.6f);
        WyvernsRadianceCharges(id + 0x1B0000u, 12.2f, true);
        SteeltailThrustChainbladeCharge2(id + 0x1C0000u, 6f);
        Roar(id + 0x1D0000u, 6.6f);
        Roar(id + 0x1E0000u, 16.8f);
        ForgedFuryEnrage(id + 0x1F0000u, 14.9f);
    }

    private void Roar(uint id, float delay)
    {
        ComponentCondition<Roar>(id, delay, comp => comp.NumCasts != 0, "Raidwide")
            .ActivateOnEnter<Roar>()
            .DeactivateOnExit<Roar>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void WyvernsRadianceCleave(uint id, float delay)
    {
        ComponentCondition<WyvernsRadianceCleave>(id, delay, comp => comp.NumCasts != 0, "Cleave 1")
            .ActivateOnEnter<ChainbladeBlow>()
            .ActivateOnEnter<WyvernsRadianceCleave>();
        ComponentCondition<WyvernsRadianceCleave>(id + 0x10u, 4.1f, comp => comp.NumCasts == 2, "Cleave 2")
            .DeactivateOnExit<ChainbladeBlow>()
            .DeactivateOnExit<WyvernsRadianceCleave>();
    }

    private void GuardianWyvernsSiegeflight(uint id, float delay)
    {
        ComponentCondition<GuardianWyvernsSiegeflight>(id, delay, comp => comp.NumCasts != 0, "Rect AOE 1")
            .ActivateOnEnter<GuardianWyvernsSiegeflight>()
            .ActivateOnEnter<WhiteFlash>()
            .ActivateOnEnter<Dragonspark>();
        ComponentCondition<GuardianWyvernsSiegeflight>(id + 0x10u, 3.5f, comp => comp.NumCasts >= 2, "Stacks resolve + Rect AOE 2")
            .DeactivateOnExit<GuardianWyvernsSiegeflight>()
            .DeactivateOnExit<WhiteFlash>()
            .DeactivateOnExit<Dragonspark>();
    }

    private void WyvernsRadianceExaflareGuardianWyvernsSiegeflight(uint id, float delay, bool second = false)
    {
        ComponentCondition<WyvernsRadianceExaflare1>(id, delay, comp => comp.Lines.Count != 0, "Exaflare + baited circles appear")
            .ActivateOnEnter<WyvernsRadianceExaflare1>()
            .ActivateOnEnter<WyvernsRadianceGuardianResonanceCircle>();
        ComponentCondition<WyvernsRadianceGuardianResonanceCircle>(id + 0x10u, 5f, comp => comp.NumCasts != 0, "Baited circles resolve");
        ComponentCondition<GuardianWyvernsSiegeflight>(id + 0x20u, second ? 7.3f : 8.3f, comp => comp.NumCasts != 0, "Rect AOE 1")
            .DeactivateOnExit<WyvernsRadianceExaflare1>()
            .DeactivateOnExit<WyvernsRadianceGuardianResonanceCircle>()
            .ActivateOnEnter<GuardianWyvernsSiegeflight>()
            .ActivateOnEnter<WhiteFlash>()
            .ActivateOnEnter<Dragonspark>();
        ComponentCondition<GuardianWyvernsSiegeflight>(id + 0x30u, 3.5f, comp => comp.NumCasts >= 2, "Stacks resolve + Rect AOE 2")
            .DeactivateOnExit<GuardianWyvernsSiegeflight>()
            .DeactivateOnExit<WhiteFlash>()
            .DeactivateOnExit<Dragonspark>();
    }

    private void WyvernsRadianceCharges(uint id, float delay, bool second = false, bool deactivateExas = false)
    {
        ComponentCondition<WyvernsRadianceRush>(id, delay, comp => comp.NumCharges != 0, "Charge telegraphs appear")
            .ActivateOnEnter<WyvernsRadianceRush>()
            .ActivateOnEnter<WyvernsRadianceConcentric>();
        for (uint i = 1; i <= 3; ++i)
        {
            var offset = id + i * 0x10u;
            var time = i == 1 ? 6f : 2.1f;

            var desc = $"Charge {i}";
            var casts = i;
            var cond = ComponentCondition<WyvernsRadianceRush>(offset, time, comp => comp.NumCasts == casts, desc);
            if (i == 3)
            {
                cond.DeactivateOnExit<WyvernsRadianceRush>();
            }
        }
        ComponentCondition<WyvernsRadianceConcentric>(id + 0x40u, 7.4f, comp => comp.Sequences.Count == 0, "Concentric AOEs finish")
            .ActivateOnEnter<WildEnergy>()
            .ActivateOnEnter<WyvernsOuroblade>()
            .DeactivateOnExit<WyvernsRadianceConcentric>();
        var cond2 = ComponentCondition<WildEnergy>(id + 0x50u, second ? 6.9f : 7.4f, comp => comp.NumFinishedSpreads != 0, "Spreads + cleave resolve")
            .DeactivateOnExit<WildEnergy>()
            .DeactivateOnExit<WyvernsOuroblade>();
        if (deactivateExas)
        {
            cond2.DeactivateOnExit<WyvernsRadianceExaflare2>();
        }
    }

    private void SteeltailThrustChainbladeCharge1(uint id, float delay)
    {
        ComponentCondition<SteeltailThrust>(id, delay, comp => comp.NumCasts != 0, "Line AOE")
            .ActivateOnEnter<SteeltailThrust>()
            .DeactivateOnExit<SteeltailThrust>();
        ComponentCondition<ChainbladeCharge>(id + 0x10u, 10.5f, comp => comp.NumFinishedStacks == 1, "Stack resolves")
            .ActivateOnEnter<ChainbladeCharge>()
            .DeactivateOnExit<ChainbladeCharge>();
    }

    private void GuardianResonanceTowers(uint id, float delay)
    {
        ComponentCondition<GuardianResonanceTowers>(id, delay, comp => comp.Towers.Count != 0, "Towers appear")
            .ActivateOnEnter<GuardianResonanceTowers>()
            .ActivateOnEnter<WyvernsRadianceGuardianResonanceCircle>();
        ComponentCondition<WyvernsRadianceGuardianResonanceCircle>(id + 0x10u, 3f, comp => comp.Casters.Count != 0, "Baited circles appear");
        ComponentCondition<GuardianResonanceTowers>(id + 0x20u, 7.9f, comp => comp.NumCasts != 0, "Towers resolve")
            .DeactivateOnExit<GuardianResonanceTowers>();
        ComponentCondition<WyvernsRadianceGuardianResonanceCircle>(id + 0x30u, 0.1f, comp => comp.Casters.Count == 0, "Baited circles end")
            .DeactivateOnExit<WyvernsRadianceGuardianResonanceCircle>();
    }

    private void WyvernsVengeanceForgedFury(uint id, float delay)
    {
        ComponentCondition<WyvernsVengeance>(id, delay, comp => comp.Lines.Count != 0, "Exaflares 1 appear")
            .ActivateOnEnter<WyvernsVengeance>()
            .ActivateOnEnter<WyvernsRadianceCrackedCrystal>();
        ComponentCondition<WyvernsVengeance>(id + 0x10u, 5f, comp => comp.NumCasts != 0, "Exaflares 1 start");
        ComponentCondition<WyvernsRadianceCrackedCrystal>(id + 0x20u, 1.7f, comp => comp.NumCasts != 0, "Crystals start exploding");
        ComponentCondition<WyvernsVengeance>(id + 0x30u, 5.3f, comp => comp.NumCasts == 8, "Exaflares 2 start")
            .ActivateOnEnter<WildEnergy>()
            .DeactivateOnExit<WyvernsRadianceCrackedCrystal>();
        ComponentCondition<WyvernsVengeance>(id + 0x40u, 8.1f, comp => comp.NumCasts == 12, "Exaflares 3 start + spreads 1 resolve");
        ComponentCondition<WyvernsVengeance>(id + 0x50u, 8.1f, comp => comp.NumCasts == 16, "Exaflares 4 start + spreads 2 resolve")
            .DeactivateOnExit<WildEnergy>();
        ComponentCondition<WyvernsVengeance>(id + 0x60u, 2.2f, comp => comp.Lines.Count == 0, "Exaflares end")
            .DeactivateOnExit<WyvernsVengeance>();
        for (uint i = 1; i <= 3; ++i)
        {
            var offset = id + 0x60u + i * 0x10u;
            var time = i switch
            {
                1 => 7.9f,
                2 => 0.8f,
                _ => 2.4f
            };

            var desc = $"Raidwide {i}";
            var casts = i;
            var cond = ComponentCondition<ForgedFury>(offset, time, comp => comp.NumCasts == casts, desc);
            cond.SetHint(StateMachine.StateHint.Raidwide);
            switch (i)
            {
                case 1:
                    cond.ActivateOnEnter<ForgedFury>();
                    break;
                case 3:
                    cond.DeactivateOnExit<ForgedFury>();
                    break;
            }
        }
    }

    private void ClamorousChase(uint id, float delay)
    {
        ComponentCondition<ClamorousChaseBait>(id, delay, comp => comp.IconsAssigned, "Limit Cut assignments")
            .ActivateOnEnter<ClamorousChaseBait>();
        for (uint i = 1; i <= 8; ++i)
        {
            var offset = id + i * 0x10u;
            var time = i == 1 ? 8.3f : 3f;
            var desc = $"Jump {i}";
            var casts = i;
            var cond = ComponentCondition<ClamorousChaseBait>(offset, time, comp => comp.NumCasts == casts, desc);
            switch (i)
            {
                case 1:
                    cond.ActivateOnEnter<ClamorousChaseAOE>();
                    break;
                case 8:
                    cond.DeactivateOnExit<ClamorousChaseBait>();
                    break;
            }
        }
        ComponentCondition<ClamorousChaseAOE>(id + 0x90u, 2f, comp => comp.NumCasts == 8, "Limit Cut end")
            .DeactivateOnExit<ClamorousChaseAOE>();
    }

    private void WyvernsWeal(uint id, float delay)
    {
        ComponentCondition<WyvernsWeal>(id, delay, comp => comp.CurrentBaits.Count != 0, "Baited laser 1 appears")
            .ActivateOnEnter<WyvernsWeal>()
            .ActivateOnEnter<WyvernsRadianceCrackedCrystal>()
            .ActivateOnExit<WyvernsWealAOE>();
        ComponentCondition<WyvernsWealAOE>(id + 0x10u, 10f, comp => comp.NumCasts != 0, "Baited laser 1 starts");
        ComponentCondition<WyvernsWeal>(id + 0x20u, 6.8f, comp => comp.NumFinishedLasers == 1, "Baited laser 1 ends")
            .ActivateOnExit<WhiteFlash>()
            .ExecOnExit<WyvernsWealAOE>(comp => comp.NumCasts = 0);
        ComponentCondition<WyvernsWealAOE>(id + 0x30u, 2.8f, comp => comp.NumCasts != 0, "Baited laser 2 starts");
        ComponentCondition<WhiteFlash>(id + 0x40u, 5.5f, comp => comp.NumFinishedStacks != 0, "Stacks resolve")
            .DeactivateOnExit<WhiteFlash>();
        ComponentCondition<WyvernsWeal>(id + 0x50u, 1.3f, comp => comp.NumFinishedLasers == 2, "Baited laser 2 ends")
            .ExecOnExit<WyvernsWealAOE>(comp => comp.NumCasts = 0);
        ComponentCondition<WyvernsWealAOE>(id + 0x60u, 2.8f, comp => comp.NumCasts != 0, "Baited laser 3 starts")
            .ActivateOnExit<WildEnergy>();
        ComponentCondition<WyvernsWeal>(id + 0x70u, 6.7f, comp => comp.NumFinishedLasers == 3, "Baited laser 3 ends")
            .DeactivateOnExit<WyvernsWeal>()
            .DeactivateOnExit<WyvernsRadianceCrackedCrystal>()
            .DeactivateOnExit<WyvernsWealAOE>();
        ComponentCondition<WildEnergy>(id + 0x80u, 3.5f, comp => comp.NumFinishedSpreads != 0, "Spreads resolve")
            .DeactivateOnExit<WildEnergy>();
    }

    private void WyvernsRadianceExaflare2(uint id, float delay)
    {
        ComponentCondition<WyvernsRadianceExaflare2>(id, delay, comp => comp.NumCasts != 0, "Exaflares start")
            .ActivateOnEnter<WyvernsRadianceExaflare2>();
        ComponentCondition<WyvernsRadianceCleave>(id + 0x10u, 10.6f, comp => comp.NumCasts != 0, "Cleave 1")
            .ActivateOnEnter<ChainbladeBlow>()
            .ActivateOnEnter<WyvernsRadianceCleave>();
        ComponentCondition<WyvernsRadianceCleave>(id + 0x20u, 4.1f, comp => comp.NumCasts == 2, "Cleave 2")
            .DeactivateOnExit<ChainbladeBlow>()
            .DeactivateOnExit<WyvernsRadianceCleave>();
        ComponentCondition<Roar>(id + 0x30u, 6.6f, comp => comp.NumCasts != 0, "Raidwide")
            .ActivateOnEnter<Roar>()
            .DeactivateOnExit<Roar>()
            .SetHint(StateMachine.StateHint.Raidwide);
        WyvernsRadianceCharges(id + 0x40u, 12.1f, true, true);
    }

    private void SteeltailThrustChainbladeCharge2(uint id, float delay)
    {
        ComponentCondition<SteeltailThrust>(id, delay, comp => comp.NumCasts != 0, "Line AOE")
            .ActivateOnEnter<SteeltailThrust>()
            .DeactivateOnExit<SteeltailThrust>();
        ComponentCondition<ChainbladeCharge>(id + 0x10u, 11.1f, comp => comp.NumFinishedStacks == 1, "Stack resolves")
            .ActivateOnEnter<ChainbladeCharge>()
            .DeactivateOnExit<ChainbladeCharge>();
        ComponentCondition<WyvernsRadianceChainbladeCharge>(id + 0x20u, 5f, comp => comp.NumCasts == 1, "Circle AOE")
            .ActivateOnEnter<ChainbladeBlow>()
            .ActivateOnEnter<WyvernsRadianceCleave>()
            .ActivateOnEnter<WyvernsRadianceChainbladeCharge>()
            .DeactivateOnExit<WyvernsRadianceChainbladeCharge>();
        ComponentCondition<WyvernsRadianceCleave>(id + 0x30u, 5f, comp => comp.NumCasts != 0, "Cleave 1");
        ComponentCondition<WyvernsRadianceCleave>(id + 0x40u, 4.1f, comp => comp.NumCasts == 2, "Cleave 2")
            .DeactivateOnExit<ChainbladeBlow>()
            .DeactivateOnExit<WyvernsRadianceCleave>();
    }

    private void WyvernsVengeance2(uint id, float delay)
    {
        ComponentCondition<WyvernsVengeance>(id, delay, comp => comp.Lines.Count != 0, "Exaflares 1 appear")
            .ActivateOnEnter<WyvernsVengeance>()
            .ActivateOnEnter<WyvernsRadianceCrackedCrystal>();
        ComponentCondition<Roar>(id + 0x10u, 3f, comp => comp.NumCasts != 0, "Raidwide")
            .ActivateOnEnter<Roar>()
            .DeactivateOnExit<Roar>()
            .SetHint(StateMachine.StateHint.Raidwide);
        for (uint i = 1; i <= 3; ++i)
        {
            var offset = id + 0x10u + i * 0x10u;
            var time = i == 1 ? 5f : 3f;

            var desc = $"Exaflares {i} start";
            var casts = 2 * i;
            var cond = ComponentCondition<WyvernsVengeance>(offset, time, comp => comp.NumCasts == casts, desc);
            if (i == 3)
            {
                cond.ActivateOnEnter<ChainbladeCharge>();
            }
        }
        ComponentCondition<WyvernsVengeance>(id + 0x50u, 5.4f, comp => comp.Lines.Count == 0, "Exaflares end")
            .DeactivateOnExit<WyvernsRadianceCrackedCrystal>()
            .DeactivateOnExit<WyvernsVengeance>();
        ComponentCondition<ChainbladeCharge>(id + 0x60u, 6.8f, comp => comp.NumFinishedStacks == 1, "Stack resolves")
            .DeactivateOnExit<ChainbladeCharge>();
        ComponentCondition<WyvernsRadianceChainbladeCharge>(id + 0x70u, 5f, comp => comp.NumCasts == 1, "Circle AOE")
            .ActivateOnEnter<ChainbladeBlow>()
            .ActivateOnEnter<WyvernsRadianceCleave>()
            .ActivateOnEnter<WyvernsRadianceChainbladeCharge>()
            .DeactivateOnExit<WyvernsRadianceChainbladeCharge>();
        ComponentCondition<WyvernsRadianceCleave>(id + 0x80u, 5f, comp => comp.NumCasts != 0, "Cleave 1");
        ComponentCondition<WyvernsRadianceCleave>(id + 0x90u, 4.1f, comp => comp.NumCasts == 2, "Cleave 2")
            .DeactivateOnExit<ChainbladeBlow>()
            .DeactivateOnExit<WyvernsRadianceCleave>();
    }

    private void ForgedFuryEnrage(uint id, float delay)
    {
        ComponentCondition<ForgedFury>(id, delay, comp => comp.NumCasts != 0, "Raidwide 1")
            .ActivateOnEnter<ForgedFury>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<ForgedFury>(id + 0x10u, 0.8f, comp => comp.NumCasts == 2, "Raidwide 2")
            .DeactivateOnExit<ForgedFury>()
            .SetHint(StateMachine.StateHint.Raidwide);
        SimpleState(id + 0x20u, 2.5f, "Enrage");
    }
}
