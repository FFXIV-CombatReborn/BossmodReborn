namespace BossMod.Dawntrail.Alliance.A22UltimaOmega;

sealed class SurfaceMissile(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(12);
    private readonly AOEShapeRect rect = new(12f, 10f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var max = count > 4 ? 4 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.SurfaceMissile)
        {
            var delay = _aoes.Count switch
            {
                < 4 => 10.1d,
                < 8 => 9.2d,
                _ => 8.2d
            };
            var rot = actor.Rotation;
            _aoes.Add(new(rect, (actor.Position - 6f * rot.Round(1f).ToDirection()).Quantized(), rot, WorldState.FutureTime(delay)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.SurfaceMissile)
        {
            _aoes.RemoveAt(0);
        }
    }
}
