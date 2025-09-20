namespace BossMod.Shadowbringers.Quest.Role.TheHuntersLegacy;

public enum OID : uint
{
    Boss = 0x29EE,
    Helper = 0x233C
}

public enum AID : uint
{
    BalamBlaster = 17137, // Boss->self, 4.5s cast, range 30+R 270-degree cone
    BalamBlasterRear = 17138, // Boss->self, 4.5s cast, range 30+R 270-degree cone
    ElectricWhisker = 17126, // Boss->self, 3.5s cast, range 8+R 90-degree cone
    RoaringThunder = 17135, // Boss->self, 4.0s cast, range 8-30 donut
    StreakLightning = 17148, // 233C->location, 2.5s cast, range 3 circle
    AlternatingCurrent1 = 17150, // Helper->self, 4.0s cast, range 60 width 5 rect
    RumblingThunderStack = 17134, // Helper->player, 6.0s cast, range 5 circle
    Thunderbolt1 = 17140, // Helper->players/29EC, 6.0s cast, range 5 circle
    StreakLightning1 = 17147, // Helper->location, 2.5s cast, range 3 circle
}

class Thunderbolt(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Thunderbolt1, 5f);
class BalamBlaster(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BalamBlaster, (uint)AID.BalamBlasterRear], new AOEShapeCone(38.05f, 135f.Degrees()));
class ElectricWhisker(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElectricWhisker, new AOEShapeCone(16.05f, 45f.Degrees()));
class RoaringThunder(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RoaringThunder, new AOEShapeDonut(8f, 30f));
class StreakLightning(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StreakLightning, 3f);
class StreakLightning1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StreakLightning1, 3f);
class AlternatingCurrent(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AlternatingCurrent1, new AOEShapeRect(60f, 2.5f));
class RumblingThunder(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.RumblingThunderStack, 5f, 1);

class RendaRae(WorldState ws) : QuestBattle.UnmanagedRotation(ws, 20f)
{
    protected override void Exec(Actor? primaryTarget)
    {
        var dot = StatusDetails(primaryTarget, Roleplay.SID.AcidicBite, Player.InstanceID);
        if (dot.Left < 2.5f)
            UseAction(Roleplay.AID.AcidicBite, primaryTarget, 10f);

        UseAction(Roleplay.AID.RadiantArrow, primaryTarget, -5f);
        UseAction(Roleplay.AID.HeavyShot, primaryTarget);

        if (primaryTarget?.CastInfo?.Interruptible ?? false)
            UseAction(Roleplay.AID.DullingArrow, primaryTarget, 5f);

        if (Player.HPMP.MaxHP * 0.8f > Player.HPMP.CurHP)
            UseAction(Roleplay.AID.HuntersPrudence, Player, -15f);
    }
}

class RendaRaeAI(BossModule module) : QuestBattle.RotationModule<RendaRae>(module);

class RonkanAura(BossModule module) : BossComponent(module)
{
    private Actor? AuraCenter => Module.Enemies(0x1EADA5).FirstOrDefault();

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (AuraCenter is Actor a)
            Arena.ZoneCircle(a.Position, 10f, Colors.SafeFromAOE);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (AuraCenter is Actor a)
            hints.AddForbiddenZone(new SDInvertedCircle(a.Position, 10f), activation: WorldState.FutureTime(5d));
    }
}

class BalamQuitzStates : StateMachineBuilder
{
    public BalamQuitzStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RendaRaeAI>()
            .ActivateOnEnter<BalamBlaster>()
            .ActivateOnEnter<ElectricWhisker>()
            .ActivateOnEnter<RoaringThunder>()
            .ActivateOnEnter<StreakLightning>()
            .ActivateOnEnter<StreakLightning1>()
            .ActivateOnEnter<AlternatingCurrent>()
            .ActivateOnEnter<RumblingThunder>()
            .ActivateOnEnter<Thunderbolt>()
            .ActivateOnEnter<RonkanAura>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68812, NameID = 8397)]
public class BalamQuitz(WorldState ws, Actor primary) : BossModule(ws, primary, new(-247.11f, 688.33f), new ArenaBoundsCircle(19.5f));
