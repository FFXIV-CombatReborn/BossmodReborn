namespace BossMod.Endwalker.VariantCriterion.C03AAI.C031Ketuduke;

class AngrySeasAOE(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    private static readonly AOEShapeRect _shape = new(46f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NAngrySeasAOE or (uint)AID.SAngrySeasAOE)
        {
            _aoe = [new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell))];
        }
    }
}

// TODO: generalize
class AngrySeasKnockback(BossModule module) : Components.GenericKnockback(module)
{
    private readonly List<Knockback> _sources = new(2);
    private static readonly AOEShapeCone _shape = new(30f, 90f.Degrees());

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(_sources);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NAngrySeasAOE or (uint)AID.SAngrySeasAOE)
        {
            _sources.Clear();
            var activation = Module.CastFinishAt(spell);
            var pos = Arena.Center;
            // charge always happens through center, so create two sources with origin at center looking orthogonally
            _sources.Add(new(pos, 12f, activation, _shape, 90f.Degrees(), Kind.DirForward));
            _sources.Add(new(pos, 12f, activation, _shape, -90f.Degrees(), Kind.DirForward));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NAngrySeasAOE or (uint)AID.SAngrySeasAOE)
        {
            _sources.Clear();
            ++NumCasts;
        }
    }
}
