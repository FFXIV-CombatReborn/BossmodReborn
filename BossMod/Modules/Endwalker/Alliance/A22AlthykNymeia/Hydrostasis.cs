namespace BossMod.Endwalker.Alliance.A22AlthykNymeia;

class Hydrostasis(BossModule module) : Components.GenericKnockback(module)
{
    private readonly List<Knockback> _sources = [];

    public bool Active => _sources.Count == 3 || NumCasts > 0;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => Active ? CollectionsMarshal.AsSpan(_sources) : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.HydrostasisAOE1 or (uint)AID.HydrostasisAOE2 or (uint)AID.HydrostasisAOE3 or (uint)AID.HydrostasisAOEDelayed)
            AddSource(caster.Position, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.HydrostasisAOE1 or (uint)AID.HydrostasisAOE2 or (uint)AID.HydrostasisAOE3 or (uint)AID.HydrostasisAOE0 or (uint)AID.HydrostasisAOEDelayed)
        {
            ++NumCasts;
            if (_sources.Count > 0)
                _sources.RemoveAt(0);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.HydrostasisQuick)
            AddSource(source.Position, WorldState.FutureTime(12d));
    }

    private void AddSource(WPos pos, DateTime activation)
    {
        _sources.Add(new(pos, 28f, activation));
        _sources.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
    }
}
