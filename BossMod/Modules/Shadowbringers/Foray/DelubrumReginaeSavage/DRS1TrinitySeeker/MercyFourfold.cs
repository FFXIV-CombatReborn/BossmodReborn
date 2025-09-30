namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker;

sealed class MercyFourfold(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(4);
    private readonly AOEShapeCone cone = new(50f, 90f.Degrees());
    private BalefulFirestorm? _aoe1 = module.FindComponent<BalefulFirestorm>();
    private IronSplitter? _aoe2 = module.FindComponent<IronSplitter>();

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoe1 ??= Module.FindComponent<BalefulFirestorm>();
        if (_aoe1 != null && _aoe1.AOEs.Count != 0)
        {
            return [];
        }
        _aoe2 ??= Module.FindComponent<IronSplitter>();
        if (_aoe2 != null && _aoe2.AOEs.Count != 0)
        {
            return CollectionsMarshal.AsSpan(AOEs)[..1];
        }
        var count = AOEs.Count;
        if (count == 0)
        {
            return [];
        }
        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(AOEs);
        return aoes[..max];
    }

    private void UpdateAOEs()
    {
        var aoes = CollectionsMarshal.AsSpan(AOEs);
        var len = aoes.Length;
        ref var aoe0 = ref aoes[0];
        aoe0.Risky = true;
        if (len > 1)
        {
            aoe0.Color = Colors.Danger;
            ref var aoe1 = ref aoes[1];
            if (aoes[0].Rotation.AlmostEqual(aoe1.Rotation + 180f.Degrees(), Angle.DegToRad))
            {
                aoe1.Risky = false;
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID != (uint)SID.Mercy)
        {
            return;
        }

        var dirOffset = status.Extra switch
        {
            0xF7 => -45f.Degrees(),
            0xF8 => -135f.Degrees(),
            0xF9 => 45f.Degrees(),
            0xFA => 135f.Degrees(),
            _ => default
        };
        if (dirOffset == default)
        {
            return;
        }
        var pos = actor.Position.Quantized();
        var rot = (Module.PrimaryActor.CastInfo?.Rotation ?? actor.Rotation) + dirOffset;
        var count = AOEs.Count;
        AOEs.Add(new(cone, pos, rot, WorldState.FutureTime(13.4d + 1.7d * count), shapeDistance: cone.Distance(pos, rot)));
        if (count >= 1)
        {
            UpdateAOEs();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.MercyFourfoldAOE)
        {
            ++NumCasts;
            var count = AOEs.Count;
            if (count != 0)
            {
                AOEs.RemoveAt(0);
                if (count > 1)
                {
                    UpdateAOEs();
                }
            }
        }
    }
}
