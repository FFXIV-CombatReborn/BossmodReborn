namespace BossMod.Dawntrail.Alliance.A23Kamlanaut;

sealed class ShieldBash(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.ShieldBash, 30f, stopAfterWall: true)
{
    private RelSimplifiedComplexPolygon? _polygon;
    private ArenaBounds? _polygonBounds;

    public RelSimplifiedComplexPolygon GetPolygon()
    {
        var bounds = Arena.Bounds;
        if (_polygon == null || !ReferenceEquals(bounds, _polygonBounds))
        {
            _polygon = bounds.Shape.Offset(-1f);
            _polygonBounds = bounds;
        }
        return _polygon;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var act = c.Activation;
            if (!IsImmune(slot, act))
            {
                hints.AddForbiddenZone(new SDKnockbackInComplexPolygonAwayFromOrigin(Arena.Center, c.Origin, 30f, GetPolygon()), act);
            }
        }
    }
}
