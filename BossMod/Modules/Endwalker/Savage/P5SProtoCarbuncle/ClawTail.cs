namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

sealed class ClawTail(BossModule module) : Components.GenericAOEs(module)
{
    public int Progress; // 7 claws + 1 tail total
    private bool _tailFirst;

    private readonly AOEShapeCone _shape = new(45f, 90f.Degrees());

    private readonly AOEInstance[] _oneAOEInstance = new AOEInstance[1];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var rotation = Module.PrimaryActor.CastInfo?.Rotation ?? Module.PrimaryActor.Rotation;
        if (_tailFirst ? Progress == 0 : Progress >= 7)
            rotation += 180f.Degrees();
        {
            _oneAOEInstance[0] = new(_shape, Module.PrimaryActor.Position, rotation, Module.CastFinishAt(Module.PrimaryActor.CastInfo));
            return _oneAOEInstance;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TailToClaw)
            _tailFirst = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RagingClawFirst or (uint)AID.RagingClawFirstRest or (uint)AID.RagingTailSecond or (uint)AID.RagingTailFirst
        or (uint)AID.RagingClawSecond or (uint)AID.RagingClawSecondRest)
            ++Progress;
    }
}
