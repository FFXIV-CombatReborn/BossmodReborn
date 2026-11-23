namespace BossMod.Global.CrucibleOfTheUnbroken.FirstMasterBoard.ProgenitrixPiece;

public enum OID : uint
{
    ProgenitrixPiece = 0x4CD2,
    BombPiece = 0x4CD4, // R0.900, x0 (spawn during fight)
    SnollPiece = 0x4CD5, // R0.900, x0 (spawn during fight)
    PyrobolusPiece = 0x4CD6, // R1.050, x0 (spawn during fight)
    GrenadePiece = 0x4CD3, // R2.800, x0 (spawn during fight)
    FirePuddle = 0x1E8D9B, // R0.500, x0 (spawn during fight), EventObj type
    Unknown = 0x4CD7, // R2.000, x9
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 50786, // ProgenitrixPiece->player, no cast, single-target
    Teleport = 48797, // ProgenitrixPiece->location, no cast, single-target

    ScaldingScoldingBoss = 48798, // ProgenitrixPiece->self, 5.0s cast, single-target
    ScaldingScolding = 48799, // Helper->self, 6.0s cast, range 40 180.000-degree cone
    MassiveExplosionBoss = 48802, // ProgenitrixPiece->self, 7.0s cast, range 50 circle
    MassiveExplosion = 48790, // 4CD3->self, 4.0s cast, range 50 circle
    MeltdownBoss = 48800, // ProgenitrixPiece->player, no cast, single-target
    Meltdown = 48801, // Helper->self, no cast, range 40 width 12 rect
    FieryFuryTeleportStart = 48803, // ProgenitrixPiece->location, 8.0+0.9s cast, single-target
    FieryFuryTeleportRest = 48804, // ProgenitrixPiece->location, no cast, single-target
    FieryFuryAOE = 48827, // Helper->self, 7.9s cast, range 6 circle
    FieryFuryKnockback = 48805, // Helper->self, 7.9s cast, range 40 circle

    // BombPiece
    AutoAttackFire = 48622, // 4CD4->player, no cast, single-target
    SelfDestruct = 48792, // 4CD4->self, no cast, range 6 circle
    FireII = 48791, // 4CD4->location, 4.0s cast, range 5 circle

    // SnollPiece
    AutoAttackBlizzard = 48621, // 4CD5->player, no cast, single-target
    HypothermalCombustion = 48794, // 4CD5->self, no cast, range 10 circle
    IceSpikes = 48793, // 4CD5->self, 3.0s cast, single-target

    // PyrobolusPiece
    AutoAttackPyrobolusPiece = 49682, // 4CD6->player, no cast, single-target
    ToxicFumesActor = 48795, // 4CD6->self, 3.0s cast, single-target
    ToxicFumes = 48796, // Helper->self, 4.0s cast, range 40 20.000-degree cone
}

public enum SID : uint
{
    Invincibility = 4410, // none->ProgenitrixPiece, extra=0x0
    Swelling = 5182, // none->4CD3, extra=0x1/0x2/0x3/0x5/0x6/0x7/0x8/0x9/0xA/0xB/0xC/0xD/0x4
    Bind = 2518, // ProgenitrixPiece->player, extra=0x0
    IceSpikes = 2528, // 4CD5->4CD5, extra=0x64
    Slow = 9, // 4CD5->player, extra=0x0
    Burns = 3065, // none->player, extra=0x0
    Burns1 = 3066, // none->player, extra=0x0
    Bleeding = 3077, // none->player, extra=0x0
    Bleeding1 = 3078, // none->player, extra=0x0
}

public enum IconID : uint
{
    MeltdownTankBuster = 412, // player->self
}

public enum TetherID : uint
{
    InvincibilityTether = 5, // 4CD3->ProgenitrixPiece
}

sealed class ScaldingScolding(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ScaldingScolding, new AOEShapeCone(40f, 90f.Degrees()));
sealed class MassiveExplosion(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.MassiveExplosion, (uint)AID.MassiveExplosionBoss]);
sealed class IceSpikes(BossModule module) : Components.Dispel(module, (uint)SID.IceSpikes, (uint)AID.IceSpikes);
sealed class ToxicFumes(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ToxicFumes, new AOEShapeCone(40f, 10f.Degrees()));
sealed class FireII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FireII, 5f);

sealed class Meltdown(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(40f, 6f), (uint)IconID.MeltdownTankBuster, (uint)AID.Meltdown,
    activationDelay: 7.9d, tankbuster: true);

sealed class MeltdownKnockback(BossModule module) : Components.SimpleKnockbacks(module, default, 15f, kind: Kind.AwayFromOrigin)
{
    private Actor? target;
    private DateTime activation = default;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) { }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell) { }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.MeltdownTankBuster)
        {
            var targetPlayer = WorldState.Actors.Find(targetID);
            if (targetPlayer == null)
            {
                return;
            }

            target = targetPlayer;
            activation = WorldState.FutureTime(7.9f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Meltdown)
        {
            target = null;
            activation = default;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            return;
        }

        var knockbacks = CollectionsMarshal.AsSpan(Casters);
        ref var knockback = ref knockbacks[0];

        if (IsImmune(slot, knockback.Activation))
        {
            return;
        }

        hints.AddForbiddenZone(new SDKnockbackInCircleAwayFromOrigin(Arena.Center, knockback.Origin, knockback.Distance, 19.0f), knockback.Activation);
    }

    public override void Update()
    {
        Casters.Clear();

        if (target == null)
        {
            return;
        }

        var minDist = KnockbackKind == Kind.TowardsOrigin ? (MinDistance + (MinDistanceBetweenHitboxes ? (Raid.Player()?.HitboxRadius ?? default) + Module.PrimaryActor.HitboxRadius : default)) : MinDistance;
        Casters.Add(new(Module.PrimaryActor.Position, Distance, activation, Shape, default, KnockbackKind, minDist, [], default, IgnoreImmunes,
            ResolveArenaProjectionLayer(target.Position.Z), RestrictToArenaProjectionLayer));
    }
}

