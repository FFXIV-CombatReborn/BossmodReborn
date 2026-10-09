namespace BossMod;

// information relevant for AI decision making process for a specific player
public sealed class AIHints
{
    public class Enemy(Actor actor, int priority, bool shouldBeTanked)
    {
        public const int PriorityPointless = -1; // attacking enemy won't improve your parse, but will give gauge and advance combo (e.g. boss locked to 1 HP, useless add in raid, etc)
        public const int PriorityInvincible = -2; // attacking enemy will have no effect at all besides breaking your combo, but hitting it with AOEs is fine
        public const int PriorityUndesirable = -3; // enemy can be attacked if targeted manually by a player, but should be considered forbidden for AOE actions (i.e. mobs that are not in combat, or are in combat with someone else's party)
        public const int PriorityForbidden = -4; // attacking this enemy will probably lead to a wipe; autoattacks and actions that target it will be forcibly prevented (if custom queueing is enabled)

        public Actor Actor = actor;
        private int _priority = priority;
        public int Priority
        {
            get => _priority;
            set
            {
                // we should never change priority if it has been set to pointless, since that means the target is dying and further actions targeting it are a waste
                if (_priority != PriorityPointless)
                {
                    _priority = value;
                }
            }
        }
        //public float TimeToKill;
        public float AttackStrength = 0.05f; // target's predicted HP percent is decreased by this amount (0.05 by default)
        public bool CanMove = true;
        public WPos? DesiredPosition; // tank AI will try to move enemy to this position
        public Angle? DesiredRotation; // tank AI will try to rotate enemy to this angle
        public float TankDistance = 2f; // enemy will start moving if distance between hitboxes is bigger than this
        public bool ShouldBeTanked = shouldBeTanked; // tank AI will try to tank this enemy
        public bool PreferProvoking; // tank AI will provoke enemy if not targeted
        public bool ForbidDOTs; // if true, dots on target are forbidden
        public bool ShouldBeInterrupted; // if set and enemy is casting interruptible spell, some ranged/tank will try to interrupt
        public bool ShouldBeStunned; // if set, AI will stun if possible
        public bool ShouldBeDispelled; // if set, AI will try to cast a dispel action; only relevant for foray content
        public bool StayAtLongRange; // if set, players with ranged attacks don't bother coming closer than max range (TODO: reconsider)
        public bool Spikes; // if set, autoattacks will be prevented

        public bool ShouldBeTargeted
        {
            set
            {
                field = value;
                if (value)
                    Priority = Math.Max(0, Priority);
            }
            get;
        }

        public void ForcePriority(int priority) => _priority = priority;
    }

    public enum SpecialMode
    {
        Normal,
        Pyretic, // pyretic/acceleration bomb type of effects - no movement, no actions, no casting allowed at activation time
        NoMovement, // no movement allowed
        Freezing, // should be moving at activation time
        Misdirection, // temporary misdirection - if current time is greater than activation, use special pathfinding codepath
    }

    public enum PredictedDamageType
    {
        None,
        Tankbuster, // cast is expected to do a decent amount of damage, tank AI should use mitigation
        Raidwide, // cast is expected to hit everyone and deal minor damage; also used for spread components
        Shared, // cast is expected to hit multiple players; modules might have special behavior when intentionally taking this damage solo
    }

    public enum FateSync
    {
        None, // do nothing
        Enable, // level sync to fate
        Disable // unsync from fate
    }

    public readonly struct DamagePrediction(BitMask players, DateTime activation, PredictedDamageType type = PredictedDamageType.None)
    {
        public readonly BitMask Players = players;
        public readonly DateTime Activation = activation;
        public readonly PredictedDamageType Type = type;
    }

    public static readonly ArenaBounds DefaultBounds = new ArenaBoundsSquare(30f, allowObstacleMap: true);

