namespace BossMod.Endwalker.Dungeon.D05Aitiascope.D052Rhitahtyn;

public enum OID : uint
{
    Boss = 0x346B, // R=9.0
    Crystal = 0x35DA, // R1.0
    TargetMarker1 = 0x346C, // R1.0
    TargetMarker2 = 0x346D, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Teleport = 25688, // Boss->location, no cast, single-target

    AnvilOfTartarus = 25686, // Boss->player, 5.0s cast, single-target, tankbuster
    Impact = 25679, // Helper->self, 4.0s cast, range 14 width 40 rect

    ShieldSkewer = 25680, // Boss->location, 11.0s cast, range 40 width 14 rect
    ShrapnelShellVisual = 25682, // Boss->self, 3.0s cast, single-target
    ShrapnelShellAOE = 25684, // Helper->location, 3.5s cast, range 5 circle
    TargetMarkerVisual = 25683, // TargetMarker1/TargetMarker2->self, no cast, single-target

    TartareanImpact = 25685, // Boss->self, 5.0s cast, range 60 circle, raidwide
    TartareanSpark = 25687, // Boss->self, 3.0s cast, range 40 width 6 rect

    Vexillatio = 25678 // Boss->self, 4.0s cast, single-target
}

class ArenaChanges(BossModule module) : BossComponent(module)
{
    public bool Safespots;
    private DateTime activation;
    public static readonly WPos ArenaCenter = new(11f, 144f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(19.5f);
    private static readonly WPos[] positions = [new(19.5f, 152f), new(2.5f, 152f), new(2.5f, 136f),
    new(19.5f, 136f)];
    private static readonly Rectangle[] squares = InitializeSquares();
    private static readonly Rectangle[] rect = [new(ArenaCenter, 6.5f, 19.5f)];
    private readonly List<Rectangle> union = new(2);

    private static Rectangle[] InitializeSquares()
    {
        var rects = new Rectangle[4];
        for (var i = 0; i < 4; ++i)
            rects[i] = new(positions[i], 2f, 1.5f);
        return rects;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x00)
            UpdateArena();
        else if (state == 0x00200010)
        {
            if (index == 0x01) // 0x01 and 0x04 always are a pair
                union.AddRange([squares[0], squares[2]]);
            else if (index == 0x02) // 0x02 and 0x03 always are a pair
                union.AddRange([squares[1], squares[3]]);

            Safespots = true;
            activation = WorldState.FutureTime(7.3d);
            UpdateArena();
        }
        else if (state == 0x00080004)
        {
            switch (index)
            {
                case 0x00:
                    union.Clear();
                    Arena.Bounds = DefaultBounds;
                    break;
                case 0x01:
                case 0x02:
                    Safespots = false;
                    break;
            }
        }
    }

    private void UpdateArena()
    {
        Arena.Bounds = new ArenaBoundsCustom([.. rect, .. union]);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {

        if (Safespots) // force AI to move to a safespot before it becomes available
        {
            var forbidden = new ShapeDistance[2];
            for (var i = 0; i < 2; ++i)
                forbidden[i] = new SDInvertedCircle(union[i].Center, 5f);
            hints.AddForbiddenZone(new SDIntersection(forbidden), activation);
        }
    }
}

class ShieldSkewer(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ShieldSkewer, new AOEShapeRect(40f, 7f))
{
    private readonly ArenaChanges _arena = module.FindComponent<ArenaChanges>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Casters.Count == 0)
            return [];
        return new AOEInstance[1] { Casters[0] with { Risky = !_arena.Safespots } };
    }
}

