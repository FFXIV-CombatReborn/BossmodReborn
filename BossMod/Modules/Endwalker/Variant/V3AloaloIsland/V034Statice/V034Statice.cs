namespace BossMod.Endwalker.VariantCriterion.V03Aloalo.V034Statice;

public enum OID : uint
{
    Boss = 0x4050, // R3.960, x1
    Helper = 0x233C, // R0.500, x?, Helper type
    HeavyWeight = 0x4053, // R1.000, x?, 4-tonze Weight drop markers
    ConeSlice = 0x40C6, // R1.000, x6, Trigger Happy pizza slices
    Mine = 0x4054, // R1.000, x?, Hidden Mine
    Needle = 0x4052, // R1.000, x?, Surprise Needle
    BallOfFire = 0x4055, // R2.250, x?, Pinwheel Fire Spread
    HomingPattern = 0x4051, // R1.000, x?, Dartboard homing dart
    Dart = 0x1EB931, // R0.500, EventObj type, Dartboard
    WhoopeeCushion = 0x1EB932, // R0.500, EventObj type, route 9 cushions
    SurprisingMissile = 0x405B, // R1.000, x?, route 10 Present Box
    SurprisingStaff = 0x405C, // R1.000, x?, route 10 Present Box
    SurprisingWeapon = 0x4057, // R0.450, x?, route 10 Present Box
    SurprisingWeaponAlt = 0x4059, // R0.450, x?, route 10 Present Box
    TreasureBox = 0x4056, // R3.500, x?, route 11
}

public enum AID : uint
{
    AutoAttack = 35332, // Boss->player, no cast, single-target
    Teleport = 35111, // Boss->location, no cast, single-target

    HiddenMine = 35117, // Mine->self, no cast, range 5 circle
    SurpriseBalloon = 35118, // Boss->self, 4.0s cast, single-target, spawn balloon
    Pop = 35119, // Helper->self, 5.0s cast, range 60 circle, knockback 13
    SurpriseNeedle = 35120, // Needle->self, 4.5s cast, range 40 width 2 rect

    FourTonzeWeightVisual = 35121, // Boss->self, 7.0s cast, single-target
    FourTonzeWeight = 35122, // HeavyWeight->self, 7.0s cast, range 4 circle

    TrickReload = 35114, // Boss->self, 4.0s cast, single-target
    Misload = 35110, // Boss->self, no cast, single-target, failed bullet
    LockedAndLoaded = 35109, // Boss->self, no cast, single-target, successful bullet

    TriggerHappyVisual = 35115, // Boss->self, 5.3+0.7s cast, single-target
    TriggerHappyAOE = 35116, // Helper->self, 6.0s cast, range 40 60-degree cone
    TriggerHappyFake = 35206, // Helper->self, 6.0s cast, range 40 60-degree cone

    Pinwheel = 35123, // Boss->self, 3.0s cast, single-target, spawn Balls of Fire
    FireSpreadVisual = 35201, // BallOfFire->self, 8.0s cast, single-target
    FireSpreadFirst = 35125, // Helper->self, 8.0s cast, range 12 width 5 rect (inner segment)
    FireSpreadFirstShort = 35124, // Helper->self, 8.0s cast, range 5 width 5 rect (outer segment)
    FireSpreadRest = 35319, // Helper->self, no cast, range 12 width 5 rect
    FireSpreadRestShort = 35318, // Helper->self, no cast, range 5 width 5 rect

    AeroIV = 35113, // Boss->self, 5.0s cast, range 60 circle, raidwide
    ShockingAbandon = 35112, // Boss->player, 5.0s cast, single-target, tb

    WhoopeeCushion = 35141, // Boss->self, 4.0s cast, single-target, spawn cushion EventObjs
    FairFlight = 35142, // Boss->self, 8.0s cast, range 40 circle, knock up; land on cushion
    FairFlightFail = 35317, // Helper->player, no cast, single-target, damage if not on cushion

    Dartboard = 35128, // Boss->self, 3.0s cast, single-target
    Thunderstorm = 35129, // Helper->self, 3.0s cast, range 5 circle
    Meteor = 35130, // Helper->self, no cast, range 60 circle, dartboard red (raidwide)
    BarbedLightning = 36075, // Helper->self, no cast, range ?-20 donut
    HunksOfJunk = 35132, // Helper->player, no cast, range 6 circle dartboard blue (spread)
    HappySurprise = 35133, // Boss->self, no cast, range 60 circle, dartboard green (speed boost)
    Sledgemagic = 35131, // Helper->self, 4.0s cast, range 40 circle, dartboard yellow proximity
    SurprisingWeaponAutoAttack = 35138, // SurprisingWeapon->player, no cast, single-target

