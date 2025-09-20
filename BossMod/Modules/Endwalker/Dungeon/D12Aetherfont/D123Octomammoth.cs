namespace BossMod.Endwalker.Dungeon.D12Aetherfont.D123Octomammoth;

public enum OID : uint
{
    Boss = 0x3EAA, // R=26.0
    MammothTentacle = 0x3EAB, // R=6.0
    Crystals = 0x3EAC, // R=0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 33357, // Boss->player, no cast, single-target

    Breathstroke = 34551, // Boss->self, 16.5s cast, range 35 180-degree cone
    Clearout = 33348, // MammothTentacle->self, 9.0s cast, range 16 120-degree cone
    Octostroke = 33347, // Boss->self, 16.0s cast, single-target
    SalineSpit1 = 33352, // Boss->self, 3.0s cast, single-target
    SalineSpit2 = 33353, // Helper->self, 6.0s cast, range 8 circle
    Telekinesis1 = 33349, // Boss->self, 5.0s cast, single-target
    Telekinesis2 = 33351, // Helper->self, 10.0s cast, range 12 circle
    TidalBreath = 33354, // Boss->self, 10.0s cast, range 35 180-degree cone
    TidalRoar = 33356, // Boss->self, 5.0s cast, range 60 circle
    VividEyes = 33355, // Boss->self, 4.0s cast, range 20-26 donut
    WaterDrop = 34436, // Helper->player, 5.0s cast, range 6 circle
    WallopVisual = 33350, // Boss->self, no cast, single-target, visual, starts tentacle wallops
    Wallop = 33346 // MammothTentacle->self, 3.0s cast, range 22 width 8 rect
}

sealed class Wallop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Wallop, new AOEShapeRect(22f, 4f));
sealed class VividEyes(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VividEyes, new AOEShapeDonut(20f, 26f));
sealed class Clearout(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Clearout, new AOEShapeCone(16f, 60f.Degrees()));
sealed class TidalBreathBreathstroke(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.TidalBreath, (uint)AID.Breathstroke], new AOEShapeCone(35f, 90f.Degrees()));
sealed class TidalRoar(BossModule module) : Components.RaidwideCast(module, (uint)AID.TidalRoar);
sealed class WaterDrop(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.WaterDrop, 6f);
sealed class SalineSpit(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SalineSpit2, 8f);
sealed class Telekinesis(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Telekinesis2, 12f);

sealed class D123OctomammothStates : StateMachineBuilder
{
    public D123OctomammothStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Wallop>()
            .ActivateOnEnter<Clearout>()
            .ActivateOnEnter<VividEyes>()
            .ActivateOnEnter<WaterDrop>()
            .ActivateOnEnter<TidalRoar>()
            .ActivateOnEnter<TidalBreathBreathstroke>()
            .ActivateOnEnter<Telekinesis>()
            .ActivateOnEnter<SalineSpit>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "dhoggpt, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 822u, NameID = 12334u, SortOrder = 8)]
public sealed class D123Octomammoth(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos arenaCenter = new(-370f, -368f);
    private static readonly WPos[] bridge1 = [new(-396.417f, -361.641f), new(-393.813f, -358.136f), new(-393.178f, -353.815f), new(-387.778f, -356.604f),
    new(-390.071f, -359.686f), new(-390.630f, -363.486f)];
    private static readonly WPos[] bridge5 = [new(-346.767f, -353.669f), new(-346.187f, -358.136f), new(-343.583f, -361.638f), new(-349.946f, -363.513f),
    new(-349.929f, -359.686f), new(-352.302f, -356.647f)]; // coordinates seem to be slightly offset from calculated values, so hardcoding a 2nd bridge here
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(-345f, -368f), 8f, 20), new Polygon(new(-352.322f, -350.322f), 8f, 20, 9f.Degrees()),
    new Polygon(new(-370f, -343f), 8f, 20), new Polygon(new(-387.678f, -350.322f), 8f, 20, 9f.Degrees()), new Polygon(new(-395f, -368f), 8f, 20),
    new PolygonCustom(bridge1), new PolygonCustom(WPos.GenerateRotatedVertices(arenaCenter, bridge1, -45f)),
    new PolygonCustom(WPos.GenerateRotatedVertices(arenaCenter, bridge1, -90f)), new PolygonCustom(bridge5)], AdjustForHitbox: true);

    protected override bool CheckPull() => Arena.InBounds(Raid.Player()!.Position);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            if (e.Actor.OID == (uint)OID.Boss)
            {
                e.Priority = 1;
                break;
            }
        }
    }
}
