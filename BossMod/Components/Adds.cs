namespace BossMod.Components;

// generic component used for drawing adds
[SkipLocalsInit]
public class Adds(BossModule module, uint oid, int priority = 0) : BossComponent(module)
{
    public readonly List<Actor> Actors = module.Enemies(oid);
    public List<Actor> ActiveActors
    {
        get
        {
            var count = Actors.Count;
            var activeActors = new List<Actor>(count);
            for (var i = 0; i < count; ++i)
            {
                var actor = Actors[i];
                if (actor.IsTargetable && !actor.IsDead)
                {
                    activeActors.Add(actor);
                }
            }
            return activeActors;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (priority != 0)
        {
            hints.PrioritizeTargetsByOID(oid, priority);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Actors);
    }
}

// component for adds that shouldn't be targeted at all, but should still be drawn
[SkipLocalsInit]
public class AddsPointless(BossModule module, uint oid) : Adds(module, oid)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Actors.Count;
        for (var i = 0; i < count; ++i)
        {
            hints.SetPriority(Actors[i], AIHints.Enemy.PriorityPointless);
        }
    }
}

// generic component used for drawing multiple adds with multiple oids, when it's not useful to distinguish between them
[SkipLocalsInit]
public class AddsMulti(BossModule module, uint[] oids, int priority = 0) : BossComponent(module)
{
    public readonly uint[] OIDs = oids;
    public List<Actor> ActiveActors
    {
        get
        {
            var enemies = Module.Enemies(OIDs);
            var count = enemies.Count;
            List<Actor> activeActors = new(count);
            for (var i = 0; i < count; ++i)
            {
                var actor = enemies[i];
                if (actor.IsTargetable && !actor.IsDead)
                {
                    activeActors.Add(actor);
                }
            }
            return activeActors;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (priority != 0)
        {
            hints.PrioritizeTargetsByOID(OIDs, priority);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Module, OIDs);
    }
}
