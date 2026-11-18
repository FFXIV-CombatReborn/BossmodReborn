namespace BossMod.Dawntrail.Trial.T06Arkveld;

sealed class WyvernsVengeance(BossModule module) : Components.Exaflare(module, 6f)
{
    private readonly List<ulong> _casters = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WyvernsVengeance)
        {
            // advance doesn't matter for caster-matched tracking; use default WDir
            Lines.Add(new(
                caster.Position,
                default,
                Module.CastFinishAt(spell),
                timeToMove: 1.6d,
                explosionsLeft: 2,
                maxShownExplosions: 1
            ));
            _casters.Add(caster.InstanceID);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.WyvernsVengeance1)
        {
            var idx = _casters.IndexOf(caster.InstanceID);
            if (idx < 0)
                return; // not one of our tracked helpers

            var line = Lines[idx];
            var loc = spell.TargetXZ;

            AdvanceLine(line, loc);

            if (line.ExplosionsLeft == 0)
            {
                Lines.RemoveAt(idx);
                _casters.RemoveAt(idx);
            }
        }
    }
}

sealed class WyvernsWeal(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(60f, 3f);
    private static readonly Angle maxStep = 11.25f.Degrees();
    private readonly List<(Angle Rotation, DateTime Activation)> _rotations = [with(16)];
    private readonly AOEInstance[] _aoes = new AOEInstance[2];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.WyvernsWeal1)
        {
            _rotations.Clear();
            _rotations.Add((spell.Rotation, Module.CastFinishAt(spell)));
        }
        else if (id == (uint)AID.WyvernsWeal3)
        {
            _rotations.Add((spell.Rotation, Module.CastFinishAt(spell, 0.7d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.WyvernsWeal1)
        {
            var count = _rotations.Count;
            if (count != 0)
            {
                var now = WorldState.CurrentTime;
                var last = _rotations[count - 1].Rotation;
                _rotations[count - 1] = (last, now);
                _rotations.Add((last, now.AddSeconds(0.7d)));
            }
        }
        else if (id == (uint)AID.WyvernsWeal2)
        {
            _rotations.Add((caster.Rotation, WorldState.FutureTime(0.7d)));
        }
        if (id is (uint)AID.WyvernsWeal1 or (uint)AID.WyvernsWeal2 or (uint)AID.WyvernsWeal3)
        {
            ++NumCasts;
        }
    }

    public override void Update()
    {
        var now = WorldState.CurrentTime;
        for (var i = _rotations.Count - 1; i >= 0; --i)
        {
            if (_rotations[i].Activation.AddSeconds(2d) < now)
            {
                _rotations.RemoveAt(i);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _rotations.Count;
        if (count == 0)
        {
            return [];
        }
        var now = WorldState.CurrentTime;
        var current = -1;
        var previous = -1;
        for (var i = count - 1; i >= 0; --i)
        {
            if (_rotations[i].Activation.AddSeconds(1d) > now)
            {
                if (current < 0)
                {
                    current = i;
                }
                else
                {
                    previous = i;
                    break;
                }
            }
        }
        if (current < 0)
        {
            return [];
        }
        var origin = Module.PrimaryActor.Position;
        var (rot, act) = _rotations[current];
        _aoes[0] = new(rect, origin, rot, act);
        if (previous >= 0)
        {
            var nextTime = act.AddSeconds(0.7d);
            if (nextTime > now && nextTime < now.AddSeconds(5d))
            {
                var delta = (rot - _rotations[previous].Rotation).Normalized().Rad;
                var step = Math.Clamp(delta, -maxStep.Rad, maxStep.Rad);
                _aoes[1] = new(rect, origin, rot + new Angle(step), nextTime);
                return _aoes;
            }
        }
        return _aoes.AsSpan(0, 1);
    }
}
