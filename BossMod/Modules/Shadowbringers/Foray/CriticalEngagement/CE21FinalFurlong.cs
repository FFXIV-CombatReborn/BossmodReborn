namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE21FinalFurlong;

public enum OID : uint
{
    Boss = 0x2DBB, // R3.375
    Monoceros = 0x2DB9, // R1.8
    LlofiiTheForthright = 0x2DBA, // R0.5
    GraspingRancor = 0x2DBC, // R1.6
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttackBoss = 6498, // Boss->player, no cast, single-target
    Teleport = 20135, // Monoceros->location, no cast, single-target, teleport

    GraspingRancor = 20118, // Boss->self, 4.0s cast, single-target, visual (spawn hands)
    SpitefulGuillotine = 20119, // GraspingRancor->player, no cast, single-target, debuffs or kills if reaches tethered player
    SpiteWave = 20124, // Boss->self, 3.0s cast, single-target, visual (stack + puddles)
    HatefulMiasma = 20125, // Helper->players, 5.0s cast, range 6 circle stack
    PoisonedWords = 20126, // Helper->location, 5.0s cast, range 6 circle puddles
    TalonedGaze = 20127, // Boss->self, 4.0s cast, single-target, visual (front/back then sides)
    TalonedWings = 20128, // Boss->self, 4.0s cast, single-target, visual, (sides then front/back)
    CoffinNails = 20129, // Helper->self, 4.7s cast, range 60 90-degree cone aoe
    Stab = 20130, // Boss->player, 4.0s cast, single-target, tankbuster
    GripOfPoison = 20131, // Boss->self, 4.0s cast, range 60 circle, raidwide
    StepsOfDestruction = 21018, // Boss->self, 4.0s cast, single-target, visual (puddles)
    StepsOfDestructionAOE = 21019, // Helper->location, 4.0s cast, range 6 circle puddle
    AutoAttackMonoceros = 871, // Monoceros->Boss, no cast, single-target
    PurifyingLight = 20132, // Monoceros->location, 12.0s cast, range 12 circle (destroys hands)
    FabledHope = 20134, // Monoceros->self, 8.0s cast, range 10 circle, damage up on players
    Ruin = 20142, // LlofiiTheForthright->Boss, 2.5s cast, single-target, autoattack
    Scupper = 21334 // LlofiiTheForthright->Boss, 2.0s cast, single-target, damage down on boss
}

public enum TetherID : uint
{
    Movable = 1, // GraspingRancor->player
    Frozen = 2, // GraspingRancor->player
    Unfreezable = 17 // GraspingRancor->player (appears if hand wasn't hit by aoe)
}

sealed class GraspingRancor : Components.SimpleAOEs
{
    private readonly List<Actor> _hands;

    public GraspingRancor(BossModule module) : base(module, (uint)AID.PurifyingLight, 12f)
    {
        Color = Colors.SafeFromAOE;
        Risky = false;
        _hands = module.Enemies((uint)OID.GraspingRancor);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (Casters.Count > 0)
        {
            var hand = _hands.FirstOrDefault(h => h.Tether.Target == actor.InstanceID);
            if (hand != null)
            {
                var shouldBeFrozen = Shape.Check(hand.Position, Casters[0].Origin, default);
                var isFrozen = hand.Tether.ID == (uint)TetherID.Frozen;
                hints.Add(shouldBeFrozen ? "Face the hand!" : "Look away from hand and kite into safezone!", shouldBeFrozen != isFrozen);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var hand = _hands.FirstOrDefault(h => h.Tether.Target == pc.InstanceID);
        if (hand != null)
        {
            var isFrozen = hand.Tether.ID == (uint)TetherID.Frozen;
            Arena.Actor(hand, Colors.Object, true);
            Arena.AddLine(hand.Position, pc.Position, isFrozen ? Colors.Safe : default);
        }
    }
}

sealed class HatefulMiasma(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.HatefulMiasma, 6f);
sealed class PoisonedWordsStepsOfDestruction(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PoisonedWords, (uint)AID.StepsOfDestructionAOE], 6f);

sealed class CoffinNails : Components.SimpleAOEs
{
    public CoffinNails(BossModule module) : base(module, (uint)AID.CoffinNails, new AOEShapeCone(60f, 45f.Degrees()))
    {
        MaxDangerColor = 2;
        MaxRisky = 2;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Casters.Count;
        if (count == 0)
            return;
        base.AddAIHints(slot, actor, assignment, hints);
        // stay close to the middle if there is next a 2nd aoe set
        if (count > 2)
        {
            var aoe = Casters[0];
            hints.AddForbiddenZone(new SDInvertedCircle(aoe.Origin, 4f), aoe.Activation);
        }
    }
}

sealed class Stab(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Stab);
sealed class GripOfPoison(BossModule module) : Components.RaidwideCast(module, (uint)AID.GripOfPoison);

sealed class CE21FinalFurlongStates : StateMachineBuilder
{
    public CE21FinalFurlongStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GraspingRancor>()
            .ActivateOnEnter<HatefulMiasma>()
            .ActivateOnEnter<PoisonedWordsStepsOfDestruction>()
            .ActivateOnEnter<CoffinNails>()
            .ActivateOnEnter<Stab>()
            .ActivateOnEnter<GripOfPoison>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 735, NameID = 6)] // bnpcname=9405
public sealed class CE21FinalFurlong(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(644f, 228f), 29.5f, 32)]);

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 30f);
}
