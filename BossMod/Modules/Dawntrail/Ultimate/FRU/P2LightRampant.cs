namespace BossMod.Dawntrail.Ultimate.FRU;

sealed class P2LightRampant(BossModule module) : BossComponent(module)
{
    private readonly Actor?[] _tetherTargets = new Actor?[PartyState.MaxPartySize];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var len = _tetherTargets.Length;
        for (var i = 0; i < len; ++i)
        {
            var source = Raid[i];
            var target = _tetherTargets[i];
            if (source != null && target != null)
                Arena.AddLine(source.Position, target.Position, (source.Position - target.Position).LengthSq() < 625f ? 0 : Colors.Safe);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.LightRampantChains or (uint)TetherID.LightRampantCurse && Raid.FindSlot(source.InstanceID) is var slot && slot >= 0)
            _tetherTargets[slot] = WorldState.Actors.Find(tether.Target);
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.LightRampantChains or (uint)TetherID.LightRampantCurse && Raid.FindSlot(source.InstanceID) is var slot && slot >= 0)
            _tetherTargets[slot] = null;
    }
}

sealed class P2LuminousHammer(BossModule module) : Components.BaitAwayIcon(module, 6f, (uint)IconID.LuminousHammer, (uint)AID.LuminousHammer, 7.1f)
{
    public readonly int[] BaitsPerPlayer = new int[PartyState.MaxPartySize];
    public readonly WDir[] PrevBaitOffset = new WDir[PartyState.MaxPartySize];

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // note: movement hints are provided by dedicated components; this only marks targeted players as expecting to be damaged
        BitMask predictedDamage = default;
        foreach (var b in CurrentBaits)
            predictedDamage[Raid.FindSlot(b.Target.InstanceID)] = true;
        if (predictedDamage.Any())
            hints.AddPredictedDamage(predictedDamage, CurrentBaits[0].Activation, AIHints.PredictedDamageType.Raidwide);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction && Raid.FindSlot(spell.MainTargetID) is var slot && slot >= 0)
        {
            ++NumCasts;
            PrevBaitOffset[slot] = (Raid[slot]?.Position ?? Arena.Center) - Arena.Center;
            if (++BaitsPerPlayer[slot] == 5)
                CurrentBaits.RemoveAll(b => b.Target == Raid[slot]); // last bait
        }
    }
}

sealed class P2BrightHunger1(BossModule module) : Components.GenericTowers(module, (uint)AID.BrightHunger)
{
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private BitMask _forbidden;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // there are dedicated components for hints

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.LuminousHammer)
        {
            _forbidden.Set(Raid.FindSlot(actor.InstanceID));
            RebuildTowers();
        }
    }

    private void RebuildTowers()
    {
        List<(int slot, int prio)> conga = [];
        foreach (var (slot, group) in _config.P2LightRampantAssignment.Resolve(Raid))
            if (!_forbidden[slot])
                conga.Add((slot, group));
        conga.Sort(static (a, b) => a.prio.CompareTo(b.prio));
        if (conga.Count == 6)
        {
            var firstSouth = conga.FindIndex(kv => kv.prio >= 4);
            if (firstSouth == 2)
            {
                // rotate SW->NW
                (conga[2], conga[1]) = (conga[1], conga[2]);
                (conga[1], conga[0]) = (conga[0], conga[1]);
            }
            else if (firstSouth == 4)
            {
                // rotate NE->SE
                (conga[3], conga[4]) = (conga[4], conga[3]);
                (conga[4], conga[5]) = (conga[5], conga[4]);
            }
            // swap SE & SW to make order CW from NW
            (conga[3], conga[5]) = (conga[5], conga[3]);
            // finally, swap N & S and NW & NE to convert prepositions to tower positions
            (conga[0], conga[2]) = (conga[2], conga[0]);
            (conga[1], conga[4]) = (conga[4], conga[1]);
        }
        else
        {
            // bad assignments, assume there are none set
            conga.Clear();
        }

        Towers.Clear();
        for (var i = 0; i < 6; ++i)
        {
            var dir = (240f - i * 60f).Degrees();
            var forbidden = conga.Count == 6 ? BitMask.Build(conga[i].slot) ^ new BitMask(0xFF) : _forbidden;
            Towers.Add(new(Arena.Center + 16f * dir.ToDirection(), 4, 1, 1, forbidden, WorldState.FutureTime(10.3d)));
        }
    }
}

