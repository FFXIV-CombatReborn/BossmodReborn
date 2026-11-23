namespace BossMod.Endwalker.VariantCriterion.V03Aloalo.V035Loquloqui;

public enum OID : uint
{
    Boss = 0x3FD6, // R6.480-16.200, x1
    Helper = 0x233C, // R0.500, x?, Helper type
    UolosapaLoqua = 0x3FD7, // R0.800-2.400, x?, birds (Rush lines)
    RepurubaLoqua = 0x3FD8, // R0.960-2.880, x?, newts (Turnabout circles)
    PetalMarkerA = 0x3FD9, // R0.050, x?, O Petals Unfurl landing spots
    PetalMarkerB = 0x3FDA, // R0.050, x?, O Petals Unfurl wall buds
    PetalTarget = 0x3FDB, // R1.000, x?, dummy tether target for petal markers
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    Teleport = 34747, // Boss->location, no cast, single-target

    LongLostLight = 34748, // Boss->self, 5.0s cast, range 55 circle, raidwide (+ outer danger zone on first cast)
    ProtectiveWill = 34750, // Boss->player, 5.0s cast, single-target, tankbuster
    LandWave = 34749, // Boss->self, 6.0s cast, range 25 width 40 rect, half-arena cleave

    SummoningRite = 34751, // Boss->self, 4.0s cast, single-target, spawn birds/newts
    OLifeFlourish = 34756, // Boss->self, 5.0s cast, single-target, tether adds to enlarge AoEs
    Rush = 34752, // UolosapaLoqua->self, 8.0s cast, range 35 width 2 rect
    RushFlourish = 34753, // UolosapaLoqua->self, 8.0s cast, range 35 width 10 rect (the enlarged variant)
    Turnabout = 34754, // RepurubaLoqua->self, 8.0s cast, range 4 circle
    TurnaboutFlourish = 34755, // RepurubaLoqua->self, 8.0s cast, range 4 circle (enlarged)

    OPetalsUnfurl = 34757, // Boss->self, 5.0s cast, single-target
    PetalTravelShort = 35892, // Helper->self, 8.0s cast, single-target
    PetalTravelLong = 35893, // Helper->self, 8.0s cast, single-target
    PliantPetals = 34758, // Helper->self, 2.0s cast, range 15 circle

    OIsleBloom = 34759, // Boss->self, 5.0s cast, single-target
    BrilliantBlossoms = 34760, // Helper->self, no cast, range 10 width 10 rect

    OSkyBeMine = 34761, // Boss->self, 5.0s cast, single-target, grow
    SanctuaryVisual = 34762, // Boss->self, 5.0s cast, range 30 circle
    Sanctuary = 36152, // Helper->self, 5.7s cast, range 30 circle, proximity
    StirringOfSpirits = 34763, // Boss->self, 5.0s cast, range 30 circle
    Shockwave = 34764, // Boss->self, no cast, range 30 circle, knockback
    IslebloomLight = 34765, // Helper->self, 5.0s cast, range 7 circle
}

public enum TetherID : uint
{
    Flourish = 259, // bird/newt/petal -> ?
}

public static class Arenas
{
    public static readonly ArenaBoundsRect Initial = new(25, 20);
    public static readonly ArenaBoundsRect Shrunk = new(20, 15);
}

sealed class LongLostLight(BossModule module) : Components.RaidwideCast(module, (uint)AID.LongLostLight)
{
    private bool _shrunk;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (!_shrunk && spell.Action.ID == (uint)AID.LongLostLight)
        {
            _shrunk = true;
            Module.Arena.Bounds = Arenas.Shrunk;
        }
    }
}

sealed class ProtectiveWill(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ProtectiveWill);
sealed class LandWave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LandWave, new AOEShapeRect(25, 20));

sealed class Rush(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Rush, new AOEShapeRect(35, 1));
sealed class RushFlourish(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RushFlourish, new AOEShapeRect(35, 5));
sealed class Turnabout(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Turnabout, 4);
sealed class TurnaboutFlourish(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TurnaboutFlourish, 12);

