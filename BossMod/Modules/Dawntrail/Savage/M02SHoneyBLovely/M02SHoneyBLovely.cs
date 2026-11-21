namespace BossMod.Dawntrail.Savage.M02SHoneyBLovely;

sealed class StingingSlash(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(50f, 45f.Degrees()), (uint)IconID.StingingSlash, (uint)AID.StingingSlashAOE);
sealed class KillerSting(BossModule module) : Components.IconSharedTankbuster(module, (uint)IconID.KillerSting, (uint)AID.KillerStingAOE, 6f);

sealed class BlindingLoveBait : Components.SimpleAOEs
{
    public BlindingLoveBait(BossModule module) : base(module, (uint)AID.BlindingLoveBaitAOE, new AOEShapeRect(50f, 4f)) { MaxDangerColor = 2; }
}

abstract class BlindingLoveCharge(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeRect(45f, 5f));
sealed class BlindingLoveCharge1(BossModule module) : BlindingLoveCharge(module, (uint)AID.BlindingLoveCharge1AOE);
sealed class BlindingLoveCharge2(BossModule module) : BlindingLoveCharge(module, (uint)AID.BlindingLoveCharge2AOE);

sealed class PoisonStingBait(BossModule module) : Components.BaitAwayCast(module, (uint)AID.PoisonStingAOE, 6f);
sealed class PoisonStingVoidzone(BossModule module) : Components.Voidzone(module, 6f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.PoisonStingVoidzone);
        var count = enemies.Count;
        if (count == 0)
        {
            return [];
        }
        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
            {
                voidzones[index++] = z;
            }
        }
        return voidzones[..index];
    }
}
sealed class BeeSting(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.BeeStingAOE, 6f, 4, 4);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 988, NameID = 12685, PlanLevel = 100)]
public sealed class M02SHoneyBLovely(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f));