namespace BossMod.Endwalker.Dungeon.D04KtisisHyperboreia.D042LadonLord;

public enum OID : uint
{
    Boss = 0x3425, // R=3.99
    PyricSphere = 0x3426, // R=0.7
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    Teleport = 25733, // Boss->location, no cast, single-target
    Inhale1 = 25732, // Boss->self, 4.0s cast, single-target
    Inhale2 = 25915, // Boss->self, no cast, single-target
    Intimidation = 25741, // Boss->self, 6.0s cast, range 40 circle, raidwide
    PyricBlast = 25742, // Boss->players, 4.0s cast, range 6 circle, stack
    PyricBreathFront = 25734, // Boss->self, 7.0s cast, range 40 120-degree cone
    PyricBreathLeft = 25735, // Boss->self, 7.0s cast, range 40 120-degree cone
    PyricBreathRight = 25736, // Boss->self, 7.0s cast, range 40 120-degree cone
    PyricBreathFront2 = 25737, // Boss->self, no cast, range 40 120-degree cone
    PyricBreathLeft2 = 25738, // Boss->self, no cast, range 40 120-degree cone
    PyricBreathRight2 = 25739, // Boss->self, no cast, range 40 120-degree cone
    PyricSphereVisual = 25744, // PyricSphere->self, 5.0s cast, single-target
    PyricSphere = 25745, // Helper->self, 10.0s cast, range 50 width 4 cross
    Scratch = 25743, // Boss->player, 5.0s cast, single-target, tankbuster
    SpawnSpheres = 25740 // Boss->self, no cast, ???
}

public enum SID : uint
{
    MiddleHead = 2812, // none->Boss, extra=0x9F6
    LeftHead = 2813, // none->Boss, extra=0x177F
    RightHead = 2814 // none->Boss, extra=0x21A8
}

class PyricBreath(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(2)];
    private readonly List<uint> buffs = [with(2)];
    private static readonly Angle angle = 120f.Degrees();
    private static readonly AOEShapeCone cone = new(40f, 60f.Degrees());

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
            else
                aoes[i] = aoe;
        }
        return aoes.AsSpan(0, aoesLen);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.MiddleHead or (uint)SID.LeftHead or (uint)SID.RightHead)
            buffs.Add(status.ID);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.PyricBreathFront or (uint)AID.PyricBreathLeft or (uint)AID.PyricBreathRight)
        {
            void AddAOE(Angle offset = default, float delay = 2.1f) => _aoes.Add(new(cone, spell.LocXZ, spell.Rotation + offset, Module.CastFinishAt(spell, delay)));
            AddAOE(delay: default);
            if (buffs.Count == 2)
            {
                var buff = buffs[0];
                switch (buffs[1])
                {
                    case (uint)SID.RightHead:
                        AddAOE(buff == (uint)SID.MiddleHead ? -angle : -2f * angle);
                        break;
                    case (uint)SID.LeftHead:
                        AddAOE(buff == (uint)SID.MiddleHead ? angle : 2f * angle);
                        break;
                    case (uint)SID.MiddleHead:
                        AddAOE(buff == (uint)SID.LeftHead ? -angle : angle);
                        break;
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.PyricBreathFront or (uint)AID.PyricBreathLeft or (uint)AID.PyricBreathRight)
        {
            ++NumCasts;
            if (_aoes.Count != 0 && buffs.Count != 0)
            {
                _aoes.RemoveAt(0);
                buffs.RemoveAt(0);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.PyricBreathFront2 or (uint)AID.PyricBreathLeft2 or (uint)AID.PyricBreathRight2)
        {
            _aoes.Clear();
            buffs.Clear();
        }
    }
}

class PyricSphere(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PyricSphere, new AOEShapeCross(50f, 2f)); // we could draw this almost 5s earlier, but why bother with 10s cast time
class PyricBlast(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.PyricBlast, 6f, 4, 4);
class Scratch(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Scratch);
class Intimidation(BossModule module) : Components.RaidwideCast(module, (uint)AID.Intimidation);

class D042LadonLordStates : StateMachineBuilder
{
    public D042LadonLordStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PyricBreath>()
            .ActivateOnEnter<PyricSphere>()
            .ActivateOnEnter<PyricBlast>()
            .ActivateOnEnter<Scratch>()
            .ActivateOnEnter<Intimidation>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 787, NameID = 10398)]
public class D042LadonLord(WorldState ws, Actor primary) : BossModule(ws, primary, new(default, 48f), new ArenaBoundsSquare(19.5f));
