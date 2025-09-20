namespace BossMod.RealmReborn.Extreme.Ex1Ultima;

class MistralSongVulcanBurst(BossModule module) : Components.GenericAOEs(module, (uint)AID.MistralSong)
{
    public bool Active;
    private Actor? _garuda; // non-null while mechanic is active
    private DateTime _resolve;
    private bool _burstImminent;
    private static readonly AOEShapeCone _shape = new(23.4f, 75f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Active)
            return new AOEInstance[1] { new(_shape, _garuda!.Position, _garuda!.Rotation, _resolve) };
        return [];
    }

    public override void Update()
    {
        Active = _garuda != null && (_resolve - WorldState.CurrentTime).TotalSeconds <= 6d; // note that garuda continues rotating for next ~0.5s
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Active)
            return;

        // we have custom shape before burst - we try to make it so that post-knockback position is safe
        if (_burstImminent)
        {
            var p1 = Arena.Center + Arena.Bounds.Radius * (_garuda!.Rotation + _shape.HalfAngle).ToDirection();
            var p2 = Arena.Center + Arena.Bounds.Radius * (_garuda!.Rotation - _shape.HalfAngle).ToDirection();
            var a1 = Angle.FromDirection(p1 - Module.PrimaryActor.Position);
            var a2 = Angle.FromDirection(p2 - Module.PrimaryActor.Position);
            if (a2.Rad > a1.Rad)
                a2 -= 360f.Degrees();
            hints.AddForbiddenZone(new SDCone(Module.PrimaryActor.Position, 40f, (a1 + a2) / 2, (a1 - a2) / 2), _resolve);
        }
        else
        {
            hints.AddForbiddenZone(_shape.Distance(_garuda!.Position, _garuda.Rotation), _resolve);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        var adjPos = _burstImminent ? Arena.ClampToBounds(Components.GenericKnockback.AwayFromSource(pc.Position, Module.PrimaryActor, 30f)) : pc.Position;
        Components.GenericKnockback.DrawKnockback(pc, adjPos, Arena);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _garuda = caster;
            _resolve = Module.CastFinishAt(spell);
            _burstImminent = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            _garuda = null;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.VulcanBurst)
            _burstImminent = false;
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.UltimaGaruda && id == 0x0588)
        {
            _garuda = actor;
            _resolve = WorldState.FutureTime(6.6d);
            _burstImminent = true;
        }
    }
}
