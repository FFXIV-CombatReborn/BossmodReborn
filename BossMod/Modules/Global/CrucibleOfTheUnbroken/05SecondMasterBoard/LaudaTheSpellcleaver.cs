namespace BossMod.Global.CrucibleOfTheUnbroken.SecondMasterBoard.LaudaTheSpellcleaver;

public enum OID : uint
{
    LaudaTheSpellcleaver = 0x4D1E,
    ThanatosIdlePiece = 0x4E5E, // R1.400, x0 (spawn during fight)
    ThanatosPiece = 0x4D24, // R2.000, x0 (spawn during fight)
    EphemeralBlade = 0x4D21, // R1.500, x14
    PossessiveBlade = 0x4D23, // R1.000, x2
    CombustingBlade = 0x4D1F, // R1.000, x6
    DeadlyDemesneGreen = 0x1EC0C4, // R0.500, x0 (spawn during fight), EventObj type
    DeadlyDemesneYellow = 0x1EC0DD, // R0.500, x0 (spawn during fight), EventObj type
    MoltenBlade = 0x4D20, // R1.000, x0 (spawn during fight)
    MagickedBlade = 0x4EB8, // R1.500, x0 (spawn during fight)
    IndefatigableBlade = 0x4D22, // R1.500, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 50860, // LaudaTheSpellcleaver->players, no cast, range 9 ?-degree cone
    Teleport = 49436, // LaudaTheSpellcleaver->location, no cast, single-target

    Thunderbolt = 49470, // LaudaTheSpellcleaver->self/player, 5.0s cast, range 50 width 6 rect
    VanishingDaggers = 49481, // LaudaTheSpellcleaver->self, 3.0s cast, single-target
    RushVisual = 49482, // 4D21->self, 0.7+0.3s cast, single-target
    Rush = 49483, // Helper->self, 1.0s cast, range 50 width 5 rect
    BeastlyAuraBoss = 49452, // LaudaTheSpellcleaver->self, 6.0s cast, single-target
    BeastlyAura = 49453, // Helper->self, 7.0s cast, range 80 width 80 rect
    GyrocleaveBoss = 49450, // LaudaTheSpellcleaver->self, 6.0s cast, single-target
    Gyrocleave = 49451, // Helper->self, 7.0s cast, range 80 width 20 rect
    GluttonousGuttingBoss = 49445, // LaudaTheSpellcleaver->self, 8.0+0.6s cast, single-target
    GluttonousGutting = 49446, // Helper->self, 13.6s cast, range 50 width 40 rect
    GluttonousGoringBoss = 49448, // LaudaTheSpellcleaver->self, 8.0+0.6s cast, single-target
    GluttonousGoring = 49449, // Helper->self, 13.6s cast, range 40 circle

    PointMaker = 49462, // LaudaTheSpellcleaver->self, 3.0s cast, single-target
    OverpoweringPointBoss = 50846, // LaudaTheSpellcleaver->self, 4.9+2.0s cast, single-target
    OverpoweringPointActor = 49463, // 4D23->self, 1.7+0.3s cast, single-target
    OverpoweringPoint1 = 50847, // LaudaTheSpellcleaver->self, no cast, single-target
    OverpoweringPoint2 = 50848, // Helper->self, 2.0s cast, range 60 width 6 rect
    OverpoweringPoint3 = 49464, // Helper->self, 2.0s cast, range 60 width 6 rect
    MoltenMetalBoss = 49465, // LaudaTheSpellcleaver->self, 6.2+2.1s cast, single-target
    MoltenMetalBoss1 = 49466, // LaudaTheSpellcleaver->self, no cast, single-target
    MoltenMetalBait = 49467, // 4D20->LaudaTheSpellcleaver, 2.5s cast, single-target
    MoltenMetalBaitCircle = 49468, // Helper->self, 3.0s cast, range 6 circle
    BeastlyFlare = 49469, // 4D20->self, 11.0s cast, range 80 circle
    CombustingBlades = 49444, // LaudaTheSpellcleaver->self, no cast, single-target
    CombustingBlades1 = 49447, // LaudaTheSpellcleaver->self, no cast, single-target
    CombustingBladesSpawn1 = 49438, // 4D1F->LaudaTheSpellcleaver, no cast, single-target
    CombustingBladesSpawn2 = 49439, // 4D1F->LaudaTheSpellcleaver, no cast, single-target
    CombustingBladesSpawn3 = 49437, // 4D1F->LaudaTheSpellcleaver, no cast, single-target
    CombustingBladesTeleport = 49440, // Helper->self, 0.5s cast, range 2 circle
    CombustingBladesTeleport1 = 49441, // Helper->self, 0.7s cast, range 2 circle
    CombustingBladesTeleport2 = 49442, // Helper->self, 0.9s cast, range 2 circle
    MagicalCombustion = 49443, // 4D1F->self, 5.0s cast, range 8 circle

