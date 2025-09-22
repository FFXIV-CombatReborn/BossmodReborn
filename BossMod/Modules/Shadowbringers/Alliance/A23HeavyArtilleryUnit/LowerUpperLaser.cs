namespace BossMod.Shadowbringers.Alliance.A23HeavyArtilleryUnit;

sealed class UpperLaser(BossModule module) : Components.GenericAOEs(module)
{
    private readonly LowerLaser _aoe = module.FindComponent<LowerLaser>()!;
    private readonly List<AOEInstance> _aoes = new(9);
    private static readonly Angle a30 = 30f.Degrees();
    private static readonly AOEShape[] donutsectors = [new AOEShapeCone(16f, a30), new AOEShapeDonutSector(14f, 23f, a30),
    new AOEShapeDonutSector(21f, 30f, a30)]; // waves slightly overlap with previous wave, we can use cone instead of donut sector for first wave since it will get clipped with arena shape anyway and save cpu cycles with less vertices

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var max = count > 6 ? 6 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);

        var color = Colors.Danger;
        var max2 = _aoe.AOEs.Count != 0 ? 3 : count;
        for (var i = 0; i < max2; ++i)
        {
            ref var aoe = ref aoes[i];
            if (i < 3 && count > 3)
            {
                aoe.Color = color;
            }
            aoe.Risky = true;
        }
        return aoes[..max];
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00800100u && actor.OID == (uint)OID.ArenaFeatures)
        {
            var center = Arena.Center;
            // the lasers are placed poorly for some reason and can be off by 1-2°, so we need to round to nearest 60°
            var angle = Angle.FromDirection(actor.Position - center).Round(60f);
            var loc = center.Quantized();
            var act = WorldState.FutureTime(9.7d);
            for (var i = 0; i < 3; ++i)
            {
                _aoes.Add(new(donutsectors[i], center, angle, act.AddSeconds(i * 3d), risky: false));
            }
            if (_aoes.Count == 9)
            {
                _aoes.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is >= (uint)AID.UpperLaserFirstSector1 and <= (uint)AID.UpperLaserFirstSector3 or >= (uint)AID.UpperLaserRepeatSector1 and <= (uint)AID.UpperLaserRepeatSector3)
        {
            if (++NumCasts == 15)
            {
                _aoes.RemoveRange(0, 3);
                NumCasts = 0;
            }
        }
    }
}

sealed class LowerLaser(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(3);
    private static readonly AOEShapeCone cone = new(40f, 30f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00100020u && actor.OID == (uint)OID.ArenaFeatures)
        {
            var center = Arena.Center;
            // the lasers are placed poorly for some reason and can be off by 1-2°, so we need to round to nearest 60°
            AOEs.Add(new(cone, center.Quantized(), Angle.FromDirection(actor.Position - center).Round(60f), WorldState.FutureTime(9.7d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LowerLaserRepeatCone)
        {
            if (++NumCasts == 18)
            {
                AOEs.Clear();
                NumCasts = 0;
            }
        }
    }
}
