namespace BossMod.Endwalker.Unreal.Un3Sophia;

sealed class ArmsOfWisdom(BossModule module) : Components.GenericKnockback(module, (uint)AID.ArmsOfWisdom)
{
    private Actor? _caster;

    private readonly Knockback[] _oneKnockback = new Knockback[1];
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_caster?.CastInfo?.TargetID == actor.InstanceID)
        {
            _oneKnockback[0] = new(_caster.Position, 5f, Module.CastFinishAt(_caster.CastInfo));
            return _oneKnockback;
        }
        return [];
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (_caster != null && _caster.CastInfo?.TargetID == _caster.TargetID)
        {
            // tank swap hints
            if (_caster.TargetID == actor.InstanceID)
                hints.Add("Pass aggro to co-tank!");
            else if (actor.Role == Role.Tank)
                hints.Add("Taunt!");
        }
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
