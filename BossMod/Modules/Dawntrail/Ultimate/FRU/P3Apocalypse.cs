﻿namespace BossMod.Dawntrail.Ultimate.FRU;

sealed class P3Apocalypse(BossModule module) : Components.GenericAOEs(module)
{
    public Angle? Starting;
    public Angle Rotation;
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shape = new(9f);

    public void Show(float delay)
    {
        void addAOE(WPos pos, DateTime activation) => _aoes.Add(new(_shape, pos, default, activation));
        void addPair(WDir offset, DateTime activation)
        {
            addAOE(Arena.Center + offset, activation);
            addAOE(Arena.Center - offset, activation);
        }
        void addAt(int position, DateTime activation)
        {
            if (position >= 0 && Starting != null)
                addPair(14 * (Starting.Value + Rotation * position).ToDirection(), activation);
            else if (position == -1)
                addPair(default, activation);
        }

        var activation = WorldState.FutureTime(delay);
        for (var i = -1; i < 5; ++i)
        {
            addAt(i + 1, activation);
            addAt(i, activation);
            addAt(i - 1, activation);
            activation = activation.AddSeconds(2d);
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 6 ? 6 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // we have dedicated components for this...

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ApocalypseLight)
        {
            if (actor.Position.AlmostEqual(Arena.Center, 1f))
            {
                if (Starting == null)
                    Starting = actor.Rotation;
            }
            else
            {
                var rot = 0.5f * (actor.Rotation - Angle.FromDirection(actor.Position - Arena.Center)).Normalized();
                if (Rotation == default)
                    Rotation = rot;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ApocalypseAOE)
        {
            ++NumCasts;
            var index = _aoes.FindIndex(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
            if (index >= 0)
                _aoes.RemoveAt(index);
        }
    }
}

sealed class P3ApocalypseDarkWater(BossModule module) : Components.UniformStackSpread(module, 6f, default, 4, 4, includeDeadTargets: true)
{
    public struct State
    {
        public int Order;
        public int InitialGroup;
        public int InitialPosition;
        public int AssignedGroup;
        public int AssignedPosition;
        public DateTime Expiration;
    }

    public int NumStatuses;
    public readonly State[] States = new State[PartyState.MaxPartySize];
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private string _swaps = "";

    // for uptime swaps, there are 6 possible swaps within each 'subgroup': no swaps, p1 with p1/p2, p2 with p1/p2 and both
    private static readonly BitMask[] _uptimeSwaps = [default, BitMask.Build(0, 4), BitMask.Build(0, 5), BitMask.Build(1, 4), BitMask.Build(1, 5), BitMask.Build(0, 1, 4, 5)];

    public void ShowOrder(int order)
    {
        for (int i = 0; i < States.Length; ++i)
            if (States[i].Order == order && Raid[i] is var player && player != null)
                AddStack(player, States[i].Expiration);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        ref var state = ref States[slot];
        if (state.AssignedGroup > 0)
            hints.Add($"Group: {state.AssignedGroup}", false);
        if (state.Order > 0)
            hints.Add($"Order: {state.Order}", false);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_swaps.Length > 0)
            hints.Add(_swaps);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // we have dedicated components for this...

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.SpellInWaitingDarkWater && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            States[slot].Expiration = status.ExpireAt;
            States[slot].Order = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds switch
            {
                < 15d => 1,
                < 34d => 2,
                _ => 3,
            };
            if (++NumStatuses == 6)
                InitAssignments();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.DarkWater)
            Stacks.Clear();
    }

