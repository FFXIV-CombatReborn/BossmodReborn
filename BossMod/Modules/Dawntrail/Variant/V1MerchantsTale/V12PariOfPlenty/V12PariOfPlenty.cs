using BossMod.Autorotation.xan;

namespace BossMod.Dawntrail.VariantCriterion.V1MerchantsTale.V12PariOfPlenty;

sealed class HeatBurst(BossModule module) : Components.RaidwideCast(module, (uint)AID.HeatBurst);
sealed class BurningBleam(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BurningGleam1, (uint)AID.BurningGleam2], new AOEShapeCross(40f, 5f));
sealed class SpurningFlames(BossModule module) : Components.RaidwideCast(module, (uint)AID.SpurningFlames);
sealed class ImpassionedSparks(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ImpassionedSparks, 8f, 4);
sealed class ScouringScorn(BossModule module) : Components.RaidwideCast(module, (uint)AID.ScouringScorn);
sealed class PredatorySwoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PredatorySwoop, 12f);
sealed class TranscendentFlight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TranscendentFlight, 12f);
sealed class CarpetRide(BossModule module) : Components.GenericAOEs(module)
{
    // tether target position not working for tethers 2 & 3 for ontether
    // helpers may be moving between tether and untether, use untethered
    private readonly List<AOEInstance> _aoes = [];
    private WPos _nextLanding = default;
    private DateTime _firstActivation = default;
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Fireflight)
        {
            var target = WorldState.Actors.Find(tether.Target);
            if (target == null)
                return;

            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var len = aoes.Length;

            var initial = len == 0 ? Arena.Center : _nextLanding;
            _nextLanding = target.Position;

            if (_firstActivation == default)
            {
                _firstActivation = WorldState.FutureTime(11.25d);
            }

            var dist = (_nextLanding - initial).Length();
            var rot = (_nextLanding - initial).ToAngle();
            var activation = _firstActivation.AddSeconds(2d * (len - 1));

            _aoes.Add(new(new AOEShapeRect(dist, 5f), initial, rot, activation));

            if (_aoes.Count == 3)
            {
                // inner slighly larger but untethered helper location not exactly where boss lands
                _aoes.Add(new(new AOEShapeDonut(7.7f, 60f), _nextLanding, activation: activation.AddSeconds(2.75d)));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CarpetRide)
        {
            if (_aoes.Count > 0)
            {
                _aoes.RemoveAt(0);
            }
        }
        else if (spell.Action.ID == (uint)AID.SunCirclet)
        {
            _aoes.Clear();
            _nextLanding = default;
            _firstActivation = default;
        }
    }
}
sealed class LeftRightFireflight(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LeftFireflightTwoNights, (uint)AID.LeftFireflightThreeNights, (uint)AID.RightFireflightTwoNights, (uint)AID.RightFireflightThreeNights], new AOEShapeRect(40f, 2f));
sealed class WheelOfFireflight(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var len = aoes.Length;

        return len == 0 ? [] : aoes[..1];
    }

    private bool _startLeft = false;
    private Angle _currentRot = default;
    private uint _prevIcon = default;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.LeftFireflightTwoNights or (uint)AID.LeftFireflightThreeNights)
        {
            _startLeft = true;
        }
        else if (spell.Action.ID is (uint)AID.RightFireflightTwoNights or (uint)AID.RightFireflightThreeNights)
        {
            _startLeft = false;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is (uint)IconID.TurningRight or (uint)IconID.TurningLeft or (uint)IconID.TurningRRight or (uint)IconID.TurningRLeft)
        {
            if (_prevIcon == default)
            {
                if (_startLeft)
                {
                    _currentRot = iconID is (uint)IconID.TurningRight or (uint)IconID.TurningRRight ? 180f.Degrees() : 0f.Degrees();
                }
                else
                {
                    _currentRot = iconID is (uint)IconID.TurningRight or (uint)IconID.TurningRRight ? 0f.Degrees() : 180f.Degrees();
                }
            }
            else
            {
                if (_prevIcon == iconID)
                {
                    _currentRot += 180f.Degrees();
                }
            }

            _aoes.Add(new(new AOEShapeCone(40f, 90f.Degrees()), Arena.Center, _currentRot));
            _prevIcon = iconID;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.WheelOfFireflight1 or (uint)AID.WheelOfFireflight2 or (uint)AID.WheelOfFireflight3 or (uint)AID.WheelOfFireflight4)
        {
            if (_aoes.Count > 0)
            {
                _aoes.RemoveAt(0);
                if (_aoes.Count == 0)
                {
                    _currentRot = default;
                    _prevIcon = default;
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_aoes.Count > 0)
        {
            hints.AddForbiddenZone(new AOEShapeDonut(3f, 60f), Arena.Center);
        }
    }
}

