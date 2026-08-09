using BossMod.Dawntrail.Foray.CriticalEngagement;
using System.Linq;

namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE214ForbiddenFolios;

public enum OID : uint
{
    Boss = 0x4BD3, // R6.0, BNpcName 14520, forbidden folios
    Pages64 = 0x4BD4, // R1.0, 64 pages - announces level-5 death sector
    Pages16 = 0x4BD5, // R1.0, 16 pages - announces level-3 flare sector
    Pages8 = 0x4BD6, // R1.0, 8 pages - announces level-4 holy sector
    Pages512 = 0x4BD7, // R1.0, 512 pages - announces prime-death sector
    BookTrap = 0x4BD8, // R1.0, book-drop trap
    Helper = 0x233C
}

public enum AID : uint
{
    KnowledgeLevelCorrectionVisual = 0xB8C0, // boss->self, 5.0s cast, applies correction statuses
    KnowledgeLevelCorrection = 0xB8C1, // helper->players, no cast, correction application
    BlotVisual = 0xB8C4, // boss->self, 3.0s cast, visual
    Blot = 0xB8C5, // helper->location, 8.0s cast, range 15 circle
    CoverToCoverFirst = 0xB8C6, // boss->self, 4.0s cast, range 30 180-degree cone
    CoverToCoverSecond = 0xB8C7, // boss->self, 1.0s cast, range 30 180-degree cone
    ArcaneRule = 0xB8C8, // boss->self, 6.0s cast, visual
    QuadRule = 0xB8C9, // boss->self, grid-rule visual
    HorizontalRule = 0xB8CA, // helper->location, range 50 width 12 rect
    SummonPages = 0xB8CB, // helper->location, page summon visual

    KnowledgeLevel4HolyWide = 0xB8CE, // helper->self, range 25 180-degree cone
    KnowledgeLevel5Death = 0xB8CF, // helper->self, range 25 120-degree cone
    KnowledgeLevel3Flare = 0xB8D0, // helper->self, range 25 120-degree cone
    KnowledgeLevel4Holy = 0xB8D1, // helper->self, range 25 120-degree cone
    PrimeKnowledgeLevelDeath = 0xB8D2, // helper->self, range 25 120-degree cone
    PageLevel5Visual = 0xB8D3, // page->self, visual
    PageLevel3Visual = 0xB8D4, // page->self, visual
    PageLevel4Visual = 0xB8D5, // page->self, visual
    PagePrimeVisual = 0xB8D6, // page->self, visual
    BookDropVisual = 0xB8D7, // boss->self, visual
    BookDrop = 0xB8DA, // book trap->self, 8.0s cast, range 3 circle
    ThunderII = 0xB8DC, // helper->self, 4.0s cast, range 50 width 5 rect
    FireII = 0xB8DD, // helper->self, 5.0s cast, range 60 45-degree cone
    FireIIVisual = 0xB8DE, // boss->self, visual
    MarginaliaHit = 0xB8DF, // helper->players, duplicate raidwide damage
    Marginalia = 0xB8E0, // boss->self, 5.0s cast, raidwide visual

    UnknownBC76 = 0xBC76, // observed boss event/cleanup
    SummonVisual = 0xBF9F, // boss->self, summon visual
    AutoAttack = 0xBFA0, // boss->player, no cast, single-target
    UnboundInk = 0xC154, // boss->self, 4.0s cast, range 9 circle
    PrimeKnowledgeLevelDeathWide = 0xC2D7, // helper->self, range 25 180-degree cone

    KnowledgeLevel4HolyWideAlt = 0xC57C, // helper->self, duplicate of B8CE
    KnowledgeLevel5DeathAlt = 0xC57D, // helper->self, duplicate of B8CF
    KnowledgeLevel3FlareAlt = 0xC57E, // helper->self, duplicate of B8D0
    KnowledgeLevel4HolyAlt = 0xC57F, // helper->self, duplicate of B8D1
    PrimeKnowledgeLevelDeathAlt = 0xC580, // helper->self, duplicate of B8D2
    PrimeKnowledgeLevelDeathWideAlt = 0xC581 // helper->self, duplicate of C2D7
}

public enum SID : uint
{
    Correction1 = 0x1396, // knowledge level +1
    Correction2 = 0x1397, // knowledge level +2
    Correction3 = 0x1398, // knowledge level +3
    Correction4 = 0x1399, // knowledge level +4
    Correction5 = 0x139A // knowledge level +5
}

