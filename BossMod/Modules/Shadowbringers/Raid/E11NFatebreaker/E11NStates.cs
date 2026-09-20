namespace BossMod.Shadowbringers.Raid.E11NFatebreaker;

sealed class E11NStates : StateMachineBuilder
{
    public E11NStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BurnishedGlory>()
            .ActivateOnEnter<PowderMark>()
            .ActivateOnEnter<BurnMark>()
            .ActivateOnEnter<BurntStrike>()
            .ActivateOnEnter<Burnout>()
            .ActivateOnEnter<ShiningBlade>()
            .ActivateOnEnter<Blastburn>()
            .ActivateOnEnter<FireStack>()
            .ActivateOnEnter<HolyBait>()
            .ActivateOnEnter<BoundOfFaithTethers>()
            .ActivateOnEnter<BrightfireSmall>()
            .ActivateOnEnter<BrightfireLarge>()
            .ActivateOnEnter<ResoundingCrack>()
            .ActivateOnEnter<PrismaticDeception>();
    }
}
