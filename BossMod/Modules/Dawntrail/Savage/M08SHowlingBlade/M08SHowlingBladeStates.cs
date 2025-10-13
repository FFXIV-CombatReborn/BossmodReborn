namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

sealed class M08SHowlingBladeStates : StateMachineBuilder
{
    private readonly M08SHowlingBlade _module;

    public M08SHowlingBladeStates(M08SHowlingBlade module) : base(module)
    {
        _module = module;
        SimplePhase(default, Phase1, "P1")
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.PrimaryActor.HPMP.CurHP <= 1u;
        SimplePhase(1u, Phase2, "P2")
            .SetHint(StateMachine.PhaseHint.StartWithDowntime)
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && (_module.BossP2()?.IsDeadOrDestroyed ?? true);
    }

    private void Phase1(uint id)
    {
        ExtraplanarPursuit(id, 10.3f);
        WindfangStonefang1(id + 0x10000u, 9f);
        WolvesReign(id + 0x20000u, 5.2f);
        ExtraplanarPursuit(id + 0x30000u, 2.2f);
        MillennialDecay(id + 0x40000u, 8.5f);
        TrackingTremors(id + 0x50000u, 6.6f);
        ExtraplanarPursuit(id + 0x60000u, 1.9f);
        GreatDivide(id + 0x70000u, 3.8f);
        TerrestrialTitans(id + 0x80000u, 14.8f);
        WolvesReign(id + 0x90000u, 0.5f);
        TacticalPack(id + 0xA0000u, 9.2f);
        TerrestrialRage(id + 0xB0000u, 14.5f);
        WolvesReign3(id + 0xC0000u, 4.1f);
        GreatDivide(id + 0xD0000u, 5.4f);
        BeckonMoonlight(id + 0xE0000u, 11.3f);
        WindfangStonefang2(id + 0xF0000u, 3.3f);
        TrackingTremors(id + 0x100000u, 10f);
        ExtraplanarPursuit(id + 0x110000u, 1.8f);
        ExtraplanarPursuit(id + 0x120000u, 10.8f, true);
        SimpleState(id + 0x130000u, 1.5f, "Enrage (timing varies)"); // timing varies depending on length of add phase, boss becomes untargetable after 406 seconds
    }

    private void Phase2(uint id)
    {
        ActorTargetable(id, _module.BossP2, true, 45.5f, "Boss targetable")
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<Teleporters>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        QuakeIII(id + 0x10000u, 12.2f);
        UltraviolentRay(id + 0x20000u, 12.2f);
        Twinbite(id + 0x30000u, 11.2f);
        HerosBlow(id + 0x40000u, 12.2f);
        UltraviolentRay(id + 0x50000u, 10.2f);
        QuakeIII(id + 0x60000u, 11.2f);
        Mooncleaver1(id + 0x70000u, 13.2f);
        ElementalPurge(id + 0x80000u, 7.1f);
        ProwlingGaleP2(id + 0x90000u, 14f);
        TwofoldTempest(id + 0xA0000u, 11.5f);
        ChampionsCircuit(id + 0xB0000u, 18.3f);
        QuakeIII(id + 0xC0000u, 9.9f);
        UltraviolentRay(id + 0xD0000u, 14.2f);
        Twinbite(id + 0xE0000u, 11.2f);
        RiseOfTheHuntersBlade(id + 0xF0000u, 8.2f);
        HerosBlow(id + 0x100000u, 9.4f);
        UltraviolentRay(id + 0x110000u, 12.3f);
        HowlingEight(id + 0x120000u, 16f);
        SimpleState(id + 0x130000u, 11.3f, "Enrage");
    }

