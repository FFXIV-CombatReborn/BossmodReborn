namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL3Adrammelech;

sealed class HolyIV(BossModule module) : Components.RaidwideCast(module, (uint)AID.HolyIV);
sealed class Flare(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Flare);

abstract class WaterIV(BossModule module, uint aid) : Components.SimpleKnockbacks(module, aid, 12f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            hints.AddForbiddenZone(new SDInvertedCircle(c.Origin, 13f), c.Activation);
        }
    }
}
sealed class WaterIV1(BossModule module) : WaterIV(module, (uint)AID.WaterIV1); // same time as WaterIV2
sealed class WaterIV3(BossModule module) : WaterIV(module, (uint)AID.WaterIV3); // same time as WaterIV4

sealed class BurstIITornado(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BurstII, (uint)AID.Tornado], 6f);
sealed class Shock(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shock, 35f);
sealed class AeroIV(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.AeroIV1, (uint)AID.AeroIV2], new AOEShapeDonut(15f, 30f));
sealed class ThunderIV(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ThunderIV1, (uint)AID.ThunderIV2], 18f);
sealed class WarpedLight(BossModule module) : Components.SimpleChargeAOEGroups(module, [(uint)AID.WarpedLight1, (uint)AID.WarpedLight2,(uint)AID.WarpedLight3,
(uint)AID.WarpedLight4, (uint)AID.WarpedLight5, (uint)AID.WarpedLight6], 1.5f, riskyWithSecondsLeft: 1d);

sealed class Twister(BossModule module) : Components.Voidzone(module, 6.5f, GetTwister, 5f)
{
    private static List<Actor> GetTwister(BossModule module) => module.Enemies((uint)OID.Twister);
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CastrumLacusLitore, GroupID = 735, NameID = 9442)]
public sealed class CLL3Adrammelech(WorldState ws, Actor primary) : BossModule(ws, primary, startingArena.Center, startingArena)
{
    private static readonly WPos arenaCenter = new(80f, -606f);
    private static readonly ArenaBoundsCustom startingArena = new([new Polygon(arenaCenter, 29.5f, 48)], [new Rectangle(new(80f, -575.788f), 20f, 1.25f),
    new Rectangle(new(80f, -636.413f), 20f, 1.25f)]);
    public static readonly ArenaBoundsCustom DefaultArena = new([new Polygon(arenaCenter, 25f, 48)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.ArcaneSphere));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.ArcaneSphere => 1,
                _ => 0
            };
        }
    }
}
