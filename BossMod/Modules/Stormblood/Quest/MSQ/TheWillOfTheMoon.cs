using RPID = BossMod.Roleplay.AID;

namespace BossMod.Stormblood.Quest.MSQ.TheWillOfTheMoon;

public enum OID : uint
{
    Boss = 0x24A0,
    Magnai = 0x24A1,
    KhunShavar = 0x252F, // R1.82
    Hien = 0x24A3,
    Daidukul = 0x24A2, // R0.5
    TheScaleOfTheFather = 0x2532, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    DispellingWind = 13223, // Boss->self, 3.0s cast, range 40+R width 8 rect
    Epigraph = 13225, // 252D->self, 3.0s cast, range 45+R width 8 rect
    WhisperOfLivesPast = 13226, // 252E->self, 3.5s cast, range -12 donut
    AncientBlizzard = 13227, // 252F->self, 3.0s cast, range 40+R 45-degree cone
    Tornado = 13228, // 252F->location, 5.0s cast, range 6 circle
    Epigraph2 = 13222, // 2530->self, 3.0s cast, range 45+R width 8 rect
    FlatlandFury = 13244, // 2532->self, 17.0s cast, range 10 circle
    FlatlandFuryEnrage = 13329, // 249F->self, 25.0s cast, range 10 circle
    ViolentEarth = 13236, // 233C->location, 3.0s cast, range 6 circle
    WindChisel = 13518, // 233C->self, 2.0s cast, range 34+R 20-degree cone
    TranquilAnnihilation = 13233, // _Gen_DaidukulTheMirthful->24A3, 15.0s cast, single-target
}

public enum SID : uint
{
    Invincibility = 775 // none->Boss, extra=0x0
}

class DispellingWind(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DispellingWind, new AOEShapeRect(40f, 4f));
class Epigraph(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Epigraph, new AOEShapeRect(45f, 4f));
class Whisper(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WhisperOfLivesPast, new AOEShapeDonut(6f, 12f));
class Blizzard(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AncientBlizzard, new AOEShapeCone(40f, 22.5f.Degrees()));
class Tornado(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Tornado, 6f);
class Epigraph1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Epigraph2, new AOEShapeRect(45f, 4f));

public class FlatlandFury(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlatlandFury, 10f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // if all 9 adds are alive, instead of drawing forbidden zones (which would fill the whole arena), force AI to target nearest one to kill it
        if (ActiveCasters.Length == 9)
            hints.ForcedTarget = Module.Enemies((uint)OID.TheScaleOfTheFather).MinBy(actor.DistanceToHitbox);
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

public class FlatlandFuryEnrage(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlatlandFuryEnrage, 10f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ActiveCasters.Length < 9)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

public class ViolentEarth(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ViolentEarth, 6f);
public class WindChisel(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WindChisel, new AOEShapeCone(34f, 10f.Degrees()));

public class Scales(BossModule module) : Components.Adds(module, (uint)OID.TheScaleOfTheFather);

class AutoYshtola(BossModule module, WorldState ws) : QuestBattle.UnmanagedRotation(ws, 25f)
{
    private Actor Magnai => module.Enemies((uint)OID.Magnai)[0];
    private Actor Hien => module.Enemies((uint)OID.Hien)[0];
    private Actor Daidukul => module.Enemies((uint)OID.Daidukul)[0];

    protected override void Exec(Actor? primaryTarget)
    {
        var hienMinHP = Daidukul.CastInfo?.Action.ID == (uint)AID.TranquilAnnihilation
            ? 28000
            : 10000;

        if (Hien.PendingHPRaw < hienMinHP)
        {
            if (Player.DistanceToHitbox(Hien) > 25f)
                Hints.ForcedMovement = Player.DirectionTo(Hien).ToVec3();

            UseAction(RPID.CureIISeventhDawn, Hien);
        }

        if (Hien.CastInfo?.Action.ID == 13234)
            Hints.GoalZones.Add(AIHints.GoalSingleTarget(Hien.Position, 2f, 5f));

        var aero = StatusDetails(Magnai, (uint)WHM.SID.AeroII, Player.InstanceID);
        if (aero.Left < 4.6f)
            UseAction(RPID.AeroIISeventhDawn, Magnai);

        UseAction(RPID.StoneIVSeventhDawn, primaryTarget);

        if (Player.HPMP.CurMP < 5000)
            UseAction(RPID.Aetherwell, Player);
    }
}

class YshtolaAI(BossModule module) : QuestBattle.RotationModule<AutoYshtola>(module);

class P1Hints(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            if (e.Actor.FindStatus((uint)SID.Invincibility) != null)
                e.Priority = AIHints.Enemy.PriorityInvincible;

            // they do very little damage and sadu will raise them after a short delay, no point in attacking
            if (e.Actor.OID == (uint)OID.KhunShavar)
                e.Priority = AIHints.Enemy.PriorityPointless;
        }
    }
}

class P2Hints(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID == (uint)OID.Magnai ? 1 : 0;
        }
    }
}

class SaduHeavensflameStates : StateMachineBuilder
{
    public SaduHeavensflameStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<P1Hints>()
            .ActivateOnEnter<DispellingWind>()
            .ActivateOnEnter<Epigraph>()
            .ActivateOnEnter<Whisper>()
            .ActivateOnEnter<Blizzard>()
            .ActivateOnEnter<Tornado>()
            .ActivateOnEnter<Epigraph1>()
            .Raw.Update = () => module.Enemies((uint)OID.Magnai).Count != 0;
        TrivialPhase(1)
            .ActivateOnEnter<P2Hints>()
            .ActivateOnEnter<YshtolaAI>()
            .ActivateOnEnter<Scales>()
            .ActivateOnEnter<FlatlandFury>()
            .ActivateOnEnter<FlatlandFuryEnrage>()
            .ActivateOnEnter<ViolentEarth>()
            .ActivateOnEnter<WindChisel>()
            .OnEnter(() =>
            {
                module.Arena.Center = new(-186.5f, 550.5f);
            })
            .Raw.Update = () => module.Raid.Player()?.IsDeadOrDestroyed ?? true;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68683, NameID = 6152)]
public class SaduHeavensflame(WorldState ws, Actor primary) : BossModule(ws, primary, new(-223, 519), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly));
    }
}
