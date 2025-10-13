namespace BossMod.Dawntrail.Savage.M05SDancingGreen;

sealed class RideTheWaves(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeRect rectFull = new(40f, 2.5f), rectMid = new(20f, 2.5f), rectShort = new(15f, 2.5f);
    public readonly List<AOEInstance> AOEs = new(8);
    private readonly M05SDancingGreenConfig _config = Service.Config.Get<M05SDancingGreenConfig>();

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x04 && AOEs.Count == 0)
        {
            var pattern = new (AOEShapeRect shape, float x, float y)[]
            {
                (rectFull, 82.5f, 80f),
                (rectFull, 87.5f, 80f),
                (rectMid, 92.5f, 100f),
                (rectShort, 97.5f, 80f),
                (rectFull, 102.5f, 80f),
                (rectMid, 107.5f, 100f),
                (rectShort, 112.5f, 80f),
                (rectFull, 117.5f, 80f),
            };

            for (var i = 0; i < 8; ++i)
            {
                ref readonly var p = ref pattern[i];
                var finalX = state switch
                {
                    0x02000200u => p.x,
                    0x00800080u => 200f - p.x,
                    _ => default
                };
                AOEs.Add(new(p.shape, new WPos(finalX, p.y) + (_config.MovingExaflares ? new WDir(default, -35f) : default)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (AOEs.Count != 0 && spell.Action.ID == (uint)AID.RideTheWaves)
        {
            ++NumCasts;
            var aoes = CollectionsMarshal.AsSpan(AOEs);
            var act = WorldState.FutureTime(2d);
            for (var i = 0; i < 8; ++i)
            {
                ref var aoe = ref aoes[i];
                if (_config.MovingExaflares || NumCasts > 7)
                {
                    var origin = aoe.Origin;
                    aoe.Origin = new(origin.X, origin.Z + 5f);
                }
                aoe.Activation = act;
            }
        }
    }
}
