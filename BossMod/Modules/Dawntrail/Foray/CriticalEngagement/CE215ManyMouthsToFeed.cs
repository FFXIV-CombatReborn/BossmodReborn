using BossMod.Dawntrail.Foray.CriticalEngagement;

namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE215ManyMouthsToFeed;

public enum OID : uint
{
    Boss = 0x4BCA, // R7.0, BNpcName 14747, 提蔛 (Many Mouths to Feed)
    BossClone = 0x4BCC, // R0.5, BNpcName 14747, spits venom during flood phase
    VenomPuddle = 0x4BCD, // R2.0, BNpcName 108, persistent venom voidzone
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 0xC6A2, // boss->player, no cast, single-target

    CentralWhipVisual = 0xB872, // boss->self, 4.7s cast, visual for CentralWhip
    SideWhipVisual = 0xB873, // boss->self, 4.7s cast, visual for SideWhip
    CentralWhip = 0xB874, // helper, 5.7s cast, 中央鞭打, through-body line (52y long, 10y wide)
    SideWhip = 0xB875, // helper, 5.7s cast, 侧方鞭打, one 135-degree cone (26y)
    SideWhip2 = 0xC241, // helper, 5.7s cast, 侧方鞭打, opposite 135-degree cone (26y)

    PollenScatter = 0xB876, // boss->self, 3.7s cast, 花粉飞散, visual precursor for Predation
    Predation = 0xB877, // boss, 6.7s cast, 捕食, 10y circle
    PoisonMistVisualA = 0xB87A, // boss->self, 4.7s cast, visual for PoisonMist
    PoisonMistVisualB = 0xB87B, // boss->self, 4.7s cast, visual for PoisonMist
    PoisonMist = 0xB87C, // helper, 5.7s cast, 毒雾喷射, 90-degree cone (30y)
    PoisonMist2 = 0xC573, // helper, 5.7s cast, 毒雾喷射, 90-degree cone (30y)
    PoisonMist3 = 0xC574, // helper, 5.7s cast, 毒雾喷射, 90-degree cone (30y)
    PoisonMist4 = 0xC575, // helper, 5.7s cast, 毒雾喷射, 90-degree cone (30y)

    VenomBlobVisual = 0xB87D, // boss->self, 3.7s cast, visual for VenomBlob
    VenomBlob = 0xB87E, // helper, 2.7s cast, 毒液块, 5y circle scattered puddles

    SecreteVenom = 0xC242, // boss->self, 2.7s cast, 分泌毒液, visual precursor for Venom
    Venom = 0xB870, // helper, 4.8s cast, 毒液, 2y circle (initial cardinal puddles)
    VenomSpread = 0xB871, // four fixed helpers, no cast, staggered spreading poison (geometry not safely established)

    PoisonRainVisual = 0xB87F, // boss->self, 4.7s cast, 毒雨, raidwide visual
    PoisonRain = 0xB880, // helper, no cast, 毒雨, raidwide damage

    SpitVenom = 0xB86E, // clone (0x4BCC), no cast, persistent outer venom boundary
    SecreteVenomVisualA = 0xB86F, // boss->self, no cast, 分泌毒液 visual
    SecreteVenomVisualB = 0xC2DD // boss->self, no cast, 分泌毒液 visual
}

