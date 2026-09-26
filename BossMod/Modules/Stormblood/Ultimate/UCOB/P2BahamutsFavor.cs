namespace BossMod.Stormblood.Ultimate.UCOB;

sealed class P2HugNael(UCOB module) : BossComponent(module)
{
    private readonly Actor _nael = module.Nael()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (hints.FindEnemy(_nael) is AIHints.Enemy nael && nael.Actor.IsTargetable)
        {
            hints.GoalZones.Add(AIHints.GoalSingleTarget(nael.Actor.Position, 10f, 0.5f));
            // we prefer to keep nael center to give casters/ranged the most options when trying to spread during mechanics
            nael.DesiredPosition = Arena.Center;

            if (nael.Actor.HPMP.CurHP == 1u)
            {
                nael.ShouldBeTargeted = true;
            }
        }
    }
}

sealed class P2BahamutsFavorFireball(UCOB module) : Components.UniformStackSpread(module, 4f, 0f, 1)
{
    public Actor? Target;
    private BitMask _fire;
    private BitMask _ice;
    private DateTime _activation;
    public bool FireOut;
    private readonly Actor _nael = module.Nael()!;

    BitMask Forbidden => FireOut ? ~_ice : _fire;

    public void Show()
    {
        if (Target != null)
        {
            AddStack(Target, _activation, Forbidden);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is var id && id == (uint)SID.Firescorched)
        {
            UpdateMask(ref _fire, actor, true);
        }
        else if (id == (uint)SID.Icebitten)
        {
            UpdateMask(ref _ice, actor, true);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is var id && id == (uint)SID.Firescorched)
        {
            UpdateMask(ref _fire, actor, false);
        }

        else if (id == (uint)SID.Icebitten)
        {
            UpdateMask(ref _ice, actor, false);
        }
    }

