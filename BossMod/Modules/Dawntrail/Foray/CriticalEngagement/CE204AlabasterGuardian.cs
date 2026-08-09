using BossMod.Dawntrail.Foray.CriticalEngagement;

namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE204AlabasterGuardian;

public enum OID : uint
{
    Boss = 0x4BBE, // R2.5, BNpcName 14509, Alabaster Blade
    AlabasterColossus = 0x4BBF, // R3.0, BNpcName 14510, four spawned command adds
    LightMagicka = 0x4BC0, // R1.0, BNpcName 14511
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 0xC648, // boss->player, no cast, single-target
    Summon = 0xB832,
    FourfoldCommand = 0xB833,
    AttackCommand = 0xB834,
    HomageLong = 0xB835, // colossus, 12.0s cast, 40y 90-degree cone
    HomageShort = 0xB836, // colossus, 3.0s cast, 40y 90-degree cone
    SummonOrbs = 0xB837,
    FabricatedHolyHit = 0xB839, // helper, no cast, raidwide damage
    MagicGust = 0xB83B, // helper, 50y long, 10y wide rect
    MagicStone = 0xB83C, // helper, 40y 60-degree cone
    MagicTornado = 0xB83D, // helper->location, 5y circle
    RightLeftSlash = 0xB83E, // boss, 40y 180-degree cone
    LeftRightSlash = 0xB83F, // boss, 40y 180-degree cone
    SweepRight = 0xB840,
    SweepLeft = 0xB841,
    MagicStorm = 0xB842, // helper, 50y long, 10y wide rect
    StoneSwordShockwave = 0xB843, // boss->self, raidwide visual
    StoneSwordShockwaveHit = 0xB844,
    FabricatedHoly = 0xBA8D // boss->self, 32.0s cast, raidwide visual
}

sealed class AlabasterAOEs(BossModule module) : ReplayValidatedCastAOEs(module)
{
    // Replay-verified: the line helpers stand on the arena mid-lines (e.g. x=-519 for a 90-degree
    // storm row) and the visual crosses the whole arena, so the rect must extend backwards too.
    private static readonly AOEShapeRect Line = new(50f, 5f, 50f);
    private static readonly AOEShapeCone Homage = new(40f, 45f.Degrees());
    private static readonly AOEShapeCone Stone = new(40f, 30f.Degrees());
    private static readonly AOEShapeCircle Tornado = new(5f);

    // Command patterns queue several waves at once. Every AOE in the first wave is real, but
    // making the following wave forbidden at the same time erases the actual safe lanes.
    protected override double RiskyActivationWindow => 0.25d;

    protected override AOEConfig? ConfigFor(uint actionID) => actionID switch
    {
        (uint)AID.HomageLong or (uint)AID.HomageShort => new(Homage),
        (uint)AID.MagicGust or (uint)AID.MagicStorm => new(Line),
        (uint)AID.MagicStone => new(Stone),
        (uint)AID.MagicTornado => new(Tornado, true),
        _ => null
    };
}

sealed class AlabasterSlashes(BossModule module) : ReplayValidatedOppositeAOEs(module)
{
    private static readonly AOEShapeCone Half = new(40f, 90f.Degrees());

    protected override SequenceConfig? ConfigFor(uint firstActionID) => firstActionID switch
    {
        // Replay-verified: the cast packet rotation already points at the first slash's half (e.g.
        // boss facing 180 casts B83E with rotation 90 = its right side; hits land within +-90 deg
        // of the cast rotation, and the follow-up sweep lands on the opposite half). Adding a side
        // offset on top would rotate the pair into a front/back cleave, which is wrong.
        (uint)AID.RightLeftSlash => new(Half, Half, (uint)AID.SweepLeft, 2.20d),
        (uint)AID.LeftRightSlash => new(Half, Half, (uint)AID.SweepRight, 2.20d),
        _ => null
    };
}

sealed class AlabasterRaidwides(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.FabricatedHoly, (uint)AID.StoneSwordShockwave]);

// 四个命令小怪 (Alabaster Colossus) 需要击杀, 否则全员吃 Homage 大伤害。人少时 AI 不打会炸。
sealed class AlabasterAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.AlabasterColossus], 1);

sealed class AlabasterGuardianStates : StateMachineBuilder
{
    public AlabasterGuardianStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AlabasterAOEs>()
            .ActivateOnEnter<AlabasterSlashes>()
            .ActivateOnEnter<AlabasterRaidwides>()
            .ActivateOnEnter<AlabasterAdds>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(AlabasterGuardianStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    PrimaryActorOID = (uint)OID.Boss,
    Contributors = "KanoNoUta",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CriticalEngagement,
    GroupID = 1093u,
    NameID = 51u,
    SortOrder = 3)]
public sealed class AlabasterGuardian(WorldState ws, Actor primary) : BossModule(ws, primary, new(-519f, -641f), new ArenaBoundsCircle(20f));
