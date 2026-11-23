namespace BossMod.Components;

// generic component that shows arbitrary shapes representing avoidable aoes
public abstract class GenericAOEs(BossModule module, uint aid = default, string warningText = "GTFO from aoe!") : CastCounter(module, aid)
{
    public struct AOEInstance(AOEShape shape, WPos origin, Angle rotation = default, DateTime activation = default, uint color = default, bool risky = true, ulong actorID = default, ShapeDistance? shapeDistance = null, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = false)
    {
        public AOEShape Shape = shape;
        public WPos Origin = origin;
        public Angle Rotation = rotation;
        public DateTime Activation = activation;
        public uint Color = color;
        public bool Risky = risky;
        public ulong ActorID = actorID;
        public ShapeDistance? ShapeDistance = shapeDistance;
        // Null = current/default floor unless RestrictToArenaProjectionLayer is null (all layers).
        // Non-null selects ArenaBoundsCustom.WorldProjectionLayers[index] for the 2D stencil and 3D projection.
        public int? ArenaProjectionLayer = arenaProjectionLayer;
        // True: restrict 2D/AI visibility to Shared2DGroup and participants to the exact physical floor.
        // False: legacy unrestricted visibility/participants, still drawn and AI-clipped to the authored floor.
        // Null: apply on every floor, ignore ArenaProjectionLayer, and project a 3D copy onto each floor.
        public bool? RestrictToArenaProjectionLayer = restrictToArenaProjectionLayer;

        public readonly bool Check(WPos pos) => Shape.Check(pos, Origin, Rotation);
    }

    public readonly string WarningText = warningText;

    public abstract ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var aoes = ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Risky && aoe.Check(actor.Position)
                && ArenaProjectionLayerParticipantApplies(actor, aoe.ArenaProjectionLayer, aoe.RestrictToArenaProjectionLayer))
            {
                hints.Add(WarningText);
                return;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var aoes = ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var c = ref aoes[i];
            if (ArenaProjectionLayerApplies(actor, c.ArenaProjectionLayer, c.RestrictToArenaProjectionLayer) && c.Risky)
            {
                hints.AddForbiddenZone(c.ShapeDistance ?? c.Shape.Distance(c.Origin, c.Rotation), c.Activation,
                    arenaProjectionLayer: ArenaProjectionLayerForAI(c.ArenaProjectionLayer, c.RestrictToArenaProjectionLayer));
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var aoes = ActiveAOEs(pcSlot, pc);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var c = ref aoes[i];
            using (Arena.WorldProjectionLayer(c.ArenaProjectionLayer, c.RestrictToArenaProjectionLayer))
            {
                c.Shape.Draw(Arena, c.Origin, c.Rotation, c.Color == default ? default : c.Color);
            }
        }
    }

    internal static int IndexOfClosestLayer(ReadOnlySpan<float> values, float y)
    {
        var bestIndex = 0;
        var bestDiff = Math.Abs(values[0] - y);
        var len = values.Length;
        for (var i = 1; i < len; ++i)
        {
            var diff = Math.Abs(values[i] - y);

            if (diff < bestDiff)
            {
                bestDiff = diff;
                bestIndex = i;
            }
        }
        return bestIndex;
    }

    public static int IndexOfClosestAOE(List<AOEInstance> aoes, WPos position, AOEShape? shape = null)
    {
        var bestIndex = -1;
        var bestDistSq = float.MaxValue;
        var span = CollectionsMarshal.AsSpan(aoes);
        var len = span.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref span[i];
            if (shape != null && aoe.Shape != shape)
            {
                continue;
            }

            var distSq = (aoe.Origin - position).LengthSq();
            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                bestIndex = i;
            }
        }
        return bestIndex;
    }
}

