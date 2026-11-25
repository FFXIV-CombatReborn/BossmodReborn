namespace BossMod.Shadowbringers.Foray.Duel.Duel6Lyon;

sealed class Duel6LyonStates : StateMachineBuilder
{
    public Duel6LyonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<OnFire>()
            .ActivateOnEnter<WildfiresFury>()
            .ActivateOnEnter<HeavenAndEarth>()
            .ActivateOnEnter<CagedHeartOfNature>()
            .ActivateOnEnter<HeartOfNatureConcentric>()
            .ActivateOnEnter<TasteOfBloodAndDuelOrDie>()
            .ActivateOnEnter<FlamesMeet>()
            .ActivateOnEnter<WindsPeak>()
            .ActivateOnEnter<WindsPeakKB>()
            .ActivateOnEnter<SplittingRage>()
            .ActivateOnEnter<NaturesBlood>()
            .ActivateOnEnter<MoveMountains>()
            .ActivateOnEnter<WildfireCrucible>();
    }
}

sealed class FlamesMeet : Components.SimpleAOEs
{
    public FlamesMeet(BossModule module) : base(module, (uint)AID.FlamesMeet, new AOEShapeCross(40f, 7f), 2)
    {
        MaxDangerColor = 1;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "SourP", GroupType = BossModuleInfo.GroupType.BozjaDuel, GroupID = 778u, NameID = 31u)]
public sealed class Duel6Lyon(WorldState ws, Actor primary) : BossModule(ws, primary, new(50f, -410f), new ArenaBoundsCircle(20f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player() is Actor player && player.Position.InCircle(Arena.Center, 20f);
}
