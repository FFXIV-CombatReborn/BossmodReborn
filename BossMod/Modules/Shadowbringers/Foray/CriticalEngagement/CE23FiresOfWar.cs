namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE23FiresOfWar;

public enum OID : uint
{
    Boss = 0x2E75, // R2.55, pyrobolus pater
    ImperialPyromancer1 = 0x2E72, // R0.5
    ImperialPyromancer2 = 0x2E73, // R0.5
    TowerVisual1 = 0x1EB040, // R0.5
    TowerVisual2 = 0x1EB041, // R0.5
    PyrobolusSoror = 0x2E76, // R0.9
    WanderingFlame = 0x2E78, // R1.5
    PyrobolusMater = 0x2E74, // R3.75
    PyrobolusFrater = 0x2E77, // R0.6
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Fire = 21256, // ImperialPyromancer1->player, 2.5s cast, single-target
    MagickedMark = 21258, // ImperialPyromancer2->player, 2.5s cast, single-target
    ScaldingScolding = 21255, // Boss->self, no cast, range 8 120-degree cone
    Pyrokinesis = 21257, // ImperialPyromancer1->player, 4.5s cast, single-target

    Pyroplexy = 20659, // ImperialPyromancer1->location, 12.0s cast, range 4 circle, tower
    Pyroclysm = 20660, // Helper->location, no cast, range 40 circle, tower fail

    CantTouchThis = 20661, // PyrobolusSoror->self, no cast, range 6 circle, stack
    OutWithABang = 20662, // PyrobolusSoror->self, no cast, range 40 circle, stack fail

    ExplosiveCountdown1 = 20665, // Helper->self, 2.0s cast, range 11-18 donut
    ExplosiveCountdown2 = 20664, // Helper->self, 2.0s cast, range 5-12 donut
    ExplosiveCountdown3 = 20663, // Helper->self, 2.0s cast, range 6 circle

    CatchingFire = 20666, // WanderingFlame->self, 5.0s cast, range 40 circle, small raidwide
    TooHotToHandle = 20667, // PyrobolusFrater->self, 15.0s cast, range 10 circle
    HotTemper = 20668, // PyrobolusMater->self, 15.0s cast, range 30 circle, proximity AOE

    SlowDeflagration = 20669, // PyrobolusMater->self, 75.0s cast, range 40 circle, enrage
    RapidDeflagration = 20670 // PyrobolusMater->self, no cast, range 40 circle, enrage repeat
}

public enum SID : uint
{
    CantTouchThis = 2056 // none->PyrobolusSoror, extra=0xB9
}

sealed class ScaldingScolding(BossModule module) : Components.Cleave(module, (uint)AID.ScaldingScolding, new AOEShapeCone(8f, 60f.Degrees()));

