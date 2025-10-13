namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

sealed class DawnOfAnAgeArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom square = new([new Square(Trial.T02ZoraalJa.ZoraalJa.ArenaCenter, 20f, Trial.T02ZoraalJa.ZoraalJa.ArenaRotation)],
    [new Square(Trial.T02ZoraalJa.ZoraalJa.ArenaCenter, 10f, Trial.T02ZoraalJa.ZoraalJa.ArenaRotation)]);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DawnOfAnAge)
        {
            _aoe = [new(square, Arena.Center, default, Module.CastFinishAt(spell, 0.9d))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x0B && state == 0x00200010u)
        {
            Module.Arena.Bounds = Trial.T02ZoraalJa.ZoraalJa.SmallBounds;
            _aoe = [];
        }
    }
}
