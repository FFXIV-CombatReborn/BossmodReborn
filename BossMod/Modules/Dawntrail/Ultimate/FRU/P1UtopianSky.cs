namespace BossMod.Dawntrail.Ultimate.FRU;

sealed class P1UtopianSkyBlastingZone(BossModule module) : Components.GenericAOEs(module, (uint)AID.BlastingZoneAOE)
{
    public readonly List<AOEInstance> AOEs = [];
    public BitMask DangerousSpots; // 0 = N, then CCW
    public DateTime Activation = DateTime.MaxValue;

    private static readonly AOEShapeRect _shape = new(50f, 8f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // micro adjust when activation is imminent; before that we have dedicated ai component
        if (WorldState.FutureTime(1d) > Activation)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.FatebreakersImage && modelState == 4)
        {
            Activation = WorldState.FutureTime(9.1d);
            AOEs.Add(new(_shape, actor.Position, actor.Rotation, Activation));
            DangerousSpots.Set((int)MathF.Round((-Angle.FromDirection(actor.Position - Arena.Center).Deg + 180f) / 45f) & 7);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            AOEs.Clear();
        }
    }
}

sealed class P1UtopianSkySpreadStack(BossModule module) : Components.UniformStackSpread(module, 6f, 5f, 4, 4)
{
    public enum Mechanic { None, Spread, Stack }

    public Mechanic CurMechanic;
    public DateTime Activation = DateTime.MaxValue;

    public void Show(DateTime activation)
    {
        Activation = activation;
        ExtraAISpreadThreshold = 0;
        switch (CurMechanic)
        {
            case Mechanic.Stack:
                // TODO: this can target either tanks or healers
                AddStacks(Raid.WithoutSlot(true, true, true).Where(p => p.Role == Role.Healer), activation);
                break;
            case Mechanic.Spread:
                AddSpreads(Raid.WithoutSlot(true, true, true), activation);
                break;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurMechanic != Mechanic.None)
            hints.Add($"Next: {CurMechanic}");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // micro adjust when activation is imminent; before that we have dedicated ai component
        if (WorldState.FutureTime(1.5d) > Activation)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        CurMechanic = spell.Action.ID switch
        {
            (uint)AID.UtopianSkyStack => Mechanic.Stack,
            (uint)AID.UtopianSkySpread => Mechanic.Spread,
            _ => CurMechanic
        };
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.SinboundFire:
                Stacks.Clear();
                break;
            case (uint)AID.SinboundThunder:
                Spreads.Clear();
                break;
        }
    }
}

// initial positions: resolve tankbuster + 'see' own image
sealed class P1UtopianSkyAIInitial(BossModule module) : BossComponent(module)
{
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.AddForbiddenZone(new SDCircle(Arena.Center, 18f)); // stay on edge

        var clockspot = _config.P1UtopianSkyInitialSpots[assignment];
        if (clockspot >= 0)
        {
            // ... and in assigned cone
            var assignedDirection = (180f - 45f * clockspot).Degrees();
            hints.AddForbiddenZone(new SDInvertedCone(Arena.Center, 50f, assignedDirection, 5f.Degrees()), DateTime.MaxValue);
        }
    }
}

// note: here we emulate 'natural' resolve:
// - if there is no aoe on our clockspot, we stay at starting spot
// - if our clone starts aoe, we move to the center immediately
// - if opposite clone starts aoe, we move to the center as soon as our opposite partner moves
// - as soon as all aoe directions have at least one person 'near center', or if resolve is imminent, we move to our final spot
sealed class P1UtopianSkyAIResolve(BossModule module) : BossComponent(module)
{
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private readonly P1UtopianSkyBlastingZone? _aoes = module.FindComponent<P1UtopianSkyBlastingZone>();
    private readonly P1UtopianSkySpreadStack? _spreadStack = module.FindComponent<P1UtopianSkySpreadStack>();
    private BitMask _seenDangerSpot;

    public override void Update()
    {
        if (_aoes == null || _seenDangerSpot.NumSetBits() >= 3)
            return;

        var folded = _aoes.DangerousSpots | (_aoes.DangerousSpots >> 4);
        if (WorldState.FutureTime(6d) > _aoes.Activation)
        {
            // can't wait any longer for people to think...
            _seenDangerSpot = folded & new BitMask(0xF);
            return;
        }

        foreach (var (slot, group) in _config.P1UtopianSkyInitialSpots.Resolve(Raid))
        {
            var spot = group & 3;
            if (folded[spot] && !_seenDangerSpot[spot] && Raid[slot] is var p && p != null && !p.Position.InDonutCone(Arena.Center, 12f, 20f, (180f - 45f * group).Degrees(), 30f.Degrees()))
                _seenDangerSpot.Set(spot);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var clockSpot = _config.P1UtopianSkyInitialSpots[assignment];
        if (_aoes == null)
            return;

        var numDangerSpots = _seenDangerSpot.NumSetBits();
        if (numDangerSpots < 3)
        {
            if (clockSpot >= 0 && (_aoes.DangerousSpots[clockSpot] || _seenDangerSpot[clockSpot & 3]))
            {
                // our spot is dangerous, or our partner's is and he has moved - move to center
                hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center, 5f), _aoes.Activation);
            }
            // else: we don't have a reason to move, stay where we are...
        }
        else if (numDangerSpots == 3/* && WorldState.FutureTime(1) <= _aoes.Activation*/)
        {
            // ok, we know where to go - move
            var spreadSpot = _config.P1UtopianSkySpreadSpots[assignment];
            if (_spreadStack?.Stacks.Count > 0)
                spreadSpot &= 4; // stack on close spot

            var direction = (4 - (~_seenDangerSpot).LowestSetBit()) % 4 * 45f.Degrees() + (spreadSpot >= 4 ? 0f : 180f).Degrees();
            spreadSpot &= 3;
            direction += spreadSpot switch
            {
                2 => -18f.Degrees(),
                3 => 18f.Degrees(),
                _ => default
            };
            var range = spreadSpot == 0 ? 13 : 19;
            hints.PathfindMapBounds = FRU.PathfindHugBorderBounds;
            hints.AddForbiddenZone(new SDPrecisePosition(Arena.Center + range * direction.ToDirection(), new(0, 1), Arena.Bounds.MapResolution, actor.Position, 0.1f), _aoes.Activation);
        }
    }
}
