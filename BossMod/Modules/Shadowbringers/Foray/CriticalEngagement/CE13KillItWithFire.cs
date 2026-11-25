namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE13KillItWithFire;

public enum OID : uint
{
    Boss = 0x2E2F, // R2.25
    RottenMandragora = 0x2E30, // R1.05
    Pheromones = 0x2E31, // R1.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    Teleport = 20513, // Boss->location, no cast, single-target, teleport

    HarvestFestival = 20511, // Boss->self, 4.0s cast, single-target, visual (summon mandragoras)
    PheromonesVisual1 = 20512, // RottenMandragora->self, no cast, single-target, visual (???)
    PheromonesVisual2 = 20514, // RottenMandragora->location, no cast, single-target, visual (???)
    RancidPheromones = 20515, // RottenMandragora->self, no cast, single-target, visual (???)
    Heartbreak = 20516, // Pheromones->self, no cast, range 4 circle when pheromone is touched
    DeadLeaves = 20517, // Boss->self, 4.0s cast, range 30 circle, visual (recolors)
    TenderAnaphylaxis = 20518, // Helper->self, 4.0s cast, range 30 90-degree cone
    JealousAnaphylaxis = 20519, // Helper->self, 4.0s cast, range 30 90-degree cone
    AnaphylacticShock = 20520, // Helper->self, 4.0s cast, range 30 width 2 rect aoe (borders)
    SplashBomb = 20521, // Boss->self, 4.0s cast, single-target, visual (puddles)
    SplashBombAOE = 20522, // Helper->self, 4.0s cast, range 6 circle puddle
    SplashGrenade = 20523, // Boss->self, 5.0s cast, single-target, visual (stack)
    SplashGrenadeAOE = 20524, // Helper->players, 5.0s cast, range 6 circle stack
    PlayfulBreeze = 20525, // Boss->self, 4.0s cast, single-target, visual (raidwide)
    PlayfulBreezeAOE = 20526, // Helper->self, 4.0s cast, range 60 circle raidwide
    Budbutt = 20527 // Boss->player, 4.0s cast, single-target, tankbuster
}

public enum SID : uint
{
    TenderAnaphylaxis = 2301, // Helper->player, extra=0x0
    JealousAnaphylaxis = 2302 // Helper->player, extra=0x0
}

sealed class Pheromones(BossModule module) : Components.Voidzone(module, 4f, GetVoidzones, 3f)
{
    private static List<Actor> GetVoidzones(BossModule module) => module.Enemies((uint)OID.Pheromones);
}

sealed class DeadLeaves(BossModule module) : Components.GenericAOEs(module, default, "Go to different color!")
{
    private BitMask _tenderStatuses;
    private BitMask _jealousStatuses;
    private readonly List<AOEInstance> _tenderAOEs = [with(2)];
    private readonly List<AOEInstance> _jealousAOEs = [with(2)];

    private static readonly AOEShapeCone _shape = new(30f, 45f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return CollectionsMarshal.AsSpan(_tenderStatuses[slot] ? _tenderAOEs : _jealousStatuses[slot] ? _jealousAOEs : []);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.TenderAnaphylaxis:
                _tenderStatuses[Raid.FindSlot(actor.InstanceID)] = true;
                break;
            case (uint)SID.JealousAnaphylaxis:
                _jealousStatuses[Raid.FindSlot(actor.InstanceID)] = true;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.TenderAnaphylaxis:
                _tenderStatuses[Raid.FindSlot(actor.InstanceID)] = false;
                break;
            case (uint)SID.JealousAnaphylaxis:
                _jealousStatuses[Raid.FindSlot(actor.InstanceID)] = false;
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        List<AOEInstance>? CastersForAction(ActionID action) => action.ID switch
        {
            (uint)AID.TenderAnaphylaxis => _tenderAOEs,
            (uint)AID.JealousAnaphylaxis => _jealousAOEs,
            _ => null
        };
        CastersForAction(spell.Action)?.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.TenderAnaphylaxis or (uint)AID.JealousAnaphylaxis)
        {
            _tenderAOEs.Clear();
            _jealousAOEs.Clear();
        }
    }
}

sealed class AnaphylacticShock(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AnaphylacticShock, new AOEShapeRect(30f, 1f));
sealed class SplashBomb(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SplashBombAOE, 6f);
sealed class SplashGrenade(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.SplashGrenadeAOE, 6f, 8);
sealed class PlayfulBreeze(BossModule module) : Components.RaidwideCast(module, (uint)AID.PlayfulBreeze);
sealed class Budbutt(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Budbutt);

sealed class CE13KillItWithFireStates : StateMachineBuilder
{
    public CE13KillItWithFireStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Pheromones>()
            .ActivateOnEnter<DeadLeaves>()
            .ActivateOnEnter<AnaphylacticShock>()
            .ActivateOnEnter<SplashBomb>()
            .ActivateOnEnter<SplashGrenade>()
            .ActivateOnEnter<PlayfulBreeze>()
            .ActivateOnEnter<Budbutt>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 735, NameID = 1)] // bnpcname=9391
public sealed class CE13KillItWithFire(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(-90f, 700f), 29.5f, 32)]);

    protected override bool CheckPull() => base.CheckPull() && Raid.Player() is Actor player && player.Position.InCircle(Arena.Center, 30f);
}
