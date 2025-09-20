namespace BossMod.Heavensward.Dungeon.D04TheVault.D043SerCharibert;

public enum OID : uint
{
    Boss = 0x1056, // R2.2
    Charibert = 0xF71EE, // R0.5
    DawnKnight = 0x1057, // R2.0
    DuskKnight = 0x1058, // R2.0
    HolyFlame = 0x1059, // R1.5
    Helper = 0xD25
}

public enum AID : uint
{
    AutoAttack = 4143, // Boss->player, no cast, single-target
    Visual1 = 4121, // Boss->self, no cast, single-target
    Visual2 = 4120, // Boss->self, no cast, single-target

    AltarCandle = 4144, // Boss->player, no cast, single-target tankbuster

    HeavensflameTelegraph = 4145, // Boss->self, 2.5s cast, single-target
    HeavensflameAOE = 4146, // Helper->location, 3.0s cast, range 5 circle ground targetted aoe
    HolyChainTelegraph = 4147, // Boss->self, 2.0s cast, single-target
    HolyChainPlayerTether = 4148, // Helper->self, no cast, ??? tether

    AltarPyre = 4149, // Boss->location, 3.0s cast, range 80 circle raidwide
    StartLimitBreakPhase = 4150, // Boss->self, no cast, single-target
    SacredFlame = 4156, // HolyFlame->self, no cast, range 80+R circle raidewide, "enrage" for each flame not killed within 30s
    PureOfHeart = 4151, // Boss->location, no cast, range 80 circle raidewide

    WhiteKnightsTour = 4152, // DawnKnight->self, 3.0s cast, range 40+R width 4 rect
    BlackKnightsTour = 4153, // DuskKnight->self, 3.0s cast, range 40+R width 4 rect

    TurretChargeDawnKnight = 4154, // Helper->player, no cast, only triggers if inside hitbox
    TurretChargeRestDuskKnight = 4155 // Helper->player, no cast, only triggers if inside hitbox
}

public enum TetherID : uint
{
    HolyChain = 9 // player->player
}

sealed class KnightsTour(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.WhiteKnightsTour, (uint)AID.BlackKnightsTour], new AOEShapeRect(40f, 2f));

sealed class AltarPyre(BossModule module) : Components.RaidwideCast(module, (uint)AID.AltarPyre);

sealed class HeavensflameAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavensflameAOE, 5f);
sealed class HolyChain(BossModule module) : Components.Chains(module, (uint)TetherID.HolyChain, (uint)AID.HolyChainPlayerTether);
sealed class TurretTour(BossModule module) : Components.Voidzone(module, 2f, GetVoidzones, 10f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies(D043SerCharibert.Knights);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.ModelState.ModelState == 8)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}
sealed class TurretTourHint(BossModule module) : Components.Voidzone(module, 2f, GetVoidzones, 3f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies(D043SerCharibert.Knights);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.ModelState.ModelState != 8 && !z.Position.AlmostEqual(module.Center, 10f))
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

sealed class D043SerCharibertStates : StateMachineBuilder
{
    public D043SerCharibertStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<KnightsTour>()
            .ActivateOnEnter<AltarPyre>()
            .ActivateOnEnter<HeavensflameAOE>()
            .ActivateOnEnter<HolyChain>()
            .ActivateOnEnter<TurretTourHint>()
            .ActivateOnEnter<TurretTour>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS), Xyzzy", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 34, NameID = 3642)]
public sealed class D043SerCharibert : BossModule
{
    public D043SerCharibert(WorldState ws, Actor primary) : base(ws, primary, new(default, 4.1f), new ArenaBoundsSquare(19.5f))
    {
        flames = Enemies((uint)OID.HolyFlame);
    }

    private readonly List<Actor> flames;
    public static readonly uint[] Knights = [(uint)OID.DawnKnight, (uint)OID.DuskKnight];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(flames, Colors.Object);
    }
}
