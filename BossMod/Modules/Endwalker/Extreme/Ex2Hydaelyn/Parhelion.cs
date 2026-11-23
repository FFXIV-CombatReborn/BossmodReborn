namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

sealed class Parhelion(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _completedParhelions = [];
    private bool _subparhelions;

    private static readonly AOEShapeRect _beacon = new(45f, 3f);
    private readonly List<Actor> _active = [];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var active = ActiveParhelions();
        for (var i = 0; i < active.Count; ++i)
        {
            if (_beacon.Check(actor.Position, active[i]))
            {
                hints.Add("GTFO from aoe!");
                return;
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var active = ActiveParhelions();
        for (var i = 0; i < active.Count; ++i)
            _beacon.Draw(Arena, active[i]);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BeaconParhelion:
                _completedParhelions.Add(caster);
                _subparhelions = _completedParhelions.Count >= 15;
                break;
            case (uint)AID.BeaconSubparhelion:
                _completedParhelions.Remove(caster);
                break;
        }
    }

    private List<Actor> ActiveParhelions()
    {
        _active.Clear();
        if (_subparhelions)
        {
            var count = Math.Min(10, _completedParhelions.Count);
            for (var i = 0; i < count; ++i)
                _active.Add(_completedParhelions[i]);
        }
        else
        {
            var parhelions = Module.Enemies((uint)OID.Parhelion);
            for (var i = 0; i < parhelions.Count; ++i)
            {
                if (parhelions[i].CastInfo != null)
                    _active.Add(parhelions[i]);
            }
        }
        return _active;
    }
}
