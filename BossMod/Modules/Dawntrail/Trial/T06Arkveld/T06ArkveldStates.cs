namespace BossMod.Dawntrail.Trial.T06Arkveld;

sealed class GuardianArkveldStates : StateMachineBuilder
{
    public GuardianArkveldStates(BossModule module) : base(module)
    {
        TrivialPhase()
            // raidwides
            .ActivateOnEnter<Roar>()
            .ActivateOnEnter<ForgedFury>()
            // line and cone aoes
            .ActivateOnEnter<ChainbladeBlow>()
            .ActivateOnEnter<ChainbladeSide>()
            .ActivateOnEnter<SiegeflightBoss>()
            .ActivateOnEnter<SiegeflightCast>()
            .ActivateOnEnter<SiegeflightFollowup>()
            .ActivateOnEnter<WyvernsRadiance>()
            .ActivateOnEnter<WyvernsOuroblade>()
            .ActivateOnEnter<SteeltailThrust>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<WyvernsWeal>()
            // moving / sequential aoes
            .ActivateOnEnter<WyvernsRadianceExawave>()
            .ActivateOnEnter<WyvernsRadianceQuake>()
            .ActivateOnEnter<WyveCannonMiddle>()
            .ActivateOnEnter<WyveCannonEdge>()
            .ActivateOnEnter<WyvernsVengeance>()
            // crystals and puddles
            .ActivateOnEnter<CrackedCrystalSmall>()
            .ActivateOnEnter<CrackedCrystalLarge>()
            .ActivateOnEnter<GuardianResonancePuddle>()
            // player mechanics
            .ActivateOnEnter<WildEnergy>()
            .ActivateOnEnter<ChainbladeCharge>()
            .ActivateOnEnter<GuardianResonanceTowerSmall>()
            .ActivateOnEnter<GuardianResonanceTowerLarge>();
    }
}
