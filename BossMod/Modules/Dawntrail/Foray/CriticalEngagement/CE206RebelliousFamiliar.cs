using BossMod.Dawntrail.Foray.CriticalEngagement;
using static BossMod.Components.GenericKnockback;

namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE206RebelliousFamiliar;

public enum OID : uint
{
    Boss = 0x4C4F, // R3.8, BNpcName 14791, cornered gemstone
    YellowGem = 0x4C50,
    BoundaryController = 0x4D88, // non-targetable controller at arena center
    RubyWallTian = 0x1EC045, // baseid 2015301 (decimal), 田 ruby wall (EAnim 2 = appear)
    RubyWallL = 0x1EC046, // baseid 2015302 (decimal), L/¬ ruby wall (EAnim 20/200 = moving)
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 0xC6A4, // boss->player, no cast, single-target
    LethalBoundary = 0xBFD0, // controller, persistent out-of-bounds kill field
    YellowGemstones = 0xBC98,
    YellowGemActiveVisual = 0xBC99, // yellow gem->location, 3.0s cast, no damage event
    TopazRay = 0xBC9A, // yellow gem->location, 3.0s cast, range 4 circle
    RubyLight = 0xBC9C,
    RubyReflectionShort = 0xBC9D, // helper, 20y long, 20y wide rect
    RubyReflectionLong1 = 0xBC9E, // helper, 40y long, 40y wide rect
    RubyReflectionLong2 = 0xBC9F,
    CircularKnockbackTelegraph = 0xBCA0, // helper, 60y circle; resolves as 30y away knockback
    KnockAsideTelegraph = 0xBCA1, // helper, 40y long, 60y wide rect; resolves as 15y source-left/right knockback per target side
    RavenousGods = 0xBCA3,
    RavenousGodsSecond = 0xBCA4,
    ClawThenTail = 0xBCA6, // 45y 180-degree cone
    TailThenClaw = 0xBCA7, // 40y 180-degree cone
    ClawThenTailSecond = 0xBCA8,
    TailThenClawSecond = 0xBCA9,
    Howl = 0xBCAA,
    ComboEndVisual = 0xBCAB, // boss, no targets/effects; animation-only combo terminator
    RubyOuterReflection = 0xC4F2,
    RevertModel = 0xC51D, // boss, model-state reset after the claw/tail sequence
    RubyGlowHit = 0xC5CD, // helpers, split packets for the Ruby Light raidwide
    HowlAlt = 0xC161,
    RavenousGodsCircleHit = 0xC162,
    RavenousGodsAsideHit = 0xC163
}


sealed class ClawTailCombo(BossModule module) : ReplayValidatedOppositeAOEs(module)
{
    private static readonly AOEShapeCone Claw = new(45f, 90f.Degrees());
    private static readonly AOEShapeCone Tail = new(40f, 90f.Degrees());

    protected override SequenceConfig? ConfigFor(uint firstActionID) => firstActionID switch
    {
        (uint)AID.ClawThenTail => new(Claw, Tail, (uint)AID.ClawThenTailSecond, 2d),
        // Replay-verified: even though the boss visually spins around before the first hit lands,
        // both first hits resolve centered on the cast-start rotation (hits within +-90 deg of it)
        // and both second hits resolve on the opposite half. No rotation offset for either combo.
        (uint)AID.TailThenClaw => new(Tail, Claw, (uint)AID.TailThenClawSecond, 2d),
        _ => null
    };
}

sealed class TopazRay(BossModule module) : ReplayValidatedCastAOEs(module)
{
    private static readonly AOEShapeCircle Shape = new(4f);

    protected override AOEConfig? ConfigFor(uint actionID) => actionID == (uint)AID.TopazRay ? new(Shape, true) : null;
}

