namespace BossMod.Endwalker.Extreme.Ex3Endsigner;

sealed class Elenchos(BossModule module) : BossComponent(module)
{
    private Actor? _center;
    private readonly List<Actor> _sides = [];

    private readonly AOEShapeRect _aoeCenter = new(40f, 7f);
    private readonly AOEShapeRect _aoeSides = new(40f, 6.5f, 40f);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var inAOE = _aoeCenter.Check(actor.Position, _center);
        for (var i = 0; i < _sides.Count && !inAOE; ++i)
            inAOE = _aoeSides.Check(actor.Position, _sides[i]);
        if (inAOE)
            hints.Add("GTFO from aoe!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        _aoeCenter.Draw(Arena, _center);
        foreach (var s in _sides)
            _aoeSides.Draw(Arena, s);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ElenchosCenter:
                _center = caster;
                break;
            case (uint)AID.ElenchosSidesAOE:
                _sides.Add(caster);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ElenchosCenter:
                _center = null;
                break;
            case (uint)AID.ElenchosSidesAOE:
                _sides.Remove(caster);
                break;
        }
    }
}
