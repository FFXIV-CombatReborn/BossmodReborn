using static BossMod.Dawntrail.Raid.BruteAmbombinatorSharedBounds.BruteAmbombinatorSharedBounds;

namespace BossMod.Dawntrail.Savage.M07SBruteAbombinator;

sealed class BrutalImpact(BossModule module) : Components.CastCounter(module, (uint)AID.BrutalImpact);
sealed class RevengeOfTheVines2(BossModule module) : Components.CastCounter(module, (uint)AID.RevengeOfTheVines2);
sealed class Slaminator(BossModule module) : Components.CastTowers(module, (uint)AID.Slaminator, 8f, 8, 8);

sealed class Explosion : Components.SimpleAOEs
{
    public Explosion(BossModule module) : base(module, (uint)AID.Explosion, 25f, 2)
    {
        MaxDangerColor = 1;
    }
}

sealed class Sporesplosion : Components.SimpleAOEs
{
    public Sporesplosion(BossModule module) : base(module, (uint)AID.Sporesplosion, 8f, 12)
    {
        MaxDangerColor = 6;
    }
}

sealed class NeoBombarianSpecialKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.NeoBombarianSpecial, 58f, true)
{
    private RelSimplifiedComplexPolygon poly;
    private bool polyInit;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Arena.Bounds == KnockbackArena) // this doesn't seem to be a regular knockback that ends up in PendingKnockbacks, so forbidden zone must stay longer than cast
        {
            if (!polyInit)
            {
                poly = KnockbackArena.Polygon.Offset(-1f); // shrink polygon by 1 yalm for less suspect kb
                polyInit = true;
            }
            ref var c = ref Casters.Ref(0);
            hints.AddForbiddenZone(new SDKnockbackInComplexPolygonAwayFromOrigin(Arena.Center, Module.PrimaryActor.Position, 58f, poly), c.Activation);
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1024, NameID = 13756, PlanLevel = 100)]
public sealed class M07SBruteAbombinator(WorldState ws, Actor primary) : BossModule(ws, primary, FirstCenter, DefaultArena)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.BloomingAbomination));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.BloomingAbomination => 1,
                _ => 0
            };
        }
    }
}
