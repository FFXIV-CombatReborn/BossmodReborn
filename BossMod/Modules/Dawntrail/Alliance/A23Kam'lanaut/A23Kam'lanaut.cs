namespace BossMod.Dawntrail.Alliance.A23Kamlanaut;

sealed class ElementalBladeWide(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.IceBladeWide, (uint)AID.LightningBladeWide, (uint)AID.FireBladeWide,
(uint)AID.EarthBladeWide, (uint)AID.WaterBladeWide, (uint)AID.WindBladeWide], new AOEShapeRect(80f, 10f), riskyWithSecondsLeft: 6d);
sealed class ElementalBladeNarrow(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.IceBladeNarrow, (uint)AID.LightningBladeNarrow, (uint)AID.FireBladeNarrow,
(uint)AID.EarthBladeNarrow, (uint)AID.WaterBladeNarrow, (uint)AID.WindBladeNarrow], new AOEShapeRect(80f, 2.5f), riskyWithSecondsLeft: 6d);
sealed class ElementalResonance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElementalResonance, 18f, riskyWithSecondsLeft: 6d);
sealed class SublimeElementsWide(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.SublimeIceWide, (uint)AID.SublimeLightningWide, (uint)AID.SublimeFireWide,
(uint)AID.SublimeEarthWide, (uint)AID.SublimeWaterWide, (uint)AID.SublimeWindWide], new AOEShapeCone(40f, 50f.Degrees()), riskyWithSecondsLeft: 6d);
sealed class SublimeElementsNarrow(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.SublimeIceNarrow, (uint)AID.SublimeLightningNarrow, (uint)AID.SublimeFireNarrow,
(uint)AID.SublimeEarthNarrow, (uint)AID.SublimeWaterNarrow, (uint)AID.SublimeWindNarrow], new AOEShapeCone(40f, 10f.Degrees()), riskyWithSecondsLeft: 6d);
sealed class EmpyrealBanishIV(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.EmpyrealBanishIV, 5f, PartyState.MaxAllianceSize, PartyState.MaxAllianceSize);
sealed class EmpyrealBanishIII(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.EmpyrealBanishIII, 5f);
sealed class GreatWheelCircle(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.GreatWheelCircle1, (uint)AID.GreatWheelCircle2,
(uint)AID.GreatWheelCircle3, (uint)AID.GreatWheelCircle4], 10f);
sealed class GreatWheelCone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GreatWheelCone, new AOEShapeCone(80f, 90f.Degrees()));
sealed class LightBladeIllumedEstoc(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.IllumedEstoc, (uint)AID.LightBlade], new AOEShapeRect(120f, 6.5f), riskyWithSecondsLeft: 3.5d)
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            return [];
        }
        if (count != 2)
        {
            return base.ActiveAOEs(slot, actor);
        }
        return CollectionsMarshal.AsSpan(Casters);
    }
}

sealed class TranscendentUnion(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.TranscendentUnionVisual, (uint)AID.TranscendentUnion, 6.6d, "Raidwide x7");
sealed class EnspiritedSwordplayShockwave(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.EnspiritedSwordplay, (uint)AID.Shockwave]);

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.Kamlanaut, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1058u, NameID = 14043u, Category = BossModuleInfo.Category.Alliance, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 6)]
public sealed class A23Kamlanaut(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, P1Arena)
{
    public static readonly WPos ArenaCenter = new(-200f, 150f);
    private static readonly Polygon[] p1Circle = [new(ArenaCenter, 29.5f, 128)]; // arena circle actually got 512 vertices, but 128 is a good enough approximation for this use case
    private static readonly Polygon[] p2Circle = [new(ArenaCenter, 20f, 128)];
    private static readonly Polygon[] voidzone = [new(ArenaCenter, 5f, 64)];
    public static readonly ArenaBoundsCustom P1Arena = new(p1Circle);
    public static readonly ArenaBoundsCustom P2Arena = new(p2Circle);
    public static readonly ArenaBoundsCustom P1ArenaDonut = new(p1Circle, voidzone);

    private static readonly Rectangle[] bridges = GenerateBridges();
    private static Rectangle[] GenerateBridges()
    {
        var northCenter = new WDir(default, -20f);
        var rects = new Rectangle[3];
        var a120 = 120f.Degrees();
        for (var i = 0; i < 3; ++i)
        {
            var angle = a120 * i;
            rects[i] = new(ArenaCenter + northCenter.Rotate(angle), 5f, 20f, angle);
        }
        return rects;
    }

    private static readonly Shape[] p2ArenaShapes = [.. p2Circle, .. bridges];
    public static readonly ArenaBoundsCustom P2ArenaWithBridges = new(p2ArenaShapes, ScaleFactor: 1.15f);
    public static readonly AOEShapeCustom P1p2transition = new(p1Circle, p2ArenaShapes);
    public static readonly ArenaBoundsCustom P2ArenaWithBridgesDonut = new(p2ArenaShapes, voidzone, ScaleFactor: 1.15f);
}
