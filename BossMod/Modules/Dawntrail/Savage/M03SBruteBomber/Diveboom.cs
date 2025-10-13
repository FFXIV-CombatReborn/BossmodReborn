namespace BossMod.Dawntrail.Savage.M03SBruteBomber;

abstract class Proximity(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 20);  // TODO: verify falloff
sealed class OctoboomDiveProximity(BossModule module) : Proximity(module, (uint)AID.OctoboomDiveProximityAOE);
sealed class QuadroboomDiveProximity(BossModule module) : Proximity(module, (uint)AID.QuadroboomDiveProximityAOE);

abstract class DiveKB(BossModule module, uint aid) : Components.SimpleKnockbacks(module, aid, 25);
sealed class OctoboomDiveKnockback(BossModule module) : DiveKB(module, (uint)AID.OctoboomDiveKnockbackAOE);
sealed class QuadroboomDiveKnockback(BossModule module) : DiveKB(module, (uint)AID.QuadroboomDiveKnockbackAOE);

class Diveboom(BossModule module) : Components.UniformStackSpread(module, 5f, 5f, 2, 2)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.OctoboomDiveProximityAOE:
            case (uint)AID.OctoboomDiveKnockbackAOE:
                AddSpreads(Raid.WithoutSlot(true, true, true), Module.CastFinishAt(spell));
                break;
            case (uint)AID.QuadroboomDiveProximityAOE:
            case (uint)AID.QuadroboomDiveKnockbackAOE:
                // TODO: can target any role
                AddStacks(Raid.WithoutSlot(true, true, true).Where(p => p.Class.IsSupport()), Module.CastFinishAt(spell));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.DiveboomSpread or (uint)AID.DiveboomPair)
        {
            Spreads.Clear();
            Stacks.Clear();
        }
    }
}
