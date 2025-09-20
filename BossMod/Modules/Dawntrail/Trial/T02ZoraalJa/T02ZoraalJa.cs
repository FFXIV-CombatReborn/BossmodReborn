namespace BossMod.Dawntrail.Trial.T02ZoraalJa;

sealed class SoulOverflowCalamitysEdge(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.SoulOverflow1, (uint)AID.SoulOverflow1, (uint)AID.CalamitysEdge]);
sealed class PatricidalPique(BossModule module) : Components.SingleTargetCast(module, (uint)AID.PatricidalPique);
sealed class Burst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Burst, 8f);

sealed class VorpalTrail(BossModule module) : Components.SimpleChargeAOEGroups(module, [(uint)AID.VorpalTrail1, (uint)AID.VorpalTrail2], 2f);

sealed class T02ZoraalJaStates : StateMachineBuilder
{
    public T02ZoraalJaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SoulOverflowCalamitysEdge>()
            .ActivateOnEnter<DoubleEdgedSwords>()
            .ActivateOnEnter<PatricidalPique>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<VorpalTrail>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed || !module.PrimaryActor.IsTargetable;
    }
}

public abstract class ZoraalJa(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultBounds)
{
    public static readonly Angle ArenaRotation = 45f.Degrees();
    public static readonly WPos ArenaCenter = new(100f, 100f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(20f, ArenaRotation);
    public static readonly ArenaBoundsSquare SmallBounds = new(10f, ArenaRotation);
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 995, NameID = 12881, SortOrder = 1)]
public sealed class T02ZoraalJa(WorldState ws, Actor primary) : ZoraalJa(ws, primary);
