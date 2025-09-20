namespace BossMod.Shadowbringers.Quest.Role.TheHardenedHeart;

public enum OID : uint
{
    Boss = 0x2919,
    Helper = 0x233C,
}

public enum AID : uint
{
    SanctifiedFireIII = 18090, // 2922/2923->players/2917/2915/2914, 8.0s cast, range 6 circle
    TwistedTalent1 = 13637, // Helper->player/2916/2914/2915/2917, 5.0s cast, range 5 circle
    AbyssalCharge1 = 15539, // 25BB->self, 3.0s cast, range 40+R width 4 rect
    DeadlyBite = 15543, // 291D/291C->player/2914, no cast, single-target
    RustingClaw = 15540, // 291B/291A->self, 5.0s cast, range 8+R ?-degree cone
}

class SanctifiedFireIII(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.SanctifiedFireIII, 6f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == StackAction && WorldState.Actors.Find(spell.TargetID) is Actor stackTarget && stackTarget.OID == 0x2915)
            AddStack(stackTarget, Module.CastFinishAt(spell));
    }
}

class TwistedTalent(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.TwistedTalent1, 5f);
class AbyssalCharge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AbyssalCharge1, new AOEShapeRect(40f, 2f));

class AutoBranden(WorldState ws) : QuestBattle.UnmanagedRotation(ws, 3f)
{
    protected override void Exec(Actor? primaryTarget)
    {
        if (primaryTarget == null)
            return;

        var numAOETargets = Hints.PotentialTargets.Count(x => x.Actor.Position.InCircle(Player.Position, 5));

        if (numAOETargets > 1)
        {
            if (ComboAction == Roleplay.AID.Sunshadow)
                UseAction(Roleplay.AID.GreatestEclipse, Player);

            UseAction(Roleplay.AID.Sunshadow, Player);
        }

        if (Player.HPMP.CurHP * 3 < Player.HPMP.MaxHP)
            UseAction(Roleplay.AID.ChivalrousSpirit, Player);

        var gcd = ComboAction switch
        {
            Roleplay.AID.RightfulSword => Roleplay.AID.Swashbuckler,
            Roleplay.AID.FastBlade => Roleplay.AID.RightfulSword,
            _ => Roleplay.AID.FastBlade
        };

        UseAction(gcd, primaryTarget);
        if (Player.DistanceToHitbox(primaryTarget) <= 3f)
            UseAction(Roleplay.AID.FightOrFlight, Player, -10f);

        if (primaryTarget.CastInfo?.Interruptible ?? false)
            UseAction(Roleplay.AID.Interject, primaryTarget, -10f);
    }
}

class TankbusterTether(BossModule module) : BossComponent(module)
{
    private record class Tether(Actor Source, Actor Target, DateTime Activation);
    private Tether? DwarfTether;

    private bool Danger => DwarfTether?.Target.OID == 0x2917;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == 84 && WorldState.Actors.Find(tether.Target) is Actor target)
            DwarfTether = new(source, target, DwarfTether?.Activation ?? WorldState.FutureTime(10));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (DwarfTether?.Target.OID == 0x2917)
            hints.AddForbiddenZone(new SDInvertedRect(DwarfTether.Source.Position, DwarfTether.Target.Position, 1), DwarfTether.Activation);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Danger)
            hints.Add("Take tether!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (DwarfTether is Tether t)
            Arena.AddLine(t.Source.Position, t.Target.Position, Colors.Danger);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.DeadlyBite)
            DwarfTether = null;
    }
}

class BrandenAI(BossModule module) : QuestBattle.RotationModule<AutoBranden>(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var h = hints.PotentialTargets[i];
            h.Priority = h.Actor.FindStatus(775u) == null ? (h.Actor.TargetID == actor.InstanceID ? 2 : 1) : 0;
            if (h.Actor.OID is not (0x291D or 0x2919) && h.Actor.CastInfo == null)
            {
                h.DesiredPosition = Arena.Center;
                if (h.Actor.TargetID == actor.InstanceID && !h.Actor.Position.InCircle(Arena.Center, 5))
                    hints.ForcedTarget = h.Actor;
            }
        }
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

class RustingClaw(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RustingClaw, new AOEShapeCone(10.3f, 45.Degrees()));

class TadricTheVaingloriousStates : StateMachineBuilder
{
    public TadricTheVaingloriousStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BrandenAI>()
            .ActivateOnEnter<SanctifiedFireIII>()
            .ActivateOnEnter<TwistedTalent>()
            .ActivateOnEnter<AbyssalCharge>()
            .ActivateOnEnter<TankbusterTether>()
            .ActivateOnEnter<RustingClaw>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68783, NameID = 8339)]
public class TadricTheVainglorious(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly));
}
