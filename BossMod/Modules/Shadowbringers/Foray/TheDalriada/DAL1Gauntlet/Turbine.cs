namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Gauntlet;

sealed class Turbine(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Turbine, 15f)
{
    private readonly FlamingCyclone _aoe = module.FindComponent<FlamingCyclone>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            // square intentionally slightly smaller to prevent sus knockback
            ref readonly var kb = ref Casters.Ref(0);
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                var aoes = CollectionsMarshal.AsSpan(_aoe.Casters);
                var len = aoes.Length;
                var origins = new WPos[len];
                for (var i = 0; i < len; ++i)
                {
                    origins[i] = aoes[i].Origin;
                }
                hints.AddForbiddenZone(new SDKnockbackInAABBSquareAwayFromOriginPlusAOECircles(Arena.Center, kb.Origin, 15f, 19f, origins, 9f, len), act);
            }
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoe.Casters);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            if (aoes[i].Check(pos))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }
}
