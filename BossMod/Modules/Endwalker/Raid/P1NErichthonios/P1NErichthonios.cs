namespace BossMod.Endwalker.Raid.P1NErichthonios;

public enum OID : uint
{
    Boss = 0x3521, // R9.975, x1
    Helper = 0x233C, // R0.500, x24, Helper type
    FlailLR = 0x3523, // R3.500, x5, Part type
    FlailI = 0x3524, // R3.500, x2, Part type
    Actor1E8536 = 0x1E8536, // R2.000, x1, EventObj type
    Exit = 0x1E850B, // R0.500, x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    GaolersFlailLR = 26073, // Boss->self, 8.0+1.0s cast, range 60 270-degree cone (P1S shape; never does damage in Normal)
    GaolersFlailRL = 26074, // Boss->self, 8.0+1.0s cast, range 60 270-degree cone (never does damage in Normal)
    AetherflailLR = 26079, // Boss->self, 8.0+1.0s cast, (as above), then aether explosion
    AetherflailRL = 26080, // Boss->self, 8.0+1.0s cast, (as above), then aether explosion
    PitilessFlail = 26085, // Boss->player, 5.0s cast, range 60 width 6 rect, knockback
    TrueHolyVisual = 26086, // Boss->self, no cast, single-target
    TrueHoly = 26087, // Helper->players, no cast, range 6 circle stack
    ShiningCells = 26089, // Boss->self, 7.0s cast, range 60 circle, raidwide and cells arena
    SlamShut = 26090, // Boss->self, 7.0s cast, range 60 circle, raidwide and square arena
    Aetherchain = 26091, // Boss->self, 5.0s cast, single-target
    PowerfulFire = 26092, // Helper->self, no cast, red cells
    PowerfulLight = 26093, // Helper->self, no cast, blue cells
    Intemperance = 26094, // Boss->self, 2.0s cast, single-target
    IntemperateTorment = 26095, // Boss->self, 6.0s cast, single-target
    HotSpellFirst = 26096, // Helper->self, 1.5s cast, range 19 width 19 rect
    ColdSpellFirst = 26097, // Helper->self, 1.5s cast, range 19 width 19 rect
    PainfulFlux = 26098, // Helper->self, no cast, kills anyone in the 2y cross between the quadrants, 0.1s after each wave
    HeavyHand = 26099, // Boss->player, 5.0s cast, single-target
    WardersWrath = 26100, // Boss->self, 5.0s cast, range 60 circle
    Teleport = 26101, // Boss->location, no cast, single-target
    HotSpell = 27845, // Helper->self, 4.0s cast, range 19 width 19 rect
    ColdSpell = 27846, // Helper->self, 4.0s cast, range 19 width 19 rect
    PitilessFlailTarget = 27925, // Helper->player, no cast, single-target
    GaolersFlailR2 = 28066, // Helper->self, 8.7s cast, range 60 180-degree cone
    GaolersFlailL2 = 28067, // Helper->self, 8.7s cast, range 60 180-degree cone
}

public enum SID : uint
{
    ColdSpell = 2739, // Helper->player, extra=0x0
    HotSpell = 2740, // Helper->player, extra=0x0
    AetherExplosion = 2195, // none->Boss, extra=0x14C (red) or 0x14D (blue)
}

public enum IconID : uint
{
    PitilessFlail = 1, // player
    TrueHoly = 62, // player
    HeavyHand = 218, // player
}

sealed class Flails(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GaolersFlailR2, new AOEShapeCone(60, 90.Degrees()));
sealed class FlailsL(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GaolersFlailL2, new AOEShapeCone(60, 90.Degrees()));