sealed class GaleForce(BossModule module) : Components.GenericAOEs(module)
{
    private List<AOEInstance> _aoes = [];
    private bool _active = false;
    private bool _tracking = false;
    private Actor? _bird = null;
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _active ? CollectionsMarshal.AsSpan(_aoes) : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GaleForce)
        {
            _active = false;
            _aoes.Add(new(new AOEShapeCircle(15f), caster.Position, actorID: caster.InstanceID));
        }
        else if (spell.Action.ID == (uint)AID.GaleCannon)
        {
            _tracking = true;
            _bird = caster;
        }
    }

    public override void Update()
    {
        // bird isn't finished rotating at start of cast
        // check until rotation doesn't change, then remove sprites that will die
        if (!_tracking)
            return;

        if (_bird == null)
            return;

        if (_bird.LastFrameMovementVec4 == default)
        {
            var shape = new AOEShapeRect(35f, 5f);
            var origin = _bird.Position;
            var rotation = _bird.Rotation;

            List<AOEInstance> survivors = [];
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var len = aoes.Length;

            for (var i = 0; i < len; i++)
            {
                if (!shape.Check(aoes[i].Origin, origin, rotation))
                {
                    survivors.Add(aoes[i]);
                }
            }

            _aoes = survivors;

            _tracking = false;
            _active = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.GaleForce)
        {
            _active = false;
            _aoes.Clear();
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.LegendaryBird1 && id == 0x1E39)
        {
            _active = false;
            _aoes.Clear();
        }
    }
}

sealed class StrongWind(BossModule module) : Components.GenericAOEs(module)
{
    // 22f, moving roughly 6.4f/s
    // initial cast x1, rest cast event x4, First happens in same location of 1st instance of Rest
    private readonly List<AOEInstance> _aoes = [];
    private readonly float dist = 6.4f;
    private readonly AOEShapeCapsule _shape = new(22f, 26f); // length +4f for safety
    private WPos _startPos = default;
    private Angle _rotation = default;
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.StrongWindDirection)
        {
            _startPos = caster.Position;
            _rotation = caster.Rotation;

            for (var i = 0; i < 4; i++)
            {
                _aoes.Add(new(_shape, _startPos + ((i * dist) * _rotation.ToDirection()), _rotation));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.StrongWindRest)
        {
            if (_aoes.Count > 0)
            {
                _aoes.RemoveAt(0);
            }
        }
    }
}

sealed class ThievesWeaves(BossModule module) : Components.GenericAOEs(module)
{
    // gems spawn in 3 of 4 -775 to -745 with 10f between
    // could just track safe carpet and fill unsafe later with AOE
    private bool _start = false;
    private bool _active = false;
    private readonly List<Actor> _gems = [];
    private readonly List<Actor> _carpets = [];
    private Actor? _safeCarpet = null;
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_active)
            return [];

        if (_safeCarpet == null)
            return [];

        List<AOEInstance> aoes = [];
        var carpets = CollectionsMarshal.AsSpan(_carpets);
        var len = carpets.Length;

        for (var i = 0; i < len; i++)
        {
            if (!carpets[i].Position.AlmostEqual(_safeCarpet.Position, 1f))
            {
                // carpets aren't exact (off by .02f) whereas gems are; doubtful it matters
                aoes.Add(new(new AOEShapeCross(40f, 5f), new WPos(MathF.Round(carpets[i].Position.X), MathF.Round(carpets[i].Position.Z))));
            }
        }
        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (!_start)
            return;

        if (actor.OID == (uint)OID._Gen_FieryBauble && id == 0x11D3)
        {
            _gems.Add(actor);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CharmingBaublesThievesWeave)
        {
            _start = true;
        }
        else if (spell.Action.ID == (uint)AID.CarpetCover)
        {
            _start = false;
            _carpets.Add(caster);
            var gems = CollectionsMarshal.AsSpan(_gems);
            var len = gems.Length;

            for (var i = 0; i < len; i++)
            {
                if (caster.Position.AlmostEqual(gems[i].Position, 1f))
                {
                    return;
                }
            }

            _safeCarpet = caster;
        }
        else if (spell.Action.ID == (uint)AID.Unravel)
        {
            _active = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BurningGleamShort)
        {
            _gems.Clear();
            _carpets.Clear();
            _safeCarpet = null;
            _start = false;
            _active = false;
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
StatesType = typeof(V12PariOfPlentyStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = typeof(SID),
TetherIDType = typeof(TetherID),
IconIDType = typeof(IconID),
PrimaryActorOID = (uint)OID.PariOfPlenty,
Contributors = "gynorhino",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.VariantCriterion,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1066u,
NameID = 14274u,
SortOrder = 3,
PlanLevel = 0)]
public class V12PariOfPlenty(WorldState ws, Actor primary) : BossModule(ws, primary, new(-760, -805), new ArenaBoundsSquare(20f));
