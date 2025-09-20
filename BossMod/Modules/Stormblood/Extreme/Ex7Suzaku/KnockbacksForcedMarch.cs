namespace BossMod.Stormblood.Extreme.Ex7Suzaku;

sealed class MesmerizingMelody(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.MesmerizingMelody, 11f, kind: Kind.TowardsOrigin, stopAfterWall: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var act = c.Activation;
            if (!IsImmune(slot, act))
                hints.AddForbiddenZone(new SDCircle(c.Origin, 14.5f), act);
        }
    }
}

sealed class RuthlessRefrain(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.RuthlessRefrain, 11f, stopAfterWall: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var act = c.Activation;
            if (!IsImmune(slot, act))
                hints.AddForbiddenZone(new SDInvertedCircle(c.Origin, 9f), act);
        }
    }
}

abstract class PayThePiper : Components.GenericForcedMarch
{
    private readonly float _offset;

    protected PayThePiper(BossModule module, float offset) : base(module, stopAfterWall: true)
    {
        _offset = offset;
        OverrideDirection = true;
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var target = WorldState.Actors.Find(tether.Target)!;
        if (target == Module.PrimaryActor || tether.ID != (uint)TetherID.PayThePiper)
            return;
        Angle? rot = source.OID switch
        {
            (uint)OID.NorthernPyre => 180f.Degrees(),
            (uint)OID.EasternPyre => 90f.Degrees(),
            (uint)OID.SouthernPyre => new Angle(),
            (uint)OID.WesternPyre => -90f.Degrees(),
            _ => default
        };
        if (rot is Angle direction)
            AddForcedMovement(target, direction, 4f, WorldState.FutureTime(10d));
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.PayingThePiper)
        {
            State.GetOrAdd(actor.InstanceID).PendingMoves.Clear();
            ActivateForcedMovement(actor, status.ExpireAt);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.PayingThePiper)
            DeactivateForcedMovement(actor);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var state = State.GetValueOrDefault(actor.InstanceID);
        if (state == null || state.PendingMoves.Count == 0)
            return;

        // adding 1 unit of safety margin to everything to make it look less suspect
        var move0 = state.PendingMoves[0];
        var dir = move0.dir.ToDirection();
        var forbidden = new ShapeDistance[2];
        forbidden[0] = new SDInvertedCircle(Ex7Suzaku.ArenaCenter - _offset * dir, 19f);
        forbidden[1] = new SDRect(Ex7Suzaku.ArenaCenter, -dir, 20f, default, 4.5f);
        hints.AddForbiddenZone(new SDUnion(forbidden), move0.activation);
    }
}

sealed class PayThePiperRegular(BossModule module) : PayThePiper(module, 25f);
sealed class PayThePiperHotspotCombo(BossModule module) : PayThePiper(module, 30f);
