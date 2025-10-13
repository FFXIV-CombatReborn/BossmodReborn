namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

sealed class HuntersHarvestBait(BossModule module) : Components.GenericBaitAway(module, (uint)AID.HuntersHarvest, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public static readonly AOEShapeCone Cone = new(40f, 105f.Degrees());
    public BitMask Bind;
    private Actor? spreadTarget;
    private readonly M08SHowlingBlade bossmod = (M08SHowlingBlade)module;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.StalkingStoneWind && actor.Role == Role.Tank)
        {
            spreadTarget = actor;
            var party = Raid.WithoutSlot(true, true, true);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                var p = party[i];
                if (p.Role == Role.Tank && p != actor)
                {
                    CurrentBaits.Add(new(bossmod.BossP2()!, p, Cone, WorldState.FutureTime(10.3d)));
                    return;
                }
            }
        }
    }

    public override void Update()
    {
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        var len = baits.Length;
        if (len == 0)
            return;
        ref var b = ref baits[0];
        for (var i = 0; i < 5; ++i)
        {
            var center = ArenaChanges.EndArenaPlatforms[i].Center;
            if (b.Target.Position.InCircle(center, 8f))
            {
                b.CustomRotation = ArenaChanges.PlatformAngles[i];
                return;
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Bind)
        {
            Bind.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.Role == Role.Tank && spreadTarget is Actor t && t != actor && bossmod.BossP2()!.TargetID != actor.InstanceID)
        {
            hints.Add("Provoke for tankbuster!");
        }
    }
}

sealed class HuntersHarvest(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HuntersHarvest, HuntersHarvestBait.Cone);

sealed class GeotemporalBlast(BossModule module) : Components.GenericBaitAway(module, (uint)AID.GeotemporalBlast, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster, centerAtTarget: true)
{
    private readonly AOEShapeCircle circle = new(16f);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.StalkingStoneWind && actor.Role == Role.Tank)
        {
            CurrentBaits.Add(new(Module.PrimaryActor, actor, circle, WorldState.FutureTime(10.3d)));
        }
    }
}

sealed class AerotemporalBlast(BossModule module) : Components.GenericBaitStack(module, (uint)AID.AerotemporalBlast)
{
    private readonly AOEShapeCircle circle = new(6f);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.StalkingStoneWind)
        {
            if (actor.Role == Role.Healer)
            {
                var party = Raid.WithSlot(true, true, true);
                var len = party.Length;
                BitMask forbidden = default;
                for (var i = 0; i < len; ++i)
                {
                    ref var p = ref party[i];
                    if (p.Item2.Role == Role.Tank)
                    {
                        forbidden.Set(p.Item1);
                    }
                }
                CurrentBaits.Add(new(actor, actor, circle, WorldState.FutureTime(10.3d), forbidden));
            }
        }
    }
}
