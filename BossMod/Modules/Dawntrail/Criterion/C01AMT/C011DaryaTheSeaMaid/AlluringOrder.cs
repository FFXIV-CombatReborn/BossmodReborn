namespace BossMod.Dawntrail.Criterion.C01AMT.C011DaryaTheSeaMaid;

sealed class AlluringOrder(BossModule module) : Components.RaidwideCast(module, (uint)AID.AlluringOrder);

sealed class AlluringOrderForcedMarch(BossModule module) : Components.StatusDrivenForcedMarch(module, 3f, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace)
{
    public bool EnableHints = true;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (EnableHints)
        {
            base.AddHints(slot, actor, hints);
        }
    }
}

sealed class Tidalspout(BossModule module) : Components.UniformStackSpread(module, 6f, default, 2, 2)
{
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.TidalspoutTarget)
        {
            AddStack(actor, status.ExpireAt);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Tidalspout)
        {
            Stacks.Clear();
        }
    }
}

sealed class SwimmingInTheAir(BossModule module) : Components.GenericAOEs(module, (uint)AID.Hydrofall)
{
    private static readonly AOEShapeCircle _shape = new(12f);
    private readonly List<AOEInstance> _aoes = [];
    private DateTime _activation;
    private bool _risky;

    public bool Risky
    {
        get => _risky;
        set
        {
            _risky = value;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var len = aoes.Length;
            for (var i = 0; i < len; ++i)
            {
                aoes[i].Risky = value;
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SwimmingInTheAir)
        {
            _activation = WorldState.FutureTime(17.6d);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.BlueOrb && _activation != default)
        {
            _aoes.Add(new(_shape, actor.Position, default, _activation, risky: _risky));
        }
    }
}

sealed class SunkenTreasure(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _circle = new(18f);
    private static readonly AOEShapeDonut _donut = new(4f, 20f);
    private readonly List<AOEInstance> _aoes = [];
    private bool _risky;

    public bool Risky
    {
        get => _risky;
        set
        {
            _risky = value;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var len = aoes.Length;
            for (var i = 0; i < len; ++i)
            {
                aoes[i].Risky = value;
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var len = aoes.Length;
        if (len == 0)
        {
            return [];
        }
        var limit = aoes[0].Activation.AddSeconds(2d);
        var count = 1;
        while (count < len && aoes[count].Activation <= limit)
        {
            ++count;
        }
        return aoes[..count];
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == (uint)STATE.FirstState)
        {
            AOEShape? shape = actor.OID switch
            {
                (uint)OID.BlueSphere => _circle,
                (uint)OID.DonutSphere => _donut,
                _ => null
            };
            if (shape != null)
            {
                _aoes.Add(new(shape, actor.Position, default, WorldState.FutureTime(10.2d), risky: _risky));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SphereShatter or (uint)AID.SphereShatter1)
        {
            ++NumCasts;
            var pos = caster.Position;
            for (var i = _aoes.Count - 1; i >= 0; --i)
            {
                if (_aoes[i].Origin.AlmostEqual(pos, 1f))
                {
                    _aoes.RemoveAt(i);
                }
            }
        }
    }
}
