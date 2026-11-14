namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

sealed class FireflightPath(BossModule module) : Components.GenericAOEs(module)
{
    private sealed class Path(Actor from, Actor to, DateTime activation)
    {
        public readonly Actor From = from;
        public readonly Actor To = to;
        public readonly DateTime Activation = activation;
        public AOEShapeRect? Shape;
        public float HalfLength = -1f;
    }

    private static readonly AOEShapeDonut _donut = new(8f, 60f);
    private readonly List<Path> _paths = [];
    private readonly List<AOEInstance> _aoes = [];
    private Angle _angle;
    private DateTime _first;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Math.Min(_paths.Count, 2);
        if (count == 0)
        {
            return [];
        }
        _aoes.Clear();
        for (var i = 0; i < count; ++i)
        {
            var p = _paths[i];
            var color = i == 0 ? Colors.Danger : default;
            if (p.From == p.To)
            {
                _aoes.Add(new(_donut, p.From.Position, default, p.Activation, color, i == 0));
                continue;
            }
            var from = p.From.Position;
            var dir = p.To.Position - from;
            var half = dir.Length() * 0.5f;
            if (p.Shape == null || Math.Abs(half - p.HalfLength) > 0.01f)
            {
                p.HalfLength = half;
                p.Shape = new(50f, half);
            }
            _aoes.Add(new(p.Shape, from + dir * 0.5f, Angle.FromDirection(dir) + _angle, p.Activation, color, i == 0));
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FireflightByEmberlightRight:
            case (uint)AID.FireflightByPyrelightRight:
                _first = Module.CastFinishAt(spell, 2.7d);
                _angle = -90f.Degrees();
                break;
            case (uint)AID.FireflightByEmberlightLeft:
            case (uint)AID.FireflightByPyrelightLeft:
                _first = Module.CastFinishAt(spell, 2.7d);
                _angle = 90f.Degrees();
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.CarpetRide1Left or (uint)AID.CarpetRide1Right or (uint)AID.SunCirclet)
        {
            ++NumCasts;
            if (_paths.Count != 0)
            {
                _paths.RemoveAt(0);
            }
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Rug && WorldState.Actors.Find(tether.Target) is Actor target)
        {
            _paths.Add(new(source, target, _first.AddSeconds(2.1d * _paths.Count)));
            if (_paths.Count == 3)
            {
                _paths.Add(new(target, target, _first.AddSeconds(6.3d)));
            }
        }
    }
}

sealed class EmberPyre(BossModule module) : Components.UniformStackSpread(module, 3f, 3f)
{
    private int _next;
    private int _numCarpets;
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FireflightByEmberlightRight:
            case (uint)AID.FireflightByEmberlightLeft:
                _next = 2;
                break;
            case (uint)AID.FireflightByPyrelightRight:
            case (uint)AID.FireflightByPyrelightLeft:
                _next = 1;
                break;
        }
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (_next == 1)
        {
            hints.Add("Next: stack");
        }
        else if (_next == 2)
        {
            hints.Add("Next: spread");
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.CarpetRide1Left:
            case (uint)AID.CarpetRide1Right:
                if (++_numCarpets == 3)
                {
                    var party = Raid.WithoutSlot(false, true, true);
                    var len = party.Length;
                    if (len == 0)
                    {
                        break;
                    }
                    var activation = WorldState.FutureTime(4.9d);
                    if (_next == 1)
                    {
                        var target = party[0];
                        for (var i = 0; i < len; ++i)
                        {
                            if (party[i].Role == Role.Healer)
                            {
                                target = party[i];
                                break;
                            }
                        }
                        AddStack(target, activation);
                    }
                    else
                    {
                        AddSpreads(party, activation);
                    }
                }
                break;
            case (uint)AID.FireflightByPyrelightSpread:
            case (uint)AID.FireflightByEmberlightStack:
                ++NumCasts;
                Stacks.Clear();
                Spreads.Clear();
                break;
        }
    }
}
