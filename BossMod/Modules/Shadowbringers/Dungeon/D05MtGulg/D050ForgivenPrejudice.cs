namespace BossMod.Shadowbringers.Dungeon.D05MtGulg.D050ForgivenPrejudice;

public enum OID : uint
{
    Boss = 0x28F2, //R=3.6-5.112
    ForgivenConformity = 0x28EE, //R=1.65
    ForgivenExtortion = 0x28EF, //R=2.7-4.509
    ForgivenVenery = 0x28F1, //R=1.6-2.0
    ForgivenApathy = 0x28F0 //R=8.4
}

public enum AID : uint
{
    AutoAttack = 870, // ForgivenExtortion/ForgivenVenery->player, no cast, single-target
    AutoAttack2 = 872, // Boss->player, no cast, single-target
    RavenousBite = 16812, // ForgivenExtortion->player, no cast, single-target
    SanctifiedAero = 16813, // ForgivenVenery->self, 5.0s cast, range 40 width 8 rect
    PunitiveLight = 16815, // Boss->self, 5.0s cast, range 20 circle
    Sanctification = 16814 // Boss->self, 5.0s cast, range 12 90-degree cone
}

class SanctifiedAero(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SanctifiedAero, new AOEShapeRect(40, 4));
class PunitiveLight(BossModule module) : Components.CastInterruptHint(module, (uint)AID.PunitiveLight, true, true, "Raidwide", true);
class Sanctification(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Sanctification, new AOEShapeCone(12, 45.Degrees()));

class D050ForgivenPrejudiceStates : StateMachineBuilder
{
    public D050ForgivenPrejudiceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Sanctification>()
            .ActivateOnEnter<PunitiveLight>()
            .ActivateOnEnter<SanctifiedAero>()
            .Raw.Update = () => D050ForgivenPrejudice.AllDead(module, (uint)OID.Boss) && D050ForgivenPrejudice.AllDead(module, (uint)OID.ForgivenVenery)
            && D050ForgivenPrejudice.AllDead(module, (uint)OID.ForgivenExtortion) && D050ForgivenPrejudice.AllDead(module, (uint)OID.ForgivenConformity)
            || D050ForgivenPrejudice.AnyInCombat(module, (uint)OID.ForgivenApathy) || D050ForgivenPrejudice.AnyTargetable(module, (uint)OID.ForgivenApathy);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 659, NameID = 8269)]

public class D050ForgivenPrejudice(WorldState ws, Actor primary) : SimpleBossModule(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.Boss));
        Arena.Actors(Enemies((uint)OID.ForgivenExtortion));
        Arena.Actors(Enemies((uint)OID.ForgivenConformity));
        Arena.Actors(Enemies((uint)OID.ForgivenVenery));
        Arena.Actors(Enemies((uint)OID.ForgivenApathy));
    }

    public override bool CheckReset() => false;
    protected override bool CheckPull() => (!AnyInCombat(this, (uint)OID.ForgivenApathy) || !AnyTargetable(this, (uint)OID.ForgivenApathy)) && PrimaryActor.IsTargetable && PrimaryActor.InCombat
        || AnyInCombat(this, (uint)OID.ForgivenExtortion) || AnyInCombat(this, (uint)OID.ForgivenConformity) || AnyInCombat(this, (uint)OID.ForgivenVenery);

    public static bool AllDead(BossModule module, uint oid)
    {
        var enemies = module.Enemies(oid);
        for (var i = 0; i < enemies.Count; ++i)
        {
            if (!enemies[i].IsDead)
            {
                return false;
            }
        }
        return true;
    }

    public static bool AnyInCombat(BossModule module, uint oid)
    {
        var enemies = module.Enemies(oid);
        for (var i = 0; i < enemies.Count; ++i)
        {
            if (enemies[i].InCombat)
            {
                return true;
            }
        }
        return false;
    }

    public static bool AnyTargetable(BossModule module, uint oid)
    {
        var enemies = module.Enemies(oid);
        for (var i = 0; i < enemies.Count; ++i)
        {
            if (enemies[i].IsTargetable)
            {
                return true;
            }
        }
        return false;
    }
}
