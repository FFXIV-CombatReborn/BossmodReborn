namespace BossMod.Global.CrucibleOfTheUnbroken.FirstMasterBoard.CorpseFlowerPiece;

public enum OID : uint
{
    CorpseFlowerPiece = 0x4CBD,
    Helper = 0x233C,
    SaplingPiece = 0x4CBE, // R0.750, x0 (spawn during fight)
    QueenHawkPiece = 0x4CBF, // R0.720, x0 (spawn during fight)
    ThornPuddle = 0x1EC0E2, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 49681, // CorpseFlowerPiece->player, no cast, single-target
    AutoAttackQueenHawk = 48688, // 4CBF->player, no cast, single-target
    BuddingThorns = 48687, // CorpseFlowerPiece->self, 3.0s cast, single-target
    RottenStenchVisual = 48690, // CorpseFlowerPiece->self, 5.0+1.0s cast, single-target
    RottenStench = 48691, // CorpseFlowerPiece->self, no cast, range 45 width 12 rect
    FloralTrap = 48683, // CorpseFlowerPiece->self, 5.0s cast, range 80 circle
    FloralTrapPull = 48684, // CorpseFlowerPiece->self, no cast, range 45 ?-degree cone
    Devour = 48685, // CorpseFlowerPiece->self, no cast, range 8 ?-degree cone
    AcidRainBoss = 48692, // CorpseFlowerPiece->self, 3.0s cast, single-target
    AcidRainStart = 48693, // Helper->location, 2.0s cast, range 6 circle
    AcidRainRest = 48694, // Helper->location, no cast, range 6 circle
    FinalSting = 48689, // 4CBF->player, 5.0s cast, single-target
    Spit = 48686, // CorpseFlowerPiece->self, 4.0s cast, range 0 ???
}

public enum SID : uint
{
    Bind = 2518, // CorpseFlowerPiece->player, extra=0x0
    Stun = 2656, // CorpseFlowerPiece->4CBF, extra=0x0
    Briar = 5176, // none->player, extra=0x32
    DamageUp = 2550, // none->CorpseFlowerPiece, extra=0x1
    Devoured = 421, // CorpseFlowerPiece->player, extra=0x0
}

public enum IconID : uint
{
    FloralTrapLockOn = 703, // player->self
    RottenStenchWildCharge = 525, // CorpseFlowerPiece->player
    AcidRainLockOn = 197, // player->self
}

// TODO consider making components for growable circles since they are so common and annoying to work with, DateTime, SID etc
public class FloralTrap(BossModule module) : Components.CastCounter(module, (uint)AID.FloralTrap)
{
    private readonly List<Actor> puddles = []; // Used since the puddles will spawn, but not do the EAnim instantly - could be combined with growPuddlesTimers
    private readonly AOEShapeCircle shape = new(5.0f);
    private readonly Dictionary<Actor, DateTime> growPuddlesTimers = [];

    private DateTime InvertResolveAt;
    private readonly int? ArenaProjectionLayer = null;
    private readonly bool? RestrictToArenaProjectionLayer = false;

