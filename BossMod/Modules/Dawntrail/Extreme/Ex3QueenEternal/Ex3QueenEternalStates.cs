namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

sealed class Ex3QueenEternalStates : StateMachineBuilder
{
    private readonly Ex3QueenEternal _module;

    public Ex3QueenEternalStates(Ex3QueenEternal module) : base(module)
    {
        _module = module;
        SimplePhase(default, Phase1, "P1")
            .ActivateOnEnter<ArenaChanges>() // it still counts if boss hp reaches 1 after cast finishes if its almost in the same moment, previously that case prevented the phase transition
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed || Module.PrimaryActor.HPMP.CurHP == 1u && _module.FindComponent<ArenaChanges>()?.EnrageCastStarted == true;
        SimplePhase(1u, Phase2, "P2")
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && _module.BossP2()?.IsDeadOrDestroyed == true;
    }

    private void Phase1(uint id)
    {
        P1Aethertithe(id, 12.2f);
        P1VirtualShiftWind(id + 0x10000u, 7.5f);
        P1DivideAndConquer(id + 0x20000u, 3.1f);
        P1RoyalDomain(id + 0x30000u, 3.0f);
        P1VirtualShiftEarth(id + 0x40000u, 8.2f);
        P1ProsecutionOfWar(id + 0x50000u, 7.2f);
        P1Coronation(id + 0x60000u, 3.1f);
        P1AbsoluteAuthority(id + 0x70000u, 6.2f);
        P1VirtualShiftIce(id + 0x80000u, 12.3f);
        P1ProsecutionOfWar(id + 0x90000u, 8.1f);
        P1RoyalDomain(id + 0xA0000u, 8.2f);
        P1LegitimateForce(id + 0xB0000u, 6.3f);
        P1RoyalDomain(id + 0xC0000u, 3.1f);
        Cast(id + 0xD0000u, AID.AuthorityEternal, 3.2f, 10f, "Enrage");
    }

    private void Phase2(uint id)
    {
        P2Intermission(id, 0f);
        P2RadicalShift(id + 0x10000u, 4.1f);
        P2DimensionalDistortion(id + 0x20000u, 7.2f);
        P2DyingMemory(id + 0x30000u, 1.3f);
        P2RadicalShift(id + 0x40000u, 11.4f);
        ActorCast(id + 0x50000u, _module.BossP2, AID.Preservation, 7.3f, 14f, true, "Enrage");
    }

    private void P1Aethertithe(uint id, float delay)
    {
        Cast(id, AID.Aethertithe, delay, 3f);
        ComponentCondition<Aethertithe>(id + 0x10u, 5f, static comp => comp.AOE.Length != 0)
            .ActivateOnEnter<Aethertithe>();
        ComponentCondition<Aethertithe>(id + 0x11u, 5.1f, static comp => comp.NumCasts >= 1, "Cone 1")
            .ActivateOnEnter<Retribute>();
        ComponentCondition<Retribute>(id + 0x12u, 2.9f, static comp => comp.NumCasts > 0, "Line stacks 1");
        ComponentCondition<Aethertithe>(id + 0x20u, 1.2f, static comp => comp.AOE.Length != 0);
        ComponentCondition<Aethertithe>(id + 0x21u, 5.1f, static comp => comp.NumCasts >= 2, "Cone 2");
        ComponentCondition<Retribute>(id + 0x22u, 2.9f, static comp => comp.NumCasts > 2, "Line stacks 2");
        ComponentCondition<Aethertithe>(id + 0x30u, 1.2f, static comp => comp.AOE.Length != 0);
        ComponentCondition<Aethertithe>(id + 0x31u, 5.1f, static comp => comp.NumCasts >= 3, "Cone 3")
            .DeactivateOnExit<Aethertithe>();
        ComponentCondition<Retribute>(id + 0x32u, 2.9f, static comp => comp.NumCasts > 4, "Line stacks 3")
            .DeactivateOnExit<Retribute>();
    }

