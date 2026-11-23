namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to fledgling flight & death toll mechanics
sealed class FledglingFlight(BossModule module) : BossComponent(module)
{
    public bool PlacementDone;
    public bool CastsDone;
    private readonly List<(Actor, Angle)> _sources = []; // actor + rotation
    private readonly int[] _playerDeathTollStacks = new int[8];
    private readonly int[] _playerAOECount = new int[8];

    private static readonly Angle _coneHalfAngle = 45f.Degrees();
    private const float _eyePlacementOffset = 10f;

    public override void Update()
    {
        if (_sources.Count == 0)
            return;

        foreach ((var i, var player) in Raid.WithSlot(false, true, true))
        {
            _playerDeathTollStacks[i] = player.FindStatus((uint)SID.DeathsToll)?.Extra ?? 0; // TODO: use status events here...
            var hits = 0;
            for (var j = 0; j < _sources.Count; ++j)
            {
                if (player.Position.InCone(_sources[j].Item1.Position, _sources[j].Item2, _coneHalfAngle))
                    ++hits;
            }
            _playerAOECount[i] = hits;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_sources.Count == 0)
            return;

        var eyePos = GetEyePlacementPosition(slot, actor);
        if (eyePos != null && !actor.Position.InCircle(eyePos.Value, 5))
        {
            hints.Add("Get closer to eye placement position!");
        }

        if (_playerAOECount[slot] < _playerDeathTollStacks[slot])
        {
            hints.Add($"Enter more aoes ({_playerAOECount[slot]}/{_playerDeathTollStacks[slot]})!");
        }
        else if (_playerAOECount[slot] > _playerDeathTollStacks[slot])
        {
            hints.Add($"GTFO from eyes ({_playerAOECount[slot]}/{_playerDeathTollStacks[slot]})!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_sources.Count == 0)
            return;

        // draw all players
        foreach ((var i, var player) in Raid.WithSlot(false, true, true))
        {
            var indanger = _playerAOECount[i] != _playerDeathTollStacks[i];
            Arena.Actor(player, indanger ? Colors.PlayerInteresting : Colors.PlayerGeneric, drawWorld: indanger ? true : null);
        }

        var eyePos = GetEyePlacementPosition(pcSlot, pc);
        if (eyePos != null)
            Arena.ZoneCircleOutline(eyePos.Value, 1, Colors.Safe);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach ((var source, var dir) in _sources)
        {
            Arena.ZoneIsoscelesTri(source.Position, dir, _coneHalfAngle, 50, Colors.AOE);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AshenEye)
        {
            if (!PlacementDone)
            {
                PlacementDone = true;
                _sources.Clear();
            }
            _sources.Add((caster, caster.Rotation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AshenEye)
        {
            _sources.RemoveAll(x => x.Item1 == caster);
            CastsDone = _sources.Count == 0;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is >= 296u and <= 299u)
        {
            if (PlacementDone)
            {
                ReportError($"Unexpected icon after eyes started casting");
                return;
            }

            var dir = iconID switch
            {
                296u => 90.Degrees(), // E
                297u => 270.Degrees(), // W
                298u => default, // S
                299u => 180.Degrees(), // N
                _ => 0.Degrees()
            };
            _sources.Add((actor, dir));
        }
    }

    private WPos? GetEyePlacementPosition(int slot, Actor player)
    {
        if (PlacementDone)
            return null;

        (var src, var rot) = _sources.Find(srcRot => srcRot.Item1 == player);
        if (src == null)
            return null;

        var offset = rot.ToDirection() * _eyePlacementOffset;
        return _playerDeathTollStacks[slot] > 0 ? Arena.Center - offset : Arena.Center + offset;
    }
}
