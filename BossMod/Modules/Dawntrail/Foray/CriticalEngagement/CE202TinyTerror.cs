namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE202TinyTerror;

public enum OID : uint {
    TinyMage = 0x4C6D,
    Helper = 0x233C,
    TinyMageHelper = 0x4D55, // R1.000, x1
    TinyApprentice = 0x4C6E, // R1.000, x0 (spawn during fight)
    ArcaneSphereSmall = 0x4C74, // R1.000, x0 (spawn during fight)
    ArcaneSphereBig = 0x4C73, // R1.000, x0 (spawn during fight)
    FlareSphereGrow = 0x4C6F, // R0.700-1.904, x0 (spawn during fight)
    FlareSphere = 0x4C70, // R1.400, x0 (spawn during fight)
    HolySphere1Grow = 0x4C71, // R0.700-1.904, x0 (spawn during fight)
    HolySphere = 0x4C72, // R1.400, x0 (spawn during fight)
    _Gen_Actor1ea1a1 = 0x1EA1A1, // R2.000, x2, EventObj type
    _Gen_Actor1ec099 = 0x1EC099, // R0.500, x1, EventObj type
    _Gen_ = 0x4EBB, // R1.750, x0 (spawn during fight)
}

public enum AID : uint {
    ElectricBoundary = 0xBFA1, // TinyMageHelper, persistent outer deathwall; observed hits at 18.1-24.4y
    AutoAttack = 48305, // TinyMage->player, no cast, single-target
    TinyWarp = 48331, // TinyMage->location, no cast, single-target
    TinyThunderIIIRaidwide = 48329, // TinyMage->self, 5.0s cast, single-target
    TinyThunderIII = 48330, // Helper->self, no cast, ???

    TinyQuakeIII = 48322, // TinyMage->self, 3.5+0.5s cast, single-target
    TinyQuakeIIIInner = 48323, // Helper->self, 4.0s cast, range 10 circle
    TinyQuakeIIIMiddle = 48324, // Helper->self, 4.0s cast, range 10-20 donut
    TinyQuakeIIIOuter = 48325, // Helper->self, 4.0s cast, range 20-30 donut

    DiminutiveDualcast = 48317, // TinyMage->self, 5.5+0.5s cast, single-target
    TinyBlizzardIII = 48319, // Helper->self, 6.0s cast, range 40 60.000-degree cone
    TinyFireIII = 48318, // Helper->self, 6.0s cast, range 14 circle

    TinyMeteorCast = 48320, // TinyMage->self, 5.0s cast, single-target
    TinyMeteor = 48321, // Helper->location, 4.0s cast, range 6 circle

    Comet = 48327, // 4C74->self, 60.0s cast, range 60 circle
    Comet1 = 49061, // Helper->self, no cast, ???
    Meteor = 48326, // 4C73->self, 130.0s cast, single-target

    SmallForOne = 48306, // TinyMage->self, 3.0s cast, single-target - Spawns actors in

    TinyFlare = 48313, // 4C6F/4C70->self, no cast, single-target
    TinyFlare1 = 48311, // Helper->self, 2.0s cast, range 18 circle
    TinyHoly = 48314, // 4C72/4C71->self, no cast, single-target
    TinyHoly1 = 48312, // Helper->self, 2.0s cast, range 50 circle
    TinyHoly2 = 49058, // Helper->self, no cast, ???

    _Spell_Recharge = 48309, // 4C6E->self, 1.5s cast, single-target
    _Spell_Recharge1 = 48310, // 4C6E->self, 1.5s cast, single-target
    _Ability_Recharge = 49059, // 4C6E/TinyMage->self, no cast, single-target

    _Ability_ = 49057, // 4D55->self, no cast, range ?-25 donut
    _Ability_1 = 50530, // 4C6E->self, no cast, single-target
    _Spell_ = 50638, // 4C6E->self, no cast, single-target

    _Spell_ArcaneAggregation = 48307, // 4C6E->self, 3.0s cast, single-target
    _Spell_ArcaneAggregation1 = 49718, // 4C6E->self, 5.5s cast, single-target
    _Spell_ArcaneAggregation2 = 49719, // 4C6E->self, 5.5s cast, single-target
    _Spell_ArcaneAggregation3 = 48308, // 4C6E->self, 3.0s cast, single-target

