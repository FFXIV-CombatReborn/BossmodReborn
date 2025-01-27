namespace BossMod.Shadowbringers.Dungeon.D02DohnMheg.D031AencThon;

public enum OID : uint
{
    Boss = 0xF14, // R=2.5-6.875
    LiarsLyre = 0xF63, // R=2.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // Boss->player, no cast, single-target
    Teleport = 13206, // Boss->location, no cast, single-target

    CripplingBlow = 13732, // Boss->player, 4.0s cast, single-target
    VirtuosicCapriccio = 13708, // Boss->self, 5.0s cast, range 80+R circle
    ImpChoir = 13552, // Boss->self, 4.0s cast, range 80+R circle
    ToadChoir = 13551, // Boss->self, 4.0s cast, range 17+R 150-degree cone

    FunambulistsFantasia = 13498, // Boss->self, 4.0s cast, single-target, changes arena to planks over a chasm
    FunambulistsFantasiaPull = 13519, // Helper->self, 4.0s cast, range 50 circle, pull 50, between hitboxes

    ChangelingsFantasia = 13521, // Boss->self, 3.0s cast, single-target
    ChangelingsFantasia2 = 13522, // Helper->self, 1.0s cast, single-target

    Malaise = 13549, // Boss->self, no cast, single-target
    BileBombardment = 13550, // Helper->location, 4.0s cast, range 8 circle
    CorrosiveBileFirst = 13547, // Boss->self, 4.0s cast, range 18+R 120-degree cone
    CorrosiveBileRest = 13548, // Helper->self, no cast, range 18+R 120-degree cone
    FlailingTentaclesVisual = 13952, // Boss->self, 5.0s cast, single-target
    FlailingTentacles = 13953, // Helper->self, no cast, range 32+R width 7 rect

    Finale = 15723, // LiarsLyre->self, 60.0s cast, single-target
    FinaleEnrage = 13520 // Boss->self, 60.0s cast, range 80+R circle
}

class VirtuosicCapriccio(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.VirtuosicCapriccio), "Raidwide + Bleed");
class CripplingBlow(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.CripplingBlow));
class ImpChoir(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.ImpChoir));
class ToadChoir(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ToadChoir), new AOEShapeCone(19.5f, 75.Degrees()));
class BileBombardment(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BileBombardment), 8);

class FunambulistsFantasia(BossModule module) : BossComponent(module)
{
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FunambulistsFantasia)
            Arena.Bounds = D033AencThon.chasmArena;
        else if ((AID)spell.Action.ID == AID.Finale)
            Arena.Bounds = D033AencThon.arena;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var lyre = Module.Enemies(OID.LiarsLyre).FirstOrDefault();
        if (Arena.Bounds == D033AencThon.chasmArena && lyre != null)
        {
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Sprint), actor, ActionQueue.Priority.High);
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(lyre.Position, 3));
        }
    }
}

class Finale(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.Finale), "Enrage, destroy the Liar's Lyre!", true);

class CorrosiveBile(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCone cone = new(24.875f, 45.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CorrosiveBileFirst)
            _aoe = new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CorrosiveBileFirst:
            case AID.CorrosiveBileRest:
                if (++NumCasts == 6)
                {
                    _aoe = null;
                    NumCasts = 0;
                }
                break;
        }
    }
}

class FlailingTentacles(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCross cross = new(38.875f, 3.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FlailingTentaclesVisual)
            _aoe = new(cross, spell.LocXZ, Module.PrimaryActor.Rotation + 45.Degrees(), Module.CastFinishAt(spell, 1));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FlailingTentaclesVisual:
            case AID.FlailingTentacles:
                if (++NumCasts == 5)
                {
                    _aoe = null;
                    NumCasts = 0;
                }
                break;
        }
    }
}

class D033AencThonStates : StateMachineBuilder
{
    public D033AencThonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VirtuosicCapriccio>()
            .ActivateOnEnter<CripplingBlow>()
            .ActivateOnEnter<ImpChoir>()
            .ActivateOnEnter<ToadChoir>()
            .ActivateOnEnter<BileBombardment>()
            .ActivateOnEnter<CorrosiveBile>()
            .ActivateOnEnter<FlailingTentacles>()
            .ActivateOnEnter<FunambulistsFantasia>()
            .ActivateOnEnter<Finale>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 649, NameID = 8146)]
public class D033AencThon(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly Polygon[] union = [new(new(-128.5f, -244), 19.7f, 40)];
    private static readonly Rectangle[] difference = [new(new(-128.5f, -224), 20, 1.5f)];
    public static readonly ArenaBoundsComplex arena = new(union, difference);
    private static readonly PolygonCustom[] union2 = [new([new(-142.32f, -233.89f), new(-140.52f, -245.64f), new(-129.91f, -241.9f), new(-113.72f, -243.84f),
    new(-113.81f, -244.74f), new(-125.19f, -249.54f), new(-123.72f, -254.08f), new(-124.58f, -254.05f), new(-126.13f, -249.73f), new(-126.39f, -249.05f),
    new(-115.51f, -244.47f), new(-129.9f, -242.73f), new(-140.47f, -246.47f), new(-141.19f, -246.74f), new(-143.12f, -233.92f)])];
    public static readonly ArenaBoundsComplex chasmArena = new(union, [.. difference, new Rectangle(new(-128.5f, -244), 20, 10)], union2, 0.25f);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.LiarsLyre => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.LiarsLyre), Colors.Object);
    }
}
