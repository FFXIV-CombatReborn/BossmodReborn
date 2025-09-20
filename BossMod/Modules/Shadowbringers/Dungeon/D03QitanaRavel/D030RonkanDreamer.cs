namespace BossMod.Shadowbringers.Dungeon.D03QitanaRavel.D030RonkanDreamer;

public enum OID : uint
{
    Boss = 0x2A40, //R=1.8
    Helper = 0x2E8, //R=0.5
    //trash that can be pulled into miniboss room
    RonkanVessel = 0x28DD, //R=3.0
    RonkanIdol = 0x28DC, //R=2.04
    RonkanThorn = 0x28E3 //R=2.4
}

public enum AID : uint
{
    AutoAttack1 = 872, // RonkanVessel/RonkanIdol->player, no cast, single-target
    AutoAttack2 = 17949, // RonkanThorn->player, no cast, single-target

    WrathOfTheRonka = 17223, // Boss->self, 6.0s cast, single-target
    WrathOfTheRonkaLong = 15918, // Helper->self, no cast, range 35 width 8 rect
    WrathOfTheRonkaShort = 15916, // Helper->self, no cast, range 12 width 8 rect
    WrathOfTheRonkaMedium = 15917, // Helper->self, no cast, range 22 width 8 rect
    RonkanFire = 17433, // Boss->player, 1.0s cast, single-target
    RonkanAbyss = 17387, // Boss->location, 3.0s cast, range 6 circle

    BurningBeam = 15923 // RonkanThorn->self, 3.0s cast, range 15 width 4 rect
}

public enum TetherID : uint
{
    StatueActivate = 37 // 28E8->Boss
}

class RonkanFire(BossModule module) : Components.SingleTargetCast(module, (uint)AID.RonkanFire);
class RonkanAbyss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RonkanAbyss, 6f);

class WrathOfTheRonka(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(6);
    private static readonly AOEShapeRect rectShort = new(12f, 4f), rectMedium = new(22f, 4f), rectLong = new(35f, 4f);

    private static readonly (WPos Position, AOEShapeRect Shape)[] aoeMap =
        [(new(-17f, 627f), rectMedium), (new(17f, 642f), rectMedium),
        (new(-17f, 436f), rectMedium), (new(17f, 421f), rectMedium),
        (new(-17f, 642f), rectShort), (new(17f, 627f), rectShort),
        (new(17f, 436f), rectShort), (new(-17f, 421f), rectShort)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.StatueActivate)
        {
            var aoeShape = GetAOEShape(source.Position) ?? rectLong;
            _aoes.Add(new(aoeShape, source.Position.Quantized(), source.Rotation, WorldState.FutureTime(6d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.WrathOfTheRonkaShort or (uint)AID.WrathOfTheRonkaMedium or (uint)AID.WrathOfTheRonkaLong)
            _aoes.Clear();
    }

    private static AOEShapeRect? GetAOEShape(WPos position)
    {
        for (var i = 0; i < 8; ++i)
        {
            var aoe = aoeMap[i];
            if (position.AlmostEqual(aoe.Position, 1f))
                return aoe.Shape;
        }
        return null;
    }
}

class BurningBeam(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BurningBeam, new AOEShapeRect(15f, 2f));

class D030RonkanDreamerStates : StateMachineBuilder
{
    public D030RonkanDreamerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RonkanFire>()
            .ActivateOnEnter<RonkanAbyss>()
            .ActivateOnEnter<WrathOfTheRonka>()
            .ActivateOnEnter<BurningBeam>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 651, NameID = 8639)]
public class D030RonkanDreamer(WorldState ws, Actor primary) : BossModule(ws, primary, primary.PosRot.Z is var Z && Z > 550f ? arena1.Center : arena2.Center, Z > 550f ? arena1 : arena2)
{
    private static readonly WPos[] vertices1 = [new(-4.299f, 646.21f), new(-4.298f, 640.685f), new(-4.118f, 640.131f), new(-4.467f, 639.508f), new(-6.074f, 639.653f),
    new(-6.195f, 639.858f), new(-5.99f, 641.313f), new(-5.958f, 643.01f), new(-6.236f, 644.911f), new(-5.856f, 646.17f)];
    private static readonly WPos[] vertices2 = [new(6.074f, 630.811f), new(6.195f, 630.606f), new(5.99f, 629.151f), new(5.958f, 627.454f), new(6.236f, 625.553f),
    new(5.856f, 624.294f), new(4.299f, 624.254f), new(4.298f, 629.779f), new(4.118f, 630.333f), new(4.467f, 630.956f)];
    private static readonly WPos[] vertices3 = [new(6.074f, 439.811f), new(6.195f, 439.606f), new(5.99f, 438.151f), new(5.958f, 436.454f), new(6.236f, 434.553f),
    new(5.856f, 433.294f), new(4.299f, 433.254f), new(4.298f, 438.779f), new(4.118f, 439.333f), new(4.467f, 439.956f)];
    private static readonly WPos[] vertices4 = [new(-4.299f, 424.978f), new(-4.298f, 419.453f), new(-4.118f, 418.899f), new(-4.467f, 418.277f), new(-6.074f, 418.421f),
    new(-6.195f, 418.626f), new(-5.99f, 420.081f), new(-5.958f, 421.778f), new(-6.236f, 423.679f), new(-5.856f, 424.938f)];
    private static readonly ArenaBoundsCustom arena1 = new([new Rectangle(new(default, 640f), 17.75f, 23.5f)], [new PolygonCustom(vertices1), new PolygonCustom(vertices2)], AdjustForHitbox: true);
    private static readonly ArenaBoundsCustom arena2 = new([new Rectangle(new(default, 434.5f), 17.75f, 24.25f)], [new PolygonCustom(vertices3), new PolygonCustom(vertices4)], AdjustForHitbox: true);
    private static readonly uint[] trash = [(uint)OID.RonkanVessel, (uint)OID.RonkanThorn, (uint)OID.RonkanIdol];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, trash);
    }
}