    DeadlyDemesne = 49454, // LaudaTheSpellcleaver->self, 3.0s cast, single-target
    FettersYellow = 49458, // Helper->self, no cast, range 5 width 5 rect
    FettersGreen = 49455, // Helper->self, no cast, range 10 width 10 rect
    CageOfMoltenMetal = 49460, // Helper->self, 3.0s cast, range 12 circle
    LifeClaim = 49457, // Helper->self, 3.0s cast, range 15 width 10 cross
    UnseenForce = 49484, // LaudaTheSpellcleaver->self, 3.0s cast, single-target
    UnseenForce1 = 49485, // LaudaTheSpellcleaver->self, no cast, single-target
    UnseenForce2 = 49486, // Helper->player, no cast, single-target
    UnseenForce3 = 49487, // Helper->player, no cast, single-target
    Shockwave = 49489, // Helper->player, no cast, single-target
    GreaterStrengthBoss = 49472, // LaudaTheSpellcleaver->self, 3.0s cast, single-target
    SearingAxeBoss = 49477, // LaudaTheSpellcleaver->self, 27.1+0.9s cast, single-target
    SearingAxeFinish = 49480, // Helper->self, 2.0s cast, range 80 circle
    SearingAxe = 49478, // Helper->self, no cast, range 80 circle
    SearingAxe1 = 49479, // Helper->self, no cast, range 80 circle
    GuttlerGlutterBoss = 49473, // LaudaTheSpellcleaver->self, 27.1+0.9s cast, single-target
    GuttlerGlutterFinish = 49476, // Helper->self, 2.0s cast, range 80 circle
    GuttlerGlutter = 49474, // Helper->self, no cast, range 80 circle
    GuttlerGlutter1 = 49475, // Helper->self, no cast, range 80 circle

    // ThanatosPiece
    AutoAttackThanatos = 49680, // 4D24->player, no cast, single-target
    InfernalPain = 50541, // 4D24->self, 8.0s cast, range 40 circle
}

public enum SID : uint
{
    HighWire = 5562, // LaudaTheSpellcleaver->LaudaTheSpellcleaver, extra=0x1/0x2
    Bind = 5555, // none->player, extra=0x0
    UnseenForce = 5341, // LaudaTheSpellcleaver->player, extra=0x0
    Petrification = 4891, // Helper->player, extra=0x0
    Paralysis = 5388, // LaudaTheSpellcleaver->player, extra=0x0
    Burns = 3065, // none->player, extra=0x0
    Burns1 = 3066, // none->player, extra=0x0
    VulnerabilityDown = 5338, // none->LaudaTheSpellcleaver, extra=0x2/0x1
    MagicDamageUp = 5340, // none->LaudaTheSpellcleaver, extra=0x10/0xF/0xD/0xC/0xB/0xA/0x9/0x8/0x5/0x4/0x3
    Unknown = 2552, // none->LaudaTheSpellcleaver, extra=0x483/0x482/0x477
}

