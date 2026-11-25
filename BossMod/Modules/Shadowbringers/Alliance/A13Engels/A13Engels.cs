namespace BossMod.Shadowbringers.Alliance.A13Engels;

sealed class DemolishStructureArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect square = new(2.5f, 2.5f, 2.5f, invertForbiddenZone: true);
    private AOEInstance[] _aoe = [];
    private bool _transitionStarted;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DemolishStructure && Arena.Center == A13MarxEngels.StartingArenaCenter)
        {
            _aoe = [new(square, A13MarxEngels.TransitionSpot, default, Module.CastFinishAt(spell), Colors.SafeFromAOE)];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x0B && state == 0x00020001u && Arena.Center == A13MarxEngels.StartingArenaCenter)
        {
            _transitionStarted = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.DemolishStructure && Arena.Center == A13MarxEngels.StartingArenaCenter)
        {
            _transitionStarted = true;
        }
    }

    public override void Update()
    {
        if (_transitionStarted && (Raid.Player() is { Position.Z: >= 755f } || Module.PrimaryActor.IsTargetable && Module.PrimaryActor.Position.Z >= 755f))
        {
            Arena.Center = A13MarxEngels.SecondArenaCenter;
            Arena.Bounds = A13MarxEngels.StartingBounds;
            _aoe = [];
            _transitionStarted = false;
        }
    }
}

sealed class IncendiarySaturationBombingArenaChange(BossModule module) : BossComponent(module)
{
    private static readonly ArenaBoundsRect halfBounds = new(30f, 15f);
    private static readonly ArenaBoundsSquare crushBounds = new(15f);
    private WPos _oldCenter;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.IncendiarySaturationBombing:
                if (Arena.Bounds != halfBounds && Arena.Bounds != crushBounds)
                {
                    _oldCenter = Arena.Center;
                    Arena.Center += new WDir(default, -15f);
                    Arena.Bounds = halfBounds;
                }
                break;
            case (uint)AID.MarxCrush:
                if (Arena.Bounds == halfBounds)
                {
                    Arena.Bounds = crushBounds;
                }
                break;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x0C && state == 0x00080004u && (Arena.Bounds == halfBounds || Arena.Bounds == crushBounds))
        {
            Arena.Center = _oldCenter;
            Arena.Bounds = A13MarxEngels.StartingBounds;
        }
    }
}

sealed class MarxCrush(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MarxCrush, new AOEShapeRect(15f, 15f));
sealed class MarxThrust(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MarxThrust, new AOEShapeRect(30f, 10f));
sealed class IncendiarySaturationBombing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IncendiarySaturationBombing, new AOEShapeRect(30f, 30f));
sealed class CrushingWheel(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CrushingWheel, new AOEShapeRect(20f, 15f));

sealed class PrecisionGuidedMissile(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.PrecisionGuidedMissile, 6f);

sealed class IncendiaryBombing(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.IncendiaryBombing, (uint)AID.IncendiaryBombing, 8f, 9.1d)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.IncendiaryBombing)
        {
            Spreads.Clear();
        }
    }
}

sealed class IncendiaryBombingVoidzone(BossModule module) : Components.VoidzoneAtCastTarget(module, 8f, (uint)AID.IncendiaryBombing, GetVoidzones, 0.1d)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.IncendiaryBomb);
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
            {
                voidzones[index++] = z;
            }
        }
        return voidzones[..index];
    }
}

sealed class DiffuseLaser(BossModule module) : Components.RaidwideCast(module, (uint)AID.DiffuseLaser);
sealed class SurfaceMissile(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SurfaceMissile, 6f);

sealed class GuidedMissile(BossModule module) : Components.StandardChasingAOEs(module, 6f, (uint)AID.GuidedMissileFirst, (uint)AID.GuidedMissileRest, 5.5f, 1d, 4, true, (uint)IconID.GuidedMissile)
{
    private static readonly AOEShapeRect center = new(25f, 25f, 25f);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (TargetsMask[slot])
        {
            hints.AddForbiddenZone(center, Arena.Center, default, Activation);
        }
        else
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}

