namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V12Silkie;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom square = new([new Square(V12Silkie.ArenaCenter, 30f)], [new Square(V12Silkie.ArenaCenter, 20f)]);

    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TotalWash && Arena.Bounds.Radius != 20f)
        {
            _aoe = [new(square, Arena.Center, default, WorldState.FutureTime(1.8d))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x04 && state == 0x00020001u)
        {
            Arena.Bounds = new ArenaBoundsSquare(20f);
            _aoe = [];
        }
    }
}
