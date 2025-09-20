namespace BossMod.RealmReborn.Extreme.Ex3Titan;

// TODO: most of what's here should be handled by SimpleKnockbacks component...
class Upheaval(BossModule module) : Components.GenericKnockback(module, (uint)AID.Upheaval)
{
    private DateTime _remainInPosition;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_remainInPosition > WorldState.CurrentTime)
            return new Knockback[1] { new(Module.PrimaryActor.Position, 13f) };
        return [];
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_remainInPosition > WorldState.CurrentTime)
        {
            // stack just behind boss, this is a good place to bait imminent landslide correctly
            var dirToCenter = (Arena.Center - Module.PrimaryActor.Position).Normalized();
            var pos = Module.PrimaryActor.Position + 2f * dirToCenter;
            hints.AddForbiddenZone(new SDInvertedCircle(pos, 1.5f), _remainInPosition);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            _remainInPosition = Module.CastFinishAt(spell, 1); // TODO: just wait for effectresult instead...
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            _remainInPosition = WorldState.FutureTime(1d); // TODO: just wait for effectresult instead...
    }
}
