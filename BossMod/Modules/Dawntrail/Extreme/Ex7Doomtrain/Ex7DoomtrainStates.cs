namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

[SkipLocalsInit]
sealed class Ex7DoomtrainStates : StateMachineBuilder
{
    bool IntermissionStarted()
    {
        intermission ??= module.FindComponent<Intermission>();
        return intermission?.Started ?? false;
    }

    public Ex7DoomtrainStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<Intermission>()
            .Raw.Update = () => module.PrimaryActor.IsDead || IntermissionStarted();
    }

    private void SinglePhase(uint id)
    {
        Car1(id, 8.2f);
        Car2(id + 0x10000u, 2.2f);
        SimpleState(id + 0xFF0000u, 10000f, "???");
    }

    private void Car1(uint id, float delay)
    {
        CastMulti(id, [(uint)AID.DeadMansOverdraughtSpread, (uint)AID.DeadMansOverdraughtStack], delay, 4f, "Select spread/stack")
            .ActivateOnEnter<DeadMansOverdraught>();
        ComponentCondition<DeadMansExpress>(id + 0x10u, 8.1f, static comp => comp.NumCasts != 0, "Knockback")
            .ActivateOnEnter<PlasmaBeam>()
            .ActivateOnEnter<DeadMansExpress>()
            .DeactivateOnExit<DeadMansExpress>()
            .ExecOnExit<DeadMansOverdraught>(static comp => comp.AddStackSpread(5.1d))
            .SetHint(StateMachine.StateHint.Knockback);
        ComponentCondition<PlasmaBeam>(id + 0x20u, 2f, static comp => comp.NumCasts != 0, "Line AOEs")
            .DeactivateOnExit<PlasmaBeam>();
        ComponentCondition<DeadMansOverdraught>(id + 0x30u, 3.1f, static comp => comp.Counter == 1u, "Spread/stack resolves");
        CastMulti(id + 0x40u, [(uint)AID.DeadMansOverdraughtSpread, (uint)AID.DeadMansOverdraughtStack], 2f, 4f, "Select spread/stack");
        ComponentCondition<DeadMansWindpipe>(id + 0x50u, 8.1f, static comp => comp.NumCasts != 0, "Pull")
            .ActivateOnEnter<DeadMansBlastpipe>()
            .ExecOnEnter<DeadMansOverdraught>(static comp => comp.AddStackSpread(5.1d))
            .ActivateOnEnter<DeadMansWindpipe>()
            .DeactivateOnExit<DeadMansWindpipe>()
            .SetHint(StateMachine.StateHint.Knockback);
        ComponentCondition<DeadMansBlastpipe>(id + 0x60u, 2f, static comp => comp.NumCasts != 0, "Rect AOE")
            .DeactivateOnExit<DeadMansBlastpipe>();
        ComponentCondition<DeadMansOverdraught>(id + 0x70u, 2.1f, static comp => comp.Counter == 2u, "Spread/stack resolves")
            .DeactivateOnExit<DeadMansOverdraught>();
        ComponentCondition<UnlimitedExpress>(id + 0x80u, 7.8f, static comp => comp.NumCasts != 0, "Raidwide")
            .ActivateOnEnter<UnlimitedExpress>()
            .DeactivateOnExit<UnlimitedExpress>()
            .SetHint(StateMachine.StateHint.Raidwide);
        Targetable(id + 0x90u, false, 0.2f, "Boss untargetable")
            .SetHint(StateMachine.StateHint.DowntimeStart);
        Targetable(id + 0xA0u, true, 5.8f, "Boss targetable")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    private void Car2(uint id, float delay)
    {
        CastMulti(id, [(uint)AID.DeadMansOverdraughtSpread, (uint)AID.DeadMansOverdraughtStack], delay, 4f, "Select spread/stack 1")
            .ActivateOnEnter<DeadMansOverdraught>();
        ComponentCondition<ElectrayLong>(id + 0x10u, 14.3f, static comp => comp.NumCasts != 0, "Line AOEs 1")
            .ActivateOnEnter<ElectrayShort>()
            .ActivateOnEnter<ElectrayMedium>()
            .ActivateOnEnter<ElectrayLong>()
            .ActivateOnEnter<PlasmaBeam>()
            .ActivateOnEnter<DeadMansBlastpipe>()
            .ActivateOnEnter<DeadMansWindpipe>()
            .ActivateOnEnter<DeadMansExpress>()
            .ExecOnExit<ElectrayLong>(static comp => comp.NumCasts = 0)
            .ExecOnExit<DeadMansOverdraught>(static comp => comp.AddStackSpread(5.9d));
        ComponentCondition<DeadMansOverdraught>(id + 0x20u, 5.9f, static comp => comp.Counter == 1u, "Spread/stack resolves 1");
        CastMulti(id + 0x40u, [(uint)AID.DeadMansOverdraughtSpread, (uint)AID.DeadMansOverdraughtStack], 2f, 4f, "Select spread/stack 2")
            .ActivateOnExit<LightningBurst>();
        ComponentCondition<ElectrayLong>(id + 0x30u, 4.6f, static comp => comp.NumCasts != 0, "Line AOEs 2")
            .ExecOnExit<ElectrayLong>(static comp => comp.NumCasts = 0);
        ComponentCondition<LightningBurst>(id + 040u, 2.6f, static comp => comp.NumCasts != 0, "Tankbusters")
            .DeactivateOnExit<LightningBurst>();
        ComponentCondition<ElectrayLong>(id + 0x50u, 8.7f, static comp => comp.NumCasts != 0, "Line AOEs 3")
            .DeactivateOnExit<ElectrayShort>()
            .DeactivateOnExit<ElectrayMedium>()
            .DeactivateOnExit<ElectrayLong>()
            .ExecOnExit<DeadMansOverdraught>(static comp => comp.AddStackSpread(5.9d));
        ComponentCondition<DeadMansOverdraught>(id + 0x60u, 5.9f, static comp => comp.Counter == 2u, "Spread/stack resolves 2")
            .DeactivateOnExit<DeadMansBlastpipe>()
            .DeactivateOnExit<DeadMansWindpipe>()
            .DeactivateOnExit<DeadMansExpress>()
            .DeactivateOnExit<PlasmaBeam>()
            .DeactivateOnExit<DeadMansOverdraught>();
        ComponentCondition<UnlimitedExpress>(id + 0x70u, 7.8f, static comp => comp.NumCasts != 0, "Raidwide")
            .ActivateOnEnter<UnlimitedExpress>()
            .DeactivateOnExit<UnlimitedExpress>()
            .SetHint(StateMachine.StateHint.Raidwide);
        Targetable(id + 0x90u, false, 0.2f, "Boss untargetable")
            .SetHint(StateMachine.StateHint.DowntimeStart);
        Targetable(id + 0xA0u, true, 5.8f, "Boss targetable")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    private void Car3p1(uint id, float delay)
    {
        ComponentCondition<LightningBurst>(id + 0x10u, 2f, static comp => comp.NumCasts != 0, "Tankbusters")
            .DeactivateOnExit<LightningBurst>();
    }

    private void Intermission(uint id, float delay)
    {
        ComponentCondition<AetherocharAethersoteStackSpread>(id + 0x10u, 5.1f, static comp => comp.NumCasts != 0, "Stack spread")
            .ActivateOnEnter<AetherocharAethersoteStackSpread>();
        ComponentCondition<AetherialRay>(id + 0x20u, 7.6f, static comp => comp.NumCasts != 0, "Tankbuster")
            .ActivateOnEnter<AetherialRay>();
        ComponentCondition<RunawayTrain>(id + 0x30u, 16f, static comp => comp.NumCasts != 0, "Raidwide")
            .DeactivateOnEnter<AetherocharAethersoteStackSpread>()
            .DeactivateOnEnter<AetherialRay>()
            .ActivateOnEnter<RunawayTrain>()
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<RunawayTrain>();
    }

    private void Car3p2(uint id, float delay)
    {
        ComponentCondition<Shockwave>(id + 0x10u, 16f, static comp => comp.NumCasts != 0, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<Shockwave>();
        ComponentCondition<LightningBurst>(id + 0x20u, 2f, static comp => comp.NumCasts != 0, "Tankbusters")
            .DeactivateOnExit<LightningBurst>();
        ComponentCondition<DerailmentSiege>(id + 0x30u, 16f, static comp => comp.NumCasts != 0, "Multi-hit tower")
            .ActivateOnEnter<DerailmentSiege>();
        // jump pad to car 4
    }

    private void Car4(uint id, float delay)
    {

    }

    private void Car5(uint id, float delay)
    {

    }

    private void Car6(uint id, float delay)
    {

    }
}
