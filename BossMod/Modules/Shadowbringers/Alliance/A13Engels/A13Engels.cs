namespace BossMod.Shadowbringers.Alliance.A13Engels;

class DemolishStructureArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect square = new(5f, 5f, 5f, invertForbiddenZone: true);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DemolishStructure2 && Arena.Bounds == A13MarxEngels.StartingBounds)
        {
            _aoe = [new(square, A13MarxEngels.TransitionSpot, color: Colors.SafeFromAOE)];
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x0B && state == 0x00020001u)
        {
            Arena.Center = A13MarxEngels.SecondArenaCenter;
            Arena.Bounds = A13MarxEngels.StartingBounds;
            _aoe = [];
        }
    }
}

class MarxSmash1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MarxSmash1, new AOEShapeRect(60, 15));
class MarxSmash2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MarxSmash2, new AOEShapeRect(60, 15));
class MarxSmash3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MarxSmash3, new AOEShapeRect(60, 15));
class MarxSmash4(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MarxSmash4, new AOEShapeRect(30, 30));
class MarxSmash5(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MarxSmash5, new AOEShapeRect(35, 30));
class MarxSmash6(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MarxSmash6, new AOEShapeRect(60, 10));
class MarxSmash7(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MarxSmash7, new AOEShapeRect(60, 10));

class MarxCrush(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MarxCrush, new AOEShapeRect(15, 15));

class PrecisionGuidedMissile2(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.PrecisionGuidedMissile2, 6);
class LaserSight1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LaserSight1, new AOEShapeRect(100, 10));
class GuidedMissile2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GuidedMissile2, 6);
class IncendiaryBombing2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IncendiaryBombing2, 8);
class IncendiaryBombing1(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, (uint)AID.IncendiaryBombing1, 8, 5);
class DiffuseLaser(BossModule module) : Components.RaidwideCast(module, (uint)AID.DiffuseLaser);
class SurfaceMissile2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SurfaceMissile2, 6);

class GuidedMissile(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(6), (uint)AID.GuidedMissile2, (uint)AID.GuidedMissile3, 5.5f, 1, 4, true);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 700, NameID = 9147)]
public class A13MarxEngels(WorldState ws, Actor primary) : BossModule(ws, primary, StartingArenaCenter, StartingBounds)
{
    public static readonly WPos TransitionSpot = new(900, 697);
    public static readonly WPos StartingArenaCenter = new(900, 670);
    public static readonly WPos SecondArenaCenter = new(900, 785);
    public static readonly ArenaBoundsSquare StartingBounds = new(30);
}
