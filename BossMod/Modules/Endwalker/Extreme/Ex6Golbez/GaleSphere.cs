namespace BossMod.Endwalker.Extreme.Ex6Golbez;

sealed class GaleSphere(BossModule module) : Components.GenericAOEs(module)
{
    public enum Side { S, E, N, W } // direction = value * 90deg

    private readonly List<Side> _sides = [];
    private readonly List<Actor>[] _spheres = [[], [], [], []];

    private readonly AOEShapeRect _shape = new(30f, 2.5f);

    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (NumCasts < _spheres.Length)
        {
            var spheres = _spheres[NumCasts];
            var count = spheres.Count;
            var aoesLen = count;
            if (_aoesBuffer.Length < aoesLen)
                _aoesBuffer = new AOEInstance[aoesLen];
            var aoes = _aoesBuffer;
            for (var i = 0; i < count; ++i)
            {
                var s = spheres[i];
                aoes[i] = new(_shape, s.Position.Quantized(), s.CastInfo?.Rotation ?? s.Rotation, Module.CastFinishAt(s.CastInfo));
            }
            return aoes.AsSpan(0, aoesLen);
        }
        return [];
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (_sides.Count > NumCasts)
            hints.Add($"Order: {string.Join(" -> ", _sides.Skip(NumCasts))}");
    }

    // note: PATE 11D5 happens 1.5s before all casts start, but we don't really care
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var order = CastOrder(spell.Action);
        if (order >= 0)
            _spheres[order].Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var order = CastOrder(spell.Action);
        if (order >= 0)
            _spheres[order].Remove(caster);
        if (order >= NumCasts)
            NumCasts = order + 1;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.GaleSpherePrepareN:
                _sides.Add(Side.N);
                break;
            case (uint)AID.GaleSpherePrepareE:
                _sides.Add(Side.E);
                break;
            case (uint)AID.GaleSpherePrepareW:
                _sides.Add(Side.W);
                break;
            case (uint)AID.GaleSpherePrepareS:
                _sides.Add(Side.S);
                break;
        }
    }

    private static int CastOrder(ActionID aid) => aid.ID switch
    {
        (uint)AID.GaleSphereAOE1 => 0,
        (uint)AID.GaleSphereAOE2 => 1,
        (uint)AID.GaleSphereAOE3 => 2,
        (uint)AID.GaleSphereAOE4 => 3,
        _ => -1
    };
}