    private void InitAssignments()
    {
        Span<int> slotPerAssignment = [-1, -1, -1, -1, -1, -1, -1, -1];
        foreach (var (slot, group) in _config.P3ApocalypseAssignments.Resolve(Raid))
        {
            ref var state = ref States[slot];
            state.InitialGroup = state.AssignedGroup = group < 4 ? 1 : 2;
            state.InitialPosition = state.AssignedPosition = group & 3;
            slotPerAssignment[group] = slot;
        }

        if (slotPerAssignment[0] < 0)
            return; // no valid assignments

        var swap = _config.P3ApocalypseUptime ? FindUptimeSwap(slotPerAssignment) : FindStandardSwap(slotPerAssignment);
        //var debugSwap = swap.Raw;
        for (int role = 0; role < slotPerAssignment.Length; ++role)
        {
            if (!swap[role])
                continue;
            var slot = slotPerAssignment[role];
            ref var state = ref States[slot];

            // find partner to swap with; prioritize same position > neighbour position (eg melee with melee) > anyone that also swaps
            int partnerRole = -1, partnerQuality = -1;
            for (var candidateRole = role + 1; candidateRole < slotPerAssignment.Length; ++candidateRole)
            {
                if (!swap[candidateRole])
                    continue; // this guy doesn't want to swap, skip
                var candidateSlot = slotPerAssignment[candidateRole];
                if (States[candidateSlot].AssignedGroup == state.AssignedGroup)
                    continue; // this guy is from same group, skip
                var positionDiff = state.AssignedPosition ^ States[candidateSlot].AssignedPosition;
                var candidateQuality = positionDiff switch
                {
                    0 => 2, // same position, best
                    1 => 1, // melee with melee / ranged with ranged (assuming sane config)
                    _ => 0, // melee with ranged
                };
                if (candidateQuality > partnerQuality)
                {
                    partnerRole = candidateRole;
                    partnerQuality = candidateQuality;
                }
            }

            if (partnerRole < 0)
            {
                ReportError($"Failed to find swap for {slot}");
                continue;
            }

            swap.Clear(role);
            swap.Clear(partnerRole);
            var partnerSlot = slotPerAssignment[partnerRole];
            ref var partnerState = ref States[partnerSlot];
            Utils.Swap(ref state.AssignedGroup, ref partnerState.AssignedGroup);
            Utils.Swap(ref state.AssignedPosition, ref partnerState.AssignedPosition);
            _swaps += $"{(_swaps.Length > 0 ? ", " : "")}{Raid[slot]?.Name} <-> {Raid[partnerSlot]?.Name}";
        }
        //ReportError($"FOO: {debugSwap:X2} == {_swaps}");
        _swaps = $"Swaps: {(_swaps.Length > 0 ? _swaps : "none")}";
    }

    private bool IsSwapValid(BitMask assignmentSwaps, ReadOnlySpan<int> slotPerAssignment)
    {
        BitMask result = default; // bits 0-3 are set if order N is in G1, 4-7 for G2
        for (var role = 0; role < slotPerAssignment.Length; ++role)
        {
            ref var state = ref States[slotPerAssignment[role]];
            var isGroup2 = state.AssignedGroup == (assignmentSwaps[role] ? 1 : 2);
            result.Set(state.Order + (isGroup2 ? 4 : 0));
        }
        return result.Raw == 0xFF;
    }

    private BitMask FindUptimeSwap(ReadOnlySpan<int> slotPerAssignment)
    {
        // search for first valid swap, starting with swaps that don't touch higher prios
        foreach (var highSwap in _uptimeSwaps)
        {
            foreach (var lowSwap in _uptimeSwaps)
            {
                var swap = lowSwap ^ new BitMask(highSwap.Raw << 2);
                if (IsSwapValid(swap, slotPerAssignment))
                    return swap;
            }
        }
        ReportError("Failed to find uptime swap");
        return FindStandardSwap(slotPerAssignment);
    }

    private BitMask FindStandardSwap(ReadOnlySpan<int> slotPerAssignment)
    {
        BitMask swap = default;
        Span<int> assignmentPerOrder = [-1, -1, -1, -1];
        for (var role = 0; role < slotPerAssignment.Length; ++role)
        {
            var slot = slotPerAssignment[role];
            var order = States[slot].Order;
            ref var partner = ref assignmentPerOrder[order];
            if (partner < 0)
                partner = role;
            else if ((role < 4) == (partner < 4))
                swap.Set(partner);
            // else: partner is naturally in other group
        }
        return swap;
    }
}

sealed class P3ApocalypseSpiritTaker(BossModule module) : SpiritTaker(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 6), DateTime.MaxValue); // don't dodge into center...
    }
}

