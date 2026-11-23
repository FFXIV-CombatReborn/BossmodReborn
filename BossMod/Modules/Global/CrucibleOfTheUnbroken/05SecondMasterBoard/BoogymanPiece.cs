namespace BossMod.Global.CrucibleOfTheUnbroken.SecondMasterBoard.BoogymanPiece;

public enum OID : uint
{
    BoogymanPiece = 0x4CE2,
    BombPiece = 0x4CE4, // R0.900, x0 (spawn during fight)
    DeepeyePiece = 0x4CE3, // R1.200, x0 (spawn during fight)
    LightSprite = 0x4CE5, // R1.200, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 49217, // BoogymanPiece->player, no cast, single-target
    AutoAttack2 = 49218, // BoogymanPiece->player, no cast, single-target
    Teleport = 49229, // BoogymanPiece->location, no cast, single-target

    VeilOfDarkness = 49209, // BoogymanPiece->self, 4.0s cast, range 50 circle
    FeralLeap = 49230, // BoogymanPiece->player, no cast, single-target
    SwingRoundKnockbackBoss = 49225, // BoogymanPiece->self, 4.0+1.0s cast, single-target
    SwingRoundKnockback = 49226, // Helper->self, 5.0s cast, range 40 circle
    SwingRoundCircleBoss = 49227, // BoogymanPiece->self, 4.0+1.0s cast, single-target
    SwingRoundCircle = 49228, // Helper->self, 5.0s cast, range 12 circle
    RipplesOfGloom = 49231, // BoogymanPiece->self, 5.0s cast, range 50 circle
    ProvisionClub = 49216, // BoogymanPiece->self, 3.0s cast, single-target
    SwoopClub = 49219, // BoogymanPiece->location, 3.0s cast, width 4 rect charge
    SpinningStrikeBoos = 49221, // BoogymanPiece->self, 1.0+1.0s cast, single-target
    SpinningStrike = 49222, // Helper->self, 2.0s cast, range 30 circle
    ProvisionSword = 49215, // BoogymanPiece->self, 3.0s cast, single-target
    SwoopSword = 49220, // BoogymanPiece->location, 3.0s cast, width 4 rect charge
    ClearoutBoss = 49223, // BoogymanPiece->self, 1.0+0.5s cast, single-target
    Clearout = 49224, // Helper->self, 1.5s cast, range 40 120.000-degree cone

    // BombPiece
    Explosion = 49210, // 4CE4->self, 5.0s cast, range 9 circle
    SelfDestruct = 49211, // 4CE4->self, 8.0s cast, range 50 circle

    // DeepeyePiece
    AutoAttackDeepeyePiece = 49682, // 4CE3->player, no cast, single-target
    Oogle = 49214, // 4CE3->self, 10.0s cast, range 40 circle

    // LightSprite
    AutoAttackBanish = 49213, // 4CE5->player, 1.5s cast, single-target
    DiffuseLight = 49212, // 4CE5->self, 1.5s cast, range 20 45.000-degree cone
}

public enum SID : uint
{
    SlashingResistanceDown = 3130, // BoogymanPiece->player, extra=0x0
    BluntResistanceDown = 3132, // BoogymanPiece->player, extra=0x0
    Concealed = 1621, // none->BoogymanPiece, extra=0x3
    Concealed1 = 3997, // none->4CE4/4CE3, extra=0xA
    Invisible = 616, // BoogymanPiece->BoogymanPiece, extra=0x0
    Blind = 5381, // BoogymanPiece->player, extra=0x0
    Bleeding = 3077, // none->player, extra=0x0
    Gen = 2659, // BoogymanPiece->BoogymanPiece, extra=0xBC5
}

sealed class RipplesOfGloom(BossModule module) : Components.RaidwideCast(module, (uint)AID.RipplesOfGloom, "Raidwide + Applies blind");
sealed class VeilOfDarkness(BossModule module) : Components.RaidwideCast(module, (uint)AID.VeilOfDarkness);
sealed class SelfDestruct(BossModule module) : Components.CastHint(module, (uint)AID.SelfDestruct, "Kill bomb before it self-destructs!", true);
sealed class SwingRound(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SwingRoundCircle, 12.0f);
sealed class BombExplosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion, 9f);
sealed class Oogle(BossModule module) : Components.CastGaze(module, (uint)AID.Oogle);
sealed class DiffuseLight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DiffuseLight, new AOEShapeCone(20f, 22.5f.Degrees()));

sealed class SwingRoundKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.SwingRoundKnockback, 20.0f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
        {
            return;
        }

        ref readonly var knockback = ref Casters.Ref(0);
        var activation = knockback.Activation;

        if (IsImmune(slot, activation))
        {
            return;
        }

        hints.AddForbiddenZone(new SDKnockbackInCircleAwayFromOrigin(Arena.Center, knockback.Origin, 20.0f, 19.0f), activation);
    }
}