sealed class LethalBoundary(BossModule module) : Components.GenericAOEs(module)
{
    // Arena radius is 20y (4x4 grid of 10y cells, trigger XML R=20); the kill fence is that square.
    private static readonly AOEShapeRect Shape = new(20f, 0.5f, 20f);
    private static readonly AOEShapeRect AIShape = new(20f, 1f, 20f);
    private readonly AOEInstance[] _aoes = Build(module.Arena.Center);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var aoe in _aoes)
            hints.AddForbiddenZone(AIShape.Distance(aoe.Origin, aoe.Rotation));
    }

    private static AOEInstance[] Build(WPos center)
    {
        var result = new AOEInstance[4];
        for (var i = 0; i < result.Length; ++i)
        {
            var normal = (i * 90f).Degrees().ToDirection();
            var rotation = Angle.FromDirection(normal.OrthoL());
            var origin = center + 20f * normal;
            result[i] = new(Shape, origin, rotation, color: Colors.Danger, risky: false, shapeDistance: Shape.Distance(origin, rotation));
        }
        return result;
    }
}

// North-Horn trigger XML logic (北岛.xml, "08. 叛逆使魔——负隅宝石兽"):
// - Arena is a 4x4 grid of 10y cells (R=20). Ruby walls (baseid 2015301 田 / 2015302 L-shaped,
//   EAnim 2 = appear, 20/200 = moving; facing parity selects the table) define four candidate
//   dangerous regions of four whole cells.
// - Yellow gemstones (0x4C50) near a cell edge (max |dx|,|dy| >= 2.9y) each contribute one
//   ColRowDir4 token. Triggernometry's VecToDir uses the game heading convention (x,z),
//   indexed N=0, W=1, S=2, E=3.
// - The dangerous region is the candidate whose edge-token set intersects the observed tokens;
//   announce it as soon as the wall appears so AI can route away before the line resolves.
internal static class RubyReflectionData
{
    // Triggernometry Roundvec indexes cardinal directions from north, counter-clockwise.
    // (N=0, W=1, S=2, E=3.)
    public static int VecToDir(float dx, float dz)
        => (int)MathF.Round((MathF.Atan2(dx, dz) / MathF.PI + 1f) * 2f) & 3;

    // Triggernometry 2.x RadToDir (renamed from roundir): north is a segment point, indexes are
    // N=0, W=1, S=2, E=3 for n=4.  Implementation mirrors MathParser.ProcessRoundir:
    //   dir = (rad/pi + 1)/2 * n;  dir = mod(dir + 0.5, n) - 0.5;  round (banker's).
    public static int RadToDir(float rad, int segments)
    {
        var dir = (rad / MathF.PI + 1f) / 2f * segments;
        dir = Mod(dir + 0.5f, segments) - 0.5f;
        return (int)MathF.Round(dir, MidpointRounding.ToEven);
    }

    private static float Mod(float a, float n) => a - MathF.Floor(a / n) * n;

    public static int[] SelectCells(string tableName, IEnumerable<string> edgeGems)
    {
        var tokens = edgeGems.ToHashSet(StringComparer.Ordinal);
        var table = tableName switch
        {
            "tian" => WallTableTian,
            "l20p0" => WallTableL20P0,
            "l20p1" => WallTableL20P1,
            "l200p0" => WallTableL200P0,
            "l200p1" => WallTableL200P1,
            _ => null
        };
        if (table == null)
            return [];

        var cells = new HashSet<int>();
        foreach (var zone in table)
            if (zone.Keys.Any(tokens.Contains))
                cells.UnionWith(zone.Cells);
        return [.. cells.Order()];
    }