sealed class P3ApocalypseDarkEruption(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.DarkEruption, (uint)AID.DarkEruption, 6, 5.1f)
{
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private readonly P3Apocalypse? _apoc = module.FindComponent<P3Apocalypse>();
    private readonly P3ApocalypseDarkWater? _water = module.FindComponent<P3ApocalypseDarkWater>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var safeSpot = SafeOffset(slot, out _);
        if (safeSpot != default)
        {
            hints.PathfindMapBounds = FRU.PathfindHugBorderBounds;
            hints.AddForbiddenZone(ShapeDistance.PrecisePosition(Arena.Center + safeSpot, new(0, 1), Arena.Bounds.MapResolution, actor.Position, 0.1f), Spreads.Count != 0 ? Spreads[0].Activation : DateTime.MaxValue);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        // draw safespots
        var safeSpot = SafeOffset(pcSlot, out var refSafeSpot);
        if (safeSpot != default)
        {
            Arena.AddCircle(Arena.Center + safeSpot, 1f, Colors.Safe);
            if (refSafeSpot != safeSpot)
                Arena.AddCircle(Arena.Center + refSafeSpot, 1f, Colors.Danger);
        }
        else if (refSafeSpot != default)
        {
            // we don't have assignments, at least draw two reference ones
            Arena.AddCircle(Arena.Center + refSafeSpot, 1f, Colors.Danger);
            Arena.AddCircle(Arena.Center - refSafeSpot, 1f, Colors.Danger);
        }
    }

    private WDir SafeOffset(int slot, out WDir reference)
    {
        reference = default;
        if (_apoc?.Starting == null || _water == null)
            return default;

        var midDir = (_apoc.Starting.Value - _apoc.Rotation).Normalized();
        reference = 10 * midDir.ToDirection();

        ref var state = ref _water.States[slot];
        var (group, pos) = _config.P3ApocalypseStaticSpreads ? (state.InitialGroup, state.InitialPosition) : (state.AssignedGroup, state.AssignedPosition);
        if (group == 0)
            return default; // no assignments - oh well, at least we know reference directions

        // G1 takes dir CCW from N, G2 takes 0/45/90/135
        var midIsForG2 = midDir.Deg is >= -20 and < 160;
        if (midIsForG2 != (group == 2))
        {
            midDir += 180f.Degrees();
            reference = -reference;
        }

        if ((pos & 2) == 0)
        {
            // melee spot; note that non-reference melee goes in right after second apoc (max range is 14-9)
            var altPos = _apoc.Rotation.Rad < 0f ? 1 : 0;
            return pos == altPos ? (_apoc.NumCasts > 4 ? 4.5f : 10f) * (midDir - _apoc.Rotation).ToDirection() : reference;
        }
        else
        {
            // ranged spot
            var offset = (pos == 2 ? -15f : +15f).Degrees();
            return 19f * (midDir + offset).ToDirection();
        }
    }
}

sealed class P3DarkestDanceBait(BossModule module) : Components.GenericBaitAway(module, (uint)AID.DarkestDanceBait, centerAtTarget: true)
{
    private Actor? _source;
    private DateTime _activation;

    private static readonly AOEShapeCircle _shape = new(8);

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_source != null && Raid.WithoutSlot(false, true, true).Farthest(_source.Position) is var target && target != null)
        {
            CurrentBaits.Add(new(_source, target, _shape, _activation));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // we have dedicated components for this...

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DarkestDance)
        {
            ForbiddenPlayers = Raid.WithSlot(true, true, true).WhereActor(p => p.Role != Role.Tank).Mask();
            _source = caster;
            _activation = Module.CastFinishAt(spell, 0.4d);
        }
    }
}

sealed class P3DarkestDanceKnockback(BossModule module) : Components.GenericKnockback(module, (uint)AID.DarkestDanceKnockback, true)
{
    public Actor? Caster;
    public DateTime Activation;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (Caster != null)
            return new Knockback[1] { new(Caster.Position, 21, Activation) };
        return [];
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.DarkestDanceBait:
                Caster = caster;
                Activation = WorldState.FutureTime(2.8d);
                break;
            case (uint)AID.DarkestDanceKnockback:
                ++NumCasts;
                break;
        }
    }
}

