namespace BossMod.Dawntrail.Ultimate.FRU;

sealed class P2HallowedRay(BossModule module) : Components.GenericWildCharge(module, 3f, (uint)AID.HallowedRayAOE, 65f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Source == null)
            return;
        if (PlayerRoles[slot] == PlayerRole.Target || Activation > WorldState.FutureTime(2.5f))
        {
            // stay at the average direction to the raid
            // TODO: for melees, allow doing positionals...
            WDir averageDir = default;
            foreach (var p in Raid.WithoutSlot(false, true, true))
                averageDir += (p.Position - Source.Position).Normalized();
            hints.AddForbiddenZone(new SDInvertedRect(Source.Position, Angle.FromDirection(averageDir), 20f, -6f, 2f), Activation);
        }
        else
        {
            // default hints (go to target)
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.HallowedRay)
        {
            Source = actor;
            Activation = WorldState.FutureTime(5.7d);
            foreach (var (i, p) in Raid.WithSlot(true, true, true))
                PlayerRoles[i] = p.InstanceID == targetID ? PlayerRole.Target : PlayerRole.Share;
        }
    }
}
