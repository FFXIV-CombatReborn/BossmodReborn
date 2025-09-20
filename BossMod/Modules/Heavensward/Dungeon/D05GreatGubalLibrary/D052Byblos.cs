namespace BossMod.Heavensward.Dungeon.D05GreatGubalLibrary.D052Byblos;

public enum OID : uint
{
    Boss = 0xE83, // R3.0
    Page64 = 0x10B7, // R1.2
    WhaleOil = 0x10C4, // R2.0
    Tomewind = 0x10C5, // R1.0
    Page64BooksVisual = 0x1E99EE, // R0.5
    BossBookVisual = 0x1E996A // R0.5
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    PageTear = 4159, // Boss->self, no cast, range 5+R ?-degree cone
    HeadDown = 4163, // Boss->player, 4.0s cast, width 8 rect charge
    BoneShaker = 4164, // Boss->self, no cast, range 50+R circle
    DeathRay = 5058, // Page64->self, 4.0s cast, range 23+R width 3 rect
    Bibliocide = 4167, // WhaleOil->self, no cast, range 3 circle
    GaleCut = 4158, // Boss->self, 3.0s cast, single-target, spawns Tomewinds
    VacuumBlade = 4168, // TomeWind->self, no cast, range 3 circle, touched Tomewind
    TailSmash = 4165 // Boss->self, 2.5s cast, range 9+R 90-degree cone
}

public enum TetherID : uint
{
    WhaleOil = 3
}

public enum SID : uint
{
    Invincibility = 325 // none->Boss, extra=0x0
}

class PageTear(BossModule module) : Components.Cleave(module, (uint)AID.PageTear, new AOEShapeCone(8f, 45f.Degrees()))
{
    public static bool IsInvincible(Actor actor) => actor.FindStatus((uint)SID.Invincibility) != null;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!IsInvincible(Module.PrimaryActor))
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!IsInvincible(Module.PrimaryActor))
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!IsInvincible(Module.PrimaryActor))
            base.DrawArenaForeground(pcSlot, pc);
    }
}

class HeadDown(BossModule module) : Components.BaitAwayChargeCast(module, (uint)AID.HeadDown, 4f);
class DeathRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeathRay, new AOEShapeRect(24.2f, 1.5f));
class Tomewind(BossModule module) : Components.Voidzone(module, 3f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.Tomewind);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (!z.IsDead)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

class TailSmash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TailSmash, new AOEShapeCone(12f, 45f.Degrees()));

class Bibliocide(BossModule module) : Components.BaitAwayTethers(module, 0f, (uint)TetherID.WhaleOil, activationDelay: 5f)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var targetFound = false;
        var count = CurrentBaits.Count;
        for (var i = 0; i < count; ++i)
        {
            if (CurrentBaits[i].Target == actor)
            {
                targetFound = true;
                break;
            }
        }
        if (!targetFound)
            return;

        hints.Add("Pull the orb to the boss!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = CurrentBaits.Count;
        Bait? target = null;
        for (var i = 0; i < count; ++i)
        {
            var bait = CurrentBaits[i];
            if (CurrentBaits[i].Target == actor)
            {
                target = bait;
                break;
            }
        }
        if (target == null)
            return;

        hints.AddForbiddenZone(new SDInvertedCircle(Module.PrimaryActor.Position, Module.PrimaryActor.HitboxRadius), target.Value.Activation);
    }
}

class D052ByblosStates : StateMachineBuilder
{
    public D052ByblosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PageTear>()
            .ActivateOnEnter<HeadDown>()
            .ActivateOnEnter<TailSmash>()
            .ActivateOnEnter<DeathRay>()
            .ActivateOnEnter<Tomewind>()
            .ActivateOnEnter<Bibliocide>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 31, NameID = 3925)]
