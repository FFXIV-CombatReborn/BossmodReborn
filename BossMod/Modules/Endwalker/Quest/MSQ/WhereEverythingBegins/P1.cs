namespace BossMod.Endwalker.Quest.MSQ.WhereEverythingBegins.P1;

public enum OID : uint
{
    Boss = 0x39B3, // R7.7
    PlunderedButler = 0x39B5, // R1.3
    PlunderedSteward = 0x39B6, // R1.95
    PlunderedPawn = 0x39B4, // R1.8
    Crystal1 = 0x1EB763, // R0.5
    Crystal2 = 0x1EB764, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 31261, // PlunderedButler->player, no cast, single-target
    AutoAttack2 = 31240, // PlunderedSteward->player, no cast, single-target
    AutoAttack3 = 31239, // PlunderedPawn->player, no cast, single-target
    AutoAttack4 = 31241, // PlunderedPawn->player, no cast, single-target

    Recomposition = 30019, // Boss->self, 8.0s cast, single-target
    NoxVisual = 30020, // Boss->self, 5.0s cast, single-target
    Nox = 30021, // Helper->self, 8.0s cast, range 10 circle
    VoidGravityVisual = 30022, // Boss->self, 5.0s cast, single-target
    VoidGravity = 30023, // Helper->player, 5.0s cast, range 6 circle
    VoidVortexVisual = 30024, // Boss->self, 4.0+1,0s cast, single-target
    VoidVortex = 30025 // Helper->39BE, 5.0s cast, range 6 circle
}

class Nox(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Nox, 10, 5);
class VoidGravity(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.VoidGravity, 6);
class VoidVortex(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.VoidVortex, 6);

class ScarmiglioneP1States : StateMachineBuilder
{
    public ScarmiglioneP1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Nox>()
            .ActivateOnEnter<VoidGravity>()
            .ActivateOnEnter<VoidVortex>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70130, NameID = 11407)]
public class ScarmiglioneP1(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaBounds.Center, ArenaBounds)
{
    public static readonly WPos ArenaCenter = new(0, -148);
    public static readonly ArenaBoundsCustom ArenaBounds = new([new Polygon(ArenaCenter, 19.5f * CosPI.Pi36th, 36),
    new Rectangle(new(0, -129.2f), 3.2f, 1), new Rectangle(new(0, -166.8f), 3.2f, 1)]);
    private static readonly uint[] trash = [(uint)OID.PlunderedButler, (uint)OID.PlunderedSteward, (uint)OID.PlunderedPawn];

    protected override bool CheckPull() => IsAnyActorInCombat(trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, trash);
    }
}

