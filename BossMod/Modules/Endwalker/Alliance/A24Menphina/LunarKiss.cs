﻿namespace BossMod.Endwalker.Alliance.A24Menphina;

class LunarKiss(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeRect _shape = new(60, 3);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.LunarKiss)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, _shape));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LunarKissAOE)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}
