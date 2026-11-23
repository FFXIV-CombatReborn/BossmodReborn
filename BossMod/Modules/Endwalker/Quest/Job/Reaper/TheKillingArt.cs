namespace BossMod.Endwalker.Quest.Job.Reaper.TheKillingArt;

public enum OID : uint
{
    Boss = 0x3664, // R1.5
    VoidHecteyes = 0x3666, // R1.2
    VoidPersona = 0x3667, // R1.2
    Voidzone = 0x1E963D,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss/VoidPersona->player/Drusilla, no cast, single-target
    AutoAttack2 = 872, // VoidHecteyes->player/Drusilla, no cast, single-target

    VoidCall = 27589, // Boss->self, 4.0s cast, single-target
    MeatySliceVisual = 27590, // Boss->self, 3.4+0.6s cast, single-target
    MeatySlice = 27591, // Helper->self, 4.0s cast, range 50 width 12 rect
    CleaverVisual = 27594, // Boss->self, 3.5+0.5s cast, single-target
    Cleaver = 27595, // Helper->self, 4.0s cast, range 40 120-degree cone
    FlankCleaverVisual = 27596, // Boss->self, 3.5+0.5s cast, single-target
    FlankCleaver = 27597, // Helper->self, 4.0s cast, range 40 120-degree cone
    Explosion1 = 27606, // VoidHecteyes->self, 20.0s cast, range 60 circle
    Explosion2 = 27607, // VoidPersona->self, 20.0s cast, range 50 circle
    FocusInferiVisual = 27592, // Boss->self, 2.9+0.6s cast, single-target
    FocusInferi = 27593, // Helper->location, 3.5s cast, range 6 circle
    CarnemLevareVisual = 27598, // Boss->self, 4.0s cast, single-target
    CarnemLevare1 = 27599, // Helper->self, 4.0s cast, range 40 width 8 cross
    CarnemLevare2 = 27602, // Helper->self, 3.5s cast, range 12-17 180-degree donut sector
    CarnemLevare3 = 27600, // Helper->self, 3.5s cast, range 2-7 180-degree donut sector
    CarnemLevare4 = 27603, // Helper->self, 3.5s cast, range 17-22 180-degree donut sector
    CarnemLevare5 = 27601, // Helper->self, 3.5s cast, range 7-12 180-degree donut sector
    VoidMortar = 27604, // Boss->self, 4.0+1.0s cast, single-target
    VoidMortar1 = 27605 // Helper->self, 5.0s cast, range 13 circle
}

class VoidMortar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidMortar1, 13);
class FocusInferi(BossModule module) : Components.VoidzoneAtCastTarget(module, 6, (uint)AID.FocusInferi, m => m.Enemies((uint)OID.Voidzone).Where(x => x.EventState != 7), 0);
class CarnemLevareCross(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CarnemLevare1, new AOEShapeCross(40, 4));

class CarnemLevareDonut(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(4)];
    private static readonly Angle a90 = 90.Degrees();
    private static readonly AOEShapeDonutSector[] sectors = [new(12, 17, a90), new(2, 7, a90), new(17, 22, a90), new(7, 12, a90)];

    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 4 ? 4 : count;
        var aoesLen = max;
        if (_aoesBuffer.Length < aoesLen)
            _aoesBuffer = new AOEInstance[aoesLen];
        var aoes = _aoesBuffer;
        for (var i = 0; i < max; ++i)
            aoes[i] = _aoes[i];
        return aoes.AsSpan(0, aoesLen);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = (AID)spell.Action.ID switch
        {
            AID.CarnemLevare2 => sectors[0],
            AID.CarnemLevare3 => sectors[1],
            AID.CarnemLevare4 => sectors[2],
            AID.CarnemLevare5 => sectors[3],
            _ => null
        };

        if (shape != null)
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.CarnemLevare2 or AID.CarnemLevare3 or AID.CarnemLevare4 or AID.CarnemLevare5)
            _aoes.RemoveAt(0);
    }
}
class MeatySlice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MeatySlice, new AOEShapeRect(50, 6));

abstract class Cleave(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(40, 60.Degrees()));
class Cleaver(BossModule module) : Cleave(module, (uint)AID.Cleaver);
class FlankCleaver(BossModule module) : Cleave(module, (uint)AID.FlankCleaver);

class Adds(BossModule module) : Components.AddsMulti(module, [(uint)OID.VoidHecteyes, (uint)OID.VoidPersona], 1);

class OrcusStates : StateMachineBuilder
{
    public OrcusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MeatySlice>()
            .ActivateOnEnter<Cleaver>()
            .ActivateOnEnter<FlankCleaver>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<FocusInferi>()
            .ActivateOnEnter<CarnemLevareCross>()
            .ActivateOnEnter<CarnemLevareDonut>()
            .ActivateOnEnter<VoidMortar>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69614, NameID = 10581)]
public class Orcus(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(-69.569f, -388), 19.5f, 64)], [new Rectangle(new(-69, -368), 20, 0.94f)]);
}
