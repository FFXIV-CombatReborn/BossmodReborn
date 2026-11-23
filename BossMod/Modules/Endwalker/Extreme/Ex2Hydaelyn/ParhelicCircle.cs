namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

sealed class ParhelicCircle(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(10)];
    private readonly AOEShapeCircle _circle = new(6);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        var activation = WorldState.FutureTime(7.6d);
        var c = Arena.Center;
        if (actor.OID is var oid && oid == (uint)OID.RefulgenceHexagon)
        {
            var dir = c + 17f * actor.Rotation.ToDirection();
            _aoes.Add(new(_circle, c.Quantized(), default, activation));
            for (var i = 1; i < 7; ++i)
                _aoes.Add(new(_circle, WPos.RotateAroundOrigin(i * 60f, c, dir.Quantized()), default, activation));
        }
        else if (oid == (uint)OID.RefulgenceTriangle)
        {
            var dir = c + 8f * actor.Rotation.ToDirection();
            for (var i = 1; i < 4; ++i)
                _aoes.Add(new(_circle, WPos.RotateAroundOrigin(-60f + i * 120f, c, dir.Quantized()), default, activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Incandescence)
        {
            ++NumCasts;
            _aoes.Clear();
        }
    }
}
