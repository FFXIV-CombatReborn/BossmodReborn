namespace BossMod.Shadowbringers.Dungeon.D12MatoyasRelict.D123MotherPorxie;

public enum OID : uint
{
    Boss = 0x300B, // R3.6
    AeolianCaveSprite = 0x3052, // R1.6
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 19087, // Boss->player, no cast, single-target

    TenderLoinVisual = 22803, // Boss->self, 5.0s cast, single-target
    TenderLoin = 22804, // Helper->self, no cast, range 60 circle

    HuffAndPuff1 = 22809, // Boss->self, 8.0s cast, range 40 width 40 rect, knockback 15, source forward

    MediumRearVisual = 22813, // Boss->self, no cast, single-target
    MediumRear1 = 22814, // Helper->self, 10.5s cast, range 5-40 donut
    MediumRear2 = 22815, // Helper->self, 16.0s cast, range 5-40 donut, after limit break phase success

    MeatMallet = 22806, // Boss->location, 7.0s cast, range 45 circle, damage fall off AOE

    Barbeque = 23331, // Boss->self, 2.5s cast, single-target
    BarbequeRect = 22807, // Helper->self, 3.0s cast, range 50 width 5 rect, knockback 15, source forward
    BarbequeCircle = 22808, // Helper->location, 3.0s cast, range 5 circle
    ToACrispVisual = 22820, // Boss->self, no cast, single-target
    ToACrisp = 22821, // Helper->self, no cast, range 10 width 40 rect

    MincedMeatVisual = 22801, // Boss->player, 4.0s cast, single-target
    MincedMeat = 22802, // Helper->player, no cast, single-target

    Buffet = 22822, // AeolianCaveSprite->self, 3.0s cast, range 40 width 6 rect
    HuffAndPuffVisual = 22810, // Boss->self, 50.0s cast, range 40 width 40 rect, casts during limit break, only visual
    Explosion = 20020, // AeolianCaveSprite->self, 2.5s cast, range 80 circle, knocks player up to see a visual knockback hint
    HuffAndPuff2 = 22811, // Boss->self, no cast, range 40 width 40 rect, knockback 15, source forward

    BlowItAllDown = 22812, // Boss->self, no cast, range 40 width 40 rect, knockback 50, source forward (on limit break fail)
    NeerDoneWell = 20045, // Helper->self, 8.0s cast, range 5-40 donut (on limit break fail)

    OpenFlameVisual = 22818, // Boss->self, 6.0s cast, single-target
    OpenFlame = 22819 // Helper->player, no cast, range 5 circle, spread
}

public enum IconID : uint
{
    Tankbuster = 198, // player
    Spreadmarker = 169 // player
}

class TenderLoin(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.TenderLoinVisual, (uint)AID.TenderLoin, 0.8f);
class MincedMeat(BossModule module) : Components.SingleTargetCastDelay(module, (uint)AID.MincedMeatVisual, (uint)AID.MincedMeat, 0.9f);
class OpenFlame(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, (uint)AID.OpenFlame, 5f, 6.7f);
class MeatMallet(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MeatMallet, 30f);
class BarbequeCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BarbequeCircle, 5f);
class BarbequeRect(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BarbequeRect, new AOEShapeRect(50f, 2.5f));
class Buffet(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Buffet, new AOEShapeRect(40f, 3f));

sealed class MediumRearNeerDoneWell(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.MediumRear1, (uint)AID.MediumRear2, (uint)AID.NeerDoneWell], new AOEShapeDonut(5f, 40f))
{
    private readonly HuffAndPuff1 _kb1 = module.FindComponent<HuffAndPuff1>()!;
    private readonly HuffAndPuff2 _kb2 = module.FindComponent<HuffAndPuff2>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb1.Casters.Count == 0 && _kb2.KB.Length == 0)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}

class HuffAndPuff1(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.HuffAndPuff1, 15f, true, stopAtWall: true, kind: Kind.DirForward)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
            hints.AddForbiddenZone(new SDInvertedCircle(Casters.Ref(0).Origin, 5f));
    }
}

class HuffAndPuff2(BossModule module) : Components.GenericKnockback(module, stopAtWall: true)
{
    private Knockback[] _kbCache = [];
    public Knockback[] KB = [];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => KB;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HuffAndPuffVisual)
        {
            _kbCache = [new(spell.LocXZ, 15f, default, null, spell.Rotation, Kind.DirForward, ignoreImmunes: true)];
        }
        else if (_kbCache.Length != 0 && spell.Action.ID == (uint)AID.NeerDoneWell)
        {
            ref readonly var kb = ref _kbCache[0];
            KB = [new(kb.Origin, 50f, WorldState.FutureTime(5.4d), null, kb.Direction, kb.Kind, ignoreImmunes: true)];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_kbCache.Length != 0 && spell.Action.ID == (uint)AID.Explosion)
        {
            ref readonly var kb = ref _kbCache[0];
            KB = [new(kb.Origin, 15f, WorldState.FutureTime(10.9d), null, kb.Direction, kb.Kind, ignoreImmunes: true)];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.HuffAndPuff2 or (uint)AID.BlowItAllDown)
        {
            KB = [];
            _kbCache = [];
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (KB.Length != 0)
        {
            ref readonly var kb = ref KB[0];
            hints.AddForbiddenZone(new SDInvertedCircle(kb.Origin, 5f));
        }
    }
}

class Barbeque(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(10f, 20f);
    private AOEInstance[] _aoe = [];
    private bool imminent;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x10 && state == 0x00020001u)
        {
            _aoe = [new(rect, new(-19.5f, default), Angle.AnglesCardinals[3])]; // activates 22.2s later, but should never be entered anyway, since you must go to the opposite of the arena
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Barbeque)
        {
            imminent = true;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BarbequeRect)
        {
            imminent = false;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ToACrisp)
        {
            _aoe = [];
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoe.Length != 0)
        {
            hints.AddForbiddenZone(new SDInvertedRect(new(imminent ? 18.5f : actor.PosRot.X + 0.1f, default), new(19f, default), 20f));
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (imminent)
        {
            hints.Add("Go to the opposite side of the arena and work against getting sucked in!");
        }
    }
}

class D123MotherPorxieStates : StateMachineBuilder
{
    public D123MotherPorxieStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HuffAndPuff1>()
            .ActivateOnEnter<HuffAndPuff2>()
            .ActivateOnEnter<TenderLoin>()
            .ActivateOnEnter<MincedMeat>()
            .ActivateOnEnter<OpenFlame>()
            .ActivateOnEnter<MeatMallet>()
            .ActivateOnEnter<BarbequeCircle>()
            .ActivateOnEnter<BarbequeRect>()
            .ActivateOnEnter<Buffet>()
            .ActivateOnEnter<MediumRearNeerDoneWell>()
            .ActivateOnEnter<Barbeque>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 746u, NameID = 9741u)]
public class D123MotherPorxie(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsSquare(19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.AeolianCaveSprite));
    }
}
