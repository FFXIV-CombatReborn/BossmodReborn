namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.DD150Tisiphone;

public enum OID : uint
{
    Boss = 0x181C, // R2.0
    FanaticGargoyle = 0x18EB, // R2.3
    FanaticSuccubus = 0x18EE, // R1.0
    FanaticVodoriga = 0x18EC, // R1.2
    FanaticZombie = 0x18ED // R0.5
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss/FanaticSuccubus->player, no cast, single-target

    BloodRain = 7153, // Boss->location, 5.0s cast, range 100 circle
    BloodSword = 7111, // Boss->FanaticSuccubus, no cast, single-target
    DarkMist = 7108, // Boss->self, 3.0s cast, range 8+R circle
    Desolation = 7112, // FanaticGargoyle->self, 4.0s cast, range 55+R width 6 rect
    FatalAllure = 7110, // Boss->FanaticSuccubus, 2.0s cast, single-target, sucks the HP that remained off the FanaticSuccubus and transfers it to boss
    SummonDarkness = 7107, // Boss->self, no cast, single-target
    SweetSteel = 7148, // FanaticSuccubus->self, no cast, range 6+R(7) 90?-degree cone, currently a safe bet on the cone angel, needs to be confirmed
    TerrorEye = 7113, // FanaticVodoriga->location, 4.0s cast, range 6 circle
    VoidAero = 7177, // Boss->self, 3.0s cast, range 40+R) width 8 rect
    VoidFireII = 7150, // FanaticSuccubus->location, 3.0s cast, range 5 circle
    VoidFireIV = 7109 // Boss->location, 3.5s cast, range 10 circle
}

class BloodRain(BossModule module) : Components.RaidwideCast(module, (uint)AID.BloodRain, "Heavy Raidwide damage! Also killing any add that is currently up");
class BossAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.FanaticZombie, (uint)OID.FanaticSuccubus])
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor.Class.GetRole() is Role.Ranged or Role.Healer)
        {
            // ignore all adds, just attack boss
            hints.PrioritizeTargetsByOID((uint)OID.Boss, 5);
            var zombies = Module.Enemies((uint)OID.FanaticZombie);
            var count = zombies.Count;
            for (var i = 0; i < count; ++i)
            {
                var zombie = zombies[i];
                hints.AddForbiddenZone(new SDCircle(zombie.Position, 3f));
                hints.AddForbiddenZone(new SDCircle(zombie.Position, 8f), WorldState.FutureTime(5d));
            }
        }
        else
        {
            // kill zombies first, they have low health
            hints.PrioritizeTargetsByOID((uint)OID.FanaticZombie, 5);
            // attack boss, ignore succubus
            hints.PrioritizeTargetsByOID((uint)OID.Boss, 1);
        }
    }
}
class DarkMist(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkMist, 10f);
class Desolation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Desolation, new AOEShapeRect(57.3f, 3f));
class FatalAllure(BossModule module) : Components.SingleTargetCast(module, (uint)AID.FatalAllure, "Boss is life stealing from the succubus");
class SweetSteel(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SweetSteel, new AOEShapeCone(7f, 45f.Degrees()));
class TerrorEye(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TerrorEye, 6f);
class VoidAero(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidAero, new AOEShapeRect(42f, 4f));
class VoidFireII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidFireII, 5f);
class VoidFireIV(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidFireIV, 10f);

class EncounterHints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"{Module.PrimaryActor.Name} will spawn 4 zombies, you can either kite them or kill them. The BloodRain raidwide will also kill them if they're still alive. \nThe boss will also life-steal however much HP is left of the Succubus, you're choice if you want to kill it or not.");
    }
}

class DD150TisiphoneStates : StateMachineBuilder
{
    public DD150TisiphoneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BloodRain>()
            .ActivateOnEnter<BossAdds>()
            .ActivateOnEnter<DarkMist>()
            .ActivateOnEnter<Desolation>()
            .ActivateOnEnter<FatalAllure>()
            .ActivateOnEnter<SweetSteel>()
            .ActivateOnEnter<TerrorEye>()
            .ActivateOnEnter<VoidAero>()
            .ActivateOnEnter<VoidFireII>()
            .ActivateOnEnter<VoidFireIV>()
            .DeactivateOnEnter<EncounterHints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 213, NameID = 5424)]
public class DD150Tisiphone : BossModule
{
    public DD150Tisiphone(WorldState ws, Actor primary) : base(ws, primary, SharedBounds.ArenaBounds140150.Center, SharedBounds.ArenaBounds140150)
    {
        ActivateComponent<EncounterHints>();
    }
}
