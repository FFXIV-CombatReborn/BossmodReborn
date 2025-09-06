namespace BossMod.Components;

// generic 'stack/spread' mechanic has some players that have to spread away from raid, some other players that other players need to stack with
// there are various variants (e.g. everyone should spread, or everyone should stack in one or more groups, or some combination of that)
public abstract class GenericStackSpread(BossModule module, bool alwaysShowSpreads = false, bool raidwideOnResolve = true, bool includeDeadTargets = false) : BossComponent(module)
{
    public struct Stack(Actor target, float radius, int minSize = 2, int maxSize = int.MaxValue, DateTime activation = default, BitMask forbiddenPlayers = default)
    {
        public Actor Target = target;
        public float Radius = radius;
        public int MinSize = minSize;
        public int MaxSize = maxSize;
        public DateTime Activation = activation;
        public BitMask ForbiddenPlayers = forbiddenPlayers; // raid members that aren't allowed to participate in the stack

        public readonly int NumInside(BossModule module)
        {
            var count = 0;
            var party = module.Raid.WithSlot();
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                var indexActor = party[i];
                if (!ForbiddenPlayers[indexActor.Item1] && indexActor.Item2.Position.InCircle(Target.Position.Quantized(), Radius))
                {
                    ++count;
                }
            }
            return count;
        }
        public readonly bool CorrectAmountInside(BossModule module) => NumInside(module) is var count && count >= MinSize && count <= MaxSize;
        public readonly bool InsufficientAmountInside(BossModule module) => NumInside(module) is var count && count < MaxSize;
        public readonly bool TooManyInside(BossModule module) => NumInside(module) is var count && count > MaxSize;
        public readonly bool IsInside(WPos pos) => pos.InCircle(Target.Position.Quantized(), Radius);
        public readonly bool IsInside(Actor actor) => IsInside(actor.Position);
    }

    public struct Spread(Actor target, float radius, DateTime activation = default)
    {
        public Actor Target = target;
        public float Radius = radius;
        public DateTime Activation = activation;
    }

    public readonly bool AlwaysShowSpreads = alwaysShowSpreads; // if false, we only shown own spread radius for spread targets - this reduces visual clutter
    public readonly bool RaidwideOnResolve = raidwideOnResolve; // if true, assume even if mechanic is correctly resolved everyone will still take damage
    public readonly bool IncludeDeadTargets = includeDeadTargets; // if false, stacks & spreads with dead targets are ignored
    public int ExtraAISpreadThreshold = 1;
    public readonly List<Stack> Stacks = [];
    public List<Spread> Spreads = [];
    public const string StackHint = "Stack!";

    public bool Active => Stacks.Count + Spreads.Count > 0;
    public List<Stack> ActiveStacks
    {
        get
        {
            if (IncludeDeadTargets)
                return Stacks;
            else
            {
                var count = Stacks.Count;
                var activeStacks = new List<Stack>(count);
                for (var i = 0; i < count; ++i)
                {
                    var stack = Stacks[i];
                    if (!stack.Target.IsDead)
                        activeStacks.Add(stack);
                }
                return activeStacks;
            }
        }
    }

    public List<Spread> ActiveSpreads
    {
        get
        {
            if (IncludeDeadTargets)
                return Spreads;
            else
            {
                var count = Spreads.Count;
                var activeSpreads = new List<Spread>(count);
                for (var i = 0; i < count; ++i)
                {
                    var spread = Spreads[i];
                    if (!spread.Target.IsDead)
                        activeSpreads.Add(spread);
                }
                return activeSpreads;
            }
        }
    }

    public bool IsStackTarget(Actor? actor)
    {
        var count = Stacks.Count;
        for (var i = 0; i < count; ++i)
        {
            if (Stacks[i].Target == actor)
                return true;
        }
        return false;
    }

    public bool IsSpreadTarget(Actor? actor)
    {
        var count = Spreads.Count;
        for (var i = 0; i < count; ++i)
        {
            if (Spreads[i].Target == actor)
                return true;
        }
        return false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Spreads.FindIndex(s => s.Target == actor) is var iSpread && iSpread >= 0)
        {
            hints.Add("Spread!", Raid.WithoutSlot().InRadiusExcluding(actor, Spreads[iSpread].Radius).Any());
        }
        else if (Stacks.FindIndex(s => s.Target == actor) is var iStack && iStack >= 0)
        {
            var stack = Stacks[iStack];
            var numStacked = 1; // always stacked with self
            var stackedWithOtherStackOrAvoid = false;
            foreach (var (j, other) in Raid.WithSlot().InRadiusExcluding(actor, stack.Radius))
            {
                ++numStacked;
                stackedWithOtherStackOrAvoid |= stack.ForbiddenPlayers[j] || IsStackTarget(other);
            }
            hints.Add(StackHint, stackedWithOtherStackOrAvoid || numStacked < stack.MinSize || numStacked > stack.MaxSize);
        }
        else
        {
            var numParticipatingStacks = 0;
            var numUnsatisfiedStacks = 0;
            foreach (var s in ActiveStacks.Where(s => !s.ForbiddenPlayers[slot]))
            {
                if (actor.Position.InCircle(s.Target.Position.Quantized(), s.Radius))
                    ++numParticipatingStacks;
                else if (Raid.WithoutSlot().InRadiusExcluding(s.Target, s.Radius).Count() + 1 < s.MinSize)
                    ++numUnsatisfiedStacks;
            }

            if (numParticipatingStacks > 1)
                hints.Add(StackHint);
            else if (numParticipatingStacks == 1)
                hints.Add(StackHint, false);
            else if (numUnsatisfiedStacks > 0)
                hints.Add(StackHint);
            // else: don't show anything, all potential stacks are already satisfied without a player
            //hints.Add("Stack!", ActiveStacks.Count(s => !s.ForbiddenPlayers[slot] && actor.Position.InCircle(s.Target.Position, s.Radius)) != 1);
        }

        if (ActiveSpreads.Any(s => s.Target != actor && actor.Position.InCircle(s.Target.Position.Quantized(), s.Radius)))
        {
            hints.Add("GTFO from spreads!");
        }
        else if (ActiveStacks.Any(s => s.Target != actor && s.ForbiddenPlayers[slot] && actor.Position.InCircle(s.Target.Position.Quantized(), s.Radius)))
        {
            hints.Add("GTFO from forbidden stacks!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.Info == null || !(Module.Info.MultiboxSupport && Service.Config.Get<BossMod.AI.AIConfig>().MultiboxMode))
        {
            // forbid standing next to spread markers
            // TODO: think how to improve this, current implementation works, but isn't particularly good - e.g. nearby players tend to move to same spot, turn around, etc.
            // ideally we should provide per-mechanic spread spots, but for simple cases we should try to let melee spread close and healers/rdd spread far from main target...
            foreach (var spreadFrom in ActiveSpreads.Where(s => s.Target != actor))
                hints.AddForbiddenZone(ShapeDistance.Circle(spreadFrom.Target.Position.Quantized(), spreadFrom.Radius + ExtraAISpreadThreshold), spreadFrom.Activation);
            foreach (var spreadFrom in ActiveSpreads.Where(s => s.Target == actor))
                foreach (var x in Raid.WithoutSlot())
                    if (!ActiveSpreads.Any(s => s.Target == x))
                        hints.AddForbiddenZone(ShapeDistance.Circle(x.Position.Quantized(), spreadFrom.Radius + ExtraAISpreadThreshold), spreadFrom.Activation);
            foreach (var avoid in ActiveStacks.Where(s => s.Target != actor && (s.ForbiddenPlayers[slot] || !s.IsInside(actor) && (s.CorrectAmountInside(Module) || s.TooManyInside(Module)) || s.IsInside(actor) && s.TooManyInside(Module))))
                hints.AddForbiddenZone(ShapeDistance.Circle(avoid.Target.Position.Quantized(), avoid.Radius), avoid.Activation);

            if (Stacks.FirstOrDefault(s => s.Target == actor) is var actorStack && actorStack.Target != null)
            {
                // forbid standing next to other stack markers or overlapping them
                foreach (var stackWith in ActiveStacks.Where(s => s.Target != actor))
                    hints.AddForbiddenZone(ShapeDistance.Circle(stackWith.Target.Position.Quantized(), stackWith.Radius * 2), stackWith.Activation);
                // if player got stackmarker and is playing with NPCs, go to a NPC to stack with them since they will likely not come to you
                if (Raid.WithoutSlot().Any(x => x.Type == ActorType.Buddy))
                {
                    var forbidden = new List<Func<WPos, float>>();
                    foreach (var stackWith in ActiveStacks.Where(s => s.Target == actor))
                        forbidden.Add(ShapeDistance.InvertedCircle(Raid.WithoutSlot().FirstOrDefault(x => !x.IsDead && !IsSpreadTarget(x) && !IsStackTarget(x))!.Position, actorStack.Radius * 0.33f));
                    if (forbidden.Count > 0)
                        hints.AddForbiddenZone(ShapeDistance.Intersection(forbidden), actorStack.Activation);
                }
            }
            else if (!IsSpreadTarget(actor) && !IsStackTarget(actor))
            {
                var forbidden = new List<Func<WPos, float>>();
                foreach (var s in ActiveStacks.Where(x => !x.ForbiddenPlayers[slot] && (x.IsInside(actor) && !x.TooManyInside(Module)
                || !x.IsInside(actor) && x.InsufficientAmountInside(Module))))
                    forbidden.Add(ShapeDistance.InvertedCircle(s.Target.Position.Quantized(), s.Radius - 0.25f));
                if (forbidden.Count > 0)
                    hints.AddForbiddenZone(ShapeDistance.Intersection(forbidden), ActiveStacks.FirstOrDefault().Activation);
            }
        }

        if (RaidwideOnResolve)
        {
            var firstActivation = DateTime.MaxValue;

            var countSpread = ActiveSpreads.Count;
            var countStack = ActiveStacks.Count;
            if (countSpread != 0)
            {
                BitMask spreadMask = default;

                var spreads = CollectionsMarshal.AsSpan(ActiveSpreads);
                for (var i = 0; i < countSpread; ++i)
                {
                    ref var s = ref spreads[i];
                    spreadMask.Set(Raid.FindSlot(s.Target.InstanceID));
                    firstActivation = firstActivation < s.Activation ? firstActivation : s.Activation;
                }
                if (spreadMask != default)
                {
                    hints.AddPredictedDamage(spreadMask, firstActivation, AIHints.PredictedDamageType.Raidwide);
                }
            }
            if (countStack != 0)
            {
                BitMask stackMask = default;
                var stacks = CollectionsMarshal.AsSpan(ActiveStacks);
                BitMask mask = default;
                mask.Raw = 0xFFFFFFFFFFFFFFFF;
                for (var i = 0; i < countStack; ++i)
                {
                    ref var s = ref stacks[i];
                    stackMask |= mask & ~s.ForbiddenPlayers; // assume everyone will take damage except forbidden players (so-so assumption really...)
                    firstActivation = firstActivation < s.Activation ? firstActivation : s.Activation;
                }
                if (stackMask != mask)
                {
                    hints.AddPredictedDamage(stackMask, firstActivation, AIHints.PredictedDamageType.Shared);
                }
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        var shouldSpread = IsSpreadTarget(player);
        var shouldStack = IsStackTarget(player);
        var shouldAvoid = !shouldSpread && !shouldStack && ActiveStacks.Any(s => s.ForbiddenPlayers[playerSlot]);
        if (shouldAvoid)
            customColor = Colors.Vulnerable;
        return shouldAvoid || shouldSpread ? PlayerPriority.Danger
            : shouldStack ? PlayerPriority.Interesting
            : Active ? PlayerPriority.Normal : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        void DrawCircle(WPos position, float radius, uint color = 0) => Arena.AddCircle(position, radius, color);
        if (!AlwaysShowSpreads && Spreads.FindIndex(s => s.Target == pc) is var iSpread && iSpread >= 0)
        {
            // Draw only own circle if spreading; no one should be inside.
            DrawCircle(pc.Position.Quantized(), Spreads[iSpread].Radius);
        }
        else
        {
            // Handle safe stack circles
            foreach (var s in ActiveStacks.Where(x => x.Target == pc || !x.ForbiddenPlayers[pcSlot]
                    && !IsSpreadTarget(pc) && !IsStackTarget(pc) && (x.IsInside(pc)
                    && !x.TooManyInside(Module) || !x.IsInside(pc) && x.InsufficientAmountInside(Module))))
                DrawCircle(s.Target.Position.Quantized(), s.Radius, Colors.Safe);

            // Handle dangerous stack circles
            foreach (var s in ActiveStacks.Where(x => x.Target != pc && (IsStackTarget(pc) || x.ForbiddenPlayers[pcSlot] || IsSpreadTarget(pc) ||
                !x.IsInside(pc) && (x.CorrectAmountInside(Module) || x.TooManyInside(Module)) ||
                x.IsInside(pc) && x.TooManyInside(Module))))
                DrawCircle(s.Target.Position.Quantized(), s.Radius);

            // Handle spread circles
            foreach (var s in ActiveSpreads)
                DrawCircle(s.Target.Position.Quantized(), s.Radius);
        }
    }
}

// stack/spread with same properties for all stacks and all spreads (most common variant)
public abstract class UniformStackSpread(BossModule module, float stackRadius, float spreadRadius, int minStackSize = 2, int maxStackSize = int.MaxValue, bool alwaysShowSpreads = false, bool raidwideOnResolve = true, bool includeDeadTargets = false)
    : GenericStackSpread(module, alwaysShowSpreads, raidwideOnResolve, includeDeadTargets)
{
    public float StackRadius = stackRadius;
    public float SpreadRadius = spreadRadius;
    public int MinStackSize = minStackSize;
    public int MaxStackSize = maxStackSize;

    public void AddStack(Actor target, DateTime activation = default, BitMask forbiddenPlayers = default) => Stacks.Add(new(target, StackRadius, MinStackSize, MaxStackSize, activation, forbiddenPlayers));
    public void AddStacks(IEnumerable<Actor> targets, DateTime activation = default) => Stacks.AddRange(targets.Select(target => new Stack(target, StackRadius, MinStackSize, MaxStackSize, activation)));
    public void AddSpread(Actor target, DateTime activation = default) => Spreads.Add(new(target, SpreadRadius, activation));
    public void AddSpreads(IEnumerable<Actor> targets, DateTime activation = default) => Spreads.AddRange(targets.Select(target => new Spread(target, SpreadRadius, activation)));
}

// spread/stack mechanic that selects targets by casts
public class CastStackSpread(BossModule module, uint stackAID, uint spreadAID, float stackRadius, float spreadRadius, int minStackSize = 2, int maxStackSize = int.MaxValue, bool alwaysShowSpreads = false)
    : UniformStackSpread(module, stackRadius, spreadRadius, minStackSize, maxStackSize, alwaysShowSpreads)
{
    public readonly uint StackAction = stackAID;
    public readonly uint SpreadAction = spreadAID;
    public int NumFinishedStacks;
    public int NumFinishedSpreads;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id == StackAction && WorldState.Actors.Find(spell.TargetID) is Actor stackTarget)
        {
            AddStack(stackTarget, Module.CastFinishAt(spell));
        }
        else if (id == SpreadAction && WorldState.Actors.Find(spell.TargetID) is Actor spreadTarget)
        {
            AddSpread(spreadTarget, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var aid = spell.Action.ID;
        if (aid == StackAction)
        {
            var count = Stacks.Count;
            var id = spell.TargetID;
            var stacks = CollectionsMarshal.AsSpan(Stacks);
            for (var i = 0; i < count; ++i)
            {
                ref var stack = ref stacks[i];
                if (stack.Target.InstanceID == id)
                {
                    ++NumFinishedStacks;
                    Stacks.RemoveAt(i);
                    return;
                }
            }
        }
        else if (aid == SpreadAction)
        {
            var count = Spreads.Count;
            var id = spell.TargetID;
            var spreads = CollectionsMarshal.AsSpan(Spreads);
            for (var i = 0; i < count; ++i)
            {
                ref var spread = ref spreads[i];
                if (spread.Target.InstanceID == id)
                {
                    ++NumFinishedSpreads;
                    Spreads.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

// generic 'spread from targets of specific cast' mechanic
public class SpreadFromCastTargets(BossModule module, uint aid, float radius, bool drawAllSpreads = true) : CastStackSpread(module, default, aid, default, radius, alwaysShowSpreads: drawAllSpreads);

// generic 'stack with targets of specific cast' mechanic
public class StackWithCastTargets(BossModule module, uint aid, float radius, int minStackSize = 2, int maxStackSize = int.MaxValue) : CastStackSpread(module, aid, default, radius, default, minStackSize, maxStackSize);

// spread/stack mechanic that selects targets by icon and finishes by cast event
public class IconStackSpread(BossModule module, uint stackIcon, uint spreadIcon, uint stackAID, uint spreadAID, float stackRadius, float spreadRadius, double activationDelay, int minStackSize = 2, int maxStackSize = int.MaxValue, bool alwaysShowSpreads = false, int maxCasts = 1)
    : UniformStackSpread(module, stackRadius, spreadRadius, minStackSize, maxStackSize, alwaysShowSpreads)
{
    public readonly uint StackIcon = stackIcon;
    public readonly uint SpreadIcon = spreadIcon;
    public readonly uint StackAction = stackAID;
    public readonly uint SpreadAction = spreadAID;
    public readonly double ActivationDelay = activationDelay;
    public int NumFinishedStacks;
    public int NumFinishedSpreads;
    public readonly int MaxCasts = maxCasts; // for stacks where the final AID hits multiple times
    public int CastCounter;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == StackIcon)
        {
            AddStack(actor, WorldState.FutureTime(ActivationDelay));
        }
        else if (iconID == SpreadIcon)
        {
            AddSpread(actor, WorldState.FutureTime(ActivationDelay));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var aid = spell.Action.ID;
        if (aid == StackAction)
        {
            var id = spell.MainTargetID;
            if (MaxCasts != 1 && Stacks.Count == 1 && Stacks.Ref(0).Target.InstanceID != id) // multi hit stack target died and new target got selected
            {
                Stacks.Ref(0).Target = WorldState.Actors.Find(id)!;
            }
            if (++CastCounter == MaxCasts)
            {
                var count = Stacks.Count;
                var stacks = CollectionsMarshal.AsSpan(Stacks);
                for (var i = 0; i < count; ++i)
                {
                    ref var stack = ref stacks[i];
                    if (stack.Target.InstanceID == id)
                    {
                        ++NumFinishedStacks;
                        CastCounter = 0;
                        Stacks.RemoveAt(i);
                        return;
                    }
                }
                // stack not found, probably due to being self targeted
                if (count != 0)
                {
                    ++NumFinishedStacks;
                    CastCounter = 0;
                    Stacks.RemoveAt(0);
                }
            }
        }
        else if (aid == SpreadAction)
        {
            var count = Spreads.Count;
            var id = spell.MainTargetID;
            var spreads = CollectionsMarshal.AsSpan(Spreads);
            for (var i = 0; i < count; ++i)
            {
                ref var spread = ref spreads[i];
                if (spread.Target.InstanceID == id)
                {
                    Spreads.RemoveAt(i);
                    ++NumFinishedSpreads;
                    return;
                }
            }
            // spread not found, probably due to being self targeted
            if (count != 0)
            {
                ++NumFinishedSpreads;
                Spreads.RemoveAt(0);
            }
        }
    }

    public override void Update()
    {
        var count = Spreads.Count - 1;
        for (var i = count; i >= 0; --i)
        {
            ref var spread = ref Spreads.Ref(i);
            if (spread.Target.IsDead)
            {
                Spreads.RemoveAt(i);
            }
        }
    }
}

// generic 'spread from actors with specific icon' mechanic
public class SpreadFromIcon(BossModule module, uint icon, uint aid, float radius, double activationDelay, bool drawAllSpreads = true) :
IconStackSpread(module, default, icon, default, aid, default, radius, activationDelay, alwaysShowSpreads: drawAllSpreads);

// generic 'stack with actors with specific icon' mechanic
public class StackWithIcon(BossModule module, uint icon, uint aid, float radius, double activationDelay, int minStackSize = 2, int maxStackSize = int.MaxValue, int maxCasts = 1) :
IconStackSpread(module, icon, default, aid, default, radius, default, activationDelay, minStackSize, maxStackSize, false, maxCasts);

// generic 'donut stack' mechanic
public class DonutStack(BossModule module, uint aid, uint icon, float innerRadius, float outerRadius, double activationDelay, int minStackSize = 2, int maxStackSize = int.MaxValue) :
UniformStackSpread(module, innerRadius / 3f, default, minStackSize, maxStackSize)
{
    // this is a donut targeted on each player, it is best solved by stacking
    // regular stack component won't work because this is self targeted
    public readonly AOEShapeDonut Donut = new(innerRadius, outerRadius);
    public readonly double ActivationDelay = activationDelay;
    public readonly uint Icon = icon;
    public readonly uint Aid = aid;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == Icon)
        {
            AddStack(actor, WorldState.FutureTime(ActivationDelay));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == Aid)
        {
            var count = Stacks.Count;
            var t = spell.MainTargetID;
            var stacks = CollectionsMarshal.AsSpan(Stacks);
            for (var i = 0; i < count; ++i)
            {
                ref var stack = ref stacks[i];
                if (stack.Target.InstanceID == t)
                {
                    Stacks.RemoveAt(i);
                    return;
                }
            }
            Stacks.Clear(); // stack was not found, just clear all - this can happen if donut stacks is self targeted instead of player targeted
        }
    }

    public override void Update()
    {
        var count = Stacks.Count - 1;
        for (var i = count; i >= 0; --i)
        {
            ref var stack = ref Stacks.Ref(i);
            if (stack.Target.IsDead)
            {
                Stacks.RemoveAt(i);
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Stacks.Count;
        if (count == 0)
            return;
        var forbidden = new List<Func<WPos, float>>(count);
        var radius = Donut.InnerRadius * 0.25f;
        for (var i = 0; i < count; ++i)
        {
            var s = Stacks[i];
            if (s.Target == actor)
            {
                continue;
            }
            forbidden.Add(ShapeDistance.InvertedCircle(s.Target.Position, radius));
        }
        if (forbidden.Count != 0)
        {
            hints.AddForbiddenZone(ShapeDistance.Intersection(forbidden), Stacks[0].Activation);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var count = Stacks.Count;
        for (var i = 0; i < count; ++i)
        {
            Donut.Draw(Arena, Stacks[i].Target.Position);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) { }
}

public abstract class GenericBaitStack(BossModule module, uint aid = default, bool onlyShowOutlines = false) : GenericBaitAway(module, aid)
{
    // TODO: add logic for min and max stack size
    public const string HintStack = "Stack!";
    public const string HintAvoidOther = "GTFO from other stacks!";
    public const string HintAvoid = "GTFO from stack!";

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var baits = CollectionsMarshal.AsSpan(ActiveBaits);
        var len = baits.Length;
        if (len == 0)
            return;
        var isBaitTarget = false; // determine if target of any stack
        for (var i = 0; i < len; ++i)
        {
            ref readonly var b = ref baits[i];
            if (b.Target == actor)
            {
                isBaitTarget = true;
                break;
            }
        }
        var forbiddenInverted = new List<Func<WPos, float>>();
        var forbidden = new List<Func<WPos, float>>();

        var hasBuddies = false;
        var p = Raid.WithoutSlot(false, true);
        var lenP = p.Length;
        for (var i = 0; i < lenP; ++i)
        {
            if (p[i].OID != default)
            {
                hasBuddies = true;
                break;
            }
        }

        for (var i = 0; i < len; ++i)
        {
            ref var b = ref baits[i];
            var origin = BaitOrigin(ref b);
            var angle = Angle.FromDirection(b.Target.Position - origin);
            if (b.Target != actor && !isBaitTarget)
            {
                if (Module.Info != null && Module.Info.MultiboxSupport && Service.Config.Get<BossMod.AI.AIConfig>().MultiboxMode)
                {
                    continue;
                }

                if (!b.Forbidden[slot])
                {
                    forbiddenInverted.Add(b.Shape.InvertedDistance(origin, angle));
                }
                else
                {
                    forbidden.Add(b.Shape.Distance(origin, angle));
                }
            }
            else if (b.Target != actor && isBaitTarget)
            {   // prevent overlapping if there are multiple stacks
                if (b.Shape is AOEShapeCone cone)
                {
                    forbidden.Add(ShapeDistance.Cone(origin, cone.Radius, angle, cone.HalfAngle * 2f));
                }
                else if (b.Shape is AOEShapeRect rect)
                {
                    forbidden.Add(ShapeDistance.Rect(origin, angle, rect.LengthFront, rect.LengthBack, rect.HalfWidth * 2f));
                }
                else if (b.Shape is AOEShapeCircle circle)
                {
                    forbiddenInverted.Add(ShapeDistance.Circle(origin, circle.Radius * 2f));
                }
            }
            else if (hasBuddies && b.Target == actor) // try to go to NPCs since they usually will not actively come to your stack
            {
                for (var j = 0; j < lenP; ++j)
                {
                    var member = p[j];
                    if (member.OID != default)
                    {
                        forbiddenInverted.Add(ShapeDistance.InvertedCircle(member.Position, 1f));
                    }
                }
            }
        }
        ref var bait = ref baits[0];
        if (forbiddenInverted.Count != 0)
            hints.AddForbiddenZone(ShapeDistance.Intersection(forbiddenInverted), bait.Activation);
        if (forbidden.Count != 0)
            hints.AddForbiddenZone(ShapeDistance.Union(forbidden), bait.Activation);

        var firstActivation = DateTime.MaxValue;
        BitMask baitMask = default;
        BitMask mask = default;
        mask.Raw = 0xFFFFFFFFFFFFFFFF;
        for (var i = 0; i < len; ++i)
        {
            ref var b = ref baits[i];
            baitMask |= mask & ~b.Forbidden; // assume everyone will take damage except forbidden players (so-so assumption really...)
            firstActivation = firstActivation < b.Activation ? firstActivation : b.Activation;
        }
        if (baitMask != default)
        {
            hints.AddPredictedDamage(baitMask, firstActivation, AIHints.PredictedDamageType.Shared);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var baits = CollectionsMarshal.AsSpan(ActiveBaits);
        var len = baits.Length;
        if (len == 0)
            return;
        var isBaitTarget = false; // determine if target of any stack
        var isInBaitShape = false; // determine if inside of any stack
        var isInWrongBait = false; // determine if inside of any forbidden stack
        var allForbidden = true; // determine if all stacks are forbidden
        var id = actor.InstanceID;
        for (var i = 0; i < len; ++i)
        {
            ref var b = ref baits[i];
            if (!b.Forbidden[slot])
            {
                allForbidden = false;
            }
            if (b.Target.InstanceID == id)
            {
                isBaitTarget = true;
                continue;
            }
            if (b.Shape.Check(actor.Position, BaitOrigin(ref b), b.Rotation))
            {
                isInBaitShape = true;
                if (b.Forbidden[slot])
                    isInWrongBait = true;
            }
        }
        var isBaitTargetAndInExtraStack = false;
        if (isBaitTarget)
        {
            for (var i = 0; i < len; ++i)
            {
                ref var b = ref baits[i];
                if (b.Target.InstanceID == id)
                    continue;
                if (b.Shape.Check(actor.Position, BaitOrigin(ref b), b.Rotation))
                {
                    isBaitTargetAndInExtraStack = true;
                    break;
                }
            }
        }
        if (isInWrongBait || allForbidden)
        {
            hints.Add(HintAvoid, isInWrongBait);
            return;
        }
        if (!isBaitTarget && !isInBaitShape)
        {
            hints.Add(HintStack);
        }
        else if (isBaitTarget || !isBaitTarget && isInBaitShape)
        {
            hints.Add(HintStack, false);
        }
        if (isBaitTarget && isBaitTargetAndInExtraStack)
        {
            hints.Add(HintAvoidOther);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (onlyShowOutlines)
            return;
        var baits = CollectionsMarshal.AsSpan(ActiveBaits);
        var len = baits.Length;
        if (len == 0)
            return;
        var isBaitTarget = false; // determine if target of any stack
        var id = pc.InstanceID;
        for (var i = 0; i < len; ++i)
        {
            ref var b = ref baits[i];
            if (b.Target.InstanceID == id)
            {
                isBaitTarget = true;
                break;
            }
        }
        for (var i = 0; i < len; ++i)
        {
            ref var b = ref baits[i];
            var color = !b.Forbidden[pcSlot] && (isBaitTarget && b.Target.InstanceID == id || !isBaitTarget && b.Target.InstanceID != id) ? Colors.SafeFromAOE : default;
            b.Shape.Draw(Arena, BaitOrigin(ref b), b.Rotation, color);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!onlyShowOutlines)
            return;
        var baits = CollectionsMarshal.AsSpan(ActiveBaits);
        var len = baits.Length;
        if (len == 0)
            return;
        var isBaitTarget = false; // determine if target of any stack
        var id = pc.InstanceID;
        for (var i = 0; i < len; ++i)
        {
            ref var b = ref baits[i];
            if (b.Target.InstanceID == id)
            {
                isBaitTarget = true;
                break;
            }
        }
        for (var i = 0; i < len; ++i)
        {
            ref var b = ref baits[i];
            var color = !b.Forbidden[pcSlot] && (isBaitTarget && b.Target.InstanceID == id || !isBaitTarget && b.Target.InstanceID != id) ? Colors.Safe : default;
            b.Shape.Outline(Arena, BaitOrigin(ref b), b.Rotation, color);
        }
    }
}

// generic single hit "line stack" component, usually do not have an iconID, instead players get marked by cast event
// usually these have 50 range and 4 halfWidth, but it can be modified
public class LineStack(BossModule module, uint aidMarker, uint aidResolve, double activationDelay = 5.1d, float range = 50f, float halfWidth = 4f, int minStackSize = 4, int maxStackSize = int.MaxValue, int maxCasts = 1, bool markerIsFinalTarget = true, uint iconID = default) : GenericBaitStack(module)
{
    public LineStack(BossModule module, uint iconID, uint aidResolve, double activationDelay = 5.1d, float range = 50f, float halfWidth = 4f, int minStackSize = 4, int maxStackSize = int.MaxValue, int maxCasts = 1, bool markerIsFinalTarget = true) : this(module, default, aidResolve, activationDelay, range, halfWidth, minStackSize, maxStackSize, maxCasts, markerIsFinalTarget, iconID) { }

    // TODO: add logic for min and max stack size
    public readonly uint AidMarker = aidMarker;
    public readonly uint AidResolve = aidResolve;
    public readonly double ActionDelay = activationDelay;
    public readonly float Range = range;
    public readonly float HalfWidth = halfWidth;
    public readonly int MaxStackSize = maxStackSize;
    public readonly int MinStackSize = minStackSize;
    public readonly int MaxCasts = maxCasts; // for stacks where the final AID hits multiple times
    public readonly bool MarkerIsFinalTarget = markerIsFinalTarget; // rarely the marked player is not the target of the line stack
    private int castCounter;
    public readonly uint IconId = iconID;
    private readonly AOEShape rect = new AOEShapeRect(range, halfWidth);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (AidMarker == default && IconId == default)
        {
            return;
        }
        var id = spell.Action.ID;
        if (id == AidMarker && WorldState.Actors.Find(spell.MainTargetID) is Actor target)
        {
            CurrentBaits.Add(new(caster, target, rect, WorldState.FutureTime(ActionDelay), maxCasts: MaxCasts));
        }
        else if (id == AidResolve)
        {
            if (MarkerIsFinalTarget)
            {
                var tID = spell.MainTargetID;
                if (CurrentBaits.Count == 1 && CurrentBaits.Ref(0).Target.InstanceID != tID && WorldState.Actors.Find(tID) is Actor t)
                {
                    CurrentBaits.Ref(0).Target = t;
                }

                var count = CurrentBaits.Count;
                var baits = CollectionsMarshal.AsSpan(CurrentBaits);
                for (var i = 0; i < count; ++i)
                {
                    ref var b = ref baits[i];
                    if (b.Target.InstanceID == tID && --CurrentBaits.Ref(i).MaxCasts == 0)
                    {
                        CurrentBaits.RemoveAt(i);
                        ++NumCasts;
                        return;
                    }
                }
            }
            else
            {
                if (++castCounter >= MaxCasts && CurrentBaits.Count != 0)
                {
                    CurrentBaits.RemoveAt(0);
                    castCounter -= MaxCasts;
                    ++NumCasts;
                }
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (IconId != default && iconID == IconId && WorldState.Actors.Find(targetID) is Actor target)
        {
            CurrentBaits.Add(new(actor, target, rect, WorldState.FutureTime(ActionDelay)));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (AidMarker != default || IconId != default)
            return;
        if (spell.Action.ID == AidResolve && WorldState.Actors.Find(spell.TargetID) is Actor target)
        {
            CurrentBaits.Add(new(caster, target, rect, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (AidMarker != default || IconId != default)
        {
            return;
        }
        if (spell.Action.ID == AidResolve)
        {
            var tID = spell.TargetID;
            if (MarkerIsFinalTarget)
            {
                if (CurrentBaits.Count == 1 && CurrentBaits[0].Target.InstanceID != tID && WorldState.Actors.Find(tID) is Actor t)
                {
                    CurrentBaits.Ref(0).Target = t;
                }
                if (++castCounter == MaxCasts)
                {
                    var count = CurrentBaits.Count;
                    var baits = CollectionsMarshal.AsSpan(CurrentBaits);
                    for (var i = 0; i < count; ++i)
                    {
                        ref var b = ref baits[i];
                        if (b.Target.InstanceID == tID)
                        {
                            CurrentBaits.RemoveAt(i);
                            castCounter = 0;
                            ++NumCasts;
                            return;
                        }
                    }
                }
            }
            else
            {
                if (++castCounter == MaxCasts && CurrentBaits.Count != 0)
                {
                    CurrentBaits.RemoveAt(0);
                    castCounter = 0;
                    ++NumCasts;
                }
            }
        }
    }
}
