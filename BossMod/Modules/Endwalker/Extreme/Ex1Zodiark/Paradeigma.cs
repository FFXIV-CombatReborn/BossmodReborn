namespace BossMod.Endwalker.Extreme.Ex1Zodiark;

// state related to paradeigma and astral flow mechanics
sealed class Paradeigma(BossModule module) : BossComponent(module)
{
    public enum FlowDirection { None, CW, CCW }

    private FlowDirection _flow;
    private readonly List<WDir> _birds = [];
    private readonly List<WDir> _behemoths = [];
    private readonly List<(WDir, Angle)> _snakes = [];
    private readonly List<WDir> _fireLine = [];

    private const float _birdBehemothOffset = 10.5f;
    private const float _snakeNOffset = 5.5f;
    private const float _snakeFOffset = 15.5f;
    private const float _snakeOrthoOffset = 21f;
    private readonly AOEShapeDonut _birdAOE = new(5f, 15f);
    private readonly AOEShapeCircle _behemothAOE = new(15f);
    private readonly AOEShapeRect _snakeAOE = new(42f, 5.5f);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var pos = actor.Position;
        var inBirdBehemoth = false;
        for (var i = 0; i < _birds.Count && !inBirdBehemoth; ++i)
            inBirdBehemoth = _birdAOE.Check(pos, RotatedPosition(_birds[i]));
        for (var i = 0; i < _behemoths.Count && !inBirdBehemoth; ++i)
            inBirdBehemoth = _behemothAOE.Check(pos, RotatedPosition(_behemoths[i]));
        if (inBirdBehemoth)
            hints.Add("GTFO from bird/behemoth aoe!");
        for (var i = 0; i < _snakes.Count; ++i)
        {
            var (p, r) = RotatedPosRot(_snakes[i]);
            if (_snakeAOE.Check(pos, p, r))
            {
                hints.Add("GTFO from snake aoe!");
                break;
            }
        }
        for (var i = 0; i < _fireLine.Count; ++i)
        {
            if (InFireAOE(_fireLine[i], pos))
            {
                hints.Add("GTFO from fire aoe!");
                break;
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        for (var i = 0; i < _birds.Count; ++i)
            _birdAOE.Draw(Arena, RotatedPosition(_birds[i]));
        for (var i = 0; i < _behemoths.Count; ++i)
            _behemothAOE.Draw(Arena, RotatedPosition(_behemoths[i]));
        for (var i = 0; i < _snakes.Count; ++i)
        {
            var (p, r) = RotatedPosRot(_snakes[i]);
            _snakeAOE.Draw(Arena, p, r);
        }
        foreach (var c in _fireLine)
            Arena.ZoneTri(Arena.Center + c, RotatedPosition(c), Arena.Center, Colors.AOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_fireLine.Count == 2)
            Arena.AddLine(Arena.Center + _fireLine[0], Arena.Center + _fireLine[1], Colors.Danger);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        // notable env controls that we don't care too much about:
        // 1: common for all flows, 00020001 = activate, 00080004 = deactivate
        // 3: common for all flows, happens a bit after cast start, always 00010001
        if (index == 2)
        {
            // flow rotation arrows (note that we could also rely on cast id for them...)
            if (state == 0x00020001u)
                _flow = FlowDirection.CW;
            else if (state == 0x00200010u)
                _flow = FlowDirection.CCW;
            // other states: 00080004, 00400004 - deactivation
        }
        else if (index == 5)
        {
            switch (state)
            {
                case 0x00020001u:
                    _fireLine.Add(new(+Arena.Bounds.Radius, -Arena.Bounds.Radius));
                    _fireLine.Add(new(-Arena.Bounds.Radius, +Arena.Bounds.Radius));
                    break;
                case 0x00400020u:
                    _fireLine.Add(new(-Arena.Bounds.Radius, -Arena.Bounds.Radius));
                    _fireLine.Add(new(+Arena.Bounds.Radius, +Arena.Bounds.Radius));
                    break;
            }
        }
        else if (index >= 9 && index <= 24 && state == 0x00200010u)
        {
            // birds, behemoths and snakes; other states: 20001000 = color change, 40000004 = disappear
            switch (index)
            {
                case 09: _behemoths.Add(new(-_birdBehemothOffset, -_birdBehemothOffset)); break;
                case 10: _behemoths.Add(new(+_birdBehemothOffset, -_birdBehemothOffset)); break;
                case 11: _behemoths.Add(new(-_birdBehemothOffset, +_birdBehemothOffset)); break;
                case 12: _behemoths.Add(new(+_birdBehemothOffset, +_birdBehemothOffset)); break;
                case 13:
                    _snakes.Add((new(-_snakeFOffset, -_snakeOrthoOffset), default));
                    _snakes.Add((new(+_snakeNOffset, -_snakeOrthoOffset), default));
                    break;
                case 14:
                    _snakes.Add((new(-_snakeNOffset, -_snakeOrthoOffset), default));
                    _snakes.Add((new(+_snakeFOffset, -_snakeOrthoOffset), default));
                    break;
                case 15:
                    _snakes.Add((new(-_snakeFOffset, _snakeOrthoOffset), 180f.Degrees()));
                    _snakes.Add((new(+_snakeNOffset, _snakeOrthoOffset), 180f.Degrees()));
                    break;
                case 16:
                    _snakes.Add((new(-_snakeNOffset, _snakeOrthoOffset), 180f.Degrees()));
                    _snakes.Add((new(+_snakeFOffset, _snakeOrthoOffset), 180f.Degrees()));
                    break;
                case 17:
                    _snakes.Add((new(-_snakeOrthoOffset, -_snakeFOffset), 90f.Degrees()));
                    _snakes.Add((new(-_snakeOrthoOffset, +_snakeNOffset), 90f.Degrees()));
                    break;
                case 18:
                    _snakes.Add((new(-_snakeOrthoOffset, -_snakeNOffset), 90f.Degrees()));
                    _snakes.Add((new(-_snakeOrthoOffset, +_snakeFOffset), 90f.Degrees()));
                    break;
                case 19:
                    _snakes.Add((new(_snakeOrthoOffset, -_snakeFOffset), -90f.Degrees()));
                    _snakes.Add((new(_snakeOrthoOffset, +_snakeNOffset), -90f.Degrees()));
                    break;
                case 20:
                    _snakes.Add((new(_snakeOrthoOffset, -_snakeNOffset), -90f.Degrees()));
                    _snakes.Add((new(_snakeOrthoOffset, +_snakeFOffset), -90f.Degrees()));
                    break;
                case 21: _birds.Add(new(-_birdBehemothOffset, -_birdBehemothOffset)); break;
                case 22: _birds.Add(new(+_birdBehemothOffset, -_birdBehemothOffset)); break;
                case 23: _birds.Add(new(-_birdBehemothOffset, +_birdBehemothOffset)); break;
                case 24: _birds.Add(new(+_birdBehemothOffset, +_birdBehemothOffset)); break;
            }
        }
    }

    private WPos RotatedPosition(WDir offset)
    {
        return _flow switch
        {
            FlowDirection.CW => Arena.Center + offset.OrthoR(),
            FlowDirection.CCW => Arena.Center + offset.OrthoL(),
            _ => Arena.Center + offset
        };
    }

    private (WPos, Angle) RotatedPosRot((WDir, Angle) posRot)
    {
        return _flow switch
        {
            FlowDirection.CW => (Arena.Center + posRot.Item1.OrthoR(), posRot.Item2 - 90f.Degrees()),
            FlowDirection.CCW => (Arena.Center + posRot.Item1.OrthoL(), posRot.Item2 + 90f.Degrees()),
            _ => (Arena.Center + posRot.Item1, posRot.Item2)
        };
    }

    private bool InFireAOE(WDir corner, WPos pos)
    {
        var p1 = Arena.Center + corner;
        var p2 = RotatedPosition(corner);
        var pMid = WPos.Lerp(p1, p2, 0.5f);
        var dirMid = (pMid - Arena.Center).Normalized();
        return pos.InCone(Arena.Center, dirMid, 45f.Degrees());
    }
}
