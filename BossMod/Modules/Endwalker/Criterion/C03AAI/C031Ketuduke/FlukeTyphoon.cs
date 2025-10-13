namespace BossMod.Endwalker.VariantCriterion.C03AAI.C031Ketuduke;

class FlukeTyphoon(BossModule module) : Components.CastCounter(module, (uint)AID.FlukeTyphoonAOE);

class FlukeTyphoonBurst(BossModule module) : Components.GenericTowers(module)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001u)
        {
            WDir offset = index switch
            {
                0 => new(-10, -15),
                1 => new(-14, 0),
                2 => new(-10, +15),
                3 => new(+10, -15),
                4 => new(+14, 0),
                5 => new(+10, +15),
                _ => default
            };
            if (offset != default)
                Towers.Add(new(Arena.Center + offset, 4f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NBurst or (uint)AID.SBurst or (uint)AID.NBigBurst or (uint)AID.SBigBurst)
        {
            Towers.Clear();
            ++NumCasts;
        }
    }
}
