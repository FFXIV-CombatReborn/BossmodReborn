namespace BossMod.Heavensward.Dungeon.D08FractalContinuum.D080CrimsonControlUnit;

public enum OID : uint
{
    Boss = 0x1E9910, // R2.0
    ClockworkPredator = 0x102B, // R0.75
    RetooledEnforcementDroid = 0x1029, // R3.5
    ImmortalizedDeathClaw = 0x102A, // R1.5
    ClockworkReservoir = 0x1027, // R0.6
    ImmortalizedInterceptorDrone = 0x1026, // R2.0
    Helper = 0x1B2
}

public enum AID : uint
{
    AutoAttack1 = 870, // ClockworkPredator->player, no cast, single-target
    AutoAttack2 = 872, // ImmortalizedDeathClaw->player, no cast, single-target

    TheHand = 2706, // ImmortalizedDeathClaw->self, 3.0s cast, range 6+R 120-degree cone
    PassiveInfraredGuidanceSystem = 4549, // RetooledEnforcementDroid->player, no cast, range 6 circle
    AetherochemicalAmplification = 4550, // RetooledEnforcementDroid->player, no cast, single-target
    Shred = 2705, // ImmortalizedDeathClaw->self, 2.5s cast, range 4+R width 4 rect
    AutoCannons = 3988, // ImmortalizedInterceptorDrone->self, 3.0s cast, range 40+R width 5 rect
    SpawnReservoir = 3987, // Helper->self, 3.0s cast, range 3 circle
    SelfDetonate = 3078 // ClockworkReservoir->player, no cast, single-target
}

class PassiveInfraredGuidanceSystem(BossModule module) : Components.Cleave(module, (uint)AID.PassiveInfraredGuidanceSystem, new AOEShapeCircle(6f), [(uint)OID.RetooledEnforcementDroid], originAtTarget: true);
class TheHand(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheHand, new AOEShapeCone(7.5f, 60f.Degrees()));
class Shred(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shred, new AOEShapeRect(5.5f, 2f));
class SpawnReservoir(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpawnReservoir, 3f);
class AutoCannons(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AutoCannons, new AOEShapeRect(42f, 2.5f));

class D080CrimsonControlUnitStates : StateMachineBuilder
{
    public D080CrimsonControlUnitStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<D080VioletControlUnit.Interact>()
            .ActivateOnEnter<TheHand>()
            .ActivateOnEnter<Shred>()
            .ActivateOnEnter<AutoCannons>()
            .ActivateOnEnter<SpawnReservoir>()
            .ActivateOnEnter<PassiveInfraredGuidanceSystem>()
            .ActivateOnEnter<D080EmeraldControlUnit.ClockworkReservoir>()
            .Raw.Update = () => module.PrimaryActor.EventState == 7;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 35, NameID = 6924, SortOrder = 8)]
