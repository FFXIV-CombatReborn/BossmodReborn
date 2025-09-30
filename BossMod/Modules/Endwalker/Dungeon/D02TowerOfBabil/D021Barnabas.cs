namespace BossMod.Endwalker.Dungeon.D02TowerOfBabil.D021Barnabas;

public enum OID : uint
{
    Boss = 0x33F7, // R=5.52
    Thunderball = 0x33F8, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    DynamicPoundMinus = 25157, // Boss->self, 7.0s cast, range 40 width 6 rect
    DynamicPoundPlus = 25326, // Boss->self, 7.0s cast, range 40 width 6 rect
    DynamicPoundPull = 24693, // Helper->self, no cast, range 50 width 50 rect, pull 9, between centers
    DynamicPoundKB = 24694, // Helper->self, no cast, range 50 width 50 rect, knockback 9, dir left/right
    DynamicScraplineMinus = 25158, // Boss->self, 7.0s cast, range 8 circle
    DynamicScraplinePlus = 25328, // Boss->self, 7.0s cast, range 8 circle
    DynamicScraplineKB = 25053, // Helper->self, no cast, range 50 circle, pull 5, between centers
    DynamicScraplinePull = 25054, // Helper->self, no cast, range 50 circle, knockback 5, away from source

    ElectromagneticRelease1 = 25327, // Helper->self, 9.5s cast, range 40 width 6 rect
    ElectromagneticRelease2 = 25329, // Helper->self, 9.5s cast, range 8 circle
    GroundAndPound1 = 25159, // Boss->self, 3.5s cast, range 40 width 6 rect
    GroundAndPound2 = 25322, // Boss->self, 3.5s cast, range 40 width 6 rect

    RollingScrapline = 25323, // Boss->self, 3.0s cast, range 8 circle
    Shock = 25330, // Thunderball->self, 3.0s cast, range 8 circle
    ShockingForce = 25324, // Boss->players, 5.0s cast, range 6 circle, stack
    Thundercall = 25325 // Boss->self, 3.0s cast, single-target
}

public enum IconID : uint
{
    Plus = 162, // player
    Minus = 163 // player
}

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(15f, 19.5f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00 && state == 0x00020001u)
        {
            Arena.Bounds = D021Barnabas.SmallerBounds;
            _aoe = [];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.GroundAndPound1 or (uint)AID.GroundAndPound2 && Arena.Bounds.Radius > 15f)
        {
            _aoe = [new(donut, Arena.Center, default, Module.CastFinishAt(spell, 6.1d))];
        }
    }
}

sealed class Magnetism(BossModule module) : Components.GenericKnockback(module)
{
    private readonly Circles _aoe1 = module.FindComponent<Circles>()!;
    private readonly Cleaves _aoe2 = module.FindComponent<Cleaves>()!;
    private BitMask positiveCharge;
    private BitMask negativeCharge;
    private DateTime activation;
    private bool bossCharge; // false if negative, true if positive
    private bool shape; // false if circle, true if rect
    private static readonly Angle a90 = 90f.Degrees();
    private static readonly AOEShapeCone _shape = new(30f, a90);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (activation != default)
        {
            var isPull = !bossCharge && positiveCharge[slot] || bossCharge && negativeCharge[slot];
            if (shape)
            {
                var kind = isPull ? Kind.DirBackward : Kind.DirForward;
                var knockback = new Knockback[2];
                knockback[0] = new(Arena.Center, 9f, activation, _shape, a90, kind);
                knockback[1] = new(Arena.Center, 9f, activation, _shape, -a90, kind);
                return knockback;
            }
            else
                return new Knockback[1] { new(Arena.Center, 5f, activation, kind: isPull ? Kind.TowardsOrigin : Kind.AwayFromOrigin, ignoreImmunes: true) };
        }
        return [];
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (_aoe1.Casters.Count != 0 && _aoe1.Casters.Ref(0).Check(pos))
            return true;
        if (_aoe2.Casters.Count != 0 && _aoe2.Casters.Ref(0).Check(pos))
            return true;
        return !Arena.InBounds(pos);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch (iconID)
        {
            case (uint)IconID.Plus:
                positiveCharge.Set(Raid.FindSlot(actor.InstanceID));
                activation = WorldState.FutureTime(8.1d);
                break;
            case (uint)IconID.Minus:
                negativeCharge.Set(Raid.FindSlot(actor.InstanceID));
                activation = WorldState.FutureTime(8.1d);
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.DynamicPoundPlus:
            case (uint)AID.DynamicScraplinePlus:
                bossCharge = true;
                break;
            case (uint)AID.DynamicScraplineMinus:
            case (uint)AID.DynamicPoundMinus:
                bossCharge = false;
                break;
            case (uint)AID.ElectromagneticRelease1:
                shape = true;
                break;
            case (uint)AID.ElectromagneticRelease2:
                shape = false;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.DynamicPoundKB or (uint)AID.DynamicPoundPull or (uint)AID.DynamicScraplinePull or (uint)AID.DynamicScraplinePull)
        {
            negativeCharge = default;
            positiveCharge = default;
            activation = default;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (activation == default)
            return;

        var active = ActiveKnockbacks(slot, actor);
        if (active.Length == 0 || IsImmune(slot, active[0].Activation))
            return;

        var isPositive = positiveCharge[slot];
        var isNegative = negativeCharge[slot];
        var isPull = !bossCharge && isPositive || bossCharge && isNegative;
        var isKnockback = bossCharge && isPositive || !bossCharge && isNegative;
        var pos = Arena.Center;

        ShapeDistance? forbidden = shape
            ? isPull ? new SDRect(pos, new WDir(default, 1f), 15f, 15f, 12f)
            : isKnockback ? new SDInvertedCircle(pos, 6f)
            : null
            : isPull ? new SDCircle(pos, 13f)
            : isKnockback ? new SDInvertedCircle(pos, 10f)
            : null;

        if (forbidden != null)
            hints.AddForbiddenZone(forbidden, activation);
    }
}

sealed class Cleaves(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ElectromagneticRelease1, (uint)AID.GroundAndPound1, (uint)AID.GroundAndPound2,
(uint)AID.DynamicPoundMinus, (uint)AID.DynamicPoundPlus], new AOEShapeRect(40f, 3f));
sealed class Circles(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ElectromagneticRelease2, (uint)AID.DynamicScraplineMinus,
(uint)AID.DynamicScraplinePlus, (uint)AID.RollingScrapline, (uint)AID.Shock], 8f);

sealed class ShockingForce(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.ShockingForce, 6f, 4, 4);

sealed class D021BarnabasStates : StateMachineBuilder
{
    public D021BarnabasStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<Cleaves>()
            .ActivateOnEnter<Circles>()
            .ActivateOnEnter<Magnetism>()
            .ActivateOnEnter<ShockingForce>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 785, NameID = 10279)]
public sealed class D021Barnabas(WorldState ws, Actor primary) : BossModule(ws, primary, arenaCenter, StartingBounds)
{
    private static readonly WPos arenaCenter = new(-300f, 71f);
    public static readonly ArenaBoundsCustom StartingBounds = new([new Polygon(arenaCenter, 19.5f, 48)], [new Rectangle(new(-300f, 91f), 20f, 1.25f), new Rectangle(new(-300f, 51f), 20f, 1.25f)]);
    public static readonly ArenaBoundsCustom SmallerBounds = new([new Polygon(arenaCenter, 15f, 48)]);
}