public enum IconID : uint
{
    ThunderboltTankBuster = 471, // player->self
    Unknown = 234, // player->self
    UnseenForceCount = 276, // player->self
    MoltenMetalIcon = 669, // player->self
}

public enum TetherID : uint
{
    OverpoweringPointTether = 1, // LaudaTheSpellcleaver/4D23->player
    MagickedBladeTether = 197, // 4EB8->LaudaTheSpellcleaver
    IndefatigableBladeTether = 5, // 4D22->LaudaTheSpellcleaver
}

sealed class GluttonousGutting(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GluttonousGutting, new AOEShapeRect(50f, 20f))
{
    private UnseenForce? unseenForce;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        unseenForce ??= Module.FindComponent<UnseenForce>();
        if (unseenForce == null || unseenForce.Forces.Count == 0)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            return;
        }

        var activation = unseenForce.Forces[0].Activation;
        var aoes = CollectionsMarshal.AsSpan(Casters);
        for (var i = 0; i < aoes.Length; ++i)
        {
            hints.AddForbiddenZone(new SDCircle(aoes[i].Origin, 30f), activation);
        }
    }
}

sealed class GluttonousGoring(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GluttonousGoring, 40f)
{
    private UnseenForce? unseenForce;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        unseenForce ??= Module.FindComponent<UnseenForce>();
        if (unseenForce == null || unseenForce.Forces.Count == 0)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            return;
        }

        var activation = unseenForce.Forces[0].Activation;
        var aoes = CollectionsMarshal.AsSpan(Casters);
        for (var i = 0; i < aoes.Length; ++i)
        {
            hints.AddForbiddenZone(new SDInvertedCircle(aoes[i].Origin, 5f), activation);
        }
    }
}

sealed class InfernalPain(BossModule module) : Components.RaidwideCast(module, (uint)AID.InfernalPain, "Kill Thanatos before the cast ends!");

sealed class AutoAttack(BossModule module) : Components.Cleave(module, (uint)AID.AutoAttack, new AOEShapeCone(9f, 55f.Degrees()))
{
    private readonly BeastlyAura? beastlyAura = module.FindComponent<BeastlyAura>();

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (beastlyAura == null || beastlyAura.knockbacks.Count > 0)
        {
            return;
        }

        base.AddHints(slot, actor, hints);
    }

    // Set the cleave aoe to be 1.5f so it doesn't overlap with really bad mechanics such as the cage - getting hitting by the cleave is fine
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (beastlyAura == null || beastlyAura.knockbacks.Count > 0)
        {
            return;
        }

        foreach (var (origin, target, angle) in OriginsAndTargets())
        {
            if (actor != target)
            {
                hints.AddForbiddenZone(Shape, origin.Position, angle, WorldState.FutureTime(1.5d));
            }
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

sealed class BeastlyAura(BossModule module) : Components.GenericKnockback(module)
{
    public readonly List<Knockback> knockbacks = [];
    private static readonly AOEShapeRect shape = new(80f, 40f);
    private ShapeDistance distance;
    private const float knockbackDistance = 20.0f;

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
                ? new SDKnockbackInAABBRectLeftRightAlongZAxis(Arena.Center, knockbackDistance, 10f, 24.0f)
                : new SDKnockbackInAABBRectLeftRightAlongXAxis(Arena.Center, knockbackDistance, 10f, 24.0f);
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

        ref readonly var kb = ref knockbacks.Ref(0);
        var activation = kb.Activation;

        if (!IsImmune(slot, activation))
        {
            hints.AddForbiddenZone(distance, activation);
            // Handles the sides of the center of the map (left & right)
            hints.AddForbiddenZone(new SDRect(Arena.Center + new WDir(-6.25f, 0f), default(Angle), 20f, 20f, 3.75f), activation);
            hints.AddForbiddenZone(new SDRect(Arena.Center + new WDir(6.25f, 0f), default(Angle), 20f, 20f, 3.75f), activation);
        }
    }
}

