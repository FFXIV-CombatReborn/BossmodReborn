namespace BossMod.Dawntrail.Trial.T02ZoraalJaP2;

sealed class DawnOfAnAgeArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly Square square = new(T02ZoraalJa.ZoraalJa.ArenaCenter, 20f, T02ZoraalJa.ZoraalJa.ArenaRotation);
    private static readonly Square smallsquare = new(T02ZoraalJa.ZoraalJa.ArenaCenter, 10f, T02ZoraalJa.ZoraalJa.ArenaRotation);
    private static readonly AOEShapeCustom transition = new([square], [smallsquare]);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x20)
        {
            switch (state)
            {
                case 0x00020001u:
                    _aoe = [new(transition, T02ZoraalJa.ZoraalJa.ArenaCenter, default, WorldState.FutureTime(8d))];
                    break;
                case 0x00080004u:
                    _aoe = [];
                    Arena.Bounds = T02ZoraalJa.ZoraalJa.SmallBounds;
                    break;
            }
        }
        else if (index == 0x1B && state == 0x00080004u)
        {
            Arena.Bounds = T02ZoraalJa.ZoraalJa.DefaultBounds;
        }
    }
}
