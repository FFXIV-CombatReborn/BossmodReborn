namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.DD30Ningishzida;

public enum OID : uint
{
    Boss = 0x16AC, // R4.800, x1
    FireVoidPuddle = 0x1E8D9B, // R0.500, x0 (spawn during fight), EventObj type
    IceVoidPuddle = 0x1E8D9C // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 6501, // Boss->player, no cast, range 6+R ?-degree cone

    Dissever = 6426, // Boss->self, no cast, range 6+R 90-degree cone
    BallOfFire = 6427, // Boss->location, no cast, range 6 circle
    BallOfIce = 6428, // Boss->location, no cast, range 6 circle
    FearItself = 6429 // Boss->self, 6.0s cast, range 54+R circle
}

class Dissever(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.Dissever), new AOEShapeCone(10.8f, 45.Degrees()), activeWhileCasting: false);
class BallofFire(BossModule module) : Components.PersistentVoidzone(module, 6, m => m.Enemies(OID.FireVoidPuddle).Where(z => z.EventState != 7));
class BallofIce(BossModule module) : Components.PersistentVoidzone(module, 6, m => m.Enemies(OID.IceVoidPuddle).Where(z => z.EventState != 7));
class FearItself(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.FearItself), new AOEShapeDonut(5, 50));

class Hints(BossModule module) : BossComponent(module)
{
    public int NumCasts { get; private set; }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BallOfFire or AID.BallOfIce or AID.FearItself)
            ++NumCasts;

        if (NumCasts >= 5)
        {
            NumCasts = 0;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (NumCasts < 4)
            hints.Add($"Bait the boss away from the middle of the arena. \n{Module.PrimaryActor.Name} will cast x2 Fire Puddles & x2 Ice Puddles. \nAfter the 4th puddle is dropped, run to the middle.");
        if (NumCasts >= 4)
            hints.Add($"Run to the middle of the arena! \n{Module.PrimaryActor.Name} is about to cast a donut AOE!");
    }
}

class DD30NingishzidaStates : StateMachineBuilder
{
    public DD30NingishzidaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Dissever>()
            .ActivateOnEnter<BallofFire>()
            .ActivateOnEnter<BallofIce>()
            .ActivateOnEnter<FearItself>()
            .ActivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 176, NameID = 5012)]
public class DD30Ningishzida(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -235), new ArenaBoundsCircle(25));
