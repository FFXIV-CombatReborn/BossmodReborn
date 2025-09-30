namespace BossMod.Endwalker.VariantCriterion.C01ASS.C011Silkie;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom square = new([new Square(C011Silkie.ArenaCenter, 30f)], [new Square(C011Silkie.ArenaCenter, 20f)]);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NFizzlingSuds or (uint)AID.SFizzlingSuds && Arena.Bounds.Radius > 20f)
        {
            _aoe = [new(square, Arena.Center, default, WorldState.FutureTime(3.8d))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x01 && state == 0x00020001u)
        {
            Arena.Bounds = C011Silkie.DefaultBounds;
            _aoe = [];
        }
    }
}
