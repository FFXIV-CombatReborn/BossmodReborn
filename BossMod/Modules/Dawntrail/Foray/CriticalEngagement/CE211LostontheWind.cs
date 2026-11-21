
namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE211LostontheWind;

public enum OID : uint
{
    Abductor = 0x4BE1, // R5.004
    Plume = 0x4BE3, // R1.000, x0 (spawn during fight)
    BuffetWind = 0x1EBFA9, // R0.500, x0 (spawn during fight), EventObj type
    BitingWind = 0x4BE2, // R1.000, x0 (spawn during fight)
    Deathwall = 0x4BE4, // R1.000, x1
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 47434, // Abductor->player, no cast, single-target
    Deathwall = 47435, // Deathwall->self, no cast, range 24-30 donut
    Teleport = 47433, // Abductor->location, no cast, single-target

    WindBlade = 47441, // Abductor->self, 5.0s cast, range 60 180-degree cone
    CyclonicRingTeleport = 47447, // Abductor->location, no cast, single-target
    CyclonicRing = 47449, // Helper->self, 5.5s cast, range 5-60 donut
    PlumefallTrap = 47442, // Abductor->self, 3.0s cast, single-target
    Splinter = 47443, // Plume->self, 4.5s cast, range 13 circle
    SkydiveTeleport = 47446, // Abductor->location, no cast, single-target
    Skydive = 47448, // Helper->self, 5.5s cast, range 15 circle
    HurricaneVisual = 47436, // Abductor->self, 5.0s cast, single-target
    Hurricane = 48120, // Helper->self, no cast, ???
    AerosnareCast = 47444, // Abductor->self, 3.5+0.5s cast, single-target
    Aerosnare = 47445, // Helper->self, 4.0s cast, range 60 60-degree cone
    BuffetVisual = 48250, // Helper->self, 4.0s cast, range 60 width 60 rect
    Buffet = 47440, // Helper->self, no cast, ???

    StrongWind = 47437, // Helper->self, no cast, range 4 circle
    TendonRipperVisual = 47438, // BitingWind->self, 1.0s cast, single-target
    TendonRipper = 47439 // Helper->self, 1.0s cast, range 60 width 8 cross
}

public enum IconID : uint
{
    BitingWindAOE = 506, // BitingWind->self
}

sealed class WindBlade(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WindBlade, new AOEShapeCone(60f, 90f.Degrees()));
sealed class CyclonicRing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CyclonicRing, new AOEShapeDonut(5f, 60f));
sealed class Splinter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Splinter, 13f);
sealed class Skydive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Skydive, 15f);
sealed class Hurricane(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.HurricaneVisual, (uint)AID.Hurricane, 0.9d);
sealed class Aerosnare(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Aerosnare, new AOEShapeCone(60f, 30f.Degrees()), 3);

sealed class Buffet(BossModule module) : Components.GenericKnockback(module)
{
    private readonly List<Knockback> knockbacks = [];
    private readonly BitingWind _aoe = module.FindComponent<BitingWind>()!;
    public bool Active;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.BuffetWind)
        {
            if (state == 0x00010002u)
            {
                var delay = NumCasts == 0 ? 11.7d : NumCasts == 3 ? 12.7d : 14.7d;
                knockbacks.Add(new(actor.Position, 24f, WorldState.FutureTime(delay), direction: actor.Rotation, kind: Kind.DirForward));
            }
            else if (state == 0x00100020u)
            {
                Active = true;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Buffet)
        {
            knockbacks.Clear();
            ++NumCasts;
            Active = false;
        }
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(knockbacks);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (knockbacks.Count == 0)
        {
            return;
        }

        ref readonly var k = ref knockbacks.Ref(0);

        var activation = k.Activation;
        if (!IsImmune(slot, activation))
        {
            var aoes = _aoe.AOEs;
            var len = aoes.Length;
            var circleArcs = new Components.GenericAOEs.AOEInstance[len];
            var circleArcsCount = 0;

            for (var i = 0; i < len; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.Shape is AOEShapeArcCapsule arcCapsule)
                {
                    var distance = (aoe.Origin - arcCapsule.OrbitCenter).Length();
                    var angle = (2.0f / distance).Radians();
                    var direction = arcCapsule.AngularLength + (arcCapsule.AngularLength.Rad < 0f ? -angle : angle);
                    var aoeInstance = aoe;
                    aoeInstance.Shape = new AOEShapeArcCapsule(arcCapsule.Radius + 2f, direction, arcCapsule.OrbitCenter);
                    circleArcs[circleArcsCount++] = aoeInstance;
                }
            }

            hints.AddForbiddenZone(new SDKnockbackInCircleAwayFromOriginMixedAOEs(Arena.Center, k.Origin, 24f, 23f, circleArcs, circleArcsCount), activation);
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe!.AOEs;
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            if (aoes[i].Check(pos))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }
}

