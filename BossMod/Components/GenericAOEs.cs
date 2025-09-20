namespace BossMod.Components;

// generic component that shows arbitrary shapes representing avoidable aoes
[SkipLocalsInit]
public abstract class GenericAOEs(BossModule module, uint aid = default, string warningText = "GTFO from aoe!") : CastCounter(module, aid)
{
    public struct AOEInstance(AOEShape shape, WPos origin, Angle rotation = default, DateTime activation = default, uint color = default, bool risky = true, ulong actorID = default)
    {
        public AOEShape Shape = shape;
        public WPos Origin = origin;
        public Angle Rotation = rotation;
        public DateTime Activation = activation;
        public uint Color = color;
        public bool Risky = risky;
        public ulong ActorID = actorID;

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
            if (aoe.Risky && aoe.Check(actor.Position))
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
            if (c.Risky)
            {
                hints.AddForbiddenZone(c.Shape, c.Origin, c.Rotation, c.Activation);
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
            c.Shape.Draw(Arena, c.Origin, c.Rotation, c.Color == default ? default : c.Color);
        }
    }
}

// For simple AOEs, formerly known as SelfTargetedAOEs and LocationTargetedAOEs, that happens at the end of the cast
[SkipLocalsInit]
public class SimpleAOEs(BossModule module, uint aid, AOEShape shape, int maxCasts = int.MaxValue, double riskyWithSecondsLeft = default) : GenericAOEs(module, aid)
{
    public SimpleAOEs(BossModule module, uint aid, float radius, int maxCasts = int.MaxValue, double riskyWithSecondsLeft = default) : this(module, aid, new AOEShapeCircle(radius), maxCasts, riskyWithSecondsLeft) { }
    public readonly AOEShape Shape = shape;
    public int MaxCasts = maxCasts; // used for staggered aoes, when showing all active would be pointless
    public uint Color; // can be customized if needed
    public bool Risky = true; // can be customized if needed
    public int? MaxDangerColor;
    public int? MaxRisky; // set a maximum amount of AOEs that are considered risky
    public readonly double RiskyWithSecondsLeft = riskyWithSecondsLeft; // can be used to delay risky status of AOEs, so AI waits longer to dodge, if 0 it will just use the bool Risky

    public readonly List<AOEInstance> Casters = [];

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
            Casters.Add(new(Shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
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
[SkipLocalsInit]
public class ChargeAOEs(BossModule module, uint aid, float halfWidth, int maxCasts = int.MaxValue, double riskyWithSecondsLeft = default, float extraLengthFront = default) : SimpleAOEs(module, aid, new AOEShapeCircle(default), maxCasts, riskyWithSecondsLeft)
{
    public readonly float HalfWidth = halfWidth;
    public readonly float ExtraLengthFront = extraLengthFront;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var dir = spell.LocXZ - caster.Position;
            Casters.Add(new(new AOEShapeRect(dir.Length() + ExtraLengthFront, HalfWidth), caster.Position.Quantized(), Angle.FromDirection(dir), Module.CastFinishAt(spell), actorID: caster.InstanceID));
        }
    }
}

// For simple AOEs where multiple AOEs use the same AOEShape
[SkipLocalsInit]
public class SimpleAOEGroups(BossModule module, uint[] aids, AOEShape shape, int maxCasts = int.MaxValue, int expectedNumCasters = 99, double riskyWithSecondsLeft = default) : SimpleAOEs(module, default, shape, maxCasts, riskyWithSecondsLeft)
{
    public SimpleAOEGroups(BossModule module, uint[] aids, float radius, int maxCasts = int.MaxValue, int expectedNumCasters = 99, double riskyWithSecondsLeft = default) : this(module, aids, new AOEShapeCircle(radius), maxCasts, expectedNumCasters, riskyWithSecondsLeft) { }

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
                Casters.Add(new(Shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
                if (Casters.Count >= ExpectedNumCasters)
                {
                    Casters.Sort((a, b) => a.Activation.CompareTo(b.Activation));
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
        for (var i = 0; i < len; ++i)
        {
            if (spell.Action.ID == AIDs[i])
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
[SkipLocalsInit]
public class SimpleAOEGroupsByTimewindow(BossModule module, uint[] aids, AOEShape shape, double timeWindowInSeconds = 1d, int expectedNumCasters = 99, double riskyWithSecondsLeft = default) : SimpleAOEGroups(module, aids, shape, maxCasts: int.MaxValue, expectedNumCasters, riskyWithSecondsLeft)
{
    public SimpleAOEGroupsByTimewindow(BossModule module, uint[] aids, float radius, double timeWindowInSeconds = 1d, int expectedNumCasters = 99, double riskyWithSecondsLeft = default) : this(module, aids, new AOEShapeCircle(radius), timeWindowInSeconds, expectedNumCasters, riskyWithSecondsLeft) { }

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

[SkipLocalsInit]
public class SimpleChargeAOEGroups(BossModule module, uint[] aids, float halfWidth, int maxCasts = int.MaxValue, int expectedNumCasters = 99, double riskyWithSecondsLeft = 0d, float extraLengthFront = 0f) : SimpleAOEGroups(module, aids, 0f, maxCasts, expectedNumCasters, riskyWithSecondsLeft)
{
    private readonly float HalfWidth = halfWidth;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var len = AIDs.Length;
        var id = spell.Action.ID;
        for (var i = 0; i < len; ++i)
        {
            if (id == AIDs[i])
            {
                var dir = spell.LocXZ - caster.Position;
                Casters.Add(new(new AOEShapeRect(dir.Length() + extraLengthFront, HalfWidth), caster.Position.Quantized(), Angle.FromDirection(dir), Module.CastFinishAt(spell), actorID: caster.InstanceID));
                if (Casters.Count == ExpectedNumCasters)
                {
                    Casters.Sort((a, b) => a.Activation.CompareTo(b.Activation));
                }
                return;
            }
        }
    }
}
