namespace BossMod.Endwalker.Extreme.Ex1Zodiark;

// state related to astral eclipse mechanic
// 'pattern' is a mask containing explosion spots; index is 4 bits, with low 2 bits describing world X position and 2 high bits describing world Z position
// so NE corner (X=+1, Z=-1) corresponds to index 0b0010 = 2; S corner (X=0, Z=+1) corresponds to index 0b1001 = 9 and so on (0b11 index is unused)
// 'completed' or 'not started' is represented as fully safe (all 0) mask, 'unknown' pattern is represented as fully dangerous (all 1) mask
sealed class AstralEclipse(BossModule module) : BossComponent(module)
{
    private readonly int[] _patterns = new int[3]; // W -> S -> E

    private readonly AOEShapeCircle _aoe = new(10f);

    // transform from 'pattern space' (X goes to the right, Y goes to the bottom) to world space
    private const float _centerOffset = 14f;
    private readonly Vector3[] _basisX = [-Vector3.UnitZ, -Vector3.UnitX, Vector3.UnitZ];
    private readonly Vector3[] _basisY = [-Vector3.UnitX, Vector3.UnitZ, Vector3.UnitX];
    private readonly List<WPos> _spots = [with(9)];
    private readonly List<(WPos From, WPos To)> _moves = [with(3)];

    private int NextPattern()
    {
        for (var i = 0; i < _patterns.Length; ++i)
        {
            if (_patterns[i] != 0)
                return _patterns[i];
        }
        return 0;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var spots = PatternSpots(NextPattern());
        for (var i = 0; i < spots.Count; ++i)
        {
            if (_aoe.Check(actor.Position, spots[i]))
            {
                hints.Add("GTFO from explosion!");
                return;
            }
        }
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        var moves = MovementPath(actor.Position);
        for (var i = 0; i < moves.Count; ++i)
            movementHints.Add(moves[i].From, moves[i].To, Colors.Safe);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var spots = PatternSpots(NextPattern());
        for (var i = 0; i < spots.Count; ++i)
            _aoe.Draw(Arena, spots[i]);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var moves = MovementPath(pc.Position);
        for (var i = 0; i < moves.Count; ++i)
            Arena.AddLine(moves[i].From, moves[i].To, Colors.Safe);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is < 6 or > 8)
            return;
        var seq = index - 6;
        _patterns[seq] = state switch
        {
            0x00080004u => 0, // done
            0x00020001u => BuildPattern(seq, new(0, 1), new(-1, 0)), // safe B & L
            0x00200010u => BuildPattern(seq, new(1, 1), new(0, 0)), // safe BR & C
            0x00800040u => BuildPattern(seq, new(-1, 1), new(1, 0)), // safe BL & R
            0x10000800u => BuildPattern(seq, new(1, 1), new(0, -1)), // safe BR & T
            _ => 0xFFFF, // unknown
        };
    }

    private int BuildMask(int seq, Vector2 patternCoords)
    {
        var w = _basisX[seq] * patternCoords.X + _basisY[seq] * patternCoords.Y;
        var xPos = w.X > 0 ? 2 : (w.X < 0 ? 0 : 1);
        var zPos = w.Z > 0 ? 2 : (w.Z < 0 ? 0 : 1);
        return 1 << (zPos * 4 + xPos);
    }

    private int BuildPattern(int seq, Vector2 safe1, Vector2 safe2)
    {
        return 0x777 & ~BuildMask(seq, safe1) & ~BuildMask(seq, safe2);
    }

    private List<WPos> PatternSpots(int pattern)
    {
        _spots.Clear();
        if (pattern != 0)
        {
            var center = Arena.Center;
            for (var z = -1; z <= 1; ++z)
                for (var x = -1; x <= 1; ++x)
                    if ((pattern & (1 << ((z + 1) * 4 + (x + 1)))) != 0)
                        _spots.Add(center + _centerOffset * new WDir(x, z));
        }
        return _spots;
    }

    // path through the safe spots of each known pattern, starting from the player, tested in an unsynced but idk might need work TODO: run full party or get full log
    private List<(WPos From, WPos To)> MovementPath(WPos startingPosition)
    {
        _moves.Clear();
        var prev = startingPosition;
        var boss = Module.PrimaryActor.Position;
        for (var i = 0; i < _patterns.Length; ++i)
        {
            var p = _patterns[i];
            if (p == 0)
                continue;
            if (p == 0xFFFF)
                break;

            var safe = PatternSpots(~p);
            if (safe.Count == 0)
                break;
            var next = safe[0];
            var best = float.MaxValue;
            for (var j = 0; j < safe.Count; ++j)
            {
                var pos = safe[j];
                var score = (pos - prev).LengthSq() + (pos - boss).LengthSq() * 0.2f;
                if (score < best)
                {
                    best = score;
                    next = pos;
                }
            }
            _moves.Add((prev, next));
            prev = next;
        }
        return _moves;
    }
}
