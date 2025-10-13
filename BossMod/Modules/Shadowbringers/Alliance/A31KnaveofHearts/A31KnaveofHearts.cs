namespace BossMod.Shadowbringers.Alliance.A31KnaveofHearts;

sealed class Roar(BossModule module) : Components.RaidwideCast(module, (uint)AID.Roar);

sealed class ColossalImpact(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ColossalImpact1, (uint)AID.ColossalImpact2, (uint)AID.ColossalImpact3,
(uint)AID.ColossalImpact4, (uint)AID.ColossalImpact5, (uint)AID.ColossalImpact6], new AOEShapeRect(61f, 10f))
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(Casters);
        ref readonly var aoe0 = ref aoes[0];
        var rot = aoe0.Rotation + 180f.Degrees();

        var index = 0;
        while (index < count)
        {
            ref var aoe = ref aoes[index];
            if (aoe.Rotation.AlmostEqual(rot, Angle.DegToRad))
            {
                break;
            }
            ++index;
        }
        return aoes[..index];
    }
}

sealed class MagicArtilleryBeta(BossModule module) : Components.BaitAwayCast(module, (uint)AID.MagicArtilleryBeta, 3f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);
sealed class MagicArtilleryAlpha(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.MagicArtilleryAlpha, 5f);
sealed class LightLeap(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightLeap, 28f);
sealed class MagicBarrage(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagicBarrage, new AOEShapeRect(61, 2.5f), 6);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9955, SortOrder = 1)]
public sealed class A31KnaveofHearts(WorldState ws, Actor primary) : BossModule(ws, primary, arenaCenter, new ArenaBoundsSquare(30f))
{
    private static readonly WPos arenaCenter = new(-800f, -724.40625f);
    public static readonly Square[] BaseSquare = [new Square(arenaCenter, 30.5f)];
    public static readonly ArenaBoundsSquare DefaultArena = new(30f);
}
