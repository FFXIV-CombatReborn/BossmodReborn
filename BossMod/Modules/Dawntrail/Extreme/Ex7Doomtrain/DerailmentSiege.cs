namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

sealed class DerailmentSiege(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DerailmentSiegeVisual1)
        {
            Towers.Add(new(spell.LocXZ, 5f, 1, 1, activation: Module.CastFinishAt(spell, 6d)));
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (Towers.Count != 0 && actor.OID == (uint)OID.Tower && state == 0x00100020u)
        {
            Towers.Ref(0).Position = actor.Position; // spell position can be about one or two pixels off the real tower position...
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.DerailmentSiege1 or (uint)AID.DerailmentSiege2)
        {
            ++NumCasts;
            if (spell.Action.ID == (uint)AID.AsuranFists3)
                Towers.Clear();
        }
    }
}

sealed class Derail(BossModule module) : Components.CastHint(module, (uint)AID.Derail1, "Derail, get to the next platform!", true);
