namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

sealed class C013PariOfPlentyStates : StateMachineBuilder
{
    private static readonly AID[] _fireflight = [AID.FireflightByEmberlightRight, AID.FireflightByEmberlightLeft, AID.FireflightByPyrelightRight, AID.FireflightByPyrelightLeft];
    private static readonly AID[] _kindling = [AID.ScatteredKindlingCast, AID.KindledFlame1Cast];
    private static readonly AID[] _longNights = [AID.FireflightFourLongNightsE, AID.FireflightFourLongNightsW];
    private static readonly AID[] _charmdFlight = [AID.CharmdFlightFourNightsW, AID.CharmdFlightFourNightsE];

    public C013PariOfPlentyStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Fireflight1(id, 8.2f);
        KindledFlame(id + 0x10000u, 12.7f);
        FourLongNights(id + 0x20000u, 7f);
        ParisCurse(id + 0x30000u, 9.2f);
        SpurningFlames(id + 0x40000u, 6.8f);
        Doubling(id + 0x50000u, 7.2f);
        CharmedFlight(id + 0x60000u, 5.9f);
        Enrage(id + 0x70000u, 8.6f);
    }

    private void Fireflight1(uint id, float delay)
    {
        Cast(id, AID.HeatBurst, delay, 5f, "Raidwide")
            .ActivateOnEnter<HeatBurst>()
            .DeactivateOnExit<HeatBurst>();

        CastStartMulti(id + 0x10u, _fireflight, 10.1f)
            .ActivateOnEnter<FireflightPath>()
            .ActivateOnEnter<FireflightRectStart>()
            .ActivateOnEnter<EmberPyre>()
            .ActivateOnEnter<SunCirclet>();

        ComponentCondition<FireflightPath>(id + 0x20u, 12.7f, static r => r.NumCasts > 0, "Carpet 1");
        ComponentCondition<FireflightPath>(id + 0x22u, 4.1f, static r => r.NumCasts > 2, "Carpet 3");
        ComponentCondition<SunCirclet>(id + 0x23u, 3.7f, static r => r.NumCasts > 0, "Donut")
            .DeactivateOnExit<FireflightRectStart>()
            .DeactivateOnExit<FireflightPath>()
            .DeactivateOnExit<SunCirclet>();

        ComponentCondition<EmberPyre>(id + 0x30u, 1.1f, static e => e.NumCasts > 0, "Stack/spread")
            .DeactivateOnExit<EmberPyre>();
    }

    private void KindledFlame(uint id, float delay)
    {
        Timeout(id, default)
            .ActivateOnEnter<WheelRectStart>()
            .ActivateOnEnter<WheelOfFireflight>();

        CastMulti(id + 0x10u, _kindling, delay, 8f)
            .ActivateOnEnter<Kindling>();

        ComponentCondition<Kindling>(id + 0x20u, 0.1f, static k => k.NumCasts > 0, "Stack/spread")
            .DeactivateOnExit<Kindling>();
        ComponentCondition<WheelOfFireflight>(id + 0x21u, 0.1f, static w => w.NumCasts > 0, "Safe corner")
            .DeactivateOnExit<WheelOfFireflight>()
            .DeactivateOnExit<WheelRectStart>();

        Cast(id + 0x30u, AID.FireOfVictory, 5f, 5f, "Tankbuster")
            .ActivateOnEnter<FireOfVictory>()
            .DeactivateOnExit<FireOfVictory>();
    }

    private void FourLongNights(uint id, float delay)
    {
        CastStartMulti(id, _longNights, delay)
            .ActivateOnEnter<FourLongNightsSide>()
            .ActivateOnEnter<FourLongNightsBait>()
            .ActivateOnEnter<LongNightRectStart>();

        ComponentCondition<FourLongNightsBait>(id + 0x10u, 19.1f, static b => b.NumSpreads > 0, "Bait 1")
            .DeactivateOnExit<LongNightRectStart>();
        ComponentCondition<FourLongNightsBait>(id + 0x11u, 3f, static b => b.NumSpreads > 1, "Bait 2");
        ComponentCondition<FourLongNightsBait>(id + 0x12u, 3f, static b => b.NumSpreads > 2, "Bait 3");
        ComponentCondition<FourLongNightsBait>(id + 0x13u, 3f, static b => b.NumSpreads > 3, "Bait 4")
            .DeactivateOnExit<FourLongNightsSide>()
            .DeactivateOnExit<FourLongNightsBait>();
    }

    private void ParisCurse(uint id, float delay)
    {
        Cast(id, AID.ParisCurse, delay, 5f, "Raidwide")
            .ActivateOnEnter<ParisCurseRaidwide>()
            .ActivateOnEnter<ParisCurseStackSpread>()
            .ActivateOnEnter<ParisCurseSolver>()
            .ActivateOnEnter<FableflightLongRectStart>()
            .ActivateOnEnter<FableflightLong>()
            .ExecOnEnter<ParisCurseStackSpread>(static p => p.EnableHints = false)
            .DeactivateOnExit<ParisCurseRaidwide>();

        Cast(id + 0x10u, AID.CharmingBaubles, 5.2f, 3f)
            .ActivateOnEnter<BurningGleam>()
            .ActivateOnEnter<ChillingGleam>();

        Cast(id + 0x20u, AID.ThievesWeave, 2.1f, 3f);
        ComponentCondition<BurningGleam>(id + 0x30u, 9.8f, static g => g.Casters.Count > 0)
            .ExecOnExit<ParisCurseStackSpread>(static p => p.EnableHints = true);

        Condition(id + 0x100u, 9.2f, () => Module.FindComponent<ChillingGleam>()?.NumCasts > 0 && Module.FindComponent<BurningGleam>()?.NumCasts > 0 && Module.FindComponent<FableflightLong>()?.NumCasts > 0, "Carpet + crosses")
            .DeactivateOnExit<FableflightLongRectStart>()
            .DeactivateOnExit<FableflightLong>()
            .DeactivateOnExit<ChillingGleam>()
            .DeactivateOnExit<BurningGleam>();

        ComponentCondition<ParisCurseStackSpread>(id + 0x110u, 0.4f, static p => p.NumCasts > 0, "Stack + spread")
            .DeactivateOnExit<ParisCurseStackSpread>()
            .DeactivateOnExit<ParisCurseSolver>();
    }

    private void SpurningFlames(uint id, float delay)
    {
        Cast(id, AID.SpurningFlames, delay, 7f, "Raidwide")
            .ActivateOnEnter<SpurningFlames>()
            .DeactivateOnExit<SpurningFlames>();

        Cast(id + 0x10u, AID.ImpassionedSparksCast, 5.2f, 5f)
            .ActivateOnEnter<ImpassionedSparks>()
            .ActivateOnEnter<BurningPillar>();

        ComponentCondition<ImpassionedSparks>(id + 0x12u, 4.5f, static s => s.Casters.Count > 0, "Fireballs start");
        ComponentCondition<BurningPillar>(id + 0x13u, 5.4f, static b => b.Groups > 0, "First baits");
        ComponentCondition<BurningPillar>(id + 0x14u, 15f, static b => b.Groups > 3, "Last baits");

        ComponentCondition<ChainsOfPassion>(id + 0x20u, 3.7f, static c => c.TethersAssigned, "Chains appear")
            .ActivateOnEnter<HotFoot>()
            .ActivateOnEnter<ChainsOfPassion>();

        ComponentCondition<HotFoot>(id + 0x30u, 4.3f, static h => h.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<HotFoot>()
            .DeactivateOnExit<ImpassionedSparks>();

        Cast(id + 0x100u, AID.ScouringScorn, 1.3f, 6f, "Raidwide")
            .ActivateOnEnter<ScouringScorn>()
            .DeactivateOnExit<ScouringScorn>()
            .DeactivateOnExit<BurningPillar>()
            .DeactivateOnExit<ChainsOfPassion>();
    }

    private void Doubling(uint id, float delay)
    {
        Cast(id, AID.Doubling, delay, 3f);
        Targetable(id + 0x10u, false, 4.1f, "Boss disappears")
            .ActivateOnEnter<Doubling>()
            .ActivateOnEnter<FableflightRectStart>()
            .ActivateOnEnter<FableflightTether>()
            .ActivateOnEnter<CarpetRideBaited>();

        ComponentCondition<Doubling>(id + 0x20u, 10.2f, static e => e.NumCasts > 0, "Towers 1");
        ComponentCondition<CarpetRideBaited>(id + 0x21u, 0.7f, static c => c.NumCasts > 0, "Baits 1");
        ComponentCondition<Doubling>(id + 0x22u, 6.3f, static e => e.NumCasts > 4, "Towers 2")
            .DeactivateOnExit<Doubling>();
        ComponentCondition<CarpetRideBaited>(id + 0x23u, 0.7f, static c => c.NumCasts > 2, "Baits 2")
            .DeactivateOnExit<CarpetRideBaited>()
            .DeactivateOnExit<FableflightRectStart>()
            .DeactivateOnExit<FableflightTether>();

        Targetable(id + 0x30u, true, 0.4f, "Boss reappears");
    }

    private void CharmedFlight(uint id, float delay)
    {
        CastStart(id, AID.KindledFlame2Cast, delay)
            .ActivateOnEnter<BurningGleam>()
            .ActivateOnEnter<KindledFlameStack>();

        ComponentCondition<KindledFlameStack>(id + 0x10u, 5.1f, static k => k.NumFinishedStacks > 0, "Stacks")
            .DeactivateOnExit<KindledFlameStack>();
        ComponentCondition<BurningGleam>(id + 0x11u, 0.5f, static b => b.NumCasts > 0, "Crosses")
            .DeactivateOnExit<BurningGleam>();

        CastMulti(id + 0x100u, _charmdFlight, 7.3f, 17.5f)
            .ActivateOnEnter<BurningGleamSlow>()
            .ActivateOnEnter<FourLongNightsSide>()
            .ActivateOnEnter<CharmdFlightRect>();

        ComponentCondition<FourLongNightsSide>(id + 0x110u, 2.1f, static f => f.NumCasts > 0, "Side 1")
            .DeactivateOnExit<CharmdFlightRect>();
        ComponentCondition<FourLongNightsSide>(id + 0x111u, 3f, static f => f.NumCasts > 1, "Side 2");
        ComponentCondition<FourLongNightsSide>(id + 0x112u, 3f, static f => f.NumCasts > 2, "Side 3");
        ComponentCondition<FourLongNightsSide>(id + 0x113u, 3f, static f => f.NumCasts > 3, "Side 4")
            .DeactivateOnExit<FourLongNightsSide>()
            .DeactivateOnExit<BurningGleamSlow>();

        Cast(id + 0x200u, AID.HeatBurst, 7f, 5f, "Raidwide")
            .ActivateOnEnter<HeatBurst>()
            .DeactivateOnExit<HeatBurst>();
    }

    private void Enrage(uint id, float delay)
    {
        Cast(id, AID.SpurningFlamesPreEnrage, delay, 7f, "Raidwide")
            .ActivateOnEnter<SpurningFlamesPreEnrage>()
            .DeactivateOnExit<SpurningFlamesPreEnrage>();

        Cast(id + 0x100u, AID.ScouringScornEnrageCast, 3.2f, 7f, "Enrage");
    }
}
