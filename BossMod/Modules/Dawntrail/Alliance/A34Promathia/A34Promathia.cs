namespace BossMod.Dawntrail.Alliance.A34Promathia;

sealed class EmptySalvation(BossModule module) : Components.RaidwideCast(module, (uint)AID.EmptySalvation);

sealed class Explosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion, 16f);

sealed class WheelofImpregnability(BossModule module) : Components.GenericAOEs(module, (uint)AID.WheelOfImpregnabilityFire)
{
    private static readonly AOEShapeCircle circle = new(13f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.WheelOfImpregnability)
        {
            _aoe = [new(circle, Module.PrimaryActor.Position, default, WorldState.FutureTime(9.9d))];
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_aoe.Length == 0 && spell.Action.ID == (uint)AID.WheelOfImpregnabilityCast)
        {
            _aoe = [new(circle, Module.PrimaryActor.Position, default, WorldState.FutureTime(10.5d))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            _aoe = [];
        }
    }
}

sealed class BastionOfTwilight(BossModule module) : Components.GenericAOEs(module, (uint)AID.BastionOfTwilightFire)
{
    private static readonly AOEShapeDonut donut = new(8f, 50f);
    private AOEInstance[] _aoe = [];
    private int _pendingExplosions;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe.Length != 0)
        {
            _aoe[0].Risky = _pendingExplosions == 0;
        }
        return _aoe;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.BastionOfTwilight)
        {
            _aoe = [new(donut, Module.PrimaryActor.Position, default, WorldState.FutureTime(9.9d))];
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.Explosion)
        {
            ++_pendingExplosions;
        }
        else if (_aoe.Length == 0 && id == (uint)AID.BastionOfTwilightCast)
        {
            _aoe = [new(donut, Module.PrimaryActor.Position, default, WorldState.FutureTime(10.5d))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Explosion && _pendingExplosions > 0)
        {
            --_pendingExplosions;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            _aoe = [];
        }
    }
}

sealed class PestilentPenance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PestilentPenance, new AOEShapeRect(50f, 25f));

sealed class Comet(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Comet, (uint)AID.Comet, 6f, 4d)
{
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Meteor1 or (uint)AID.Meteor2) // Hey guess what?  The tankbuster in meteor is actually just Comet again!
        {
            Spreads.Clear();
        }
    }
}

sealed class FalseGenesis(BossModule module) : BossComponent(module)
{
    private bool _casting = false;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!_casting)
        {
            return;
        }
        Arena.ZoneRect(new(-820f, -807f), default(Angle), 6.5f, 6.5f, 6.5f, Colors.SafeFromAOE);
        Arena.ZoneRect(new(-808.724f, -826.5f), 120.Degrees(), 6.5f, 6.5f, 6.5f, Colors.SafeFromAOE);
        Arena.ZoneRect(new(-831.258f, -826.5f), -120.Degrees(), 6.5f, 6.5f, 6.5f, Colors.SafeFromAOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_casting)
        {
            return;
        }
        var pos = actor.Position;
        if (!(pos.InRect(new(-820f, -807f), default(Angle), 6.5f, 6.5f, 6.5f) || pos.InRect(new(-808.724f, -826.5f), 120.Degrees(), 6.5f, 6.5f, 6.5f) || pos.InRect(new(-831.258f, -826.5f), -120.Degrees(), 6.5f, 6.5f, 6.5f)))
        {
            hints.Add("Move to platform spawn!");
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FalseGenesis)
        {
            _casting = true;
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FalseGenesis)
        {
            _casting = false;
        }
    }
}

sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FalseGenesis)
        {
            var bounds = new ArenaBoundsCustom([new Square(new(-820f, -807f), 6.5f), new Square(new(-808.724f, -826.5f), 6.5f, 120f.Degrees()),
                new Square(new(-831.258f, -826.5f), 6.5f, -120f.Degrees())]);
            Arena.Bounds = bounds;
            Arena.Center = bounds.Center;
        }
    }
    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x0C && state == 0x00080004u)
        {
            Arena.Bounds = new ArenaBoundsCircle(25f);
            Arena.Center = new(-820f, -820f);
        }
    }
}

sealed class MemoryReceptacle(BossModule module) : Components.Adds(module, (uint)OID.MemoryReceptacle);

