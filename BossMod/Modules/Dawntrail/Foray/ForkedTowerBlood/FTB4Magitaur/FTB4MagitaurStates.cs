namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB4Magitaur;

sealed class FTB4MagitaurStates : StateMachineBuilder
{
    public FTB4MagitaurStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        UnsealedAura(id, 15.2f);
        Unseal(id + 0x10000u, 23.3f);
        AssassinsDagger(id + 0x20000u, 6.6f);
        ForkedFury(id + 0x30000u, 10.1f);
        AuraBurstHoly(id + 0x40000u, 1.5f);
        SagesStaff(id + 0x50000u, 23.1f);
        UnsealedAura(id + 0x60000u, 4.8f);
        Unseal(id + 0x70000u, 19.3f);
        RuneAxe(id + 0x80000u, 7.9f);
        ForkedFury(id + 0x90000u, 12.5f);
        AuraBurstHoly(id + 0xA0000u, 1.8f);
        AssassinsDagger(id + 0xB0000u, 9.2f);
        Unseal(id + 0xC0000u, 15.3f);
        HolyLance(id + 0xD0000u, 17.2f);
        UnsealedAura(id + 0xE0000u, 9f);
        ForkedFury(id + 0xF0000u, 9.2f);
        AuraBurstHoly(id + 0x100000u, 1.4f);
        AssassinsDagger(id + 0x110000u, 9.1f);
        SimpleState(id + 0x120000u, 27f, "Enrage");
    }

    private void UnsealedAura(uint id, float delay)
    {
        ComponentCondition<UnsealedAura>(id, delay, comp => comp.NumCasts != 0, "Raidwide")
            .ActivateOnEnter<UnsealedAura>()
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<UnsealedAura>();
    }

    private void Unseal(uint id, float delay)
    {
        for (var i = 1; i <= 3; ++i)
        {
            var offset = id + (uint)((i - 1) * 0x10u);
            var casts = i * 6;
            var condition = ComponentCondition<Unseal>(offset, i == 1 ? delay : 3.2f, comp => comp.NumCasts >= casts, $"Baited tankbusters {i}")
                .SetHint(StateMachine.StateHint.Tankbuster);
            if (i == 1)
            {
                condition
                    .ActivateOnEnter<Unseal>();
            }
            else if (i == 3)
            {
                condition
                    .DeactivateOnExit<Unseal>();
            }
        }
    }

    private void AssassinsDagger(uint id, float delay)
    {
        for (var i = 1; i <= 7; ++i)
        {
            var offset = id + (uint)((i - 1) * 0x10u);
            var casts = i * 3;
            var condition = ComponentCondition<AssassinsDagger>(offset, i == 1 ? delay : 2f, comp => comp.NumCasts == casts, $"Line AOEs {i}");
            if (i == 1)
            {
                condition
                    .ActivateOnEnter<AssassinsDagger>();
            }
            else if (i == 4)
            {
                condition
                    .ActivateOnExit<CriticalAxeLanceBlow>();
            }
        }
        ComponentCondition<CriticalAxeLanceBlow>(id + 0x70u, 1.9f, comp => comp.NumCasts > 1, "In OR Out AOEs + Line AOEs 8") // these AOEs can either be a couple 100ms apart or in the same frame...
            .DeactivateOnExit<CriticalAxeLanceBlow>();
        for (var i = 9; i <= 12; ++i)
        {
            var offset = id + (uint)((i - 1) * 0x10u);
            var casts = i * 3;
            var condition = ComponentCondition<AssassinsDagger>(offset, 2f, comp => comp.NumCasts == casts, $"Line AOEs {i}");
            if (i == 9)
            {
                condition
                    .ActivateOnExit<CriticalAxeLanceBlow>();
            }
            else if (i == 12)
            {
                condition
                    .DeactivateOnExit<AssassinsDagger>();
            }
        }
        ComponentCondition<CriticalAxeLanceBlow>(id + 0xD0u, 2.4f, comp => comp.NumCasts > 1, "In OR Out AOEs")
            .DeactivateOnExit<CriticalAxeLanceBlow>();
    }

    private void ForkedFury(uint id, float delay)
    {
        ComponentCondition<ForkedFury>(id, delay, comp => comp.NumCasts > 1, "Baited tankbusters 1")
            .ActivateOnEnter<ForkedFury>()
            .ActivateOnEnter<Unseal>()
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<ForkedFury>();
        for (var i = 2; i <= 3; ++i)
        {
            var offset = id + (uint)((i - 1) * 0x10u);
            var casts = i * 6 - 6;
            var condition = ComponentCondition<Unseal>(offset, i == 2 ? 6.2f : 3.2f, comp => comp.NumCasts >= casts, $"Baited tankbusters {i}")
                .SetHint(StateMachine.StateHint.Tankbuster);
            if (i == 3)
            {
                condition
                    .DeactivateOnExit<Unseal>();
            }
        }
    }

    private void AuraBurstHoly(uint id, float delay)
    {
        ComponentCondition<AuraBurstHolyRaidwide>(id, delay, comp => comp.Activation != default, "Conduit phase start")
            .ActivateOnEnter<AuraBurstHoly>()
            .ActivateOnEnter<AuraBurstHolyRaidwide>()
            .ActivateOnEnter<ArcaneRecoil>()
            .ActivateOnEnter<ArcaneReaction>();
        ComponentCondition<AuraBurstHolyRaidwide>(id + 0x10u, 20f, comp => comp.NumCasts != 0, "Conduit phase end")
            .DeactivateOnExit<AuraBurstHoly>()
            .DeactivateOnExit<AuraBurstHolyRaidwide>()
            .DeactivateOnExit<ArcaneRecoil>()
            .DeactivateOnExit<ArcaneReaction>();
    }

    private void SagesStaff(uint id, float delay)
    {
        ComponentCondition<SagesStaff>(id, delay, comp => comp.CurrentBaits.Count != 0, "Line stacks 1 appear")
            .ActivateOnEnter<SagesStaff>()
            .ActivateOnEnter<CriticalAxeLanceBlow>();
        ComponentCondition<CriticalAxeLanceBlow>(id + 0x10u, 6.5f, comp => comp.NumCasts > 1, "In OR Out AOEs 1")
            .DeactivateOnExit<CriticalAxeLanceBlow>();
        ComponentCondition<SagesStaff>(id + 0x20u, 4.8f, comp => comp.NumCasts == 3, "Line stacks 1 resolve")
            .ResetComp<SagesStaff>()
            .ActivateOnExit<CriticalAxeLanceBlow>();
        ComponentCondition<SagesStaff>(id + 0x30u, 6.1f, comp => comp.CurrentBaits.Count != 0, "Line stacks 2 appear");
        ComponentCondition<CriticalAxeLanceBlow>(id + 0x40u, 6.3f, comp => comp.NumCasts > 1, "In OR Out AOEs 2")
            .DeactivateOnExit<CriticalAxeLanceBlow>();
        ComponentCondition<SagesStaff>(id + 0x50u, 5.2f, comp => comp.NumCasts == 3, "Line stacks 2 resolve")
            .DeactivateOnExit<SagesStaff>();
    }

    private void RuneAxe(uint id, float delay)
    {
        ComponentCondition<RuneAxeStatus>(id, delay, comp => comp.StatusSmall.Count != 0, "Apply spread statuses")
            .ActivateOnEnter<RuneAxeStatus>()
            .ActivateOnEnter<RuneAxeSmallSpreadAOEs>()
            .ActivateOnEnter<RuneAxeAOEs>();
        ComponentCondition<RuneAxeStatus>(id + 0x10u, 9.1f, comp => comp.NumCasts != 0, "Big spread 1");
        ComponentCondition<RuneAxeStatus>(id + 0x20u, 3.9f, comp => comp.NumCasts > 2, "Small spreads 1")
            .ExecOnExit<RuneAxeSmallSpreadAOEs>(comp => comp.Show = false)
            .ExecOnExit<RuneAxeAOEs>(comp => comp.Show = false);
        ComponentCondition<CriticalAxeLanceBlow>(id + 0x30u, 2.6f, comp => comp.NumCasts == 4, "Donut and square AOEs")
            .ActivateOnEnter<CriticalAxeLanceBlow>()
            .ExecOnExit<RuneAxeSmallSpreadAOEs>(comp => comp.Show = true)
            .ExecOnExit<RuneAxeAOEs>(comp => comp.Show = true)
            .DeactivateOnExit<CriticalAxeLanceBlow>();
        ComponentCondition<RuneAxeStatus>(id + 0x40u, 5.4f, comp => comp.StatusBig.Count == 0 && comp.StatusSmall.Count == 0, "Remaining spreads")
            .DeactivateOnExit<RuneAxeAOEs>()
            .DeactivateOnExit<RuneAxeSmallSpreadAOEs>()
            .DeactivateOnExit<RuneAxeStatus>();
    }

    private void HolyLance(uint id, float delay)
    {
        ComponentCondition<CriticalAxeLanceBlow>(id, delay, comp => comp.NumCasts > 1, "In OR Out AOEs")
            .ActivateOnEnter<CriticalAxeLanceBlow>()
            .ActivateOnEnter<HolyLance>()
            .ActivateOnEnter<HolyIV>()
            .ActivateOnEnter<HolyIVHints>()
            .ExecOnExit<HolyLance>(comp => comp.Show = true)
            .DeactivateOnExit<CriticalAxeLanceBlow>();
        static string GetString(int i)
        {
            if (i is 1 or 5 or 9)
            {
                var aoeNum = i == 1 ? "1" : i == 5 ? "2" : "3";
                return $"Outside AOE {aoeNum}";
            }
            else if (i is 3 or 7 or 11)
            {
                var stackNum = i == 3 ? "1" : i == 7 ? "2" : "3";
                var squareNum = i == 3 ? "2" : i == 7 ? "5" : "8";
                return $"Stack {stackNum} + square AOE {squareNum}";
            }
            else
            {
                var squareNum = i switch
                {
                    2 => "1",
                    4 => "3",
                    6 => "4",
                    8 => "6",
                    10 => "7",
                    _ => "9"
                };
                return $"Square AOE {squareNum}";
            }
        }
        for (var i = 1; i <= 12; ++i)
        {
            var offset = id + (uint)(i * 0x10u);
            var casts = i;
            var condition = ComponentCondition<HolyIV>(offset, i == 1 ? 3.5f : 2f, comp => comp.NumCasts == casts, GetString(i));
            if (i is 3 or 7 or 11)
            {
                condition
                    .SetHint(StateMachine.StateHint.Raidwide);
            }
            else if (i == 12)
            {
                condition
                    .DeactivateOnExit<HolyIVHints>()
                    .DeactivateOnExit<HolyIV>()
                    .DeactivateOnExit<HolyLance>();
            }
        }
        ComponentCondition<CriticalAxeLanceBlow>(id + 0xD0u, 2.6f, comp => comp.NumCasts > 1, "In OR Out AOEs")
            .ActivateOnEnter<CriticalAxeLanceBlow>()
            .DeactivateOnExit<CriticalAxeLanceBlow>();
    }
}