sealed class SwoopCharge : Components.SimpleChargeAOEGroups
{
    public SwoopCharge(BossModule module) : base(module, [(uint)AID.SwoopClub, (uint)AID.SwoopSword], 2f)
    {
        Color = Colors.Danger;
    }
}

sealed class Swoop(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCircle circle = new(30f);
    private readonly AOEShapeCone cone = new(40f, 60f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is var id && id is (uint)AID.SwoopClub or (uint)AID.SwoopSword)
        {
            var loc = spell.LocXZ;
            var direction = Angle.FromDirection(Arena.Center - loc);
            AOEShape shape = id == (uint)AID.SwoopClub ? circle : cone;
            aoes.Add(new(shape, loc, direction, shapeDistance: shape.Distance(loc, direction)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SpinningStrike or (uint)AID.Clearout)
        {
            if (aoes.Count > 0)
            {
                aoes.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(aoes);
}

// Used to track where the adds are base on if they're dead or not - They go invisible, so we have to manually draw them like this
sealed class AddTrack(BossModule module) : Components.AddsMulti(module, [(uint)OID.BombPiece, (uint)OID.DeepeyePiece, (uint)OID.LightSprite],
    allowUntargetable: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.PrioritizeTargetsByOIDAndForbidDOTs((uint)OID.BombPiece, 4);
        hints.PrioritizeTargetsByOIDAndForbidDOTs((uint)OID.DeepeyePiece, 3);
        hints.PrioritizeTargetsByOIDAndForbidDOTs((uint)OID.LightSprite, 2);

        var enemies = ActiveActors;
        var count = enemies.Count;
        if (count == 0)
        {
            return;
        }

        Actor? closest = null;
        var closestDistance = float.MaxValue;
        var closestPriority = 0;

        for (var i = 0; i < count; i++)
        {
            var enemy = enemies[i];
            var priority = enemy.OID switch
            {
                (uint)OID.BombPiece => 4,
                (uint)OID.DeepeyePiece => 3,
                (uint)OID.LightSprite => 2,
                _ => 0
            };

            if (priority == 0)
            {
                continue;
            }

            var distance = (actor.Position - enemy.Position).LengthSq();

            if (priority > closestPriority || priority == closestPriority && distance < closestDistance)
            {
                closestDistance = distance;
                closest = enemy;
                closestPriority = priority;
            }
        }

        if (closest == null)
        {
            return;
        }

        hints.GoalZones.Add(AIHints.GoalSingleTarget(closest.Position, 3.0f, 5.0f));
    }
}

sealed class RevealMainBoss(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> sprites = module.Enemies((uint)OID.LightSprite);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Module.PrimaryActor.IsTargetable)
        {
            return;
        }

        var count = sprites.Count;
        WPos position = default;
        for (var i = 0; i < count; ++i)
        {
            var s = sprites[i];
            if (s.IsDead)
            {
                return;
            }

            if (position == default)
            {
                position = s.Position;
            }
        }

        var spriteToBoss = position - Module.PrimaryActor.Position;
        hints.GoalZones.Add(AIHints.GoalSingleTarget(position + 2f * spriteToBoss.Normalized(), 1f, 2f));
    }
}

sealed class BoogymanPieceStates : StateMachineBuilder
{
    public BoogymanPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SelfDestruct>()
            .ActivateOnEnter<RipplesOfGloom>()
            .ActivateOnEnter<VeilOfDarkness>()
            .ActivateOnEnter<SwingRound>()
            .ActivateOnEnter<SwingRoundKnockback>()
            .ActivateOnEnter<BombExplosion>()
            .ActivateOnEnter<Oogle>()
            .ActivateOnEnter<DiffuseLight>()
            .ActivateOnEnter<SwoopCharge>()
            .ActivateOnEnter<Swoop>()
            .ActivateOnEnter<AddTrack>()
            .ActivateOnEnter<RevealMainBoss>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.BoogymanPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1092u, NameID = 14638u, SortOrder = 3)]
[PreferredKinship(BST.Kinship.Vile, "Ignore Swing Round's knockback")]
[PreferredKinship(BST.Kinship.Ash, "Cleanse Blind")]
public sealed class BoogymanPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f))
{
    private readonly string[] _prePullHints = [
        "Fight kill priority: BombPiece -> DeepeyePiece -> LightSprite -> boss",
        "Sprite: Will cast a cone toward where it's facing when it dies, aim it toward the boss to reveal it"
    ];

    public override string[] PrePullHints => _prePullHints;
}
