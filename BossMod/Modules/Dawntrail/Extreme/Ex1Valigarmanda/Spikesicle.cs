namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

sealed class Spikesicle(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(5);

    private static readonly AOEShape[] _shapes = [new AOEShapeDonut(20f, 25f), new AOEShapeDonut(25f, 30f), new AOEShapeDonut(30f, 35f), new AOEShapeDonut(35f, 40f), new AOEShapeRect(40f, 2.5f)]; // TODO: verify inner radius

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = new AOEInstance[max];
        for (var i = 0; i < max; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes[i] = count > 1 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe;
        }
        return aoes;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020004 && index is >= 4 and <= 13)
        {
            // 4  - 53 +20
            // 5  - 53 -20
            // 6  - 54 +20
            // 7  - 54 -20
            // 8  - 55 +20
            // 9  - 55 -20
            // 10 - 56 +20
            // 11 - 56 -20
            // 12 - 57 -17
            // 13 - 57 +17
            var shape = _shapes[(index - 4) >> 1];
            var odd = (index & 1) != 0;
            var x = index < 12 ? (odd ? -20 : +20) : (odd ? +17 : -17);
            var activationDelay = 11.3f + 0.2f * _aoes.Count;
            _aoes.Add(new(shape, (Module.PrimaryActor.Position + new WDir(x, default)).Quantized(), default, WorldState.FutureTime(activationDelay)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SpikesicleAOE1 or (uint)AID.SpikesicleAOE2 or (uint)AID.SpikesicleAOE3 or (uint)AID.SpikesicleAOE4 or (uint)AID.SpikesicleAOE5)
        {
            ++NumCasts;
            _aoes.RemoveAt(0);
        }
    }
}

sealed class SphereShatter(BossModule module) : Components.GenericAOEs(module, (uint)AID.SphereShatter)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shape = new(13f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        if (count > 1)
        {
            ref var aoe = ref aoes[0];
            aoe.Color = Colors.Danger;
        }
        return aoes[..max];
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.IceBoulder)
        {
            _aoes.Add(new(_shape, actor.Position.Quantized(), default, WorldState.FutureTime(6.5d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SphereShatter)
        {
            ++NumCasts;
            _aoes.RemoveAt(0);
        }
    }
}
