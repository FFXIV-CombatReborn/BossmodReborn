namespace BossMod.Dawntrail.Quest.MSQ.TheProtectorAndTheDestroyer.Gwyddrud;

public enum OID : uint
{
    Boss = 0x4349, // R5.0    
    LimitBreakHelper = 0x40B5, // R1.0
    BallOfLevin = 0x434A, // R1.5
    SuperchargedLevin = 0x39C4, // R2.0
    Helper2 = 0x3A5E, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player/WukLamat, no cast, single-target
    CracklingHowlVisual = 38211, // Boss->self, 4.3+0,7s cast, single-target
    CracklingHowl = 38212, // Helper->self, 5.0s cast, range 40 circle
    Teleport = 38213, // Boss->location, no cast, single-target
    VioletVoltageTelegraph = 38220, // Helper->self, 2.5s cast, range 20 180-degree cone
    VioletVoltage = 38221, // Helper->self, no cast, range 20 180-degree cone
    VioletVoltageVisual1 = 38214, // Boss->self, 8.3+0,7s cast, single-target
    VioletVoltageVisual2 = 38216, // Boss->self, no cast, single-target
    VioletVoltageVisual3 = 38217, // Boss->self, no cast, single-target
    VioletVoltageVisual4 = 38215, // Boss->self, 10.3+0,7s cast, single-target
    VioletVoltageVisual5 = 38218, // Boss->self, no cast, single-target
    VioletVoltageVisual6 = 38219, // Boss->self, no cast, single-target
    Gnaw = 38222, // Boss->tank, 5.0s cast, single-target
    RollingThunderVisual = 38223, // Boss->self, 4.3+0,7s cast, single-target
    RollingThunder = 38224, // Helper->self, 5.0s cast, range 20 45-degree cone
    RoaringBoltKB = 38230, // Boss->self, 4.3+0,7s cast, range 20 circle, knockback 12, away from source
    RoaringBolt = 38231, // Helper->location, 7.0s cast, range 6 circle
    GatheringStorm = 38225, // Boss->self, no cast, single-target, limit break phase
    LevinStartMoving = 38226, // BallOfLevin/SuperchargedLevin->LimitBreakHelper, no cast, single-target
    GatheringSurge = 38243, // Boss->self, no cast, single-target
    UntamedCurrent = 38232, // Boss->self, 3.3+0,7s cast, range 40 circle, knockback 15, away from source
    UntamedCurrentAOE1 = 38233, // Helper2->location, 3.1s cast, range 5 circle
    UntamedCurrentAOE2 = 19718, // Helper2->location, 3.2s cast, range 5 circle
    UntamedCurrentAOE3 = 19719, // Helper2->location, 3.3s cast, range 5 circle
    UntamedCurrentAOE4 = 19999, // Helper2->location, 3.0s cast, range 5 circle
    UntamedCurrentAOE5 = 38234, // Helper2->location, 3.1s cast, range 5 circle
    UntamedCurrentAOE6 = 19720, // Helper2->location, 3.2s cast, range 5 circle
    UntamedCurrentAOE7 = 19721, // Helper2->location, 3.3s cast, range 5 circle
    UntamedCurrentAOE8 = 19728, // Helper2->location, 3.3s cast, range 5 circle (outside arena)
    UntamedCurrentAOE9 = 19727, // Helper2->location, 3.2s cast, range 5 circle (outside arena)
    UntamedCurrentAOE10 = 19179, // Helper2->location, 3.1s cast, range 5 circle (outside arena)
    UntamedCurrentSpread = 19181, // Helper->all, 5.0s cast, range 5 circle
    UntamedCurrentStack = 19276, // Helper->Alisaie, 5.0s cast, range 6 circle
}

class Gnaw(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Gnaw));
class CracklingHowl(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.CracklingHowl));
class UntamedCurrentRaidwide(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.UntamedCurrent));