sealed class Rush(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeRect shape = new(50f, 2.5f);
    private const double TimeWindowInSeconds = 1d;

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.EphemeralBlade && id == 4562)
        {
            aoes.Add(new(shape, actor.Position, actor.Rotation, WorldState.FutureTime(15d)));
            SortHelpers.SortAOEByActivation(aoes);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Rush)
        {
            var index = IndexOfClosestAOE(aoes, caster.Position);
            if (index >= 0)
            {
                aoes.RemoveAt(index);
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

        var nextAOEs = CollectionsMarshal.AsSpan(aoes);
        var aoeGroups = 0; // number of aoes that are grouped together, e.g. first group has 4 aoes, second group has 3 aoes
        var lastAOE = 0; // number of aoes we want to show

        while (lastAOE < nextAOEs.Length)
        {
            // Groups the aoes by their activation time base on the TimeWindowInSecond
            if (lastAOE > 0 && nextAOEs[lastAOE].Activation > nextAOEs[lastAOE - 1].Activation.AddSeconds(TimeWindowInSeconds))
            {
                ++aoeGroups;
            }

            // Only show the first two groups of AOEs
            if (aoeGroups == 2)
            {
                break;
            }

            nextAOEs[lastAOE++].Color = aoeGroups == 0 ? Colors.Danger : default;
        }

        return nextAOEs[..lastAOE];
    }
}

// Used to set up fake aoes to get the player to stand correctly for the mechanic
sealed class OverpoweringPointFake(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeRect shape = new(40f, 1f);
    private readonly DeadlyDemesne? deadlyDemesne = module.FindComponent<DeadlyDemesne>();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.OverpoweringPoint2 or (uint)AID.OverpoweringPoint3)
        {
            aoes.Clear();
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.PointMaker)
        {
            var center = Arena.Center;
            var color = Colors.SafeFromAOE;
            aoes.Add(new(shape, center + new WDir(0f, 6.0f), Angle.AnglesCardinals[1], color: color, risky: false));
            aoes.Add(new(shape, center + new WDir(0f, -6.0f), Angle.AnglesCardinals[2], color: color, risky: false));
        }
    }

    // Draw on the front since it can overlap with other mechanics
    public override void DrawArenaBackground(int pcSlot, Actor pc) { }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (deadlyDemesne == null || deadlyDemesne.greenAOEs.Count > 0 || deadlyDemesne.yellowAOEs.Count > 0)
        {
            return;
        }

        var aoes = ActiveAOEs(pcSlot, pc);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var c = ref aoes[i];
            c.Shape.Draw(Arena, c.Origin, c.Rotation, c.Color);
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (deadlyDemesne == null || deadlyDemesne.greenAOEs.Count > 0 || deadlyDemesne.yellowAOEs.Count > 0)
        {
            return [];
        }

        return CollectionsMarshal.AsSpan(aoes);
    }
}
sealed class OverpoweringPoint(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.OverpoweringPoint2, (uint)AID.OverpoweringPoint3],
    new AOEShapeRect(60f, 3f));

