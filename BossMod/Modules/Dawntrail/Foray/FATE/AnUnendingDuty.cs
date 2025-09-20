namespace BossMod.Dawntrail.Foray.FATE.AnUnendingDuty;

public enum OID : uint
{
    Boss = 0x4735, // R5.8
    Helper = 0x4736
}

public enum AID : uint
{
    AutoAttack = 41994, // Boss->player, no cast, single-target
    Teleport = 41971, // Boss->location, no cast, single-target

    ThunderousMemoryVisualCircle = 41973, // Boss->self, 3.3+0,7s cast, single-target
    ThunderousMemoryVisualCone = 41977, // Boss->self, 3.3+0,7s cast, single-target
    ThunderousMemoryVisualDonut = 41975, // Boss->self, 3.3+0,7s cast, single-target
    ThunderousMemoryCircle = 41974, // Helper->location, 4.0s cast, range 10 circle
    ThunderousMemoryDonut = 41976, // Helper->location, 4.0s cast, range 10-30 donut
    ThunderousMemoryCone = 41978, // Helper->location, 4.0s cast, range 70 45-degree cone
    ResoundingMemory = 41979, // Boss->self, 4.3+0,7s cast, single-target
    ResoundingMemoryCone = 41982, // Helper->location, 5.0s cast, range 70 45-degree cone
    ResoundingMemoryCircle = 41980, // Helper->location, 5.0s cast, range 10 circle
    ResoundingMemoryDonut = 41981, // Helper->location, 5.0s cast, range 10-30 donut

    ThriceComeThunderVisual = 41983, // Boss->self, 4.3+0,7s cast, single-target
    ThriceComeThunder1 = 41984, // Helper->location, 5.0s cast, range 10 circle
    ThriceComeThunder2 = 41985, // Helper->location, 7.0s cast, range 10-20 donut
    ThriceComeThunder3 = 41986, // Helper->location, 9.0s cast, range 20-30 donut

    ThunderIIVisual = 41987, // Boss->self, 2.3+0,7s cast, single-target
    ThunderII = 41988, // Helper->location, 3.0s cast, range 6 circle
    ThunderIVVisual = 41991, // Boss->self, 4.3+0,7s cast, single-target
    ThunderIV = 41992, // Helper->location, 5.0s cast, range 40 circle
    Trounce = 41972 // Boss->self, 5.0s cast, range 40 60-degree cone
}

sealed class Trounce(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Trounce, new AOEShapeCone(40f, 30f.Degrees()));
sealed class ThunderIV(BossModule module) : Components.RaidwideCast(module, (uint)AID.ThunderIV);
sealed class ThunderII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThunderII, 6f);
sealed class ThunderousResoundingMemoryCone(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ThunderousMemoryCone, (uint)AID.ResoundingMemoryCone], new AOEShapeCone(70f, 22.5f.Degrees()));
sealed class ThunderousResoundingMemoryCircle(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ThunderousMemoryCircle, (uint)AID.ResoundingMemoryCircle], 10f);
sealed class ThunderousResoundingMemoryDonut(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ThunderousMemoryDonut, (uint)AID.ResoundingMemoryDonut], new AOEShapeDonut(10f, 30f));

sealed class ThriceComeThunder(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 20f), new AOEShapeDonut(20f, 30f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ThriceComeThunder1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.ThriceComeThunder1 => 0,
                (uint)AID.ThriceComeThunder2 => 1,
                (uint)AID.ThriceComeThunder3 => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

sealed class AnUnendingDutyStates : StateMachineBuilder
{
    public AnUnendingDutyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ThriceComeThunder>()
            .ActivateOnEnter<Trounce>()
            .ActivateOnEnter<ThunderII>()
            .ActivateOnEnter<ThunderIV>()
            .ActivateOnEnter<ThunderousResoundingMemoryCircle>()
            .ActivateOnEnter<ThunderousResoundingMemoryCone>()
            .ActivateOnEnter<ThunderousResoundingMemoryDonut>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.ForayFATE, GroupID = 1018, NameID = 1966)]
public sealed class AnUnendingDuty(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
