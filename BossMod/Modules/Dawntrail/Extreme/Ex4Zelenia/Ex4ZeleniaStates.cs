namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

sealed class Ex4ZeleniaStates : StateMachineBuilder
{
    public Ex4ZeleniaStates(BossModule module) : base(module)
    {
        // splitting fight into 3 parts since you will likely be able to skip mechs in phase 1 once item lvl goes up
        SimplePhase(default, Phase1, "P1")
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed || (Module.PrimaryActor.CastInfo?.IsSpell(AID.BlessedBarricade) ?? false);
        SimplePhase(1u, BlessedBarricade, "Blessed Barricade")
            .ActivateOnEnter<RosebloodDrop>()
            .ActivateOnEnter<Towers2>()
            .ActivateOnEnter<SpearpointPushAOE>()
            .ActivateOnEnter<SpearpointPushBait>()
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed || (Module.PrimaryActor.CastInfo?.IsSpell(AID.PerfumedQuietusVisual) ?? false);
        DeathPhase(2u, Phase2)
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<FloorTiles>();
    }

    private void Phase1(uint id)
    {
        ThornedCatharsis(id, 6.2f);
        AlexandrianHoly(id + 0x10000u, 11.5f);
        SpecterOfTheLost(id + 0x20000u, 0.8f);
        EscelonsFall1(id + 0x30000u, 6.6f);
        StockBreak(id + 0x40000u, 10.3f);
        SimpleState(id + 0x50000u, 3f, "Blessed Barricade");
    }

    private void BlessedBarricade(uint id)
    {
        BlessedBarricade(id, 4.3f);
        SimpleState(id + 0x10000u, 8.2f, "Enrage");
    }

    private void Phase2(uint id)
    {
        AlexandrianThunderIIplusIII(id, 33.3f);
        ThornedCatharsis(id + 0x10000u, 2.5f);
        AlexandrianThunderIV(id + 0x20000u, 20.3f);
        SpecterOfTheLost(id + 0x30000u, 2.4f);
        RosebloodBloomIII(id + 0x40000u, 22.7f);
        ThornedCatharsis(id + 0x50000u, 3.3f);
        EscelonsFall2(id + 0x60000u, 8.5f);
        StockBreak(id + 0x70000u, 10.4f);
        RosebloodBloomIV(id + 0x80000u, 15.1f);
        ThornedCatharsis(id + 0x90000u, 6.3f);
        EscelonsFall3(id + 0xA0000u, 8.6f);
        StockBreak(id + 0xB0000u, 8.5f);
        RosebloodBloomV(id + 0xC0000u, 17f);
        SpecterOfTheLost(id + 0xD0000u, 2.4f);
        RosebloodBloomVI(id + 0xE0000u, 24.9f);
        AlexandrianThunderIIplusIII(id + 0xF0000u, 20.6f);
        ThornedCatharsis(id + 0x100000u, 2.4f);
        SpecterOfTheLost(id + 0x110000u, 3f);
        StockBreak(id + 0x120000u, 14.8f);
        StockBreak(id + 0x130000u, 10f);
        StockBreak(id + 0x140000u, 10f);
        SimpleState(id + 0x150000u, 22f, "Enrage"); // spell ends 4.1s earlier, but damage is delayed a lot for some reason
    }

