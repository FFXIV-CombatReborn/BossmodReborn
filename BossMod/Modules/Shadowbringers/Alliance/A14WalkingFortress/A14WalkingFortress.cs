namespace BossMod.Shadowbringers.Alliance.A14WalkingFortress;

sealed class LaserSaturation(BossModule module) : Components.RaidwideCast(module, (uint)AID.LaserSaturation);
sealed class LaserTurret(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LaserTurret, new AOEShapeRect(90f, 4f));
sealed class BallisticImpact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BallisticImpactLocation, 6f);
sealed class BallisticImpactSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.BallisticImpactSpread, 6f);
sealed class LaserSuppression(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LaserSuppression, new AOEShapeCone(60f, 45f.Degrees()));
sealed class MarxImpact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MarxImpact, 22f);
sealed class Neutralization(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Neutralization);
sealed class BossInvincible(BossModule module) : Components.InvincibleStatus(module, (uint)SID.Invincibility);
sealed class GoliathTank(BossModule module) : Components.Adds(module, (uint)OID.GoliathTank);
sealed class SerialJointedServiceModel(BossModule module) : Components.Adds(module, (uint)OID.SerialJointedServiceModel);
sealed class ClangingBlow(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ClangingBlow);
sealed class CentrifugalSpin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CentrifugalSpin, new AOEShapeRect(30f, 4f));
sealed class SidestrikingSpin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SidestrikingSpin, new AOEShapeRect(12f, 15f));
sealed class ShrapnelImpact(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.ShrapnelImpact, 6f);
sealed class ConvenientSelfDestruction2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ConvenientSelfDestruction, 22f);

sealed class GoliathTankLaserTurret(BossModule module) : Components.GenericBaitAway(module, (uint)AID.LaserTurretTank)
{
    private static readonly AOEShapeRect rect = new(85f, 5f);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID != (uint)IconID.TurretLockon)
        {
            return;
        }

        var tanks = Module.Enemies((uint)OID.GoliathTank);
        var count = tanks.Count;
        Actor? closest = null;
        var bestDistSq = float.MaxValue;
        for (var i = 0; i < count; ++i)
        {
            var t = tanks[i];
            if (!t.IsDead && t.IsTargetable)
            {
                var distSq = (t.Position - actor.Position).LengthSq();
                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    closest = t;
                }
            }
        }
        if (closest != null)
        {
            CurrentBaits.Add(new(closest, actor, rect, WorldState.FutureTime(3.3d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            var count = CurrentBaits.Count;
            for (var i = count - 1; i >= 0; --i)
            {
                if (CurrentBaits[i].Source == caster)
                {
                    CurrentBaits.RemoveAt(i);
                }
            }
        }
    }

    public override void Update()
    {
        var count = CurrentBaits.Count;
        for (var i = count - 1; i >= 0; --i)
        {
            var source = CurrentBaits[i].Source;
            if (!source.IsTargetable || source.IsDead)
            {
                CurrentBaits.RemoveAt(i);
            }
        }
    }
}

sealed class ConvenientSelfDestruction(BossModule module) : Components.GenericLineOfSightAOE(module, (uint)AID.ConvenientSelfDestructionLOS, 85f)
{
    private const float BlockerRadius = 6f;
    private readonly List<Actor> _casters = [];
    private readonly List<(WPos, float)> _blockers = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _casters.Add(caster);
            Refresh();
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _casters.Remove(caster);
            Refresh();
        }
    }

    private void Refresh()
    {
        Safezones.Clear();
        _blockers.Clear();
        if (_casters.Count == 0)
        {
            Modify(null, _blockers);
            return;
        }

        var caster = _casters[0];
        var tanks = Module.Enemies((uint)OID.GoliathTank);
        var count = tanks.Count;
        for (var i = 0; i < count; ++i)
        {
            var t = tanks[i];
            if (t.CastInfo == null && !t.IsDead)
            {
                _blockers.Add((t.Position, BlockerRadius));
            }
        }
        var activation = Module.CastFinishAt(caster.CastInfo);
        Modify(caster.Position, _blockers, activation);
        AddSafezone(activation);
    }
}

sealed class TotalAnnihilationManeuver(BossModule module) : Components.RaidwideCast(module, (uint)AID.TotalAnnihilationManeuver);

sealed class DeployDefenses(BossModule module) : Components.GenericAOEs(module, (uint)AID.TotalAnnihilationManeuver, "Go to safe spot!")
{
    private static readonly AOEShapeDonut donut = new(6f, 80f);
    private Actor? _shield;
    private Actor? _caster;
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _caster = caster;
            UpdateAOE();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.DeployDefenses)
        {
            _shield = caster;
            UpdateAOE();
        }
        else if (id == WatchedAction)
        {
            ++NumCasts;
            _caster = null;
            _shield = null;
            _aoe = [];
        }
    }

    private void UpdateAOE()
    {
        if (_caster is Actor c && _shield is Actor s)
        {
            _aoe = [new(donut, s.Position, default, Module.CastFinishAt(c.CastInfo))];
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 700, NameID = 9153, SortOrder = 6)]
public sealed class A14WalkingFortress(WorldState ws, Actor primary) : BossModule(ws, primary, new(900f, 427f), new ArenaBoundsSquare(30f));
