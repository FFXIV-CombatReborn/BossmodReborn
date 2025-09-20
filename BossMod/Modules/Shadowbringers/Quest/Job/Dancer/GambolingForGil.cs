namespace BossMod.Shadowbringers.Quest.Job.Dancer.GambolingForGil;

public enum OID : uint
{
    Boss = 0x29D2, // R0.5
    Whirlwind = 0x29D5, // R1.0
}

public enum AID : uint
{
    WarDance = 17197, // Boss->self, 3.0s cast, range 5 circle
    CharmingChasse = 17198, // Boss->self, 3.0s cast, range 40 circle
    HannishFire1 = 17204, // 29D6->location, 3.3s cast, range 6 circle
    Foxshot = 17289, // Boss->player, 6.0s cast, width 4 rect charge
    HannishWaters = 17214, // 2A0B->self, 5.0s cast, range 40+R 30-degree cone
    RanaasFinish = 15646, // Boss->self, 6.0s cast, range 15 circle
}

class Foxshot(BossModule module) : Components.BaitAwayChargeCast(module, (uint)AID.Foxshot, 2f);

class FoxshotKB(BossModule module) : Components.GenericKnockback(module, stopAtWall: true)
{
    private Knockback[] _kb = [];
    private readonly List<Actor> waterVZs = module.Enemies((uint)OID.Whirlwind);
    private readonly Whirlwind _aoe = module.FindComponent<Whirlwind>()!;

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
            ref var kb = ref _kb[0];
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                var origin = kb.Origin;
                var count = waterVZs.Count;
                var forbidden = new ShapeDistance[count];

                for (var i = 0; i < count; ++i)
                {
                    var a = waterVZs[i].Position;
                    forbidden[i] = new SDCone(origin, 100f, Angle.FromDirection(a - origin), Angle.Asin(6f / (a - origin).Length()));
                }
                hints.AddForbiddenZone(new SDUnion(forbidden), act);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Foxshot)
        {
            _kb = [new(caster.Position.Quantized(), 25f, Module.CastFinishAt(spell))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Foxshot)
        {
            _kb = [];
        }
    }
}

class Whirlwind(BossModule module) : Components.Voidzone(module, 6f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.Whirlwind);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (!z.IsDead)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}
class WarDance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WarDance, 5f);
class CharmingChasse(BossModule module) : Components.CastGaze(module, (uint)AID.CharmingChasse);
class HannishFire(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HannishFire1, 6f);
class HannishWaters(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HannishWaters, new AOEShapeCone(40f, 15f.Degrees()));
class RanaasFinish(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RanaasFinish, 15f);

class RanaaMihgoStates : StateMachineBuilder
{
    public RanaaMihgoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WarDance>()
            .ActivateOnEnter<CharmingChasse>()
            .ActivateOnEnter<HannishFire>()
            .ActivateOnEnter<HannishWaters>()
            .ActivateOnEnter<RanaasFinish>()
            .ActivateOnEnter<Whirlwind>()
            .ActivateOnEnter<Foxshot>()
            .ActivateOnEnter<FoxshotKB>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68786, NameID = 8489)]
public class RanaaMihgo(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public static readonly ArenaBoundsCustom arena = new([new Ellipse(new(520.47f, 124.99f), 17.5f, 16, 50)]);
}
