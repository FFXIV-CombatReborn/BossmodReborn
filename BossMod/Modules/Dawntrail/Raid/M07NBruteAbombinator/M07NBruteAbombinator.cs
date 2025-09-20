using static BossMod.Dawntrail.Raid.BruteAmbombinatorSharedBounds.BruteAmbombinatorSharedBounds;

namespace BossMod.Dawntrail.Raid.M07NBruteAbombinator;

sealed class BrutalImpactRevengeOfTheVines1NeoBombarianSpecial(BossModule module) : Components.RaidwideCasts(module,
[(uint)AID.BrutalImpact, (uint)AID.RevengeOfTheVines1, (uint)AID.NeoBombarianSpecial]);

sealed class Powerslam(BossModule module) : Components.RaidwideCast(module, (uint)AID.Powerslam);
sealed class Slaminator(BossModule module) : Components.CastTowers(module, (uint)AID.Slaminator, 8f, 8, 8);

sealed class ElectrogeneticForce(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.ElectrogeneticForce, 6f);
sealed class SporeSac(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SporeSac, 8f);
sealed class Pollen(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Pollen, 8f);
sealed class Sporesplosion : Components.SimpleAOEs
{
    public Sporesplosion(BossModule module) : base(module, (uint)AID.Sporesplosion, 8f, 12)
    {
        MaxDangerColor = 6;
    }
}

sealed class Explosion : Components.SimpleAOEs
{
    public Explosion(BossModule module) : base(module, (uint)AID.Explosion, 25f, 2)
    {
        MaxDangerColor = 1;
    }
}

sealed class ItCameFromTheDirt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ItCameFromTheDirt, 6f);
sealed class CrossingCrosswinds(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CrossingCrosswinds, new AOEShapeCross(50f, 5f));
sealed class CrossingCrosswindsHint(BossModule module) : Components.CastInterruptHint(module, (uint)AID.CrossingCrosswinds, showNameInHint: true);
sealed class TheUnpotted(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheUnpotted, new AOEShapeCone(60f, 15f.Degrees()));
sealed class WindingWildwinds(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WindingWildwinds, new AOEShapeDonut(5f, 60f));
sealed class WindingWildwindsHint(BossModule module) : Components.CastInterruptHint(module, (uint)AID.WindingWildwinds, showNameInHint: true);
sealed class GlowerPower(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GlowerPower, new AOEShapeRect(65f, 7f));

sealed class BrutishSwingCircle2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BrutishSwingCircle2, 12f);
sealed class BrutishSwingDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BrutishSwingDonut, new AOEShapeDonut(9f, 60f));

sealed class BrutishSwingCone(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BrutishSwingCone1, (uint)AID.BrutishSwingCone2], new AOEShapeCone(25f, 90f.Degrees()));
sealed class BrutishSwingDonutSegment(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BrutishSwingDonutSegment1, (uint)AID.BrutishSwingDonutSegment2], new AOEShapeDonutSector(22f, 88f, 90f.Degrees()));
sealed class LashingLariat(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LashingLariat1, (uint)AID.LashingLariat2], new AOEShapeRect(70f, 16f));

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

sealed class PulpSmash(BossModule module) : Components.StackWithIcon(module, (uint)IconID.PulpSmash, (uint)AID.PulpSmash, 6f, 5.2f, 8, 8);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1023, NameID = 13756)]
public sealed class M07NBruteAbombinator(WorldState ws, Actor primary) : BossModule(ws, primary, FirstCenter, DefaultArena)
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
