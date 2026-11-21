namespace BossMod.Dawntrail.Raid.M01NBlackCat;

sealed class Shockwave(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Shockwave, 18f, stopAfterWall: true)
{
    private RelSimplifiedComplexPolygon polygon = new();
    private ArenaBounds? bounds;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var act = c.Activation;
            if (!IsImmune(slot, act))
            {
                if (bounds != Arena.Bounds)
                {
                    bounds = Arena.Bounds;
                    polygon = bounds.Shape.Offset(-1f); // pretend polygon is 1y smaller than real for less suspect knockbacks
                }

                hints.AddForbiddenZone(new SDKnockbackInComplexPolygonAwayFromOriginPlusIntersectionTest(Arena.Center, c.Origin, 18f, polygon), act);
            }
        }
    }
}
