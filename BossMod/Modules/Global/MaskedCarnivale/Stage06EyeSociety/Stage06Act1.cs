namespace BossMod.Global.MaskedCarnivale.Stage06.Act1;

public enum OID : uint
{
    Boss = 0x25CD, //R=2.53
    Mandragora = 0x2700 //R=0.3
}

public enum AID : uint
{
    TearyTwirl = 14693, // Mandragora->self, 3.0s cast, range 6+R circle
    DemonEye = 14691, // Boss->self, 5.0s cast, range 50+R circle
    Attack = 6499, // Mandragora/Boss->player, no cast, single-target
    ColdStare = 14692 // Boss->self, 2.5s cast, range 40+R 90-degree cone
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

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Get blinded by the Teary Twirl AOE from the mandragoras.\nBlindness makes you immune to all the gaze attacks.\nThe eyes in act 2 are weak to lightning damage.");
    }
}

sealed class Stage06Act1States : StateMachineBuilder
{
    public Stage06Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<ColdStare>()
            .Raw.Update = () => AllDeadOrDestroyed(Stage06Act1.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 616, NameID = 8090, SortOrder = 1)]
public sealed class Stage06Act1 : BossModule
{
    public Stage06Act1(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.LayoutBigQuad)
    {
        ActivateComponent<Hints>();
        ActivateComponent<TearyTwirl>();
        ActivateComponent<DemonEye>();
    }
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.Mandragora];

    protected override bool CheckPull() => IsAnyActorInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.Boss));
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
                (uint)OID.Boss => 1,
                _ => 0
            };
        }
    }
}
