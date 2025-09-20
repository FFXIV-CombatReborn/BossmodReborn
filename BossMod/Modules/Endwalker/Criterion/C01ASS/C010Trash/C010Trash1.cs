namespace BossMod.Endwalker.VariantCriterion.C01ASS.C010Trash1;

public enum OID : uint
{
    NBelladonna = 0x3AD5, // R4.0
    SBelladonna = 0x3ADE, // R4.0
    NDryad = 0x3AD1, // R3.0
    NOdqan = 0x3AD2, // R1.05
    SDryad = 0x3ADA, // R3.0
    SOdqan = 0x3ADB, // R1.05
    NKaluk = 0x3AD6, // R2.8
    SKaluk = 0x3ADF, // R2.8
    NUdumbara = 0x3AD3, // R4.0
    NSapria = 0x3AD4, // R1.44
    SUdumbara = 0x3ADC, // R4.0
    SSapria = 0x3ADD, // R1.44
}

public enum AID : uint
{
    AutoAttack = 31320, // NBelladonna/SBelladonna/NDryad/NOdqan/SDryad/SOdqan/NBKaluk/SKaluk/NUdumbara/NSapria/SUdumbara/SSapria->player, no cast, single-target

    // Belladonna
    NAtropineSpore = 31072, // NBelladonna->self, 4.0s cast, range 10-40 donut aoe
    NFrondAffront = 31073, // NBelladonna->self, 3.0s cast, gaze
    NDeracinator = 31074, // NBBelladonna->player, 4.0s cast, single-target tankbuster
    SAtropineSpore = 31096, // SBelladonna->self, 4.0s cast, range 10-40 donut aoe
    SFrondAffront = 31097, // SBelladonna->self, 3.0s cast, gaze
    SDeracinator = 31098, // SBelladonna->player, 4.0s cast, single-target tankbuster

    // Odqan, Dryad
    NArborealStorm = 31063, // NDryad->self, 5.0s cast, range 12 circle
    NAcornBomb = 31064, // NDryad->location, 3.0s cast, range 6 circle
    NGelidGale = 31065, // NOdqan->location, 3.0s cast, range 6 circle
    NUproot = 31066, // NOdqan->self, 3.0s cast, range 6 circle
    SArborealStorm = 31087, // SDryad->self, 5.0s cast, range 12 circle
    SAcornBomb = 31088, // SDryad->location, 3.0s cast, range 6 circle
    SGelidGale = 31089, // SOdqan->location, 3.0s cast, range 6 circle
    SUproot = 31090, // SOdqan->self, 3.0s cast, range 6 circle

    // Kaluk
    NRightSweep = 31075, // NKaluk->self, 4.0s cast, range 30 210-degree cone aoe
    NLeftSweep = 31076, // NKaluk->self, 4.0s cast, range 30 210-degree cone aoe
    NCreepingIvy = 31077, // NKaluk->self, 3.0s cast, range 10 90-degree cone aoe
    SRightSweep = 31099, // SKaluk->self, 4.0s cast, range 30 210-degree cone aoe
    SLeftSweep = 31100, // SKaluk->self, 4.0s cast, range 30 210-degree cone aoe
    SCreepingIvy = 31101, // SKaluk->self, 3.0s cast, range 10 90-degree cone aoe

    // Udumbara, Sapria
    NHoneyedLeft = 31067, // NUdumbara->self, 4.0s cast, range 30 180-degree cone
    NHoneyedRight = 31068, // NUdumbara->self, 4.0s cast, range 30 180-degree cone
    NHoneyedFront = 31069, // NUdumbara->self, 4.0s cast, range 30 120-degree cone
    NBloodyCaress = 31071, // NSapria->self, 3.0s cast, range 12 120-degree cone
    SHoneyedLeft = 31091, // SUdumbara->self, 4.0s cast, range 30 180-degree cone
    SHoneyedRight = 31092, // SUdumbara->self, 4.0s cast, range 30 180-degree cone
    SHoneyedFront = 31093, // SUdumbara->self, 4.0s cast, range 30 120-degree cone
    SBloodyCaress = 31095 // SSapria->self, 3.0s cast, range 12 120-degree cone
}

abstract class AtropineSpore(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeDonut(10f, 40f));
sealed class NAtropineSpore(BossModule module) : AtropineSpore(module, (uint)AID.NAtropineSpore);
sealed class SAtropineSpore(BossModule module) : AtropineSpore(module, (uint)AID.SAtropineSpore);