// note: we can start showing aoes ~3s earlier if we check spawns, but it's not really needed
sealed class P2HolyLightBurst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HolyLightBurst, 11f, 3)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // there are dedicated components for hints
}

sealed class P2PowerfulLight(BossModule module) : Components.UniformStackSpread(module, 5f, default, 4, 4)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // there are dedicated components for hints

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.WeightOfLight)
            AddStack(actor, status.ExpireAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.PowerfulLight)
            Stacks.Clear();
    }
}

sealed class P2BrightHunger2(BossModule module) : Components.GenericTowers(module, (uint)AID.BrightHunger)
{
    private BitMask _forbidden;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // there are dedicated components for hints

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Lightsteeped && status.Extra >= 3)
            _forbidden[Raid.FindSlot(actor.InstanceID)] = true;
    }

    // TODO: better criteria for activating a tower...
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Towers.Count == 0 && spell.Action.ID == (uint)AID.HolyLightBurst)
            Towers.Add(new(Arena.Center, 4f, 1, 8, _forbidden, WorldState.FutureTime(6.5d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Towers.Clear();
            ++NumCasts;
        }
    }
}

// note: this also moves to soak or avoid the central tower, because these mechanics overlap
sealed class P2LightRampantBanish(BossModule module) : P2Banish(module)
{
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private readonly P2BrightHunger2? _tower = module.FindComponent<P2BrightHunger2>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_tower?.Towers.Count > 0)
        {
            // first deal with towers
            ref var t = ref _tower.Towers.Ref(0);
            if (t.ForbiddenSoakers[slot])
            {
                // we should not be soaking a tower
                hints.AddForbiddenZone(new SDCircle(t.Position, 4f), t.Activation);
                hints.AddForbiddenZone(new SDCircle(t.Position, 6f), WorldState.FutureTime(30d));

                var prepos = PrepositionLocation(assignment);
                if (prepos != null)
                {
                    // we know the mechanic, so preposition immediately
                    // there might be puddles covering prepos spot, so add extra rougher hint
                    hints.AddForbiddenZone(new SDInvertedCircle(prepos.Value, 1f), DateTime.MaxValue);
                    hints.AddForbiddenZone(new SDInvertedCone(Arena.Center, 50f, Angle.FromDirection(prepos.Value - Arena.Center), 15f.Degrees()), WorldState.FutureTime(60d));
                }
            }
            else
            {
                // go soak the tower, somewhat offset to the assigned direction
                hints.AddForbiddenZone(new SDInvertedCircle(t.Position, 4f), t.Activation);
                hints.AddForbiddenZone(new SDInvertedCircle(t.Position, 2f), DateTime.MaxValue);

                var clockspot = _config.P2Banish2SpreadSpots[assignment];
                if (clockspot >= 0)
                {
                    var assignedDirection = (180f - 45f * clockspot).Degrees();
                    hints.AddForbiddenZone(new SDInvertedCone(t.Position, 50f, assignedDirection, 30f.Degrees()), DateTime.MaxValue);
                }
            }
        }
        else
        {
            // now that towers are done, resolve the spread/stack
            var prepos = PrepositionLocation(assignment);
            if (prepos != null)
                hints.AddForbiddenZone(new SDInvertedCircle(prepos.Value, 1f), DateTime.MaxValue);
            else
                base.AddAIHints(slot, actor, assignment, hints);
        }
    }

    private WPos? PrepositionLocation(PartyRolesConfig.Assignment assignment)
    {
        var clockspot = _config.P2Banish2SpreadSpots[assignment];
        if (clockspot < 0)
            return null; // no assignment

        var assignedDirection = (180f - 45f * clockspot).Degrees();
        if (Stacks.Count > 0 && Stacks[0].Activation > WorldState.FutureTime(1d))
        {
            var isSupport = assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT or PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.H2;
            if (_config.P2Banish2SupportsMoveToStack == isSupport)
                assignedDirection += (_config.P2Banish2MoveCCWToStack ? 45f : -45f).Degrees();
            return Arena.Center + 10f * assignedDirection.ToDirection();
        }
        else if (Spreads.Count > 0 && Spreads[0].Activation > WorldState.FutureTime(1d))
        {
            return Arena.Center + 13f * assignedDirection.ToDirection();
        }
        return null;
    }
}

