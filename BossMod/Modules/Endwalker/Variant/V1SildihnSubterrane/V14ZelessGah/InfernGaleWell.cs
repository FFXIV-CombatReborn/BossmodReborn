namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V14ZelessGah;

sealed class InfernGale(BossModule module) : Components.GenericKnockback(module)
{
    private Knockback[] _kb = [];
    private bool kbInit;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _kb;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.MechanicStatus && status.Extra == 0x1CA)
        {
            _kb = [new(actor.Position.Quantized(), 20f, WorldState.FutureTime(5.6d))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.InfernGale)
        {
            kbInit = false;
            _kb = [];
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (kbInit)
        {
            ref var kb = ref _kb[0];
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                // rect intentionally slightly smaller to prevent sus knockbacks
                hints.AddForbiddenZone(new SDKnockbackInAABBRectAwayFromOrigin(Arena.Center, kb.Origin, 20f, 14f, 19f), act);
            }
        }
    }
}

sealed class InfernWellPull(BossModule module) : Components.GenericKnockback(module)
{
    private Knockback[] _kb = [];
    private bool kbInit;

    private readonly InfernWellAOE _aoe = module.FindComponent<InfernWellAOE>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => kbInit ? _kb : [];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.MechanicStatus && status.Extra == 0x1CB)
        {
            _kb = [new(actor.Position.Quantized(), 15f, WorldState.FutureTime(7.6d), kind: Kind.TowardsOrigin, minDistance: 1f)]; // min distance: actor (0x233C helper) + player hitbox radius
            kbInit = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.InfernWell)
        {
            kbInit = false;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (kbInit)
        {
            ref var kb = ref _kb[0];
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                hints.AddForbiddenZone(new SDCircle(kb.Origin, 23f), act); // radius: 15 pull distance + 8 aoe radius
            }
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (_aoe.AOEInit)
        {
            ref var aoe = ref _aoe.AOE[0];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return false;
    }
}

sealed class InfernWellAOE(BossModule module) : Components.GenericAOEs(module)
{
    public AOEInstance[] AOE = [];
    public bool AOEInit;
    private static readonly AOEShapeCircle circle = new(8f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEInit ? AOE : [];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.MechanicStatus && status.Extra == 0x1CB)
        {
            AOE = [new(circle, actor.Position.Quantized(), default, WorldState.FutureTime(9.1d))];
            AOEInit = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.InfernWellAOE)
        {
            AOEInit = false;
        }
    }
}
