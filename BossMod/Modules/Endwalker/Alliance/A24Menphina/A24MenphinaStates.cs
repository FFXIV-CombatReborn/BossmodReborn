namespace BossMod.Endwalker.Alliance.A24Menphina;

sealed class A24MenphinaStates : StateMachineBuilder
{
    public A24MenphinaStates(BossModule module) : base(module)
    {
        DeathPhase(0u, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        BlueMoon(id, 7.2f, false);
        LovesLightNormalOne(id + 0x10000u, 3.8f);
        MidnightFrost(id + 0x20000u, 4.7f);
        LunarKiss(id + 0x30000u, 5.4f, false);
        SilverMirrorNormalMoonsetWinterHalo(id + 0x40000u, 3.8f);
        LovesLightNormalFourMidnightFrost(id + 0x50000u, 5.5f);
        SelenainMysteria(id + 0x60000u, 7.3f);
        MidnightFrostWaxingClaw(id + 0x100000u, 5.4f);
        PlayfulOrbitMidnightFrostWaxingClaw(id + 0x110000u, 6.2f);
        BlueMoon(id + 0x120000u, 8.1f, true);
        KeenMoonbeamMidnightFrostWaxingClaw(id + 0x130000u, 3.9f);
        CrateringChill(id + 0x140000u, 3.9f);
        MoonsetRays(id + 0x150000u, 6.0f);
        LovesLightMountedFour(id + 0x160000u, 5.7f);
        SilverMirrorMounted(id + 0x170000u, 2.0f);
        LovesLightMountedOneMidnightFrost(id + 0x180000u, 7.2f);
        MoonsetRays(id + 0x190000u, 6.0f);
        LunarKiss(id + 0x1A0000u, 7.6f, true);
        LovesLightMountedFourMidnightFrost(id + 0x1B0000u, 3.9f);
        BlueMoon(id + 0x1C0000u, 0.7f, true);
        LunarKiss(id + 0x1D0000u, 2.8f, true);
        KeenMoonbeamMidnightFrostWaxingClaw(id + 0x1E0000u, 3.0f);
        //CrateringChill(id + 0x1F0000, 3.9f); // not sure, didn't see beyond first visual cast...
        SimpleState(id + 0xFF0000u, 100f, "???");
    }

    private void BlueMoon(uint id, float delay, bool mounted)
    {
        Cast(id, mounted ? AID.BlueMoonMounted : AID.BlueMoonNormal, delay, 5);
        ComponentCondition<BlueMoon>(id + 2u, 0.9f, static comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<BlueMoon>()
            .DeactivateOnExit<BlueMoon>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void LunarKiss(uint id, float delay, bool mounted)
    {
        CastStart(id, mounted ? AID.LunarKissMounted : AID.LunarKissNormal, delay)
            .ActivateOnEnter<LunarKiss>();
        CastEnd(id + 1u, 7);
        ComponentCondition<LunarKiss>(id + 2u, 0.9f, static comp => comp.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<LunarKiss>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void MoonsetRays(uint id, float delay)
    {
        Cast(id, AID.MoonsetRays, delay, 5f, "Stack")
            .ActivateOnEnter<MoonsetRays>()
            .DeactivateOnExit<MoonsetRays>();
    }

    private void MidnightFrost(uint id, float delay)
    {
        CastMulti(id, [AID.MidnightFrostShortNormalFront, AID.MidnightFrostShortNormalBack], delay, 6)
            .ActivateOnEnter<MidnightFrostWaxingClaw>();
        ComponentCondition<MidnightFrostWaxingClaw>(id + 2u, 0.2f, static comp => comp.NumCasts > 0, "Half-arena cleave")
            .DeactivateOnExit<MidnightFrostWaxingClaw>();
    }

    private void MidnightFrostWaxingClaw(uint id, float delay)
    {
        CastMulti(id, [AID.MidnightFrostLongMountedFrontRight, AID.MidnightFrostLongMountedFrontLeft, AID.MidnightFrostLongMountedBackRight, AID.MidnightFrostLongMountedBackLeft], delay, 8)
            .ActivateOnEnter<MidnightFrostWaxingClaw>();
        ComponentCondition<MidnightFrostWaxingClaw>(id + 2u, 0.2f, static comp => comp.NumCasts > 0, "Double cleave")
            .DeactivateOnExit<MidnightFrostWaxingClaw>();
    }

    private void PlayfulOrbitMidnightFrostWaxingClaw(uint id, float delay)
    {
        CastMulti(id, [AID.PlayfulOrbit1, AID.PlayfulOrbit2], delay, 2.6f);
        CastMulti(id + 0x10, [AID.MidnightFrostLongDismounted1FrontRight, AID.MidnightFrostLongDismounted1FrontLeft, AID.MidnightFrostLongDismounted1BackRight, AID.MidnightFrostLongDismounted1BackLeft, AID.MidnightFrostLongDismounted2FrontRight, AID.MidnightFrostLongDismounted2FrontLeft, AID.MidnightFrostLongDismounted2BackRight, AID.MidnightFrostLongDismounted2BackLeft], 2.2f, 8)
            .ActivateOnEnter<MidnightFrostWaxingClaw>();
        ComponentCondition<MidnightFrostWaxingClaw>(id + 0x12u, 0.2f, static comp => comp.NumCasts > 0, "Double cleave")
            .DeactivateOnExit<MidnightFrostWaxingClaw>();
    }

    private void KeenMoonbeamMidnightFrostWaxingClaw(uint id, float delay)
    {
        Cast(id, AID.KeenMoonbeam, delay, 3f);
        ComponentCondition<KeenMoonbeam>(id + 2u, 1.5f, static comp => comp.Active)
            .ActivateOnEnter<KeenMoonbeam>();
        CastStartMulti(id + 0x10u, [AID.MidnightFrostLongMountedFrontRight, AID.MidnightFrostLongMountedFrontLeft, AID.MidnightFrostLongMountedBackRight, AID.MidnightFrostLongMountedBackLeft], 2.9f);
        ComponentCondition<KeenMoonbeam>(id + 0x11u, 2.1f, static comp => !comp.Active, "Spreads")
            .ActivateOnEnter<MidnightFrostWaxingClaw>()
            .DeactivateOnExit<KeenMoonbeam>();
        CastEnd(id + 0x12u, 5.9f);
        ComponentCondition<MidnightFrostWaxingClaw>(id + 0x13u, 0.2f, static comp => comp.NumCasts > 0, "Double cleave")
            .DeactivateOnExit<MidnightFrostWaxingClaw>();
    }

    private void CrateringChill(uint id, float delay)
    {
        Cast(id, AID.CrateringChill, delay, 3f);
        ComponentCondition<CrateringChill>(id + 2u, 1.5f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<CrateringChill>();
        Cast(id + 0x10, AID.WinterSolstice, 2.7f, 3)
            .ActivateOnEnter<ThinIce>();
        ComponentCondition<CrateringChill>(id + 0x20u, 0.3f, static comp => comp.NumCasts > 0, "Proximity + icy floor")
            .DeactivateOnExit<CrateringChill>();
        CastMulti(id + 0x30u, [AID.PlayfulOrbit1, AID.PlayfulOrbit2], 1.6f, 2.6f);
        CastMulti(id + 0x40u, [AID.WinterHaloLongDismounted1Right, AID.WinterHaloLongDismounted1Left, AID.WinterHaloLongDismounted2Right, AID.WinterHaloLongDismounted2Left], 2.2f, 8)
            .ActivateOnEnter<MidnightFrostWaxingClaw>()
            .ActivateOnEnter<WinterHalo>();
        ComponentCondition<WinterHalo>(id + 0x42u, 0.2f, static comp => comp.NumCasts > 0, "Donut + half-arena cleave")
            .DeactivateOnExit<MidnightFrostWaxingClaw>()
            .DeactivateOnExit<WinterHalo>()
            .DeactivateOnExit<ThinIce>();
    }

    private void LovesLightNormalOne(uint id, float delay)
    {
        Cast(id, AID.LovesLightNormalOne, delay, 3);
        // +0.9s: envcontrol .0E=00020001 = N moon
        Cast(id + 0x10u, AID.FullBrightNormal, 2.7f, 3);
        ComponentCondition<FirstBlush>(id + 0x20u, 0.9f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<FirstBlush>();
        ComponentCondition<FirstBlush>(id + 0x21u, 10.5f, static comp => comp.NumCasts > 0, "Line through center")
            .DeactivateOnExit<FirstBlush>();
    }

    private void LovesLightMountedOneMidnightFrost(uint id, float delay)
    {
        Cast(id, AID.LovesLightMountedOne, delay, 3f);
        // +0.8s: envcontrol .11=00020001 = SW moon
        Cast(id + 0x10u, AID.FullBrightMounted, 1.6f, 3);
        ComponentCondition<FirstBlush>(id + 0x20u, 0.8f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<FirstBlush>();
        CastMulti(id + 0x30u, [AID.PlayfulOrbit1, AID.PlayfulOrbit2], 1.9f, 2.6f);
        CastStartMulti(id + 0x40u, [AID.MidnightFrostLongDismounted1FrontRight, AID.MidnightFrostLongDismounted1FrontLeft, AID.MidnightFrostLongDismounted1BackRight, AID.MidnightFrostLongDismounted1BackLeft, AID.MidnightFrostLongDismounted2FrontRight, AID.MidnightFrostLongDismounted2FrontLeft, AID.MidnightFrostLongDismounted2BackRight, AID.MidnightFrostLongDismounted2BackLeft], 2.2f);
        ComponentCondition<FirstBlush>(id + 0x50u, 3.8f, static comp => comp.NumCasts > 0, "Line through center")
            .ActivateOnEnter<MidnightFrostWaxingClaw>()
            .DeactivateOnExit<FirstBlush>();
        CastEnd(id + 0x60u, 4.2f);
        ComponentCondition<MidnightFrostWaxingClaw>(id + 0x70u, 0.2f, static comp => comp.NumCasts > 0, "Double cleave")
            .DeactivateOnExit<MidnightFrostWaxingClaw>();
    }

    private void LovesLightNormalFourMidnightFrost(uint id, float delay)
    {
        Cast(id, AID.LovesLightNormalFour, delay, 3f);
        // +0.9s: envcontrol .17/19/1A/1C=00020001 = N/S -> E/W moons
        Cast(id + 0x10u, AID.FullBrightNormal, 2.7f, 3);
        ComponentCondition<LoversBridgeShort>(id + 0x20u, 0.9f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<LoversBridgeShort>();
        ComponentCondition<LoversBridgeShort>(id + 0x21u, 6f, static comp => comp.NumCasts > 0, "Moons 1")
            .DeactivateOnExit<LoversBridgeShort>();
        CastStartMulti(id + 0x30u, [AID.MidnightFrostShortNormalFront, AID.MidnightFrostShortNormalBack], 5)
            .ActivateOnEnter<LoversBridgeLong>();
        ComponentCondition<LoversBridgeLong>(id + 0x31u, 1, static comp => comp.NumCasts > 0, "Moons 2")
            .ActivateOnEnter<MidnightFrostWaxingClaw>()
            .DeactivateOnExit<LoversBridgeLong>();
        CastEnd(id + 0x32u, 5f);
        ComponentCondition<MidnightFrostWaxingClaw>(id + 0x33u, 0.2f, static comp => comp.NumCasts > 0, "Half-arena cleave")
            .DeactivateOnExit<MidnightFrostWaxingClaw>();
    }

    private void LovesLightMountedFour(uint id, float delay)
    {
        Cast(id, AID.LovesLightMountedFour, delay, 3f);
        // +0.7s: envcontrol .16/18/1B/1D=00020001 = E/W -> N/S moons
        Cast(id + 0x10u, AID.FullBrightMounted, 1.6f, 3);
        ComponentCondition<LoversBridgeShort>(id + 0x20u, 0.8f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<LoversBridgeShort>();
        ComponentCondition<LoversBridgeShort>(id + 0x21u, 6f, static comp => comp.NumCasts > 0, "Moons 1")
            .DeactivateOnExit<LoversBridgeShort>();
        ComponentCondition<LoversBridgeLong>(id + 0x22u, 6f, static comp => comp.NumCasts > 0, "Moons 2")
            .ActivateOnEnter<LoversBridgeLong>()
            .DeactivateOnExit<LoversBridgeLong>();
    }

    private void LovesLightMountedFourMidnightFrost(uint id, float delay)
    {
        Cast(id, AID.LovesLightMountedFour, delay, 3f);
        // +0.7s: envcontrol .17/19/1A/1C=00020001 = N/S -> E/W moons
        Cast(id + 0x10u, AID.FullBrightMounted, 1.6f, 3);
        ComponentCondition<LoversBridgeShort>(id + 0x20u, 0.8f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<LoversBridgeShort>();
        CastStartMulti(id + 0x30u, [AID.MidnightFrostShortMountedFront, AID.MidnightFrostShortMountedBack], 3.1f);
        ComponentCondition<LoversBridgeShort>(id + 0x40u, 2.9f, static comp => comp.NumCasts > 0, "Moons 1")
            .ActivateOnEnter<MidnightFrostWaxingClaw>()
            .DeactivateOnExit<LoversBridgeShort>();
        CastEnd(id + 0x50u, 3.1f)
            .ActivateOnEnter<LoversBridgeLong>();
        ComponentCondition<MidnightFrostWaxingClaw>(id + 0x51u, 0.2f, static comp => comp.NumCasts > 0, "Half-arena cleave")
            .DeactivateOnExit<MidnightFrostWaxingClaw>();
        ComponentCondition<LoversBridgeLong>(id + 0x60u, 2.7f, static comp => comp.NumCasts > 0, "Moons 2")
            .DeactivateOnExit<LoversBridgeLong>();
    }

    private void SilverMirrorNormalMoonsetWinterHalo(uint id, float delay)
    {
        Cast(id, AID.SilverMirrorNormal, delay, 4f, "Puddles")
            .ActivateOnEnter<SilverMirror>();
        Cast(id + 0x10u, AID.Moonset, 2.7f, 4f)
            .ActivateOnEnter<Moonset>()
            .DeactivateOnExit<SilverMirror>(); // last puddle ends ~1.3s into cast
        ComponentCondition<Moonset>(id + 0x20u, 1f, static comp => comp.NumCasts >= 1, "Jump 1");
        ComponentCondition<Moonset>(id + 0x21u, 2.2f, static comp => comp.NumCasts >= 2, "Jump 2");
        ComponentCondition<Moonset>(id + 0x22u, 2.2f, static comp => comp.NumCasts >= 3, "Jump 3")
            .DeactivateOnExit<Moonset>();
        Cast(id + 0x30u, AID.WinterHaloShort, 1.5f, 5f)
            .ActivateOnEnter<WinterHalo>();
        ComponentCondition<WinterHalo>(id + 0x32f, 0.3f, static comp => comp.NumCasts > 0, "Donut")
            .DeactivateOnExit<WinterHalo>();
    }

    private void SilverMirrorMounted(uint id, float delay)
    {
        Cast(id, AID.SilverMirrorMounted, delay, 4f, "Puddles")
            .ActivateOnEnter<SilverMirror>();
        CastMulti(id + 0x10u, [AID.MidnightFrostLongMountedFrontRight, AID.MidnightFrostLongMountedFrontLeft, AID.MidnightFrostLongMountedBackRight, AID.MidnightFrostLongMountedBackLeft, AID.WinterHaloLongMountedRight, AID.WinterHaloLongMountedLeft], 1.7f, 8)
            .ActivateOnEnter<MidnightFrostWaxingClaw>()
            .ActivateOnEnter<WinterHalo>()
            .DeactivateOnExit<SilverMirror>(); // last puddle ends ~2.3s into cast
        ComponentCondition<MidnightFrostWaxingClaw>(id + 0x12u, 0.2f, static comp => comp.NumCasts > 0, "Donut + half-arena cleave / double cleave")
            .DeactivateOnExit<MidnightFrostWaxingClaw>()
            .DeactivateOnExit<WinterHalo>();
    }

    private void SelenainMysteria(uint id, float delay)
    {
        Cast(id, AID.SelenainMysteria, delay, 3f, "Boss disappears")
            .SetHint(StateMachine.StateHint.DowntimeStart);
        ComponentCondition<CeremonialPillar>(id + 0x10u, 4.5f, static comp => comp.ActiveActorsCount != 0, "Adds appear")
            .ActivateOnEnter<CeremonialPillar>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ComponentCondition<CeremonialPillar>(id + 0x100u, 100f, static comp => comp.ActiveActorsCount == 0, "Adds enrage")
            .ActivateOnEnter<AncientBlizzard>()
            .ActivateOnEnter<KeenMoonbeam>()
            .DeactivateOnExit<AncientBlizzard>()
            .DeactivateOnExit<KeenMoonbeam>()
            .DeactivateOnExit<CeremonialPillar>()
            .SetHint(StateMachine.StateHint.DowntimeStart);
        ComponentCondition<RiseOfTheTwinMoons>(id + 0x110u, 11.1f, static comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<RiseOfTheTwinMoons>()
            .DeactivateOnExit<RiseOfTheTwinMoons>()
            .SetHint(StateMachine.StateHint.Raidwide);
        Targetable(id + 0x120u, true, 3.3f, "Boss reappears");
    }
}
