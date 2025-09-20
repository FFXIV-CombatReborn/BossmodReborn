using BossMod.Global.DeepDungeon;

namespace BossMod.Shadowbringers.Alliance.A24TheCompound2P;

sealed class ThreePartsDisdainStack(BossModule module) : Components.GenericStackSpread(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ThreePartsDisdain1 && WorldState.Actors.Find(spell.TargetID) is Actor t)
        {
            Stacks.Add(new(t, 6f, 24, 24, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ThreePartsDisdain3)
        {
            Stacks.Clear();
        }
    }

    public override void Update()
    {
        if (Stacks.Count != 0) // assume dead target means stack is cancelled
        {
            ref readonly var stack = ref Stacks.Ref(0);
            if (stack.Target.IsDead)
            {
                Stacks.Clear();
            }
        }
    }
}

sealed class ThreePartsDisdainKnockback(BossModule module) : Components.GenericKnockback(module)
{
    private Actor? target;
    private readonly DateTime[] activation = new DateTime[3];
    private readonly A24TheCompound2P bossmod = (A24TheCompound2P)module;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (target == null)
        {
            return [];
        }

        if (actor.Position.InCircle(target.Position, 6f))
        {
            var primaryPos = bossmod.BossP2!.Position;
            var count = 3 - NumCasts;
            var knockback = new Knockback[count];
            for (var i = 0; i < count; ++i)
            {
                knockback[i] = new Knockback(primaryPos, i != count - 1 ? 8f : 12f, activation[i], direction: Angle.FromDirection(target.Position - primaryPos), kind: Kind.DirForward, ignoreImmunes: true);
            }
            return knockback;
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ThreePartsDisdain1 && WorldState.Actors.Find(spell.TargetID) is Actor t)
        {
            target = t;
            var act = Module.CastFinishAt(spell);
            for (var i = 0; i < 3; ++i)
            {
                activation[i] = act.AddSeconds(i == 1 ? i * 1.2d : i == 2 ? 1.4d : default);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ThreePartsDisdain1 or (uint)AID.ThreePartsDisdain2 or (uint)AID.ThreePartsDisdain3)
        {
            if (++NumCasts == 3) // TODO: find out what happens if stack target dies after first hit...
            {
                target = null;
                NumCasts = 0;
            }
        }
    }

    public override void Update()
    {
        if (target != null && target.IsDead) // assume dead target means stack is cancelled
        {
            target = null;
            NumCasts = 0;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var knockback = ActiveKnockbacks(slot, actor);
        if (knockback.Length != 0)
        {
            ref readonly var kb = ref knockback[0];
            var loc = kb.Origin;
            var distTarget = 28f - NumCasts * 8f;
            var destDir = kb.Direction.ToDirection();
            var distStackers = NumCasts < 2 ? 8f : 12f;
            var destPos = loc + distStackers * destDir;
            var act = kb.Activation.AddSeconds(1d);
            if (actor != target)
            {
                hints.AddForbiddenZone(new SDKnockbackFixedDirectionIntoCircle(distStackers * destDir, destPos, 6f), act); // we want to stay inside stack
            }
            else // if we are bait target we have more freedom
            {
                hints.AddForbiddenZone(new SDKnockbackInAABBSquareAwayFromOrigin(Arena.Center, loc, distTarget, 29f), act);
            }
        }
    }
}
