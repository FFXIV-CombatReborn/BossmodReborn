namespace BossMod.Dawntrail.Dungeon.D05Origenics.D050OrigenicsAerostat;

public enum OID : uint
{
    Boss = 0x41E2, //R=2.3
    Aerostat2 = 0x42BA, //R=2.3
    OrigenicsSentryS9 = 0x43D6, // R0.65
    OrigenicsSentryS92 = 0x4189, // R0.65
    OrigenicsSentryG10 = 0x43D7 // R0.8
}

public enum AID : uint
{
    AutoAttack1 = 871, // Boss/Aerostat2->player, no cast, single-target
    AutoAttack2 = 873, // OrigenicsSentryG10->player, no cast, single-target
    AutoAttack3 = 870, // OrigenicsSentryS9/OrigenicsSentryS92->player, no cast, single-target

    IncendiaryCircle = 38328, // Aerostat2->self, 4.0s cast, range 3-12 donut
    SentryfugalSlash = 35425, // OrigenicsSentryS92->player, no cast, single-target
    GrenadoShot = 35428, // OrigenicsSentryG10->location, 3.0s cast, range 5 circle
}

sealed class IncendiaryCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IncendiaryCircle, new AOEShapeDonut(3f, 12f));
sealed class GrenadoShot(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrenadoShot, 5f);

sealed class D050OrigenicsAerostatStates : StateMachineBuilder
{
    public D050OrigenicsAerostatStates(D050OrigenicsAerostat module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IncendiaryCircle>()
            .ActivateOnEnter<GrenadoShot>()
            .Raw.Update = () => AllDeadOrDestroyed(D050OrigenicsAerostat.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 825, NameID = 12895, SortOrder = 2)]
public sealed class D050OrigenicsAerostat(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly Angle a30 = 30f.Degrees();
    private static readonly WPos node1Center = new(-60f, -80f), node2Center = new(-116f, -80f), node3Center = new(-172f, -80f);
    private static readonly ArenaBoundsCustom arena = new([new Polygon(node1Center, 16f, 6, a30), new Rectangle(new(-88f, -80f), 20f, 6f),
    new Polygon(node2Center, 16f, 6, a30), new Polygon(node3Center, 16f, 6, a30), new Rectangle(new(-144f, -80f), 20f, 6f),
    new Rectangle(node3Center, 39f, 6f), new Rectangle(node3Center, 6f, 19.6f), new Rectangle(node2Center, 6f, 19.6f), new Rectangle(node1Center, 6f, 19.6f)]);
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.Aerostat2, (uint)OID.OrigenicsSentryS9, (uint)OID.OrigenicsSentryS92, (uint)OID.OrigenicsSentryG10];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Trash);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            hints.PotentialTargets[i].Priority = 0;
        }
    }
}
