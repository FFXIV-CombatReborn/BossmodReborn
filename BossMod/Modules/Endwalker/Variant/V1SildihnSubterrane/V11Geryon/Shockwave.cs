namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V11Geryon;

sealed class Shockwave(BossModule module) : Components.GenericKnockback(module)
{
    private readonly List<Knockback> _kbs = new(2);
    private static readonly AOEShapeRect _shape = new(40f, 40f);
    private readonly RunawaySludge _aoe = module.FindComponent<RunawaySludge>()!;
    private readonly WPos[] voidzones = new WPos[4];
    private readonly List<Actor> sludgeVZs = module.Enemies((uint)OID.SludgeVoidzone);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(_kbs);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Shockwave)
        {
            var act = Module.CastFinishAt(spell);
            // knockback rect always happens through center, so create two sources with origin at center looking orthogonally
            AddKnockback(-90f.Degrees());
            AddKnockback(90f.Degrees());

            for (var i = 0; i < 4; ++i)
            {
                voidzones[i] = sludgeVZs[i].Position;
            }
            void AddKnockback(Angle offset) => _kbs.Add(new(Arena.Center, 15f, act, _shape, offset, Kind.DirForward));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Shockwave)
        {
            _kbs.Clear();
            Array.Clear(voidzones);
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
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

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kbs.Count != 0)
        {
            // square intentionally slightly smaller to prevent sus knockback, voidzones are treated as squares because we dont want to get knocked through them (there might be space behind them otherwise)
            ref readonly var kb = ref _kbs.Ref(0);
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                hints.AddForbiddenZone(new SDKnockbackInAABBSquareLeftRightAlongXAxisPlusAOECircles(Arena.Center, 15f, 19f, voidzones, 10f, 4), act);
            }
        }
    }
}
