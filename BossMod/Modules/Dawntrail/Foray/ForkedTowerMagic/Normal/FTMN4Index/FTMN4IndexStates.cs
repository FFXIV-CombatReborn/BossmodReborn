namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN4Index;

[SkipLocalsInit]
sealed class IndexStates : StateMachineBuilder
{
    public IndexStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IndexArenaOutline>()
            .ActivateOnEnter<IndexAOEs>()
            .ActivateOnEnter<ElementaryChemistryPlatforms>()
            .ActivateOnEnter<ElementalSectors>()
            .ActivateOnEnter<PropulsiveShockwave>()
            .ActivateOnEnter<AllConsumingFlames>();
    }
}
