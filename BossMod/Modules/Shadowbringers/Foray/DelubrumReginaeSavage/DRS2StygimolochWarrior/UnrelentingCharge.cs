namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS2StygimolochWarrior;

sealed class UnrelentingCharge(BossModule module) : Components.GenericKnockback(module)
{
    private Actor? _source;
    private DateTime _activation;

    private readonly Knockback[] _oneKnockback = new Knockback[1];
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_source != null)
        {
            _oneKnockback[0] = new(_source.Position, 10f, _activation);
            return _oneKnockback;
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.UnrelentingCharge)
        {
            _source = caster;
            _activation = Module.CastFinishAt(spell, 0.3d);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.UnrelentingChargeAOE)
        {
            ++NumCasts;
            _activation = WorldState.FutureTime(1.6d);
        }
    }
}
