namespace BossMod.Endwalker.Unreal.Un4Zurvan;

sealed class P2DemonsClawKnockback(BossModule module) : Components.GenericKnockback(module, (uint)AID.DemonsClaw)
{
    private Actor? _caster;

    private readonly Knockback[] _oneKnockback = new Knockback[1];
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_caster?.CastInfo?.TargetID == actor.InstanceID)
            {
                _oneKnockback[0] = new(_caster.Position, 17f, Module.CastFinishAt(_caster.CastInfo), ignoreImmunes: true);
                return _oneKnockback;
            }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            _caster = caster;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            _caster = null;
    }
}

sealed class P2DemonsClawWaveCannon(BossModule module) : Components.GenericWildCharge(module, 5f, (uint)AID.WaveCannonShared)
{
    public Actor? Target;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is var id && id == WatchedAction)
        {
            Source = caster;
            foreach (var (slot, player) in Raid.WithSlot(false, true, true))
            {
                PlayerRoles[slot] = player == Target ? PlayerRole.Target : PlayerRole.Share;
            }
        }
        else if (id == (uint)AID.DemonsClaw)
        {
            Target = WorldState.Actors.Find(spell.TargetID);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            Source = null;
    }
}
