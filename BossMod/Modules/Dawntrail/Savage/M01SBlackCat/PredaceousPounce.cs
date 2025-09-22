namespace BossMod.Dawntrail.Savage.M01SBlackCat;

sealed class PredaceousPounce(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(12);
    private static readonly AOEShapeCircle circle = new(11);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];

        var aoes = CollectionsMarshal.AsSpan(AOEs);
        if (count >= 3)
        {
            for (var i = 0; i < 2; ++i)
            {
                aoes[i].Color = Colors.Danger;
            }
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShape shape, WPos position, Angle rotation = default)
        {
            AOEs.Add(new(shape, position, rotation, Module.CastFinishAt(spell)));
            var count = AOEs.Count;
            if (count == 12)
            {
                AOEs.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
                var aoes = CollectionsMarshal.AsSpan(AOEs);
                for (var i = 0; i < count; ++i)
                    aoes[i].Activation = WorldState.FutureTime(13.5d + i * 0.5d);
            }
        }
        switch (spell.Action.ID)
        {
            case (uint)AID.PredaceousPounceTelegraphCharge1:
            case (uint)AID.PredaceousPounceTelegraphCharge2:
            case (uint)AID.PredaceousPounceTelegraphCharge3:
            case (uint)AID.PredaceousPounceTelegraphCharge4:
            case (uint)AID.PredaceousPounceTelegraphCharge5:
            case (uint)AID.PredaceousPounceTelegraphCharge6:
                var dir = spell.LocXZ - caster.Position;
                AddAOE(new AOEShapeRect(dir.Length(), 3f), caster.Position, Angle.FromDirection(dir));
                break;
            case (uint)AID.PredaceousPounceTelegraphCircle1:
            case (uint)AID.PredaceousPounceTelegraphCircle2:
            case (uint)AID.PredaceousPounceTelegraphCircle3:
            case (uint)AID.PredaceousPounceTelegraphCircle4:
            case (uint)AID.PredaceousPounceTelegraphCircle5:
            case (uint)AID.PredaceousPounceTelegraphCircle6:
                AddAOE(circle, spell.LocXZ);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.PredaceousPounceCharge1:
            case (uint)AID.PredaceousPounceCharge2:
            case (uint)AID.PredaceousPounceCircle1:
            case (uint)AID.PredaceousPounceCircle2:
                ++NumCasts;
                if (AOEs.Count != 0)
                    AOEs.RemoveAt(0);
                break;
        }
    }
}
