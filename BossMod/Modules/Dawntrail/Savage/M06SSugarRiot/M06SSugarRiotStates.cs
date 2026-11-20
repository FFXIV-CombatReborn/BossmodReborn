namespace BossMod.Dawntrail.Savage.M06SSugarRiot;

sealed class M06SSugarRiotStates : StateMachineBuilder
{
    public M06SSugarRiotStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<ColorRiot>();
    }

    private void SinglePhase(uint id)
    {
        MousseMural(id, 6.1f);
        ColorRiot(id + 0x10000u, 13.2f, 2);
        DoubleStyle(id + 0x20000u, 6.6f);
        StickyMousse(id + 0x30000u, 2.3f);
        ColorRiot(id + 0x40000u, 7.5f, 4);
        SugarscapeDesert(id + 0x50000u, 6.4f);
        ColorRiot(id + 0x60000u, 9.5f, 6);
        AddPhase(id + 0x70000u, 5.1f);
        ColorRiot(id + 0x80000u, 11.2f, 8);
        MousseMural(id + 0x90000u, 1.1f);
        SugarscapeRiver(id + 0xA0000u, 8.4f);
        LavaArena(id + 0xB0000u, 15.2f);
        MousseMural(id + 0xC0000u, 0f);
        LavaArenaEnd(id + 0xD0000u, 0.8f);
        StickyMousse(id + 0xE0000u, 4.3f);
        ColorRiot(id + 0xF0000u, 7.5f, 10);
        DoubleStyle(id + 0x100000u, 6.6f);
        SimpleState(id + 0x110000u, 23.5f, "Enrage");
    }

    private void MousseMural(uint id, float delay)
    {
        Cast(id, AID.MousseMural, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void ColorRiot(uint id, float delay, int numcasts)
    {
        ComponentCondition<ColorRiot>(id, delay, comp => comp.NumCasts == numcasts, "Proximity tankbusters")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void DoubleStyle(uint id, float delay)
    {
        Cast(id, AID.Wingmark, delay, 4f, "Apply wingmarks")
            .ActivateOnEnter<Wingmark>();
        CastMulti(id + 0x10u, [AID.ColorClashVisual1, AID.ColorClashVisual2], 3.2f, 3f, "Store stack kind")
            .ActivateOnEnter<ColorClash>();
        ComponentCondition<SingleDoubleStyle1>(id + 0x20u, 7.2f, static comp => comp.AOEs.Count != 0, "Double Style starts")
            .ActivateOnEnter<SingleDoubleStyle1>();
        ComponentCondition<Wingmark>(id + 0x30u, 8.6f, static comp => comp.StunStatus != default, "Stun");
        ComponentCondition<Wingmark>(id + 0x40u, 3f, static comp => comp.StunStatus == default, "Knockback resolves");
        ComponentCondition<SingleDoubleStyle1>(id + 0x50u, 1.5f, static comp => comp.NumCasts != 0, "Double Style resolves")
            .DeactivateOnExit<Wingmark>()
            .DeactivateOnExit<SingleDoubleStyle1>();
        ComponentCondition<ColorClash>(id + 0x60u, 0.7f, static comp => comp.Stacks.Count == 0, "Stored stacks resolve")
            .DeactivateOnExit<ColorClash>();
    }

    private void StickyMousse(uint id, float delay)
    {
        ComponentCondition<StickyMousse>(id, delay, static comp => comp.Spreads.Count != 0, "Spreads appear")
            .ActivateOnEnter<StickyMousse>();
        ComponentCondition<StickyMousse>(id + 0x10u, 5.8f, static comp => comp.Stacks.Count != 0, "Spreads resolve");
        ComponentCondition<StickyMousse>(id + 0x20u, 6f, static comp => comp.NumCasts != 0, "Light party stacks resolve")
            .DeactivateOnExit<StickyMousse>();
    }

    private void SugarscapeDesert(uint id, float delay)
    {
        Cast(id, AID.Sugarscape1, delay, 1f, "Desert painting")
            .ActivateOnEnter<Sweltering>()
            .ActivateOnEnter<HeatingBurningUp>()
            .ActivateOnEnter<SprayPain1>();
        ComponentCondition<Sweltering>(id + 0x10u, 7f, static comp => comp.SwelteringStatus != default, "Bleed starts + defamation debuffs");
        for (var i = 1; i <= 6; ++i)
        {
            var offset = id + 0x20u + (uint)((i - 1) * 0x10u);
            var time = i == 1 ? 23.4f : 3f;
            var desc = $"Circle AOEs {i}";
            var casts = i * 5;
            var cond = ComponentCondition<SprayPain1>(offset, time, comp => comp.NumCasts == casts, desc);
            if (i == 6)
            {
                cond.DeactivateOnExit<SprayPain1>();
            }
        }
        ComponentCondition<HeatingBurningUp>(id + 0x90u, 4.5f, static comp => comp.NumCasts == 3, "Defamation 1 + stack resolve");
        ComponentCondition<StickyMousse>(id + 0xA0u, 2.6f, static comp => comp.Spreads.Count != 0, "Spreads appear")
            .ActivateOnEnter<StickyMousse>();
        ComponentCondition<StickyMousse>(id + 0xB0u, 5.7f, static comp => comp.Stacks.Count != 0, "Spreads resolve");
        ComponentCondition<StickyMousse>(id + 0xC0u, 6f, static comp => comp.NumCasts != 0, "Light party stacks resolve")
            .DeactivateOnExit<StickyMousse>();
        ComponentCondition<Quicksand>(id + 0xD0u, 8.7f, static comp => comp.AOE.Length != 0, "Quicksand 1 appears")
            .ActivateOnEnter<SprayPain2>()
            .ActivateOnEnter<QuicksandDoubleStyleHeavenBomb>()
            .ActivateOnEnter<QuicksandDoubleStylePaintBomb>()
            .ActivateOnEnter<PaintBombHeavenBomb>()
            .ActivateOnEnter<Quicksand>();
        ComponentCondition<HeatingBurningUp>(id + 0xE0u, 7.9f, static comp => comp.NumCasts == 5, "Defamation 2 resolves")
            .DeactivateOnExit<HeatingBurningUp>();
        ComponentCondition<SprayPain2>(id + 0xF0u, 0.7f, static comp => comp.NumCasts != 0, "Circle AOEs")
            .DeactivateOnExit<SprayPain2>();
        ComponentCondition<QuicksandDoubleStylePaintBomb>(id + 0x100u, 10.9f, static comp => comp.Targets != default, "Bomb baits start")
            .ActivateOnExit<PuddingGraf>();
        ComponentCondition<PuddingGraf>(id + 0x110u, 6f, static comp => comp.NumFinishedSpreads != 0, "Spreads resolve")
            .DeactivateOnExit<PuddingGraf>();
        ComponentCondition<QuicksandDoubleStylePaintBomb>(id + 0x120u, 0.1f, static comp => comp.Targets == default, "Bomb baits resolve");
        Cast(id + 0x130u, AID.MousseMural, 4.1f, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<QuicksandDoubleStyleHeavenBomb>()
            .DeactivateOnExit<QuicksandDoubleStylePaintBomb>()
            .DeactivateOnExit<PaintBombHeavenBomb>()
            .DeactivateOnExit<Quicksand>();
        ComponentCondition<Sweltering>(id + 0x140u, 0.7f, static comp => comp.SwelteringStatus == default, "Bleed ends")
            .DeactivateOnExit<Sweltering>();
    }

    private void AddPhase(uint id, float delay)
    {
        Cast(id, AID.SoulSugar, delay, 3f, "Add Phase")
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<HangryHiss>()
            .ActivateOnEnter<HuffyCatHint>();
        ComponentCondition<Adds>(id + 0x10u, 10.2f, static comp => comp.ActiveActorsCount != 0, "2x Mu + 1x Yan targetable");
        ComponentCondition<Adds>(id + 0x20u, 2f, static comp => comp.ActiveActorsCount > 3, "GimmeCat targetable")
            .ActivateOnEnter<ICraveViolence>()
            .ActivateOnEnter<OreRigato>();
        ComponentCondition<Adds>(id + 0x30u, 25.1f, static comp => comp.CountMu == 4 && comp.CountFeatherRay == 2, "2x Mu + 2x Featheray spawn")
            .ActivateOnEnter<WaterIIIVoidzone>()
            .ActivateOnEnter<WaterIIIBait>();
        ComponentCondition<Adds>(id + 0x40u, 22.2f, static comp => comp.CountYan == 2 && comp.CountGimmeCat == 2 && comp.CountJabberwock == 1, "Yan, GimmeCat, Jabberwock spawn")
            .ActivateOnEnter<ManxomeWindersnatch>();
        Cast(id + 0x45u, AID.ReadyOreNot, 22f, 7f, "Raidwide") // was 0x40, which duplicated the adds condition above
            .ActivateOnEnter<ReadyOreNot>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Adds>(id + 0x50u, 10.1f, static comp => comp.CountYan == 3 && comp.CountGimmeCat == 3 && comp.CountJabberwock == 2 && comp.CountMu == 6 && comp.CountFeatherRay == 4, "Yan, GimmeCat, Jabberwock, 2x Mu, 2x FeatherRay spawn");
        Cast(id + 0x60u, AID.ReadyOreNot, 66.1f, 7f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<ReadyOreNot>()
            .ActivateOnExit<SingleDoubleStyle1>();
        ComponentCondition<SingleDoubleStyle1>(id + 0x70u, 13.3f, static comp => comp.NumCasts != 0, "Single Style resolves")
            .DeactivateOnExit<ICraveViolence>()
            .DeactivateOnExit<WaterIIIBait>()
            .DeactivateOnExit<WaterIIIVoidzone>()
            .DeactivateOnExit<SingleDoubleStyle1>()
            .DeactivateOnExit<ManxomeWindersnatch>()
            .DeactivateOnExit<OreRigato>()
            .DeactivateOnExit<Adds>();
    }

    private void SugarscapeRiver(uint id, float delay)
    {
        Cast(id, AID.Sugarscape2, delay, 1f, "River painting")
            .ActivateOnEnter<ArenaChanges>();
        ComponentCondition<SingleDoubleStyle1>(id + 0x10u, 24.3f, static comp => comp.NumCasts != 0, "Single Style resolves")
            .ActivateOnEnter<TasteOfThunderFire>()
            .ActivateOnEnter<SingleDoubleStyle1>()
            .DeactivateOnExit<SingleDoubleStyle1>();
        ComponentCondition<TasteOfThunderFire>(id + 0x20u, 0.1f, static comp => comp.NumCasts != 0, "Spreads OR stacks resolve")
            .DeactivateOnExit<TasteOfThunderFire>();
        ComponentCondition<ArenaChanges>(id + 0x30u, 8.2f, static comp => comp.DangerousRiver, "River becomes dangerous")
            .ActivateOnEnter<Highlightning>()
            .ActivateOnExit<LightningStorm>()
            .ActivateOnExit<LightningStormHint>()
            .ActivateOnEnter<TasteOfThunderAOE>();
        ComponentCondition<TasteOfThunderAOE>(id + 0x40u, 6.2f, static comp => comp.NumCasts != 0, "Small circle AOEs 1")
            .DeactivateOnExit<TasteOfThunderAOE>();
        ComponentCondition<Highlightning>(id + 0x50u, 1.9f, static comp => comp.NumCasts != 0, "Big circle AOE 1");
        ComponentCondition<LightningBolt>(id + 0x60u, 4.6f, static comp => comp.NumCasts != 0, "Small circle AOEs 2")
            .ActivateOnEnter<LightningBolt>();
        ComponentCondition<LightningBolt>(id + 0x70u, 2.6f, static comp => comp.NumCasts > 6, "Small circle AOEs 3");
        ComponentCondition<LightningStorm>(id + 0x80u, 3.5f, static comp => comp.NumCasts != 0, "Baited AOEs 1 + big circle AOE 2");
        ComponentCondition<LightningStormHint>(id + 0x90u, 0.7f, static comp => comp.NumCasts != 0, "AOE on grass 1");
        ComponentCondition<LightningBolt>(id + 0xA0u, 3.8f, static comp => comp.NumCasts > 12, "Small circle AOEs 4");
        ComponentCondition<LightningBolt>(id + 0xB0u, 3.8f, static comp => comp.NumCasts > 18, "Small circle AOEs 5");
        ComponentCondition<LightningStorm>(id + 0xC0u, 3.5f, static comp => comp.NumCasts > 2, "Baited AOEs 2 + big circle AOE 3");
        ComponentCondition<LightningStormHint>(id + 0xD0u, 0.6f, static comp => comp.NumCasts > 2, "AOE on grass 2");
        ComponentCondition<LightningBolt>(id + 0xE0u, 3.8f, static comp => comp.NumCasts > 24, "Small circle AOEs 6");
        ComponentCondition<LightningBolt>(id + 0xF0u, 2.6f, static comp => comp.NumCasts > 30, "Small circle AOEs 7");
        ComponentCondition<LightningStorm>(id + 0x100u, 3.5f, static comp => comp.NumCasts > 4, "Baited AOEs 3");
        ComponentCondition<Highlightning>(id + 0x110u, 0.3f, static comp => comp.NumCasts == 4, "Big circle AOE 4");
        ComponentCondition<LightningStormHint>(id + 0x120u, 0.4f, static comp => comp.NumCasts > 4, "AOE on grass 3");
        ComponentCondition<LightningBolt>(id + 0x130u, 3.8f, static comp => comp.NumCasts > 36, "Small circle AOEs 8");
        ComponentCondition<LightningBolt>(id + 0x140u, 2.6f, static comp => comp.NumCasts > 42, "Small circle AOEs 9");
        ComponentCondition<LightningStorm>(id + 0x150u, 3.5f, static comp => comp.NumCasts > 6, "Baited AOEs 4")
            .DeactivateOnExit<LightningStorm>();
        ComponentCondition<Highlightning>(id + 0x160u, 0.3f, static comp => comp.NumCasts == 5, "Big circle AOE 5")
            .DeactivateOnExit<LightningBolt>()
            .DeactivateOnExit<Highlightning>();
        ComponentCondition<LightningStormHint>(id + 0x170u, 0.3f, static comp => comp.NumCasts > 6, "AOE on grass 4")
            .DeactivateOnExit<LightningStormHint>();
        ComponentCondition<PuddingParty>(id + 0x180u, 5.5f, static comp => comp.NumCasts != 0, "Stack hit 1")
            .ActivateOnEnter<PuddingParty>();
        ComponentCondition<PuddingParty>(id + 0x190u, 4.2f, static comp => comp.NumCasts == 5, "Stack hit 5")
            .DeactivateOnExit<PuddingParty>();
    }

    private void LavaArena(uint id, float delay)
    {
        ComponentCondition<ArenaChanges>(id, delay, static comp => comp.DangerousLava, "Bridges disappear, lava arena")
            .ActivateOnEnter<MousseDripTowers>();
        ComponentCondition<MousseDripStack>(id + 0x10u, 10.1f, static comp => comp.Stacks.Count != 0, "Towers 1 + partner stacks appear")
            .ActivateOnEnter<Moussacre>()
            .ActivateOnEnter<MousseDripVoidzone>()
            .ActivateOnEnter<MousseDripStack>();
        ComponentCondition<MousseDripStack>(id + 0x20u, 5.2f, static comp => comp.NumCasts != 0, "Partner stacks hit 1");
        ComponentCondition<MousseDripStack>(id + 0x30u, 2f, static comp => comp.NumCasts > 2, "Partner stacks hit 2");
        ComponentCondition<MousseDripStack>(id + 0x40u, 2f, static comp => comp.NumCasts > 4, "Partner stacks hit 3");
        ComponentCondition<Moussacre>(id + 0x50u, 1.8f, static comp => comp.NumCasts != 0, "Baited cones")
            .DeactivateOnExit<Moussacre>();
        ComponentCondition<MousseDripStack>(id + 0x60u, 0.3f, static comp => comp.NumCasts > 6, "Partner stacks hit 4")
            .ActivateOnEnter<TasteOfThunderAOE>()
            .DeactivateOnExit<MousseDripStack>();
        ComponentCondition<TasteOfThunderAOE>(id + 0x70u, 6f, static comp => comp.NumCasts != 0, "Small circle AOEs 1")
            .DeactivateOnExit<TasteOfThunderAOE>();
        ComponentCondition<MousseDripTowers>(id + 0x80u, 1.8f, static comp => comp.Towers.Count == 0, "Towers 1 resolve");
        Cast(id + 0x90u, AID.Wingmark, 0.3f, 4f, "Apply wingmarks")
            .ActivateOnEnter<Wingmark>();
        ComponentCondition<MousseDripTowers>(id + 0xA0u, 3.1f, static comp => comp.Towers.Count != 0, "Towers 2 appear")
            .DeactivateOnEnter<MousseDripVoidzone>();
        ComponentCondition<Wingmark>(id + 0xB0u, 8f, static comp => comp.StunStatus != default, "Stun");
        ComponentCondition<Wingmark>(id + 0xC0u, 3f, static comp => comp.StunStatus == default, "Knockback resolves")
            .DeactivateOnExit<Wingmark>();
        ComponentCondition<TasteOfThunderAOE>(id + 0xD0u, 6.2f, static comp => comp.NumCasts != 0, "Small circle AOEs 2")
            .ActivateOnEnter<TasteOfThunderAOE>()
            .DeactivateOnExit<TasteOfThunderAOE>();
        ComponentCondition<MousseDripTowers>(id + 0xE0u, 1.9f, static comp => comp.Towers.Count == 0, "Towers 2 resolve")
            .DeactivateOnExit<MousseDripTowers>();
    }

    private void LavaArenaEnd(uint id, float delay)
    {
        ComponentCondition<ArenaChanges>(id, delay, static comp => !comp.DangerousLava, "Arena returns to normal")
            .DeactivateOnExit<ArenaChanges>();
    }
}
