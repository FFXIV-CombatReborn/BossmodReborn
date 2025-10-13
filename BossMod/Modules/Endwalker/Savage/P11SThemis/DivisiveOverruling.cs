namespace BossMod.Endwalker.Savage.P11SThemis;

class DivisiveOverruling(BossModule module) : Components.GenericAOEs(module)
{
    public List<AOEInstance> AOEs = [];

    private static readonly AOEShapeRect _shapeNarrow = new(46f, 8f);
    private static readonly AOEShapeRect _shapeWide = new(46f, 13f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(AOEs);
        var deadline = aoes[0].Activation.AddSeconds(1d);

        var index = 0;
        while (index < count)
        {
            ref var aoe = ref aoes[index];
            if (aoe.Activation >= deadline)
            {
                break;
            }
            ++index;
        }

        return aoes[..index];
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.DivisiveOverrulingSoloAOE:
            case (uint)AID.DivisiveRulingAOE:
            case (uint)AID.DivisiveOverrulingBossAOE:
            case (uint)AID.RipplesOfGloomSoloR:
            case (uint)AID.RipplesOfGloomCloneR:
            case (uint)AID.RipplesOfGloomBossR:
            case (uint)AID.RipplesOfGloomSoloL:
            case (uint)AID.RipplesOfGloomCloneL:
            case (uint)AID.RipplesOfGloomBossL:
                AddAOE(_shapeNarrow);
                break;
            case (uint)AID.DivineRuinationSolo:
            case (uint)AID.DivineRuinationClone:
            case (uint)AID.DivineRuinationBoss:
                AddAOE(_shapeWide);
                break;
        }
        void AddAOE(AOEShapeRect shape)
        {
            AOEs.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            AOEs.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.DivisiveOverrulingSoloAOE:
            case (uint)AID.DivisiveRulingAOE:
            case (uint)AID.DivisiveOverrulingBossAOE:
            case (uint)AID.DivineRuinationSolo:
            case (uint)AID.DivineRuinationClone:
            case (uint)AID.DivineRuinationBoss:
            case (uint)AID.RipplesOfGloomSoloR:
            case (uint)AID.RipplesOfGloomCloneR:
            case (uint)AID.RipplesOfGloomBossR:
            case (uint)AID.RipplesOfGloomSoloL:
            case (uint)AID.RipplesOfGloomCloneL:
            case (uint)AID.RipplesOfGloomBossL:
                if (AOEs.Count > 0)
                {
                    AOEs.RemoveAt(0);
                }
                ++NumCasts;
                break;
        }
    }
}
