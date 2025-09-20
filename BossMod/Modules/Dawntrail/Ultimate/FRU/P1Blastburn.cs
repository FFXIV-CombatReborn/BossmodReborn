namespace BossMod.Dawntrail.Ultimate.FRU;

sealed class P1Blastburn(BossModule module) : Components.GenericKnockback(module)
{
    private Actor? _caster;
    private bool _aoeDone;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_caster != null)
        {
            var dir = _caster.CastInfo?.Rotation ?? _caster.Rotation;
            var kind = dir.ToDirection().OrthoL().Dot(actor.Position - _caster.Position) > 0 ? Kind.DirLeft : Kind.DirRight;
            return new Knockback[1] { new(_caster.Position, 15f, Module.CastFinishAt(_caster.CastInfo), null, dir, kind, ignoreImmunes: true) };
        }
        return [];
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        // don't show kb hints until aoe is done
        if (_aoeDone)
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_caster != null)
            hints.AddForbiddenZone(new SDInvertedRect(_caster.Position, _caster.CastInfo?.Rotation ?? _caster.Rotation, 40f, 40f, 2f + (_aoeDone ? 0 : 5)), Module.CastFinishAt(_caster.CastInfo));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.TurnOfHeavensBlastburn or (uint)AID.ExplosionBlastburn)
        {
            _caster = caster;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TurnOfHeavensBlastburn:
            case (uint)AID.ExplosionBlastburn:
                _caster = null;
                ++NumCasts;
                break;
            case (uint)AID.TurnOfHeavensBurntStrikeFire:
            case (uint)AID.ExplosionBurntStrikeFire:
                _aoeDone = true;
                break;
        }
    }
}