abstract class FrondAffront(BossModule module, uint aid) : Components.CastGaze(module, aid);
sealed class NFrondAffront(BossModule module) : FrondAffront(module, (uint)AID.NFrondAffront);
sealed class SFrondAffront(BossModule module) : FrondAffront(module, (uint)AID.SFrondAffront);

abstract class Deracinator(BossModule module, uint aid) : Components.SingleTargetCast(module, aid);
sealed class NDeracinator(BossModule module) : Deracinator(module, (uint)AID.NDeracinator);
sealed class SDeracinator(BossModule module) : Deracinator(module, (uint)AID.SDeracinator);

abstract class ArborealStorm(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 12f);
sealed class NArborealStorm(BossModule module) : ArborealStorm(module, (uint)AID.NArborealStorm);
sealed class SArborealStorm(BossModule module) : ArborealStorm(module, (uint)AID.SArborealStorm);

abstract class AcornBomb(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 6f);
sealed class NAcornBomb(BossModule module) : AcornBomb(module, (uint)AID.NAcornBomb);
sealed class SAcornBomb(BossModule module) : AcornBomb(module, (uint)AID.SAcornBomb);

abstract class GelidGale(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 6f);
sealed class NGelidGale(BossModule module) : GelidGale(module, (uint)AID.NGelidGale);
sealed class SGelidGale(BossModule module) : GelidGale(module, (uint)AID.SGelidGale);

abstract class Uproot(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 6f);
sealed class NUproot(BossModule module) : Uproot(module, (uint)AID.NUproot);
sealed class SUproot(BossModule module) : Uproot(module, (uint)AID.SUproot);

abstract class Sweep(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(30f, 105f.Degrees()));
sealed class NRightSweep(BossModule module) : Sweep(module, (uint)AID.NRightSweep);
sealed class SRightSweep(BossModule module) : Sweep(module, (uint)AID.SRightSweep);
sealed class NLeftSweep(BossModule module) : Sweep(module, (uint)AID.NLeftSweep);
sealed class SLeftSweep(BossModule module) : Sweep(module, (uint)AID.SLeftSweep);

abstract class CreepingIvy(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(10f, 45f.Degrees()));
sealed class NCreepingIvy(BossModule module) : CreepingIvy(module, (uint)AID.NCreepingIvy);
sealed class SCreepingIvy(BossModule module) : CreepingIvy(module, (uint)AID.SCreepingIvy);

abstract class Honeyed(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(30f, 90f.Degrees()));
sealed class NHoneyedLeft(BossModule module) : Honeyed(module, (uint)AID.NHoneyedLeft);
sealed class SHoneyedLeft(BossModule module) : Honeyed(module, (uint)AID.SHoneyedLeft);
sealed class NHoneyedRight(BossModule module) : Honeyed(module, (uint)AID.NHoneyedRight);
sealed class SHoneyedRight(BossModule module) : Honeyed(module, (uint)AID.SHoneyedRight);

abstract class HoneyedFront(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(30f, 60f.Degrees()));
sealed class NHoneyedFront(BossModule module) : HoneyedFront(module, (uint)AID.NHoneyedFront);
sealed class SHoneyedFront(BossModule module) : HoneyedFront(module, (uint)AID.SHoneyedFront);

abstract class BloodyCaress(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(12f, 60f.Degrees()));
sealed class NBloodyCaress(BossModule module) : BloodyCaress(module, (uint)AID.NBloodyCaress);
sealed class SBloodyCaress(BossModule module) : BloodyCaress(module, (uint)AID.SBloodyCaress);

