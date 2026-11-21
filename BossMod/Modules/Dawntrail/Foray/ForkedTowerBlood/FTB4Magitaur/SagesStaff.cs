namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB4Magitaur;

sealed class SagesStaff(BossModule module) : Components.GenericBaitStack(module, (uint)AID.ManaExpulsion)
{
    private static readonly AOEShapeRect rect = new(40f, 2f);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11D4 && actor.OID == (uint)OID.SagesStaff)
        {
            CurrentBaits.Add(new(actor, actor, rect, WorldState.FutureTime(11.4d)));
        }
    }

    private readonly List<Actor> _playersBuffer = [];
    public override void Update()
    {
        var count = CurrentBaits.Count;
        if (count == 0)
        {
            return;
        }
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        var players = _playersBuffer;
        players.Clear();
        // note: this is problematic because player culling messes this up and leads to incorrect results (eg not finding the actual bait target)
        // in any frame players can be removed or added to the object table and will likely never contain all 48 players at the same time
        // caching removed players is also pointless since the player position no longer gets updated when this happens
        foreach (var a in Module.WorldState.Actors.Actors.Values)
        {
            if (a.OID == default && !a.IsDead)
            {
                players.Add(a);
            }
        }

        var countP = players.Count;
        for (var i = 0; i < count; ++i)
        {
            ref var b = ref baits[i];
            Actor? closest = null;
            var minDistSq = float.MaxValue;
            var staffPosition = b.Source.Position;

            for (var j = 0; j < countP; ++j)
            {
                var actor = players[j];
                var distSq = (actor.Position - staffPosition).LengthSq();
                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    closest = actor;
                }
            }
            if (closest != null)
            {
                b.Target = closest;
            }
        }
    }
}
