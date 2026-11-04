namespace BossMod.Endwalker.VariantCriterion.C01ASS.C013Shadowcaster;

sealed class CastShadow(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCone cone = new(65f, 15f.Degrees());
    public readonly List<AOEInstance> AOEs = [with(8)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var remaining = AOEs.Count - NumCasts;
        if (remaining <= 0)
        {
            return [];
        }

        var max = remaining > 6 ? 6 : remaining;
        return CollectionsMarshal.AsSpan(AOEs).Slice(NumCasts, max);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NCastShadowAOE1 or (uint)AID.SCastShadowAOE1 or (uint)AID.NCastShadowAOE2 or (uint)AID.SCastShadowAOE2)
        {
            AOEs.Add(new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            if (AOEs.Count == 12)
            {
                SortHelpers.SortAOEByActivation(AOEs);
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (AOEs.Count != 0 && spell.Action.ID is (uint)AID.NCastShadowAOE1 or (uint)AID.SCastShadowAOE1 or (uint)AID.NCastShadowAOE2 or (uint)AID.SCastShadowAOE2)
        {
            ++NumCasts;
        }
    }
}
