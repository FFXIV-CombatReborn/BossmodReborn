namespace BossMod.Endwalker.VariantCriterion.C02AMR.C021Shishio;

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom square = new([new Square(C021Shishio.ArenaCenter, 25f)], [new Square(C021Shishio.ArenaCenter, 20f)]);
    private static readonly AOEShapeDonut donut = new(20f, 30f);

    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShape shape) => _aoe = [new(shape, Arena.Center, default, Module.CastFinishAt(spell, 0.8d))];
        var id = spell.Action.ID;
        if (id is (uint)AID.NEnkyo or (uint)AID.SEnkyo && Arena.Bounds.Radius > 20f)
        {
            AddAOE(square);
        }
        else if (id is (uint)AID.NStormcloudSummons or (uint)AID.SStormcloudSummons)
        {
            AddAOE(donut);
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001)
        {
            if (index == 0x34)
            {
                Arena.Bounds = C021Shishio.CircleBounds;
                _aoe = [];
            }
            else if (index == 0x35)
            {
                Arena.Bounds = C021Shishio.DefaultBounds;
                _aoe = [];
            }
        }
    }
}
