namespace BossMod.Dawntrail.Trial.T04Zelenia;

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(4f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.QueensCrusade)
        {
            _aoe = [new(circle, Arena.Center, default, Module.CastFinishAt(spell, 0.1d))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x01)
        {
            return;
        }
        switch (state)
        {
            case 0x00020001u:
                _aoe = [];
                Arena.Bounds = T04Zelenia.DonutArena;
                break;
            case 0x00080004u:
                Arena.Bounds = T04Zelenia.DefaultArena;
                break;
        }
    }
}
