namespace BossMod.RealmReborn.Dungeon.D26Snowcloak.D260NorthernBateleur;

public enum OID : uint
{
    Boss = 0xD1E, // R0.8
    IceSprite = 0xD31, // R0.9
    Hrimthurs = 0xD1F, // R1.69
    SnowcloakGoobbue = 0xD1B, // R1.9
    WallController1 = 0x1E94F1,
    WindController1 = 0x1E96D4,
    WindController2 = 0x1E94F0,
    WindController3 = 0x1E94F2,
    Yeti = 0x3977 // R3.8
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // Hrimthurs/SnowcloakGoobbue->player, no cast, single-target

    Blizzard = 967, // IceSprite->player, 1.0s cast, single-target
    WingCutter = 331, // Boss->self, 2.5s cast, range 6+R 60-degree cone
    DoubleSmash = 3259, // Hrimthurs->self, 2.5s cast, range 6+R 120-degree cone
    SicklySneeze = 31264, // SnowcloakGoobbue->self, 2.5s cast, range 6+R 90-degree cone
    Peck = 965, // Boss->player, no cast, single-target
    Beatdown = 575 // SnowcloakGoobbue->player, no cast, single-target
}

sealed class WallRemoval(BossModule module) : BossComponent(module)
{
    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008u && actor.OID == (uint)OID.WallController1)
        {
            Arena.Bounds = D260NorthernBateleur.Arena2;
            Arena.Center = D260NorthernBateleur.Arena2.Center;
        }
    }
}

sealed class Frostbite(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(17f, 17f);
    private readonly List<AOEInstance> _aoes = new(3);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        (var origin, var angle) = actor.OID switch
        {
            (uint)OID.WindController1 => (new WPos(1.5f, -140.75f), -135f.Degrees()),
            (uint)OID.WindController2 => (new(-31.462f, -167.332f), -36f.Degrees()),
            (uint)OID.WindController3 => (new(-51.839f, -132.241f), -13f.Degrees()),
            _ => default
        };
        if (origin == default)
        {
            return;
        }
        if (state == 0x00040008u)
        {
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var count = _aoes.Count;
            var id = actor.InstanceID;

            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.ActorID == id)
                {
                    aoe.Color = default;
                    aoe.Risky = true;
                    return;
                }
            }
            _aoes.Add(new(rect, origin, angle, actorID: actor.InstanceID));
        }
        else if (state == 0x00010002u)
        {
            var count = _aoes.Count;
            var id = actor.InstanceID;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.ActorID == id)
                {
                    aoe.Activation = WorldState.FutureTime(5d);
                    aoe.Color = Colors.FutureVulnerable;
                    aoe.Risky = false;
                    return;
                }
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return;
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        for (var i = 0; i < count; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(actor.Position))
            {
                if (aoe.Risky)
                {
                    hints.Add("GTFO from wind area!");
                }
                else
                {
                    hints.Add("Leave unsafe area!" + $" {(aoe.Activation - WorldState.CurrentTime).TotalSeconds:F1}s until activation.");
                }
                return;
            }
        }
    }
}

sealed class WingCutter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WingCutter, new AOEShapeCone(6.9f, 30f.Degrees()));
sealed class DoubleSmash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DoubleSmash, new AOEShapeCone(7.69f, 60f.Degrees()));
sealed class SicklySneeze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SicklySneeze, new AOEShapeCone(7.9f, 45f.Degrees()));

sealed class D260NorthernBateleurStates : StateMachineBuilder
{
    public D260NorthernBateleurStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WallRemoval>()
            .ActivateOnEnter<Frostbite>()
            .ActivateOnEnter<WingCutter>()
            .ActivateOnEnter<DoubleSmash>()
            .ActivateOnEnter<SicklySneeze>()
            .Raw.Update = () => AllDeadOrDestroyedInBounds(D260NorthernBateleur.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 27, NameID = 3220, SortOrder = 2)]
