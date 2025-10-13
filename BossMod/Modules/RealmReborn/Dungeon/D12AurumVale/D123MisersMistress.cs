namespace BossMod.RealmReborn.Dungeon.D12AurumVale.D123MisersMistress;

public enum OID : uint
{
    Boss = 0x3AF, // R3.85
    MorbolFruit = 0x5BC, // R0.6-1.8
    Plume1 = 0x5CA, // R0.5
    Plume2 = 0x603, // R1.5
    MorbolFruit1 = 0x1E8EE7, // R0.5
    MorbolFruit2 = 0x1E8EDE, // R0.5
    MorbolFruit3 = 0x1E8EE6, // R0.5
    MorbolFruit4 = 0x1E8EDF, // R0.5
    MorbolFruit5 = 0x1E8EE4, // R0.5
    MorbolFruit6 = 0x1E8EE5, // R0.5
    MorbolFruit7 = 0x1E8EE0, // R0.5
    MorbolFruit8 = 0x1E8EE1, // R0.5
    MorbolFruit9 = 0x1E8EE3, // R0.5
    MorbolFruit10 = 0x1E8EE2, // R0.5
    MorbolSeedling = 0x5BB // R0.9
}

public enum AID : uint
{
    AutoAttack1 = 1350, // Boss->player, no cast, single-target
    AutoAttack2 = 1041, // MorbolSeedling->player, no cast, single-target

    VineProbe = 1037, // Boss->self, 1.0s cast, range 6+R width 8 rect
    BadBreath = 1036, // Boss->self, 2.5s cast, range 12+R 120-degree cone aoe
    BurrBurrow = 1038, // Boss->self, 3.0s cast, raidwide, apply burrs stack
    HookedBurrs = 1039, // Boss->player, 1.5s cast, single-target
    Sow = 1081, // Boss->player, 3.0s cast, single-target, spawns adds
    GoldBilePlume1 = 1042, // Plume1->self, no cast, range 2 circle, knockback 5, away from source
    GoldBilePlume2 = 1043, // Plume2->self, no cast, range 3+R width 2 rect
    Germinate = 1040 // MorbolFruit->self, 15.0s cast, single-target
}

public enum SID : uint
{
    Burrs = 303 // Boss->player, extra=0x1/0x2/0x3
}

sealed class VineProbeCleave(BossModule module) : Components.Cleave(module, (uint)AID.VineProbe, new AOEShapeRect(9.85f, 4f), activeWhileCasting: false);
sealed class VineProbe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VineProbe, new AOEShapeRect(9.85f, 4f));
sealed class BadBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BadBreath, new AOEShapeCone(16f, 60f.Degrees()));

sealed class GoldBilePlume(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(2f);
    private static readonly AOEShapeRect rect = new(4.5f, 1f);
    private readonly List<AOEInstance> _aoes = new(7);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        void AddAOE(AOEShape shape) => _aoes.Add(new(shape, actor.Position.Quantized(), actor.Rotation));
        if (actor.OID == (uint)OID.Plume1)
        {
            AddAOE(circle);
        }
        else if (actor.OID == (uint)OID.Plume2)
        {
            AddAOE(rect);
        }
    }
}

sealed class MorbolFruit(BossModule module) : BossComponent(module)
{
    private BitMask burrs;
    private readonly List<Actor> _fruits = module.Enemies([(uint)OID.MorbolFruit1, (uint)OID.MorbolFruit2, (uint)OID.MorbolFruit3, (uint)OID.MorbolFruit4,
    (uint)OID.MorbolFruit5, (uint)OID.MorbolFruit6, (uint)OID.MorbolFruit7, (uint)OID.MorbolFruit8, (uint)OID.MorbolFruit9, (uint)OID.MorbolFruit10]);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Burrs && status.Extra > 0x02)
        {
            burrs.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Burrs && status.Extra > 0x02)
        {
            burrs.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!burrs[slot])
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
        if (!burrs[slot])
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
        if (!burrs[pcSlot])
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

sealed class D123MisersMistressStates : StateMachineBuilder
{
    public D123MisersMistressStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MorbolFruit>()
            .ActivateOnEnter<VineProbeCleave>()
            .ActivateOnEnter<VineProbe>()
            .ActivateOnEnter<GoldBilePlume>()
            .ActivateOnEnter<BadBreath>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 5, NameID = 1532)]
