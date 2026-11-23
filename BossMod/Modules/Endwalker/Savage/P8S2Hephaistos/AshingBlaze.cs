namespace BossMod.Endwalker.Savage.P8S2;

sealed class AshingBlaze(BossModule module) : Components.GenericAOEs(module)
{
    private WPos? _origin;
    private readonly AOEShapeRect _shape = new(46f, 10f);

    private readonly AOEInstance[] _oneAOEInstance = new AOEInstance[1];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_origin != null)
        {
            _oneAOEInstance[0] = new(_shape, _origin.Value, default, Module.CastFinishAt(Module.PrimaryActor.CastInfo));
            return _oneAOEInstance;
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.AshingBlazeL:
                _origin = (caster.Position - new WDir(_shape.HalfWidth, 0)).Quantized();
                break;
            case (uint)AID.AshingBlazeR:
                _origin = (caster.Position + new WDir(_shape.HalfWidth, 0)).Quantized();
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.AshingBlazeL or (uint)AID.AshingBlazeR)
            _origin = null;
    }
}