    _Ability_AllForOne = 50762, // TinyMage->self, 3.0s cast, single-target
}

public enum SID : uint {
    _Gen_1 = 2552, // none->4C6E, extra=0x198
    _Gen_2 = 3445, // none->4C74/4C73, extra=0x15/0xA/0x1E
}

public enum TetherID : uint {
    OrbPairs = 415, // 4C72/4C70->4C72/4C70 - change name its when the orbs fire / white merge together
    _Gen_Tether_chn_m0012af = 60, // 4C74->4EBB
    CometTethers = 422, // 4C6E/TinyMage->4C74/4EBB
}

sealed class TinyThunderIII(BossModule module) : Components.RaidwideCast(module, (uint)AID.TinyThunderIIIRaidwide);

// The center controller pulses throughout the encounter. Eleven recorded damage events place the
// dangerous band at 18.1-24.4y from center, so show the outer two yalms inside the nominal arena.
sealed class ElectricBoundary(BossModule module) : Components.GenericAOEs(module) {
    private static readonly AOEShapeDonut Shape = new(18f, 25f);
    private readonly AOEInstance[] aoe = [new(Shape, module.Arena.Center)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => aoe;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        // The 18-25 donut is mostly clipped by the r20 arena; draw a visible yellow 19-20 band
        // plus the fence outline so the kill ring reads clearly.
        Arena.ZoneDonut(Arena.Center, 19f, 20f, Colors.Danger);
        Arena.ZoneCircleOutlineUnclipped(Arena.Center, 20f, Colors.Danger, 3f);
    }
}

sealed class TinyQuake(BossModule module) : ReplayValidatedCastAOEs(module) {
    private static readonly AOEShapeCircle Inner = new(10.0f);
    private static readonly AOEShapeDonut Middle = new(10.0f, 20.0f);
    private static readonly AOEShapeDonut Outer = new(20.0f, 30.0f);

    protected override int MaxDisplayed => 2;
    protected override int MaxRisky => 1;

    protected override AOEConfig? ConfigFor(uint actionID) => actionID switch {
        (uint)AID.TinyQuakeIIIInner => new(Inner),
        (uint)AID.TinyQuakeIIIMiddle => new(Middle),
        (uint)AID.TinyQuakeIIIOuter => new(Outer),
        _ => null
    };
}

sealed class DiminutiveDualcast(BossModule module) : ReplayValidatedCastAOEs(module) {
    private static readonly AOEShapeCone Blizzard = new(40.0f, 30.0f.Degrees());
    private static readonly AOEShapeCircle Fire = new(14.0f);

    // A complete dualcast wave contains two groups of three cones plus one fire circle.
    // All seven overlap before the first group resolves, so limiting the queue to four drops
    // the second cone group entirely.
    protected override int MaxDisplayed => 7;
    // The sequence resolves as three cones, then the fire circle, then three more cones at roughly
    // two-second intervals. Display all seven, but only mark the current cones plus the fire risky;
    // otherwise both cone groups combine into a false whole-arena danger zone.
    protected override double RiskyActivationWindow => 2.5d;

    protected override AOEConfig? ConfigFor(uint actionID) => actionID switch {
        (uint)AID.TinyBlizzardIII => new(Blizzard),
        (uint)AID.TinyFireIII => new(Fire),
        _ => null
    };

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        var aoes = ActiveAOEs(slot, actor).ToArray();
        foreach (ref readonly var aoe in aoes.AsSpan()) {
            if (!aoe.Risky) {
                continue;
            }

            // BCBE is a 14y circle centered on the boss. Treat it as forbidden from cast start;
            // otherwise pathfinding can enter it for uptime and schedule the escape too late.
            var activation = ReferenceEquals(aoe.Shape, Fire) ? WorldState.CurrentTime : aoe.Activation;
            hints.AddForbiddenZone(aoe.ShapeDistance ?? aoe.Shape.Distance(aoe.Origin, aoe.Rotation), activation);
        }

        // 引导 AI 到当前和未来都安全的位置: 只躲 risky 会让 AI 站进第二组扇形(左右互搏)。
        hints.GoalZones.Add(position =>
        {
            foreach (ref readonly var aoe in aoes.AsSpan())
            {
                var sd = aoe.ShapeDistance ?? aoe.Shape.Distance(aoe.Origin, aoe.Rotation);
                if (sd.Distance(position) <= 0)
                    return 0f;
            }
            return 5f;
        });
    }
}