sealed class P2HouseOfLightBoss(BossModule module) : Components.GenericBaitAway(module, (uint)AID.HouseOfLightBossAOE, false)
{
    private static readonly AOEShapeCone _shape = new(60f, 30f.Degrees()); // TODO: verify angle

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HouseOfLightBoss)
            foreach (var p in Raid.WithoutSlot(true, true, true))
                CurrentBaits.Add(new(caster, p, _shape, Module.CastFinishAt(spell, 0.9f)));
    }
}

// movement to soak towers and bait first 3 puddles (third puddle is baited right before towers resolve)
sealed class P2LightRampantAITowers(BossModule module) : BossComponent(module)
{
    private readonly P2LuminousHammer? _puddles = module.FindComponent<P2LuminousHammer>();
    private readonly P2BrightHunger1? _towers = module.FindComponent<P2BrightHunger1>();

    private const float BaitOffset = 8;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_puddles == null || _towers == null)
            return;

        var bait = _puddles.ActiveBaitsOn(actor).FirstOrDefault();
        if (bait.Target != null)
        {
            if (_puddles.BaitsPerPlayer[slot] == 0)
            {
                // position for first bait
                var partner = _puddles.ActiveBaitsNotOn(actor).FirstOrDefault().Target;
                if (partner == null)
                    return; // we can't resolve the hint without knowing the partner

                // logic:
                // - if actor and partner are north and south, stay on current side
                // - if both are on the same side, the 'more clockwise' one (NE/SW) moves to the opposite side
                // TODO: last rule is fuzzy in practice, see if we can adjust better
                var z = Arena.Center.Z;
                var north = actor.PosRot.Z < z;
                if (north == (partner.PosRot.Z < z))
                {
                    // same side, see if we need to swap
                    var moreRight = actor.PosRot.X > partner.PosRot.X;
                    var moreCW = north == moreRight;
                    north ^= moreCW;
                }

                var preposSpot = Arena.Center + new WDir(default, north ? -BaitOffset : BaitOffset);
                hints.AddForbiddenZone(new SDInvertedCircle(preposSpot, 1), bait.Activation);
            }
            else
            {
                // each next bait is just previous position rotated CW by 45 degrees
                // note that this is only really relevant for second and third puddles - after that towers resolve and we use different component
                //var nextSpot = Arena.Center + BaitOffset * _puddles.PrevBaitOffset[slot].Normalized().Rotate(-45.Degrees());
                //hints.AddForbiddenZone(new SDInvertedCircle(nextSpot, 3));
                hints.AddForbiddenZone(new SDInvertedDonutSector(Arena.Center, BaitOffset - 1f, BaitOffset + 2f, Angle.FromDirection(_puddles.PrevBaitOffset[slot]) - 45f.Degrees(), 30f.Degrees()), DateTime.MaxValue);
            }
        }
        else
        {
            // if we have one tower assigned, stay inside it, somewhat closer to the edge
            var assignedTowerIndex = _towers.Towers.FindIndex(t => !t.ForbiddenSoakers[slot]);
            if (assignedTowerIndex >= 0 && _towers.Towers.FindIndex(assignedTowerIndex + 1, t => !t.ForbiddenSoakers[slot]) < 0)
            {
                ref var t = ref _towers.Towers.Ref(assignedTowerIndex);
                hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center + (t.Position - Arena.Center) * 1.125f, 2f), t.Activation); // center is at R16, x1.125 == R18
            }
            // else: we either have no towers assigned (== doing puddles), or have multiple assigned (== assignments failed), so do nothing
        }
    }
}

// movement to stack N/S after towers (and bait last two puddles)
sealed class P2LightRampantAIStackPrepos(BossModule module) : BossComponent(module)
{
    private readonly P2LuminousHammer? _puddles = module.FindComponent<P2LuminousHammer>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var isPuddleBaiter = _puddles?.ActiveBaitsOn(actor).Count != 0;
        var northCamp = isPuddleBaiter ? actor.PosRot.X < Arena.Center.X : actor.PosRot.Z < Arena.Center.Z; // this assumes CW movement for baiter
        var dest = Arena.Center + new WDir(default, northCamp ? -18f : 18f);
        if (isPuddleBaiter)
        {
            var maxDist = _puddles?.BaitsPerPlayer[slot] == 4 ? 7f : 13f;
            if (dest.InCircle(actor.Position, maxDist))
                return; // don't move _too_ fast as a baiter
        }
        hints.AddForbiddenZone(new SDInvertedCircle(dest, 1f), DateTime.MaxValue);
    }
}

