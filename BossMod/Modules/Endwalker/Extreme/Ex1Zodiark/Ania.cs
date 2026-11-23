namespace BossMod.Endwalker.Extreme.Ex1Zodiark;

// state related to ania mechanic
sealed class Ania(BossModule module) : BossComponent(module)
{
    private Actor? _target;

    private const float _aoeRadius = 3f;

    public bool Done => _target == null;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_target == null)
            return;

        if (actor == _target)
        {
            var party = Raid.WithoutSlot(false, true, true);
            for (var i = 0; i < party.Length; ++i)
            {
                var p = party[i];
                if (p != actor && p.Position.InCircle(actor.Position, _aoeRadius))
                {
                    hints.Add("GTFO from raid!");
                    break;
                }
            }
            if (Module.PrimaryActor.TargetID == _target.InstanceID)
                hints.Add("Pass aggro!");
        }
        else
        {
            if (actor.Position.InCircle(_target.Position, _aoeRadius))
                hints.Add("GTFO from tank!");
            if (actor.Role == Role.Tank && Module.PrimaryActor.TargetID != actor.InstanceID)
                hints.Add("Taunt!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_target == null)
            return;

        Arena.ZoneCircleOutline(_target.Position, _aoeRadius, Colors.Danger);
        if (pc == _target)
        {
            foreach (var a in Raid.WithoutSlot(false, true, true))
            {
                if (a == pc)
                {
                    continue;
                }
                var isinaoe = a.Position.InCircle(_target.Position, _aoeRadius);
                Arena.Actor(a, isinaoe ? Colors.PlayerInteresting : Colors.PlayerGeneric, drawWorld: isinaoe ? true : null);
            }
        }
        else
        {
            Arena.Actor(_target, Colors.Danger, drawWorld: true);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AniaAOE)
            _target = WorldState.Actors.Find(spell.TargetID);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AniaAOE)
            _target = null;
    }
}