sealed class FirePuddles(BossModule module) : Components.Voidzone(module, 6f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.FirePuddle);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

sealed class FieryFuryKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.FieryFuryKnockback, 15f, maxCasts: 2)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            return;
        }

        var knockbacks = CollectionsMarshal.AsSpan(Casters);
        ref var knockback = ref knockbacks[0];

        if (IsImmune(slot, knockback.Activation))
        {
            return;
        }

        if (count == 3)
        {
            hints.AddForbiddenZone(new SDKnockbackInCircleAwayFromOriginMulti3(Arena.Center, [.. knockbacks], knockback.Distance, 19.0f), knockback.Activation);
            return;
        }

        if (count == 2)
        {
            hints.AddForbiddenZone(new SDKnockbackInCircleAwayFromOriginMulti2(Arena.Center, [.. knockbacks], knockback.Distance, 19.0f), knockback.Activation);
            return;
        }

        hints.AddForbiddenZone(new SDKnockbackInCircleAwayFromOrigin(Arena.Center, knockback.Origin, knockback.Distance, 19.0f), knockback.Activation);
    }
}
sealed class FieryFuryAOE : Components.SimpleAOEs
{
    public FieryFuryAOE(BossModule module) : base(module, (uint)AID.FieryFuryAOE, 6f, maxCasts: 2)
    {
        MaxDangerColor = 1;
    }
}

sealed class SnollPieceTarget(BossModule module) : Components.Adds(module, (uint)OID.SnollPiece, AIHints.Enemy.PriorityForbidden)
{
    private readonly FirePuddles? firePuddles = module.FindComponent<FirePuddles>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        // Are any enemy of this type alive
        var enemies = CollectionsMarshal.AsSpan(ActiveActors);
        var count = enemies.Length;
        if (count == 0 || firePuddles == null)
        {
            return;
        }

        // Do we have any fire puddles available? If not just let the player kill them
        if (!firePuddles.Sources(Module).Any())
        {
            foreach (var enemy in enemies)
            {
                hints.SetPriority(enemy, 2);
            }
            return;
        }

        // Find the closest puddle
        Actor? closestPuddle = null;
        var closestDistance = float.MaxValue;
        foreach (var puddle in firePuddles.Sources(Module))
        {
            var distance = (puddle.Position - actor.Position).Length();
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPuddle = puddle;
            }
        }

        // Case: Checks if we are close enough to a puddle & the enemy is close enough. We check these together as we need to ensure both are valid for it to work
        var nearPuddle = closestPuddle != null && closestDistance <= 8.0f;
        for (var i = 0; i < count; i++)
        {
            ref var enemy = ref enemies[i];
            var mainAggro = enemy.TargetID == actor.InstanceID;
            var enemyCloseEnough = mainAggro && (enemy.Position - actor.Position).Length() <= 5.0f; // Melee range of the enemy to player
            var ready = enemyCloseEnough && nearPuddle;
            hints.SetPriority(enemy, ready ? 2 : AIHints.Enemy.PriorityForbidden);
            if (mainAggro && !ready && closestPuddle != null)
            {
                hints.GoalZones.Add(AIHints.GoalDonut(closestPuddle.Position, 6.0f, 8.0f, 5.0f));
            }
        }
    }
}

sealed class ProgenitrixPieceStates : StateMachineBuilder
{
    public ProgenitrixPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IceSpikes>()
            .ActivateOnEnter<ScaldingScolding>()
            .ActivateOnEnter<MassiveExplosion>()
            .ActivateOnEnter<ToxicFumes>()
            .ActivateOnEnter<FireII>()
            .ActivateOnEnter<FirePuddles>()
            .ActivateOnEnter<Meltdown>()
            .ActivateOnEnter<MeltdownKnockback>()
            .ActivateOnEnter<FieryFuryAOE>()
            .ActivateOnEnter<FieryFuryKnockback>()
            .ActivateOnEnter<SnollPieceTarget>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.ProgenitrixPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1091u, NameID = 14623u, SortOrder = 9)]
[PreferredKinship(BST.Kinship.Wave, "Remove Ice Spikes")]
[PreferredKinship(BST.Kinship.Vile, "Ignore Meltdown and Fiery Fury knockbacks")]
[PreferredKinship(BST.Kinship.Ash, "Cleanse Slow from hitting Ice Spikes")]
public sealed class ProgenitrixPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];

            if (e.Actor.OID == (uint)OID.SnollPiece)
            {
                continue;
            }

            e.Priority = e.Actor.OID switch
            {
                (uint)OID.GrenadePiece => 5,
                (uint)OID.PyrobolusPiece => 4,
                (uint)OID.BombPiece => 3,
                (uint)OID.ProgenitrixPiece => e.Actor.FindStatus((uint)SID.Invincibility) != null ? AIHints.Enemy.PriorityForbidden : 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.GrenadePiece), Colors.Vulnerable);
        Arena.Actors(Enemies((uint)OID.PyrobolusPiece));
        Arena.Actors(Enemies((uint)OID.BombPiece));
    }

    private readonly string[] _prePullHints = [
        "Fight kill order priority: GrenadePiece (purple) -> Any other add -> Boss",
        "Killing ice bombs near fire puddles will get rid of them",
    ];

    public override string[] PrePullHints => _prePullHints;
}