sealed class AetherExplosion(BossModule module) : Components.GenericAOEs(module, (uint)AID.PowerfulFire)
{
    private static readonly AOEShapeCone _inner = new(P1NErichthonios.InnerCircleRadius, 22.5f.Degrees());
    private static readonly AOEShapeDonutSector _outer = new(P1NErichthonios.InnerCircleRadius, 60, 22.5f.Degrees());
    private Angle? _start;
    private DateTime _activation;

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoesBuffer.Clear();
        if (_start is not { } start)
            return CollectionsMarshal.AsSpan(_aoesBuffer);
        for (var i = 0; i < 4; ++i)
        {
            var dir = start + (90 * i).Degrees();
            _aoesBuffer.Add(new(_inner, Arena.Center, dir + 22.5f.Degrees(), _activation));
            _aoesBuffer.Add(new(_outer, Arena.Center, dir + 67.5f.Degrees(), _activation));
        }
        return CollectionsMarshal.AsSpan(_aoesBuffer);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (actor == Module.PrimaryActor && status.ID == (uint)SID.AetherExplosion)
        {
            _start = status.Extra switch
            {
                0x14C => 45.Degrees(),
                0x14D => 0.Degrees(),
                _ => null
            };
            if (_start == null)
                ReportError($"Unexpected aether explosion param {status.Extra:X}");
            _activation = WorldState.FutureTime(5.8f);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Aetherchain:
                _activation = Module.CastFinishAt(spell, 0.9f);
                break;
            case (uint)AID.AetherflailLR:
            case (uint)AID.AetherflailRL:
                _activation = Module.CastFinishAt(spell, 0.8f);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.PowerfulFire or (uint)AID.PowerfulLight)
        {
            ++NumCasts;
            _start = null;
        }
    }
}

sealed class PitilessFlail(BossModule module) : Components.BaitAwayCast(module, (uint)AID.PitilessFlail, new AOEShapeRect(60, 3));

sealed class PitilessFlailKnockback(BossModule module) : Components.GenericKnockback(module, (uint)AID.PitilessFlail)
{
    private Actor? _caster;

    private readonly List<Knockback> _kbBuffer = [];
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        _kbBuffer.Clear();
        if (_caster?.CastInfo is { } ci && ci.TargetID == actor.InstanceID)
            _kbBuffer.Add(new(_caster.Position, 11, Module.CastFinishAt(ci)));
        return CollectionsMarshal.AsSpan(_kbBuffer);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.PitilessFlail)
            _caster = caster;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.PitilessFlail)
            _caster = null;
    }
}
sealed class TrueHoly(BossModule module) : Components.StackWithIcon(module, (uint)IconID.TrueHoly, (uint)AID.TrueHoly, 6, 5.1f);

