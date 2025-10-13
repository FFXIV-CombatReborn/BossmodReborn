namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V15ThorneKnight;

sealed class SpringToLife(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(6);
    private static readonly AOEShapeRect rect = new(45f, 3f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001u)
        {
            (WPos[], Angle) pos = index switch
            {
                0x00 => ([new(263f, -235f), new(280f, -252f), new(271.5f, -243.5f)], Angle.AnglesIntercardinals[1]),
                0x01 => ([new(294f, -256f), new(311f, -239f), new(302.5f, -247.5f)], Angle.AnglesIntercardinals[0]),
                0x02 => ([new(275.5f, -212f), new(284f, -203.5f), new(267f, -220.5f)], Angle.AnglesIntercardinals[2]),
                0x03 => ([new(315f, -225f), new(306.5f, -216.5f), new(298f, -208f)], Angle.AnglesIntercardinals[3]),
                _ => default
            };
            if (pos.Item1.Length != 0)
            {
                var act = WorldState.FutureTime(3.7d);
                var angle = pos.Item2;
                var positions = pos.Item1;
                for (var i = 0; i < 3; ++i)
                {
                    _aoes.Add(new(rect, positions[i].Quantized(), angle, act, actorID: 1ul));
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ActivatePuppetVisual:
                _aoes.Add(new(rect, caster.Position.Quantized(), spell.Rotation, WorldState.FutureTime(2.9d)));
                break;
            case (uint)AID.MagicCannon:
                if (++NumCasts == 3)
                {
                    RemoveAOEs();
                }
                break;
            case (uint)AID.AmaljaaArtillery:
                if (++NumCasts == 6)
                {
                    RemoveAOEs(1ul);
                    NumCasts = 0;
                }
                break;
        }

        void RemoveAOEs(ulong id = default)
        {
            var count = _aoes.Count;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.ActorID == id)
                {
                    _aoes.RemoveRange(i, 3);
                    return;
                }
            }
        }
    }
}
