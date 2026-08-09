namespace BossMod.Dawntrail.Foray.FATE.InconstantGardener;

public enum OID : uint {
    Iambe = 0x4C41,
    Helper = 0x233C,
    Iambe1 = 0x4C42, // R1.000, x0 (spawn during fight)
    WinsomeSeed = 0x4C43, // R0.240-0.528, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 50855, // Iambe->player, no cast, single-target
    DirectSeeding = 48029, // Iambe->self, 3.0s cast, single-target
    GardenersHymnCast = 48031, // Iambe->self, 2.5s cast, single-target
    GardenersHymn = 48032, // 4C42->location, 6.0s cast, range 5 circle
    Burst = 48033, // 4C43->self, 2.0s cast, range 15 circle
    OdeOfTheUnderfoot = 48037, // Iambe->self, 5.0s cast, range 10 circle
    IambicMarch = 48035, // Iambe->self, 3.0s cast, range 40 circle
}

public enum SID : uint {
    ForwardMarch = 5142, // Iambe->player, extra=0x0
    AboutFace = 5143, // Iambe->player, extra=0x0
    ForcedMarch = 1257, // Iambe->player, extra=0x1/0x2
    Gen = 5106, // 4C42->4C43, extra=0x1
    Gen1 = 5107, // 4C42->4C43, extra=0x1
}

sealed class GardenersHymn(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GardenersHymn, new AOEShapeCircle(5.0f));
sealed class OdeOfTheUnderfoot(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OdeOfTheUnderfoot, new AOEShapeCircle(10.0f));
sealed class IambicMarch(BossModule module) : Components.StatusDrivenForcedMarch(module, 3.0f, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)default,
// The march direction follows the player's facing, so automation must pre-aim: a forward march
// towards the boss lands inside the OdeOfTheUnderfoot circle and the seed bursts. Replay shows the
// AI marched east into the 10y circle and ate the hit, so mark any position whose forced-march
// destination is inside those zones as forbidden, forcing it to turn/relocate before the march.
    (uint)default, (uint)SID.ForcedMarch)
{
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (base.DestinationUnsafe(slot, actor, pos))
            return true;

        foreach (var component in Module.Components)
        {
            if (component is Burst burst)
            {
                foreach (ref readonly var aoe in burst.ActiveAOEs(slot, actor))
                    if (aoe.Check(pos))
                        return true;
            }
            if (component is OdeOfTheUnderfoot ode)
            {
                foreach (ref readonly var aoe in ode.ActiveAOEs(slot, actor))
                    if (aoe.Check(pos))
                        return true;
            }
        }
        return false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var movements = ForcedMovements(actor);
        if (movements.Count == 0)
            return;

        // The march destination depends on facing. A forbidden circle at the current position only
        // makes the pathfinder move somewhere else while it keeps facing the boss. Score candidate
        // destinations by the facing they create, so movement itself pre-aims the upcoming march.
        if (State.TryGetValue(actor.InstanceID, out var state) && state.ForcedEnd <= WorldState.CurrentTime && state.PendingMoves.Count != 0)
        {
            hints.GoalZones.Add(position => CandidateMarchSafe(slot, actor, position, state) ? 20f : 0f);
        }

        if (DestinationUnsafe(slot, actor, movements[^1].to))
            hints.AddForbiddenZone(new SDCircle(actor.Position, 1.5f), WorldState.FutureTime(10d));
    }

    private bool CandidateMarchSafe(int slot, Actor actor, WPos position, PlayerState state)
    {
        var travel = position - actor.Position;
        var direction = travel.LengthSq() > 0.01f ? Angle.FromDirection(travel) : actor.Rotation;
        var destination = position;
        foreach (var move in state.PendingMoves)
        {
            direction += move.dir;
            destination += MovementSpeed * move.duration * direction.ToDirection();
        }
        return !DestinationUnsafe(slot, actor, destination);
    }
}

// Gardeners' Hymn identifies the four seeds that will explode about 3.5s after its cast resolves.
// Keep that long advance warning, then replace its estimated activation with the authoritative
// Burst cast finish when the selected seed starts its own cast.
sealed class Burst(BossModule module) : Components.GenericAOEs(module) {
    private static readonly AOEShapeCircle Shape = new(15f);
    private readonly List<AOEInstance> _aoes = [];
    private readonly List<Actor> _seeds = [];
    private readonly HashSet<uint> _seenGlobalSequences = [];

    public override void OnActorCreated(Actor actor) {
        if (actor.OID == (uint)OID.WinsomeSeed) {
            _seeds.Add(actor);
        }
    }

    public override void OnActorDestroyed(Actor actor) {
        if (actor.OID == (uint)OID.WinsomeSeed) {
            _seeds.Remove(actor);
            _aoes.RemoveAll(aoe => aoe.ActorID == actor.InstanceID);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.EventHappened)
            return;

        if (spell.Action.ID == (uint)AID.GardenersHymn) {
            var activation = Module.CastFinishAt(spell).AddSeconds(3.5d);
            foreach (var seed in _seeds) {
                if (caster.Position.AlmostEqual(seed.Position, 0.5f)) {
                    _aoes.RemoveAll(aoe => aoe.ActorID == seed.InstanceID);
                    _aoes.Add(new(Shape, seed.Position, activation: activation, actorID: seed.InstanceID,
                        shapeDistance: Shape.Distance(seed.Position, default)));
                }
            }
        }
        else if (spell.Action.ID == (uint)AID.Burst) {
            var activation = Module.CastFinishAt(spell);
            _aoes.RemoveAll(aoe => aoe.ActorID == caster.InstanceID || aoe.Origin.AlmostEqual(caster.Position, 0.5f));
            _aoes.Add(new(Shape, caster.Position, activation: activation, actorID: caster.InstanceID,
                shapeDistance: Shape.Distance(caster.Position, default)));
        }

        _aoes.Sort((left, right) => left.Activation.CompareTo(right.Activation));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID != (uint)AID.Burst
            || spell.GlobalSequence != 0 && !_seenGlobalSequences.Add(spell.GlobalSequence))
            return;

        _aoes.RemoveAll(aoe => aoe.ActorID == caster.InstanceID || aoe.Origin.AlmostEqual(caster.Position, 0.5f));
        ++NumCasts;
    }

    public override void OnActorDeath(Actor actor) => _aoes.RemoveAll(aoe => aoe.ActorID == actor.InstanceID);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);
}

[SkipLocalsInit]
sealed class InconstantGardenerStates : StateMachineBuilder {
    public InconstantGardenerStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<GardenersHymn>()
            .ActivateOnEnter<OdeOfTheUnderfoot>()
            .ActivateOnEnter<IambicMarch>()
            .ActivateOnEnter<Burst>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(InconstantGardenerStates),
    ConfigType = null, // replace null with typeof(IambeConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.Iambe,
    Contributors = "KanoNoUta",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.ForayFATE,
    GroupID = 1093u,
    NameID = 2079u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class InconstantGardener(WorldState ws, Actor primary) : OpenWorldFate(ws, primary);
