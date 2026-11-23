namespace BossMod.Endwalker.Savage.P4S2Hesperos;

// state related to act 3 wreath of thorns
// note: there should be four tethered helpers on activation
sealed class WreathOfThorns3(BossModule module) : BossComponent(module)
{
    public enum State { RangedTowers, Knockback, MeleeTowers, Done }

    public State CurState = State.RangedTowers;
    public int NumJumps;
    public int NumCones;
    private readonly AOEShapeCone _coneAOE = new(50f, 45f.Degrees()); // not sure about half-width...
    private readonly List<Actor> _relevantHelpers = []; // 4 towers -> knockback -> 4 towers
    private Actor? _jumpTarget; // either predicted (if jump is imminent) or last actual (if cones are imminent)
    private BitMask _coneTargets;
    private BitMask _playersInAOE;

    private ReadOnlySpan<Actor> RangedTowers => _relevantHelpers.SpanRange(0, 4);
    //private IEnumerable<Actor> _knockbackThorn => _relevantHelpers.Skip(4).Take(1);
    private ReadOnlySpan<Actor> MeleeTowers => _relevantHelpers.SpanRange(5);
    private readonly List<(int, Actor)> _sorted = new(PartyState.MaxPartySize);

    private const float _jumpAOERadius = 10f;

    public override void Update()
    {
        _coneTargets = _playersInAOE = default;
        if (NumCones == NumJumps)
        {
            _jumpTarget = Raid.WithoutSlot(false, true, true).FarthestFrom(Module.PrimaryActor.Position);
            _playersInAOE = _jumpTarget != null ? Raid.WithSlot(false, true, true).MaskInRadiusExcluding(_jumpTarget, _jumpAOERadius) : default;
        }
        else
        {
            Raid.WithSlot(false, true, true).SortedByRange(Module.PrimaryActor.Position, _sorted);
            for (var k = 0; k < _sorted.Count && k < 3; ++k)
            {
                var (i, player) = _sorted[k];
                _coneTargets.Set(i);
                if (player.Position != Module.PrimaryActor.Position)
                {
                    var direction = (player.Position - Module.PrimaryActor.Position).Normalized();
                    foreach (var (j, p) in Raid.WithSlot(false, true, true))
                    {
                        if (j != i && p.Position.InCone(Module.PrimaryActor.Position, direction, _coneAOE.HalfAngle))
                            _playersInAOE.Set(j);
                    }
                }
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurState != State.Done)
        {
            // TODO: consider raid comps with 3+ melee or ranged...
            var shouldSoakTower = CurState == State.RangedTowers ? actor.Role is Role.Ranged or Role.Healer : actor.Role is Role.Melee or Role.Tank;
            var soakedTower = (CurState == State.RangedTowers ? RangedTowers : MeleeTowers).FirstInRadius(actor.Position, P4S2.WreathTowerRadius);
            if (shouldSoakTower)
            {
                hints.Add("Soak the tower!", soakedTower == null);
            }
            else if (soakedTower != null)
            {
                hints.Add("GTFO from tower!");
            }
        }

        if (_playersInAOE[slot])
        {
            hints.Add("GTFO from aoe!");
        }
        if (NumCones == NumJumps && actor == _jumpTarget && _playersInAOE.Any())
        {
            hints.Add("GTFO from raid!");
        }
        if (NumCones != NumJumps && actor == _jumpTarget && _coneTargets[slot])
        {
            hints.Add("GTFO from boss!");
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_coneTargets.Any())
        {
            foreach (var (s, player) in Raid.WithSlot(false, true, true))
            {
                if (_coneTargets[s])
                    _coneAOE.Draw(Arena, Module.PrimaryActor.Position, Angle.FromDirection(player.Position - Module.PrimaryActor.Position));
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach ((var i, var player) in Raid.WithSlot(false, true, true))
        {
            var isinaoe = _playersInAOE[i];
            Arena.Actor(player, isinaoe ? Colors.PlayerInteresting : Colors.PlayerGeneric, drawWorld: isinaoe ? true : null);
        }

        if (CurState != State.Done)
        {
            foreach (var tower in CurState == State.RangedTowers ? RangedTowers : MeleeTowers)
                Arena.ZoneCircleOutline(tower.Position, P4S2.WreathTowerRadius, Colors.Safe);
        }

        if (NumCones != NumJumps)
        {
            foreach (var (s, player) in Raid.WithSlot(false, true, true))
                if (_coneTargets[s])
                    Arena.Actor(player, Colors.Danger, drawWorld: true);
            Arena.Actor(_jumpTarget, Colors.Vulnerable, drawWorld: true);
        }
        else if (_jumpTarget != null)
        {
            Arena.Actor(_jumpTarget, Colors.Danger, drawWorld: true);
            Arena.ZoneCircleOutline(_jumpTarget.Position, _jumpAOERadius, Colors.Danger);
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.Helper && tether.ID == (uint)TetherID.WreathOfThorns)
            _relevantHelpers.Add(source);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (CurState == State.RangedTowers && spell.Action.ID == (uint)AID.AkanthaiExplodeTower)
            CurState = State.Knockback;
        else if (CurState == State.Knockback && spell.Action.ID == (uint)AID.AkanthaiExplodeKnockback)
            CurState = State.MeleeTowers;
        else if (CurState == State.MeleeTowers && spell.Action.ID == (uint)AID.AkanthaiExplodeTower)
            CurState = State.Done;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.KothornosKickJump:
                ++NumJumps;
                _jumpTarget = WorldState.Actors.Find(spell.MainTargetID);
                break;
            case (uint)AID.KothornosQuake1:
            case (uint)AID.KothornosQuake2:
                ++NumCones;
                break;
        }
    }
}
