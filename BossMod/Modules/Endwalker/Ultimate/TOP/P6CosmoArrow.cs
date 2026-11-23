namespace BossMod.Endwalker.Ultimate.TOP;

sealed class P6CosmoArrow(BossModule module) : Components.GenericAOEs(module)
{
    public enum Pattern { Unknown, InOut, OutIn }
    public record struct Line(AOEShapeRect? Shape, WPos Next, Angle Direction, WDir Advance, DateTime NextExplosion, int ExplosionsLeft);

    public Pattern CurPattern;
    private readonly List<Line> _lines = [];

    public bool Active => _lines.Count > 0;

    private readonly AOEShapeRect _shapeFirst = new(40f, 5f);
    private readonly AOEShapeRect _shapeRest = new(100f, 2.5f);

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _lines.Count;
        if (count == 0)
            return [];
        var aoes = _aoesBuffer;
        aoes.Clear();
        for (var i = 0; i < count; ++i)
        {
            var l = _lines[i];
            if (l.Shape != null && l.ExplosionsLeft > 0)
                aoes.Add(new(l.Shape, l.Next, l.Direction, l.NextExplosion));
        }
        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (CurPattern != Pattern.Unknown)
            hints.Add($"Pattern: {(CurPattern == Pattern.InOut ? "in -> out" : "out -> in")}");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CosmoArrowFirst)
        {
            var offset = caster.Position - Arena.Center;
            var offsetAbs = offset.Abs();
            var act = Module.CastFinishAt(spell);
            var rot = spell.Rotation;
            var pos = caster.Position;
            if (offsetAbs.X < 5f)
            {
                // central vertical
                _lines.Add(new(_shapeFirst, pos, rot, new(1, 0), act, 4));
                _lines.Add(new(null, pos, rot, new(-1, 0), act, 4));
                if (CurPattern == Pattern.Unknown)
                    CurPattern = Pattern.InOut;
            }
            else if (offsetAbs.Z < 5f)
            {
                // central horizontal
                _lines.Add(new(_shapeFirst, pos, rot, new(0, 1), act, 4));
                _lines.Add(new(null, pos, rot, new(0, -1), act, 4));
                if (CurPattern == Pattern.Unknown)
                    CurPattern = Pattern.InOut;
            }
            else if (offsetAbs.X < 18f)
            {
                // side vertical
                _lines.Add(new(_shapeFirst, pos, rot, new(offset.X < 0 ? 1 : -1, 0), act, 7));
                if (CurPattern == Pattern.Unknown)
                    CurPattern = Pattern.OutIn;
            }
            else if (offsetAbs.Z < 18f)
            {
                // side horizontal
                _lines.Add(new(_shapeFirst, pos, rot, new(0, offset.Z < 0 ? 1 : -1), act, 7));
                if (CurPattern == Pattern.Unknown)
                    CurPattern = Pattern.OutIn;
            }
            else
            {
                ReportError($"Unexpected exasquare origin: {caster.Position}");
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var dist = spell.Action.ID switch
        {
            (uint)AID.CosmoArrowFirst => 7.5f,
            (uint)AID.CosmoArrowRest => 5f,
            _ => default
        };
        if (dist == 0)
            return;

        ++NumCasts;

        var numLines = 0;
        foreach (ref var l in _lines.AsSpan())
        {
            if (!l.Next.AlmostEqual(caster.Position, 1) || !l.Direction.AlmostEqual(caster.Rotation, 0.1f) || (l.NextExplosion - WorldState.CurrentTime).TotalSeconds > 1)
                continue;

            if (l.ExplosionsLeft <= 0)
                ReportError($"Too many explosions: {caster.Position}");

            l.Shape = _shapeRest;
            l.Next += l.Advance * dist;
            l.NextExplosion = WorldState.FutureTime(2);
            --l.ExplosionsLeft;
            ++numLines;
        }
        if (numLines == 0)
            ReportError($"Failed to match any lines for {caster}");
    }
}
