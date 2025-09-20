namespace BossMod.Endwalker.Alliance.A32Llymlaen;

sealed class DireStraits(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeRect _shape = new(40f, 40f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DireStraitsVisualFirst or (uint)AID.DireStraitsVisualSecond)
        {
            _aoes.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 4.8d)));
            if (_aoes.Count > 1)
            {
                var aoes = CollectionsMarshal.AsSpan(_aoes);
                ref var aoe1 = ref aoes[0];
                ref var aoe2 = ref aoes[1];
                if (aoe1.Activation > aoe2.Activation)
                {
                    (aoe1, aoe2) = (aoe2, aoe1);
                }
                aoe2.Origin += 3f * aoe2.Rotation.ToDirection();
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DireStraitsAOEFirst or (uint)AID.DireStraitsAOESecond)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
            {
                _aoes.RemoveAt(0);
            }
            if (_aoes.Count == 1)
            {
                ref var aoe = ref CollectionsMarshal.AsSpan(_aoes)[0];
                aoe.Origin -= 1.5f * aoe.Rotation.ToDirection();
            }
        }
    }
}

sealed class NavigatorsTridentAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NavigatorsTridentAOE, new AOEShapeRect(40f, 5f));

sealed class NavigatorsTridentKnockback(BossModule module) : Components.GenericKnockback(module)
{
    private SerpentsTide? _serpentsTide = module.FindComponent<SerpentsTide>();
    private readonly List<Knockback> _sources = new(2);

    private static readonly AOEShapeCone _shape = new(30f, 90f.Degrees());

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(_sources);

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        _serpentsTide ??= Module.FindComponent<SerpentsTide>();
        if (_serpentsTide != null)
        {
            var aoes = _serpentsTide.ActiveAOEs(slot, actor);
            var len = aoes.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var aoe = ref aoes[i];
                if (aoe.Check(pos))
                {
                    return true;
                }
            }
        }
        return !Arena.InBounds(pos);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.NavigatorsTridentAOE)
        {
            _sources.Add(new(spell.LocXZ, 20f, Module.CastFinishAt(spell), _shape, default, Kind.DirForward));
            _sources.Add(new(spell.LocXZ, 20f, Module.CastFinishAt(spell), _shape, 180f.Degrees(), Kind.DirForward));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.NavigatorsTridentAOE)
        {
            _sources.Clear();
            ++NumCasts;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_sources.Count != 0)
        {
            _serpentsTide ??= Module.FindComponent<SerpentsTide>();
            // rect intentionally slightly smaller to prevent sus knockback
            ref var kb = ref _sources.Ref(0);
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                if (_serpentsTide != null && _serpentsTide.AOEs.Count != 0)
                {
                    var aoes = CollectionsMarshal.AsSpan(_serpentsTide.AOEs);
                    var len = aoes.Length;
                    var rects = new (WPos origin, WDir rotation)[len];
                    for (var i = 0; i < len; ++i)
                    {
                        ref var aoe = ref aoes[i];
                        rects[i] = (aoe.Origin, aoe.Rotation.ToDirection());
                    }
                    hints.AddForbiddenZone(new SDKnockbackInAABBRectLeftRightAlongZAxisPlusAOERects(Arena.Center, 20f, 19f, 28f, rects, 80f, 10f, len), act);
                }
                else
                {
                    hints.AddForbiddenZone(new SDKnockbackInAABBRectLeftRightAlongZAxis(Arena.Center, 20f, 19f, 28f), act);
                }
            }
        }
    }
}
