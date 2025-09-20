namespace BossMod.Heavensward.Dungeon.D08FractalContinuum.D080FlawlessNaga;

public enum OID : uint
{
    Boss = 0x1023, // R1.35
    Shabti = 0x1022, // R1.1
    FlawlessEmpuse = 0x1021, // R1.5
    Iksalion = 0x1025, // R1.2
    FlawlessChimera = 0x1024 // R3.7
}

public enum AID : uint
{
    AutoAttack1 = 870, // Shabti/FlawlessEmpuse/FlawlessChimera->player, no cast, single-target
    AutoAttack2 = 871, // Iksalion->player, no cast, single-target
    AutoAttack3 = 872, // Boss->player, no cast, single-target

    Furore = 4555, // Iksalion->self, 4.0s cast, range 30+R circle
    CalcifyingMist = 3027, // Boss->self, 3.0s cast, range 6+R 90-degree cone
    BalefulRoar = 3028, // Boss->self, 4.0s cast, range 20+R circle
    TheLionsBreath = 2143, // FlawlessChimera->self, no cast, range 6+R 120-degree cone
    TheRamsVoice = 2144, // FlawlessChimera->self, 4.0s cast, range 6+R circle
    TheDragonsVoice = 2145 // FlawlessChimera->self, 4.0s cast, range 4+R-30 donut
}

class TheDragonsVoice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheDragonsVoice, new AOEShapeDonut(7.7f, 30f));
class TheRamsVoice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheRamsVoice, 9.7f);
class CalcifyingMist(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CalcifyingMist, new AOEShapeCone(7.35f, 45f.Degrees()));
class TheLionsBreath(BossModule module) : Components.Cleave(module, (uint)AID.TheLionsBreath, new AOEShapeCone(9.7f, 60f.Degrees()), [(uint)OID.FlawlessChimera], activeWhileCasting: false);

abstract class InterruptHints(BossModule module, uint aid) : Components.CastInterruptHint(module, aid, showNameInHint: true, canBeStunned: true);
class TheRamsVoiceHint(BossModule module) : InterruptHints(module, (uint)AID.TheRamsVoice);
class TheDragonsVoiceHint(BossModule module) : InterruptHints(module, (uint)AID.TheDragonsVoice);
class Furore(BossModule module) : InterruptHints(module, (uint)AID.Furore);
class BalefulRoar(BossModule module) : InterruptHints(module, (uint)AID.BalefulRoar);

class D080FlawlessNagaStates : StateMachineBuilder
{
    private static readonly uint[] trash = [(uint)OID.FlawlessEmpuse, (uint)OID.Boss];
    public D080FlawlessNagaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheDragonsVoice>()
            .ActivateOnEnter<TheRamsVoice>()
            .ActivateOnEnter<CalcifyingMist>()
            .ActivateOnEnter<TheLionsBreath>()
            .ActivateOnEnter<TheRamsVoiceHint>()
            .ActivateOnEnter<TheDragonsVoiceHint>()
            .ActivateOnEnter<Furore>()
            .ActivateOnEnter<BalefulRoar>()
            .Raw.Update = () => AllDestroyed(trash) && AllDeadOrDestroyed(D080FlawlessNaga.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 35, NameID = 3419, SortOrder = 4)]