    private void ThornedCatharsis(uint id, float delay)
    {
        Cast(id, (uint)AID.ThornedCatharsis, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void AlexandrianHoly(uint id, float delay)
    {
        ComponentCondition<ShockSpread>(id, delay, comp => comp.CurrentBaits.Count != 0, "Baits appear")
            .ActivateOnEnter<Towers1>()
            .ActivateOnEnter<ShockSpread>();
        ComponentCondition<Towers1>(id + 0x10u, 2.3f, comp => comp.Towers.Count != 0, "Towers appear");
        ComponentCondition<ShockSpread>(id + 0x20u, 5.7f, comp => comp.NumCasts != 0, "Baits turn into AOEs")
            .ActivateOnEnter<ShockAOE>()
            .DeactivateOnExit<ShockSpread>();
        ComponentCondition<Towers1>(id + 0x30u, 1.2f, comp => comp.NumCasts != 0, "Towers resolve")
            .DeactivateOnExit<Towers1>();
        ComponentCondition<ShockAOE>(id + 0x40u, 4.2f, comp => comp.Done, "AOEs disappear")
            .DeactivateOnExit<ShockAOE>();
    }

    private void SpecterOfTheLost(uint id, float delay)
    {
        ComponentCondition<SpecterOfTheLost>(id, delay, comp => comp.Active, "Tank tethers appear")
            .ActivateOnEnter<SpecterOfTheLost>();
        ComponentCondition<SpecterOfTheLost>(id + 0x10u, 7.7f, comp => comp.NumCasts != 0, "Tankbusters")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<SpecterOfTheLost>();
    }

    private void EscelonsFall1(uint id, float delay)
    {
        Cast(id, (uint)AID.EscelonsFallVisual1, delay, 13f, "Select bait order")
            .ActivateOnEnter<EscelonsFall>();
        EscelonsFall(id, 0x30u, 1f);
    }

    private void StockBreak(uint id, float delay)
    {
        ComponentCondition<StockBreak>(id, delay, comp => comp.NumCasts != 0, "Stack hit 1")
            .ActivateOnEnter<StockBreak>();
        ComponentCondition<StockBreak>(id + 0x10u, 3.3f, comp => comp.NumCasts == 2, "Stack hit 4")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<StockBreak>();
    }

    private void BlessedBarricade(uint id, float delay)
    {
        ComponentCondition<RosebloodDrop>(id, delay, comp => comp.ActiveActors.Count != 0, "Adds become targetable");

        for (var i = 0; i < 4; ++i)
        {
            var baseOffset = (uint)(0x50 * i);
            ComponentCondition<SpearpointPushBait>(id + baseOffset + 0x10u, i == 0 ? 3.8f : 5.2f, comp => comp.CurrentBaits.Count != 0, $"Baits {i + 1} appear");
            if (i < 3)
                ComponentCondition<Towers2>(id + baseOffset + 0x20u, 1f, comp => comp.Towers.Count != 0, $"Towers {i + 1} appear");
            ComponentCondition<SpearpointPushAOE>(id + baseOffset + (i > 2 ? 0x20u : 0x30u), i == 3 ? 5.3f : 4.3f, comp => comp.AOEs.Count != 0, $"Baits {i + 1} end + AOEs {i + 1} appear");
            ComponentCondition<SpearpointPushAOE>(id + baseOffset + (i > 2 ? 0x30u : 0x40u), 1.5f, comp => comp.AOEs.Count == 0, $"AOEs {i + 1} resolve");
            if (i < 3)
                ComponentCondition<Towers2>(id + baseOffset + 0x50u, 0.3f, comp => comp.Towers.Count == 0, $"Towers {i + 1} resolve");
        }
    }

    private void AlexandrianThunderIIplusIII(uint id, float delay)
    {
        ComponentCondition<AlexandrianThunderII>(id, delay, comp => comp.NumCasts != 0, "Rotation start")
            .ActivateOnEnter<ActiveTiles>()
            .ActivateOnEnter<AlexandrianThunderII>();
        ComponentCondition<AlexandrianThunderII>(id + 0x10u, 14.2f, comp => comp.NumCasts == 45, "Rotation end")
            .DeactivateOnExit<AlexandrianThunderII>();
        ComponentCondition<AlexandrianThunderIIISpread>(id + 0x20u, 0.5f, comp => comp.Spreads.Count != 0, "Spreads appear")
            .ActivateOnEnter<AlexandrianThunderIIISpread>();
        ComponentCondition<AlexandrianThunderIIISpread>(id + 0x30u, 5f, comp => comp.NumFinishedSpreads > 5, "Spreads resolved")
            .DeactivateOnExit<ActiveTiles>()
            .DeactivateOnExit<AlexandrianThunderIIISpread>();
    }

    private void AlexandrianThunderIV(uint id, float delay)
    {
        ComponentCondition<AlexandrianThunderIV>(id, delay, comp => comp.NumCasts != 0, "In OR Out AOE 1 + Cone AOE 1")
            .ActivateOnEnter<ActiveTiles>()
            .ActivateOnEnter<ThunderSlash>()
            .ActivateOnEnter<AlexandrianThunderIV>();
        for (var i = 2; i <= 3; ++i)
        {
            var offset = id + (uint)((i - 1) * 0x10u);
            var time = 1f;
            var desc = $"Cone AOE {i}";
            var casts = i;
            ComponentCondition<ThunderSlash>(offset, time, comp => comp.NumCasts == casts, desc);
        }
        ComponentCondition<AlexandrianThunderIV>(id + 0x30u, 1f, comp => comp.NumCasts == 2, "In OR Out AOE 2 + Cone AOE 4")
            .DeactivateOnExit<ActiveTiles>()
            .DeactivateOnExit<AlexandrianThunderIV>();
        for (var i = 5; i <= 6; ++i)
        {
            var offset = id + (uint)((i - 1) * 0x10u);
            var time = 1f;
            var desc = $"Cone AOE {i}";
            var casts = i;
            var cond = ComponentCondition<ThunderSlash>(offset, time, comp => comp.NumCasts == casts, desc);
            if (i == 6)
            {
                cond
                .DeactivateOnExit<ThunderSlash>();
            }
        }
    }

    private void RosebloodBloomIII(uint id, float delay)
    {
        ComponentCondition<DonutSectorTowers>(id, delay, comp => comp.Towers.Count != 0, "Towers appear")
            .ActivateOnEnter<DonutSectorTowers>()
            .ActivateOnEnter<Emblazon>();
        ComponentCondition<Emblazon>(id + 0x10u, 1.9f, comp => comp.Towers.Count != 0, "Rose markers appear");
        ComponentCondition<Emblazon>(id + 0x20u, 6.8f, comp => comp.NumCasts != 0, "Rose markers resolve")
            .DeactivateOnExit<Emblazon>();
        ComponentCondition<DonutSectorTowers>(id + 0x30u, 4.3f, comp => comp.NumCasts != 0, "Towers resolve")
            .DeactivateOnExit<DonutSectorTowers>();
    }

    private void EscelonsFall2(uint id, float delay)
    {
        Cast(id, (uint)AID.BudOfValor, delay, 3f, "");
        ComponentCondition<ShockSpread>(id + 0x10u, 7.9f, comp => comp.CurrentBaits.Count != 0, "Baits appear")
            .ActivateOnEnter<ShockSpread>();
        CastStart(id + 0x20u, (uint)AID.EscelonsFallVisual1, 1.3f, "Select bait order")
            .ActivateOnEnter<EscelonsFall>();
        ComponentCondition<AlexandrianBanishII>(id + 0x30u, 2f, comp => comp.Stacks.Count != 0, "Stacks appear")
            .ActivateOnEnter<AlexandrianBanishII>();
        ComponentCondition<ShockSpread>(id + 0x40u, 4.7f, comp => comp.NumCasts != 0, "Baits turn into AOEs")
            .ActivateOnEnter<ShockAOE>()
            .DeactivateOnExit<ShockSpread>();
        ComponentCondition<AlexandrianBanishII>(id + 0x50u, 1.1f, comp => comp.Stacks.Count == 0, "Stacks resolve")
            .DeactivateOnExit<AlexandrianBanishII>();
        ComponentCondition<ShockAOE>(id + 0x60u, 4.4f, comp => comp.Done, "AOEs disappear")
            .DeactivateOnExit<ShockAOE>();
        EscelonsFall(id, 0x70u, 1.7f);
    }

    private void RosebloodBloomIV(uint id, float delay)
    {
        ComponentCondition<Emblazon>(id, delay, comp => comp.Towers.Count != 0, "Rose markers appear")
            .ActivateOnEnter<Emblazon>()
            .ActivateOnEnter<ActiveTiles>()
            .ActivateOnEnter<AlexandrianThunderIIIAOE>()
            .ExecOnEnter<Emblazon>(comp => comp.Mechanic = false);
        ComponentCondition<AlexandrianThunderIIISpread>(id + 0x10u, 2.3f, comp => comp.Spreads.Count != 0, "Spreads appear")
            .ActivateOnEnter<AlexandrianThunderIIISpread>();
        ComponentCondition<Emblazon>(id + 0x20u, 4.5f, comp => comp.NumCasts != 0, "Rose markers resolve")
            .ExecOnExit<Emblazon>(comp => comp.Towers.Clear());
        ComponentCondition<AlexandrianThunderIIISpread>(id + 0x30u, 0.5f, comp => comp.Spreads.Count == 0, "Spreads resolve")
            .DeactivateOnExit<AlexandrianThunderIIISpread>();
        ComponentCondition<AlexandrianThunderIIIAOE>(id + 0x40u, 0.1f, comp => comp.NumCasts != 0, "Circle AOEs")
            .DeactivateOnExit<ActiveTiles>()
            .DeactivateOnExit<AlexandrianThunderIIIAOE>();
        ComponentCondition<ThornyVine>(id + 0x50u, 7.8f, comp => comp.TethersAssigned, "Chains appear")
            .DeactivateOnExit<Emblazon>()
            .ActivateOnEnter<ThornyVine>();
        ComponentCondition<AlexandrianBanishIII>(id + 0x60u, 0.3f, comp => comp.CurrentBaits.Count != 0, "Stack appears")
            .ActivateOnEnter<AlexandrianBanishIIITargetHint>()
            .ActivateOnEnter<AlexandrianBanishIII>();
        ComponentCondition<ThornyVine>(id + 0x70u, 3.4f, comp => comp.NumCasts > 1, "Chains resolve", 15f)
            .DeactivateOnExit<ThornyVine>();
        ComponentCondition<AlexandrianBanishIII>(id + 0x80u, 1.5f, comp => comp.NumCasts != 0, "Stack resolves")
            .DeactivateOnExit<AlexandrianBanishIII>()
            .DeactivateOnExit<AlexandrianBanishIIITargetHint>();
    }

    private void EscelonsFall3(uint id, float delay)
    {
        Cast(id, (uint)AID.BudOfValor, delay, 3f, "");
        CastStart(id + 0x10u, (uint)AID.EscelonsFallVisual1, 3.1f, "Select bait order")
            .ActivateOnEnter<EscelonsFall>();
        ComponentCondition<PowerBreak>(id + 0x20u, 9.9f, comp => comp.NumCasts != 0, "Half room cleave 1")
            .ActivateOnEnter<PowerBreak>();
        EscelonsFall(id, 0x30u, 4f);
        ComponentCondition<PowerBreak>(id + 0x70u, 1.9f, comp => comp.NumCasts == 2, "Half room cleave 2")
            .DeactivateOnExit<PowerBreak>();
    }

    private bool EscelonsFall(uint id, uint extra, float delay1)
    {
        for (var i = 1; i <= 4; ++i)
        {
            var offset = id + extra + (uint)((i - 1) * 0x10u);
            var time = i == 1 ? delay1 : 3.1f;
            var desc = $"Baits {i}";
            var casts = i * 4;
            var cond = ComponentCondition<EscelonsFall>(offset, time, comp => comp.NumCasts == casts, desc);
            if (i == 4)
            {
                cond.DeactivateOnExit<EscelonsFall>();
            }
        }
        return true;
    }

    private void RosebloodBloomV(uint id, float delay)
    {
        ComponentCondition<ValorousAscension>(id, delay, comp => comp.NumCasts != 0, "Raidwide 1")
            .SetHint(StateMachine.StateHint.Raidwide)
            .ActivateOnEnter<ValorousAscension>()
            .ActivateOnEnter<ValorousAscensionRect>();
        ComponentCondition<ValorousAscension>(id + 0x10u, 1.8f, comp => comp.NumCasts == 3, "Raidwide 3")
            .DeactivateOnExit<ValorousAscension>()
            .ActivateOnExit<ActiveTiles>()
            .ActivateOnExit<ThunderSlash>()
            .ActivateOnExit<AlexandrianThunderIV>();
        ComponentCondition<ValorousAscensionRect>(id + 0x20u, 9.2f, comp => comp.NumCasts != 0, "Line AOEs 1");
        ComponentCondition<AlexandrianThunderIV>(id + 0x30u, 0.1f, comp => comp.NumCasts != 0, "In OR Out AOE 1 + Cone AOE 1");
        for (var i = 2; i <= 3; ++i)
        {
            var offset = id + 0x30u + (uint)((i - 1) * 0x10u);
            var time = 1f;
            var desc = $"Cone AOE {i}";
            var casts = i;
            ComponentCondition<ThunderSlash>(offset, time, comp => comp.NumCasts == casts, desc);
        }
        ComponentCondition<ValorousAscensionRect>(id + 0x60u, 0.8f, comp => comp.NumCasts == 4, "Line AOEs 2")
            .DeactivateOnExit<ValorousAscensionRect>();
        ComponentCondition<AlexandrianThunderIV>(id + 0x70u, 0.1f, comp => comp.NumCasts == 2, "In OR Out AOE 2 + Cone AOE 4")
            .DeactivateOnExit<ActiveTiles>()
            .DeactivateOnExit<AlexandrianThunderIV>();
        for (var i = 5; i <= 6; ++i)
        {
            var offset = id + 0x40u + (uint)((i - 1) * 0x10u);
            var time = 1f;
            var desc = $"Cone AOE {i}";
            var casts = i;
            var cond = ComponentCondition<ThunderSlash>(offset, time, comp => comp.NumCasts == casts, desc);
            if (i == 6)
            {
                cond
                .DeactivateOnExit<ThunderSlash>();
            }
        }
    }

    private void RosebloodBloomVI(uint id, float delay)
    {
        ComponentCondition<Emblazon>(id, delay, comp => comp.Allowed != default, "Rose markers appear")
            .ActivateOnEnter<Emblazon>()
            .ExecOnEnter<Emblazon>(comp => comp.Mechanic = true)
            .ActivateOnEnter<DonutSectorTowers>();
        ComponentCondition<DonutSectorTowers>(id + 0x10u, 0.1f, comp => comp.Towers.Count != 0, "Towers appear");
        ComponentCondition<Emblazon>(id + 0x20u, 6.8f, comp => comp.NumCasts != 0, "Rose markers resolve")
            .ActivateOnEnter<HolyHazard>()
            .DeactivateOnExit<Emblazon>();
        ComponentCondition<HolyHazard>(id + 0x30u, 3.3f, comp => comp.NumCasts != 0, "Cone AOEs 1");
        ComponentCondition<HolyHazard>(id + 0x40u, 3f, comp => comp.NumCasts == 4, "Cone AOEs 2")
            .DeactivateOnExit<HolyHazard>();
        ComponentCondition<DonutSectorTowers>(id + 0x50u, 0.1f, comp => comp.NumCasts != 0, "Towers resolve")
            .DeactivateOnExit<DonutSectorTowers>();
    }
}