    private static readonly (string[] Keys, int[] Cells)[] WallTableTian =
    [
        (["122", "213", "222", "223"], [11, 12, 21, 22]),
        (["130", "230", "233", "243"], [13, 14, 23, 24]),
        (["311", "321", "322", "422"], [31, 32, 41, 42]),
        (["330", "331", "341", "430"], [33, 34, 43, 44]),
    ];
    private static readonly (string[] Keys, int[] Cells)[] WallTableL20P0 =
    [
        (["113", "122", "220", "222", "320", "322", "323"], [11, 12, 22, 32]),
        (["130", "133", "240", "340", "343"], [13, 14, 24, 34]),
        (["211", "212", "312", "421", "422"], [21, 31, 41, 42]),
        (["230", "231", "232", "330", "332", "430", "441"], [23, 33, 43, 44]),
    ];
    private static readonly (string[] Keys, int[] Cells)[] WallTableL20P1 =
    [
        (["123", "132", "133", "212", "213"], [11, 12, 13, 21]),
        (["140", "220", "221", "223", "231", "233", "243"], [14, 22, 23, 24]),
        (["311", "321", "323", "331", "332", "333", "412"], [31, 32, 33, 41]),
        (["340", "341", "420", "421", "431"], [34, 42, 43, 44]),
    ];
    private static readonly (string[] Keys, int[] Cells)[] WallTableL200P0 =
    [
        (["112", "213", "221", "223", "231", "232", "233"], [11, 21, 22, 23]),
        (["120", "123", "133", "240", "243"], [12, 13, 14, 24]),
        (["311", "312", "421", "431", "432"], [31, 41, 42, 43]),
        (["320", "321", "323", "331", "333", "341", "440"], [32, 33, 34, 44]),
    ];
    private static readonly (string[] Keys, int[] Cells)[] WallTableL200P1 =
    [
        (["122", "123", "212", "312", "313"], [11, 12, 21, 31]),
        (["130", "143", "230", "232", "330", "332", "333"], [13, 23, 33, 14]),
        (["220", "221", "222", "320", "322", "411", "422"], [22, 32, 41, 42]),
        (["240", "241", "340", "430", "431"], [24, 34, 43, 44]),
    ];
}

