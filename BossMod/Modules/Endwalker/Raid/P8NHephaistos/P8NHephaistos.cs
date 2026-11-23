namespace BossMod.Endwalker.Raid.P8NHephaistos;

public enum OID : uint
{
    Boss = 0x3AC5, // R10.000, x2
    Helper = 0x233C, // R0.500, x57, Helper type
    Actor3AC6 = 0x3AC6, // R0.000, x2, Part type
    Actor3AC7 = 0x3AC7, // R0.000, x2, Part type
    Gorgon = 0x3AC8, // R1.200, x2
    Suneater = 0x3ACF, // R7.000, x4
    IllusoryHephaistos = 0x3AEF, // R10.000, x6
    ActorFED4A = 0xFED4A, // R0.500, x2, EventNpc type
    ActorFED4B = 0xFED4B, // R0.500, x1, EventNpc type
    ActorFED4C = 0xFED4C, // R0.500, x1, EventNpc type
    Exit = 0x1E850B, // R0.500, x2, EventObj type
    Actor1EA1A1 = 0x1EA1A1, // R2.000, x2, EventObj type
    MagitekArmor = 0x1EB703, // R0.500, x4, EventObj type
}

public enum AID : uint
{
    PetrifactionAOE = 26403, // Helper->self, no cast, range 60 circle, gaze from a gorgon
    GorgospitIllusory = 28631, // IllusoryHephaistos->self, 6.0s cast, range 60 width 10 rect
    QuadrupedalImpactAOE = 28931, // Helper->location, 0.8s cast, range 60 circle, knockback from the landing spot
    Ability28936 = 28936, // Boss->location, no cast, single-target
    VolcanicTorchesAlt = 29150, // Boss->self, 3.0s cast, single-target
    SunforgeSnake = 30956, // Boss->self, 7.0+1.0s cast, single-target
    SunforgePhoenix = 30957, // Boss->self, 7.0+1.0s cast, single-target
    ScorchingFang = 30958, // Helper->self, 8.0s cast, range 42 width 14 rect
    SunsPinion = 30959, // Helper->self, 8.0s cast, range 14 width 42 rect
    ChthonicVent = 30960, // Boss->self, 3.0s cast, single-target
    AbyssalFires = 30961, // Helper->location, 5.0s cast, range 60 circle, proximity from a corner
    FourfoldFires = 30962, // Boss->self, 3.0s cast, single-target
    ChthonicVentMoveDiag = 30963, // Suneater->self, no cast, single-target
    ChthonicVentMoveNear = 30964, // Suneater->self, no cast, single-target
    ChthonicVentAOE1 = 30965, // Helper->self, 5.0s cast, range 23 circle
    ChthonicVentAOE2 = 30966, // Suneater->self, 2.0s cast, range 23 circle
    VolcanicTorches = 30967, // Boss->self, 3.0s cast, single-target
    TorchFlame = 30968, // Helper->self, 10.0s cast, range 10 width 10 rect
    IntoTheShadows = 30971, // Boss->self, 3.0s cast, single-target
    Petrifaction = 30972, // Gorgon->self, 5.0s cast, single-target
    Gorgoneion = 30973, // Gorgon->self, 15.0s cast, range 60 circle
    Ektothermos = 30974, // Boss->self, 5.0s cast, range 60 circle
    Gorgospit = 30975, // Boss->self, 3.0s cast, range 60 width 10 rect
    BlazingFootfalls = 30976, // Boss->location, 10.0+0.6s cast, single-target
    QuadrupedalImpact = 30978, // Boss->location, no cast, single-target
    QuadrupedalCrush = 30980, // Boss->location, no cast, single-target
    GenesisOfFlame = 30981, // Boss->self, 5.0s cast, range 60 circle
    HemitheossFlare = 30982, // Boss->self, 3.0s cast, single-target
    HemitheossFlareAOE = 30983, // Helper->players, 5.0s cast, range 6 circle spread
    Flameviper = 30984, // Boss->self/player, 5.0s cast, range 60 width 5 rect
    AutoAttack = 30985, // Boss->player, no cast, single-target
    TrailblazePreview = 30987, // Helper->self, 2.5s cast, range 42 width 42 rect, covers the whole arena
    QuadrupedalImpactMarker = 30988, // Helper->location, 2.5s cast, range 60 circle, marks the next landing spot in order
    Trailblaze = 30989, // Helper->self, 0.6s cast, range 42 width 42 rect
    AutoAttack1 = 30990, // Boss->player, no cast, single-target
    AutoAttack2 = 30991, // Boss->player, no cast, single-target
    ReforgedReflectionCentaur = 31051, // Boss->self, 3.0s cast, single-target
    ReforgedReflectionSnake = 31052, // Boss->self, 3.0s cast, single-target
    ChthonicVentAOE3 = 31053, // Helper->self, 2.0s cast, range 23 circle
    RearingRampage = 31147, // Boss->self, 5.0s cast, range 60 circle
}