    void UpdateMask(ref BitMask mask, Actor actor, bool setBit)
    {
        mask[Raid.FindSlot(actor.InstanceID)] = setBit;
        var stacks = CollectionsMarshal.AsSpan(Stacks);
        var len = stacks.Length;
        for (var i = 0; i < len; ++i)
        {
            stacks[i].ForbiddenPlayers = Forbidden;
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Fireball)
        {
            Target = WorldState.Actors.Find(tether.Target);
            _activation = WorldState.FutureTime(5.1d);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.FireballP2)
        {
            Stacks.Clear();
            Target = null;
            _activation = default;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (IsStackTarget(actor))
        {
            hints.GoalZonesEnabled = false;

            if (!FireOut)
            {
                hints.AddForbiddenZone(new SDInvertedCircle(_nael.Position, 5f), Stacks.Ref(0).Activation);
            }
        }
        else
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}

// note: if player dies immediately after chain lightning cast, he won't get a status or have aoe cast; if he dies after status application, aoe will be triggered immediately
sealed class P2BahamutsFavorChainLightning : Components.UniformStackSpread
{
    private BitMask _pendingTargets;
    private DateTime _expectedStatuses;
    private readonly Actor _nael;
    private readonly PartyRolesConfig partyRolesConfig = Service.Config.Get<PartyRolesConfig>();
    private readonly List<Actor> _voidzones;

    public bool FirstSet;

    public P2BahamutsFavorChainLightning(UCOB module) : base(module, 0f, 5f)
    {
        ExtraAISpreadThreshold = 0;
        _voidzones = module.Enemies((uint)OID.VoidzoneSalvation);
        _nael = module.Nael()!;
    }

    public bool ActiveOrSkipped()
    {
        if (Active)
        {
            return true;
        }

        if (!_pendingTargets.Any() || WorldState.CurrentTime < _expectedStatuses)
        {
            return false;
        }

        var raid = Raid.WithSlot(true, true, true);
        var len = raid.Length;
        for (var i = 0; i < len; ++i)
        {
            var p = raid[i];
            if (_pendingTargets[p.Item1] && !p.Item2.IsDead)
            {
                return false;
            }
        }
        return true;
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Thunderstruck)
        {
            AddSpread(actor, status.ExpireAt);
            _pendingTargets = default;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ChainLightning:
                _expectedStatuses = WorldState.FutureTime(1d);
                var targets = CollectionsMarshal.AsSpan(spell.Targets);
                var len = targets.Length;
                for (var i = 0; i < len; ++i)
                {
                    ref readonly var t = ref targets[i];
                    _pendingTargets.Set(Raid.FindSlot(t.ID));
                }
                break;
            case (uint)AID.ChainLightningAOE:
                Spreads.Clear();
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!EnableHints)
        {
            return;
        }
        var count = Spreads.Count;
        if (count == 0)
        {
            return;
        }
        var spreads = CollectionsMarshal.AsSpan(Spreads);
        var act = spreads[0].Activation;

        if (IsSpreadTarget(actor))
        {
            hints.GoalZonesEnabled = false;
            // don't you dare use a gap closer
            hints.ForbidDashes = true;

            if (FirstSet)
            {
                var ordered = partyRolesConfig.AssignmentsPerSlot(Raid);
                if (ordered.Length == 0)
                {
                    return;
                }
                var myOrder = -1;
                var actorIndex = -1;

                for (var i = 0; i < count; ++i)
                {
                    if (spreads[i].Target == actor)
                    {
                        actorIndex = i;
                        break;
                    }
                }

                if (actorIndex >= 0 && count > actorIndex + 1)
                {
                    var myAssignment = ordered[Raid.FindSlot(spreads[actorIndex].Target.InstanceID)];
                    myOrder = 0;

                    for (var i = 0; i < count; ++i)
                    {
                        if (i == actorIndex)
                        {
                            continue;
                        }

                        var assignment2 = ordered[Raid.FindSlot(spreads[i].Target.InstanceID)];

                        if (assignment2 < myAssignment || assignment == myAssignment && i < actorIndex)
                        {
                            ++myOrder;
                        }
                    }
                }

                var myDir = myOrder == 0 ? -45f.Degrees() : 45f.Degrees();
                hints.AddForbiddenZone(new SDInvertedCircle(_nael.Position + myDir.ToDirection() * 5f, 1f), act);
                return;
            }

            // avoid doom cleanse puddles (unless we are doomed, in which case ignore them)
            // note no activation time specified here, we want to clear the path to the puddle ASAP for other players
            // since they won't want to walk through lightning aoe
            if (!(actor.FindStatus((uint)SID.Doom)?.ExpireAt < act.AddSeconds(1d)))
            {
                var countV = _voidzones.Count;
                for (var i = 0; i < countV; ++i)
                {
                    var p = _voidzones[i];
                    if (p.EventState != 7)
                    {
                        hints.AddForbiddenZone(new SDCircle(p.Position, 1f + SpreadRadius + ExtraAISpreadThreshold));
                    }
                }
            }

            for (var i = 0; i < count; ++i)
            {
                ref var spread = ref spreads[i];
                hints.AddForbiddenZone(new SDCircle(spread.Target.Position, 5f + ExtraAISpreadThreshold), act);
            }
        }
    }
}

sealed class P2BahamutsFavorDeathstorm(BossModule module) : BossComponent(module)
{
    public int NumDeathstorms;
    private P2BahamutsFavorWingsOfSalvation? _wings;

    sealed class Doom
    {
        public required Actor Player;
        public required DateTime Expiration;
        public int Order;
        public bool Cleansed;
        public WPos? ZonePredicted;
        public Actor? Voidzone;
    }