sealed class RubyReflection(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect Cell = new(5f, 5f, 5f);
    private const float EdgeGemMinOffset = 2.9f;
    // The trigger XML draws the danger region for t:15; keep the same lifetime so old zones
    // disappear once the mechanic resolves instead of lingering on the floor.
    private const double DisplayTimeout = 15d;
    private readonly List<AOEInstance> _displayed = [with(8)];
    private sealed class GemBatch(DateTime created)
    {
        public readonly DateTime Created = created;
        public readonly HashSet<string> Tokens = [];
    }

    private readonly List<GemBatch> _gemBatches = [];
    private GemBatch? _currentBatch;
    private readonly HashSet<string> _edgeGems = [];
    private readonly List<(int[] Cells, DateTime Activation)> _zones = [];
    private int _wallKind = -1;
    private Angle _wallRot;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _displayed.Clear();

        foreach (var zone in _zones)
        {
            foreach (var cell in zone.Cells)
            {
                var col = cell / 10;
                var row = cell % 10;
                // Triggernometry's PictoACT position is in the same world X/Z basis as WPos.
                // Pos=(col-2.5)*10,(row-2)*10 is the lower edge of a 10y cell; AOEShapeRect is
                // centered, so use the cell center (row-2.5) here and do not rotate the grid.
                var center = Arena.Center + new WDir((col - 2.5f) * 10f, (row - 2.5f) * 10f);
                _displayed.Add(new(Cell, center, activation: zone.Activation, color: Colors.Danger, risky: true,
                    shapeDistance: Cell.Distance(center, default)));
            }
        }
        return CollectionsMarshal.AsSpan(_displayed);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (ref readonly var aoe in ActiveAOEs(slot, actor))
            hints.AddForbiddenZone(aoe.ShapeDistance ?? aoe.Shape.Distance(aoe.Origin, aoe.Rotation), aoe.Activation);
    }

    public override void Update()
    {
        var now = WorldState.CurrentTime;
        _zones.RemoveAll(z => now > z.Activation.AddSeconds(DisplayTimeout));
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID is (uint)OID.RubyWallTian or (uint)OID.RubyWallL)
            Service.Logger.Information($"[CE206] wall EAnim oid={actor.OID:X} p1={(state >> 16) & 0xFFFF:X4} p2={state & 0xFFFF:X4} rot={actor.Rotation.Rad:f3}");
        var anim = state & 0xFFF;
        if (actor.OID == (uint)OID.RubyWallTian && anim == 2)
            SetWall(0, actor.Rotation);
        // The ACT log line prints these anim values in hex: "2" == 0x02 (appear), "20" == 0x20 (L
        // moving), "200" == 0x200 (¬ moving). Decimal 20/200 are accepted too as a belt-and-braces
        // fallback in case a client reports them as raw decimal.
        else if (actor.OID == (uint)OID.RubyWallL && (anim is 0x20 or 0x200 or 20 or 200))
            SetWall(anim is 0x20 or 20 ? 1 : 3, actor.Rotation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.YellowGemstones)
        {
            // Keep a snapshot per BC98. A second BC98 may arrive while the wall from the
            // previous snapshot is still resolving; overwriting one shared set loses that
            // wall's real gemstones.
            _currentBatch = new(WorldState.CurrentTime);
            _gemBatches.Add(_currentBatch);
            if (_gemBatches.Count > 8)
                _gemBatches.RemoveAt(0);
            _edgeGems.Clear();
            _zones.Clear();
            Service.Logger.Information($"[CE206] BC98 batch created idx={_gemBatches.Count - 1}");
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID != (uint)OID.YellowGem || id != 0x2489)
            return;

        var pos = actor.Position;
        // Triggernometry rounds half-values away from zero for this positive grid coordinate.
        var col = (int)MathF.Floor((pos.X + 25f - Arena.Center.X) / 10f + 0.5f);
        var row = (int)MathF.Floor((pos.Z + 25f - Arena.Center.Z) / 10f + 0.5f);
        if (col is < 1 or > 4 || row is < 1 or > 4)
            return;

        var dx = pos.X - Arena.Center.X - (col - 2.5f) * 10f;
        var dz = pos.Z - Arena.Center.Z - (row - 2.5f) * 10f;
        if (MathF.Max(MathF.Abs(dx), MathF.Abs(dz)) >= EdgeGemMinOffset)
        {
            // VecToDir(dx, dz, 4) uses atan2(x,z), matching Angle.FromDirection/WPos heading.
            // Using atan2(z,x)+2 (the old code) rotates every edge token and selects the wrong
            // region or no region at all.
            var dir = RubyReflectionData.VecToDir(dx, dz);
            var token = $"{col}{row}{dir}";
            _edgeGems.Add(token);
            _currentBatch?.Tokens.Add(token);
            Service.Logger.Information($"[CE206] gem PAT id={actor.InstanceID:X} pos=({pos.X:f2},{pos.Z:f2}) cell={col}{row} d=({dx:f2},{dz:f2}) dir={dir} token={col}{row}{dir}");
        }
    }

    private void SetWall(int kind, Angle rotation)
    {
        _wallKind = kind;
        _wallRot = rotation;
        // Use the most recent gem batch created within the last 15s that actually collected PAT
        // tokens (gems may be repositioned in several PAT waves before the wall appears; the last
        // wave is the one the wall resolves). Fall back to the shared edge set if no batch has
        // tokens yet (e.g. the wall EAnim arrived before the gem PAT events).
        var now = WorldState.CurrentTime;
        var batchIndex = _gemBatches.Count - 1;
        while (batchIndex >= 0 && (now - _gemBatches[batchIndex].Created).TotalSeconds > DisplayTimeout)
            --batchIndex;
        while (batchIndex >= 0 && _gemBatches[batchIndex].Tokens.Count == 0)
            --batchIndex;
        var tokens = (batchIndex >= 0 ? _gemBatches[batchIndex].Tokens : _edgeGems).ToArray();
        _edgeGems.Clear();
        _edgeGems.UnionWith(tokens);
        var cells = Recalculate();
        Service.Logger.Information($"[CE206] wall set kind={kind} rot={rotation.Rad:f3} tokens=[{string.Join(",", _edgeGems)}] cells=[{string.Join(",", cells)}]");
        if (cells.Length != 0)
        {
            _zones.Add((cells, WorldState.CurrentTime));
            ++NumCasts;
        }
    }

    private int[] Recalculate()
    {
        if (_wallKind < 0)
            return [];

        // Triggernometry RadToDir(h,4) % 2 (h follows the game heading convention: 0 = south,
        // CCW, so east = +pi/2 - identical to BossMod's Angle.Rad).
        var parity = RubyReflectionData.RadToDir(_wallRot.Rad, 4) % 2;
        var tableName = _wallKind switch
        {
            0 => "tian",
            1 => parity == 0 ? "l20p0" : "l20p1",
            3 => parity == 0 ? "l200p0" : "l200p1",
            _ => ""
        };
        return RubyReflectionData.SelectCells(tableName, _edgeGems);
    }
}

