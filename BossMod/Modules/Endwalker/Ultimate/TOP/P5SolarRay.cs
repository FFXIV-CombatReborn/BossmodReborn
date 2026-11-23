namespace BossMod.Endwalker.Ultimate.TOP;

// TODO: not sure how exactly second target is selected, I think it is snapshotted to the current target when first cast happens?
// TODO: consider generalizing - same as P12S1 Glaukopis and others...
sealed class P5SolarRay(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private static readonly AOEShapeCircle _shape = new(5);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (NumCasts == 0 && actor.Role == Role.Tank)
        {
            var baits = CollectionsMarshal.AsSpan(CurrentBaits);
            for (var i = 0; i < baits.Length; ++i)
            {
                ref var b = ref baits[i];
                if (b.Source.TargetID == b.Target.InstanceID)
                {
                    hints.Add(b.Source.TargetID != actor.InstanceID ? "Taunt!" : "Pass aggro!");
                    break;
                }
            }
        }
        base.AddHints(slot, actor, hints);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.P5SolarRayM or (uint)AID.P5SolarRayF)
        {
            var target = WorldState.Actors.Find(spell.TargetID);
            if (target != null)
                CurrentBaits.Add(new(caster, target, _shape));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.P5SolarRayM or (uint)AID.P5SolarRayMSecond or (uint)AID.P5SolarRayF or (uint)AID.P5SolarRayFSecond)
        {
            CurrentBaits.Clear();
            if (++NumCasts < 2 && WorldState.Actors.Find(caster.TargetID) is var target && target != null)
                CurrentBaits.Add(new(caster, target, _shape));
        }
    }
}
