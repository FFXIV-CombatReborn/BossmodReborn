namespace BossMod.Shadowbringers.Alliance.A14WalkingFortress;

sealed class BallisticExaImpact(BossModule module) : Components.Exaflare(module, new AOEShapeRect(15f, 10f))
{
    private readonly List<Line> _toAdd = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BallisticImpactIndicator1:
            case (uint)AID.BallisticImpactIndicator2:
            case (uint)AID.BallisticImpactIndicator3:
                var rot = caster.Rotation;
                _toAdd.Add(new(caster.Position, 8.7f * rot.ToDirection(), Module.CastFinishAt(spell), 0.7d, 7, 4, rot));
                break;
        }
    }

    public override void Update()
    {
        var threshold = WorldState.FutureTime(3d);
        for (var i = _toAdd.Count - 1; i >= 0; --i)
        {
            var line = _toAdd[i];
            if (line.NextExplosion < threshold)
            {
                Lines.Add(line);
                _toAdd.RemoveAt(i);
            }
        }
        base.Update();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.BallisticImpactExa)
        {
            var pos = caster.Position;
            var count = Lines.Count;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                    {
                        Lines.RemoveAt(i);
                    }
                    return;
                }
            }
        }
    }
}

sealed class BallisticFloor(BossModule module) : Components.GenericAOEs(module, (uint)AID.BallisticImpactFloor)
{
    private sealed class Pattern(WPos firstTile, DateTime appear, DateTime activate)
    {
        public readonly List<WPos> Tiles = [firstTile];
        public readonly DateTime Appear = appear;
        public DateTime Activate = activate;
    }

    private static readonly AOEShapeRect square = new(7.5f, 7.5f, 7.5f);
    private readonly List<Pattern> _patterns = [];
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID != (uint)IconID.BallisticImpactGroundSquare)
        {
            return;
        }

        var count = _patterns.Count;
        if (count != 0 && _patterns[count - 1].Appear.AddSeconds(2d) > WorldState.CurrentTime)
        {
            _patterns[count - 1].Tiles.Add(actor.Position);
        }
        else
        {
            _patterns.Add(new(actor.Position, WorldState.CurrentTime, WorldState.FutureTime(5.1d)));
            ++count;
        }

        var startDelay = count switch
        {
            2 => 6.1d,
            3 => 9.2d,
            _ => default
        };

        if (startDelay != default)
        {
            var activation = _patterns[0].Appear.AddSeconds(startDelay);
            for (var i = 0; i < count; ++i)
            {
                _patterns[i].Activate = activation;
                activation = activation.AddSeconds(2.1d);
            }
        }
        UpdateAOEs();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            if (_patterns.Count != 0)
            {
                var tiles = _patterns[0].Tiles;
                var pos = caster.Position;
                for (var i = tiles.Count - 1; i >= 0; --i)
                {
                    if (tiles[i].AlmostEqual(pos, 1f))
                    {
                        tiles.RemoveAt(i);
                    }
                }
                if (tiles.Count == 0)
                {
                    _patterns.RemoveAt(0);
                }
                UpdateAOEs();
            }
        }
    }

    private void UpdateAOEs()
    {
        _aoes.Clear();
        if (_patterns.Count == 0)
        {
            return;
        }
        var p = _patterns[0];
        var tiles = p.Tiles;
        var count = tiles.Count;
        for (var i = 0; i < count; ++i)
        {
            _aoes.Add(new(square, tiles[i], default, p.Activate));
        }
    }
}
