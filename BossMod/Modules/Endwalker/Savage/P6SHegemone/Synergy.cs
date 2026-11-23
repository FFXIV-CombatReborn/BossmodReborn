namespace BossMod.Endwalker.Savage.P6SHegemone;

sealed class Synergy(BossModule module) : BossComponent(module)
{
    public bool Done;
    private readonly Actor?[] _targets = [null, null]; // second target is for non-chelic synergy
    private bool _chelic;

    private readonly AOEShapeCircle _shapeNormal = new(5f);
    private readonly AOEShapeCone _shapeChelic = new(60f, 30f.Degrees());

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_chelic)
        {
            if (_targets[0] is var t0 && actor != t0 && _shapeNormal.Check(actor.Position, t0) || _targets[1] is var t1 && actor != t1 && _shapeNormal.Check(actor.Position, t1))
                hints.Add("GTFO from tanks!");

            if (Module.PrimaryActor.TargetID == _targets[0]?.InstanceID)
            {
                if (actor == _targets[0])
                    hints.Add("Shirk!");
                else if (actor.Role == Role.Tank)
                    hints.Add("Taunt!");
            }
        }
        else if (_targets[0] == actor)
        {
            hints.Add("Stack with other tanks or press invuln!", false);
        }
        else
        {
            hints.Add("GTFO from tank!", _shapeChelic.Check(actor.Position, Module.PrimaryActor.Position, Angle.FromDirection(_targets[0]!.Position - Module.PrimaryActor.Position)));
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _targets.Contains(player) ? PlayerPriority.Danger : PlayerPriority.Irrelevant;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!_chelic)
        {
            foreach (var t in _targets)
                _shapeNormal.Outline(Arena, t);
        }
        else
        {
            _shapeChelic.Outline(Arena, Module.PrimaryActor.Position, Angle.FromDirection(_targets[0]!.Position - Module.PrimaryActor.Position));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.SynergyAOE1:
                _targets[0] = WorldState.Actors.Find(spell.TargetID);
                break;
            case (uint)AID.SynergyAOE2:
                _targets[1] = WorldState.Actors.Find(spell.TargetID);
                break;
            case (uint)AID.ChelicSynergy:
                _targets[0] = WorldState.Actors.Find(spell.TargetID);
                _chelic = true;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SynergyAOE1 or (uint)AID.SynergyAOE2 or (uint)AID.ChelicSynergy)
            Done = true;
    }
}