sealed class PliantPetals(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(WPos Origin, DateTime Activation, int Wave)> _aoes = [];
    private static readonly AOEShapeCircle _shape = new(15);

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoesBuffer.Clear();
        if (_aoes.Count == 0)
            return CollectionsMarshal.AsSpan(_aoesBuffer);

        // waves that are already known: the lowest one is imminent
        var knownCount = 0;
        var imminentWave = int.MaxValue;
        foreach (var a in _aoes)
        {
            if (a.Wave >= 0)
            {
                ++knownCount;
                imminentWave = Math.Min(imminentWave, a.Wave);
            }
        }
        if (knownCount == 0)
            imminentWave = -1;
        var hasSubsequent = false;
        foreach (var a in _aoes)
            hasSubsequent |= a.Wave >= 0 && a.Wave > imminentWave;
        foreach (var a in _aoes)
        {
            var isImminent = knownCount == 0 || a.Wave >= 0 && a.Wave == imminentWave;
            var solid = isImminent && hasSubsequent;
            var risky = !hasSubsequent || isImminent;
            _aoesBuffer.Add(new(_shape, a.Origin, default, a.Activation, solid ? Colors.Danger : Colors.AOE, risky: risky));
        }
        return CollectionsMarshal.AsSpan(_aoesBuffer);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.OPetalsUnfurl:
                _aoes.Clear();
                break;
            case (uint)AID.PetalTravelShort:
            case (uint)AID.PetalTravelLong:
                RefreshFromTravels();
                break;
            case (uint)AID.PliantPetals:
                Upsert(spell.LocXZ, Module.CastFinishAt(spell), wave: null);
                break;
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (source.OID != (uint)OID.PetalMarkerA || tether.ID != (uint)TetherID.Flourish)
            return;
        Upsert(source.Position, WorldState.FutureTime(20), -1);
        ReclassifyByDistance();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.PliantPetals)
            return;

        var origin = caster.CastInfo?.LocXZ ?? caster.Position;
        if (origin == default)
            origin = caster.Position;

        var i = _aoes.FindIndex(a => a.Origin.AlmostEqual(origin, 3f));
        if (i < 0 && _aoes.Count > 0)
            i = _aoes.Select((a, idx) => (a, idx)).MinBy(t => (t.a.Origin - origin).LengthSq()).idx;
        if (i >= 0)
            _aoes.RemoveAt(i);
    }

    private void RefreshFromTravels()
    {
        if (_aoes.Count == 0)
            return;

        var travels = new List<(WPos Wall, ActorCastInfo Cast)>();
        foreach (var a in WorldState.Actors)
        {
            if (a.CastInfo == null)
                continue;
            if (a.CastInfo.Action.ID is not ((uint)AID.PetalTravelShort or (uint)AID.PetalTravelLong))
                continue;
            var wall = a.CastInfo.LocXZ;
            if (wall == default)
                wall = a.Position;
            travels.Add((wall, a.CastInfo));
        }

        if (travels.Count == 0)
        {
            ReclassifyByDistance();
            return;
        }

        // wall <-> landing pairs, ranked by length
        var usedLanding = new bool[_aoes.Count];
        var usedTravel = new bool[travels.Count];
        var pairs = new List<(int Landing, int Travel, float DistSq)>();
        var n = Math.Min(_aoes.Count, travels.Count);
        for (var k = 0; k < n; ++k)
        {
            var bestD = float.MaxValue;
            var bestL = -1;
            var bestT = -1;
            for (var li = 0; li < _aoes.Count; ++li)
            {
                if (usedLanding[li])
                    continue;
                for (var ti = 0; ti < travels.Count; ++ti)
                {
                    if (usedTravel[ti])
                        continue;
                    var d = (_aoes[li].Origin - travels[ti].Wall).LengthSq();
                    if (d < bestD)
                    {
                        bestD = d;
                        bestL = li;
                        bestT = ti;
                    }
                }
            }
            if (bestL < 0)
                break;
            usedLanding[bestL] = usedTravel[bestT] = true;
            pairs.Add((bestL, bestT, bestD));
        }

        pairs.Sort((a, b) => a.DistSq.CompareTo(b.DistSq));
        var staggered = pairs.Count >= 4;
        var half = pairs.Count / 2;
        for (var r = 0; r < pairs.Count; ++r)
        {
            var (li, ti, _) = pairs[r];
            var wave = staggered && r >= half ? 1 : 0;
            var delay = wave == 0 ? 2.6f : 6.5f;
            var prev = _aoes[li];
            _aoes[li] = (prev.Origin, Module.CastFinishAt(travels[ti].Cast, delay), wave);
        }
    }

    private void ReclassifyByDistance()
    {
        if (_aoes.Count == 0)
            return;

        if (_aoes.Count < 4)
        {
            for (var i = 0; i < _aoes.Count; ++i)
            {
                var p = _aoes[i];
                _aoes[i] = (p.Origin, p.Activation, 0);
            }
            return;
        }

        // only add tethered ones since there's a bunch inactive at all times
        var walls = Module.Enemies((uint)OID.PetalMarkerB).Where(w => w.Tether.ID == (uint)TetherID.Flourish).Select(w => w.Position).ToList();
        if (walls.Count == 0)
            return;

        var usedL = new bool[_aoes.Count];
        var usedW = new bool[walls.Count];
        var distSq = new float[_aoes.Count];
        Array.Fill(distSq, float.MaxValue);
        var n = Math.Min(_aoes.Count, walls.Count);
        for (var k = 0; k < n; ++k)
        {
            var bestD = float.MaxValue;
            var bestL = -1;
            var bestW = -1;
            for (var li = 0; li < _aoes.Count; ++li)
            {
                if (usedL[li])
                    continue;
                for (var wi = 0; wi < walls.Count; ++wi)
                {
                    if (usedW[wi])
                        continue;
                    var d = (_aoes[li].Origin - walls[wi]).LengthSq();
                    if (d < bestD)
                    {
                        bestD = d;
                        bestL = li;
                        bestW = wi;
                    }
                }
            }
            if (bestL < 0)
                break;
            usedL[bestL] = usedW[bestW] = true;
            distSq[bestL] = bestD;
        }

        var order = Enumerable.Range(0, _aoes.Count).OrderBy(i => distSq[i]).ToArray();
        var half = order.Length / 2;
        for (var r = 0; r < order.Length; ++r)
        {
            var i = order[r];
            var p = _aoes[i];
            _aoes[i] = (p.Origin, p.Activation, r < half ? 0 : 1);
        }
    }

    private void Upsert(WPos origin, DateTime activation, int? wave)
    {
        if (origin == default)
            return;
        var i = _aoes.FindIndex(a => a.Origin.AlmostEqual(origin, 1.5f));
        if (i >= 0)
        {
            var prev = _aoes[i];
            _aoes[i] = (origin,
                activation != default ? activation : prev.Activation,
                wave ?? prev.Wave);
        }
        else
            _aoes.Add((origin, activation, wave ?? -1));
    }
}

