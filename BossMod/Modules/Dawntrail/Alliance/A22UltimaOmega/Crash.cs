namespace BossMod.Dawntrail.Alliance.A22UltimaOmega;

sealed class Crash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Crash, new AOEShapeRect(40f, 12f));

sealed class TractorBeam(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.TractorBeam, 25f, minDistance: -5f, kind: Kind.DirBackward)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref var c = ref Casters.Ref(0);

            var act = c.Activation;
            if (!IsImmune(slot, act))
            {
                var center = Arena.Center;
                var dir = -25f * c.Direction.ToDirection();
                hints.AddForbiddenZone(new SDKnockbackInAABBRectFixedDirection(center, dir, 19f, 23.5f), act); // rect intentionally slightly smaller to prevent sus knockback
            }
        }
    }
}
