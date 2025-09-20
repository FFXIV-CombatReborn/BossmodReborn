namespace BossMod.Shadowbringers.Quest.Role.ATearfulReunion;

public enum OID : uint
{
    Boss = 0x29C5,
    Hollow = 0x29C6, // R0.750-2.250, x0 (spawn during fight)
}

public enum AID : uint
{
    SanctifiedFireIII = 17036, // 29E7->location, 4.0s cast, range 6 circle
    SanctifiedFlare = 17039, // Boss->players, 5.0s cast, range 6 circle
    // spread from npc
    SanctifiedFireIV1 = 17038, // _Gen_Phronesis->players/29C3, 4.0s cast, range 10 circle
    // stack with npc
    SanctifiedBlizzardII = 17044, // Boss->self, 3.0s cast, range 5 circle
    SanctifiedBlizzardIII = 17045, // Boss->self, 4.0s cast, range 40+R 45-degree cone
    SanctifiedBlizzardIV = 17047, // _Gen_Phronesis->self, 5.0s cast, range 5-20 donut
}

class SanctifiedBlizzardIV(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SanctifiedBlizzardIV, new AOEShapeDonut(5, 20));
class SanctifiedBlizzardII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SanctifiedBlizzardII, 5);
class SanctifiedFireIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SanctifiedFireIII, 6);
class SanctifiedBlizzardIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SanctifiedBlizzardIII, new AOEShapeCone(40.5f, 22.5f.Degrees()));
class Hollow(BossModule module) : Components.Voidzone(module, 4, m => m.Enemies((uint)OID.Hollow));
class HollowTether(BossModule module) : Components.Chains(module, 1, chainLength: 5);
class SanctifiedFireIV(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.SanctifiedFireIV1, 10);
class SanctifiedFlare(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.SanctifiedFlare, 6, 1)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (ActiveStacks.Count != 0 && WorldState.Actors.First(x => x.OID == 0x29C3) is Actor cerigg)
        {
            hints.AddForbiddenZone(new SDInvertedCircle(cerigg.Position, 6), ActiveStacks.First().Activation);
        }
    }
}

class LightningGlobe(BossModule module) : Components.GenericLineOfSightAOE(module, default, 100, false)
{
    private readonly List<Actor> Balls = [];
    private IEnumerable<(WPos Center, float Radius)> Hollows => Module.Enemies((uint)OID.Hollow).Select(h => (h.Position, h.HitboxRadius));

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == 6)
            Balls.Add(source);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var b in Balls)
            Arena.AddLine(pc.Position, b.Position, Colors.Danger);
    }

    public override void Update()
    {
        var player = Raid.Player();
        if (player == null)
            return;

        Balls.RemoveAll(b => b.IsDead);

        var closestBall = Balls.OrderBy(player.DistanceToHitbox).FirstOrDefault();
        Modify(closestBall?.Position, Hollows);
        Safezones.Clear();
        AddSafezone(NextExplosion, default);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Origin != null
            && actor.Position.InCircle(Origin.Value, MaxRange)
            && !Visibility.Any(v => !actor.Position.InCircle(Origin.Value, v.Distance) && actor.Position.InCone(Origin.Value, v.Dir, v.HalfWidth)))
        {
            hints.Add("Pull lightning orb into black hole!");
        }
    }
}

class PhronesisStates : StateMachineBuilder
{
    public PhronesisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SanctifiedFireIII>()
            .ActivateOnEnter<SanctifiedBlizzardIII>()
            .ActivateOnEnter<Hollow>()
            .ActivateOnEnter<HollowTether>()
            .ActivateOnEnter<SanctifiedFireIV>()
            .ActivateOnEnter<SanctifiedFlare>()
            .ActivateOnEnter<LightningGlobe>()
            .ActivateOnEnter<SanctifiedBlizzardII>()
            .ActivateOnEnter<SanctifiedBlizzardIV>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69164, NameID = 8931)]
public class Phronesis(WorldState ws, Actor primary) : BossModule(ws, primary, new(-256, -284), new ArenaBoundsCircle(20));