abstract class C010Trash1States : StateMachineBuilder
{
    public C010Trash1States(BossModule module, bool savage) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<NAtropineSpore>(!savage)
            .ActivateOnEnter<NFrondAffront>(!savage)
            .ActivateOnEnter<NDeracinator>(!savage)
            .ActivateOnEnter<SAtropineSpore>(savage)
            .ActivateOnEnter<SFrondAffront>(savage)
            .ActivateOnEnter<SDeracinator>(savage)
            .ActivateOnEnter<NArborealStorm>(!savage)
            .ActivateOnEnter<NAcornBomb>(!savage)
            .ActivateOnEnter<NGelidGale>(!savage)
            .ActivateOnEnter<NUproot>(!savage)
            .ActivateOnEnter<SArborealStorm>(savage)
            .ActivateOnEnter<SAcornBomb>(savage)
            .ActivateOnEnter<SGelidGale>(savage)
            .ActivateOnEnter<SUproot>(savage)
            .ActivateOnEnter<NRightSweep>(!savage)
            .ActivateOnEnter<NLeftSweep>(!savage)
            .ActivateOnEnter<NCreepingIvy>(!savage)
            .ActivateOnEnter<SRightSweep>(savage)
            .ActivateOnEnter<SLeftSweep>(savage)
            .ActivateOnEnter<SCreepingIvy>(savage)
            .ActivateOnEnter<NHoneyedLeft>(!savage)
            .ActivateOnEnter<NHoneyedRight>(!savage)
            .ActivateOnEnter<NHoneyedFront>(!savage)
            .ActivateOnEnter<NBloodyCaress>(!savage)
            .ActivateOnEnter<SHoneyedLeft>(savage)
            .ActivateOnEnter<SHoneyedRight>(savage)
            .ActivateOnEnter<SHoneyedFront>(savage)
            .ActivateOnEnter<SBloodyCaress>(savage)
            .Raw.Update = () => AllDeadOrDestroyed(savage ? Trash1Arena.TrashSavage : Trash1Arena.TrashNormal);
    }
}
sealed class C010NTrash1States(BossModule module) : C010Trash1States(module, false);
sealed class C010STrash1States(BossModule module) : C010Trash1States(module, true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.NUdumbara, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 878, NameID = 11511, SortOrder = 1)]
public sealed class C010NTrash1(WorldState ws, Actor primary) : Trash1Arena(ws, primary, false);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.SUdumbara, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 879, NameID = 11511, SortOrder = 1)]
public sealed class C010STrash1(WorldState ws, Actor primary) : Trash1Arena(ws, primary, true);

