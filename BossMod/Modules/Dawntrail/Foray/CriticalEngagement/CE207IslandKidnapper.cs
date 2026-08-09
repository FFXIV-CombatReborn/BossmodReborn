using BossMod.Dawntrail.Foray.CriticalEngagement;
using static BossMod.Components.GenericKnockback;

namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE207IslandKidnapper;

public enum OID : uint
{
    Boss = 0x4BE1, // R3.2, BNpcName 14505, kidnapper
    Hurricane = 0x4BE2,
    Emitter = 0x4BE3, // wind spirit, spawns on the R16 ring facing center, casts the B953 ice-flower circles
    Anchor = 0x4BE4, // non-targetable arena controller at center
    Helper = 0x233C
}

public enum AID : uint
{
    IdleVisual = 0xB949, // boss->event target, no effects
    AutoAttack = 0xB94A, // boss->player, no cast, single-target
    WindBoundary = 0xB94B, // anchor, persistent outer deathwall; ARR player-center kills start at ~23y
    HurricaneVisual = 0xB94C,
    HurricaneKnockback = 0xB94D, // 5y away knockback
    RendingWindVisual = 0xB94E,
    RendingWind = 0xB94F, // range 60, 8y wide cross; two rotated crosses form the eight-way pattern
    GustHit = 0xB950, // raidwide damage and 24y forward knockback
    GaleBlade = 0xB951, // 60y 180-degree cone
    ScatterFeathers = 0xB952,
    WindBloom = 0xB953, // emitter self-centered 13y circle; four-six of them ring the arena and rotate wave to wave into a moving "ice flower", safe pocket near dead center
    DispersingGalesVisual = 0xB954,
    DispersingGales = 0xB955, // 60y 60-degree cone
    DownburstVisual = 0xB956,
    CycloneRingVisual = 0xB957,
    Downburst = 0xB958, // location, 15y circle
    CycloneRing = 0xB959, // 5-60y donut
    HurricaneHit = 0xBBF8, // helpers, no cast, raidwide damage
    GustTelegraph = 0xBC7A // helper, 60y long, 60y wide rect
}

sealed class WindBoundary(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut Visual = new(23f, 30f);
    private static readonly AOEShapeDonut Forbidden = new(22f, 30f);
    private readonly AOEInstance[] _aoe = [new(Visual, module.Arena.Center)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        => hints.AddForbiddenZone(Forbidden, Module.Arena.Center);

    public override void DrawArenaBackground(int pcSlot, Actor pc)
        => Arena.ZoneCircleOutlineUnclipped(Arena.Center, 23.5f, Colors.Danger, 2f);
}

sealed class KidnapperAOEs(BossModule module) : ReplayValidatedCastAOEs(module)
{
    private static readonly AOEShapeCone Half = new(60f, 90f.Degrees());
    private static readonly AOEShapeCone Cone = new(60f, 30f.Degrees());
    private static readonly AOEShapeCross Rending = new(60f, 4f);
    private static readonly AOEShapeCircle Downburst = new(15f);
    private static readonly AOEShapeCircle Bloom = new(13f);
    private static readonly AOEShapeDonut Ring = new(5f, 60f);

    protected override AOEConfig? ConfigFor(uint actionID) => actionID switch
    {
        (uint)AID.GaleBlade => new(Half),
        (uint)AID.DispersingGales => new(Cone),
        (uint)AID.RendingWind => new(Rending),
        (uint)AID.Downburst => new(Downburst, true),
        (uint)AID.CycloneRing => new(Ring),
        _ => null
    };
}

// 冰花: emitter 的 B953 cast 事件偶发缺失 (ARR 第 4 波无 cast), 依赖 cast 会漏画。
// 改为直接从存活 emitter 实时画 13y 圈, 不依赖 cast 事件。
sealed class WindBloomAOEs(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Shape = new(13f);
    private readonly List<AOEInstance> _displayed = [with(8)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _displayed.Clear();
        foreach (var emitter in Module.Enemies((uint)OID.Emitter))
            if (!emitter.IsDeadOrDestroyed)
                _displayed.Add(new(Shape, emitter.Position, color: Colors.Danger, actorID: emitter.InstanceID,
                    shapeDistance: Shape.Distance(emitter.Position, default)));
        return CollectionsMarshal.AsSpan(_displayed);
    }
}

// GenericKnockback only renders displacement and does not add an AI forbidden zone. The moving
// hurricane body is itself the four-yalm contact AOE, so publish a slightly padded live hazard too.
sealed class HurricaneHazards(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Shape = new(4.5f);
    private readonly List<AOEInstance> _displayed = [with(8)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _displayed.Clear();
        foreach (var hurricane in Module.Enemies((uint)OID.Hurricane))
            if (!hurricane.IsDeadOrDestroyed)
                _displayed.Add(new(Shape, hurricane.Position, color: Colors.Danger, actorID: hurricane.InstanceID,
                    shapeDistance: Shape.Distance(hurricane.Position, default)));
        return CollectionsMarshal.AsSpan(_displayed);
    }
}

// Hurricanes are persistent actors rather than cast bars. Their B94D contact event applies a
// five-yalm radial knockback inside the four-yalm hit area, so keep the live actor position.
sealed class HurricaneKnockbacks(BossModule module) : Components.GenericKnockback(module)
{
    // Contact is four yalms, but a warning only drawn inside the contact circle is invisible until
    // the player is already being knocked. Use a wider preview radius so the arrow appears as the
    // moving storm approaches; the separate HurricaneHazards circle still marks the lethal body.
    private static readonly AOEShapeCircle Shape = new(10f);
    private readonly List<Knockback> _displayed = [with(8)];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        _displayed.Clear();
        foreach (var hurricane in Module.Enemies((uint)OID.Hurricane))
            if (!hurricane.IsDeadOrDestroyed)
                _displayed.Add(new(hurricane.Position, 5f, WorldState.FutureTime(0.25d), Shape, default, Kind.AwayFromOrigin, actorID: hurricane.InstanceID));
        return CollectionsMarshal.AsSpan(_displayed);
    }
}

