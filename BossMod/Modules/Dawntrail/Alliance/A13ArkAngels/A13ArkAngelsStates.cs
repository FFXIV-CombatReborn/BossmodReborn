namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

sealed class A13ArkAngelsStates : StateMachineBuilder
{
    private readonly A13ArkAngels _module;

    public A13ArkAngelsStates(A13ArkAngels module) : base(module)
    {
        _module = module;
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<DecisiveBattle>()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<Cloudsplitter>()
            .ActivateOnEnter<TachiYukikaze>()
            .ActivateOnEnter<TachiGekko>()
            .ActivateOnEnter<TachiKasha>()
            .ActivateOnEnter<ConcertedDissolution>()
            .ActivateOnEnter<LightsChain>()
            .ActivateOnEnter<Meteor>()
            .ActivateOnEnter<Utsusemi>()
            .ActivateOnEnter<MijinGakure>()
            .ActivateOnEnter<HavokSpiral>()
            .ActivateOnEnter<SpiralFinish>()
            .ActivateOnEnter<Dragonfall>()
            .ActivateOnEnter<Guillotine>()
            .ActivateOnEnter<DominionSlash>()
            .ActivateOnEnter<CrossReaver>()
            .ActivateOnEnter<Rampage>()
            .ActivateOnEnter<Raiton>()
            .ActivateOnEnter<ArroganceIncarnate>()
            .Raw.Update = () => AllDeadOrDestroyed(A13ArkAngels.Bosses);
    }

    private void SinglePhase(uint id)
    {
        DecisiveBattle(id, 0.2f);
        Cloudsplitter(id + 0x1000u, 4.2f);
        MeikyoShisui(id + 0x20000u, 4.6f);
        Meteor(id + 0x30000u, 2.1f);
        HavocSpiral(id + 0x40000u, 3.2f);
        Dragonfall(id + 0x50000u, 5.6f);
        Guillotine(id + 0x60000u, 2.4f);

        Intermission(id + 0x100000u, 10.2f);
        UtsusemiDominionSlash(id + 0x110000u, 3.2f);
        Holy(id + 0x120000u, 4.2f);
        MijinGakure(id + 0x130000u, 7.9f);
        Rampage(id + 0x140000u, 5.8f);
        Guillotine(id + 0x150000u, 6.0f);
        MeikyoShisuiCrossReaverMeteor(id + 0x160000u, 4.4f);
        ArroganceIncarnate(id + 0x170000u, 4.2f);
        Cloudsplitter(id + 0x180000u, 1.0f);
        Rampage(id + 0x190000u, 12.7f);
        Guillotine(id + 0x1A0000u, 0.6f);
        CriticalReaver(id + 0x1B0000u, 7.0f);

        DominionSlashHavokSpiral(id + 0x200000u, 9.2f);
        Meteor(id + 0x210000u, 7.2f);
        MeikyoShisuiCrossReaver(id + 0x220000u, 5.1f);
        Cloudsplitter(id + 0x230000u, 3.0f);
        Raiton(id + 0x240000u, 8.2f);
        DominionSlashCrossReaver(id + 0x250000u, 2.9f);
        Dragonfall(id + 0x260000u, 1.3f);
        ArroganceIncarnate(id + 0x270000u, 15.2f);
        Rampage(id + 0x280000u, 15.1f);

        SimpleState(id + 0xFF0000u, 10000f, "???");
    }

    private void DecisiveBattle(uint id, float delay)
    {
        ActorCast(id, _module.BossMR, AID.DecisiveBattleMR, delay, 4f, true, "Assign target");
    }

