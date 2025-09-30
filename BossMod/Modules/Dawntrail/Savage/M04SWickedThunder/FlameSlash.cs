namespace BossMod.Dawntrail.Savage.M04SWickedThunder;

sealed class FlameSlash(BossModule module) : Components.GenericAOEs(module, (uint)AID.FlameSlashAOE)
{
    private AOEInstance[] _aoe = [];
    public bool SmallArena;

    private static readonly AOEShapeRect _shape = new(40f, 10f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _aoe = [new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _aoe = [];
            SmallArena = true;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 17 && state == 0x00400001u)
        {
            SmallArena = false;
        }
    }
}

sealed class RainingSwords(BossModule module) : Components.CastTowers(module, (uint)AID.RainingSwordsAOE, 3f);

sealed class ChainLightning(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shape = new(7f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 6 ? 6 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.ChainLightning1 or (uint)TetherID.ChainLightning2)
        {
            _aoes.Add(new(_shape, source.Position.Quantized())); // TODO: activation
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ChainLightningAOEFirst or (uint)AID.ChainLightningAOERest)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
                _aoes.RemoveAt(0);
        }
    }
}