    PresentBox = 35134, // Boss->self, 4.0s cast, single-target, spawn staves/missiles/weapon
    FaerieRing = 35135, // SurprisingStaff->self, 3.0s cast, range 6-12 donut
    FaerieRoad = 35136, // SurprisingStaff->self, 3.0s cast, range 45 width 8 rect
    MissileBurst = 35137, // SurprisingMissile->player, no cast, single-target, kill if missile reaches

    JackInTheBox = 35126, // Boss->self, 4.0s cast, single-target, spawn treasure coffers
    TreasureBurst = 35127, // TreasureBox->self, 1.0s cast, range 15 circle
}

public enum IconID : uint
{
    Order1 = 390, // ConeSlice
    Order2 = 391, // ConeSlice
    Order3 = 392, // ConeSlice
    Order4 = 393, // ConeSlice
    Order5 = 394, // ConeSlice
    Order6 = 395, // ConeSlice
    RotateCW = 156, // BallOfFire
    RotateCCW = 157, // BallOfFire
}

public enum TetherID : uint
{
    Follow = 17, // SurprisingMissile/SurprisingWeapon->player
}

sealed class AeroIV(BossModule module) : Components.RaidwideCast(module, (uint)AID.AeroIV);
sealed class ShockingAbandon(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ShockingAbandon);

sealed class HiddenMine(BossModule module) : Components.GenericAOEs(module, (uint)AID.HiddenMine)
{
    private bool _active = true;
    private static readonly AOEShapeCircle _shape = new(5);

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoesBuffer.Clear();
        if (_active)
            _aoesBuffer.Add(new(_shape, Module.Center));
        return CollectionsMarshal.AsSpan(_aoesBuffer);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.Mine)
            _active = false;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.HiddenMine)
            _active = false;
    }
}

sealed class SurpriseNeedle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SurpriseNeedle, new AOEShapeRect(40, 1));
sealed class FourTonzeWeight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FourTonzeWeight, 4);

sealed class SurpriseBalloon(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Pop, 13)
{
    private readonly FourTonzeWeight? _weights = module.FindComponent<FourTonzeWeight>();
    private const float StandOffset = 3f;
    private const float WeightRadius = 4.5f;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
            return;
        var activation = Casters[0].Activation;
        if (IsImmune(slot, activation))
            return;

        var origin = Casters[0].Origin;
        _weightsBuffer.Clear();
        if (_weights != null)
        {
            var aoes = _weights.ActiveAOEs(slot, actor);
            for (var i = 0; i < aoes.Length; ++i)
                _weightsBuffer.Add(aoes[i].Origin);
        }
        var dir = SafeAim(origin, CollectionsMarshal.AsSpan(_weightsBuffer));
        var stand = origin + dir * StandOffset;
        hints.GoalZones.Add(AIHints.GoalProximity(stand, 1.5f, 80));
    }

    private readonly List<WPos> _weightsBuffer = [];

    private WDir SafeAim(WPos origin, ReadOnlySpan<WPos> weights)
    {
        WDir best = new(0, 1);
        var bestScore = float.MinValue;
        for (var i = 0; i < 36; ++i)
        {
            var dir = (i * 10).Degrees().ToDirection();
            var dest = origin + dir * (StandOffset + Distance);
            if (!Arena.InBounds(dest))
                continue;
            var clear = Module.Bounds.Radius - (dest - Module.Center).Length() - 1.5f;
            foreach (var w in weights)
                clear = Math.Min(clear, (dest - w).Length() - WeightRadius);
            if (clear > bestScore)
            {
                bestScore = clear;
                best = dir;
            }
        }
        return best;
    }
}

sealed class TrickReload(BossModule module) : BossComponent(module)
{
    public BitMask SafeLoads;
    public int NumLoads { get; private set; }
    private bool _showHint;

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (!_showHint || NumLoads == 0)
            return;
        Span<char> chars = stackalloc char[Math.Max(NumLoads, 6)];
        for (var i = 0; i < chars.Length; ++i)
            chars[i] = i >= NumLoads ? '?' : SafeLoads[i] ? 'X' : 'O';
        var pattern = new string(chars);
        hints.Add($"Reload: {pattern} (X = safe)");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TrickReload)
        {
            SafeLoads = default;
            NumLoads = 0;
            _showHint = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TriggerHappyVisual)
            _showHint = false;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Misload:
                SafeLoads.Set(NumLoads++);
                break;
            case (uint)AID.LockedAndLoaded:
                ++NumLoads;
                break;
            case (uint)AID.TriggerHappyAOE:
            case (uint)AID.TriggerHappyFake:
                _showHint = false;
                break;
        }
    }
}

