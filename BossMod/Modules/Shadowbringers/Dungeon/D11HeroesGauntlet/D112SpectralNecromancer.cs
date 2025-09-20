namespace BossMod.Shadowbringers.Dungeon.D11HeroesGauntlet.D112SpectralNecromancer;

public enum OID : uint
{

    Boss = 0x2DF1, // R2.3
    Necrobomb1 = 0x2DF2, // R0.75
    Necrobomb2 = 0x2DF3, // R0.75
    Necrobomb3 = 0x2DF4, // R0.75
    Necrobomb4 = 0x2DF5, // R0.75
    Necrobomb5 = 0x2DF6, // R0.75
    Necrobomb6 = 0x2DF7, // R0.75
    Necrobomb7 = 0x2DF8, // R0.75
    Necrobomb8 = 0x2DF9, // R0.75
    BleedVoidzone = 0x1EB02C,
    NecroPortal = 0x1EB07A,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6499, // Necrobomb3/Necrobomb4/Necrobomb1/Necrobomb2->player, no cast, single-target
    FellForces = 20305, // Boss->player, no cast, single-target

    AbsoluteDarkII = 20321, // Boss->self, 5.0s cast, range 40 120-degree cone

    TwistedTouch = 20318, // Boss->player, 4.0s cast, single-target
    Necromancy1 = 20311, // Boss->self, 3.0s cast, single-target
    Necromancy2 = 20312, // Boss->self, 3.0s cast, single-target
    Necroburst1 = 20313, // Boss->self, 4.3s cast, single-target
    Necroburst2 = 20314, // Boss->self, 4.3s cast, single-target

    Burst1 = 20322, // Necrobomb1->self, 4.0s cast, range 8 circle
    Burst2 = 21429, // Necrobomb2->self, 4.0s cast, range 8 circle
    Burst3 = 21430, // Necrobomb3->self, 4.0s cast, range 8 circle
    Burst4 = 21431, // Necrobomb4->self, 4.0s cast, range 8 circle
    Burst5 = 20324, // Necrobomb5->self, 4.0s cast, range 8 circle
    Burst6 = 21432, // Necrobomb6->self, 4.0s cast, range 8 circle
    Burst7 = 21433, // Necrobomb7->self, 4.0s cast, range 8 circle
    Burst8 = 21434, // Necrobomb8->self, 4.0s cast, range 8 circle

    PainMireVisual = 20387, // Boss->self, no cast, single-target
    PainMire = 20388, // Helper->location, 5.5s cast, range 9 circle, spawns voidzone smaller than AOE
    DeathThroes = 20323, // Necrobomb5/Necrobomb6/Necrobomb7/Necrobomb8->player, no cast, single-target

    ChaosStorm = 20320, // Boss->self, 4.0s cast, range 40 circle, raidwide
    DarkDelugeVisual = 20316, // Boss->self, 4.0s cast, single-target
    DarkDeluge = 20317 // Helper->location, 5.0s cast, range 5 circle
}

public enum IconID : uint
{
    Baitaway = 23, // player
    Tankbuster = 198 // player
}

public enum SID : uint
{
    Doom = 910 // Boss->player, extra=0x0
}

public enum TetherID : uint
{
    WalkingNecrobombs = 17, // Necrobomb3/Necrobomb1/Necrobomb2/Necrobomb4->2753/player/2757/2752
    CrawlingNecrobombs = 79 // Necrobomb7/Necrobomb8/Necrobomb5/Necrobomb6->player/2753/2757/2752
}

class AbsoluteDarkII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AbsoluteDarkII, new AOEShapeCone(40f, 60f.Degrees()));
class PainMire(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PainMire, 9f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.Enemies((uint)OID.BleedVoidzone).Any(x => x.EventState != 7))
        { }
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class BleedVoidzone(BossModule module) : Components.Voidzone(module, 8f, m => m.Enemies((uint)OID.BleedVoidzone).Where(x => x.EventState != 7));
class TwistedTouch(BossModule module) : Components.SingleTargetCast(module, (uint)AID.TwistedTouch);
class ChaosStorm(BossModule module) : Components.RaidwideCast(module, (uint)AID.ChaosStorm);
class DarkDeluge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkDeluge, 5f);
class NecrobombBaitAway(BossModule module) : Components.BaitAwayIcon(module, 9.25f, (uint)IconID.Baitaway, (uint)AID.DeathThroes); // note: explosion is not always exactly the position of player, if zombie teleports to player it is player + zombie hitboxradius = 1.25 away

class Necrobombs(BossModule module) : BossComponent(module)
{
    private readonly NecrobombBaitAway _ba = module.FindComponent<NecrobombBaitAway>()!;
    private static readonly AOEShapeCircle circle = new(8f);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_ba.ActiveBaits.Count != 0)
            return;
        var forbidden = new List<ShapeDistance>();
        foreach (var e in WorldState.Actors.Where(x => !x.IsAlly && x.Tether.ID == (uint)TetherID.CrawlingNecrobombs))
            forbidden.Add(circle.Distance(e.Position, default));
        if (forbidden.Count != 0)
            hints.AddForbiddenZone(new SDUnion([.. forbidden]));
    }
}

class Burst(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeCircle circle = new(8f);

    // Note: Burst5 to Burst8 locations are unknown until players unable to move, so they are irrelevant and not drawn
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (modelState == 54)
        {
            _aoes.Add(new(circle, actor.Position.Quantized(), default, WorldState.FutureTime(6d))); // activation time can be vastly different, even twice as high so we take a conservative delay
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.Burst1:
                case (uint)AID.Burst2:
                case (uint)AID.Burst3:
                case (uint)AID.Burst4:
                case (uint)AID.Burst5:
                case (uint)AID.Burst6:
                case (uint)AID.Burst7:
                case (uint)AID.Burst8:
                    _aoes.Clear();
                    break;
            }
        }
    }
}

class Doom(BossModule module) : Components.CleansableDebuff(module, (uint)SID.Doom);

class D112SpectralNecromancerStates : StateMachineBuilder
{
    public D112SpectralNecromancerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AbsoluteDarkII>()
            .ActivateOnEnter<PainMire>()
            .ActivateOnEnter<BleedVoidzone>()
            .ActivateOnEnter<TwistedTouch>()
            .ActivateOnEnter<ChaosStorm>()
            .ActivateOnEnter<DarkDeluge>()
            .ActivateOnEnter<NecrobombBaitAway>()
            .ActivateOnEnter<Necrobombs>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<Doom>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 737, NameID = 9508)]
public class D112SpectralNecromancer(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Circle(new(-450f, -531f), 19.5f)], [new Rectangle(new(-470f, -531f), 1.25f, 20f)]);
}
