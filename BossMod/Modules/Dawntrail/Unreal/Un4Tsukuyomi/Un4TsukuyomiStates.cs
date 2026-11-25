namespace BossMod.Dawntrail.Unreal.Un4Tsukuyomi;

sealed class Un4TsukuyomiStates : StateMachineBuilder
{
    public Un4TsukuyomiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Reprimand>()
            .ActivateOnEnter<Nightbloom>()
            .ActivateOnEnter<NightbloomHelper>()
            .ActivateOnEnter<Antitwilight>()
            .ActivateOnEnter<MidnightRain>()
            .ActivateOnEnter<DanceOfTheDead>()
            .ActivateOnEnter<NightfallHint>()
            .ActivateOnEnter<LeadOfTheUnderworld>()
            .ActivateOnEnter<SteelOfTheUnderworld>()
            .ActivateOnEnter<TormentUntoDeath>()
            .ActivateOnEnter<TormentUntoDeathPhysical>()
            .ActivateOnEnter<GosetsuCharge>()
            .ActivateOnEnter<TetheredSpecters>()
            .ActivateOnEnter<Specters>()
            .ActivateOnEnter<LunarHalo>()
            .ActivateOnEnter<Moonfall>()
            .ActivateOnEnter<MoonfallBait>()
            .ActivateOnEnter<TsukiNoMaiogi>()
            .ActivateOnEnter<BrightDarkBlade>()
            .ActivateOnEnter<WaxingWaningGrudge>()
            .ActivateOnEnter<Lunacy>()
            .ActivateOnEnter<Hagetsu>()
            .ActivateOnEnter<MoonStatus>();
    }
}
