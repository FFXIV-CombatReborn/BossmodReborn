namespace BossMod.Shadowbringers.Hunt.RankA.Huracan;

public enum OID : uint
{
    Boss = 0x28B5 // R=4.9
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    WindsEnd = 17494, // Boss->player, no cast, single-target
    WinterRain = 17497, // Boss->location, 4.0s cast, range 6 circle
    Windburst = 18042, // Boss->self, no cast, range 80 width 10 rect
    SummerHeat = 17499, // Boss->self, 4.0s cast, range 40 circle
    DawnsEdge = 17495, // Boss->self, 3.5s cast, range 15 width 10 rect
    SpringBreeze = 17496, // Boss->self, 3.5s cast, range 80 width 10 rect
    AutumnWreath = 17498 // Boss->self, 4.0s cast, range 10-20 donut
}

sealed class SpringBreeze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpringBreeze, new AOEShapeRect(80f, 5f));
sealed class SummerHeat(BossModule module) : Components.RaidwideCast(module, (uint)AID.SummerHeat);

sealed class Combos(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeDonut donut = new(10f, 20f);
    private readonly AOEShapeCircle circle = new(6f);
    private readonly AOEShapeRect rect = new(15f, 5f);
    private readonly AOEShapeRect rect2 = new(80f, 5f);
    private readonly List<AOEInstance> _aoes = new(2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void Update()
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return;
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        ref var aoe0 = ref aoes[0];
        if (count > 1)
        {
            aoe0.Color = Colors.Danger;
            ref var aoe1 = ref aoes[1];
            if (aoe0.Shape == circle) // unfortunately the circle is targeting a location and the boss can still slightly move after cast started 
            {
                var prim = Module.PrimaryActor;
                var rot = prim.Rotation;
                var pos = prim.Position;
                var origin = (pos - 40f * rot.ToDirection()).Quantized();
                aoe1.Origin = origin;
                aoe1.Rotation = rot;
                aoe1.ShapeDistance = aoe1.Shape.Distance(origin, rot);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.AutumnWreath => donut,
            (uint)AID.DawnsEdge => rect,
            (uint)AID.WinterRain => circle,
            _ => null
        };
        if (shape != null)
        {
            var pos = spell.LocXZ;
            var rot = spell.Rotation;
            AddAOE(shape, pos, rot);
            var primary = Module.PrimaryActor;
            var check = shape == circle;
            var pos2 = check ? primary.Position : pos;
            AddAOE(rect2, pos2 - 40f * rot.ToDirection(), check ? primary.Rotation : rot, 3.1d);
        }
        void AddAOE(AOEShape shape, WPos origin, Angle rotation, double delay = default) => _aoes.Add(new(shape, origin, rotation, Module.CastFinishAt(spell, delay), shapeDistance: shape.Distance(origin, rotation)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.Windburst or (uint)AID.AutumnWreath or (uint)AID.DawnsEdge or (uint)AID.WinterRain)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class HuracanStates : StateMachineBuilder
{
    public HuracanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SpringBreeze>()
            .ActivateOnEnter<SummerHeat>()
            .ActivateOnEnter<Combos>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 8912)]
public sealed class Huracan(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
