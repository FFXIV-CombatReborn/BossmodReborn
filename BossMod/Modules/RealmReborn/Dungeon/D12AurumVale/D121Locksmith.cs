namespace BossMod.RealmReborn.Dungeon.D12AurumVale.D121Locksmith;

public enum OID : uint
{
    Boss = 0x5BF, // R4.0
    MorbolFruit1 = 0x1E8ED9, // R0.5 --
    MorbolFruit2 = 0x1E8ED7, // R0.5 --
    MorbolFruit3 = 0x1E8ED8, // R0.5 --
    MorbolFruit4 = 0x1E878A, // R0.5 --
}

public enum SID : uint
{
    GoldLung = 302 // Boss->player, extra=0x1/0x2/0x3/0x4
}

public enum AID : uint
{
    AutoAttack = 1350, // Boss->player, no cast, single-target

    HundredLashings = 1031, // Boss->self, no cast, range 8+R 120-degree cone
    GoldRush = 1032, // Boss->self, no cast, raidwide
    GoldDust = 1033 // Boss->location, 3.5s cast, range 8 circle
}

sealed class HundredLashings(BossModule module) : Components.Cleave(module, (uint)AID.HundredLashings, new AOEShapeCone(12f, 60f.Degrees()));
sealed class GoldDust(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GoldDust, 8f);

sealed class MorbolFruit(BossModule module) : BossComponent(module)
{
    private BitMask goldlung;
    private readonly List<Actor> _fruits = module.Enemies([(uint)OID.MorbolFruit1, (uint)OID.MorbolFruit2, (uint)OID.MorbolFruit3, (uint)OID.MorbolFruit4]);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.GoldLung && status.Extra > 0x02)
        {
            goldlung.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.GoldLung && status.Extra > 0x02)
        {
            goldlung.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!goldlung[slot])
        {
            return;
        }

        Actor? closest = null;
        var minDistSq = float.MaxValue;

        var count = _fruits.Count;
        for (var i = 0; i < count; ++i)
        {
            var fruit = _fruits[i];
            if (fruit.IsTargetable)
            {
                hints.GoalZones.Add(AIHints.GoalSingleTarget(fruit, 2f, 5f));
                var distSq = (actor.Position - fruit.Position).LengthSq();
                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    closest = fruit;
                }
            }
        }
        hints.InteractWithTarget = closest;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!goldlung[slot])
        {
            return;
        }

        var count = _fruits.Count;
        for (var i = 0; i < count; ++i)
        {
            if (_fruits[i].IsTargetable)
            {
                hints.Add("Eat a fruit to cleanse debuff!");
                return;
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!goldlung[pcSlot])
        {
            return;
        }

        var count = _fruits.Count;
        for (var i = 0; i < count; ++i)
        {
            var fruit = _fruits[i];
            if (fruit.IsTargetable)
            {
                Arena.AddCircle(fruit.Position, 3f, Colors.Safe);
            }
        }
    }
}

sealed class D121LocksmithStates : StateMachineBuilder
{
    public D121LocksmithStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MorbolFruit>()
            .ActivateOnEnter<HundredLashings>()
            .ActivateOnEnter<GoldDust>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 5, NameID = 1534)]