    // information needed to build base pathfinding map (onto which forbidden/goal zones are later rasterized), if needed (lazy, since it's somewhat expensive and not always needed)
    public WPos PathfindMapCenter;
    public ArenaBounds PathfindMapBounds = DefaultBounds;
    public int? PathfindMapArenaProjectionLayer;
    public Bitmap.Region PathfindMapObstacles;
    private ArenaBoundsCustom? _arenaProjectionLayerClipOwner;
    private WPos _arenaProjectionLayerClipCenter;
    private ShapeDistance?[]? _arenaProjectionLayerClips;
    private static readonly AI.AIConfig _config = Service.Config.Get<AI.AIConfig>();

    // list of potential targets
    public const int NumEnemies = 100;
    public readonly Enemy?[] Enemies = new Enemy?[NumEnemies];
    public Enemy? FindEnemy(Actor? actor) => Enemies.BoundSafeAt(actor?.CharacterSpawnIndex ?? -1);

    // enemies in priority order
    public readonly List<Enemy> PotentialTargets = [];

    public int HighestPotentialTargetPriority;

    // forced target
    // this should be set only if either explicitly planned by user or by ai, otherwise it will be annoying to user
    public Actor? ForcedTarget;
    public Actor? ForcedFocusTarget;

    // low-level forced movement - if set, character will move in specified direction (ignoring casts, uptime, forbidden zones, etc), or stay in place if set to default
    public Vector3? ForcedMovement;

    // which direction should we point during the Spinning status in Alzadaal's Legacy? (yes, this is a bespoke movement gimmick for one dungeon boss)
    public Angle? SpinDirection;

    // indicates to AI mode that it should try to interact with some object
    public Actor? InteractWithTarget;

    // positioning: list of shapes that are either forbidden to stand in now or will be in near future
    // AI will try to move in such a way to avoid standing in any forbidden zone after its activation or outside of some restricted zone after its activation, even at the cost of uptime
    public readonly List<(ShapeDistance shapeDistance, DateTime activation, ulong Source)> ForbiddenZones = [];

    // positioning: list of goal functions
    // AI will try to move to reach non-forbidden point with highest goal value (sum of values returned by all functions)
    // guideline: rotation modules should return 1 if it would use single-target action from that spot, 2 if it is also a positional, 3 if it would use aoe that would hit minimal viable number of targets, +1 for each extra target
    // other parts of the code can return small (e.g. 0.01) values to slightly (de)prioritize some positions, or large (e.g. 1000) values to effectively soft-override target position (but still utilize pathfinding)
    public readonly List<Func<WPos, float>> GoalZones = [];

    public bool GoalZonesEnabled = true;

    // AI will treat the pixels inside these shapes as unreachable and not try to pathfind through them (unlike imminent forbidden zones)
    public List<ShapeDistance> TemporaryObstacles = [];

    // AI will treat the pixels inside these shapes as unreachable and not try to pathfind through them (unlike imminent forbidden zones)
    public readonly List<Pathfinding.Teleporter> Teleporters = [];

    // positioning: next positional hint (TODO: reconsider, maybe it should be a list prioritized by in-gcds, and imminent should be in-gcds instead? or maybe it should be property of an enemy? do we need correct?)
    public (Actor? Target, Positional Pos, bool Imminent, bool Correct) RecommendedPositional;

    // positional currently desired by RotationSolverReborn (if installed and providing that info over IPC), otherwise Any
    public Positional RSRDesiredPositional;

    // orientation restrictions (e.g. for gaze attacks): a list of forbidden orientation ranges, now or in near future
    // AI will rotate to face allowed orientation at last possible moment, potentially losing uptime
    public readonly List<(Angle center, Angle halfWidth, DateTime activation)> ForbiddenDirections = [];

    // closest special movement/targeting/action mode, if any
    // activation = when the restriction starts (e.g. bomb detonation), finish = when the restriction ends (e.g. pyretic expires)
    public (SpecialMode mode, DateTime activation, DateTime finish) ImminentSpecialMode;

    // for misdirection: if forced movement is set, make real direction be within this angle
    public Angle MisdirectionThreshold;

    // predicted incoming damage (raidwides, tankbusters, etc.)
    // AI will attempt to shield & mitigate
    public readonly List<DamagePrediction> PredictedDamage = [];

    // list of party members with cleansable debuffs that are dangerous enough to sacrifice a GCD to cleanse them, i.e. doom, throttle, some types of vuln debuff, etc
    public BitMask ShouldCleanse;

