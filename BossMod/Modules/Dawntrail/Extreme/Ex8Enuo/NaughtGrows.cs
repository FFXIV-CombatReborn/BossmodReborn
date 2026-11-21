namespace BossMod.Dawntrail.Extreme.Ex8Enuo;

//TODO: This needs a way to move the source to the other side of the target.
sealed class NaughtGrowsWildCharge(BossModule module) : Components.InverseWildCharge(module, 3f, 6f)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        Source = null;
        if (iconID is not ((uint)IconID.NaughtGrowsWildChargeSingle or (uint)IconID.NaughtGrowsWildChargeDouble))
        {
            return;
        }

        var single = iconID == (uint)IconID.NaughtGrowsWildChargeSingle;
        Source = actor;
        var party = Raid.WithSlot(true, false, false);
        var len = party.Length;
        var max = PlayerRoles.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var p = ref party[i];
            var slot = p.Item1;
            if (slot >= max)
            {
                continue;
            }
            var member = p.Item2;
            var isTarget = single ? member.InstanceID == targetID : member.Role == Role.Healer;
            PlayerRoles[slot] = isTarget ? PlayerRole.TargetNotFirst : member.Role == Role.Tank ? PlayerRole.Share : PlayerRole.ShareNotFirst;
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.GreatReturnToNothing or (uint)AID.ReturnToNothing)
        {
            Array.Clear(PlayerRoles);
        }
    }
}

sealed class NaughtGrowsDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NaughtGrowsDonut, new AOEShapeDonut(40f, 60f));

sealed class NaughtGrowsCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NaughtGrowsCircle, new AOEShapeCircle(40f));

sealed class NaughtGrowsBossDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NaughtGrowsBossDonut, new AOEShapeDonut(6f, 20f));

sealed class NaughtGrowsBossCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NaughtGrowsBossCircle, new AOEShapeCircle(12f));
