using BossMod.Network.ServerIPC;

namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.DD40TwintaniasClone;

public enum OID : uint
{
    Boss = 0x3D1D, // R6.0
    Twister = 0x1E8910, // R0.5
    BitingWind = 0x3D1E, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    TwisterVisual = 31468, // Boss->self, 5.0s cast, single-target
    TwisterTouch = 31470, // Helper->player, no cast, single-target, player got hit by twister
    MeracydianCyclone = 31462, // Boss->self, 3.0s cast, single-target
    Gust = 31463, // Helper->location, 4.0s cast, range 5 circle
    MeracydianSquallVisual = 31465, // Boss->self, 3.0s cast, single-target
    MeracydianSquall = 31466, // Helper->location, 5.0s cast, range 5 circle
    BitingWind = 31464, // BitingWind->self, no cast, range 5 circle
    Turbine = 31467, // Boss->self, 6.0s cast, range 60 circle, knockback 15, away from source
    TwistingDive = 31471 // Boss->self, 5.0s cast, range 50 width 15 rect
}

class Twister(BossModule module) : Components.CastTwister(module, 1.5f, (uint)OID.Twister, (uint)AID.TwisterVisual, 0.4f, 0.25f);
class BitingWind(BossModule module) : Components.VoidzoneAtCastTarget(module, 5f, (uint)AID.Gust, GetVoidzones, 0.9f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.BitingWind);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

class MeracydianSquall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MeracydianSquall, 5f);

class TwistersHint(BossModule module, uint aid) : Components.CastHint(module, aid, "Twisters soon, get moving!");
class Twisters1(BossModule module) : TwistersHint(module, (uint)AID.TwisterVisual);
class Twisters2(BossModule module) : TwistersHint(module, (uint)AID.TwistingDive);
class DiveTwister(BossModule module) : Components.CastTwister(module, 1.5f, (uint)OID.Twister, (uint)AID.TwistingDive, 0.4f, 0.25f);

class TwistingDive(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeRect rect = new(50f, 7.5f);
    private bool preparing;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor == Module.PrimaryActor)
        {
            if (id == 0x1E3A)
            {
                preparing = true;
            }
            else if (preparing && id == 0x1E43)
            {
                _aoe = [new(rect, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(6.9d))];
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TwistingDive)
        {
            _aoe = [];
            preparing = false;
        }
    }
}

class Turbine(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Turbine, 15f, true)
{
    private readonly BitingWind _aoe = module.FindComponent<BitingWind>()!;
    private static readonly Angle a20 = 20f.Degrees();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var component = _aoe.ActiveAOEs(slot, actor);
            var len = component.Length;
            var forbidden = new ShapeDistance[len + 1];
            forbidden[len] = new SDInvertedCircle(Arena.Center, 5f);
            for (var i = 0; i < len; ++i)
            {
                forbidden[i] = new SDCone(Arena.Center, 20f, Angle.FromDirection(component[i].Origin - Arena.Center), a20);
            }
            hints.AddForbiddenZone(new SDIntersection(forbidden), c.Activation);
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }
}

class DD40TwintaniasCloneStates : StateMachineBuilder
{
    public DD40TwintaniasCloneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Twister>()
            .ActivateOnEnter<Twisters1>()
            .ActivateOnEnter<Twisters2>()
            .ActivateOnEnter<BitingWind>()
            .ActivateOnEnter<MeracydianSquall>()
            .ActivateOnEnter<Turbine>()
            .ActivateOnEnter<TwistingDive>()
            .ActivateOnEnter<DiveTwister>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 900, NameID = 12263)]
public class DD40TwintaniasClone(WorldState ws, Actor primary) : BossModule(ws, primary, new(-600f, -300f), new ArenaBoundsCircle(20f));