sealed class DeadlyDemesne(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCircle yellowCircle = new(12f);
    private readonly AOEShapeRect yellowRect = new(2.5f, 2.5f, 2.5f);
    private readonly AOEShapeCross greenCross = new(15f, 5f);
    private readonly AOEShapeRect greenRect = new(5f, 5f, 5f);
    private const double riskyWindow = 6d;
    private readonly Rush? rush = module.FindComponent<Rush>();

    // All AOEs are separate due to how the aoes can either spawn in from renderflag only or EAnim
    public readonly List<AOEInstance> greenAOEs = [];
    public readonly List<AOEInstance> yellowAOEs = [];
    private readonly List<AOEInstance> yellowCages = [];
    private readonly List<AOEInstance> greenCages = [];

    private enum ColourType { NONE, GREEN, YELLOW };
    private ColourType colourFirst = ColourType.NONE;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DeadlyDemesne)
        {
            colourFirst = ColourType.NONE;
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is var oid && oid == (uint)OID.DeadlyDemesneGreen)
        {
            var pos = actor.Position.Quantized();
            greenAOEs.Add(new(greenCross, pos, actor.Rotation, WorldState.FutureTime(12.1d)));
            greenCages.Add(new(greenRect, pos, actor.Rotation, WorldState.FutureTime(12d), color: Colors.Danger));
            SortHelpers.SortAOEByActivation(greenAOEs);
        }
        else if (oid == (uint)OID.DeadlyDemesneYellow)
        {
            var pos = actor.Position.Quantized();
            yellowAOEs.Add(new(yellowCircle, pos, default, WorldState.FutureTime(11.1d)));
            yellowCages.Add(new(yellowRect, pos, actor.Rotation, WorldState.FutureTime(11d), color: Colors.Danger));
            SortHelpers.SortAOEByActivation(yellowAOEs);
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID is var oid && oid is not (uint)OID.DeadlyDemesneGreen and not (uint)OID.DeadlyDemesneYellow)
        {
            return;
        }

        if (state == 0x00010002u && colourFirst == ColourType.NONE)
        {
            if (oid == (uint)OID.DeadlyDemesneGreen)
            {
                colourFirst = ColourType.GREEN;
            }
            else if (oid == (uint)OID.DeadlyDemesneYellow)
            {
                colourFirst = ColourType.YELLOW;
            }
        }
        else if (state == 0x00040008u)
        {
            var cages = oid == (uint)OID.DeadlyDemesneGreen ? greenCages : yellowCages;
            var index = IndexOfClosestAOE(cages, actor.Position);
            if (index >= 0)
            {
                cages.RemoveAt(index);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is var id && id is (uint)AID.LifeClaim or (uint)AID.CageOfMoltenMetal)
        {
            var list = id == (uint)AID.LifeClaim ? greenAOEs : yellowAOEs;
            var index = IndexOfClosestAOE(list, caster.Position);
            if (index >= 0)
            {
                list.RemoveAt(index);
            }
        }
    }

    private readonly List<AOEInstance> _colourAOEsBuffer = [];
    private readonly List<AOEInstance> _incomingAOEsBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var incomingAOEs = _incomingAOEsBuffer; // Combines the cage & standard AOEs
        incomingAOEs.Clear();

        // green cages AOEs
        var greenCageCount = greenCages.Count;
        if (greenCageCount != 0)
        {
            incomingAOEs.AddRange(greenCages);
        }

        // yellow cages AOEs
        var yellowCageCount = yellowCages.Count;
        if (yellowCageCount != 0)
        {
            incomingAOEs.AddRange(yellowCages);
        }

        // standard aoes
        var count = greenAOEs.Count + yellowAOEs.Count;
        if (count == 0)
        { // If there are no standard aoes we can just return the cages
            return CollectionsMarshal.AsSpan(incomingAOEs);
        }

        // If rush is active then we will only show the cages since standing in a cage is worse then getting hit by a rush aoe
        if (rush == null || rush.aoes.Count > 7)
        {
            return CollectionsMarshal.AsSpan(incomingAOEs);
        }

        // If the total number of actors is > 4 it means it both colours and we have to wait to know which one is first
        var total = greenAOEs.Count + yellowAOEs.Count;
        if (total > 4 && colourFirst == ColourType.NONE)
        {
            return CollectionsMarshal.AsSpan(incomingAOEs);
        }

        var colourAOEs = _colourAOEsBuffer;
        colourAOEs.Clear();

        // If colourFirst is not set then it means we are only dealing with one colour
        if (colourFirst == ColourType.NONE)
        {
            colourAOEs.AddRange(greenAOEs);
            colourAOEs.AddRange(yellowAOEs);
        }

        // Otherwie if colourFirst is set then we only add whichever type has aoes until it hits 0 then add the other type
        if (colourFirst == ColourType.GREEN)
        {
            colourAOEs.AddRange(greenAOEs.Count != 0 ? greenAOEs : yellowAOEs);
        }

        if (colourFirst == ColourType.YELLOW)
        {
            colourAOEs.AddRange(yellowAOEs.Count != 0 ? yellowAOEs : greenAOEs);
        }

        var standardAOEs = CollectionsMarshal.AsSpan(colourAOEs);
        var time = WorldState.CurrentTime;

        for (var i = 0; i < standardAOEs.Length; ++i)
        {
            ref var aoe = ref standardAOEs[i];
            aoe.Risky = aoe.Activation == default || aoe.Activation <= time.AddSeconds(riskyWindow);
        }

        incomingAOEs.AddRange(standardAOEs);
        return CollectionsMarshal.AsSpan(incomingAOEs);
    }
}

