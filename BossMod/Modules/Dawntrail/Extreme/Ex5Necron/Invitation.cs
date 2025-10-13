namespace BossMod.Dawntrail.Extreme.Ex5Necron;

sealed class Invitation(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(3);
    private readonly AOEShapeRect rect = new(36f, 5f);
    public bool Show = true;
    public bool NextIsDanger;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (!Show || count == 0)
        {
            return [];
        }
        var max = count > 2 ? 2 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.LoomingSpecter1)
        {
            _aoes.Add(new(rect, source.Position.Quantized(), source.Rotation, WorldState.FutureTime(12.3d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Invitation)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
            {
                _aoes.RemoveAt(0);
            }
        }
    }

    public override void Update()
    {
        if (NextIsDanger && _aoes.Count != 0)
        {
            _aoes.Ref(0).Color = Colors.Danger;
            NextIsDanger = false;
        }
    }
}