sealed class TinyMeteor(BossModule module) : ReplayValidatedCastAOEs(module) {
    private static readonly AOEShapeCircle Shape = new(6.0f);

    protected override double RiskyActivationWindow => 0.8d;

    protected override AOEConfig? ConfigFor(uint actionID) => actionID == (uint)AID.TinyMeteor ? new(Shape, true) : null;
}

// TODO add timers - its not 60 seconds since it goes faster depending on the number of actors around it
// TODO _Gen_2 = 3445, // none->4C74/4C73, extra=0x15/0xA/0x1E
sealed class Comet(BossModule module) : BossComponent(module) {
    private readonly Dictionary<ulong, CometActor> comets = [];
    private readonly HashSet<TetherLink> activeTethers = [];

    private sealed class CometActor(Actor actor) {
        public readonly Actor Actor = actor;
        public int Tethers;
    }

    private readonly record struct TetherLink(ulong First, ulong Second);

    private static TetherLink MakeLink(ulong first, ulong second) {
        return first < second ? new(first, second) : new(second, first);
    }

    public override void OnActorCreated(Actor actor) {
        if (actor.OID == (uint)OID.ArcaneSphereSmall) {
            if (comets.Count == 0) {
                activeTethers.Clear();
            }
            comets.TryAdd(actor.InstanceID, new(actor));
        }
    }

    public override void OnActorDeath(Actor actor) {
        RemoveActor(actor.InstanceID);
    }

