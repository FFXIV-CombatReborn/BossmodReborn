namespace BossMod.Dawntrail.Ultimate.DMU;

using Direction = P1TeleTrouncingData.Direction;

sealed class TeleTrouncing : Components.GenericAOEs
{
    private readonly record struct arrowDebuff(Direction direction, DateTime activation);
    private readonly List<List<arrowDebuff>> debuffs = [];
    private readonly List<List<(WPos safeSpot, DateTime activation)>> arrowHints = [];
    private readonly List<AOEInstance> arrows = [];
    private readonly AOEShapeCircle shape = new(2.0f);
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();

    public TeleTrouncing(BossModule module) : base(module)
    {
        for (int i = 0; i < PartyState.MaxPartySize; i++)
        {
            debuffs.Add([
                new arrowDebuff(),
                new arrowDebuff()
            ]);

            arrowHints.Add([]);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        Direction direction = (SID)status.ID switch
        {
            SID.TelePortentUP or SID.TelePortentUP2 => Direction.UP,
            SID.TelePortentDOWN or SID.TelePortentDOWN2 => Direction.DOWN,
            SID.TelePortentLEFT or SID.TelePortentLEFT2 => Direction.LEFT,
            SID.TelePortentRIGHT or SID.TelePortentRIGHT2 => Direction.RIGHT,
            _ => Direction.NONE
        };

        if (direction == Direction.NONE)
        {
            return;
        }

        if (Raid.FindSlot(actor.InstanceID) is var slot && slot < 0)
        {
            return;
        }

        var duration = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds;
        if (duration > 8)
        {
            debuffs[slot][1] = new arrowDebuff(direction, status.ExpireAt);
        }
        else
        {
            debuffs[slot][0] = new arrowDebuff(direction, status.ExpireAt);
        }

        var firstArrow = debuffs[slot][0];
        var secondArrow = debuffs[slot][1];

        if (firstArrow.direction == Direction.NONE || secondArrow.direction == Direction.NONE)
        {
            return;
        }

        if (!P1TeleTrouncingData.TryGetSafeSpots(dmuConfig.P1TeleTrouncing, firstArrow.direction, secondArrow.direction, out var safeSpots))
        {
            return;
        }

        if (safeSpots.arrow1.direction == firstArrow.direction)
        {
            arrowHints[slot].Add((safeSpots.arrow1.safeSpot, firstArrow.activation));
            arrowHints[slot].Add((safeSpots.arrow2.safeSpot, secondArrow.activation));
        }
        else
        {
            arrowHints[slot].Add((safeSpots.arrow2.safeSpot, firstArrow.activation));
            arrowHints[slot].Add((safeSpots.arrow1.safeSpot, secondArrow.activation));
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        Direction direction = (SID)status.ID switch
        {
            SID.TelePortentUP or SID.TelePortentUP2 => Direction.UP,
            SID.TelePortentDOWN or SID.TelePortentDOWN2 => Direction.DOWN,
            SID.TelePortentLEFT or SID.TelePortentLEFT2 => Direction.LEFT,
            SID.TelePortentRIGHT or SID.TelePortentRIGHT2 => Direction.RIGHT,
            _ => Direction.NONE
        };

        if (direction == Direction.NONE)
        {
            return;
        }

        if (Raid.FindSlot(actor.InstanceID) is var slot && slot < 0)
        {
            return;
        }

        if (arrowHints[slot].Count != 0)
        {
            arrowHints[slot].RemoveAt(0);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.TeleTrouncing1)
        {
            NumCasts++;
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.TeleTrouncingArrow)
        {
            arrows.Add(new(shape, actor.Position, actor.Rotation, actorID: actor.InstanceID));
        }
    }

    public override void OnActorEventStateChange(Actor actor, byte value)
    {
        if (actor.OID == (uint)OID.TeleTrouncingArrow && value == 7)
        {
            arrows.RemoveAll(aoe => aoe.ActorID == actor.InstanceID);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.TeleTrouncingNone)
        {
            return;
        }

        if (arrowHints[pcSlot].Count == 0)
        {
            return;
        }

        for (int i = 0; i < arrowHints[pcSlot].Count; i++)
        {
            Arena.ZoneCircleOutline(arrowHints[pcSlot][i].safeSpot, PositionDrawSize.NORMAL, i == 0 ? Colors.Safe : Colors.Danger, 2.0f);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);

        if (arrows.Count == 0)
        {
            return;
        }

        foreach (var arrow in arrows)
        {
            Arena.AddLine(arrow.Origin + arrow.Rotation.ToDirection(), arrow.Origin + (arrow.Rotation + 90.Degrees()).ToDirection(), Colors.Background);
            Arena.AddLine(arrow.Origin + arrow.Rotation.ToDirection(), arrow.Origin + (arrow.Rotation - 90.Degrees()).ToDirection(), Colors.Background);
            Arena.AddLine(arrow.Origin + arrow.Rotation.ToDirection(), arrow.Origin + (arrow.Rotation + 180.Degrees()).ToDirection(), Colors.Background);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.TeleTrouncingNone)
        {
            return;
        }

        if (arrowHints[slot].Count == 0 || debuffs[slot].Count == 0)
        {
            return;
        }

        hints.AddForbiddenZone(new SDInvertedCircle(arrowHints[slot][0].safeSpot, PositionAIRadius.PRECISE), arrowHints[slot][0].activation);
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(arrows);
}

sealed class TeleTrouncingTethers(BossModule module) : Components.UniformStackSpread(module, 5.0f, 5.0f)
{
    private readonly WPos tetherSleepSource = new WPos(107.000f, 43.000f);
    //private readonly WPos tetherConfusionSource = new WPos(95.000f, 25.000f); - keep for the position as it might be needed later on
    public bool? supportHasSleep = null;
    private static readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();
    private static readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();
    private readonly DoubleTroubleTrapStacksTeleTrouncing? doubleTroubleTrapStacksTeleTrouncing = module.FindComponent<DoubleTroubleTrapStacksTeleTrouncing>();
    public bool debuffActive = false; // There is a slight delay between the tethers being removed & applying the debuff, so we want for a player to actually get the debuff
    public BitMask debuffPlayers = default; // Used to count how many have the debuffs since players can die, and we don't want to exit the component early

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.Sleep or (uint)SID.Confused && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            debuffActive = true;
            debuffPlayers.Set(slot);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.Sleep or (uint)SID.Confused && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            debuffPlayers.Clear(slot);
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID != (uint)TetherID.GravenImageTether || Raid.FindSlot(tether.Target) is var slot && slot < 0)
        {
            return;
        }

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
        {
            return;
        }

        if (source.Position.AlmostEqual(tetherSleepSource, 5.0f))
        {
            supportHasSleep = target.Class.IsSupport();
            AddSpread(target, WorldState.FutureTime(5.7f));
            return;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.IdyllicWill)
        {
            Spreads.RemoveAll(actor => actor.Target.InstanceID == spell.MainTargetID);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.TeleTrouncingNone)
        {
            return;
        }

        if (!dmuConfig.P1TeleTrouncingStaticSpots && supportHasSleep == null)
        {
            return;
        }

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0)
        {
            return;
        }
        var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

        WPos safeSpot = default;
        P1TeleTrouncingTethersData.TryGetSafeSpots(dmuConfig.P1TeleTrouncing, assignment, dmuConfig.P1TeleTrouncingStaticSpots, supportHasSleep.GetValueOrDefault(), out safeSpot);
        if (safeSpot == default)
        {
            return;
        }

        if (supportHasSleep == null || debuffPlayers[pcSlot])
        {
            return;
        }

        Arena.ZoneCircleOutline(safeSpot, PositionDrawSize.NORMAL, Colors.Safe, 2.0f);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.TeleTrouncingNone)
        {
            return;
        }