// For simple AOEs, formerly known as SelfTargetedAOEs and LocationTargetedAOEs, that happens at the end of the cast
public class SimpleAOEs(BossModule module, uint aid, AOEShape shape, int maxCasts = int.MaxValue, double riskyWithSecondsLeft = default, float[]? arenaProjectionLayers = null, bool? restrictToArenaProjectionLayer = true, int? arenaProjectionLayer = null) : GenericAOEs(module, aid)
{
    public SimpleAOEs(BossModule module, uint aid, float radius, int maxCasts = int.MaxValue, double riskyWithSecondsLeft = default, float[]? arenaProjectionLayers = null, bool? restrictToArenaProjectionLayer = true, int? arenaProjectionLayer = null)
        : this(module, aid, new AOEShapeCircle(radius), maxCasts, riskyWithSecondsLeft, arenaProjectionLayers, restrictToArenaProjectionLayer, arenaProjectionLayer) { }
    public readonly AOEShape Shape = shape;
    public int MaxCasts = maxCasts; // used for staggered aoes, when showing all active would be pointless
    public uint Color; // can be customized if needed
    public bool Risky = true; // can be customized if needed
    public int? MaxDangerColor;
    public int? MaxRisky; // set a maximum amount of AOEs that are considered risky
    public readonly double RiskyWithSecondsLeft = riskyWithSecondsLeft; // can be used to delay risky status of AOEs, so AI waits longer to dodge, if 0 it will just use the bool Risky
    public float[]? ArenaProjectionLayers = arenaProjectionLayers; // y value of each layer
    public int? ArenaProjectionLayer = arenaProjectionLayer; // explicit ID takes precedence over height-based selection
    public bool? RestrictToArenaProjectionLayer = restrictToArenaProjectionLayer;

    public readonly List<AOEInstance> Casters = [];

    protected int? ResolveArenaProjectionLayer(float y)
        => !RestrictToArenaProjectionLayer.HasValue ? null : ArenaProjectionLayer
            ?? (ArenaProjectionLayers is { Length: > 0 } layers ? IndexOfClosestLayer(layers, y) : null);

    public ReadOnlySpan<AOEInstance> ActiveCasters
    {
        get
        {
            var count = Casters.Count;
            var max = count > MaxCasts ? MaxCasts : count;
            return CollectionsMarshal.AsSpan(Casters)[..max];
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            return [];
        }

        var time = WorldState.CurrentTime;
        var max = count > MaxCasts ? MaxCasts : count;
        var hasMaxDangerColor = count > MaxDangerColor;

        var aoes = CollectionsMarshal.AsSpan(Casters);
        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];
            var color = (hasMaxDangerColor && i < MaxDangerColor) ? Colors.Danger : Color;
            var risky = Risky && (MaxRisky == null || i < MaxRisky);

            if (RiskyWithSecondsLeft != default)
            {
                risky &= aoe.Activation.AddSeconds(-RiskyWithSecondsLeft) <= time;
            }
            aoe.Color = color;
            aoe.Risky = risky;
        }
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var origin = spell.LocXZ;
            var rotation = spell.Rotation;
            var layer = ResolveArenaProjectionLayer(spell.Location.Y);
            Casters.Add(new(Shape, origin, rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, shapeDistance: Shape.Distance(origin, rotation),
                arenaProjectionLayer: layer, restrictToArenaProjectionLayer: RestrictToArenaProjectionLayer));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var count = Casters.Count;
            var id = caster.InstanceID;
            var aoes = CollectionsMarshal.AsSpan(Casters);
            for (var i = 0; i < count; ++i)
            {
                if (aoes[i].ActorID == id)
                {
                    Casters.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

// 'charge at location' aoes that happen at the end of the cast
public class ChargeAOEs(BossModule module, uint aid, float halfWidth, int maxCasts = int.MaxValue, double riskyWithSecondsLeft = default, float extraLengthFront = default, float[]? arenaProjectionLayers = null, bool? restrictToArenaProjectionLayer = true, int? arenaProjectionLayer = null) : SimpleAOEs(module, aid, new AOEShapeCircle(default), maxCasts, riskyWithSecondsLeft, arenaProjectionLayers, restrictToArenaProjectionLayer, arenaProjectionLayer)
{
    public readonly float HalfWidth = halfWidth;
    public readonly float ExtraLengthFront = extraLengthFront;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var dir = spell.LocXZ - caster.Position;
            var shape = new AOEShapeRect(dir.Length() + ExtraLengthFront, HalfWidth);
            var origin = caster.Position.Quantized();
            var rotation = Angle.FromDirection(dir);
            Casters.Add(new(shape, origin, rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, shapeDistance: shape.Distance(origin, rotation),
                arenaProjectionLayer: ResolveArenaProjectionLayer(spell.Location.Y), restrictToArenaProjectionLayer: RestrictToArenaProjectionLayer));
        }
    }
}

