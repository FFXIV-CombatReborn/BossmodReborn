namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

sealed class Ex7DoomtrainStates : StateMachineBuilder
{
    public Ex7DoomtrainStates(BossModule module) : base(module)
    {
        var aether = Module.Enemies((uint)OID.Aether);
        SimplePhase(default, Phase1, "Cars 1-3 + intermission")
            .ActivateOnEnter<ArenaChanges>()
            .Raw.Update = () =>
            {
                var count = aether.Count;
                for (var i = 0; i < count; ++i)
                {
                    if (aether[i].IsDead)
                    {
                        return true;
                    }
                }
                return false;
            };
        DeathPhase(1u, Phase2);
    }

    private void Phase1(uint id)
    {
        Car1(id, 8.2f);
        Car2(id + 0x10000u, 2.2f);
        Car3p1(id + 0x20000u, 5.8f);
        Intermission(id + 0x30000u, 10f);
    }

    private void Phase2(uint id)
    {
        ComponentCondition<RunawayTrain>(id, 4.3f, static comp => comp.Activation != default)
            .ActivateOnEnter<RunawayTrain>()
            .SetHint(StateMachine.StateHint.DowntimeStart);
        ComponentCondition<RunawayTrain>(id + 1u, 15.2f, static comp => comp.NumCasts != 0, "Raidwide")
            .DeactivateOnExit<RunawayTrain>()
            .ExecOnExit<ArenaChanges>(static comp => comp.SetCar(3u)) // restore arena bounds
            .SetHint(StateMachine.StateHint.Raidwide);
        Car3p2(id + 0x10u, 5.1f);
        Car4(id + 0x10000u, 5.2f);
        Car5(id + 0x20000u, 18.3f);
        Car6(id + 0x30000u, 10.6f);
    }

    private void Car1(uint id, float delay)
    {
        CastMulti(id, [AID.DeadMansOverdraughtSpread, AID.DeadMansOverdraughtStack], delay, 4f, "Select spread/stack")
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
        CastMulti(id + 0x40u, [AID.DeadMansOverdraughtSpread, AID.DeadMansOverdraughtStack], 2f, 4f, "Select spread/stack");
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
        CastMulti(id, [AID.DeadMansOverdraughtSpread, AID.DeadMansOverdraughtStack], delay, 4f, "Select spread/stack 1")
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
        CastMulti(id + 0x40u, [AID.DeadMansOverdraughtSpread, AID.DeadMansOverdraughtStack], 2f, 4f, "Select spread/stack 2")
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
    }

    private void Car3p1(uint id, float delay)
    {
        Targetable(id, true, delay, "Boss targetable")
            .ActivateOnEnter<LightningBurst>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ComponentCondition<LightningBurst>(id + 0x10u, 2.6f, static comp => comp.NumCasts != 0, "Tankbusters")
            .DeactivateOnExit<LightningBurst>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        Cast(id + 0x20u, AID.RunawayTrainVisual1, 8.5f, 5f, "Runaway Train Phase Transition");
        Targetable(id + 0x30u, false, 0.2f, "Boss untargetable")
            .SetHint(StateMachine.StateHint.DowntimeStart);
    }

    private void Intermission(uint id, float delay)
    {
        Timeout(id + 0x10u, 75f + delay, "Orb enrage")
            .ActivateOnEnter<AetherocharAetherosote>()
            .ActivateOnEnter<AetherialRay>();
    }

    private void Car3p2(uint id, float delay)
    {
        Timeout(id, delay, "Boss reappears")
            .ActivateOnEnter<Shockwave>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ComponentCondition<Shockwave>(id + 0x10u, 13.1f, static comp => comp.NumCasts != 0, "Raidwide")
            .ActivateOnEnter<LightningBurst>()
            .DeactivateOnExit<Shockwave>()
            .SetHint(StateMachine.StateHint.Raidwide);
        Tankbusters(id + 0x20u, 10f, false);
        TowersAndDerail(id + 0x30u, 8.6f, 2.9f, 3, 5.2f);
    }

