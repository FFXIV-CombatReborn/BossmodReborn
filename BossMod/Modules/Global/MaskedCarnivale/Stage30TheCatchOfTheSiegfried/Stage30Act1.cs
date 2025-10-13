namespace BossMod.Global.MaskedCarnivale.Stage30.Act1;

public enum OID : uint
{
    Boss = 0x2C67, //R=2.0
    Bomb = 0x2C68, // R=0.4
    Helper = 0x233C
}

public enum AID : uint
{
    MagicDrain = 18890, // Boss->self, 3.0s cast, single-target
    AnkleGraze = 18846, // Boss->player, 3.0s cast, single-target
    HyperdriveFirst = 18836, // Boss->location, 5.0s cast, range 5 circle
    HyperdriveRest = 18837, // Helper->location, 2.5s cast, range 5 circle
    Hyperdrive = 18893, // Boss->self, no cast, single-target, between Hyperdrive casts
    Teleport = 18848, // Boss->location, no cast, ???
    MagitekExplosive = 18849, // Boss->self, 3.0s cast, single-target
    RubberBullet = 18847, // Boss->player, 4.0s cast, single-target, knockback 20 away from source
    Explosion = 18888 // Bomb->self, 3.5s cast, range 8 circle
}

public enum SID : uint
{
    MagitekField = 2166, // Boss->Boss, extra=0x64, reflects magic damage
    MagicVulnerabilityDown = 812, // Boss->Boss, extra=0x0, invulnerable to magic
    Bind = 564 // Boss->player, extra=0x0, dispellable
}

sealed class MagicDrain(BossModule module) : Components.CastHint(module, (uint)AID.MagicDrain, "Reflect magic damage for 30s");
sealed class Hyperdrive(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.HyperdriveFirst, (uint)AID.HyperdriveRest], 5f);
sealed class AnkleGraze(BossModule module) : Components.CastHint(module, (uint)AID.AnkleGraze, "Applies bind, prepare to use Excuviation!");

sealed class RubberBullet(BossModule module) : Components.GenericKnockback(module)
{
    private Knockback[] _kb = [];
    private readonly Explosion _aoe = module.FindComponent<Explosion>()!;

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
        return !Arena.InBounds(pos);
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _kb;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Bomb)
        {
            _kb = [new(Module.PrimaryActor.Position, 20f, WorldState.FutureTime(6.3d))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RubberBullet)
        {
            _kb = [];
        }
    }
}

sealed class Explosion(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCircle circle = new(8);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Bomb)
        {
            var pos = actor.Position.Quantized();
            _aoes.Add(new(circle, pos, default, WorldState.FutureTime(8.4d), shapeDistance: circle.Distance(pos, default)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Explosion)
        {
            _aoes.Clear();
        }
    }
}

sealed class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Module.PrimaryActor.FindStatus((uint)SID.MagitekField) != null)
        {
            hints.Add($"{Module.PrimaryActor.Name} will reflect all magic damage!");
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.FindStatus((uint)SID.Bind) != null)
        {
            hints.Add("You were bound! Cleanse it with Exuviation.");
        }
    }
}

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"{Module.PrimaryActor.Name} will have phases where all magic damage gets reflected.\nExuviation, a melee ability and fire, wind and ice spells are recommended.");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        hints.Add("Requirements for achievement: Take no damage, use all 6 magic elements,\nuse all 3 melee types, kill all 3 clones in act3 and finish faster than ideal time.", false);
    }
}

sealed class Stage30Act1States : StateMachineBuilder
{
    public Stage30Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hyperdrive>()
            .ActivateOnEnter<AnkleGraze>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<RubberBullet>()
            .ActivateOnEnter<MagicDrain>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 699, NameID = 9245, SortOrder = 1)]
public sealed class Stage30Act1 : BossModule
{
    public Stage30Act1(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall)
    {
        ActivateComponent<Hints>();
    }
}