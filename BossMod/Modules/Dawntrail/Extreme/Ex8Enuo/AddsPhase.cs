namespace BossMod.Dawntrail.Extreme.Ex8Enuo;

sealed class LoomingShadowAdd(BossModule module) : Components.Adds(module, (uint)OID.LoomingShadow);

sealed class AggressiveShadowAdd(BossModule module) : Components.Adds(module, (uint)OID.AggressiveShadow);

sealed class SupportShadowAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.ProtectiveShadow, (uint)OID.SoothingShadow]);

sealed class BeaconAdd(BossModule module) : Components.Adds(module, (uint)OID.BeaconInTheDark);

sealed class LoomingEmptinessKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.LoomingEmptinessKnockback, 20f);

sealed class LoomingEmptinessKillZone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LoomingEmptinessKillzone, 8f);

sealed class EmptyShadowTower(BossModule module) : Components.CastTowers(module, (uint)AID.EmptyShadow, 7f)
{
    private BitMask badsoakers;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.VoidTurbulanceCone)
        {
            var slot = Raid.FindSlot(targetID);
            badsoakers.Set(slot);
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.VoidalTurbulenceCone)
        {
            badsoakers = default;
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Towers.Add(new(spell.LocXZ, Radius, MinSoakers, MaxSoakers, activation: Module.CastFinishAt(spell), actorID: caster.InstanceID, forbiddenSoakers: badsoakers));
        }
    }
}

//Cone halfangle is an estimate here, but 30 Degrees looks pretty close.
sealed class VoidalTurbulanceCone(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60f, 30f.Degrees()), (uint)IconID.VoidTurbulanceCone, (uint)AID.VoidalTurbulenceCone)
{
    public override Actor? BaitSource(Actor target)
    {
        var enemies = Module.Enemies((uint)OID.LoomingShadow);
        var count = enemies.Count;
        if (count == 0)
        {
            return null;
        }
        return enemies[0];
    }
}
sealed class DemonEye(BossModule module) : Components.CastGaze(module, (uint)AID.DemonEyeCastbar);

sealed class DrainTouch(BossModule module) : Components.CastInterruptHint(module, (uint)AID.DrainTouch);

sealed class WeightofNothing(BossModule module) : Components.BaitAwayCast(module, (uint)AID.WeightOfNothing, new AOEShapeRect(100f, 4f));

sealed class CurseoftheFlesh(BossModule module) : Components.CleansableDebuff(module, (uint)SID.Disease);
sealed class Nothingness(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Nothingness, new AOEShapeRect(100f, 2f));