sealed class TriggerHappy(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TriggerHappyAOE, new AOEShapeCone(40, 30.Degrees()));

sealed class FireSpread(BossModule module) : Components.GenericAOEs(module)
{
    public struct Sequence
    {
        public WPos Pivot;
        public float Along; // distance from pivot along facing
        public Angle NextRotation;
        public int RemainingExplosions;
        public DateTime NextActivation;
        public float Length;

        public readonly WPos Origin => Pivot + Along * NextRotation.ToDirection();
    }

    public List<Sequence> Sequences = [];
    private Angle _increment;
    private const int MaxShown = 3;
    private const int TotalExplosions = 12;

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoesBuffer.Clear();
        foreach (var s in Sequences)
        {
            if (s.RemainingExplosions <= 0)
                continue;
            var shape = new AOEShapeRect(s.Length, 2.5f);
            _aoesBuffer.Add(new(shape, s.Origin, s.NextRotation, s.NextActivation, Colors.Danger));

            if (_increment == default)
                continue;
            var rot = s.NextRotation;
            var act = s.NextActivation;
            var max = Math.Min(s.RemainingExplosions, MaxShown);
            for (var i = 1; i < max; ++i)
            {
                rot += _increment;
                act = act.AddSeconds(1.1f);
                _aoesBuffer.Add(new(shape, s.Pivot + s.Along * rot.ToDirection(), rot, act));
            }
        }
        return CollectionsMarshal.AsSpan(_aoesBuffer);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (actor.OID != (uint)OID.BallOfFire)
            return;
        var rot = iconID switch
        {
            (uint)IconID.RotateCW => -10.Degrees(),
            (uint)IconID.RotateCCW => 10.Degrees(),
            _ => default
        };
        if (rot != default)
            _increment = rot;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var len = spell.Action.ID switch
        {
            (uint)AID.FireSpreadFirst => 12f,
            (uint)AID.FireSpreadFirstShort => 5f,
            _ => 0f
        };
        if (len == 0)
            return;

        var balls = Module.Enemies((uint)OID.BallOfFire).Where(b => !b.IsDead);
        var pivot = balls.Any() ? balls.MinBy(b => (b.Position - spell.LocXZ).LengthSq())!.Position : spell.LocXZ;
        // short sit on the ball, long start further along and leave a gap
        var along = len < 6 ? 0f : (spell.LocXZ - pivot).Length();

        Sequences.Add(new()
        {
            Pivot = pivot,
            Along = along,
            NextRotation = spell.Rotation,
            RemainingExplosions = TotalExplosions,
            NextActivation = Module.CastFinishAt(spell),
            Length = len
        });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var len = spell.Action.ID switch
        {
            (uint)AID.FireSpreadFirst or (uint)AID.FireSpreadRest => 12f,
            (uint)AID.FireSpreadFirstShort or (uint)AID.FireSpreadRestShort => 5f,
            _ => 0f
        };
        if (len == 0)
            return;

        ++NumCasts;
        var rot = spell.Rotation;
        var index = FindSequence(len, rot);
        if (index < 0)
            return;

        ref var s = ref Sequences.AsSpan()[index];

        if (_increment == default)
        {
            var delta = (rot - s.NextRotation).Normalized();
            if (Math.Abs(delta.Deg) is > 5 and < 20)
                _increment = delta.Deg > 0 ? 10.Degrees() : -10.Degrees();
        }

        if (--s.RemainingExplosions <= 0)
        {
            Sequences.RemoveAt(index);
            return;
        }

        s.NextRotation = _increment != default ? rot + _increment : rot;
        s.NextActivation = WorldState.FutureTime(1.1f);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID != (uint)OID.BallOfFire)
            return;
        Sequences.RemoveAll(s => s.Pivot.AlmostEqual(actor.Position, 3));
        if (!Module.Enemies((uint)OID.BallOfFire).Any(b => !b.IsDead))
            Sequences.Clear();
    }

    private int FindSequence(float len, Angle rot)
    {
        var best = -1;
        var bestDiff = 0.35f;
        for (var i = 0; i < Sequences.Count; ++i)
        {
            if (Math.Abs(Sequences[i].Length - len) > 0.1f)
                continue;
            var diff = Math.Abs((Sequences[i].NextRotation - rot).Normalized().Rad);
            if (diff < bestDiff)
            {
                bestDiff = diff;
                best = i;
            }
        }
        return best;
    }
}

