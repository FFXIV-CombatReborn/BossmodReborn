namespace BossMod.Dawntrail.Dungeon.D11MesoTerminal.D112FourHeadsmen;

public enum OID : uint
{
    PaleHeadsman = 0x4891, // R2.72
    RavenousHeadsman = 0x4892, // R2.72
    PestilentHeadsman = 0x4893, // R2.72
    BloodyHeadsman = 0x4890, // R2.72
    HoodedHeadsman = 0x49E1, // R1.0
    SwordOfJustice = 0x4894, // R1.0
    Hellmaker = 0x48D2, // R2.5
    Flail = 0x1EBE41, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 43598, // PaleHeadsman->player, no cast, single-target
    AutoAttack2 = 43599, // BloodyHeadsman->player, no cast, single-target
    AutoAttack3 = 43600, // RavenousHeadsman/PestilentHeadsman->player, no cast, single-target
    Teleport = 43580, // BloodyHeadsman/PaleHeadsman/RavenousHeadsman/PestilentHeadsman->location, no cast, single-target

    LawlessPursuit = 43577, // BloodyHeadsman/PaleHeadsman/RavenousHeadsman/PestilentHeadsman->self, 3.0s cast, single-target
    HeadSplittingRoarVisual = 43578, // BloodyHeadsman/PaleHeadsman/RavenousHeadsman/PestilentHeadsman->self, 5.0s cast, single-target
    HeadSplittingRoar = 43579, // HoodedHeadsman->self, 5.5s cast, range 60 circle

    ShacklesOfFateVisual = 43581, // PaleHeadsman/RavenousHeadsman/PestilentHeadsman/BloodyHeadsman->player, 4.0s cast, single-target
    ShacklesOfFate1 = 43582, // Helper->player, 1.0s cast, single-target, pull into circle
    ShacklesOfFate2 = 43583, // Helper->player, no cast, single-target, if attempting to leave circle early

    DismembermentVisual = 43586, // PaleHeadsman/BloodyHeadsman/RavenousHeadsman/PestilentHeadsman->self, 3.0s cast, single-target
    Dismemberment = 43587, // Helper->self, 6.0s cast, range 16 width 4 rect
    PealOfJudgmentVisual = 43593, // PaleHeadsman/BloodyHeadsman/RavenousHeadsman/PestilentHeadsman->self, 3.0s cast, single-target
    PealOfJudgment = 43594, // Helper->self, no cast, range 2 width 4 rect
    ExecutionWheel = 43596, // PaleHeadsman/BloodyHeadsman/RavenousHeadsman/PestilentHeadsman->self, 3.5s cast, range 4-9 donut
    FlayingFlailVisual = 43591, // PaleHeadsman/BloodyHeadsman/RavenousHeadsman/PestilentHeadsman->self, 3.0s cast, single-target
    FlayingFlail = 43592, // Helper->location, 5.0s cast, range 5 circle
    DeathPenalty = 43588, // BloodyHeadsman->player, 5.0s cast, single-target
    WillBreaker = 44856, // PaleHeadsman->player, 7.0s cast, single-target, interruptible tank buster
    RelentlessTormentVisual = 43589, // PaleHeadsman->player, 5.0s cast, single-target, tankbuster x3
    RelentlessTorment = 43590, // Helper->player, no cast, single-target
    SerialTorture = 43597, // BloodyHeadsman/RavenousHeadsman/PaleHeadsman/PestilentHeadsman->self, 3.0s cast, single-target
    ChoppingBlock = 43595, // BloodyHeadsman/RavenousHeadsman/PaleHeadsman/PestilentHeadsman->self, 3.5s cast, range 6 circle

    PrisonerDied = 43584, // BloodyHeadsman/PestilentHeadsman/PaleHeadsman/RavenousHeadsman->self, no cast, single-target
    PrisonerRechained = 43585 // BloodyHeadsman/PestilentHeadsman/PaleHeadsman/RavenousHeadsman->self, no cast, single-target
}

