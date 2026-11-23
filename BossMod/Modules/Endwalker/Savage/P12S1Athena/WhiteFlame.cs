namespace BossMod.Endwalker.Savage.P12S1Athena;

// TODO: for first instance, common strategy has tanks baiting everything and invulning - accomodate that
// TODO: for second instance, set forbidden baiters
sealed class WhiteFlame(BossModule module) : Components.GenericBaitAway(module)
{
    private bool _enabled;
    private readonly List<Actor> _sources = [];

    private static readonly AOEShapeRect _shape = new(100f, 2f);

    public void Enable() => _enabled = true;

    private readonly List<Actor> _sorted = new(PartyState.MaxPartySize);

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_enabled)
        {
            var party = Raid.WithoutSlot(false, true, true);
            for (var k = 0; k < _sources.Count && k < 2; ++k)
            {
                var s = _sources[k];
                party.SortedByRange(s.Position, _sorted);
                for (var i = 0; i < _sorted.Count && i < 2; ++i)
                    CurrentBaits.Add(new(s, _sorted[i], _shape));
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        Arena.Actors(_sources, Colors.Object, true);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.WhiteFlameAOE)
        {
            foreach (var t in spell.Targets)
                ForbiddenPlayers.Set(Raid.FindSlot(t.ID));
            _sources.RemoveAll(p => p.Position.AlmostEqual(caster.Position, 1f));
            ++NumCasts;
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        // note: technically we could determine location ~1.1s earlier by looking at TeleportAdd cast location
        // however, BaitAway component requires source actor, and teleporting actors don't move to target immediately
        // TODO: consider using 3F2E actor PATE 24E3 instead (it gets destroyed before resolve, guaranteeing that source doesn't move)
        if (id == 0x1E46 && actor.OID == (uint)OID.Anthropos)
            _sources.Add(actor);
    }
}
