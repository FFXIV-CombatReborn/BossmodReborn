namespace BossMod.Dawntrail.Trial.T01Valigarmanda;

sealed class SlitheringStrike(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SlitheringStrike, new AOEShapeCone(24f, 90f.Degrees()));
sealed class SkyruinHailOfFeathersDisasterZoneRuinForetold(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.Skyruin1, (uint)AID.Skyruin2, (uint)AID.HailOfFeathers,
(uint)AID.DisasterZone1, (uint)AID.DisasterZone2, (uint)AID.RuinForetold]);

abstract class CalamitousCry(BossModule module, uint aid) : Components.LineStack(module, aidMarker: aid, (uint)AID.CalamitousCry, 5d, 60f, 3f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.LimitBreakVisual4) // not sure if line stack gets cancelled when limit break phase ends, just a safety feature
        {
            CurrentBaits.Clear();
        }
    }
}

sealed class CalamitousCry1(BossModule module) : CalamitousCry(module, (uint)AID.CalamitousCryMarker1);
sealed class CalamitousCry2(BossModule module) : CalamitousCry(module, (uint)AID.CalamitousCryMarker2);

sealed class CalamitousEcho(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CalamitousEcho, new AOEShapeCone(40f, 10f.Degrees()));

abstract class Tulidisaster(BossModule module, uint aid, double delay) : Components.RaidwideCastDelay(module, (uint)AID.TulidisasterVisual, aid, delay);
sealed class Tulidisaster1(BossModule module) : Tulidisaster(module, (uint)AID.Tulidisaster1, 3.1d);
sealed class Tulidisaster2(BossModule module) : Tulidisaster(module, (uint)AID.Tulidisaster2, 11.6d);
sealed class Tulidisaster3(BossModule module) : Tulidisaster(module, (uint)AID.Tulidisaster3, 19.6d);

sealed class Eruption(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Eruption, 6f);
sealed class IceTalon(BossModule module) : Components.BaitAwayIcon(module, 6f, (uint)IconID.Tankbuster, (uint)AID.IceTalon, 5d, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 832, NameID = 12854)]
public sealed class T01Valigarmanda(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsRect(20f, 15f))
{
    private static readonly uint[] objects = [(uint)OID.IceBoulder, (uint)OID.FlameKissedBeacon, (uint)OID.GlacialBeacon, (uint)OID.ThunderousBeacon];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, objects, Colors.Object);
    }
}
