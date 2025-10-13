namespace BossMod.Dawntrail.Foray.FATE.ServingDarkness;

public enum OID : uint
{
    Boss = 0x4772, // R3.5
    Helper = 0x4773
}

public enum AID : uint
{
    AutoAttack = 42901, // Boss->player, no cast, single-target
    Teleport1 = 42196, // Boss->location, no cast, single-target
    Teleport2 = 42186, // Boss->location, no cast, single-target

    Menace = 42175, // Boss->self, 6.0s cast, range 20 circle
    DismalRoarVisual = 42184, // Boss->self, 5.0s cast, single-target
    DismalRoar = 42185, // Helper->self, 5.0s cast, range 60 circle
    SoulSweepTarget = 42192, // Boss->player, no cast, single-target
    SoulSweep = 42177, // Boss->self, 6.0s cast, range 60 130-degree cone

    SweepingCharge = 42178, // Boss->location, 8.0s cast, width 8 rect charge
    SweepingChargeCone = 42181, // Boss->self, 2.0s cast, range 60 130-degree cone
    MenacingCharge = 42179, // Boss->location, 8.0s cast, width 8 rect charge
    MenaceCharge = 42180, // Boss->self, 2.0s cast, range 20 circle

    HallOfSorrowVisual = 42182, // Boss->self, 3.0s cast, single-target
    HallOfSorrow = 42183, // Helper->location, 4.0s cast, range 10 circle
}

sealed class DismalRoar(BossModule module) : Components.RaidwideCast(module, (uint)AID.DismalRoar);
sealed class Menace(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Menace, 20f);
sealed class SoulSweep(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.SoulSweep, (uint)AID.SweepingChargeCone], new AOEShapeCone(60f, 65f.Degrees()));
sealed class SweepingMenacingCharge(BossModule module) : Components.SimpleChargeAOEGroups(module, [(uint)AID.SweepingCharge, (uint)AID.MenacingCharge], 4f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Module.PrimaryActor.CastInfo is var spell && spell != null && spell.Action.ID == (uint)AID.SweepingCharge)
        {
            hints.GoalZones.Add(AIHints.GoalSingleTarget(spell.LocXZ, 5f, 5f)); // follow the charge
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Module.PrimaryActor.CastInfo is var spell && spell != null && spell.Action.ID == (uint)AID.SweepingCharge)
        {
            hints.Add("Follow the charge!");
        }
    }
}

sealed class HallOfSorrow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HallOfSorrow, 10f);

sealed class MenaceCharge(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeCircle circle = new(20f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MenacingCharge)
        {
            _aoe = [new(circle, spell.LocXZ, default, Module.CastFinishAt(spell, 5.2d))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MenaceCharge)
        {
            _aoe = [];
        }
    }
}

sealed class ServingDarknessStates : StateMachineBuilder
{
    public ServingDarknessStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DismalRoar>()
            .ActivateOnEnter<Menace>()
            .ActivateOnEnter<SoulSweep>()
            .ActivateOnEnter<SweepingMenacingCharge>()
            .ActivateOnEnter<HallOfSorrow>()
            .ActivateOnEnter<MenaceCharge>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.ForayFATE, GroupID = 1018, NameID = 1972)]
public sealed class ServingDarkness(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
