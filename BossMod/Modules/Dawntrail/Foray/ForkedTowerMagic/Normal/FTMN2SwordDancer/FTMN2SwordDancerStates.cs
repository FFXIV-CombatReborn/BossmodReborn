namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN2SwordDancer;

[SkipLocalsInit]
sealed class SwordDancerStates : StateMachineBuilder
{
    public SwordDancerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SwordDancerAOEs>()
            .ActivateOnEnter<LeapLandingAOE>()
            .ActivateOnEnter<SwordSpinAOEs>()
            .ActivateOnEnter<DancingSwordPreview>()
            .ActivateOnEnter<SwordRush>()
            .ActivateOnEnter<SwordBladeRects>()
            .ActivateOnEnter<Steelsbreath>()
            .ActivateOnEnter<SurgeswordSequence>()
            .ActivateOnEnter<ElectricBoundary>();
    }
}
