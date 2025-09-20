namespace BossMod.Dawntrail.Quest.MSQ.TakingAStand;

public enum OID : uint
{
    Boss = 0x4211, // R=3.5
    BakoolJaJaShade = 0x4215, // R3.5
    Deathwall = 0x1EBA1F,
    MagickedStandardOrange = 0x4212, // R1.0
    MagickedStandardGreen = 0x4213, // R1.0
    GrowingAOE = 0x421D, // R1.0-3.58
    Garrote = 0x4214, // R1.0
    HiredThug1 = 0x4219, // R0.5
    HiredThug2 = 0x421A, // R0.5
    HiredThug3 = 0x421B, // R0.5
    HoobigoGuardian = 0x4216, // R1.0
    HoobigoLancer = 0x4217, // R1.0
    DopproIllusionist = 0x4218, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 871, // HoobigoLancer->player, no cast, single-target
    AutoAttack3 = 873, // HiredThug3->player, no cast, single-target
    Teleport = 37089, // Boss->location, no cast, single-target

    Roar1 = 37090, // Boss->self, 5.0s cast, range 45 circle, spawns death wall
    Roar2 = 37091, // Boss->self, 5.0s cast, range 45 circle
    LethalSwipe = 37092, // Boss->self, 6.0s cast, range 45 180-degree cone
    MagickedStandard = 37098, // Boss->self, 4.0s cast, single-target
    Kickdown = 37101, // Boss->player, 5.0s cast, single-target, knockback 18, away from source
    ArcaneActivationCircle = 37099, // MagickedStandardOrange->self, 4.0s cast, range 10 circle
    ArcaneActivationDonut = 37100, // MagickedStandardGreen->self, 4.0s cast, range 3-10 donut
    FireshowerVisual = 37751, // Boss->self, 3.0s cast, single-target
    Fireshower = 37802, // Helper->location, 4.0s cast, range 6 circle
    GreatLeapVisual = 37093, // Boss->self, 8.5s cast, single-target
    GreatLeap = 37094, // Boss->location, no cast, range 18 circle
    ShadowSiblings = 37103, // Boss->self, 4.0s cast, single-target
    LethalSwipeShade = 37104, // BakoolJaJaShade->self, 5.9+0,1s cast, range 10 circle
    SelfSacrifice = 37105, // BakoolJaJaShade->self, no cast, range 10 circle

    RiotousRampageVisual1 = 37095, // Boss->location, 5.9s cast, single-target
    RiotousRampageVisual2 = 37096, // Boss->location, no cast, single-target
    RiotousRampage = 36416, // Helper->self, 7.2s cast, range 4 circle
    HeavyImpact = 36444, // Helper->self, no cast, range 40 circle, tower fail
    SupremeStandard = 37106, // Boss->self, 4.0s cast, single-target
    SupremeStandardPull = 37107, // Helper->self, no cast, range 50 circle
    SupremeStandardModelChange = 37108, // Boss->self, 1.5s cast, single-target
    MightAndMagicVisual = 35333, // Boss->self, no cast, single-target, QTE, nothing of this can be dodged
    MightAndMagic = 15552, // Helper->self, 1.4s cast, range 50 width 10 rect
    MightAndMagic2 = 13270, // Helper->self, 8.1s cast, range 50 width 10 rect
    MightAndMagic3 = 13271, // Helper->self, 8.8s cast, range 50 width 10 rect
    MightAndMagicEnrage = 15554, // Helper->self, 2.3s cast, range 50 width 10 rect, QTE failed

