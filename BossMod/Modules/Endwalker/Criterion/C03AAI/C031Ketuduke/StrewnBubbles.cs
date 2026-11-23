namespace BossMod.Endwalker.VariantCriterion.C03AAI.C031Ketuduke;

sealed class StrewnBubbles(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private readonly AOEShapeRect _shape = new(20f, 5f);

    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 4 ? 4 : count;
        var aoesLen = max;
        if (_aoesBuffer.Length < aoesLen)
            _aoesBuffer = new AOEInstance[aoesLen];
        var aoes = _aoesBuffer;
        for (var i = 0; i < max; ++i)
            aoes[i] = _aoes[i];
        return aoes.AsSpan(0, aoesLen);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.BubbleStrewer)
        {
            _aoes.Add(new(_shape, actor.Position, actor.Rotation, WorldState.FutureTime(10.7d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NSphereShatter or (uint)AID.SSphereShatter)
        {
            var count = _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
            if (count != 1)
                ReportError($"{spell.Action} removed {count} aoes");
            ++NumCasts;
        }
    }
}

sealed class RecedingEncroachingTwintides(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(2)];
    private readonly AOEShapeCircle _shapeOut = new(14f);
    private readonly AOEShapeDonut _shapeIn = new(8f, 60f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.NRecedingTwintides:
            case (uint)AID.SRecedingTwintides:
                AddAOEs(_shapeOut, _shapeIn);
                break;
            case (uint)AID.NEncroachingTwintides:
            case (uint)AID.SEncroachingTwintides:
                AddAOEs(_shapeIn, _shapeOut);
                break;
        }
        void AddAOEs(AOEShape shape1, AOEShape shape2)
        {
            _aoes.Add(new(shape1, spell.LocXZ, default, Module.CastFinishAt(spell)));
            _aoes.Add(new(shape2, spell.LocXZ, default, Module.CastFinishAt(spell, 3.1f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.NRecedingTwintides:
            case (uint)AID.NNearTide:
            case (uint)AID.NEncroachingTwintides:
            case (uint)AID.NFarTide:
            case (uint)AID.SRecedingTwintides:
            case (uint)AID.SNearTide:
            case (uint)AID.SEncroachingTwintides:
            case (uint)AID.SFarTide:
                if (_aoes.Count != 0)
                    _aoes.RemoveAt(0);
                ++NumCasts;
                break;
        }
    }
}
