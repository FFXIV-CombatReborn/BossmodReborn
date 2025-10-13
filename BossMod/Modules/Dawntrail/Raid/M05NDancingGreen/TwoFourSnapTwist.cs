namespace BossMod.Dawntrail.Raid.M05NDancingGreen;

sealed class TwoFourSnapTwist(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private readonly AOEShapeRect rect = new(20f, 20f);

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

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TwoSnapTwistFirst1:
            case (uint)AID.TwoSnapTwistFirst2:
            case (uint)AID.TwoSnapTwistFirst3:
            case (uint)AID.TwoSnapTwistFirst4:
            case (uint)AID.TwoSnapTwistFirst5:
            case (uint)AID.TwoSnapTwistFirst6:
            case (uint)AID.TwoSnapTwistFirst7:
            case (uint)AID.TwoSnapTwistFirst8:
            case (uint)AID.FourSnapTwistFirst1:
            case (uint)AID.FourSnapTwistFirst2:
            case (uint)AID.FourSnapTwistFirst3:
            case (uint)AID.FourSnapTwistFirst4:
            case (uint)AID.FourSnapTwistFirst5:
            case (uint)AID.FourSnapTwistFirst6:
            case (uint)AID.FourSnapTwistFirst7:
            case (uint)AID.FourSnapTwistFirst8:
                AddAOE();
                AddAOE(180f.Degrees(), 3.5d);
                break;
        }
        void AddAOE(Angle offset = default, double delay = default)
        {
            var loc = spell.LocXZ;
            var rot = spell.Rotation;
            var pos = delay != default ? loc - 5f * rot.ToDirection() : loc;
            var rot2 = rot + offset;
            _aoes.Add(new(rect, pos, rot2, Module.CastFinishAt(spell, delay), shapeDistance: rect.Distance(pos, rot2)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var count = _aoes.Count;
        if (count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.TwoSnapTwist2:
                case (uint)AID.TwoSnapTwist3:
                case (uint)AID.FourSnapTwist4:
                case (uint)AID.FourSnapTwist5:
                    _aoes.RemoveAt(0);
                    if (count == 2)
                    {
                        ref var aoe2 = ref _aoes.Ref(0);
                        var rot = aoe2.Rotation;
                        aoe2.Origin -= 5f * rot.ToDirection();
                        aoe2.ShapeDistance = rect.Distance(aoe2.Origin, rot);
                    }
                    break;
            }
        }
    }
}
