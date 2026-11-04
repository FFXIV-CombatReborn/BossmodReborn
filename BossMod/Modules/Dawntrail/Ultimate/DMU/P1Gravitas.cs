namespace BossMod.Dawntrail.Ultimate.DMU;

sealed class Gravitas(BossModule module) : Components.UniformStackSpread(module, 5, 5, 4, 8) {
    private readonly WPos stackSource = new WPos(102.500f, 22.500f);
    private readonly WPos spreadSource = new WPos(126.000f, 41.500f);
    private readonly List<Spread> spreadsIncoming = [];
    private BitMask spreadTethers = default;
    public P1GravitasData.Side side = P1GravitasData.Side.NONE;
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();
    private readonly BlizzardIIIBlowout? blizzardIIIBlowout = module.FindComponent<BlizzardIIIBlowout>();
    private readonly GravitationalWave? gravitationalWave = module.FindComponent<GravitationalWave>();
    private WPos? cachedSafeSpot = null;

    public override void OnTethered(Actor source, in ActorTetherInfo tether) {
        if (tether.ID != (uint)TetherID.GravenImageTether || Raid.FindSlot(tether.Target) is var slot && slot < 0) {
            return;
        }

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null) {
            return;
        }

        if (source.Position.AlmostEqual(stackSource, 5.0f)) {
            AddStack(target, WorldState.FutureTime(6.5f));
            return;
        }

        if (source.Position.AlmostEqual(spreadSource, 5.0f)) {
            spreadsIncoming.Add(new(target, 5.0f, WorldState.FutureTime(10.6f)));
            spreadTethers.Set(slot);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.Gravitas && Stacks.Count > 0) {
            Stacks.RemoveAll(actor => actor.Target.InstanceID == spell.MainTargetID);

            if (Stacks.Count == 0) {
                Spreads.AddRange(spreadsIncoming);
                spreadsIncoming.Clear();
            }

            return;
        }

        if (spell.Action.ID == (uint)AID.Vitrophyre && Spreads.Count > 0) {
            Spreads.RemoveAll(actor => actor.Target.InstanceID ==  spell.MainTargetID);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        base.DrawArenaForeground(pcSlot, pc);

        if (dmuConfig.P1GravenImage2 == DMUConfig.P1GravenImage2Strategy.GravenImage2None) {
            return;
        }

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0) {
            return;
        }
        var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

        if (Stacks.Count != 0) {
            var safeSpot = getStackSafeSpot();
            if (safeSpot == default) {
                return;
            }

            Arena.ZoneCircleOutline(safeSpot, PositionDrawSize.NORMAL, Colors.Safe, 2.0f);
            return;
        }