// movement to resolve stacks
sealed class P2LightRampantAIStackResolve(BossModule module) : BossComponent(module)
{
    private readonly P2PowerfulLight? _stack = module.FindComponent<P2PowerfulLight>();
    private readonly P2HolyLightBurst? _orbs = module.FindComponent<P2HolyLightBurst>();

    public const float Radius = 18;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_stack == null || _orbs == null)
            return;

        var northCamp = IsNorthCamp(actor);
        var centerDangerous = false;
        var len = _orbs.ActiveCasters.Length;

        for (var i = 0; i < len; ++i)
        {
            var offset = _orbs.ActiveCasters[i].Origin.Z - Arena.Center.Z;
            if (northCamp && offset < -15f || !northCamp && offset > 15f)
            {
                centerDangerous = true;
                break;
            }
        }

        var destDir = (northCamp ? 180f : default).Degrees() - (centerDangerous ? 40f : 20f).Degrees();
        var destPos = Arena.Center + Radius * destDir.ToDirection();
        if (_stack.IsStackTarget(actor))
        {
            // as a stack target, our responsibility is to wait for everyone to stack up, then carefully move towards destination
            // note that we need to be careful to avoid oscillations
            var toDest = destPos - actor.Position;
            bool needToWaitFor(Actor partner)
            {
                if (partner == actor || IsNorthCamp(partner) != northCamp)
                    return false; // this is not our partner, we don't need to wait for him
                var toPartner = partner.Position - actor.Position;
                var distSq = toPartner.LengthSq();
                return distSq > 9f && toDest.Dot(toPartner) < 0f; // partner is far enough away, and moving towards destination will not bring us closer
            }
            if (!Raid.WithoutSlot(false, true, true).Any(needToWaitFor))
                hints.AddForbiddenZone(new SDInvertedCircle(destPos, 1f), DateTime.MaxValue);
            // else: we still have someone we need to wait for, just stay where we are...
        }
        else if (_stack.Stacks.FirstOrDefault(s => IsNorthCamp(s.Target) == northCamp).Target is var stackTarget && stackTarget != null)
        {
            // we just want to stay close to the stack target, slightly offset to the destination
            var dirToDest = destPos - stackTarget.Position;
            var dest = dirToDest.LengthSq() <= 4f ? destPos : stackTarget.Position + 2f * dirToDest.Normalized();
            hints.AddForbiddenZone(new SDInvertedCircle(dest, 1), DateTime.MaxValue);
        }
    }

    private bool IsNorthCamp(Actor actor) => actor.PosRot.Z < Arena.Center.Z;
}

// movement to dodge orbs after resolving stack
sealed class P2LightRampantAIOrbs(BossModule module) : BossComponent(module)
{
    private readonly P2HolyLightBurst? _orbs = module.FindComponent<P2HolyLightBurst>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_orbs == null || _orbs.Casters.Count == 0)
            return;

        // actual orb aoes; use slightly bigger radius to make dodges less sus
        foreach (var c in _orbs.ActiveCasters)
            hints.AddForbiddenZone(new SDCircle(c.Origin, 12f), c.Activation);

        if (_orbs.NumCasts == 0)
        {
            // dodge first orbs, while staying near edge
            hints.AddForbiddenZone(new SDCircle(Arena.Center, 16f));
            // ... and close to the aoes
            var len = _orbs.ActiveCasters.Length;
            var orbs = new ShapeDistance[len];
            for (var i = 0; i < len; ++i)
                orbs[i] = new SDCircle(_orbs.ActiveCasters[i].Origin, 13.5f);
            hints.AddForbiddenZone(new SDInvertedUnion(orbs), DateTime.MaxValue);
        }
        else if (_orbs.Casters.Any(c => _orbs.Shape.Check(actor.Position, c.Origin, default)))
        {
            // dodge second orbs while staying near edge (tethers are still up for a bit after first dodge)
            hints.AddForbiddenZone(new SDCircle(Arena.Center, 16f));
        }
        else
        {
            // now that we're safe, move closer to the center (this is a bit sus, but whatever...)
            hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center, 16f), WorldState.FutureTime(30d));
            hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center, 13f), WorldState.FutureTime(40d));
            hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center, 10f), WorldState.FutureTime(50d));
            hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center, 7f), DateTime.MaxValue);
        }
    }
}
