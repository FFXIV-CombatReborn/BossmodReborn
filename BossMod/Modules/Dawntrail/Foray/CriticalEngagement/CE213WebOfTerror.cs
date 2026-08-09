using BossMod.Dawntrail.Foray.CriticalEngagement;

namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE213WebOfTerror;

public enum OID : uint
{
    Boss = 0x4DFA, // R6.5, BNpcName 14840, crescent arachne
    Daughter = 0x4DFB, // R2.4, BNpcName 14841, daughter of arachne
    BoundaryController = 0x4DFC, // R1.0, non-targetable arena controller
    Helper = 0x233C
}

public enum AID : uint
{
    LethalBoundary = 0xC4BD, // controller, repeated persistent 20-30y outer deathwall pulse
    ImplosionVisual = 0xC4BE, // boss->self, 5.0s cast, raidwide visual
    ImplosionHit = 0xC4BF, // helpers->players, no cast, raidwide damage
    Summon = 0xC4C0, // boss->self, 3.0s cast, summons daughters
    ArachnidWebStart = 0xC4C1, // boss->daughter, 3.0s cast, visual/link start
    ArachnidWebLink = 0xC4C2, // daughter->daughter, no cast, visual/link propagation
    ArachnidFunnel = 0xC4C3, // boss->location, 5.0s cast, charge width 20
    ArachnidFunnelContinue = 0xC4C4, // boss->location, no cast, subsequent charge width 20
    VenomEruption = 0xC4C7, // daughter->self, 12.0s cast, lethal raidwide if daughter survives
    ConformityBoss = 0xC4C8, // boss->self, 3.0s cast, range 50 45-degree cone
    ConformityDaughter = 0xC4C9, // daughter->self, 3.0s cast, range 50 45-degree cone
    BedrockUpliftVisual = 0xC4CA, // boss->self, 4.7s cast, visual
    BedrockUpliftCircle = 0xC4CB, // helpers->self, 5.0s cast, range 10 circle
    BedrockUpliftMiddle = 0xC4CC, // helpers->self, 7.0s cast, range 10-20 donut
    BedrockUpliftOuter = 0xC4CD, // helpers->self, 9.0s cast, range 20-30 donut
    DaughterAutoAttack = 0xC5CB, // daughter->player, no cast, single-target
    QueensOrders = 0xC5D7, // boss->self, 3.0s cast, orders daughter Conformity casts
    ArachnidFunnelAftershock = 0xC5F8, // helper->location, no cast, charge width 20
    AutoAttack = 0xC6A5 // boss->player, no cast, single-target
}

sealed class LethalBoundary(BossModule module) : Components.GenericAOEs(module)
{
    // Death points cluster at 20.9-28.1y from center, so the lethal band is the 20-30 ring. The
    // arena bounds are r20, which would clip a donut starting exactly at 20 into an invisible
    // sliver; pull the inner edge slightly inside the arena so the fence is actually visible.
    private static readonly AOEShapeDonut Shape = new(19.5f, 30f);
    private readonly AOEInstance[] _aoe = [new(Shape, module.Arena.Center)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        // The 19.5-30 donut is clipped by the r20 arena into an invisible sliver; draw a visible
        // yellow 19-20 band plus the fence outline so the kill ring reads clearly.
        Arena.ZoneDonut(Arena.Center, 19f, 20f, Colors.Danger);
        Arena.ZoneCircleOutlineUnclipped(Arena.Center, 20f, Colors.Danger, 3f);
    }
}