    private void P1ProsecutionOfWar(uint id, float delay)
    {
        Cast(id, AID.ProsecutionOfWar, delay, 5f, "Tankbuster 1")
            .ActivateOnEnter<ProsecutionOfWar>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<ProsecutionOfWar>(id + 2u, 3.2f, static comp => comp.NumCasts > 1, "Tankbuster 2")
            .DeactivateOnExit<ProsecutionOfWar>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void P1DivideAndConquer(uint id, float delay)
    {
        Cast(id, AID.DivideAndConquer, delay, 7.5f)
            .ActivateOnEnter<DivideAndConquerBait>()
            .ActivateOnEnter<DivideAndConquerAOE>();
        ComponentCondition<DivideAndConquerBait>(id + 0x10u, 0.1f, static comp => comp.NumCasts > 0, "Protean 1");
        ComponentCondition<DivideAndConquerBait>(id + 0x20u, 7f, static comp => comp.NumCasts >= 8, "Protean 8")
            .DeactivateOnExit<DivideAndConquerBait>();
        ComponentCondition<DivideAndConquerAOE>(id + 0x30u, 4f, static comp => comp.NumCasts > 0, "Lines")
            .DeactivateOnExit<DivideAndConquerAOE>();
    }

    private void P1RoyalDomain(uint id, float delay)
    {
        Cast(id, AID.RoyalDomain, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P1LegitimateForce(uint id, float delay)
    {
        CastMulti(id, [AID.LegitimateForceFirstR, AID.LegitimateForceFirstL], delay, 8f, "Side 1")
            .ActivateOnEnter<LegitimateForce>();
        ComponentCondition<LegitimateForce>(id + 0x10u, 3.1f, static comp => comp.NumCasts > 1, "Side 2")
            .DeactivateOnExit<LegitimateForce>();
    }

    private void P1Coronation(uint id, float delay)
    {
        Cast(id, AID.Coronation, delay, 3f);
        ComponentCondition<Coronation>(id + 0x10u, 2.1f, static comp => comp.Groups.Count > 0)
            .ActivateOnEnter<Coronation>();
        Cast(id + 0x20u, AID.AtomicRay, 1.1f, 3);
        ComponentCondition<AtomicRay>(id + 0x30u, 1.2f, static comp => comp.Active)
            .ActivateOnEnter<AtomicRay>();
        ComponentCondition<Coronation>(id + 0x40u, 4.9f, static comp => comp.NumCasts > 0, "Coronation")
            .DeactivateOnExit<Coronation>();
        ComponentCondition<AtomicRay>(id + 0x41u, 1.1f, static comp => comp.NumFinishedSpreads > 0, "Spread")
            .DeactivateOnExit<AtomicRay>();
    }

    private void P1AbsoluteAuthority(uint id, float delay)
    {
        Cast(id, AID.AbsoluteAuthorityPuddles, delay, 10f);
        ComponentCondition<AbsoluteAuthorityPuddles>(id + 0x10u, 0.1f, static comp => comp.Casters.Count > 0, "Puddles bait")
            .ActivateOnEnter<AbsoluteAuthorityPuddles>();
        ComponentCondition<AbsoluteAuthorityExpansionBoot>(id + 0x20u, 10f, static comp => comp.NumCasts > 0, "Spread/stack")
            .ActivateOnEnter<AbsoluteAuthorityExpansionBoot>()
            .ActivateOnEnter<AbsoluteAuthorityHeel>()
            .DeactivateOnExit<AbsoluteAuthorityPuddles>() // last puddle resolves right before stack/spread
            .DeactivateOnExit<AbsoluteAuthorityExpansionBoot>();
        ComponentCondition<AbsoluteAuthorityHeel>(id + 0x30u, 4f, static comp => comp.NumCasts > 0, "Stack")
            .DeactivateOnExit<AbsoluteAuthorityHeel>();
        ComponentCondition<AbsoluteAuthorityKnockback>(id + 0x40u, 6.9f, static comp => comp.NumCasts > 0, "Knockback")
            .ActivateOnEnter<AbsoluteAuthorityKnockback>()
            .DeactivateOnExit<AbsoluteAuthorityKnockback>();
    }

    private void P1VirtualShiftWind(uint id, float delay)
    {
        Cast(id, AID.VirtualShiftWind, delay, 5f, "Raidwide (wind platform)")
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x10u, AID.LawsOfWind, 5.2f, 4f);
        ComponentCondition<Aeroquell>(id + 0x20u, 0.1f, static comp => comp.Active)
            .ActivateOnEnter<Aeroquell>();
        ComponentCondition<Aeroquell>(id + 0x21u, 5f, static comp => !comp.Active, "Party stacks")
            .DeactivateOnExit<Aeroquell>();

        CastStartMulti(id + 0x100, [AID.LegitimateForceFirstR, AID.LegitimateForceFirstL], 5.1f)
            .ActivateOnEnter<AeroquellTwister>(); // voidzones appear ~0.6s after stacks
        ComponentCondition<MissingLink>(id + 0x101u, 0.8f, static comp => comp.TethersAssigned, "Chains")
            .ActivateOnEnter<LegitimateForce>()
            .ActivateOnEnter<MissingLink>();
        CastEnd(id + 0x102u, 7.2f, "Side 1");
        ComponentCondition<LegitimateForce>(id + 0x103u, 3.1f, static comp => comp.NumCasts > 1, "Side 2")
            .DeactivateOnExit<LegitimateForce>();

        ComponentCondition<WindOfChange>(id + 0x200u, 3.2f, static comp => comp.NumCasts > 0, "Knockback")
            .ActivateOnEnter<WindOfChange>()
            .DeactivateOnExit<WindOfChange>()
            .DeactivateOnExit<MissingLink>();

        Cast(id + 0x300u, AID.WorldShatterP1, 3f, 5f, "Raidwide + platform end")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<AeroquellTwister>(id + 0x310u, 2.6f, comp => !comp.Sources(Module).Any())
            .DeactivateOnExit<AeroquellTwister>();

        P1ProsecutionOfWar(id + 0x1000u, 4.5f);
    }

    private void P1VirtualShiftEarth(uint id, float delay)
    {
        Cast(id, AID.VirtualShiftEarth, delay, 5f, "Raidwide (earth platform)")
            .SetHint(StateMachine.StateHint.Raidwide);
        CastStart(id + 0x10u, AID.LawsOfEarth, 5.2f)
            .ActivateOnEnter<VirtualShiftEarth>();
        CastEnd(id + 0x11u, 4f);

        P1LegitimateForce(id + 0x100u, 3.2f);
        ComponentCondition<LawsOfEarthBurst>(id + 0x120u, 5f, static comp => comp.NumCasts > 0, "Towers")
            .ActivateOnEnter<LawsOfEarthBurst1>()
            .DeactivateOnExit<LawsOfEarthBurst>();

        Cast(id + 0x200u, AID.GravitationalEmpire, 5.2f, 7f)
            .ActivateOnEnter<GravityPillar>()
            .ActivateOnEnter<GravityRay>()
            .ActivateOnEnter<LawsOfEarthBurst2>();
        ComponentCondition<GravityRay>(id + 0x210u, 1f, static comp => comp.NumCasts > 0, "Defamations + Cones")
            .DeactivateOnExit<GravityPillar>() // resolves right before cones
            .DeactivateOnExit<GravityRay>();
        ComponentCondition<LawsOfEarthBurst>(id + 0x220u, 0.8f, static comp => comp.NumCasts > 0, "Towers")
            .DeactivateOnExit<LawsOfEarthBurst>();

        ComponentCondition<MeteorImpact>(id + 0x300u, 5.5f, static comp => comp.Active)
            .ActivateOnEnter<MeteorImpact>();
        ComponentCondition<MeteorImpact>(id + 0x301u, 6.1f, static comp => comp.NumCasts > 0, "Meteors 1")
            .ActivateOnEnter<WeightyBlow>();
        ComponentCondition<MeteorImpact>(id + 0x310u, 1f, static comp => comp.Active);
        ComponentCondition<MeteorImpact>(id + 0x311u, 6.1f, static comp => comp.NumCasts > 0, "Meteors 2")
            .DeactivateOnExit<MeteorImpact>();

        Cast(id + 0x400u, AID.WeightyBlow, 2f, 5f);
        ComponentCondition<WeightyBlow>(id + 0x410u, 0.1f, static comp => comp.NumCasts > 0, "LOS 1");
        ComponentCondition<WeightyBlow>(id + 0x411u, 3.1f, static comp => comp.NumCasts > 2, "LOS 2");
        ComponentCondition<WeightyBlow>(id + 0x412u, 3.1f, static comp => comp.NumCasts > 4, "LOS 3");
        ComponentCondition<WeightyBlow>(id + 0x413u, 3.1f, static comp => comp.NumCasts > 6, "LOS 4")
            .DeactivateOnExit<WeightyBlow>();

        Cast(id + 0x500u, AID.WorldShatterP1, 0.7f, 5f, "Raidwide + platform end")
            .DeactivateOnExit<VirtualShiftEarth>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P1VirtualShiftIce(uint id, float delay)
    {
        Cast(id, AID.VirtualShiftIce, delay, 5f, "Raidwide (ice platform)")
            .ActivateOnEnter<VirtualShiftIce>()
            .SetHint(StateMachine.StateHint.Raidwide);
        CastStart(id + 0x10u, AID.LawsOfIce, 5.2f);
        CastEnd(id + 0x11u, 4f)
            .ActivateOnEnter<LawsOfIce>();
        ComponentCondition<LawsOfIce>(id + 0x12u, 1f, static comp => comp.NumCasts > 0, "Move");
        ComponentCondition<Rush>(id + 0x100u, 4.3f, static comp => comp.Activation != default)
            .ActivateOnEnter<Rush>()
            .DeactivateOnExit<LawsOfIce>();
        CastStartMulti(id + 0x101u, [AID.LegitimateForceFirstR, AID.LegitimateForceFirstL], 11.9f);
        ComponentCondition<Rush>(id + 0x102u, 0.3f, static comp => comp.NumCasts > 0, "Stretch tethers")
            .DeactivateOnExit<Rush>();
        CastEnd(id + 0x103u, 7.7f, "Side 1")
            .ActivateOnEnter<LegitimateForce>();
        ComponentCondition<LegitimateForce>(id + 0x104u, 3.1f, static comp => comp.NumCasts > 1, "Side 2")
            .DeactivateOnExit<LegitimateForce>();

        Cast(id + 0x200u, AID.LawsOfIce, 6.1f, 4f)
            .ActivateOnEnter<LawsOfIce>();
        ComponentCondition<LawsOfIce>(id + 0x202u, 1f, static comp => comp.NumCasts > 0, "Move")
            .ActivateOnEnter<IceDart>()
            .ActivateOnEnter<RaisedTribute>();
        ComponentCondition<IceDart>(id + 0x210u, 6.1f, static comp => comp.NumCasts > 0, "Tethers + Line stack 1")
            .DeactivateOnExit<LawsOfIce>();
        ComponentCondition<IceDart>(id + 0x220u, 7.1f, static comp => comp.NumCasts > 2, "Tethers + Line stack 2");
        ComponentCondition<IceDart>(id + 0x230u, 7.1f, static comp => comp.NumCasts > 4, "Tethers + Line stack 3");
        ComponentCondition<IceDart>(id + 0x240u, 7.1f, static comp => comp.NumCasts > 6, "Tethers + Line stack 4")
            .DeactivateOnExit<IceDart>()
            .DeactivateOnExit<RaisedTribute>();

        Cast(id + 0x300u, AID.WorldShatterP1, 3.1f, 5f, "Raidwide + platform end")
            .DeactivateOnExit<VirtualShiftIce>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P2Intermission(uint id, float delay)
    {
        ComponentCondition<ArenaChanges>(id, delay, static comp => comp.EnrageCastStarted);
        ComponentCondition<ArenaChanges>(id + 0x01u, 10f, static comp => comp.EnrageCastEnded);
        Targetable(id + 0x10u, false, 0.2f, "Boss disappears + Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorTargetable(id + 0x20u, _module.BossP2, true, 24.8f, "Boss appears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    private void P2RadicalShift(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.RadicalShift, delay, 11f, true, "Raidwide (platform change)")
            .ActivateOnEnter<RadicalShift>()
            .ActivateOnEnter<VirtualShiftIce>()
            .ActivateOnEnter<RadicalShiftAOE>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<RadicalShiftAOE>(id + 0x10, 5.2f, static comp => comp.NumFinishedSpreads > 0, "Spread")
            .ExecOnExit<RadicalShiftAOE>(static comp => comp.NumFinishedSpreads = 0);
        ActorCast(id + 0x100u, _module.BossP2, AID.RadicalShift, 3, 11, true, "Raidwide (platform change)")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<RadicalShiftAOE>(id + 0x110u, 5.2f, static comp => comp.NumFinishedSpreads > 0, "Spread")
            .DeactivateOnExit<RadicalShiftAOE>();
        ActorCast(id + 0x200u, _module.BossP2, AID.WorldShatterP2, 3f, 5f, true, "Raidwide + platform end")
            .DeactivateOnExit<RadicalShift>()
            .DeactivateOnExit<VirtualShiftIce>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P2DimensionalDistortion(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.DimensionalDistortion, delay, 4f, true)
            .ActivateOnEnter<DimensionalDistortion>();
        ComponentCondition<DimensionalDistortion>(id + 0x10u, 1f, static comp => comp.NumCasts > 0, "Exaflares start");

        ActorCast(id + 0x100u, _module.BossP2, AID.TyrannysGrasp, 5.2f, 5f, true, "Front half cleave")
            .ActivateOnEnter<TyrannysGraspAOE>()
            .ActivateOnEnter<TyrannysGraspTowers>()
            .DeactivateOnExit<DimensionalDistortion>()
            .DeactivateOnExit<TyrannysGraspAOE>();
        ComponentCondition<TyrannysGraspTowers>(id + 0x110u, 1.2f, static comp => comp.NumCasts >= 1, "Tankbuster tower 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<TyrannysGraspTowers>(id + 0x120u, 2.7f, static comp => comp.NumCasts >= 2, "Tankbuster tower 2")
            .DeactivateOnExit<TyrannysGraspTowers>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void P2DyingMemory(uint id, float delay)
    {
        ComponentCondition<DyingMemory>(id, delay, static comp => comp.NumCasts > 0, "Raidwide 1")
            .ActivateOnEnter<DyingMemory>()
            .DeactivateOnExit<DyingMemory>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<DyingMemoryLast>(id + 0x10u, 7.8f, static comp => comp.NumCasts > 0, "Raidwide 8")
            .ActivateOnEnter<DyingMemoryLast>()
            .DeactivateOnExit<DyingMemoryLast>()
            .SetHint(StateMachine.StateHint.Raidwide);

        ActorCastStart(id + 0x100u, _module.BossP2, AID.RoyalBanishment, 3.1f, true)
            .ActivateOnEnter<RoyalBanishment>(); // icon appears right before cast start
        ActorCastEnd(id + 0x101u, _module.BossP2, 5f, true);
        ComponentCondition<RoyalBanishment>(id + 0x110u, 0.8f, static comp => comp.NumCasts > 0, "Line stack 1");
        ComponentCondition<RoyalBanishment>(id + 0x120u, 6f, static comp => comp.NumCasts >= 7);
        ComponentCondition<RoyalBanishment>(id + 0x130u, 3f, static comp => comp.NumCasts >= 8, "Line stack 8")
            .DeactivateOnExit<RoyalBanishment>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }
}