sealed class LaserSight(BossModule module) : Components.GenericAOEs(module, (uint)AID.LaserSight)
{
    private static readonly AOEShapeRect rect = new(100f, 10f);
    private readonly List<Actor> _casters = [];
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void Update()
    {
        _aoes.Clear();
        var count = _casters.Count;
        for (var i = 0; i < count; ++i)
        {
            var c = _casters[i];
            _aoes.Add(new(rect, c.Position, c.Rotation, Module.CastFinishAt(c.CastInfo)));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _casters.Add(caster);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is var id && (id == WatchedAction || id == (uint)AID.LaserSightRepeat))
        {
            if (++NumCasts >= 5)
            {
                _casters.Clear();
                NumCasts = 0;
            }
        }
    }
}

sealed class EnergyDispersal(BossModule module) : Components.GenericTowers(module, (uint)AID.EnergyDispersal)
{
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.EnergyDispersalTower)
        {
            Towers.Add(new(actor.Position, 4f, maxSoakers: int.MaxValue, activation: WorldState.FutureTime(12.1d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id == WatchedAction)
        {
            ++NumCasts;
            var count = Towers.Count;
            var pos = caster.Position;
            for (var i = count - 1; i >= 0; --i)
            {
                if (Towers[i].Position.AlmostEqual(pos, 1f))
                {
                    Towers.RemoveAt(i);
                }
            }
        }
        else if (id == (uint)AID.EnergyBlast)
        {
            Towers.Clear();
        }
    }
}

sealed class Adds(BossModule module) : Components.AddsMulti(module, [(uint)OID.SmallBiped, (uint)OID.ReverseJointedGoliath]);
sealed class ArmLaser(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArmLaser, new AOEShapeCone(30f, 45f.Degrees()));
sealed class ReverseJointedGoliathCleave(BossModule module) : Components.Cleave(module, (uint)AID.ArmLaser, new AOEShapeCone(30f, 45f.Degrees()), [(uint)OID.ReverseJointedGoliath], activeWhileCasting: false);
sealed class AddsArms(BossModule module) : Components.AddsMulti(module, [(uint)OID.MarxR, (uint)OID.MarxL], 2);

sealed class RadiateHeat(BossModule module) : Components.RaidwideInstant(module, (uint)AID.RadiateHeat, 7d)
{
    private bool _armsActive;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.MarxCrush)
        {
            Activation = WorldState.FutureTime(2d);
        }
        else if (id == WatchedAction)
        {
            ++NumCasts;
            Activation = WorldState.FutureTime(Delay);
        }
    }

    public override void Update()
    {
        if (Activation == default)
        {
            return;
        }
        var armTargetable = AnyArmTargetable((uint)OID.MarxL) || AnyArmTargetable((uint)OID.MarxR);
        if (armTargetable)
        {
            _armsActive = true;
        }
        else if (_armsActive)
        {
            _armsActive = false;
            Activation = default;
        }
    }

    private bool AnyArmTargetable(uint oid)
    {
        var arms = Module.Enemies(oid);
        var count = arms.Count;
        for (var i = 0; i < count; ++i)
        {
            var a = arms[i];
            if (a.IsTargetable && !a.IsDead)
            {
                return true;
            }
        }
        return false;
    }
}

sealed class WideAngleDiffuseLaser(BossModule module) : Components.RaidwideInstant(module, (uint)AID.WideAngleDiffuseLaser, 2.0d)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.WideAngleDiffuseLaserVisual)
        {
            Activation = WorldState.FutureTime(Delay);
        }
        else
        {
            base.OnEventCast(caster, spell);
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 700, NameID = 9147, SortOrder = 5)]
public sealed class A13MarxEngels(WorldState ws, Actor primary) : BossModule(ws, primary, StartingArenaCenter, StartingBounds)
{
    public static readonly WPos TransitionSpot = new(900f, 696.5f);
    public static readonly WPos StartingArenaCenter = new(900f, 670f);
    public static readonly WPos SecondArenaCenter = new(900f, 785f);
    public static readonly ArenaBoundsSquare StartingBounds = new(30f);
}
