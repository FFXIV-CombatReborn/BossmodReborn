namespace BossMod.Endwalker.Alliance.A34Eulogia;

sealed class ClimbingShot(BossModule module) : Components.GenericKnockback(module)
{
    private AsAboveSoBelow? _exaflare = module.FindComponent<AsAboveSoBelow>();
    private Knockback[] _kb = [];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _kb;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _exaflare!.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            if (aoes[i].Check(pos))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ClimbingShotNald or (uint)AID.ClimbingShotThal)
        {
            _kb = [new(spell.LocXZ, 20f, Module.CastFinishAt(spell, 0.2d))];
            _exaflare ??= Module.FindComponent<AsAboveSoBelow>();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ClimbingShotAOE1 or (uint)AID.ClimbingShotAOE2 or (uint)AID.ClimbingShotAOE3 or (uint)AID.ClimbingShotAOE4)
        {
            ++NumCasts;
            _kb = [];
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb.Length != 0)
        {
            ref readonly var kb = ref _kb[0];
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                var aoes = _exaflare!.ActiveAOEs(slot, actor);
                var len = aoes.Length;
                var origins = new WPos[len];
                for (var i = 0; i < len; ++i)
                {
                    origins[i] = aoes[i].Origin;
                }
                hints.AddForbiddenZone(new SDKnockbackInCircleAwayFromOriginPlusAOECircles(Arena.Center, kb.Origin, 20f, 29f, origins, 6f, len), act);
            }
        }
    }
}
