namespace BossMod.Shadowbringers.Hunt.RankS.ForgivenPedantry;

public enum OID : uint
{
    Boss = 0x298A, // R=5.5
}

public enum AID : uint
{
    AutoAttackSanctifiedScathe = 17439, // Boss->player, no cast, single-target
    LeftCheek1 = 17446, // Boss->self, 5.0s cast, range 60 180-degree cone
    LeftCheek2 = 17447, // Boss->self, no cast, range 60 180-degree cone
    RightCheek1 = 17448, // Boss->self, 5.0s cast, range 60 180-degree cone
    RightCheek2 = 17449, // Boss->self, no cast, range 60 180-degree cone
    TerrifyingGlance = 17955, // Boss->self, 3.0s cast, range 50 circle, gaze
    TheStake = 17443, // Boss->self, 4.0s cast, range 18 circle
    SecondCircle = 17441, // Boss->self, 3.0s cast, range 40 width 8 rect
    CleansingFire = 17442, // Boss->self, 4.0s cast, range 40 circle
    FeveredFlagellation = 17440, // Boss->players, 4.0s cast, range 15 90-degree cone, tankbuster
    SanctifiedShock = 17900, // Boss->player, no cast, single-target, stuns target before WitchHunt
    WitchHunt = 17444, // Boss->players, 3.0s cast, width 10 rect charge
    WitchHunt2 = 17445, // Boss->players, no cast, width 10 rect charge, targets main tank
}

class LeftRightCheek(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(60f, 90f.Degrees());
    private readonly List<AOEInstance> _aoes = new(2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes[i] = count > 1 ? aoe with { Color = Colors.Danger } : aoe;
            else if (i == 1)
                aoes[i] = aoe with { Risky = false };
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.LeftCheek1 or (uint)AID.RightCheek1)
        {
            _aoes.Add(new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.Add(new(cone, spell.LocXZ, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell, 3.1f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.LeftCheek1 or (uint)AID.RightCheek1)
            _aoes.RemoveAt(0);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.LeftCheek2 or (uint)AID.RightCheek2)
            _aoes.RemoveAt(0);
    }
}

class TerrifyingGlance(BossModule module) : Components.CastGaze(module, (uint)AID.TerrifyingGlance);
class TheStake(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheStake, 18f);
class SecondCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SecondCircle, new AOEShapeRect(40f, 4f));
class CleansingFire(BossModule module) : Components.RaidwideCast(module, (uint)AID.CleansingFire);

class FeveredFlagellation(BossModule module) : Components.BaitAwayCast(module, (uint)AID.FeveredFlagellation, new AOEShapeCone(15f, 45f.Degrees()), tankbuster: true, endsOnCastEvent: true, damageType: AIHints.PredictedDamageType.Tankbuster);

class WitchHunt(BossModule module) : Components.GenericBaitAway(module)
{
    private bool witchHunt1done;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SanctifiedShock && WorldState.Actors.Find(spell.MainTargetID) is Actor target)
        {
            CurrentBaits.Add(new(Module.PrimaryActor, target, new AOEShapeRect((target.Position - caster.Position).Length(), 5f)));
        }
        else if (spell.Action.ID == (uint)AID.WitchHunt2)
        {
            CurrentBaits.Clear();
            witchHunt1done = false;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WitchHunt && WorldState.Actors.Find(spell.TargetID) is Actor target)
        {
            CurrentBaits.Clear();
            CurrentBaits.Add(new(Module.PrimaryActor, target, new AOEShapeRect((target.Position - caster.Position).Length(), 5f)));
        }
    }

    public override void Update()
    {
        var count = CurrentBaits.Count;
        if (count == 0)
            return;
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        for (var i = 0; i < count; ++i)
        {
            ref var b = ref baits[i];
            var length = (b.Target.Position - b.Source.Position).Length();
            if (b.Shape is AOEShapeRect rect && rect.LengthFront != length)
            {
                b.Shape = new AOEShapeRect(length, 5f);
            }
        }

        if (witchHunt1done) // updating WitchHunt2 target incase of sudden tank swap
        {
            CurrentBaits.Ref(0).Target = WorldState.Actors.Find(Module.PrimaryActor.TargetID)!;
        }
    }
}

class ForgivenPedantryStates : StateMachineBuilder
{
    public ForgivenPedantryStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LeftRightCheek>()
            .ActivateOnEnter<TerrifyingGlance>()
            .ActivateOnEnter<TheStake>()
            .ActivateOnEnter<SecondCircle>()
            .ActivateOnEnter<CleansingFire>()
            .ActivateOnEnter<FeveredFlagellation>()
            .ActivateOnEnter<WitchHunt>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 8910)]
public class ForgivenPedantry(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
