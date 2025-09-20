namespace BossMod.Shadowbringers.Alliance.A24TheCompound2P;

sealed class MechanicalLaceration(BossModule module) : Components.RaidwideCast(module, (uint)AID.MechanicalLaceration);
sealed class MechanicalDissection(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MechanicalDissection, new AOEShapeRect(85f, 5.5f));
sealed class MechanicalDecapitation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MechanicalDecapitation, new AOEShapeDonut(8f, 43f));
sealed class MechanicalContusionAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MechanicalContusionAOE, 6f);
sealed class MechanicalContusionSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.MechanicalContusionSpread, 6f);
sealed class IncongruousSpin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IncongruousSpin, new AOEShapeRect(80f, 75f));

sealed class MechanicalLacerationPhaseChange(BossModule module) : Components.GenericKnockback(module)
{
    private Knockback[] _kb = [];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _kb;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Compound2P)
        {
            _kb = [new(Arena.Center.Quantized(), 10f, WorldState.FutureTime(3.8d), ignoreImmunes: true)];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.MechanicalLacerationPhaseChange)
        {
            _kb = [];
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb.Length != 0)
        {
            ref readonly var kb = ref _kb[0];

            // square intentionally slightly smaller to prevent sus knockback
            hints.AddForbiddenZone(new SDKnockbackInAABBSquareAwayFromOrigin(Arena.Center, kb.Origin, 10f, 28f), kb.Activation);
        }
    }
}