sealed class EmptyBeleaguer(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EmptyBeleaguer, 6f);
sealed class AuroralDrape(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AuroralDrape, new AOEShapeRect(7f, 3.5f));
sealed class WindsOfPromyvion(BossModule module) : Components.GenericRotatingAOE(module)
{
    private readonly AOEShapeRect _rect = new(16f, 3f);
    private Actor? _caster;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.WindsLeft)
        {
            Sequences.Add(new(_rect, actor.Position, actor.Rotation, 30.Degrees(), WorldState.FutureTime(4.5d), 1.4d, 4));
            _caster = actor;
        }
        if (iconID == (uint)IconID.WindsRight)
        {
            Sequences.Add(new(_rect, actor.Position, actor.Rotation, -30.Degrees(), WorldState.FutureTime(4.5d), 1.4d, 4));
            _caster = actor;
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.WindsOfPromyvionCast or (uint)AID.WindsOfPromyvionSpam)
        {
            AdvanceSequence(0, WorldState.CurrentTime);
        }
    }
    public override void OnActorRenderflagsChange(Actor actor, int renderflags)
    {
        if (_caster != null)
        {
            if (actor.InstanceID == _caster.InstanceID && renderflags == 16384)
            {
                Sequences.Clear();
                Update();
            }
        }
    }
}
sealed class EmptySeed(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.EmptySeed, 10f, shape: new AOEShapeCircle(10f))
{
    private Angle PlatformOrientation(WPos p)
    {
        if (p.Z > Arena.Center.Z)
        {
            return default;
        }
        if (p.X < Arena.Center.X)
        {
            return -120.Degrees();
        }

        return 120.Degrees();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var firstcornerdir = PlatformOrientation(caster.Position) + 45.Degrees();
            var walls = new SafeWall[8];
            var dist = 3.5f;
            for (var i = 0; i < 4; ++i)
            {
                var z = i * 2;
                var angle = firstcornerdir + i * 90f.Degrees();
                var corner = caster.Position + angle.ToDirection() * 8.9f; // This isn't 5e!  Diagonals are Longer!
                var end1 = corner + (angle + 225f.Degrees()).ToDirection() * dist; // 180+45, turn around, then turn one way or the other.
                var end2 = corner + (angle - 225f.Degrees()).ToDirection() * dist;
                walls[z] = new(corner, end1);
                walls[z + 1] = new(corner, end2);
            }

            var minDist = KnockbackKind == Kind.TowardsOrigin ? (MinDistance + (MinDistanceBetweenHitboxes ? (Raid.Player()?.HitboxRadius ?? 0.5f) + caster.HitboxRadius : default)) : MinDistance;
            Casters.Add(new(spell.LocXZ, Distance, Module.CastFinishAt(spell), Shape, spell.Rotation, KnockbackKind, minDist, walls, caster.InstanceID, IgnoreImmunes));
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
            return;
        for (var i = 0; i < Casters.Count; ++i)
        {
            var source = Casters[i].Origin;
            if (actor.DistanceToPoint(source) < 9f)
            {
                var firstcornerdir = PlatformOrientation(source) + 45.Degrees();
                var angle = firstcornerdir;
                var corner = source + angle.ToDirection() * 9f;
                hints.AddForbiddenZone(new SDInvertedCone(corner, 5f, angle + 180.Degrees(), 22.5f.Degrees()));
            }
        }
    }
}

sealed class MalevolentBlessingCone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MalevolentBlessingCone, new AOEShapeCone(40f, 11.5f.Degrees()));

sealed class MalevolentBlessingRect(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MalevolentBlessingRect, new AOEShapeRect(50f, 25f));

sealed class PestilentPenanceLink(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PestilentPenanceLink, new AOEShapeRect(50f, 2.5f));

sealed class InfernalDeliveranceTower(BossModule module) : Components.CastTowers(module, (uint)AID.InfernalDeliveranceTower, 4f, 1, 8);

sealed class InfernalDeliveranceAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.InfernalDeliveranceAOE, 8f);

sealed class Meteor(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Meteor, (uint)AID.Meteor2, 6f, 5f);  // this is the regular spreads only, Tankbuster is handled in Comet.

sealed class DeadlyRebirth(BossModule module) : Components.RaidwideCast(module, (uint)AID.DeadlyRebirth);

sealed class DeadlyRebirthKB(BossModule module) : Components.GenericKnockback(module, (uint)AID.DeadlyRebirth)
{
    private Knockback[] _kb = [];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _kb;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DeadlyRebirthCast)
        {
            _kb = [new(Arena.Center, 20f, Module.CastFinishAt(spell, 2d), direction: default, kind: Kind.DirForward)];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            _kb = [];
        }
    }
}

sealed class FalseGenesisRaidwide(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.FalseGenesis, (uint)AID.FalseGenesis1, 0.6d);

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team, HerStolenLight", PrimaryActorOID = (uint)OID.Promathia, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1117u, NameID = 14779u)]

public sealed class A34Promathia(WorldState ws, Actor primary) : BossModule(ws, primary, new(-820f, -820f), new ArenaBoundsCircle(25f))
{
    public override bool ShouldPrioritizeAllEnemies => true;
}
