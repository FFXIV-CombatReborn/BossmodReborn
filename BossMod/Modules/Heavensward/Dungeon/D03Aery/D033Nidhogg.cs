namespace BossMod.Heavensward.Dungeon.D03Aery.D033Nidhogg;

public enum OID : uint
{
    Boss = 0x39CA, // R12.0
    TheSablePrice = 0x39CB, // R1.0
    Liegedrake = 0x39CD, // R3.6
    Ahleh = 0x39CC, // R5.0
    Helper = 0x233C
}

public enum AID : uint
{
    Attack = 870, // Boss/Liegedrake/Ahleh->player, no cast, single-target

    DeafeningBellow = 30206, // Boss->self, 5.0s cast, range 80 circle
    HotTailVisual = 30196, // Boss->self, 3.0s cast, single-target
    HotTail = 30197, // Helper->self, 3.5s cast, range 68 width 16 rect
    HotWingVisual = 30194, // Boss->self, 3.0s cast, single-target
    HotWing = 30195, // Helper->self, 3.5s cast, range 30 width 68 rect
    HorridRoarVisual = 30201, // Boss->self, no cast, single-target
    HorridRoarSpread = 30202, // Helper->player, 5.0s cast, range 6 circle
    HorridRoar = 30200, // Helper->location, 4.0s cast, range 6 circle
    Cauterize = 30198, // Boss->self, 5.0s cast, range 80 width 22 rect
    Touchdown = 30199, // Boss->self, no cast, range 80 circle
    TheSablePrice = 30203, // Boss->self, 3.0s cast, single-target
    TheScarletPrice = 30205, // Boss->player, 5.0s cast, single-target
    Massacre = 30207, // Boss->location, 6.0s cast, range 80 circle
    HorridBlaze = 30224, // Boss->players, 7.0s cast, range 6 circle
    SableWeave = 30204, // TheSablePrice->player, 15.0s cast, single-target
    Roast = 30209 // Ahleh->self, 4.0s cast, range 30 width 8 rect
}

public enum SID : uint
{
    Fetters = 3324, // none->player, extra=0x0
    DraconianGaze = 703 // none->Boss, extra=0x0
}

class Fetters(BossModule module) : BossComponent(module)
{
    private bool chained;
    private bool chainsactive;
    private Actor? chaintarget;
    private bool casting;

    public override void Update()
    {
        var fetters = chaintarget?.FindStatus((uint)SID.Fetters) != null;
        if (fetters)
            chainsactive = true;
        if (fetters && !chained)
            chained = true;
        if (chaintarget != null && !fetters && !casting)
        {
            chained = false;
            chaintarget = null;
            chainsactive = false;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (chaintarget != null && chainsactive)
            hints.Add($"Destroy fetters on {chaintarget.Name}!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (chained && actor != chaintarget)
        {
            var count = hints.PotentialTargets.Count;
            for (var i = 0; i < count; ++i)
            {
                var e = hints.PotentialTargets[i];
                e.Priority = e.Actor.OID switch
                {
                    (uint)OID.TheSablePrice => 1,
                    (uint)OID.Boss => -1,
                    _ => 0
                };
            }
        }
        var fetters = Module.Enemies((uint)OID.TheSablePrice);
        var fetter = fetters.Count != 0 ? fetters[0] : null;
        if (fetter != null && !fetter.IsDead)
            hints.AddForbiddenZone(new SDInvertedCircle(fetter.Position, fetter.HitboxRadius + 3));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SableWeave)
        {
            chaintarget = WorldState.Actors.Find(spell.TargetID)!;
            casting = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SableWeave)
            casting = false;
    }
}

class DeafeningBellow(BossModule module) : Components.RaidwideCast(module, (uint)AID.DeafeningBellow);

class HotTail(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HotTail, new AOEShapeRect(68f, 8f));
class HotWing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HotWing, new AOEShapeRect(30f, 34f));
class Cauterize(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Cauterize, new AOEShapeRect(80f, 11f));
class HorridRoarSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.HorridRoarSpread, 6f);
class HorridRoar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HorridRoar, 6f);
class HorridBlaze(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.HorridBlaze, 6f, 4, 4);
class Massacre(BossModule module) : Components.RaidwideCast(module, (uint)AID.Massacre);
class TheScarletPrice(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.TheScarletPrice);
class Roast(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Roast, new AOEShapeRect(30f, 4f));

class D033NidhoggStates : StateMachineBuilder
{
    public D033NidhoggStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HotTail>()
            .ActivateOnEnter<HotWing>()
            .ActivateOnEnter<Cauterize>()
            .ActivateOnEnter<HorridRoarSpread>()
            .ActivateOnEnter<HorridRoar>()
            .ActivateOnEnter<HorridBlaze>()
            .ActivateOnEnter<DeafeningBellow>()
            .ActivateOnEnter<Massacre>()
            .ActivateOnEnter<Fetters>()
            .ActivateOnEnter<TheScarletPrice>()
            .ActivateOnEnter<Roast>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 39, NameID = 3458)]
public class D033Nidhogg(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Circle(new(35, -267), 32.5f)], [new Rectangle(new(35, -299.5f), 20, 0.75f), new Rectangle(new(35, -233.5f), 20, 1.25f)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Liegedrake));
        Arena.Actors(Enemies((uint)OID.Ahleh));
        Arena.Actors(Enemies((uint)OID.TheSablePrice), Colors.Object);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.TheSablePrice => 2,
                (uint)OID.Ahleh or (uint)OID.Liegedrake => 1,
                _ => 0
            };
        }
    }
}
