namespace BossMod.Shadowbringers.Quest.MSQ.TheGreatShipVylbrand;

public enum OID : uint
{
    Boss = 0x3187, // R0.5
    SecondOrderRocksplitter = 0x3107, // R1.08
    SecondOrderRoundsman = 0x3102, // R0.9
    SecondOrderPickman = 0x3100, // R0.9
    SecondOrderAlchemist = 0x3101, // R0.9
    OghomoroGolem = 0x3103, // R1.1
    Construct2 = 0x3104, // R3.2
    Bomb = 0x3105, // R0.9
    Grenade1 = 0x318C, // R1.8-3.6
    Grenade2 = 0x3106, // R1.8
    ChannelAether = 0x1EB0F7, // R0.5
    Alphinaud = 0x30FE, // R0.500, x1
    Helper = 0x233C // R0.5
}

public enum AID : uint
{
    AutoAttack1 = 6499, // SecondOrderRocksplitter->player, no cast, single-target
    AutoAttack2 = 6497, // SecondOrderRoundsman/SecondOrderPickman/Construct2->allies, no cast, single-target
    Teleport = 22947, // Construct2->location, no cast, single-target

    KoboldDrill = 22967, // SecondOrderRocksplitter->player, 4.0s cast, single-target
    BulldozeTelegraph1 = 22955, // SecondOrderRocksplitter->location, 8.0s cast, width 6 rect charge
    BulldozeTelegraph2 = 22957, // Helper->location, 8.0s cast, width 6 rect charge
    Bulldoze = 22956, // SecondOrderRocksplitter->location, no cast, width 6 rect charge
    TunnelShakerVisual = 22958, // SecondOrderRocksplitter->self, 5.0s cast, single-target
    TunnelShaker1 = 22959, // Helper->self, 5.0s cast, range 60 30-degree cone
    StrataSmasher = 22960, // SecondOrderRocksplitter->location, no cast, range 60 circle
    Uplift1 = 22961, // Helper->self, 6.0s cast, range 10 circle
    Uplift2 = 22962, // Helper->self, 8.0s cast, range 10-20 donut
    Uplift3 = 22963, // Helper->self, 10.0s cast, range 20-30 donut

    Stone = 21588, // SecondOrderAlchemist->allies, 1.0s cast, single-target

    Breakthrough = 22948, // Construct2->ally, 11.0s cast, width 8 rect charge, wild charges

    TenTrolleyWallop = 22950, // Construct2->self, 6.0s cast, range 40 60-degree cone
    TenTrolleyTorque = 22949, // Construct2->self, 6.0s cast, range 16 circle
    TenTrolleyTap = 23362, // Construct2->self, 3.5s cast, range 8 120-degree cone
    ExplosiveChemistry = 23497, // Grenade1/Grenade2->self, 12.0s cast, single-target
    SelfDestructVisual = 23500, // Grenade2->self, no cast, single-target
    SelfDestruct1 = 22952, // Grenade1/Grenade2->self, no cast, range 6 circle
    SelfDestruct2 = 23501, // Boss->self, 3.5s cast, range 10 circle

    Quakedown = 22953, // SecondOrderRocksplitter->location, no cast, range 60 circle, phase transition, excavate happens while player is stunned and thus useles to draw
    ExcavateVisual = 23132, // SecondOrderRocksplitter->self, 17.0s cast, single-target
    Excavate = 22954 // SecondOrderRocksplitter->ally, no cast, width 6 rect charge
}

public enum TetherID : uint
{
    BombTether = 97 // Grenade2->Alphinaud
}

class TenTrolleyTorque(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TenTrolleyTorque, 16f);
class TenTrolleyTap(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TenTrolleyTap, new AOEShapeCone(8f, 60f.Degrees()));
class TenTrolleyWallop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TenTrolleyWallop, new AOEShapeCone(40f, 30f.Degrees()));
class SelfDestruct2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SelfDestruct2, 10f);