sealed class CantTouchThis(BossModule module) : Components.GenericStackSpread(module)
{
    private readonly List<Actor> participants = [];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.CantTouchThis)
            Stacks.Add(new(actor, 6f, 2, activation: WorldState.FutureTime(16d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.CantTouchThis or (uint)AID.OutWithABang)
        {
            var count = Stacks.Count;
            for (var i = 0; i < count; ++i)
            {
                if (Stacks[i].Target == caster)
                {
                    Stacks.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var count = Stacks.Count;
        for (var i = 0; i < count; ++i)
        {
            var stack = Stacks[i];
            var stackPos = stack.Target.Position;
            if (stack.IsInside(stackPos))
            {
                hints.Add("Touch bomb when ready to resolve stack!");
                return;
            }
        }
        base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var countP = participants.Count;
        if (countP == 0)
        {
            foreach (var a in Module.WorldState.Actors.Actors.Values)
                if (a.OID == default && a.Position.InCircle(Arena.Center, 20f))
                    participants.Add(a);
        }
        var count = Stacks.Count;
        var stacks = CollectionsMarshal.AsSpan(Stacks);
        for (var i = 0; i < count; ++i)
        {
            ref readonly var stack = ref stacks[i];
            var stackPos = stack.Target.Position;
            if (stack.IsInside(stackPos))
            {
                var stackRot = stack.Target.Rotation;
                void AddGoalzone() => hints.GoalZones.Add(hints.GoalSingleTarget(stackPos + 0.5f * stackRot.ToDirection(), 0.7f, 5f));
                if (actor.HPMP.CurHP > 30000u) // can take stack solo
                {
                    AddGoalzone();
                    goto end;
                }
                var numInside = 0;
                for (var j = 0; j < countP; ++j)
                {
                    var p = participants[i];
                    if (p.Position.InCircle(stackPos, 6f))
                    {
                        if (++numInside > 1)
                        {
                            AddGoalzone();
                            goto end;
                        }
                    }
                }
            }
        }
    end:
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

sealed class Pyroplexy(BossModule module) : Components.CastTowersOpenWorld(module, (uint)AID.Pyroplexy, 4f);
sealed class CatchingFire(BossModule module) : Components.RaidwideCast(module, (uint)AID.CatchingFire);

sealed class TooHotToHandle(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(10f);
    private readonly List<AOEInstance> _aoes = new(9);
    private WPos? mater;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        if (count == 9)
        {
            mater ??= Module.Enemies((uint)OID.PyrobolusMater)[0].Position;
            var pos = mater.Value;
            WPos furthest = default;
            var maxDistSq = float.MinValue;
            for (var i = 0; i < 9; ++i)
            {
                ref readonly var aoe = ref aoes[i];
                var distSq = (aoe.Origin - pos).LengthSq();
                if (distSq > maxDistSq)
                {
                    maxDistSq = distSq;
                    furthest = aoe.Origin;
                }
            }
            Span<AOEInstance> filteredAOEs = new AOEInstance[8];
            var index = 0;
            for (var i = 0; i < 9; ++i)
            {
                ref readonly var aoe = ref aoes[i];
                if (aoe.Origin != furthest)
                    filteredAOEs[index++] = aoe;
            }
            return filteredAOEs;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TooHotToHandle)
            _aoes.Add(new(circle, spell.LocXZ, default, Module.CastFinishAt(spell), actorID: caster.InstanceID));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TooHotToHandle)
        {
            var count = _aoes.Count;
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _aoes.Count;
        if (count == 9)
        {
            mater ??= Module.Enemies((uint)OID.PyrobolusMater)[0].Position;
            var pos = mater.Value;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            WPos furthest = default;
            var maxDistSq = float.MinValue;
            for (var i = 0; i < 9; ++i)
            {
                ref readonly var aoe = ref aoes[i];
                var distSq = (aoe.Origin - pos).LengthSq();
                if (distSq > maxDistSq)
                {
                    maxDistSq = distSq;
                    furthest = aoe.Origin;
                }
            }
            for (var i = 0; i < 9; ++i)
            {
                ref readonly var aoe = ref aoes[i];
                if (aoe.Origin != furthest)
                    hints.AddForbiddenZone(aoe.Shape, aoe.Origin, default, aoe.Activation);
            }
            var countT = hints.PotentialTargets.Count;
            for (var i = 0; i < countT; ++i)
            {
                var e = hints.PotentialTargets[i];
                e.Priority = e.Actor.Position.AlmostEqual(furthest, 1f) ? 1 : AIHints.Enemy.PriorityPointless;
            }
        }
        else if (count > 0)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            var countT = hints.PotentialTargets.Count;
            for (var i = 0; i < countT; ++i)
            {
                hints.PotentialTargets[i].Priority = AIHints.Enemy.PriorityPointless;
            }
        }
    }
}

sealed class ExplosiveCountdown(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeDonut(11f, 18f), new AOEShapeDonut(5f, 12f), new AOEShapeCircle(6f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ExplosiveCountdown1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.ExplosiveCountdown1 => 0,
                (uint)AID.ExplosiveCountdown2 => 1,
                (uint)AID.ExplosiveCountdown3 => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(4d));
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.WanderingFlame)
        {
            Sequences.Clear();
        }
    }
}

sealed class HotTemper(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HotTemper, 24f);
sealed class SlowDeflagration(BossModule module) : Components.CastHint(module, (uint)AID.SlowDeflagration, "Enrage!", true);

sealed class CE23FiresOfWarStates : StateMachineBuilder
{
    public CE23FiresOfWarStates(CE23FiresOfWar module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CantTouchThis>()
            .ActivateOnEnter<Pyroplexy>()
            .ActivateOnEnter<CatchingFire>()
            .ActivateOnEnter<ExplosiveCountdown>()
            .ActivateOnEnter<HotTemper>()
            .ActivateOnEnter<TooHotToHandle>()
            .ActivateOnEnter<SlowDeflagration>()
            .ActivateOnEnter<ScaldingScolding>()
            .Raw.Update = () =>
            {
                return (module.BossMater()?.IsDead ?? false) || AllDeadOrDestroyed(CE23FiresOfWar.Trash);
            };
        ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 735, NameID = 9)]
public sealed class CE23FiresOfWar(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(83f, 563f), 19.5f, 32)]);
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.ImperialPyromancer1, (uint)OID.ImperialPyromancer2, (uint)OID.PyrobolusFrater];
    private Actor? _bossMater;
    public Actor? BossMater() => _bossMater;

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 20f);

    protected override void UpdateModule()
    {
        _bossMater ??= GetActor((uint)OID.PyrobolusMater);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(_bossMater);
        Arena.Actors(this, Trash);
    }
}
