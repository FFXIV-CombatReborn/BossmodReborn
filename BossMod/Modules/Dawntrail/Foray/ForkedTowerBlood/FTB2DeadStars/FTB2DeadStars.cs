namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB2DeadStars;

sealed class NoxiousNova(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.NoxiousNovaVisual, (uint)AID.NoxiousNova, 0.8d);
sealed class PrimordialChaosRaidwide(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.PrimordialChaosVisual1, (uint)AID.PrimordialChaos, 1.2d);
sealed class SliceNDice(BossModule module) : Components.BaitAwayCast(module, (uint)AID.SliceNDice, new AOEShapeCone(70f, 45f.Degrees()), tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);
sealed class NoisomeNuisanceIceboundBuffoonBlazingBelligerent(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.NoisomeNuisance,
(uint)AID.IceboundBuffoon, (uint)AID.BlazingBelligerent], 6f);
sealed class VengefulBioIIIBlizzardIIIFireIII(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.VengefulBioIII,
(uint)AID.VengefulBlizzardIII, (uint)AID.VengefulFireIII], new AOEShapeCone(60f, 60f.Degrees()));
sealed class DeltaAttack(BossModule module) : Components.CastCounterMulti(module, [(uint)AID.DeltaAttackFirst, (uint)AID.DeltaAttackRepeat]);
sealed class DeltaAttackRaidwide(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.DeltaAttackVisual1, (uint)AID.DeltaAttackFirst, 0.5d);
sealed class Avalaunch(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Avalaunch, (uint)AID.Avalaunch, 8f, 7.8d, 24, 24);
sealed class SelfDestruct(BossModule module) : Components.CastHints(module, [(uint)AID.SelfDestructVisual1, (uint)AID.SelfDestructVisual2], "Enrage!", true);
sealed class GeothermalRupture(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GeothermalRupture, 8f);
sealed class FlameThrower(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.FlameThrowerMarker, (uint)AID.FlameThrower, 5d, 40f, 4f, 24, 24, 1, false);

abstract class Firestrike(BossModule module, uint aidMarker, double delay) : Components.LineStack(module, aidMarker: aidMarker, (uint)AID.Firestrike, delay, 70f, 5, 16, 24);
sealed class Firestrike1(BossModule module) : Firestrike(module, (uint)AID.FirestrikeMarker1, 5.1d);
sealed class Firestrike2(BossModule module) : Firestrike(module, (uint)AID.FirestrikeMarker2, 6.2d);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.Triton, GroupType = BossModuleInfo.GroupType.TheForkedTowerBlood, GroupID = 1018u, NameID = 13737u, PlanLevel = 100, SortOrder = 3, Category = BossModuleInfo.Category.Foray, Expansion = BossModuleInfo.Expansion.Dawntrail)]
public sealed class FTB2DeadStars(WorldState ws, Actor primary) : BossModule(ws, primary, startingArena.Center, startingArena)
{
    public static readonly WPos ArenaCenter = new(-800f, 360f);
    private Actor? _bossPhobos;
    private Actor? _bossNereid;
    public Actor? BossNereid() => _bossNereid;
    private Actor? _bossLiquifiedTriton;
    private Actor? _bossLiquifiedNereid;
    private Actor? _bossFrozenTriton;
    private Actor? _bossFrozenPhobos;
    private Actor? _bossGaseousNereid;
    private Actor? _bossGaseousPhobos;
    public Actor? BossDeadStars;
    public Actor? DeathWall;

    protected override void UpdateModule()
    {
        _bossPhobos ??= GetActor((uint)OID.Phobos);
        _bossNereid ??= GetActor((uint)OID.Nereid);
        _bossLiquifiedTriton ??= GetActor((uint)OID.LiquifiedTriton);
        _bossLiquifiedNereid ??= GetActor((uint)OID.LiquifiedNereid);
        _bossFrozenTriton ??= GetActor((uint)OID.FrozenTriton);
        _bossFrozenPhobos ??= GetActor((uint)OID.FrozenPhobos);
        _bossGaseousNereid ??= GetActor((uint)OID.GaseousNereid);
        _bossGaseousPhobos ??= GetActor((uint)OID.GaseousPhobos);
        BossDeadStars ??= GetActor((uint)OID.DeadStars);
        DeathWall ??= GetActor((uint)OID.Deathwall);
    }

    private static readonly ArenaBoundsCustom startingArena = new([new Polygon(ArenaCenter, 39.5f * CosPI.Pi48th, 48)], [new Rectangle(new(-800f, 400f), 7.5f, 1.25f),
    new Rectangle(new(-800f, 320f), 7.5f, 1.25f)]);
    public static readonly ArenaBoundsCircle DefaultArena = new(30f);
    public static readonly ArenaBoundsCustom FistFightArena = new([new DonutV(ArenaCenter.Quantized(), 12f, 30f, 64)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(BossDeadStars);
        Arena.Actor(_bossNereid);
        Arena.Actor(_bossPhobos);
        Arena.Actor(_bossGaseousPhobos);
        Arena.Actor(_bossFrozenPhobos);
        Arena.Actor(_bossFrozenTriton);
        Arena.Actor(_bossGaseousNereid);
        Arena.Actor(_bossLiquifiedNereid);
        Arena.Actor(_bossLiquifiedTriton);
        Arena.Actor(PrimaryActor);
    }
}