    private void ExtraplanarPursuit(uint id, float delay, bool last = false)
    {
        Cast(id, (uint)AID.ExtraplanarPursuitVisual, delay, 1.6f)
            .ActivateOnEnter<ExtraplanarPursuit>();
        var cond = ComponentCondition<ExtraplanarPursuit>(id + 0x10u, 2.4f, comp => comp.NumCasts != 0, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<ExtraplanarPursuit>();
        if (last)
        {
            cond
                .SetHint(StateMachine.StateHint.GroupWithNext);
        }
    }

    private void GreatDivide(uint id, float delay)
    {
        Cast(id, (uint)AID.GreatDivide, delay, 5f, "Shared tankbuster")
            .ActivateOnEnter<GreatDivide>()
            .DeactivateOnExit<GreatDivide>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void TrackingTremors(uint id, float delay)
    {
        ComponentCondition<TrackingTremors>(id, delay, comp => comp.CastCounter != 0, "Stack x8 starts")
            .ActivateOnEnter<TrackingTremors>();
        ComponentCondition<TrackingTremors>(id + 0x10u, 7.5f, comp => comp.Stacks.Count == 0, "Stack finishes")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<TrackingTremors>();
    }

    private void WindfangStonefang1(uint id, float delay)
    {
        CastStartMulti(id, [(uint)AID.WindfangCross1, (uint)AID.WindfangCross2, (uint)AID.StonefangCross1, (uint)AID.StonefangCross2], delay, "Wind-/Stonefang 1")
            .ActivateOnEnter<StonefangBait>()
            .ActivateOnEnter<WindfangBait>()
            .ActivateOnEnter<WindfangStonefang>()
            .ExecOnEnter<WindfangStonefang>(comp => comp.Draw = true);
        ComponentCondition<WindfangStonefang>(id + 0x10u, 6f, comp => comp.NumCasts != 0, "Baits + cross + circle OR donut")
            .DeactivateOnExit<StonefangBait>()
            .DeactivateOnExit<WindfangBait>()
            .DeactivateOnExit<WindfangStonefang>();
    }

    private void WolvesReign(uint id, float delay)
    {
        CastStartMulti(id, [(uint)AID.RevolutionaryReignVisual1, (uint)AID.RevolutionaryReignVisual2, (uint)AID.EminentReignVisual1, (uint)AID.EminentReignVisual2], delay, "Wolvesreign")
            .ActivateOnExit<WolvesReignConeCircle>()
            .ActivateOnExit<WolvesReignRect>()
            .ActivateOnEnter<WolvesReignCircle>();
        ComponentCondition<WolvesReignCircle>(id + 0x10u, 7f, comp => comp.NumCasts == 4, "Circles resolve")
            .DeactivateOnExit<WolvesReignCircle>()
            .ActivateOnExit<ReignsEnd>()
            .ActivateOnExit<SovereignScar>();
        ComponentCondition<WolvesReignRect>(id + 0x20u, 2.5f, comp => comp.NumCasts != 0, "Line AOE")
            .DeactivateOnExit<WolvesReignRect>();
        ComponentCondition<WolvesReignConeCircle>(id + 0x30u, 3.1f, comp => comp.NumCasts != 0, "Baits resolve + cone OR circle", 2f)
            .DeactivateOnExit<WolvesReignConeCircle>()
            .DeactivateOnExit<ReignsEnd>()
            .DeactivateOnExit<SovereignScar>();
    }

    private void MillennialDecay(uint id, float delay)
    {
        Cast(id, (uint)AID.MillennialDecay, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .ActivateOnExit<BreathOfDecay>()
            .ActivateOnExit<AeroIII>()
            .ActivateOnExit<Gust>();
        ComponentCondition<AeroIII>(id + 0x10u, 10.6f, comp => comp.NumCasts != 0, "Knockback 1");
        ComponentCondition<BreathOfDecay>(id + 0x20u, 1.5f, comp => comp.NumCasts != 0, "Line AOE 1");
        ComponentCondition<Gust>(id + 0x30u, 0.4f, comp => comp.NumFinishedSpreads == 4, "Spreads 1 resolve");
        ComponentCondition<BreathOfDecay>(id + 0x40u, 1.5f, comp => comp.NumCasts == 2, "Line AOE 2");
        ComponentCondition<BreathOfDecay>(id + 0x50u, 2f, comp => comp.NumCasts == 3, "Line AOE 3");
        ComponentCondition<Gust>(id + 0x60u, 1.5f, comp => comp.NumFinishedSpreads == 8, "Spreads 2 resolve")
            .DeactivateOnExit<Gust>();
        ComponentCondition<BreathOfDecay>(id + 0x70u, 0.5f, comp => comp.NumCasts == 4, "Line AOE 4");
        ComponentCondition<BreathOfDecay>(id + 0x80u, 2f, comp => comp.NumCasts == 5, "Line AOE 5")
            .ActivateOnExit<ProwlingGale>()
            .ActivateOnExit<WindsOfDecayBait>()
            .ActivateOnExit<WindsOfDecayTether>()
            .DeactivateOnExit<BreathOfDecay>();
        ComponentCondition<AeroIII>(id + 0x90u, 6.2f, comp => comp.NumCasts == 2, "Knockback 2")
            .DeactivateOnExit<AeroIII>();
        ComponentCondition<ProwlingGale>(id + 0xA0u, 2.3f, comp => comp.NumCasts != 0, "Towers resolve")
            .DeactivateOnExit<ProwlingGale>();
        ComponentCondition<WindsOfDecayBait>(id + 0xB0u, 0.2f, comp => comp.NumCasts != 0, "Baits resolve")
            .DeactivateOnExit<WindsOfDecayTether>()
            .DeactivateOnExit<WindsOfDecayBait>();
    }

    private void TerrestrialTitans(uint id, float delay)
    {
        ComponentCondition<TerrestrialTitans>(id, delay, comp => comp.NumCasts != 0, "Circle AOEs + pillars spawn")
            .ActivateOnEnter<TerrestrialTitans>()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<Towerfall>()
            .DeactivateOnExit<TerrestrialTitans>();
        ComponentCondition<TitanicPursuit>(id + 0x10u, 7.1f, comp => comp.NumCasts != 0, "Raidwide")
            .ActivateOnEnter<TitanicPursuit>()
            .ActivateOnEnter<FangedCrossing>()
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<TitanicPursuit>();
        ComponentCondition<Towerfall>(id + 0x20u, 7.6f, comp => comp.NumCasts != 0, "Line AOEs + pillars disappear")
            .DeactivateOnExit<ArenaChanges>()
            .DeactivateOnExit<Towerfall>();
        ComponentCondition<FangedCrossing>(id + 0x30u, 0.2f, comp => comp.NumCasts != 0, "Cross AOEs")
            .DeactivateOnExit<FangedCrossing>();
    }

    private void TacticalPack(uint id, float delay)
    {
        Cast(id, (uint)AID.TacticalPack, delay, 3f, "Tactical Pack")
            .ActivateOnExit<Adds>()
            .ActivateOnExit<ArenaChanges>();
        Targetable(id + 0x10u, false, 2f, "Boss untargetable");
        ComponentCondition<HowlingHavoc>(id + 0x20u, 7.3f, comp => comp.NumCasts != 0, "Raidwide")
            .ActivateOnEnter<HowlingHavoc>()
            .DeactivateOnExit<HowlingHavoc>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Adds>(id + 0x30u, 0.5f, comp => comp.Windpack != default, "Debuff assignment + donut arena", 3f)
            .ActivateOnEnter<EarthWindborneEnd>()
            .ActivateOnExit<StalkingStoneWind>()
            .ActivateOnExit<AlphaWindStone>();
        ComponentCondition<StalkingStoneWind>(id + 0x40u, 9.7f, comp => comp.NumCasts != 0, "Baits and Line stacks 1");
        ComponentCondition<StalkingStoneWind>(id + 0x50u, 14.1f, comp => comp.NumCasts > 2, "Baits and Line stacks 2");
        ComponentCondition<StalkingStoneWind>(id + 0x60u, 14.2f, comp => comp.NumCasts > 4, "Baits and Line stacks 3")
            .DeactivateOnExit<StalkingStoneWind>()
            .DeactivateOnExit<AlphaWindStone>();
        ComponentCondition<RavenousSaber>(id + 0x70u, 28.3f, comp => comp.NumCasts != 0, "Raidwides x5 starts", 10f)  // note: time varies a lot depending on how fast phase gets completed
            .DeactivateOnExit<ArenaChanges>()
            .SetHint(StateMachine.StateHint.DowntimeEnd)
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<EarthWindborneEnd>()
            .DeactivateOnExit<Adds>()
            .ActivateOnEnter<RavenousSaber>();
        ComponentCondition<RavenousSaber>(id + 0x80u, 3.9f, comp => comp.NumCasts == 5, "Raidwides finish")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<RavenousSaber>();
    }

    private void TerrestrialRage(uint id, float delay)
    {
        Cast(id, (uint)AID.TerrestrialRage, delay, 3f, "Terrestial Rage")
            .ActivateOnExit<FangedCharge>()
            .ActivateOnExit<HeavensearthSuspendedStone>();
        ComponentCondition<FangedCharge>(id + 0x10u, 7f, comp => comp.NumCasts != 0, "Line AOEs 1");
        ComponentCondition<HeavensearthSuspendedStone>(id + 0x20u, 1.3f, comp => comp.NumFinishedSpreads == 4, "Spreads + Stack 1 resolve");
        ComponentCondition<FangedCharge>(id + 0x30u, 1.2f, comp => comp.NumCasts > 2, "Line AOEs 2")
            .DeactivateOnExit<FangedCharge>()
            .ActivateOnExit<RoaringWind>()
            .ActivateOnExit<Shadowchase>();
        ComponentCondition<HeavensearthSuspendedStone>(id + 0x40u, 5.5f, comp => comp.NumFinishedSpreads == 8, "Spreads + Stack 2 resolve")
            .DeactivateOnExit<HeavensearthSuspendedStone>();
        ComponentCondition<Shadowchase>(id + 0x50u, 0.4f, comp => comp.NumCasts != 0, "Line AOEs 3")
            .ExecOnExit<RoaringWind>(comp => comp.Draw = true)
            .DeactivateOnExit<Shadowchase>();
    }

    private void WolvesReign3(uint id, float delay)
    {
        CastStartMulti(id, [(uint)AID.RevolutionaryReignVisual1, (uint)AID.RevolutionaryReignVisual2, (uint)AID.EminentReignVisual1, (uint)AID.EminentReignVisual2], delay, "Wolvesreign");
        ComponentCondition<RoaringWind>(id + 0x10u, 0.5f, comp => comp.NumCasts != 0, "Line AOEs 1")
            .ActivateOnExit<WolvesReignConeCircle>()
            .ActivateOnExit<WolvesReignRect>()
            .ActivateOnExit<WolvesReignCircle>()
            .ActivateOnExit<WealOfStone>()
            .DeactivateOnExit<RoaringWind>();
        ComponentCondition<WolvesReignCircle>(id + 0x20u, 6.5f, comp => comp.NumCasts == 4, "Circles resolve")
            .DeactivateOnExit<WolvesReignCircle>()
            .ActivateOnExit<ReignsEnd>()
            .ActivateOnExit<SovereignScar>();
        ComponentCondition<WolvesReignRect>(id + 0x30u, 2.5f, comp => comp.NumCasts != 0, "Line AOE")
            .DeactivateOnExit<WolvesReignRect>();
        ComponentCondition<WolvesReignConeCircle>(id + 0x40u, 3.1f, comp => comp.NumCasts != 0, "Baits resolve + cone OR circle")
            .DeactivateOnExit<WolvesReignConeCircle>()
            .DeactivateOnExit<ReignsEnd>()
            .DeactivateOnExit<SovereignScar>()
            .ExecOnExit<WealOfStone>(comp => comp.Draw = true);
        ComponentCondition<WealOfStone>(id + 0x50u, 2.9f, comp => comp.NumCasts != 0, "Line AOEs 2")
            .DeactivateOnExit<WealOfStone>();
    }

    private void BeckonMoonlight(uint id, float delay)
    {
        Cast(id, (uint)AID.BeckonMoonlight, delay, 3f, "Beckon Moonlight")
            .ActivateOnExit<MoonbeamsBite>()
            .ActivateOnExit<HeavensearthSuspendedStone>();
        ComponentCondition<HeavensearthSuspendedStone>(id + 0x10u, 12.7f, comp => comp.NumFinishedSpreads == 4, "Spreads + Stack 1 resolve");
        for (var i = 1; i <= 4; ++i)
        {
            var offset = id + 0x20u + (uint)((i - 1) * 0x10u);
            var time = i == 1 ? 1.4f : 2f;
            var desc = $"Halfroom cleave {i}";
            var casts = i;
            var cond = ComponentCondition<MoonbeamsBite>(offset, time, comp => comp.NumCasts == casts, desc);
            if (i == 4)
            {
                cond
                    .ActivateOnEnter<WealOfStone>();
            }
        }
        ComponentCondition<HeavensearthSuspendedStone>(id + 0x60u, 1f, comp => comp.NumFinishedSpreads == 8, "Spreads + Stack 2 resolve")
            .DeactivateOnEnter<MoonbeamsBite>()
            .DeactivateOnExit<HeavensearthSuspendedStone>()
            .ExecOnExit<WealOfStone>(comp => comp.Draw = true);
    }

    private void WindfangStonefang2(uint id, float delay)
    {
        CastStartMulti(id, [(uint)AID.WindfangCross1, (uint)AID.WindfangCross2, (uint)AID.StonefangCross1, (uint)AID.StonefangCross2], delay, "Wind-/Stonefang 2")
            .ActivateOnEnter<WindfangStonefang>();
        ComponentCondition<WealOfStone>(id + 0x10u, 1.1f, comp => comp.NumCasts == 4, "Line AOEs")
            .ExecOnExit<WindfangStonefang>(comp => comp.Draw = true)
            .ActivateOnExit<StonefangBait>()
            .ActivateOnExit<WindfangBait>()
            .DeactivateOnExit<WealOfStone>();
        ComponentCondition<WindfangStonefang>(id + 0x20u, 4.9f, comp => comp.NumCasts != 0, "Baits + cross + circle OR donut")
            .DeactivateOnExit<StonefangBait>()
            .DeactivateOnExit<WindfangBait>()
            .DeactivateOnExit<WindfangStonefang>();
    }

    private void QuakeIII(uint id, float delay)
    {
        ComponentCondition<QuakeIII>(id, delay, comp => comp.NumCasts != 0, "Light party stacks")
            .SetHint(StateMachine.StateHint.Raidwide)
            .ActivateOnEnter<QuakeIII>()
            .DeactivateOnExit<QuakeIII>();
    }

    private void UltraviolentRay(uint id, float delay)
    {
        ComponentCondition<GleamingBeam>(id, delay, comp => comp.NumCasts == 5, "Defamations + Line AOEs")
            .ActivateOnEnter<UltraviolentRay>()
            .ActivateOnEnter<GleamingBeam>()
            .DeactivateOnExit<UltraviolentRay>()
            .DeactivateOnExit<GleamingBeam>();
    }

    private void Twinbite(uint id, float delay)
    {
        ComponentCondition<Twinbite>(id, delay, comp => comp.NumCasts != 0, "Tankbusters")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .ActivateOnEnter<Twinbite>()
            .DeactivateOnExit<Twinbite>();
    }

    private void HerosBlow(uint id, float delay)
    {
        ComponentCondition<HerosBlow>(id, delay, comp => comp.NumCasts != 0, "Cone + circle OR donut AOE")
            .ActivateOnEnter<HerosBlow>()
            .DeactivateOnExit<HerosBlow>();
    }

    private void Mooncleaver1(uint id, float delay)
    {
        ComponentCondition<Mooncleaver1>(id, delay, comp => comp.NumCasts != 0, "Destroy a platform")
            .ActivateOnEnter<Mooncleaver1>()
            .DeactivateOnExit<Mooncleaver1>();
    }

    private void ElementalPurge(uint id, float delay)
    {
        ComponentCondition<HuntersHarvestBait>(id, delay, comp => comp.Bind != default, "Bind main tank")
            .ActivateOnEnter<HuntersHarvest>()
            .ActivateOnEnter<HuntersHarvestBait>()
            .ActivateOnEnter<AerotemporalBlast>()
            .ActivateOnEnter<GeotemporalBlast>();
        ComponentCondition<GeotemporalBlast>(id + 0x10u, 5.2f, comp => comp.NumCasts != 0, "Stack + baited tankbusters")
            .DeactivateOnExit<GeotemporalBlast>()
            .DeactivateOnExit<HuntersHarvestBait>()
            .DeactivateOnExit<AerotemporalBlast>()
            .DeactivateOnExit<HuntersHarvest>();
    }

    private void ProwlingGaleP2(uint id, float delay)
    {
        ComponentCondition<ProwlingGaleP2>(id, delay, comp => comp.NumCasts != 0, "Towers resolve")
            .ActivateOnEnter<ProwlingGaleP2>()
            .DeactivateOnExit<ProwlingGaleP2>();
    }

    private void TwofoldTempest(uint id, float delay)
    {
        for (var i = 1; i <= 4; ++i)
        {
            var offset = id + (uint)((i - 1) * 0x10u);
            var time = i == 1 ? delay : 7.1f;
            var desc = $"Baits {i} resolve";
            var casts = i;
            var cond = ComponentCondition<TwofoldTempestRect>(offset, time, comp => comp.NumCasts == casts, desc);
            if (i == 1)
            {
                cond
                .ActivateOnEnter<TwofoldTempestTetherVoidzone>()
                .ActivateOnEnter<TwofoldTempestTetherAOE>()
                .ActivateOnEnter<TwofoldTempestRect>()
                .ActivateOnEnter<TwofoldTempestVoidzone>();
            }
            else if (i == 4)
            {
                cond
                .DeactivateOnExit<TwofoldTempestTetherVoidzone>()
                .DeactivateOnExit<TwofoldTempestTetherAOE>()
                .DeactivateOnExit<TwofoldTempestRect>();
            }
        }
        ComponentCondition<ArenaChanges>(id + 0x40u, 5.9f, comp => comp.Repaired, "Repair broken platform");
    }

    private void ChampionsCircuit(uint id, float delay)
    {
        for (var i = 1; i <= 5; ++i)
        {
            var offset = id + (uint)((i - 1) * 0x10u);
            var time = i == 1 ? delay : 4.4f;
            var desc = $"Rotation {i}";
            var casts = i;
            var cond = ComponentCondition<ChampionsCircuit>(offset, time, comp => comp.NumCasts == casts, desc);
            if (i == 1)
            {
                cond
                .ActivateOnEnter<ChampionsCircuit>()
                .ActivateOnEnter<GleamingBarrage>()
                .DeactivateOnExit<TwofoldTempestVoidzone>();
            }
            else if (i == 5)
            {
                cond
                .DeactivateOnExit<ChampionsCircuit>()
                .DeactivateOnExit<GleamingBarrage>();
            }
        }
    }

    private void RiseOfTheHuntersBlade(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, (uint)AID.RiseOfTheHuntersBlade, delay, 7f, true, "Rise of the Hunter's Blade")
            .ActivateOnEnter<LamentOfTheCloseDistant>();
        ActorCast(id + 0x10u, _module.BossP2, (uint)AID.LoneWolfsLament, 2.2f, 3f, true, "Lone Wolf's Lament");
        ComponentCondition<LamentOfTheCloseDistant>(id + 0x20u, 0.8f, comp => comp.TethersAssigned, "Tethers assigned");
        ComponentCondition<ProwlingGaleLast>(id + 0x30u, 18.2f, comp => comp.NumCasts != 0, "Towers resolve")
            .ActivateOnEnter<ProwlingGaleLast>()
            .DeactivateOnExit<ProwlingGaleLast>();
    }

    private void HowlingEight(uint id, float delay)
    {
        for (var i = 0; i < 5; ++i)
        {
            var phaseOffset = (uint)(i * 0x30u);
            var cast1Id = id + 0x10u + phaseOffset;
            var cast8Id = id + 0x20u + phaseOffset;
            var platformId = id + 0x30u + phaseOffset;
            var casts = i + 1;
            var cond = ComponentCondition<HowlingEight>(cast1Id, i == 0 ? delay : 9.3f, comp => comp.NumCasts == casts, $"Tower {casts} cast 1");
            if (i == 0)
            {
                cond
                .ActivateOnEnter<HowlingEight>()
                .ActivateOnEnter<Mooncleaver2>()
                .DeactivateOnEnter<LamentOfTheCloseDistant>();
            }
            ComponentCondition<HowlingEight>(cast8Id, 6f, comp => comp.Towers.Count == 0, $"Tower {casts} cast 8")
                .SetHint(StateMachine.StateHint.Raidwide);
            if (i < 4)
                ComponentCondition<Mooncleaver2>(platformId, 4.4f, comp => comp.NumCasts == casts, "Destroy platform");
        }
    }
}
