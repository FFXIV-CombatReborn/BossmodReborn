namespace BossMod.Endwalker.Dungeon.D03Vanaspati.D032Wrecker;

public enum OID : uint
{
    Boss = 0x33E9, // R=6.0
    QueerBubble = 0x3731, // R2.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    AetherSiphonFire = 25145, // Boss->self, 3.0s cast, single-target
    AetherSiphonWater = 25146, // Boss->self, 3.0s cast, single-target
    AetherSprayFire = 25147, // Boss->location, 7.0s cast, range 30, raidwide, be in bubble
    AetherSprayWater = 25148, // Boss->location, 7.0s cast, range 30, knockback 13 away from source
    MeaninglessDestruction = 25153, // Boss->self, 5.0s cast, range 100 circle
    PoisonHeartVisual = 25151, // Boss->self, 5.0s cast, single-target
    PoisonHeartStack = 27851, // Helper->players, 5.0s cast, range 6 circle
    TotalWreck = 25154, // Boss->player, 5.0s cast, single-target
    UnholyWater = 27852, // Boss->self, 3.0s cast, single-target, spawns bubbles
    Withdraw = 27847 // 3731->player, 1.0s cast, single-target, pull 30 between centers
}

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(20f, 30f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MeaninglessDestruction && Arena.Bounds.Radius > 20f)
        {
            _aoe = [new(donut, Arena.Center, default, Module.CastFinishAt(spell, 0.7d))];
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x06 && state == 0x00020001u)
        {
            Arena.Bounds = D032Wrecker.DefaultArena;
            Arena.Center = D032Wrecker.DefaultArena.Center;
            _aoe = [];
        }
    }
}

sealed class QueerBubble(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AetherSprayFire _aoe = module.FindComponent<AetherSprayFire>()!;
    public readonly List<Actor> AOEs = [];
    private static readonly AOEShapeCircle circle = new(2.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        var color = Colors.SafeFromAOE;
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var b = AOEs[i];
            if (!b.IsDead)
                aoes[index++] = new(circle, b.Position, color: _aoe.Active ? color : default);
        }
        return aoes.AsSpan()[..index];
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.QueerBubble)
            AOEs.Add(actor);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (AOEs.Count != 0 && actor.OID == (uint)OID.QueerBubble)
            AOEs.Remove(actor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (AOEs.Count != 0 && spell.Action.ID == (uint)AID.Withdraw)
            AOEs.Remove(caster);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoe.Active)
        {
            var count = AOEs.Count;
            if (count == 0)
                return;
            var forbidden = new ShapeDistance[count];

            for (var i = 0; i < count; ++i)
                forbidden[i] = new SDInvertedCircle(AOEs[i].Position, 2.5f);
            hints.AddForbiddenZone(new SDIntersection(forbidden), Module.CastFinishAt(_aoe.Casters[0].CastInfo));
        }
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

sealed class MeaninglessDestruction(BossModule module) : Components.RaidwideCast(module, (uint)AID.MeaninglessDestruction);
sealed class PoisonHeartStack(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.PoisonHeartStack, 6f, 4, 4);
sealed class TotalWreck(BossModule module) : Components.SingleTargetCast(module, (uint)AID.TotalWreck);
sealed class AetherSprayWater(BossModule module) : Components.RaidwideCast(module, (uint)AID.AetherSprayWater);
sealed class AetherSprayFire(BossModule module) : Components.RaidwideCast(module, (uint)AID.AetherSprayFire, "Go into a bubble! (Raidwide)");

sealed class AetherSprayWaterKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.AetherSprayWater, 13f)
{
    private readonly QueerBubble _aoe = module.FindComponent<QueerBubble>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0 && _aoe.AOEs.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var act = c.Activation;
            if (IsImmune(slot, act))
                return;
            var pos = c.Origin;
            var bubbles = Module.Enemies((uint)OID.QueerBubble);
            var count = bubbles.Count;
            var forbidden = new ShapeDistance[count + 1];
            forbidden[count] = new SDInvertedCircle(pos, 7f);

            for (var i = 0; i < count; ++i)
            {
                var a = bubbles[i].Position;
                forbidden[i] = new SDCone(pos, 100f, Angle.FromDirection(a - pos), Angle.Asin(2.5f / (a - pos).Length()));
            }
            hints.AddForbiddenZone(new SDUnion(forbidden), act);
        }
    }
}

sealed class D032WreckerStates : StateMachineBuilder
{
    public D032WreckerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<AetherSprayFire>()
            .ActivateOnEnter<QueerBubble>()
            .ActivateOnEnter<AetherSprayWater>()
            .ActivateOnEnter<AetherSprayWaterKB>()
            .ActivateOnEnter<TotalWreck>()
            .ActivateOnEnter<PoisonHeartStack>()
            .ActivateOnEnter<MeaninglessDestruction>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 789u, NameID = 10718u)]
public sealed class D032Wrecker(WorldState ws, Actor primary) : BossModule(ws, primary, StartingArena.Center, StartingArena)
{
    private static readonly WPos arenaCenter = new(-295f, -354f);
    public static readonly ArenaBoundsCustom StartingArena = new([new Polygon(arenaCenter, 24.5f, 36)],
    [new Rectangle(new(-295f, -328f), 20f, 2.5f), new Rectangle(new(-295f, -379f), 20f, 1.32f)]);
    public static readonly ArenaBoundsCustom DefaultArena = new([new Polygon(arenaCenter, 20f, 36)]);
}