public sealed class D121Locksmith(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(26.26f, -26.39f), new(27.55f, -26.38f), new(28.27f, -26.26f), new(38.31f, -26.38f), new(38.83f, -25.95f),
    new(39.62f, -24.91f), new(40.05f, -24.44f), new(41.3f, -24.3f), new(41.91f, -24.3f), new(43.22f, -24.45f),
    new(43.81f, -24.13f), new(44.35f, -23.91f), new(45.64f, -23.63f), new(46.27f, -23.35f), new(46.41f, -22.75f),
    new(46.71f, -22.15f), new(46.89f, -21.56f), new(47.66f, -20.54f), new(47.9f, -19.91f), new(48.2f, -19.39f),
    new(48.76f, -19.25f), new(49.1f, -18.72f), new(49.6f, -18.31f), new(50.2f, -17.93f), new(50.76f, -17.5f),
    new(51.19f, -16.98f), new(51.58f, -16.42f), new(52.04f, -16.1f), new(52.63f, -15.93f), new(53.21f, -15.66f),
    new(53.75f, -15.35f), new(54.31f, -15.25f), new(54.74f, -14.76f), new(55.08f, -14.24f), new(55.54f, -13.75f),
    new(56.01f, -13.42f), new(56.6f, -13.25f), new(57.1f, -12.75f), new(57.5f, -12.22f), new(58.13f, -11f),
    new(58.55f, -10.58f), new(59.07f, -10.26f), new(59.18f, -9.64f), new(57.84f, -5.86f), new(57.71f, -5.26f),
    new(57.99f, -2.69f), new(58.13f, -2.16f), new(58.94f, -1.21f), new(59.21f, 6.58f), new(58.89f, 6.97f),
    new(58.34f, 7.29f), new(56.92f, 9.24f), new(56.63f, 11.9f), new(55.45f, 12.34f), new(55.09f, 12.85f),
    new(54.47f, 14.17f), new(54.19f, 14.6f), new(53.53f, 14.91f), new(53.01f, 15.3f), new(52.39f, 15.67f),
    new(51.72f, 15.97f), new(50.82f, 16.87f), new(50.4f, 18.1f), new(49.95f, 18.6f), new(47.64f, 20.06f),
    new(47.26f, 21.47f), new(47.27f, 22.16f), new(46.84f, 22.47f), new(46.17f, 22.71f), new(45.58f, 23.02f),
    new(45.2f, 23.54f), new(45.02f, 24.17f), new(44.91f, 24.84f), new(43.99f, 25.67f), new(43.67f, 26.2f),
    new(43.5f, 26.77f), new(43.66f, 27.35f), new(44.86f, 28.8f), new(44.05f, 29.68f), new(43.92f, 30.17f),
    new(44.2f, 30.71f), new(44.4f, 31.32f), new(44.31f, 32.63f), new(43.84f, 33.17f), new(42.6f, 33.54f),
    new(42.03f, 33.85f), new(40.95f, 34.27f), new(40.38f, 33.85f), new(39.74f, 33.62f), new(39.11f, 33.61f),
    new(38.58f, 33.92f), new(37.16f, 35.53f), new(36.38f, 36.76f), new(35.96f, 37.33f), new(35.58f, 37.65f),
    new(35.16f, 37.93f), new(34.51f, 38.19f), new(33.84f, 38.39f), new(31.16f, 38.17f), new(28.52f, 37.45f),
    new(27.99f, 36.96f), new(27.2f, 35.84f), new(26.88f, 35.2f), new(26.4f, 33.98f), new(25.91f, 33.67f),
    new(25.25f, 33.65f), new(21.89f, 34.06f), new(21.67f, 34.56f), new(21.16f, 34.8f), new(20.47f, 34.64f),
    new(19.77f, 34.33f), new(19.98f, 33.88f), new(20.04f, 33.32f), new(19.82f, 32.75f), new(19.32f, 32.32f),
    new(19.41f, 31.6f), new(19.89f, 31.14f), new(20.6f, 30.01f), new(21.63f, 29.21f), new(21.42f, 28.61f),
    new(19.93f, 27.34f), new(18.85f, 26.68f), new(18.26f, 26.64f), new(17.6f, 26.39f), new(17.05f, 26.11f),
    new(15.13f, 26.04f), new(14.65f, 25.52f), new(14.24f, 24.94f), new(14.34f, 24.45f), new(15.14f, 23.38f),
    new(14.73f, 20.06f), new(14.2f, 19.84f), new(14.17f, 19.31f), new(14.86f, 16.15f), new(15.33f, 15.63f),
    new(16.45f, 14.87f), new(16.65f, 14.32f), new(16.45f, 13.79f), new(16.03f, 13.32f), new(15.43f, 13.19f),
    new(15.21f, 12.52f), new(15.3f, 9.89f), new(16.42f, 7.26f), new(16.47f, 6.65f), new(16.4f, 6f),
    new(16.46f, 5.3f), new(16.89f, 5.01f), new(17.54f, 4.77f), new(17.82f, 4.29f), new(18.28f, 3.78f),
    new(19.59f, 3.6f), new(19.92f, 2.99f), new(19.9f, 2.41f), new(19.5f, 1.92f), new(19.59f, 1.22f),
    new(20.88f, 1.09f), new(21.28f, 0.7f), new(21.32f, 0.11f), new(21.62f, -0.55f), new(22.31f, -1.59f),
    new(22.3f, -2.82f), new(22.35f, -3.47f), new(22.6f, -4.15f), new(22.73f, -4.84f), new(22.7f, -5.53f),
    new(22.55f, -6.22f), new(22.31f, -6.9f), new(21.36f, -8.65f), new(20.74f, -8.93f), new(21.24f, -9.17f),
    new(21.78f, -9.5f), new(22.18f, -10.85f), new(22f, -12.9f), new(21.89f, -13.52f), new(19.19f, -15.18f),
    new(18.58f, -15.29f), new(17.91f, -15.53f), new(17.75f, -16.73f), new(17.59f, -17.36f), new(17.76f, -17.84f),
    new(18.16f, -18.36f), new(18.16f, -18.99f), new(18.66f, -19.51f), new(19.05f, -20.04f), new(19.37f, -20.62f),
    new(19.53f, -21.2f), new(19.33f, -21.8f), new(17.78f, -22.87f), new(17.02f, -23.05f), new(17.25f, -23.65f),
    new(17.23f, -24.95f), new(17.37f, -25.45f), new(17.89f, -25.38f), new(18.49f, -25.47f), new(20.38f, -26.29f),
    new(26.26f, -26.39f)];
    private static readonly WPos[] verticesPuddle1 = [new(39.41f, 19f), new(39.886f, 18f), new(40.276f, 17.209f), new(39.282f, 16.743f),
    new(38.956f, 16f), new(38f, 15.436f), new(37f, 15.613f), new(36f, 16f), new(35.229f, 15.511f), new(34.487f, 15.369f), new(34.068f, 14.691f),
    new(34.066f, 14f), new(33.884f, 13f), new(34.319f, 12f), new(34.25f, 11.267f), new(34.44f, 10.534f), new(35.22f, 10.088f),
    new(35.162f, 9.044f), new(34f, 8f), new(33.5f, 7.693f), new(33f, 7.142f), new(32.5f, 7f), new(31.5f, 7f), new(31.5f, 7f),
    new(31f, 7.174f), new(30.577f, 7.654f), new(30f, 8f), new(29.094f, 8.78f), new(28f, 9f), new(27.303f, 8.86f),
    new(26.606f, 8.489f), new(26f, 8.427f), new(25.35f, 8.581f), new(24.7f, 8.977f), new(24.414f, 10f), new(24.657f, 10.911f),
    new(23.951f, 12f), new(23.292f, 12.378f), new(22.976f, 12.954f), new(22f, 13.321f), new(21.246f, 13.645f), new(20.769f, 14f),
    new(20.272f, 15f), new(20.094f, 16f), new(20.185f, 16.5f), new(20.643f, 17f), new(21.066f, 17.097f), new(21.458f, 17.359f),
    new(22.127f, 17.235f), new(23f, 16.58f), new(24f, 16.369f), new(25f, 15.602f), new(26f, 16.054f), new(26.951f, 16.082f),
    new(27.726f, 16.852f), new(28.104f, 18f), new(27.648f, 19f), new(27.506f, 20f), new(27.735f, 20.456f), new(27.81f, 20.912f),
    new(28.266f, 21.133f), new(28.722f, 21.111f), new(29.228f, 21.701f), new(29.897f, 22f), new(30.18f, 22.596f), new(29.992f, 23.298f),
    new(30f, 24f), new(30.354f, 24.888f), new(31f, 25.168f), new(31.5f, 25f), new(32.5f, 25f), new(33f, 25.153f), new(33.5f, 25.473f),
    new(34f, 25.627f), new(35f, 25.677f), new(35.735f, 25f), new(36f, 24f), new(36.564f, 23.442f), new(37.254f, 23.159f), new(38f, 22.92f),
    new(38.996f, 22f), new(38.783f, 21f), new(39.035f, 20.165f), new(39.501f, 19.642f)];
    private static readonly WPos[] verticesPuddle2 = [new(30.5f, -8f), new(31f, -7.205f), new(31.5f, -7f), new(32.5f, -7f), new(33f, -7.177f),
    new(33.346f, -7.679f), new(33.779f, -8f), new(34f, -9f), new(34.381f, -10f), new(34.418f, -11f), new(34f, -12f), new(34f, -13f),
    new(33.5f, -14f), new(34f, -15f), new(34.747f, -15.41f), new(35.178f, -16f), new(35.035f, -17f), new(35.423f, -17.569f),
    new(36f, -18f), new(36f, -19f), new(35.665f, -20f), new(34.891f, -20.5f), new(34.404f, -21.141f), new(34f, -22f), new(34.117f, -22.633f),
    new(34f, -23.265f), new(33.194f, -24f), new(33.101f, -24.479f), new(32.5f, -25f), new(31.5f, -25f), new(31f, -24.814f),
    new(30.79f, -24.5f), new(30.756f, -24f), new(31.041f, -23f), new(30.871f, -22f), new(30.204f, -21f), new(30f, -20f), new(30f, -19.417f),
    new(29.56f, -18.709f), new(29f, -18f), new(29f, -17.142f), new(29.337f, -16.57f), new(30f, -16f), new(29.924f, -15.368f), new(30.166f, -14.724f),
    new(30.278f, -14f), new(30f, -13.236f), new(29.28f, -13.008f), new(28.561f, -13.103f), new(28f, -13.426f), new(27.177f, -13.074f),
    new(26.895f, -12f), new(26.311f, -11.51f), new(26.1f, -10.77f), new(26.601f, -9.867f), new(27.341f, -9.787f), new(28f, -9.591f),
    new(28.652f, -9.901f), new(29.304f, -9.984f), new(30, -10.445f), new(30.297f, -10.269f), new(30.5f, -10f), new(30.306f, -9f)];
    private static readonly WPos[] verticesPuddle3 = [new(57f, -0.5f), new(57.153f, -1f), new(57.473f, -1.5f), new(57.627f, -2f),
    new(57.677f, -3f), new(57f, -3.735f), new(56f, -4f), new(55.442f, -4.564f), new(55.159f, -5.254f), new(54.92f, -6f), new(54f, -6.484f),
    new(53f, -6.324f), new(52.165f, -6.577f), new(51.642f, -6.76f), new(51f, -6.94f), new(50f, -7.009f), new(49.209f, -7.509f), new(48.743f, -7.282f),
    new(48f, -6.956f), new(47.436f, -6f), new(47.613f, -5f), new(48f, -4f), new(47.511f, -3.229f), new(47.369f, -2.488f), new(46.691f, -2.068f),
    new(46f, -2.067f), new(45f, -1.885f), new(44f, -2.319f), new(43.267f, -2.25f), new(42.534f, -2.44f), new(42.089f, -3.22f), new(41.044f, -3.163f),
    new(40f, -2f), new(39.693f, -1.5f), new(39.142f, -1f), new(39f, -0.5f), new(39f, 0.5f), new(39.177f, 1f), new(39.679f, 1.346f),
    new(40f, 1.779f), new(41f, 2f), new(42f, 2.381f), new(43f, 2.418f), new(44f, 2f), new(45f, 2f), new(46f, 1.5f), new(47f, 2f),
    new(47.41f, 2.747f), new(48f, 3.178f), new(49f, 3.035f), new(49.569f, 3.424f), new(50f, 4f), new(51f, 4f), new(52f, 3.665f), new(52.5f, 2.891f),
    new(53.141f, 2.404f), new(54f, 2f), new(54.633f, 2.117f), new(55.265f, 2f), new(56f, 1.317f), new(56.479f, 1.101f), new(57f, 0.5f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)], [new PolygonCustom(verticesPuddle1),
    new PolygonCustom(verticesPuddle2), new PolygonCustom(verticesPuddle3)]);
}