        if (Spreads.Count != 0) {
            if (spreadTethers[pcSlot]) {
                var safeSpot = side == P1GravitasData.Side.NORTH
                    ? P1GravitasData.SpreadSafeSpots.GetValueOrDefault(assignment).north
                    : P1GravitasData.SpreadSafeSpots.GetValueOrDefault(assignment).south;
                if (safeSpot == default) {
                    return;
                }

                Arena.ZoneCircleOutline(safeSpot, PositionDrawSize.NORMAL, Colors.Safe, 2.0f);
                return;
            }

            Arena.ZoneCircleOutline(Module.Center, PositionDrawSize.NORMAL, Colors.Safe, 2.0f);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        if (dmuConfig.P1GravenImage2 == DMUConfig.P1GravenImage2Strategy.GravenImage2None) {
            return;
        }

        // Pre-position for stack
        if (!Active) {
            if (!P1GravitasData.TryGetPuddleSpot(dmuConfig.P1GravenImage2, side, out var safeSpot)) {
                return;
            }

            hints.GoalZones.Add(AIHints.GoalProximity(safeSpot, PositionAIRadius.NORMAL, PositionWeights.PRE_POSITION));
            return;
        }

        // Position for stack
        if (Stacks.Count != 0) {
            var safeSpot = getStackSafeSpot();
            if (safeSpot == default) {
                return;
            }

            hints.AddForbiddenZone(new SDInvertedCircle(safeSpot, PositionAIRadius.PRECISE), Stacks[0].Activation);
            return;
        }

        // Position for spreads
        if (Spreads.Count == 0) {
            return;
        }

        // Micro adjustment for spreads
        var remaining = (Spreads[0].Activation - WorldState.CurrentTime).TotalSeconds;
        if (remaining <= 1.0f) {
            base.AddAIHints(slot, actor, assignment, hints);
        }

        if (spreadTethers[slot]) {
            var safeSpot = side == P1GravitasData.Side.NORTH
                ? P1GravitasData.SpreadSafeSpots.GetValueOrDefault(assignment).north
                : P1GravitasData.SpreadSafeSpots.GetValueOrDefault(assignment).south;
            if (safeSpot == default) {
                return;
            }

            var puddles = Module.Enemies((uint)OID.GravitasPuddles);
            foreach (var puddle in puddles) {
                hints.AddForbiddenZone(new AOEShapeCircle(5.0f + SpreadRadius + ExtraAISpreadThreshold), puddle.Position);
            }

            // If micro adjustment is active, then the zone will become a goal zone rather than a forbidden zone, so the pathfinder can move anywhere
            // not forbidden if needed - this is mainly here if the puddles are placed slightly incorrect and still close to the boss, so the AI can still
            // try and adjust
            if (remaining <= 1.0f) {
                hints.GoalZones.Add(AIHints.GoalProximity(safeSpot, PositionAIRadius.PRECISE, PositionWeights.MECHANIC));
                return;
            }

            hints.AddForbiddenZone(new SDInvertedCircle(safeSpot, PositionAIRadius.NORMAL), Spreads[0].Activation);
            return;
        }

        // Players without a spread stand directly middle under boss
        if (remaining <= 1.0f) {
            hints.GoalZones.Add(AIHints.GoalProximity(Module.Center, PositionAIRadius.PRECISE, PositionWeights.MECHANIC));
            return;
        }

        hints.AddForbiddenZone(new SDInvertedCircle(Module.Center, PositionAIRadius.PRECISE), Spreads[0].Activation);
    }

    private WPos getStackSafeSpot() {
        if (!P1GravitasData.TryGetPuddleSpot(dmuConfig.P1GravenImage2, side, out var safeSpot)) {
            return default;
        }

        // If a blizzard blowout is being cast, we move the safeSpot 0.5f away from the aoe zone
        // The safespot will be cached instead of moving back to the original position after BlizzardBlowOut has finished - typically what players actually do
        if (blizzardIIIBlowout != null) {
            foreach (var caster in blizzardIIIBlowout.Casters) {

                // Check the cast is happening north
                if (!caster.Check(new WPos(89.000f, 89.000f)) && !caster.Check(new WPos(111.000f, 89.000f))) {
                    continue;
                }

                var tempSafeSpot = safeSpot + new WDir(-0.5f, 0);
                cachedSafeSpot = caster.Check(tempSafeSpot) ? safeSpot + new WDir(0.5f, 0) : tempSafeSpot;
            }
        }

        // The safespot will not move back to the orignal position after gravtiationalWave has finished, this is intended so casters don't have to re-position
        // again potentially losing a GCD - the safeSpot is 0.5f difference so they don't need to be dead center on the position
        if (gravitationalWave != null) {
            foreach (var aoe in gravitationalWave.aoes) {
                // Check which side the cast is happening
                var tempSafeSpot = safeSpot + new WDir(-0.5f, 0);
                cachedSafeSpot = aoe.Check(tempSafeSpot) ? safeSpot + new WDir(0.5f, 0) : tempSafeSpot;
            }
        }

        return cachedSafeSpot ?? safeSpot;
    }
}

sealed class GravitasPuddles : Components.PersistentInvertibleVoidzone {
    private static List<Actor> puddles = [];
    public IReadOnlyList<Actor> Puddles => puddles;
    private BitMask doubleTroublePlayers = default;
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();

    public GravitasPuddles(BossModule module) : base(module, 5.0f, GetVoidzones) {
        puddles = []; // To ensure no puddles exist upon loading the module
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status) {
        // Case: if we have already set two players as the debuff players we shouldn't set anymore. This prevents the issue of after the knockback goes off
        // new players getting the debuff and the pathfinder trying to update which puddles they should soak since there conditions has changed
        if (doubleTroublePlayers.NumSetBits() >= 2) {
            return;
        }

        if (status.ID == (uint)SID.DoubleTroubleTrap && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0) {
            doubleTroublePlayers.Set(slot);
        }
    }

    public override void OnActorCreated(Actor actor) {
        if (actor.OID == (uint)OID.GravitasPuddles) {
            puddles.Add(actor);
        }
    }