public class D080CrimsonControlUnit(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(-92.05f, -480.33f), new(-81.49f, -479.48f), new(-80.99f, -479.05f), new(-78.48f, -473.69f), new(-78.13f, -473.22f),
    new(-70.41f, -468.28f), new(-69.9f, -468.24f), new(-68.74f, -468.72f), new(-68.11f, -468.75f), new(-59.33f, -468.37f),
    new(-58.57f, -468.24f), new(-59.59f, -466.07f), new(-59.96f, -465.52f), new(-60.77f, -464.53f), new(-61.25f, -464.04f),
    new(-69.97f, -462.72f), new(-70.66f, -462.68f), new(-72.23f, -463.61f), new(-72.78f, -463.46f), new(-75.83f, -462.19f),
    new(-75.88f, -461.53f), new(-75.7f, -460.93f), new(-76.02f, -460.38f), new(-76.62f, -460.18f), new(-77.12f, -460.08f),
    new(-77.33f, -460.55f), new(-77.65f, -461.07f), new(-78.16f, -461.07f), new(-89.62f, -455.68f), new(-90.3f, -455.56f),
    new(-93.62f, -455.55f), new(-94.08f, -455.17f), new(-108.67f, -440.39f), new(-110.58f, -439.96f), new(-110.95f, -439.58f),
    new(-111.28f, -439.05f), new(-111.46f, -438.54f), new(-111.38f, -437.93f), new(-112.75f, -435.89f), new(-113.24f, -435.46f),
    new(-115.12f, -435.02f), new(-115.61f, -434.76f), new(-130.92f, -408.29f), new(-131.05f, -407.72f), new(-130.46f, -405.75f),
    new(-130.54f, -405.1f), new(-131.65f, -402.82f), new(-132.3f, -402.56f), new(-132.64f, -402.05f), new(-132.96f, -401.46f),
    new(-132.93f, -400.9f), new(-132.5f, -399.62f), new(-132.49f, -398.94f), new(-138.01f, -378.9f), new(-137.87f, -378.33f),
    new(-136.25f, -375.53f), new(-136.11f, -374.89f), new(-135.07f, -362.66f), new(-134.87f, -362.12f), new(-134.36f, -362.06f),
    new(-133.63f, -362.15f), new(-133.41f, -361.04f), new(-133.36f, -360.53f), new(-134.48f, -360.24f), new(-134.91f, -359.9f),
    new(-134.45f, -356.65f), new(-134.3f, -356.07f), new(-132.62f, -355.14f), new(-129.32f, -346.73f), new(-129.2f, -346.07f),
    new(-129.93f, -344.26f), new(-131.39f, -342.08f), new(-131.95f, -342.47f), new(-136.9f, -350.25f), new(-137.05f, -350.9f),
    new(-137.14f, -351.58f), new(-137.31f, -352.1f), new(-145.39f, -356.29f), new(-145.98f, -356.48f), new(-152.39f, -355.93f),
    new(-152.87f, -356.41f), new(-158.97f, -365.4f), new(-161.27f, -371.83f), new(-167.35f, -399.67f), new(-164.89f, -404.44f),
    new(-164.85f, -406.41f), new(-165.13f, -406.88f), new(-167.62f, -409.97f), new(-166.23f, -422.48f), new(-165.59f, -422.74f),
    new(-164.99f, -422.9f), new(-164.73f, -423.35f), new(-165.05f, -424.55f), new(-165.39f, -424.93f), new(-166.05f, -424.83f),
    new(-166.31f, -425.26f), new(-166.72f, -425.59f), new(-168.6f, -424.97f), new(-168.87f, -425.51f), new(-168.27f, -429.82f),
    new(-167.83f, -429.3f), new(-167.32f, -429.26f), new(-166.94f, -429.61f), new(-165.7f, -430.06f), new(-165.54f, -430.7f),
    new(-165.68f, -431.31f), new(-165.92f, -431.79f), new(-176.35f, -439.56f), new(-179.51f, -442.87f), new(-179.62f, -443.6f),
    new(-170.41f, -459.46f), new(-165.88f, -458.41f), new(-153.74f, -453.15f), new(-153.17f, -453.14f), new(-152.03f, -453.5f),
    new(-152.18f, -454.88f), new(-152.06f, -455.49f), new(-152.57f, -455.9f), new(-153.1f, -456.13f), new(-149.08f, -459.18f),
    new(-148.73f, -457.34f), new(-148.51f, -456.8f), new(-147.98f, -456.73f), new(-147.11f, -455.99f), new(-145.81f, -456.3f),
    new(-145.44f, -456.8f), new(-145.58f, -457.39f), new(-145.66f, -458.07f), new(-135.59f, -465.54f), new(-131.55f, -464.89f),
    new(-130.99f, -464.95f), new(-129.35f, -465.95f), new(-126.72f, -470.09f), new(-126.23f, -470.57f), new(-99.48f, -479.05f),
    new(-92.71f, -480.29f), new(-92.05f, -480.33f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)], [new Rectangle(new(-126.383f, -423.171f), 1.39f, 0.9f, 60f.Degrees()),
    new Circle(new(-89.835f, -468.233f), 1.5f), new Circle(new(-147.133f, -369.01f), 1.5f)]);
    private static readonly uint[] trash = [(uint)OID.ImmortalizedDeathClaw, (uint)OID.RetooledEnforcementDroid, (uint)OID.ClockworkPredator, (uint)OID.ClockworkReservoir,
    (uint)OID.ImmortalizedInterceptorDrone];

    public override bool CheckReset() => !Raid.Player()!.Position.InSquare(Arena.Center, 70f);

    protected override bool CheckPull() => IsAnyActorInCombat(trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, trash);
        Arena.Actor(PrimaryActor, Colors.Object);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
            hints.PotentialTargets[i].Priority = 0;
    }
}