    TeleportAdds = 34873, // HoobigoGuardian/HoobigoLancer->location, no cast, single-target
    RunThrough1 = 37111, // HoobigoLancer->self, 4.0s cast, range 45 width 5 rect
    RunThrough2 = 37110, // HoobigoGuardian->self, 4.0s cast, range 45 width 5 rect
    FirefloodVisual = 37112, // DopproIllusionist->self, 5.0s cast, single-target
    Fireflood = 37113, // Helper->location, 5.0s cast, range 40 circle, distances based AOE, guessed optimal range 18
    TuraliStoneIIIVisual = 37114, // DopproIllusionist->self, 4.0s cast, single-target
    TuraliStoneIII = 37115, // Helper->location, 3.0s cast, range 4 circle
    Desperation = 37119, // Boss->self, no cast, range 10 90-degree cone
    TuraliQuakeVisual = 37116, // DopproIllusionist->self, 4.0s cast, single-target
    TuraliQuake = 37117, // Helper->location, 5.0s cast, range 9 circle
    Fire = 966 // DopproIllusionist->player, 1.0s cast, single-target
}

sealed class RoarArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(20f, 25f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Deathwall)
        {
            _aoe = [];
            Arena.Bounds = TakingAStand.DefaultArena;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Roar1)
        {
            _aoe = [new(donut, Arena.Center, default, Module.CastFinishAt(spell, 0.9d))];
        }
    }
}

sealed class MagickedStandard(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(9);
    private static readonly AOEShapeCircle circle = new(10f);
    private static readonly AOEShapeDonut donut = new(3f, 10f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnActorCreated(Actor actor)
    {
        AOEShape? shape = actor.OID switch
        {
            (uint)OID.MagickedStandardGreen => donut,
            (uint)OID.MagickedStandardOrange => circle,
            _ => null
        };
        if (shape != null)
        {
            AOEs.Add(new(shape, actor.Position.Quantized(), default, WorldState.FutureTime(12.6d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ArcaneActivationCircle or (uint)AID.ArcaneActivationDonut)
        {
            AOEs.Clear();
        }
    }
}

sealed class GreatLeap(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime activation;
    private AOEInstance[] _aoe = [];
    private Actor? source;
    private bool aoeInit;
    private static readonly AOEShapeCircle circle = new(18);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (activation != default)
        {
            source ??= Module.Enemies((uint)OID.GrowingAOE) is var aoes && aoes.Count != 0 ? aoes[0] : null;
            if (source != null)
            {
                var pos = source.Position;
                if (!aoeInit)
                {
                    aoeInit = true;
                    return _aoe = [new(circle, pos, default, activation)];
                }
                else
                {
                    ref var aoe = ref _aoe[0];
                    aoe.Origin = pos;
                    return _aoe;
                }
            }
        }
        return [];
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GreatLeapVisual)
        {
            activation = Module.CastFinishAt(spell, 0.2d);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.GreatLeap)
        {
            activation = default;
            source = null;
            aoeInit = false;
        }
    }
}

sealed class SelfSacrifice(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(3);
    private static readonly AOEShapeCircle circle = new(10f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LethalSwipeShade)
        {
            _aoes.Add(new(circle, spell.LocXZ, default, Module.CastFinishAt(spell, 0.2d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SelfSacrifice)
        {
            _aoes.Clear();
        }
    }
}

sealed class Kickdown(BossModule module) : Components.GenericKnockback(module)
{
    private DateTime activation;
    private readonly MagickedStandard _aoe = module.FindComponent<MagickedStandard>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (activation != default)
        {
            return new Knockback[1] { new(Module.PrimaryActor.Position, 18f, activation) };
        }
        return [];
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoe.AOEs);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            if (aoes[i].Check(pos))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Kickdown)
        {
            activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Kickdown)
        {
            activation = default;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (activation == default)
        {
            return;
        }

        if (!IsImmune(slot, activation))
        {
            hints.AddForbiddenZone(new SDKnockbackInCircleAwayFromOriginMixedAOEs(Arena.Center, Module.PrimaryActor.Position, 18f, 20f, [.. _aoe.AOEs], _aoe.AOEs.Count), activation);
        }
    }
}

sealed class RiotousRampage(BossModule module) : Components.CastTowers(module, (uint)AID.RiotousRampage, 4f);
sealed class Roar(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.Roar1, (uint)AID.Roar2]);
sealed class LethalSwipe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LethalSwipe, new AOEShapeCone(45f, 90f.Degrees()));
sealed class Fireshower(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Fireshower, 6f);
sealed class RunThrough(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.RunThrough1, (uint)AID.RunThrough2], new AOEShapeRect(45f, 2.5f));
sealed class Fireflood(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Fireflood, 18f);
sealed class TuraliStoneIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TuraliStoneIII, 4f);
sealed class TuraliQuake(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TuraliQuake, 9f, maxCasts: 5);

