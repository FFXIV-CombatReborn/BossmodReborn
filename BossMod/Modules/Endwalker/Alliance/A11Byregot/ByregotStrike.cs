namespace BossMod.Endwalker.Alliance.A11Byregot;

class ByregotStrikeJump(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ByregotStrikeJump, (uint)AID.ByregotStrikeJumpCone], 8f);

class ByregotStrikeKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.ByregotStrikeKnockback, 18f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var act = c.Activation;
            if (!IsImmune(slot, act))
            {
                hints.AddForbiddenZone(new SDKnockbackInAABBSquareAwayFromOrigin(Arena.Center, c.Origin, 18f, 23f), act);
            }
        }
    }
}

class ByregotStrikeCone(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);

    private static readonly AOEShapeCone _shape = new(90f, 22.5f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ByregotStrikeKnockback && Module.PrimaryActor.FindStatus((uint)SID.Glow) != null)
        {
            var act = Module.CastFinishAt(spell);
            var pos = spell.LocXZ;
            var rot = spell.Rotation;
            var a90 = 90f.Degrees();
            for (var i = 0; i < 4; ++i)
            {
                _aoes.Add(new(_shape, pos, rot + i * a90, act));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ByregotStrikeCone)
        {
            _aoes.Clear();
            ++NumCasts;
        }
    }
}
