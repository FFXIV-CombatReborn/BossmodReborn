namespace BossMod.Dawntrail.Raid.M02NHoneyBLovely;

sealed class CallMeHoney(BossModule module) : Components.RaidwideCast(module, (uint)AID.CallMeHoney);
sealed class TemptingTwist(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.TemptingTwist1, (uint)AID.TemptingTwist2], new AOEShapeDonut(7f, 30f));
sealed class HoneyBeeline(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.HoneyBeeline1, (uint)AID.HoneyBeeline2], new AOEShapeRect(60f, 7f));

sealed class HoneyedBreeze(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(40f, 15f.Degrees()), (uint)IconID.HoneyedBreezeTB, (uint)AID.HoneyedBreeze, 5f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

sealed class HoneyBLive(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.HoneyBLiveVisual, (uint)AID.HoneyBLive, 8.3f);
sealed class Heartsore(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Heartsore, 6f);
sealed class Heartsick(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Heartsick, 6f, 4, 4);
sealed class Loveseeker(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Loveseeker, 10f);
sealed class BlowKiss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlowKiss, new AOEShapeCone(40f, 60f.Degrees()));
sealed class HoneyBFinale(BossModule module) : Components.RaidwideCast(module, (uint)AID.HoneyBFinale);
sealed class DropOfVenom(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.DropOfVenom, 6f, 8, 8);
sealed class SplashOfVenom(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.SplashOfVenom, 6f);

sealed class BlindingLove1 : Components.SimpleAOEs
{
    public BlindingLove1(BossModule module) : base(module, (uint)AID.BlindingLove1, new AOEShapeRect(50f, 4f)) { MaxDangerColor = 2; }
}
sealed class BlindingLove2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlindingLove2, new AOEShapeRect(50f, 4f));
sealed class HeartStruck1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeartStruck1, 4f);
sealed class HeartStruck2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeartStruck2, 6f);
sealed class HeartStruck3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeartStruck3, 10f, maxCasts: 8);

sealed class Fracture(BossModule module) : Components.CastTowers(module, (uint)AID.Fracture, 4f)
{
    public override void Update()
    {
        var count = Towers.Count;
        if (count == 0)
            return;
        var party = Raid.WithSlot(false, true, true);
        var len = party.Length;
        BitMask forbidden = default;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var p = ref party[i];
            ref readonly var statuses = ref p.Item2.Statuses;
            var lenStatuses = statuses.Length;
            for (var j = 0; j < lenStatuses; ++j)
            {
                if (statuses[j].ID is ((uint)SID.HeadOverHeels) or ((uint)SID.HopelessDevotion))
                {
                    forbidden[p.Item1] = true;
                    break;
                }
            }
        }
        var towers = CollectionsMarshal.AsSpan(Towers);
        for (var i = 0; i < count; ++i)
        {
            ref var t = ref towers[i];
            t.ForbiddenSoakers = forbidden;
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 987, NameID = 12685)]
public sealed class M02NHoneyBLovely(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f));