    private bool Inverted => InvertResolveAt != default;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ThornPuddle)
        {
            puddles.Add(actor);
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.ThornPuddle && state == 0x00100020u)
        {
            growPuddlesTimers[actor] = WorldState.CurrentTime;
        }
    }

    public override void OnActorEState(Actor actor, ushort state)
    {
        if (actor.OID == (uint)OID.ThornPuddle && state == 4)
        {
            puddles.Remove(actor);
            growPuddlesTimers.Remove(actor);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FloralTrap)
        {
            InvertResolveAt = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Devour)
        {
            InvertResolveAt = default;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!ArenaProjectionLayerParticipantApplies(actor, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            return;
        }

        var inVoidzone = false;
        foreach (var s in puddles)
        {
            if (ArenaProjectionLayerParticipantApplies(s, ArenaProjectionLayer, RestrictToArenaProjectionLayer) && ShapeSize(s).Check(actor.Position, s))
            {
                inVoidzone = true;
                break;
            }
        }

        if (Inverted)
        {
            hints.Add(inVoidzone ? "Stay in voidzone" : "Go to voidzone!", !inVoidzone);
        }
        else if (inVoidzone)
        {
            hints.Add("GTFO from voidzone!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!ArenaProjectionLayerApplies(actor, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            return;
        }

        var shapes = new List<ShapeDistance>();
        foreach (var source in puddles)
        {
            if (ArenaProjectionLayerParticipantApplies(source, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
            {
                var shape = ShapeSize(source).Distance(source.Position.Quantized(), source.Rotation);
                shapes.Add(shape);
            }
        }

        if (shapes.Count == 0)
        {
            return;
        }

        hints.AddForbiddenZone(Inverted ? new SDInvertedUnion([.. shapes]) : new SDUnion([.. shapes]), InvertResolveAt, arenaProjectionLayer: ArenaProjectionLayerForAI(ArenaProjectionLayer, RestrictToArenaProjectionLayer));
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var color = Inverted ? Colors.SafeFromAOE : default;
        using (Arena.WorldProjectionLayer(ArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            foreach (var s in puddles)
            {
                if (ArenaProjectionLayerParticipantApplies(s, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
                    ShapeSize(s).Draw(Arena, s.Position, s.Rotation, color);
            }
        }
    }

    private AOEShapeCircle ShapeSize(Actor source)
    {
        if (!growPuddlesTimers.TryGetValue(source, out var growPuddle))
        {
            return shape;
        }

        var duration = (WorldState.CurrentTime - growPuddle).TotalSeconds;
        // base + (max grow - base) * (time happened / max growth time to reach max size)
        return new((float)Math.Min(5.0f + (10.0f - 5.0f) * (duration / 10.0f), 10.0f));
    }
}

sealed class RottenStench(BossModule module) : Components.GenericWildCharge(module, 6f, (uint)AID.RottenStenchVisual)
{
    private Actor? target;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.RottenStenchWildCharge)
        {
            var targetPlayer = WorldState.Actors.Find(targetID);
            if (targetPlayer == null)
            {
                return;
            }

            target = targetPlayer;
            InitIfReady();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Source = caster;
            Activation = Module.CastFinishAt(spell);
            InitIfReady();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.RottenStench)
        {
            target = null;
            Source = null;
        }
    }

    private void InitIfReady()
    {
        if (target == null || Source == null)
        {
            return;
        }

        foreach (var (slot, player) in Raid.WithSlot(false, true, true))
        {
            PlayerRoles[slot] = player.InstanceID == target.InstanceID ? PlayerRole.Target : PlayerRole.Share;
        }
    }
}

sealed class AcidRainBait(BossModule module) : Components.BaitAwayIcon(module, 6f, (uint)IconID.AcidRainLockOn)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AcidRainStart)
        {
            CurrentBaits.Clear();
        }
    }
}

sealed class AcidRain(BossModule module) : Components.StandardChasingAOEs(module, 6.0f, (uint)AID.AcidRainStart, (uint)AID.AcidRainRest, 5f, 1d, 8,
    icon: (uint)IconID.AcidRainLockOn)
{

    // Since the activation is 1.0 seconds between each, we make the player move ahead of 2 casts
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Chasers.Count;
        for (var i = 0; i < count; ++i)
        {
            var c = Chasers[i];
            if (!ArenaProjectionLayerApplies(actor, c.ResolveArenaProjectionLayer(Module), c.RestrictToArenaProjectionLayer))
            {
                continue;
            }

            var predicted = c.PredictedPosition();
            if (c.Shape is AOEShapeCircle circle)
            {
                var isTarget = c.Target == actor && ArenaProjectionLayerParticipantApplies(actor, c.ResolveArenaProjectionLayer(Module), c.RestrictToArenaProjectionLayer);
                var radius = circle.Radius;
                var radiusAdj = isTarget ? MoveDistance + radius : radius + 1f;
                hints.AddForbiddenZone(new SDCircle(predicted, radiusAdj), c.NextActivation);

                if (isTarget)
                {
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Sprint), actor, ActionQueue.Priority.High);
                }

                if (c.NumRemaining > 1)
                {
                    hints.AddForbiddenZone(new SDCircle(predicted, radiusAdj + MoveDistance), c.NextActivation.AddSeconds(c.SecondsBetweenActivations));
                }
            }
            else
            {
                var offset = predicted - c.PrevPos;
                var distance = c.Shape.Distance(predicted, offset.LengthSq() > 0f ? Angle.FromDirection(offset) : default);
                hints.AddForbiddenZone(distance, c.NextActivation);
            }
        }
    }
}

sealed class FinalSting(BossModule module) : Components.SingleTargetCast(module, (uint)AID.FinalSting, "Final Sting on highest enmity");

// TODO add AI to bait the bee? Need a rotation logic to taunt it tho as the pet might have aggro
sealed class Devour(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(7.0f, 20.0f.Degrees()), (uint)IconID.FloralTrapLockOn)
{
    private DateTime waitTime = default; // Used for devour eventcast as its an animation that plays out
    private readonly AOEShapeCircle circleAI = new(9.0f); // Used by AIHints to ensure the player just move backwards out of it

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Devour)
        {
            waitTime = WorldState.FutureTime(2.5f);
        }
    }

    // We should avoid actually getting hit by the bait as well
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var baits = CollectionsMarshal.AsSpan(ActiveBaits);
        var count = baits.Length;
        if (count == 0)
        {
            return;
        }

        for (var i = 0; i < count; i++)
        {
            ref var bait = ref baits[i];
            hints.AddForbiddenZone(circleAI, bait.Source.Position, bait.Rotation, bait.Activation);
        }
    }

    public override void Update()
    {
        base.Update();

        if (waitTime != default && WorldState.CurrentTime > waitTime)
        {
            if (CurrentBaits.Count > 0)
            {
                CurrentBaits.RemoveAt(0);
            }

            waitTime = default;
        }
    }
}

sealed class CorpseFlowerPieceStates : StateMachineBuilder
{
    public CorpseFlowerPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FinalSting>()
            .ActivateOnEnter<FloralTrap>()
            .ActivateOnEnter<RottenStench>()
            .ActivateOnEnter<AcidRainBait>()
            .ActivateOnEnter<AcidRain>()
            .ActivateOnEnter<Devour>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.CorpseFlowerPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1091u, NameID = 14603u, SortOrder = 3)]
public sealed class CorpseFlowerPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.QueenHawkPiece => 2,
                (uint)OID.CorpseFlowerPiece => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.QueenHawkPiece), Colors.Vulnerable);
    }

    private readonly string[] _prePullHints = [
        "Fight kill order priority: QueenHawkPiece (purple) -> Boss (red)",
        "You can use the FloralTrap pull-in to have the boss kill the QueenHawkPiece if you stand behind it during the mechanic"
    ];

    public override string[] PrePullHints => _prePullHints;
}
