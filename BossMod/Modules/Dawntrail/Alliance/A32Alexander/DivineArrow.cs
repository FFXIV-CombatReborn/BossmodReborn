namespace BossMod.Dawntrail.Alliance.A32Alexander;

sealed class DivineArrowCone(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle startrot;
    private readonly AOEShapeCone cone = new(45f, 45.Degrees());
    private DivineArrowLines? _lines;
    private DateTime _lastCutoff;
    private bool _lastLinesActive;

    public override void Update()
    {
        _lines ??= Module.FindComponent<DivineArrowLines>();
        var count = Sequences.Count;
        DateTime cutoff = default;
        var linesActive = _lines != null && _lines.TryGetCutoff(this, out cutoff, out _);
        if (lastCount == count && lastVersion == NumCasts && _lastLinesActive == linesActive && _lastCutoff == cutoff)
        {
            return;
        }

        lastCount = count;
        lastVersion = NumCasts;
        _lastLinesActive = linesActive;
        _lastCutoff = cutoff;
        _aoes.Clear();
        if (count == 0)
        {
            return;
        }

        var curTime = WorldState.CurrentTime;
        var maxConeTime = cutoff.AddSeconds(0.35d);
        var sequences = CollectionsMarshal.AsSpan(Sequences);
        for (var j = 0; j < count; ++j)
        {
            ref var s = ref sequences[j];
            var remaining = s.NumRemainingCasts;
            if (remaining <= 0)
            {
                continue;
            }

            var timeBetween = s.SecondsBetweenActivations;
            var nextAct = s.NextActivation;
            var time = nextAct > curTime ? nextAct : curTime;
            var num = Math.Min(remaining, s.MaxShownAOEs);
            if (linesActive)
            {
                var minShow = Math.Min(remaining, 2);
                num = 1;
                var checkTime = time;
                while (num < remaining)
                {
                    checkTime = checkTime.AddSeconds(timeBetween);
                    if (checkTime > maxConeTime && num >= minShow)
                    {
                        break;
                    }
                    ++num;
                }
            }

            var shape = s.Shape;
            var origin = s.Origin;
            var rot = s.Rotation;
            var inc = s.Increment;
            var futureTime = time;
            for (var i = 1; i < num; ++i)
            {
                rot += inc;
                futureTime = futureTime.AddSeconds(timeBetween);
                _aoes.Add(new(shape, origin, rot, futureTime, FutureColor, shapeDistance: shape.Distance(origin, rot),
                    arenaProjectionLayer: s.ArenaProjectionLayer, restrictToArenaProjectionLayer: s.RestrictToArenaProjectionLayer));
            }

            var rot2 = s.Rotation;
            _aoes.Add(new(shape, origin, rot2, nextAct, remaining > 1 ? ImminentColor : FutureColor, shapeDistance: shape.Distance(origin, rot2),
                arenaProjectionLayer: s.ArenaProjectionLayer, restrictToArenaProjectionLayer: s.RestrictToArenaProjectionLayer));
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var inc = -20.Degrees();
        if (iconID is ((uint)IconID.DivineArrowER) or ((uint)IconID.DivineArrowSR) or ((uint)IconID.DivineArrowNR) or ((uint)IconID.DivineArrowWR))
        {
            inc = -inc;
        }
        if (iconID is (uint)IconID.DivineArrowN or ((uint)IconID.DivineArrowNR))
        {
            startrot = 180.Degrees();
            Sequences.Add(new(cone, actor.Position, startrot, inc, WorldState.FutureTime(13.3f), 0.67f, 18, maxShownAOEs: 8));
        }
        if (iconID is (uint)IconID.DivineArrowS or ((uint)IconID.DivineArrowSR))
        {
            startrot = default;
            Sequences.Add(new(cone, actor.Position, startrot, inc, WorldState.FutureTime(13.3f), 0.67f, 18, maxShownAOEs: 8));
        }
        if (iconID is (uint)IconID.DivineArrowE or ((uint)IconID.DivineArrowER))
        {
            startrot = 90.Degrees();
            Sequences.Add(new(cone, actor.Position, startrot, inc, WorldState.FutureTime(13.3f), 0.67f, 18, maxShownAOEs: 8));
        }
        if (iconID is (uint)IconID.DivineArrowW or ((uint)IconID.DivineArrowWR))
        {
            startrot = -90.Degrees();
            Sequences.Add(new(cone, actor.Position, startrot, inc, WorldState.FutureTime(13.3f), 0.67f, 18, maxShownAOEs: 8));
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.DivineArrowCone or (uint)AID.DivineArrowSpamCone)
        {
            AdvanceSequence(0, WorldState.CurrentTime);
        }
    }
}

sealed class DivineArrowCircles(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.DivineArrowClose, (uint)AID.DivineArrowClose2], new AOEShapeCircle(10f), 1)
{
    //protected readonly uint[] CloseAIDs = [(uint)AID.DivineArrowClose, (uint)AID.DivineArrowClose2];
    private readonly uint[] MidAIDs = [(uint)AID.DivineArrowMid, (uint)AID.DivineArrowMid2];
    private readonly AOEShapeDonut MidShape = new(10f, 23f);
    private readonly uint[] FarAIDs = [(uint)AID.DivineArrowFar, (uint)AID.DivineArrowFar2];
    private readonly AOEShapeDonut FarShape = new(23f, 36f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var len = AIDs.Length;
        var id = spell.Action.ID;
        for (var i = 0; i < len; ++i)
        {
            if (id == AIDs[i])
            {
                var origin = spell.LocXZ;
                var rotation = spell.Rotation;
                Casters.Add(new(Shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, shapeDistance: Shape.Distance(origin, rotation)));
                SortHelpers.SortAOEByActivation(Casters);
                return;
            }
        }
        for (var i = 0; i < MidAIDs.Length; ++i)
        {
            if (id == MidAIDs[i])
            {
                var origin = spell.LocXZ;
                var rotation = spell.Rotation;
                Casters.Add(new(MidShape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, shapeDistance: MidShape.Distance(origin, rotation)));
                SortHelpers.SortAOEByActivation(Casters);
            }
        }
        for (var i = 0; i < FarAIDs.Length; ++i)
        {
            if (id == FarAIDs[i])
            {
                var origin = spell.LocXZ;
                var rotation = spell.Rotation;
                Casters.Add(new(FarShape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, shapeDistance: FarShape.Distance(origin, rotation)));
                SortHelpers.SortAOEByActivation(Casters);
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        // we probably dont need to check for AIDs here since actorID should already be unique to any active spell
        var count = Casters.Count;
        var id = caster.InstanceID;
        var aoes = CollectionsMarshal.AsSpan(Casters);
        for (var i = 0; i < count; ++i)
        {
            if (aoes[i].ActorID == id)
            {
                Casters.RemoveAt(i);
                return;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var len = AIDs.Length;
        for (var i = 0; i < len; ++i)
        {
            if (spell.Action.ID == AIDs[i])
            {
                ++NumCasts;
                return;
            }
        }
        for (var i = 0; i < MidAIDs.Length; ++i)
        {
            if (spell.Action.ID == MidAIDs[i])
            {
                ++NumCasts;
                return;
            }
        }
        for (var i = 0; i < FarAIDs.Length; ++i)
        {
            if (spell.Action.ID == FarAIDs[i])
            {
                ++NumCasts;
                return;
            }
        }
    }
}

sealed class DivineArrowLines(BossModule module) : Components.SimpleAOEGroupsByTimewindow(module, [(uint)AID.DivineArrowLines, (uint)AID.DivineArrowLines2], new AOEShapeRect(60f, 5f), 0.5d, 2)
{
    private const float TileHalfSize = 5f;
    private const float SafeZoneHalfSize = 4.5f;

    private DivineArrowCone? _cone;
    private readonly List<WPos> _lineSafeSquares = new(25);
    private readonly List<WPos> _safeSquares = new(9);
    private SDInvertedUnion? _safeForbiddenZone;
    private DateTime _cachedWaveEnd;
    private DateTime _cachedCutoff;
    private int _cachedConeSeqCount = -1;
    private int _cachedConeNumCasts = -1;

    /// <summary>
    /// Computes the safety cutoff timestamp for the current line wave: the end of the current wave,
    /// or the end of the rotating cone sequence if this is the final line wave and the cones finish shortly after.
    /// </summary>
    public bool TryGetCutoff(DivineArrowCone? cone, out DateTime cutoff, out DateTime waveEnd)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            cutoff = default;
            waveEnd = default;
            return false;
        }

        var waveEndIdx = GetCurrentWaveEndIndex();
        waveEnd = Casters[waveEndIdx - 1].Activation;
        cutoff = waveEnd;

        if (cone != null && cone.Sequences.Count != 0)
        {
            ref readonly var seq = ref CollectionsMarshal.AsSpan(cone.Sequences)[0];
            var rem = seq.NumRemainingCasts;
            if (rem > 0)
            {
                var coneEnd = seq.NextActivation.AddSeconds((rem - 1) * seq.SecondsBetweenActivations);
                // Wave 4 ends ~1.8s before the final cone; Wave 2 ends ~8.0s before it, so 3s cleanly separates final wave.
                if (count <= waveEndIdx && coneEnd <= waveEnd.AddSeconds(3.0d))
                {
                    cutoff = coneEnd;
                }
            }
        }
        return true;
    }

    private int GetCurrentWaveEndIndex()
    {
        var aoes = CollectionsMarshal.AsSpan(Casters);
        var count = aoes.Length;
        var deadline = aoes[0].Activation.AddSeconds(TimeWindowInSeconds);
        var end = 0;
        while (end < count && aoes[end].Activation < deadline)
        {
            ++end;
        }
        return end;
    }

    public override void Update()
    {
        _cone ??= Module.FindComponent<DivineArrowCone>();
        if (!TryGetCutoff(_cone, out var cutoff, out var waveEnd))
        {
            if (_safeSquares.Count != 0)
            {
                _safeSquares.Clear();
                _safeForbiddenZone = null;
                _cachedWaveEnd = default;
                _cachedCutoff = default;
                _cachedConeSeqCount = -1;
                _cachedConeNumCasts = -1;
            }
            return;
        }

        var seqCount = _cone?.Sequences.Count ?? 0;
        var coneCasts = _cone?.NumCasts ?? 0;
        if (_cachedWaveEnd == waveEnd && _cachedCutoff == cutoff && _cachedConeSeqCount == seqCount && _cachedConeNumCasts == coneCasts)
        {
            return;
        }

        _cachedWaveEnd = waveEnd;
        _cachedCutoff = cutoff;
        _cachedConeSeqCount = seqCount;
        _cachedConeNumCasts = coneCasts;
        RebuildSafeSquares(cutoff);
    }

    private void RebuildSafeSquares(DateTime cutoff)
    {
        _lineSafeSquares.Clear();
        _safeSquares.Clear();
        _safeForbiddenZone = null;

        var waveCount = GetCurrentWaveEndIndex();
        var wave = CollectionsMarshal.AsSpan(Casters)[..waveCount];
        var center = Arena.Center;
        for (var iz = 0; iz < 5; ++iz)
        {
            var dz = (iz - 2) * 10f;
            for (var ix = 0; ix < 5; ++ix)
            {
                var sq = center + new WDir((ix - 2) * 10f, dz);
                var hit = false;
                for (var i = 0; i < wave.Length; ++i)
                {
                    if (wave[i].Check(sq))
                    {
                        hit = true;
                        break;
                    }
                }
                if (!hit)
                {
                    _lineSafeSquares.Add(sq);
                }
            }
        }

        if (_lineSafeSquares.Count == 0)
        {
            return;
        }

        if (_cone != null && _cone.Sequences.Count != 0 && _cone.Sequences[0].NumRemainingCasts > 0)
        {
            ref readonly var seq = ref CollectionsMarshal.AsSpan(_cone.Sequences)[0];
            FilterSquaresAgainstCones(in seq, cutoff.AddSeconds(0.35d), SafeZoneHalfSize);
            if (_safeSquares.Count == 0)
            {
                FilterSquaresAgainstCones(in seq, cutoff, 2.5f);
            }
            if (_safeSquares.Count == 0)
            {
                FilterSquaresAgainstCones(in seq, cutoff, 0f);
            }
        }

        if (_safeSquares.Count == 0)
        {
            _safeSquares.AddRange(_lineSafeSquares);
        }

        var count = _safeSquares.Count;
        var rects = new ShapeDistance[count];
        for (var i = 0; i < count; ++i)
        {
            rects[i] = new SDRect(_safeSquares[i], default(Angle), SafeZoneHalfSize, SafeZoneHalfSize, SafeZoneHalfSize);
        }
        _safeForbiddenZone = new SDInvertedUnion(rects);
    }

    private void FilterSquaresAgainstCones(in Components.GenericRotatingAOE.Sequence seq, DateTime maxConeTime, float sampleHalf)
    {
        var curTime = WorldState.CurrentTime;
        var baseTime = seq.NextActivation > curTime ? seq.NextActivation : curTime;
        var lineSafeCount = _lineSafeSquares.Count;
        for (var i = 0; i < lineSafeCount; ++i)
        {
            var sq = _lineSafeSquares[i];
            if (!IsSquareHitByCones(sq, in seq, baseTime, maxConeTime, sampleHalf))
            {
                _safeSquares.Add(sq);
            }
        }
    }

    private static bool IsSquareHitByCones(WPos sq, in Components.GenericRotatingAOE.Sequence seq, DateTime baseTime, DateTime maxConeTime, float sampleHalf)
    {
        var rem = seq.NumRemainingCasts;
        var stepTime = seq.SecondsBetweenActivations;
        var shape = seq.Shape;
        var origin = seq.Origin;

        for (var k = 0; k < rem; ++k)
        {
            var coneTime = k == 0 ? seq.NextActivation : baseTime.AddSeconds(k * stepTime);
            if (coneTime > maxConeTime)
            {
                break;
            }
            var rot = seq.Rotation + k * seq.Increment;
            if (IsSampleGridHit(sq, sampleHalf, shape, origin, rot))
            {
                return true;
            }
        }
        return false;
    }

    private static bool IsSampleGridHit(WPos sq, float sampleHalf, AOEShape shape, WPos origin, Angle rot)
    {
        if (sampleHalf <= 0f)
        {
            return shape.Check(sq, origin, rot);
        }
        for (var sz = -1; sz <= 1; ++sz)
        {
            var oz = sz * sampleHalf;
            for (var sx = -1; sx <= 1; ++sx)
            {
                if (shape.Check(sq + new WDir(sx * sampleHalf, oz), origin, rot))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);
        var count = _safeSquares.Count;
        for (var i = 0; i < count; ++i)
        {
            Arena.ZoneRect(_safeSquares[i], default(Angle), TileHalfSize, TileHalfSize, TileHalfSize, Colors.SafeFromAOE);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        var count = _safeSquares.Count;
        if (count == 0)
        {
            return;
        }

        var pos = actor.Position;
        for (var i = 0; i < count; ++i)
        {
            var d = pos - _safeSquares[i];
            if (Math.Abs(d.X) <= TileHalfSize && Math.Abs(d.Z) <= TileHalfSize)
            {
                hints.Add("Stay in safe square!", false);
                return;
            }
        }
        hints.Add("Move to safe square!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_safeForbiddenZone != null)
        {
            hints.AddForbiddenZone(_safeForbiddenZone, _cachedWaveEnd);
        }
    }
}
