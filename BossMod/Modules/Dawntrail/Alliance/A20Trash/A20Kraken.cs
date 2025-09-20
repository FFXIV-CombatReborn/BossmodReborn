namespace BossMod.Dawntrail.Alliance.A20Kraken;

public enum OID : uint
{
    Kraken = 0x4885, // R3.6
    Clipper = 0x4888, // R1.12
    Acrophies = 0x488A, // R0.64
    LightElemental = 0x4887, // R1.28
    GreaterPugil = 0x4886, // R2.08
    Banshee = 0x4889, // R1.62
    GiantAscetic = 0x488D, // R2.47
    Alkyoneus = 0x488B, // R3.61
    GiantRanger = 0x488C // R2.47
}

public enum AID : uint
{
    AutoAttack1 = 870, // Clipper/GreaterPugil/GiantRanger->player, no cast, single-target
    AutoAttack2 = 872, // Acrophies/LightElemental/Banshee/Kraken/Alkyoneus/GiantAscetic->player, no cast, single-target

    CrossAttack = 43560, // Kraken->player, no cast, single-target
    SmallClaw = 43563, // Clipper->player, no cast, single-target
    DigestiveFluid = 43565, // Acrophies->player, no cast, single-target
    Banishga = 43562, // LightElemental->location, 3.0s cast, range 6 circle
    Sucker = 43791, // Kraken->self, 5.0s cast, range 30 circle, pull 15 between centers
    Flood = 43792, // Kraken->self, 1.5s cast, range 8 circle
    ParalyzeIII = 43570, // Banshee->location, 6.0s cast, range 6 circle
    ImpactRoar = 43566, // Alkyoneus->self, 5.0s cast, range 40 circle
    Catapult1 = 43567, // Alkyoneus->location, 3.0s cast, range 6 circle
    Catapult2 = 43568, // GiantAscetic->location, 3.0s cast, range 6 circle
    PowerAttack = 43569, // GiantRanger->self, 3.0s cast, range 20 120-degree cone
    MightyStrikes = 43575 // Alkyoneus->self, 6.0s cast, single-target
}

sealed class MightyStrikes(BossModule module) : Components.CastInterruptHint(module, (uint)AID.MightyStrikes, showNameInHint: true);
sealed class PowerAttack(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PowerAttack, new AOEShapeCone(20f, 60f.Degrees()));
sealed class CatapultParalyzeIIIBanishga(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Catapult1, (uint)AID.Catapult2,
(uint)AID.Banishga, (uint)AID.ParalyzeIII], 6f);
sealed class ImpactRoar(BossModule module) : Components.RaidwideCast(module, (uint)AID.ImpactRoar);

sealed class Sucker(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Sucker, 15f, kind: Kind.TowardsOrigin)
{
    private readonly Flood _aoe = module.FindComponent<Flood>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (_aoe.AOE.Length != 0)
        {
            ref var aoe = ref _aoe.AOE[0];
            return aoe.Check(pos);
        }
        return false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var source = ref Casters.Ref(0);
            hints.AddForbiddenZone(new SDCircle(source.Origin, 23f), source.Activation);
        }
    }
}

sealed class Flood(BossModule module) : Components.GenericAOEs(module)
{
    public AOEInstance[] AOE = [];
    private static readonly AOEShapeCircle circle = new(8f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOE;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Sucker)
        {
            AOE = [new(circle, spell.LocXZ, default, Module.CastFinishAt(spell, 3.5d))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Flood)
        {
            AOE = [];
        }
    }
}

sealed class A20KrakenStates : StateMachineBuilder
{
    public A20KrakenStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MightyStrikes>()
            .ActivateOnEnter<PowerAttack>()
            .ActivateOnEnter<CatapultParalyzeIIIBanishga>()
            .ActivateOnEnter<ImpactRoar>()
            .ActivateOnEnter<Flood>()
            .ActivateOnEnter<Sucker>()
            .Raw.Update = () => AllDestroyed(A20Kraken.TrashP1) && AllDeadOrDestroyed(A20Kraken.TrashP2);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.Kraken, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1058u, NameID = 14071u, Category = BossModuleInfo.Category.Alliance, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 5)]
public sealed class A20Kraken(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom([new(-829.36f, -873.39f), new(-828.99f, -873.05f),
    new(-828.26f, -871.95f), new(-827.92f, -871.33f), new(-825.72f, -864.52f),
    new(-826.10f, -858.99f), new(-826.31f, -858.45f), new(-826.17f, -857.81f), new(-825.82f, -857.40f), new(-822.08f, -855.15f),
    new(-819.16f, -852.18f), new(-818.78f, -851.61f), new(-818.49f, -846.43f), new(-818.59f, -845.73f), new(-819.93f, -843.54f),
    new(-820.38f, -843.04f), new(-821.88f, -841.66f), new(-822.09f, -841.15f), new(-822.15f, -839.77f), new(-822.85f, -838.58f),
    new(-823.29f, -838.31f), new(-828.26f, -836.25f), new(-829.89f, -834.86f), new(-831.26f, -833.36f), new(-832.31f, -831.83f),
    new(-832.30f, -831.21f), new(-832.61f, -830.24f), new(-865.20f, -836.93f),
    new(-866.57f, -836.95f), new(-870.77f, -837.53f), new(-871.48f, -837.72f), new(-871.50f, -839.61f), new(-871.63f, -840.20f),
    new(-873.99f, -843.50f), new(-874.57f, -856.16f), new(-874.41f, -856.84f), new(-872.68f, -860.34f), new(-872.13f, -863.63f),
    new(-871.93f, -864.27f), new(-870.61f, -865.86f), new(-870.05f, -866.31f), new(-866.28f, -867.92f), new(-866.06f, -868.38f),
    new(-865.86f, -871.94f), new(-866.24f, -873.39f), new(-829.36f, -873.39f)])]);
    public static readonly uint[] TrashP1 = [(uint)OID.Kraken, (uint)OID.Clipper, (uint)OID.Acrophies, (uint)OID.LightElemental,
    (uint)OID.GreaterPugil, (uint)OID.Banshee];
    public static readonly uint[] TrashP2 = [(uint)OID.Alkyoneus, (uint)OID.GiantAscetic, (uint)OID.GiantRanger];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        var m = this;
        Arena.Actors(m, TrashP1);
        Arena.Actors(m, TrashP2);
    }
}