    public override void OnActorEAnim(Actor actor, uint state) {
        if (actor.OID == (uint)OID.GravitasPuddles && state == (uint)Animations.PuddleSoakReady) {
            InvertResolveAt = WorldState.CurrentTime;
        }

        if (actor.OID == (uint)OID.GravitasPuddles && state == (uint)Animations.PuddleExplosion) {
            if (puddles.Count > 0) {
                puddles.Remove(actor);
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        if (dmuConfig.P1DoubleTrouble == DMUConfig.P1DoubleTroubleStrategy.DoubleTroubleNone) {
            base.AddAIHints(slot, actor, assignment, hints);
            return;
        }

        // Once inverted, don't show hints until Knockback casts have gone off
        var knockbacks = Module.FindComponent<DoubleTroubleTrapStacksGravitas>();
        if (knockbacks != null && knockbacks.resolving && Inverted) {
            return;
        }

        // If not inverted make the puddles act as temporary obstacles
        if (!Inverted) {
            foreach (var puddle in Sources(Module)) {
                hints.TemporaryObstacles.Add(new SDCircle(puddle.Position, Shape.Radius));
            }

            base.AddAIHints(slot, actor, assignment, hints);
            return;
        }

        // Cases for which puddles the player should soak - setup like this so it can be changed later on for other strats if needed
        // true = soak north puddles
        var soakNorthPuddles = (actor.Class.IsSupport(), doubleTroublePlayers[slot]) switch {
            (false, false) => true, // DPS with no debuff -> takes north puddles
            (false, true) => false, // DPS with debuff -> takes south puddles
            (true, false) => false, // SUPPORT with no debuff -> takes south puddles
            (true, true) => true, // SUPPORT with debuff -> takes north puddles
        };

        var puddleGroups = new List<ShapeDistance>();
        foreach (var puddle in Sources(Module)) {
            var north = puddle.Position.Z < Module.Center.Z;
            if ((north && soakNorthPuddles) || (!north && !soakNorthPuddles)) {
                puddleGroups.Add(new SDInvertedCircle(puddle.Position, Shape.Radius));
            }
        }

        if (puddleGroups.Count == 0) {
            base.AddAIHints(slot, actor, assignment, hints);
            return;
        }

        hints.AddForbiddenZone(new SDUnion([..puddleGroups]), InvertResolveAt);
    }

    private static List<Actor> GetVoidzones(BossModule module) => puddles;
}

sealed class GravitationalWave(BossModule module) : Components.GenericAOEs(module) {
    private readonly AOEShapeRect shape = new(40.0f, 20.0f);
    public readonly List<AOEInstance> aoes = [];
    public bool Risky = false;
    public enum Sides { NONE, LEFT, RIGHT }
    public Sides side = Sides.NONE;

    public override void OnActorEAnim(Actor actor, uint state) {
        if (state == (uint)Animations.PulseOrbStart) {
            aoes.Add(new(shape, Arena.Center.Quantized(), (actor.OID == (uint)OID.YellowOrb ? 1f : -1f) * 90.Degrees()));

            if (actor.OID == (uint)OID.YellowOrb) {
                side = Sides.LEFT;
                return;
            }

            if (actor.OID == (uint)OID.PurpleOrb) {
                side = Sides.RIGHT;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID is (uint)AID.GravitationalWave or (uint)AID.IntemperateWill) {
            if (aoes.Count > 0) {
                NumCasts++;
                aoes.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        var count = aoes.Count;
        if (count == 0) {
            return [];
        }

        var incomingAOEs = CollectionsMarshal.AsSpan(aoes);
        for (var i = 0; i < count; i++) {
            ref var aoe = ref incomingAOEs[i];
            aoe.Risky = Risky;
        }

        return incomingAOEs;
    }
}

sealed class DoubleTroubleTrapStacksGravitas : DoubleTroubleTrapStacks {
    private readonly GravitationalWave? gravitationalWave;
    private readonly record struct safeSpots(WPos front, WPos back);
    private safeSpots? northSafeSpots;
    private safeSpots? southSafeSpots;

    public DoubleTroubleTrapStacksGravitas(BossModule module) : base(module) {
        gravitationalWave = module.FindComponent<GravitationalWave>();
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        if (!resolving || !Active)
            return;

        base.DrawArenaForeground(pcSlot, pc);

        if (!dmuConfig.P1DoubleTroubleKnockbackHints || dmuConfig.P1DoubleTrouble == DMUConfig.P1DoubleTroubleStrategy.DoubleTroubleNone) {
            return;
        }

        drawForegroundHints(pcSlot, pc);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        if (!resolving || !Active) {
            return;
        }

        if (dmuConfig.P1DoubleTrouble == DMUConfig.P1DoubleTroubleStrategy.DoubleTroubleNone) {
            return;
        }

        if (northSafeSpots == null || southSafeSpots == null) {
            setupSafeSpots();
            return;
        }

        WPos safeSpot = default;
        DateTime activation = Stacks[0].Activation;

        if (IsStackTarget(actor)) {
            if (actor.Class.IsSupport()) {
                safeSpot = northSafeSpots.Value.back;
            }

            if (actor.Class.IsDD()) {
                safeSpot = southSafeSpots.Value.back;
            }
        }

        if (!IsStackTarget(actor)) {
            if (actor.Class.IsSupport()) {
                safeSpot = northSafeSpots.Value.front;
            }

            if (actor.Class.IsDD()) {
                safeSpot = southSafeSpots.Value.front;
            }
        }

        if (safeSpot == default) {
            return;
        }

        safeSpot = adjustSafeSpot(safeSpot);
        hints.AddForbiddenZone(new SDInvertedCircle(safeSpot, PositionAIRadius.PRECISE), activation);
    }

    protected override void drawForegroundHints(int pcSlot, Actor pc) {
        if (northSafeSpots == null || southSafeSpots == null) {
            setupSafeSpots();
            return;
        }

        WPos safeSpot = default;

        if (IsStackTarget(pc)) {
            if (pc.Class.IsSupport()) {
                safeSpot = northSafeSpots.Value.back;
            }

            if (pc.Class.IsDD()) {
                safeSpot = southSafeSpots.Value.back;
            }
        }

        if (!IsStackTarget(pc)) {
            if (pc.Class.IsSupport()) {
                safeSpot = northSafeSpots.Value.front;
            }

            if (pc.Class.IsDD()) {
                safeSpot = southSafeSpots.Value.front;
            }
        }

        if (safeSpot == default) {
            return;
        }

        safeSpot = adjustSafeSpot(safeSpot);
        Arena.ZoneCircleOutline(safeSpot, PositionDrawSize.NORMAL, Colors.Safe, 2.0f);
    }

    // Handles checking which strategy to use for setting up the safe spots
    private void setupSafeSpots() {
        switch (dmuConfig.P1DoubleTrouble) {
            case DMUConfig.P1DoubleTroubleStrategy.DoubleTroubleLPDU:
                setupSafeSpotsLPDU();
                break;
            case DMUConfig.P1DoubleTroubleStrategy.DoubleTroubleKefkaBin:
                setupSafeSpotsKefkaBin();
                break;
        }
    }

    // Creates the safe spot hints of where people should stand to resolve the knockbacks. Caches the safe spots since it's a lot of working out that
    // only needs to run once.
    private void setupSafeSpotsLPDU() {
        if (gravitationalWave == null || gravitationalWave.side == GravitationalWave.Sides.NONE) {
            return;
        }
        var side = gravitationalWave.side;

        var puddleActors = Module.Enemies((uint)OID.GravitasPuddles);
        if (puddleActors.Count == 0) {
            return;
        }

        // Split puddles into north and south
        var northPuddles = new List<Actor>();
        var southPuddles = new List<Actor>();
        foreach (var puddle in puddleActors) {
            if (puddle.Position.Z < Module.Center.Z) {
                northPuddles.Add(puddle);
            } else {
                southPuddles.Add(puddle);
            }
        }

        const float puddleRadius = 5.0f;
        const float puddleDistance = 2.0f; // Distance we want to be away from the puddle
        const float backDistance = 1.5f; // Distance to put between the front safe spot & the back safe spot
        const float backTowardsPuddle = 2.0f; // Distance the back puddle goes towards the puddle to create the KB angle for people in front

        foreach (var puddles in new[] { northPuddles, southPuddles }) {
            if (puddles.Count == 0) {
                continue;
            }

            // Midpoint of the puddles for that zone since the puddles can be slightly off from each since they're placed by players
            var sum = new WDir();
            foreach (var puddle in puddles) {
                sum = sum + puddle.Position - Arena.Center;
            }
            var puddleCenter = Arena.Center + sum / puddles.Count;

            var toCenter = Angle.FromDirection(Arena.Center - puddleCenter);
            var directionClockwise = (toCenter + 45.0f.Degrees()).ToDirection();
            var directionCounterClockwise = (toCenter - 45.0f.Degrees()).ToDirection();
            var isEast = side == GravitationalWave.Sides.RIGHT;
            var frontDirection = directionClockwise.X >= directionCounterClockwise.X == isEast ? directionClockwise : directionCounterClockwise;

            // Looks at the frontDirection and if the puddle is too close, it will move it along a bit more that direction to avoid the puddle
            var spread = 0.0f;
            foreach (var puddle in puddles) {
                var reach = (puddle.Position - puddleCenter).Dot(frontDirection);
                if (reach > spread) {
                    spread = reach;
                }
            }

            var frontSafeSpot = puddleCenter + frontDirection * (spread + puddleRadius + puddleDistance);
            var backDirection = (frontSafeSpot - Arena.Center).Normalized();
            var puddleDirection = (puddleCenter - frontSafeSpot).Normalized();
            var backSafeSpot = frontSafeSpot + backDirection * (2 * backDistance) + puddleDirection * backTowardsPuddle;

            if (puddles == northPuddles) {
                northSafeSpots = new safeSpots(frontSafeSpot, backSafeSpot);
            } else {
                southSafeSpots = new safeSpots(frontSafeSpot, backSafeSpot);
            }
        }
    }

    // Similar to setupSafeSpotsLPDU but for a different strategy
    private void setupSafeSpotsKefkaBin() {
        var puddleActors = Module.Enemies((uint)OID.GravitasPuddles);
        if (puddleActors.Count == 0) {
            return;
        }

        // Split puddles into north and south
        var northPuddles = new List<Actor>();
        var southPuddles = new List<Actor>();
        foreach (var puddle in puddleActors) {
            if (puddle.Position.Z < Module.Center.Z) {
                northPuddles.Add(puddle);
            } else {
                southPuddles.Add(puddle);
            }
        }

        const float puddleRadius = 5.0f;
        const float puddleDistance = 1.0f; // Distance the debuff player stands in front of the puddle
        const float frontDistance = puddleDistance + 1.0f; // Distance the non-debuff players stand further in front of the debuff player

        foreach (var puddles in new[] { northPuddles, southPuddles }) {
            if (puddles.Count == 0) {
                continue;
            }

            // Midpoint of the puddles for that zone since the puddles can be slightly off from each since they're placed by players
            var sum = new WDir();
            foreach (var puddle in puddles) {
                sum = sum + puddle.Position - Arena.Center;
            }
            var puddleCenter = Arena.Center + sum / puddles.Count;
            var frontDirection = (Arena.Center - puddleCenter).Normalized();

            // Looks at the frontDirection and if the puddle is too close, it will move it along a bit more that direction to avoid the puddle
            var spread = 0.0f;
            foreach (var puddle in puddles) {
                var reach = (puddle.Position - puddleCenter).Dot(frontDirection);
                if (reach > spread) {
                    spread = reach;
                }
            }

            var debuffSafeSpot = puddleCenter + frontDirection * (spread + puddleRadius + puddleDistance);
            var nonDebuffSafeSpot = debuffSafeSpot + frontDirection * frontDistance;

            if (puddles == northPuddles) {
                northSafeSpots = new safeSpots(nonDebuffSafeSpot, debuffSafeSpot);
            } else {
                southSafeSpots = new safeSpots(nonDebuffSafeSpot, debuffSafeSpot);
            }
        }
    }

    // Handles moving the safe spot left / right if it's inside the GravitationalWave aoe so its easier to see for some strats
    private WPos adjustSafeSpot(WPos safeSpot) {
        if (gravitationalWave == null || gravitationalWave.aoes.Count == 0) {
            return safeSpot;
        }

        var aoe = gravitationalWave.aoes[0];
        var leftSpot = safeSpot + new WDir(-1.0f, 0);
        var rightSpot = safeSpot + new WDir(1.0f, 0);

        if (aoe.Check(leftSpot)) {
            return rightSpot;
        }

        if (aoe.Check(rightSpot)) {
            return leftSpot;
        }

        return safeSpot;
    }
}