class VioletVoltage(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeCone cone = new(20, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            yield break;
        for (var i = 0; i < (count > 2 ? 2 : count); ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                yield return count != 1 ? aoe with { Color = Colors.Danger } : aoe;
            else
                yield return aoe with { Risky = !aoe.Rotation.AlmostEqual(_aoes[0].Rotation + 180.Degrees(), Angle.DegToRad) };
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.VioletVoltageTelegraph)
            _aoes.Add(new(cone, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 6)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && (AID)spell.Action.ID == AID.VioletVoltage)
            _aoes.RemoveAt(0);
    }
}

class RoaringBoltKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.RoaringBoltKB), 12, stopAtWall: true)
{
    private readonly RoaringBolt _aoe = module.FindComponent<RoaringBolt>()!;
    private static readonly Angle a25 = 25.Degrees();

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        foreach (var z in _aoe.ActiveAOEs(slot, actor))
            if (z.Shape.Check(pos, z.Origin, z.Rotation))
                return true;
        return false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
            return;

        var forbidden = new List<Func<WPos, float>>(6);
        var aoes = _aoe.Casters;

        Source source = default;
        foreach (var s in Sources(slot, actor))
        {
            source = s;
            break;
        }
        var count = aoes.Count;
        if (source != default && count != 0)
        {
            for (var i = 0; i < count; ++i)
                forbidden.Add(ShapeDistance.Cone(Arena.Center, 20, Angle.FromDirection(aoes[i].Origin - Arena.Center), a25));
            float minDistanceFunc(WPos pos)
            {
                var minDistance = float.MaxValue;
                for (var i = 0; i < forbidden.Count; ++i)
                {
                    var distance = forbidden[i](pos);
                    if (distance < minDistance)
                        minDistance = distance;
                }
                return minDistance;
            }

            hints.AddForbiddenZone(minDistanceFunc, source.Activation);
        }
    }
}

class RollingThunder : Components.SimpleAOEs
{
    public RollingThunder(BossModule module) : base(module, ActionID.MakeSpell(AID.RollingThunder), new AOEShapeCone(20, 22.5f.Degrees()), 6) { MaxDangerColor = 2; }
}

class RoaringBolt(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.RoaringBolt), 6);
class UntamedCurrentSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.UntamedCurrentSpread), 5);
class UntamedCurrentStack(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.UntamedCurrentStack), 6);

class UntamedCurrentAOEs(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(11);
    private static readonly AOEShapeCircle circle = new(5);
    private static readonly HashSet<AID> casts = [AID.UntamedCurrentAOE1, AID.UntamedCurrentAOE2, AID.UntamedCurrentAOE3, AID.UntamedCurrentAOE4,
    AID.UntamedCurrentAOE5, AID.UntamedCurrentAOE6, AID.UntamedCurrentAOE7, AID.UntamedCurrentAOE8, AID.UntamedCurrentAOE9, AID.UntamedCurrentAOE10];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (casts.Contains((AID)spell.Action.ID))
            _aoes.Add(new(circle, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), ActorID: caster.InstanceID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (casts.Contains((AID)spell.Action.ID))
        {
            for (var i = 0; i < _aoes.Count; ++i)
            {
                var aoe = _aoes[i];
                if (aoe.ActorID == caster.InstanceID)
                {
                    _aoes.Remove(aoe);
                    break;
                }
            }
        }
    }
}

class GwyddrudStates : StateMachineBuilder
{
    public GwyddrudStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CracklingHowl>()
            .ActivateOnEnter<UntamedCurrentRaidwide>()
            .ActivateOnEnter<UntamedCurrentAOEs>()
            .ActivateOnEnter<UntamedCurrentSpread>()
            .ActivateOnEnter<UntamedCurrentStack>()
            .ActivateOnEnter<RoaringBolt>()
            .ActivateOnEnter<RoaringBoltKB>()
            .ActivateOnEnter<RollingThunder>()
            .ActivateOnEnter<VioletVoltage>()
            .ActivateOnEnter<Gnaw>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70478, NameID = 13170)]
public class Gwyddrud(WorldState ws, Actor primary) : BossModule(ws, primary, new(349, -14), new ArenaBoundsCircle(19.5f))
{
    private static readonly uint[] all = [(uint)OID.Boss, (uint)OID.SuperchargedLevin, (uint)OID.BallOfLevin];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(all));
    }
}