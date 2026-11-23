namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

// note: currently we use visual casts to determine all safespots, these happen much earlier than animation changes...
// note: it's somewhat simpler to count casts rather than activating/deactivating stones
sealed class RubyGlow3(BossModule module) : RubyGlowCommon(module, (uint)AID.RubyReflectionQuarter)
{
    private readonly BitMask[] _aoeQuadrants = new BitMask[4]; // [i] = danger quardants at explosion #i, bits: 0=NW, 1=NE, 2=SW, 3=SE

    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoeQuadrants = NumCasts switch
        {
            < 2 => _aoeQuadrants[0],
            < 4 => _aoeQuadrants[1],
            < 7 => _aoeQuadrants[2],
            < 10 => _aoeQuadrants[3],
            _ => new BitMask()
        };
        // TODO: correct explosion time
        var quadrants = aoeQuadrants.SetBits();
        var len = quadrants.Length;
        var aoesLen = len;
        if (_aoesBuffer.Length < aoesLen)
            _aoesBuffer = new AOEInstance[aoesLen];
        var aoes = _aoesBuffer;
        for (var i = 0; i < len; ++i)
            aoes[i] = new(ShapeQuadrant, QuadrantCenter(quadrants[i]));
        return aoes.AsSpan(0, aoesLen);
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (_aoeQuadrants[2].Any())
        {
            var safe = (~_aoeQuadrants[2]).LowestSetBit();
            var safeWaymark = safe < 4 ? WaymarkForQuadrant(safe) : Waymark.Count;
            if (safeWaymark != Waymark.Count)
            {
                hints.Add($"Safespot for third: {safeWaymark}");
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var order = spell.Action.ID switch
        {
            (uint)AID.TopazClusterHit1 => 0,
            (uint)AID.TopazClusterHit2 => 1,
            (uint)AID.TopazClusterHit3 => 2,
            (uint)AID.TopazClusterHit4 => 3,
            _ => -1
        };

        if (order >= 0)
            _aoeQuadrants[order][QuadrantForPosition(caster.Position)] = true;
    }
}