public abstract class Trash1Arena(WorldState ws, Actor primary, bool savage) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] verticesExterior = [new(-338.64f, -114.93f), new(-331.43f, -114.84f), new(-330.79f, -114.66f), new(-330.34f, -114.12f), new(-330.20f, -113.44f),
    new(-329.88f, -112.87f), new(-328.55f, -111.45f), new(-328.17f, -110.88f), new(-328.50f, -108.78f), new(-328.39f, -108.20f),
    new(-327.47f, -105.66f), new(-327.21f, -105.07f), new(-326.02f, -103.39f), new(-325.55f, -103.14f), new(-324.21f, -103.10f),
    new(-323.77f, -102.54f), new(-323.88f, -102.03f), new(-324.29f, -101.48f), new(-324.88f, -100.34f), new(-326.18f, -98.55f),
    new(-326.38f, -97.86f), new(-326.03f, -97.39f), new(-322.98f, -95.10f), new(-322.43f, -94.98f), new(-321.86f, -95.21f),
    new(-321.20f, -95.39f), new(-320.75f, -95.70f), new(-319.92f, -96.75f), new(-319.32f, -97.95f), new(-318.89f, -98.49f),
    new(-316.77f, -99.07f), new(-314.39f, -100.19f), new(-314.01f, -101.55f), new(-312.25f, -100.49f), new(-311.06f, -99.91f),
    new(-310.47f, -99.54f), new(-309.90f, -99.44f), new(-309.25f, -99.19f), new(-308.66f, -99.20f), new(-307.55f, -99.92f),
    new(-306.85f, -99.81f), new(-306.41f, -99.53f), new(-305.73f, -99.32f), new(-304.32f, -99.05f), new(-302.43f, -99.63f),
    new(-299.76f, -99.84f), new(-298.45f, -100.19f), new(-297.83f, -100.45f), new(-297.42f, -99.95f), new(-297.13f, -99.40f),
    new(-296.70f, -98.90f), new(-296.22f, -98.47f), new(-294.83f, -98.30f), new(-292.75f, -98.37f), new(-292.27f, -98.65f),
    new(-291.45f, -99.71f), new(-290.99f, -99.45f), new(-290.34f, -98.68f), new(-289.79f, -98.50f), new(-287.20f, -98.15f),
    new(-286.57f, -98.14f), new(-284.60f, -98.38f), new(-284.05f, -98.65f), new(-283.49f, -99.01f), new(-278.58f, -99.08f),
    new(-276.72f, -99.55f), new(-276.15f, -99.18f), new(-275.83f, -98.53f), new(-275.36f, -97.98f), new(-274.73f, -97.89f),
    new(-274.02f, -97.88f), new(-273.38f, -98.06f), new(-271.54f, -98.89f), new(-270.91f, -99.08f), new(-270.25f, -99.19f),
    new(-270.09f, -98.57f), new(-270.25f, -97.87f), new(-270.07f, -97.31f), new(-269.16f, -96.33f), new(-266.03f, -95.21f),
    new(-264.85f, -94.45f), new(-264.98f, -93.29f), new(-264.83f, -92.73f), new(-264.82f, -92.04f), new(-265.16f, -91.49f),
    new(-264.83f, -90.29f), new(-265.39f, -89.85f), new(-266.62f, -89.49f), new(-267.70f, -88.59f), new(-267.81f, -87.38f),
    new(-268.11f, -86.82f), new(-268.63f, -86.39f), new(-270.25f, -85.36f), new(-270.68f, -84.99f), new(-270.96f, -84.43f),
    new(-271.57f, -84.08f), new(-272.98f, -84.18f), new(-274.67f, -83.32f), new(-275.02f, -82.88f), new(-276.33f, -80.55f),
    new(-276.87f, -80.06f), new(-277.54f, -79.82f), new(-278.03f, -79.46f), new(-279.73f, -77.49f), new(-279.99f, -76.95f),
    new(-280.36f, -75.56f), new(-281.26f, -74.50f), new(-281.68f, -74.22f), new(-281.99f, -73.62f), new(-282.00f, -72.32f),
    new(-281.81f, -71.71f), new(-281.87f, -71.00f), new(-282.15f, -70.58f), new(-285.51f, -70.58f), new(-285.77f, -71.15f),
    new(-285.78f, -71.75f), new(-285.94f, -72.32f), new(-290.59f, -72.57f), new(-291.06f, -72.33f), new(-291.12f, -70.82f),
    new(-291.81f, -70.57f), new(-303.97f, -70.60f), new(-304.30f, -71.15f), new(-304.47f, -72.33f), new(-309.19f, -72.57f),
    new(-309.63f, -72.14f), new(-309.63f, -70.46f), new(-311.23f, -70.17f), new(-319.45f, -70.20f), new(-319.45f, -72.23f),
    new(-319.83f, -72.58f), new(-324.21f, -72.58f), new(-324.61f, -72.27f), new(-324.70f, -67.92f), new(-324.38f, -67.43f),
    new(-321.63f, -67.41f), new(-321.36f, -66.99f), new(-321.31f, -62.98f), new(-322.96f, -62.57f), new(-323.60f, -62.69f),
    new(-324.22f, -62.60f), new(-324.56f, -62.23f), new(-324.57f, -59.91f), new(-325.15f, -59.63f), new(-328.44f, -59.90f),
    new(-329.04f, -59.87f), new(-329.66f, -59.62f), new(-340.52f, -60.11f), new(-341.80f, -59.68f), new(-344.68f, -60.89f),
    new(-345.06f, -61.24f), new(-345.46f, -61.85f), new(-345.91f, -62.41f), new(-347.85f, -62.57f), new(-347.84f, -63.24f),
    new(-347.31f, -63.70f), new(-344.32f, -64.86f), new(-343.64f, -64.90f), new(-342.48f, -65.61f), new(-342.40f, -66.13f),
    new(-342.58f, -69.30f), new(-342.94f, -70.53f), new(-342.77f, -71.02f), new(-342.49f, -71.64f), new(-342.34f, -73.00f),
    new(-341.63f, -73.80f), new(-338.20f, -78.82f), new(-337.75f, -79.29f), new(-337.57f, -79.93f), new(-337.59f, -80.95f),
    new(-337.85f, -81.49f), new(-338.67f, -82.45f), new(-339.22f, -82.87f), new(-339.87f, -82.86f), new(-341.05f, -82.72f),
    new(-341.68f, -82.52f), new(-342.42f, -81.48f), new(-343.08f, -81.56f), new(-343.70f, -81.81f), new(-344.22f, -81.83f),
    new(-344.56f, -81.36f), new(-345.57f, -79.60f), new(-346.19f, -79.39f), new(-346.82f, -79.43f), new(-347.43f, -79.34f),
    new(-349.98f, -78.72f), new(-350.36f, -79.31f), new(-350.64f, -79.88f), new(-351.49f, -81.06f), new(-352.02f, -81.50f),
    new(-354.08f, -81.33f), new(-354.61f, -81.00f), new(-355.17f, -80.57f), new(-355.64f, -80.12f), new(-358.60f, -75.73f),
    new(-358.76f, -75.17f), new(-358.25f, -73.93f), new(-357.54f, -72.84f), new(-357.05f, -72.53f), new(-356.51f, -72.05f),
    new(-356.64f, -71.37f), new(-356.97f, -70.91f), new(-357.58f, -70.55f), new(-358.12f, -70.46f), new(-359.37f, -70.44f),
    new(-360.11f, -70.52f), new(-360.81f, -71.63f), new(-361.33f, -71.99f), new(-361.94f, -72.32f), new(-362.55f, -72.58f),
    new(-364.52f, -72.58f), new(-366.62f, -72.94f), new(-367.12f, -72.49f), new(-367.30f, -70.98f), new(-367.57f, -70.53f),
    new(-374.13f, -70.46f), new(-374.76f, -70.74f), new(-375.31f, -71.18f), new(-375.71f, -71.75f), new(-376.57f, -73.57f),
    new(-376.98f, -74.08f), new(-377.55f, -74.48f), new(-378.20f, -74.79f), new(-379.63f, -75.12f), new(-380.21f, -74.79f),
    new(-382.21f, -73.28f), new(-382.72f, -73.20f), new(-384.85f, -73.39f), new(-385.87f, -72.49f), new(-386.04f, -70.58f),
    new(-389.27f, -70.59f), new(-389.64f, -70.92f), new(-390.93f, -72.47f), new(-391.25f, -73.04f), new(-392.38f, -73.75f),
    new(-392.84f, -74.16f), new(-394.68f, -74.59f), new(-394.90f, -75.23f), new(-394.56f, -75.82f), new(-394.29f, -76.45f),
    new(-393.72f, -78.34f), new(-393.88f, -78.85f), new(-393.95f, -79.52f), new(-394.32f, -79.98f), new(-396.28f, -81.70f),
    new(-396.89f, -81.97f), new(-397.28f, -82.31f), new(-396.73f, -84.07f), new(-397.09f, -84.48f), new(-398.64f, -85.69f),
    new(-402.64f, -87.72f), new(-403.17f, -87.84f), new(-403.77f, -87.65f), new(-404.29f, -88.11f), new(-405.17f, -90.08f),
    new(-405.48f, -90.64f), new(-405.66f, -91.29f), new(-405.36f, -91.92f), new(-403.97f, -93.19f), new(-402.97f, -94.82f),
    new(-402.20f, -95.92f), new(-401.43f, -97.66f), new(-401.40f, -98.30f), new(-401.44f, -98.95f), new(-401.41f, -99.70f),
    new(-400.88f, -99.77f), new(-400.28f, -99.93f), new(-399.78f, -100.35f), new(-399.11f, -100.33f), new(-396.74f, -98.71f),
    new(-396.14f, -98.77f), new(-395.52f, -98.95f), new(-392.93f, -100.96f), new(-392.31f, -101.29f), new(-391.27f, -100.63f),
    new(-390.02f, -100.73f), new(-389.42f, -100.86f), new(-389.03f, -101.31f), new(-387.61f, -103.32f), new(-383.56f, -100.70f),
    new(-383.02f, -100.39f), new(-381.78f, -100.48f), new(-380.58f, -101.18f), new(-380.33f, -101.67f), new(-379.99f, -102.87f),
    new(-379.58f, -103.47f), new(-378.60f, -102.20f), new(-378.09f, -101.82f), new(-377.62f, -101.62f), new(-376.56f, -100.97f),
    new(-376.00f, -100.98f), new(-374.24f, -101.97f), new(-373.89f, -102.40f), new(-373.19f, -104.87f), new(-372.88f, -104.25f),
    new(-370.92f, -102.02f), new(-368.71f, -100.31f), new(-365.76f, -98.49f), new(-365.21f, -98.37f), new(-364.56f, -98.60f),
    new(-363.89f, -98.75f), new(-361.17f, -98.05f), new(-360.54f, -98.00f), new(-357.32f, -99.72f), new(-356.63f, -99.98f),
    new(-356.00f, -100.14f), new(-355.46f, -99.69f), new(-354.63f, -97.62f), new(-354.28f, -97.10f), new(-353.68f, -97.00f),
    new(-353.07f, -96.71f), new(-350.51f, -93.89f), new(-350.21f, -93.49f), new(-349.70f, -92.95f), new(-349.05f, -92.63f),
    new(-348.52f, -92.90f), new(-345.38f, -95.07f), new(-344.90f, -95.57f), new(-345.11f, -96.89f), new(-345.33f, -97.42f),
    new(-345.71f, -97.95f), new(-347.26f, -99.14f), new(-348.65f, -100.50f), new(-348.94f, -101.11f), new(-348.54f, -102.42f),
    new(-346.93f, -103.51f), new(-342.76f, -107.11f), new(-342.61f, -107.64f), new(-342.64f, -108.28f), new(-342.39f, -108.73f),
    new(-341.76f, -109.13f), new(-341.39f, -109.61f), new(-340.12f, -113.61f), new(-339.75f, -114.21f), new(-339.29f, -114.67f),
    new(-338.64f, -114.93f)];
    private static readonly WPos[] verticesHole1 = [new(-297.76f, -91.76f), new(-297.22f, -91.67f), new(-295.75f, -89.65f), new(-295.95f, -86.31f), new(-294.77f, -85.82f),
    new(-294.32f, -85.51f), new(-294.11f, -85.05f), new(-293.90f, -81.64f), new(-293.92f, -81.02f), new(-294.05f, -80.41f),
    new(-294.29f, -79.97f), new(-294.98f, -79.76f), new(-296.96f, -79.33f), new(-299.04f, -79.21f), new(-301.01f, -79.52f),
    new(-301.39f, -79.89f), new(-301.68f, -80.47f), new(-302.67f, -82.81f), new(-303.06f, -83.35f), new(-303.39f, -83.90f),
    new(-303.55f, -84.60f), new(-303.48f, -85.27f), new(-303.77f, -85.77f), new(-303.98f, -86.27f), new(-304.05f, -86.78f),
    new(-304.02f, -87.30f), new(-303.61f, -87.88f), new(-301.26f, -90.56f), new(-300.68f, -90.89f), new(-300.31f, -91.24f),
    new(-299.87f, -91.47f), new(-298.70f, -91.82f)];
    private static readonly WPos[] verticesHole2 = [new(-377.26f, -93.01f), new(-376.64f, -92.84f), new(-375.96f, -92.78f), new(-375.36f, -92.50f), new(-372.55f, -91.71f),
    new(-371.89f, -91.58f), new(-371.28f, -91.27f), new(-370.76f, -91.21f), new(-369.55f, -90.95f), new(-369.04f, -90.47f),
    new(-367.94f, -89.11f), new(-367.67f, -88.68f), new(-367.57f, -88.18f), new(-367.87f, -87.63f), new(-367.53f, -87.02f),
    new(-367.03f, -86.46f), new(-366.73f, -86.02f), new(-366.51f, -85.57f), new(-366.66f, -83.63f), new(-367.05f, -83.22f),
    new(-367.60f, -82.82f), new(-368.84f, -82.16f), new(-369.49f, -81.90f), new(-370.07f, -82.22f), new(-370.50f, -82.66f),
    new(-371.50f, -83.50f), new(-371.91f, -83.99f), new(-372.96f, -85.48f), new(-373.59f, -85.62f), new(-374.33f, -85.66f),
    new(-376.05f, -86.08f), new(-376.96f, -86.51f), new(-377.54f, -86.72f), new(-378.09f, -87.00f), new(-378.60f, -87.36f),
    new(-379.05f, -87.78f), new(-379.25f, -88.88f), new(-379.08f, -91.38f), new(-378.83f, -91.83f), new(-377.98f, -92.76f),
    new(-377.52f, -93.02f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(verticesExterior)], [new PolygonCustom(verticesHole1), new PolygonCustom(verticesHole2)]);

    public static readonly uint[] TrashNormal = [(uint)OID.NKaluk, (uint)OID.NOdqan, (uint)OID.NUdumbara, (uint)OID.NDryad, (uint)OID.NBelladonna, (uint)OID.NSapria];
    public static readonly uint[] TrashSavage = [(uint)OID.SKaluk, (uint)OID.SOdqan, (uint)OID.SUdumbara, (uint)OID.SDryad, (uint)OID.SBelladonna, (uint)OID.SSapria];

    protected override bool CheckPull() => IsAnyActorInCombat(savage ? TrashSavage : TrashNormal);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, savage ? TrashSavage : TrashNormal);
    }
}
