namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.DD50EddaBlackbosom;

public enum OID : uint
{
    Boss = 0x16C6, // R1.500, x1
    DemonButler = 0x16E9, // R1.200, x4
    GargoyleSteward = 0x16E8 // R2.300, x4
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target

    BlackHoneymoon = 6402, // Boss->location, 3.0s cast, range 40 circle
    ColdFeet = 6403, // Boss->self, 3.0s cast, range 40 circle, gaze mechanic
    DarkHarvest = 6400, // Boss->player, 2.0s cast, single-target
    Desolation = 6404, // GargoyleSteward->self, 4.0s cast, range 55+R width 6 rect
    InHealthCircle = 6398, // Boss->self, 4.5s cast, range 16 circle
    InHealthDonut = 6399, // Boss->self, 4.5s cast, range 3-50+R donut
    TerrorEye = 6405 // DemonButler->location, 4.0s cast, range 6 circle
}

sealed class BlackHoneymoon(BossModule module) : Components.RaidwideCast(module, (uint)AID.BlackHoneymoon);
sealed class ColdFeet(BossModule module) : Components.CastGaze(module, (uint)AID.ColdFeet);
sealed class DarkHarvest(BossModule module) : Components.SingleTargetCast(module, (uint)AID.DarkHarvest);
sealed class Desolation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Desolation, new AOEShapeRect(57.3f, 3f));
sealed class InHeathCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.InHealthCircle, 16f);
sealed class InHeathDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.InHealthDonut, new AOEShapeDonut(3f, 51.5f));
sealed class TerrorEye(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TerrorEye, 6f);

sealed class DD50EddaBlackbosomStates : StateMachineBuilder
{
    public DD50EddaBlackbosomStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BlackHoneymoon>()
            .ActivateOnEnter<ColdFeet>()
            .ActivateOnEnter<DarkHarvest>()
            .ActivateOnEnter<Desolation>()
            .ActivateOnEnter<InHeathCircle>()
            .ActivateOnEnter<InHeathDonut>()
            .ActivateOnEnter<TerrorEye>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 178u, NameID = 5038u)]
public sealed class DD50EddaBlackbosom : BossModule
{
    public DD50EddaBlackbosom(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private DD50EddaBlackbosom(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(300f, 374f), 24.18f, 32)], [new Rectangle(new(300f, 349.299f), 20f, 1.25f)]);
        return (arena.Center, arena);
    }
}
