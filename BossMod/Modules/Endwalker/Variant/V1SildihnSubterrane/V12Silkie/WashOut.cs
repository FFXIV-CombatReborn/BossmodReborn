namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V12Silkie;

sealed class WashOut(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.WashOut, 35f, kind: Kind.DirForward)
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
                var count = waterVZs.Count;
                var dir = kb.Direction.ToDirection();

                // square intentionally slightly smaller to prevent sus knockback
                hints.AddForbiddenZone(new SDKnockbackInAABBSquareFixedDirection(Arena.Center, 35f * dir, 19f));

                for (var i = 0; i < count; ++i)
                {
                    hints.AddForbiddenZone(new SDRect(waterVZs[i].Position, dir, 40f, 40f, 5f), act);
                }
            }
        }
    }
}
