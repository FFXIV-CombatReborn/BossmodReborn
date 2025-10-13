namespace BossMod.Shadowbringers.Dungeon.D13Paglthan.D132MagitekFortress;

public enum OID : uint
{
    Boss = 0x32FE, // R1.0
    MagitekCore = 0x31AC, // R2.3
    TemperedImperial = 0x31AD, // R0.5
    TelotekPredator = 0x31AF, // R2.1
    MagitekMissile = 0x31B2, // R1.0
    TelotekSkyArmor = 0x31B0, // R2.0
    MarkIITelotekColossus = 0x31AE, // R3.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // TemperedImperial/TelotekPredator/TelotekSkyArmor/MarkIITelotekColossus->player, no cast, single-target

    MagitekClaw = 23706, // TelotekPredator->player, 4.0s cast, single-target, mini tank buster, can be ignored
    StableCannon = 23700, // Helper->self, no cast, range 60 width 10 rect
    TwoTonzeMagitekMissile = 23701, // Helper->location, 5.0s cast, range 12 circle
    GroundToGroundBallistic = 23703, // Helper->location, 5.0s cast, range 40 circle, knockback 10, away from source
    MissileActivation = 10758, // MagitekMissile->self, no cast, single-target
    ExplosiveForce = 23704, // MagitekMissile->player, no cast, single-target
    DefensiveReaction = 23710, // MagitekCore->self, 5.0s cast, range 60 circle
    Aethershot = 23708, // TelotekSkyArmor->location, 4.0s cast, range 6 circle
    Exhaust = 23705 // MarkIITelotekColossus->self, 4.0s cast, range 40 width 7 rect
}

sealed class TwoTonzeMagitekMissile(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TwoTonzeMagitekMissile, 12f);
sealed class Aethershot(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Aethershot, 6f);
sealed class DefensiveReaction(BossModule module) : Components.RaidwideCast(module, (uint)AID.DefensiveReaction);
sealed class Exhaust(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Exhaust, new AOEShapeRect(40f, 3.5f));

sealed class GroundToGroundBallistic(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.GroundToGroundBallistic, 10f)
{
    private static readonly Angle a180 = 180f.Degrees(), a18 = 18f.Degrees();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            var forbidden = new ShapeDistance[2];
            forbidden[0] = new SDInvertedCone(D132MagitekFortress.DefaultCenter, 20f, a180, a18);
            forbidden[1] = new SDInvertedCone(D132MagitekFortress.DefaultCenter, 20f, default, a18);
            hints.AddForbiddenZone(new SDIntersection(forbidden), Casters.Ref(0).Activation);
        }
    }
}

sealed class StableCannon(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(60f, 5f);
    private readonly List<AOEInstance> _aoes = new(2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00200010u && index is >= 0x08 and <= 0x0A)
        {
            _aoes.Add(new(rect, new WPos(-185f + 10f * (index - 0x08), 28.3f).Quantized(), Angle.AnglesCardinals[1], WorldState.FutureTime(12.1d)));
        }
        else if (index == 0x0D && state == 0x00020001u)
        {
            _aoes.Clear();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.StableCannon)
        {
            _aoes.Clear();
        }
    }
}

sealed class MagitekMissile(BossModule module) : Components.GenericAOEs(module)
{
    private const float Radius = 1f, Length = 10f;
    private static readonly AOEShapeCapsule capsule = new(Radius, Length);
    private readonly List<Actor> _missiles = new(15);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _missiles.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var m = _missiles[i];
            aoes[i] = new(capsule, m.Position, m.Rotation);
        }
        return aoes;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.MagitekMissile)
        {
            _missiles.Add(actor);
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.MagitekMissile)
        {
            _missiles.Remove(actor);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ExplosiveForce)
        {
            _missiles.Remove(caster);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _missiles.Count;
        if (count == 0)
        {
            return;
        }
        var forbiddenImminent = new ShapeDistance[count];
        var forbiddenFuture = new ShapeDistance[count];
        for (var i = 0; i < count; ++i)
        {
            var m = _missiles[i];
            forbiddenFuture[i] = new SDCapsule(m.Position, m.Rotation, Length, Radius);
            forbiddenImminent[i] = new SDCircle(m.Position, Radius);
        }
        hints.AddForbiddenZone(new SDUnion(forbiddenFuture), WorldState.FutureTime(1.1d));
        hints.AddForbiddenZone(new SDUnion(forbiddenImminent));
    }
}

sealed class CorePlatform(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(2, true);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Arena.Bounds.Radius == 14.5f)
        {
            return _aoe;
        }
        return [];
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x0D)
        {
            if (state == 0x00020001u)
            {
                _aoe = [new(circle, new(-175f, 30f), default, DateTime.MaxValue, Colors.SafeFromAOE)];
            }
            else if (state == 0x00080004u)
            {
                _aoe = [];
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);
        var r = Arena.Bounds.Radius;
        var inCoreBounds = pc.Position.InSquare(D132MagitekFortress.CoreCenter, 7f);
        if (r == 14.5f && inCoreBounds)
        {
            SetArena(new ArenaBoundsSquare(7f), D132MagitekFortress.CoreCenter);
        }
        else if (r == 7f && !inCoreBounds)
        {
            SetArena(new ArenaBoundsSquare(14.5f), D132MagitekFortress.DefaultCenter);
        }

        void SetArena(ArenaBounds bounds, WPos center)
        {
            Arena.Bounds = bounds;
            Arena.Center = center;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoe.Length != 0 && Arena.Bounds.Radius == 14.5f)
        {
            ref var a = ref _aoe[0];
            if (!a.Check(actor.Position))
            {
                hints.Add("Walk into the glowing circle!");
            }
        }
    }
}

sealed class D132MagitekFortressStates : StateMachineBuilder
{
    public D132MagitekFortressStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CorePlatform>()
            .ActivateOnEnter<StableCannon>()
            .ActivateOnEnter<Aethershot>()
            .ActivateOnEnter<DefensiveReaction>()
            .ActivateOnEnter<Exhaust>()
            .ActivateOnEnter<TwoTonzeMagitekMissile>()
            .ActivateOnEnter<GroundToGroundBallistic>()
            .ActivateOnEnter<MagitekMissile>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 777u, NameID = 10067u)]
public sealed class D132MagitekFortress(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultCenter, new ArenaBoundsSquare(14.5f))
{
    public static readonly WPos DefaultCenter = new(-175f, 43f), CoreCenter = new(-175f, 8.5f);
    private static readonly uint[] trash = [(uint)OID.TelotekPredator, (uint)OID.TemperedImperial, (uint)OID.TelotekSkyArmor, (uint)OID.MarkIITelotekColossus, (uint)OID.MagitekCore];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, trash);
    }

    protected override bool CheckPull()
    {
        var enemies = Enemies((uint)OID.TelotekPredator);
        var count = enemies.Count;
        for (var i = 0; i < count; ++i)
        {
            var enemy = enemies[i];
            if (enemy.InCombat)
            {
                return true;
            }
        }
        return false;
    }
}
