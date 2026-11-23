namespace BossMod.Global.CrucibleOfTheUnbroken.SecondMasterBoard.DrakePiece;

public enum OID : uint
{
    DrakePiece = 0x4CF0,
    BallOfFire = 0x4CF1, // R1.500, x0 (spawn during fight)
    TwistingBlaze = 0x4CF3, // R1.000, x0 (spawn during fight)
    BarbmolePiece = 0x4CF2, // R1.750, x0 (spawn during fight)
    MorphoPiece = 0x4CF5, // R0.800, x0 (spawn during fight)
    AbaddonPiece = 0x4CF4, // R4.800, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 49681, // DrakePiece->player, no cast, single-target
    Teleport = 49257, // DrakePiece->location, no cast, single-target

    BlazeSpikes = 49253, // DrakePiece->self, 3.0s cast, single-target
    ArmOfPurgatory = 49258, // BallOfFire->self, 1.0s cast, range 4 circle
    BlowingRingOfFire = 49259, // TwistingBlaze->self, 5.0s cast, range 3-50 donut
    BurningCycloneBoss = 49255, // DrakePiece->self, 4.0+1.0s cast, single-target
    BurningCyclone = 49256, // Helper->self, 5.0s cast, range 50 120.000-degree cone
    ManglingFang = 49254, // DrakePiece->player, 5.0s cast, single-target

    // BarbmolePiece
    AutoAttackBarbmolePiece = 50397, // BarbmolePiece->player, no cast, single-target
    NeedlesOut = 49250, // BarbmolePiece->self, 3.0s cast, single-target
    SeedingNeedles = 49252, // BarbmolePiece->self, 5.0s cast, range 20 circle

    // MorphoPiece
    UnwittingWings = 49260, // MorphoPiece->player, 4.0s cast, single-target
    DelusionDust = 49261, // MorphoPiece->self, no cast, range 50 circle

    // AbaddonPiece
    AutoAttackAbaddonPiece = 49682, // AbaddonPiece->player, no cast, single-target
}

public enum SID : uint
{
    BlazeSpikes = 5465, // DrakePiece->DrakePiece, extra=0x64
    NeedlesOut = 5145, // 4CF2->4CF2, extra=0x64
    Bleeding = 4068, // 4CF2->player, extra=0x1/0x2
    WitsEnd = 5146, // 4CF5->player, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7/0x8/0x9/0xA
    Confused = 1283, // 4CF5->player, extra=0x0
    Gen = 3572, // 4CF4->4CF5, extra=0x12
    Rehabilitation = 989, // none->AbaddonPiece, extra=0x5/0xB/0x10
}

public enum IconID : uint
{
    ManglingFangTankBuster = 218, // player->self
}

public enum TetherID : uint
{
    MorphoPieceTether = 17, // 4CF5->player
}

sealed class ManglingFang(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ManglingFang);
sealed class SeedingNeedles(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SeedingNeedles, 20f);

sealed class BlazeSpikes(BossModule module) : Components.Dispel(module, (uint)SID.BlazeSpikes, (uint)AID.BlazeSpikes);

