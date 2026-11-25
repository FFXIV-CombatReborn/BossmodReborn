namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4QueensGuard;

sealed class GreatBallOfFire(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _smallFlames = module.Enemies((uint)OID.RagingFlame);
    private readonly List<Actor> _bigFlames = module.Enemies((uint)OID.ImmolatingFlame);
    private readonly DateTime _activation = module.WorldState.FutureTime(6.6d);

    private readonly AOEShapeCircle _shapeSmall = new(10f);
    private readonly AOEShapeCircle _shapeBig = new(18f);

    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var countS = _smallFlames.Count;
        var countB = _bigFlames.Count;
        var aoesLen = countS + countB;
        if (_aoesBuffer.Length < aoesLen)
            _aoesBuffer = new AOEInstance[aoesLen];
        var aoes = _aoesBuffer;
        var index = 0;
        for (var i = 0; i < countS; ++i)
        {
            var f = _smallFlames[i];
            aoes[index++] = new(_shapeSmall, f.Position, default, Module.CastFinishAt(f.CastInfo, 0, _activation));
        }
        for (var i = 0; i < countB; ++i)
        {
            var f = _bigFlames[i];
            aoes[index++] = new(_shapeBig, f.Position, default, Module.CastFinishAt(f.CastInfo, 0, _activation));
        }
        return aoes.AsSpan(0, aoesLen);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.BurnSmall or (uint)AID.BurnBig)
            ++NumCasts;
    }
}
