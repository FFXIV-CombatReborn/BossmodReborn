namespace BossMod.Global.CrucibleOfTheUnbroken.SecondBoard.WyvernPiece;

public enum OID : uint
{
    WyvernPiece = 0x4C58,
    WindSprite = 0x4C5B, // R1.600, x0 (spawn during fight)
    WhirlwindSmall = 0x4C59, // R2.000, x0 (spawn during fight), mixed types
    WhirlwindBig = 0x4C5A, // R3.000, x0 (spawn during fight)
    LiquidHellPuddle = 0x1EA66D, // R0.500, x0 (spawn during fight), EventObj type
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 49680, // WyvernPiece->player, no cast, single-target
    Teleport = 48173, // WyvernPiece->location, no cast, single-target
    TheStormsGrip = 48166, // WyvernPiece->self, 4.0s cast, range 60 circle
    Buffet = 48167, // WindSprite->self, 6.0s cast, range 40 width 10 rect
    Typhoon = 48168, // WyvernPiece->self, 3.0s cast, range 40 circle
    LiquidHellBoss = 48171, // WyvernPiece->self, 3.0s cast, single-target
    LiquidHell = 48172, // Helper->self, 6.0s cast, range 6 circle
    BlazingTrailBoss = 48174, // WyvernPiece->self, 7.0+1.0s cast, single-target
    BlazingTrail = 48175, // Helper->self, 8.0s cast, range 60 180.000-degree cone
    StormTrailBoss = 48176, // WyvernPiece->self, 5.0+1.0s cast, single-target
    StormTrailBoss1 = 48177, // WyvernPiece->self, 5.0+1.0s cast, single-target
    StormTrail = 48178, // Helper->self, 6.0s cast, range 25 60.000-degree cone
}

public enum SID : uint
{
    Burns = 3065, // none->player, extra=0x0
    Burns1 = 3066, // none->player, extra=0x0
}

sealed class TheStormsGrip(BossModule module) : Components.RaidwideCast(module, (uint)AID.TheStormsGrip);
sealed class LiquidHell(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LiquidHell, 6.0f);
sealed class BlazingTrail(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlazingTrail, new AOEShapeCone(60.0f, 90.0f.Degrees()));
sealed class StormTrail(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StormTrail, new AOEShapeCone(25.0f, 30.0f.Degrees()));

sealed class LiquidHellPuddle(BossModule module) : Components.Voidzone(module, 5.0f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.LiquidHellPuddle);
        var count = enemies.Count;
        if (count == 0)
        {
            return [];
        }

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

sealed class Typhoon(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Typhoon, 10.0f)
{
    private readonly LiquidHellPuddle? liquidHellPuddle = module.FindComponent<LiquidHellPuddle>();
    private readonly Buffet? buffet = module.FindComponent<Buffet>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            return;
        }

        var knockback = Casters[0];
        if (IsImmune(slot, knockback.Activation))
        {
            return;
        }

        List<WPos> circles = [];
        if (liquidHellPuddle != null)
        {
            foreach (var puddle in liquidHellPuddle.ActiveAOEs(slot, actor))
            {
                circles.Add(puddle.Origin);
            }
        }

        if (circles.Count != 0)
        {
            hints.AddForbiddenZone(new SDKnockbackInAABBRectAwayFromOriginPlusIntersectAOECircles(Arena.Center, knockback.Origin, Distance, 19.0f, 13.8f,
                [.. circles], 5.0f, circles.Count), knockback.Activation);
        }

        List<(WPos origin, WDir direction)> rects = [];
        if (buffet != null)
        {
            foreach (var aoe in buffet.ActiveAOEs(slot, actor))
            {
                rects.Add((aoe.Origin, aoe.Rotation.ToDirection()));
            }
        }

        if (rects.Count != 0)
        {
            hints.AddForbiddenZone(new SDKnockbackInAABBRectAwayFromOriginPlusAOERects(Arena.Center, knockback.Origin, Distance, 19.0f, 13.8f, [.. rects],
                40.0f, 5.0f, rects.Count), knockback.Activation);
        }
    }
}

