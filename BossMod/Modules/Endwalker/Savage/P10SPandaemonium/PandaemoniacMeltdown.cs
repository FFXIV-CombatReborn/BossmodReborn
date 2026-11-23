namespace BossMod.Endwalker.Savage.P10SPandaemonium;

// TODO: generalize (line stack/spread)
sealed class PandaemoniacMeltdown(BossModule module) : Components.CastCounter(module, (uint)AID.PandaemoniacMeltdownStack)
{
    private Actor? _stackTarget;
    private readonly List<Actor> _spreadTargets = [];

    private readonly AOEShapeRect _shapeStack = new(50f, 3f);
    private readonly AOEShapeRect _shapeSpread = new(50f, 2f);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var isSpread = _spreadTargets.Contains(actor);
        var bossPos = Module.PrimaryActor.Position;
        for (var i = 0; i < _spreadTargets.Count; ++i)
        {
            var t = _spreadTargets[i];
            if (t != actor && _shapeSpread.Check(actor.Position, bossPos, Angle.FromDirection(t.Position - bossPos)))
            {
                hints.Add("GTFO from other spreads!");
                break;
            }
        }
        if (isSpread && Raid.WithoutSlot(false, true, true).AnyInShape(_shapeSpread, Module.PrimaryActor.Position, Angle.FromDirection(actor.Position - Module.PrimaryActor.Position), actor))
            hints.Add("Aim spread away from raid!");

        if (_stackTarget == actor)
        {
            var stackDir = Angle.FromDirection(actor.Position - bossPos);
            for (var i = 0; i < _spreadTargets.Count; ++i)
            {
                if (_shapeStack.Check(_spreadTargets[i].Position, bossPos, stackDir))
                {
                    hints.Add("Aim stack away from spreads!");
                    break;
                }
            }
        }
        else if (_stackTarget != null)
        {
            var inStack = _shapeStack.Check(actor.Position, Module.PrimaryActor.Position, Angle.FromDirection(_stackTarget.Position - Module.PrimaryActor.Position));
            if (isSpread == inStack)
                hints.Add(isSpread ? "GTFO from stack!" : "Stack in line!");
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return player == _stackTarget ? PlayerPriority.Interesting : _spreadTargets.Contains(player) ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var isSpread = _spreadTargets.Contains(pc);
        if (_stackTarget != null && _stackTarget != pc)
            _shapeStack.Draw(Arena, Module.PrimaryActor.Position, Angle.FromDirection(_stackTarget.Position - Module.PrimaryActor.Position), isSpread ? Colors.AOE : Colors.SafeFromAOE);
        foreach (var t in _spreadTargets)
            if (t != pc)
                _shapeStack.Draw(Arena, Module.PrimaryActor.Position, Angle.FromDirection(t.Position - Module.PrimaryActor.Position), Colors.AOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (pc == _stackTarget)
            _shapeStack.Outline(Arena, Module.PrimaryActor.Position, Angle.FromDirection(pc.Position - Module.PrimaryActor.Position), Colors.Safe);
        else if (_spreadTargets.Contains(pc))
            _shapeSpread.Outline(Arena, Module.PrimaryActor.Position, Angle.FromDirection(pc.Position - Module.PrimaryActor.Position), Colors.Danger);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.PandaemoniacMeltdownTargetSelect)
            _stackTarget = WorldState.Actors.Find(spell.MainTargetID);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.PandaemoniacMeltdownSpread)
            _spreadTargets.Add(actor);
    }
}
