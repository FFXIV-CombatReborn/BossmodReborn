namespace BossMod.Dawntrail.Dungeon.D08StrayboroughDeadwalk.D083Träumerei;

public enum OID : uint
{
    Boss = 0x421F, // R26.0
    StrayGeist = 0x4221, // R2.0
    StrayPhantagenitrix = 0x4220, // R1.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 16764, // Boss->player, no cast, single-target

    BitterRegretVisual = 37140, // Boss->self, 6.0s cast, single-target
    BitterRegret1 = 37139, // Boss->self, 6.0+0.7s cast, range 40 width 16 rect
    BitterRegret2 = 37147, // Helper->self, 6.7s cast, range 50 width 12 rect
    BitterRegret3 = 37340, // StrayPhantagenitrix->self, 6.0s cast, range 40 width 4 rect

    Poltergeist = 37132, // Boss->self, 3.0s cast, single-target

    MemorialMarch1 = 37136, // Boss->self, 3.0s cast, single-target
    MemorialMarch2 = 37065, // Boss->self, 6.0s cast, single-target

    Impact = 37133, // Helper->self, 6.0s cast, range 40 width 4 rect

    IllIntent = 39607, // StrayGeist->player, 10.0s cast, single-target
    MaliciousMistTether = 37138, // StrayGeist->player, 10.0s cast, single-target

    GhostdusterSpreadVisual = 37145, // Boss->self, 8.0s cast, single-target
    Ghostduster = 37146, // Helper->player, 8.0s cast, range 8 circle, spread

    MaliciousMistRaidwide = 37168, // Boss->self, 5.0s cast, range 60 circle

    Fleshbuster = 37148, // Boss->self, 8.0s cast, range 60 circle

    GhostcrusherVisual = 37142, // Boss->self, 5.0s cast, single-target, line stack
    GhostcrusherMarker = 37144, // Helper->player, no cast, single-target
    Ghostcrusher = 37143 // Helper->self, no cast, range 80 width 8 rect
}

public enum SID : uint
{
    GhostlyGuise = 3949 // none->player, extra=0x0
}

sealed class ImpactArenaChange(BossModule module) : BossComponent(module)
{
    private bool active;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x0B)
        {
            if (state == 0x00800040u)
            {
                active = true;
            }
            else if (state == 0x00080004u)
            {
                active = false;
                Arena.Bounds = D083Träumerei.DefaultBounds;
            }
        }
    }
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (active)
            Arena.Bounds = GhostlyGuise.IsGhostly(pc) ? D083Träumerei.DefaultBounds : D083Träumerei.CrossBounds;
    }
}

sealed class GhostlyGuise(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Ghostduster _avoid = module.FindComponent<Ghostduster>()!;
    private readonly IllIntentMaliciousMist _seek = module.FindComponent<IllIntentMaliciousMist>()!;

    private static readonly WPos[] positions = [new(137.5f, -443.5f), new(158.5f, -443.5f), new(137.5f, -422.5f), new(158.5f, -422.5f)];
    private static readonly Circle[] circles = GenerateCircles();
    private static readonly AOEShapeCustom circlesInverted = new(circles, invertForbiddenZone: true);
    private static readonly AOEShapeCustom circlesAvoid = new(circles);
    private bool activated;
    private (bool isActive, DateTime activation) fleshbuster;

    private const string GhostHint = "Turn into a ghost!";
    private const string FleshHint = "Turn into flesh!";

    private static Circle[] GenerateCircles()
    {
        var circles = new Circle[4];
        for (var i = 0; i < 4; ++i)
        {
            circles[i] = new Circle(positions[i], 3f);
        }
        return circles;
    }

    public static bool IsGhostly(Actor actor) => actor.FindStatus((uint)SID.GhostlyGuise) != null;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!activated)
            return [];

        var shape = circlesAvoid;
        DateTime activation = default;

        if (_avoid.ActiveSpreads.Count != 0)
        {
            shape = IsGhostly(actor) ? circlesInverted : circlesAvoid;
            activation = _avoid.ActiveSpreads[0].Activation;
        }
        else if (fleshbuster.isActive)
        {
            shape = IsGhostly(actor) ? circlesAvoid : circlesInverted;
            activation = fleshbuster.activation;
        }
        else if (_seek.ActiveBaits.Count != 0)
            shape = IsGhostly(actor) ? circlesAvoid : circlesInverted;

        return new AOEInstance[1] { new(shape, Arena.Center, default, activation, shape == circlesInverted ? Colors.SafeFromAOE : default) };
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001u && index == 0x0Cu) // 0x0C, 0x0D, 0x0E, 0xOF happen at the same time, one for each platform
            activated = true;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Fleshbuster)
            fleshbuster = (true, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Fleshbuster)
            fleshbuster = default;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var isGhostly = IsGhostly(actor);
        if (fleshbuster.isActive || _seek.ActiveBaits.Count != 0)
            hints.Add(GhostHint, !isGhostly);
        else if (_avoid.ActiveSpreads.Count != 0)
            hints.Add(FleshHint, isGhostly);
    }
}

sealed class MaliciousMistRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.MaliciousMistRaidwide);
sealed class IllIntentMaliciousMist(BossModule module) : Components.StretchTetherDuo(module, 20f, 10f)
{
    // ill intent seems to break after 17, malicious mist after 20, not worth the effort to differentiate
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (GhostlyGuise.IsGhostly(actor))
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

sealed class BitterRegret1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BitterRegret1, new AOEShapeRect(50f, 8f));
sealed class BitterRegret2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BitterRegret2, new AOEShapeRect(50f, 6f));
sealed class BitterRegret3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BitterRegret3, new AOEShapeRect(40f, 2f), 5);
sealed class Impact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Impact, new AOEShapeRect(40f, 2f));
sealed class Ghostcrusher(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.GhostcrusherMarker, (uint)AID.Ghostcrusher, 5d, 80f, maxStackSize: 4);
sealed class Ghostduster(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Ghostduster, 8f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!GhostlyGuise.IsGhostly(actor))
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

sealed class D083TräumereiStates : StateMachineBuilder
{
    public D083TräumereiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ImpactArenaChange>()
            .ActivateOnEnter<Ghostcrusher>()
            .ActivateOnEnter<MaliciousMistRaidwide>()
            .ActivateOnEnter<IllIntentMaliciousMist>()
            .ActivateOnEnter<Ghostduster>()
            .ActivateOnEnter<GhostlyGuise>()
            .ActivateOnEnter<Impact>()
            .ActivateOnEnter<BitterRegret1>()
            .ActivateOnEnter<BitterRegret2>()
            .ActivateOnEnter<BitterRegret3>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 981, NameID = 12763)]
public sealed class D083Träumerei(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsSquare(19.5f))
{
    public static readonly WPos ArenaCenter = new(148f, -433f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(19.5f);
    public static readonly ArenaBoundsCustom CrossBounds = new([new Square(ArenaCenter, 19.5f)], [new Cross(ArenaCenter, 20f, 1.5f)]); // for some reason the obstacle cross is smaller than the AOE
}
