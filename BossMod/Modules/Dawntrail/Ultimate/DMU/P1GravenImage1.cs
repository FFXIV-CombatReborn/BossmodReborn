namespace BossMod.Dawntrail.Ultimate.DMU;

sealed class PulseWave(BossModule module) : Components.GenericKnockback(module, (uint)AID.PulseWave)
{
    public bool active = false;
    public DateTime activation;
    public const float KnockbackDistance = 13.0f;
    public BitMask affectedPlayers;
    public Actor? tetherSource = null;
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.GravenImageTether && Raid.FindSlot(tether.Target) is var slot && slot >= 0)
        {
            tetherSource = source;
            affectedPlayers[slot] = true;
            activation = WorldState.FutureTime(5.1f);
            active = true;
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.GravenImageTether && Raid.FindSlot(tether.Target) is var slot && slot >= 0)
        {
            affectedPlayers[slot] = false;
            NumCasts++;

            if (NumCasts == 4)
            {
                active = false;
            }
        }
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (affectedPlayers[slot] && tetherSource != null)
        {
            return new Knockback[1] { new(tetherSource.Position, KnockbackDistance, activation, ignoreImmunes: true) };
        }

        return [];
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!active)
        {
            return;
        }

        var spot = P1GravenImage1Data.PulseWavePrePositions.GetValueOrDefault(actor.Role);
        if (spot == default)
        {
            return;
        }

        // Case: Non-tethered players
        if (!affectedPlayers[slot])
        {
            hints.GoalZones.Add(AIHints.GoalProximity(spot.normal, PositionAIRadius.PRECISE, PositionWeights.PRE_POSITION));
        }

        // Case: Tethered players
        if (affectedPlayers[slot])
        {
            hints.GoalZones.Add(AIHints.GoalProximity(spot.tethered, PositionAIRadius.PRECISE, PositionWeights.PRE_POSITION));
        }
    }
}

// Custom version specially for GravenImage1 to make the player avoid going to the incorrect side as the pathfinder will override everything when the cast
// has <1.0f left, so this is needed to ensure the player move to their side only (left/right depending on the role)
sealed class BlizzardIIIBlowoutGravenImage1(BossModule module) : BlizzardIIIBlowout(module)
{
    public bool? supportNorth = null;
    public bool? dpsNorth = null;
    private DateTime activation;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action.ID is (uint)AID.BlizzardIIIBlowout or (uint)AID.BlizzardIIIBlowout1)
        {
            supportNorth = !Casters.Exists(c => c.Check(new WPos(89.000f, 89.000f)));
            dpsNorth = !Casters.Exists(c => c.Check(new WPos(111.000f, 89.000f)));
            activation = Module.CastFinishAt(spell);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Risky || activation == default)
        {
            return;
        }

        base.AddAIHints(slot, actor, assignment, hints);

        // Case: support players will avoid right side completely
        if (actor.Role is Role.Tank or Role.Healer)
        {
            hints.AddForbiddenZone(new SDCone(new WPos(100.0f, 100.0f), 100.0f, Angle.AnglesCardinals[2].ToDirection().OrthoR().ToAngle(), 90.0f.Degrees()),
                activation);
        }

        // Case: damage dealers will avoid left side completely
        if (actor.Role is Role.Melee or Role.Ranged)
        {
            hints.AddForbiddenZone(new SDCone(new WPos(100.0f, 100.0f), 100.0f, Angle.AnglesCardinals[2].ToDirection().OrthoL().ToAngle(), 90.0f.Degrees()),
                activation);
        }
    }
}

