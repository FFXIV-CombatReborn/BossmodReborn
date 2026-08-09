namespace BossMod.Dawntrail.Foray.FATE.NH111RegnantChimera;

public enum OID : uint
{
    RegnantChimera = 0x4C7D, // R5.180
    FulmipotentOrb = 0x4C7F,
    IceOrb = 0x4C80, // ice orb, spawns during the fight and emits 12y ice-roar circles
    ChaoticNoise = 0x4B71,
}

public enum AID : uint
{
    AutoAttack = 50856, // RegnantChimera->player, no cast, single-target
    DragonsBreathFirst = 48629, // RegnantChimera->self, 6.0s cast, range 30 120-degree cone; first of three clockwise hits
    DragonsBreathSecond = 48630, // RegnantChimera->self, no cast, range 30 120-degree cone; second hit, 120 degrees clockwise
    RamsVoice = 48633, // RegnantChimera->self, 4.0s cast, range 9 circle
    DragonsVoice = 48634, // RegnantChimera->self, 4.0s cast, range 8-30 donut
    DragonsVoiceOrb = 48636, // FulmipotentOrb->self, 4.0s cast, range 8-30 donut
    DragonsBreathThird = 49747, // RegnantChimera->self, no cast, range 30 120-degree cone; third hit, 240 degrees clockwise
    IceBreathFirst = 48631, // RegnantChimera->self, 6.0s cast, range 30 120-degree cone; first of three counterclockwise hits
    IceBreathSecond = 48632, // RegnantChimera->self, no cast, second hit, 120 degrees counterclockwise
    IceBreathThird = 49748, // RegnantChimera->self, no cast, third hit, 240 degrees counterclockwise
    IceRoar = 48635, // IceOrb->self, 1.0s cast, range 12 circle
    LeftDuobreath = 50111, // RegnantChimera->self, 5.0s cast, range 40 180-degree cone; left then right (dragon first)
    RightDuobreath = 50112, // RegnantChimera->self, 5.0s cast, range 40 180-degree cone; right then left (ram first)
    Cacophony = 50113, // RegnantChimera->self, 4.0s cast, single-target
    ChaoticChorus = 50114, // ChaoticNoise->self, 1.5s cast, range 6 circle
    DuobreathDragonFollowup = 50115, // RegnantChimera->self, no cast, range 40 180-degree cone; follow-up to RightDuobreath
    DuobreathRamFollowup = 50116, // RegnantChimera->self, no cast, range 40 180-degree cone; follow-up to LeftDuobreath
}

// ARR records a fully deterministic clockwise sequence: the cast resolves at t=0, followed by
// BDF6 at +2.709s and C253 at +5.446s. The packet rotations advance by -120 degrees each time.
sealed class DragonsBreathSequence(BossModule module) : Components.GenericAOEs(module)
{
    private readonly record struct Pending(uint ActionID, AOEInstance AOE);

    private static readonly AOEShapeCone Shape = new(30f, 60f.Degrees());
    private static readonly Angle Step = 120f.Degrees();
    private readonly List<Pending> _pending = [with(3)];
    private readonly List<AOEInstance> _displayed = [with(2)];
    private readonly HashSet<uint> _seenGlobalSequences = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        PruneExpired();
        _displayed.Clear();
        var count = Math.Min(_pending.Count, 2);
        for (var i = 0; i < count; ++i)
        {
            var aoe = _pending[i].AOE;
            aoe.Risky = i == 0;
            aoe.Color = i == 0 ? Colors.Danger : Colors.AOE;
            _displayed.Add(aoe);
        }
        return CollectionsMarshal.AsSpan(_displayed);
    }

    public override void Update() => PruneExpired();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.DragonsBreathFirst || spell.EventHappened)
            return;

        var firstActivation = Module.CastFinishAt(spell);
        if (firstActivation <= WorldState.CurrentTime)
            return;

        _pending.Clear();
        Add(AID.DragonsBreathFirst, caster, spell.Rotation, firstActivation);
        Add(AID.DragonsBreathSecond, caster, spell.Rotation - Step, firstActivation.AddSeconds(2.709d));
        Add(AID.DragonsBreathThird, caster, spell.Rotation - 2f * Step, firstActivation.AddSeconds(5.446d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is not ((uint)AID.DragonsBreathFirst or (uint)AID.DragonsBreathSecond or (uint)AID.DragonsBreathThird)
            || spell.GlobalSequence != 0 && !_seenGlobalSequences.Add(spell.GlobalSequence))
            return;

        ++NumCasts;
        _pending.RemoveAll(entry => entry.ActionID == spell.Action.ID && entry.AOE.ActorID == caster.InstanceID);
        PruneExpired();
    }

    public override void OnActorDeath(Actor actor) => RemoveActor(actor.InstanceID);
    public override void OnActorDestroyed(Actor actor) => RemoveActor(actor.InstanceID);

    private void Add(AID action, Actor caster, Angle rotation, DateTime activation)
        => _pending.Add(new((uint)action, new(Shape, caster.Position, rotation, activation, actorID: caster.InstanceID, shapeDistance: Shape.Distance(caster.Position, rotation))));

    private void PruneExpired()
        => _pending.RemoveAll(entry => WorldState.CurrentTime > entry.AOE.Activation.AddSeconds(0.75d));

    private void RemoveActor(ulong actorID)
        => _pending.RemoveAll(entry => entry.AOE.ActorID == actorID);
}

