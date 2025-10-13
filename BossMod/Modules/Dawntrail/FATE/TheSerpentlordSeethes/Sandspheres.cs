namespace BossMod.Dawntrail.FATE.Ttokrrone;

sealed class Sandspheres(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circleSmall = new(6);
    private static readonly AOEShapeCircle circleBig = new(12);
    private readonly List<AOEInstance> _aoes = new(18);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var deadline = aoes[0].Activation.AddSeconds(4.5d);

        var index = 0;
        while (index < count)
        {
            ref var aoe = ref aoes[index];
            if (aoe.Activation >= deadline)
            {
                break;
            }
            ++index;
        }

        return aoes[..index];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.SummoningSands:
                AddAOE(circleSmall);
                break;
            case (uint)AID.Sandburst1:
            case (uint)AID.Sandburst2:
                AddAOE(circleBig);
                break;
        }
        void AddAOE(AOEShape circle) => _aoes.Add(new(circle, spell.LocXZ, default, Module.CastFinishAt(spell), actorID: caster.InstanceID));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.SummoningSands or (uint)AID.Sandburst1 or (uint)AID.Sandburst2)
        {
            var count = _aoes.Count;
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
