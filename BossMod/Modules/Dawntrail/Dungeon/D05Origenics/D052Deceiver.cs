namespace BossMod.Dawntrail.Dungeon.D05Origenics.D052Deceiver;

public enum OID : uint
{
    Boss = 0x4170, // R5.0
    Cahciua = 0x418F, // R0.96
    OrigenicsSentryG91 = 0x4172, // R0.9
    OrigenicsSentryG92 = 0x4171, // R0.9
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 873, // OrigenicsSentryG92->player, no cast, single-target
    Teleport = 36362, // Boss->location, no cast, single-target

    Electrowave = 36371, // Boss->self, 5.0s cast, range 72 circle, raidwide

    BionicThrashVisual1 = 36369, // Boss->self, 7.0s cast, single-target
    BionicThrashVisual2 = 36368, // Boss->self, 7.0s cast, single-target
    BionicThrash = 36370, // Helper->self, 8.0s cast, range 30 90-degree cone

    InitializeAndroids = 36363, // Boss->self, 4.0s cast, single-target, spawns OrigenicsSentryG91 and OrigenicsSentryG92

    SynchroshotFake = 36373, // OrigenicsSentryG91->self, 5.0s cast, range 40 width 4 rect
    SynchroshotReal = 36372, // OrigenicsSentryG92->self, 5.0s cast, range 40 width 4 rect

    InitializeTurretsVisual = 36364, // Boss->self, 4.0s cast, single-target
    InitializeTurretsFake = 36426, // Helper->self, 4.7s cast, range 4 width 10 rect
    InitializeTurretsReal = 36365, // Helper->self, 4.7s cast, range 4 width 10 rect

    LaserLashReal = 36366, // Helper->self, 5.0s cast, range 40 width 10 rect
    LaserLashFake = 38807, // Helper->self, 5.0s cast, range 40 width 10 rect

    SurgeNPCs = 39736, // Helper->self, 8.5s cast, range 40 width 40 rect, knockback 15 dir left/right, only seems to apply to NPCs
    Surge = 36367, // Boss->location, 8.0s cast, range 40 width 40 rect, knockback 30 dir left/right

    Electray = 38320 // Helper->player, 8.0s cast, range 5 circle
}

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private const float HalfWidth = 5.5f; // adjusted for 0.5 player hitbox
    public static readonly WPos ArenaCenter = new(-172f, -142f);
    public static readonly ArenaBoundsSquare StartingBounds = new(24.5f);
    private static readonly ArenaBoundsSquare defaultBounds = new(20f);
    private static readonly Square[] defaultSquare = [new(ArenaCenter, 20f)];
    private static readonly AOEShapeCustom square = new([new Square(ArenaCenter, 25f)], defaultSquare);
    private const float XWest2 = -187.5f, XEast2 = -156.5f;
    private const float XWest1 = -192f, XEast1 = -152f, ZRow1 = -127f, ZRow2 = -137f, ZRow3 = -147f, ZRow4 = -157f;
    public static readonly Dictionary<byte, ArenaBoundsCustom> ArenaBoundsMap = InitializeArenaBounds();
    private static RectangleSE[] CreateRows(float x1, float x2)
    => [
        new(new(x1, ZRow4), new(x2, ZRow4), HalfWidth),
        new(new(x1, ZRow3), new(x2, ZRow3), HalfWidth),
        new(new(x1, ZRow2), new(x2, ZRow2), HalfWidth),
        new(new(x1, ZRow1), new(x2, ZRow1), HalfWidth),
    ];
    private static Dictionary<byte, ArenaBoundsCustom> InitializeArenaBounds()
    {
        var westRows = CreateRows(XWest1, XWest2);
        var eastRows = CreateRows(XEast1, XEast2);

        return new Dictionary<byte, ArenaBoundsCustom>
        {
            { 0x2A, new(defaultSquare, [westRows[1], westRows[3]]) },
            { 0x1B, new(defaultSquare, [westRows[1], westRows[3], eastRows[0], eastRows[2]]) },
            { 0x2C, new(defaultSquare, [westRows[1], westRows[2]]) },
            { 0x1E, new(defaultSquare, [westRows[1], westRows[2], eastRows[0], eastRows[3]]) },
            { 0x2D, new(defaultSquare, [westRows[0], westRows[3]]) },
            { 0x1D, new(defaultSquare, [westRows[0], westRows[3], eastRows[1], eastRows[2]]) },
            { 0x2B, new(defaultSquare, [westRows[0], westRows[2]]) },
            { 0x1C, new(defaultSquare, [westRows[0], westRows[2], eastRows[1], eastRows[3]]) }
        };
    }

    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Electrowave && Arena.Bounds == StartingBounds)
        {
            _aoe = [new(square, Arena.Center, default, Module.CastFinishAt(spell, 0.7d))];
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001u)
        {
            if (ArenaBoundsMap.TryGetValue(index, out var value))
            {
                Arena.Bounds = value;
            }
            else if (index == 0x12)
            {
                Arena.Bounds = defaultBounds;
                _aoe = [];
            }
        }
        else if (state == 0x00080004u)
            Arena.Bounds = defaultBounds;
    }
}

sealed class Electrowave(BossModule module) : Components.RaidwideCast(module, (uint)AID.Electrowave);
sealed class BionicThrash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BionicThrash, new AOEShapeCone(30f, 45f.Degrees()));
sealed class Synchroshot(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SynchroshotReal, new AOEShapeRect(40f, 2f));
sealed class InitializeTurrets(BossModule module) : Components.SimpleAOEs(module, (uint)AID.InitializeTurretsReal, new AOEShapeRect(4f, 5f));
sealed class LaserLash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LaserLashReal, new AOEShapeRect(40f, 5f));
sealed class Electray(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Electray, 5f);