    // maximal time we can spend casting before we need to move
    // this is used by the action queue to skip casts that we won't be able to finish and execute lower-priority fallback actions instead
    public float MaxCastTime = float.MaxValue;
    public bool ForceCancelCastOther;

    public bool ForceCancelCastMechanic;

    // set by the active movement system while settling after a move that interrupted a cast (see CastSettleHold); external rotations should not start casts
    public bool HoldCasts;

    // actions that we want to be executed, gathered from various sources (manual input, autorotation, planner, ai, modules, etc.)
    public readonly ActionQueue ActionsToExecute = new();

    // buffs to be canceled asap
    public readonly List<(uint statusId, ulong sourceId)> StatusesToCancel = [];

    // misc stuff to execute
    public bool WantJump;
    public bool WantDismount;
    public bool ForbidDashes; // if set, gap closers/dashes will be prevented
    public FateSync WantFateSync;
    public bool ShouldLeaveDuty;

    // clear all stored data
    public void Clear()
    {
        PathfindMapCenter = default;
        PathfindMapBounds = DefaultBounds;
        PathfindMapArenaProjectionLayer = null;
        PathfindMapObstacles = default;
        Array.Clear(Enemies);
        PotentialTargets.Clear();
        ForcedTarget = null;
        ForcedFocusTarget = null;
        ForcedMovement = null;
        SpinDirection = null;
        InteractWithTarget = null;
        ForbiddenZones.Clear();
        GoalZones.Clear();
        GoalZonesEnabled = true;
        TemporaryObstacles.Clear();
        Teleporters.Clear();
        RecommendedPositional = default;
        ForbiddenDirections.Clear();
        ImminentSpecialMode = default;
        MisdirectionThreshold = 15f.Degrees();
        PredictedDamage.Clear();
        ShouldCleanse.Reset();
        MaxCastTime = float.MaxValue;
        ForceCancelCastOther = false;
        ForceCancelCastMechanic = false;
        HoldCasts = false;
        ActionsToExecute.Clear();
        StatusesToCancel.Clear();
        WantJump = false;
        WantDismount = false;
        WantFateSync = FateSync.None;
        ShouldLeaveDuty = false;
        ForbidDashes = false;
    }