sealed class MoltenMetalBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6.0f), (uint)IconID.MoltenMetalIcon, centerAtTarget: true)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MoltenMetalBaitCircle)
        {
            CurrentBaits.Clear();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Count == 0)
        {
            return;
        }

        hints.Add("Bait far away on one side of the map!");
    }

    private readonly WPos[] baitSpots = [new(520f, -440f), new(520f, -400f)];

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentBaits.Count == 0 || !IsBaitTarget(actor))
        {
            return;
        }

        for (var i = 0; i < baitSpots.Length; ++i)
        {
            hints.GoalZones.Add(AIHints.GoalSingleTarget(baitSpots[i], 2f, 5f));
        }
    }
}
sealed class MoltenMetalBaitAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MoltenMetalBaitCircle, 6f);

sealed class BeastlyFlare(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BeastlyFlare, 30f)
{
    private readonly DeadlyDemesne? deadlyDemesne = module.FindComponent<DeadlyDemesne>();

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (deadlyDemesne == null || deadlyDemesne.greenAOEs.Count > 0 || deadlyDemesne.yellowAOEs.Count > 0)
        {
            return [];
        }

        return base.ActiveAOEs(slot, actor);
    }
}

sealed class MagicalCombustion : Components.SimpleAOEs
{
    public MagicalCombustion(BossModule module) : base(module, (uint)AID.MagicalCombustion, 8f)
    {
        Color = Colors.Danger;
    }
}

sealed class UnseenForce(BossModule module) : Components.GenericKnockback(module)
{
    public readonly List<(Angle Offset, DateTime Activation)> Forces = [];
    private readonly List<Knockback> _knockbacks = [];
    private DateTime pendingActivation;
    private const float knockbackDistance = 40f;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.UnseenForce2 or (uint)AID.UnseenForce3 && WorldState.Actors.Find(spell.MainTargetID) is Actor target)
        {
            Forces.Add(((target.Rotation - spell.Rotation).Normalized(), pendingActivation));
        }
        else if (spell.Action.ID == (uint)AID.Shockwave)
        {
            Forces.Clear();
            pendingActivation = default;
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.UnseenForce && actor.Type == ActorType.Player)
        {
            pendingActivation = status.ExpireAt;
            var forces = CollectionsMarshal.AsSpan(Forces);
            for (var i = 0; i < forces.Length; ++i)
            {
                forces[i].Activation = status.ExpireAt;
            }
        }
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        _knockbacks.Clear();
        var count = Forces.Count;
        for (var i = 0; i < count; ++i)
        {
            var (offset, activation) = Forces[i];
            if (activation != default)
            {
                _knockbacks.Add(new(actor.Position, knockbackDistance, activation, null, actor.Rotation + offset, Kind.DirForward, ignoreImmunes: true));
            }
        }
        return CollectionsMarshal.AsSpan(_knockbacks);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Forces.Count;
        for (var i = 0; i < count; ++i)
        {
            var (offset, activation) = Forces[i];
            if (activation == default)
            {
                continue;
            }

            hints.AddForbiddenZone(new SDCircle(Arena.Center, 20f), activation);
            var wantedDir = (Arena.Center - actor.Position).ToAngle() + offset;
            hints.ForbiddenDirections.Add(((wantedDir + 180f.Degrees()).Normalized(), 178f.Degrees(), activation));
        }
    }
}