// These location/self casts expose authoritative warning packets, including the initial cross
// writing and the four-yalm page landing circles.
sealed class BasicAOEs(BossModule module) : ReplayValidatedCastAOEs(module)
{
    private static readonly AOEShapeCircle Blot = new(15f);
    private static readonly AOEShapeCircle SummonPages = new(4f);
    private static readonly AOEShapeCross QuadRule = new(25f, 5f);
    private static readonly AOEShapeCone FireII = new(60f, 22.5f.Degrees());

    // Blot exposes three rows of three circles at roughly two-second intervals. The opener is
    // "third into first": both of the first two rows must be forbidden so the third row is the
    // only pre-position, then the first row becomes available after it resolves. Replay cast-start
    // spacing reaches 2.026s, so a literal 2.0s cutoff incorrectly made the second row look safe.
    protected override double RiskyActivationWindow => 2.25d;

    protected override AOEConfig? ConfigFor(uint actionID) => actionID switch
    {
        (uint)AID.Blot => new(Blot, true),
        (uint)AID.QuadRule => new(QuadRule, true),
        (uint)AID.SummonPages => new(SummonPages),
        (uint)AID.FireII => new(FireII),
        _ => null
    };
}

// Cover to Cover sweeps one half first, then the opposite half roughly four seconds later. The
// second sweep's own cast is only 0.7s, which automation cannot react to, so publish the second
// sweep's danger zone from the moment the first sweep resolves (replay: 16 victims in the second
// sweep because it appeared too late).
sealed class CoverToCoverSequence(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone Shape = new(30f, 90f.Degrees());
    private const double SecondResolveDelay = 4.2d;
    private readonly List<AOEInstance> _displayed = [with(2)];
    private AOEInstance? _first;
    private AOEInstance? _second;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        PruneExpired();
        _displayed.Clear();
        if (_first is { } first)
            _displayed.Add(first);
        if (_second is { } second)
            _displayed.Add(second);
        return CollectionsMarshal.AsSpan(_displayed);
    }

    public override void Update() => PruneExpired();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.EventHappened)
            return;

        switch (spell.Action.ID)
        {
            case (uint)AID.CoverToCoverFirst:
                _first = new(Shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell), Colors.Danger, true, caster.InstanceID, Shape.Distance(caster.Position, spell.Rotation));
                break;
            case (uint)AID.CoverToCoverSecond:
                _second = new(Shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell), Colors.Danger, true, caster.InstanceID, Shape.Distance(caster.Position, spell.Rotation));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var now = WorldState.CurrentTime;
        switch (spell.Action.ID)
        {
            case (uint)AID.CoverToCoverFirst:
                _first = null;
                // The first half is now swept; warn about the opposite half until the second sweep lands.
                var predictedRotation = spell.Rotation + 180f.Degrees();
                _second = new(Shape, caster.Position, predictedRotation, now.AddSeconds(SecondResolveDelay), Colors.Danger, true, caster.InstanceID, Shape.Distance(caster.Position, predictedRotation));
                break;
            case (uint)AID.CoverToCoverSecond:
                _second = null;
                break;
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (_first is { ActorID: var firstID } && firstID == actor.InstanceID)
            _first = null;
        if (_second is { ActorID: var secondID } && secondID == actor.InstanceID)
            _second = null;
    }

    private void PruneExpired()
    {
        var now = WorldState.CurrentTime;
        if (_first is { Activation: var firstAct } && now > firstAct.AddSeconds(1d))
            _first = null;
        if (_second is { Activation: var secondAct } && now > secondAct.AddSeconds(1d))
            _second = null;
    }
}

