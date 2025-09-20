namespace BossMod.Dawntrail.Foray.FATE.FatalAllure;

public enum OID : uint
{
    Boss = 0x47DF, //R3.0
    BallOfFire = 0x483D, // R1.0
    VodorigaServant = 0x47E0, // R1.2
}

public enum AID : uint
{
    AutoAttack = 43273, // Boss->player, no cast, single-target
    AutoAttackAdd = 43371, // VodorigaServant->player, no cast, single-target
    Teleport = 43165, // Boss->location, no cast, single-target

    Max = 43270, // Boss->self, 5.0s cast, range 100 circle, increases size of fireball AOEs
    ArmOfPurgatory = 43269, // BallOfFire->location, 8.0s cast, range 20 circle
    Mini = 43046, // Boss->self, 4.0s cast, range 30 120-degree cone
    GreatBallOfFire = 43271, // Boss->self, 4.0s cast, single-target

    BloodyClaw = 43051, // VodorigaServant->player, 4.0s cast, single-target, mini tankbuster
    DarkMist = 43048, // Boss->self, 5.0s cast, range 30 circle, seduces players with 2.5y per second for 6s = 15y
    VoidFireIII = 43049 // Boss->self, 4.0s cast, range 10 circle
}

public enum SID : uint
{
    Seduced = 991 // Boss->player, extra=0x19 (run speed is status extra * 0.1)
}

class DarkMistVoidFireIII(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    bool done;
    private static readonly AOEShapeCircle circle = new(25f); // circle + minimum distance to survive seducing status

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DarkMist)
        {
            _aoe = [new(circle, spell.LocXZ, default, Module.CastFinishAt(spell))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.VoidFireIII)
        {
            done = true;
        }
    }

    public override void Update()
    {
        // this AOE should wait for Effect Results, since they can be delayed by over 0.7s, which would cause unknowning players and AI to run back into the death zone
        if (done)
        {
            var player = Module.Raid.Player()!;
            var statuses = CollectionsMarshal.AsSpan(player.PendingStatuses);
            var len = statuses.Length;
            for (var i = 0; i < len; ++i)
            {
                ref var s = ref statuses[i];
                if (s.StatusId == (uint)SID.Seduced)
                {
                    return;
                }
            }
            _aoe = [];
            done = false;
        }
    }
}

sealed class ArmOfPurgatory(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArmOfPurgatory, 20f);
sealed class Mini(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mini, new AOEShapeCone(30f, 60f.Degrees()));
sealed class VoidFireIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidFireIII, 10f);

sealed class FatalAllureStates : StateMachineBuilder
{
    public FatalAllureStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Mini>()
            .ActivateOnEnter<ArmOfPurgatory>()
            .ActivateOnEnter<VoidFireIII>()
            .ActivateOnEnter<DarkMistVoidFireIII>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.ForayFATE, GroupID = 1018, NameID = 1971)]
public sealed class FatalAllure(WorldState ws, Actor primary) : SimpleBossModule(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.VodorigaServant));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.VodorigaServant => 2,
                (uint)OID.Boss => 1,
                _ when e.Actor.InCombat => 0,
                _ => AIHints.Enemy.PriorityUndesirable
            };
        }
    }
}