static class KnockbackGeometry
{
    // C163 row 90 is SourceRight and row 91 is SourceLeft. The server chooses the row from
    // the target's side of the helper's facing axis, so this must be evaluated at each candidate.
    public static WDir AsideDirection(WPos point, WPos origin, WDir facing)
    {
        var right = facing.OrthoR();
        return (point - origin).Dot(right) >= 0f ? right : -right;
    }
}

// Local attract-style square projection. The CN branch carries this helper in the shared
// shape-distance library, while upstream does not have that utility yet; keeping the small
// implementation local avoids widening this PR beyond the North Island CE module.
sealed class SDKnockbackInAABBSquareTowardsOrigin(WPos center, WPos origin, float distance, float halfWidth) : ShapeDistance
{
    public override bool Contains(in WPos p)
    {
        var toOrigin = origin - p;
        var normalized = toOrigin.Normalized();
        return normalized == default || !(p + distance * normalized).InSquare(center, halfWidth);
    }

    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

sealed class SDAsideKnockbackInAABBSquare(WPos center, WPos origin, WDir facing, float distance, float halfWidth) : ShapeDistance
{
    public override bool Contains(in WPos p)
        => !(p + distance * KnockbackGeometry.AsideDirection(p, origin, facing)).InSquare(center, halfWidth);

    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

sealed class SDAsideThenCenterPullInAABBSquare(WPos center, WPos asideOrigin, WDir asideFacing, float asideDistance, float pullDistance, float halfWidth) : ShapeDistance
{
    public override bool Contains(in WPos p)
    {
        var p1 = p + asideDistance * KnockbackGeometry.AsideDirection(p, asideOrigin, asideFacing);
        if (!p1.InSquare(center, halfWidth))
            return true;

        // The second hit pulls the player back toward the arena center (ARR player displacement:
        // C163 lands ~15y aside, C162 then carries the survivor ~20y back toward center, e.g.
        // x_enc -25502 -> -24911 on both recorded rounds).
        var toward = center - p1;
        var p2 = toward == default ? p1 : p1 + pullDistance * toward.Normalized();
        return !p2.InSquare(center, halfWidth);
    }

    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}

// BCA0 resolves about six seconds after the telegraph as a 20y pull toward the arena center
// (replay: after the 15y aside lands at x ~= 223.4, both recorded survivors move to x ~= 243.4).
// The earlier AwayFromOrigin 30y interpretation was wrong - it pushed players into the fence.
sealed class CircularKnockback(BossModule module) : Components.GenericKnockback(module)
{
    private static readonly AOEShapeCircle Shape = new(60f);
    internal const float Distance = 20f;
    private const float SafeHalfWidth = 19f;
    // The second knockback's safe landing is hardcoded to where the first shove direction meets
    // the electric fence (arena edge): push the small zone out to ~14y from center (radius 5
    // covers 9..19, just inside the 19y boundary/电网).
    internal const float SecondSafeOffset = 14f;
    internal const float SecondSafeRadius = 5f;
    private const double HitDelay = 6.0d;
    private readonly List<Knockback> _casters = [];
    private readonly List<Knockback> _displayed = [with(4)];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        PruneExpired();
        _displayed.Clear();
        foreach (var kb in _casters)
            _displayed.Add(kb);
        return CollectionsMarshal.AsSpan(_displayed);
    }