// position for first dark water - note that this is somewhat arbitrary (range etc)
sealed class P3ApocalypseAIWater1(BossModule module) : BossComponent(module)
{
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private readonly P3ApocalypseDarkWater? _water = module.FindComponent<P3ApocalypseDarkWater>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_water == null)
            return;
        ref var state = ref _water.States[slot];
        if (state.AssignedGroup == 0)
            return; // no assignment (yet?)

        var (dir, range) = state.AssignedPosition switch
        {
            0 => (-15f.Degrees(), 5f),
            1 => (15f.Degrees(), 5f),
            2 => (-10f.Degrees(), 8f),
            3 => (10f.Degrees(), 8f),
            _ => (default, default)
        };
        dir += _config.P3ApocalypseDarkWater1ReferenceDirection.Degrees();
        if (state.AssignedGroup == 2)
            dir += 180f.Degrees();
        hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center + range * dir.ToDirection(), 1f), _water.Stacks.Count > 0 ? _water.Stacks[0].Activation : DateTime.MaxValue);
    }
}

// position for second dark water & darkest dance - for simplicity, we position in the direction tank would take darkest dance
sealed class P3ApocalypseAIWater2(BossModule module) : BossComponent(module)
{
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private readonly P3Apocalypse? _apoc = module.FindComponent<P3Apocalypse>();
    private readonly P3ApocalypseDarkWater? _water = module.FindComponent<P3ApocalypseDarkWater>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_apoc?.Starting == null || _water == null)
            return;

        // add imminent apoc aoes
        foreach (var aoe in _apoc.ActiveAOEs(slot, actor))
            hints.AddForbiddenZone(aoe.Shape.Distance(aoe.Origin, aoe.Rotation), aoe.Activation);

        ref var state = ref _water.States[slot];
        if (state.AssignedGroup == 0)
            return; // no assignments - oh well

        var midDir = (_apoc.Starting.Value - _apoc.Rotation).Normalized();

        // G1 takes dir CCW from N, G2 takes 0/45/90/135
        var midIsForG2 = midDir.Deg is >= -20 and < 160;
        if (midIsForG2 != (state.AssignedGroup == 2))
            midDir += 180.Degrees();

        var distance = 4.5f;
        if (_apoc.NumCasts >= 28 && assignment == (_config.P3DarkestDanceOTBait ? PartyRolesConfig.Assignment.OT : PartyRolesConfig.Assignment.MT))
        {
            // bait darkest dance (but make sure to share water first!)
            distance = _water.Stacks.Count == 0 ? 19 : 8;
        }

        var destOff = distance * (midDir - _apoc.Rotation).ToDirection();
        hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center + destOff, 1f), DateTime.MaxValue);
    }
}

// position for darkest dance knockback & third dark water
sealed class P3ApocalypseAIWater3(BossModule module) : BossComponent(module)
{
    private readonly P3ApocalypseDarkWater? _water = module.FindComponent<P3ApocalypseDarkWater>();
    private readonly P3DarkestDanceKnockback? _knockback = module.FindComponent<P3DarkestDanceKnockback>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_water == null || _knockback?.Caster == null)
            return;

        var toCenter = Arena.Center - _knockback.Caster.Position;
        if (toCenter.LengthSq() < 1)
            return; // did not jump yet, wait...

        var angle = 20f.Degrees(); //(_knockback.NumCasts == 0 ? 30 : 45).Degrees();
        var dir = Angle.FromDirection(toCenter) + _water.States[slot].AssignedGroup switch
        {
            1 => -angle,
            2 => angle,
            _ => default
        };

        if (_knockback.NumCasts == 0)
        {
            // preposition for knockback
            hints.AddForbiddenZone(ShapeDistance.PrecisePosition(_knockback.Caster.Position + 2f * dir.ToDirection(), new(0, 1), Arena.Bounds.MapResolution, actor.Position, 0.1f), _knockback.Activation);
        }
        else if (_water.Stacks.Count > 0)
        {
            // stack at maxmelee
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(_knockback.Caster.Position + 10f * dir.ToDirection(), 1f), _water.Stacks[0].Activation);
        }
    }
}
