namespace BossMod.Dawntrail.Ultimate.DMU;

class DoubleTroubleTrapStacks(BossModule module) : Components.UniformStackSpread(module, 6.0f, 0.0f, 4, 4)
{
    // Used for when we are interested in showing the mechanic, not really needed since we can just use the base Active variable, but helps with
    // handling the state machine and keeping the DoubleTroubleTrap components together so it is clear what is happening
    public bool resolving = false;
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();
    protected readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();
    public int NumCasts = 0;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.DoubleTroubleTrap)
        {
            var party = Raid.WithSlot(true, true, true);
            BitMask allowedSupports = default;
            BitMask allowedDDs = default;

            for (var i = 0; i < party.Length; ++i)
            {
                ref var p = ref party[i];

                if (p.Item2.Role is Role.Tank or Role.Healer)
                {
                    allowedSupports.Set(p.Item1);
                }

                if (p.Item2.Role is Role.Melee or Role.Ranged)
                {
                    allowedDDs.Set(p.Item1);
                }
            }

            if (actor.Role is Role.Tank or Role.Healer)
            {
                AddStack(actor, status.ExpireAt, ~allowedSupports);
            }

            if (actor.Role is Role.Melee or Role.Ranged)
            {
                AddStack(actor, status.ExpireAt, ~allowedDDs);
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.DoubleTroubleTrap)
        {
            Stacks.RemoveAt(0);
            NumCasts++;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!resolving || !Active)
        {
            return;
        }

        base.DrawArenaForeground(pcSlot, pc);

        if (!dmuConfig.P1DoubleTroubleKnockbackHints || dmuConfig.P1DoubleTrouble == DMUConfig.P1DoubleTroubleStrategy.DoubleTroubleNone)
        {
            return;
        }

        DrawForegroundHints(pcSlot, pc);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!resolving || !Active)
        {
            return;
        }

        base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!resolving || !Active)
        {
            return;
        }

        if (dmuConfig.P1DoubleTrouble == DMUConfig.P1DoubleTroubleStrategy.DoubleTroubleNone)
        {
            return;
        }

        WPos safeSpot = default;
        if (Stacks.Count == 0)
        {
            return;
        }
        var activation = Stacks[0].Activation;

        if (IsStackTarget(actor))
        {
            safeSpot = P1DoubleTroubleKnockBackData.StackSafeSpots.GetValueOrDefault(actor.Role).stackDebuff;
        }

        if (!IsStackTarget(actor))
        {
            safeSpot = P1DoubleTroubleKnockBackData.StackSafeSpots.GetValueOrDefault(actor.Role).stackHelper;
        }

        if (safeSpot == default)
        {
            return;
        }

        hints.AddForbiddenZone(new SDInvertedCircle(safeSpot, PositionAIRadius.PRECISE), activation);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (!resolving || !Active)
        {
            return PlayerPriority.Irrelevant;
        }

        return base.CalcPriority(pcSlot, pc, playerSlot, player, ref customColor);
    }

    // Used as we still want to use the base drawArenaForeground, but similar versions of this component will have to show different spots
    protected virtual void DrawForegroundHints(int pcSlot, Actor pc)
    {
        WPos safeSpot = default;

        if (IsStackTarget(pc))
        {
            safeSpot = P1DoubleTroubleKnockBackData.StackSafeSpots.GetValueOrDefault(pc.Role).stackDebuff;
        }

        if (!IsStackTarget(pc))
        {
            safeSpot = P1DoubleTroubleKnockBackData.StackSafeSpots.GetValueOrDefault(pc.Role).stackHelper;
        }

        if (safeSpot == default)
        {
            return;
        }

        Arena.ZoneCircleOutline(safeSpot, PositionDrawSize.NORMAL, Colors.Safe, 2.0f);
    }
}

// Knockback with distance 6 is different to other knockbacks, so we have to clear the pending knockbacks after a set amount of time so the player
// can move out of the active aoes after being knocked back across the map - currently they have around ~2.7 seconds to get out of the aoes
// Does the following:
//  1. Any player getting knocked back will have their pending knockbacks cleared after ~1.2 seconds (unless changed via the component)
//  2. Any player with the stack debuff will be forced to stand still until all pending knockbacks have been cleared
sealed class DoubleTroubleTrapKnockback(BossModule module) : Components.GenericKnockback(module)
{
    private readonly List<Knockback> knockbacks = [];
    private readonly DoubleTroubleTrapStacks? doubleTroubleTrapStacks = module.FindComponent<DoubleTroubleTrapStacks>();
    private DateTime activation;
    private const double knockbackResolveTimer = 1.0f; // Used to clear up the pending knockback list
    private BitMask debuffPlayers = default; // Tracks the players who have the stack debuffs as they shouldn't move as soon as it resolves

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.DoubleTroubleTrapStack)
        {
            var target = Raid.FindSlot(spell.MainTargetID);
            activation = WorldState.CurrentTime.AddSeconds(knockbackResolveTimer);
            debuffPlayers.Set(target);
            NumCasts++;
        }
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        knockbacks.Clear();

        if (doubleTroubleTrapStacks == null || !doubleTroubleTrapStacks.resolving)
        {
            return [];
        }

        foreach (var stack in doubleTroubleTrapStacks.Stacks)
        {
            if (actor.Position.InCircle(stack.Target.Position, stack.Radius))
            {
                knockbacks.Add(new(stack.Target.Position, 14f, stack.Activation));
            }
        }

        return CollectionsMarshal.AsSpan(knockbacks);
    }

    public override void Update()
    {
        // Once activation is set back to default clear the debuff player bitMask so they can move around
        if (activation == default)
        {
            debuffPlayers.Reset();
            return;
        }

        if (WorldState.CurrentTime <= activation)
        {
            return;
        }

        // Clears all the players pending knockbacks
        var party = Raid.WithoutSlot();
        for (var i = 0; i < party.Length; i++)
        {
            ref var p = ref party[i];
            p.PendingKnockbacks.Clear();
        }
        activation = default;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (debuffPlayers[slot])
        {
            hints.ForcedMovement = new(0);
        }
    }
}
