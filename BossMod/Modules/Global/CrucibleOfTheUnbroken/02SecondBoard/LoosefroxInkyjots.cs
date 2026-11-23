namespace BossMod.Global.CrucibleOfTheUnbroken.SecondBoard.LoosefroxInkyjots;

public enum OID : uint
{
    LoosefroxInkyjots = 0x4C65,
    ChewchumPopoto = 0x4C66, // R6.105, x1
    Quicksand = 0x1EC026, // R0.500, x6, EventObj type
    GobbieBombBig = 0x4C67, // R1.200, x0 (spawn during fight)
    GobbieBombSmall = 0x4C69, // R0.600, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 50784, // LoosefroxInkyjots/4C66->player, no cast, single-target

    Burrow = 48224, // 4C66->self, no cast, single-target
    SandPillar = 48225, // Helper->location, no cast, range 4 circle
    Teleport = 50451, // Helper->location, no cast, single-target
    SandBreathVisual = 48226, // 4C66->self, 7.8+1.2s cast, single-target
    SandBreath = 48227, // Helper->self, 9.0s cast, range 60 90.000-degree cone
    GobspinHeadlopsVisual = 48228, // LoosefroxInkyjots->self, 5.3+0.7s cast, single-target
    GobspinHeadlops = 48229, // Helper->self, 6.0s cast, range 8 circle
    Kinborrow = 48242, // LoosefroxInkyjots->self, 4.0s cast, single-target
    GobbieboomBarrage1 = 50446, // LoosefroxInkyjots->self, 3.0s cast, single-target
    GobbieboomBarrage2 = 48234, // LoosefroxInkyjots->self, 3.0s cast, single-target

    ExplosionReal = 48235, // 4C69->self, 2.0s cast, range 12 circle
    ExplosionDrowned = 48238, // Helper->self, no cast, single-target
    BombTossVisual = 48518, // LoosefroxInkyjots->location, 3.0+1.0s cast, single-target
    BombToss = 48519, // Helper->location, 4.0s cast, range 6 circle

    BombAttract = 50452, // Helper->location, no cast, range 6 circle
    HitByBomb1 = 48520, // ChewchumPopoto->self, no cast, single-target
    HitByBomb2 = 50444, // ChewchumPopoto->self, no cast, single-target

    GobspinHeadlopsDonutVisual = 48230, // LoosefroxInkyjots->self, 5.3+0.7s cast, single-target
    GobspinHeadlopsDonut = 48231, // Helper->self, 6.0s cast, range 4-40 donut
    ExplosionCross = 48236, // 4C67->self, 2.0s cast, range 40 width 8 cross
    GoblinHammerVisual = 48232, // LoosefroxInkyjots->self, 3.0s cast, single-target
    GoblinHammer = 48233, // Helper->self, 5.0s cast, range 8 circle
    Earthquake = 48239, // ChewchumPopoto->self, 5.0s cast, range 60 circle
    EarthquakeRest = 48240, // ChewchumPopoto->self, no cast, range 60 circle
    GoblinRush = 48517, // LoosefroxInkyjots->player, 5.0s cast, single-target
}

public enum SID : uint
{
    Blind = 5381, // Helper->player, extra=0x0
    PopotoSkin = 5423, // LoosefroxInkyjots->LoosefroxInkyjots, extra=0x0
    SixFulmsUnder = 567, // none->player, extra=0x522
}

public enum IconID : uint
{
    Exclamation_8s_x = 569, // Helper->self
}

