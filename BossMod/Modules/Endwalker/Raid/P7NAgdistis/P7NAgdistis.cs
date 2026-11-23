namespace BossMod.Endwalker.Raid.P7NAgdistis;

public enum OID : uint
{
    Boss = 0x3A28, // R32.025, x1
    Helper = 0x233C, // R0.500, x22, Helper type
    ForbiddenFruit = 0x3A29, // R2.340, x3
    ForbiddenFruit1 = 0x3A2A, // R2.340, x4
    ImmatureIo = 0x3A2B, // R4.020, x3
    ImmatureStymphalide = 0x3A2C, // R3.024, x4
    Exit = 0x1E850B, // R0.500, x1, EventObj type
    Actor1EA1A1 = 0x1EA1A1, // R0.500-2.000, x1, EventObj type
    Tower = 0x1EB793, // R0.500, x4, EventObj type
}

public enum AID : uint
{
    ImmortalsObol = 30709, // Boss->self, 5.5+2.0s cast, single-target
    ImmortalsObolAOE = 30710, // Helper->self, 7.5s cast, range 20 circle, raidwide, then the arena splits into three platforms
    HemitheossGlareIII = 30711, // Boss->self, 3.0s cast, single-target
    HemitheossGlareIIIAOE = 30712, // Helper->self, 7.0s cast, range 5-30 donut
    BoughOfAttisFront = 30713, // Boss->self, 6.8+1.2s cast, single-target
    BoughOfAttisFrontAOE = 30714, // Helper->self, 8.0s cast, range 19 circle
    BoughOfAttisLeft = 30715, // Boss->self, 4.0s cast, single-target
    BoughOfAttisRight = 30716, // Boss->self, 4.0s cast, single-target
    BoughOfAttisSideAOE = 30717, // Helper->self, 5.0s cast, range 50 width 25 rect
    BoughOfAttisBack = 30718, // Boss->self, 7.2+0.8s cast, single-target
    BoughOfAttisBackAOE = 30719, // Helper->self, 8.0s cast, range 25 circle
    ForbiddenFruit = 30720, // Boss->self, 4.0s cast, single-target
    ForbiddenFruit1 = 30721, // Boss->self, 3.0s cast, single-target
    StaticMoon = 30722, // ImmatureIo->self, 5.0s cast, range 10 circle
    StymphalianStrike = 30723, // ImmatureStymphalide->self, 5.0s cast, range 60 width 8 rect
    BladesOfAttis = 30724, // Boss->self, no cast, single-target
    BladesOfAttisFirst = 30725, // Helper->self, 5.0s cast, range 7 circle
    BladesOfAttisRest = 30726, // Helper->self, no cast, range 7 circle
    HemitheossHoly = 30727, // Boss->self, 3.0s cast, single-target
    HemitheossHolyAOE = 30728, // Helper->players, 6.0s cast, range 6 circle spread
    HemitheossAeroII = 30729, // Boss->self, 4.9s cast, single-target
    HemitheossAeroIIAOE = 30730, // Helper->players, 5.4s cast, range 8 circle, tankbuster on both tanks
    SparkOfLife = 30731, // Boss->self, 5.0s cast, range 60 circle
    ShadowOfAttis = 30780, // Boss->self, no cast, single-target
    Burst = 30781, // Helper->self, no cast, range 5 circle, tower
    BigBurst = 30782, // Helper->self, no cast, raidwide, a tower nobody soaked
    HemitheossAeroIV = 30784, // Boss->self, 7.0s cast, single-target
    HemitheossAeroIVAOE = 30785, // Helper->self, 7.0s cast, range 60 circle, knockback ~20y away from a platform centre
    AutoAttack = 30947, // Boss->self, no cast, single-target
    AutoAttack1 = 30948, // Helper->player, no cast, single-target
    HemitheossGlareIIIAOELong = 31226, // Helper->self, 10.0s cast, range 5-30 donut
}

public enum IconID : uint
{
    HemitheossHoly = 311, // player
    HemitheossAeroII = 364, // player
}

sealed class ImmortalsObol(BossModule module) : Components.ProximityAOEs(module, (uint)AID.ImmortalsObolAOE, 15)
{
    private static readonly AOEShapeCircle _margin = new(16);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var c in Casters)
            hints.AddForbiddenZone(_margin, c.Origin, default, c.Activation);
    }
}

