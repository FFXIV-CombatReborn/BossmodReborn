namespace BossMod.Dawntrail.Dungeon.D07TenderValley.D072Anthracite;

public enum OID : uint
{
    Boss = 0x41BE, // R4.0
    Bomb1 = 0x1EBA63, // R0.5
    Bomb2 = 0x1EBA62, // R0.5
    Bomb3 = 0x1EBA64, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    AnthrabombVisual1 = 36542, // Boss->self, 6.5s cast, single-target
    AnthrabombVisual2 = 36543, // Helper->location, 7.5s cast, range 10 circle
    AnthrabombVisual3 = 36549, // Helper->location, 4.5s cast, range 10 circle
    AnthrabombVisual4 = 36550, // Helper->location, 4.5s cast, single-target
    AnthrabombVisual5 = 36552, // Helper->location, 8.5s cast, single-target
    Anthrabomb1 = 36401, // Helper->location, 11.5s cast, range 10 circle
    Anthrabomb2 = 36402, // Helper->location, 8.5s cast, range 10 circle
    AnthrabombSpread = 36553, // Helper->player, 6.0s cast, range 6 circle

    HotBlastVisual1 = 36544, // Helper->location, 7.5s cast, single-target
    HotBlastVisual2 = 36546, // Helper->location, 11.5s cast, single-target
    HotBlast1 = 36545, // Helper->self, 11.5s cast, range 40 width 6 rect
    HotBlast2 = 36551, // Helper->self, 8.5s cast, range 40 width 6 rect

    CarbonaceousCombustionVisual = 36556, // Boss->self, 5.0s cast, single-target
    CarbonaceousCombustion = 36557, // Helper->self, 5.5s cast, range 80 circle

    Carniflagration1 = 36547, // Boss->self, 6.5s cast, single-target
    Carniflagration2 = 36548, // Boss->self, 3.5s cast, single-target

    BurningCoalsVisual = 36554, // Boss->self, 5.5s cast, single-target
    BurningCoals = 36555, // Helper->players, 6.0s cast, range 6 circle

    ChimneySmackVisual = 38467, // Boss->self, 4.5s cast, single-target
    ChimneySmack = 38468, // Helper->player, 5.0s cast, single-target, tankbuster
}

sealed class Anthrabomb(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Anthrabomb1, (uint)AID.Anthrabomb2], 10f);
sealed class AnthrabombSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.AnthrabombSpread, 6f);
sealed class HotBlast(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.HotBlast1, (uint)AID.HotBlast2], new AOEShapeRect(40f, 3f));
sealed class CarbonaceousCombustion(BossModule module) : Components.RaidwideCast(module, (uint)AID.CarbonaceousCombustion);
sealed class ChimneySmack(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ChimneySmack);
sealed class BurningCoals(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.BurningCoals, 6f, 4, 4);

sealed class D072AnthraciteStates : StateMachineBuilder
{
    public D072AnthraciteStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Anthrabomb>()
            .ActivateOnEnter<AnthrabombSpread>()
            .ActivateOnEnter<ChimneySmack>()
            .ActivateOnEnter<HotBlast>()
            .ActivateOnEnter<CarbonaceousCombustion>()
            .ActivateOnEnter<BurningCoals>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 834, NameID = 12853)]
public sealed class D072Anthracite : BossModule
{
    public D072Anthracite(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D072Anthracite(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        const float Radius = 3.3f;
        var arena = new ArenaBoundsCustom([new Square(new(-130f, -51f), 18f)], [new Square(new(-118.001f, -56.999f), Radius), new Square(new(-142.001f, -44.999f), Radius),
        new Polygon(new(-124.001f, -38.999f), Radius, 20), new Polygon(new(-136.001f, -62.999f), Radius, 20)], AdjustForHitbox: true);
        return (arena.Center, arena);
    }
}
