namespace BossMod.Shadowbringers.Dungeon.D06Amaurot.D063Therion;

public enum OID : uint
{
    Boss = 0x27C1, // R=25.84
    TheFaceOfTheBeast = 0x27C3, // R=2.1
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 15574, // Boss->player, no cast, single-target

    ShadowWreck = 15587, // Boss->self, 4.0s cast, range 100 circle
    ApokalypsisFirst = 15575, // Boss->self, 6.0s cast, range 76 width 20 rect
    ApokalypsisRest = 15577, // Helper->self, no cast, range 76 width 20 rect
    TherionCharge = 15578, // Boss->location, 7.0s cast, range 100 circle, damage fall off AOE

    DeathlyRayVisualFaces1 = 15579, // Boss->self, 3.0s cast, single-target
    DeathlyRayVisualFaces2 = 16786, // Boss->self, no cast, single-target
    DeathlyRayVisualThereion1 = 17107, // Helper->self, 5.0s cast, range 80 width 6 rect
    DeathlyRayVisualThereion2 = 15582, // Boss->self, 3.0s cast, single-target
    DeathlyRayVisualThereion3 = 16785, // Boss->self, no cast, single-target

    DeathlyRayFacesFirst = 15580, // TheFaceOfTheBeast->self, no cast, range 60 width 6 rect
    DeathlyRayFacesRest = 15581, // Helper->self, no cast, range 60 width 6 rect
    DeathlyRayThereionFirst = 15583, // Helper->self, no cast, range 60 width 6 rect
    DeathlyRayThereionRest = 15585, // Helper->self, no cast, range 60 width 6 rect
    Misfortune = 15586 // Helper->location, 3.0s cast, range 6 circle
}

sealed class ShadowWreck(BossModule module) : Components.RaidwideCast(module, (uint)AID.ShadowWreck);
sealed class Misfortune(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Misfortune, 6f);
sealed class ThereionCharge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TherionCharge, 20f);

sealed class Border(BossModule module) : Components.GenericAOEs(module, warningText: "Platform will be removed during next Apokalypsis!")
{
    private const float MaxError = 5f;
    private static readonly AOEShapeRect _square = new(2f, 2f, 2f);
    private Apokalypsis? _aoe = module.FindComponent<Apokalypsis>();

    public readonly List<AOEInstance> BreakingPlatforms = new(2);

    private static readonly WPos[] positions = [new(-12f, -71f), new(12f, -71f), new(-12f, -51f),
    new(12f, -51f), new(-12f, -31f), new(12f, -31f), new(-12f, -17f), new(12f, -17f), new(default, -65f), new(default, -45f)];

    private static readonly Square[] alcoves = Squares();
    private static readonly Square[] differences = [new(positions[8], 10.01f), new(positions[9], 10.01f)];

    private static readonly Rectangle[] rect = [new(new(default, -45f), 10f, 30f)];
    public readonly List<Shape> UnionRefresh = Union();
    private readonly List<Shape> difference = new(8);
    public static readonly ArenaBoundsCustom DefaultArena = new([.. Union()], Offset: -1f);

    private static Square[] Squares()
    {
        var squares = new Square[8];
        var square = new Square(default, 2f);
        for (var i = 0; i < 8; ++i)
        {
            squares[i] = square with { Center = positions[i] };
        }
        return squares;
    }

    private static List<Shape> Union()
    {
        var union = new List<Shape>(rect);
        for (var i = 0; i < 8; ++i)
        {
            union.Add(alcoves[i]);
        }
        return union;
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = BreakingPlatforms.Count;
        if (count == 0)
        {
            return [];
        }
        _aoe ??= Module.FindComponent<Apokalypsis>();
        var aoes = CollectionsMarshal.AsSpan(BreakingPlatforms);
        for (var i = 0; i < count; ++i)
        {
            ref var p = ref aoes[i];
            aoes[i].Risky = _aoe!.NumCasts == 0;
        }
        return aoes;
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008u)
        {
            for (var i = 0; i < 8; ++i)
            {
                if (actor.Position.AlmostEqual(positions[i], MaxError))
                {
                    if (UnionRefresh.Remove(alcoves[i]))
                    {
                        if (UnionRefresh.Count == 7)
                            difference.Add(differences[0]);
                        else if (UnionRefresh.Count == 5)
                            difference.Add(differences[1]);
                        ArenaBoundsCustom arena = new([.. UnionRefresh], [.. difference], Offset: -1f);
                        Arena.Bounds = arena;
                        Arena.Center = arena.Center;
                    }
                    BreakingPlatforms.Remove(new(_square, positions[i], color: Colors.FutureVulnerable));
                }
            }
        }
        else if (state == 0x00100020u)
        {
            for (var i = 0; i < 8; ++i)
            {
                if (actor.Position.AlmostEqual(positions[i], MaxError))
                    BreakingPlatforms.Add(new(_square, positions[i], color: Colors.FutureVulnerable));
            }
        }
    }
}

sealed class Apokalypsis(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeRect _rect = new(76f, 10f);
    private readonly Border _arena = module.FindComponent<Border>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ApokalypsisFirst && (_arena.UnionRefresh.Count - _arena.BreakingPlatforms.Count) >= 1)
        {
            _aoe = [new(_rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ApokalypsisRest)
        {
            if (++NumCasts == 4)
            {
                _aoe = [];
                NumCasts = 0;
            }
        }
    }
}

sealed class DeathlyRayThereion(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeRect rect = new(60f, 3f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DeathlyRayVisualThereion1)
        {
            _aoe = [new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.DeathlyRayThereionRest)
        {
            if (++NumCasts == 4)
            {
                _aoe = [];
                NumCasts = 0;
            }
        }
    }
}

sealed class DeathlyRayFaces(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _rect = new(60f, 3f);
    private readonly List<AOEInstance> _aoes = new(9);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        ref var aoe0 = ref aoes[0];
        if (!aoe0.Risky)
        {
            var deadline = aoe0.Activation.AddSeconds(1d);
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.Activation < deadline)
                {
                    aoe.Risky = true;
                }
            }
        }
        return aoes;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.DeathlyRayFacesFirst or (uint)AID.DeathlyRayFacesRest)
        {
            if (_aoes.Count == 0)
            {
                var faces = Module.Enemies((uint)OID.TheFaceOfTheBeast);
                var countF = faces.Count;
                for (var i = 0; i < countF; ++i)
                {
                    var f = faces[i];
                    var isFirstSet = f.Rotation == caster.Rotation;
                    _aoes.Add(new(_rect, f.Position.Quantized(), f.Rotation, !isFirstSet ? WorldState.FutureTime(8.5d) : default, isFirstSet ? Colors.Danger : default, isFirstSet));
                    _aoes.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
                }
            }
            var pos = caster.Position;
            var count = _aoes.Count;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.Origin.AlmostEqual(pos, 0.1f))
                {
                    if (++aoe.ActorID == 5ul)
                    {
                        _aoes.RemoveAt(i);
                    }
                    return;
                }
            }
        }
    }
}

sealed class D063TherionStates : StateMachineBuilder
{
    public D063TherionStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ThereionCharge>()
            .ActivateOnEnter<Misfortune>()
            .ActivateOnEnter<ShadowWreck>()
            .ActivateOnEnter<DeathlyRayFaces>()
            .ActivateOnEnter<DeathlyRayThereion>()
            .ActivateOnEnter<Border>()
            .ActivateOnEnter<Apokalypsis>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 652, NameID = 8210)]
public sealed class D063Therion(WorldState ws, Actor primary) : BossModule(ws, primary, Border.DefaultArena.Center, Border.DefaultArena);