// Thunder II arrives in two batches, two seconds apart. Draw both batches for planning while only
// making the earliest simultaneous set risky; otherwise automation sees the complete square as
// forbidden and oscillates. Each helper's origin and rotation are the actual lane geometry.
sealed class ThunderII(BossModule module) : Components.GenericAOEs(module)
{
    // 井字整体: helper 的 ThunderII cast 事件偶发缺失会导致部分条不画。
    // 从存活 helper 实体实时画 (施法中)。helper 站在场地边缘的 50x50 方形边框上，
    // 直条必须对称 50+50 (100y) 才能从任意一侧贯通全场，横竖交叉成完整井字网格；
    // 60y (30+30) 只能覆盖到场地中部，会导致网格缺 1/4、条子看起来是分开的。
    private static readonly AOEShapeRect Shape = new(50f, 2.5f, 50f);
    private readonly List<AOEInstance> _displayed = [with(16)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _displayed.Clear();
        var now = WorldState.CurrentTime;
        // 两批直条间隔 2s、互相交错 5y；若同时画，条形会铺满全场变成一整片颜色。
        // 先找最早结算的那批，重叠期间只画它，等它消失后再显示下一批。
        var earliest = default(DateTime);
        foreach (var helper in Module.Enemies((uint)OID.Helper))
        {
            if (helper.IsDeadOrDestroyed || helper.CastInfo is not { } cast || (cast.Action.ID & 0xFFFF) != (uint)AID.ThunderII)
                continue;
            var activation = Module.CastFinishAt(cast);
            if (earliest == default || activation < earliest)
                earliest = activation;
        }
        if (earliest == default)
            return CollectionsMarshal.AsSpan(_displayed);

        foreach (var helper in Module.Enemies((uint)OID.Helper))
        {
            if (helper.IsDeadOrDestroyed || helper.CastInfo is not { } cast || (cast.Action.ID & 0xFFFF) != (uint)AID.ThunderII)
                continue;
            var activation = Module.CastFinishAt(cast);
            if (activation > earliest.AddSeconds(0.5d))
                continue;
            var origin = helper.Position;
            var imminent = activation <= now.AddSeconds(1d);
            _displayed.Add(new(Shape, origin, cast.Rotation, activation,
                imminent ? Colors.Danger : Colors.AOE, imminent, helper.InstanceID,
                Shape.Distance(origin, cast.Rotation)));
        }
        return CollectionsMarshal.AsSpan(_displayed);
    }
}

sealed class HorizontalRule(BossModule module) : Components.GenericAOEs(module)
{
    // The reference trigger uses refY=50/offY=-50/radius=3: every rule is a 6y-wide line
    // extending 50y in both directions from the boss center, so it always crosses the full arena.
    private static readonly AOEShapeRect Shape = new(50f, 3f, 50f);
    private const double EventResolveTolerance = 0.5d;
    private const double ExpireDelay = 2d;
    private readonly List<AOEInstance> _pending = [with(16)];
    private readonly List<AOEInstance> _displayed = [with(16)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        PruneExpired();
        _displayed.Clear();
        if (_pending.Count == 0)
            return CollectionsMarshal.AsSpan(_displayed);

        var deadline = _pending[0].Activation.AddSeconds(0.25d);
        foreach (var source in _pending)
        {
            var aoe = source;
            var imminent = aoe.Activation <= deadline;
            aoe.Color = imminent ? Colors.Danger : Colors.AOE;
            aoe.Risky = imminent;
            _displayed.Add(aoe);
        }
        return CollectionsMarshal.AsSpan(_displayed);
    }

    public override void Update() => PruneExpired();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.HorizontalRule || spell.EventHappened)
            return;

        PruneExpired();
        var activation = Module.CastFinishAt(spell);
        if (activation <= WorldState.CurrentTime)
            return;

        _pending.RemoveAll(aoe => aoe.ActorID == caster.InstanceID);
        // The location field is occasionally omitted on one of the four concurrent helper
        // packets. The cast rotation is always present and is the authoritative cardinal lane
        // orientation; the shape is symmetric front/back, so its facing sign is immaterial.
        var rotation = spell.Rotation;
        var origin = caster.Position;
        _pending.Add(new(Shape, origin, rotation, activation, actorID: caster.InstanceID, shapeDistance: Shape.Distance(origin, rotation)));
        _pending.Sort((left, right) => left.Activation.CompareTo(right.Activation));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HorizontalRule && (spell.EventHappened || Module.CastFinishAt(spell) <= WorldState.CurrentTime.AddSeconds(EventResolveTolerance)))
            _pending.RemoveAll(aoe => aoe.ActorID == caster.InstanceID);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.HorizontalRule)
            return;

        _pending.RemoveAll(aoe => aoe.ActorID == caster.InstanceID);
        ++NumCasts;
    }

    public override void OnActorDeath(Actor actor) => _pending.RemoveAll(aoe => aoe.ActorID == actor.InstanceID);
    public override void OnActorDestroyed(Actor actor) => _pending.RemoveAll(aoe => aoe.ActorID == actor.InstanceID);

    private void PruneExpired()
    {
        var now = WorldState.CurrentTime;
        _pending.RemoveAll(aoe => now > aoe.Activation.AddSeconds(ExpireDelay));
    }
}

