namespace BossMod.Endwalker.Raid.P5NProtoCarbuncle;

public enum OID : uint
{
    Boss = 0x39E6, // R8.500, x1
    Helper = 0x233C, // R0.500, x20, Helper type
    Daphnia = 0x3432, // R1.500-3.000, x1
    Osculator = 0x3433, // R0.600, x1
    StampedeMarker = 0x39E7, // R1.000, x1, runs the starving stampede path before the boss
    StampedeSpot = 0x39E8, // R1.000, x5
    LivelyBait = 0x39E9, // R0.960, x1
    ActorFEC1B = 0xFEC1B, // R0.500, x1, EventNpc type
    ActorFEC1C = 0xFEC1C, // R0.500, x1, EventNpc type
    Exit = 0x1E850B, // R0.500, x1, EventObj type
    Actor1EA1A1 = 0x1EA1A1, // R2.000, x1, EventObj type
    TopazStone = 0x1EB78C, // R0.500, x6, EventObj type
    TopazStonePoison = 0x1EB78D, // R0.500, x2, EventObj type
}

public enum AID : uint
{
    RubyGlow = 30420, // Boss->self, 5.0s cast, single-target
    RubyGlowAOE = 30421, // Helper->self, 5.0s cast, range 60 circle
    Ability30422 = 30422, // Boss->location, no cast, single-target
    SearingRay = 30423, // Boss->self, 7.0s cast, range 45 180-degree cone
    SearingRayCage = 30424, // Boss->self, 6.5+0.5s cast, hits the boss's side of the diagonal wall
    SearingRayCageReflection = 30426, // Helper->self, 7.0s cast, range 1 width 4 rect in the sheet; really the half of the arena in its direction
    TopazStones = 30430, // Boss->self, 4.0s cast, single-target
    TopazStonesVisual = 30431, // Helper->self, 4.0s cast, single-target
    TopazRay = 30432, // Helper->self, 6.5s cast, range 4 circle
    RubyReflectionQuadrant = 30434, // Helper->self, 7.0s cast, range 15 width 15 rect
    Ability30435 = 30435, // Boss->self, no cast, single-target
    StampedeMarkerMove = 30436, // StampedeMarker->location, no cast, single-target
    StarvingStampedeJump = 30437, // Boss->location, no cast, single-target
    TopazCluster = 30438, // Boss->self, 4.0s cast, single-target
    VenomSquall = 30444, // Boss->self, 5.0s cast, single-target
    VenomRain = 30445, // Helper->player, 5.0s cast, range 5 circle spread
    VenomDrops = 30446, // Helper->location, 5.0s cast, range 5 circle
    VenomPool = 30447, // Helper->players, 8.0s cast, range 6 circle stack on a healer
    Crunch = 30448, // Boss->player, 5.0s cast, single-target
    ToxicCrunch = 30449, // Boss->player, 5.0s cast, single-target
    SonicHowl = 30450, // Boss->self, 5.0s cast, range 60 circle
    Devour = 30501, // Boss->player, no cast, single-target, eats players hit by the last stampede jump
    DevourAlt = 30502, // Boss->player, no cast, single-target
    Spit = 30503, // Boss->self, 2.0s cast, single-target
    DevourKill = 30504, // Helper->player, no cast, single-target
    Ability30954 = 30954, // Boss->location, no cast, single-target
    AcidicSlaver = 30955, // Boss->self, 3.0s cast, range 60 circle
    StarvingStampede = 31209, // Helper->self, 1.5s cast, range 12 circle
    SearingRayCageBounce = 31217, // Helper->self, no cast, range 0 ???
    RubyReflectionQuadrantRepeat = 31218, // Helper->self, no cast, range 15 width 15 rect
    TopazRayBent = 31228, // Helper->self, 6.5s cast, range 4 circle cut off by the wall it faces
    TopazRayBentCluster = 31229, // Helper->self, 6.5s cast, range 4 circle cut off by the wall it faces
    AutoAttack = 31245, // Boss->player, no cast, single-target
    Impact = 31263, // Helper->self, 4.5s cast, range 6 circle
}

public enum IconID : uint
{
    VenomRain = 100, // player
    Tankbuster = 218, // player
    VenomPool = 376, // player
}

sealed class RubyGlow(BossModule module) : Components.RaidwideCast(module, (uint)AID.RubyGlowAOE);
sealed class SearingRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SearingRay, new AOEShapeCone(45, 90.Degrees()));
sealed class TopazRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TopazRay, 4);
sealed class VenomDrops(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VenomDrops, 5);
sealed class VenomRain(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.VenomRain, 5);
sealed class VenomPool(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.VenomPool, 6);
sealed class Crunch(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Crunch);
sealed class ToxicCrunch(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ToxicCrunch);
sealed class SonicHowl(BossModule module) : Components.RaidwideCast(module, (uint)AID.SonicHowl);
sealed class AcidicSlaver(BossModule module) : Components.RaidwideCast(module, (uint)AID.AcidicSlaver);
sealed class Impact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Impact, 6);

