namespace BossMod.Shadowbringers.Foray.Duel.Duel2Lyon;

sealed class Duel2LyonStates : StateMachineBuilder
{
    public Duel2LyonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<Enaero>()
            .ActivateOnEnter<HeartOfNature>()
            .ActivateOnEnter<TasteOfBlood>()
            .ActivateOnEnter<TasteOfBloodHint>()
            .ActivateOnEnter<RavenousGale>()
            .ActivateOnEnter<WindsPeakKB>()
            .ActivateOnEnter<WindsPeak>()
            .ActivateOnEnter<SplittingRage>()
            .ActivateOnEnter<TheKingsNotice>()
            .ActivateOnEnter<TwinAgonies>()
            .ActivateOnEnter<NaturesBlood>()
            .ActivateOnEnter<SpitefulFlameCircleVoidzone>()
            .ActivateOnEnter<SpitefulFlameRect>()
            .ActivateOnEnter<DynasticFlame>()
            .ActivateOnEnter<SkyrendingStrike>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.BozjaDuel, GroupID = 735u, NameID = 8u)] // bnpcname=9409
public sealed class Duel2Lyon : BossModule
{
    public Duel2Lyon(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private Duel2Lyon(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(211f, 380f), 24.5f, 32)]);
        return (arena.Center, arena);
    }

    protected override bool CheckPull() => base.CheckPull() && Raid.Player() is Actor player && player.Position.InCircle(Arena.Center, 25f);
}