class Shrapnel(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(18);
    private static readonly AOEShapeCircle circle = new(6f);
    private static readonly WPos[] shrapnelPositionsSWNE =
        [
            new(-6.027f, 153.979f), new(7.98f, 160.998f), new(20.981f, 126.97f), new(7.98f, 146.99f), new(27.97f, 140.978f),
            new(20.981f, 140.978f), new(27.97f, 133.99f), new(13.992f, 140.978f), new(20.981f, 133.99f), new(0.992f, 146.99f),
            new(0.992f, 153.979f), new(-6.027f, 160.998f), new(13.992f, 126.97f), new(13.992f, 133.99f), new(0.992f, 160.998f),
            new(-6.027f, 146.99f), new(7.98f, 153.979f), new(27.97f, 126.97f)
        ];
    private static readonly WPos[] shrapnelPositionsNWSE =
        [
            new(20.981f, 160.998f), new(13.992f, 153.979f), new(-6.027f, 140.978f), new(13.992f, 160.998f), new(-6.027f, 126.97f),
            new(7.98f, 126.97f), new(0.992f, 133.99f), new(7.98f, 133.99f), new(-6.027f, 133.99f), new(27.97f, 153.979f),
            new(13.992f, 146.99f), new(20.981f, 146.99f), new(0.992f, 126.97f), new(0.992f, 140.978f), new(27.97f, 160.998f),
            new(27.97f, 146.99f), new(20.981f, 153.979f), new(7.98f, 140.978f)
        ];
    private static readonly WPos[] shrapnelPositionsW =
        [
            new(-6.027f, 160.998f), new(0.992f, 153.979f), new(7.98f, 133.99f), new(-6.027f, 146.99f), new(-6.027f, 140.978f),
            new(0.992f, 133.99f), new(-6.027f, 133.99f), new(7.98f, 126.97f), new(0.992f, 140.978f), new(7.98f, 153.979f),
            new(-6.027f, 153.979f), new(0.992f, 146.99f), new(0.992f, 126.97f), new(-6.027f, 126.97f), new(0.992f, 160.998f),
            new(7.98f, 160.998f), new(7.98f, 146.99f), new(7.98f, 140.978f)
        ];
    private static readonly WPos[] shrapnelPositionsE =
        [
            new(13.992f, 160.998f), new(20.981f, 146.99f), new(20.981f, 140.978f), new(27.97f, 146.99f), new(27.97f, 140.978f),
            new(20.981f, 133.99f), new(13.992f, 126.97f), new(27.97f, 126.97f), new(27.97f, 133.99f), new(13.992f, 146.99f),
            new(27.97f, 153.979f), new(20.981f, 153.979f), new(20.981f, 126.97f), new(13.992f, 133.99f), new(13.992f, 153.979f),
            new(27.97f, 160.998f), new(20.981f, 160.998f), new(13.992f, 140.978f)
        ];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void Update()
    {
        if (_aoes.Count == 0)
        {
            var marker1 = Module.Enemies((uint)OID.TargetMarker1);
            var count1 = marker1.Count;
            for (var i = 0; i < count1; ++i)
            {
                var marker = marker1[i];
                if (marker.PosRot.Z < 144f)
                {
                    if (marker.Rotation >= 0.1f.Degrees())
                        AddAOEs(shrapnelPositionsSWNE);
                    else if (marker.Rotation <= -0.1f.Degrees())
                        AddAOEs(shrapnelPositionsNWSE);
                    return;
                }
            }
            var marker2 = Module.Enemies((uint)OID.TargetMarker2);
            var count2 = marker2.Count;
            for (var i = 0; i < count2; ++i)
            {
                var marker = marker2[i];
                if (marker.PosRot.X < 11f)
                {
                    AddAOEs(shrapnelPositionsW);
                    return;
                }
                else if (marker.PosRot.X > 11f)
                {
                    AddAOEs(shrapnelPositionsE);
                    return;
                }
            }
        }
    }

    private void AddAOEs(WPos[] coords)
    {
        for (var i = 0; i < 18; ++i)
            _aoes.Add(new(circle, coords[i], default, WorldState.FutureTime(8d)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.ShrapnelShellAOE)
        {
            var count = _aoes.Count;
            var pos = spell.LocXZ;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].Origin.AlmostEqual(pos, 1f))
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

class Impact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Impact, new AOEShapeRect(14f, 20f));
class TartareanSpark(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TartareanSpark, new AOEShapeRect(40f, 3f));
class AnvilOfTartarus(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.AnvilOfTartarus);
class TartareanImpact(BossModule module) : Components.RaidwideCast(module, (uint)AID.TartareanImpact);

class D052RhitahtynStates : StateMachineBuilder
{
    public D052RhitahtynStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<ShieldSkewer>()
            .ActivateOnEnter<Impact>()
            .ActivateOnEnter<Shrapnel>()
            .ActivateOnEnter<TartareanSpark>()
            .ActivateOnEnter<AnvilOfTartarus>()
            .ActivateOnEnter<TartareanImpact>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 786, NameID = 10292)]
public class D052Rhitahtyn(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaChanges.ArenaCenter, ArenaChanges.DefaultBounds);