    public override void OnActorDestroyed(Actor actor) {
        RemoveActor(actor.InstanceID);
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether) {
        if (tether.ID != (uint)TetherID.CometTethers) {
            return;
        }

        var comet = FindComet(source.InstanceID, tether.Target);
        var link = MakeLink(source.InstanceID, tether.Target);
        if (comet != null && activeTethers.Add(link)) {
            ++comet.Tethers;
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether) {
        if (tether.ID != (uint)TetherID.CometTethers) {
            return;
        }

        var link = MakeLink(source.InstanceID, tether.Target);
        if (activeTethers.Remove(link)) {
            var comet = FindComet(source.InstanceID, tether.Target);
            if (comet != null) {
                comet.Tethers = Math.Max(0, comet.Tethers - 1);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        var maxTethers = MaxTetherCount();
        if (maxTethers <= 0) {
            return;
        }

        // The comet's nominal cast reads ~60s but it detonates far sooner once tethered (replay:
        // cast start +20s). The blast is a 60y circle covering the whole r20 arena, so the only
        // counter is killing the tethered comet; outline the blast radius around the kill target
        // so players see how urgent it is to focus it down.
        foreach (var comet in comets.Values) {
            if (comet.Tethers == maxTethers) {
                Arena.ZoneCircleOutline(comet.Actor.Position, 2.0f, Colors.Safe, 2.0f);
                Arena.ZoneCircleOutline(comet.Actor.Position, 60.0f, Colors.Danger, 1.0f);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) {
        if (MaxTetherCount() > 0) {
            hints.Add("Attack a comet with a green circle around it!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        var maxTethers = MaxTetherCount();
        if (maxTethers <= 0) {
            return;
        }

        foreach (var target in hints.PotentialTargets) {
            if (target.Actor.OID == (uint)OID.ArcaneSphereSmall
                && comets.GetValueOrDefault(target.Actor.InstanceID)?.Tethers == maxTethers) {
                target.Priority = 2;
            }
        }
    }

    private CometActor? FindComet(ulong first, ulong second) {
        if (comets.TryGetValue(first, out var comet)) {
            return comet;
        }
        return comets.GetValueOrDefault(second);
    }

    private int MaxTetherCount() {
        var max = 0;
        foreach (var comet in comets.Values) {
            max = Math.Max(max, comet.Tethers);
        }
        return max;
    }

    private void RemoveActor(ulong instanceID) {
        activeTethers.RemoveWhere(link => {
            if (link.First != instanceID && link.Second != instanceID) {
                return false;
            }

            var other = link.First == instanceID ? link.Second : link.First;
            if (comets.TryGetValue(other, out var comet)) {
                comet.Tethers = Math.Max(0, comet.Tethers - 1);
            }
            return true;
        });
        comets.Remove(instanceID);
        if (comets.Count == 0) {
            activeTethers.Clear();
        }
    }
}

static class TinyMageMechanic {
    private const float DirectionThreshold = 0.05f;

    // Replay-verified: the growing orb resolves 15.4-15.6s after it spawns (27.0->42.5,
    // 174.2->189.6, 241.6->256.8), so both growable components share this delay.
    public const double GrowResolveDelay = 15.5d;
    public const double ResolveVisualHold = 0.9d;

    public static List<Actor> ExistingMages(BossModule module) {
        var result = new List<Actor>(4);
        foreach (var mage in module.Enemies((uint)OID.TinyApprentice)) {
            if (!mage.IsDeadOrDestroyed) {
                AddMage(result, mage, module.Arena.Center);
            }
        }
        return result;
    }

    public static void AddMage(List<Actor> mages, Actor actor, WPos center) {
        if (mages.Any(mage => mage.InstanceID == actor.InstanceID)) {
            return;
        }

        // Small for One normally resets the wave; this is a fallback for a missed cast-start event.
        if (mages.Count >= 4) {
            mages.Clear();
        }

        mages.Add(actor);
        mages.Sort((left, right) => {
            var result = ClockwiseFromNorth(left.Position, center).CompareTo(ClockwiseFromNorth(right.Position, center));
            return result != 0 ? result : left.InstanceID.CompareTo(right.InstanceID);
        });
    }

    public static void RemoveMage(List<Actor> mages, ulong instanceID) {
        mages.RemoveAll(mage => mage.InstanceID == instanceID);
    }

    public static int ObserveDirection(Actor orb, WPos center, ref float previousAngle, ref float angularTravel) {
        var currentAngle = ClockwiseFromNorth(orb.Position, center);
        var delta = currentAngle - previousAngle;
        if (delta > MathF.PI) {
            delta -= Angle.DoublePI;
        } else if (delta < -MathF.PI) {
            delta += Angle.DoublePI;
        }

        previousAngle = currentAngle;
        angularTravel += delta;
        return angularTravel > DirectionThreshold ? 1 : angularTravel < -DirectionThreshold ? -1 : 0;
    }

    public static float AngleFromNorth(WPos position, WPos center) => ClockwiseFromNorth(position, center);

    private static float ClockwiseFromNorth(WPos position, WPos center) {
        var angle = (position - center).ToAngle().Normalized().Rad;
        var result = (MathF.PI - angle) % Angle.DoublePI;
        return result < 0.0f ? result + Angle.DoublePI : result;
    }
}

sealed class FlareGrowable(BossModule module) : Components.GenericAOEs(module) {
    private static readonly AOEShapeCircle Shape = new(18.0f);
    private readonly List<Actor> mages = TinyMageMechanic.ExistingMages(module);
    private Actor? orb;
    private WPos? start;
    private WPos? predictedTarget;
    private WPos? directTarget;
    private ulong startActorID;
    private DateTime activation;
    private DateTime clearAt;
    private float previousAngle;
    private float angularTravel;
    private int direction;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.SmallForOne) {
            ResetWave();
        } else if (spell.Action.ID == (uint)AID.TinyFlare1 && !spell.EventHappened) {
            // The resolving helper is authoritative and is also available when apprentice/orb
            // spawn or movement packets were missed. This prevents an entire fire wave from
            // disappearing merely because the four-actor reconstruction was incomplete.
            directTarget = caster.Position;
            activation = Module.CastFinishAt(spell);
            clearAt = default;
        }
    }

    public override void OnActorCreated(Actor actor) {
        if (actor.OID == (uint)OID.TinyApprentice) {
            TinyMageMechanic.AddMage(mages, actor, Arena.Center);
        } else if (actor.OID == (uint)OID.FlareSphereGrow) {
            StartOrb(actor);
        }
    }

    public override void Update() {
        RecoverExistingActors();
        if (clearAt != default && WorldState.CurrentTime >= clearAt) {
            ClearStart();
            return;
        }
        if (orb != null && direction == 0) {
            direction = TinyMageMechanic.ObserveDirection(orb, Arena.Center, ref previousAngle, ref angularTravel);
        }
        UpdatePredictedTarget();
    }

    public override void OnActorDeath(Actor actor) {
        RemoveActor(actor);
    }

    public override void OnActorDestroyed(Actor actor) {
        RemoveActor(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.TinyFlare1) {
            // The damage packet precedes the visible floor telegraph disappearing. Retain the
            // forbidden circle briefly so automation does not immediately walk into the fading VFX.
            clearAt = WorldState.FutureTime(TinyMageMechanic.ResolveVisualHold);
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        RecoverExistingActors();
        if (directTarget is { } resolved) {
            return new AOEInstance[1] { new(Shape, resolved, activation: activation) };
        }
        UpdatePredictedTarget();
        return predictedTarget is { } predicted ? new AOEInstance[1] { new(Shape, predicted, activation: activation) } : [];
    }

    private void RemoveActor(Actor actor) {
        TinyMageMechanic.RemoveMage(mages, actor.InstanceID);
        if (actor.InstanceID == startActorID) {
            clearAt = WorldState.FutureTime(TinyMageMechanic.ResolveVisualHold);
        }
    }

    private void RecoverExistingActors() {
        foreach (var mage in Module.Enemies((uint)OID.TinyApprentice)) {
            if (!mage.IsDeadOrDestroyed) {
                TinyMageMechanic.AddMage(mages, mage, Arena.Center);
            }
        }
        if (orb == null) {
            var existing = Module.Enemies((uint)OID.FlareSphereGrow).FirstOrDefault(actor => !actor.IsDeadOrDestroyed);
            if (existing != null) {
                StartOrb(existing);
            }
        }
    }

    private void StartOrb(Actor actor) {
        orb = actor;
        start = actor.Position;
        predictedTarget = null;
        startActorID = actor.InstanceID;
        activation = WorldState.FutureTime(TinyMageMechanic.GrowResolveDelay);
        clearAt = default;
        previousAngle = TinyMageMechanic.AngleFromNorth(actor.Position, Arena.Center);
        angularTravel = default;
        direction = default;
    }

    private void ClearStart() {
        orb = null;
        start = null;
        predictedTarget = null;
        directTarget = null;
        startActorID = default;
        activation = default;
        clearAt = default;
        previousAngle = default;
        angularTravel = default;
        direction = default;
    }

    private void UpdatePredictedTarget() {
        if (mages.Count != 4 || start == null || direction == 0) {
            return;
        }

        var startAOE = mages.FindIndex(mage => mage.Position.AlmostEqual(start.Value, 0.5f));
        if (startAOE >= 0) {
            predictedTarget = mages[(startAOE + mages.Count - direction) % mages.Count].Position;
        }
    }

    private void ResetWave() {
        mages.Clear();
        ClearStart();
    }
}

sealed class HolyGrowable(BossModule module) : Components.GenericKnockback(module) {
    private readonly List<Actor> mages = TinyMageMechanic.ExistingMages(module);
    private Actor? orb;
    private WPos? start;
    private WPos? predictedTarget;
    private WPos? directTarget;
    private ulong startActorID;
    private DateTime activation;
    private DateTime clearAt;
    private float previousAngle;
    private float angularTravel;
    private int direction;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.SmallForOne) {
            ResetWave();
        } else if (spell.Action.ID == (uint)AID.TinyHoly1 && !spell.EventHappened) {
            directTarget = caster.Position;
            activation = Module.CastFinishAt(spell);
            clearAt = default;
        }
    }

    public override void OnActorCreated(Actor actor) {
        if (actor.OID == (uint)OID.TinyApprentice) {
            TinyMageMechanic.AddMage(mages, actor, Arena.Center);
        } else if (actor.OID == (uint)OID.HolySphere1Grow) {
            StartOrb(actor);
        }
    }

    public override void Update() {
        RecoverExistingActors();
        if (clearAt != default && WorldState.CurrentTime >= clearAt) {
            ClearStart();
            return;
        }
        if (orb != null && direction == 0) {
            direction = TinyMageMechanic.ObserveDirection(orb, Arena.Center, ref previousAngle, ref angularTravel);
        }
        UpdatePredictedTarget();
    }

    public override void OnActorDeath(Actor actor) {
        RemoveActor(actor);
    }

    public override void OnActorDestroyed(Actor actor) {
        RemoveActor(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.TinyHoly1) {
            ClearStart();
        }
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) {
        RecoverExistingActors();
        if (directTarget is { } resolved) {
            return new Knockback[1] { new(resolved, 15.0f, activation) };
        }
        UpdatePredictedTarget();
        return predictedTarget is { } predicted ? new Knockback[1] { new(predicted, 15.0f, activation) } : [];
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        var knockbacks = ActiveKnockbacks(slot, actor);
        if (knockbacks.Length != 0) {
            ref readonly var knockback = ref knockbacks[0];
            var sd = new SDKnockbackInCircleAwayFromOrigin(Arena.Center, knockback.Origin, knockback.Distance, 19f);
            hints.AddForbiddenZone(sd, knockback.Activation);
            // 引导 AI 到安全位置: 被 15y 击退后落点仍在 19y 电网内, 落点越靠中心分越高。
            var origin = knockback.Origin;
            var distance = knockback.Distance;
            hints.GoalZones.Add(position =>
            {
                var projected = position + distance * (position - origin).Normalized();
                var dist = (projected - Arena.Center).Length();
                return dist < 19f ? 10f - dist * 0.5f : 0f;
            });
        }
    }

    private void RemoveActor(Actor actor) {
        TinyMageMechanic.RemoveMage(mages, actor.InstanceID);
        if (actor.InstanceID == startActorID) {
            clearAt = WorldState.FutureTime(TinyMageMechanic.ResolveVisualHold);
        }
    }

    private void RecoverExistingActors() {
        foreach (var mage in Module.Enemies((uint)OID.TinyApprentice)) {
            if (!mage.IsDeadOrDestroyed) {
                TinyMageMechanic.AddMage(mages, mage, Arena.Center);
            }
        }
        if (orb == null) {
            var existing = Module.Enemies((uint)OID.HolySphere1Grow).FirstOrDefault(actor => !actor.IsDeadOrDestroyed);
            if (existing != null) {
                StartOrb(existing);
            }
        }
    }

    private void StartOrb(Actor actor) {
        orb = actor;
        start = actor.Position;
        predictedTarget = null;
        startActorID = actor.InstanceID;
        activation = WorldState.FutureTime(TinyMageMechanic.GrowResolveDelay);
        clearAt = default;
        previousAngle = TinyMageMechanic.AngleFromNorth(actor.Position, Arena.Center);
        angularTravel = default;
        direction = default;
    }

    private void UpdatePredictedTarget() {
        if (mages.Count != 4 || start == null || direction == 0) {
            return;
        }

        var startAOE = mages.FindIndex(mage => mage.Position.AlmostEqual(start.Value, 0.5f));
        if (startAOE >= 0) {
            predictedTarget = mages[(startAOE + mages.Count - direction) % mages.Count].Position;
        }
    }

    private void ClearStart() {
        orb = null;
        start = null;
        predictedTarget = null;
        directTarget = null;
        startActorID = default;
        activation = default;
        clearAt = default;
        previousAngle = default;
        angularTravel = default;
        direction = default;
    }

    private void ResetWave() {
        mages.Clear();
        ClearStart();
    }
}

static class OrbPairMechanic {
    // Replay-verified (two full waves): the first pair pops 10s after the orbs spawn/tether and
    // the remaining pairs follow in tether order every 3s; the 1.7s resolve cast then refines
    // the final activation via UpdateActivation.
    private const double FirstResolveDelay = 10.0d;
    private const double ResolveStagger = 3.0d;

    public readonly record struct Key(ulong First, ulong Second);

    public static Key MakeKey(ulong first, ulong second) {
        return first < second ? new(first, second) : new(second, first);
    }

    public static DateTime EstimateActivation(WorldState worldState, int orderIndex) {
        return worldState.FutureTime(FirstResolveDelay + ResolveStagger * orderIndex);
    }
}

// The four pair resolutions form one mixed flare/holy sequence. Track both types in one
// timeline so a later holy does not become the active knockback while an earlier flare is
// still pending (and vice versa).
sealed class OrbPairTimeline(BossModule module) : BossComponent(module) {
    private const double ExpireDelay = 10.0d;

    public enum Kind : byte {
        Flare,
        Holy
    }

    public sealed class Entry(OrbPairMechanic.Key key, Kind kind, WPos origin, float distance, DateTime activation) {
        public readonly OrbPairMechanic.Key Key = key;
        public readonly Kind Type = kind;
        public readonly WPos Origin = origin;
        public readonly float Distance = distance;
        public DateTime Activation = activation;
        public ulong ActorID;
        public DateTime ClearAt;
    }

    private readonly List<Entry> entries = [];
    private readonly HashSet<OrbPairMechanic.Key> pairs = [];
    private readonly HashSet<uint> seenGlobalSequences = [];

    public List<Entry> Upcoming(int count) {
        PruneExpired();
        return entries
            .Where(entry => entry.ClearAt == default)
            .OrderBy(entry => entry.Activation)
            .ThenBy(entry => entry.Distance)
            .ThenBy(entry => entry.Key.First)
            .Take(count)
            .ToList();
    }

    public List<Entry> RetainedFlares() {
        PruneExpired();
        return entries.Where(entry => entry.Type == Kind.Flare && entry.ClearAt != default).ToList();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID._Ability_AllForOne) {
            ResetWave();
            return;
        }

        var kind = KindForAction(spell.Action.ID);
        if (kind != null) {
            UpdateActivation(kind.Value, caster, Module.CastFinishAt(spell));
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether) {
        if (tether.ID != (uint)TetherID.OrbPairs) {
            return;
        }

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null) {
            return;
        }

        var kind = KindForPair(source, target);
        if (kind == null) {
            return;
        }

        var key = OrbPairMechanic.MakeKey(source.InstanceID, target.InstanceID);
        if (!pairs.Add(key)) {
            return;
        }

        var distance = (target.Position - source.Position).Length();
        entries.Add(new(key, kind.Value, WPos.Lerp(source.Position, target.Position, 0.5f), distance, OrbPairMechanic.EstimateActivation(WorldState, entries.Count)));
    }

    public override void OnActorDeath(Actor actor) => RemoveActor(actor.InstanceID);
    public override void OnActorDestroyed(Actor actor) => RemoveActor(actor.InstanceID);

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        var kind = KindForAction(spell.Action.ID);
        if (kind == null || spell.GlobalSequence != 0 && !seenGlobalSequences.Add(spell.GlobalSequence)) {
            return;
        }

        ResolveAt(kind.Value, caster);
    }

    public override void Update() => PruneExpired();

    private static Kind? KindForAction(uint actionID) => actionID switch {
        (uint)AID.TinyFlare1 => Kind.Flare,
        (uint)AID.TinyHoly1 => Kind.Holy,
        _ => null
    };

    private static Kind? KindForPair(Actor first, Actor second) {
        if (first.OID == (uint)OID.FlareSphere && second.OID == (uint)OID.FlareSphere) {
            return Kind.Flare;
        }
        if (first.OID == (uint)OID.HolySphere && second.OID == (uint)OID.HolySphere) {
            return Kind.Holy;
        }
        return null;
    }

    private void UpdateActivation(Kind kind, Actor caster, DateTime activation) {
        foreach (var entry in entries) {
            if (entry.Type == kind && entry.ClearAt == default && entry.Origin.AlmostEqual(caster.Position, 1.5f)) {
                entry.Activation = activation;
                entry.ActorID = caster.InstanceID;
            }
        }
    }

    private void ResolveAt(Kind kind, Actor caster) {
        for (var i = entries.Count - 1; i >= 0; --i) {
            var entry = entries[i];
            if (entry.Type != kind || entry.ClearAt != default || entry.ActorID != caster.InstanceID && !entry.Origin.AlmostEqual(caster.Position, 1.5f)) {
                continue;
            }

            if (kind == Kind.Flare) {
                entry.ClearAt = WorldState.FutureTime(TinyMageMechanic.ResolveVisualHold);
            } else {
                pairs.Remove(entry.Key);
                entries.RemoveAt(i);
            }
        }
    }

    private void RemoveActor(ulong instanceID) {
        for (var i = entries.Count - 1; i >= 0; --i) {
            var key = entries[i].Key;
            if (key.First == instanceID || key.Second == instanceID) {
                if (entries[i].Type == Kind.Flare && entries[i].ClearAt != default) {
                    continue;
                }
                pairs.Remove(key);
                entries.RemoveAt(i);
            }
        }
    }

    private void ResetWave() {
        entries.Clear();
        pairs.Clear();
        seenGlobalSequences.Clear();
    }

    private void PruneExpired() {
        var now = WorldState.CurrentTime;
        for (var i = entries.Count - 1; i >= 0; --i) {
            var entry = entries[i];
            if (entry.ClearAt != default ? now >= entry.ClearAt : now > entry.Activation.AddSeconds(ExpireDelay)) {
                pairs.Remove(entry.Key);
                entries.RemoveAt(i);
            }
        }
    }
}

sealed class FlareCombo(BossModule module) : Components.GenericAOEs(module) {
    private static readonly AOEShapeCircle Shape = new(18.0f);
    private readonly OrbPairTimeline timeline = module.FindComponent<OrbPairTimeline>()!;
    private readonly List<AOEInstance> displayed = [with(2)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        displayed.Clear();
        foreach (var entry in timeline.RetainedFlares()) {
            displayed.Add(new(Shape, entry.Origin, activation: entry.Activation, color: Colors.Danger, risky: true, actorID: entry.ActorID));
        }

        var upcoming = timeline.Upcoming(2);
        if (upcoming.Count != 0) {
            // Flare resolves as a 18y circle; automation needs the full remaining cast to move out
            // (replay: AI was clipped at 17.8y in the 18y radius). Mark risky well before resolve.
            var imminentDeadline = upcoming[0].Activation.AddSeconds(1.0d);
            foreach (var entry in upcoming) {
                if (entry.Type == OrbPairTimeline.Kind.Flare) {
                    var imminent = entry.Activation <= imminentDeadline;
                    displayed.Add(new(Shape, entry.Origin, activation: entry.Activation, color: imminent ? Colors.Danger : Colors.AOE, risky: imminent, actorID: entry.ActorID));
                }
            }
        }
        return CollectionsMarshal.AsSpan(displayed);
    }
}

sealed class HolyCombo(BossModule module) : Components.GenericKnockback(module) {
    private readonly OrbPairTimeline timeline = module.FindComponent<OrbPairTimeline>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) {
        var upcoming = timeline.Upcoming(1);
        if (upcoming.Count == 0 || upcoming[0].Type != OrbPairTimeline.Kind.Holy) {
            return [];
        }

        var next = upcoming[0];
        return new Knockback[1] { new(next.Origin, 15.0f, next.Activation, actorID: next.ActorID) };
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        var knockbacks = ActiveKnockbacks(slot, actor);
        if (knockbacks.Length != 0) {
            ref readonly var knockback = ref knockbacks[0];
            hints.AddForbiddenZone(new SDKnockbackInCircleAwayFromOrigin(Arena.Center, knockback.Origin, knockback.Distance, 19f), knockback.Activation);
        }
    }
}

[SkipLocalsInit]
sealed class TinyMageStates : StateMachineBuilder {
    public TinyMageStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<ElectricBoundary>()
            .ActivateOnEnter<TinyThunderIII>()
            .ActivateOnEnter<TinyQuake>()
            .ActivateOnEnter<DiminutiveDualcast>()
            .ActivateOnEnter<TinyMeteor>()
            .ActivateOnEnter<Comet>()
            .ActivateOnEnter<HolyGrowable>()
            .ActivateOnEnter<FlareGrowable>()
            .ActivateOnEnter<OrbPairTimeline>()
            .ActivateOnEnter<FlareCombo>()
            .ActivateOnEnter<HolyCombo>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(TinyMageStates),
    ConfigType = null, // replace null with typeof(TinyMageConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = typeof(TetherID),
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.TinyMage,
    Contributors = "KanoNoUta",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CriticalEngagement,
    GroupID = 1093u,
    NameID = 60u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class TinyMage(WorldState ws, Actor primary) : BossModule(ws, primary, new(152.000f, 716.000f), new ArenaBoundsCircle(20f)) {
    protected override void DrawEnemies(int pcSlot, Actor pc) {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.ArcaneSphereSmall));
        Arena.Actors(Enemies((uint)OID.ArcaneSphereBig));
    }
}
