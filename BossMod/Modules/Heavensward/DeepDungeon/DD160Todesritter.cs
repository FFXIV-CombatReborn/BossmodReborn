namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.DD160Todesritter;

public enum OID : uint
{
    Boss = 0x181D, // R3.920, x1
    VoidsentDiscarnate = 0x18EF, // R1.000, x0 (spawn during fight)
    Voidzone = 0x1E858E // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 7179, // Boss->players, no cast, range 8+R 90?-degree cone

    Geirrothr = 7154, // Boss->self, no cast, range 6+R 90?-degree cone
    HallOfSorrow = 7155, // Boss->location, no cast, range 9 circle
    Infatuation = 7090, // VoidsentDiscarnate->self, 6.5s cast, range 6+R circle
    Valfodr = 7156, // Boss->player, 4.0s cast, width 6 rect charge + kb
}

class CleaveAuto(BossModule module) : Components.Cleave(module, (uint)AID.AutoAttack, new AOEShapeCone(11.92f, 45f.Degrees()), activeWhileCasting: false);
class HallOfSorrow(BossModule module) : Components.VoidzoneAtCastTarget(module, 9f, (uint)AID.HallOfSorrow, GetVoidzones, 1.3d)
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

class Infatuation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Infatuation, 7f);
class Valfodr(BossModule module) : Components.BaitAwayChargeCast(module, (uint)AID.Valfodr, 3f);

class ValfodrKB(BossModule module) : Components.GenericKnockback(module, (uint)AID.Valfodr, stopAtWall: true)
{
    private readonly Infatuation _aoe1 = module.FindComponent<Infatuation>()!;
    private readonly HallOfSorrow _aoe2 = module.FindComponent<HallOfSorrow>()!;

    private int _target;
    private Knockback[] _kb = [];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_target == slot)
        {
            return _kb;
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _kb = [new(caster.Position, 25f, Module.CastFinishAt(spell))];
            _target = Raid.FindSlot(spell.TargetID);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _target = -1;
            _kb = [];
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes1 = CollectionsMarshal.AsSpan(_aoe1.Casters);
        var count = aoes1.Length;
        for (var i = 0; i < count; ++i)
        {
            if (aoes1[i].Check(pos))
            {
                return true;
            }
        }
        var aoes2 = _aoe2.ActiveAOEs(slot, actor);
        var len = aoes2.Length;
        for (var i = 0; i < len; ++i)
        {
            if (aoes2[i].Check(pos))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_target == slot && _kb.Length != 0)
        {
            var voidzones = Module.Enemies((uint)OID.Voidzone);
            var countV = voidzones.Count;
            var countI = _aoe1.Casters.Count;
            var total = countV + countI;
            if (total == 0)
                return;
            var forbidden = new ShapeDistance[total];
            var pos = Module.PrimaryActor.Position;
            for (var i = 0; i < countV; ++i)
            {
                var a = voidzones[i];
                forbidden[i] = new SDCone(pos, 100f, Module.PrimaryActor.AngleTo(a), Angle.Asin(9f / (a.Position - pos).Length()));
            }
            for (var i = 0; i < countI; ++i)
            {
                var a = _aoe1.Casters[i];
                forbidden[i + countV] = new SDCone(pos, 100f, Module.PrimaryActor.AngleTo(a.Origin), Angle.Asin(7f / (a.Origin - pos).Length()));
            }
            ref readonly var kb = ref _kb[0];
            hints.AddForbiddenZone(new SDUnion(forbidden), kb.Activation);
        }
    }
}

class DD160TodesritterStates : StateMachineBuilder
{
    public DD160TodesritterStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CleaveAuto>()
            .ActivateOnEnter<HallOfSorrow>()
            .ActivateOnEnter<Infatuation>()
            .ActivateOnEnter<Valfodr>()
            .ActivateOnEnter<ValfodrKB>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 214, NameID = 5438)]
public class DD160Todesritter(WorldState ws, Actor primary) : BossModule(ws, primary, SharedBounds.ArenaBounds160170180190.Center, SharedBounds.ArenaBounds160170180190);
