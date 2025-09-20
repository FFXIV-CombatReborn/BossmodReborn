namespace BossMod.Dawntrail.Raid.M05NDancingGreen;

sealed class LetsDance(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(8);
    private static readonly AOEShapeRect rect = new(25f, 45f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var max = count > 2 ? 2 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.Frogtourage && modelState is 5 or 7)
        {
            var count = _aoes.Count;
            var act = count != 0 ? _aoes.Ref(0).Activation.AddSeconds(count * 2d) : WorldState.FutureTime(18.2d);
            _aoes.Add(new(rect, Arena.Center.Quantized(), modelState == 5 ? Angle.AnglesCardinals[3] : Angle.AnglesCardinals[0], act));
            if (_aoes.Count == 2)
            {
                ref var aoe2 = ref _aoes.Ref(1);
                aoe2.Origin += 5f * aoe2.Rotation.ToDirection();
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var count = _aoes.Count;
        if (count != 0 && spell.Action.ID == (uint)AID.LetsDance)
        {
            _aoes.RemoveAt(0);

            if (count > 1)
            {
                var aoes = CollectionsMarshal.AsSpan(_aoes);
                ref var aoe1 = ref aoes[0];
                aoe1.Origin -= 5f * aoe1.Rotation.ToDirection();
                if (count > 2)
                {
                    ref var aoe2 = ref aoes[1];
                    aoe2.Origin += 1.5f * aoe2.Rotation.ToDirection();
                }
            }
        }
    }
}
