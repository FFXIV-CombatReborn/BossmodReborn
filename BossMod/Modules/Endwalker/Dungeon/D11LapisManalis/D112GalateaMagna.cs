namespace BossMod.Endwalker.Dungeon.D11LapisManalis.D112GalateaMagna;

public enum OID : uint
{
    GalateaMagna = 0x3971, //R=5.0
    Helper2 = 0x3D06, // R3.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // GalateaMagna->player, no cast, single-target
    Teleport = 32625, // GalateaMagna->location, no cast, single-target, GalateaMagna teleports to spot marked by icons 1,2,3,4 or to mid

    WaningCycleVisual = 32623, // GalateaMagna->self, no cast, single-target (between in->out)
    WaxingCycleVisual = 31378, // GalateaMagna->self, no cast, single-target (between out->in)
    WaningCycle1 = 32622, // GalateaMagna->self, 4.0s cast, range 10-40 donut
    WaningCycle2 = 32624, // Helper->self, 6.0s cast, range 10 circle
    WaxingCycle1 = 31377, // GalateaMagna->self, 4.0s cast, range 10 circle
    WaxingCycle2 = 31379, // Helper->self, 6.7s cast, range 10-40 donut

    SoulScythe = 31386, // GalateaMagna->location, 6.0s cast, range 18 circle
    SoulNebula = 31390, // GalateaMagna->self, 5.0s cast, range 40 circle, raidwide
    ScarecrowChaseVisual1 = 31387, // GalateaMagna->self, 8.0s cast, single-target
    ScarecrowChaseVisual2 = 31389, // GalateaMagna->self, no cast, single-target
    ScarecrowChase = 32703, // Helper->self, 1.8s cast, range 40 width 10 cross

    Tenebrism = 31382, // GalateaMagna->self, 4.0s cast, range 40 circle, small raidwide, spawns 4 towers, applies glass-eyed on tower resolve
    Burst = 31383, // Helper->self, no cast, range 5 circle, tower success
    BigBurst = 31384, // Helper->self, no cast, range 60 circle, tower fail
    StonyGaze = 31385 // Helper->self, no cast, gaze
}

public enum IconID : uint
{
    Icon1 = 336, // Helper2
    Icon2 = 337, // Helper2
    Icon3 = 338, // Helper2
    Icon4 = 339 // Helper2
}

public enum SID : uint
{
    Doom = 3364, // Helper->player, extra=0x0
    GlassyEyed = 3511 // Boss->player, extra=0x0, takes possession of the player after status ends and does a petrifying attack in all direction
}

sealed class ScarecrowChase(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCross cross = new(40f, 5f);
    private readonly List<AOEInstance> _aoes = new(4);
    private bool first = true;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var max = count > 2 ? 2 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void Update()
    {
        var count = _aoes.Count;
        if (count != 0)
        {
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            ref var aoe0 = ref aoes[0];
            aoe0.Risky = true;
            if (count > 1)
            {
                aoe0.Color = Colors.Danger;
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is >= (uint)IconID.Icon1 and <= (uint)IconID.Icon4)
        {
            _aoes.Add(new(cross, actor.Position.Quantized(), Angle.AnglesIntercardinals[1], WorldState.FutureTime(9.9d + (-(uint)IconID.Icon1 + iconID) * 3d), risky: false));
            if (_aoes.Count is var count && (count == 4 || count == 2 && first))
            {
                first = false;
                _aoes.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.ScarecrowChase)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class WaxingCycle(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 40f)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WaxingCycle1)
        {
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.WaxingCycle1 => 0,
                (uint)AID.WaxingCycle2 => 1,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2.7d));
        }
    }
}

sealed class WaningCycle(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeDonut(10f, 40f), new AOEShapeCircle(10f)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WaningCycle1)
        {
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.WaningCycle1 => 0,
                (uint)AID.WaningCycle2 => 1,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

sealed class GlassyEyed(BossModule module) : Components.GenericGaze(module)
{
    private DateTime _activation;
    private readonly List<Actor> _affected = new(4);

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        var count = _affected.Count;
        if (count == 0 || WorldState.CurrentTime < _activation.AddSeconds(-10d))
        {
            return [];
        }
        var eyes = new Eye[count];

        for (var i = 0; i < count; ++i)
        {
            eyes[i] = new(_affected[i].Position, _activation);
        }
        return eyes;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.GlassyEyed)
        {
            _activation = status.ExpireAt;
            _affected.Add(actor);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.GlassyEyed)
        {
            _affected.Remove(actor);
        }
    }
}

sealed class TenebrismTowers(BossModule module) : Components.GenericTowers(module)
{
    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00010008u)
        {
            WPos position = index switch
            {
                0x07 => new(350f, -404f),
                0x08 => new(360f, -394f),
                0x09 => new(350f, -384f),
                0x0A => new(340f, -394f),
                _ => default
            };
            if (position != default)
            {
                Towers.Add(new(position.Quantized(), 5f, activation: WorldState.FutureTime(6d)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Burst)
        {
            var towers = CollectionsMarshal.AsSpan(Towers);
            var len = towers.Length;
            var pos = caster.Position.Quantized();
            for (var i = 0; i < len; ++i)
            {
                ref var tower = ref towers[i];
                if (tower.Position == pos)
                {
                    Towers.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

sealed class Doom(BossModule module) : Components.CleansableDebuff(module, (uint)SID.Doom);

sealed class SoulScythe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SoulScythe, 18f);

sealed class D112GalateaMagnaStates : StateMachineBuilder
{
    public D112GalateaMagnaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Doom>()
            .ActivateOnEnter<TenebrismTowers>()
            .ActivateOnEnter<WaningCycle>()
            .ActivateOnEnter<WaxingCycle>()
            .ActivateOnEnter<GlassyEyed>()
            .ActivateOnEnter<SoulScythe>()
            .ActivateOnEnter<ScarecrowChase>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.GalateaMagna, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 896u, NameID = 10308u, Category = BossModuleInfo.Category.Dungeon, Expansion = BossModuleInfo.Expansion.Endwalker, SortOrder = 3)]
public sealed class D112GalateaMagna(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Circle(new(350f, -394f), 19.5f)], [new Rectangle(new(350f, -373f), 20f, 2.25f), new Rectangle(new(350f, -414f), 20f, 1.25f)]);
}