// The three (sometimes two) page actors each announce a sector type via NPC yell, then a helper
// casts the corresponding cone. The cone originates at the page's own position (12.5y from center)
// and faces the arena center; replay victims all sit inside a 25y cone from the page toward the
// center (half-angle 60 for 120-degree sectors, 90 for the 180-degree wide variants). A player's
// final knowledge level is the absolute account-wide progress (ForayInfo.Level, 20-40) plus the
// per-round correction status; a sector is dangerous only when that final level satisfies the
// sector's rule (final % N == 0, or prime for the prime sectors). This must remain a per-player
// ActiveAOEs calculation; globally painting every sector red is mechanically wrong.
sealed class KnowledgeSectors(BossModule module) : Components.GenericAOEs(module)
{
    private enum SectorKind { Level3, Level4, Level4Wide, Level5, Prime, PrimeWide }
    private readonly record struct SectorConfig(SectorKind Kind, AOEShape Shape, OID PageOID);

    private sealed class PendingSector(SectorKind kind, AOEShape shape, Angle rotation, DateTime activation, ulong casterID)
    {
        public readonly SectorKind Kind = kind;
        public readonly AOEShape Shape = shape;
        public readonly Angle Rotation = rotation;
        public readonly DateTime Activation = activation;
        public readonly HashSet<ulong> Casters = [casterID];
    }

    private static readonly AOEShapeCone Sector120 = new(25f, 60f.Degrees());
    private static readonly AOEShapeCone Sector180 = new(25f, 90f.Degrees());
    private readonly List<PendingSector> _pending = [with(6)];
    private readonly List<AOEInstance> _displayed = [with(6)];
    private readonly HashSet<uint> _seenGlobalSequences = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        PruneExpired();
        _displayed.Clear();
        var correction = Correction(actor);
        // If the knowledge level or the correction status is unavailable (e.g. ForayInfo memory
        // read failed and Level stayed 0), we cannot tell which sectors are safe for this player.
        // Never return empty - fall back to painting every sector dangerous so the player still
        // gets warned.
        var unknown = actor.ForayInfo.Level <= 0 || correction == 0;
        var level = actor.ForayInfo.Level + correction;

