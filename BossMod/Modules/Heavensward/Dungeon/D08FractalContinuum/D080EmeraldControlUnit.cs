namespace BossMod.Heavensward.Dungeon.D08FractalContinuum.D080EmeraldControlUnit;

public enum OID : uint
{
    Boss = 0x1E990E, // R2.0
    ImmortalizedColossus = 0x1028, // R3.0
    ClockworkPredator = 0x102B, // R0.75
    ClockworkReservoir = 0x1027, // R0.6
    ImmortalizedInterceptorDrone = 0x1026, // R2.0
    Helper = 0x1B2
}

public enum AID : uint
{
    AutoAttack2 = 870, // ClockworkPredator/ImmortalizedColossus->player, no cast, single-target

    Exhaust = 1427, // ImmortalizedColossus->self, 3.0s cast, range 30+R width 12 rect
    GrandSword = 1425, // ImmortalizedColossus->self, no cast, range 6+R 120-degree cone
    MagitekRay = 2842, // ImmortalizedColossus->location, no cast, range 6 circle
    Overheat = 2843, // ImmortalizedColossus->self, 3.0s cast, single-target
    AutoCannons = 3988, // ImmortalizedInterceptorDrone->self, 3.0s cast, range 40+R width 5 rect
    SpawnReservoir = 3987, // Helper->self, 3.0s cast, range 3 circle
    SelfDetonate = 3078 // ClockworkReservoir->player, no cast, single-target
}

class Exhaust(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Exhaust, new AOEShapeRect(33f, 6f));
class AutoCannons(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AutoCannons, new AOEShapeRect(42f, 2.5f));
class SpawnReservoir(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpawnReservoir, 3f);

class ClockworkReservoir(BossModule module) : Components.Voidzone(module, 3f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.ClockworkReservoir);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

class D080EmeraldControlUnitStates : StateMachineBuilder
{
    public D080EmeraldControlUnitStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<D080VioletControlUnit.Interact>()
            .ActivateOnEnter<AutoCannons>()
            .ActivateOnEnter<Exhaust>()
            .ActivateOnEnter<SpawnReservoir>()
            .ActivateOnEnter<ClockworkReservoir>()
            .Raw.Update = () => module.PrimaryActor.EventState == 7;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 35, NameID = 6924, SortOrder = 7)]
public class D080EmeraldControlUnit(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(92.74f, -479.98f), new(99.71f, -478.71f), new(126.55f, -470.17f), new(126.99f, -469.63f), new(129.46f, -465.74f),
    new(129.96f, -465.37f), new(131.09f, -464.68f), new(131.6f, -464.52f), new(135.34f, -465.12f), new(136f, -465.06f),
    new(145.71f, -457.82f), new(145.8f, -457.14f), new(145.63f, -456.57f), new(145.77f, -456.08f), new(147.02f, -455.68f),
    new(147.52f, -455.73f), new(147.81f, -456.25f), new(148.31f, -456.4f), new(148.88f, -456.69f), new(149.25f, -458.6f),
    new(149.76f, -458.55f), new(152.98f, -456.02f), new(152.66f, -455.54f), new(152.27f, -455.16f), new(152.5f, -454.61f),
    new(152.19f, -453.39f), new(152.57f, -453.05f), new(153.17f, -452.86f), new(153.75f, -452.77f), new(166.04f, -458.07f),
    new(170.38f, -459.08f), new(170.85f, -458.81f), new(179.79f, -443.29f), new(179.97f, -442.82f), new(176.9f, -439.54f),
    new(165.96f, -431.31f), new(165.7f, -430.14f), new(166.1f, -429.66f), new(166.71f, -429.5f), new(167.21f, -429.08f),
    new(167.84f, -429.02f), new(168.47f, -429.5f), new(169.09f, -425.09f), new(168.95f, -424.57f), new(167.22f, -425.18f),
    new(166.58f, -425.08f), new(166.23f, -424.49f), new(165.58f, -424.56f), new(165.26f, -424.18f), new(164.93f, -422.96f),
    new(165.37f, -422.5f), new(165.96f, -422.34f), new(166.46f, -421.93f), new(167.79f, -410.17f), new(167.73f, -409.5f),
    new(165.18f, -406.36f), new(165.05f, -405.81f), new(165.08f, -404.52f), new(165.15f, -403.91f), new(167.43f, -399.54f),
    new(167.49f, -398.83f), new(161.53f, -371.73f), new(159.01f, -364.74f), new(152.92f, -355.87f), new(152.31f, -355.61f),
    new(146.44f, -356.11f), new(145.82f, -356.07f), new(137.44f, -351.69f), new(137.29f, -351.13f), new(137.21f, -350.49f),
    new(137.04f, -349.85f), new(131.93f, -341.83f), new(131.42f, -341.98f), new(130.34f, -343.54f), new(130.03f, -344.15f),
    new(129.38f, -345.99f), new(132.65f, -354.33f), new(132.99f, -354.89f), new(134.2f, -355.54f), new(134.58f, -355.91f),
    new(135.06f, -359.23f), new(135f, -359.74f), new(134.48f, -359.96f), new(133.86f, -360.11f), new(133.58f, -360.69f),
    new(133.74f, -361.3f), new(134.27f, -361.66f), new(134.86f, -361.71f), new(135.26f, -362.02f), new(136.34f, -374.87f),
    new(136.61f, -375.51f), new(138.24f, -378.39f), new(132.64f, -398.83f), new(132.78f, -399.51f), new(133.21f, -400.8f),
    new(133.33f, -401.49f), new(133.06f, -402f), new(131.83f, -402.48f), new(130.73f, -404.74f), new(130.65f, -405.42f),
    new(131.23f, -407.35f), new(131.13f, -407.92f), new(115.79f, -434.46f), new(115.27f, -434.69f), new(113.41f, -435.13f),
    new(112.91f, -435.57f), new(111.82f, -437.19f), new(111.66f, -438.28f), new(111.41f, -438.84f), new(111.06f, -439.4f),
    new(110.6f, -439.67f), new(109.33f, -439.93f), new(108.69f, -440.21f), new(93.79f, -455.23f), new(90.5f, -455.22f),
    new(89.85f, -455.32f), new(78.52f, -460.64f), new(77.95f, -460.76f), new(77.64f, -460.31f), new(77.29f, -459.67f),
    new(76.01f, -460.09f), new(75.92f, -460.74f), new(76.11f, -461.35f), new(76.05f, -461.86f), new(72.49f, -463.27f),
    new(71.98f, -463.04f), new(70.9f, -462.37f), new(70.2f, -462.36f), new(61.66f, -463.66f), new(61.09f, -464.01f),
    new(59.89f, -465.54f), new(59.06f, -467.3f), new(58.83f, -468.01f), new(68.92f, -468.4f), new(70.18f, -467.88f),
    new(70.7f, -468.02f), new(78.35f, -472.9f), new(78.69f, -473.39f), new(81.11f, -478.57f), new(81.51f, -479.14f),
    new(92.74f, -479.98f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)], [new Rectangle(new(127.436f, -423.387f), 1.39f, 0.9f, -60f.Degrees()),
    new Circle(new(147.318f, -368.699f), 1.5f), new Circle(new(90.074f, -467.885f), 1.5f)]);
    private static readonly uint[] trash = [(uint)OID.ImmortalizedColossus, (uint)OID.ImmortalizedInterceptorDrone, (uint)OID.ClockworkPredator, (uint)OID.ClockworkReservoir];

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
