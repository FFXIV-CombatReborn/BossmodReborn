namespace BossMod.Dawntrail.Raid.M06NSugarRiot;

sealed class Quicksand(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(23f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 0x1F and <= 0x23)
        {
            if (state == 0x00020001u)
            {
                var pos = index switch
                {
                    0x1F => Arena.Center,
                    0x20 => new(100f, 80f),
                    0x21 => new(100f, 120f),
                    0x22 => new(120f, 100f),
                    0x23 => new(80f, 100f),
                    _ => default
                };
                if (pos != default)
                {
                    _aoe = [new(circle, pos, default, WorldState.FutureTime(6d))];
                }
            }
            else if (state == 0x00080004u)
            {
                _aoe = [];
            }
        }
    }
}
