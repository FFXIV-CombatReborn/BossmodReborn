namespace BossMod.Endwalker.Quest.MSQ.AFrostyReception;

public enum OID : uint
{
    Boss = 0x3646,
    Helper = 0x233C,
    LockOn = 0x3648, // R1.200, x0 (spawn during fight)
}

public enum AID : uint
{
    GigaTempest = 27440, // Boss->self, 5.0s cast, range 20 circle
    SpiralScourge = 27441, // Boss->player/Lucia, 4.0s cast, single-target
    GunberdVisual = 27447, // Boss->self, 5.0s cast, single-target
    Gunberd = 27448, // Boss->players/allies, no cast, range 40 width 6 line stack
    BombsAway1 = 27460, // Boss->self, 3.0s cast, single-target
    BombsAway2 = 27462, // Boss->self, 3.0s cast, single-target
    GaseousBomb = 27464, // Helper->self, 10.0s cast, range 20 proximity
    AetherochemicalGrenadoVisual = 27501, // Boss->player, 5.0s cast, single-target
    AetherochemicalGrenado = 27510, // Helper->player, no cast, range 6 circle
    Ruination1 = 27443, // Boss->self, 5.0s cast, range 40 width 8 cross
    Ruination2 = 27444, // Helper->self, 5.0s cast, range 30 width 8 rect
    ResinBomb = 27449, // Helper->Lyse/Magnai/Sadu/Pipin/Lucia/Cirina, 5.0s cast, range 5 circle
    LockOn1 = 27461, // _Gen_6->self, 1.0s cast, range 6 circle
    MagitekCannon = 27457, // _Gen_TelotekReaper1->player/Lyse/Sadu/Magnai/Lucia/Cirina/Pipin, 5.0s cast, range 6 circle
    Bombardment = 27459, // Helper->location, 4.0s cast, range 6 circle
    LockOn2 = 27463, // _Gen_6->self, 1.0s cast, range 6 circle
}

class GigaTempest(BossModule module) : Components.RaidwideCast(module, (uint)AID.GigaTempest);
class Ruination(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Ruination1, new AOEShapeCross(40f, 4f));
class Ruination2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Ruination2, new AOEShapeRect(30f, 4f));
class ResinBomb(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.ResinBomb, 5f);
class MagitekCannon(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.MagitekCannon, 6f);
class Bombardment(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Bombardment, 6f);
class SpiralScourge(BossModule module) : Components.SingleTargetCast(module, (uint)AID.SpiralScourge);
class GaseousBomb(BossModule module) : Components.ProximityAOEs(module, (uint)AID.GaseousBomb, 18f);

sealed class Gunberd(BossModule module) : Components.GenericAOEs(module, (uint)AID.Gunberd, "Stack in the line!")
{
    private readonly AOEInstance[] _aoe = new AOEInstance[1];
    private bool _active;
    private static readonly AOEShapeRect _shape = new(40f, 3f, invertForbiddenZone: true);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _active ? _aoe : [];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_active && !_aoe[0].Check(actor.Position))
            hints.Add(WarningText);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_active)
            hints.AddPredictedDamage(Raid.WithSlot().Mask(), _aoe[0].Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GunberdVisual)
        {
            _aoe[0] = new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 0.2d), Colors.SafeFromAOE);
            _active = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Gunberd)
            _active = false;
    }
}

sealed class AetherochemicalGrenado(BossModule module) : Components.GenericBaitAway(module, (uint)AID.AetherochemicalGrenado, centerAtTarget: true, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private static readonly AOEShapeCircle _shape = new(6f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AetherochemicalGrenadoVisual && WorldState.Actors.Find(spell.TargetID) is Actor target)
            CurrentBaits.Add(new(caster, target, _shape, Module.CastFinishAt(spell, 0.8d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.AetherochemicalGrenado)
            CurrentBaits.Clear();
    }
}

class LockOn(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor, DateTime)> _casters = [];
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circle = new(6);

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var countC = _casters.Count;
        var countA = _aoes.Count;
        if (countC == 0 && countA == 0)
            return [];
        var aoes = _aoesBuffer;
        aoes.Clear();
        if (countC != 0)
            for (var i = 0; i < countC; ++i)
            {
                var c = _casters[i];
                aoes.Add(new(circle, c.Item1.Position, default, c.Item2));
            }
        aoes.AddRange(_aoes);
        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.LockOn)
            _casters.Add((actor, WorldState.FutureTime(6.6d)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.LockOn1 or (uint)AID.LockOn2)
        {
            for (var i = 0; i < _casters.Count; ++i)
            {
                if (_casters[i].Item1 == caster)
                {
                    _casters.RemoveAt(i);
                    break;
                }
            }
            _aoes.Add(new(circle, spell.LocXZ, default, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.LockOn1 or (uint)AID.LockOn2)
            _aoes.RemoveAt(0);
    }
}

class VergiliaVanCorculumStates : StateMachineBuilder
{
    public VergiliaVanCorculumStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GigaTempest>()
            .ActivateOnEnter<SpiralScourge>()
            .ActivateOnEnter<Gunberd>()
            .ActivateOnEnter<AetherochemicalGrenado>()
            .ActivateOnEnter<GaseousBomb>()
            .ActivateOnEnter<Ruination>()
            .ActivateOnEnter<Ruination2>()
            .ActivateOnEnter<LockOn>()
            .ActivateOnEnter<ResinBomb>()
            .ActivateOnEnter<MagitekCannon>()
            .ActivateOnEnter<Bombardment>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "xan, Kagekazu (ported from upstream by LTS)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69919, NameID = 10572)]
public class VergiliaVanCorculum(WorldState ws, Actor primary) : BossModule(ws, primary, arenaCenter, arena)
{
    private static readonly WPos arenaCenter = new(-0.07f, -79);
    private static readonly ArenaBoundsCustom arena = new([new Polygon(arenaCenter, 19.5f, 20)]);
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        foreach (var a in WorldState.Actors)
        {
            if (!a.IsAlly)
                Arena.Actor(a);
        }
    }
}

