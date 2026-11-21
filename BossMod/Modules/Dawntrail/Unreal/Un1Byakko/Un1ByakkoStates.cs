namespace BossMod.Dawntrail.Unreal.Un1Byakko;

sealed class Un1ByakkoStates : StateMachineBuilder
{
    private readonly Un1Byakko _module;

    public Un1ByakkoStates(Un1Byakko module) : base(module)
    {
        _module = module;
        DeathPhase(0u, SinglePhase)
            .ActivateOnEnter<UnrelentingAnguish>(); // these orbs linger after mechanic ends
    }

    private void SinglePhase(uint id)
    {
        StormPulse(id, 6.2f);
        HeavenlyStrike(id + 0x10000u, 2.1f);
        StateOfShock(id + 0x20000u, 6.1f);
        UnrelentingAnguish1(id + 0x30000u, 11.1f);
        Hakutei1(id + 0x40000u, 13.4f);
        Intermission(id + 0x50000u, 2.1f);
        HeavenlyStrike(id + 0x60000u, 7.2f);
        HundredfoldHavoc1(id + 0x70000u, 6.2f);
        UnrelentingAnguish2(id + 0x80000u, 11.5f);
        Hakutei2(id + 0x90000u, 8.9f); // note: variance is quite large
        StormPulseDouble(id + 0xA0000u, 11.2f);
        HundredfoldHavoc2(id + 0xB0000u, 8f);
        HeavenlyStrike(id + 0xC0000u, 10.2f);
        StormPulseDouble(id + 0xD0000u, 6.2f);
        DistantClap(id + 0xE0000u, 6.1f);
        HeavenlyStrike(id + 0xF0000u, 2.1f);
        UnrelentingAnguish2(id + 0x100000u, 9.5f);
        StormPulseDouble(id + 0x110000u, 10.5f);
        HundredfoldHavoc1(id + 0x120000u, 6.1f);
        StormPulseQuadruple(id + 0x130000u, 11.5f);
        Cast(id + 0x140000u, AID.StormPulseEnrage, 2.1f, 8f, "Enrage");
    }