sealed class TopazRayBent(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone Shape = new(5, 90.Degrees());
    private readonly List<(Actor caster, AOEInstance aoe)> _aoes = [];

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoesBuffer.Clear();
        var count = _aoes.Count;
        for (var i = 0; i < count; ++i)
            _aoesBuffer.Add(_aoes[i].aoe);
        return CollectionsMarshal.AsSpan(_aoesBuffer);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var quadrants = Module.FindComponent<RubyReflectionQuadrant>();
        foreach (var (_, a) in _aoes)
        {
            var probe = a.Origin + 2f * a.Rotation.ToDirection();
            var covered = false;
            if (quadrants != null)
            {
                var cells = quadrants.ActiveAOEs(pcSlot, pc);
                for (var i = 0; i < cells.Length; ++i)
                {
                    if (cells[i].Check(probe))
                    {
                        covered = true;
                        break;
                    }
                }
            }
            if (!covered)
                a.Shape.Draw(Arena, a.Origin, a.Rotation, a.Color);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.TopazRayBent or (uint)AID.TopazRayBentCluster)
            _aoes.Add((caster, new(Shape, spell.LocXZ + spell.Rotation.ToDirection(), spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell))));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.TopazRayBent or (uint)AID.TopazRayBentCluster)
            _aoes.RemoveAll(a => a.caster == caster);
    }
}

sealed class SearingRayCage(BossModule module) : Components.GenericAOEs(module, (uint)AID.SearingRayCage)
{
    private static readonly AOEShapeRect Shape = new(45, 45);
    private AOEInstance? _aoe;
    public AOEInstance? Current => _aoe;

    private readonly AOEInstance[] _aoeBuffer = new AOEInstance[1];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe is not AOEInstance aoe)
            return [];
        _aoeBuffer[0] = aoe;
        return _aoeBuffer;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SearingRayCage)
            _aoe = new(Shape, Module.Center, Angle.FromDirection(caster.Position - Module.Center), Module.CastFinishAt(spell, 0.5f));
    }

    public override void Update()
    {
        if (_aoe is var aoe && aoe != null && WorldState.CurrentTime > aoe.Value.Activation.AddSeconds(1.2f))
            _aoe = null;
    }
}

sealed class RubyReflection(BossModule module) : Components.GenericAOEs(module, (uint)AID.SearingRayCageReflection)
{
    private static readonly AOEShapeRect Shape = new(45, 45);
    private readonly List<AOEInstance> _aoes = [];

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Module.FindComponent<SearingRayCage>()?.Current is not AOEInstance cage)
            return CollectionsMarshal.AsSpan(_aoes);
        _aoesBuffer.Clear();
        var count = _aoes.Count;
        for (var i = 0; i < count; ++i)
        {
            if (!_aoes[i].Rotation.AlmostEqual(cage.Rotation, 0.1f))
                _aoesBuffer.Add(_aoes[i]);
        }
        return CollectionsMarshal.AsSpan(_aoesBuffer);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SearingRayCageReflection)
        {
            _aoes.Add(new(Shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
        }
    }

    public override void Update() => _aoes.RemoveAll(a => WorldState.CurrentTime > a.Activation.AddSeconds(0.9f));

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SearingRayCageBounce)
            ++NumCasts;
    }
}

sealed class RubyReflectionQuadrant(BossModule module) : Components.GenericAOEs(module, (uint)AID.RubyReflectionQuadrant)
{
    private static readonly AOEShapeRect Shape = new(15, 7.5f);
    private readonly List<(Actor caster, AOEInstance aoe, int repeats)> _aoes = [];

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // earliest three
        _aoesBuffer.Clear();
        var count = _aoes.Count;
        for (var i = 0; i < count; ++i)
            _aoesBuffer.Add(_aoes[i].aoe);
        _aoesBuffer.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
        var span = CollectionsMarshal.AsSpan(_aoesBuffer);
        return span.Length > 3 ? span[..3] : span;
    }

    public override void Update()
    {
        _aoes.RemoveAll(a => WorldState.CurrentTime > a.aoe.Activation.AddSeconds(2));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RubyReflectionQuadrant)
        {
            _aoes.RemoveAll(a => a.caster == caster);
            _aoes.Add((caster, new(Shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)), 0));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.RubyReflectionQuadrantRepeat)
            return;
        var index = _aoes.FindIndex(a => a.caster == caster);
        if (index < 0)
            return;
        if (_aoes[index].repeats >= 1)
            _aoes.RemoveAt(index);
        else
            _aoes[index] = _aoes[index] with { repeats = 1 };
    }
}

sealed class PoisonStones(BossModule module) : Components.GenericAOEs(module)
{
    private const float StartRadius = 5;
    private const float MaxRadius = 12;
    private const float GrowthPerSecond = 0.75f;
    private const float Lead = 2;
    private readonly List<(Actor stone, DateTime activation)> _stones = [];

