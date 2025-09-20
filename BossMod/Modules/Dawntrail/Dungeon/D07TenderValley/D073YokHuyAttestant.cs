namespace BossMod.Dawntrail.Dungeon.D07TenderValley.D073YokHuyAttestant;

public enum OID : uint
{
    Boss = 0x4253, // R4.0
    BlackenedStatue = 0x4254, // R0.5
    // trash that can be pulled into boss room
    YokHuyOrb = 0x424F, // R1.5
    YokHuyAltar = 0x448D, // R1.7
    YokHuyAltar2 = 0x4251 // R1.7
}

public enum AID : uint
{
    AutoAttack = 872, // YokHuyAltar2/YokHuyOrb/YokHuyAltar->player, no cast, single-target

    TectonicShift = 39221, // YokHuyAltar->self, 3.0s cast, range 8 circle
    AncientWrathVisual = 38538, // Boss->self, 6.0s cast, single-target
    AncientWrathLong = 39825, // BlackenedStatue->self, no cast, range 35 width 8 rect
    AncientWrathShort = 39823, // BlackenedStatue->self, no cast, range 12 width 8 rect
    AncientWrathMedium = 39824, // BlackenedStatue->self, no cast, range 22 width 8 rect
    BoulderToss = 38540, // Boss->player, 1.0s cast, single-target
    SunToss = 38539 // Boss->location, 3.0s cast, range 6 circle
}

public enum TetherID : uint
{
    StatueActivate = 37 // 28E8->Boss
}

sealed class TectonicShift(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TectonicShift, 8f);
sealed class BoulderToss(BossModule module) : Components.SingleTargetCast(module, (uint)AID.BoulderToss);
sealed class SunToss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SunToss, 6f);

sealed class AncientWrath(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(6);
    private static readonly AOEShapeRect rectShort = new(12f, 4f);
    private static readonly AOEShapeRect rectMedium = new(22f, 4f);
    private static readonly AOEShapeRect rectLong = new(35f, 4f);

    private static readonly (WPos Position, AOEShapeRect Shape)[] aoeMap =
        [(new(-112.5f, -486.5f), rectMedium), (new(-147.5f, -471.5f), rectMedium),
        (new(-147.5f, -486.5f), rectShort), (new(-112.5f, -471.5f), rectShort)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.StatueActivate)
        {
            var aoeShape = GetAOEShape(source.Position) ?? rectLong;
            _aoes.Add(new(aoeShape, source.Position.Quantized(), source.Rotation, WorldState.FutureTime(8.1d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AncientWrathShort or (uint)AID.AncientWrathMedium or (uint)AID.AncientWrathLong)
            _aoes.Clear();
    }

    private static AOEShapeRect? GetAOEShape(WPos position)
    {
        for (var i = 0; i < 4; ++i)
        {
            var aoe = aoeMap[i];
            if (position.AlmostEqual(aoe.Position, 1f))
            {
                return aoe.Shape;
            }
        }
        return null;
    }
}

sealed class D073YokHuyAttestantStates : StateMachineBuilder
{
    public D073YokHuyAttestantStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AncientWrath>()
            .ActivateOnEnter<BoulderToss>()
            .ActivateOnEnter<SunToss>()
            .ActivateOnEnter<TectonicShift>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 834, NameID = 12801)]
public sealed class D073YokHuyAttestant : BossModule
{
    public D073YokHuyAttestant(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D073YokHuyAttestant(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        WPos[] vertices1 = [new(-134.226f, -482.956f), new(-134.105f, -483.161f), new(-134.227f, -484.617f), new(-134.259f, -486.314f), new(-134.168f, -488.471f),
        new(-134.361f, -489.473f), new(-135.919f, -489.514f), new(-135.919f, -483.988f), new(-136.099f, -483.434f), new(-135.750f, -482.812f)];
        WPos[] vertices2 = [new(-125.309f, -474.651f), new(-125.429f, -474.446f), new(-125.308f, -472.991f), new(-125.276f, -471.294f), new(-125.366f, -469.136f),
        new(-125.173f, -468.134f), new(-123.615f, -468.094f), new(-123.615f, -473.619f), new(-123.436f, -474.173f), new(-123.784f, -474.795f)];
        var arena = new ArenaBoundsCustom([new Rectangle(new(-130f, -475f), 17.9f, 22.5f)], [new PolygonCustom(vertices1), new PolygonCustom(vertices2)], AdjustForHitbox: true);
        return (arena.Center, arena);
    }

    private static readonly uint[] Trash = [(uint)OID.YokHuyAltar, (uint)OID.YokHuyAltar2, (uint)OID.YokHuyOrb];
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, Trash);
    }
}
