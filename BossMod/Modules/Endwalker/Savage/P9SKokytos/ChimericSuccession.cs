namespace BossMod.Endwalker.Savage.P9SKokytos;

class ChimericSuccession(BossModule module) : Components.UniformStackSpread(module, 6f, 20f, 4)
{
    public int NumCasts;
    private readonly Actor?[] _baitOrder = [null, null, null, null];
    private BitMask _forbiddenStack;
    private DateTime _jumpActivation;

    public bool JumpActive => _jumpActivation != default;

    public override void Update()
    {
        Stacks.Clear();
        var target = JumpActive ? Raid.WithSlot(false, true, true).ExcludedFromMask(_forbiddenStack).Actors().Farthest(Module.PrimaryActor.Position) : null;
        if (target != null)
            AddStack(target, _jumpActivation, _forbiddenStack);
        base.Update();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FrontFirestrikes or (uint)AID.RearFirestrikes)
            _jumpActivation = Module.CastFinishAt(spell, 0.4d);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Icemeld1:
            case (uint)AID.Icemeld2:
            case (uint)AID.Icemeld3:
            case (uint)AID.Icemeld4:
                ++NumCasts;
                InitBaits();
                break;
            case (uint)AID.PyremeldFront:
            case (uint)AID.PyremeldRear:
                _jumpActivation = default;
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var order = (IconID)iconID switch
        {
            IconID.Icon1 => 0,
            IconID.Icon2 => 1,
            IconID.Icon3 => 2,
            IconID.Icon4 => 3,
            _ => -1
        };
        if (order < 0)
            return;
        _baitOrder[order] = actor;
        _forbiddenStack.Set(Raid.FindSlot(actor.InstanceID));
        if (order == 0)
            InitBaits();
    }

    private void InitBaits()
    {
        Spreads.Clear();
        var target = NumCasts < _baitOrder.Length ? _baitOrder[NumCasts] : null;
        if (target != null)
            AddSpread(target, WorldState.FutureTime(NumCasts == 0 ? 10.1f : 3));
    }
}

// TODO: think of a way to show baits before cast start to help aiming outside...
abstract class SwingingKick(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(40f, 90f.Degrees()));
class SwingingKickFront(BossModule module) : SwingingKick(module, (uint)AID.SwingingKickFront);
class SwingingKickRear(BossModule module) : SwingingKick(module, (uint)AID.SwingingKickRear);
