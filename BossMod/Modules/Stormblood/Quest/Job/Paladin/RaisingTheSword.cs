namespace BossMod.Stormblood.Quest.Job.Paladin.RaisingTheSword;

public enum OID : uint
{
    Boss = 0x1B51,
    AldisSwordOfNald = 0x18D6, // R0.5
    TaintedWindSprite = 0x1B52, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    FastBlade = 9, // Boss->player, no cast, single-target
    SavageBlade = 11, // Boss->player, no cast, single-target
    ageOfHalone = 21, // Boss->player, no cast, single-target
    ShudderingSwipeKB = 8136, // Boss->player, 3.0s cast, single-target
    ShudderingSwipeAOE = 8137, // AldisSwordOfNald->self, 3.0s cast, range 60+R 30-degree cone
    TheFourWinds = 8138, // Boss->self, 3.0s cast, range 60+R circle
    TheFourWindsVoidzone = 8139, // TaintedWindSprite->self, no cast, range 6 circle
    ShieldBlast = 8135, // Boss->player, 3.0s cast, single-target
    NaldsWhisperVisual = 8140, // Boss->self, 9.0s cast, single-target
    NaldsWhisper = 8141, // AldisSwordOfNald->self, 9.0s cast, range 20 circle (4 + 16 from status effect)
    LungeCut = 8133, // Boss->player, no cast, single-target
    VictorySlash = 8134 // Boss->self, 3.0s cast, range 6+R 120-degree cone
}

class VictorySlash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VictorySlash, new AOEShapeCone(6.5f, 60f.Degrees()));
class ShudderingSwipeCone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ShudderingSwipeAOE, new AOEShapeCone(60f, 15f.Degrees()));

class ShudderingSwipeKB(BossModule module) : Components.GenericKnockback(module, stopAtWall: true)
{
    private Knockback[] _kb = [];
    private readonly List<Actor> voidzones = module.Enemies((uint)OID.TaintedWindSprite);
    private readonly TheFourWinds _aoe = module.FindComponent<TheFourWinds>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _kb;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            if (aoes[i].Check(pos))
            {
                return true;
            }
        }
        return false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb.Length != 0)
        {
            ref readonly var kb = ref _kb[0];
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                var origin = kb.Origin;
                var count = voidzones.Count;
                var forbidden = new ShapeDistance[count];

                for (var i = 0; i < count; ++i)
                {
                    var a = voidzones[i].Position;
                    forbidden[i] = new SDCone(origin, 100f, Angle.FromDirection(a - origin), Angle.Asin(6f / (a - origin).Length()));
                }
                hints.AddForbiddenZone(new SDUnion(forbidden), act);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ShudderingSwipeKB)
        {
            _kb = [new(caster.Position.Quantized(), 10f, Module.CastFinishAt(spell))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ShudderingSwipeKB)
        {
            _kb = [];
        }
    }
}

class NaldsWhisper(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NaldsWhisper, 20f);
class TheFourWinds(BossModule module) : Components.Voidzone(module, 6f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.TaintedWindSprite);
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

class AldisSwordOfNaldStates : StateMachineBuilder
{
    public AldisSwordOfNaldStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheFourWinds>()
            .ActivateOnEnter<NaldsWhisper>()
            .ActivateOnEnter<VictorySlash>()
            .ActivateOnEnter<ShudderingSwipeKB>()
            .ActivateOnEnter<ShudderingSwipeCone>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 270, NameID = 6311)]
public class AldisSwordOfNald(WorldState ws, Actor primary) : BossModule(ws, primary, new(-89.3f, default), new ArenaBoundsCircle(20.5f));