sealed class BrilliantBlossoms(BossModule module) : Components.GenericAOEs(module, (uint)AID.BrilliantBlossoms)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect _shape = new(5, 5, 5);

    // relative to arena center
    private static readonly WDir[] _pattern =
    [
        new(-15, -10), new(-5, -10), new(5, -10), new(15, -10),
        new(15, 0), new(5, 0), new(-5, 0), new(-15, 0),
        new(-15, 10), new(-5, 10), new(5, 10), new(15, 10),
    ];

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoesBuffer.Clear();
        var imminent = _aoes.Count > 0 ? _aoes[0].Activation.AddSeconds(1.0f) : default;
        foreach (var a in _aoes)
            _aoesBuffer.Add(a with { Color = a.Activation <= imminent ? Colors.Danger : Colors.AOE });
        return CollectionsMarshal.AsSpan(_aoesBuffer);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.OIsleBloom)
            return;

        _aoes.Clear();
        var t = Module.CastFinishAt(spell, 11.5f);
        for (var i = 0; i < _pattern.Length; ++i)
        {
            _aoes.Add(new(_shape, Module.Center + _pattern[i], default, t));
            t = t.AddSeconds(0.5f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.BrilliantBlossoms)
            return;

        ++NumCasts;
        var origin = caster.Position;
        var i = _aoes.FindIndex(a => a.Origin.AlmostEqual(origin, 2));
        if (i >= 0)
            _aoes.RemoveAt(i);
        else if (_aoes.Count > 0)
            _aoes.RemoveAt(0);
    }
}