sealed class SearingAxe(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCircle shape = new(15f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SearingAxeBoss)
        {
            aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SearingAxeFinish)
        {
            if (aoes.Count > 0)
            {
                aoes.RemoveAt(0);
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

        if (aoes[0].Activation > WorldState.CurrentTime.AddSeconds(5.0f))
        {
            return [];
        }

        return CollectionsMarshal.AsSpan(aoes);
    }
}

sealed class GuttlerGlutter(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.GuttlerGlutterBoss, 20f)
{
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            return [];
        }

        var kbs = CollectionsMarshal.AsSpan(Casters);
        if (kbs[0].Activation > WorldState.CurrentTime.AddSeconds(5d))
        {
            return [];
        }

        return kbs;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GuttlerGlutterFinish)
        {
            if (Casters.Count > 0)
            {
                Casters.RemoveAt(0);
            }
        }
    }
}

sealed class Gyrocleave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Gyrocleave, new AOEShapeRect(80f, 10f))
{
    private readonly OverpoweringPointFake? overpoweringPointFake = module.FindComponent<OverpoweringPointFake>();
    private readonly Rush? rush = module.FindComponent<Rush>();

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (overpoweringPointFake == null || overpoweringPointFake.aoes.Count > 0)
        {
            return;
        }

        base.AddHints(slot, actor, hints);
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (rush == null || rush.aoes.Count > 7)
        {
            return [];
        }

        return base.ActiveAOEs(slot, actor);
    }
}

sealed class LaudaTheSpellcleaverStates : StateMachineBuilder
{
    public LaudaTheSpellcleaverStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BeastlyAura>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<DeadlyDemesne>()
            .ActivateOnEnter<Thunderbolt>()
            .ActivateOnEnter<OverpoweringPointFake>()
            .ActivateOnEnter<OverpoweringPoint>()
            .ActivateOnEnter<Gyrocleave>()
            .ActivateOnEnter<MoltenMetalBait>()
            .ActivateOnEnter<MoltenMetalBaitAOE>()
            .ActivateOnEnter<BeastlyFlare>()
            .ActivateOnEnter<GluttonousGutting>()
            .ActivateOnEnter<GluttonousGoring>()
            .ActivateOnEnter<MagicalCombustion>()
            .ActivateOnEnter<UnseenForce>()
            .ActivateOnEnter<SearingAxe>()
            .ActivateOnEnter<GuttlerGlutter>()
            .ActivateOnEnter<InfernalPain>()
            .ActivateOnEnter<AutoAttack>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.LaudaTheSpellcleaver, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1092u, NameID = 14693u, SortOrder = 14)]
[PreferredKinship(BST.Kinship.Vile, "Ignore Beastly Aura, Unseen Force and Guttler Glutter knockbacks")]
public sealed class LaudaTheSpellcleaver : BossModule
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.MagickedBlade => 3,
                (uint)OID.IndefatigableBlade => 3,
                (uint)OID.ThanatosPiece => 2,
                (uint)OID.LaudaTheSpellcleaver => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.ThanatosPiece));
        Arena.Actors(Enemies((uint)OID.MagickedBlade));
        Arena.Actors(Enemies((uint)OID.IndefatigableBlade));
    }

    public LaudaTheSpellcleaver(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private LaudaTheSpellcleaver(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

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
