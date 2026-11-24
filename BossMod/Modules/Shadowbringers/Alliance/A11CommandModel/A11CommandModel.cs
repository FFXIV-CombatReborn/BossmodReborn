namespace BossMod.Shadowbringers.Alliance.A11CommandModel;

sealed class ClangingBlow(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ClangingBlow);
sealed class ForcefulImpact(BossModule module) : Components.RaidwideCast(module, (uint)AID.ForcefulImpact);
sealed class EnergyBombardment(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EnergyBombardment2, 4f);
sealed class CentrifugalSpin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CentrifugalSpin1, new AOEShapeRect(30f, 4f));
sealed class SidestrikingSpin(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.SidestrikingSpin2, (uint)AID.SidestrikingSpin3], new AOEShapeRect(30f, 6f));
sealed class HighCaliberLaser(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HighCaliberLaser2, new AOEShapeRect(70f, 12f));
sealed class Shockwave(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Shockwave, 8f, stopAtWall: true);
sealed class ShockwaveRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.Shockwave);

sealed class EnergyRing(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(12f), new AOEShapeDonut(12f, 24f), new AOEShapeDonut(24f, 36f), new AOEShapeDonut(36f, 48f)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EnergyRing2)
        {
            AddSequence(caster.Position, Module.CastFinishAt(spell));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = spell.Action.ID switch
        {
            (uint)AID.EnergyRing2 => 0,
            (uint)AID.EnergyRing4 => 1,
            (uint)AID.EnergyRing6 => 2,
            (uint)AID.EnergyRing8 => 3,
            _ => -1
        };
        if (order >= 0)
        {
            AdvanceSequence(order, caster.Position, WorldState.FutureTime(2.0d));
        }
    }
}

sealed class EnergyAssault(BossModule module) : Components.GenericAOEs(module, (uint)AID.EnergyAssault2)
{
    private static readonly AOEShapeCone cone = new(30f, 45f.Degrees());
    private Actor? _caster;
    private Angle _rotation;
    private DateTime _activation;
    private int _remaining;
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EnergyAssault1)
        {
            _caster = caster;
            _rotation = spell.Rotation;
            _activation = Module.CastFinishAt(spell, 2.1d);
            _remaining = 5;
            _aoe = [new(cone, caster.Position, _rotation, _activation)];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EnergyAssault1 && _caster == caster)
        {
            _rotation = caster.Rotation;
            _aoe = [new(cone, caster.Position, _rotation, _activation)];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id == WatchedAction)
        {
            ++NumCasts;
            if (--_remaining <= 0 || _caster == null)
            {
                _caster = null;
                _aoe = [];
            }
            else
            {
                _activation = WorldState.FutureTime(1.07d);
                _aoe = [new(cone, _caster.Position, _caster.CastInfo?.Rotation ?? _rotation, _activation)];
            }
        }
        else if (id == (uint)AID.EnergyAssaultEnd)
        {
            _caster = null;
            _remaining = 0;
            _aoe = [];
        }
    }

    public override void Update()
    {
        if (_caster != null)
        {
            if (_caster.IsDeadOrDestroyed || !_caster.IsTargetable)
            {
                _caster = null;
                _remaining = 0;
                _aoe = [];
            }
            else
            {
                _aoe[0] = new(cone, _caster.Position, _caster.CastInfo?.Rotation ?? _rotation, _activation);
            }
        }
    }
}

sealed class EnergyBomb(BossModule module) : Components.Voidzone(module, 3.0f, GetOrbs, moveHintLength: 4f)
{
    private static Actor[] GetOrbs(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.SJSM2);
        var count = enemies.Count;
        if (count == 0)
        {
            return [];
        }

        var orbs = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var a = enemies[i];
            if (!a.IsDead && a.Renderflags == 0)
            {
                orbs[index++] = a;
            }
        }
        return index == 0 ? [] : orbs[..index];
    }
}

sealed class HighPoweredLaser(BossModule module) : Components.GenericAOEs(module, (uint)AID.HighPoweredLaser)
{
    private static readonly AOEShapeRect rect = new(70f, 2f);
    private readonly List<(Actor actor, DateTime activation)> _casters = [];
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (actor.OID == (uint)OID.SJSM1 && status.ID == (uint)SID.VisualState && status.Extra == 0x87)
        {
            RemoveCaster(actor);
            _casters.Add((actor, WorldState.FutureTime(6.6d)));
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (actor.OID == (uint)OID.SJSM1 && status.ID == (uint)SID.VisualState && status.Extra == 0x87)
        {
            RemoveCaster(actor);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            RemoveCaster(caster);
        }
    }

    public override void Update()
    {
        _aoes.Clear();
        for (var i = _casters.Count - 1; i >= 0; --i)
        {
            var c = _casters[i];
            if (c.actor.IsDeadOrDestroyed)
            {
                _casters.RemoveAt(i);
            }
            else
            {
                _aoes.Add(new(rect, c.actor.Position, c.actor.Rotation, c.activation));
            }
        }
    }

    private void RemoveCaster(Actor actor)
    {
        for (var i = _casters.Count - 1; i >= 0; --i)
        {
            if (_casters[i].actor == actor)
            {
                _casters.RemoveAt(i);
            }
        }
    }
}

sealed class AirToSurfaceEnergy(BossModule module) : Components.Voidzone(module, 5f, GetBombers, moveHintLength: 3.5f)
{
    private bool _active;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x17)
        {
            _active = state == 0x00020001u || state != 0x00080004u && _active;
        }
    }

    private static Actor[] GetBombers(BossModule module)
    {
        if (module.FindComponent<AirToSurfaceEnergy>() is not { _active: true })
        {
            return [];
        }

        var enemies = module.Enemies((uint)OID.SJSM1);
        var count = enemies.Count;
        if (count == 0)
        {
            return [];
        }

        var bombers = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var a = enemies[i];
            if (!a.IsDead && a.FindStatus((uint)SID.Airborne) != null)
            {
                bombers[index++] = a;
            }
        }
        return index == 0 ? [] : bombers[..index];
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 700, NameID = 9141, PrimaryActorOID = (uint)OID.Boss, SortOrder = 1)]
public sealed class A11CommandModel(WorldState ws, Actor primary) : BossModule(ws, primary, new(-500f, 0f), new ArenaBoundsSquare(24f));