        foreach (var sector in _pending)
        {
            // The helper's rotation is the authoritative sector direction. The damage cone
            // originates at the arena center; page actors only announce the rule for that sector.
            var direction = sector.Rotation;
            if (!unknown && SatisfiesRule(level, sector.Kind))
            {
                // This sector is safe for this player: outline it green so the eye can see where to
                // stand, without making it risky for automation.
                _displayed.Add(new(sector.Shape, Module.Arena.Center, direction, sector.Activation,
                    Colors.Safe, false, sector.Casters.FirstOrDefault(), sector.Shape.Distance(Module.Arena.Center, direction)));
                continue;
            }

            _displayed.Add(new(sector.Shape, Module.Arena.Center, direction, sector.Activation,
                actorID: sector.Casters.FirstOrDefault(), shapeDistance: sector.Shape.Distance(Module.Arena.Center, direction)));
        }
        return CollectionsMarshal.AsSpan(_displayed);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_pending.Count == 0)
            return;

        var correction = Correction(actor);
        var unknown = actor.ForayInfo.Level <= 0 || correction == 0;
        if (unknown)
        {
            hints.Add("Knowledge level unavailable - all sectors marked dangerous", true);
            return;
        }

        var level = actor.ForayInfo.Level + correction;
        hints.Add($"Knowledge level {level} (base {actor.ForayInfo.Level} + {correction})");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        // The dangerous sectors are already forbidden by the base implementation; without a
        // positive goal the AI just avoids them and never commits to a safe sector. Guide it into
        // every sector that satisfies this player's rule, scoring deeper positions higher.
        var correction = Correction(actor);
        if (actor.ForayInfo.Level <= 0 || correction == 0)
            return;
        var level = actor.ForayInfo.Level + correction;

        foreach (var sector in _pending)
        {
            if (!SatisfiesRule(level, sector.Kind))
                continue;

            var direction = sector.Rotation;
            var shapeDistance = sector.Shape.Distance(Module.Arena.Center, direction);
            hints.GoalZones.Add(position =>
            {
                var distance = shapeDistance.Distance(position);
                return distance <= 0 ? Math.Clamp(-distance, 0f, 10f) : 0f;
            });
        }
    }

    public override void Update() => PruneExpired();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (ConfigFor(spell.Action.ID) is not { } config || spell.EventHappened)
            return;

        PruneExpired();
        var activation = Module.CastFinishAt(spell);
        if (activation <= WorldState.CurrentTime)
            return;

        var existing = _pending.FirstOrDefault(sector => sector.Kind == config.Kind
            && Math.Abs((sector.Activation - activation).TotalSeconds) <= 0.25d
            && sector.Rotation.AlmostEqual(spell.Rotation, Angle.DegToRad));
        if (existing != null)
        {
            existing.Casters.Add(caster.InstanceID);
            return;
        }

        _pending.Add(new(config.Kind, config.Shape, spell.Rotation, activation, caster.InstanceID));
        _pending.Sort((left, right) => left.Activation.CompareTo(right.Activation));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (ConfigFor(spell.Action.ID) != null)
            RemoveCaster(caster.InstanceID);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (ConfigFor(spell.Action.ID) == null || spell.GlobalSequence != 0 && !_seenGlobalSequences.Add(spell.GlobalSequence))
            return;

        RemoveCaster(caster.InstanceID);
        ++NumCasts;
    }

    public override void OnActorDeath(Actor actor) => RemoveCaster(actor.InstanceID);
    public override void OnActorDestroyed(Actor actor) => RemoveCaster(actor.InstanceID);

    private static SectorConfig? ConfigFor(uint actionID) => actionID switch
    {
        (uint)AID.KnowledgeLevel3Flare or (uint)AID.KnowledgeLevel3FlareAlt => new(SectorKind.Level3, Sector120, OID.Pages16),
        (uint)AID.KnowledgeLevel4Holy or (uint)AID.KnowledgeLevel4HolyAlt => new(SectorKind.Level4, Sector120, OID.Pages8),
        (uint)AID.KnowledgeLevel4HolyWide or (uint)AID.KnowledgeLevel4HolyWideAlt => new(SectorKind.Level4Wide, Sector180, OID.Pages8),
        (uint)AID.KnowledgeLevel5Death or (uint)AID.KnowledgeLevel5DeathAlt => new(SectorKind.Level5, Sector120, OID.Pages64),
        (uint)AID.PrimeKnowledgeLevelDeath or (uint)AID.PrimeKnowledgeLevelDeathAlt => new(SectorKind.Prime, Sector120, OID.Pages512),
        (uint)AID.PrimeKnowledgeLevelDeathWide or (uint)AID.PrimeKnowledgeLevelDeathWideAlt => new(SectorKind.PrimeWide, Sector180, OID.Pages512),
        _ => null
    };

    private static int Correction(Actor actor)
    {
        if (actor.FindStatus((uint)SID.Correction1) != null) return 1;
        if (actor.FindStatus((uint)SID.Correction2) != null) return 2;
        if (actor.FindStatus((uint)SID.Correction3) != null) return 3;
        if (actor.FindStatus((uint)SID.Correction4) != null) return 4;
        if (actor.FindStatus((uint)SID.Correction5) != null) return 5;
        return 0;
    }

    // Replay-verified: the sectors are named 知见3级核爆 / 知见4级神圣 / 知见5级即死 / 知见质数即死,
    // and every recorded victim died in a sector whose condition their final absolute knowledge
    // level satisfied. The sector is therefore SAFE only when the condition does NOT hold.
    private static bool SatisfiesRule(int level, SectorKind kind) => kind switch
    {
        SectorKind.Level3 => level % 3 != 0,
        SectorKind.Level4 or SectorKind.Level4Wide => level % 4 != 0,
        SectorKind.Level5 => level % 5 != 0,
        SectorKind.Prime or SectorKind.PrimeWide => !IsPrime(level),
        _ => false
    };

    private static bool IsPrime(int value)
    {
        if (value < 2)
            return false;
        for (var divisor = 2; divisor * divisor <= value; ++divisor)
            if (value % divisor == 0)
                return false;
        return true;
    }

    private void RemoveCaster(ulong casterID)
    {
        for (var i = _pending.Count - 1; i >= 0; --i)
        {
            var sector = _pending[i];
            if (sector.Casters.Remove(casterID) && sector.Casters.Count == 0)
                _pending.RemoveAt(i);
        }
    }

    private void PruneExpired()
    {
        var now = WorldState.CurrentTime;
        _pending.RemoveAll(sector => now > sector.Activation.AddSeconds(2d));
    }
}

