namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.DD60TheBlackRider;

public enum OID : uint
{
    Boss = 0x1814, // R3.92
    Voidzone = 0x1E858E, // R0.5
    VoidsentDiscarnate = 0x18E6 // R1.0
}

public enum AID : uint
{
    AutoAttack = 7179, // Boss->player, no cast, range 8+R 90-degree cone

    Geirrothr = 7087, // Boss->self, no cast, range 6+R 90-degree cone, 5.1s after pull, 7.1s after Valfodr + 8.1s after every 2nd HallofSorrow
    HallOfSorrow = 7088, // Boss->location, no cast, range 9 circle
    Infatuation = 7157, // VoidsentDiscarnate->self, 6.5s cast, range 6+R circle
    Valfodr = 7089 // Boss->player, 4.0s cast, width 6 rect charge, knockback 25, dir forward
}

class CleaveAuto(BossModule module) : Components.Cleave(module, default, new AOEShapeCone(11.92f, 45f.Degrees()), activeWhileCasting: false);
class Infatuation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Infatuation, 7f);
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

class DD60TheBlackRiderStates : StateMachineBuilder
{
    public DD60TheBlackRiderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Infatuation>()
            .ActivateOnEnter<HallOfSorrow>()
            .ActivateOnEnter<Valfodr>()
            .ActivateOnEnter<ValfodrKB>()
            .ActivateOnEnter<CleaveAuto>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "legendoficeman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 204, NameID = 5309)]
public class DD60TheBlackRider(WorldState ws, Actor primary) : BossModule(ws, primary, SharedBounds.ArenaBounds607080.Center, SharedBounds.ArenaBounds607080);