sealed class Sanctuary(BossModule module) : Components.ProximityAOEs(module, (uint)AID.Sanctuary, 18);

sealed class IslebloomLight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IslebloomLight, 7);

// TODO: better AI support. I can get it to adjust on the first shockwave but subsequent it gives up cause my inverted rect forbids the whole arena.
sealed class Shockwave(BossModule module) : Components.GenericKnockback(module, (uint)AID.Shockwave, stopAtWall: false)
{
    private DateTime _next;
    private int _left;
    private const float Distance = 8;
    private const float StandOffset = 2f;
    private const float WallMargin = 2.5f;

    private readonly List<Knockback> _kbBuffer = [];
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        _kbBuffer.Clear();
        if (_left > 0 && _next != default)
            _kbBuffer.Add(new(Module.Center, Distance, _next));
        return CollectionsMarshal.AsSpan(_kbBuffer);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_left <= 0 || _next == default || IsImmune(slot, _next))
            return;

        var origin = Module.Center;
        _puddlesBuffer.Clear();
        if (Module.FindComponent<IslebloomLight>() is { } light)
        {
            var aoes = light.ActiveAOEs(slot, actor);
            for (var i = 0; i < aoes.Length; ++i)
            {
                if (!_puddlesBuffer.Contains(aoes[i].Origin))
                    _puddlesBuffer.Add(aoes[i].Origin);
            }
        }
        var aim = SafeAim(origin, CollectionsMarshal.AsSpan(_puddlesBuffer));
        var stand = origin + aim * StandOffset;

        // soft hint only cause a hard makes the ai confused
        hints.GoalZones.Add(AIHints.GoalProximity(stand, 1.5f, 80));
        hints.AddForbiddenZone(new SDInvertedCircle(stand, 1.5f), _next);
    }

    private readonly List<WPos> _puddlesBuffer = [];

    private WDir SafeAim(WPos origin, ReadOnlySpan<WPos> puddles)
    {
        WDir best = new(1, 0); // prefer long axis of shrunk rect
        var bestScore = float.MinValue;
        for (var i = 0; i < 24; ++i)
        {
            var dir = (i * 15).Degrees().ToDirection();
            var dest = origin + dir * (StandOffset + Distance);
            var off = dest - Module.Center;
            var clear = Math.Min(Arenas.Shrunk.HalfWidth - WallMargin - Math.Abs(off.X),
                Arenas.Shrunk.HalfHeight - WallMargin - Math.Abs(off.Z));
            if (clear < 0)
                continue;
            foreach (var p in puddles)
                clear = Math.Min(clear, (dest - p).Length() - 7.5f);
            if (clear > bestScore)
            {
                bestScore = clear;
                best = dir;
            }
        }
        return best;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.StirringOfSpirits)
        {
            _left = 4;
            NumCasts = 0;
            _next = Module.CastFinishAt(spell, 2.8f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.Shockwave || _left <= 0)
            return;

        ++NumCasts;
        --_left;
        _next = _left > 0 ? WorldState.FutureTime(4.5f) : default;
    }
}

sealed class V035LoquloquiStates : StateMachineBuilder
{
    public V035LoquloquiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LongLostLight>()
            .ActivateOnEnter<ProtectiveWill>()
            .ActivateOnEnter<LandWave>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<RushFlourish>()
            .ActivateOnEnter<Turnabout>()
            .ActivateOnEnter<TurnaboutFlourish>()
            .ActivateOnEnter<PliantPetals>()
            .ActivateOnEnter<BrilliantBlossoms>()
            .ActivateOnEnter<Sanctuary>()
            .ActivateOnEnter<IslebloomLight>()
            .ActivateOnEnter<Shockwave>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 961, NameID = 12636, Contributors = "croizat (ported from upstream and modified by LTS)")]
public sealed class V035Loquloqui(WorldState ws, Actor primary) : BossModule(ws, primary, new(950, -860), Arenas.Initial);