// B86E has no cast packet, but repeats for the entire encounter and kills targets in the outer
// band. ARR hit positions start just outside 24.5y and the Action sheet gives a 30y outer radius.
sealed class VenomBoundary(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut Shape = new(24.5f, 30f);
    private readonly AOEInstance[] _aoe = [new(Shape, module.Arena.Center)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;
}

// Cast rotations are replay-verified: each helper cast already carries the packet rotation pointing
// at its own slice, so we register every slice as an independent AOE off the caster position.
sealed class ManyMouthsAOEs(BossModule module) : ReplayValidatedCastAOEs(module)
{
    // 52y long / 10y wide line, centered on the caster (extends front and back).
    private static readonly AOEShapeRect CentralLine = new(26f, 5f, 26f);
    // Side whip is a pair of opposing 135-degree cones; the safe gap is the narrow front/back sliver.
    // Replay-verified: B875 hits span +18..+161 deg from facing (C241 the mirrored -138..-16),
    // i.e. a 135-degree cone - the official sheet's fan180 Omen is not representative here.
    private static readonly AOEShapeCone Whip = new(26f, 67.5f.Degrees());
    // Poison mist fills 3 of the 4 quadrants with 90-degree cones (45-degree half-angle).
    private static readonly AOEShapeCone Mist = new(30f, 45f.Degrees());
    private static readonly AOEShapeCircle Predation = new(10f);
    private static readonly AOEShapeCircle Blob = new(5f);
    private static readonly AOEShapeCircle VenomCircle = new(2f);

    protected override AOEConfig? ConfigFor(uint actionID) => actionID switch
    {
        (uint)AID.CentralWhip => new(CentralLine),
        (uint)AID.SideWhip or (uint)AID.SideWhip2 => new(Whip),
        (uint)AID.PoisonMist or (uint)AID.PoisonMist2 or (uint)AID.PoisonMist3 or (uint)AID.PoisonMist4 => new(Mist),
        (uint)AID.Predation => new(Predation),
        (uint)AID.VenomBlob => new(Blob),
        (uint)AID.Venom => new(VenomCircle),
        _ => null
    };
}

// Persistent venom puddles (0x4BCD) are initially created at (0, 0), then moved to their real
// cardinal positions. Ignore the uninitialized spawn coordinates so they cannot create a bogus
// off-arena forbidden zone. These live actors are the later persistent puddles near the arena rim;
// they are separate from the four fixed helpers used by VenomSpread.
sealed class VenomPuddles(BossModule module) : Components.Voidzone(module, 2f,
    static module => module.Enemies((uint)OID.VenomPuddle).Where(actor => !actor.IsDeadOrDestroyed && actor.Position.InCircle(module.Arena.Center, 30f)));

// Each pair of cardinal helpers starts with a 2y Venom cast, then emits eight B871 pulses at
// ~1.07s intervals. Replay hit distances establish an expanding circle: the first pulse reaches
// about 5y and each subsequent pulse grows by roughly 2.5y. Predict the first spread from the
// visible B870 cast instead of waiting until players have already been hit.
sealed class VenomSpread(BossModule module) : Components.GenericAOEs(module)
{
    private sealed class Spread(ulong actorID, WPos origin, DateTime activation)
    {
        public readonly ulong ActorID = actorID;
        public readonly WPos Origin = origin;
        public DateTime Activation = activation;
        public int Pulse;
    }

    private const int PulseCount = 8;
    private const float InitialRadius = 5f;
    private const float RadiusStep = 2.5f;
    private const float FinalRadius = InitialRadius + RadiusStep * (PulseCount - 1);
    private const double FirstDelayAfterVenom = 2.45d;
    private const double PulseInterval = 1.07d;
    private readonly List<Spread> _spreads = [with(4)];
    private readonly List<AOEInstance> _displayed = [with(4)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        Prune();
        _displayed.Clear();
        foreach (var spread in _spreads)
        {
            var shape = new AOEShapeCircle(InitialRadius + RadiusStep * spread.Pulse);
            _displayed.Add(new(shape, spread.Origin, activation: spread.Activation,
                actorID: spread.ActorID, shapeDistance: shape.Distance(spread.Origin, default)));
        }
        return CollectionsMarshal.AsSpan(_displayed);
    }

    public override void Update() => Prune();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var spread in _spreads)
        {
            if (spread.Pulse >= PulseCount - 1)
                continue;

            var finalActivation = spread.Activation.AddSeconds((PulseCount - 1 - spread.Pulse) * PulseInterval);
            hints.AddForbiddenZone(new SDCircle(spread.Origin, FinalRadius), finalActivation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.Venom || spell.EventHappened)
            return;

        var activation = Module.CastFinishAt(spell, (float)FirstDelayAfterVenom);
        _spreads.RemoveAll(spread => spread.ActorID == caster.InstanceID);
        _spreads.Add(new(caster.InstanceID, caster.Position, activation));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.VenomSpread)
            return;

        var spread = _spreads.FirstOrDefault(entry => entry.ActorID == caster.InstanceID);
        if (spread == null)
        {
            // Mid-mechanic activation/replay recovery: the event source is the actual circle center.
            spread = new(caster.InstanceID, caster.Position, WorldState.FutureTime(PulseInterval)) { Pulse = 1 };
            _spreads.Add(spread);
        }
        else if (++spread.Pulse >= PulseCount)
        {
            _spreads.Remove(spread);
            ++NumCasts;
            return;
        }
        else
        {
            spread.Activation = WorldState.FutureTime(PulseInterval);
        }
        ++NumCasts;
    }

    private void Prune()
    {
        var now = WorldState.CurrentTime;
        _spreads.RemoveAll(spread => now > spread.Activation.AddSeconds(2d));
    }
}

sealed class PoisonRain(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.PoisonRainVisual]);

sealed class ManyMouthsToFeedStates : StateMachineBuilder
{
    public ManyMouthsToFeedStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VenomBoundary>()
            .ActivateOnEnter<ManyMouthsAOEs>()
            .ActivateOnEnter<VenomPuddles>()
            .ActivateOnEnter<VenomSpread>()
            .ActivateOnEnter<PoisonRain>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(ManyMouthsToFeedStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    PrimaryActorOID = (uint)OID.Boss,
    Contributors = "KanoNoUta",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CriticalEngagement,
    GroupID = 1093u,
    NameID = 49u,
    SortOrder = 14)]
public sealed class ManyMouthsToFeed(WorldState ws, Actor primary) : BossModule(ws, primary, new(-870f, -560f), new ArenaBoundsCircle(30f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.BossClone));
    }
}