        if (!dmuConfig.P1TeleTrouncingStaticSpots && supportHasSleep == null)
        {
            return;
        }

        // Case: if knockbacks are active, we resolve them first then move to our position
        if (doubleTroubleTrapStacksTeleTrouncing == null || doubleTroubleTrapStacksTeleTrouncing.Active)
        {
            return;
        }

        WPos safeSpot = default;
        P1TeleTrouncingTethersData.TryGetSafeSpots(dmuConfig.P1TeleTrouncing, assignment, dmuConfig.P1TeleTrouncingStaticSpots, supportHasSleep.GetValueOrDefault(), out safeSpot);
        if (safeSpot == default)
        {
            return;
        }

        if (supportHasSleep == null || debuffPlayers[slot])
        {
            return;
        }

        // There is a gap between the spread being removed & the debuff being applied
        var activation = Spreads.Count > 0 ? Spreads[0].Activation : default;
        hints.AddForbiddenZone(new SDInvertedCircle(safeSpot, PositionAIRadius.PRECISE), activation);
    }
}

sealed class DoubleTroubleTrapStacksTeleTrouncing : DoubleTroubleTrapStacks
{

    public DoubleTroubleTrapStacksTeleTrouncing(BossModule module) : base(module) { }

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
        DateTime activation = Stacks[0].Activation;

        if (IsStackTarget(actor))
        {
            safeSpot = P1DoubleTroubleKnockBackData.TeleTrouncingDoubleTroubleSpots.GetValueOrDefault(actor.Role).stackDebuff;
        }

        if (!IsStackTarget(actor))
        {
            safeSpot = P1DoubleTroubleKnockBackData.TeleTrouncingDoubleTroubleSpots.GetValueOrDefault(actor.Role).stackHelper;
        }

        if (safeSpot == default)
        {
            return;
        }

        hints.AddForbiddenZone(new SDInvertedCircle(safeSpot, PositionAIRadius.PRECISE), activation);
    }

    protected override void DrawForegroundHints(int pcSlot, Actor pc)
    {
        WPos safeSpot = default;

        if (IsStackTarget(pc))
        {
            safeSpot = P1DoubleTroubleKnockBackData.TeleTrouncingDoubleTroubleSpots.GetValueOrDefault(pc.Role).stackDebuff;
        }

        if (!IsStackTarget(pc))
        {
            safeSpot = P1DoubleTroubleKnockBackData.TeleTrouncingDoubleTroubleSpots.GetValueOrDefault(pc.Role).stackHelper;
        }

        if (safeSpot == default)
        {
            return;
        }

        Arena.ZoneCircleOutline(safeSpot, PositionDrawSize.NORMAL, Colors.Safe, 2.0f);
    }
}
