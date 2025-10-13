namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

// TODO: generalize to 'baited puddles' component
sealed class RapidBoltsBait(BossModule module) : Components.UniformStackSpread(module, default, 5f)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.RapidBolts)
            AddSpread(actor);
    }
}

sealed class RapidBoltsAOE(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(WPos pos, int numCasts)> _puddles = [];
    private static readonly AOEShapeCircle _shape = new(5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _puddles.Count;
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            aoes[i] = new(_shape, _puddles[i].pos);
        }
        return aoes;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.RapidBoltsAOE)
        {
            ++NumCasts;
            var count = _puddles.Count - 1;
            for (var i = count; i >= 0; --i)
            {
                var puddle = _puddles[i];
                if (puddle.pos.InCircle(spell.TargetXZ, 1f))
                {
                    if (puddle.numCasts < 11)
                        _puddles[i] = (spell.TargetXZ, puddle.numCasts + 1);
                    else
                        _puddles.RemoveAt(i);
                    return;
                }
            }
            _puddles.Add((spell.TargetXZ, 1));
        }
    }
}