sealed class Thunderstorm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Thunderstorm, 5);
sealed class Sledgemagic(BossModule module) : Components.ProximityAOEs(module, (uint)AID.Sledgemagic, 15);

sealed class WhoopeeCushion(BossModule module) : Components.GenericTowers(module, (uint)AID.FairFlight)
{
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.WhoopeeCushion)
            Towers.Add(new(actor.Position, 1.5f, maxSoakers: 4));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.WhoopeeCushion)
            Towers.RemoveAll(t => t.Position.AlmostEqual(actor.Position, 1));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.FairFlight)
            return;
        var act = Module.CastFinishAt(spell);
        for (var i = 0; i < Towers.Count; ++i)
            Towers[i] = Towers[i] with { Activation = act };
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FairFlight)
        {
            Towers.Clear();
            ++NumCasts;
        }
    }
}

sealed class FairFlight(BossModule module) : Components.RaidwideCast(module, (uint)AID.FairFlight, "Knock up — land on a cushion!");

sealed class FaerieRing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FaerieRing, new AOEShapeDonut(6, 12));
sealed class FaerieRoad(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FaerieRoad, new AOEShapeRect(45, 4));

sealed class SurprisingMissile(BossModule module) : Components.GenericAOEs(module)
{
    private const float ForwardExtend = 2f;

    private readonly List<Actor> _missiles = [];

    // note: returns a shared buffer, which is overwritten by the next access
    private List<Actor> Missiles
    {
        get
        {
            _missiles.Clear();
            foreach (var m in Module.Enemies((uint)OID.SurprisingMissile))
                if (!m.IsDead)
                    _missiles.Add(m);
            return _missiles;
        }
    }

    private static float Radius(Actor m) => m.HitboxRadius + 0.5f;

    private Actor? TetherTarget(Actor missile)
    {
        if (missile.Tether.Target != 0 && WorldState.Actors.Find(missile.Tether.Target) is { } t && t.Type is ActorType.Player or ActorType.Buddy)
            return t;
        foreach (var p in Raid.WithoutSlot(true))
            if (p.Tether.Target == missile.InstanceID)
                return p;
        return null;
    }

    private static WDir TravelDir(Actor m, Actor target)
        => (target.Position - m.Position).LengthSq() > 0.01f ? (target.Position - m.Position).Normalized() : m.Rotation.ToDirection();

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoesBuffer.Clear();
        foreach (var m in Missiles)
        {
            if (TetherTarget(m) is not { } target)
                continue;
            var r = Radius(m);
            var dir = TravelDir(m, target);
            // add a forward rect to account for ping
            _aoesBuffer.Add(new(new AOEShapeRect(ForwardExtend + r, r, r), m.Position, Angle.FromDirection(dir)));
        }
        return CollectionsMarshal.AsSpan(_aoesBuffer);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var m in Missiles)
        {
            if (TetherTarget(m) is not { } target)
                continue;
            var r = Radius(m);
            var dir = TravelDir(m, target);
            if (actor.Position.InRect(m.Position, dir, ForwardExtend + r, r, r))
            {
                hints.Add("Away from missile!");
                return;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var m in Missiles)
        {
            if (TetherTarget(m) is not { } target)
                continue;
            var r = Radius(m);
            var dir = TravelDir(m, target);
            hints.AddForbiddenZone(new SDCapsule(m.Position, dir, ForwardExtend, r));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var m in Missiles)
        {
            if (TetherTarget(m) is not { } target)
                continue;
            Arena.Actor(m, Colors.Danger, true);
            Arena.AddLine(m.Position, target.Position, Colors.Danger);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.MissileBurst)
            ++NumCasts;
    }
}

sealed class SurprisingWeapon(BossModule module) : Components.GenericAOEs(module)
{
    private const float KeepAway = 3f;
    private const float ForwardExtend = 2f;

    private static bool IsWeapon(uint oid) => oid is (uint)OID.SurprisingWeapon or (uint)OID.SurprisingWeaponAlt;

    private readonly List<Actor> _weapons = [];

    // note: returns a shared buffer, which is overwritten by the next access
    private List<Actor> Weapons
    {
        get
        {
            _weapons.Clear();
            foreach (var w in Module.Enemies((uint)OID.SurprisingWeapon))
                if (!w.IsDead)
                    _weapons.Add(w);
            foreach (var w in Module.Enemies((uint)OID.SurprisingWeaponAlt))
                if (!w.IsDead)
                    _weapons.Add(w);
            return _weapons;
        }
    }