sealed class HemitheossAeroIV(BossModule module) : Components.GenericKnockback(module, (uint)AID.HemitheossAeroIVAOE, maxCasts: 1)
{
    private static readonly float[] _distances = [24, 26];
    private readonly List<Actor> _casters = [];

    private readonly List<Knockback> _kbBuffer = [];
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        _kbBuffer.Clear();
        foreach (var c in _casters)
            foreach (var d in _distances)
                _kbBuffer.Add(new(c.CastInfo!.LocXZ, d, Module.CastFinishAt(c.CastInfo)));
        return CollectionsMarshal.AsSpan(_kbBuffer);
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => false;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HemitheossAeroIVAOE)
            _casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HemitheossAeroIVAOE)
            _casters.Remove(caster);
    }
}

sealed class HemitheossGlareIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HemitheossGlareIIIAOE, new AOEShapeDonut(4, 30));
sealed class HemitheossGlareIIILong(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HemitheossGlareIIIAOELong, new AOEShapeDonut(4, 30));
sealed class BoughOfAttisFront(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BoughOfAttisFrontAOE, 19);
sealed class BoughOfAttisSide(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BoughOfAttisSideAOE, new AOEShapeRect(50, 12.5f))
{
    private static readonly AOEShapeRect _margin = new(50, 12.8f);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var c in Casters)
            hints.AddForbiddenZone(_margin, c.Origin, c.Rotation, c.Activation);
    }
}
sealed class BoughOfAttisSideKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.BoughOfAttisSideAOE, 60, shape: new AOEShapeRect(50, 12.5f), kind: Kind.DirForward);
sealed class BoughOfAttisBack(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BoughOfAttisBackAOE, 25);
sealed class StaticMoon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StaticMoon, 10);

sealed class ForbiddenFruitPreview(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _moon = new(10);
    private static readonly AOEShapeRect _line = new(60, 4);
    private const float LeadToHit = 10.5f;
    private readonly Dictionary<ulong, (WPos pos, DateTime moved, bool pending)> _state = [];
    private readonly Dictionary<uint, WPos> _parking = [];

    private readonly List<Actor> _ios = module.Enemies((uint)OID.ImmatureIo);
    private readonly List<Actor> _stymphalides = module.Enemies((uint)OID.ImmatureStymphalide);
    private readonly List<Actor> _adds = [];

    private List<Actor> Adds()
    {
        _adds.Clear();
        _adds.AddRange(_ios);
        _adds.AddRange(_stymphalides);
        return _adds;
    }

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoesBuffer.Clear();
        foreach (var a in Adds())
            if (_state.TryGetValue(a.InstanceID, out var st) && st.pending && a.CastInfo == null)
                _aoesBuffer.Add(new(a.OID == (uint)OID.ImmatureIo ? _moon : _line, a.Position, a.Rotation, st.moved.AddSeconds(LeadToHit), risky: false));
        return CollectionsMarshal.AsSpan(_aoesBuffer);
    }

    public override void Update()
    {
        foreach (var a in Adds())
        {
            if (!_parking.ContainsKey(a.OID))
                _parking[a.OID] = a.Position;
            if (!_state.TryGetValue(a.InstanceID, out var st))
                _state[a.InstanceID] = (a.Position, default, false);
            else if (!a.Position.AlmostEqual(st.pos, 1))
                _state[a.InstanceID] = (a.Position, WorldState.CurrentTime, !a.Position.AlmostEqual(_parking[a.OID], 1));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.StaticMoon or (uint)AID.StymphalianStrike && _state.TryGetValue(caster.InstanceID, out var st))
            _state[caster.InstanceID] = st with { pending = false };
    }
}
sealed class StymphalianStrike(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StymphalianStrike, new AOEShapeRect(60, 4));
sealed class HemitheossHoly(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.HemitheossHolyAOE, 6);
sealed class HemitheossAeroII(BossModule module) : Components.BaitAwayCast(module, (uint)AID.HemitheossAeroIIAOE, new AOEShapeCircle(8), true);
sealed class SparkOfLife(BossModule module) : Components.RaidwideCast(module, (uint)AID.SparkOfLife);

