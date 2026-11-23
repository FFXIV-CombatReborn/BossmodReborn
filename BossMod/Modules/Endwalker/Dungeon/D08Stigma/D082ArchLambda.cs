namespace BossMod.Endwalker.Dungeon.D08Stigma.D082ArchLambda;

public enum OID : uint
{
    Boss = 0x3416, // R=6.0
    Helper = 0x233C
}
public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    AtomicFlame = 25524, // Boss->self, 5.0s cast, range 40 circle //Raidwide
    AutoMobileAssaultCannon = 25515, // Boss->self, 5.9s cast, single-target
    AutoMobileSniperCannon1 = 25520, // Boss->location, 7.0s cast, single-target
    AutoMobileSniperCannon2 = 25522, // Helper->self, no cast, range 40 width 6 rect 
    Entrench = 25521, // Helper->self, 7.5s cast, range 41 width 8 rect
    Tread1 = 25516, // Boss->location, no cast, width 8 rect charge
    Tread2 = 25517, // Boss->location, no cast, width 8 rect charge
    AutoMobileAssaultCannonStart = 25514, // Boss->location, no cast, single-target
    TreadTelegraph = 25518, // Helper->location, 1.5s cast, width 8 rect charge
    WaveCannon = 25519, // Boss->self, 2.5s cast, range 40 180-degree cone
    Wheel = 25525 // Boss->player, 5.0s cast, single-target //Tankbuster
}

public enum IconID : uint
{
    SniperCannon1 = 79,
    SniperCannon4 = 82
}

sealed class AutoMobileSniperCannon(BossModule module) : Components.GenericBaitAway(module, (uint)AID.AutoMobileSniperCannon2)
{
    private static readonly AOEShapeRect rect = new(40f, 3f);
    private readonly Dictionary<uint, (Actor? Helper, Actor? Player)> _pairs = [];

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is < (uint)IconID.SniperCannon1 or > (uint)IconID.SniperCannon4)
        {
            return;
        }
        var pair = _pairs.GetValueOrDefault(iconID);
        if (actor.OID == (uint)OID.Helper)
        {
            pair.Helper = actor;
        }
        else
        {
            pair.Player = actor;
        }
        if (pair.Helper != null && pair.Player != null)
        {
            CurrentBaits.Add(new(pair.Helper, pair.Player, rect, WorldState.FutureTime(7.5d)));
            _pairs.Remove(iconID);
        }
        else
        {
            _pairs[iconID] = pair;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            var count = CurrentBaits.Count;
            for (var i = count - 1; i >= 0; --i)
            {
                if (CurrentBaits[i].Source == caster)
                {
                    CurrentBaits.RemoveAt(i);
                }
            }
        }
    }
}

class Tread(BossModule module) : Components.ChargeAOEs(module, (uint)AID.TreadTelegraph, 4f);
class Entrench(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Entrench, new AOEShapeRect(41, 4));

class WaveCannon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WaveCannon, new AOEShapeCone(40, 90.Degrees()));

class AtomicFlame(BossModule module) : Components.RaidwideCast(module, (uint)AID.AtomicFlame);
class Wheel(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Wheel);

class D082ArchLambdaStates : StateMachineBuilder
{
    public D082ArchLambdaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Tread>()
            .ActivateOnEnter<Entrench>()
            .ActivateOnEnter<AutoMobileSniperCannon>()
            .ActivateOnEnter<WaveCannon>()
            .ActivateOnEnter<AtomicFlame>()
            .ActivateOnEnter<Wheel>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team (LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 784, NameID = 10403)]
public class D082ArchLambda(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(20));
