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

class VirtuosicCapriccio(BossModule module) : Components.RaidwideCast(module, (uint)AID.VirtuosicCapriccio, "Raidwide + Bleed");
class CripplingBlow(BossModule module) : Components.SingleTargetCast(module, (uint)AID.CripplingBlow);
class ImpChoir(BossModule module) : Components.CastGaze(module, (uint)AID.ImpChoir);
class ToadChoir(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ToadChoir, new AOEShapeCone(19.5f, 75f.Degrees()));
class BileBombardment(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BileBombardment, 8f);

class FunambulistsFantasia(BossModule module) : BossComponent(module)
{
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FunambulistsFantasia)
        {
            Arena.Bounds = D033AencThon.ChasmArena;
        }
        else if (spell.Action.ID == (uint)AID.Finale)
        {
            Arena.Bounds = D033AencThon.ArenaBounds;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Arena.Bounds == D033AencThon.ChasmArena && Module.Enemies((uint)OID.LiarsLyre) is var lyre && lyre.Count != 0)
        {
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Sprint), actor, ActionQueue.Priority.High);
            hints.GoalZones.Add(AIHints.GoalSingleTarget(lyre[0], 1f, 5f));
        }
    }
}

class Finale(BossModule module) : Components.CastHint(module, (uint)AID.Finale, "Enrage, destroy the Liar's Lyre!", true);

class CorrosiveBile(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeCone cone = new(24.875f, 45f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CorrosiveBileFirst)
        {
            _aoe = [new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.CorrosiveBileFirst:
            case (uint)AID.CorrosiveBileRest:
                if (++NumCasts == 6)
                {
                    _aoe = [];
                    NumCasts = 0;
                }
                break;
        }
    }
}

class FlailingTentacles(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeCross cross = new(38.875f, 3.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FlailingTentaclesVisual)
        {
            _aoe = [new(cross, spell.LocXZ, Module.PrimaryActor.Rotation + 45f.Degrees(), Module.CastFinishAt(spell, 1d))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FlailingTentaclesVisual:
            case (uint)AID.FlailingTentacles:
                if (++NumCasts == 5)
                {
                    _aoe = [];
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
public class D033AencThon(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaBounds.Center, ArenaBounds)
{
    private static readonly Polygon[] union = [new(new(-128.5f, -244f), 19.7f, 40)];
    private static readonly Rectangle[] difference = [new(new(-128.5f, -224f), 20f, 1.5f)];
    public static readonly ArenaBoundsCustom ArenaBounds = new(union, difference);
    private static readonly PolygonCustom[] union2 = [new([new(-142.32f, -234f), new(-140.533f, -245.712f), new(-129.976f, -241.934f), new(-113.76f, -243.889f),
    new(-113.87f, -244.775f), new(-125.28f, -249.556f), new(-123.83f, -254f), new(-124.66f, -254f), new(-126.205f, -249.744f), new(-126.421f, -249.072f),
    new(-115.56f, -244.512f), new(-129.954f, -242.795f), new(-141.178f, -246.795f), new(-143.12f, -234f)])];
    public static readonly ArenaBoundsCustom ChasmArena = new(union, [.. difference, new Rectangle(new(-128.5f, -244f), 20f, 10f)], union2, 0.25f);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        if (count == 0)
            return;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            if (e.Actor.OID == (uint)OID.LiarsLyre && (actor.Position - e.Actor.Position).LengthSq() > 15f)
                e.Priority = AIHints.Enemy.PriorityInvincible;
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.LiarsLyre), Colors.Object);
    }
}
