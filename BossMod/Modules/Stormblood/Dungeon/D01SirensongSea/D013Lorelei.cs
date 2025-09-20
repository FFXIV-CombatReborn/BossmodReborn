namespace BossMod.Stormblood.Dungeon.D01SirensongSea.D013Lorelei;

public enum OID : uint
{
    Boss = 0x1AFE, // R3.36
    ArenaVoidzone = 0x1EA2FF, // R2.0
    Voidzone = 0x1EA300 // R0.5
}

public enum AID : uint
{
    IllWill = 8035, // Boss->player, no cast, single-target
    VirginTears = 8041, // Boss->self, 3.0s cast, single-target
    MorbidAdvance = 8037, // Boss->self, 5.0s cast, range 80+R circle
    HeadButt = 8036, // Boss->player, no cast, single-target
    SomberMelody = 8039, // Boss->self, 4.0s cast, range 80+R circle
    MorbidRetreat = 8038, // Boss->self, 5.0s cast, range 80+R circle
    VoidWaterIII = 8040 // Boss->location, 3.5s cast, range 8 circle
}

class VirginTearsArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly ArenaBoundsCircle smallerBounds = new(15.75f);
    private static readonly AOEShapeDonut donut = new(15.75f, 22);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnActorEState(Actor actor, ushort state)
    {
        if (state == 0x001)
        {
            Arena.Bounds = D013Lorelei.DefaultArena;
            Arena.Center = D013Lorelei.DefaultArena.Center;
        }
        else if (state == 0x002)
        {
            _aoe = [];
            Arena.Bounds = smallerBounds;
            Arena.Center = D013Lorelei.ArenaCenter;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.VirginTears && Arena.Bounds == D013Lorelei.DefaultArena)
        {
            if (++NumCasts > 3)
            {
                _aoe = [new(donut, D013Lorelei.ArenaCenter, default, Module.CastFinishAt(spell, 0.7d))];
            }
        }
    }
}

class MorbidAdvance(BossModule module) : Components.ActionDrivenForcedMarch(module, (uint)AID.MorbidAdvance, 3f, default, 1f)
{
    private readonly Voidzone _aoe = module.FindComponent<Voidzone>()!;

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

class MorbidRetreat(BossModule module) : Components.ActionDrivenForcedMarch(module, (uint)AID.MorbidRetreat, 3f, 180f.Degrees(), 1f)
{
    private readonly Voidzone _aoe = module.FindComponent<Voidzone>()!;

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

class SomberMelody(BossModule module) : Components.RaidwideCast(module, (uint)AID.SomberMelody);
class VoidWaterIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidWaterIII, 8f);
class Voidzone(BossModule module) : Components.Voidzone(module, 7f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.Voidzone);
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

class D013LoreleiStates : StateMachineBuilder
{
    public D013LoreleiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VirginTearsArenaChange>()
            .ActivateOnEnter<Voidzone>()
            .ActivateOnEnter<MorbidAdvance>()
            .ActivateOnEnter<MorbidRetreat>()
            .ActivateOnEnter<SomberMelody>()
            .ActivateOnEnter<VoidWaterIII>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus), erdelf", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 238, NameID = 6074)]
public class D013Lorelei(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultArena.Center, DefaultArena)
{
    public static readonly WPos ArenaCenter = new(-44.5f, 465);
    public static readonly ArenaBoundsCustom DefaultArena = new([new Circle(ArenaCenter, 21.6f)], [new Rectangle(new(-44.5f, 443), 20, 1)]);
}