// Replay/operator correction: Unbound Ink (泼墨) is a steel-style avoidable 9y circle - victims
// stood inside it and died - not a soak tower. BookDrop (丢书) is the actual tower players must
// stand in (victims cluster inside each 3y book). Draw Unbound Ink as a red circle and BookDrop
// as a tower.
sealed class UnboundInk(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UnboundInk, new AOEShapeCircle(9f));
// 丢书塔: 所有书塔都需要显示，但 AI 只从同一波中固定选一个前往，不能把显示列表
// 直接交给 GenericTowers，否则它会尝试同时补满每一个塔。
sealed class BookDropTower(BossModule module) : Components.GenericTowers(module, (uint)AID.BookDrop)
{
    private const float Radius = 3f;
    private const int MaxTowersPerWave = 1;
    private const int WaveSize = 5;
    private readonly List<(ulong ID, WPos Pos, DateTime Act)> _pending = [with(8)];
    private readonly List<Components.GenericTowers.Tower> _visibleTowers = [with(8)];
    private readonly Random _rng = new();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != WatchedAction)
            return;
        var tower = (caster.InstanceID, spell.LocXZ, Module.CastFinishAt(spell));
        _pending.Add(tower);
        _visibleTowers.Add(new(tower.Item2, Radius, 1, 2, activation: tower.Item3, actorID: tower.Item1));
        if (_pending.Count < WaveSize)
            return;

        // 每波收齐后只固定选择一个 AI 目标，避免每帧重新选塔导致寻路抖动；所有塔仍保留
        // 在 _visibleTowers 中给玩家绘制。
        for (var i = 0; i < MaxTowersPerWave; ++i)
        {
            var index = _rng.Next(_pending.Count);
            var t = _pending[index];
            Towers.Add(new(t.Pos, Radius, 1, 2, activation: t.Act, actorID: t.ID));
            _pending.RemoveAt(index);
        }
        _pending.Clear();
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Towers.RemoveAll(t => t.ActorID == caster.InstanceID);
            _visibleTowers.RemoveAll(t => t.ActorID == caster.InstanceID);
            _pending.RemoveAll(t => t.ID == caster.InstanceID);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var towers = CollectionsMarshal.AsSpan(_visibleTowers);
        for (var i = 0; i < towers.Length; ++i)
        {
            ref var tower = ref towers[i];
            Components.GenericTowers.DrawTower(Arena, ref tower, safe: true);
        }
    }

    // Cast-finished 事件偶发缺失会留下残留塔; activation 过 2s 后强制清除。
    public override void Update()
    {
        var now = WorldState.CurrentTime;
        Towers.RemoveAll(t => now > t.Activation.AddSeconds(2d));
        _visibleTowers.RemoveAll(t => now > t.Activation.AddSeconds(2d));
        _pending.RemoveAll(t => now > t.Act.AddSeconds(2d));
        base.Update();
    }
}

// The three B8DF helpers carry duplicate damage packets; the boss cast is the stable warning.
sealed class Marginalia(BossModule module) : Components.RaidwideCast(module, (uint)AID.Marginalia);

sealed class ForbiddenFoliosStates : StateMachineBuilder
{
    public ForbiddenFoliosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BasicAOEs>()
            .ActivateOnEnter<CoverToCoverSequence>()
            .ActivateOnEnter<ThunderII>()
            .ActivateOnEnter<HorizontalRule>()
            .ActivateOnEnter<KnowledgeSectors>()
            .ActivateOnEnter<UnboundInk>()
            .ActivateOnEnter<BookDropTower>()
            .ActivateOnEnter<Marginalia>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(ForbiddenFoliosStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    PrimaryActorOID = (uint)OID.Boss,
    Contributors = "KanoNoUta",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CriticalEngagement,
    GroupID = 1093u,
    NameID = 52u,
    SortOrder = 13)]
// The encounter floor is circular (R20). Horizontal Rule still uses 100y rectangles, but anchors
// them at the boss center so the arena clips each one to a full diameter instead of an offset chord.
public sealed class ForbiddenFolios(WorldState ws, Actor primary) : BossModule(ws, primary, new(659f, 659f), new ArenaBoundsCircle(20f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Pages8));
        Arena.Actors(Enemies((uint)OID.Pages16));
        Arena.Actors(Enemies((uint)OID.Pages64));
        Arena.Actors(Enemies((uint)OID.Pages512));
    }
}
