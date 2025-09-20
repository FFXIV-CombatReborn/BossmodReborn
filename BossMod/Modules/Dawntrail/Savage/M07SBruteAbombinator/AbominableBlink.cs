namespace BossMod.Dawntrail.Savage.M07SBruteAbombinator;

sealed class AbominableBlink(BossModule module) : Components.BaitAwayIcon(module, 24f, (uint)IconID.AbominableBlink, (uint)AID.AbominableBlink, 6.4d)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var baits = ActiveBaitsOn(actor);
        if (baits.Count != 0)
        {
            hints.AddForbiddenZone(new SDRect(Arena.Center, new WDir(default, 1f), 24f, 24f, 25f), baits.Ref(0).Activation.AddSeconds(1d));
        }
    }
}