sealed class BlazeSpikesTarget(BossModule module) : Components.Adds(module, (uint)OID.DrakePiece, 1)
{
    private readonly List<Actor> avoidBosses = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BlazeSpikes)
        {
            avoidBosses.Add(caster);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.BlazeSpikes)
        {
            avoidBosses.Remove(actor);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var count = avoidBosses.Count;
        if (count == 0)
        {
            return;
        }

        for (var i = 0; i < count; i++)
        {
            if (avoidBosses[i].InstanceID == actor.TargetID)
            {
                hints.Add("Attacking boss with spikes debuff!");
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var enemies = ActiveActors;
        var count = enemies.Count;
        if (count == 0)
        {
            return;
        }

        for (var i = 0; i < count; i++)
        {
            var enemy = enemies[i];
            hints.SetPriority(enemy, avoidBosses.Contains(enemy) ? AIHints.Enemy.PriorityForbidden : 1);
        }
    }
}

sealed class NeedlesOutTarget(BossModule module) : Components.Adds(module, (uint)OID.BarbmolePiece, 2)
{
    private readonly List<Actor> avoidBosses = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.NeedlesOut)
        {
            avoidBosses.Add(caster);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.NeedlesOut)
        {
            avoidBosses.Remove(actor);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var count = avoidBosses.Count;
        if (count == 0)
        {
            return;
        }

        for (var i = 0; i < count; i++)
        {
            if (avoidBosses[i].InstanceID == actor.TargetID)
            {
                hints.Add("Attacking boss with spikes debuff!");
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var enemies = ActiveActors;
        var count = enemies.Count;
        if (count == 0)
        {
            return;
        }

        for (var i = 0; i < count; i++)
        {
            var enemy = enemies[i];
            hints.SetPriority(enemy, avoidBosses.Contains(enemy) ? AIHints.Enemy.PriorityForbidden : 2);
        }
    }
}

// Uses GenericAOEs component to show it sooner
sealed class BurningCyclone(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private readonly AOEShapeCone shape = new(50f, 60f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BurningCyclone)
        {
            var loc = spell.LocXZ;
            var rot = spell.Rotation;
            _aoe = [new(shape, loc, rot, Module.CastFinishAt(spell), shapeDistance: shape.Distance(loc, rot))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BurningCyclone)
        {
            _aoe = [];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Teleport)
        {
            var loc = spell.TargetXZ;
            var direction = Angle.FromDirection(Arena.Center - loc);
            _aoe = [new(shape, loc, direction, WorldState.FutureTime(5d), shapeDistance: shape.Distance(loc, direction))];
        }
    }

    // If the caster dies before doing the cast, we have to clean it up manually
    public override void OnActorDeath(Actor actor)
    {
        if (actor.OID == (uint)OID.DrakePiece)
        {
            _aoe = [];
        }
    }
}

sealed class BallOfFireRing(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeCircle circle = new(4f);
    private readonly AOEShapeDonut donut = new(3f, 50f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is var oid && oid == (uint)OID.BallOfFire)
        {
            var pos = actor.Position.Quantized();
            _aoes.Add(new(circle, pos, default, actorID: actor.InstanceID, shapeDistance: circle.Distance(pos, default)));
        }
        else if (oid == (uint)OID.TwistingBlaze)
        {
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var len = aoes.Length;
            var pos = actor.Position.Quantized();
            for (var i = 0; i < len; ++i)
            {
                if (aoes[i].Origin.AlmostEqual(pos, 0.5f))
                {
                    _aoes.RemoveAt(i);
                    break;
                }
            }

            _aoes.Add(new(donut, pos, default, actorID: actor.InstanceID, shapeDistance: donut.Distance(pos, default)));
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.BallOfFire)
        {
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var len = aoes.Length;
            var id = actor.InstanceID;
            for (var i = 0; i < len; ++i)
            {
                if (aoes[i].ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BlowingRingOfFire)
        {
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var len = aoes.Length;
            var id = caster.InstanceID;
            for (var i = 0; i < len; ++i)
            {
                if (aoes[i].ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

sealed class AbaddonEat(DrakePiece module) : BossComponent(module)
{
    private readonly AOEShapeCircle shape = new(1f);
    private readonly List<Actor> morphos = module.Morphos;
    private Actor? abaddon;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        abaddon ??= module.Abaddon;

        if (abaddon == null || abaddon.IsDeadOrDestroyed)
        {
            return;
        }

        var count = morphos.Count;

        var spots = new List<ShapeDistance>(count);
        for (var i = 0; i < count; ++i)
        {
            var m = morphos[i];
            if (!m.IsDeadOrDestroyed)
            {
                spots.Add(shape.Distance(m.Position, default));
            }
        }

        if (spots.Count == 0)
        {
            return;
        }

        if (abaddon.TargetID != actor.InstanceID)
        {
            hints.FindEnemy(abaddon)?.PreferProvoking = true;
            return;
        }

        hints.AddForbiddenZone(new SDInvertedUnion([.. spots]));
    }
}

sealed class DrakePieceStates : StateMachineBuilder
{
    private readonly DrakePiece _module;

    public DrakePieceStates(DrakePiece module) : base(module)
    {
        _module = module;
        TrivialPhase()
            .ActivateOnEnter<BurningCyclone>()
            .ActivateOnEnter<ManglingFang>()
            .ActivateOnEnter<BallOfFireRing>()
            .ActivateOnEnter<BlazeSpikes>()
            .ActivateOnEnter<BlazeSpikesTarget>()
            .ActivateOnEnter<AbaddonEat>()
            .ActivateOnEnter<SeedingNeedles>()
            .ActivateOnEnter<NeedlesOutTarget>()
            .Raw.Update = () =>
            {
                // abaddon should turn != null after it spawned, if player wipes, zone changes inits module unload
                return _module.Abaddon != null && AllDeadOrDestroyed(DrakePiece.Bosses);
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.DrakePiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1092u, NameID = 14651u, SortOrder = 4)]
[PreferredKinship(BST.Kinship.Wave, "Remove Blaze Spikes")]
public sealed class DrakePiece : BossModule
{
    public static readonly uint[] Bosses = [(uint)OID.AbaddonPiece, (uint)OID.MorphoPiece, (uint)OID.BarbmolePiece, (uint)OID.DrakePiece];

    public readonly List<Actor> Morphos;
    private readonly List<Actor> barbmoles;

    public DrakePiece(WorldState ws, Actor primary) : base(ws, primary, new(520f, 0f), new ArenaBoundsRect(20f, 15f))
    {
        Morphos = Enemies((uint)OID.MorphoPiece);
        barbmoles = Enemies((uint)OID.BarbmolePiece);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var countM = Morphos.Count;
        var morphoPieceAlive = false;
        for (var i = 0; i < countM; ++i)
        {
            if (!Morphos[i].IsDead)
            {
                morphoPieceAlive = true;
                break;
            }
        }

        var AbaddonPieceAlive = Abaddon?.IsDeadOrDestroyed == false;

        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];

            if (e.Actor.OID is (uint)OID.DrakePiece or (uint)OID.BarbmolePiece)
            {
                continue;
            }

            e.Priority = e.Actor.OID switch
            {
                (uint)OID.AbaddonPiece => morphoPieceAlive ? AIHints.Enemy.PriorityForbidden
                    : (e.Actor.FindStatus((uint)SID.Rehabilitation)?.Extra ?? 0) > 4 ? AIHints.Enemy.PriorityPointless : 4,
                (uint)OID.MorphoPiece => AbaddonPieceAlive ? AIHints.Enemy.PriorityForbidden : 3, // This should never happen, but it could
                _ => 0
            };
        }
    }

    public Actor? Abaddon;

    protected override void UpdateModule()
    {
        Abaddon ??= GetActor((uint)OID.AbaddonPiece);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(Abaddon);
        Arena.Actors(Morphos);
        Arena.Actors(barbmoles);
    }

    private readonly string[] _prePullHints = [
        "DrakePiece: Uses Blaze Spikes which will reflect damage when attacked - This can be dispelled",
        "BarbmolePiece: Casts Needles Out, which will make it reflect with a stacking bleed when attacked.",
        "AbaddonPiece: Eats MorphoPiece and gains regen stacks. Will not gain regen if MorphoPiece is low HP when eaten - Don't kill until all MorphoPiece are dead",
        "MorphoPiece: Applies WitsEnd upon reaching 16 stacks will cause Confused debuff. Killing MorphoPiece will cause confuse debuff as well, must be eaten by AbaddonPiece"
    ];

    public override string[] PrePullHints => _prePullHints;
}
