namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Gauntlet;

sealed class NorthSouthwind(BossModule module) : Components.GenericKnockback(module)
{
    private Knockback[] _kb = [];
    private readonly Stormcall _aoe1 = module.FindComponent<Stormcall>()!;
    private PainStormFrigidPulsePainfulGust? _aoe2;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _kb;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WindVisual)
        {
            _kb = [new(spell.LocXZ, 40f, Module.CastFinishAt(spell, 0.1d), direction: spell.Rotation, kind: Kind.DirForward)];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NorthWind or (uint)AID.SouthWind)
        {
            _kb = [];
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb.Length != 0)
        {
            // square intentionally slightly smaller to prevent sus knockback
            ref readonly var kb = ref _kb[0];
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                var center = Arena.Center;
                var dir = 40f * kb.Direction.ToDirection();
                _aoe2 ??= Module.FindComponent<PainStormFrigidPulsePainfulGust>();
                var count = _aoe2!.AOEs.Count;
                if (count != 0)
                {
                    hints.AddForbiddenZone(new SDKnockbackInAABBSquareFixedDirectionPlusMixedAOEs(Arena.Center, dir, 22f, [.. _aoe2.AOEs], count), act);
                }
                else
                {
                    var aoes = CollectionsMarshal.AsSpan(_aoe1.AOEs);
                    if (aoes.Length != 0)
                    {
                        hints.AddForbiddenZone(new SDKnockbackInAABBSquareFixedDirectionPlusAOECircle(center, dir, 22f, aoes[0].Origin, 35f), act);
                    }
                    else
                    {
                        hints.AddForbiddenZone(new SDKnockbackInAABBSquareFixedDirection(center, dir, 22f), act);
                    }
                }
            }
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        _aoe2 ??= Module.FindComponent<PainStormFrigidPulsePainfulGust>();
        var aoes1 = CollectionsMarshal.AsSpan(_aoe2!.AOEs);
        var len = aoes1.Length;
        for (var i = 0; i < len; ++i)
        {
            if (aoes1[i].Check(pos))
            {
                return true;
            }
        }

        var aoes2 = _aoe1.AOEs;
        return aoes2.Count != 0 && aoes2.Ref(0).Check(pos) || !Arena.InBounds(pos);
    }
}