    public override void Update() => PruneExpired();

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        // Second knockback: safe landing follows the second knockback's own direction (the spot the
        // boss jumped to), not a fixed direction. Only show it when the second hit is imminent.
        var aside = Module.FindComponent<KnockAside>();
        if (aside?.SecondDir is not { } t)
            return;
        var imminent = false;
        foreach (var kb in _casters)
            if ((kb.Activation - WorldState.CurrentTime).TotalSeconds <= 3d)
            {
                imminent = true;
                break;
            }
        if (!imminent)
            return;
        // 第二段安全区画成矩形框 (好看), 位置卡在第二段击退方向 (发出点) 的电网内侧, 尺寸按击退
        // 距离/角度留出安全余量, 保证被第二段击退后落点不会撞进电网.
        var center = Arena.Center + t * SecondSafeOffset;
        Arena.AddRect(center, t.OrthoL(), 5f, 5f, 3f, Colors.Safe, 2f);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var aside = Module.FindComponent<KnockAside>();
        foreach (var kb in _casters)
        {
            // Before C163, solve both hits from the candidate start: the first displacement can
            // differ by side even for two players in the same packet. After C163, only C162 remains.
            if (aside == null || !aside.AddCombinedAIHint(kb, hints))
                hints.AddForbiddenZone(new SDKnockbackInAABBSquareTowardsOrigin(Arena.Center, kb.Origin, kb.Distance, SafeHalfWidth), kb.Activation);
        }

