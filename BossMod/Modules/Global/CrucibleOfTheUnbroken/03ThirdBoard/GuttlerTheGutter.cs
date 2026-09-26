namespace BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.GuttlerTheGutter;

public enum OID : uint
{
    GuttlerTheGutter = 0x4CAA,
    CombustingBlade = 0x4CAB, // R1.000, x6
    ThanatosPiece = 0x4CAD, // R2.000, x0 (spawn during fight)
    ThanatosIdlePiece = 0x4E5D, // R1.400, x0 (spawn during fight)
    DeadlyDemesne = 0x1EC0C3, // R0.500, x0 (spawn during fight), EventObj type
    MoltenBlade = 0x4CAC, // R1.000, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 49714, // GuttlerTheGutter->players, no cast, range 9 ?-degree cone
    AutoAttackThanatos = 870, // _Gen_ThanatosPiece->player, no cast, single-target
    Teleport = 48590, // GuttlerTheGutter->location, no cast, single-target

    GyrocleaveBoss = 48604, // GuttlerTheGutter->self, 6.0s cast, single-target
    Gyrocleave = 48605, // Helper->self, 7.0s cast, range 80 width 20 rect
    GluttonousGoringBoss = 48602, // GuttlerTheGutter->self, 6.0+0.6s cast, single-target
    GluttonousGoring = 48603, // Helper->self, 11.6s cast, range 40 circle
    BeastlyAuraBoss = 48606, // GuttlerTheGutter->self, 6.0s cast, single-target
    BeastlyAura = 48607, // Helper->self, 7.0s cast, range 80 width 80 rect
    Thunderbolt = 48620, // GuttlerTheGutter->self/player, 5.0s cast, range 50 width 6 rect
    GluttonousGuttingBoss = 48599, // GuttlerTheGutter->self, 6.0+0.6s cast, single-target
    GluttonousGutting = 48600, // Helper->self, 11.6s cast, range 50 width 40 rect
    DeadlyDemesneBoss = 48608, // GuttlerTheGutter->self, 3.0s cast, single-target
    Fetters = 48609, // Helper->self, no cast, range 10 width 10 rect - AOE inside the cage that binds you if you're inside
    LifeClaim = 48611, // Helper->self, 3.0s cast, range 15 width 10 cross
    MoltenMetalBoss = 48615, // GuttlerTheGutter->self, 6.2+2.1s cast, single-target
    MoltenMetal1 = 48616, // GuttlerTheGutter->self, no cast, single-target
    MoltenMetalBait = 48617, // _Gen_MoltenBlade->GuttlerTheGutter, 2.5s cast, single-target
    MoltenMetalBaitCircle = 48618, // Helper->self, 3.0s cast, range 6 circle
    BeastlyFlare = 48619, // _Gen_MoltenBlade->self, 8.0s cast, range 80 circle
    OverpoweringPoint = 48612, // GuttlerTheGutter->self, 4.9+3.5s cast, single-target
    OverpoweringPoint1 = 48613, // GuttlerTheGutter->self, no cast, single-target
    OverpoweringPoint2 = 48614, // Helper->self, 3.5s cast, range 60 width 6 rect

    CombustingBlades = 48601, // GuttlerTheGutter->self, no cast, single-target
    CombustingBlades1 = 48598, // GuttlerTheGutter->self, no cast, single-target
    CombustingBladesSpawn = 48591, // _Gen_CombustingBlade->GuttlerTheGutter, no cast, single-target
    CombustingBladesSpawn1 = 48592, // _Gen_CombustingBlade->GuttlerTheGutter, no cast, single-target
    CombustingBladesSpawn2 = 48593, // _Gen_CombustingBlade->GuttlerTheGutter, no cast, single-target
    CombustingBladesTeleport = 48594, // Helper->self, 0.5s cast, range 2 circle
    CombustingBladesTeleport1 = 48595, // Helper->self, 0.7s cast, range 2 circle
    CombustingBladesTeleport2 = 48596, // Helper->self, 0.9s cast, range 2 circle
    MagicalCombustion = 48597, // _Gen_CombustingBlade->self, 5.0s cast, range 8 circle

    Unknown = 48610, // Helper->self, no cast, range 100 circle
}

public enum SID : uint
{
    Gen = 2552, // none->GuttlerTheGutter, extra=0x478/0x483/0x482/0x477
    Bind = 3625, // Helper->player, extra=0x0
    Fetters = 1614, // none->player, extra=0xEC4
    DamageDown = 4874, // Helper->player, extra=0x1
    Paralysis = 5388, // GuttlerTheGutter->player, extra=0x0
}