public enum IconID : uint
{
    HemitheossFlare = 139, // player
    Flameviper = 230, // player
}

public enum TetherID : uint
{
    ChthonicVent = 5, // Helper->Boss, the helper sits on the first vent
}

sealed class ScorchingFang(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ScorchingFang, new AOEShapeRect(42, 7));
sealed class SunsPinion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SunsPinion, new AOEShapeRect(14, 21));
sealed class AbyssalFires(BossModule module) : Components.ProximityAOEs(module, (uint)AID.AbyssalFires, 15);
sealed class TorchFlame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TorchFlame, new AOEShapeRect(10, 5));
sealed class Gorgoneion(BossModule module) : Components.CastHint(module, (uint)AID.Gorgoneion, "Kill the gorgons!");
sealed class Gorgons(BossModule module) : Components.Adds(module, (uint)OID.Gorgon, 1);
sealed class Ektothermos(BossModule module) : Components.RaidwideCast(module, (uint)AID.Ektothermos);
sealed class Gorgospit(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Gorgospit, new AOEShapeRect(60, 5));
sealed class GorgospitIllusory(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GorgospitIllusory, new AOEShapeRect(60, 5));
sealed class GenesisOfFlame(BossModule module) : Components.RaidwideCast(module, (uint)AID.GenesisOfFlame);
sealed class Trailblaze(BossModule module) : Components.RaidwideCast(module, (uint)AID.TrailblazePreview, "Raidwide (charge across the arena)");
sealed class RearingRampage(BossModule module) : Components.RaidwideCast(module, (uint)AID.RearingRampage);
sealed class HemitheossFlare(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.HemitheossFlareAOE, 6);
sealed class Flameviper(BossModule module) : Components.BaitAwayCast(module, (uint)AID.Flameviper, new AOEShapeRect(60, 2.5f));

sealed class Petrifaction(BossModule module) : Components.GenericGaze(module, (uint)AID.PetrifactionAOE)
{
    private readonly List<Eye> _eyes = [];

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor) => CollectionsMarshal.AsSpan(_eyes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Petrifaction)
            _eyes.Add(new(caster.Position, Module.CastFinishAt(spell, 1.5f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.PetrifactionAOE)
        {
            ++NumCasts;
            _eyes.RemoveAll(e => e.Position.AlmostEqual(caster.Position, 1));
        }
    }
}

sealed class ChthonicVent(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _shape = new(23);
    private readonly List<WPos> _centers = [];

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoesBuffer.Clear();
        var count = _centers.Count;
        for (var i = 0; i < count; ++i)
            _aoesBuffer.Add(new(_shape, _centers[i]));
        return CollectionsMarshal.AsSpan(_aoesBuffer);
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.ChthonicVent)
            Add(source.Position);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ChthonicVentAOE1 or (uint)AID.ChthonicVentAOE2 or (uint)AID.ChthonicVentAOE3)
            Add(caster.Position);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ChthonicVentMoveNear:
                Add(caster.Position + caster.Rotation.ToDirection() * 30);
                break;
            case (uint)AID.ChthonicVentMoveDiag:
                Add(caster.Position + caster.Rotation.ToDirection() * 42.426407f);
                break;
            case (uint)AID.ChthonicVentAOE1:
            case (uint)AID.ChthonicVentAOE2:
            case (uint)AID.ChthonicVentAOE3:
                ++NumCasts;
                _centers.RemoveAll(c => c.AlmostEqual(caster.Position, 2));
                break;
        }
    }

    private void Add(WPos pos)
    {
        if (!_centers.Any(c => c.AlmostEqual(pos, 2)))
            _centers.Add(pos);
    }
}

