namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

sealed class EvilSeedBait(BossModule module) : BossComponent(module)
{
    public BitMask Baiters;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var p in Raid.WithSlot(false, false, true).IncludedInMask(Baiters).Actors())
            Arena.ZoneCircleOutline(p.Position, 5f);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.EvilSeed)
        {
            Baiters.Set(Raid.FindSlot(actor.InstanceID));
        }
    }
}

sealed class EvilSeedAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EvilSeedAOE, 5f);

sealed class EvilSeedVoidzone(BossModule module) : Components.Voidzone(module, 5f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.EvilSeed);
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

sealed class ThornyVine(BossModule module) : Components.Chains(module, (uint)TetherID.ThornyVine, default, 25f)
{
    public BitMask Targets;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.ThornyVineBait)
        {
            Targets.Set(Raid.FindSlot(actor.InstanceID));
        }
    }
}
