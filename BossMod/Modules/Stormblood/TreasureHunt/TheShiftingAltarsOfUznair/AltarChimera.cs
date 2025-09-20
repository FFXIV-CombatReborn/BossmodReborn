namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarChimera;

public enum OID : uint
{
    Boss = 0x2539, //R=5.92
    AltarAhriman = 0x256A, //R=2.07
    IceVoidzone = 0x1E8D9C,
    AltarMatanga = 0x2545, // R3.42
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // AltarMatanga->player, no cast, single-target
    AutoAttack3 = 6499, // AltarAhriman->player, no cast, single-target

    TheScorpionsSting = 13393, // Boss->self, 3.5s cast, range 6+R 90-degree cone
    TheRamsVoice = 13394, // Boss->self, 5.0s cast, range 4+R circle, interruptible, deep freeze + frostbite
    TheLionsBreath = 13392, // Boss->self, 3.5s cast, range 6+R 120-degree cone, burn
    LanguorousGaze = 13742, // AltarAhriman->self, 3.0s cast, range 6+R 90-degree cone
    TheRamsKeeper = 13396, // Boss->location, 3.5s cast, range 6 circle, voidzone
    TheDragonsVoice = 13395, // Boss->self, 5.0s cast, range 8-30 donut, interruptible, paralaysis

    MatangaActivate = 9636, // AltarMatanga->self, no cast, single-target
    Spin = 8599, // AltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // AltarMatanga->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // AltarMatanga->location, 3.0s cast, range 6 circle
    Telega = 9630 // AltarMatanga->self, no cast, single-target, bonus add disappear
}

public enum IconID : uint
{
    Baitaway = 23 // player
}

class TheScorpionsSting(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheScorpionsSting, new AOEShapeCone(11.92f, 45f.Degrees()));
class TheRamsVoice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheRamsVoice, 9.92f);
class TheRamsVoiceHint(BossModule module) : Components.CastInterruptHint(module, (uint)AID.TheRamsVoice);
class TheLionsBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheLionsBreath, new AOEShapeCone(11.92f, 60f.Degrees()));
class LanguorousGaze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LanguorousGaze, new AOEShapeCone(8.07f, 45f.Degrees()));
class TheDragonsVoice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheDragonsVoice, new AOEShapeDonut(8f, 30f));
class TheDragonsVoiceHint(BossModule module) : Components.CastInterruptHint(module, (uint)AID.TheDragonsVoice);
class TheRamsKeeper(BossModule module) : Components.VoidzoneAtCastTarget(module, 6f, (uint)AID.TheRamsKeeper, GetVoidzones, 0.9f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.IceVoidzone);
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

class TheRamsKeeperBait(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private static readonly AOEShapeCircle circle = new(6f);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Baitaway)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, circle));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TheRamsKeeper)
            CurrentBaits.Clear();
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
            hints.Add("Bait away!");
    }
}

class RaucousScritch(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RaucousScritch, new AOEShapeCone(8.42f, 60f.Degrees()));
class Hurl(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hurl, 6f);
class Spin(BossModule module) : Components.Cleave(module, (uint)AID.Spin, new AOEShapeCone(9.42f, 60f.Degrees()), [(uint)OID.AltarMatanga]);

class AltarChimeraStates : StateMachineBuilder
{
    public AltarChimeraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheScorpionsSting>()
            .ActivateOnEnter<TheRamsVoice>()
            .ActivateOnEnter<TheRamsVoiceHint>()
            .ActivateOnEnter<TheDragonsVoice>()
            .ActivateOnEnter<TheDragonsVoiceHint>()
            .ActivateOnEnter<TheLionsBreath>()
            .ActivateOnEnter<LanguorousGaze>()
            .ActivateOnEnter<TheRamsKeeper>()
            .ActivateOnEnter<TheRamsKeeperBait>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () => AllDeadOrDestroyed(AltarChimera.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7591)]
public class AltarChimera(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.AltarAhriman, (uint)OID.AltarMatanga];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.AltarAhriman));
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
                (uint)OID.AltarAhriman => 1,
                _ => 0
            };
        }
    }
}