// The spider web: the boss casts C4C1 on a daughter, then the link propagates daughter to
// daughter (C4C2). Replay exposes it as real tether events (IDs 0x1A4 boss->daughter, 0x198
// daughter->daughter); draw the live lines so players can see the web structure while the
// daughters walk and before the funnel charge follows the web.
sealed class ArachnidWeb(BossModule module) : BossComponent(module)
{
    private readonly List<(Actor Source, Actor Target)> _links = [];
    private readonly HashSet<ulong> _seenSources = [];

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID is not (0x1A4u or 0x198u))
            return;
        if (WorldState.Actors.Find(tether.Target) is not { } target)
            return;

        var sourceID = source.InstanceID;
        var targetID = tether.Target;
        _links.RemoveAll(l => l.Source.InstanceID == sourceID && l.Target.InstanceID == targetID
            || l.Source.InstanceID == targetID && l.Target.InstanceID == sourceID);
        _links.Add((source, target));
        _seenSources.Add(source.InstanceID);
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        var sourceID = source.InstanceID;
        var targetID = tether.Target;
        _links.RemoveAll(l => l.Source.InstanceID == sourceID && l.Target.InstanceID == targetID
            || l.Source.InstanceID == targetID && l.Target.InstanceID == sourceID);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        _links.RemoveAll(l => l.Source.InstanceID == actor.InstanceID || l.Target.InstanceID == actor.InstanceID);
        _seenSources.Remove(actor.InstanceID);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (source, target) in _links)
            if (!source.IsDeadOrDestroyed && !target.IsDeadOrDestroyed)
                Arena.AddLine(source.Position, target.Position, Colors.Danger);
    }
}

sealed class Conformity(BossModule module) : ReplayValidatedCastAOEs(module)
{
    private static readonly AOEShapeCone Shape = new(50f, 22.5f.Degrees());

    protected override AOEConfig? ConfigFor(uint actionID) => actionID is (uint)AID.ConformityBoss or (uint)AID.ConformityDaughter ? new(Shape) : null;
}

// The first funnel follows a moving daughter. Re-evaluate the target's live position while the
// cast is active; the recorded LocXZ is the daughter's position at cast start and can be stale by
// resolution. Each instant C4C4 charge is followed about 0.65s later by a C5F8 hit on the same
// lane, which gives a short but authoritative warning for the second pulse.
sealed class ArachnidFunnel(BossModule module) : Components.GenericAOEs(module)
{
    private const float HalfWidth = 10f;
    private const double AftershockDelay = 0.65d;
    private const double ExpireDelay = 0.5d;

    private sealed record CastCharge(ulong CasterID, ulong TargetID, WPos FallbackDestination, DateTime Activation);

    private CastCharge? _cast;
    private readonly List<AOEInstance> _aftershocks = [with(2)];
    private readonly List<AOEInstance> _displayed = [with(3)];
    private readonly HashSet<uint> _seenGlobalSequences = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        PruneExpired();
        _displayed.Clear();

