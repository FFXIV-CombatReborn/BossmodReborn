namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

sealed class RubyGlow6(BossModule module) : RubyGlowRecolor(module, 9)
{
    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // TODO: correct explosion time
        var condition = CurRecolorState != RecolorState.BeforeStones && MagicStones.Count != 0;
        var poison = ActivePoisonAOEs();
        var len = poison.Length;
        var aoesLen = (condition ? 1 : 0) + len;
        if (_aoesBuffer.Length < aoesLen)
            _aoesBuffer = new AOEInstance[aoesLen];
        var aoes = _aoesBuffer;
        var index = 0;
        if (condition)
            aoes[index++] = new(ShapeQuadrant, QuadrantCenter(AOEQuadrant));
        for (var i = 0; i < len; ++i)
        {
            aoes[index++] = poison[i];
        }
        return aoes.AsSpan(0, aoesLen);
    }
}