    private float RadiusAt(DateTime activation, DateTime time) => Math.Clamp(StartRadius + GrowthPerSecond * (float)(time - activation).TotalSeconds, StartRadius, MaxRadius);
    private float LeadRadius(DateTime activation) => Math.Min(RadiusAt(activation, WorldState.CurrentTime) + Lead, MaxRadius);

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoesBuffer.Clear();
        var count = _stones.Count;
        var now = WorldState.CurrentTime;
        for (var i = 0; i < count; ++i)
        {
            var s = _stones[i];
            _aoesBuffer.Add(new(new AOEShapeCircle(RadiusAt(s.activation, now)), s.stone.Position, default, s.activation)); // radius grows, so the shape is rebuilt
        }
        return CollectionsMarshal.AsSpan(_aoesBuffer);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        for (var i = 0; i < _stones.Count; ++i)
        {
            var s = _stones[i];
            if (actor.Position.InCircle(s.stone.Position, LeadRadius(s.activation)))
            {
                hints.Add("Poison spreads here, move away!");
                return;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var s in _stones)
            if (RadiusAt(s.activation, WorldState.CurrentTime) < MaxRadius)
                hints.AddForbiddenZone(new AOEShapeCircle(LeadRadius(s.activation)), s.stone.Position, default, DateTime.MaxValue);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID != (uint)OID.TopazStone)
            return;
        if (state is 0x00100080 or 0x00100020 && !_stones.Any(s => s.stone == actor))
            _stones.Add((actor, WorldState.FutureTime(5)));
        else if (state == 0x00040200)
            _stones.RemoveAll(s => s.stone == actor);
    }
}

sealed class StarvingStampede(BossModule module) : Components.GenericAOEs(module, (uint)AID.StarvingStampede)
{
    private const float Radius = 12;
    private const float JumpInterval = 1.05f;
    private const float MarkerInterval = 1.55f;
    private static readonly AOEShapeCircle Shape = new(Radius);
    private readonly List<WPos> _path = [];
    private int _numJumps;
    private DateTime _nextJump;
    private readonly List<DateTime> _casts = [];

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoesBuffer.Clear();
        if (_nextJump == default && _casts.Count == 0)
            return CollectionsMarshal.AsSpan(_aoesBuffer);
        var next = _casts.Count > 0 ? _casts[0] : _nextJump;
        for (var i = _numJumps; i < Math.Min(_numJumps + 3, _path.Count); ++i)
        {
            var k = i - _numJumps;
            var activation = k < _casts.Count ? _casts[k] : next.AddSeconds(JumpInterval * k);
            _aoesBuffer.Add(new(Shape, _path[i], default, activation, k == 0 ? Colors.Danger : 0));
        }
        return CollectionsMarshal.AsSpan(_aoesBuffer);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.StampedeMarker)
        {
            _path.Clear();
            _path.Add(actor.Position);
            _numJumps = 0;
            _nextJump = default;
            _casts.Clear();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.StarvingStampede)
            _casts.Add(Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.StampedeMarkerMove:
                _path.Add(spell.TargetXZ);
                _nextJump = WorldState.FutureTime(7.3f + MarkerInterval * Math.Max(0, Module.Enemies((uint)OID.StampedeSpot).Count(s => !s.IsDestroyed) - _path.Count));
                break;
            case (uint)AID.StarvingStampede:
                if (_casts.Count > 0)
                    _casts.RemoveAt(0);
                _nextJump = WorldState.FutureTime(JumpInterval);
                if (++_numJumps >= _path.Count)
                {
                    _path.Clear();
                    _nextJump = default;
                    _casts.Clear();
                }
                break;
        }
    }
}

sealed class P5NProtoCarbuncleStates : StateMachineBuilder
{
    public P5NProtoCarbuncleStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RubyGlow>()
            .ActivateOnEnter<SearingRay>()
            .ActivateOnEnter<SearingRayCage>()
            .ActivateOnEnter<RubyReflection>()
            .ActivateOnEnter<TopazRay>()
            .ActivateOnEnter<TopazRayBent>()
            .ActivateOnEnter<RubyReflectionQuadrant>()
            .ActivateOnEnter<PoisonStones>()
            .ActivateOnEnter<VenomDrops>()
            .ActivateOnEnter<VenomRain>()
            .ActivateOnEnter<VenomPool>()
            .ActivateOnEnter<Crunch>()
            .ActivateOnEnter<ToxicCrunch>()
            .ActivateOnEnter<SonicHowl>()
            .ActivateOnEnter<AcidicSlaver>()
            .ActivateOnEnter<StarvingStampede>()
            .ActivateOnEnter<Impact>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Nonu (ported from upstream and modified by LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 872, NameID = 11440)]
public sealed class P5NProtoCarbuncle(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(15));
