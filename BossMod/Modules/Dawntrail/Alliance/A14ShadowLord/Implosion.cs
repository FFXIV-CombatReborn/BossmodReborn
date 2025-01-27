namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

class Implosion(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);

    private static readonly AOEShapeCone _shapeSmall = new(12, 90.Degrees()), _shapeLarge = new(90, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Module.FindComponent<GigaSlash>()?.AOEs.Count == 0 ? _aoes : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = (AID)spell.Action.ID switch
        {
            AID.ImplosionLargeL or AID.ImplosionLargeR => _shapeLarge,
            AID.ImplosionSmallL or AID.ImplosionSmallR => _shapeSmall,
            _ => null
        };
        if (shape != null)
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), ActorID: caster.InstanceID));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ImplosionLargeL or AID.ImplosionSmallR or AID.ImplosionLargeR or AID.ImplosionSmallL)
        {
            ++NumCasts;
            for (var i = 0; i < _aoes.Count; ++i)
            {
                var aoe = _aoes[i];
                if (aoe.ActorID == caster.InstanceID)
                {
                    _aoes.Remove(aoe);
                    break;
                }
            }
        }
    }
}