public class D080FlawlessNaga(WorldState ws, Actor primary) : BossModule(ws, primary, arena1.Center, arena1)
{
    private static readonly WPos[] vertices = [new(-299.46f, 53.5f), new(-298.11f, 55.78f), new(-297.83f, 56.41f), new(-297.19f, 60.28f), new(-295.58f, 63.81f),
    new(-294.81f, 64.8f), new(-294.44f, 65.4f), new(-294.25f, 66.09f), new(-295.04f, 69.26f), new(-295.38f, 69.85f),
    new(-295.49f, 70.36f), new(-295.24f, 70.85f), new(-294.7f, 71.21f), new(-294.29f, 71.62f), new(-302.87f, 75.17f),
    new(-303.36f, 75.03f), new(-303.93f, 74.73f), new(-304.56f, 74.82f), new(-305.15f, 74.96f), new(-305.57f, 75.46f),
    new(-305.91f, 75.99f), new(-305.86f, 76.63f), new(-306.25f, 77.17f), new(-310.28f, 80.21f), new(-310.94f, 80.34f),
    new(-311.55f, 80.31f), new(-312.11f, 80.58f), new(-312.51f, 81.04f), new(-312.7f, 81.62f), new(-312.63f, 82.23f),
    new(-312.38f, 82.74f), new(-311.88f, 83.08f), new(-311.25f, 83.24f), new(-310.61f, 83.11f), new(-310.08f, 83.53f),
    new(-308.32f, 85.28f), new(-307.92f, 85.84f), new(-307.81f, 86.46f), new(-305.99f, 88.32f), new(-305.44f, 88.81f),
    new(-304.34f, 89.21f), new(-298.83f, 94.76f), new(-298.42f, 95.33f), new(-298.52f, 96.03f), new(-298.35f, 96.58f),
    new(-297.96f, 97.04f), new(-297.44f, 97.29f), new(-296.81f, 97.32f), new(-296.25f, 97.1f), new(-295.81f, 96.64f),
    new(-295.4f, 95.5f), new(-293.92f, 94.36f), new(-287.88f, 91.84f), new(-287.23f, 91.8f), new(-286.68f, 91.98f),
    new(-286.03f, 92.06f), new(-285.51f, 91.83f), new(-284.98f, 91.42f), new(-284.66f, 90.9f), new(-277.75f, 89.99f),
    new(-270.97f, 90.91f), new(-270.55f, 91.42f), new(-270.05f, 91.8f), new(-269.54f, 92.07f), new(-268.84f, 91.97f),
    new(-268.27f, 91.81f), new(-267.63f, 91.81f), new(-261.78f, 94.24f), new(-256.51f, 98.22f), new(-256.05f, 98.7f),
    new(-256.02f, 99.32f), new(-255.76f, 99.92f), new(-255.36f, 100.3f), new(-254.79f, 100.53f), new(-254.2f, 100.6f),
    new(-253.67f, 101.07f), new(-249.83f, 106.1f), new(-249.06f, 107.91f), new(-249.25f, 109.16f), new(-249.09f, 109.75f),
    new(-248.71f, 110.22f), new(-248.15f, 110.49f), new(-247.57f, 110.52f), new(-246.97f, 110.28f), new(-246.56f, 109.87f),
    new(-246.35f, 109.28f), new(-245.93f, 108.99f), new(-212.79f, 100.14f), new(-212.23f, 100.49f), new(-211.19f, 100.59f),
    new(-209.46f, 107.7f), new(-209.64f, 108.33f), new(-210.16f, 108.69f), new(-210.32f, 109.32f), new(-243.97f, 117.94f),
    new(-244.5f, 117.46f), new(-245.11f, 117.29f), new(-245.72f, 117.38f), new(-246.22f, 117.7f), new(-246.56f, 118.21f),
    new(-246.66f, 118.84f), new(-246.5f, 119.41f), new(-245.7f, 120.44f), new(-245.52f, 121.88f), new(-245.53f, 122.59f),
    new(-245.63f, 123.35f), new(-250.09f, 120.89f), new(-250.68f, 120.64f), new(-252.72f, 120.18f), new(-253.33f, 120.27f),
    new(-257.15f, 121.21f), new(-257.71f, 121.44f), new(-259.28f, 122.65f), new(-261.17f, 122.86f), new(-261.86f, 122.84f),
    new(-265.09f, 122.15f), new(-265.57f, 121.65f), new(-265.66f, 120.93f), new(-266.54f, 119.02f), new(-268.05f, 117.62f),
    new(-269.9f, 116.9f), new(-270.56f, 116.83f), new(-271.09f, 114.24f), new(-271.39f, 113.65f), new(-272.59f, 112.05f),
    new(-274.3f, 111.01f), new(-274.86f, 110.8f), new(-276.75f, 110.51f), new(-279.15f, 111.05f), new(-280.68f, 112.17f),
    new(-281.12f, 112.57f), new(-281.45f, 113.13f), new(-282.03f, 113.5f), new(-282.61f, 113.29f), new(-283.92f, 113.01f),
    new(-284.56f, 113.02f), new(-285.83f, 113.26f), new(-286.39f, 113.48f), new(-288.01f, 114.57f), new(-289.06f, 116.15f),
    new(-289.55f, 116.65f), new(-293.09f, 115.05f), new(-295.47f, 113.04f), new(-297.46f, 110.62f), new(-297.9f, 110.17f),
    new(-299.76f, 110.15f), new(-300.34f, 109.83f), new(-301.74f, 108.48f), new(-303.58f, 108.15f), new(-305.69f, 108.06f),
    new(-306.47f, 107.95f), new(-305.74f, 106.2f), new(-304.56f, 104.64f), new(-303.45f, 104.25f), new(-302.97f, 103.82f),
    new(-302.73f, 103.21f), new(-302.75f, 102.58f), new(-303.03f, 102.01f), new(-303.53f, 101.63f), new(-304.1f, 101.47f),
    new(-304.71f, 101.57f), new(-305.3f, 101.25f), new(-310.78f, 95.76f), new(-311.04f, 95.12f), new(-311.19f, 94.6f),
    new(-312.56f, 93.23f), new(-313f, 92.71f), new(-313.57f, 92.24f), new(-314.13f, 92.16f), new(-314.59f, 91.93f),
    new(-316.87f, 89.57f), new(-316.8f, 88.9f), new(-316.9f, 88.29f), new(-317.24f, 87.74f), new(-317.76f, 87.42f),
    new(-318.34f, 87.32f), new(-318.99f, 87.52f), new(-319.47f, 87.94f), new(-319.71f, 88.54f), new(-319.7f, 89.1f),
    new(-319.74f, 89.72f), new(-322.89f, 93.75f), new(-323.38f, 94.2f), new(-324.02f, 94.14f), new(-324.52f, 94.43f),
    new(-325.04f, 94.84f), new(-325.21f, 95.4f), new(-325.29f, 96.03f), new(-324.87f, 97.18f), new(-329.04f, 106.98f),
    new(-329.71f, 107.07f), new(-330.12f, 107.46f), new(-330.54f, 108.02f), new(-330.56f, 108.61f), new(-330.48f, 109.22f),
    new(-330.12f, 109.7f), new(-329.79f, 110.28f), new(-331.06f, 119.91f), new(-331.24f, 120.59f), new(-331.78f, 120.97f),
    new(-332.16f, 121.43f), new(-332.41f, 122.05f), new(-332.35f, 122.64f), new(-332.07f, 123.29f), new(-331.69f, 123.65f),
    new(-331.13f, 124.04f), new(-330.44f, 129.45f), new(-330.84f, 129.99f), new(-331.07f, 130.51f), new(-331.08f, 131.09f),
    new(-330.85f, 131.76f), new(-330.41f, 132.24f), new(-329.85f, 132.48f), new(-329.23f, 132.46f), new(-328.67f, 132.18f),
    new(-328.28f, 131.7f), new(-328.05f, 131.04f), new(-324.87f, 130.18f), new(-324.38f, 130.29f), new(-323.81f, 130.37f),
    new(-320.6f, 129.51f), new(-320.26f, 129.06f), new(-319.82f, 128.81f), new(-311.86f, 126.67f), new(-310.65f, 127.23f),
    new(-310.04f, 127.26f), new(-309.47f, 127.04f), new(-308.99f, 126.55f), new(-308.29f, 126.49f), new(-299.42f, 127.75f),
    new(-298.92f, 127.47f), new(-297.19f, 125.59f), new(-296.58f, 125.25f), new(-292.01f, 125.12f), new(-290.1f, 124.73f),
    new(-289.56f, 124.85f), new(-289.66f, 125.4f), new(-289.43f, 126.7f), new(-289.21f, 127.3f), new(-288.46f, 128.43f),
    new(-287.34f, 129.18f), new(-286.72f, 129.44f), new(-285.38f, 129.69f), new(-284.73f, 129.56f), new(-284.34f, 130.07f),
    new(-284.21f, 130.72f), new(-284.01f, 131.31f), new(-283.29f, 132.39f), new(-282.82f, 132.81f), new(-281.68f, 133.53f),
    new(-280.42f, 133.78f), new(-279.79f, 133.8f), new(-278.52f, 133.55f), new(-277.97f, 133.27f), new(-276.92f, 132.6f),
    new(-276.6f, 132.11f), new(-276.22f, 131.76f), new(-275.12f, 132.5f), new(-274.6f, 132.7f), new(-273.31f, 132.96f),
    new(-272.14f, 132.73f), new(-271.58f, 132.55f), new(-270.5f, 131.84f), new(-270.08f, 131.41f), new(-269.57f, 131.77f),
    new(-268.86f, 133.59f), new(-268.56f, 134.09f), new(-268.06f, 134.29f), new(-264.24f, 134.57f), new(-254.92f, 137.42f),
    new(-251.74f, 137.75f), new(-250.43f, 138.07f), new(-249.72f, 138.37f), new(-250.84f, 139.83f), new(-251.42f, 140.16f),
    new(-251.98f, 140.3f), new(-252.46f, 140.67f), new(-252.73f, 141.21f), new(-252.81f, 141.8f), new(-252.56f, 142.39f),
    new(-252.11f, 142.84f), new(-251.49f, 143.06f), new(-256.94f, 148.19f), new(-257.3f, 147.73f), new(-257.81f, 147.29f),
    new(-258.37f, 147.08f), new(-258.98f, 147.12f), new(-259.53f, 147.41f), new(-259.95f, 147.88f), new(-260.06f, 148.44f),
    new(-260.07f, 149.09f), new(-261.56f, 150.23f), new(-268.14f, 152.94f), new(-268.7f, 152.62f), new(-269.4f, 152.53f),
    new(-269.94f, 152.7f), new(-270.49f, 153.13f), new(-270.78f, 153.67f), new(-277.55f, 154.55f), new(-284.74f, 153.66f),
    new(-285.04f, 153.1f), new(-285.57f, 152.7f), new(-286.07f, 152.52f), new(-286.68f, 152.6f), new(-287.26f, 152.75f),
    new(-287.94f, 152.74f), new(-294f, 150.2f), new(-299.03f, 146.29f), new(-299.43f, 145.78f), new(-299.54f, 145.19f),
    new(-299.79f, 144.59f), new(-300.23f, 144.24f), new(-300.83f, 144f), new(-301.45f, 144.03f), new(-305.61f, 138.59f),
    new(-306.42f, 136.78f), new(-306.36f, 136.06f), new(-306.24f, 135.42f), new(-306.39f, 134.84f), new(-306.75f, 134.38f),
    new(-307.27f, 134.09f), new(-307.88f, 134.05f), new(-308.47f, 134.26f), new(-308.94f, 134.67f), new(-309.17f, 135.25f),
    new(-309.73f, 135.62f), new(-317.88f, 137.71f), new(-318.42f, 137.39f), new(-320.34f, 137.91f), new(-321.01f, 137.92f),
    new(-321.51f, 137.99f), new(-321.98f, 138.39f), new(-322.24f, 138.94f), new(-325.3f, 139.75f), new(-325.97f, 139.63f),
    new(-326.49f, 139.28f), new(-327.05f, 139.04f), new(-328.19f, 139.63f), new(-328.5f, 140.14f), new(-328.48f, 140.77f),
    new(-328.27f, 141.4f), new(-327.84f, 141.85f), new(-327.24f, 142.1f), new(-326.84f, 142.68f), new(-324.99f, 147.14f),
    new(-324.92f, 147.81f), new(-325.29f, 148.33f), new(-325.31f, 148.99f), new(-325.26f, 149.51f), new(-324.22f, 150.37f),
    new(-323.67f, 150.43f), new(-323.06f, 150.82f), new(-321.31f, 152.83f), new(-321.81f, 153.07f), new(-323.56f, 152.55f),
    new(-328.98f, 154.46f), new(-329.36f, 154.79f), new(-329.68f, 155.38f), new(-330.33f, 155.67f), new(-330.81f, 155.49f),
    new(-331.38f, 155.2f), new(-333.96f, 155.31f), new(-334.51f, 155.47f), new(-338.22f, 160.69f), new(-338.8f, 161.23f),
    new(-340.62f, 158.86f), new(-340.93f, 158.27f), new(-345.43f, 147.33f), new(-345.05f, 146.82f), new(-344.58f, 146.43f),
    new(-344.33f, 145.9f), new(-344.4f, 145.32f), new(-344.63f, 144.87f), new(-345.21f, 144.61f), new(-345.81f, 144.57f),
    new(-346.5f, 144.83f), new(-347.22f, 143.1f), new(-347.38f, 142.42f), new(-346.9f, 141.96f), new(-346.82f, 141.31f),
    new(-346.83f, 140.69f), new(-347.58f, 139.77f), new(-348.24f, 139.64f), new(-348.55f, 137.13f), new(-348.29f, 136.69f),
    new(-347.81f, 136.3f), new(-347.72f, 135.71f), new(-348.04f, 135.3f), new(-348.72f, 135.25f), new(-348.87f, 134.76f),
    new(-350.49f, 122.45f), new(-348.25f, 105.08f), new(-347.77f, 104.87f), new(-347.3f, 104.5f), new(-346.89f, 103.95f),
    new(-346.74f, 103.41f), new(-346.9f, 102.73f), new(-347.21f, 102.22f), new(-347.26f, 101.57f), new(-346.48f, 99.71f),
    new(-346.23f, 99.26f), new(-345.69f, 99.26f), new(-345.17f, 98.91f), new(-345.11f, 98.32f), new(-345.59f, 97.94f),
    new(-345.54f, 97.43f), new(-340.89f, 86.21f), new(-340.54f, 85.61f), new(-332.45f, 75.13f), new(-331.79f, 74.98f),
    new(-331.23f, 75.19f), new(-330.62f, 75.11f), new(-330.1f, 74.72f), new(-329.9f, 74.18f), new(-329.97f, 73.56f),
    new(-330.29f, 73.06f), new(-330.75f, 72.85f), new(-330.41f, 72.41f), new(-330.05f, 72.04f), new(-329.56f, 72.24f),
    new(-328.99f, 72.06f), new(-328.42f, 71.82f), new(-328.09f, 71.33f), new(-327.84f, 70.72f), new(-327.9f, 70.09f),
    new(-327.47f, 69.51f), new(-325.8f, 68.32f), new(-325.35f, 68.54f), new(-324.76f, 68.56f), new(-324.26f, 68.11f),
    new(-324.42f, 67.56f), new(-324.13f, 66.97f), new(-314.35f, 59.47f), new(-313.74f, 59.12f), new(-299.91f, 53.4f)];
    private static readonly ArenaBoundsCustom arena1 = new([new PolygonCustom(vertices)]);
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.Iksalion, (uint)OID.FlawlessEmpuse, (uint)OID.Shabti, (uint)OID.FlawlessChimera];

    protected override bool CheckPull() => IsAnyActorInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.ActorsInBounds(this, Trash);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            hints.PotentialTargets[i].Priority = 0;
        }
    }
}
