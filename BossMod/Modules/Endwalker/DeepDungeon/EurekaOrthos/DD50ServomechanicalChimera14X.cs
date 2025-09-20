namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.DD50ServomechanicalChimera14X;

public enum OID : uint
{
    Boss = 0x3D9C, // R6.0
    Cacophony = 0x3D9D, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target

    SongsOfIceAndThunder = 31851, // Boss->self, 5.0s cast, range 9 circle
    SongsOfThunderAndIce = 31852, // Boss->self, 5.0s cast, range 8-40 donut
    TheRamsVoice1 = 31854, // Boss->self, no cast, range 9 circle
    TheRamsVoice2 = 32807, // Boss->self, no cast, range 9 circle
    TheDragonsVoice1 = 31853, // Boss->self, no cast, range 8-40 donut
    TheDragonsVoice2 = 32806, // Boss->self, no cast, range 8-40 donut
    RightbreathedCold = 31863, // Boss->self, 5.0s cast, range 40 180-degree cone
    LeftbreathedThunder = 31861, // Boss->self, 5.0s cast, range 40 180-degree cone
    ColdThunder = 31855, // Boss->player, 5.0s cast, width 8 rect charge
    ThunderousCold = 31856, // Boss->player, 5.0s cast, width 8 rect charge
    Cacophony = 31864, // Boss->self, 3.0s cast, single-target
    ChaoticChorus = 31865 // Cacophony->self, no cast, range 6 circle
}

public enum TetherID : uint
{
    OrbTether = 17 // Cacophony->player
}

class RamsDragonVoice(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(8f, 40f);
    private static readonly AOEShapeCircle circle = new(9f);
    private readonly List<AOEInstance> _aoes = new(2);
    private readonly List<(AOEShape, DateTime)> _shapes = new(2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var countA = _aoes.Count;
        if (countA != 0)
        {
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < countA; ++i)
            {
                ref var aoe = ref aoes[i];
                if (i == 0)
                {
                    if (countA > 1)
                        aoe.Color = Colors.Danger;
                    aoe.Risky = true;
                }
                else
                    aoe.Risky = false;
            }
            return aoes;
        }
        var countS = _shapes.Count;
        if (countS != 0)
        {
            var aoes = new AOEInstance[countS];
            var shapes = CollectionsMarshal.AsSpan(_shapes);
            var pos = Module.PrimaryActor.Position.Quantized();
            for (var i = 0; i < countS; ++i)
            {
                ref readonly var shape = ref shapes[i];
                aoes[i] = new(shape.Item1, pos, default, shape.Item2, i == 0 && countS > 1 ? Colors.Danger : 0u, i != 0);
            }
            return aoes;
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.SongsOfIceAndThunder:
                AddAOEs(circle, donut);
                break;
            case (uint)AID.SongsOfThunderAndIce:
                AddAOEs(donut, circle);
                break;
        }

        void AddAOEs(AOEShape first, AOEShape second)
        {
            var position = spell.LocXZ;
            _aoes.Add(new(first, position, default, Module.CastFinishAt(spell)));
            _aoes.Add(new(second, position, default, Module.CastFinishAt(spell, 3f)));
        }
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (_aoes.Count != 0 || _shapes.Count != 0)
            return;

        if (modelState == 4)
            AddAOEs(circle, donut);
        else if (modelState == 5)
            AddAOEs(donut, circle);
        void AddAOEs(AOEShape first, AOEShape second)
        {
            _shapes.Add((first, WorldState.FutureTime(2.4d)));
            _shapes.Add((second, WorldState.FutureTime(5.5d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TheDragonsVoice1:
            case (uint)AID.TheRamsVoice1:
            case (uint)AID.TheDragonsVoice2:
            case (uint)AID.TheRamsVoice2:
            case (uint)AID.SongsOfIceAndThunder:
            case (uint)AID.SongsOfThunderAndIce:
                if (_aoes.Count != 0)
                    _aoes.RemoveAt(0);
                else if (_shapes.Count != 0)
                    _shapes.RemoveAt(0);
                break;
        }
    }
}

class TC(BossModule module, uint aid) : Components.BaitAwayChargeCast(module, aid, 4f);
class ThunderousCold(BossModule module) : TC(module, (uint)AID.ThunderousCold);
class ColdThunder(BossModule module) : TC(module, (uint)AID.ColdThunder);

class Breath(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(40f, 90f.Degrees()));
class RightbreathedCold(BossModule module) : Breath(module, (uint)AID.RightbreathedCold);
class LeftbreathedThunder(BossModule module) : Breath(module, (uint)AID.LeftbreathedThunder);

class ChargeTether(BossModule module) : Components.StretchTetherDuo(module, 15f, 5.1f);

class Cacophony(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(6);
    private readonly List<Actor> _orbs = new(2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _orbs.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
            aoes[i] = new(circle, _orbs[i].Position);
        return aoes;
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.Cacophony)
            _orbs.Add(source);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ChaoticChorus)
            _orbs.Remove(caster);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _orbs.Count;
        if (count == 0)
            return;
        for (var i = 0; i < count; ++i)
        {
            var w = _orbs[i];
            hints.AddForbiddenZone(new SDCapsule(w.Position, w.Rotation, 10f, 6f));
        }
    }
}

class CacophonyTether(BossModule module) : Components.StretchTetherSingle(module, (uint)TetherID.OrbTether, 15f, needToKite: true);

class DD50ServomechanicalChimera14XStates : StateMachineBuilder
{
    public DD50ServomechanicalChimera14XStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Cacophony>()
            .ActivateOnEnter<CacophonyTether>()
            .ActivateOnEnter<RamsDragonVoice>()
            .ActivateOnEnter<ThunderousCold>()
            .ActivateOnEnter<ColdThunder>()
            .ActivateOnEnter<RightbreathedCold>()
            .ActivateOnEnter<LeftbreathedThunder>()
            .ActivateOnEnter<ChargeTether>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 901, NameID = 12265)]
public class DD50ServomechanicalChimera14X(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300f, -300f), new ArenaBoundsCircle(19.5f));