// BC7A is the cast-bar telegraph for B950, whose action effect is a 24y directional knockback.
// The gust comes from the wall on the main tank's side and flings everyone across the arena, so
// the helper's cast rotation already encodes the true push direction; the tank only decides which
// side the helper spawns on. Do not re-derive the direction from the tank's position - the helper
// rotation is authoritative (and stays valid even when the tank is mid-arena, which will carry the
// whole party out of bounds).
sealed class GustKnockback(BossModule module) : Components.GenericKnockback(module)
{
    private static readonly AOEShapeRect Shape = new(60f, 30f);
    private const float Distance = 24f;
    private const float SafeRadius = 22f;
    private const float LethalRadius = 23f;
    private const float PreferredStartDistance = 20.5f;
    private const float PreferredStartRadius = 2f;
    // Replay event timing is consistently about 0.60s after the helper cast finishes. Using the
    // old 1.05s estimate scheduled the safe-edge constraint roughly 0.4s after the real knockback.
    private const double HitDelay = 0.60d;
    private readonly List<Knockback> _casters = [with(2)];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        PruneExpired();
        return CollectionsMarshal.AsSpan(_casters);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var kb in _casters)
        {
            var displacement = Distance * kb.Direction.ToDirection();
            var center = Arena.Center;
            hints.AddForbiddenZone(new SDKnockbackInCircleFixedDirection(center, displacement, SafeRadius), kb.Activation);
            var preferredStart = center - PreferredStartDistance * kb.Direction.ToDirection();
            hints.GoalZones.Add(position => position.InCircle(preferredStart, PreferredStartRadius) ? 5f : 0f);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_casters.Count == 0)
            return;

        var tank = Module.PrimaryActor?.TargetID is ulong id && id != 0 ? WorldState.Actors.Find(id) : null;
        if (tank != null && !tank.IsDeadOrDestroyed && (tank.Position - Module.Arena.Center).Length() < 5f)
            hints.Add("Main tank in the middle - the gust will push the whole party out of bounds!");
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !pos.InCircle(Arena.Center, LethalRadius);

    public override void Update() => PruneExpired();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.GustTelegraph || spell.EventHappened)
            return;

        _casters.RemoveAll(kb => kb.ActorID == caster.InstanceID);
        _casters.Add(new(spell.LocXZ, Distance, Module.CastFinishAt(spell, HitDelay), Shape, spell.Rotation, Kind.DirForward, actorID: caster.InstanceID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.GustHit)
            return;

        _casters.Clear();
        ++NumCasts;
    }

    private void PruneExpired()
    {
        var now = WorldState.CurrentTime;
        _casters.RemoveAll(kb => now > kb.Activation.AddSeconds(1d));
    }
}
// B94C resolves into the BBF8 helper raidwide about 0.9s after the boss cast. BC7A similarly
// resolves into B950 while applying the directional knockback.
sealed class KidnapperRaidwides(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.HurricaneVisual, (uint)AID.GustTelegraph]);

sealed class IslandKidnapperStates : StateMachineBuilder
{
    public IslandKidnapperStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WindBoundary>()
            .ActivateOnEnter<KidnapperAOEs>()
            .ActivateOnEnter<WindBloomAOEs>()
            .ActivateOnEnter<HurricaneHazards>()
            .ActivateOnEnter<HurricaneKnockbacks>()
            .ActivateOnEnter<GustKnockback>()
            .ActivateOnEnter<KidnapperRaidwides>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(IslandKidnapperStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    PrimaryActorOID = (uint)OID.Boss,
    Contributors = "KanoNoUta",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CriticalEngagement,
    GroupID = 1093u,
    NameID = 61u,
    SortOrder = 6)]
public sealed class IslandKidnapper(WorldState ws, Actor primary) : BossModule(ws, primary, new(-150f, -860f), new ArenaBoundsCircle(23.5f));
