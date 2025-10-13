namespace BossMod.Endwalker.VariantCriterion.C02AMR.C021Shishio;

abstract class LightningBolt(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 6f);
sealed class NLightningBolt(BossModule module) : LightningBolt(module, (uint)AID.NLightningBoltAOE);
sealed class SLightningBolt(BossModule module) : LightningBolt(module, (uint)AID.SLightningBoltAOE);

sealed class CloudToCloud(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape1 = new(100f, 1f);
    private static readonly AOEShapeRect _shape2 = new(100f, 3f);
    private static readonly AOEShapeRect _shape3 = new(100f, 6f);

    public bool Active => _aoes.Count > 0;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var deadline = aoes[0].Activation.AddSeconds(1.4d);

        var index = 0;
        while (index < count)
        {
            ref var aoe = ref aoes[index];
            if (aoe.Activation >= deadline)
            {
                break;
            }
            ++index;
        }

        return aoes[..index];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = ShapeForAction(spell.Action.ID);
        if (shape != null)
        {
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (ShapeForAction(spell.Action.ID) != null)
        {
            ++NumCasts;
            var numRemoved = _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1f));
            if (numRemoved != 1)
            {
                ReportError($"Unexpected number of matching aoes: {numRemoved}");
            }
        }
    }

    private static AOEShapeRect? ShapeForAction(uint action) => action switch
    {
        (uint)AID.NCloudToCloud1 or (uint)AID.SCloudToCloud1 => _shape1,
        (uint)AID.NCloudToCloud2 or (uint)AID.SCloudToCloud2 => _shape2,
        (uint)AID.NCloudToCloud3 or (uint)AID.SCloudToCloud3 => _shape3,
        _ => null
    };
}