// For simple AOEs where multiple AOEs use the same AOEShape
public class SimpleAOEGroups(BossModule module, uint[] aids, AOEShape shape, int maxCasts = int.MaxValue, int expectedNumCasters = 99, double riskyWithSecondsLeft = default, float[]? arenaProjectionLayers = null, bool? restrictToArenaProjectionLayer = true, int? arenaProjectionLayer = null)
        : SimpleAOEs(module, default, shape, maxCasts, riskyWithSecondsLeft, arenaProjectionLayers, restrictToArenaProjectionLayer, arenaProjectionLayer)
{
    public SimpleAOEGroups(BossModule module, uint[] aids, float radius, int maxCasts = int.MaxValue, int expectedNumCasters = 99, double riskyWithSecondsLeft = default, float[]? arenaProjectionLayers = null, bool? restrictToArenaProjectionLayer = true, int? arenaProjectionLayer = null)
        : this(module, aids, new AOEShapeCircle(radius), maxCasts, expectedNumCasters, riskyWithSecondsLeft, arenaProjectionLayers, restrictToArenaProjectionLayer, arenaProjectionLayer) { }

    protected readonly uint[] AIDs = aids;
    protected readonly int ExpectedNumCasters = expectedNumCasters;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var len = AIDs.Length;
        var id = spell.Action.ID;
        for (var i = 0; i < len; ++i)
        {
            if (id == AIDs[i])
            {
                var loc = spell.LocXZ;
                var rotation = spell.Rotation;
                var layer = ResolveArenaProjectionLayer(spell.Location.Y);
                Casters.Add(new(Shape, loc, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, shapeDistance: Shape.Distance(loc, rotation),
                    arenaProjectionLayer: layer, restrictToArenaProjectionLayer: RestrictToArenaProjectionLayer));
                if (Casters.Count >= ExpectedNumCasters)
                {
                    SortHelpers.SortAOEByActivation(Casters);
                }
                return;
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        // we probably dont need to check for AIDs here since actorID should already be unique to any active spell
        var count = Casters.Count;
        var id = caster.InstanceID;
        var aoes = CollectionsMarshal.AsSpan(Casters);
        for (var i = 0; i < count; ++i)
        {
            if (aoes[i].ActorID == id)
            {
                Casters.RemoveAt(i);
                return;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var len = AIDs.Length;
        var id = spell.Action.ID;
        for (var i = 0; i < len; ++i)
        {
            if (id == AIDs[i])
            {
                ++NumCasts;
                return;
            }
        }
    }
}

// For simple AOEs where multiple AOEs use the same AOEShape and are grouped by activation time, expectedNumCasters sorts Casters by activation when number is reached
// set to correct amount if sorting is needed (eg skills with different activation times start at the same time)
// useful if the amount of casts in a group of AOEs can vary
public class SimpleAOEGroupsByTimewindow(BossModule module, uint[] aids, AOEShape shape, double timeWindowInSeconds = 1d, int expectedNumCasters = 99, double riskyWithSecondsLeft = default, float[]? arenaProjectionLayers = null, bool? restrictToArenaProjectionLayer = true, int? arenaProjectionLayer = null)
    : SimpleAOEGroups(module, aids, shape, maxCasts: int.MaxValue, expectedNumCasters, riskyWithSecondsLeft, arenaProjectionLayers, restrictToArenaProjectionLayer, arenaProjectionLayer)
{
    public SimpleAOEGroupsByTimewindow(BossModule module, uint[] aids, float radius, double timeWindowInSeconds = 1d, int expectedNumCasters = 99, double riskyWithSecondsLeft = default, float[]? arenaProjectionLayers = null, bool? restrictToArenaProjectionLayer = true, int? arenaProjectionLayer = null)
        : this(module, aids, new AOEShapeCircle(radius), timeWindowInSeconds, expectedNumCasters, riskyWithSecondsLeft, arenaProjectionLayers, restrictToArenaProjectionLayer, arenaProjectionLayer) { }

    protected readonly double TimeWindowInSeconds = timeWindowInSeconds;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(Casters);
        var deadline = aoes[0].Activation.AddSeconds(TimeWindowInSeconds);

        var index = 0;
        while (index < count)
        {
            ref var aoe = ref aoes[index];
            if (aoe.Activation >= deadline)
            {
                break;
            }
            ++index;
        }
        if (RiskyWithSecondsLeft != default)
        {
            var time = WorldState.CurrentTime;
            for (var i = 0; i < index; ++i)
            {
                ref var aoe = ref aoes[i];
                aoe.Risky = aoe.Activation.AddSeconds(-RiskyWithSecondsLeft) <= time;
            }
        }
        return aoes[..index];
    }
}

public class SimpleChargeAOEGroups(BossModule module, uint[] aids, float halfWidth, int maxCasts = int.MaxValue, int expectedNumCasters = 99, double riskyWithSecondsLeft = 0d, float extraLengthFront = 0f, float[]? arenaProjectionLayers = null, bool? restrictToArenaProjectionLayer = true, int? arenaProjectionLayer = null) : SimpleAOEGroups(module, aids, 0f, maxCasts, expectedNumCasters, riskyWithSecondsLeft, arenaProjectionLayers, restrictToArenaProjectionLayer, arenaProjectionLayer)
{
    private readonly float HalfWidth = halfWidth;
    private readonly float ExtraLengthFront = extraLengthFront;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var len = AIDs.Length;
        var id = spell.Action.ID;
        for (var i = 0; i < len; ++i)
        {
            if (id == AIDs[i])
            {
                var dir = spell.LocXZ - caster.Position;
                var shape = new AOEShapeRect(dir.Length() + ExtraLengthFront, HalfWidth);
                var origin = caster.Position.Quantized();
                var rotation = Angle.FromDirection(dir);
                Casters.Add(new(shape, origin, rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, shapeDistance: shape.Distance(origin, rotation),
                    arenaProjectionLayer: ResolveArenaProjectionLayer(spell.Location.Y), restrictToArenaProjectionLayer: RestrictToArenaProjectionLayer));
                if (Casters.Count == ExpectedNumCasters)
                {
                    SortHelpers.SortAOEByActivation(Casters);
                }
                return;
            }
        }
    }
}

public class ProximityAOEs(BossModule module, uint aid, float radius, float[]? arenaProjectionLayers = null, bool? restrictToArenaProjectionLayer = true, int? arenaProjectionLayer = null) : SimpleAOEs(module, aid, radius, arenaProjectionLayers: arenaProjectionLayers, restrictToArenaProjectionLayer: restrictToArenaProjectionLayer, arenaProjectionLayer: arenaProjectionLayer)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        var count = Casters.Count;
        if (count != 0)
        {
            // Keep the first damage prediction for each affected floor, including simultaneous casts.
            // Unrestricted and all-layer casts share one prediction
            HashSet<int?> seenLayers = [];
            var casters = CollectionsMarshal.AsSpan(Casters);
            var raid = Raid.WithSlot();
            var len = raid.Length;
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref casters[i];
                var layer = aoe.RestrictToArenaProjectionLayer == true ? aoe.ArenaProjectionLayer : null;
                if (!seenLayers.Add(layer))
                {
                    continue;
                }
                BitMask affected = default;
                for (var j = 0; j < len; ++j)
                {
                    var member = raid[j];
                    if (ArenaProjectionLayerParticipantApplies(member.Item2, aoe.ArenaProjectionLayer, aoe.RestrictToArenaProjectionLayer))
                    {
                        affected.Set(member.Item1);
                    }
                }
                hints.AddPredictedDamage(affected, aoe.Activation);
            }
        }
    }
}
