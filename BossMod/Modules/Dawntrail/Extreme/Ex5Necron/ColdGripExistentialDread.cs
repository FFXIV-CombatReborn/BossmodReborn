namespace BossMod.Dawntrail.Extreme.Ex5Necron;

sealed class ColdGripExistentialDread(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(3);
    private readonly AOEShapeRect rect1 = new(100f, 6f), rect2 = new(100f, 12f);
    private float offset;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ColdGrip:
                _aoes.Add(new(rect1, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                if (offset != default)
                {
                    AddLastAOE(1.6d);
                }
                break;
            case (uint)AID.ColdGripVisual1:
                offset = 6f;
                AddLastAOE();
                break;
            case (uint)AID.ColdGripVisual2:
                offset = -6f;
                AddLastAOE();
                break;
        }
        void AddLastAOE(double delay = 2.6d)
        {
            if (_aoes.Count == 2) // create 3rd AOE with small offset to leave a safespot in the right direction
            {
                _aoes.Add(new(rect2, new WPos(100f + offset + (offset > 0 ? 1f : -1f), 85f), spell.Rotation, Module.CastFinishAt(spell, delay)));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ColdGrip or (uint)AID.ExistentialDread)
        {
            ++NumCasts;
            var count = _aoes.Count;
            if (count != 0)
            {
                _aoes.RemoveAt(0);
                if (count == 2) // update 3rd AOE to the actual position
                {
                    ref var aoe = ref _aoes.Ref(0);
                    aoe.Origin = new WPos(100f + offset, 85f).Quantized();
                }
            }
        }
    }
}
