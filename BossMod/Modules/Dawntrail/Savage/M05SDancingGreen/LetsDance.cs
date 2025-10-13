namespace BossMod.Dawntrail.Savage.M05SDancingGreen;

sealed class LetsDance(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(8);
    public static readonly AOEShapeRect Rect = new(25f, 25f);
    private readonly GetDownBait _bait = module.FindComponent<GetDownBait>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (_bait.CurrentBaits.Count != 0 || count == 0)
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
            var act = count != 0 ? _aoes.Ref(0).Activation.AddSeconds(count * 2.4d) : WorldState.FutureTime(26.1d);
            var pos = Arena.Center.Quantized();
            var rot = modelState == 5 ? Angle.AnglesCardinals[3] : Angle.AnglesCardinals[0];
            _aoes.Add(new(Rect, pos, rot, act, shapeDistance: count == 0 ? Rect.Distance(pos, rot) : null));
            if (count == 1)
            {
                ref var aoe2 = ref _aoes.Ref(1);
                aoe2.Origin += 5f * rot.ToDirection();
                aoe2.ShapeDistance = Rect.Distance(aoe2.Origin, rot);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var count = _aoes.Count;
        if (spell.Action.ID == (uint)AID.LetsDance)
        {
            ++NumCasts;
            if (count != 0)
            {
                _aoes.RemoveAt(0);
                if (count > 1)
                {
                    var aoes = CollectionsMarshal.AsSpan(_aoes);
                    ref var aoe1 = ref aoes[0];
                    var rot1 = aoe1.Rotation;
                    aoe1.Origin -= 5f * rot1.ToDirection();
                    aoe1.ShapeDistance = Rect.Distance(aoe1.Origin, rot1);
                    if (count > 2)
                    {
                        ref var aoe2 = ref aoes[1];
                        var rot2 = aoe2.Rotation;
                        aoe2.Origin += 5f * rot2.ToDirection();
                        aoe2.ShapeDistance = Rect.Distance(aoe2.Origin, rot2);
                    }
                }
            }
        }
    }
}

sealed class LetsDanceRemix(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly Angle a180 = 180f.Degrees();
    private readonly List<AOEInstance> _aoes = new(8);
    private readonly GetDownBait _bait = module.FindComponent<GetDownBait>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0 || _bait.CurrentBaits.Count != 0)
        {
            return [];
        }
        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        if (count > 1)
        {
            ref var aoe0 = ref aoes[0];
            ref var aoe1 = ref aoes[1];
            if (!aoe0.Rotation.AlmostEqual(aoe1.Rotation + a180, Angle.DegToRad))
            {
                aoe0.Color = Colors.Danger;
            }
        }
        return aoes[..max];
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        var count = _aoes.Count;
        if (count > 0)
        {
            var sb = new StringBuilder(4 * (count - 1) + count * 5);
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                var roundedrot = (int)aoes[i].Rotation.Deg;
                var shapeHint = roundedrot switch
                {
                    89 => "East",
                    -90 => "West",
                    180 => "North",
                    0 => "South",
                    _ => ""
                };
                sb.Append(shapeHint);

                if (i < count - 1)
                    sb.Append(" -> ");
            }
            hints.Add(sb.ToString());
        }
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.Frogtourage && modelState is 5 or 7 or 31 or 32)
        {
            var count = _aoes.Count;
            var act = count != 0 ? _aoes.Ref(0).Activation.AddSeconds(count * 1.5d) : WorldState.FutureTime(26d);
            var rot = modelState == 5 ? Angle.AnglesCardinals[3] : modelState == 31 ? Angle.AnglesCardinals[1] : modelState == 32 ? a180 : Angle.AnglesCardinals[0];
            var pos = Arena.Center.Quantized();
            var rect = LetsDance.Rect;
            var check = count != 0 && _aoes.Ref(0).Rotation.AlmostEqual(rot + a180, Angle.DegToRad);
            _aoes.Add(new(rect, pos, rot, act, shapeDistance: count == 0 || !check ? rect.Distance(pos, rot) : null));
            if (count == 1 && check)
            {
                ref var aoe2 = ref _aoes.Ref(1);
                aoe2.Origin += 5f * rot.ToDirection();
                aoe2.ShapeDistance = rect.Distance(aoe2.Origin, rot);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LetsDanceRemix)
        {
            ++NumCasts;
            var count = _aoes.Count;
            if (count != 0)
            {
                _aoes.RemoveAt(0);
                if (count > 1)
                {
                    var aoes = CollectionsMarshal.AsSpan(_aoes);
                    ref var aoe1 = ref aoes[0];
                    var rot1 = aoe1.Rotation;
                    if (aoe1.Origin != Arena.Center.Quantized())
                    {
                        aoe1.Origin -= 5f * rot1.ToDirection();
                        aoe1.ShapeDistance = LetsDance.Rect.Distance(aoe1.Origin, rot1);
                    }
                    if (count > 2)
                    {
                        ref var aoe2 = ref aoes[1];
                        var rot2 = aoe2.Rotation;
                        if (rot1.AlmostEqual(rot2 + a180, Angle.DegToRad))
                        {
                            aoe2.Origin += 5f * rot2.ToDirection();
                            aoe2.ShapeDistance = LetsDance.Rect.Distance(aoe2.Origin, rot2);
                        }
                    }
                }
            }
        }
    }
}
