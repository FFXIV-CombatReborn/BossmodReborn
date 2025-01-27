﻿namespace BossMod.Endwalker.Hunt.RankS.Ruminator;

public enum OID : uint
{
    Boss = 0x35BD, // R7.800, x1
}

public enum AID : uint
{
    AutoAttack = 871, // Boss->player, no cast, single-target
    ChitinousTraceCircleFirst = 26874, // Boss->self, 4.0s cast, range 8 circle - always followed by circle, does not 'record' shape by itself
    ChitinousTraceDonutFirst = 26875, // Boss->self, 4.0s cast, range 8-40 donut - always followed by donut, does not 'record' shape by itself
    ChitinousTraceCircle = 26876, // Boss->self, 2.5s cast, range 8 circle
    ChitinousTraceDonut = 26877, // Boss->self, 2.5s cast, range 8-40 donut
    ChitinousAdvanceCircleFirst = 26878, // Boss->self, 3.0s cast, range 8 circle
    ChitinousAdvanceDonutFirst = 26879, // Boss->self, 3.0s cast, range 8-40 donut
    ChitinousAdvanceCircleRest = 26880, // Boss->self, no cast, range 8 circle
    ChitinousAdvanceDonutRest = 26881, // Boss->self, no cast, range 8-40 donut
    ChitinousReversalCircleFirst = 26915, // Boss->self, 3.0s cast, range 8 circle
    ChitinousReversalDonutFirst = 26916, // Boss->self, 3.0s cast, range 8-40 donut
    ChitinousReversalCircleRest = 26167, // Boss->self, no cast, range 8 circle
    ChitinousReversalDonutRest = 26168, // Boss->self, no cast, range 8-40 donut
    StygianVapor = 26882, // Boss->self, 5.0s cast, range 40 circle
}

class ChitinousTrace(BossModule module) : Components.GenericAOEs(module)
{
    private bool _active;
    private static readonly AOEShapeCircle circle = new(8);
    private static readonly AOEShapeDonut donut = new(8, 40);
    private static readonly HashSet<AID> castEnds = [AID.ChitinousAdvanceCircleFirst, AID.ChitinousAdvanceCircleRest, AID.ChitinousAdvanceDonutFirst,
    AID.ChitinousAdvanceDonutRest, AID.ChitinousReversalCircleFirst, AID.ChitinousReversalCircleRest, AID.ChitinousReversalDonutFirst, AID.ChitinousReversalDonutRest];
    private readonly List<AOEShape> _pendingShapes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_active && _pendingShapes.Count != 0)
            yield return new(_pendingShapes[0], Module.PrimaryActor.Position); // TODO: activation
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ChitinousTraceCircle:
                _pendingShapes.Add(circle);
                break;
            case AID.ChitinousTraceDonut:
                _pendingShapes.Add(donut);
                break;
            case AID.ChitinousAdvanceCircleFirst:
            case AID.ChitinousAdvanceDonutFirst:
                _active = true;
                break;
            case AID.ChitinousReversalCircleFirst:
            case AID.ChitinousReversalDonutFirst:
                _pendingShapes.Reverse();
                _active = true;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_pendingShapes.Count > 0 && castEnds.Contains((AID)spell.Action.ID))
        {
            _pendingShapes.RemoveAt(0);
            _active = _pendingShapes.Count > 0;
        }
    }
}

class StygianVapor(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.StygianVapor));

class RuminatorStates : StateMachineBuilder
{
    public RuminatorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ChitinousTrace>()
            .ActivateOnEnter<StygianVapor>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 10620)]
public class Ruminator(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
