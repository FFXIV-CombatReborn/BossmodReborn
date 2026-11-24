namespace BossMod.Shadowbringers.Alliance.A12Hobbes;

sealed class A12HobbesStates : StateMachineBuilder
{
    public A12HobbesStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LaserResistanceTest>()
            .ActivateOnEnter<LaserResistanceTestRepeat>()
            .ActivateOnEnter<ShockingDischarge>()
            .ActivateOnEnter<VariableCombatTestCone>()
            .ActivateOnEnter<VariableCombatTestCircle>()
            .ActivateOnEnter<VariableCombatTestDonut>()
            .ActivateOnEnter<Impact>()
            .ActivateOnEnter<ConvenientSelfDestruction>()
            .ActivateOnEnter<Towerfall>()
            .ActivateOnEnter<FireResistanceTest>()
            .ActivateOnEnter<RingLaser>()
            .ActivateOnEnter<LaserSight>()
            .ActivateOnEnter<UnwillingCargo>()
            .ActivateOnEnter<UnwillingCargoRing>()
            .ActivateOnEnter<ShortRangeMissile>()
            .ActivateOnEnter<OilWell>()
            .ActivateOnEnter<ElectromagneticPulse>();
    }
}
