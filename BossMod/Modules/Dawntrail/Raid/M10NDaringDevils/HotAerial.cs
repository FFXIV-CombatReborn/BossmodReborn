namespace BossMod.Dawntrail.Raid.M10NDaringDevils;

// Towers: 4 intercardinal, require at least 1 soaker.
sealed class HotAerialTowers(BossModule module) : Components.CastTowers(
    module,
    (uint)AID.HotAerial2,
    radius: 6f,
    minSoakers: 1,
    maxSoakers: 1,
    damageType: AIHints.PredictedDamageType.Raidwide);

// Persistent fire puddles left behind after Red Hot jumps to each tower location.
// prediction is hidden until the puddle actually appears, so remaining towers stay reachable
sealed class HotAerialFirePuddles(BossModule module) : TrackedVoidzones(module, new AOEShapeCircle(6f), (uint)OID.HotAerialVoidzone, true)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.HotAerial1)
        {
            Predict(spell.TargetXZ, default, WorldState.FutureTime(1.5d));
        }
    }
}