public class D052Byblos(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(182.36f, 3.29f), new(182.89f, 3.68f), new(183.4f, 3.98f), new(185.25f, 3.93f), new(185.87f, 4.23f),
    new(187, 4.87f), new(187.68f, 5.04f), new(188.36f, 5.23f), new(189, 5.48f), new(190.01f, 6.3f),
    new(190.52f, 6.64f), new(191.63f, 7.14f), new(192.21f, 7.45f), new(193.13f, 8.47f), new(193.62f, 8.91f),
    new(194.81f, 9.7f), new(195.26f, 10.2f), new(195.97f, 11.28f), new(196.41f, 11.75f), new(197.46f, 12.7f),
    new(197.77f, 13.3f), new(198.31f, 14.52f), new(199.19f, 15.61f), new(199.55f, 16.13f), new(199.91f, 17.43f),
    new(200.11f, 18), new(200.78f, 19.17f), new(201.03f, 19.82f), new(201.18f, 21.12f), new(201.31f, 21.74f),
    new(201.79f, 23), new(201.88f, 23.68f), new(201.82f, 24.95f), new(201.88f, 25.56f), new(202.15f, 26.85f),
    new(202.11f, 27.53f), new(201.85f, 28.75f), new(201.83f, 29.37f), new(201.89f, 30.66f), new(201.76f, 31.3f),
    new(201.31f, 32.48f), new(201.18f, 33.07f), new(201.04f, 34.44f), new(200.77f, 35.07f), new(200.13f, 36.19f),
    new(199.91f, 36.76f), new(199.6f, 37.94f), new(199.28f, 38.51f), new(198.45f, 39.53f), new(198.13f, 40.07f),
    new(197.9f, 40.67f), new(197.64f, 41.27f), new(197.19f, 41.79f), new(196.2f, 42.67f), new(195.83f, 43.15f),
    new(195.16f, 44.18f), new(194.67f, 44.61f), new(193.64f, 45.27f), new(193.2f, 45.69f), new(192.37f, 46.61f),
    new(191.85f, 47.01f), new(190.73f, 47.5f), new(190.19f, 47.79f), new(189.19f, 48.59f), new(188.63f, 48.93f),
    new(187.41f, 49.25f), new(186.82f, 49.44f), new(186.28f, 49.8f), new(185.7f, 50.14f), new(185.06f, 50.31f),
    new(183.78f, 50.26f), new(183.21f, 50.32f), new(182.73f, 50.68f), new(182.15f, 50.96f), new(173.51f, 50.96f),
    new(172.88f, 50.62f), new(172.34f, 50.3f), new(171.74f, 50.27f), new(170.48f, 50.32f), new(169.83f, 50.1f),
    new(168.68f, 49.42f), new(168.09f, 49.22f), new(166.79f, 48.87f), new(166.28f, 48.49f), new(165.3f, 47.7f),
    new(163.58f, 46.95f), new(163.08f, 46.48f), new(162.21f, 45.51f), new(161.71f, 45.13f), new(160.58f, 44.4f),
    new(160.22f, 43.87f), new(159.51f, 42.78f), new(159.05f, 42.39f), new(158.05f, 41.49f), new(157.26f, 39.73f),
    new(156.88f, 39.22f), new(156.07f, 38.25f), new(155.88f, 37.61f), new(155.55f, 36.39f), new(155.25f, 35.81f),
    new(154.59f, 34.67f), new(154.39f, 32.77f), new(154.18f, 32.15f), new(153.93f, 31.52f), new(153.72f, 30.86f),
    new(153.81f, 28.96f), new(153.64f, 28.34f), new(153.5f, 27.68f), new(153.41f, 26.98f), new(153.79f, 25.15f),
    new(153.73f, 23.82f), new(153.76f, 23.16f), new(154.21f, 21.98f), new(154.38f, 21.37f), new(154.51f, 20.13f),
    new(154.64f, 19.48f), new(155.56f, 17.86f), new(155.92f, 16.56f), new(156.18f, 15.9f), new(156.95f, 14.94f),
    new(157.28f, 14.46f), new(157.83f, 13.24f), new(158.19f, 12.65f), new(159.55f, 11.41f), new(160.23f, 10.36f),
    new(160.64f, 9.79f), new(161.79f, 9.03f), new(162.3f, 8.63f), new(163.16f, 7.69f), new(163.7f, 7.27f),
    new(164.8f, 6.77f), new(165.35f, 6.46f), new(166.46f, 5.57f), new(167.11f, 5.27f), new(168.4f, 4.92f),
    new(168.92f, 4.66f), new(170.08f, 4), new(170.72f, 3.95f), new(171.94f, 3.99f), new(172.48f, 3.84f),
    new(173.01f, 3.48f), new(179.1f, 3.26f), new(182.13f, 3.26f)];

    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        if (PrimaryActor.FindStatus((uint)SID.Invincibility) == null)
            Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Page64));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            if (e.Actor.FindStatus((uint)SID.Invincibility) != null)
            {
                e.Priority = AIHints.Enemy.PriorityInvincible;
                break;
            }
        }
    }
}