    private void Cloudsplitter(uint id, float delay)
    {
        ActorCast(id, _module.BossMR, AID.Cloudsplitter, delay, 5f, true);
        ComponentCondition<Cloudsplitter>(id + 2u, 0.5f, static comp => comp.NumCasts > 0, "Tankbusters")
            .ExecOnExit<Cloudsplitter>(static comp => comp.NumCasts = 0)
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void MeikyoShisuiStart(uint id, float delay)
    {
        ActorCast(id, _module.BossGK, AID.MeikyoShisui, delay, 4f, true);
        ComponentCondition<TachiYukikaze>(id + 0x10u, 1.1f, static comp => comp.Casters.Count > 0);
        ComponentCondition<TachiYukikaze>(id + 0x11u, 3f, static comp => comp.Casters.Count == 0, "Criss-cross 1");
        ComponentCondition<TachiYukikaze>(id + 0x12u, 1.6f, static comp => comp.Casters.Count > 0);
        ComponentCondition<TachiGekko>(id + 0x13u, 2.4f, static comp => comp.NumCasts > 0, "Gaze")
            .ExecOnExit<TachiGekko>(static comp => comp.NumCasts = 0);
        ComponentCondition<TachiYukikaze>(id + 0x14u, 0.6f, static comp => comp.Casters.Count == 0, "Criss-cross 2");
        ComponentCondition<ConcertedDissolution>(id + 0x15u, 1.5f, static comp => comp.Casters.Count > 0);
        ComponentCondition<TachiKasha>(id + 0x16u, 2.9f, static comp => comp.NumCasts > 0, "Out")
            .ExecOnExit<TachiKasha>(static comp => comp.NumCasts = 0);
        ComponentCondition<LightsChain>(id + 0x17u, 0.6f, static comp => comp.Casters.Count > 0);
    }

    private void MeikyoShisui(uint id, float delay)
    {
        MeikyoShisuiStart(id, delay);
        ComponentCondition<ConcertedDissolution>(id + 0x100u, 2.5f, static comp => comp.NumCasts == 6, "Cones")
            .ExecOnExit<ConcertedDissolution>(static comp => comp.NumCasts = 0);
        ComponentCondition<LightsChain>(id + 0x110u, 5.5f, static comp => comp.NumCasts > 0, "Donut")
            .ExecOnExit<LightsChain>(static comp => comp.NumCasts = 0);
    }

    private void Meteor(uint id, float delay)
    {
        ActorCast(id, _module.BossTT, AID.Meteor, delay, 11f, true, "Interrupt", true)
            .OnExit(() => _module.Arena.Bounds = new ArenaBoundsCircle(25f)); // fall back for people who joined fight late
    }

    private State HavocSpiral(uint id, float delay)
    {
        ActorCastStart(id, _module.BossMR, AID.HavocSpiral, delay, true);
        ActorCastEnd(id + 1u, _module.BossMR, 5f);
        ComponentCondition<HavokSpiral>(id + 2u, 0.5f, static comp => comp.NumCasts > 0, "Rotation start");
        ActorCast(id + 0x10u, _module.BossMR, AID.SpiralFinish, 0.6f, 11, true)
            .ExecOnExit<HavokSpiral>(static comp => comp.NumCasts = 0);
        return ComponentCondition<SpiralFinish>(id + 0x12u, 0.5f, static comp => comp.NumCasts > 0, "Knockback")
            .ExecOnExit<SpiralFinish>(static comp => comp.NumCasts = 0);
    }

    private void Dragonfall(uint id, float delay)
    {
        ActorCast(id, _module.BossGK, AID.Dragonfall, delay, 9, true);
        ComponentCondition<Dragonfall>(id + 2u, 0.3f, static comp => comp.NumCasts > 0, "Stack 1");
        ComponentCondition<Dragonfall>(id + 3u, 2.4f, static comp => comp.NumCasts > 1, "Stack 2");
        ComponentCondition<Dragonfall>(id + 4u, 2.4f, static comp => comp.NumCasts > 2, "Stack 3")
            .ExecOnExit<Dragonfall>(static comp => comp.NumCasts = 0);
    }

    private void Guillotine(uint id, float delay)
    {
        ActorCast(id, _module.BossTT, AID.Guillotine, delay, 10.5f, true);
        ComponentCondition<Guillotine>(id + 2u, 0.6f, static comp => comp.NumCasts > 0, "Cone start");
        ComponentCondition<Guillotine>(id + 0x10u, 3.4f, static comp => comp.NumCasts >= 4, "Cone resolve")
            .ExecOnExit<Guillotine>(static comp => comp.NumCasts = 0);
    }

    private void Intermission(uint id, float delay)
    {
        Targetable(id, false, delay, "Bosses disappear")
            .DeactivateOnExit<ArenaChange>()
            .DeactivateOnExit<DecisiveBattle>();
        ActorTargetable(id + 1u, _module.BossHM, true, 4.3f, "Bosses appear")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    private void UtsusemiDominionSlash(uint id, float delay)
    {
        ActorCast(id, _module.BossHM, AID.Utsusemi, delay, 3);
        ActorCastEnd(id + 2u, _module.BossEV, 2f, false, "Raidwide") // dominion slash starts at the same time
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<DominionSlash>(id + 3u, 0.7f, static comp => comp.AOEs.Count > 0);
        ActorCast(id + 0x10u, _module.BossHM, AID.MightyStrikesClones, 0.5f, 5);
        ActorCast(id + 0x20u, _module.BossHM, AID.CrossReaver, 6.3f, 3);
        ComponentCondition<CrossReaver>(id + 0x22u, 1f, static comp => comp.Casters.Count > 0);
        ComponentCondition<DominionSlash>(id + 0x23u, 2.3f, static comp => comp.AOEs.Count == 0);
        ComponentCondition<CrossReaver>(id + 0x24u, 3.7f, static comp => comp.NumCasts > 0, "Cross")
            .ExecOnExit<CrossReaver>(static comp => comp.NumCasts = 0);
    }

    private void Holy(uint id, float delay)
    {
        ActorCast(id, _module.BossEV, AID.Holy, delay, 5f, true, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void MijinGakure(uint id, float delay)
    {
        ActorTargetable(id, _module.BossEV, false, delay, "Shield");
        ActorCast(id + 0x10u, _module.BossHM, AID.MijinGakure, 1f, 30f, true, "Interrupt", true);
    }

    private void Rampage(uint id, float delay)
    {
        ActorCast(id, _module.BossMR, AID.Rampage, delay, 8, true);
        ComponentCondition<Rampage>(id + 0x10u, 0.2f, static comp => comp.NumCasts > 0, "Charges start");
        ComponentCondition<Rampage>(id + 0x20u, 5.2f, static comp => comp.NumCasts > 4, "Charges resolve")
            .ExecOnExit<Rampage>(static comp => comp.NumCasts = 0);
    }

    private void MeikyoShisuiCrossReaverStart(uint id, float delay)
    {
        MeikyoShisuiStart(id, delay);

        ActorCastStart(id + 0x100u, _module.BossHM, AID.CrossReaver, 1.9f, true);
        ComponentCondition<ConcertedDissolution>(id + 0x101u, 0.6f, static comp => comp.NumCasts == 6, "Cones")
            .ExecOnExit<ConcertedDissolution>(static comp => comp.NumCasts = 0);
        ActorCastEnd(id + 0x102u, _module.BossHM, 2.4f, true);
        ComponentCondition<CrossReaver>(id + 0x103u, 1, static comp => comp.Casters.Count > 0);
        ComponentCondition<LightsChain>(id + 0x104u, 2.1f, static comp => comp.NumCasts > 0, "Donut")
            .ExecOnExit<LightsChain>(static comp => comp.NumCasts = 0);
    }

    private void MeikyoShisuiCrossReaver(uint id, float delay)
    {
        MeikyoShisuiCrossReaverStart(id, delay);
        ComponentCondition<CrossReaver>(id + 0x200u, 3.9f, static comp => comp.NumCasts > 0, "Cross")
            .ExecOnExit<CrossReaver>(static comp => comp.NumCasts = 0);
    }

    private void MeikyoShisuiCrossReaverMeteor(uint id, float delay)
    {
        MeikyoShisuiCrossReaverStart(id, delay);

        ActorCastStart(id + 0x200u, _module.BossTT, AID.Meteor, 1.1f, true);
        ComponentCondition<CrossReaver>(id + 0x201u, 2.8f, static comp => comp.NumCasts > 0, "Cross")
            .ExecOnExit<CrossReaver>(static comp => comp.NumCasts = 0);
        ActorCastEnd(id + 0x202u, _module.BossTT, 8.2f, true, "Interrupt", true);
    }

    private void ArroganceIncarnate(uint id, float delay)
    {
        ActorCastStart(id, _module.BossEV, AID.ArroganceIncarnate, delay, true);
        ActorCastEnd(id + 1u, _module.BossEV, 5f, true);
        ComponentCondition<ArroganceIncarnate>(id + 2u, 0.7f, static comp => comp.NumFinishedStacks > 0, "Stack 1");
        ComponentCondition<ArroganceIncarnate>(id + 0x10u, 4.3f, static comp => comp.NumFinishedStacks >= 5, "Stack 5")
            .ExecOnExit<ArroganceIncarnate>(static comp => comp.NumFinishedStacks = 0);
    }

    private void CriticalReaver(uint id, float delay)
    {
        ActorCast(id, _module.BossHM, AID.MightyStrikesBoss, delay, 5f, true);
        ComponentCondition<CriticalReaverRaidwide>(id + 0x10u, 2.1f, static comp => comp.NumCasts >= 1, "Raidwide 1")
            .ActivateOnEnter<CriticalReaverRaidwide>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<CriticalReaverRaidwide>(id + 0x11u, 2.1f, static comp => comp.NumCasts >= 2, "Raidwide 2")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<CriticalReaverRaidwide>(id + 0x12u, 2.1f, static comp => comp.NumCasts >= 3, "Raidwide 3")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<CriticalReaverRaidwide>(id + 0x13u, 2.1f, static comp => comp.NumCasts >= 4, "Raidwide 4")
            .DeactivateOnExit<CriticalReaverRaidwide>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorCast(id + 0x20u, _module.BossHM, AID.CriticalReaverEnrage, 2.1f, 10f, true, "Interrupt", true)
            .ActivateOnEnter<CriticalReaverEnrage>()
            .DeactivateOnExit<CriticalReaverEnrage>();
    }

    private void DominionSlashHavokSpiral(uint id, float delay)
    {
        ActorCast(id, _module.BossEV, AID.DominionSlash, delay, 5f, true, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<DominionSlash>(id + 2u, 0.7f, static comp => comp.AOEs.Count > 0);
        HavocSpiral(id + 0x100u, 2.6f);
    }

    private void Raiton(uint id, float delay)
    {
        ActorCast(id, _module.BossHM, AID.Raiton, delay, 5f, true, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DominionSlashCrossReaver(uint id, float delay)
    {
        ActorCast(id, _module.BossEV, AID.DominionSlash, delay, 5f, true, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<DominionSlash>(id + 2u, 0.7f, static comp => comp.AOEs.Count > 0);
        ActorCast(id + 0x10u, _module.BossHM, AID.CrossReaver, 8.8f, 3, true);
        ComponentCondition<CrossReaver>(id + 0x20u, 1, static comp => comp.Casters.Count > 0);
        ComponentCondition<CrossReaver>(id + 0x30u, 6, static comp => comp.NumCasts > 0, "Cross")
            .ExecOnExit<CrossReaver>(static comp => comp.NumCasts = 0);
    }
}
