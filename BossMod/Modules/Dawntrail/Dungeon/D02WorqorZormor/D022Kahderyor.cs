namespace BossMod.Dawntrail.Dungeon.D02WorqorZormor.D022Kahderyor;

public enum OID : uint
{
    Boss = 0x415D, // R7.0
    CrystallineDebris = 0x415E, // R1.4
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    WindUnbound = 36282, // Boss->self, 5.0s cast, range 60 circle

    CrystallineCrushVisual = 36285, // Boss->location, 5.0+1.0s cast, single-target
    CrystallineCrush = 36153, // Helper->self, 6.3s cast, range 6 circle, tower

    WindShotVisual1 = 36284, // Boss->self, 5.5s cast, single-target
    WindshotVisual2 = 36300, // Helper->player, no cast, single-target
    WindShot = 36296, // Helper->players, 6.0s cast, range 5-10 donut, stack

    EarthenShotVisual1 = 36283, // Boss->self, 5.0+0.5s cast, single-target
    EarthenShotVisual2 = 36299, // Helper->player, no cast, single-target
    EarthenShot = 36295, // Helper->player, 6.0s cast, range 6 circle, spread

    CrystallineStormVisual = 36286, // Boss->self, 3.0+1.0s cast, single-target
    CrystallineStorm = 36290, // Helper->self, 4.0s cast, range 50 width 2 rect

    SeedCrystalsVisual = 36291, // Boss->self, 4.5+0.5s cast, single-target
    SeedCrystals = 36298, // Helper->player, 5.0s cast, range 6 circle, spread

    SharpenedSights = 36287, // Boss->self, 3.0s cast, single-target
    EyeOfTheFierce = 36297, // Helper->self, 5.0s cast, range 60 circle

    StalagmiteCircleVisual = 36288, // Boss->self, 5.0s cast, single-target
    StalagmiteCircle = 36293, // Helper->self, 5.0s cast, range 15 circle

    CyclonicRingVisual = 36289, // Boss->self, 5.0s cast, single-target
    CyclonicRing = 36294 // Helper->self, 5.0s cast, range 8-40 donut
}

public enum IconID : uint
{
    WindShot = 511 // player
}

