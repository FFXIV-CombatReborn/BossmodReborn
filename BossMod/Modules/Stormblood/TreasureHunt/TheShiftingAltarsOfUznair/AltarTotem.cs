namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarTotem;

public enum OID : uint
{
    Boss = 0x2534, //R=5.06
    TotemsHead = 0x2566, //R=2.2
    FireVoidzone = 0x1EA8BB,
    AltarMatanga = 0x2545, // R3.42
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // AltarMatanga->player, no cast, single-target
    AutoAttack3 = 6499, // TotemsHead->player, no cast, single-target

    FlurryOfRage = 13451, // Boss->self, 3.0s cast, range 8+R 120-degree cone
    WhorlOfFrenzy = 13453, // Boss->self, 3.0s cast, range 6+R circle
    WaveOfMalice = 13454, // Boss->location, 2.5s cast, range 5 circle
    TheWardensVerdict = 13739, // TotemsHead->self, 3.0s cast, range 40+R width 4 rect
    FlamesOfFury = 13452, // Boss->location, 4.0s cast, range 10 circle

    MatangaActivate = 9636, // AltarMatanga->self, no cast, single-target
    Spin = 8599, // AltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // AltarMatanga->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // AltarMatanga->location, 3.0s cast, range 6 circle
    Telega = 9630 // AltarMatanga->self, no cast, single-target, bonus adds disappear
}

public enum IconID : uint
{
    Baitaway = 23 // player
}

class FlurryOfRage(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlurryOfRage, new AOEShapeCone(13.06f, 60f.Degrees()));
class WaveOfMalice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WaveOfMalice, 5f);
class WhorlOfFrenzy(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WhorlOfFrenzy, 11.06f);
class TheWardensVerdict(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheWardensVerdict, new AOEShapeRect(45.06f, 2f));
class FlamesOfFury(BossModule module) : Components.VoidzoneAtCastTarget(module, 10f, (uint)AID.FlamesOfFury, GetVoidzones, 1.2f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.FireVoidzone);
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

class FlamesOfFuryBait(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private static readonly AOEShapeCircle circle = new(10f);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Baitaway)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, circle));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FlamesOfFury)
            ++NumCasts;
        if (NumCasts == 3)
        {
            CurrentBaits.Clear();
            NumCasts = 0;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (CurrentBaits.Count != 0 && CurrentBaits[0].Target == actor)
            hints.AddForbiddenZone(new SDCircle(Arena.Center, 17.5f));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Count == 0)
            return;
        if (CurrentBaits[0].Target != actor)
            base.AddHints(slot, actor, hints);
        else
            hints.Add("Bait away! (3 times)");
    }
}

class RaucousScritch(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RaucousScritch, new AOEShapeCone(8.42f, 60f.Degrees()));
class Hurl(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hurl, 6f);
class Spin(BossModule module) : Components.Cleave(module, (uint)AID.Spin, new AOEShapeCone(9.42f, 60f.Degrees()), [(uint)OID.AltarMatanga]);

class AltarTotemStates : StateMachineBuilder
{
    public AltarTotemStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FlurryOfRage>()
            .ActivateOnEnter<WaveOfMalice>()
            .ActivateOnEnter<WhorlOfFrenzy>()
            .ActivateOnEnter<TheWardensVerdict>()
            .ActivateOnEnter<FlamesOfFury>()
            .ActivateOnEnter<FlamesOfFuryBait>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () => AllDeadOrDestroyed(AltarTotem.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7586)]
public class AltarTotem(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.TotemsHead, (uint)OID.AltarMatanga];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.TotemsHead));
        Arena.Actors(Enemies((uint)OID.AltarMatanga), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.AltarMatanga => 2,
                (uint)OID.TotemsHead => 1,
                _ => 0
            };
        }
    }
}
