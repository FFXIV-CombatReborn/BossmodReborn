namespace BossMod.Dawntrail.Foray.FATE.ADelicateBalance;

public enum OID : uint
{
    Boss = 0x46B8, // R3.5
    HandDryer = 0x46B9, // R2.0
    HotAir = 0x46BA, // R3.2
    CryOfHavoc = 0x4770, // R3.0
    Helper2 = 0x46BB,
    Helper = 0x46DB
}

public enum AID : uint
{
    AutoAttack = 39460, // Boss->player, no cast, single-target
    Teleport = 30352, // Boss->location, no cast, single-target

    FluidSwing1 = 30351, // Boss->self, 7.0s cast, range 60 90-degree cone
    FluidSwing2 = 30353, // Boss->location, 7.0s cast, range 60 90-degree cone

    Reproduce = 30339, // Boss->self, 3.0s cast, single-target
    HeatVortex = 30340, // HotAir->self, 4.0s cast, range 10 circle
    FireBlast = 30341, // HandDryer->self, 5.0s cast, range 25 width 6 rect
    EruptionVisual = 30345, // Boss->self, 3.0s cast, single-target
    Eruption = 30346, // Helper2->location, 3.0s cast, range 8 circle
    Visual = 4731, // Helper2->self, no cast, single-target
    DryCycleVisual = 30342, // Boss->self, 3.0s cast, single-target
    DryCycle = 30344, // Helper->self, 8.0s cast, range 5-40 donut

    ParanormalWave = 42160 // CryOfHavoc->self, 3.0s cast, range 10 120-degree cone
}

sealed class FluidSwing(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.FluidSwing1, (uint)AID.FluidSwing2], new AOEShapeCone(60f, 45f.Degrees()));
sealed class HeatVortex(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeatVortex, 10f);
sealed class FireBlast(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FireBlast, new AOEShapeRect(25f, 3f));
sealed class Eruption(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Eruption, 8f);
sealed class DryCycle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DryCycle, new AOEShapeDonut(5f, 40f));
sealed class ParanormalWave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ParanormalWave, new AOEShapeCone(10f, 60f.Degrees()));

sealed class ADelicateBalanceStates : StateMachineBuilder
{
    public ADelicateBalanceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FluidSwing>()
            .ActivateOnEnter<HeatVortex>()
            .ActivateOnEnter<FireBlast>()
            .ActivateOnEnter<Eruption>()
            .ActivateOnEnter<DryCycle>()
            .ActivateOnEnter<ParanormalWave>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.ForayFATE, GroupID = 1018, NameID = 1968)]
public sealed class ADelicateBalance(WorldState ws, Actor primary) : SimpleBossModule(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.CryOfHavoc));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Boss => 1,
                _ when e.Actor.InCombat => 0,
                _ => AIHints.Enemy.PriorityUndesirable
            };
        }
    }
}