sealed class BladesOfAttis(BossModule module) : Components.Exaflare(module, 7)
{
    private static readonly AOEShapeCircle _margin = new(7.5f);
    private readonly List<Actor> _casters = [];

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var l in Lines)
        {
            if (l.ExplosionsLeft <= 0)
                continue;
            var pos = l.Next;
            var time = l.NextExplosion;
            for (var i = 0; i < l.ExplosionsLeft; ++i, pos += l.Advance, time = time.AddSeconds(l.TimeToMove))
                hints.AddForbiddenZone(_margin, pos, default, time);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BladesOfAttisFirst)
        {
            Lines.Add(new(caster.Position, 7f * spell.Rotation.ToDirection(), Module.CastFinishAt(spell), 1.25d, 8, 3));
            _casters.Add(caster);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.BladesOfAttisFirst or (uint)AID.BladesOfAttisRest)
        {
            var index = _casters.IndexOf(caster);
            if (index < 0)
                return;
            AdvanceLine(Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
            {
                Lines.RemoveAt(index);
                _casters.RemoveAt(index);
            }
        }
    }
}

sealed class Towers(BossModule module) : Components.GenericTowers(module, (uint)AID.Burst)
{
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Tower)
            Towers.Add(new(actor.Position, 5, 1, 2, default, WorldState.FutureTime(9.7f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.Burst or (uint)AID.BigBurst)
        {
            ++NumCasts;
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.Tower)
            Towers.RemoveAll(t => t.Position.AlmostEqual(actor.Position, 1));
    }
}

sealed class Platforms(BossModule module) : Components.GenericAOEs(module)
{
    public const float SmallRadius = 10;
    public const float SmallOffset = 16.5f;
    public const float BridgeHalfWidth = 4;

    private static readonly WPos _center = new(100f, 100f);
    private static readonly ArenaBoundsCircle _big = new(20);
    private static readonly WDir[] _centers = [SmallOffset * new WDir(0, 1), SmallOffset * 120.Degrees().ToDirection(), SmallOffset * (-120).Degrees().ToDirection()];
    private static readonly ArenaBoundsCustom _small = new(Pieces(false));
    private static readonly ArenaBoundsCustom _bridged = new(Pieces(true));
    private static readonly AOEShapeCustom _falling = new(_center, [new Polygon(_center, 20f, 128)], Pieces(false));
    private AOEInstance? _fallingAOE;

    private readonly AOEInstance[] _aoeBuffer = new AOEInstance[1];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_fallingAOE is not AOEInstance aoe)
            return [];
        _aoeBuffer[0] = aoe;
        return _aoeBuffer;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ImmortalsObolAOE)
            _fallingAOE = new(_falling, _center, default, Module.CastFinishAt(spell));
    }

    private static Shape[] Pieces(bool bridges)
    {
        var pieces = new List<Shape>(6);
        for (var i = 0; i < 3; ++i)
            pieces.Add(new Polygon(_center + _centers[i], SmallRadius, 64));
        if (bridges)
            for (var i = 0; i < 3; ++i)
                pieces.Add(new RectangleSE(_center + _centers[i], _center + _centers[(i + 1) % 3], BridgeHalfWidth));
        return [.. pieces];
    }

    private void SetArena(ArenaBounds bounds)
    {
        Arena.Bounds = bounds;
        Arena.Center = bounds is ArenaBoundsCustom custom ? custom.Center : _center;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        switch (index, state)
        {
            case (0, 0x00200010):
                SetArena(_small);
                _fallingAOE = null;
                break;
            case (0, 0x08000004):
                SetArena(_big);
                break;
            case (1 or 2 or 3, 0x00020001):
                if (Arena.Bounds == _small)
                    SetArena(_bridged);
                break;
        }
    }
}

sealed class P7NAgdistisStates : StateMachineBuilder
{
    public P7NAgdistisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ImmortalsObol>()
            .ActivateOnEnter<HemitheossGlareIII>()
            .ActivateOnEnter<HemitheossGlareIIILong>()
            .ActivateOnEnter<BoughOfAttisFront>()
            .ActivateOnEnter<BoughOfAttisSide>()
            .ActivateOnEnter<BoughOfAttisSideKnockback>()
            .ActivateOnEnter<BoughOfAttisBack>()
            .ActivateOnEnter<HemitheossAeroIV>()
            .ActivateOnEnter<StaticMoon>()
            .ActivateOnEnter<ForbiddenFruitPreview>()
            .ActivateOnEnter<StymphalianStrike>()
            .ActivateOnEnter<HemitheossHoly>()
            .ActivateOnEnter<HemitheossAeroII>()
            .ActivateOnEnter<SparkOfLife>()
            .ActivateOnEnter<BladesOfAttis>()
            .ActivateOnEnter<Towers>()
            .ActivateOnEnter<Platforms>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Nonu (ported from upstream and modified by LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 876, NameID = 11374)]
public sealed class P7NAgdistis(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