sealed class AutoWukLamat(WorldState ws) : QuestBattle.UnmanagedRotation(ws, 3f)
{
    protected override void Exec(Actor? primaryTarget)
    {
        if (primaryTarget == null)
        {
            return;
        }

        if (World.Party.LimitBreakCur == 10000)
        {
            UseAction(Roleplay.AID.DawnlitConviction, primaryTarget, 100f);
        }

        var numAOETargets = 0;
        var count = Hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            if (Hints.PotentialTargets[i].Actor.Position.InCircle(primaryTarget.Position, 8f))
            {
                ++numAOETargets;
            }
        }

        var gcd = ComboAction switch
        {
            Roleplay.AID.ClawOfTheBraax => Roleplay.AID.FangsOfTheBraax,
            Roleplay.AID.FangsOfTheBraax => Roleplay.AID.TailOfTheBraax,
            Roleplay.AID.TuraliFervor => Roleplay.AID.TuraliJudgment,
            Roleplay.AID.TrialsOfTural => Roleplay.AID.TuraliFervor,
            _ => numAOETargets > 1 ? Roleplay.AID.TrialsOfTural : Roleplay.AID.ClawOfTheBraax
        };

        UseAction(gcd, primaryTarget);
        UseAction(Roleplay.AID.BeakOfTheLuwatena, primaryTarget, -5f);

        if (Player.DistanceToHitbox(primaryTarget) < 3f)
        {
            UseAction(Roleplay.AID.RunOfTheRroneek, primaryTarget, -10f);
        }

        if (Player.HPMP.CurHP * 2u < Player.HPMP.MaxHP)
        {
            UseAction(Roleplay.AID.LuwatenaPulse, Player, -10f);
        }
    }
}

sealed class WukLamatAI(BossModule module) : QuestBattle.RotationModule<AutoWukLamat>(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.BakoolJaJaShade => AIHints.Enemy.PriorityInvincible,
                _ => 0
            };
        }
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

sealed class TakingAStandStates : StateMachineBuilder
{
    public TakingAStandStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RoarArenaChange>()
            .ActivateOnEnter<Roar>()
            .ActivateOnEnter<MagickedStandard>()
            .ActivateOnEnter<Kickdown>()
            .ActivateOnEnter<RiotousRampage>()
            .ActivateOnEnter<LethalSwipe>()
            .ActivateOnEnter<Fireshower>()
            .ActivateOnEnter<GreatLeap>()
            .ActivateOnEnter<SelfSacrifice>()
            .ActivateOnEnter<TuraliQuake>()
            .ActivateOnEnter<TuraliStoneIII>()
            .ActivateOnEnter<Fireflood>()
            .ActivateOnEnter<RunThrough>()
            .ActivateOnEnter<WukLamatAI>()
            .Raw.Update = () => module.PrimaryActor.IsDestroyed || module.PrimaryActor.HPMP.CurHP == 1u;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70438, NameID = 12677)]
public sealed class TakingAStand(WorldState ws, Actor primary) : BossModule(ws, primary, arenaCenter, startingArena)
{
    private static readonly WPos arenaCenter = new(500f, -175f);
    private static readonly ArenaBoundsCustom startingArena = new([new Polygon(arenaCenter, 24.5f, 20)]);
    public static readonly ArenaBoundsCustom DefaultArena = new([new Polygon(arenaCenter, 20f, 20)]);
    private static readonly uint[] all = [(uint)OID.Boss, (uint)OID.HiredThug1, (uint)OID.HiredThug2, (uint)OID.HiredThug3, (uint)OID.HiredThug3, (uint)OID.HoobigoGuardian,
    (uint)OID.HoobigoLancer, (uint)OID.DopproIllusionist];
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, all);
    }
}