// The first cast covers one half of the arena and the no-cast follow-up covers the opposite half
// 3.175s later. The full C3BF -> C3C4 sequence is replay-verified: the first packet hit the western
// group and the follow-up hit the eastern group. C3C0 -> C3C3 is the mirrored action pair. Show
// both for planning, but only let the currently resolving half steer automation.
sealed class Duobreath(BossModule module) : Components.GenericAOEs(module)
{
    private readonly record struct Pending(uint ActionID, AOEInstance AOE);

    private static readonly AOEShapeCone Shape = new(40f, 90f.Degrees());
    private static readonly Angle Opposite = 180f.Degrees();
    private const double FollowupDelay = 3.175d;
    private readonly List<Pending> _pending = [with(2)];
    private readonly List<AOEInstance> _displayed = [with(2)];
    private readonly HashSet<uint> _seenGlobalSequences = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        PruneExpired();
        _displayed.Clear();
        for (var i = 0; i < _pending.Count; ++i)
        {
            var aoe = _pending[i].AOE;
            aoe.Risky = i == 0;
            aoe.Color = i == 0 ? Colors.Danger : Colors.AOE;
            _displayed.Add(aoe);
        }
        return CollectionsMarshal.AsSpan(_displayed);
    }

    public override void Update() => PruneExpired();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is not ((uint)AID.LeftDuobreath or (uint)AID.RightDuobreath) || spell.EventHappened)
            return;

        var firstActivation = Module.CastFinishAt(spell);
        if (firstActivation <= WorldState.CurrentTime)
            return;

        var followup = spell.Action.ID == (uint)AID.LeftDuobreath ? AID.DuobreathRamFollowup : AID.DuobreathDragonFollowup;
        _pending.RemoveAll(entry => entry.AOE.ActorID == caster.InstanceID);
        Add(spell.Action.ID, caster, spell.Rotation, firstActivation);
        Add((uint)followup, caster, spell.Rotation + Opposite, firstActivation.AddSeconds(FollowupDelay));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is not ((uint)AID.LeftDuobreath or (uint)AID.RightDuobreath
            or (uint)AID.DuobreathDragonFollowup or (uint)AID.DuobreathRamFollowup)
            || spell.GlobalSequence != 0 && !_seenGlobalSequences.Add(spell.GlobalSequence))
            return;

        _pending.RemoveAll(entry => entry.ActionID == spell.Action.ID && entry.AOE.ActorID == caster.InstanceID);
        ++NumCasts;
        PruneExpired();
    }

    public override void OnActorDeath(Actor actor) => RemoveActor(actor.InstanceID);
    public override void OnActorDestroyed(Actor actor) => RemoveActor(actor.InstanceID);

    private void Add(uint actionID, Actor caster, Angle rotation, DateTime activation)
        => _pending.Add(new(actionID, new(Shape, caster.Position, rotation, activation, actorID: caster.InstanceID,
            shapeDistance: Shape.Distance(caster.Position, rotation))));

    private void PruneExpired()
        => _pending.RemoveAll(entry => WorldState.CurrentTime > entry.AOE.Activation.AddSeconds(0.75d));

    private void RemoveActor(ulong actorID)
        => _pending.RemoveAll(entry => entry.AOE.ActorID == actorID);
}
// Mirrors the thunder breath, but the ice version rotates counterclockwise (+120 degrees per step,
// replay-verified: first hit 135->180 packet facing, second 180+120, third 180+240) with the same
// 2.72s/5.49s cadence.
sealed class IceBreathSequence(BossModule module) : Components.GenericAOEs(module)
{
    private readonly record struct Pending(uint ActionID, AOEInstance AOE);

