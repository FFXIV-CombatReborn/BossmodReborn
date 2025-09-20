namespace BossMod.Dawntrail.Raid.M07NBruteAbombinator;

sealed class AbominableBlink(BossModule module) : Components.BaitAwayIcon(module, 24f, (uint)IconID.AbominableBlink, (uint)AID.AbominableBlink, 6.3f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var baits = ActiveBaitsOn(actor);
        if (baits.Count != 0)
            hints.AddForbiddenZone(new SDRect(Arena.Center, new WDir(default, 1f), 24f, 24f, 25f), baits.Ref(0).Activation.AddSeconds(1d));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (ActiveBaitsOn(actor).Count != 0)
            hints.Add("Bait away!");
    }
}
