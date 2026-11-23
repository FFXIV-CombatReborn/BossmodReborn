namespace BossMod.Endwalker.Hunt.RankS.Burfurlur;

public enum OID : uint
{
    Boss = 0x360A // R6.0
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    Trolling = 27316, // Boss->self, no cast, range 10 circle
    QuintupleInhale1 = 27307, // Boss->self, 4.0s cast, range 27 120-degree cone
    QuintupleInhale24 = 27308, // Boss->self, 0.5s cast, range 27 120-degree cone
    QuintupleSneeze1 = 27309, // Boss->self, 5.0s cast, range 40 120-degree cone
    QuintupleSneeze24 = 27310, // Boss->self, 0.5s cast, range 40 120-degree cone
    QuintupleInhale35 = 27692, // Boss->self, 0.5s cast, range 27 120-degree cone
    QuintupleSneeze35 = 27693, // Boss->self, 0.5s cast, range 40 120-degree cone
    Uppercut = 27314, // Boss->self, 3.0s cast, range 15 120-degree cone
    RottenSpores = 27313 // Boss->location, 3.0s cast, range 6 circle
}

class QuintupleSneeze(BossModule module) : Components.GenericAOEs(module)
{
    private Angle _referenceAngle;
    private readonly List<Angle> _pendingOffsets = [];
    private DateTime _nextSneeze;

    private static readonly AOEShapeCone _shape = new(40f, 60f.Degrees());

    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_nextSneeze != default)
        {
            var count = _pendingOffsets.Count;
            var max = count > 2 ? 2 : count;
            var aoesLen = max;
            if (_aoesBuffer.Length < aoesLen)
                _aoesBuffer = new AOEInstance[aoesLen];
            var aoes = _aoesBuffer;
            for (var i = 0; i < max; ++i)
            {
                aoes[i] = new(_shape, Module.PrimaryActor.Position, _referenceAngle + _pendingOffsets[i], i == 0 ? _nextSneeze : _nextSneeze.AddSeconds(2.2d), count > 1 && i == 0 ? Colors.Danger : default);
            }
            return aoes.AsSpan(0, aoesLen);
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.QuintupleInhale1:
                _referenceAngle = spell.Rotation;
                _pendingOffsets.Clear();
                _pendingOffsets.Add(default);
                _nextSneeze = default;
                break;
            case (uint)AID.QuintupleInhale24:
            case (uint)AID.QuintupleInhale35:
                _pendingOffsets.Add(spell.Rotation - _referenceAngle);
                break;
            case (uint)AID.QuintupleSneeze1:
                _referenceAngle = spell.Rotation;
                _nextSneeze = Module.CastFinishAt(spell);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_pendingOffsets.Count > 0 && spell.Action.ID is (uint)AID.QuintupleSneeze1 or (uint)AID.QuintupleSneeze24 or (uint)AID.QuintupleSneeze35)
        {
            _pendingOffsets.RemoveAt(0);
            _nextSneeze = WorldState.FutureTime(2.2d);
        }
    }
}

class Uppercut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Uppercut, new AOEShapeCone(15f, 60f.Degrees()));
class RottenSpores(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RottenSpores, 6f);

class BurfurlurStates : StateMachineBuilder
{
    public BurfurlurStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<QuintupleSneeze>()
            .ActivateOnEnter<Uppercut>()
            .ActivateOnEnter<RottenSpores>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 10617)]
public class Burfurlur(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
