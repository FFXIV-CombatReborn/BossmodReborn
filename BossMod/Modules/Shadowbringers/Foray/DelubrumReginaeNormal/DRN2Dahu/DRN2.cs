namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN2Dahu;

sealed class FallingRock(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FallingRock, 4f);
sealed class HotCharge(BossModule module) : Components.ChargeAOEs(module, (uint)AID.HotCharge, 4f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (((++NumCasts) & 1) == 0)
        {
            return;
        }
        hints.GoalZones.Add(AIHints.GoalSingleTarget(Module.PrimaryActor.CastInfo?.LocXZ ?? Arena.Center, 6f, 5f)); // follow the charge
    }
}

sealed class Firebreathe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Firebreathe, new AOEShapeCone(60f, 45f.Degrees()));
sealed class HeadDown(BossModule module) : Components.ChargeAOEs(module, (uint)AID.HeadDown, 2f);
sealed class HuntersClaw(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HuntersClaw, 8f);
sealed class HeatBreath(BossModule module) : Components.BaitAwayCast(module, (uint)AID.HeatBreath, new AOEShapeCone(10f, 45f.Degrees()), endsOnCastEvent: true, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);
sealed class RipperClaw(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RipperClaw, new AOEShapeCone(10f, 45f.Degrees()));
sealed class TailSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TailSwing, 10f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 760, NameID = 9751)]
public sealed class DRN2Dahu(WorldState ws, Actor primary) : Dahu(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies((uint)OID.Marchosias));
    }
}