class Breakthrough(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Breakthrough)
        {
            var dir = spell.LocXZ - caster.Position;
            _aoes.Add(new(new AOEShapeRect(dir.Length(), 4f), caster.Position.Quantized(), Angle.FromDirection(dir), Module.CastFinishAt(spell), Colors.SafeFromAOE));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.Breakthrough)
            _aoes.RemoveAt(0);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _aoes.Count;
        if (count != 0)
        {
            var forbidden = new ShapeDistance[count];
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.Shape is AOEShapeRect shape)
                {
                    forbidden[i] = shape.InvertedDistance(aoe.Origin, aoe.Rotation);
                }
            }
            hints.AddForbiddenZone(new SDUnion(forbidden), aoes[0].Activation);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var count = _aoes.Count;
        if (count == 0)
            return;

        var risky = true;
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (aoe.Check(actor.Position))
            {
                risky = false;
                break;
            }
        }
        hints.Add("Share damage inside wildcharge!", risky);
    }
}

class Bulldoze(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes[i] = count > 1 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BulldozeTelegraph2)
        {
            var dir = spell.LocXZ - caster.Position;
            _aoes.Add(new(new AOEShapeRect(dir.Length(), 3f), caster.Position.Quantized(), Angle.FromDirection(dir), Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.BulldozeTelegraph1 or (uint)AID.Bulldoze)
            _aoes.RemoveAt(0);
    }
}

class TunnelShaker(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TunnelShaker1, new AOEShapeCone(60f, 15f.Degrees()));
class Uplift(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 20f), new AOEShapeDonut(20f, 30f)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Uplift1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.Uplift1 => 0,
                (uint)AID.Uplift2 => 1,
                (uint)AID.Uplift3 => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

class BombTether(BossModule module) : Components.InterceptTetherAOE(module, (uint)AID.SelfDestruct1, (uint)TetherID.BombTether, 6, [(uint)OID.Alphinaud])
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Tethers.Count != 0)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            var tether = Tethers[0];
            if (tether.Player != Raid.Player())
            {
                var source = tether.Enemy;
                var target = Module.Enemies((uint)OID.Alphinaud)[0];
                hints.AddForbiddenZone(new SDInvertedRect(target.Position + (target.HitboxRadius + 0.1f) * target.DirectionTo(source), source.Position, 0.5f), Activation);
            }
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        base.OnTethered(source, tether);
        if (Activation != default && tether.ID == TID)
            Activation = WorldState.FutureTime(15d);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
            Activation = default;
    }
}

public class SecondOrderRocksplitterStates : StateMachineBuilder
{
    public SecondOrderRocksplitterStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Breakthrough>()
            .ActivateOnEnter<TenTrolleyTap>()
            .ActivateOnEnter<TenTrolleyTorque>()
            .ActivateOnEnter<TenTrolleyWallop>()
            .ActivateOnEnter<SelfDestruct2>()
            .ActivateOnEnter<Bulldoze>()
            .ActivateOnEnter<TunnelShaker>()
            .ActivateOnEnter<Uplift>()
            .ActivateOnEnter<BombTether>()
            .Raw.Update = () => module.Enemies((uint)OID.SecondOrderRocksplitter) is var boss && boss.Count != 0 && boss[0].HPMP.CurHP == 1 || module.WorldState.CurrentCFCID != 764;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69551)]
public class SecondOrderRocksplitter(WorldState ws, Actor primary) : BossModule(ws, primary, default, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(default, 26.5f, 24)]);
    private static readonly uint[] all = [(uint)OID.Grenade1, (uint)OID.Grenade2, (uint)OID.SecondOrderRoundsman, (uint)OID.SecondOrderRocksplitter, (uint)OID.SecondOrderPickman,
    (uint)OID.SecondOrderAlchemist, (uint)OID.Bomb, (uint)OID.Construct2, (uint)OID.OghomoroGolem];

    protected override bool CheckPull() => Raid.Player()!.InCombat;
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(this, all);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var aether = Enemies((uint)OID.ChannelAether);
        var aethercount = aether.Count;
        if (aethercount != 0)
            for (var i = 0; i < aethercount; ++i)
            {
                var interact = aether[i];
                if (interact.IsTargetable)
                {
                    hints.InteractWithTarget = interact;
                    break;
                }
            }

        if (Enemies((uint)OID.Grenade2).Count != 0)
            for (var i = 0; i < hints.PotentialTargets.Count; ++i)
            {
                var e = hints.PotentialTargets[i];
                if (e.Actor.OID == (uint)OID.Grenade2)
                    e.Priority = AIHints.Enemy.PriorityPointless;
            }
    }
}
