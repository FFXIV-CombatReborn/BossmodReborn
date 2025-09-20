namespace BossMod.Dawntrail.Raid.M01NBlackCat;

sealed class Shockwave(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Shockwave, 18f, stopAfterWall: true)
{
    private RelSimplifiedComplexPolygon polygon;
    private bool polygonInit;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (spell.Action.ID == WatchedAction)
        {
            polygonInit = false;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var act = c.Activation;
            if (!IsImmune(slot, act))
            {
                if (!polygonInit)
                {
                    if (Arena.Bounds is ArenaBoundsCustom arena)
                    {
                        polygon = arena.Polygon.Offset(-1f); // pretend polygon is 1y smaller than real for less suspect knockbacks
                        polygonInit = true;
                    }
                }

                hints.AddForbiddenZone(new SDKnockbackInComplexPolygonAwayFromOriginPlusIntersectionTest(Arena.Center, c.Origin, 10f, polygon), act);
            }
        }
    }
}
