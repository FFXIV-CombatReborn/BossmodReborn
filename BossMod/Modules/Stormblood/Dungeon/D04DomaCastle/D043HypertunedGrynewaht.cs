namespace BossMod.Stormblood.Dungeon.D04DomaCastle.D043HypertunedGrynewaht;

public enum OID : uint
{
    Boss = 0x1BD4, // R0.6
    MagitekChakram = 0x1BD8, // R3.0
    RetunedMagitekBit = 0x1BD6, // R0.9
    RetunedMagitekBitHelper = 0x1BD7, // R0.5
    Helper = 0x1BD5
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    CleanCut = 8369, // MagitekChakram->location, 6.0s cast, width 8 rect charge
    ChainsawFirst = 8360, // Boss->self, 2.0s cast, range 4+R width 2 rect
    ChainsawRest = 8361, // Helper->self, no cast, range 4+R width 2 rect
    DelayActionChargeVisual = 8364, // Boss->self, no cast, single-target
    DelayActionCharge = 8365, // Helper->player, no cast, range 6 circle

    GunsawFirst = 8362, // Boss->self, no cast, range 60+R width 2 rect
    GunsawRest = 8363, // Boss->self, no cast, range 60+R width 2 rect

    ThermobaricChargeVisual = 8366, // Boss->self, no cast, single-target
    ThermobaricChargeVisual2 = 8368, // Boss->self, no cast, single-target
    ThermobaricCharge = 8367, // Helper->location, 10.0s cast, range 60 circle

    ChainMineVisual1 = 9287, // RetunedMagitekBit->self, no cast, range 50+R width 3 rect
    ChainMineVisual2 = 9144, // RetunedMagitekBitHelper->self, no cast, range 3 circle
    ChainMine = 8359 // RetunedMagitekBitHelper->player, no cast, single-target
}

public enum IconID : uint
{
    Spreadmarker = 99 // player
}

public enum TetherID : uint
{
    HexadroneBits = 60 // RetunedMagitekBit->RetunedMagitekBit
}

public enum SID : uint
{
    Prey = 1253 // Boss->player, extra=0x0
}

class CleanCut(BossModule module) : Components.ChargeAOEs(module, (uint)AID.CleanCut, 4f);
class DelayActionCharge(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, (uint)AID.DelayActionCharge, 6f, 4);
class ThermobaricCharge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThermobaricCharge, 30f);

class Chainsaw(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(4.6f, 1f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ChainsawFirst)
        {
            _aoe = [new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ChainsawFirst:
            case (uint)AID.ChainsawRest:
                if (++NumCasts == 5)
                {
                    _aoe = [];
                    NumCasts = 0;
                }
                break;
        }
    }
}

class ChainMine(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(40f, 2f, 10f);
    private readonly ThermobaricCharge _aoe = module.FindComponent<ThermobaricCharge>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);
    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.HexadroneBits)
            _aoes.Add(new(rect, source.Position.Quantized(), source.Rotation, WorldState.FutureTime(5.6d)));
    }
    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.HexadroneBits)
        {
            var count = _aoes.Count;
            var pos = source.Position;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.Origin.AlmostEqual(pos, 1f))
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoe.Casters.Count <= 1)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class ThermobaricChargeBait(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private static readonly AOEShapeCircle circle = new(30f);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Prey)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, circle, status.ExpireAt));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Prey)
            CurrentBaits.Clear();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Count != 0 && CurrentBaits.Ref(0).Target == actor)
            hints.Add("Bait away!");
        else
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentBaits.Count != 0 && CurrentBaits.Ref(0).Target == actor)
            hints.AddForbiddenZone(new SDCircle(Arena.Center, 23f));
    }
}

class Gunsaw(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeRect rect = new(60.9f, 1f);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.GunsawFirst)
        {
            var party = Raid.WithoutSlot(false, true, true);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                var p = party[i];
                if (p.Position.InRect(Module.PrimaryActor.Position.Quantized(), Module.PrimaryActor.Rotation, rect.LengthFront, 0, 0.02f))
                {
                    CurrentBaits.Add(new(caster, p, rect));
                    return;
                }
            }
        }
        else if (spell.Action.ID == (uint)AID.GunsawRest)
        {
            if (++NumCasts == 4)
            {
                CurrentBaits.Clear();
                NumCasts = 0;
            }
        }
    }
}

class D043HypertunedGrynewahtStates : StateMachineBuilder
{
    public D043HypertunedGrynewahtStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ThermobaricChargeBait>()
            .ActivateOnEnter<ThermobaricCharge>()
            .ActivateOnEnter<CleanCut>()
            .ActivateOnEnter<Chainsaw>()
            .ActivateOnEnter<ChainMine>()
            .ActivateOnEnter<DelayActionCharge>()
            .ActivateOnEnter<Gunsaw>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 241, NameID = 6205)]
public class D043HypertunedGrynewaht(WorldState ws, Actor primary) : BossModule(ws, primary, new(-240, -198), new ArenaBoundsSquare(19.5f));