        // 第二段安全区引导：第二段 imminent 时引导 AI 到矩形框（第二段击退方向电网内侧）。
        if (aside?.SecondDir is { } t && _casters.Any(kb => (kb.Activation - WorldState.CurrentTime).TotalSeconds <= 3d))
        {
            var center = Arena.Center + t * SecondSafeOffset;
            hints.GoalZones.Add(position => position.InRect(center, t.OrthoL(), 5f, 5f, 3f) ? 50f : 0f);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // The same action can arrive with extra high bytes (e.g. 0x4BCA0); only the low 16 bits
        // are the real action id.
        if ((spell.Action.ID & 0xFFFF) == (uint)AID.CircularKnockbackTelegraph)
        {
            _casters.RemoveAll(k => k.ActorID == caster.InstanceID);
            _casters.Add(new(Arena.Center, Distance, Module.CastFinishAt(spell).AddSeconds(HitDelay), Shape, spell.Rotation, Kind.TowardsOrigin, actorID: caster.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((spell.Action.ID & 0xFFFF) == (uint)AID.RavenousGodsCircleHit)
        {
            _casters.Clear();
            ++NumCasts;
        }
    }

    private void PruneExpired()
    {
        var now = WorldState.CurrentTime;
        _casters.RemoveAll(k => now > k.Activation.AddSeconds(1d));
    }
}

// BCA1 resolves first as a 15y lateral shove. C163 chooses SourceRight or SourceLeft separately
// for every target from that target's side of the helper's facing axis.
sealed class KnockAside(BossModule module) : Components.GenericKnockback(module)
{
    private static readonly AOEShapeRect Shape = new(40f, 30f);
    private const float Distance = 15f;
    private const float SafeHalfWidth = 19f;
    // 第一段击退安全起始半径: 距中心超过这个值的玩家被 15y 侧向击退后落点会出界进电网.
    internal static readonly float SafeStartRadius = MathF.Sqrt(SafeHalfWidth * SafeHalfWidth - Distance * Distance);
    private static readonly AOEShapeCone SafeHalfCircle = new(SafeStartRadius, 90f.Degrees());
    private const double HitDelay = 5.1d;

    private sealed class AsideSource(WPos asidePos, WPos circlePos, Angle facing, DateTime activation, ulong actorID)
    {
        public readonly WPos AsidePos = asidePos;
        public readonly WPos CirclePos = circlePos;
        public readonly Angle Facing = facing;
        public readonly DateTime Activation = activation;
        public readonly ulong ActorID = actorID;

        public Kind KindFor(WPos point) => (point - AsidePos).Dot(Facing.ToDirection().OrthoR()) >= 0f ? Kind.DirRight : Kind.DirLeft;
    }

    private readonly List<AsideSource> _sources = [];
    private readonly List<(WPos AsidePos, Angle Facing, DateTime Activation, ulong ActorID)> _pendingAside = [];
    private readonly List<Knockback> _displayed = [with(4)];
    private WDir? _lastAsideDir;
    private DateTime _lastAsideSeen;
    private WDir? _lastSecondDir;
    private DateTime _lastSecondSeen;

    // The lateral shove direction of the current aside. It survives the C163 resolution (sources
    // are cleared) so the second-stage safe zone keeps being drawn on that half-field.
    public WDir? CurrentAsideDir => WorldState.CurrentTime <= _lastAsideSeen.AddSeconds(15d) ? _lastAsideDir : null;

    public bool HasActiveAside => _sources.Count != 0;

    // 第二段击退发出点方向: boss 跳过去的位置 (BCA0 cast 位置), 第一段半场击退朝向它.
    public WDir? SecondDir => WorldState.CurrentTime <= _lastSecondSeen.AddSeconds(15d) ? _lastSecondDir : null;

    public bool AddCombinedAIHint(Knockback circle, AIHints hints)
    {
        foreach (var source in _sources)
            if (source.Activation < circle.Activation && source.CirclePos.AlmostEqual(circle.Origin, 0.5f))
            {
                hints.AddForbiddenZone(new SDAsideThenCenterPullInAABBSquare(Arena.Center, source.AsidePos,
                    source.Facing.ToDirection(), Distance, CircularKnockback.Distance, SafeHalfWidth), source.Activation);
                return true;
            }
        return false;
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        PruneExpired();
        _displayed.Clear();
        foreach (var source in _sources)
            _displayed.Add(new(source.AsidePos, Distance, source.Activation, Shape, source.Facing, source.KindFor(actor.Position), actorID: source.ActorID));
        return CollectionsMarshal.AsSpan(_displayed);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        // First knockback (C163, 15y lateral). Safe zone is a half-circle on the shove side, radius
        // sqrt(fence² - shove²)≈11.7y, so the 15y shove (and the second knockback) cannot land in
        // the electric fence. The half-circle points at the second knockback's origin, and only
        // shows during the first knockback (cleared after C163 resolves).
        if (HasActiveAside && SecondDir is { } dir)
        {
            SafeHalfCircle.Outline(Arena, Arena.Center, Angle.FromDirection(dir), Colors.Safe, 2f);
        }
    }

    public override void Update() => PruneExpired();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var source in _sources)
            hints.AddForbiddenZone(new SDAsideKnockbackInAABBSquare(Arena.Center, source.AsidePos, source.Facing.ToDirection(), Distance, SafeHalfWidth), source.Activation);

        // 第一段正确站位: 两段击退间隔极短 (~0.9s), AI 落地后没时间移动。引导到
        // 安全半圆内 (固定 SecondDir 半场), 站半圆内任意点被 15y 侧击后落点都在 19y 电网内。
        if (HasActiveAside && SecondDir is { } t)
        {
            // 目标区偏向安全半场深处 (SecondDir 方向 ~60% 半径处), 半径收窄, AI 停在半场内
            // 且不贴着场地中线。
            var goal = Arena.Center + t * (SafeStartRadius * 0.6f);
            hints.GoalZones.Add(AIHints.GoalSingleTarget(goal, SafeStartRadius * 0.35f, 50f));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID & 0xFFFF)
        {
            case (uint)AID.KnockAsideTelegraph:
                _pendingAside.RemoveAll(p => p.ActorID == caster.InstanceID);
                _pendingAside.Add((caster.Position, spell.Rotation, Module.CastFinishAt(spell).AddSeconds(HitDelay), caster.InstanceID));
                break;
            case (uint)AID.CircularKnockbackTelegraph:
                // The circle helper arrives a couple of seconds after the aside telegraph; pair it
                // with the pending aside so AI can evaluate both landings from one candidate point.
                for (var i = _pendingAside.Count - 1; i >= 0; --i)
                {
                    var p = _pendingAside[i];
                    _sources.RemoveAll(s => s.ActorID == p.ActorID);
                    _sources.Add(new(p.AsidePos, spell.LocXZ, p.Facing, p.Activation, p.ActorID));
                    _lastAsideDir = p.Facing.ToDirection().OrthoR();
                    _lastAsideSeen = WorldState.CurrentTime;
                    var toSecond = spell.LocXZ - Arena.Center;
                    _lastSecondDir = toSecond.LengthSq() > 1f ? toSecond.Normalized() : null;
                    _lastSecondSeen = WorldState.CurrentTime;
                    _pendingAside.RemoveAt(i);
                }
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((spell.Action.ID & 0xFFFF) == (uint)AID.RavenousGodsAsideHit)
        {
            _sources.Clear();
            _pendingAside.Clear();
            ++NumCasts;
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        _sources.RemoveAll(s => s.ActorID == actor.InstanceID);
        _pendingAside.RemoveAll(p => p.ActorID == actor.InstanceID);
    }

    private void PruneExpired()
    {
        var now = WorldState.CurrentTime;
        _sources.RemoveAll(s => now > s.Activation.AddSeconds(1d));
        _pendingAside.RemoveAll(p => now > p.Activation.AddSeconds(1d));
    }
}
sealed class GemstoneRaidwides(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.RubyLight, (uint)AID.RavenousGods, (uint)AID.Howl]);
sealed class RubyReflectionHint(BossModule module) : Components.CastHint(module, (uint)AID.RubyLight, "Ruby reflection - watch the gemstone lines");

sealed class RebelliousFamiliarStates : StateMachineBuilder
{
    public RebelliousFamiliarStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ClawTailCombo>()
            .ActivateOnEnter<TopazRay>()
            .ActivateOnEnter<LethalBoundary>()
            .ActivateOnEnter<RubyReflection>()
            .ActivateOnEnter<CircularKnockback>()
            .ActivateOnEnter<KnockAside>()
            .ActivateOnEnter<GemstoneRaidwides>()
            .ActivateOnEnter<RubyReflectionHint>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(RebelliousFamiliarStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    PrimaryActorOID = (uint)OID.Boss,
    Contributors = "KanoNoUta",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CriticalEngagement,
    GroupID = 1093u,
    NameID = 56u,
    SortOrder = 5)]
// The arena is a 20y square: the floor is a 4x4 grid of 10y cells (North-Horn trigger XML R=20).
public sealed class RebelliousFamiliar(WorldState ws, Actor primary) : BossModule(ws, primary, new(238f, 352f), new ArenaBoundsSquare(20f))
{
    // 双段击退期间 AI 必须待在安全区而不是追 boss: 第二段安全区硬编码在第一次击退方向贴近电网处.
    // Without this, automation walks after the boss and gets shoved into the fence.
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var aside = FindComponent<KnockAside>();
        if (aside?.SecondDir is not { } t)
            return;
        // 第一段击退前: AI 必须站在安全起始半径内 (击退后落点才不会进电网), 而不是在 boss 目标圈
        // 里打. 半圆/引导都朝向第二段击退发出点, 第一段结束后切到第二段安全区.
        if (aside.HasActiveAside)
            hints.GoalZones.Add(AIHints.GoalSingleTarget(Arena.Center + t * (KnockAside.SafeStartRadius * 0.5f), KnockAside.SafeStartRadius * 0.5f));
        else
            hints.GoalZones.Add(AIHints.GoalSingleTarget(Arena.Center + t * CircularKnockback.SecondSafeOffset, CircularKnockback.SecondSafeRadius));
    }
}