class FlagrantFire(BossModule module) : Components.UniformStackSpread(module, 6.0f, 5.0f, 4, 4)
{
    protected enum SpreadStack { None, Spread, Stack }
    private enum TellingTheTruth { Unknown, Yes, No }
    protected SpreadStack mechanic = SpreadStack.None;
    private TellingTheTruth tellingTheTruth = TellingTheTruth.Unknown;
    private readonly PulseWave? PulseWave = module.FindComponent<PulseWave>();
    private readonly BlizzardIIIBlowoutGravenImage1? blizzardSafeSpots = module.FindComponent<BlizzardIIIBlowoutGravenImage1>();
    protected readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();
    protected readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();
    protected DateTime activation;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID.P1SpreadIcon:
                mechanic = SpreadStack.Spread;
                activation = WorldState.FutureTime(5.8f);
                SolveMechanic();
                break;
            case IconID.P1StackIcon:
                mechanic = SpreadStack.Stack;
                activation = WorldState.FutureTime(5.8f);
                SolveMechanic();
                break;
            case IconID.FireRingBlueOrb:
                tellingTheTruth = TellingTheTruth.Yes;
                SolveMechanic();
                break;
            case IconID.FireRingQuestionMark:
                tellingTheTruth = TellingTheTruth.No;
                SolveMechanic();
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.FlagrantFireIIISpread or (uint)AID.FlagrantFireIIIStack)
        {
            if (Stacks.Count > 0 || Spreads.Count > 0)
            {
                Stacks.Clear();
                Spreads.Clear();
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        drawSafeSpots(pcSlot, pc);
    }

    // Handles drawing the safe spots for each player - can be overridden dependent on the specific versions (e.g. FlagrantFire 2 at the end of P1)
    protected virtual void drawSafeSpots(int pcSlot, Actor pc)
    {
        if (PulseWave == null || PulseWave.tetherSource == null)
        {
            return;
        }

        var safeSpot = SafeSpot(pcSlot, pc);
        if (safeSpot == default)
        {
            return;
        }

        if (PulseWave.affectedPlayers[pcSlot] && dmuConfig.P1GravenImage1KnockbackAdditionalHints)
        {
            Arena.ZoneCircleOutline(GetKnockbackPosition(PulseWave.tetherSource.Position, safeSpot), PositionDrawSize.NORMAL, Colors.Safe, 2.0f);
            Arena.ZoneCircleOutline(safeSpot, PositionDrawSize.NORMAL, Colors.Danger, 2.0f);
            return;
        }

        if (PulseWave.affectedPlayers[pcSlot] && !dmuConfig.P1GravenImage1KnockbackAdditionalHints)
        {
            Arena.ZoneCircleOutline(safeSpot, PositionDrawSize.NORMAL, Colors.Danger, 2.0f);
            return;
        }

        Arena.ZoneCircleOutline(safeSpot, PositionDrawSize.NORMAL, Colors.Safe, 2.0f);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        addAIHintSafeSpots(slot, actor, assignment, hints);
    }

    // Handles adding the basic component AI hints (used for specific versions that need the basic AI hints, but don't wanna go through FlagrantFire class)
    protected void addBasicAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
    }

    // Handles the pathfinder AI safe spots for each player - can be overridden dependent on the specific versions (e.g. FlagrantFire 2 at the end of P1)
    protected virtual void addAIHintSafeSpots(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (PulseWave == null || PulseWave.tetherSource == null)
        {
            return;
        }

        var safeSpot = SafeSpot(slot, actor);
        if (safeSpot == default)
        {
            return;
        }

        if (PulseWave.affectedPlayers[slot])
        {
            hints.AddForbiddenZone(new SDInvertedCircle(GetKnockbackPosition(PulseWave.tetherSource.Position, safeSpot), PositionAIRadius.PRECISE),
                PulseWave.activation);
            return;
        }

        hints.AddForbiddenZone(new SDInvertedCircle(safeSpot, PositionAIRadius.PRECISE), PulseWave.activation);
    }

    // Pulls the data spot for the player
    private WPos SafeSpot(int pcSlot, Actor pc)
    {
        if (dmuConfig.P1GravenImage1 == DMUConfig.P1GravenImage1Strategy.GravenImage1None)
        {
            return default;
        }

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0)
        {
            return default;
        }
        var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

        if (blizzardSafeSpots == null || blizzardSafeSpots.dpsNorth == null || blizzardSafeSpots.supportNorth == null)
        {
            return default;
        }

        var spots = mechanic switch
        {
            SpreadStack.Spread => P1GravenImage1Data.SpreadSafeSpots.GetValueOrDefault(assignment),
            SpreadStack.Stack => P1GravenImage1Data.StackSafeSpots.GetValueOrDefault(pc.Role),
            _ => default,
        };

        if (spots == default)
        {
            return default;
        }

        var northSafe = pc.Role is Role.Tank or Role.Healer ? blizzardSafeSpots.supportNorth : blizzardSafeSpots.dpsNorth;
        return northSafe.Value ? spots.north : spots.south;
    }

    // Used to correctly assign what the mechanic is out of all the possible cases
    private void SolveMechanic()
    {
        if (mechanic == SpreadStack.None || tellingTheTruth == TellingTheTruth.Unknown)
        {
            return;
        }

        switch (mechanic, tellingTheTruth)
        {
            case (SpreadStack.Spread, TellingTheTruth.Yes):
            case (SpreadStack.Stack, TellingTheTruth.No):
                mechanic = SpreadStack.Spread;
                break;
            case (SpreadStack.Stack, TellingTheTruth.Yes):
            case (SpreadStack.Spread, TellingTheTruth.No):
                mechanic = SpreadStack.Stack;
                break;
        }

        SetupSpreadStacks();
    }

    // Sets up the spread or stack baits
    private void SetupSpreadStacks()
    {
        if (mechanic == SpreadStack.Spread)
        {
            AddSpreads(Raid.WithoutSlot(true, true, true), activation);
            return;
        }

        if (mechanic == SpreadStack.Stack)
        {
            var party = Raid.WithSlot(true, true, true);
            BitMask allowedSupports = default;
            BitMask allowedDDs = default;

            for (int i = 0; i < party.Length; i++)
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

            var addedSupport = false;
            var addedDD = false;

            for (int i = 0; i < party.Length; i++)
            {
                ref var player = ref party[i];
                var p = player.Item2;

                if (p.IsDead)
                {
                    continue;
                }

                if (p.Role is Role.Tank or Role.Healer)
                {
                    if (!addedSupport)
                    {
                        AddStack(p, activation, ~allowedSupports);
                        addedSupport = true;
                    }
                }

                if (p.Role is Role.Melee or Role.Ranged)
                {
                    if (!addedDD)
                    {
                        AddStack(p, activation, ~allowedDDs);
                        addedDD = true;
                    }
                }
            }
        }
    }

    // Calculates the knockback position for the player to land as close as possible to the safe spot for the player
    private WPos GetKnockbackPosition(WPos tetherPosition, WPos safePosition)
    {
        var distance = safePosition - tetherPosition;
        var length = distance.Length();
        var position = length > PulseWave.KnockbackDistance ? tetherPosition + (length - PulseWave.KnockbackDistance) * distance.Normalized() : safePosition;
        var safetyRadius = Arena.Bounds.Radius - 1.0f; // 1.0f away from the edge of the map for safety
        return (position - Arena.Center).LengthSq() > safetyRadius * safetyRadius ? Arena.Center + safetyRadius * (position - Arena.Center).Normalized() : position;
    }
}
