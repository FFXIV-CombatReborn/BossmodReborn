namespace BossMod.Heavensward.Dungeon.D02SohmAl.D023Tioman;

public enum OID : uint
{
    Boss = 0xE96, // R6.84
    Comet = 0x13AD, // R1.44
    RightWingOfInjury = 0x10B5, // R6.84
    LeftWingOfTragedy = 0x10B4, // R6.84
    Helper = 0x1B2
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    AbyssicBuster = 3811, // Boss->self, no cast, range 25+R 90-degree cone
    ChaosBlastCircle = 3813, // Boss->location, 2.0s cast, range 2 circle
    ChaosBlastRect = 3819, // Helper->self, 2.0s cast, range 50+R width 4 rect
    CometVisual = 3814, // Boss->self, 4.0s cast, single-target
    Comet = 3816, // Helper->location, 3.0s cast, range 4 circle
    MeteorImpactVisual = 4999, // Helper->self, 3.5s cast, range 30+R circle
    MeteorImpact = 4997, // Comet->self, no cast, range 30 circle, damage fall off AOE
    HeavensfallVisual = 3815, // Boss->self, no cast, single-target
    Heavensfall1 = 3817, // Helper->player, no cast, range 5 circle
    Heavensfall2 = 3818, // Helper->location, 3.0s cast, range 5 circle
    DarkStar = 3812 // Boss->self, 5.0s cast, range 50+R circle
}

public enum IconID : uint
{
    Comet = 10, // player
    Meteor = 7 // player
}

class HeavensfallBait(BossModule module) : Components.BaitAwayIcon(module, 5f, (uint)IconID.Comet, (uint)AID.Heavensfall1, 3.1f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (ActiveBaitsOn(actor).Count != 0)
            hints.AddForbiddenZone(new SDCircle(D023Tioman.ArenaCenter, 27f));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (ActiveBaitsOn(actor).Count != 0)
            hints.Add("Bait away!");
    }
}

class Meteor(BossModule module) : Components.BaitAwayIcon(module, 20f, (uint)IconID.Meteor, (uint)AID.MeteorImpactVisual, 8.1f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (ActiveBaitsOn(actor).Count != 0)
            hints.AddForbiddenZone(new SDCircle(D023Tioman.ArenaCenter, 27f));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (ActiveBaitsOn(actor).Count != 0)
            hints.Add("Bait away!");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            CurrentBaits.Clear();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) { }
}

class MeteorImpact(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circle = new(20f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MeteorImpactVisual)
            _aoes.Add(new(circle, spell.LocXZ, default, Module.CastFinishAt(spell, 1.5f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.MeteorImpact)
            _aoes.Clear();
    }
}

class DarkStar(BossModule module) : Components.RaidwideCast(module, (uint)AID.DarkStar);
class ChaosBlastCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ChaosBlastCircle, 2f);
class ChaosBlastRect(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ChaosBlastRect, new AOEShapeRect(50.5f, 2f));
class AbyssicBuster(BossModule module) : Components.Cleave(module, (uint)AID.AbyssicBuster, new AOEShapeCone(31.84f, 45f.Degrees()));
class Comet(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Comet, 4f);
class Heavensfall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Heavensfall2, 5f);

class D023TiomanStates : StateMachineBuilder
{
    public D023TiomanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Meteor>()
            .ActivateOnEnter<MeteorImpact>()
            .ActivateOnEnter<DarkStar>()
            .ActivateOnEnter<AbyssicBuster>()
            .ActivateOnEnter<ChaosBlastCircle>()
            .ActivateOnEnter<ChaosBlastRect>()
            .ActivateOnEnter<Comet>()
            .ActivateOnEnter<Heavensfall>()
            .ActivateOnEnter<HeavensfallBait>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 37, NameID = 3798)]
public class D023Tioman(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public static readonly WPos ArenaCenter = new(-104f, -395f);
    private static readonly ArenaBoundsCustom arena = new([new Circle(ArenaCenter, 27.5f)], [new Rectangle(new(-112.465f, -368.177f), 20, 1.25f, -19.24f.Degrees())]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.LeftWingOfTragedy));
        Arena.Actors(Enemies((uint)OID.RightWingOfInjury));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.RightWingOfInjury or (uint)OID.LeftWingOfTragedy => 1,
                _ => 0
            };
        }
    }
}