sealed class BlazingFootfalls(BossModule module) : Components.GenericKnockback(module)
{
    private static readonly AOEShapeRect _side = new(21, 10.5f, 21);
    private readonly List<(WPos pos, DateTime activation)> _spots = [];
    private (WPos origin, Angle dir, DateTime activation)? _charge;

    private readonly List<Knockback> _kbBuffer = [];
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        _kbBuffer.Clear();
        if (_charge is var (origin, dir, activation))
        {
            var mid = origin + (Module.Center - origin).Dot(dir.ToDirection()) * dir.ToDirection();
            _kbBuffer.Add(new(mid + 10.5f * dir.ToDirection().OrthoL(), 10, activation, _side, dir, Kind.DirLeft));
            _kbBuffer.Add(new(mid + 10.5f * dir.ToDirection().OrthoR(), 10, activation, _side, dir, Kind.DirRight));
        }
        foreach (var s in _spots)
            _kbBuffer.Add(new(s.pos, 20, s.activation));
        return CollectionsMarshal.AsSpan(_kbBuffer);
    }

    private const float LaneHalfWidth = 1.5f;
    private const float LaneOffset = 3.5f;
    private const float WallMargin = 3;

    private (WPos mid, WDir normal, float side)? Lane()
    {
        if (_charge is not var (origin, dir, _))
            return null;
        var d = dir.ToDirection();
        var mid = origin + (Module.Center - origin).Dot(d) * d;
        var normal = d.OrthoL();
        var side = 0f;
        for (var i = 0; i < _spots.Count; ++i)
        {
            var off = normal.Dot(_spots[i].pos - mid);
            if (Math.Abs(off) > 2f)
            {
                side = off;
                break;
            }
        }
        return (mid, normal, Math.Sign(side));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (Lane() is var (mid, normal, side))
        {
            hints.AddForbiddenZone(new SDInvertedRect(Arena.Center, default(Angle), 20f - WallMargin, 20f - WallMargin, 20f - WallMargin), _charge?.activation ?? default);
            var target = normal.Dot(mid.ToWDir()) + side * LaneOffset;
            hints.GoalZones.Add(p => Math.Max(0f, 1f - Math.Abs(normal.Dot(p.ToWDir()) - target) / LaneHalfWidth) * 10f);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);
        if (Lane() is var (mid, normal, side) && _charge is var (_, dir, _))
            Arena.ZoneRect(mid + side * LaneOffset * normal, dir, 21, 21, LaneHalfWidth, Colors.SafeFromAOE);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BlazingFootfalls:
                _charge = (caster.Position, Angle.FromDirection(spell.LocXZ - caster.Position), Module.CastFinishAt(spell, 0.6f));
                break;
            case (uint)AID.QuadrupedalImpactMarker:
                _spots.Add((spell.LocXZ, Module.CastFinishAt(spell, 6.7f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Trailblaze:
                _charge = null;
                break;
            case (uint)AID.QuadrupedalImpactAOE:
                ++NumCasts;
                if (_spots.Count > 0)
                    _spots.RemoveAt(0);
                break;
        }
    }
}

sealed class P8NHephaistosStates : StateMachineBuilder
{
    public P8NHephaistosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ScorchingFang>()
            .ActivateOnEnter<SunsPinion>()
            .ActivateOnEnter<AbyssalFires>()
            .ActivateOnEnter<ChthonicVent>()
            .ActivateOnEnter<TorchFlame>()
            .ActivateOnEnter<Petrifaction>()
            .ActivateOnEnter<Gorgoneion>()
            .ActivateOnEnter<Gorgons>()
            .ActivateOnEnter<Ektothermos>()
            .ActivateOnEnter<Gorgospit>()
            .ActivateOnEnter<GorgospitIllusory>()
            .ActivateOnEnter<GenesisOfFlame>()
            .ActivateOnEnter<Trailblaze>()
            .ActivateOnEnter<BlazingFootfalls>()
            .ActivateOnEnter<RearingRampage>()
            .ActivateOnEnter<HemitheossFlare>()
            .ActivateOnEnter<Flameviper>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Nonu (ported from upstream and modified by LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 883, NameID = 11399)]
public sealed class P8NHephaistos(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));
