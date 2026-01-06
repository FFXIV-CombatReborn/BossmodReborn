namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

[SkipLocalsInit]
sealed class UnlimitedExpress(BossModule module) : Components.RaidwideCast(module, (uint)AID.UnlimitedExpress);
[SkipLocalsInit]
sealed class ElectrayLong(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Electray1, (uint)AID.Electray4], new AOEShapeRect(25f, 2.5f));
[SkipLocalsInit]
sealed class ElectrayMedium(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Electray2, new AOEShapeRect(20f, 2.5f));
[SkipLocalsInit]
sealed class ElectrayShort(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Electray3, new AOEShapeRect(5f, 2.5f));
[SkipLocalsInit]
sealed class LightningBurst(BossModule module) : Components.BaitAwayIcon(module, 5f, (uint)IconID.LightningBurst, (uint)AID.LightningBurst, 5.6f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

[SkipLocalsInit]
sealed class Shockwave(BossModule module) : Components.RaidwideAfterMapEffect(module, aid=(uint)AID.Shockwave, mapEffectIndex=0x30, mapEffectState=0x00080004u, delay=16d);

[SkipLocalsInit]
sealed class ThirdRail(BossModule module) : Components.CastTwister(module, radius=4f, oid=(uint)OID.Doomtrain, aid=(uint)AID.ThirdRail, activationDelay=3d);

[SkipLocalsInit]
sealed class Intermission(BossModule module) : BossComponent(module)
{
    public bool Started;

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        // This *seems* to be how we detect intermission starts, it does a bit of setup before it triggers this.
        if (updateID == 0x8000000C && param1 == 0x59 && param2 == 0x0 && param3 == 0x0 && param4 == 0x0)
        {
            Started = true;
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
StatesType = typeof(Ex7DoomtrainStates),
ConfigType = null, // replace null with typeof(DoomtrainConfig) if applicable
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = typeof(SID),
TetherIDType = null,
IconIDType = typeof(IconID),
PrimaryActorOID = (uint)OID.Doomtrain,
Contributors = "",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.Dungeon,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1077u,
NameID = 14284u,
SortOrder = 1,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class Ex7Doomtrain : BossModule
{
    public Ex7Doomtrain(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private Ex7Doomtrain(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Rectangle(new(100f, 100f), 10.5f, 15f)],
        [new Square(new(102.5f, 97.5f), 2.01f), new Square(new(97.5f, 107.5f), 2.01f)], AdjustForHitboxInwards: true);
        return (arena.Center, arena);
    }

    // car 3: new ArenaBoundsRect(10.5f, 15f), new(100f, 200f)
    // intermission: new ArenaBoundsCustom([new Polygon(new(-400f, -400f), 14.495f, 64)]);
    // private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    // {
    //     var arena = new ArenaBoundsCustom([new Rectangle(new(100f, 250f), 10.5f, 15f)],
    //     [new Rectangle(new(107.5f, 250f), 2.297f, 4.797f), new Rectangle(new(100f, 252.5f), 5f, 1.95f),
    //     new Rectangle(new(92.5f, 257.5f), 2.297f, 7.297f)], [new Square(new(107.5f, 247.5f), 2.5f), new Rectangle(new(107.75f, 252.5f), 2.25f, 2.5f),
    //     new Rectangle(new(92.5f, 259.75f), 2.5f, 4.75f), new Rectangle(new(92.25f, 252.5f), 2.25f, 2.5f)], AdjustForHitboxInwards: true);
    //     return (arena.Center, arena);
    // }

    // car 5: 92.5, 292.5, 97.5, 292.5, 97.5, 297.5
    // 0x10         0x11                0x15
}
