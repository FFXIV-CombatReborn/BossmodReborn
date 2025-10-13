namespace BossMod.Endwalker.Trial.T08Asura;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(19f, 20f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LowerRealm && Arena.Bounds.Radius > 19.5f)
        {
            _aoe = [new(donut, Arena.Center, default, Module.CastFinishAt(spell, 0.8d))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x01 && state == 0x00020001u)
        {
            Arena.Bounds = T08Asura.DefaultArena;
            _aoe = [];
        }
    }
}