    private void Car4(uint id, float delay)
    {
        ComponentCondition<ZoomCounter>(id, delay, static comp => comp.NumCasts != 0)
            .ActivateOnEnter<ZoomCounter>()
            .DeactivateOnExit<ZoomCounter>();
        ThirdRail(id + 0x10u, 10.1f);
        HeadlightBreath(id + 0x20u, 10.2f);
        Cast(id + 0x100u, AID.ArcaneRevelation, 3.5f, 2f, "Arcane Revelation start")
            .ActivateOnEnter<HailOfThunder>()
            .ActivateOnEnter<DesignatedConductor>();
        ComponentCondition<HailOfThunder>(id + 0x110u, 39.8f, static comp => comp.NumCasts >= 3, "Arcane Revelation end")
            .DeactivateOnExit<HailOfThunder>()
            .DeactivateOnExit<DesignatedConductor>();
        HeadlightBreath(id + 0x200u, 12.8f);
        ThirdRail(id + 0x210u, 6.3f);
        Tankbusters(id + 0x220u, 8.6f);
        TowersAndDerail(id + 0x230u, 10.7f, 3.9f, 4, 6.2f);
    }

    private void Car5(uint id, float delay)
    {
        ComponentCondition<Plummet>(id, delay, static comp => comp.NumFinishedSpreads >= 2, "Spreads 1")
            .ActivateOnEnter<Plummet>();
        ThirdRail(id + 0x10u, 5f);
        ComponentCondition<Plummet>(id + 0x20u, 10.1f, static comp => comp.NumFinishedSpreads >= 5, "Spreads 2");
        Tankbusters(id + 0x30u, 6.6f);
        ComponentCondition<Plummet>(id + 0x40u, 9.6f, static comp => comp.NumFinishedSpreads >= 10, "Spreads 3")
            .DeactivateOnExit<Plummet>();
        ThirdRail(id + 0x50u, 5f);
        TowersAndDerail(id + 0x60u, 12.3f, 4.9f, 5, 5.6f);
    }

    private void Car6(uint id, float delay)
    {
        CastMulti(id, [AID.DeadMansOverdraughtSpread, AID.DeadMansOverdraughtStack], delay, 4f, "Select spread/stack 1")
            .ActivateOnEnter<DeadMansOverdraught>()
            .ExecOnEnter<DeadMansOverdraught>(static comp => comp.AutoActivate = true)
            .ActivateOnEnter<PlasmaBeam>();
        Cast(id + 0x10u, AID.TurretCrossing, 2.1f, 3f)
            .ActivateOnEnter<ElectrayShort>()
            .ActivateOnEnter<ElectrayMedium>()
            .ActivateOnEnter<ElectrayLong>()
            .ActivateOnEnter<Breathlight>()
            .ExecOnEnter<Breathlight>(static comp => comp.Draw = false);
        Lasers(id + 0x20u, 9.2f, "Lasers 1")
            .ExecOnExit<Breathlight>(static comp => comp.Draw = true);
        HeadlightBreath(id + 0x21u, 4f, false);
        Lasers(id + 0x30u, 9.5f, "Lasers 2 + crates disappear")
            .ActivateOnEnter<Plummet>();
        ComponentCondition<Plummet>(id + 0x40u, 3.8f, static comp => comp.NumFinishedSpreads != 0, "Spreads")
            .DeactivateOnExit<Plummet>();
        ThirdRail(id + 0x50u, 5f);
        Lasers(id + 0x60u, 8.3f, "Lasers 3")
            .ActivateOnEnter<DeadMansBlastpipe>()
            .ActivateOnEnter<DeadMansWindpipe>()
            .ActivateOnEnter<DeadMansExpress>();
        ComponentCondition<DeadMansOverdraught>(id + 0x62u, 6.9f, static comp => comp.Counter >= 1u, "Spread/stack resolves 1", 100f);
        Tankbusters(id + 0x70u, 7.5f);
        CastMulti(id + 0x80u, [AID.DeadMansOverdraughtSpread, AID.DeadMansOverdraughtStack], 3.5f, 4f, "Select spread/stack 2");
        Cast(id + 0x100u, AID.ArcaneRevelation, 2.1f, 2f, "Short Arcane Revelation start")
            .ActivateOnEnter<HailOfThunder>()
            .ActivateOnEnter<DesignatedConductor>();
        ComponentCondition<HailOfThunder>(id + 0x110u, 25.7f, static comp => comp.NumCasts >= 2, "Short Arcane Revelation end")
            .DeactivateOnExit<HailOfThunder>()
            .DeactivateOnExit<DesignatedConductor>();
        ThirdRail(id + 0x120u, 6.6f);
        ComponentCondition<DeadMansOverdraught>(id + 0x131u, 14.9f, static comp => comp.Counter >= 2u, "Spread/stack resolves 2", 100f)
            .DeactivateOnExit<DeadMansOverdraught>()
            .DeactivateOnExit<DeadMansWindpipe>()
            .DeactivateOnExit<DeadMansBlastpipe>()
            .DeactivateOnExit<DeadMansExpress>()
            .DeactivateOnExit<ElectrayShort>()
            .DeactivateOnExit<ElectrayMedium>()
            .DeactivateOnExit<ElectrayLong>()
            .DeactivateOnExit<PlasmaBeam>();
        HeadlightBreath(id + 0x200u, 9.1f);
        Tankbusters(id + 0x210u, 9f);
        ThirdRail(id + 0x220u, 5.5f);
        ComponentCondition<DerailmentSiege>(id + 0x300u, 11f, static comp => comp.NumCasts != 0, "Tower hit 1")
            .ActivateOnEnter<DerailmentSiege>();
        ComponentCondition<DerailmentSiege>(id + 0x310u, 5.9f, static comp => comp.NumCasts > 5, "Tower hit 6")
            .DeactivateOnExit<DerailmentSiege>();
        Cast(id + 0x400u, AID.DerailEnrageVisual, 0.2f, 15.1f, "Enrage");
    }