sealed class BitingWind : Components.GenericAOEs
{
    public BitingWind(BossModule module) : base(module)
    {
        bitingWinds = module.Enemies((uint)OID.BitingWind);
        var a30 = 30f.Degrees();
        arcCWnear = new(4f, a30, arenaCenter);
        arcCCWnear = new(4f, -a30, arenaCenter);
        var a20 = 20f.Degrees();
        arcCWfar = new(4f, a20, arenaCenter);
        arcCCWfar = new(4f, -a20, arenaCenter);
    }
    private readonly WPos arenaCenter = new(-150f, -860f); // different from quantized donut death wall
    private readonly List<Actor> bitingWinds;
    private readonly AOEShapeArcCapsule arcCWnear, arcCCWnear, arcCWfar, arcCCWfar;
    public AOEInstance[] AOEs = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void Update()
    {
        var count = bitingWinds.Count;
        if (count == 0 && AOEs.Length != 0)
        {
            AOEs = [];
            return;
        }
        AOEs = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var wind = bitingWinds[i];
            var pos = wind.Position;
            var rot = wind.Rotation;
            var dir = pos - arenaCenter;
            var ccw = rot.ToDirection().OrthoR().Dot(dir) < 0f;
            const float lengthSq = 16f * 16f;
            var isClose = (pos - arenaCenter).LengthSq() < lengthSq;
            AOEShapeArcCapsule shape;
            if (isClose)
            {
                shape = ccw ? arcCCWnear : arcCWnear;
            }
            else
            {
                shape = ccw ? arcCCWfar : arcCWfar;
            }
            AOEs[i] = new(shape, pos.Quantized(), color: Colors.Danger);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = bitingWinds.Count;
        if (count == 0)
        {
            return;
        }
        var forbiddenNearFuture = WorldState.FutureTime(1.1d);
        var forbiddenSoon = WorldState.FutureTime(3d);
        var forbiddenFarFuture = DateTime.MaxValue;
        var center = arenaCenter;
        var a15 = 15f.Degrees();
        var a25 = 25f.Degrees();
        var a35 = 35f.Degrees();
        for (var i = 0; i < count; ++i)
        {
            var vz = bitingWinds[i];
            var pos = vz.Position;
            var dir = pos - center;
            var ccw = vz.Rotation.ToDirection().OrthoR().Dot(dir) < 0f;
            var mult = ccw ? -1f : 1f;
            {
                hints.AddForbiddenZone(new SDArcCapsule(pos, center, mult * a15, 4f), forbiddenNearFuture);
                hints.AddForbiddenZone(new SDArcCapsule(pos, center, mult * a25, 4f), forbiddenSoon);
                hints.AddForbiddenZone(new SDArcCapsule(pos, center, mult * a35, 4f), forbiddenFarFuture);
            }
            hints.TemporaryObstacles.Add(new SDCircle(pos.Quantized(), 5f));
        }
    }
}

sealed class TendronRipper(BossModule module) : Components.GenericAOEs(module)
{
    public AOEInstance[] _aoes = [];
    private readonly AOEShapeCross crossPredict = new(60f, 4.2f), crossReal = new(60f, 4f); // Slightly bigger since the predicted aoes can be like a pixel off
    private const float innerCircleAngle = 35f;
    private const float outerCircleAngle = 40f;
    private readonly Buffet buffetWind = module.FindComponent<Buffet>()!;
    private readonly WPos arenaCenter = new(-150f, -860f); // different from quantized donut death wall

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.BitingWindAOE)
        {
            var circleDistance = actor.Position - arenaCenter;
            var circleDirection = actor.Rotation.ToDirection();
            var direction = circleDistance.Cross(circleDirection) > 0f;
            const float lengthSq = 16f * 16f;
            var length = circleDistance.LengthSq() > lengthSq ? outerCircleAngle : innerCircleAngle; // check which circle (outer or inner)
            // predicted position is not exact (error bigger than quantization error), probably due to using the interpolated actor.Position
            var predictedPosition = WPos.RotateAroundOrigin(direction ? length : -length, arenaCenter, actor.Position);
            var activation = WorldState.FutureTime(5.1d);
            _aoes = new AOEInstance[2];
            _aoes[0] = new(crossPredict, predictedPosition, -180f.Degrees(), activation, shapeDistance: crossPredict.Distance(predictedPosition, default));
            var rot = -135f.Degrees();
            _aoes[1] = new(crossPredict, predictedPosition, -135f.Degrees(), activation, shapeDistance: crossPredict.Distance(predictedPosition, rot));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TendonRipper)
        {
            var count = _aoes.Length;
            var rot = spell.Rotation;
            var pos = spell.LocXZ;
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref _aoes[i];
                if (aoe.Rotation.AlmostEqual(rot, Angle.DegToRad))
                {
                    aoe.Rotation = rot;
                    aoe.Origin = pos;
                    aoe.Shape = crossReal;
                    aoe.ShapeDistance = crossReal.Distance(pos, rot);
                    break;
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TendonRipper)
        {
            _aoes = [];
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoes.Length == 0 || buffetWind.Active)
        {
            return;
        }
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

sealed class CE211LostontheWindStates : StateMachineBuilder
{
    public CE211LostontheWindStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WindBlade>()
            .ActivateOnEnter<CyclonicRing>()
            .ActivateOnEnter<Splinter>()
            .ActivateOnEnter<Skydive>()
            .ActivateOnEnter<Hurricane>()
            .ActivateOnEnter<Aerosnare>()
            .ActivateOnEnter<BitingWind>()
            .ActivateOnEnter<Buffet>()
            .ActivateOnEnter<TendronRipper>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.Abductor, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 1093u, NameID = 61u)]
public sealed class CE211LostontheWind(WorldState ws, Actor primary) : BossModule(ws, primary, new WPos(-150f, -860f).Quantized(), new ArenaBoundsCircle(24f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player() is Actor player && player.Position.InCircle(Arena.Center, 24f);
}
