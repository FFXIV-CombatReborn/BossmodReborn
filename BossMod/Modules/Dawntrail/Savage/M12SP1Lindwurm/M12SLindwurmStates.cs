namespace BossMod.Dawntrail.Savage.M12SLindwurm;

sealed class M12SLindwurmStates : StateMachineBuilder
{
    public M12SLindwurmStates(BossModule module) : base(module)
    {
        DeathPhase(default, InitialPhase);
    }

    private void InitialPhase(uint id)
    {
        Opening(id, 340f);
        Slaughtershed(id + 0x10000, 340f);
    }

    private void Opening(uint id, float delay)
    {
        ComponentCondition<Slaughtershed>(id, delay, comp => comp.NumCasts > 0, "Slaughtershed", 300, 300) // First cast around 5:40.
        .ActivateOnEnter<Slaughtershed>()
        .ActivateOnEnter<TheFixer>()
        .ActivateOnEnter<MortalSlayer>()
        .ActivateOnEnter<BurstingGrotesquerieAct1>()
        .ActivateOnEnter<SharedGrotesquerieAct1>()
        .ActivateOnEnter<DirectedGrotesquerieAct1>()
        .ActivateOnEnter<PhagocyteSpotlight0>()
        .ActivateOnEnter<RavenousReach1>()
        .ActivateOnEnter<DramaticLysis0>()
        .ActivateOnEnter<FourthWallFusion0>()
        .ActivateOnEnter<Burst>()
        .ActivateOnEnter<VisceralBurst>()
        .ActivateOnEnter<FourthWallFusion2>()
        .ActivateOnEnter<GrotesquerieAct2>()
        .ActivateOnEnter<PhagocyteSpotlight1>()
        .ActivateOnEnter<CruelCoil>()
        .ActivateOnEnter<unk_46194>()
        .ActivateOnEnter<DramaticLysis2>()
        .ActivateOnEnter<UnmitigatedExplosion>()
        .ActivateOnEnter<Constrictor>()
        .ActivateOnEnter<Splattershed2>()
        .ActivateOnEnter<GrotesquerieAct3>()
        .ActivateOnEnter<GrandEntrance1>()
        .ActivateOnEnter<GrandEntrance3>()
        .ActivateOnEnter<GrandEntrance0>()
        .ActivateOnEnter<BringDownTheHouse0>()
        .ActivateOnEnter<BringDownTheHouse1>()
        .ActivateOnEnter<DramaticLysis1>()
        .ActivateOnEnter<SplitScourge>()
        .ActivateOnEnter<VenomousScourge>()
        .ActivateOnEnter<GrandEntrance2>()
        .ActivateOnEnter<BringDownTheHouse2>()
        .ActivateOnEnter<GrotesquerieCurtainCall>()
        .ActivateOnEnter<Slaughtershed0>()
        .ActivateOnEnter<DramaticLysis4>()
        .ActivateOnEnter<DramaticLysis>()
        .ActivateOnEnter<FourthWallFusion1>()
        .ActivateOnEnter<RaptorKnuckles2>()
        .ActivateOnEnter<SerpentineScourge2>()
        .ActivateOnEnter<RefreshingOverkill2>()
        .ActivateOnEnter<RefreshingOverkill1>()
        .ActivateOnEnter<unk_48028>()
        .ActivateOnEnter<unk_46395>()
        .ActivateOnEnter<ArcadiaAflame>()
        .ActivateOnEnter<TopTierSlam1>()
        .ActivateOnEnter<WingedScourge2>()
        .ActivateOnEnter<MightyMagic1>()
        .ActivateOnEnter<EsotericFinisher>()
        .ActivateOnEnter<FirefallSplash1>()
        .ActivateOnEnter<ManaBurst1>()
        .ActivateOnEnter<HeavySlam0>()
        .ActivateOnEnter<UnmitigatedImpact>()
        .ActivateOnEnter<HeavySlam2>();
    }

    // -------------------------
    // Final phase — after first SlaughterShed2
    // -------------------------

    private void Slaughtershed(uint id, float delay)
    {
        SimpleState(id, 700f, "Slaughtershed to Clear")
            .ActivateOnEnter<SerpentineScourge>()
            .ActivateOnEnter<RaptorKnuckles>();
    }
}

/*
sealed class M12SLindwurmStates : StateMachineBuilder
{
    public M12SLindwurmStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheFixer>()
            .ActivateOnEnter<MortalSlayer>()
            .ActivateOnEnter<BurstingGrotesquerieAct1>()
            .ActivateOnEnter<SharedGrotesquerieAct1>()
            .ActivateOnEnter<DirectedGrotesquerieAct1>()
            .ActivateOnEnter<PhagocyteSpotlight0>()
            .ActivateOnEnter<RavenousReach1>()
            .ActivateOnEnter<DramaticLysis0>()
            .ActivateOnEnter<FourthWallFusion0>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<VisceralBurst>()
            .ActivateOnEnter<FourthWallFusion2>()
            .ActivateOnEnter<GrotesquerieAct2>()
            .ActivateOnEnter<PhagocyteSpotlight1>()
            .ActivateOnEnter<CruelCoil>()
            .ActivateOnEnter<unk_46194>()
            .ActivateOnEnter<DramaticLysis2>()
            .ActivateOnEnter<UnmitigatedExplosion>()
            .ActivateOnEnter<Constrictor>()
            .ActivateOnEnter<Splattershed2>()
            .ActivateOnEnter<GrotesquerieAct3>()
            .ActivateOnEnter<GrandEntrance1>()
            .ActivateOnEnter<GrandEntrance3>()
            .ActivateOnEnter<GrandEntrance0>()
            .ActivateOnEnter<BringDownTheHouse0>()
            .ActivateOnEnter<BringDownTheHouse1>()
            .ActivateOnEnter<DramaticLysis1>()
            .ActivateOnEnter<SplitScourge1>()
            .ActivateOnEnter<GrandEntrance2>()
            .ActivateOnEnter<BringDownTheHouse2>()
            .ActivateOnEnter<GrotesquerieCurtainCall>()
            //TODO: SplitScourge1 and VenomousScourge baits
            .ActivateOnEnter<Slaughtershed0>()
            .ActivateOnEnter<DramaticLysis4>()
            .ActivateOnEnter<FourthWallFusion1>()
            .ActivateOnEnter<RaptorKnuckles2>()
            .ActivateOnEnter<SerpentineScourge2>()
            .ActivateOnEnter<RefreshingOverkill2>()
            .ActivateOnEnter<RefreshingOverkill1>()
            .ActivateOnEnter<unk_48028>()
            .ActivateOnEnter<unk_46395>()
            .ActivateOnEnter<ArcadiaAflame>()
            .ActivateOnEnter<TopTierSlam1>()
            .ActivateOnEnter<WingedScourge2>()
            .ActivateOnEnter<MightyMagic1>()
            .ActivateOnEnter<EsotericFinisher>()
            .ActivateOnEnter<FirefallSplash1>()
            .ActivateOnEnter<ManaBurst1>()
            .ActivateOnEnter<HeavySlam0>()
            .ActivateOnEnter<UnmitigatedImpact>()
            .ActivateOnEnter<HeavySlam2>()
            .ActivateOnEnter<SerpentineScourge>()
            .ActivateOnEnter<RaptorKnuckles>();
    }
}
*/
