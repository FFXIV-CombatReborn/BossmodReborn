namespace BossMod.Dawntrail.VariantCriterion.V1MerchantsTale.V15DeadlyDandan;

sealed class MurkyWaters(BossModule module) : Components.RaidwideCast(module, (uint)AID.MurkyWaters);
sealed class Spit(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spit, new AOEShapeCone(50f, 60f.Degrees())); // could draw early during 2nd devour 
sealed class Dropsea(BossModule module) : Components.BaitAwayCast(module, (uint)AID.Dropsea, 5f);
sealed class MawOfTheDeep(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MawOfTheDeep, 8f, 5);
sealed class TidalGuillotine(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TidalGuillotine, 20f);
sealed class SwallowedSea(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SwallowedSea, new AOEShapeCone(50f, 60f.Degrees())); // could draw earlier by checking action timeline 0x11D1
sealed class UnfathomableHorror(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(8f), new AOEShapeDonut(8f, 16f), new AOEShapeDonut(16f, 24f), new AOEShapeDonut(24f, 36f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.UnfathomableHorror1)
        {
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.UnfathomableHorror1 => 0,
                (uint)AID.UnfathomableHorror2 => 1,
                (uint)AID.UnfathomableHorror3 => 2,
                (uint)AID.UnfathomableHorror4 => 3,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}
sealed class AiryBubbles(BossModule module) : Components.Voidzone(module, 5f, GetBubbles, 4f)
{
    public static Actor[] GetBubbles(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.AiryBubble);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.Renderflags == 0 && z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}
sealed class Devour(BossModule module) : Components.GenericAOEs(module)
{
    // jump distance variable, stops once far enough from arena? eg. 55.5f on one jump, 60f the next, stops 30f away from arena center
    // gains/loses status #2195, extra decides whether boss ends in front or back of starting position
    // doesn't start moving until model state change, AnimState1 0/1 for stop/start, starts moving roughly 3s after, stops roughly 1.5s after
    // casts ability #47546 X times, with last time being final spot; nothing to indicate which might be the last cast
    // no recorded ops for devour indicators, no helpers moving; could be fixed degrees?
    // initial angle: 67.5f if backwards, 112.5f if forwards
    // 2nd angle: 157.5 +-
    private readonly Angle backwardsAngle = 67.5f.Degrees();
    private readonly Angle forwardAngle = 112.5f.Degrees();
    private readonly Angle secondAngle = 157.5f.Degrees();
    private bool isBackwards = false;

    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (actor.OID == (uint)OID.DeadlyDandan && status.ID == (uint)SID.DevourDirection)
        {
            isBackwards = status.Extra == 0x3F8;
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DevourCast)
        {
            var startPos = caster.Position;
            var startAngle = caster.Rotation - (isBackwards ? backwardsAngle : forwardAngle);
            var dir = startAngle.ToDirection().Normalized();
            // find point where line is 30f away from center
            var midDist = Intersect.RayCircle(startPos, dir, Arena.Center, 30f);
            var midPos = startPos + dir * midDist;
            var endAngle = startAngle + (isBackwards ? secondAngle * -1 : secondAngle);
            var endDir = endAngle.ToDirection().Normalized();
            var endDist = Intersect.RayCircle(midPos, endDir, Arena.Center, 30f);
            // var endPos = midPos + endDir * endDist;

            _aoes.Add(new(new AOEShapeRect(midDist, 10f), startPos, dir.ToAngle()));
            _aoes.Add(new(new AOEShapeRect(endDist, 10f), midPos, endDir.ToAngle()));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        // looks like all of them cast but we know #45600 does something
        if (spell.Action.ID == (uint)AID._Ability_Devour5)
        {
            if (_aoes.Count > 0)
            {
                _aoes.RemoveAt(0);
            }
        }
    }
}

sealed class StingingTentacle(BossModule module) : Components.GenericAOEs(module)
{
    // initial tentacle rotation random
    // relative to boss rotation, roughly 10 deg outwards and 57 deg inwards
    // slows down slightly at max in/out rotation before moving back opposite way
    private bool _active = false;
    private readonly List<Actor> _tentacles = [];
    private DateTime _activation = default;
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_active)
            return [];

        var tentacles = CollectionsMarshal.AsSpan(_tentacles);
        var len = tentacles.Length;

        if (len == 0)
            return [];

        List<AOEInstance> aoes = [];
        for (var i = 0; i < len; ++i)
        {
            aoes.Add(new(new AOEShapeRect(50f, 7f), tentacles[i].Position, tentacles[i].Rotation, _activation));
        }
        return CollectionsMarshal.AsSpan(aoes);
    }
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Countdown)
        {
            if (!_active)
            {
                _active = true;
                _tentacles.AddRange(Module.Enemies((uint)OID.DeadlyDandan_Tentacle));
            }
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.StingingTentacleCast)
        {
            _activation = Module.CastFinishAt(spell);
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.StingingTentacle)
        {
            _active = false;
            _activation = default;
            _tentacles.Clear();
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
StatesType = typeof(V15DeadlyDandanStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = typeof(SID),
TetherIDType = null,
IconIDType = typeof(IconID),
PrimaryActorOID = (uint)OID.DeadlyDandan,
Contributors = "gynorhino",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.VariantCriterion,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1066u,
NameID = 14475u,
SortOrder = 6,
PlanLevel = 0)]
public class V15DeadlyDandan(WorldState ws, Actor primary) : BossModule(ws, primary, new(805f, 670f), new ArenaBoundsCircle(20f));
