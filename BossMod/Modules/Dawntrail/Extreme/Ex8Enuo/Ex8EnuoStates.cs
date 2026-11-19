namespace BossMod.Dawntrail.Extreme.Ex8Enuo;

sealed class Ex8EnuoStates : StateMachineBuilder
{
    private readonly Ex8Enuo _module;
    public Ex8EnuoStates(Ex8Enuo module) : base(module)
    {
        _module = module;
        SimplePhase(default, Phase1, "P1")
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed || (Module.PrimaryActor.CastInfo?.Action.ID ?? 0u) == (uint)AID.AllForNaught;
        SimplePhase(2u, AddPhase, "Adds")
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<LoomingShadowAdd>()
            .ActivateOnEnter<AggressiveShadowAdd>()
            .ActivateOnEnter<SupportShadowAdds>()
            .ActivateOnEnter<Gauntlet>()
            .ActivateOnEnter<LoomingEmptinessKB>()
            .ActivateOnEnter<LoomingEmptinessKillZone>()
            .ActivateOnEnter<EmptyShadowTower>()
            .ActivateOnEnter<VoidalTurbulanceCone>()
            .ActivateOnEnter<DemonEye>()
            .ActivateOnEnter<DrainTouch>()
            .ActivateOnEnter<WeightofNothing>()
            .ActivateOnEnter<CurseoftheFlesh>()
            .ActivateOnEnter<BeaconAdd>()
            .ActivateOnEnter<Nothingness>()
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed || (Module.PrimaryActor.CastInfo?.Action.ID ?? 0u) == (uint)AID.LightlessWorldCastbar;
        SimplePhase(3u, Phase2, "P2")
            .ActivateOnEnter<ArenaChanges>()
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed;
    }

    private void Phase1(uint id)
    {
        Meteorain(id, 9.2f);
        NaughtGrowsSingle(id + 0x010000u, 7.4f);
        NaughtWakesSimple(id + 0x010002u, 7.2f);
        Meltdown(id + 0x020000u, 2.1f);
        Emptiness(id + 0x030000u, 8.3f);
        NaughtGrowsSingle(id + 0x040000u, 3.5f);
        GazeOfTheVoid(id + 0x050000u, 10.5f);
        Vacuum(id + 0x060000u, 24.2f);
        Emptiness(id + 0x070000u, 8.7f);
        DeepFreeze(id + 0x080000u, 6.1f);
        Meteorain(id + 0x090000u, 3.2f);
        SimpleState(id + 0x100000u, 8.9f, "Adds");
    }

    private void AddPhase(uint id)
    {
        Cast(id, AID.AllForNaught, 0f, 5f, "Add Transition");
        ActorCast(id + 0x01u, _module.CastingAdd, AID.LoomingEmptinessKnockback, 14.2f, 5.0f, false, "Knockback");
        ActorCast(id + 0x02u, _module.CastingAdd, AID.VoidalTurbulenceCastBar, 3.2f, 7f, false, "Cones + Towers");
        ActorCast(id + 0x03u, _module.CastingAdd, AID.VoidalTurbulenceCastBar, 13.6f, 7f, false, "Cones + Towers (again)"); // Timing on this one could be 'fun'
        SimpleState(id + 0x04u, 60f, "Add Enrage?"); // I have no idea when this would actually go off.
    }

    private void Phase2(uint id)
    {
        LightlessWorld(id, 96.8f);
        Almagest(id + 0x01, 10.2f);
        DoubleNaughtGrows(id + 0x02, 5.8f);
        NaughtWakesActive(id + 0x03, 7.3f);
        ShroudedHoly(id + 0x04, 19.5f);
        DoubleNaughtGrows(id + 0x05, 8.5f);
        DimensionZero(id + 0x06, 7.2f);
        VacuumMeltdown(id + 0x07, 11f);
        GazeOfTheVoid(id + 0x09, 13.3f);
        Almagest(id + 0x10, 28.3f);
        NaughtWakesActive(id + 0x11, 8.4f);
        NaughtHunts(id + 0x12, 2.1f);
        Emptiness(id + 0x13, 23.3f);
        NaughtHunts(id + 0x14, 6.3f);
        Emptiness(id + 0x15, 23.2f);
        DimensionZero(id + 0x17, 5.2f);
        Almagest(id + 0x17, 10.1f);
        DoubleNaughtGrows(id + 0x18, 5.7f);
        NaughtWakesActive(id + 0x19, 7.2f);
        DeepFreeze(id + 0x20, 19.4f);
        DoubleNaughtGrows(id + 0x21, 7f); //? Timing unknown
        DimensionZero(id + 0x22, 7f); //? timing unknown
        Meteorain(id + 0x23, 7f); //? timing unknown
        Meteorain(id + 0x24, 7f); //? timing unknown
    }