sealed class Quicksand(BossModule module) : BossComponent(module)
{
    private const float Radius = 6f;
    private readonly List<Actor> quicksands = module.Enemies((uint)OID.Quicksand);

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var count = quicksands.Count;
        for (var i = 0; i < count; ++i)
        {
            Arena.ZoneCircle(quicksands[i].Position, Radius, Colors.AOE);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.FindStatus((uint)SID.SixFulmsUnder) != null)
        {
            hints.Add("Leave the quicksand!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var drown = actor.FindStatus((uint)SID.SixFulmsUnder)?.ExpireAt ?? DateTime.MaxValue;
        var count = quicksands.Count;
        for (var i = 0; i < count; ++i)
        {
            hints.AddForbiddenZone(new SDCircle(quicksands[i].Position, Radius), drown);
        }
    }
}
sealed class SandBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SandBreath, new AOEShapeCone(60f, 45f.Degrees()));
sealed class GobspinHeadlops(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GobspinHeadlops, 8f);
sealed class BombToss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BombToss, 6f);
sealed class GobspinHeadlopsDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GobspinHeadlopsDonut, new AOEShapeDonut(4f, 40f));
sealed class GoblinHammer(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GoblinHammer, 8f);
sealed class GobbieBombs(BossModule module) : Components.AddsMulti(module, [(uint)OID.GobbieBombBig, (uint)OID.GobbieBombSmall], AIHints.Enemy.PriorityUndesirable);
sealed class Earthquake(BossModule module) : Components.RaidwideCast(module, (uint)AID.Earthquake);
sealed class Kinborrow(BossModule module) : Components.Dispel(module, (uint)SID.PopotoSkin, (uint)AID.Kinborrow);
sealed class GoblinRush(BossModule module) : Components.SingleTargetCast(module, (uint)AID.GoblinRush);
sealed class Explosion(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(4)];
    private readonly AOEShapeCircle circle = new(12f);
    private readonly List<Actor> _bombs = module.Enemies((uint)OID.GobbieBombSmall);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    // TODO: probably shouldn't be considered risky until shortly before cast starts, since you need to knock the bomb onto the worm somehow...
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ExplosionReal)
        {
            var count = _aoes.Count;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var id = caster.InstanceID;
            var loc = spell.LocXZ;
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (id == aoe.ActorID)
                {
                    aoe.Origin = loc;
                    aoe.ShapeDistance = circle.Distance(loc, default);
                    aoe.Activation = Module.CastFinishAt(spell);
                    return;
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ExplosionReal)
        {
            var count = _aoes.Count;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (id == aoe.ActorID)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.GobbieBombSmall && id == 0x11E4) // bomb got knocked into a sand pit and will no longer explode
        {
            var count = _aoes.Count;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var iid = actor.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (iid == aoe.ActorID)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.GobbieBombSmall && animState1 == 1)
        {
            var origin = actor.Position.Quantized();
            _aoes.Add(new(circle, origin, default, WorldState.CurrentTime.AddSeconds(7d), actorID: actor.InstanceID, shapeDistance: circle.Distance(origin, default)));
        }
    }

    public override void Update()
    {
        var count = _bombs.Count;
        if (count == 0)
        {
            return;
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var len = aoes.Length;
        for (var i = 0; i < count; ++i)
        {
            var b = _bombs[i];
            if (b.LastFrameMovement != default)
            {
                var id = b.InstanceID;
                for (var j = 0; j < len; ++j)
                {
                    ref var aoe = ref aoes[j];
                    if (id == aoe.ActorID)
                    {
                        var pos = b.Position;
                        aoe.Origin = pos;
                        aoe.ShapeDistance = circle.Distance(pos, default);
                    }
                }
            }
        }
    }
}

sealed class ExplosionCross(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ExplosionCross, new AOEShapeCross(40f, 4f))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell) { }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.GobbieBombBig && animState1 == 1)
        {
            var origin = actor.Position.Quantized();
            var rotation = actor.Rotation;
            var activation = WorldState.CurrentTime.AddSeconds(6d);
            Casters.Add(new(Shape, origin, rotation, activation, actorID: actor.InstanceID, shapeDistance: Shape.Distance(origin, rotation)));
        }
    }
}

sealed class LoosefroxInkyjotsStates : StateMachineBuilder
{
    public LoosefroxInkyjotsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Kinborrow>()
            .ActivateOnEnter<GoblinRush>()
            .ActivateOnEnter<Quicksand>()
            .ActivateOnEnter<SandBreath>()
            .ActivateOnEnter<GobspinHeadlops>()
            .ActivateOnEnter<BombToss>()
            .ActivateOnEnter<GobspinHeadlopsDonut>()
            .ActivateOnEnter<GoblinHammer>()
            .ActivateOnEnter<GobbieBombs>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<ExplosionCross>()
            .ActivateOnEnter<Earthquake>()
            .Raw.Update = () => AllDeadOrDestroyed(LoosefroxInkyjots.Bosses);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.LoosefroxInkyjots, Contributors = "gynorhino", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1089u, NameID = 14561u, SortOrder = 7)]
[PreferredKinship(BST.Kinship.Wave, "Remove Popoto Skin from Kinborrow")]
[PreferredKinship(BST.Kinship.Ash, "Cleanse Blind from Sand Breath")]
public sealed class LoosefroxInkyjots(WorldState ws, Actor primary) : BossModule(ws, primary, new(520f, -420f), new ArenaBoundsCircle(22f))
{
    public static readonly uint[] Bosses = [(uint)OID.LoosefroxInkyjots, (uint)OID.ChewchumPopoto];
    private Actor? _loosefrox;
    private Actor? _chewchum;

    public Actor? Loosefrox() => _loosefrox;
    public Actor? Chewchum() => _chewchum;

    protected override void UpdatePreModuleActivation()
    {
        _loosefrox ??= GetActor((uint)OID.LoosefroxInkyjots);
        _chewchum ??= GetActor((uint)OID.ChewchumPopoto);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(_loosefrox);
        Arena.Actor(_chewchum);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            if (e.Actor.OID == (uint)OID.ChewchumPopoto)
            {
                if (!_loosefrox?.IsDead ?? false)
                {
                    e.Priority = AIHints.Enemy.PriorityUndesirable;
                }
                break;
            }
        }
    }
}

