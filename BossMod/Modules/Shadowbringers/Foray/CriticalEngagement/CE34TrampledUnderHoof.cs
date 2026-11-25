namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE34TrampledUnderHoof;

public enum OID : uint
{
    Boss = 0x2ECF, // R6.44
    DemonicEye1 = 0x2ED0, // R2.0
    DemonicEye2 = 0x2ED1, // R1.0
    FourthThLegionAquilifer = 0x2D8A, // R0.5
    EalesGolem = 0x2D89, // R2.2
    Deathwall = 0x1EB030, // R0.5
    DeathwallHelper = 0x2EE8, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttackBoss = 6497, // Boss->player, no cast, single-target
    AutoAttackGolem = 6499, // EalesGolem->player, no cast, single-target

    BullHorn = 20968, // Boss->player, 5.0s cast, single-target
    BestialRoar = 20969, // Boss->self, no cast, range 50 circle, tiny raidwide
    DeadlyToxin = 20958, // DeathwallHelper->self, no cast, range 20-25 donut
    ShiftingGaze1 = 20959, // Boss->DemonicEye1, 5.0s cast, single-target
    ShiftingGaze2 = 20960, // Boss->DemonicEye1, no cast, single-target
    FalseDemonEye = 20962, // DemonicEye1->self, 5.0s cast, range 50 circle
    DemonEye = 20961, // Boss->self, 7.0s cast, range 50 circle
    GlimmerInTheDark = 20963, // DemonicEye2->self, no cast, range 6 circle
    BalefulGlintVisual = 20965, // Boss->self, 5.0s cast, single-target
    BalefulGlint = 20966, // Helper->location, 5.0s cast, range 8 circle
    ArrestingGaze = 20964, // Boss->self, 3.0s cast, range 40 90-degree cone
    MagickedMark = 21252, // FourthThLegionAquilifer->player, 2.5s cast, single-target
    ShudderingImpact = 21253, // EalesGolem->self, 3.0s cast, range 40 circle
    AccursedLight = 20967 // Boss->players, 5.0s cast, range 8 circle, stack
}

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(20f, 25f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BullHorn)
        {
            _aoe = [new(donut, Arena.Center, default, Module.CastFinishAt(spell, 6d))];
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Deathwall)
        {
            Arena.Bounds = new ArenaBoundsCircle(20f); // default arena got no extra collision, just a donut aoe
            Arena.Center = Arena.Center.Quantized();
            _aoe = [];
        }
    }
}

sealed class BullHorn(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.BullHorn);
sealed class BalefulGlint(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BalefulGlint, 8f);
sealed class ArrestingGaze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArrestingGaze, new AOEShapeCone(40f, 45f.Degrees()));

sealed class GlimmerInTheDark(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(2)];
    private static readonly AOEShapeCircle circle = new(6f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.DemonicEye2)
        {
            _aoes.Add(new(circle, actor.Position.Quantized(), default, WorldState.FutureTime(4.9d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.GlimmerInTheDark)
        {
            var count = _aoes.Count;
            var pos = caster.Position;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.Origin.AlmostEqual(pos, 0.1f))
                {
                    if (++aoe.ActorID == 10ul)
                    {
                        _aoes.RemoveAt(i);
                    }
                    return;
                }
            }
        }
    }
}

sealed class DemonEye(BossModule module) : Components.GenericGaze(module)
{
    private readonly List<Eye> _eyes = [with(3)];
    private bool inverted;

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor) => CollectionsMarshal.AsSpan(_eyes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.FalseDemonEye)
        {
            var loc = spell.LocXZ;
            _eyes.Add(new(loc, Module.CastFinishAt(spell), inverted: inverted, eyeCenter: IndicatorWorldPos(loc)));
        }
        else if (id == (uint)AID.DemonEye)
        {
            inverted = true; // depending on timing eyes can spawn after cast start
            var count = _eyes.Count;
            var eyes = CollectionsMarshal.AsSpan(_eyes);
            for (var i = 0; i < count; ++i)
            {
                ref readonly var eye = ref eyes[i];
                if (!eye.Inverted)
                {
                    eyes[i] = new(eye.Position, eye.Activation, inverted: true, eyeCenter: eye.EyeCenter);
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FalseDemonEye)
        {
            _eyes.Clear();
            inverted = false;
        }
    }
}

sealed class AccursedLight(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.AccursedLight, 8f, 8);

sealed class CE34TrampledUnderHoofStates : StateMachineBuilder
{
    public CE34TrampledUnderHoofStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<BullHorn>()
            .ActivateOnEnter<GlimmerInTheDark>()
            .ActivateOnEnter<AccursedLight>()
            .ActivateOnEnter<BalefulGlint>()
            .ActivateOnEnter<ArrestingGaze>()
            .ActivateOnEnter<DemonEye>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 735, NameID = 11)]
public sealed class CE34TrampledUnderHoof(WorldState ws, Actor primary) : BossModule(ws, primary, startingArena.Center, startingArena)
{
    private static readonly ArenaBoundsCustom startingArena = new([new Polygon(new(-450f, 262f), 24.5f, 32)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.EalesGolem));
        Arena.Actors(Enemies((uint)OID.FourthThLegionAquilifer));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Boss => 0,
                _ => 1
            };
        }
    }

    protected override bool CheckPull() => base.CheckPull() && Raid.Player() is Actor player && player.Position.InCircle(Arena.Center, 25f);
}
