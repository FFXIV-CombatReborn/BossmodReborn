namespace BossMod.Dawntrail.Savage.M05SDancingGreen;

sealed class M05SDancingGreenStates : StateMachineBuilder
{
    public M05SDancingGreenStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        DeepCut(id, 9.1f);
        FlipToABSideSnapTwist(id + 0x10000u, 7.8f);
        CelebrateGoodTimes(id + 0x20000u, 0.9f);
        DiscoInfernal1(id + 0x30000u, 8.4f);
        CelebrateGoodTimes(id + 0x40000u, 0.9f);
        DeepCut(id + 0x50000u, 2.1f);
        GetDownLetsDance(id + 0x60000u, 8f);
        FlipToABSideRideTheWaves(id + 0x70000u, 14.2f);
        DeepCut(id + 0x80000u, 1.9f);
        CelebrateGoodTimes(id + 0x90000u, 1.5f);
        Frogtourage1(id + 0xA0000u, 15.6f);
        DiscoInfernal2(id + 0xB0000u, 3.5f);
        CelebrateGoodTimes(id + 0xC0000u, 0.9f);
        GetDownLetsDanceRemix(id + 0xD0000u, 8.6f);
        Frogtourage2(id + 0xE0000u, 26.6f);
        DeepCut(id + 0xF0000u, 1.8f);
        FunkyFloor2(id + 0x100000u, 10.8f);
        CelebrateGoodTimes(id + 0x110000u, 1.5f);
        CelebrateGoodTimes(id + 0x120000u, 5.2f);
        SimpleState(id + 0x130000u, 35.5f, "Enrage");
    }

    private void DeepCut(uint id, float delay)
    {
        ComponentCondition<DeepCut>(id, delay, comp => comp.CurrentBaits.Count != 0, "Tankbusters appear")
            .ActivateOnEnter<DeepCut>();
        ComponentCondition<DeepCut>(id + 0x10u, 5.7f, comp => comp.NumCasts != 0, "Tankbuster resolve")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<DeepCut>();
    }

    private void CelebrateGoodTimes(uint id, float delay)
    {
        Cast(id, (uint)AID.CelebrateGoodTimes, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void FlipToABSideSnapTwist(uint id, float delay)
    {
        ComponentCondition<FlipToABSide>(id, delay, comp => comp.NumCasts != 0, "Select stack kind")
            .ActivateOnEnter<TwoThreeFourSnapTwistDropTheNeedle>()
            .ActivateOnEnter<FlipToABSide>()
            .ExecOnExit<FlipToABSide>(comp => comp.SetActivation(16.2d));
        ComponentCondition<TwoThreeFourSnapTwistDropTheNeedle>(id + 0x10u, 11f, comp => comp.NumCasts != 0, "Half room cleave 1");
        ComponentCondition<TwoThreeFourSnapTwistDropTheNeedle>(id + 0x20u, 3.5f, comp => comp.NumCasts == 3, "Half room cleave 2");
        ComponentCondition<FlipToABSide>(id + 0x30u, 1.7f, comp => comp.Source == null, "Stack resolves")
            .ResetComp<TwoThreeFourSnapTwistDropTheNeedle>()
            .ResetComp<FlipToABSide>();
        ComponentCondition<FlipToABSide>(id + 0x40u, 2.9f, comp => comp.NumCasts != 0, "Store opposite stack kind")
            .ExecOnExit<FlipToABSide>(comp => comp.SetActivation(16.2d));
        ComponentCondition<TwoThreeFourSnapTwistDropTheNeedle>(id + 0x50u, 11f, comp => comp.NumCasts != 0, "Half room cleave 1");
        ComponentCondition<TwoThreeFourSnapTwistDropTheNeedle>(id + 0x60u, 3.5f, comp => comp.NumCasts == 3, "Half room cleave 2");
        ComponentCondition<FlipToABSide>(id + 0x70u, 1.7f, comp => comp.Source == null, "Stack resolves")
            .DeactivateOnExit<TwoThreeFourSnapTwistDropTheNeedle>()
            .DeactivateOnExit<FlipToABSide>();
    }

    private void DiscoInfernal1(uint id, float delay)
    {
        Cast(id, (uint)AID.DiscoInfernal, delay, 4f, "Raidwide")
            .ActivateOnEnter<FunkyFloor>()
            .ActivateOnEnter<Spotlights1>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<FunkyFloor>(id + 0x10u, 5.2f, comp => comp.NumCasts != 0, "Checkerboard 1")
            .ActivateOnEnter<InsideOutOutsideIn>();
        ComponentCondition<FunkyFloor>(id + 0x20u, 4f, comp => comp.NumCasts == 2, "Checkerboard 2");
        ComponentCondition<FunkyFloor>(id + 0x30u, 4f, comp => comp.NumCasts == 3, "Checkerboard 3");
        ComponentCondition<InsideOutOutsideIn>(id + 0x40u, 0.1f, comp => comp.AOEs.Count == 1, "In/Out 1");
        ComponentCondition<InsideOutOutsideIn>(id + 0x50u, 2.4f, comp => comp.AOEs.Count == 0, "In/Out 2")
            .DeactivateOnExit<InsideOutOutsideIn>();
        ComponentCondition<FunkyFloor>(id + 0x60u, 1.6f, comp => comp.NumCasts == 4, "Checkerboard 4");
        ComponentCondition<FlipToABSide>(id + 0x70u, 0.9f, comp => comp.NumCasts != 0, "Select stack kind")
            .ActivateOnEnter<TwoThreeFourSnapTwistDropTheNeedle>()
            .ActivateOnEnter<FlipToABSide>()
            .ExecOnExit<FlipToABSide>(comp => comp.SetActivation(26.3d));
        ComponentCondition<FunkyFloor>(id + 0x80u, 3f, comp => comp.NumCasts == 5, "Checkerboard 5");
        ComponentCondition<Spotlights1>(id + 0x90u, 2.5f, comp => comp.FinishedCount > 0, "Spotlights resolve 1");
        ComponentCondition<FunkyFloor>(id + 0xA0u, 1.4f, comp => comp.NumCasts == 6, "Checkerboard 6");
        ComponentCondition<FunkyFloor>(id + 0xB0u, 4f, comp => comp.NumCasts == 7, "Checkerboard 7");
        ComponentCondition<Spotlights1>(id + 0xC0u, 2.6f, comp => comp.FinishedCount > 4, "Spotlights resolve 2")
            .DeactivateOnExit<Spotlights1>();
        ComponentCondition<FunkyFloor>(id + 0xD0u, 1.4f, comp => comp.NumCasts == 8, "Checkerboard 8");
        ComponentCondition<FunkyFloor>(id + 0xE0u, 4f, comp => comp.NumCasts == 9, "Checkerboard 9");
        ComponentCondition<TwoThreeFourSnapTwistDropTheNeedle>(id + 0xF0u, 2.2f, comp => comp.NumCasts != 0, "Half room cleave 1");
        ComponentCondition<FunkyFloor>(id + 0x100u, 1.9f, comp => comp.NumCasts == 10, "Checkerboard 10")
            .DeactivateOnExit<FunkyFloor>();
        ComponentCondition<TwoThreeFourSnapTwistDropTheNeedle>(id + 0x110u, 1.6f, comp => comp.NumCasts == 3, "Half room cleave 2")
            .DeactivateOnExit<TwoThreeFourSnapTwistDropTheNeedle>();
        ComponentCondition<FlipToABSide>(id + 0x120u, 1.7f, comp => comp.Source == null, "Stack resolves")
            .DeactivateOnExit<FlipToABSide>();
    }

    private void GetDownLetsDance(uint id, float delay)
    {
        Cast(id, (uint)AID.EnsembleAssemble, delay, 3f, "Spawn dancers");
        ComponentCondition<GetDownOutIn>(id + 0x10u, 8.4f, comp => comp.NumCasts != 0, "Circle AOE")
            .ActivateOnEnter<GetDownOutIn>()
            .ActivateOnEnter<GetDownBait>()
            .ActivateOnEnter<GetDownCone>()
            .ActivateOnEnter<LetsDance>()
            .ActivateOnEnter<WavelengthAlphaBeta>();
        for (var i = 1; i <= 8; ++i)
        {
            var offset = id + (uint)(0x10u * (i + 1));
            var time = i == 1 ? 0.3f : 2.4f;
            var pattern = (i & 1) == 0 ? "donut" : "circle";
            var desc = i == 1 ? "Bait 1" : $"Bait {i} + {pattern} + cone repeat";
            var casts = i;

            var cond = ComponentCondition<GetDownBait>(offset, time, comp => comp.NumCasts == casts, desc);
            if (i == 8)
                cond.DeactivateOnExit<GetDownBait>();
        }

        ComponentCondition<GetDownCone>(id + 0xA0u, 2.5f, comp => comp.NumCasts == 8, "Cone repeat")
            .DeactivateOnExit<GetDownCone>()
            .DeactivateOnExit<GetDownOutIn>();

        for (var i = 1; i <= 8; ++i)
        {
            var offset = id + 0xB0u + (uint)((i - 1) * 0x10u);
            var time = i == 1 ? 8.4f : 2.4f;
            var desc = ((i & 1) == 0 ? $"Stack {i / 2} + " : "") + $"Halfroom cleave {i}";
            var casts = i;
            ComponentCondition<LetsDance>(offset, time, comp => comp.NumCasts == casts, desc);
        }

        Cast(id + 0x130u, (uint)AID.LetsPose, 3.2f, 5f, "Raidwide")
            .DeactivateOnExit<LetsDance>()
            .DeactivateOnExit<WavelengthAlphaBeta>();
    }

    private void FlipToABSideRideTheWaves(uint id, float delay)
    {
        ComponentCondition<FlipToABSide>(id, delay, comp => comp.NumCasts != 0, "Select stack kind")
            .ActivateOnEnter<TwoThreeFourSnapTwistDropTheNeedle>()
            .ActivateOnEnter<RideTheWaves>()
            .ActivateOnEnter<FlipToABSide>()
            .ExecOnExit<FlipToABSide>(comp => comp.SetActivation(53.4d));
        ComponentCondition<RideTheWaves>(id + 0x10u, 10.5f, comp => comp.AOEs.Count != 0, "Exaflare appears");
        ComponentCondition<RideTheWaves>(id + 0x20u, 3.1f, comp => comp.NumCasts != 0, "Exaflare starts");
        Condition(id + 0x30u, 1.1f, () => Module.FindComponent<QuarterBeats>()?.Stacks.Count != 0 || Module.FindComponent<EighthBeats>()?.Spreads.Count != 0, "Spreads OR stacks 1")
            .ActivateOnEnter<QuarterBeats>()
            .ActivateOnEnter<EighthBeats>();
        Condition(id + 0x40u, 5f, () => Module.FindComponent<QuarterBeats>()?.Stacks.Count == 0 && Module.FindComponent<EighthBeats>()?.Spreads.Count == 0, "Spreads/stacks 1 resolve");
        Condition(id + 0x50u, 3.2f, () => Module.FindComponent<QuarterBeats>()?.Stacks.Count != 0 || Module.FindComponent<EighthBeats>()?.Spreads.Count != 0, "Spreads OR stacks 2");
        Condition(id + 0x60u, 5f, () => Module.FindComponent<QuarterBeats>()?.Stacks.Count == 0 && Module.FindComponent<EighthBeats>()?.Spreads.Count == 0, "Spreads/stacks 2 resolve")
            .ActivateOnExit<InsideOutOutsideIn>()
            .DeactivateOnExit<QuarterBeats>()
            .DeactivateOnExit<EighthBeats>();
        ComponentCondition<InsideOutOutsideIn>(id + 0x70u, 10.5f, comp => comp.AOEs.Count == 1, "In/Out 1");
        ComponentCondition<InsideOutOutsideIn>(id + 0x80u, 2.4f, comp => comp.AOEs.Count == 0, "In/Out 2")
            .DeactivateOnExit<InsideOutOutsideIn>();
        ComponentCondition<RideTheWaves>(id + 0x90u, 3.4f, comp => comp.NumCasts == 16, "Exaflare finishes")
            .DeactivateOnExit<RideTheWaves>();
        ComponentCondition<TwoThreeFourSnapTwistDropTheNeedle>(id + 0xA0u, 4f, comp => comp.NumCasts != 0, "Half room cleave 1");
        ComponentCondition<TwoThreeFourSnapTwistDropTheNeedle>(id + 0xB0u, 3.5f, comp => comp.NumCasts == 3, "Half room cleave 2");
        ComponentCondition<FlipToABSide>(id + 0xC0u, 1.7f, comp => comp.Source == null, "Stored stack resolves")
            .DeactivateOnExit<TwoThreeFourSnapTwistDropTheNeedle>()
            .DeactivateOnExit<FlipToABSide>();
    }

    private void Frogtourage1(uint id, float delay)
    {
        ComponentCondition<Moonburn>(id, delay, comp => comp.Casters.Count != 0, "Line AOEs appear")
            .ActivateOnEnter<Moonburn>()
            .ActivateOnExit<QuarterBeats>()
            .ActivateOnExit<EighthBeats>();
        Condition(id + 0x10u, 4.9f, () => Module.FindComponent<QuarterBeats>()?.Stacks.Count != 0 || Module.FindComponent<EighthBeats>()?.Spreads.Count != 0, "Spreads OR stacks");
        Condition(id + 0x20u, 5f, () => Module.FindComponent<QuarterBeats>()?.Stacks.Count == 0 && Module.FindComponent<EighthBeats>()?.Spreads.Count == 0, "Spreads/stacks resolve")
            .DeactivateOnExit<QuarterBeats>()
            .DeactivateOnExit<EighthBeats>();
        ComponentCondition<Moonburn>(id + 0x30u, 0.6f, comp => comp.Casters.Count == 0, "Line AOEs resolve")
            .DeactivateOnExit<Moonburn>();
    }

    private void DiscoInfernal2(uint id, float delay)
    {
        Cast(id, (uint)AID.DiscoInfernal, delay, 4f, "Raidwide")
            .ActivateOnEnter<BackUpDance>()
            .ActivateOnEnter<Spotlights2>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Spotlights2>(id + 0x10u, 10f, comp => comp.FinishedCount != 0, "Spotlights resolve 1");
        ComponentCondition<BackUpDance>(id + 0x20u, 1.8f, comp => comp.NumCasts != 0, "Baits 1 resolve");
        ComponentCondition<FlipToABSide>(id + 0x30u, 5.5f, comp => comp.NumCasts != 0, "Select stack kind")
            .ActivateOnEnter<TwoThreeFourSnapTwistDropTheNeedle>()
            .ActivateOnEnter<FlipToABSide>()
            .ExecOnExit<FlipToABSide>(comp => comp.SetActivation(16.2d));
        ComponentCondition<Spotlights2>(id + 0x40u, 2.7f, comp => comp.FinishedCount > 4, "Spotlights resolve 2");
        ComponentCondition<BackUpDance>(id + 0x50u, 1.9f, comp => comp.NumCasts > 4, "Baits 2 resolve")
            .DeactivateOnExit<BackUpDance>()
            .DeactivateOnExit<Spotlights2>();
        ComponentCondition<TwoThreeFourSnapTwistDropTheNeedle>(id + 0x60u, 6.5f, comp => comp.NumCasts != 0, "Half room cleave 1");
        ComponentCondition<TwoThreeFourSnapTwistDropTheNeedle>(id + 0x70u, 3.5f, comp => comp.NumCasts == 3, "Half room cleave 2");
        ComponentCondition<FlipToABSide>(id + 0x80u, 1.7f, comp => comp.Source == null, "Stored stack resolves")
            .DeactivateOnExit<TwoThreeFourSnapTwistDropTheNeedle>()
            .DeactivateOnExit<FlipToABSide>();
    }

    private void GetDownLetsDanceRemix(uint id, float delay)
    {
        Cast(id, (uint)AID.EnsembleAssemble, delay, 3f, "Spawn dancers");
        ComponentCondition<GetDownOutIn>(id + 0x10u, 8.4f, comp => comp.NumCasts != 0, "Circle AOE")
            .ActivateOnEnter<GetDownOutIn>()
            .ActivateOnEnter<GetDownBait>()
            .ExecOnEnter<GetDownBait>(comp => comp.First = false)
            .ActivateOnEnter<GetDownCone>()
            .ActivateOnEnter<LetsDanceRemix>();
        for (var i = 1; i <= 8; ++i)
        {
            var offset = id + (uint)(0x20u * (i + 1));
            var time = i == 1 ? 0.3f : 2.4f;
            var pattern = (i & 1) == 0 ? "donut" : "circle";
            var desc = i == 1 ? "Bait 1" : $"Bait {i} + {pattern} + cone repeat";
            var casts = i;

            var cond = ComponentCondition<GetDownBait>(offset, time, comp => comp.NumCasts == casts, desc);
            if (i == 8)
                cond.DeactivateOnExit<GetDownBait>();
        }

        ComponentCondition<GetDownCone>(id + 0x130u, 2.5f, comp => comp.NumCasts == 8, "Cone repeat")
            .DeactivateOnExit<GetDownCone>()
            .DeactivateOnExit<GetDownOutIn>();
        for (var i = 1; i <= 8; ++i)
        {
            var offset = id + 0x140u + (uint)((i - 1) * 0x10u);
            var casts = i;
            var desc = $"Halfroom cleave {i}";
            ComponentCondition<LetsDanceRemix>(offset, i == 1 ? 8.4f : 1.5f, comp => comp.NumCasts == casts, desc);
        }
        Cast(id + 0x1C0, (uint)AID.LetsPoseRemix, 2.2f, 5f, "Raidwide")
            .DeactivateOnExit<LetsDanceRemix>();
    }

    private void Frogtourage2(uint id, float delay)
    {
        ComponentCondition<DoTheHustle>(id, delay, comp => comp.NumCasts != 0, "Cleaves 1")
            .ActivateOnEnter<DoTheHustle>();
        ComponentCondition<DoTheHustle>(id + 0x10u, 4f, comp => comp.NumCasts == 4, "Cleaves 2");
        ComponentCondition<DoTheHustle>(id + 0x20u, 4.2f, comp => comp.NumCasts == 5, "Halfroom cleave")
            .DeactivateOnExit<DoTheHustle>();
        ComponentCondition<Moonburn>(id + 0x30u, 6.3f, comp => comp.Casters.Count != 0, "Line AOEs 1 appear")
            .ActivateOnExit<BackUpDance>()
            .ActivateOnEnter<Moonburn>();
        ComponentCondition<Moonburn>(id + 0x40u, 10.5f, comp => comp.Casters.Count == 0, "Line AOEs 1 resolve");
        ComponentCondition<BackUpDance>(id + 0x50u, 0.1f, comp => comp.NumCasts != 0, "Baits 1 resolve");
        ComponentCondition<Moonburn>(id + 0x60u, 5.5f, comp => comp.Casters.Count != 0, "Line AOEs 2 appear");
        ComponentCondition<Moonburn>(id + 0x70u, 10.5f, comp => comp.Casters.Count == 0, "Line AOEs 2 resolve")
            .DeactivateOnExit<Moonburn>();
        ComponentCondition<BackUpDance>(id + 0x80u, 0.1f, comp => comp.NumCasts > 4, "Baits 2 resolve")
            .ActivateOnExit<DoTheHustle>()
            .DeactivateOnExit<BackUpDance>();
        ComponentCondition<DoTheHustle>(id + 0x90u, 9.5f, comp => comp.NumCasts == 3, "Cleaves 3")
            .ActivateOnExit<FunkyFloor>()
            .DeactivateOnExit<DoTheHustle>();
    }

    private void FunkyFloor2(uint id, float delay)
    {
        ComponentCondition<FunkyFloor>(id, delay, comp => comp.NumCasts != 0, "Checkerboard 1")
            .ActivateOnExit<QuarterBeats>()
            .ActivateOnExit<EighthBeats>();
        Condition(id + 0x10u, 2.6f, () => Module.FindComponent<QuarterBeats>()?.Stacks.Count != 0 || Module.FindComponent<EighthBeats>()?.Spreads.Count != 0, "Spreads OR stacks 1");
        ComponentCondition<FunkyFloor>(id + 0x20u, 1.4f, comp => comp.NumCasts == 2, "Checkerboard 2");
        Condition(id + 0x30u, 3.6f, () => Module.FindComponent<QuarterBeats>()?.Stacks.Count == 0 && Module.FindComponent<EighthBeats>()?.Spreads.Count == 0, "Spreads/stacks resolve 1");
        ComponentCondition<FunkyFloor>(id + 0x40u, 0.5f, comp => comp.NumCasts == 3, "Checkerboard 3")
            .ActivateOnExit<InsideOutOutsideIn>();
        ComponentCondition<FunkyFloor>(id + 0x50u, 4f, comp => comp.NumCasts == 4, "Checkerboard 4");
        ComponentCondition<InsideOutOutsideIn>(id + 0x60u, 3.8f, comp => comp.AOEs.Count == 1, "In/Out 1");
        ComponentCondition<FunkyFloor>(id + 0x70u, 0.3f, comp => comp.NumCasts == 5, "Checkerboard 5");
        ComponentCondition<InsideOutOutsideIn>(id + 0x80u, 2.1f, comp => comp.AOEs.Count == 0, "In/Out 2")
            .DeactivateOnExit<InsideOutOutsideIn>();
        ComponentCondition<FunkyFloor>(id + 0x90u, 1.9f, comp => comp.NumCasts == 6, "Checkerboard 6");
        Condition(id + 0xA0u, 2.5f, () => Module.FindComponent<QuarterBeats>()?.Stacks.Count != 0 || Module.FindComponent<EighthBeats>()?.Spreads.Count != 0, "Spreads OR stacks 2");
        ComponentCondition<FunkyFloor>(id + 0xB0u, 1.5f, comp => comp.NumCasts == 7, "Checkerboard 7");
        Condition(id + 0xC0u, 3.5f, () => Module.FindComponent<QuarterBeats>()?.Stacks.Count == 0 && Module.FindComponent<EighthBeats>()?.Spreads.Count == 0, "Spreads/stacks resolve 2")
            .DeactivateOnExit<QuarterBeats>()
            .DeactivateOnExit<EighthBeats>();
        ComponentCondition<FunkyFloor>(id + 0xD0u, 0.6f, comp => comp.NumCasts == 8, "Checkerboard 8")
            .DeactivateOnExit<FunkyFloor>();
    }
}