public sealed class D123MisersMistress(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(-390.6f, -142.52f), new(-390.05f, -142.12f), new(-389.51f, -141.84f), new(-388.26f, -141.72f), new(-387.03f, -141.22f),
    new(-386.41f, -141.16f), new(-385.89f, -140.77f), new(-385.31f, -140.56f), new(-384.72f, -140.55f), new(-383.84f, -140.67f),
    new(-383.32f, -140.25f), new(-382.26f, -139.66f), new(-381.04f, -139.47f), new(-379.84f, -139.63f), new(-378.07f, -140.6f),
    new(-370.65f, -137.12f), new(-369.7f, -136.22f), new(-369.82f, -135.61f), new(-370.69f, -134.65f), new(-370.98f, -134.05f),
    new(-371.18f, -133.4f), new(-371.47f, -132.8f), new(-372.21f, -131.71f), new(-372.03f, -131.13f), new(-371.75f, -130.51f),
    new(-371.98f, -129.82f), new(-372.41f, -129.26f), new(-372.56f, -128.68f), new(-372.47f, -127.96f), new(-372.19f, -127.4f),
    new(-372.47f, -126.91f), new(-376.18f, -125.2f), new(-376.63f, -124.67f), new(-376.99f, -124.15f), new(-377.24f, -123.58f),
    new(-377.05f, -123f), new(-376.7f, -122.42f), new(-376.48f, -121.81f), new(-375.99f, -121.44f), new(-374.84f, -120.73f),
    new(-374.65f, -120.14f), new(-377.79f, -120.89f), new(-378.35f, -120.81f), new(-378.94f, -120.49f), new(-379.63f, -120.34f),
    new(-380.21f, -120.08f), new(-380.64f, -119.68f), new(-380.68f, -119.05f), new(-380.86f, -118.41f), new(-381.19f, -117.85f),
    new(-381.34f, -117.33f), new(-381.54f, -116.85f), new(-383.29f, -115.8f), new(-383.78f, -115.32f), new(-384.48f, -115.36f),
    new(-385.07f, -115.63f), new(-385.53f, -116.17f), new(-385.48f, -116.83f), new(-385.66f, -117.37f), new(-386.88f, -118.89f),
    new(-387.42f, -119.23f), new(-387.97f, -119.47f), new(-389.22f, -119.13f), new(-390.12f, -118.26f), new(-390.58f, -117.04f),
    new(-391.03f, -116.56f), new(-391.33f, -116.02f), new(-391.58f, -115.43f), new(-392.06f, -114.98f), new(-392.58f, -114.85f),
    new(-392.95f, -114.38f), new(-393.22f, -113.77f), new(-393.78f, -113.31f), new(-394.38f, -113.15f), new(-394.9f, -112.79f),
    new(-395.39f, -112.37f), new(-396.08f, -112.11f), new(-396.58f, -111.81f), new(-396.73f, -111.22f), new(-397f, -110.6f),
    new(-397.98f, -109.71f), new(-398.5f, -109.43f), new(-399.86f, -108.11f), new(-400.04f, -107.47f), new(-400.34f, -106.81f),
    new(-400.69f, -106.21f), new(-400.75f, -105.59f), new(-401.32f, -105.25f), new(-401.78f, -104.91f), new(-402.76f, -103.94f),
    new(-403.81f, -104.36f), new(-404.43f, -104.17f), new(-406.34f, -104.5f), new(-408.24f, -104.23f), new(-408.97f, -104.22f),
    new(-409.11f, -106.11f), new(-409.25f, -106.76f), new(-410.16f, -107.68f), new(-411.67f, -108.74f), new(-412.19f, -108.56f),
    new(-412.83f, -108.47f), new(-413.51f, -108.55f), new(-414.12f, -108.69f), new(-414.71f, -108.7f), new(-415.36f, -108.79f),
    new(-421.12f, -108.45f), new(-421.78f, -108.65f), new(-422.91f, -109.34f), new(-423.49f, -109.27f), new(-424.13f, -108.97f),
    new(-425.7f, -110.28f), new(-426.15f, -110.82f), new(-426.23f, -111.48f), new(-426.46f, -112.09f), new(-426.79f, -112.69f),
    new(-426.42f, -113.9f), new(-426.67f, -114.48f), new(-426.88f, -115.18f), new(-426.88f, -115.7f), new(-427.45f, -116.8f),
    new(-427.91f, -117.1f), new(-428.55f, -117.32f), new(-428.95f, -117.62f), new(-429.41f, -118.13f), new(-429.77f, -118.69f),
    new(-429.97f, -119.33f), new(-429.6f, -119.69f), new(-429.35f, -120.27f), new(-428.91f, -120.85f), new(-428f, -121.65f),
    new(-427.73f, -122.24f), new(-427.53f, -122.87f), new(-427.71f, -123.41f), new(-428.51f, -124.37f), new(-428.38f, -125.04f),
    new(-428.06f, -125.59f), new(-427.89f, -126.11f), new(-428.47f, -127.25f), new(-428.68f, -128.32f), new(-428.16f, -128.76f),
    new(-427.67f, -129.25f), new(-427.01f, -129.37f), new(-426.43f, -129.62f), new(-425.89f, -129.92f), new(-425.26f, -130.13f),
    new(-424.83f, -130.53f), new(-424.14f, -130.72f), new(-423.62f, -130.97f), new(-423.25f, -131.49f), new(-422.9f, -132.1f),
    new(-422.8f, -132.68f), new(-423.4f, -133.8f), new(-423.65f, -134.36f), new(-424.81f, -135.07f), new(-425.26f, -135.58f),
    new(-425.25f, -136.27f), new(-424.76f, -136.8f), new(-424.07f, -136.73f), new(-423.35f, -135.77f), new(-422.78f, -135.41f),
    new(-422.23f, -135.13f), new(-421.69f, -135.18f), new(-421.08f, -135.49f), new(-420.46f, -135.64f), new(-419.88f, -135.88f),
    new(-419.39f, -136.28f), new(-419.34f, -136.85f), new(-419.68f, -138.11f), new(-420.04f, -138.61f), new(-420.45f, -139.06f),
    new(-420.94f, -139.5f), new(-421.08f, -140.17f), new(-420.6f, -140.67f), new(-419.89f, -140.76f), new(-419.24f, -140.71f),
    new(-417.97f, -140.26f), new(-416.6f, -140.05f), new(-416.14f, -139.6f), new(-415.63f, -139.27f), new(-413.86f, -138.71f),
    new(-413.25f, -138.77f), new(-412.66f, -138.58f), new(-412.12f, -138.82f), new(-411.64f, -139.14f), new(-411.16f, -139.65f),
    new(-410.65f, -139.89f), new(-410.12f, -140.34f), new(-409.43f, -140.34f), new(-409.21f, -141.03f), new(-408.07f, -141.58f),
    new(-407.37f, -141.49f), new(-406.52f, -138.39f), new(-406.06f, -138.07f), new(-405.43f, -137.9f), new(-404.78f, -137.83f),
    new(-404.09f, -137.92f), new(-403.58f, -137.93f), new(-403.03f, -137.86f), new(-402.46f, -138.13f), new(-400.99f, -140.5f),
    new(-400.77f, -141.06f), new(-400.67f, -141.67f), new(-400.34f, -142.29f), new(-399.77f, -142.76f), new(-399.14f, -143.04f),
    new(-398.71f, -143.54f), new(-398.07f, -143.6f), new(-397.61f, -143.12f), new(-396.57f, -141.51f), new(-396.16f, -141.07f),
    new(-395.61f, -140.73f), new(-395.07f, -140.5f), new(-394.48f, -140.59f), new(-394.04f, -140.86f), new(-393.27f, -141.99f),
    new(-393.1f, -142.76f), new(-392.83f, -143.4f), new(-392.4f, -143.93f), new(-391.8f, -144.32f)];
    private static readonly WPos[] verticesPuddle1 = [new(-374f, -135f), new(-375.682f, -136.176f), new(-376.432f, -136.279f),
    new(-377.182f, -135.75f), new(-377.932f, -135.588f), new(-378.682f, -135.456f), new(-379.432f, -135.702f), new(-380.182f, -135.75f),
    new(-380.932f, -135f), new(-381f, -134f), new(-381.23f, -133f), new(-382f, -132f), new(-383f, -131.148f), new(-383.25f, -131f),
    new(-384f, -131f), new(-385f, -130.708f), new(-385.493f, -130.5f), new(-386f, -130f), new(-385.774f, -129f), new(-385f, -128f),
    new(-384f, -127.5f), new(-383.182f, -127.443f), new(-381.682f, -128f), new(-380.188f, -128.998f), new(-379.433f, -129.220f),
    new(-378.68f, -128.62f), new(-377.935f, -128.337f), new(-377.182f, -128.53f), new(-376.432f, -129f), new(-376f, -130f),
    new(-374f, -131f), new(-373f, -132f), new(-372.699f, -133f), new(-373f, -134f)];
    private static readonly WPos[] verticesPuddle2 = [new(-386.148f, -136.269f), new(-386.268f, -136.969f), new(-386.5f, -138f),
    new(-387.159f, -138.378f), new(-387.074f, -138.634f), new(-386.958f, -139.252f), new(-387.019f, -140f),new(-387f, -141.623f),
    new(-389f, -142.363f), new(-388.953f, -140.002f), new(-389.211f, -139.147f), new(-388.935f, -138.276f), new(-388.823f, -137.791f),
    new(-389.255f, -137.375f), new(-389.565f, -136.625f), new(-389.567f, -136.031f), new(-389.369f, -135.319f), new(-389.291f, -134.94f),
    new(-389.389f, -134.768f), new(-389.531f, -134.692f), new(-389.512f, -134.117f), new(-389.624f, -133.733f), new(-389.775f, -133.036f),
    new(-389.361f, -132.469f), new(-388.836f, -132.209f), new(-388.232f, -132.331f), new(-387.954f, -133.036f), new(-388.033f, -133.347f),
    new(-388f, -134f), new(-387.695f, -134.608f), new(-387.551f, -135.086f), new(-387.138f, -135.501f),new(-386.523f, -135.754f),
    new(-386.272f, -136.099f)];
    private static readonly WPos[] verticesPuddle3 = [new(-395.063f, -136f), new(-394.544f, -135.806f), new(-394.011f, -136f), new(-393.538f, -137.115f),
    new(-393.411f, -138f), new(-393.225f, -138.5f), new(-392.851f, -139f), new(-392.839f, -140f), new(-393.331f, -141.167f), new(-396.475f, -141.502f),
    new(-396.883f, -140.706f), new(-396.992f, -140f), new(-396.761f, -139.093f), new(-396.57f, -138f), new(-396.305f, -137f), new(-395.966f, -136.712f),
    new(-395.532f, -136.559f)];
    private static readonly WPos[] verticesPuddle4 = [new(-376f, -124.844f), new(-376.405f, -125.716f), new(-377f, -126f), new(-377.571f, -125.636f),
    new(-377.919f, -125.5f), new(-378.148f, -125f), new(-378.382f, -124.299f), new(-379f, -124f), new(-379.422f, -123.5f), new(-379.5f, -123f),
    new(-379.4f, -122.5f),new(-379.5f, -122f), new(-380f, -121.5f), new(-380.5f, -121.572f), new(-381f, -121.5f), new(-381.5f, -121.346f),
    new(-382f, -121f), new(-382.702f, -121f), new(-384f, -121.378f), new(-385f, -122f), new(-385.5f, -122.288f), new(-386f, -122.505f),
    new(-386.372f, -122.870f), new(-386.768f, -123.2f), new(-387f, -123.5f), new(-387.31f, -124f), new(-388f, -124.5f), new(-388.5f, -124.661f),
    new(-389f, -124.5f), new(-389.5f, -124f), new(-389.745f, -123.5f), new(-389.835f, -123f), new(-389.745f, -122.5f), new(-389.5f, -122f),
    new(-389.232f, -121.5f), new(-388.685f, -121f), new(-388.185f, -120.5f), new(-387.917f, -120.25f), new(-387.5f, -120f), new(-386.903f, -119.725f),
    new(-386.306f, -119.668f), new(-385.742f, -119.435f), new(-385f, -119.234f), new(-384f, -119.266f), new(-383f, -119.128f), new(-382f, -118.786f),
    new(-381.5f, -118.5f), new(-381f, -118f), new(-374.803f, -118.826f), new(-374.134f, -120f)];
    private static readonly WPos[] verticesPuddle5 = [new(-392f, -120f), new(-392.571f, -120.247f), new(-393.236f, -120.38f), new(-394f, -120.611f),
    new(-395f, -119.935f), new(-395.254f, -119.353f), new(-395.986f, -119f), new(-396.467f, -118f), new(-396.484f, -117f), new(-396f, -116.316f),
    new(-395.5f, -116.276f), new(-395f, -116.37f), new(-394.5f, -116.651f), new(-394f, -116.767f), new(-393.5f, -116.343f), new(-393f, -116.179f),
    new(-392.488f, -116.331f), new(-391.966f, -117f), new(-391.542f, -118f), new(-391.455f, -119f)];
    private static readonly WPos[] verticesPuddle6 = [new(-401.265f, -128f), new(-402.135f, -129.441f), new(-402.764f, -130f), new(-402.992f, -130.719f),
    new(-402.897f, -131.439f),new(-402.574f, -132f), new(-402.926f, -132.823f), new(-404f, -133.106f), new(-404.503f, -133.622f), new(-405.23f, -133.9f),
    new(-406.133f, -133.4f), new(-406.293f, -132.694f), new(-406.409f, -132f), new(-406.099f, -131.348f), new(-406.016f, -130.696f), new(-405.555f, -130f),
    new(-405.731f, -129.703f), new(-406f, -129.5f), new(-407f, -129.694f), new(-408f, -129.5f), new(-408.795f, -129f), new(-409f, -128.5f), new(-409f, -127.5f),
    new(-408.823f, -127f), new(-408.321f, -126.654f), new(-408f, -126.221f), new(-407f, -126f), new(-406f, -125.619f), new(-405f, -125.582f),
    new(-404f, -126f), new(-403f, -126f), new(-401.895f, -126.5f), new(-401.383f, -127.431f)];
    private static readonly WPos[] verticesPuddle7 = [new(-404.284f, -119.023f), new(-403.794f, -119.454f), new(-403.575f, -120.129f), new(-403.619f, -120.735f),
    new(-403.886f, -121.163f), new(-404.310f, -121.591f), new(-404.474f, -122f), new(-404.553f, -122.428f), new(-404.907f, -122.856f), new(-405.454f, -123.05f),
    new(-406, -122.853f), new(-406.587f, -122.881f), new(-407.294f, -123.334f), new(-408f, -123.506f), new(-409f, -123.33f), new(-410f, -122.997f),
    new(-410.459f, -122.943f), new(-410.988f, -122.464f), new(-411.115f, -122f), new(-411.115f, -121.364f), new(-410.875f, -120.964f), new(-410.45f, -120.785f),
    new(-409.9f, -120.449f), new(-409.246f, -120.213f), new(-408.593f, -120.159f), new(-408f, -120.19f), new(-407.318f, -119.826f), new(-407.075f, -119.175f),
    new(-406.678f, -118.641f), new(-406, -118.45f), new(-405.47f, -118.701f), new(-404.89f, -118.911f)];
    private static readonly WPos[] verticesPuddle8 = [new(-411.374f, -130.476f), new(-410.843f, -130.781f), new(-410.56f, -131.536f), new(-410.222f, -132.124f),
    new(-409.772f, -132.562f), new(-409.495f, -133f), new(-409.308f, -134f), new(-409.52f, -134.408f), new(-409.843f, -134.816f), new(-410.185f, -135.536f),
    new(-411f, -135.802f), new(-412f, -135.631f), new(-412.632f, -135.178f),new(-413.052f, -134.482f), new(-413.692f, -134f), new(-414.12f, -133.056f),
    new(-413.692f, -132.112f), new(-413.043f, -131.714f), new(-412.687f, -131.01f), new(-412f, -130.369f)];
    private static readonly WPos[] verticesPuddle9 = [new(-417.5f, -124f), new(-417.866f, -124.5f), new(-418f, -125f), new(-418.558f, -125.595f),
    new(-419.279f, -125.483f), new(-420f, -125f), new(-420.154f, -124.5f), new(-420f, -124f),new(-419.454f, -123.654f), new(-419f, -122.999f),
    new(-418.501f, -122.854f), new(-418f, -123.003f), new(-417.564f, -123.441f)];
    private static readonly WPos[] verticesPuddle10 = [new(-409.676f, -107f), new(-409.423f, -107.835f), new(-409.241f, -108.358f), new(-409.061f, -109f),
    new(-408.991f, -110f), new(-408.491f, -110.791f), new(-409.002f, -111.257f), new(-409.317f, -112f), new(-409.771f, -112.564f), new(-411f, -112.387f),
    new(-412f, -112f), new(-412.771f, -112.489f), new(-413.513f, -112.631f), new(-413.932f, -113.309f), new(-413.933f, -114f), new(-414.116f, -115f),
    new(-413.681f, -116f), new(-413.75f, -116.733f), new(-413.56f, -117.467f), new(-412.78f, -117.912f), new(-412.838f, -118.956f), new(-414f, -120f),
    new(-414.5f, -120.307f), new(-415f, -120.858f), new(-415.5f, -121f), new(-416.5f, -121f), new(-417f, -121.154f), new(-417.5f, -121.473f),
    new(-418f, -121.627f), new(-419f, -121.677f), new(-419.735f, -121f), new(-420f, -120f), new(-420.564f, -119.442f), new(-421.254f, -119.159f),
    new(-422f, -118.92f), new(-422.484f, -118f), new(-422.324f, -117f), new(-422.577f, -116.165f), new(-422.76f, -115.642f), new(-422.94f, -115f),
    new(-423.009f, -114f), new(-423.509f, -113.210f), new(-422.998f, -112.743f), new(-422.683f, -112f), new(-422.229f, -111.436f), new(-421f, -111.613f),
    new(-421f, -111.613f), new(-420f, -112f), new(-418.3f, -111.504f), new(-418.068f, -110.691f), new(-418.067f, -110f), new(-417.884f, -109f),
    new(-418.502f, -108f)];
    private static readonly WPos[] verticesPuddle11 = [new(-428.683f, -124f), new(-428f, -124f), new(-427f, -123.764f), new(-426.05f, -123f),
    new(-425.331f, -123f), new(-424.837f, -122f), new(-424.908f, -121.285f), new(-424.537f, -120.868f), new(-424f, -120.59f), new(-423.325f, -120.582f),
    new(-422.651f, -120.65f), new(-422f, -121.126f), new(-421.396f, -121.389f), new(-421f, -122f), new(-420.702f, -123f), new(-421f, -124f),
    new(-421.568f, -124.416f), new(-422f, -125f), new(-422.697f, -125.237f), new(-423.394f, -125.336f), new(-424f, -125.87f), new(-424.791f, -126f),
    new(-425.173f, -125.499f), new(-425.365f, -125f), new(-427f, -125f), new(-428.291f, -126f)];
    private static readonly WPos[] verticesPuddle12 = [new(-424f, -131.156f), new(-423.595f, -130.285f), new(-423f, -130f), new(-422.429f, -130.364f),
    new(-422.081f, -130.5f), new(-421.852f, -131f), new(-421.618f, -131.701f), new(-421f, -132f), new(-420.578f, -132.5f), new(-420.5f, -133f),
    new(-420.6f, -133.5f), new(-420.5f, -134f), new(-420f, -134.5f), new(-419.5f, -134.428f), new(-419f, -134.5f), new(-418.5f, -134.654f),
    new(-418f, -135f), new(-417.892f, -135.5f), new(-418f, -136f), new(-418.5f, -136.5f), new(-418.627f, -137f), new(-418.5f, -137.5f),
    new(-419f, -138f), new(-419.288f, -138.487f), new(-419.422f, -138.973f), new(-419.346f, -139.488f), new(-419.305f, -140.003f), new(-419.414f, -140.418f),
    new(-419.414f, -140.418f), new(-419.524f, -140.833f), new(-419.661f, -143.331f), new(-430f, -133f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)], [new PolygonCustom(verticesPuddle1),
    new PolygonCustom(verticesPuddle2), new PolygonCustom(verticesPuddle3), new PolygonCustom(verticesPuddle4), new PolygonCustom(verticesPuddle5),
    new PolygonCustom(verticesPuddle6), new PolygonCustom(verticesPuddle7), new PolygonCustom(verticesPuddle8), new PolygonCustom(verticesPuddle9),
    new PolygonCustom(verticesPuddle10), new PolygonCustom(verticesPuddle11), new PolygonCustom(verticesPuddle12)]);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.MorbolFruit or (uint)OID.MorbolSeedling => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.MorbolFruit));
        Arena.Actors(Enemies((uint)OID.MorbolSeedling));
    }
}