    private static readonly AOEShapeCone Shape = new(30f, 60f.Degrees());
    private static readonly Angle Step = 120f.Degrees();
    private readonly List<Pending> _pending = [with(3)];
    private readonly List<AOEInstance> _displayed = [with(2)];
    private readonly HashSet<uint> _seenGlobalSequences = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        PruneExpired();
        _displayed.Clear();
        var count = Math.Min(_pending.Count, 2);
        for (var i = 0; i < count; ++i)
        {
            var aoe = _pending[i].AOE;
            aoe.Risky = i == 0;
            aoe.Color = i == 0 ? Colors.Danger : Colors.AOE;
            _displayed.Add(aoe);
        }
        return CollectionsMarshal.AsSpan(_displayed);
    }

    public override void Update() => PruneExpired();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.IceBreathFirst || spell.EventHappened)
            return;

        var firstActivation = Module.CastFinishAt(spell);
        if (firstActivation <= WorldState.CurrentTime)
            return;

        _pending.Clear();
        Add(AID.IceBreathFirst, caster, spell.Rotation, firstActivation);
        Add(AID.IceBreathSecond, caster, spell.Rotation + Step, firstActivation.AddSeconds(2.72d));
        Add(AID.IceBreathThird, caster, spell.Rotation + 2f * Step, firstActivation.AddSeconds(5.49d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is not ((uint)AID.IceBreathFirst or (uint)AID.IceBreathSecond or (uint)AID.IceBreathThird)
            || spell.GlobalSequence != 0 && !_seenGlobalSequences.Add(spell.GlobalSequence))
            return;

        ++NumCasts;
        _pending.RemoveAll(entry => entry.ActionID == spell.Action.ID && entry.AOE.ActorID == caster.InstanceID);
        PruneExpired();
    }

    public override void OnActorDeath(Actor actor) => RemoveActor(actor.InstanceID);
    public override void OnActorDestroyed(Actor actor) => RemoveActor(actor.InstanceID);

    private void Add(AID action, Actor caster, Angle rotation, DateTime activation)
        => _pending.Add(new((uint)action, new(Shape, caster.Position, rotation, activation, actorID: caster.InstanceID, shapeDistance: Shape.Distance(caster.Position, rotation))));

    private void PruneExpired()
        => _pending.RemoveAll(entry => WorldState.CurrentTime > entry.AOE.Activation.AddSeconds(0.75d));

    private void RemoveActor(ulong actorID)
        => _pending.RemoveAll(entry => entry.AOE.ActorID == actorID);
}

// 小冰钢铁: IceOrb 的 IceRoar 只有 0.7-1.0s 读条, SimpleAOEs 显示太晚 (<1s 走位)。
// 改为从 IceOrb 实体实时画 12y 圈, 球一出现就提前显示。
sealed class IceRoar(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Shape = new(12f);
    private readonly List<AOEInstance> _displayed = [with(8)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _displayed.Clear();
        foreach (var orb in Module.Enemies((uint)OID.IceOrb))
            if (!orb.IsDeadOrDestroyed)
                _displayed.Add(new(Shape, orb.Position, color: Colors.Danger, actorID: orb.InstanceID,
                    shapeDistance: Shape.Distance(orb.Position, default)));
        return CollectionsMarshal.AsSpan(_displayed);
    }
}
sealed class ChaoticChorus(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ChaoticChorus, new AOEShapeCircle(6f));
// Ice orbs begin their 0.7s casts roughly one second after Ram's Voice resolves. Retain the center
// circle briefly so navigation does not immediately run back in, then reverse course as the first
// orb warning appears. During that overlap the boss circle and first orb circles are solved as one
// safe-region problem.
sealed class RamsVoice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RamsVoice, new AOEShapeCircle(9f))
{
    private const double HoldAfterResolve = 2.25d;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        // Deliberately retained through the first IceRoar activation; Update clears it below.
    }

    public override void Update()
        => Casters.RemoveAll(aoe => WorldState.CurrentTime > aoe.Activation.AddSeconds(HoldAfterResolve));
}
sealed class DragonsVoice(BossModule module) : Components.SimpleAOEGroups(module,
    [(uint)AID.DragonsVoice, (uint)AID.DragonsVoiceOrb], new AOEShapeDonut(8f, 30f));

[SkipLocalsInit]
sealed class RegnantChimeraStates : StateMachineBuilder
{
    public RegnantChimeraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DragonsBreathSequence>()
            .ActivateOnEnter<IceBreathSequence>()
            .ActivateOnEnter<Duobreath>()
            .ActivateOnEnter<IceRoar>()
            .ActivateOnEnter<ChaoticChorus>()
            .ActivateOnEnter<RamsVoice>()
            .ActivateOnEnter<DragonsVoice>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(RegnantChimeraStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    PrimaryActorOID = (uint)OID.RegnantChimera,
    Contributors = "KanoNoUta",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.ForayFATE,
    GroupID = 1093u,
    NameID = 2076u,
    SortOrder = 1)]
[SkipLocalsInit]
public sealed class RegnantChimera(WorldState ws, Actor primary) : OpenWorldFate(ws, primary);
