namespace BossMod.Dawntrail.Savage.M1SBlackCat;

class M1SBlackCatStates : StateMachineBuilder
{
    // TODO: Fix naming where appropriate
    public M1SBlackCatStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<DoubleSwipe>()
            .ActivateOnEnter<QuadrupleSwipe>()
            .ActivateOnEnter<ArenaChanges>();
    }

    private void SinglePhase(uint id)
    {
        QuadrupleCrossing(id);
        BiscuitMaker(id + 0x100);
        OneTwoPaw(id + 0x200);
        LeapingQuadrupleCrossing(id + 0x300);
        Cast(id + 0x400, AID.BloodyScratch, 6.5f, 5.0f, "Raidwide")
            .ActivateOnEnter<BloodyScratch>()
            .DeactivateOnExit<BloodyScratch>();
        Mouser(id + 0x500);
        LeapingOneTwoPaw(id + 0x600);
        LeapingQuadrupleCrossing(id + 0x700, true);
    }

    private void QuadrupleCrossing(uint id)
    {
        Cast(id, AID.QuadrupleCrossingStart, 8.28f, 5.0f)
            .ActivateOnEnter<QuadrupleCrossing>()
            .ActivateOnEnter<QuadrupleCrossingRepeat>();
        Cast(id + 0x10, AID.QuadrupleCrossingStep, 2.0f, 1.0f);
        ComponentCondition<QuadrupleCrossing>(id + 0x20, 0.75f, comp => comp.NumCasts >= 8)
            .DeactivateOnExit<QuadrupleCrossing>();
        Cast(id + 0x30, AID.QuadrupleCrossingStep, 2.0f - 0.75f, 1.0f);
        Cast(id + 0x40, AID.QuadrupleCrossingFinish, 2.0f, 1.0f);
        ComponentCondition<QuadrupleCrossingRepeat>(id + 0x50, 0.55f, comp => comp.NumCasts >= 8)
            .DeactivateOnExit<QuadrupleCrossingRepeat>();
    }

    private void BiscuitMaker(uint id)
    {
        Cast(id, AID.BiscuitMakerFirst, 5.0f - 0.55f, 5.0f, "Tankbuster")
            .ActivateOnEnter<BiscuitMaker>();
        ComponentCondition<QuadrupleCrossingRepeat>(id + 0x10, 2.0f, comp => comp.NumCasts > 0)
            .DeactivateOnExit<BiscuitMaker>();
    }

    private void OneTwoPaw(uint id)
    {
        Cast(id, AID.NineLives, 14.4f, 3.0f)
            .ActivateOnEnter<OneTwoPaw>();
        ComponentCondition<OneTwoPaw>(id + 0x10, 35.5f, comp => comp.Done, "OneTwoPaw resolve")
            .DeactivateOnExit<OneTwoPaw>();
        CastEnd(id + 0x20, 2.0f);
    }

    private void LeapingQuadrupleCrossing(uint id, bool nineLives = false)
    {
        Cast(id, AID.Soulshade, 3f, 3.0f, "LeapingQuadrupleCrossing start")
            .ActivateOnEnter<LeapingQuadrupleCrossing>()
            .ActivateOnEnter<QuadrupleCrossingRepeat>();
        if (nineLives)
        {
            Cast(id + 0x10, AID.NineLives, 2f, 3.0f);
        }
        CastMulti(id + 0x20, [AID.LeapingQuadrupleCrossingCastA, AID.LeapingQuadrupleCrossingCastB], 2.0f, 5.0f);
        Cast(id + 0x30, AID.LeapingQuadrupleCrossingStep, 3.0f, 1.0f);
        ComponentCondition<LeapingQuadrupleCrossing>(id + 0x40, 0.7f, comp => comp.NumCasts >= 8)
            .DeactivateOnExit<LeapingQuadrupleCrossing>();
        Cast(id + 0x50, AID.LeapingQuadrupleCrossingStep, 0.3f, 1.0f);
        Cast(id + 0x60, AID.LeapingQuadrupleCrossingFinish, 2.0f, 1.0f);
        ComponentCondition<QuadrupleCrossingRepeat>(id + 0x70, 0.55f, comp => comp.NumCasts >= 8)
            .DeactivateOnExit<QuadrupleCrossingRepeat>();
    }

    private void Mouser(uint id)
    {
        Cast(id, AID.Mouser, 13.5f, 10f)
            .ActivateOnEnter<Mouser>()
            .DeactivateOnExit<Mouser>();
        Cast(id + 0x10, AID.Copycat, 18.0f, 3.0f);
        // TODO: Copycat
        Cast(id + 0x20, AID.BiscuitMakerFirst, 52.0f, 5.0f)
            .ActivateOnEnter<BiscuitMaker>();
        Cast(id + 0x30, AID.ShockwaveCast, 6.25f, 6.0f, "Knockback")
            .DeactivateOnEnter<BiscuitMaker>()
            .ActivateOnEnter<Shockwave>()
            .DeactivateOnExit<Shockwave>();
    }

    private void LeapingOneTwoPaw(uint id)
    {
        Cast(id, AID.NineLives, 14.4f, 3.0f)
            .ActivateOnEnter<LeapingOneTwoPaw>();
        ComponentCondition<LeapingOneTwoPaw>(id + 0x10, 10.8f, comp => comp.Done, "OneTwoPaw resolve")
            .DeactivateOnExit<LeapingOneTwoPaw>();
        CastEnd(id + 0x20, 2.0f);
    }
}
