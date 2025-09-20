namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL3SaunionDawon;

sealed class OneMind(BossModule module) : BossComponent(module)
{
    private readonly DAL3SaunionDawon bossmod = (DAL3SaunionDawon)module;
    private bool oneMind;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.OneMind)
        {
            oneMind = true;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.OneMind)
        {
            oneMind = false;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (oneMind)
        {
            hints.Add($"Pull {Module.PrimaryActor.Name} & {bossmod.BossDawon?.Name} further apart!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (oneMind && bossmod.BossDawon is Actor dawon)
        {
            var primary = Module.PrimaryActor;
            var primaryID = primary.TargetID;
            var actorID = actor.InstanceID;
            var isPrimaryTarget = actorID == primaryID;
            if (isPrimaryTarget ^ actor.InstanceID == dawon.TargetID) // if actor got aggro from both bosses we can do nothing
            {
                var pos = isPrimaryTarget ? dawon.Position : primary.Position;
                hints.AddForbiddenZone(new SDCircle(pos, (pos - actor.Position).Length() + 2f), DateTime.MaxValue);
            }
        }
    }
}