        if (_cast is { } cast && WorldState.Actors.Find(cast.CasterID) is { } caster)
        {
            var destination = WorldState.Actors.Find(cast.TargetID)?.Position ?? cast.FallbackDestination;
            AddDynamicAOE(_displayed, caster.Position, destination, cast.Activation, cast.CasterID);
        }
        _displayed.AddRange(_aftershocks);
        return CollectionsMarshal.AsSpan(_displayed);
    }

    public override void Update() => PruneExpired();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.ArachnidFunnel || spell.EventHappened)
        {
            return;
        }

        var activation = Module.CastFinishAt(spell);
        if (activation > WorldState.CurrentTime)
        {
            _cast = new(caster.InstanceID, spell.TargetID, spell.LocXZ, activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ArachnidFunnel && _cast?.CasterID == caster.InstanceID)
        {
            _cast = null;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.GlobalSequence != 0 && !_seenGlobalSequences.Add(spell.GlobalSequence))
        {
            return;
        }

        switch (spell.Action.ID)
        {
            case (uint)AID.ArachnidFunnel:
                if (_cast?.CasterID == caster.InstanceID)
                {
                    _cast = null;
                }
                ++NumCasts;
                break;
            case (uint)AID.ArachnidFunnelContinue:
                AddDynamicAOE(_aftershocks, caster.Position, spell.TargetXZ, WorldState.FutureTime(AftershockDelay), caster.InstanceID);
                ++NumCasts;
                break;
            case (uint)AID.ArachnidFunnelAftershock:
                if (_aftershocks.Count != 0)
                {
                    _aftershocks.RemoveAt(0);
                }
                ++NumCasts;
                break;
        }
    }

    public override void OnActorDeath(Actor actor) => RemoveActor(actor.InstanceID);
    public override void OnActorDestroyed(Actor actor) => RemoveActor(actor.InstanceID);

    private static void AddDynamicAOE(List<AOEInstance> destination, WPos origin, WPos target, DateTime activation, ulong actorID)
    {
        var direction = target - origin;
        if (direction.LengthSq() < 0.01f)
        {
            return;
        }

        var shape = new AOEShapeRect(direction.Length(), HalfWidth);
        var rotation = Angle.FromDirection(direction);
        destination.Add(new(shape, origin, rotation, activation, actorID: actorID, shapeDistance: shape.Distance(origin, rotation)));
    }

    private void PruneExpired()
    {
        var now = WorldState.CurrentTime;
        if (_cast is { } cast && now > cast.Activation.AddSeconds(ExpireDelay))
        {
            _cast = null;
        }
        _aftershocks.RemoveAll(aoe => now > aoe.Activation.AddSeconds(ExpireDelay));
    }

    private void RemoveActor(ulong actorID)
    {
        if (_cast?.CasterID == actorID)
        {
            _cast = null;
        }
        _aftershocks.RemoveAll(aoe => aoe.ActorID == actorID);
    }
}

// Two origins execute the 0-10, 10-20 and 20-30 waves at two-second intervals. Draw the entire
// upcoming sequence, but only make the currently resolving pair risky so automation never treats
// all three concentric regions as simultaneously forbidden.
sealed class BedrockUplift(BossModule module) : ReplayValidatedCastAOEs(module)
{
    private static readonly AOEShapeCircle Circle = new(10f);
    private static readonly AOEShapeDonut Middle = new(10f, 20f);
    private static readonly AOEShapeDonut Outer = new(20f, 30f);

    protected override int MaxDisplayed => 6;
    protected override double RiskyActivationWindow => 0.25d;

    protected override AOEConfig? ConfigFor(uint actionID) => actionID switch
    {
        (uint)AID.BedrockUpliftCircle => new(Circle),
        (uint)AID.BedrockUpliftMiddle => new(Middle),
        (uint)AID.BedrockUpliftOuter => new(Outer),
        _ => null
    };
}

// Each helper carries a different subset of players, so use the boss visual once per raidwide.
sealed class Implosion(BossModule module) : Components.RaidwideCast(module, (uint)AID.ImplosionVisual);

// The daughters normally die before this finishes. If one survives, the cast is a raidwide enrage;
// keeping it as predicted damage also makes automation prioritize the already-drawn adds.
sealed class VenomEruption(BossModule module) : Components.RaidwideCast(module, (uint)AID.VenomEruption);
sealed class Daughters(BossModule module) : Components.Adds(module, (uint)OID.Daughter, 1);

sealed class WebOfTerrorStates : StateMachineBuilder
{
    public WebOfTerrorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LethalBoundary>()
            .ActivateOnEnter<ArachnidWeb>()
            .ActivateOnEnter<Conformity>()
            .ActivateOnEnter<ArachnidFunnel>()
            .ActivateOnEnter<BedrockUplift>()
            .ActivateOnEnter<Implosion>()
            .ActivateOnEnter<VenomEruption>()
            .ActivateOnEnter<Daughters>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(WebOfTerrorStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    PrimaryActorOID = (uint)OID.Boss,
    Contributors = "KanoNoUta",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CriticalEngagement,
    GroupID = 1093u,
    NameID = 55u,
    SortOrder = 12)]
public sealed class WebOfTerror(WorldState ws, Actor primary) : BossModule(ws, primary, new(170f, -136f), new ArenaBoundsCircle(20f));
