namespace BossMod.Dawntrail.Savage.M04SWickedThunder;

sealed class BewitchingFlight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BewitchingFlightAOE, new AOEShapeRect(40, 2.5f));
sealed class WickedJolt(BossModule module) : Components.TankSwap(module, (uint)AID.WickedJolt, (uint)AID.WickedJolt, (uint)AID.WickedJoltSecond, default, 3.2d, new AOEShapeRect(60f, 2.5f));
sealed class Soulshock(BossModule module) : Components.CastCounter(module, (uint)AID.Soulshock);
sealed class Impact(BossModule module) : Components.CastCounter(module, (uint)AID.Impact);
sealed class Cannonbolt(BossModule module) : Components.CastCounter(module, (uint)AID.Cannonbolt);

sealed class CannonboltKB(BossModule module) : Components.GenericKnockback(module)
{
    private readonly Knockback[] _kb = new Knockback[1];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        _kb[0] = new(Module.PrimaryActor.Position, 50f, ignoreImmunes: true);
        return _kb;
    }
}

sealed class CrossTailSwitch(BossModule module) : Components.CastCounter(module, (uint)AID.CrossTailSwitchAOE);
sealed class CrossTailSwitchLast(BossModule module) : Components.CastCounter(module, (uint)AID.CrossTailSwitchLast);
sealed class WickedSpecialCenter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WickedSpecialCenterAOE, new AOEShapeRect(40f, 10f));
sealed class WickedSpecialSides(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WickedSpecialSidesAOE, new AOEShapeRect(40f, 7.5f));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 992u, NameID = 13057u, PlanLevel = 100)]
public sealed class M04SWickedThunder(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f))
{
    public static ArenaBoundsCustom GetTransitionBounds() => new([new Square(new(100f, 100f), 20f), new Rectangle(new(100f, 165f), 20f, 15f)]);
    public static ArenaBoundsCustom GetP2CircleBounds() => new([new Polygon(new(100f, 165f), 15f, 50, 3.6f.Degrees())]);
    public static ArenaBoundsCustom GetP2TowersBounds() => new([new Rectangle(new(115f, 100f), 5f, 15f), new Rectangle(new(85f, 100f), 5f, 15f)]);

    public Actor? BossP1() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
    public Actor? BossP2() => _bossP2;

    private Actor? _bossP2;

    protected override void UpdateModule()
    {
        _bossP2 ??= GetActor((uint)OID.BossP2);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_bossP2);
    }
}