    private Actor? TetherTarget(Actor weapon)
    {
        if (weapon.Tether.Target != 0 && WorldState.Actors.Find(weapon.Tether.Target) is { } t && t.Type is ActorType.Player or ActorType.Buddy)
            return t;
        foreach (var p in Raid.WithoutSlot(true))
            if (p.Tether.Target == weapon.InstanceID)
                return p;
        return null;
    }

    private static WDir TravelDir(Actor w, Actor target)
        => (target.Position - w.Position).LengthSq() > 0.01f ? (target.Position - w.Position).Normalized() : w.Rotation.ToDirection();

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoesBuffer.Clear();
        foreach (var w in Weapons)
        {
            if (TetherTarget(w) is not { } target)
                continue;
            var dir = TravelDir(w, target);
            _aoesBuffer.Add(new(new AOEShapeRect(ForwardExtend + KeepAway, KeepAway, KeepAway), w.Position, Angle.FromDirection(dir)));
        }
        return CollectionsMarshal.AsSpan(_aoesBuffer);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var w in Weapons)
        {
            if (TetherTarget(w) is not { } target || target != actor)
                continue;
            var dir = TravelDir(w, target);
            if (actor.Position.InRect(w.Position, dir, ForwardExtend + KeepAway, KeepAway, KeepAway))
            {
                hints.Add("Away from surprising weapon!");
                return;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var w in Weapons)
        {
            if (TetherTarget(w) is not { } target)
                continue;
            if (target != actor)
                continue;
            var dir = TravelDir(w, target);
            hints.AddForbiddenZone(new SDCapsule(w.Position, dir, ForwardExtend, KeepAway));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var w in Weapons)
        {
            if (TetherTarget(w) is not { } target)
                continue;
            Arena.Actor(w, Colors.Object, true);
            Arena.AddLine(w.Position, target.Position, target == pc ? Colors.Danger : Colors.Safe);
        }
    }
}

sealed class JackInTheBox(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor Caster, DateTime Activation)> _aoes = [];
    private static readonly AOEShapeCircle _shape = new(15);

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoesBuffer.Clear();
        var count = _aoes.Count;
        for (var i = 0; i < count; ++i)
            _aoesBuffer.Add(new(_shape, _aoes[i].Caster.Position, default, _aoes[i].Activation));
        return CollectionsMarshal.AsSpan(_aoesBuffer);
    }

    public override void Update()
    {
        foreach (var b in Module.Enemies((uint)OID.TreasureBox))
            if (b.PosRot != b.PrevPosRot)
                Mark(b);
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.TreasureBox)
            Mark(actor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.TreasureBurst)
            return;
        var i = _aoes.FindIndex(a => a.Caster == caster);
        if (i >= 0)
            _aoes[i] = (caster, Module.CastFinishAt(spell));
        else
            _aoes.Add((caster, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TreasureBurst)
            _aoes.RemoveAll(a => a.Caster == caster);
    }

    private void Mark(Actor box)
    {
        if (_aoes.Any(a => a.Caster == box))
            return;
        _aoes.Add((box, WorldState.CurrentTime));
    }
}

sealed class V034StaticeStates : StateMachineBuilder
{
    public V034StaticeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AeroIV>()
            .ActivateOnEnter<ShockingAbandon>()
            .ActivateOnEnter<HiddenMine>()
            .ActivateOnEnter<FourTonzeWeight>()
            .ActivateOnEnter<SurpriseBalloon>()
            .ActivateOnEnter<SurpriseNeedle>()
            .ActivateOnEnter<TrickReload>()
            .ActivateOnEnter<TriggerHappy>()
            .ActivateOnEnter<FireSpread>()
            .ActivateOnEnter<Thunderstorm>()
            .ActivateOnEnter<Sledgemagic>()
            .ActivateOnEnter<WhoopeeCushion>()
            .ActivateOnEnter<FairFlight>()
            .ActivateOnEnter<FaerieRing>()
            .ActivateOnEnter<FaerieRoad>()
            .ActivateOnEnter<SurprisingMissile>()
            .ActivateOnEnter<SurprisingWeapon>()
            .ActivateOnEnter<JackInTheBox>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 961, NameID = 12506, Contributors = "croizat, ported from upstream")]
public sealed class V034Statice(WorldState ws, Actor primary) : BossModule(ws, primary, new(650, -833), new ArenaBoundsCircle(20));