sealed class Intemperance(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _quadrant = new(9.5f, 9.5f, 9.5f);
    private static readonly AOEShapeCross _cross = new(20, 1.5f);
    private static readonly WPos[] _centers = [new(110.5f, 110.5f), new(89.5f, 110.5f), new(89.5f, 89.5f), new(110.5f, 89.5f)];
    private readonly bool?[] _hot = new bool?[12];
    private int _wave;
    private DateTime _next;
    private DateTime _lastWave;
    private readonly bool?[] _casts = new bool?[4];
    private bool _active;

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoesBuffer.Clear();
        if (!_active || _wave >= 3)
            return CollectionsMarshal.AsSpan(_aoesBuffer);
        var casting = _casts[0] != null || _casts[1] != null || _casts[2] != null || _casts[3] != null;
        if (casting || _wave == 0)
            _aoesBuffer.Add(new(_cross, Arena.Center, default, _next.AddSeconds(0.1f)));
        bool? hot = actor.FindStatus((uint)SID.HotSpell) != null ? true : actor.FindStatus((uint)SID.ColdSpell) != null ? false : null;
        if (hot == null || !casting && _wave > 0)
            return CollectionsMarshal.AsSpan(_aoesBuffer);
        for (var q = 0; q < 4; ++q)
            if ((casting ? _casts[q] : _hot[3 * q + _wave]) == hot)
                _aoesBuffer.Add(new(_quadrant, _centers[q], default, _next));
        return CollectionsMarshal.AsSpan(_aoesBuffer);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_active && _wave < 3 && actor.FindStatus((uint)SID.HotSpell) != null)
            hints.Add("Go to a cold quadrant!", false);
        else if (_active && _wave < 3 && actor.FindStatus((uint)SID.ColdSpell) != null)
            hints.Add("Go to a hot quadrant!", false);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index < 12)
        {
            switch (state)
            {
                case 0x00200010:
                    _hot[index] = true;
                    break;
                case 0x00020001:
                    _hot[index] = false;
                    break;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Intemperance:
                _active = true;
                _wave = 0;
                _next = Module.CastFinishAt(spell, 14.6f);
                break;
            case (uint)AID.HotSpellFirst:
            case (uint)AID.ColdSpellFirst:
            case (uint)AID.HotSpell:
            case (uint)AID.ColdSpell:
                _next = Module.CastFinishAt(spell);
                var off = spell.LocXZ - Arena.Center;
                _casts[off.X > 0 ? (off.Z > 0 ? 0 : 3) : (off.Z > 0 ? 1 : 2)] = spell.Action.ID is (uint)AID.HotSpellFirst or (uint)AID.HotSpell;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.HotSpellFirst or (uint)AID.ColdSpellFirst or (uint)AID.HotSpell or (uint)AID.ColdSpell && WorldState.CurrentTime > _lastWave.AddSeconds(1))
        {
            _lastWave = WorldState.CurrentTime;
            Array.Fill(_casts, null);
            ++_wave;
            ++NumCasts;
            _active = _wave < 3;
            _next = WorldState.FutureTime(9);
        }
    }
}

sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ShiningCells:
                Arena.Bounds = new ArenaBoundsCircle(20);
                break;
            case (uint)AID.SlamShut:
                Arena.Bounds = new ArenaBoundsSquare(20);
                break;
        }
    }
}

sealed class ShiningCells(BossModule module) : Components.RaidwideCast(module, (uint)AID.ShiningCells);
sealed class SlamShut(BossModule module) : Components.RaidwideCast(module, (uint)AID.SlamShut);
sealed class HeavyHand(BossModule module) : Components.SingleTargetCast(module, (uint)AID.HeavyHand);
sealed class WardersWrath(BossModule module) : Components.RaidwideCast(module, (uint)AID.WardersWrath);

sealed class P1NErichthoniosStates : StateMachineBuilder
{
    public P1NErichthoniosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<Flails>()
            .ActivateOnEnter<FlailsL>()
            .ActivateOnEnter<AetherExplosion>()
            .ActivateOnEnter<PitilessFlail>()
            .ActivateOnEnter<PitilessFlailKnockback>()
            .ActivateOnEnter<TrueHoly>()
            .ActivateOnEnter<Intemperance>()
            .ActivateOnEnter<ShiningCells>()
            .ActivateOnEnter<SlamShut>()
            .ActivateOnEnter<HeavyHand>()
            .ActivateOnEnter<WardersWrath>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Nonu (ported from upstream and modified by LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 808, NameID = 10345)]
public sealed class P1NErichthonios(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20))
{
    public const float InnerCircleRadius = 11.5f;

    protected override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Bounds is ArenaBoundsCircle)
        {
            var diag = Bounds.Radius / 1.414214f;
            Arena.ZoneCircleOutline(Center, InnerCircleRadius, Colors.Border);
            Arena.AddLine(Center + new WDir(Bounds.Radius, 0), Center - new WDir(Bounds.Radius, 0), Colors.Border);
            Arena.AddLine(Center + new WDir(0, Bounds.Radius), Center - new WDir(0, Bounds.Radius), Colors.Border);
            Arena.AddLine(Center + new WDir(diag, +diag), Center - new WDir(diag, +diag), Colors.Border);
            Arena.AddLine(Center + new WDir(diag, -diag), Center - new WDir(diag, -diag), Colors.Border);
        }
    }
}
