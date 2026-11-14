namespace BossMod.Dawntrail.Criterion.C01AMT.C011DaryaTheSeaMaid;

sealed class C011DaryaTheSeaMaidStates : StateMachineBuilder
{
    public C011DaryaTheSeaMaidStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        FamiliarCall1(id, 7.2f);
        AlluringOrder1(id + 0x10000u, 6.2f);
        CeaselessCurrent(id + 0x20000u, 7.2f);
        AlluringOrder2(id + 0x30000u, 7.2f);
        AquaSpear(id + 0x40000u, 6.1f);
        SeaShackles(id + 0x50000u, 5.3f);
        Cast(id + 0x60000u, AID.PiercingPlungeEnrage, 9.6f, 10f, "Enrage");
    }

    private void PiercingPlunge(uint id, float delay)
    {
        Cast(id, AID.PiercingPlunge, delay, 5f, "Raidwide")
            .ActivateOnEnter<PiercingPlunge>()
            .DeactivateOnExit<PiercingPlunge>();
    }

    private void FamiliarCall1(uint id, float delay)
    {
        PiercingPlunge(id, delay);

        Cast(id + 0x100u, AID.FamiliarCall, 10.3f, 3f, "Adds spawn")
            .ActivateOnEnter<FamiliarCall>()
            .ActivateOnEnter<Hydrobullet>()
            .ActivateOnEnter<SurgingCurrent>()
            .ExecOnEnter<SurgingCurrent>(static c => c.Risky = false);
        Cast(id + 0x110u, AID.EchoedSerenade, 5.1f, 8.5f);
        ComponentCondition<FamiliarCall>(id + 0x120u, 3.6f, static c => c.NumCasts > 0, "Adds 1");
        ComponentCondition<FamiliarCall>(id + 0x121u, 3.1f, static c => c.NumCasts > 1, "Adds 2");
        ComponentCondition<Hydrobullet>(id + 0x122u, 0.1f, static h => h.NumFinishedSpreads > 0, "Spreads");
        ComponentCondition<FamiliarCall>(id + 0x123u, 3f, static c => c.NumCasts > 2, "Adds 3");
        ComponentCondition<FamiliarCall>(id + 0x124u, 3.1f, static c => c.NumCasts > 3, "Adds 4");
        ComponentCondition<Hydrobullet>(id + 0x125u, 0.1f, static h => h.NumFinishedSpreads > 4, "Spreads")
            .DeactivateOnExit<FamiliarCall>()
            .DeactivateOnExit<Hydrobullet>()
            .ExecOnExit<SurgingCurrent>(static c => c.Risky = true);

        ComponentCondition<SurgingCurrent>(id + 0x200u, 4.9f, static s => s.NumCasts > 0, "Diagonals 1");
        ComponentCondition<SurgingCurrent>(id + 0x201u, 3.1f, static s => s.NumCasts > 2, "Diagonals 2")
            .DeactivateOnExit<SurgingCurrent>();
    }

    private void AlluringOrder1(uint id, float delay)
    {
        Cast(id, AID.AlluringOrder, delay, 4f, "Raidwide")
            .ActivateOnEnter<AlluringOrder>()
            .ActivateOnEnter<AlluringOrderForcedMarch>()
            .ActivateOnEnter<Tidalspout>()
            .DeactivateOnExit<AlluringOrder>()
            .ExecOnEnter<Tidalspout>(static t => t.EnableHints = false);

        Cast(id + 0x10u, AID.SwimmingInTheAir, 5.5f, 4f)
            .ActivateOnEnter<SwimmingInTheAir>();

        ComponentCondition<AlluringOrderForcedMarch>(id + 0x20u, 9.7f, static f => f.NumActiveForcedMarches > 0, "Forced march")
            .ExecOnExit<Tidalspout>(static t => t.EnableHints = true)
            .ExecOnExit<SwimmingInTheAir>(static t => t.Risky = true);

        ComponentCondition<SwimmingInTheAir>(id + 0x30u, 3.9f, static h => h.NumCasts > 0, "Puddles")
            .DeactivateOnExit<SwimmingInTheAir>();
        ComponentCondition<Tidalspout>(id + 0x31u, 0.2f, static t => !t.Active, "Stacks")
            .DeactivateOnExit<Tidalspout>()
            .DeactivateOnExit<AlluringOrderForcedMarch>();
    }

    private void CeaselessCurrent(uint id, float delay)
    {
        Cast(id, AID.CeaselessCurrent, delay, 4f)
            .ActivateOnEnter<CrossCurrent>()
            .ActivateOnEnter<CeaselessCurrent>()
            .ActivateOnEnter<SurgingCurrent>();

        ComponentCondition<CeaselessCurrent>(id + 0x10u, 8.1f, static c => c.NumCasts > 0, "Waves start");
        ComponentCondition<CrossCurrent>(id + 0x11u, 4.2f, static c => c.NumCasts > 0, "Crosses + diagonals")
            .DeactivateOnExit<CrossCurrent>();
        ComponentCondition<SurgingCurrent>(id + 0x12u, 2.9f, static s => s.NumCasts > 2, "Diagonals 2")
            .DeactivateOnExit<SurgingCurrent>();
        ComponentCondition<CeaselessCurrent>(id + 0x20u, 1.3f, static c => c.NumCasts >= 10, "Waves end")
            .DeactivateOnExit<CeaselessCurrent>();

        PiercingPlunge(id + 0x100u, 7f);
    }

    private void AlluringOrder2(uint id, float delay)
    {
        Cast(id, AID.AlluringOrder, delay, 4f, "Raidwide")
            .ActivateOnEnter<AlluringOrder>()
            .ActivateOnEnter<AlluringOrderForcedMarch>()
            .ActivateOnEnter<Tidalspout>()
            .DeactivateOnExit<AlluringOrder>()
            .ExecOnEnter<AlluringOrderForcedMarch>(static m => m.EnableHints = false)
            .ExecOnEnter<Tidalspout>(static m => m.EnableHints = false);

        Cast(id + 0x10u, AID.SunkenTreasure, 5.2f, 3f, "Orbs appear")
            .ActivateOnEnter<SunkenTreasure>()
            .ActivateOnEnter<SurgingCurrent>()
            .ExecOnEnter<SurgingCurrent>(static c => c.Risky = false);

        ComponentCondition<AlluringOrderForcedMarch>(id + 0x20u, 13f, static f => f.NumActiveForcedMarches > 0, "Forced march")
            .ExecOnEnter<AlluringOrderForcedMarch>(static m => m.EnableHints = true)
            .ExecOnEnter<SurgingCurrent>(static c => c.Risky = true)
            .ExecOnExit<SunkenTreasure>(static s => s.Risky = true)
            .ExecOnExit<Tidalspout>(static s => s.EnableHints = true);

        ComponentCondition<Tidalspout>(id + 0x21u, 4.1f, static t => !t.Active, "Stacks")
            .DeactivateOnExit<Tidalspout>()
            .DeactivateOnExit<AlluringOrderForcedMarch>();
        ComponentCondition<SunkenTreasure>(id + 0x22u, 0.1f, static s => s.NumCasts > 0, "Diagonals 1 + orbs");
        ComponentCondition<SurgingCurrent>(id + 0x23u, 3.1f, static s => s.NumCasts > 2, "Diagonals 2")
            .DeactivateOnExit<SurgingCurrent>();
        ComponentCondition<SunkenTreasure>(id + 0x24u, 3.4f, static s => s.NumCasts > 5, "Orb")
            .DeactivateOnExit<SunkenTreasure>();
    }

    private void AquaSpear(uint id, float delay)
    {
        Cast(id, AID.AquaSpear, delay, 3f)
            .ActivateOnEnter<AquaSpearCast>()
            .ActivateOnEnter<AquaSpearPlayer>()
            .ActivateOnEnter<AquaSpearTile>();

        ComponentCondition<AquaSpearTile>(id + 0x10u, 7f, static t => t.Active, "Tiles")
            .DeactivateOnExit<AquaSpearCast>()
            .DeactivateOnExit<AquaSpearPlayer>();

        Cast(id + 0x20u, AID.FamiliarCall, 4.3f, 3f, "Adds spawn")
            .ActivateOnEnter<FamiliarCall2>()
            .ActivateOnEnter<EchoedReprise>()
            .ActivateOnEnter<AquaBall>()
            .ActivateOnEnter<CrossCurrent>();

        Cast(id + 0x30u, AID.EchoedSerenade, 5.2f, 8.5f);

        ComponentCondition<FamiliarCall2>(id + 0x40u, 3.7f, static f => f.NumCasts > 0, "Adds 1");
        ComponentCondition<FamiliarCall2>(id + 0x41u, 3.1f, static f => f.NumCasts > 1, "Adds 2");
        ComponentCondition<FamiliarCall2>(id + 0x42u, 3.1f, static f => f.NumCasts > 2, "Adds 3");
        ComponentCondition<FamiliarCall2>(id + 0x43u, 3.1f, static f => f.NumCasts > 3, "Adds 4")
            .DeactivateOnExit<FamiliarCall2>();

        ComponentCondition<CrossCurrent>(id + 0x50u, 5.8f, static c => c.NumCasts > 0, "Crosses")
            .DeactivateOnExit<CrossCurrent>();

        Cast(id + 0x100u, AID.FamiliarCall, 8.1f, 3f);
        Cast(id + 0x110u, AID.EchoedReprise, 6.2f, 4f)
            .ExecOnEnter<EchoedReprise>(static s => s.Activate());

        ComponentCondition<EchoedReprise>(id + 0x200u, 3.6f, static s => s.NumCasts > 0, "Repeat 1");
        ComponentCondition<EchoedReprise>(id + 0x201u, 3.1f, static s => s.NumCasts > 1, "Repeat 2");
        ComponentCondition<EchoedReprise>(id + 0x202u, 3.1f, static s => s.NumCasts > 2, "Repeat 3");
        ComponentCondition<EchoedReprise>(id + 0x203u, 3.1f, static s => s.NumCasts > 3, "Repeat 4")
            .DeactivateOnExit<EchoedReprise>()
            .DeactivateOnExit<AquaBall>();

        ComponentCondition<SurgingCurrent>(id + 0x210u, 2.8f, static s => s.NumCasts > 0, "Diagonals 1")
            .ActivateOnEnter<SurgingCurrent>();
        ComponentCondition<SurgingCurrent>(id + 0x211u, 3f, static s => s.NumCasts > 2, "Diagonals 2")
            .DeactivateOnExit<SurgingCurrent>();
    }

    private void SeaShackles(uint id, float delay)
    {
        Cast(id, AID.SeaShackles, delay, 4f)
            .ActivateOnEnter<AquaBall>()
            .ActivateOnEnter<HydrobulletStack>()
            .ActivateOnEnter<SunkenTreasure>()
            .ActivateOnEnter<SeaShackles>()
            .ExecOnExit<HydrobulletStack>(static s => s.EnableHints = false)
            .DeactivateOnExit<AquaSpearTile>();

        ComponentCondition<SeaShackles>(id + 0x2u, 1.1f, static s => s.Active, "Tethers appear");

        Cast(id + 0x10u, AID.SunkenTreasure, 2.1f, 3f, "Orbs appear");

        ComponentCondition<AquaBall>(id + 0x20u, 11.1f, static b => b.Casters.Count > 0, "Puddle baits start");

        ComponentCondition<AquaBall>(id + 0x100u, 4.1f, static b => b.NumBaits == 3)
            .ExecOnExit<SunkenTreasure>(static s => s.Risky = true);

        ComponentCondition<SunkenTreasure>(id + 0x110u, 1.9f, static s => s.NumCasts > 0, "Orbs 1");
        ComponentCondition<SunkenTreasure>(id + 0x111u, 6.5f, static s => s.NumCasts > 6, "Orbs 2")
            .DeactivateOnExit<SunkenTreasure>()
            .DeactivateOnExit<AquaBall>()
            .DeactivateOnExit<HydrobulletStack>()
            .DeactivateOnExit<SeaShackles>();

        PiercingPlunge(id + 0x200u, 4.7f);
    }
}