sealed class WhirlwindSmall(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] aoes = [];
    private readonly List<Actor> puddles = [];
    private readonly AOEShapeCapsule shape = new(2.0f, 2.5f);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.WhirlwindSmall)
        {
            puddles.Add(actor);
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID is (uint)OID.WhirlwindSmall)
        {
            puddles.Remove(actor);
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return aoes;
    }

    public override void Update()
    {
        var count = puddles.Count;
        if (aoes.Length != count)
            aoes = new AOEInstance[count]; // only reallocate when the puddle count changes
        for (var i = 0; i < count; ++i)
        {
            var puddle = puddles[i];
            aoes[i] = new(shape, puddle.Position, puddle.Rotation, color: Colors.Danger);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = puddles.Count;
        if (count == 0)
        {
            return;
        }

        var forbiddenSoon = WorldState.FutureTime(1.1d);

        for (var i = 0; i < count; i++)
        {
            var puddle = puddles[i];
            var position = puddle.Position;
            var rotation = puddle.Rotation;
            hints.TemporaryObstacles.Add(new SDCapsule(position, rotation, shape.Length, shape.Radius));
            hints.AddForbiddenZone(new SDCapsule(position, rotation, shape.Length * 1.5f, shape.Radius), forbiddenSoon);
            hints.TemporaryObstacles.Add(new SDCircle(position.Quantized(), shape.Radius));
        }
    }
}

// Split from the other Whirlwind component so we can change how the Buffet aoe works for the pathfinder
sealed class WhirlwindBig(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] aoes = [];
    private readonly List<Actor> puddles = [];
    private readonly AOEShapeCapsule shape = new(3.0f, 3.5f);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.WhirlwindBig)
        {
            puddles.Add(actor);
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID is (uint)OID.WhirlwindBig)
        {
            puddles.Remove(actor);
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return aoes;
    }

    public override void Update()
    {
        var count = puddles.Count;
        if (aoes.Length != count)
            aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var puddle = puddles[i];
            aoes[i] = new(shape, puddle.Position, puddle.Rotation, color: Colors.Danger);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = puddles.Count;
        if (count == 0)
        {
            return;
        }

        var forbiddenSoon = WorldState.FutureTime(1.1d);
        var forbiddenFarFuture = WorldState.FutureTime(5.0d);

        for (var i = 0; i < count; i++)
        {
            var puddle = puddles[i];
            var position = puddle.Position;
            var rotation = puddle.Rotation;
            hints.AddForbiddenZone(new SDCapsule(position, rotation, shape.Length, shape.Radius), forbiddenSoon);
            hints.AddForbiddenZone(new SDCapsule(position, rotation, shape.Length * 1.5f, shape.Radius), forbiddenFarFuture);
            hints.TemporaryObstacles.Add(new SDCircle(position.Quantized(), shape.Radius));
        }
    }
}

sealed class Buffet(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Buffet, new AOEShapeRect(40.0f, 5.0f))
{
    private readonly WhirlwindBig? whirlwindBig = module.FindComponent<WhirlwindBig>();

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            return [];
        }

        var aoes = CollectionsMarshal.AsSpan(Casters);

        // if there are no big whirlwind then we do return the aoe with its standard activation aoe
        if (whirlwindBig == null || whirlwindBig.ActiveAOEs(slot, actor).Length == 0)
        {
            return aoes;
        }

        // if we have big whirlwinds then we set the timer for the buffet aoe to max so the pathfinder doesn't get confused dodging all the different aoes
        for (var i = 0; i < count; i++)
        {
            ref var aoe = ref aoes[i];
            aoe.Activation = DateTime.MaxValue;
        }

        return aoes;
    }
}

sealed class WyvernPieceStates : StateMachineBuilder
{
    public WyvernPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheStormsGrip>()
            .ActivateOnEnter<LiquidHell>()
            .ActivateOnEnter<WhirlwindSmall>()
            .ActivateOnEnter<WhirlwindBig>()
            .ActivateOnEnter<LiquidHellPuddle>()
            .ActivateOnEnter<StormTrail>()
            .ActivateOnEnter<Buffet>()
            .ActivateOnEnter<Typhoon>()
            .ActivateOnEnter<BlazingTrail>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.WyvernPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1089u, NameID = 14549u, SortOrder = 3)]
[PreferredKinship(BST.Kinship.Vile, "Ignore Typhoon's knockback")]
public sealed class WyvernPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(520f, 0f), new ArenaBoundsRect(20f, 14.8f));