public enum SID : uint
{
    CellBlockAlpha = 4542, // none->player, extra=0x0
    CellBlockBeta = 4543, // none->player, extra=0x0
    CellBlockGamma = 4544, // none->player, extra=0x0
    CellBlockDelta = 4545, // none->player, extra=0x0
    Doom = 4594, // BloodyHeadsman->player, extra=0x0
    FugitiveFromJustice = 4550, // none->player, extra=0x0
    Mechanic = 2552 // none->PaleHeadsman/RavenousHeadsman/PestilentHeadsman/BloodyHeadsman/SwordOfJustice, extra=0x394/0x3A3
}

public enum TetherID : uint
{
    CellBlock = 249 // BloodyHeadsman/PaleHeadsman/RavenousHeadsman/PestilentHeadsman->player
}

sealed class Prisons(BossModule module) : BossComponent(module)
{
    private readonly List<DonutV> borders = new(4);
    private readonly CellBlock _cell = module.FindComponent<CellBlock>()!;
    private bool prisonsDisabled;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Mechanic && status.Extra == 0x394)
        {
            borders.Add(new(actor.Position.Rounded(0.5f), 8f, 8.25f, 40)); // not sure if position needs to be rounded, depends on how the prison is actually implemented
            UpdateArena();
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Mechanic && status.Extra == 0x394)
        {
            var count = borders.Count;
            var pos = actor.Position.Rounded(0.5f);
            for (var i = 0; i < count; ++i)
            {
                if (borders[i].Center == pos)
                {
                    borders.RemoveAt(i);
                    UpdateArena();
                    return;
                }
            }
        }
    }

    private void UpdateArena() => Arena.Bounds = borders.Count == 0 ? D112FourHeadsmen.DefaultBounds : new ArenaBoundsCustom(D112FourHeadsmen.DefaultRect, [.. D112FourHeadsmen.Differences, .. borders]);

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_cell.AssignedBoss[pcSlot] == null)
        {
            if (Arena.Bounds != D112FourHeadsmen.DefaultBounds)
            {
                Arena.Bounds = D112FourHeadsmen.DefaultBounds;
                prisonsDisabled = true;
            }
        }
        else if (prisonsDisabled)
        {
            UpdateArena();
            prisonsDisabled = false;
        }
    }
}

