namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE53HereComesTheCavalry;

public enum OID : uint
{
    Boss = 0x31C7, // R7.200, x1
    ImperialAssaultCraft = 0x2EE8, // R0.500, x22, also helper?
    Cavalry = 0x31C6, // R4.000, x9, and more spawn during fight
    FireShot = 0x1EB1D3, // R0.500, EventObj type, spawn during fight
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6497, // Cavalry/Boss->player, no cast, single-target
    KillZone = 24700, // ImperialAssaultCraft->self, no cast, range 25-30 donut deathwall

    StormSlash = 23931, // Cavalry->self, 5.0s cast, range 8 120-degree cone
    MagitekBurst = 23932, // Cavalry->location, 5.0s cast, range 8 circle
    BurnishedJoust = 23936, // Cavalry->location, 3.0s cast, width 6 rect charge

    GustSlash = 23933, // Boss->self, 7.0s cast, range 60 ?-degree cone visual
    GustSlashAOE = 23934, // Helper->self, 8.0s cast, ???, knockback 'forward' 35
    CallFireShot = 23935, // Boss->self, 3.0s cast, single-target, visual
    FireShot = 23937, // ImperialAssaultCraft->location, 3.0s cast, range 6 circle
    Burn = 23938, // ImperialAssaultCraft->self, no cast, range 6 circle
    CallStrategicRaid = 24578, // Boss->self, 3.0s cast, single-target, visual
    AirborneExplosion = 24872, // ImperialAssaultCraft->location, 9.0s cast, range 10 circle
    RideDown = 23939, // Boss->self, 6.0s cast, range 60 width 60 rect visual
    RideDownAOE = 23940, // Helper->self, 6.5s cast, ???, knockback side 12
    CallRaze = 23948, // Boss->self, 3.0s cast, single-target, visual
    Raze = 23949, // ImperialAssaultCraft->location, no cast, ???, raidwide?
    RawSteel = 23943, // Boss->player, 5.0s cast, width 4 rect charge cleaving tankbuster
    CloseQuarters = 23944, // Boss->self, 5.0s cast, single-target, visual
    CloseQuartersAOE = 23945, // Helper->self, 5.0s cast, range 15 circle
    FarAfield = 23946, // Boss->self, 5.0s cast, single-target, visual
    FarAfieldAOE = 23947, // Helper->self, 5.0s cast, range 10-30 donut
    CallControlledBurn = 23950, // Boss->self, 5.0s cast, single-target, visual (spread)
    CallControlledBurnAOE = 23951, // ImperialAssaultCraft->players, 5.0s cast, range 6 circle spread
    MagitekBlaster = 23952 // Boss->players, 5.0s cast, range 8 circle stack
}

public enum TetherID : uint
{
    RawSteel = 57 // Boss->player
}

sealed class StormSlash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StormSlash, new AOEShapeCone(8f, 60f.Degrees()));
sealed class MagitekBurst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagitekBurst, 8f);
sealed class BurnishedJoust(BossModule module) : Components.ChargeAOEs(module, (uint)AID.BurnishedJoust, 3f);

// note: there are two casters, probably to avoid 32-target limit - we only want to show one
sealed class GustSlash(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.GustSlashAOE, 35f, true, 1, null, Kind.DirForward);

sealed class FireShot(BossModule module) : Components.VoidzoneAtCastTarget(module, 6f, (uint)AID.FireShot, GetVoidzones, default)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.FireShot);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

sealed class AirborneExplosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AirborneExplosion, 10f);
sealed class RideDownAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RideDown, new AOEShapeRect(60f, 5f));

// note: there are two casters, probably to avoid 32-target limit - we only want to show one
// TODO: generalize to reusable component
sealed class RideDownKnockback(BossModule module) : Components.GenericKnockback(module, (uint)AID.RideDownAOE, 1)
{
    private readonly List<Knockback> _sources = [with(2)];
    private static readonly AOEShapeCone _shape = new(30f, 90f.Degrees());

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(_sources);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _sources.Clear();
            var act = Module.CastFinishAt(spell);
            // charge always happens through center, so create two sources with origin at center looking orthogonally
            _sources.Add(new(Arena.Center, 12f, act, _shape, spell.Rotation + 90f.Degrees(), Kind.DirForward, ignoreImmunes: true));
            _sources.Add(new(Arena.Center, 12f, act, _shape, spell.Rotation - 90f.Degrees(), Kind.DirForward, ignoreImmunes: true));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            _sources.Clear();
    }
}

sealed class CallRaze(BossModule module) : Components.RaidwideCast(module, (uint)AID.CallRaze, "Multi raidwide");

// TODO: find out optimal distance, test results so far:
// - distance ~6.4 (inside hitbox) and 1 vuln stack: 79194 damage
// - distance ~22.2 and 4 vuln stacks: 21083 damage
// since hitbox is 7.2 it is probably starting to be optimal around distance 15
sealed class RawSteel(BossModule module) : Components.BaitAwayChargeTether(module, 2f, 5f, (uint)AID.RawSteel, (uint)AID.RawSteel, minimumDistance: 15f);
sealed class CloseQuarters(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CloseQuartersAOE, 15f);
sealed class FarAfield(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FarAfieldAOE, new AOEShapeDonut(10f, 30f));
sealed class CallControlledBurn(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.CallControlledBurnAOE, 6f);
sealed class MagitekBlaster(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.MagitekBlaster, 8f);

sealed class CE53HereComesTheCavalryStates : StateMachineBuilder
{
    public CE53HereComesTheCavalryStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StormSlash>()
            .ActivateOnEnter<MagitekBurst>()
            .ActivateOnEnter<BurnishedJoust>()
            .ActivateOnEnter<GustSlash>()
            .ActivateOnEnter<FireShot>()
            .ActivateOnEnter<AirborneExplosion>()
            .ActivateOnEnter<RideDownAOE>()
            .ActivateOnEnter<RideDownKnockback>()
            .ActivateOnEnter<CallRaze>()
            .ActivateOnEnter<RawSteel>()
            .ActivateOnEnter<CloseQuarters>()
            .ActivateOnEnter<FarAfield>()
            .ActivateOnEnter<CallControlledBurn>()
            .ActivateOnEnter<MagitekBlaster>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 778, NameID = 22)] // bnpcname=9929
public sealed class CE53HereComesTheCavalry(WorldState ws, Actor primary) : BossModule(ws, primary, new(-750f, 790f), new ArenaBoundsCircle(25f))
{
    protected override bool CheckPull() => PrimaryActor.InCombat && Raid.Player() is Actor player && player.Position.InCircle(Arena.Center, 25f); // not targetable at start

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies((uint)OID.Cavalry));
    }
}
