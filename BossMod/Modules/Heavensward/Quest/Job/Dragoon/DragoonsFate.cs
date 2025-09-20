namespace BossMod.Heavensward.Quest.Job.DragoonsFate;

public enum OID : uint
{
    Boss = 0x10B9, // R7.0
    Icicle = 0x10BC, // R2.5
    Graoully = 0x10BA, // R7.0
}

public enum AID : uint
{
    PillarImpact = 3095, // Icicle->self, 3.0s cast, range 4+R circle
    PillarPierce = 4259, // Icicle->self, 2.0s cast, range 80+R width 4 rect
    Cauterize = 4260, // Graoully->self, 3.0s cast, range 48+R width 20 rect
    SheetOfIce = 4261 // Boss->location, 2.5s cast, range 5 circle
}

public enum SID : uint
{
    Prey = 904, // none->player/10BB, extra=0x0
    ThinIce = 905 // Boss->player/10BB, extra=0x1/0x2/0x3
}

class SheetOfIce(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SheetOfIce, 5);
class PillarImpact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PillarImpact, 6.5f);
class PillarPierce(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PillarPierce, new AOEShapeRect(82.5f, 2));
class Cauterize(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Cauterize, new AOEShapeRect(55, 10));

class Prey(BossModule module) : BossComponent(module)
{
    private static readonly AOEShape Cleave = new AOEShapeCone(27, 65.Degrees());
    private int IceStacks(Actor actor) => actor.FindStatus(SID.ThinIce) is ActorStatus st ? st.Extra & 0xFF : 0;

    private Actor? PreyCur;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Prey)
            PreyCur = actor;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (PreyCur is not Actor prey)
            return;

        var partner = WorldState.Party[slot == 0 ? PartyState.MaxAllianceSize : slot]!;

        // force debuff swap
        if (IceStacks(prey) == 3)
            hints.GoalZones.Add(p => p.InCircle(partner.Position, 2) ? 1 : 0);
        else
        {
            // prevent premature swap, even though it doesn't really matter, because the debuff generally falls off with plenty of time left
            hints.AddForbiddenZone(new SDCircle(partner.Position, 5), WorldState.FutureTime(1));

            if (Module.PrimaryActor.IsTargetable)
                hints.AddForbiddenZone(Cleave.Distance(Module.PrimaryActor.Position, Module.PrimaryActor.AngleTo(partner)), WorldState.FutureTime(1));
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        // sometimes partner loses prey status *after* we get it
        if (status.ID == (uint)SID.Prey && actor == PreyCur)
            PreyCur = null;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (PreyCur is Actor p && Module.PrimaryActor is var primary && primary.IsTargetable)
            Cleave.Outline(Arena, primary.Position, primary.AngleTo(p), Colors.Danger);
    }
}

class GraoullyStates : StateMachineBuilder
{
    public GraoullyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Prey>()
            .ActivateOnEnter<PillarImpact>()
            .ActivateOnEnter<PillarPierce>()
            .ActivateOnEnter<Cauterize>()
            .ActivateOnEnter<SheetOfIce>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 67231, NameID = 4190)]
public class Graoully(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom([new(-483.91f, -299.22f), new(-519.70f, -272.85f), new(-546.66f, -309.50f), new(-510.38f, -336.53f)])]);
}