sealed class WindEarthShot(BossModule module) : Components.GenericAOEs(module)
{
    private const string Hint = "Be inside a crystal line!";
    private static readonly AOEShapeDonut donut = new(8f, 50f);
    private static readonly AOEShapeCircle circle = new(15f);
    private static readonly Angle[] angles = [119.997f.Degrees(), 29.996f.Degrees(), -80.001f.Degrees(), 99.996f.Degrees()];
    private static readonly WPos[] positions = [new(-43, -57), new(-63, -57), new(-53, -47), new(-53, -67)];
    private const float Length = 50f;
    private static readonly AOEShapeCustom ENVC21Inverted = CreateShape(positions[1], positions[2], positions[3], angles[1], angles[0], angles[2], 1, true);
    private static readonly AOEShapeCustom ENVC21 = CreateShape(positions[1], positions[2], positions[3], angles[1], angles[0], angles[2], 7);
    private static readonly AOEShapeCustom ENVC20Inverted = CreateShape(positions[0], positions[2], positions[3], angles[1], angles[3], angles[0], 1, true);
    private static readonly AOEShapeCustom ENVC20 = CreateShape(positions[0], positions[2], positions[3], angles[1], angles[3], angles[0], 7);
    public AOEInstance[] AOE = [];
    private static readonly WDir am40 = -40f.Degrees().ToDirection(), a0 = new(0f, 1f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOE;

    public override void OnMapEffect(byte index, uint state)
    {
        if (state is 0x00800040u or 0x00200010u)
        {
            AddAOE(index, state, state == 0x00800040u ? donut : circle);
        }
        else if (state is 0x02000001u or 0x04000004u or 0x08000004u or 0x01000001u)
        {
            AOE = [];
        }
    }

    private void AddAOE(byte index, uint state, AOEShape shape)
    {
        var activation = WorldState.FutureTime(5.9d);
        var color = state == 0x00800040u ? Colors.SafeFromAOE : default;
        AOE = index switch
        {
            0x1E => [new(shape, positions[0], default, activation)],
            0x1F => [new(shape, positions[1], default, activation)],
            0x20 => [new(state == 0x00800040u ? ENVC20Inverted : ENVC20, Arena.Center, default, activation, color)],
            0x21 => [new(state == 0x00800040u ? ENVC21Inverted : ENVC21, Arena.Center, default, activation, color)],
            _ => AOE
        };
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (AOE.Length == 0)
        {
            return;
        }
        ref var aoe = ref AOE[0];
        var aoeShape = aoe.Shape;
        if (aoeShape != ENVC20Inverted && aoeShape != ENVC21Inverted)
        {
            base.AddHints(slot, actor, hints);
        }
        else
        {
            hints.Add(Hint, !aoe.Check(actor.Position));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (AOE.Length == 0)
        {
            return;
        }
        ref var aoe = ref AOE[0];
        base.AddAIHints(slot, actor, assignment, hints);

        var aoeShape = aoe.Shape;
        var containsENVC20 = aoeShape == ENVC20;
        var containsENVC21 = aoeShape == ENVC21;

        if (containsENVC20 || containsENVC21)
        {
            ShapeDistance forbiddenZone = actor.Role != Role.Tank
                ? new SDRect(Arena.Center, containsENVC20 ? am40 : a0, 20f, containsENVC20 ? 1f : 10f, 20f)
                : new SDInvertedCircle(Arena.Center, 12f);

            hints.AddForbiddenZone(forbiddenZone, aoe.Activation);
        }
    }

    private static AOEShapeCustom CreateShape(WPos pos1, WPos pos2, WPos pos3, Angle angle1, Angle angle2, Angle angle3, int halfWidth, bool inverted = false)
        => new([new Rectangle(pos1, halfWidth, Length, angle1), new Rectangle(pos2, halfWidth, Length, angle2), new Rectangle(pos3, halfWidth, Length, angle3)],
        invertForbiddenZone: inverted);
}

sealed class WindShotStack(BossModule module) : Components.DonutStack(module, (uint)AID.WindShot, (uint)IconID.WindShot, 5f, 10f, 6f, 4, 4)
{
    private readonly WindEarthShot _aoe = module.FindComponent<WindEarthShot>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Stacks.Count == 0)
            return;

        ref var aoe = ref _aoe.AOE[0];
        var forbidden = new List<ShapeDistance>(3);
        var party = Raid.WithoutSlot(false, true, true);
        var len = party.Length;
        for (var i = 0; i < len; ++i)
        {
            var p = party[i];
            if (p == actor)
            {
                continue;
            }

            var addForbidden = false;
            if (aoe.Shape is AOEShapeDonut && !aoe.Check(p.Position) || aoe.Shape is AOEShapeCustom && aoe.Check(p.Position))
            {
                addForbidden = true;
            }
            if (addForbidden)
            {
                forbidden.Add(new SDInvertedCircle(p.Position, 1.66f));
            }
        }

        if (forbidden.Count != 0)
        {
            hints.AddForbiddenZone(new SDIntersection([.. forbidden]), Stacks.Ref(0).Activation);
        }
    }
}

sealed class WindUnbound(BossModule module) : Components.RaidwideCast(module, (uint)AID.WindUnbound);
sealed class CrystallineCrush(BossModule module) : Components.CastTowers(module, (uint)AID.CrystallineCrush, 6f, 4, 4);
sealed class EarthenShot(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.EarthenShot, 6f);
sealed class StalagmiteCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StalagmiteCircle, 15f);
sealed class CrystallineStorm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CrystallineStorm, new AOEShapeRect(50f, 1f));
sealed class CyclonicRing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CyclonicRing, new AOEShapeDonut(8f, 40f));
sealed class EyeOfTheFierce(BossModule module) : Components.CastGaze(module, (uint)AID.EyeOfTheFierce);
sealed class SeedCrystals(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.SeedCrystals, 6f);

sealed class D022KahderyorStates : StateMachineBuilder
{
    public D022KahderyorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WindEarthShot>()
            .ActivateOnEnter<WindShotStack>()
            .ActivateOnEnter<WindUnbound>()
            .ActivateOnEnter<CrystallineStorm>()
            .ActivateOnEnter<CrystallineCrush>()
            .ActivateOnEnter<EarthenShot>()
            .ActivateOnEnter<StalagmiteCircle>()
            .ActivateOnEnter<CyclonicRing>()
            .ActivateOnEnter<EyeOfTheFierce>()
            .ActivateOnEnter<SeedCrystals>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 824, NameID = 12703)]
public sealed class D022Kahderyor(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultBounds.Center, DefaultBounds)
{
    public static readonly ArenaBoundsCustom DefaultBounds = new([new Polygon(new(-53f, -57f), 19.5f, 40)], [new Rectangle(new(-72.5f, -57f), 0.75f, 20), new Rectangle(new(-53f, -37f), 20f, 1.5f)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.CrystallineDebris), Colors.Object);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.CrystallineDebris => 1,
                _ => 0
            };
        }
    }
}
