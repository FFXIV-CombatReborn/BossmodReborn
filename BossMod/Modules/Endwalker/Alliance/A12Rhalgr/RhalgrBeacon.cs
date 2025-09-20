namespace BossMod.Endwalker.Alliance.A12Rhalgr;

sealed class RhalgrBeaconAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RhalgrsBeaconAOE, 10f);

sealed class RhalgrBeaconShock(BossModule module) : Components.GenericAOEs(module, (uint)AID.Shock)
{
    private readonly List<AOEInstance> _aoes = new(7);
    private static readonly AOEShapeCircle _shape = new(8);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (NumCasts != 7 && actor.OID == (uint)OID.LightningOrb)
        {
            _aoes.Add(new(_shape, actor.Position.Quantized(), default, WorldState.FutureTime(12.7d), actorID: actor.InstanceID));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var count = _aoes.Count;
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.LightningOrb)
        {
            NumCasts = 0;
        }
    }
}

sealed class RhalgrBeaconKnockback(BossModule module) : Components.GenericKnockback(module, (uint)AID.RhalgrsBeaconKnockback, stopAfterWall: true)
{
    private Knockback[] _kb = [];
    private static readonly SafeWall[] safewalls = [new(new(9.09f, 293.91f), new(3.31f, 297.2f)), new(new(-6.23f, 304.72f), new(-13.9f, 303.98f)),
    new(new(-22.35f, 306.16f), new(-31.3f, 304.94f)), new(new(-40.96f, 300.2f), new(-49.39f, 296.73f))];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _kb;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _kb = [new(spell.LocXZ, 50f, Module.CastFinishAt(spell), safeWalls: safewalls, ignoreImmunes: true)];
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb.Length == 0)
        {
            return;
        }
        ref readonly var kb = ref _kb[0];
        var shock = Module.Enemies((uint)OID.LightningOrb);
        var count = shock.Count;
        var z = kb.Origin.Z;
        var forbidden = DetermineForbiddenZones(kb.Origin, shock, count, z);

        hints.AddForbiddenZone(new SDIntersection(forbidden), DateTime.MaxValue);
    }

    private static ShapeDistance[] DetermineForbiddenZones(WPos sourcePos, List<Actor> shock, int count, float z)
    {
        var isZone268 = z == 268.5f;
        WPos zone1, zone2;

        if (isZone268)
        {
            zone1 = new(6.3f, 295.7f);
            zone2 = new(-10f, 304.2f);
        }
        else
        {
            zone1 = new(-27f, 305.7f);
            zone2 = new(-45f, 298.7f);
        }

        if (count == 0)
        {
            return [new SDInvertedRect(sourcePos, zone1, 0.5f), new SDInvertedRect(sourcePos, zone2, 0.5f)];
        }
        var inFirstZone = false;
        for (var i = 0; i < count; ++i)
        {
            if (shock[i].Position.InRect(sourcePos, zone1, 5f))
            {
                inFirstZone = true;
                break;
            }
        }
        return [new SDInvertedRect(sourcePos, inFirstZone ? zone2 : zone1, 0.5f)];
    }
}
