namespace BossMod.Endwalker.Extreme.Ex1Zodiark;

// state related to exoterikos, trimorphos exoterikos and triple esoteric ray mechanics
sealed class Exoterikos(BossModule module) : BossComponent(module)
{
    private readonly List<(Actor, AOEShape)> _sources = [];

    private readonly AOEShapeRect _aoeSquare = new(21f, 21f);
    private readonly AOEShapeCone _aoeTriangle = new(47f, 30f.Degrees());
    private readonly AOEShapeRect _aoeRay = new(42f, 7f);

    private readonly List<(Actor, AOEShape)> _active = [];

    public bool Done => _sources.Count == 0;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var active = ActiveKnockbacks();
        for (var i = 0; i < active.Count; ++i)
        {
            var (src, shape) = active[i];
            if (shape.Check(actor.Position, src))
            {
                hints.Add("GTFO from exo aoe!");
                return;
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var active = ActiveKnockbacks();
        for (var i = 0; i < active.Count; ++i)
        {
            var (src, shape) = active[i];
            shape.Draw(Arena, src);
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        // tethers appear much earlier than cast start, but not for all sigils
        var target = WorldState.Actors.Find(tether.Target);
        if (source != Module.PrimaryActor || target == null)
            return;

        var shape = ShapeForSigil(target);
        if (shape != null)
            _sources.Add((target, shape));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = ShapeForSigil(caster);
        if (shape == null)
            return;
        for (var i = 0; i < _sources.Count; ++i)
        {
            if (_sources[i].Item1 == caster)
                return;
        }
        _sources.Add((caster, shape));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        for (var i = _sources.Count - 1; i >= 0; --i)
        {
            if (_sources[i].Item1 == caster)
                _sources.RemoveAt(i);
        }
    }

    private AOEShape? ShapeForSigil(Actor sigil)
    {
        return sigil.OID switch
        {
            (uint)OID.ExoSquare => _aoeSquare,
            (uint)OID.ExoTri => _aoeTriangle,
            (uint)OID.ExoGreen => _aoeRay,
            _ => null
        };
    }

    private List<(Actor, AOEShape)> ActiveKnockbacks()
    {
        _active.Clear();
        var hadSideSquare = false; // we don't show multiple side-squares, since that would cover whole arena and be useless
        DateTime lastRay = default; // we only show first rays, otherwise triple rays would cover whole arena and be useless
        foreach (var (actor, shape) in _sources)
        {
            if (shape == _aoeSquare && Math.Abs(actor.PosRot.X - Arena.Center.X) > 10f)
            {
                if (hadSideSquare)
                    continue;
                hadSideSquare = true;
            }
            else if (shape == _aoeRay)
            {
                if (lastRay != default && (actor.CastInfo == null || (Module.CastFinishAt(actor.CastInfo) - lastRay).TotalSeconds > 2d))
                    continue;
                lastRay = Module.CastFinishAt(actor.CastInfo);
            }
            _active.Add((actor, shape));
        }
        return _active;
    }
}
