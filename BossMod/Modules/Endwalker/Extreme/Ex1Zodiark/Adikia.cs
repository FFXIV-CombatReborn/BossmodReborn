namespace BossMod.Endwalker.Extreme.Ex1Zodiark;

// state related to adikia mechanic
sealed class Adikia(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _casters = [];

    private static readonly AOEShapeCircle _shape = new(21f);

    public bool Done => _casters.Count == 0;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        for (var i = 0; i < _casters.Count; ++i)
        {
            if (_shape.Check(actor.Position, _casters[i]))
            {
                hints.Add("GTFO from side smash aoe!");
                return;
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var c in _casters)
            _shape.Draw(Arena, c);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.AdikiaL or (uint)AID.AdikiaR)
            _casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.AdikiaL or (uint)AID.AdikiaR)
            _casters.Remove(caster);
    }
}