sealed class CellBlock(BossModule module) : Components.GenericAOEs(module)
{
    public readonly Actor?[] AssignedBoss = new Actor?[4];
    private readonly D112FourHeadsmen bossmod = (D112FourHeadsmen)module;
    public readonly List<AOEInstance> _aoes = new(2);
    private readonly AOEShapeRect square = new(8f, 8f, 8f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => AssignedBoss[slot] == null ? CollectionsMarshal.AsSpan(_aoes) : [];

    public override void OnEventEnvControl(byte index, uint state) // Hellmaker AOEs, only get drawn if not chained anymore
    {
        WPos pos = index switch
        {
            0x19 => new(42.5f, -258f),
            0x1A => new(77.5f, -258f),
            _ => default
        };
        if (pos != default)
        {
            if (state == 0x00020001u)
            {
                _aoes.Add(new(square, pos));
            }
            else if (state == 0x00200004u)
            {
                var aoes = CollectionsMarshal.AsSpan(_aoes);
                var len = aoes.Length;
                for (var i = 0; i < len; ++i)
                {
                    if (aoes[i].Origin == pos)
                    {
                        _aoes.RemoveAt(i);
                        return;
                    }
                }
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (AssignedBoss[slot] is var assignedSlot && assignedSlot != null && WorldState.Actors.Find(actor.TargetID) is Actor target)
        {
            if (target != assignedSlot && target.OID is (uint)OID.PestilentHeadsman or (uint)OID.BloodyHeadsman or (uint)OID.PaleHeadsman or (uint)OID.RavenousHeadsman)
            {
                hints.Add($"Target {assignedSlot?.Name}!");
            }
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.CellBlock && Raid.FindSlot(source.InstanceID) is var slot && slot >= 0)
        {
            AssignedBoss[slot] = WorldState.Actors.Find(tether.Target);
        }
    }

    // fall back since players outside arena bounds do not get tethered but will still receive status effects
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var id = status.ID;
        var boss = id switch
        {
            (uint)SID.CellBlockAlpha => bossmod.BloodyHeadsman,
            (uint)SID.CellBlockBeta => Module.PrimaryActor,
            (uint)SID.CellBlockDelta => bossmod.PestilentHeadsman,
            (uint)SID.CellBlockGamma => bossmod.RavenousHeadsman,
            _ => null
        };
        if (boss != null && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            AssignedBoss[slot] = boss;
        }
        else if (id == (uint)SID.FugitiveFromJustice && Raid.FindSlot(actor.InstanceID) is var slot2 && slot2 >= 0)
        {
            AssignedBoss[slot2] = null;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        var assigned = AssignedBoss[slot];
        for (var i = 0; i < count; ++i)
        {
            var enemy = hints.PotentialTargets[i];
            var a = enemy.Actor;
            if (a.OID == (uint)OID.Hellmaker && (a.Position - actor.Position).LengthSq() < 324f) // can attack them from any prison, but it doesn't seem needed
            {
                enemy.Priority = 1;
                if (actor.Role == Role.Melee)
                {
                    hints.GoalZones.Add(hints.GoalSingleTarget(a, 3f, 99f)); // needed for RSR since it won't attack targets outside melee range on melees
                }
            }
            else if (assigned != null && a != assigned)
            {
                enemy.Priority = AIHints.Enemy.PriorityInvincible;
            }
        }
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

sealed class DismembermentExecutionWheelFlayingFlailChoppingBlock(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeDonut donut = new(4f, 9f);
    private readonly AOEShapeCircle circleSmall = new(5f), circleBig = new(6f);
    private readonly AOEShapeRect rect = new(16f, 2f);

    private readonly List<AOEInstance> _aoes = new(7);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var deadline = aoes[0].Activation.AddSeconds(2d);

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
        return aoes[..index];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.Dismemberment => rect,
            (uint)AID.ExecutionWheel => donut,
            (uint)AID.FlayingFlail => circleSmall,
            (uint)AID.ChoppingBlock => circleBig,
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var count = _aoes.Count;
        var id = caster.InstanceID;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        for (var i = 0; i < count; ++i)
        {
            if (aoes[i].ActorID == id)
            {
                _aoes.RemoveAt(i);
                return;
            }
        }
    }
}

sealed class HeadSplittingRoar(BossModule module) : Components.RaidwideCast(module, (uint)AID.HeadSplittingRoar);
sealed class DeathPenalty(BossModule module) : Components.SingleTargetCast(module, (uint)AID.DeathPenalty, "Healerbuster");
sealed class Doom(BossModule module) : Components.CleansableDebuff(module, (uint)SID.Doom);
sealed class WillBreaker(BossModule module) : Components.CastInterruptHint(module, (uint)AID.WillBreaker, showNameInHint: true);
sealed class RelentlessTorment(BossModule module) : Components.SingleTargetCastDelay(module, (uint)AID.RelentlessTormentVisual, (uint)AID.RelentlessTorment, 0.4d, "Tankbuster x3")
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            if (++NumCasts == 3)
            {
                Targets.Clear();
                NumCasts = 0;
            }
        }
    }

    public override void Update()
    {
        if (Targets.Count != 0)
        {
            ref var t = ref Targets.Ref(0);
            if (t.target.IsDead || t.caster.IsDead)
            {
                Targets.Clear();
            }
        }
    }
}

sealed class PealOfJudgment(BossModule module) : Components.Exaflare(module, new AOEShapeRect(2f, 2f))
{
    private readonly D112FourHeadsmen bossmod = (D112FourHeadsmen)module;
    private readonly List<Actor> swords = module.Enemies((uint)OID.SwordOfJustice);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.SwordOfJustice)
        {
            // first hit moves only 0.24118 units, remaining move 0.3222 units
            var rot = actor.Rotation;
            Lines.Add(new(actor.Position, 0.24118f * rot.ToDirection(), WorldState.FutureTime(5.5d), 0.2d, 51, 12, rot));
        }
    }

    public override void OnActorDeath(Actor actor)
    {
        var count = Lines.Count;
        if (count != 0 && (actor == bossmod.RavenousHeadsman || actor == bossmod.BloodyHeadsman || actor == bossmod.PestilentHeadsman || actor == Module.PrimaryActor))
        {
            var count2 = count - 1;
            var pos = actor.Position;
            for (var i = count2; i >= 0; --i)
            {
                if (Lines[i].Next.AlmostEqual(pos, 10f))
                {
                    Lines.RemoveAt(i);
                }
            }
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        var count = Lines.Count;
        if (count != 0 && actor.OID == (uint)OID.SwordOfJustice)
        {
            var pos = actor.Position;
            for (var i = 0; i < count; ++i)
            {
                if (Lines[i].Next.AlmostEqual(pos, 1f))
                {
                    Lines.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.PealOfJudgment)
        {
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f)) // 0.1 should suffice, but depending on the current server tick casts can be skipped since the time between steps is only 0.2s and an actor can't do more than one cast event per tick
                {
                    if (line.ExplosionsLeft == 50)
                    {
                        line.Advance = 0.3222f * caster.Rotation.ToDirection();
                    }
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0 || spell.Targets.Count != 0)
                    {
                        Lines.RemoveAt(i);
                    }
                    return;
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var count = swords.Count;
        for (var i = 0; i < count; ++i)
        {
            var s = swords[i];
            if (!s.IsDead)
            {
                hints.TemporaryObstacles.Add(new SDRect(s.Position, s.Rotation, 4f, default, 2f));
            }
        }
    }
}

sealed class D112FourHeadsmenStates : StateMachineBuilder
{
    public D112FourHeadsmenStates(D112FourHeadsmen module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CellBlock>()
            .ActivateOnEnter<Prisons>()
            .ActivateOnEnter<HeadSplittingRoar>()
            .ActivateOnEnter<DismembermentExecutionWheelFlayingFlailChoppingBlock>()
            .ActivateOnEnter<DeathPenalty>()
            .ActivateOnEnter<Doom>()
            .ActivateOnEnter<WillBreaker>()
            .ActivateOnEnter<RelentlessTorment>()
            .ActivateOnEnter<PealOfJudgment>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed && (module.RavenousHeadsman?.IsDeadOrDestroyed ?? true)
            && (module.PestilentHeadsman?.IsDeadOrDestroyed ?? true) && (module.BloodyHeadsman?.IsDeadOrDestroyed ?? true);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.PaleHeadsman, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1028u, NameID = 14049u, Category = BossModuleInfo.Category.Dungeon, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 2)]
public sealed class D112FourHeadsmen(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultBounds.Center, DefaultBounds)
{
    public Actor? RavenousHeadsman;
    public Actor? PestilentHeadsman;
    public Actor? BloodyHeadsman;

    public static readonly Rectangle[] Differences = [new(new(60f, -235.2f), 8f, 1.25f), new(new(29.3f, -258f), 8f, 1.25f, 89.98f.Degrees())];
    public static readonly Rectangle[] DefaultRect = [new Rectangle(new(60f, -258f), 29.5f, 22f)];
    public static readonly ArenaBoundsCustom DefaultBounds = new(DefaultRect, Differences);

    protected override void UpdateModule()
    {
        BloodyHeadsman ??= GetActor((uint)OID.BloodyHeadsman);
        RavenousHeadsman ??= GetActor((uint)OID.RavenousHeadsman);
        PestilentHeadsman ??= GetActor((uint)OID.PestilentHeadsman);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(RavenousHeadsman);
        Arena.Actor(PestilentHeadsman);
        Arena.Actor(BloodyHeadsman);
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Hellmaker), Colors.Object);
    }
}