public enum IconID : uint
{
    ThunderboltTankBuster = 471, // player->self
    MoltenMetal = 669, // player->self
}

public enum TetherID : uint
{
    OverpoweringPointTether = 1, // GuttlerTheGutter->player
}

sealed class AutoAttack(BossModule module) : Components.Cleave(module, (uint)AID.AutoAttack, new AOEShapeCone(9f, 55f.Degrees()), activeWhileCasting: false) {
    private readonly BeastlyAura? beastlyAura = module.FindComponent<BeastlyAura>();

    public override void AddHints(int slot, Actor actor, TextHints hints) {
        if (beastlyAura == null || beastlyAura.knockbacks.Count > 0) {
            return;
        }

        base.AddHints(slot, actor, hints);
    }

    // Set the cleave aoe to be maxValue so it doesn't overlap with really bad mechanics such as the cage - getting hitting by the cleave is fine
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        AllowPetTargets = true;
        NextExpected = DateTime.MaxValue;

        if (beastlyAura == null || beastlyAura.knockbacks.Count > 0) {
            return;
        }

        base.AddAIHints(slot, actor, assignment, hints);
    }
}

sealed class Gyrocleave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Gyrocleave, new AOEShapeRect(80f, 10f));
sealed class GluttonousGoring(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GluttonousGoring, 40f);
sealed class MoltenMetalBaitAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MoltenMetalBaitCircle, 6f);

sealed class BeastlyFlare(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BeastlyFlare, 30f);
sealed class GluttonousGutting(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GluttonousGutting, new AOEShapeRect(50f, 20f));

sealed class MagicalCombustion : Components.SimpleAOEs
{
    public MagicalCombustion(BossModule module) : base(module, (uint)AID.MagicalCombustion, 8f)
    {
        Color = Colors.Danger;
    }
}

sealed class DeadlyDemesne(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCross shape = new(15f, 5f);
    private readonly AOEShapeRect rect = new(5f, 5f, 5f);
    private const float riskyWindow = 6f;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.DeadlyDemesne)
        {
            aoes.Add(new(shape, actor.Position, actor.Rotation, WorldState.FutureTime(12.7d)));
            aoes.Add(new(rect, actor.Position, actor.Rotation, color: Colors.Danger));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LifeClaim)
        {
            if (aoes.Count > 0)
            {
                aoes.RemoveRange(0, 2);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = aoes.Count;
        if (count == 0)
        {
            return [];
        }

        var incomingAOEs = CollectionsMarshal.AsSpan(aoes);
        var time = WorldState.CurrentTime;

        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref incomingAOEs[i];
            aoe.Risky = aoe.Activation == default || aoe.Activation <= time.AddSeconds(riskyWindow);
        }

        return incomingAOEs;
    }
}

sealed class MoltenMetalBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6.0f), (uint)IconID.MoltenMetal, centerAtTarget: true) {
    private readonly WPos[] safeSpots = [new WPos(520.0f, -440.0f), new WPos(520.0f, -400.0f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.MoltenMetalBaitCircle) {
            CurrentBaits.Clear();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) {
        if (CurrentBaits.Count == 0) {
            return;
        }

        hints.Add("Bait far away on one side of the map!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        if (CurrentBaits.Count == 0 || !IsBaitTarget(actor)) {
            return;
        }

        var closestSpot = safeSpots[0];
        var bestDistance = float.MaxValue;

        foreach (var spot in safeSpots) {
            var distance = (spot - actor.Position).LengthSq();
            if (distance < bestDistance) {
                bestDistance = distance;
                closestSpot = spot;
            }
        }

        hints.AddForbiddenZone(new SDInvertedCircle(closestSpot, 2.0f));
    }
}

sealed class OverpoweringPoint(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCone cone = new(40f, 65f.Degrees()); // Cones to prevent baiting towards the adds
    private readonly AOEShapeRect rect = new(60f, 3f); // Actual bait shape
    private readonly AOEShapeCircle circle = new(6f); // Circle under boss to prevent baiting under the boss

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OverpoweringPoint)
        {
            aoes.Add(new(cone, Arena.Center, Angle.AnglesCardinals[0]));
            aoes.Add(new(cone, Arena.Center, Angle.AnglesCardinals[3]));
            aoes.Add(new(circle, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation));
        }