    private State StormPulse(uint id, float delay, string name = "Raidwide")
    {
        return Cast(id, AID.StormPulse, delay, 4f, name)
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void StormPulseDouble(uint id, float delay)
    {
        StormPulse(id, delay, "Raidwide 1");
        ComponentCondition<StormPulseRepeat>(id + 2u, 2.2f, static comp => comp.NumCasts > 0, "Raidwide 2")
            .ActivateOnEnter<StormPulseRepeat>()
            .DeactivateOnExit<StormPulseRepeat>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void StormPulseQuadruple(uint id, float delay)
    {
        StormPulse(id, delay, "Raidwide 1");
        ComponentCondition<StormPulseRepeat>(id + 2u, 2.2f, static comp => comp.NumCasts > 0, "Raidwide 2")
            .ActivateOnEnter<StormPulseRepeat>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<StormPulseRepeat>(id + 3u, 2.1f, static comp => comp.NumCasts > 1, "Raidwide 3")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<StormPulseRepeat>(id + 4u, 2.1f, static comp => comp.NumCasts > 2, "Raidwide 4")
            .DeactivateOnExit<StormPulseRepeat>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State HeavenlyStrike(uint id, float delay)
    {
        return Cast(id, AID.HeavenlyStrike, delay, 4f, "Tankbuster")
            .ActivateOnEnter<HeavenlyStrike>()
            .DeactivateOnExit<HeavenlyStrike>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void DistantClap(uint id, float delay)
    {
        Cast(id, AID.DistantClap, delay, 5f, "Donut")
            .ActivateOnEnter<DistantClap>()
            .DeactivateOnExit<DistantClap>();
    }

    private void StateOfShock(uint id, float delay)
    {
        Cast(id, AID.StateOfShock, delay, 4f);
        ComponentCondition<StateOfShock>(id + 0x10u, 0.9f, static comp => comp.NumStuns > 0, "Grab tank")
            .ActivateOnEnter<StateOfShock>()
            .ActivateOnEnter<HighestStakes>();
        Cast(id + 0x20u, AID.HighestStakes, 1.5f, 5);
        ComponentCondition<HighestStakes>(id + 0x30u, 0.8f, static comp => comp.NumCasts > 0, "Tower 1")
            .DeactivateOnExit<StateOfShock>();
        ComponentCondition<StateOfShock>(id + 0x40u, 2f, static comp => comp.NumCasts > 0)
            .ActivateOnEnter<StateOfShock>();
        ComponentCondition<StateOfShock>(id + 0x41u, 0.9f, static comp => comp.NumStuns > 0, "Grab tank");
        Cast(id + 0x50u, AID.HighestStakes, 1.2f, 5);
        ComponentCondition<HighestStakes>(id + 0x60u, 0.8f, static comp => comp.NumCasts > 1, "Tower 2")
            .DeactivateOnExit<StateOfShock>()
            .DeactivateOnExit<HighestStakes>();
    }

    private void UnrelentingAnguish1(uint id, float delay)
    {
        Cast(id, AID.UnrelentingAnguish, delay, 3f, "Orbs start");
        StormPulse(id + 0x10u, 2.2f);
        ComponentCondition<OminousWind>(id + 0x20u, 2.9f, static comp => comp.Targets.Any())
            .ActivateOnEnter<OminousWind>();
        Cast(id + 0x30u, AID.FireAndLightningBoss, 4.5f, 4f, "Line")
            .ActivateOnEnter<FireAndLightningBoss>()
            .DeactivateOnExit<FireAndLightningBoss>();
        ComponentCondition<OminousWind>(id + 0x40u, 1.5f, static comp => comp.Targets.None(), "Orbs end")
            .DeactivateOnExit<OminousWind>();
    }

    private void Hakutei1(uint id, float delay)
    {
        ActorTargetable(id, _module.Hakutei, true, delay, "Tiger appears")
            .ActivateOnEnter<AratamaPuddleBait>(); // icons appear ~2s before tiger
        ComponentCondition<AratamaPuddleBait>(id + 0x10, 3.1f, static comp => comp.NumFinishedSpreads > 0, "Puddle baits start")
            .ActivateOnEnter<AratamaPuddleVoidzone>();
        StormPulse(id + 0x20u, 4f)
            .ActivateOnEnter<SteelClaw>()
            .DeactivateOnExit<AratamaPuddleBait>();
        ComponentCondition<SteelClaw>(id + 0x30u, 0.1f, static comp => comp.NumCasts > 0, "Cleave 1");
        CastStart(id + 0x40u, AID.HeavenlyStrike, 6);
        ComponentCondition<SteelClaw>(id + 0x41u, 0.1f, static comp => comp.NumCasts > 1, "Cleave 2")
            .ActivateOnEnter<HeavenlyStrike>()
            .DeactivateOnExit<SteelClaw>();
        CastEnd(id + 0x42u, 3.9f, "Tankbuster")
            .DeactivateOnExit<HeavenlyStrike>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ActorTargetable(id + 0x50u, _module.Hakutei, false, 3.1f, "Tiger disappears");

        ComponentCondition<WhiteHerald>(id + 0x60u, 0.7f, static comp => comp.Active)
            .ActivateOnEnter<WhiteHerald>();
        CastStart(id + 0x61u, AID.DistantClap, 2.4f);
        ComponentCondition<WhiteHerald>(id + 0x62u, 2.7f, static comp => comp.NumFinishedSpreads > 0, "Flare")
            .ActivateOnEnter<DistantClap>()
            .DeactivateOnExit<WhiteHerald>();
        ActorTargetable(id + 0x63u, _module.Hakutei, true, 2f, "Tiger reappears");
        CastEnd(id + 0x64u, 0.3f, "Donut")
            .DeactivateOnExit<DistantClap>();
        ComponentCondition<FireAndLightningAdd>(id + 0x65u, 3.9f, static comp => comp.NumCasts > 0, "Line") // note: pretty large variance here
            .ActivateOnEnter<FireAndLightningAdd>()
            .DeactivateOnExit<FireAndLightningAdd>();
        StormPulse(id + 0x70u, 0.2f)
            .DeactivateOnExit<AratamaPuddleVoidzone>();
    }

    private void Intermission(uint id, float delay)
    {
        ActorTargetable(id, _module.Boss, false, delay, "Boss disappears");
        ActorCast(id + 0x10u, _module.Hakutei, AID.RoarOfThunder, 4.4f, 20f, true, "Add enrage") // note: pretty large variance here
            .ActivateOnEnter<VoiceOfThunder>()
            .DeactivateOnExit<VoiceOfThunder>()
            .SetHint(StateMachine.StateHint.Raidwide | StateMachine.StateHint.DowntimeStart);
        ComponentCondition<Intermission>(id + 0x12u, 5.7f, static comp => comp.Active)
            .ActivateOnEnter<Intermission>()
            .OnExit(() => Module.Arena.Bounds = Un1Byakko.GetIntermissionBounds());
        ComponentCondition<IntermissionSweepTheLeg>(id + 0x20u, 36.5f, static comp => comp.NumCasts > 0, "Donut 1")
            .ActivateOnEnter<IntermissionOrbAratama>()
            .ActivateOnEnter<IntermissionSweepTheLeg>();
        ComponentCondition<ImperialGuard>(id + 0x21u, 5.7f, static comp => comp.NumCasts > 0, "Line 1")
            .ActivateOnEnter<ImperialGuard>();
        ComponentCondition<ImperialGuard>(id + 0x22u, 12f, static comp => comp.NumCasts > 1, "Line 2");
        ComponentCondition<IntermissionSweepTheLeg>(id + 0x23, 13.6f, static comp => comp.NumCasts > 1, "Donut 2")
            .DeactivateOnExit<IntermissionOrbAratama>()
            .DeactivateOnExit<IntermissionSweepTheLeg>();
        ComponentCondition<ImperialGuard>(id + 0x24u, 3.4f, static comp => comp.NumCasts > 2, "Line 3")
            .DeactivateOnExit<ImperialGuard>();
        ComponentCondition<Intermission>(id + 0x25u, 7.5f, static comp => !comp.Active)
            .DeactivateOnExit<Intermission>()
            .OnExit(() => Module.Arena.Bounds = Un1Byakko.BuildArena().arena);
        ComponentCondition<FellSwoop>(id + 0x26u, 20.2f, static comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<FellSwoop>()
            .DeactivateOnExit<FellSwoop>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorTargetable(id + 0x30u, _module.Boss, true, 7.9f, "Boss reappears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    private void HundredfoldHavoc1(uint id, float delay)
    {
        ComponentCondition<HundredfoldHavoc>(id, delay, static comp => comp.Active)
            .ActivateOnEnter<HundredfoldHavoc>();
        CastStart(id + 1u, AID.StateOfShock, 2.1f);
        ComponentCondition<HundredfoldHavoc>(id + 2u, 2.9f, static comp => comp.NumCasts > 0, "Exaflares start");
        CastEnd(id + 3u, 1.1f);

        ComponentCondition<StateOfShock>(id + 0x10u, 0.9f, static comp => comp.NumStuns > 0, "Grab tank")
            .ActivateOnEnter<StateOfShock>()
            .ActivateOnEnter<HighestStakes>();
        Cast(id + 0x20u, AID.HighestStakes, 1.5f, 5f)
            .DeactivateOnExit<HundredfoldHavoc>();
        ComponentCondition<HighestStakes>(id + 0x30u, 0.8f, static comp => comp.NumCasts > 0, "Tower 1")
            .DeactivateOnExit<StateOfShock>();
        ComponentCondition<StateOfShock>(id + 0x40u, 2f, static comp => comp.NumCasts > 0)
            .ActivateOnEnter<StateOfShock>();
        ComponentCondition<StateOfShock>(id + 0x41u, 0.9f, static comp => comp.NumStuns > 0, "Grab tank");
        Cast(id + 0x50u, AID.HighestStakes, 1.3f, 5f);
        ComponentCondition<HighestStakes>(id + 0x60u, 0.8f, static comp => comp.NumCasts > 1, "Tower 2")
            .DeactivateOnExit<StateOfShock>()
            .DeactivateOnExit<HighestStakes>();

        Cast(id + 0x1000u, AID.SweepTheLegBoss, 1.9f, 4f, "Wide cone")
            .ActivateOnEnter<SweepTheLegBoss>()
            .DeactivateOnExit<SweepTheLegBoss>();
    }

    private void UnrelentingAnguish2(uint id, float delay)
    {
        Cast(id, AID.UnrelentingAnguish, delay, 3f, "Orbs start");
        StormPulseDouble(id + 0x10u, 2.1f);

        ComponentCondition<GaleForce>(id + 0x20u, 2f, static comp => comp.CurrentBaits.Count > 0)
            .ActivateOnEnter<GaleForce>();
        ComponentCondition<OminousWind>(id + 0x21u, 6.9f, static comp => comp.Targets.Any())
            .ActivateOnEnter<VacuumClaw>()
            .ActivateOnEnter<OminousWind>();
        ComponentCondition<GaleForce>(id + 0x22u, 1.2f, static comp => comp.NumCasts > 0, "Baits")
            .DeactivateOnExit<GaleForce>();

        Cast(id + 0x30u, AID.FireAndLightningBoss, 3.4f, 4f, "Line")
            .ActivateOnEnter<FireAndLightningBoss>()
            .DeactivateOnExit<FireAndLightningBoss>();
        ComponentCondition<OminousWind>(id + 0x40u, 1.5f, static comp => comp.Targets.None(), "Orbs end")
            .DeactivateOnExit<OminousWind>();

        Cast(id + 0x50u, AID.FireAndLightningBoss, 3.4f, 4f, "Line")
            .ActivateOnEnter<FireAndLightningBoss>()
            .DeactivateOnExit<FireAndLightningBoss>();
        ComponentCondition<VacuumClaw>(id + 0x60u, 0.7f, static comp => !comp.Active, "Voidzones end")
            .DeactivateOnExit<VacuumClaw>();
    }

    private void Hakutei2(uint id, float delay)
    {
        ActorTargetable(id, _module.Hakutei, true, delay, "Tiger appears")
            .ActivateOnEnter<AratamaPuddleBait>(); // icons appear ~2s before tiger

        ComponentCondition<AratamaPuddleBait>(id + 0x10u, 3.1f, static comp => comp.NumFinishedSpreads > 0, "Puddle baits start")
            .ActivateOnEnter<AratamaPuddleVoidzone>();
        ActorTargetable(id + 0x20u, _module.Hakutei, false, 6f, "Tiger disappears");

        ComponentCondition<WhiteHerald>(id + 0x30u, 0.7f, static comp => comp.Active)
            .ActivateOnEnter<WhiteHerald>();
        CastStart(id + 0x31u, AID.DistantClap, 2.4f);
        ComponentCondition<WhiteHerald>(id + 0x32u, 2.7f, static comp => comp.NumFinishedSpreads > 0, "Flare")
            .ActivateOnEnter<DistantClap>()
            .DeactivateOnExit<WhiteHerald>();
        ActorTargetable(id + 0x33u, _module.Hakutei, true, 2f, "Tiger reappears");
        CastEnd(id + 0x34u, 0.3f, "Donut 1")
            .DeactivateOnExit<DistantClap>();

        CastStart(id + 0x40u, AID.DistantClap, 3.2f)
            .ActivateOnEnter<FireAndLightningAdd>();
        ComponentCondition<FireAndLightningAdd>(id + 0x41u, 0.7f, static comp => comp.NumCasts > 0, "Line 1")
            .ActivateOnEnter<DistantClap>()
            .DeactivateOnExit<FireAndLightningAdd>();
        CastEnd(id + 0x42u, 4.3f, "Donut 2")
            .DeactivateOnExit<DistantClap>();
        ComponentCondition<FireAndLightningAdd>(id + 0x50u, 4f, static comp => comp.NumCasts > 0, "Line 2")
            .ActivateOnEnter<FireAndLightningAdd>()
            .DeactivateOnExit<FireAndLightningAdd>();

        HeavenlyStrike(id + 0x100u, 3.2f)
            .ActivateOnEnter<SteelClaw>()
            .DeactivateOnExit<SteelClaw>();

        ActorCastStart(id + 0x200u, _module.Hakutei, AID.RoarOfThunder, 10.6f, false)
            .ActivateOnEnter<VoiceOfThunder>()
            .DeactivateOnExit<AratamaPuddleVoidzone>();
        StormPulseDouble(id + 0x210u, 5.6f);
        ActorCastEnd(id + 0x220u, _module.Hakutei, 8.2f, false, "Add enrage")
            .DeactivateOnExit<VoiceOfThunder>()
            .SetHint(StateMachine.StateHint.Raidwide);

        Cast(id + 0x300u, AID.FireAndLightningBoss, 8.3f, 4f, "Line")
            .ActivateOnEnter<FireAndLightningBoss>()
            .DeactivateOnExit<FireAndLightningBoss>();
    }

    private void HundredfoldHavoc2(uint id, float delay)
    {
        ComponentCondition<GaleForce>(id, delay, static comp => comp.CurrentBaits.Count > 0)
            .ActivateOnEnter<GaleForce>();

        ComponentCondition<HundredfoldHavoc>(id + 0x10u, 2.2f, static comp => comp.Active)
            .ActivateOnEnter<HundredfoldHavoc>();
        CastStart(id + 0x11u, AID.StateOfShock, 4.6f);
        ComponentCondition<HundredfoldHavoc>(id + 0x12u, 0.4f, static comp => comp.NumCasts > 0, "Exaflares start");
        ComponentCondition<GaleForce>(id + 0x13u, 0.9f, static comp => comp.NumCasts > 0, "Baits")
            .ActivateOnEnter<VacuumClaw>()
            .DeactivateOnExit<GaleForce>();
        CastEnd(id + 0x14u, 2.7f);

        ComponentCondition<StateOfShock>(id + 0x20u, 0.9f, static comp => comp.NumStuns > 0, "Grab tank")
            .ActivateOnEnter<StateOfShock>()
            .ActivateOnEnter<HighestStakes>();
        Cast(id + 0x30u, AID.HighestStakes, 1.3f, 5f)
            .DeactivateOnExit<HundredfoldHavoc>();
        ComponentCondition<HighestStakes>(id + 0x40u, 0.8f, static comp => comp.NumCasts > 0, "Tower 1")
            .DeactivateOnExit<StateOfShock>();
        ComponentCondition<StateOfShock>(id + 0x50u, 2f, static comp => comp.NumCasts > 0)
            .ActivateOnEnter<StateOfShock>();
        ComponentCondition<StateOfShock>(id + 0x51u, 0.9f, static comp => comp.NumStuns > 0, "Grab tank");
        Cast(id + 0x60u, AID.HighestStakes, 1.2f, 5f)
            .DeactivateOnExit<VacuumClaw>();
        ComponentCondition<HighestStakes>(id + 0x70u, 0.8f, static comp => comp.NumCasts > 1, "Tower 2")
            .DeactivateOnExit<StateOfShock>()
            .DeactivateOnExit<HighestStakes>();

        Cast(id + 0x1000u, AID.SweepTheLegBoss, 1.9f, 4f, "Wide cone")
            .ActivateOnEnter<SweepTheLegBoss>()
            .DeactivateOnExit<SweepTheLegBoss>();
    }
}
