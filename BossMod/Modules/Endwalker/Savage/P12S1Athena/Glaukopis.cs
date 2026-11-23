namespace BossMod.Endwalker.Savage.P12S1Athena;

// TODO: not sure how exactly second target is selected, I think it is snapshotted to the current target when first cast happens? I can definitely taunt between casts without dying...
// TODO: consider generalizing...
sealed class Glaukopis(BossModule module) : Components.GenericBaitAway(module)
{
    private readonly AOEShapeRect _shape = new(60f, 2.5f);

    private bool BossTargetsBaiter()
    {
        for (var i = 0; i < CurrentBaits.Count; ++i)
        {
            if (CurrentBaits[i].Source.TargetID == CurrentBaits[i].Target.InstanceID)
                return true;
        }
        return false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.Role == Role.Tank && BossTargetsBaiter())
            hints.Add(Module.PrimaryActor.TargetID != actor.InstanceID ? "Taunt!" : "Pass aggro!");
        base.AddHints(slot, actor, hints);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Glaukopis)
        {
            var target = WorldState.Actors.Find(spell.TargetID);
            if (target != null)
                CurrentBaits.Add(new(caster, target, _shape));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.Glaukopis or (uint)AID.GlaukopisSecond)
        {
            CurrentBaits.Clear();
            if (++NumCasts < 2 && WorldState.Actors.Find(caster.TargetID) is var target && target != null)
                CurrentBaits.Add(new(caster, target, _shape));
        }
    }
}