        if (spell.Action.ID == (uint)AID.OverpoweringPoint2)
        {
            aoes.Clear();
            aoes.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.OverpoweringPoint2)
        {
            if (aoes.Count > 0)
            {
                aoes.Clear();
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(aoes);
}

sealed class BeastlyAura(BossModule module) : Components.GenericKnockback(module)
{
    public readonly List<Knockback> knockbacks = [];
    private static readonly AOEShapeRect shape = new(80f, 40f);
    private ShapeDistance distance;
    private const float knockbackDistance = 20f;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BeastlyAura)
        {
            var act = Module.CastFinishAt(spell);
            var pos = Arena.Center;
            var rot = spell.Rotation;
            var offset = 90f.Degrees();
            var rot1 = rot + offset;
            var isAlongZAxis = rot1.AlmostEqual(default, Angle.DegToRad) || rot1.AlmostEqual(180f.Degrees(), Angle.DegToRad);
            knockbacks.Add(new(pos, knockbackDistance, act, shape, rot1, Kind.DirForward));
            knockbacks.Add(new(pos, knockbackDistance, act, shape, rot - offset, Kind.DirForward));
            distance = isAlongZAxis
                ? new SDKnockbackInAABBRectLeftRightAlongZAxis(Arena.Center, knockbackDistance, 10f, 24f)
                : new SDKnockbackInAABBRectLeftRightAlongXAxis(Arena.Center, knockbackDistance, 10f, 24f);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BeastlyAura)
        {
            if (knockbacks.Count > 0)
            {
                knockbacks.Clear();
            }
        }
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(knockbacks);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (knockbacks.Count == 0)
        {
            return;
        }

        var activation = knockbacks[0].Activation;

        if (!IsImmune(slot, activation))
        {
            hints.AddForbiddenZone(distance, activation);
            // Handles the sides of the center of the map (left & right)
            hints.AddForbiddenZone(new SDRect(Arena.Center + new WDir(-6.25f, 0f), default(Angle), 20f, 20f, 3.75f), activation);
            hints.AddForbiddenZone(new SDRect(Arena.Center + new WDir(6.25f, 0f), default(Angle), 20f, 20f, 3.75f), activation);
        }
    }
}

sealed class Thunderbolt(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(50f, 3f), (uint)IconID.ThunderboltTankBuster,
    (uint)AID.Thunderbolt, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Count == 0)
        {
            return;
        }

        base.AddHints(slot, actor, hints);
        hints.Add("Applies paralysis debuff");
    }
}

sealed class GuttlerTheGutterStates : StateMachineBuilder
{
    public GuttlerTheGutterStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Gyrocleave>()
            .ActivateOnEnter<MagicalCombustion>()
            .ActivateOnEnter<GluttonousGoring>()
            .ActivateOnEnter<BeastlyAura>()
            .ActivateOnEnter<Thunderbolt>()
            .ActivateOnEnter<DeadlyDemesne>()
            .ActivateOnEnter<MoltenMetalBait>()
            .ActivateOnEnter<MoltenMetalBaitAOE>()
            .ActivateOnEnter<BeastlyFlare>()
            .ActivateOnEnter<GluttonousGutting>()
            .ActivateOnEnter<OverpoweringPoint>()
            .ActivateOnEnter<AutoAttack>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.GuttlerTheGutter, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1090u, NameID = 14592u, SortOrder = 8)]
public sealed class GuttlerTheGutter : BossModule
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.ThanatosPiece => 2,
                (uint)OID.GuttlerTheGutter => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.ThanatosPiece));
    }

    public GuttlerTheGutter(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private GuttlerTheGutter(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    public static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([
            new Rectangle(new(520f, -420f), 10f, 20f), // Base map
            new Rectangle(new(520f, -397.5f), 2.5f, 2.5f), // Bottom of map
            new Rectangle(new(520f, -442.5f), 2.5f, 2.5f), // Bottom of map

            // Left side of map
            new Rectangle(new(508f, -412.5f), 2.5f, 2.5f),
            new Rectangle(new(508f, -422.5f), 2.5f, 2.5f),

            // Right side of map
            new Rectangle(new(532f, -417.5f), 2.5f, 2.5f),
            new Rectangle(new(532f, -427.5f), 2.5f, 2.5f),
        ]);

        return (arena.Center, arena);
    }
}