    private readonly List<Doom> _dooms = [];
    private readonly Doom?[] _doomArray = new Doom?[PartyState.MaxPartySize];
    private readonly DoomExpirationComparer _comparer = new();

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_doomArray[slot] is Doom d && !d.Cleansed)
        {
            hints.Add($"Doom {d.Order + 1}", false);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_doomArray[pcSlot] is Doom { Cleansed: false } d)
        {
            var pos = d.Voidzone?.Position ?? d.ZonePredicted;
            if (pos != null)
            {
                Arena.ZoneCircleOutline(pos.Value, 1f, Colors.Safe);
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var count = _dooms.Count;
        for (var i = 0; i < count; ++i)
        {
            var d = _dooms[i];
            if (!d.Cleansed && d.Player != pc)
            {
                var pos = d.Voidzone?.Position ?? d.ZonePredicted;
                if (pos == null)
                {
                    continue;
                }

                Arena.ZoneCircle(pos.Value, 1f, Colors.AOE);
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _dooms.Count;
        for (var i = 0; i < count; ++i)
        {
            var d = _dooms[i];
            {
                if (!d.Cleansed)
                {
                    var pos = d.Voidzone?.Position ?? d.ZonePredicted;
                    if (pos is not WPos position)
                    {
                        continue;
                    }

                    if (d.Player == actor)
                    {
                        if (actor.FindStatus((uint)SID.Thunderstruck)?.ExpireAt < d.Expiration)
                        {
                            return;
                        }

                        // despite our best efforts, it's possible that a wings puddle can spawn on top of the cleanse puddle
                        if (_wings == null)
                        {
                            var comp = Module.FindComponent<P2BahamutsFavorWingsOfSalvation>();
                            if (comp != null)
                            {
                                _wings = comp;
                            }
                            else
                            {
                                return;
                            }
                        }
                        var aoes = _wings.ActiveAOEs(slot, actor);
                        var len = aoes.Length;
                        var isCovered = false;
                        for (var j = 0; j < len; ++j)
                        {
                            if (position.InCircle(aoes[j].Origin, 4f))
                            {
                                isCovered = true;
                                break;
                            }
                        }

                        hints.AddForbiddenZone(new SDInvertedCircle(pos.Value, 1), isCovered ? default : d.Expiration.AddSeconds(-0.5d));
                    }
                    else
                    {
                        hints.AddForbiddenZone(new SDCircle(position, 1f));
                    }
                }
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.VoidzoneSalvation)
        {
            Doom? first = null;
            var count = _dooms.Count;
            for (var i = 0; i < count; ++i)
            {
                var d = _dooms[i];
                if (d.Voidzone == null)
                {
                    first = d;
                    break;
                }
            }

            if (first != null)
            {
                first.Voidzone = actor;
            }
            else
            {
                ReportError($"Failed to find voidzone predicted pos for {actor}");
            }
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Doom)
        {
            var d = new Doom()
            {
                Player = actor,
                Expiration = status.ExpireAt
            };

            _dooms.Add(d);
            var dooms = CollectionsMarshal.AsSpan(_dooms);
            RefSort.Sort(dooms, _comparer);
            var len = dooms.Length;

            for (var i = 0; i < len; ++i)
            {
                dooms[i].Order = i;
            }

            if (Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            {
                _doomArray[slot] = d;
            }
        }
    }

    private readonly struct DoomExpirationComparer : IRefComparer<Doom>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(ref Doom a, ref Doom b) => a.Expiration.CompareTo(b.Expiration);
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Doom)
        {
            Doom? first = null;
            var count = _dooms.Count;
            for (var i = 0; i < count; ++i)
            {
                var d = _dooms[i];
                if (d.Player == actor)
                {
                    first = d;
                    break;
                }
            }

            if (first != null)
            {
                first.Cleansed = true;
            }
            else
            {
                ReportError($"Failed to find doom on {actor}");
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WingsOfSalvation)
        {
            Doom? first = null;
            var count = _dooms.Count;
            for (var i = 0; i < count; ++i)
            {
                var d = _dooms[i];
                if (d.ZonePredicted == null)
                {
                    first = d;
                    break;
                }
            }

            if (first != null)
            {
                first.ZonePredicted = spell.LocXZ;
            }
            else
            {
                ReportError($"No spare dooms for puddle at {spell.LocXZ}");
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Deathstorm)
        {
            _dooms.Clear();
            Array.Fill(_doomArray, null);
            ++NumDeathstorms;
        }
    }
}

// TODO: we need everyone to spread away from non-expired puddles while baits are active
sealed class P2BahamutsFavorWingsOfSalvation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WingsOfSalvation, 4f);