    private void Meteorain(uint id, float delay)
    {
        Cast(id, AID.Meteorain, delay, 5f, "Meteorain")
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<Meteorain>()
            .ActivateOnEnter<NaughtGrowsWildCharge>()
            .ActivateOnEnter<NaughtGrowsDonut>()
            .ActivateOnEnter<NaughtGrowsCircle>();
    }

    private void NaughtGrowsSingle(uint id, float delay)
    {
        Cast(id, AID.NaughtGrows, delay, 7f, "Naught Grows (Single)");
    }

    private void NaughtWakesSimple(uint id, float delay)
    {
        Cast(id, AID.NaughtWakes, delay, 2f, "Naught Wakes");
    }

    private void Meltdown(uint id, float delay)
    {
        Cast(id, AID.Meltdown, delay, 4f, "Meltdown")
            .ActivateOnEnter<MeltdownAoE>()
            .ActivateOnEnter<MeltdownSpread>()
            .ActivateOnEnter<MeltdownWait>();
    }

    private void Emptiness(uint id, float delay)
    {
        CastMulti(id, [AID.AiryEmptiness, AID.DenseEmptiness], delay, 4f, "Dense/Airy Emptiness")
            .ActivateOnEnter<DenseAiryEmptiness>();
    }

    private void GazeOfTheVoid(uint id, float delay)
    {
        Cast(id, AID.GazeOfTheVoid, delay, 6f, "Gaze of the Void")
            .ActivateOnEnter<GazeOfTheVoidAOE>()
            .ActivateOnEnter<GazeOfTheVoidSoaks>();
    }

    private void Vacuum(uint id, float delay)
    {
        Cast(id, AID.Vacuum, delay, 2.0f, "Vacuum")
            .ActivateOnEnter<VacuumAOE>()
            .ActivateOnEnter<VacuumArc1>()
            .ActivateOnEnter<VacuumArc2>()
            .ActivateOnEnter<VacuumArc3>()
            .ActivateOnEnter<VacuumTelegraph>();
    }

    private void DeepFreeze(uint id, float delay)
    {
        Cast(id, AID.DeepFreezeCastBar, delay, 5f, "Deep Freeze")
            .ActivateOnEnter<DeepFreezeFlares>()
            .ActivateOnEnter<DeepFreeze>();
    }

    private void LightlessWorld(uint id, float delay)
    {
        Cast(id, AID.LightlessWorldCastbar, delay, 10f, "Lightless World")
            .ActivateOnEnter<LightlessWorld>();
    }
    private void Almagest(uint id, float delay)
    {
        Cast(id, AID.Almagest, delay, 5f, "Almagest")
            .ActivateOnEnter<NaughtGrowsWildCharge>()
            .ActivateOnEnter<NaughtGrowsCircle>()
            .ActivateOnEnter<NaughtGrowsDonut>()
            .ActivateOnEnter<NaughtGrowsBossCircle>()
            .ActivateOnEnter<NaughtGrowsBossDonut>()
            .ActivateOnEnter<Almagest>();
    }

    private void DoubleNaughtGrows(uint id, float delay)
    {
        Cast(id, AID.NaughtGrowsDoubleCast, delay, 8f, "Double Naught Grows");
    }

    private void NaughtWakesActive(uint id, float delay)
    {
        Cast(id, AID.NaughtWakes, delay, 2f, "Naught Wakes AOEs")
            .ActivateOnEnter<PassageOfNaught>()
            .ActivateOnEnter<NaughtHunts>()
            .ActivateOnEnter<NaughtHuntsJumps>();
    }

    private void ShroudedHoly(uint id, float delay)
    {
        Cast(id, AID.ShroudedHolyCastbar, delay, 6f, "Shrouded Holy")
            .ActivateOnEnter<ShroudedHoly>()
            .ActivateOnEnter<DimensionZero>();
    }

    private void DimensionZero(uint id, float delay)
    {
        Cast(id, AID.DimensionZeroCastbar, delay, 5f, "Dimension Zero");
    }

    private void VacuumMeltdown(uint id, float delay)
    {
        Cast(id, AID.Vacuum, delay, 3f, "Vacuum/Meltdown")
            .ActivateOnEnter<VacuumAOE>()
            .ActivateOnEnter<VacuumArc1>()
            .ActivateOnEnter<VacuumArc2>()
            .ActivateOnEnter<VacuumArc3>()
            .ActivateOnEnter<VacuumTelegraph>()
            .ActivateOnEnter<MeltdownAoE>()
            .ActivateOnEnter<MeltdownSpread>()
            .ActivateOnEnter<MeltdownWait>();
        Cast(id + 0x1u, AID.Meltdown, 2.21f, 4f, "Meltdown");
    }

    private void NaughtHunts(uint id, float delay)
    {
        Cast(id, AID.NaughtHunts, delay, 7f, "Naught Hunts");
    }

    //private void XXX(uint id, float delay)
}
