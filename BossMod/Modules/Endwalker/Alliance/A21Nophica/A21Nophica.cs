namespace BossMod.Endwalker.Alliance.A21Nophica;

sealed class ArenaBounds(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(28f, 34f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x39)
        {
            switch (state)
            {
                case 0x02000200u:
                    _aoe = [new(donut, Arena.Center, default, WorldState.FutureTime(5.8d))];
                    break;
                case 0x00200010u:
                case 0x00020001u:
                    Arena.Bounds = A21Nophica.SmallerBounds;
                    _aoe = [];
                    break;
                case 0x00080004u:
                case 0x00400004u:
                    Arena.Bounds = A21Nophica.DefaultBounds;
                    break;
            }
        }
    }
}

sealed class FloralHaze(BossModule module) : Components.StatusDrivenForcedMarch(module, 2, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace, activationLimit: 8);
sealed class SummerShade(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SummerShade, new AOEShapeDonut(12f, 40f));
sealed class SpringFlowers(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpringFlowers, 12f);
sealed class ReapersGale(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ReapersGaleAOE, new AOEShapeRect(72f, 4f), 9);
sealed class Landwaker(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LandwakerAOE, 10f);
sealed class Furrow(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Furrow, 6f, 8);
sealed class HeavensEarth(BossModule module) : Components.BaitAwayCast(module, (uint)AID.HeavensEarthAOE, 5f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 911u, NameID = 12065u, PlanLevel = 90)]
public sealed class A21Nophica(WorldState ws, Actor primary) : BossModule(ws, primary, new(default, -238f), DefaultBounds)
{
    public static readonly ArenaBoundsCircle DefaultBounds = new(34f);
    public static readonly ArenaBoundsCircle SmallerBounds = new(28f);
}