    public void PrioritizeTargetsByOID(uint oid, int priority = default)
    {
        var count = PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var h = PotentialTargets[i];
            if (h.Actor.OID == oid)
            {
                h.Priority = priority;
            }
        }
    }

    public void PrioritizeTargetsByOIDAndForbidDOTs(uint oid, int priority = default, bool forbidDots = false)
    {
        var count = PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var h = PotentialTargets[i];
            if (h.Actor.OID == oid)
            {
                h.Priority = priority;
                if (forbidDots)
                {
                    h.ForbidDOTs = true;
                }
            }
        }
    }

    public void PrioritizeTargetsByOID(uint[] oids, int priority = default)
    {
        var count = PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var h = PotentialTargets[i];
            var len = oids.Length;
            for (var j = 0; j < len; ++j)
            {
                if (oids[j] == h.Actor.OID)
                {
                    h.Priority = priority;
                    break;
                }
            }
        }
    }

    public void PrioritizeAll()
    {
        var count = PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var h = PotentialTargets[i];
            // Math.Max(h.priority, 0)
            var mask = h.Priority >> 31; // 0 if positive, -1 if negative
            h.Priority &= ~mask; // clears to 0 if negative
        }
    }

    public void SetPriority(Actor? actor, int priority)
    {
        var enemy = FindEnemy(actor);
        enemy?.Priority = priority;
    }

    public void InteractWithOID(WorldState ws, uint oid)
    {
        foreach (var a in ws.Actors)
        {
            if (a.OID == oid && a.IsTargetable)
            {
                InteractWithTarget = a;
                return;
            }
        }

        InteractWithTarget = null;
    }
    public void InteractWithOID<OID>(WorldState ws, OID oid) where OID : Enum => InteractWithOID(ws, (uint)(object)oid);

    // Explicitly layered zones are intersected with their physical floor. This matters when several
    // disjoint floors share one pathfinding grid: a large shape must not spill into another island.
    public ShapeDistance ClipToArenaProjectionLayer(ShapeDistance shapeDistance, int? arenaProjectionLayer)
    {
        if (arenaProjectionLayer is not int index
            || PathfindMapBounds is not ArenaBoundsCustom { WorldProjectionLayers: { Length: > 0 } layers } custom
            || (uint)index >= (uint)layers.Length)
        {
            return shapeDistance;
        }

        if (!ReferenceEquals(_arenaProjectionLayerClipOwner, custom) || _arenaProjectionLayerClipCenter != PathfindMapCenter
            || _arenaProjectionLayerClips == null || _arenaProjectionLayerClips.Length != layers.Length)
        {
            _arenaProjectionLayerClipOwner = custom;
            _arenaProjectionLayerClipCenter = PathfindMapCenter;
            _arenaProjectionLayerClips = new ShapeDistance?[layers.Length];
        }

        var clips = _arenaProjectionLayerClips!;
        var clip = clips[index];
        if (clip == null)
        {
            var polygon = layers[index].Shape;
            polygon.VerifyPolygonIndexExistance();
            clip = clips[index] = new SDPolygonWithHoles(new SDPolygonWithHolesBase(PathfindMapCenter, polygon));
        }
        return new SDIntersection([shapeDistance, clip]);
    }

    public void AddForbiddenZone(ShapeDistance shapeDistance, DateTime activation = default, ulong source = default, int? arenaProjectionLayer = null)
        => ForbiddenZones.Add((ClipToArenaProjectionLayer(shapeDistance, arenaProjectionLayer), activation, source));
    public void AddForbiddenZone(AOEShape shape, WPos origin, Angle rot = default, DateTime activation = default, ulong source = default, int? arenaProjectionLayer = null)
        => ForbiddenZones.Add((ClipToArenaProjectionLayer(shape.Distance(origin, rot), arenaProjectionLayer), activation, source));

    public void AddPredictedDamage(BitMask players, DateTime activation, PredictedDamageType type = PredictedDamageType.Raidwide) => PredictedDamage.Add(new(players, activation, type));

    public void AddSpecialMode(SpecialMode mode, DateTime activation, DateTime finish = default)
    {
        if (ImminentSpecialMode == default || ImminentSpecialMode.activation > activation)
        {
            ImminentSpecialMode = (mode, activation, finish);
        }
    }

    public void AddForbiddenDirections(ArcList list, DateTime activation)
    {
        foreach (var (from, to) in list.Forbidden.Segments)
        {
            var center = (to + from) * 0.5f;
            var width = (to - from) * 0.5f;
            ForbiddenDirections.Add((center.Radians(), width.Radians(), activation));
        }
    }

    // normalize all entries after gathering data: sort by priority / activation timestamp
    // TODO: note that the name is misleading - it actually happens mid frame, before all actions are gathered (eg before autorotation runs), but further steps (eg ai) might consume previously gathered data
    public void Normalize()
    {
        PotentialTargets.Sort(static (b, a) => a.Priority.CompareTo(b.Priority));
        HighestPotentialTargetPriority = PotentialTargets.Count > 0 ? Math.Max(0, PotentialTargets[0].Priority) : 0;
        SortHelpers.SortForbiddenZonesByActivation(ForbiddenZones);
        SortHelpers.SortForbiddenDirectionsByActivation(ForbiddenDirections);
        PredictedDamage.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
    }

    public void InitPathfindMap(Pathfinding.Map map)
    {
        if (PathfindMapBounds is ArenaBoundsCustom custom)
        {
            custom.PathfindMap(map, PathfindMapCenter, PathfindMapArenaProjectionLayer);
        }
        else
        {
            PathfindMapBounds.PathfindMap(map, PathfindMapCenter);
        }
        if (PathfindMapObstacles.Bitmap != null && !_config.DisableObstacleMaps)
        {
            var offX = -PathfindMapObstacles.Rect.Left;
            var offY = -PathfindMapObstacles.Rect.Top;
            var r = PathfindMapObstacles.Rect.Clamped(PathfindMapObstacles.Bitmap.FullRect);
            var height = map.Height;
            var width = map.Width;
            var rTop = r.Top;
            var rBottom = r.Bottom;
            var rLeft = r.Left;
            var rRight = r.Right;

            for (var y = rTop; y < rBottom; ++y)
            {
                var my = y + offY;
                if (my < 0 || my >= height)
                {
                    continue;
                }
                for (var x = rLeft; x < rRight; ++x)
                {
                    if (!PathfindMapObstacles.Bitmap[x, y])
                    {
                        continue;
                    }

                    var mx = x + offX;
                    if (mx < 0 || mx >= width)
                    {
                        continue;
                    }
                    var index = map.GridToIndex(mx, my);
                    map.PixelMaxG[index] = -1000f;
                    map.PixelPriority[index] = float.MinValue;
                }
            }
        }
    }

    // Allocation-free views used by consumers. These remain valid only until PotentialTargets is modified; in normal use that means until the next hints update.
    public ReadOnlySpan<Enemy> PriorityTargetsSpan
    {
        get
        {
            var targets = CollectionsMarshal.AsSpan(PotentialTargets);
            var count = 0;
            while (count < targets.Length && targets[count].Priority == HighestPotentialTargetPriority)
            {
                ++count;
            }
            return targets[..count];
        }
    }

    // This view retains PotentialTargets' descending-priority order
    public ReadOnlySpan<Enemy> ForbiddenTargetsSpan
    {
        get
        {
            var targets = CollectionsMarshal.AsSpan(PotentialTargets);
            var first = targets.Length;
            while (first > 0 && targets[first - 1].Priority <= Enemy.PriorityUndesirable)
            {
                --first;
            }
            return targets[first..];
        }
    }

    public bool AnyPriorityTarget(Func<Enemy, bool> predicate)
    {
        var span = PriorityTargetsSpan;
        var len = span.Length;
        for (var i = 0; i < len; ++i)
        {
            if (predicate(span[i]))
            {
                return true;
            }
        }
        return false;
    }

    public Enemy? FirstPriorityTarget(Func<Enemy, bool> predicate)
    {
        var span = PriorityTargetsSpan;
        var len = span.Length;
        for (var i = 0; i < len; ++i)
        {
            var t = span[i];
            if (predicate(t))
            {
                return t;
            }
        }
        return null;
    }

    public int CountPriorityTargets(Func<Enemy, bool> predicate)
    {
        var count = 0;
        var span = PriorityTargetsSpan;
        var len = span.Length;
        for (var i = 0; i < len; ++i)
        {
            if (predicate(span[i]))
            {
                ++count;
            }
        }
        return count;
    }

    // TODO: verify how source/target hitboxes are accounted for by various aoe shapes
    public int NumPriorityTargetsInAOE(Func<Enemy, bool> pred)
    {
        var forbidden = ForbiddenTargetsSpan;
        var lenF = forbidden.Length - 1;
        for (var i = lenF; i >= 0; --i)
        {
            if (pred(forbidden[i]))
            {
                return 0;
            }
        }

        var priority = PriorityTargetsSpan;
        var count = 0;
        var lenP = priority.Length;
        for (var i = 0; i < lenP; ++i)
        {
            if (pred(priority[i]))
            {
                ++count;
            }
        }

        return count;
    }

    public int NumPriorityTargetsInAOECircle(WPos origin, float radius)
    {
        var forbidden = ForbiddenTargetsSpan;
        var lenF = forbidden.Length - 1;
        for (var i = lenF; i >= 0; --i)
        {
            if (TargetInAOECircle(forbidden[i].Actor, origin, radius))
            {
                return 0;
            }
        }

        var priority = PriorityTargetsSpan;
        var count = 0;
        var lenP = priority.Length;
        for (var i = 0; i < lenP; ++i)
        {
            if (TargetInAOECircle(priority[i].Actor, origin, radius))
            {
                ++count;
            }
        }
        return count;
    }

    public int NumPriorityTargetsInAOECone(WPos origin, float radius, WDir direction, Angle halfAngle)
    {
        var forbidden = ForbiddenTargetsSpan;
        var lenF = forbidden.Length - 1;
        for (var i = lenF; i >= 0; --i)
        {
            if (TargetInAOECone(forbidden[i].Actor, origin, radius, direction, halfAngle))
            {
                return 0;
            }
        }

        var priority = PriorityTargetsSpan;
        var count = 0;
        var lenP = priority.Length;
        for (var i = 0; i < lenP; ++i)
        {
            if (TargetInAOECone(priority[i].Actor, origin, radius, direction, halfAngle))
            {
                ++count;
            }
        }
        return count;
    }

    public int NumPriorityTargetsInAOERect(WPos origin, WDir direction, float lenFront, float halfWidth, float lenBack = 0)
    {
        var rectCenterOffset = (lenFront - lenBack) * 0.5f;
        var rectCenter = origin + direction * rectCenterOffset;
        var halfLength = (lenFront + lenBack) * 0.5f;

        var forbidden = ForbiddenTargetsSpan;
        var lenF = forbidden.Length - 1;
        for (var i = lenF; i >= 0; --i)
        {
            var actor = forbidden[i].Actor;
            if (Intersect.CircleRect(actor.Position, actor.HitboxRadius, rectCenter, direction, halfWidth, halfLength))
            {
                return 0;
            }
        }

        var priority = PriorityTargetsSpan;
        var count = 0;
        var lenP = priority.Length;
        for (var i = 0; i < lenP; ++i)
        {
            var actor = priority[i].Actor;
            if (Intersect.CircleRect(actor.Position, actor.HitboxRadius, rectCenter, direction, halfWidth, halfLength))
            {
                ++count;
            }
        }
        return count;
    }
    public static bool TargetInAOECircle(Actor target, WPos origin, float radius) => target.Position.InCircle(origin, radius + target.HitboxRadius);
    public static bool TargetInAOECone(Actor target, WPos origin, float radius, WDir direction, Angle halfAngle) => Intersect.CircleCone(target.Position, target.HitboxRadius, origin, radius, direction, halfAngle);
    public static bool TargetInAOERect(Actor target, WPos origin, WDir direction, float lenFront, float halfWidth, float lenBack = default)
    {
        var rectCenterOffset = (lenFront - lenBack) * 0.5f;
        var rectCenter = origin + direction * rectCenterOffset;
        return Intersect.CircleRect(target.Position, target.HitboxRadius, rectCenter, direction, halfWidth, (lenFront + lenBack) * 0.5f);
    }

    // goal zones
    // simple goal zone that returns 1 if target is in range, useful for single-target actions
    public static Func<WPos, float> GoalSingleTarget(WPos target, float radius, float weight = 1f)
    {
        var effRsq = radius * radius;
        return p => (p - target).LengthSq() <= effRsq ? weight : default;
    }

    public static Func<WPos, float> GoalSingleTarget(Actor target, float radius, float weight = 1f)
    {
        var effRsq = radius * radius;
        return p => (p - target.Position).LengthSq() <= effRsq ? weight : default;
    }

    public Func<WPos, float> GoalSingleTarget(Actor target, Actor player, ActorState actorState, float range, float weight = 1f)
    {
        var f = GoalSingleTarget(target.Position, range + target.HitboxRadius, weight);
        var g = FollowTarget(target, player, actorState, range);

        return p => f(p) + g(p);
    }

    // simple goal zone that returns 1 if target is in range (usually melee), 2 if it's also in correct positional
    public static Func<WPos, float> GoalSingleTarget(WPos target, Angle rotation, Positional positional, float radius, float cushion = 0f)
    {
        if (positional == Positional.Any)
        {
            return GoalSingleTarget(target, radius); // more efficient implementation
        }

        var effRsq = radius * radius;
        var targetDir = rotation.ToDirection();
        const float sqrt2 = 1.41421356f;
        var cushionThreshold = cushion * sqrt2;
        return p =>
        {
            var offset = p - target;
            var lsq = offset.LengthSq();
            if (lsq > effRsq)
            {
                return 0f; // out of range
            }
            // note: this assumes that extra dot is cheaper than sqrt?..
            var front = targetDir.Dot(offset);
            var side = Math.Abs(targetDir.Dot(offset.OrthoL()));
            var inPositional = positional switch
            {
                Positional.Flank => side - Math.Abs(front) > cushionThreshold,
                Positional.Rear => -front - side > cushionThreshold,
                Positional.Front => front - side > cushionThreshold, // TODO: reconsider this, it's not a real positional?..
                _ => false
            };
            return inPositional ? 2f : 1f;
        };
    }

    public static Func<WPos, float> GoalSingleTarget(Actor target, Positional positional, float range = 2.6f, float cushion = 0f)
    => GoalSingleTarget(target.Position, target.Rotation, positional, range + target.HitboxRadius, cushion);

    public Func<WPos, float> GoalSingleTarget(Actor target, Positional positional, Actor player, ActorState actorState, float range = 2.6f, float cushion = 0f)
    {
        var f = GoalSingleTarget(target.Position, target.Rotation, positional, range + target.HitboxRadius, cushion);
        var g = FollowTarget(target, player, actorState, range);

        return p => f(p) + g(p);
    }

    Func<WPos, float> FollowTarget(Actor target, Actor player, ActorState actorState, float maxRange)
    {
        if (target.CastInfo == null && target.TargetID != player.InstanceID && FindEnemy(target) is { } e && actorState.Find(target.TargetID) is { } tt)
        {
            var tankDistance = e.Actor.HitboxRadius + e.TankDistance;
            var targetToTT = tt.Position - target.Position;
            if (targetToTT.LengthSq() > tankDistance * tankDistance)
            {
                var effRange = target.HitboxRadius + maxRange + 0.5f;
                var expectedPosition = target.Position + targetToTT.Normalized() * Math.Min(effRange - 1f, targetToTT.Length() - tankDistance);
                return GoalSingleTarget(expectedPosition, effRange, 0.1f);
            }
        }

        return _ => 0f;
    }

    // simple goal zone that returns number of targets in aoes; note that performance is a concern for these functions, and perfection isn't required, so eg they ignore forbidden targets, etc
    public Func<WPos, float> GoalAOECircle(float radius)
    {
        var priority = PriorityTargetsSpan;
        var count = priority.Length;
        var targets = new (WPos pos, float radius)[count];
        for (var i = 0; i < count; ++i)
        {
            var e = priority[i];
            targets[i] = (e.Actor.Position, e.Actor.HitboxRadius);
        }
        return p =>
        {
            var countInCircle = 0;
            for (var i = 0; i < count; ++i)
            {
                var t = targets[i];
                if (t.pos.InCircle(p, radius + t.radius))
                {
                    ++countInCircle;
                }
            }

            return countInCircle;
        };
    }

    public Func<WPos, float> GoalAOECone(Actor primaryTarget, float radius, Angle halfAngle)
    {
        var priority = PriorityTargetsSpan;
        var count = priority.Length;
        var targets = new (WPos pos, float radius)[count];
        for (var i = 0; i < count; ++i)
        {
            var e = priority[i];
            targets[i] = (e.Actor.Position, e.Actor.HitboxRadius);
        }
        var aimPoint = primaryTarget.Position;
        var effRange = radius + primaryTarget.HitboxRadius;
        var effRsq = effRange * effRange;
        return p =>
        {
            var toTarget = aimPoint - p;
            var lenSq = toTarget.LengthSq();
            if (lenSq > effRsq)
            {
                return 0;
            }

            var dir = toTarget / MathF.Sqrt(lenSq);
            var countInCone = 0;
            for (var i = 0; i < count; ++i)
            {
                var t = targets[i];
                if (t.pos.InCircleCone(p, radius + t.radius, dir, halfAngle))
                {
                    ++countInCone;
                }
            }

            return countInCone;
        };
    }

    public Func<WPos, float> GoalAOERect(Actor primaryTarget, float lenFront, float halfWidth, float lenBack = default)
    {
        var priority = PriorityTargetsSpan;
        var count = priority.Length;
        var targets = new (WPos pos, float radius)[count];
        for (var i = 0; i < count; ++i)
        {
            var e = priority[i];
            targets[i] = (e.Actor.Position, e.Actor.HitboxRadius);
        }
        var aimPoint = primaryTarget.Position;
        var effRange = lenFront + primaryTarget.HitboxRadius;
        var effRsq = effRange * effRange;

        return p =>
        {
            var toTarget = aimPoint - p;
            var lenSq = toTarget.LengthSq();
            if (lenSq > effRsq)
            {
                return 0;
            }

            var dir = toTarget / MathF.Sqrt(lenSq);

            var countInRect = 0;
            for (var i = 0; i < count; ++i)
            {
                if (targets[i].pos.InRect(p, dir, lenFront, lenBack, halfWidth))
                {
                    ++countInRect;
                }
            }

            return countInRect;
        };
    }

    // combined goal zone: returns 'aoe' priority if targets hit are at or above minimum, otherwise returns 'single-target' priority
    public static Func<WPos, float> GoalCombined(Func<WPos, float> singleTarget, Func<WPos, float> aoe, int minAOETargets)
    {
        if (minAOETargets >= 50)
        {
            return singleTarget; // assume aoe is never efficient, so don't bother
        }

        return p =>
        {
            var aoeTargets = aoe(p) - minAOETargets;
            return aoeTargets >= 0 ? 3f + aoeTargets : singleTarget(p);
        };
    }

    // goal zone that returns a value between 0 and weight depending on distance to point; useful for downtime movement targets
    public static Func<WPos, float> GoalProximity(WPos destination, float maxDistance, float maxWeight)
    {
        var maxDistSq = maxDistance * maxDistance;
        var invDistSq = 1f / maxDistSq;

        return p =>
        {
            var delta = p - destination;
            var distSq = delta.LengthSq();

            var weight = 1f - Math.Clamp(invDistSq * distSq, 0f, 1f);
            return maxWeight * weight;
        };
    }
    public static Func<WPos, float> GoalProximity(Actor target, float range, float weight = 1f) => GoalProximity(target.Position, range + target.HitboxRadius, weight);

    public static Func<WPos, float> GoalDonut(WPos center, float innerRadius, float outerRadius, float weight = 1f)
    {
        var innerR = Math.Max(0f, innerRadius);
        var outerR = Math.Max(innerR + 1f, outerRadius);
        var innerSQ = innerR * innerR;
        var outerSQ = outerR * outerR;
        return p =>
        {
            var distSq = (p - center).LengthSq();
            return distSq <= innerSQ || distSq >= outerSQ ? default : weight;
        };
    }

    public Func<WPos, float> PullTargetToLocation(Actor target, WPos destination, Actor player, float gcd, float destRadius = 2f)
    {
        var enemy = FindEnemy(target);
        if (enemy == null)
        {
            return _ => 0;
        }

        var adjRange = enemy.TankDistance + target.HitboxRadius + player.HitboxRadius;
        var dirToGoal = destination - target.Position;
        var distToGoal = dirToGoal.Length() + adjRange;
        var leewaySq = destRadius * destRadius;

        // try to stay within pull range
        if (dirToGoal.LengthSq() <= leewaySq)
            return GoalSingleTarget(target.Position, adjRange, 0.1f);

        var distance = distToGoal;
        if (gcd < 0.5f)
        {
            var playerEffRange = player.Role is Role.Tank or Role.Melee ? 3 : 25;
            distToGoal = Math.Min(distToGoal, target.HitboxRadius + player.HitboxRadius + playerEffRange);
        }

        var sh = new SDPrecisePosition(target.Position + dirToGoal.Normalized() * distToGoal, new(0f, 1f), PathfindMapBounds.MapResolution, player.Position, 0.1f);
        return p => sh.Distance(p) >= 0f ? 10f : 0f;
    }

    public static Func<WPos, float> GoalRectangle(WPos center, WDir direction, float halfWidth, float halfHeight, float weight = 1f)
    {
        var fwd = direction.Normalized();
        var right = fwd.OrthoR();
        return p =>
        {
            var offset = p - center;
            var localX = fwd.Dot(offset);
            var localY = right.Dot(offset);
            return Math.Abs(localX) <= halfHeight && Math.Abs(localY) <= halfWidth ? weight : default;
        };
    }
}
