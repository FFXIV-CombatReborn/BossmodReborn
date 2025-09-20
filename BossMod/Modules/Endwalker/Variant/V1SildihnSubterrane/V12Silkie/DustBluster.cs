namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V12Silkie;

sealed class DustBluster(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.DustBluster, 16f)
{
    private readonly List<Actor> waterVZs = module.Enemies((uint)OID.WaterVoidzone);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref var kb = ref Casters.Ref(0);
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                var origin = kb.Origin;
                var count = waterVZs.Count;
                var forbidden = new ShapeDistance[count + 1];

                // square intentionally slightly smaller to prevent sus knockback
                forbidden[count] = new SDKnockbackInAABBSquareAwayFromOrigin(Arena.Center, origin, 16f, 19f);

                for (var i = 0; i < count; ++i)
                {
                    var a = waterVZs[i].Position;
                    forbidden[i] = new SDCone(origin, 100f, Angle.FromDirection(a - origin), Angle.Asin(5f / (a - origin).Length()));
                }
                hints.AddForbiddenZone(new SDUnion(forbidden), act);
            }
        }
    }
}
