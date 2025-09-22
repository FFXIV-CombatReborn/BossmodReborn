namespace BossMod.Endwalker.Hunt.RankS.Sphatika;

public enum OID : uint
{
    Boss = 0x3670 // R8.750, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Gnaw = 27617, // Boss->player, 5.0s cast, single-target tankbuster
    Infusion = 27618, // Boss->players, no cast, width 10 rect charge
    Caterwaul = 27619, // Boss->self, 5.0s cast, range 40 circle

    Brace1 = 27620, // Boss->self, 3.0s cast, single-target (??? forward->backward, lickwhip 624->626->715->627->715)
    Brace2 = 27621, // ???
    Brace3 = 27622, // Boss->self, 3.0s cast, single-target (??? backward->rightward, whiplick 625->631->714->633->714)
    Brace4 = 27623, // Boss->self, 3.0s cast, single-target (??? left->right, whiplick 625->632->714->633->714)
    LickwhipStance = 27624, // Boss->self, 5.0s cast, single-target, visual (lick -> whip using first bearing buff)
    WhiplickStance = 27625, // Boss->self, 5.0s cast, single-target, visual (whip -> lick using first bearing buff)
    LongLickForward = 27626, // Boss->self, 1.0s cast, range 40 180-degree cone aimed forward (with forward bearing)
    LongLickBackward = 27627, // Boss->self, 1.0s cast, range 40 180-degree cone aimed backward (with backward bearing)
    LongLickLeftward = 27628, // Boss->self, 1.0s cast, range 40 180-degree cone aimed left (with leftward bearing)
    LongLickRightward = 27629, // Boss->self, 1.0s cast, range 40 180-degree cone aimed right (with rightward bearing)
    HindWhipForward = 27630, // Boss->self, 1.0s cast, range 40 180-degree cone aimed backward (with forward bearing)
    HindWhipBackward = 27631, // Boss->self, 1.0s cast, range 40 180-degree cone aimed forward (with backward bearing)
    HindWhipLeftward = 27632, // Boss->self, 1.0s cast, range 40 180-degree cone aimed right (with leftward bearing)
    HindWhipRightward = 27633, // Boss->self, 1.0s cast, range 40 180-degree cone aimed left (with rightward bearing)
    LongLickSecond = 27714, // Boss->self, 1.0s cast, range 40 180-degree cone (after hind whip)
    HindWhipSecond = 27715 // Boss->self, 1.0s cast, range 40 180-degree cone (after long lick)
}

public enum SID : uint
{
    ForwardBearing = 2835, // Boss->Boss, extra=0x0
    BackwardBearing = 2836, // Boss->Boss, extra=0x0
    LeftwardBearing = 2837, // Boss->Boss, extra=0x0
    RightwardBearing = 2838 // Boss->Boss, extra=0x0
}

class Gnaw(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Gnaw);
class Caterwaul(BossModule module) : Components.RaidwideCast(module, (uint)AID.Caterwaul);

class Stance(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Angle> _pendingCleaves = [];
    private static readonly AOEShapeCone _shape = new(40f, 90f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_pendingCleaves.Count > 0)
            return new AOEInstance[1] { new(_shape, Module.PrimaryActor.Position, _pendingCleaves[0]) }; // TODO: activation
        else
            return [];
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (!(Module.PrimaryActor.CastInfo?.IsSpell() ?? false))
            return;
        var hint = Module.PrimaryActor.CastInfo!.Action.ID switch
        {
            (uint)AID.Brace1 or (uint)AID.Brace2 or (uint)AID.Brace3 or (uint)AID.Brace4 => "Select directions",
            (uint)AID.LickwhipStance => "Cleave sides literal",
            (uint)AID.WhiplickStance => "Cleave sides inverted",
            _ => ""
        };
        if (hint.Length > 0)
            hints.Add(hint);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (caster != Module.PrimaryActor)
            return;

        switch (spell.Action.ID)
        {
            case (uint)AID.LickwhipStance:
                InitCleaves(spell.Rotation, false);
                break;
            case (uint)AID.WhiplickStance:
                InitCleaves(spell.Rotation, true);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_pendingCleaves.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.LongLickForward:
                case (uint)AID.LongLickBackward:
                case (uint)AID.LongLickLeftward:
                case (uint)AID.LongLickRightward:
                case (uint)AID.LongLickSecond:
                case (uint)AID.HindWhipForward:
                case (uint)AID.HindWhipBackward:
                case (uint)AID.HindWhipLeftward:
                case (uint)AID.HindWhipRightward:
                case (uint)AID.HindWhipSecond:
                    _pendingCleaves.RemoveAt(0);
                    break;
            }
    }

    private void InitCleaves(Angle reference, bool inverted)
    {
        // bearings are resolved in UI order; it is forward > backward > left > right, see PartyListPriority column
        List<(Angle offset, int priority)> bearings = [];
        foreach (var s in Module.PrimaryActor.Statuses)
        {
            switch (s.ID)
            {
                case (uint)SID.ForwardBearing:
                    bearings.Add((default, 0));
                    break;
                case (uint)SID.BackwardBearing:
                    bearings.Add((180f.Degrees(), 1));
                    break;
                case (uint)SID.LeftwardBearing:
                    bearings.Add((90f.Degrees(), 2));
                    break;
                case (uint)SID.RightwardBearing:
                    bearings.Add((-90f.Degrees(), 3));
                    break;
            }
        }
        bearings.Sort(static (a, b) => a.priority.CompareTo(b.priority));

        _pendingCleaves.Clear();
        foreach (var b in bearings)
        {
            var dir = reference + b.offset;
            if (inverted)
                dir += 180f.Degrees();
            _pendingCleaves.Add(dir);
            _pendingCleaves.Add(dir + 180f.Degrees());
        }
    }
}

class SphatikaStates : StateMachineBuilder
{
    public SphatikaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Gnaw>()
            .ActivateOnEnter<Caterwaul>()
            .ActivateOnEnter<Stance>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 10618)]
public class Sphatika(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
