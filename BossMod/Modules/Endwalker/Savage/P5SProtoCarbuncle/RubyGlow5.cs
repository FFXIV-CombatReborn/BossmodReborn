namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

sealed class RubyGlow5(BossModule module) : RubyGlowCommon(module)
{
    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // TODO: correct explosion time
        var stones = MagicStones;
        var poison = ActivePoisonAOEs();
        var countS = stones.Count;
        var len = poison.Length;
        var aoesLen = countS + len;
        if (_aoesBuffer.Length < aoesLen)
            _aoesBuffer = new AOEInstance[aoesLen];
        var aoes = _aoesBuffer;
        var index = 0;
        for (var i = 0; i < countS; ++i)
        {
            aoes[index++] = new(ShapeQuadrant, QuadrantCenter(QuadrantForPosition(stones[i].Position)));
        }
        for (var i = 0; i < len; ++i)
        {
            aoes[index++] = poison[i];
        }
        return aoes.AsSpan(0, aoesLen);
    }
}
