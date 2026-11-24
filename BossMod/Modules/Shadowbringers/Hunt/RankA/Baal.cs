namespace BossMod.Shadowbringers.Hunt.RankA.Baal;

public enum OID : uint
{
    Boss = 0x2854 // R=3.2
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    SewerWater1 = 17956, // Boss->self, 3.0s cast, range 12 180-degree cone
    SewerWater2 = 17957, // Boss->self, 3.0s cast, range 12 180-degree cone
    SewageWaveFirst1 = 17423, // Boss->self, 5.0s cast, range 30 180-degree cone
    SewageWaveFirst2 = 17424, // Boss->self, 5.0s cast, range 30 180-degree cone
    SewageWaveSecond1 = 17422, // Boss->self, no cast, range 30 180-degree cone
    SewageWaveSecond2 = 17421 // Boss->self, no cast, range 30 180-degree cone
}

abstract class SewerWater(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(12f, 90f.Degrees()));
class SewerWater1(BossModule module) : SewerWater(module, (uint)AID.SewerWater1);
class SewerWater2(BossModule module) : SewerWater(module, (uint)AID.SewerWater2);

class SewageWave(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(30f, 90f.Degrees());
    private readonly List<AOEInstance> _aoes = [with(2)];

    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoesLen = count;
        if (_aoesBuffer.Length < aoesLen)
            _aoesBuffer = new AOEInstance[aoesLen];
        var aoes = _aoesBuffer;
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes[i] = count > 1 ? aoe with { Color = Colors.Danger } : aoe;
            else if (i == 1)
                aoes[i] = aoe with { Risky = false };
        }
        return aoes.AsSpan(0, aoesLen);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.SewageWaveFirst1 or (uint)AID.SewageWaveFirst2)
        {
            _aoes.Add(new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.Add(new(cone, spell.LocXZ, spell.Rotation + 180f.Degrees(), Module.CastFinishAt(spell, 2.3f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.SewageWaveFirst1 or (uint)AID.SewageWaveFirst2)
            _aoes.RemoveAt(0);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.SewageWaveSecond1 or (uint)AID.SewageWaveSecond2)
            _aoes.RemoveAt(0);
    }
}

class BaalStates : StateMachineBuilder
{
    public BaalStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SewageWave>()
            .ActivateOnEnter<SewerWater1>()
            .ActivateOnEnter<SewerWater2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 8897)]
public class Baal(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
