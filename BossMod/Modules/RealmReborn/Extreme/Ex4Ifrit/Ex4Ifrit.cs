namespace BossMod.RealmReborn.Extreme.Ex4Ifrit;

class Incinerate(BossModule module) : Components.Cleave(module, (uint)AID.Incinerate, CleaveShape)
{
    public static readonly AOEShapeCone CleaveShape = new(21f, 60f.Degrees());
}

class RadiantPlume(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RadiantPlumeAOE, 8f);

// TODO: consider showing next charge before its cast starts...
class CrimsonCyclone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CrimsonCyclone, new AOEShapeRect(49f, 9f));

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 63, NameID = 1185)]
public class Ex4Ifrit : BossModule
{
    public readonly List<Actor> SmallNails;
    public readonly List<Actor> LargeNails;

    public Ex4Ifrit(WorldState ws, Actor primary) : base(ws, primary, default, Trial.T01IfritN.T01IfritN.IfritArena)
    {
        SmallNails = Enemies((uint)OID.InfernalNailSmall);
        LargeNails = Enemies((uint)OID.InfernalNailLarge);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(SmallNails, Colors.Object);
        Arena.Actors(LargeNails, Colors.Object);
    }
}
