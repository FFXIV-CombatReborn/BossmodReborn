namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

sealed class LegitimateForce(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [with(2)];
    private readonly AOEShapeRect rect = new(60f, 15f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(AOEs);
        if (count == 1)
        {
            aoes[0].Risky = true;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LegitimateForceFirstR:
                AddAOEs(true);
                break;
            case (uint)AID.LegitimateForceFirstL:
                AddAOEs(false);
                break;
        }
        void AddAOEs(bool rightFirst)
        {
            WDir right = new(-15f, -10f);
            WDir left = new(15f, -10f);
            var rot = spell.Rotation;
            var pos = caster.Position;
            AddAOE(rightFirst ? right : left);
            AddAOE(rightFirst ? left : right, 3.1d, false);
            void AddAOE(WDir offset, double delay = default, bool first = true) => AOEs.Add(new(rect, (pos + offset).Quantized(), rot, Module.CastFinishAt(spell, delay), first ? Colors.Danger : default, first));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LegitimateForceFirstL:
            case (uint)AID.LegitimateForceFirstR:
            case (uint)AID.LegitimateForceSecondL:
            case (uint)AID.LegitimateForceSecondR:
                ++NumCasts;
                if (AOEs.Count != 0)
                {
                    AOEs.RemoveAt(0);
                }
                break;
        }
    }
}
