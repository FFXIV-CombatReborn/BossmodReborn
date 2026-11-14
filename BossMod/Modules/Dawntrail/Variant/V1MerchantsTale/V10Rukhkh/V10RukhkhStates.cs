namespace BossMod.Dawntrail.VariantCriterion.V1MerchantsTale.V10Rukhkh;

class V10RukhkhStates : StateMachineBuilder
{
    public V10RukhkhStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SphereOfSand>()
            .ActivateOnEnter<SandPlume>()
            .ActivateOnEnter<SandBurst>()
            .ActivateOnEnter<SonicHowl>()
            .ActivateOnEnter<DryTyphoon>()
            .ActivateOnEnter<WindborneSeeds>()
            .ActivateOnEnter<StreamingSands>()
            .ActivateOnEnter<BitingScratch>()
            .ActivateOnEnter<Beaksbane>()
            .ActivateOnEnter<BigBurst>()
            .ActivateOnEnter<FallingRock>();
    }
}
