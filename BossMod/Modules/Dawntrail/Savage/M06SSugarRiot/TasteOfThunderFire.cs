namespace BossMod.Dawntrail.Savage.M06SSugarRiot;

sealed class TasteOfThunderFire(BossModule module) : Components.GenericStackSpread(module, false)
{
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DoubleStyle3)
        {
            var party = Raid.WithoutSlot(true, true, true);
            var len = party.Length;
            var act = Module.CastFinishAt(spell, 4.2d);
            for (var i = 0; i < len; ++i)
            {
                var p = party[i];
                if (p.Role == Role.Healer)
                {
                    Stacks.Add(new(p, 6f, 4, 4, act));
                }
            }
        }
        else if (spell.Action.ID == (uint)AID.DoubleStyle5)
        {
            var party = Raid.WithoutSlot(false, true, true);
            var len = party.Length;
            var act = Module.CastFinishAt(spell, 4.2d);
            for (var i = 0; i < len; ++i)
            {
                Spreads.Add(new(party[i], 6f, act));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.TasteOfFire or (uint)AID.TasteOfThunderSpread)
        {
            ++NumCasts;
        }
    }
}

sealed class TasteOfThunderAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TasteOfThunderAOE, 3f);
