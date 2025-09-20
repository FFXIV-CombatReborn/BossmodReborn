namespace BossMod.Endwalker.Alliance.A33Oschon;

sealed class P2WanderingVolleyDownhill(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WanderingVolleyDownhillAOE, 8f);

sealed class P2WanderingVolleyKnockback(BossModule module) : Components.GenericKnockback(module)
{
    private readonly P2WanderingVolleyDownhill? _downhill = module.FindComponent<P2WanderingVolleyDownhill>();
    private readonly List<Knockback> _sources = new(2);
    private static readonly AOEShapeCone _shape = new(30f, 90f.Degrees());

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(_sources);

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (_downhill == null)
            return false;
        var aoes = _downhill.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WanderingVolleyN or (uint)AID.WanderingVolleyS)
        {
            _sources.Clear();
            // happens through center, so create two sources with origin at center looking orthogonally
            _sources.Add(new(Arena.Center, 12f, Module.CastFinishAt(spell), _shape, spell.Rotation + 90f.Degrees(), Kind.DirForward));
            _sources.Add(new(Arena.Center, 12f, Module.CastFinishAt(spell), _shape, spell.Rotation - 90f.Degrees(), Kind.DirForward));
        }
    }
}

sealed class P2WanderingVolleyAOE(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeRect _shape = new(40f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WanderingVolleyN or (uint)AID.WanderingVolleyS)
        {
            _aoe = [new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WanderingVolleyN or (uint)AID.WanderingVolleyS)
        {
            _aoe = [];
            ++NumCasts;
        }
    }
}
