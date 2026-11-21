namespace BossMod.Dawntrail.Alliance.A34Promathia;

sealed class A34PromathiaStates : StateMachineBuilder
{
    public A34PromathiaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EmptySalvation>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<WheelofImpregnability>()
            .ActivateOnEnter<BastionOfTwilight>()
            .ActivateOnEnter<PestilentPenance>()
            .ActivateOnEnter<Comet>()
            .ActivateOnEnter<FalseGenesis>()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<MemoryReceptacle>()
            .ActivateOnEnter<EmptyBeleaguer>()
            .ActivateOnEnter<AuroralDrape>()
            .ActivateOnEnter<WindsOfPromyvion>()
            .ActivateOnEnter<EmptySeed>()
            .ActivateOnEnter<DeadlyRebirth>()
            .ActivateOnEnter<DeadlyRebirthKB>()
            .ActivateOnEnter<FalseGenesisRaidwide>()
            .ActivateOnEnter<MalevolentBlessingCone>()
            .ActivateOnEnter<MalevolentBlessingRect>()
            .ActivateOnEnter<PestilentPenanceLink>()
            .ActivateOnEnter<InfernalDeliveranceTower>()
            .ActivateOnEnter<InfernalDeliveranceAOE>()
            .ActivateOnEnter<HeavyPuddles>()
            .ActivateOnEnter<Meteor>();
    }
}
