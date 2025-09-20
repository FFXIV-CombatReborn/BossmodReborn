namespace BossMod.Dawntrail.Unreal.UnSeiryu;

sealed class CoursingRiverForceOfNature(BossModule module) : Components.GenericKnockback(module)
{
    private readonly List<Knockback> _kbs = new(2);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(_kbs);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.CoursingRiverKB:
                _kbs.Add(new(spell.LocXZ, 25f, Module.CastFinishAt(spell), null, spell.Rotation, Kind.DirForward, ignoreImmunes: true));
                break;
            case (uint)AID.ForceOfNatureKB:
                _kbs.Insert(0, new(spell.LocXZ, 10f, Module.CastFinishAt(spell)));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.CoursingRiverKB or (uint)AID.ForceOfNatureKB)
        {
            ++NumCasts;
            if (_kbs.Count != 0)
            {
                _kbs.RemoveAt(0);
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _kbs.Count;
        if (count != 0)
        {
            ref readonly var c = ref _kbs.Ref(0);
            var center = Arena.Center;
            var act = c.Activation;
            if (count == 2)
            {
                ref readonly var c1 = ref _kbs.Ref(1);
                var dir = 25f * c1.Direction.ToDirection();
                if (!IsImmune(slot, act))
                {
                    hints.AddForbiddenZone(new SDKnockbackInCircleFixedDirectionAndAwayFromOrigin(center, c.Origin, dir, 10f, 19f), act); // circle intentionally slightly smaller to prevent sus knockback
                }
                else
                {
                    hints.AddForbiddenZone(new SDKnockbackInCircleFixedDirection(center, dir, 19f), act);
                }
            }
            else
            {
                hints.AddForbiddenZone(new SDKnockbackInCircleFixedDirection(center, 25f * c.Direction.ToDirection(), 19f), act);
            }
        }
    }
}

sealed class ForceOfNatureAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ForceOfNature, 5f);