public sealed class D260NorthernBateleur(WorldState ws, Actor primary) : BossModule(ws, primary, arena1.Center, arena1)
{
    private static readonly WPos[] vertices1 = [new(-31.24f, -185.2f), new(-30.26f, -184.84f), new(-29.79f, -184.64f), new(-29.32f, -184.42f), new(-28.88f, -184.17f),
    new(-28.44f, -183.9f), new(-24.36f, -180.74f), new(-21.94f, -179.18f), new(-21.43f, -179.14f), new(-19.93f, -179.49f),
    new(-19.43f, -179.27f), new(-19.01f, -178.96f), new(-18.65f, -178.58f), new(-18.34f, -178.17f), new(-18.07f, -177.74f),
    new(-14.64f, -174.04f), new(-14.25f, -173.69f), new(-10.62f, -172.81f), new(-10.26f, -172.24f), new(-10.37f, -167.91f),
    new(-9.29f, -166.02f), new(-11.34f, -161.52f), new(-11.59f, -161.04f), new(-11.35f, -160.53f), new(-10.42f, -159.99f),
    new(-9.94f, -159.8f), new(-9.35f, -159.92f), new(-8.82f, -160.11f), new(-6.48f, -161.71f), new(-5.99f, -161.8f),
    new(-4.37f, -161.7f), new(-3.8f, -161.52f), new(-3.66f, -161), new(-3.53f, -160.44f), new(-3, -160.15f),
    new(-2.55f, -159.92f), new(-2.11f, -159.61f), new(-1.77f, -159.23f), new(-1.34f, -158.92f), new(0.58f, -157.74f),
    new(1.07f, -157.34f), new(4.54f, -154.11f), new(4.61f, -153.6f), new(4.49f, -151.22f), new(4.54f, -150.68f),
    new(4.95f, -147.82f), new(5.22f, -147.36f), new(6.74f, -145.18f), new(6.99f, -144.61f), new(6.2f, -133.45f),
    new(5.7f, -133.39f), new(5.36f, -133.01f), new(5.03f, -132.61f), new(4.74f, -132.16f), new(4.98f, -131.66f),
    new(6.71f, -129.68f), new(7.27f, -129.59f), new(7.75f, -129.41f), new(7.92f, -128.93f), new(8.78f, -126.15f),
    new(8.87f, -125.57f), new(9.01f, -123.86f), new(9.32f, -123.4f), new(10.15f, -122.7f), new(10.6f, -122.28f),
    new(10.93f, -121.79f), new(11.78f, -120.42f), new(12.12f, -120), new(14.98f, -118.18f), new(15.46f, -117.91f),
    new(17.73f, -117.53f), new(18.29f, -117.61f), new(19.78f, -118.39f), new(20.24f, -118.58f), new(24.03f, -119.39f),
    new(24.5f, -119.58f), new(25.17f, -120.49f), new(26.08f, -121.01f), new(26.31f, -121.48f), new(26.32f, -122.02f),
    new(26.43f, -122.56f), new(26.9f, -122.81f), new(27.38f, -123.01f), new(29.37f, -123.71f), new(29.92f, -123.71f),
    new(31.51f, -123.57f), new(32.02f, -123.28f), new(32.42f, -122.87f), new(32.82f, -122.48f), new(33.4f, -122.5f),
    new(35.94f, -122.98f), new(36.39f, -123.34f), new(36.66f, -123.82f), new(36.74f, -123.26f), new(37.02f, -122.82f),
    new(37.17f, -122.3f), new(37.11f, -121.78f), new(36.94f, -121.3f), new(36.76f, -120.83f), new(35.78f, -118.67f),
    new(35.54f, -118.23f), new(35.24f, -117.81f), new(34.8f, -117.36f), new(34.42f, -116.9f), new(34.42f, -116.27f),
    new(34.05f, -115.88f), new(33.64f, -115.55f), new(32.36f, -114.66f), new(31.9f, -114.39f), new(31.42f, -114.15f),
    new(30, -113.6f), new(22.15f, -111.67f), new(20.56f, -111.12f), new(20.08f, -110.85f), new(19.67f, -110.54f),
    new(19.2f, -110.25f), new(18.6f, -110.09f), new(15.59f, -109.66f), new(15.09f, -109.53f), new(8.71f, -107.29f),
    new(8.2f, -107.14f), new(7.68f, -107.09f), new(6.66f, -107.07f), new(6.15f, -107.1f), new(5.65f, -107.25f),
    new(5.19f, -107.47f), new(4.79f, -107.77f), new(4.43f, -108.13f), new(4.02f, -108.6f), new(1.88f, -111.61f),
    new(-1.37f, -118.12f), new(-1.31f, -118.66f), new(-0.47f, -120.75f), new(-0.84f, -121.09f), new(-1.33f, -121.35f),
    new(-1.68f, -121.79f), new(-1.68f, -123.58f), new(-1.62f, -124.27f), new(-1.06f, -129.25f), new(-1.11f, -129.86f),
    new(-1.55f, -131.5f), new(-1.93f, -131.87f), new(-5.36f, -133.77f), new(-5.91f, -133.93f), new(-9.15f, -134.24f),
    new(-9.71f, -134.43f), new(-12.54f, -141), new(-12.92f, -141.36f), new(-15.16f, -141.51f), new(-15.7f, -141.72f),
    new(-16.35f, -142.6f), new(-16.64f, -143.03f), new(-18.33f, -145.34f), new(-18.56f, -145.9f), new(-18.5f, -147.42f),
    new(-17.26f, -151.75f), new(-16.97f, -152.24f), new(-15.93f, -153.68f), new(-15.67f, -154.11f), new(-15.81f, -154.7f),
    new(-16.7f, -157.22f), new(-16.93f, -157.74f), new(-17.41f, -157.9f), new(-17.94f, -157.99f), new(-18.53f, -157.83f),
    new(-20.59f, -157.14f), new(-21.16f, -157.03f), new(-22.32f, -157.19f), new(-22.87f, -157.23f), new(-25.07f, -156.84f),
    new(-25.65f, -156.69f), new(-28.55f, -154.19f), new(-29.15f, -154.09f), new(-31.56f, -154.39f), new(-32.04f, -154.19f),
    new(-32.93f, -152.88f), new(-35.41f, -151.44f), new(-35.76f, -150.89f), new(-45.14f, -155.08f), new(-45.65f, -155.38f),
    new(-47.11f, -155.95f), new(-47.66f, -155.65f), new(-48.58f, -156.14f), new(-48.93f, -156.52f), new(-49.4f, -157.43f),
    new(-49.64f, -158.01f), new(-49.18f, -164.11f), new(-49.46f, -164.58f), new(-50.6f, -166), new(-50.81f, -166.49f),
    new(-51.51f, -168.47f), new(-51.48f, -168.99f), new(-49.59f, -170.26f), new(-46.27f, -173.37f), new(-45.88f, -173.8f),
    new(-45.6f, -175.44f), new(-45.43f, -176.03f), new(-43.97f, -177.79f), new(-43.42f, -177.81f), new(-42.9f, -177.67f),
    new(-42.42f, -177.85f), new(-40.85f, -179.42f), new(-40.46f, -179.88f), new(-39.58f, -181.88f), new(-39.09f, -182.17f),
    new(-38.53f, -182.31f), new(-38.1f, -182.68f), new(-36.87f, -183.87f), new(-36.58f, -184.39f), new(-36.11f, -184.61f),
    new(-33.79f, -185.05f), new(-33.26f, -185.13f), new(-32.74f, -185.05f), new(-32.25f, -184.89f), new(-31.69f, -184.99f)];
    private static readonly WPos[] vertices2 = [new(-46.63f, -157.97f), new(-46.09f, -157.85f), new(-45.05f, -157.56f), new(-44.54f, -157.39f), new(-40.51f, -155.73f),
    new(-40.02f, -155.49f), new(-39.56f, -155.22f), new(-39.11f, -154.93f), new(-38.67f, -154.62f), new(-38.25f, -154.3f),
    new(-37.06f, -153.23f), new(-36.7f, -152.83f), new(-36.38f, -152.39f), new(-36.11f, -151.93f), new(-35.92f, -151.43f),
    new(-35.82f, -150.89f), new(-35.9f, -150.35f), new(-36.48f, -148.21f), new(-36.81f, -147.69f), new(-37.99f, -146.44f),
    new(-38.29f, -145.95f), new(-38.61f, -142.66f), new(-38.79f, -142.12f), new(-39.15f, -141.7f), new(-39.4f, -141.19f),
    new(-39.65f, -138.38f), new(-39.83f, -137.89f), new(-40.39f, -136.94f), new(-40.61f, -136.4f), new(-40.6f, -132.4f),
    new(-41, -131.95f), new(-43.41f, -130.56f), new(-43.85f, -130.22f), new(-47.29f, -126.38f), new(-48.57f, -124.68f),
    new(-49.96f, -123.58f), new(-50.36f, -123.25f), new(-50.42f, -122.67f), new(-50.33f, -119.95f), new(-50.33f, -119.4f),
    new(-50.61f, -117.12f), new(-50.54f, -116.57f), new(-49.99f, -113.81f), new(-49.86f, -113.29f), new(-49.33f, -112.98f),
    new(-48.31f, -112.52f), new(-47.72f, -112.4f), new(-46.59f, -112.39f), new(-46, -112.33f), new(-42.99f, -110.62f),
    new(-42.51f, -110.28f), new(-40.58f, -108.26f), new(-40.2f, -107.84f), new(-39.58f, -105.68f), new(-39.45f, -105.07f),
    new(-39.25f, -103.58f), new(-39.22f, -103.07f), new(-39.22f, -102.55f), new(-39.25f, -102.04f), new(-39.35f, -101.02f),
    new(-39.44f, -100.5f), new(-39.54f, -100), new(-39.99f, -98.51f), new(-40.18f, -98.03f), new(-40.39f, -97.56f),
    new(-40.63f, -97.09f), new(-40.89f, -96.64f), new(-41.17f, -96.2f), new(-41.47f, -95.77f), new(-41.82f, -95.36f),
    new(-42.21f, -95), new(-43.46f, -94.05f), new(-43.91f, -93.77f), new(-44.4f, -93.56f), new(-48.41f, -92.51f),
    new(-54.4f, -91.26f), new(-57.14f, -90.89f), new(-57.66f, -90.91f), new(-58.01f, -91.33f), new(-58.27f, -91.78f),
    new(-58.47f, -92.27f), new(-58.63f, -92.77f), new(-58.75f, -93.29f), new(-58.84f, -93.81f), new(-58.91f, -94.33f),
    new(-58.86f, -99.54f), new(-56.9f, -110.02f), new(-56.95f, -110.6f), new(-57.68f, -110.8f), new(-57.45f, -111.27f),
    new(-57.34f, -111.77f), new(-57.28f, -112.28f), new(-57.4f, -112.78f), new(-57.77f, -113.2f), new(-58.14f, -115.4f),
    new(-58.16f, -115.91f), new(-56.28f, -119.28f), new(-56.06f, -119.78f), new(-55.66f, -121.9f), new(-55.71f, -122.45f),
    new(-55.89f, -123.62f), new(-56.01f, -124.13f), new(-57.82f, -128.2f), new(-58.17f, -128.66f), new(-58.89f, -129.48f),
    new(-59.21f, -129.98f), new(-59.7f, -132.62f), new(-59.63f, -133.13f), new(-59.81f, -133.65f), new(-61.4f, -137.14f),
    new(-61.59f, -137.72f), new(-61.72f, -142.29f), new(-61.41f, -145.09f), new(-60.61f, -148.9f), new(-60.12f, -149.19f),
    new(-58.39f, -149.3f), new(-57.83f, -149.37f), new(-54.84f, -151.15f), new(-54.24f, -151.18f), new(-51.9f, -150.95f),
    new(-48.54f, -151.6f), new(-46.8f, -151.47f), new(-45.63f, -151.44f), new(-45.24f, -151.75f), new(-45.06f, -152.28f),
    new(-45.05f, -153.96f), new(-45.32f, -154.4f), new(-48.66f, -156.18f), new(-48.98f, -156.6f), new(-49.53f, -157.66f),
    new(-49.02f, -157.71f), new(-48.47f, -157.89f), new(-47.9f, -158.01f)];

    private static readonly ArenaBoundsCustom arena1 = new([new PolygonCustom(vertices1)]);
    public static readonly ArenaBoundsCustom Arena2 = new([new PolygonCustom(vertices1), new PolygonCustom(vertices2)]);
    public static readonly uint[] Trash = [(uint)OID.IceSprite, (uint)OID.Boss, (uint)OID.Hrimthurs, (uint)OID.SnowcloakGoobbue];

    protected override bool CheckPull() => IsAnyActorInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.ActorsInBounds(this, Trash);
    }
}
