namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

sealed class Dragonfall(BossModule module) : Components.UniformStackSpread(module, 6f, default, 8, 8)
{
    public int NumCasts;

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Dragonfall)
        {
            AddStack(source, WorldState.FutureTime(9.5d));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.DragonfallAOE)
        {
            ++NumCasts;
            var mainTarget = spell.MainTargetID;
            for (var i = Stacks.Count - 1; i >= 0; --i)
            {
                if (Stacks[i].Target.InstanceID == mainTarget)
                {
                    Stacks.RemoveAt(i);
                }
            }
            BitMask forbidden = default;
            var party = Raid.WithSlot(false, false, true);
            var lenP = party.Length;
            var targets = CollectionsMarshal.AsSpan(spell.Targets);
            var lenT = targets.Length;
            for (var i = 0; i < lenT; ++i)
            {
                var id = targets[i].ID;
                for (var j = 0; j < lenP; ++j)
                {
                    ref readonly var p = ref party[j];
                    if (p.Item2.InstanceID == id)
                    {
                        forbidden.Set(p.Item1);
                        break;
                    }
                }
            }
            var stacks = CollectionsMarshal.AsSpan(Stacks);
            var lenS = stacks.Length;
            for (var i = 0; i < lenS; ++i)
            {
                stacks[i].ForbiddenPlayers |= forbidden;
            }
        }
    }
}
