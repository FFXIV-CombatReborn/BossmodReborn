namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to sunshadow tethers during fountain of fire mechanics
sealed class SunshadowTether(BossModule module) : BossComponent(module)
{
    private readonly HashSet<ulong> _chargedSunshadows = [];
    private BitMask _playersInAOE;

    private const float _chargeHalfWidth = 3;

    public int NumCharges => _chargedSunshadows.Count;

    public override void Update()
    {
        _playersInAOE.Reset();
        foreach (var bird in Module.Enemies((uint)OID.Sunshadow))
        {
            if (_chargedSunshadows.Contains(bird.InstanceID))
                continue;
            var targetID = BirdTarget(bird);
            var target = targetID != 0 ? WorldState.Actors.Find(targetID) : null;
            if (target != null && target.Position != bird.Position)
            {
                var dir = (target.Position - bird.Position).Normalized();
                foreach ((var i, var player) in Raid.WithSlot(false, true, true))
                {
                    if (player != target && player.Position.InRect(bird.Position, dir, 50, 0, _chargeHalfWidth))
                    {
                        _playersInAOE.Set(i);
                    }
                }
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var bird in Module.Enemies((uint)OID.Sunshadow))
        {
            if (_chargedSunshadows.Contains(bird.InstanceID))
                continue;
            var birdTarget = BirdTarget(bird);
            if (birdTarget == actor.InstanceID && bird.Tether.ID != (uint)TetherID.LargeBirdFar)
            {
                hints.Add("Too close!");
            }
        }

        if (_playersInAOE[slot])
        {
            hints.Add("GTFO from charge zone!");
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var bird in Module.Enemies((uint)OID.Sunshadow))
        {
            if (_chargedSunshadows.Contains(bird.InstanceID))
                continue;
            var targetID = BirdTarget(bird);
            var target = (targetID != 0 && targetID != pc.InstanceID) ? WorldState.Actors.Find(targetID) : null;
            if (target != null && target.Position != bird.Position)
            {
                var dir = (target.Position - bird.Position).Normalized();
                Arena.ZoneRect(bird.Position, dir, 50, 0, _chargeHalfWidth, Colors.AOE);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var birds = Module.Enemies((uint)OID.Sunshadow);
        var anyActive = false;
        Actor? myBird = null;
        for (var b = 0; b < birds.Count; ++b)
        {
            var bird = birds[b];
            anyActive |= !_chargedSunshadows.Contains(bird.InstanceID);
            if (myBird == null && BirdTarget(bird) == pc.InstanceID)
                myBird = bird;
        }
        if (!anyActive)
            return;

        // draw all players
        foreach ((var i, var player) in Raid.WithSlot(false, true, true))
        {
            var isinaoe = _playersInAOE[i];
            Arena.Actor(player, isinaoe ? Colors.PlayerInteresting : Colors.PlayerGeneric, drawWorld: isinaoe ? true : null);
        }

        // draw my tether
        if (myBird != null && !_chargedSunshadows.Contains(myBird.InstanceID))
        {
            Arena.AddLine(myBird.Position, pc.Position, myBird.Tether.ID != (uint)TetherID.LargeBirdFar ? Colors.Danger : Colors.Safe);
            Arena.Actor(myBird);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Fireglide)
            _chargedSunshadows.Add(caster.InstanceID);
    }

    private ulong BirdTarget(Actor bird)
    {
        // we don't get tether messages when birds spawn, so use target as a fallback
        // TODO: investigate this... we do get actor-control 503 before spawn, maybe this is related somehow...
        return bird.Tether.Target != 0 ? bird.Tether.Target : bird.TargetID;
    }

}
