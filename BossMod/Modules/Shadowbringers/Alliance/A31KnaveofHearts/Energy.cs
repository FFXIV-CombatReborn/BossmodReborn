namespace BossMod.Shadowbringers.Alliance.A31KnaveofHearts;

sealed class Energy(BossModule module) : Components.GenericAOEs(module)
{
    private const float Radius = 1f, Length = 3f;
    private static readonly AOEShapeCapsule capsule = new(Radius, Length);
    private readonly List<Actor> _energy = new(26);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _energy.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var h = _energy[i];
            aoes[i] = new(capsule, h.Position, h.Rotation);
        }
        return aoes;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Energy)
        {
            _energy.Add(actor);
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.Energy)
        {
            _energy.Remove(actor);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Burst)
        {
            _energy.Remove(caster);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _energy.Count;
        if (count == 0)
        {
            return;
        }
        var forbiddenImminent = new ShapeDistance[count];
        var forbiddenFuture = new ShapeDistance[count];
        for (var i = 0; i < count; ++i)
        {
            var h = _energy[i];
            forbiddenFuture[i] = new SDCapsule(h.Position, h.Rotation, Length, 1.5f);
            forbiddenImminent[i] = new SDCircle(h.Position, Radius);
        }
        hints.AddForbiddenZone(new SDUnion(forbiddenFuture), WorldState.FutureTime(1.1d));
        hints.AddForbiddenZone(new SDUnion(forbiddenImminent));
    }
}
