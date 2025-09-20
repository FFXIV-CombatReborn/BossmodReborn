namespace BossMod.Global.MaskedCarnivale.Stage06.Act2;

public enum OID : uint
{
    Boss = 0x26FF, //R=2.53
    Eye = 0x25CE, //R=1.35
    Mandragora = 0x2701 //R=0.3
}

public enum AID : uint
{
    TearyTwirl = 14693, // Mandragora->self, 3.0s cast, range 6+R circle
    DemonEye = 14691, // Boss->self, 5.0s cast, range 50+R circle
    Attack = 6499, // /Boss/Mandragora->player, no cast, single-target
    ColdStare = 14692, // Boss->self, 2.5s cast, range 40+R 90-degree cone
    Stone = 14695, // Eye->player, 1.0s cast, single-target
    DreadGaze = 14694 // Eye->self, 3.0s cast, range 6+R ?-degree cone
}

public enum SID : uint
{
    Blind = 571 // Mandragora->player, extra=0x0
}

sealed class DemonEye(BossModule module) : Components.CastGaze(module, (uint)AID.DemonEye)
{
    private BitMask _blinded;

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        return _blinded[slot] ? [] : base.ActiveEyes(slot, actor);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Blind)
        {
            _blinded[Raid.FindSlot(actor.InstanceID)] = true;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Blind)
        {
            _blinded[Raid.FindSlot(actor.InstanceID)] = false;
        }
    }
}

sealed class ColdStare(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ColdStare, new AOEShapeCone(42.53f, 45f.Degrees())) // TODO: cone based gaze
{
    private BitMask _blinded;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _blinded[slot] ? [] : base.ActiveAOEs(slot, actor);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Blind)
        {
            _blinded[Raid.FindSlot(actor.InstanceID)] = true;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Blind)
        {
            _blinded[Raid.FindSlot(actor.InstanceID)] = false;
        }
    }
}

sealed class TearyTwirl(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.TearyTwirl, 6.3f)
{
    private BitMask _blinded;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Blind)
        {
            _blinded[Raid.FindSlot(actor.InstanceID)] = true;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Blind)
        {
            _blinded[Raid.FindSlot(actor.InstanceID)] = false;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_blinded[slot])
        {
            hints.Add("Kill mandragoras last incase you need to get blinded again.", false);
        }
        else
        {
            hints.Add("Stack to get blinded!", false);
        }
    }
}

sealed class DreadGaze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DreadGaze, new AOEShapeCone(7.35f, 45f.Degrees())) // TODO: cone based gaze
{
    private BitMask _blinded;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _blinded[slot] ? [] : base.ActiveAOEs(slot, actor);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Blind)
        {
            _blinded[Raid.FindSlot(actor.InstanceID)] = true;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Blind)
        {
            _blinded[Raid.FindSlot(actor.InstanceID)] = false;
        }
    }
}

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("The eyes are weak to lightning spells.");
    }
}

sealed class Stage06Act2States : StateMachineBuilder
{
    public Stage06Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ColdStare>()
            .ActivateOnEnter<DreadGaze>()
            .Raw.Update = () => AllDeadOrDestroyed(Stage06Act2.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 616, NameID = 8092, SortOrder = 2)]
public sealed class Stage06Act2 : BossModule
{
    public Stage06Act2(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
    {
        ActivateComponent<DemonEye>();
        ActivateComponent<TearyTwirl>();
        ActivateComponent<Hints>();
    }
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.Mandragora, (uint)OID.Eye];

    protected override bool CheckPull() => IsAnyActorInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Eye));
        Arena.Actors(Enemies((uint)OID.Mandragora), Colors.Object);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Boss or (uint)OID.Eye => 1,
                _ => 0
            };
        }
    }
}