sealed class Surge(BossModule module) : Components.GenericKnockback(module)
{
    private readonly List<Knockback> _kbs = new(2);
    private const float XWest = -187.5f, XEast = -156.5f;
    private const float ZRow1 = -122f, ZRow2 = -132f, ZRow3 = -142f, ZRow4 = -152f, ZRow5 = -162f;
    private static readonly WDir offset = new(4f, default);
    private static readonly SafeWall[] walls2A1B = [new(new(XWest, ZRow3), new(XWest, ZRow4)), new(new(XWest, ZRow1), new(XWest, ZRow2)),
    new(new(XEast, ZRow4), new(XEast, ZRow5)), new(new(XEast, ZRow2), new(XEast, ZRow3))];
    private static readonly SafeWall[] walls2C1E = [new(new(XWest, ZRow3), new(XWest, ZRow4)), new(new(XWest, ZRow2), new(XWest, ZRow3)),
    new(new(XEast, ZRow4), new(XEast, ZRow5)), new(new(XEast, ZRow1), new(XEast, ZRow2))];
    private static readonly SafeWall[] walls2D1D = [new(new(XWest, ZRow4), new(XWest, ZRow5)), new(new(XWest, ZRow1), new(XWest, ZRow2)),
    new(new(XEast, ZRow3), new(XEast, ZRow4)), new(new(XEast, ZRow2), new(XEast, ZRow3))];
    private static readonly SafeWall[] walls2B1C = [new(new(XWest, ZRow4), new(XWest, ZRow5)), new(new(XWest, ZRow2), new(XWest, ZRow3)),
    new(new(XEast, ZRow3), new(XEast, ZRow4)), new(new(XEast, ZRow1), new(XEast, ZRow2))];
    private static readonly AOEShapeCone _shape = new(60f, 90f.Degrees());

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(_kbs);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddSource(Angle offset, SafeWall[] safeWalls)
            => _kbs.Add(new(caster.Position, 30f, Module.CastFinishAt(spell), _shape, spell.Rotation + offset, Kind.DirForward, default, safeWalls));
        if (spell.Action.ID == (uint)AID.Surge)
        {
            var safewalls = GetActiveSafeWalls();
            AddSource(90f.Degrees(), safewalls);
            AddSource(-90f.Degrees(), safewalls);
        }
    }

    public SafeWall[] GetActiveSafeWalls()
    {
        foreach (var kvp in ArenaChanges.ArenaBoundsMap)
        {
            if (Arena.Bounds == kvp.Value)
            {
                return kvp.Key switch
                {
                    0x1B => walls2A1B,
                    0x1E => walls2C1E,
                    0x1D => walls2D1D,
                    0x1C => walls2B1C,
                    _ => []
                };
            }
        }
        return [];
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Surge)
        {
            _kbs.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kbs.Count != 0)
        {
            var safewalls = GetActiveSafeWalls();
            var forbidden = new ShapeDistance[4];

            var centerX = Arena.Center.X;
            for (var i = 0; i < 4; ++i)
            {
                var safeWall = safewalls[i];
                forbidden[i] = new SDInvertedRect(new(centerX, safeWall.Vertex1.Z - 5f), safeWall.Vertex1.X == XWest ? -offset : offset, 10f, default, 20f);
            }
            hints.AddForbiddenZone(new SDIntersection(forbidden), _kbs.Ref(0).Activation);
        }
    }
}

sealed class SurgeHint(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(15.5f, 5);
    private readonly List<AOEInstance> _hints = new(4);
    private readonly Surge _kb = module.FindComponent<Surge>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_hints);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Surge)
        {
            var activeSafeWalls = _kb.GetActiveSafeWalls();
            var centerX = Arena.Center.X;
            for (var i = 0; i < 4; ++i)
            {
                var safewall = activeSafeWalls[i].Vertex1;
                _hints.Add(new(rect, new(centerX, safewall.Z - 5f), (safewall.X == -187.5f ? -1f : 1f) * 90f.Degrees(), default, Colors.SafeFromAOE, false));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Surge)
            _hints.Clear();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var count = _hints.Count;
        if (count != 0)
        {
            var isPositionSafe = false;
            var hintz = CollectionsMarshal.AsSpan(_hints);
            for (var i = 0; i < count; ++i)
            {
                ref var hint = ref hintz[i];
                if (hint.Check(actor.Position))
                {
                    isPositionSafe = true;
                    break;
                }
            }
            hints.Add("Wait inside safespot for knockback!", !isPositionSafe);
        }
    }
}

sealed class D052DeceiverStates : StateMachineBuilder
{
    public D052DeceiverStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<Electrowave>()
            .ActivateOnEnter<BionicThrash>()
            .ActivateOnEnter<Synchroshot>()
            .ActivateOnEnter<InitializeTurrets>()
            .ActivateOnEnter<LaserLash>()
            .ActivateOnEnter<Electray>()
            .ActivateOnEnter<Surge>()
            .ActivateOnEnter<SurgeHint>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 825, NameID = 12693, SortOrder = 3)]
public sealed class D052Deceiver(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaChanges.ArenaCenter, ArenaChanges.StartingBounds)
{
    private static readonly uint[] adds = [(uint)OID.OrigenicsSentryG92, (uint)OID.OrigenicsSentryG91];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, adds);
    }
}
