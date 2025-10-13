namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

sealed class GleamingBarrage(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GleamingBarrage, GleamingBeam.Rect);

sealed class ChampionsCircuit(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(5);
    private readonly AOEShapeCone cone = new(22f, 30f.Degrees());
    private readonly AOEShapeDonut donut = new(4f, 13f);
    private readonly AOEShapeDonutSector donutS = new(16f, 28f, 30f.Degrees());
    private readonly AOEShapeRect rect = new(30f, 6f);
    private bool clockwise;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.ChampionsCircuitCW)
        {
            clockwise = true;
            return;
        }

        AOEShape? shape = id switch
        {
            (uint)AID.ChampionsCircuitDonutSectorFirst1 or (uint)AID.ChampionsCircuitDonutSectorFirst2 => donutS,
            (uint)AID.ChampionsCircuitDonutFirst => donut,
            (uint)AID.ChampionsCircuitConeFirst => cone,
            (uint)AID.ChampionsCircuitRectFirst => rect,
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ChampionsCircuitDonutFirst or (uint)AID.ChampionsCircuitDonutRest)
        {
            if (++NumCasts < 5)
            {
                var increment = 72f.Degrees();
                var incrAdj = clockwise ? -increment : increment;
                var activation = WorldState.FutureTime(4.4d);
                var aoes = CollectionsMarshal.AsSpan(_aoes);
                for (var i = 0; i < 5; ++i)
                {
                    ref var aoe = ref aoes[i];

                    if (aoe.Shape == donut)
                    {
                        var rotate = (caster.Position - M08SHowlingBlade.ArenaCenter).Rotate(incrAdj) + M08SHowlingBlade.ArenaCenter;
                        aoe.Origin = rotate.Quantized();
                    }
                    else
                    {
                        aoe.Rotation += incrAdj;
                    }
                    aoe.Activation = activation;
                }
            }
        }
    }
}