    private State Tankbusters(uint id, float delay, bool activate = true)
    {
        return ComponentCondition<LightningBurst>(id, delay, static comp => comp.NumCasts != 0, "Tankbusters")
            .ActivateOnEnter<LightningBurst>(activate)
            .DeactivateOnExit<LightningBurst>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void TowersAndDerail(uint id, float delay, float towerDuration, int lastHit, float derailDelay)
    {
        ComponentCondition<DerailmentSiege>(id, delay, static comp => comp.NumCasts != 0, "Tower hit 1")
            .ActivateOnEnter<Launchpad>()
            .ActivateOnEnter<Derail>()
            .ActivateOnEnter<DerailmentSiege>();
        ComponentCondition<DerailmentSiege>(id + 1u, towerDuration, comp => comp.NumCasts >= lastHit, $"Tower hit {lastHit}");
        ComponentCondition<Derail>(id + 2u, derailDelay, static comp => comp.NumCasts != 0, "Derail + next car")
            .DeactivateOnExit<Launchpad>()
            .DeactivateOnExit<DerailmentSiege>()
            .DeactivateOnExit<Derail>()
            .ExecOnExit<ArenaChanges>(static comp => comp.NextCar());
    }

    private State ThirdRail(uint id, float delay)
    {
        return ComponentCondition<ThirdRail>(id, delay, static comp => comp.NumCasts != 0, "Puddles")
            .ActivateOnEnter<ThirdRailBait>()
            .ActivateOnEnter<ThirdRail>()
            .DeactivateOnExit<ThirdRailBait>()
            .DeactivateOnExit<ThirdRail>();
    }

    private void HeadlightBreath(uint id, float delay, bool activate = true)
    {
        ComponentCondition<Breathlight>(id, delay, static comp => comp.NumCasts != 0, "Headlight/breath 1")
            .ActivateOnEnter<Breathlight>(activate);
        ComponentCondition<Breathlight>(id + 1u, 2.5f, static comp => comp.NumCasts > 1, "Headlight/breath 2")
            .DeactivateOnExit<Breathlight>();
    }

    private State Lasers(uint id, float delay, string name)
    {
        bool AnyDone() => (Module.FindComponent<ElectrayLong>()?.NumCasts ?? 0) + (Module.FindComponent<ElectrayMedium>()?.NumCasts ?? 0) + (Module.FindComponent<ElectrayShort>()?.NumCasts ?? 0) != 0;
        return Condition(id, delay, AnyDone, name)
            .ExecOnExit<ElectrayLong>(static comp => comp.NumCasts = 0)
            .ExecOnExit<ElectrayMedium>(static comp => comp.NumCasts = 0)
            .ExecOnExit<ElectrayShort>(static comp => comp.NumCasts = 0);
    }
}
