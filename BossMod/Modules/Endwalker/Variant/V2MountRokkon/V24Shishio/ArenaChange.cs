namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V24Shishio;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(20f, 28f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.StormcloudSummons)
        {
            _aoe = [new(donut, Arena.Center, default, Module.CastFinishAt(spell, 0.7d))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x64)
        {
            if (state == 0x00020001u)
            {
                Arena.Bounds = V24Shishio.CircleBounds;
                Arena.Center = V24Shishio.CircleBounds.Center;
                _aoe = [];
            }
            else if (state == 0x00080004u)
            {
                Arena.Bounds = new ArenaBoundsSquare(20f);
                Arena.Center = V24Shishio.ArenaCenter;
            }
        }
    }
}
