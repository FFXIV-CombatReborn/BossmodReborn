namespace BossMod.Heavensward.Dungeon.D09SaintMociannesArboretum.D092QueenHawk;

public enum OID : uint
{
    Boss = 0x1432, // R2.4
    KnightHawk = 0x1433, // R1.08
    PoisonVoidzone = 0x1E8FCA, // R0.5
    Helper = 0x1B2
}

public enum AID : uint
{
    AutoAttack = 871, // Boss->player, no cast, single-target

    StingerCell = 5236, // Boss->self, no cast, range 6+R width 5 rect
    SpawnKnights = 5378, // Boss->self, no cast, single-target
    SharpSpindle = 5241, // KnightHawk->player, no cast, single-target
    PheromoneLeak = 5235, // Boss->self, no cast, range 40 circle, raidwides while knights are alive
    Assail = 5238, // Boss->self, 3.0s cast, single-target
    StraightSpindle = 5240, // KnightHawk->self, 2.5s cast, range 50+R width 3 rect
    Apitoxin = 5237, // Boss->location, 3.0s cast, range 6 circle
    Avail = 5233, // Boss->self, 3.0s cast, single-target
    Ally = 5239, // Boss->self, 3.0s cast, single-target
    CrossfireVisual = 5242, // KnightHawk->self, 3.3s cast, single-target
    Crossfire = 5243 // Helper->self, 4.0s cast, range 50+R width 14 rect
}

public enum SID : uint
{
    StoneskinPhysical = 152 // none->Boss, extra=0x0
}

sealed class StingerCell(BossModule module) : Components.Cleave(module, (uint)AID.StingerCell, new AOEShapeRect(8.4f, 2.5f), activeWhileCasting: false)
{
    private bool stoneskin;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.StoneskinPhysical)
        {
            stoneskin = true;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.StoneskinPhysical)
        {
            stoneskin = false;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (stoneskin)
        {
            base.AddHints(slot, actor, hints);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (stoneskin)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (stoneskin)
        {
            base.DrawArenaForeground(pcSlot, pc);
        }
    }
}

sealed class Apitoxin(BossModule module) : Components.VoidzoneAtCastTarget(module, 6f, (uint)AID.Apitoxin, GetVoidzones, 0.7d)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.PoisonVoidzone);
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

sealed class StraightSpindle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StraightSpindle, new AOEShapeRect(51.08f, 1.5f));
sealed class Crossfire(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Crossfire, new AOEShapeRect(50.5f, 7f));

sealed class D092QueenHawkStates : StateMachineBuilder
{
    public D092QueenHawkStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StingerCell>()
            .ActivateOnEnter<Apitoxin>()
            .ActivateOnEnter<StraightSpindle>()
            .ActivateOnEnter<Crossfire>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 41, NameID = 4656, SortOrder = 4)]
public sealed class D092QueenHawk(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaBounds.Center, ArenaBounds)
{
    public static readonly ArenaBoundsCustom ArenaBounds = new([new Polygon(new(-268f, -134f), 19.5f * CosPI.Pi48th, 48)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        if (PrimaryActor.FindStatus((uint)SID.StoneskinPhysical) == null)
            Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.KnightHawk));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            if (e.Actor.FindStatus((uint)SID.StoneskinPhysical) != null)
            {
                e.Priority = AIHints.Enemy.PriorityInvincible;
                break;
            }
        }
    }
}
