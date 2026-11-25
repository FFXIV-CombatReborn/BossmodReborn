namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE12BayingOfHounds;

public enum OID : uint
{
    Boss = 0x2E66, // R7.020, x1
    Hellsfire = 0x2E67, // R1.000-2.500, spawn during fight
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    Hellclaw = 20534, // Boss->player, 4.0s cast, single-target, tankbuster
    TailBlow = 20535, // Boss->self, 3.0s cast, range 19 90-degree cone aoe
    LavaSpit1 = 20536, // Boss->self, 3.0s cast, single-target, visual (summon hellsfires)
    LavaSpit2 = 20537, // Boss->self, no cast, single-target, visual (summon second set of hellsfires)
    LavaSpitAOE = 20538, // Helper->location, 3.0s cast, range 5 circle
    HellsfireActivate = 19647, // Hellsfire->self, no cast, range ?-50 donut, visual (prepare for activation)
    ScorchingLash = 20553, // Hellsfire->self, 4.0s cast, range 50 width 10 rect aoe
    Hellpounce = 20539, // Boss->location, 4.0s cast, width 10 rect charge aoe, knockback away from source, dist 5 (consider showing?)
    HellpounceSecond = 20540, // Boss->location, 1.0s cast, width 10 rect charge, knockback away from source, dist 5 (consider showing?)
    LionsBreath = 20541, // Boss->self, 4.0s cast, single-target, visual (frontal cone)
    LionsBreathAOE = 20542, // Helper->self, 4.5s cast, range 60 45-degree cone aoe
    DragonsBreath = 20543, // Boss->self, 4.0s cast, single-target, visual (side cones)
    DragonsBreathAOER = 20544, // Helper->self, 4.5s cast, range 60 30-degree cone
    DragonsBreathAOEL = 20545, // Helper->self, 4.5s cast, range 60 30-degree cone
    VoidTornado = 20546, // Boss->self, 4.0s cast, single-target, visual (set hp to 1)
    VoidTornadoAOE = 20547, // Helper->self, no cast, range 30 circle, set hp to 1
    VoidQuake = 20548, // Boss->self, 3.0s cast, single-target, visual (staggered circle/donuts)
    VoidQuakeAOE1 = 20549, // Helper->self, 3.0s cast, range 10 circle aoe
    VoidQuakeAOE2 = 20550, // Helper->self, 3.0s cast, range 10-20 donut aoe
    VoidQuakeAOE3 = 20551, // Helper->self, 3.0s cast, range 20-30 donut aoe
}

sealed class Hellclaw(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Hellclaw);
sealed class TailBlow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TailBlow, new AOEShapeCone(19f, 45f.Degrees()));
sealed class LavaSpit(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LavaSpitAOE, 5f);
sealed class ScorchingLash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ScorchingLash, new AOEShapeRect(50f, 5f));

sealed class Hellpounce(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Hellpounce or (uint)AID.HellpounceSecond)
        {
            Activate(caster.Position, spell.LocXZ, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Hellpounce:
                var center = CE12BayingOfHounds.ArenaCenter;
                var loc = spell.LocXZ;
                var offset = loc - center;
                Activate(loc, center - offset, WorldState.FutureTime(3.7d));
                break;
            case (uint)AID.HellpounceSecond:
                _aoe = [];
                break;
        }
    }

    private void Activate(WPos source, WPos target, DateTime activation)
    {
        var toTarget = target - source;
        _aoe = [new(new AOEShapeRect(toTarget.Length(), 5f), source.Quantized(), Angle.FromDirection(toTarget), activation)];
    }
}

sealed class DragonsLionsBreath(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.DragonsBreathAOER, (uint)AID.DragonsBreathAOEL, (uint)AID.LionsBreathAOE], new AOEShapeCone(60f, 30f.Degrees()));

sealed class VoidTornado(BossModule module) : Components.CastHint(module, (uint)AID.VoidTornado, "Set hp to 1");

sealed class VoidQuake(BossModule module) : Components.GenericAOEs(module) // this concentric AOE can happen forwards or backwards in order with the same AID as the starter
{
    private readonly List<AOEInstance> _aoes = [with(2)];
    private static readonly AOEShapeCircle _shape1 = new(10f);
    private static readonly AOEShapeDonut _shape2 = new(10f, 20f);
    private static readonly AOEShapeDonut _shape3 = new(20f, 30f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count == 0 ? [] : CollectionsMarshal.AsSpan(_aoes)[..1];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.VoidQuakeAOE1 => _shape1,
            (uint)AID.VoidQuakeAOE2 => _shape2,
            (uint)AID.VoidQuakeAOE3 => _shape3,
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, spell.LocXZ, default, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.VoidQuakeAOE1 or (uint)AID.VoidQuakeAOE2 or (uint)AID.VoidQuakeAOE3)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class CE12BayingOfHoundsStates : StateMachineBuilder
{
    public CE12BayingOfHoundsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hellclaw>()
            .ActivateOnEnter<TailBlow>()
            .ActivateOnEnter<LavaSpit>()
            .ActivateOnEnter<ScorchingLash>()
            .ActivateOnEnter<Hellpounce>()
            .ActivateOnEnter<DragonsLionsBreath>()
            .ActivateOnEnter<VoidTornado>()
            .ActivateOnEnter<VoidQuake>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 735, NameID = 2)] // bnpcname=9394
public sealed class CE12BayingOfHounds(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public static readonly WPos ArenaCenter = new(154f, 785f);
    private static readonly ArenaBoundsCustom arena = new([new Polygon(ArenaCenter, 24.5f, 32)]);

    protected override bool CheckPull() => base.CheckPull() && Raid.Player() is Actor player && player.Position.InCircle(Arena.Center, 25f);
}
