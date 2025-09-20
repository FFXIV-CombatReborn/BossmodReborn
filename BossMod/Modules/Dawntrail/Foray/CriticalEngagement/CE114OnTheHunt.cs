namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE114OnTheHunt;

public enum OID : uint
{
    Boss = 0x46DC, // R4.6
    Deathwall = 0x1EBD94, // R0.5
    DeathwallHelper = 0x46B4, // R1.0
    RadiantBeacon = 0x46DD, // R3.0
    RadiantRoundel = 0x46DE, // R1.5
    LightSprite = 0x46DF, // R1.76
    OchreStone = 0x46E0, // R2.7
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Deathwall = 41400, // DeathwallHelper->self, no cast, range 25-60 donut

    FearsomeGlint = 41411, // Boss->self, 6.0s cast, range 60 90-degree cone

    ShockwaveVisual1 = 41412, // Boss->self, 5.0s cast, single-target, raidwide x3
    ShockwaveVisual2 = 41413, // Boss->self, no cast, single-target
    Shockwave = 41414, // Helper->self, no cast, ???

    AetherialRay = 41402, // Helper->self, no cast, range 28 width 10 rect
    BrightPulse = 41403, // Helper->location, no cast, range 12 circle
    AugmentationOfRoundels = 41404, // Boss->self, 3.0s cast, single-target
    AugmentationOfStones = 41405, // Boss->self, 3.0s cast, single-target
    AugmentationOfBeacons = 41401, // Boss->self, 3.0s cast, single-target

    FallingRockVisual = 41409, // Boss->self, 3.0s cast, single-target
    FallingRock = 41410, // Helper->location, 4.0s cast, range 10 circle

    Flatten1 = 30787, // Boss->self, 5.0s cast, single-target
    Flatten2 = 41406, // OchreStone->self, 5.0s cast, single-target
    Flatten3 = 41408, // OchreStone->self, 5.0s cast, single-target
    Decompress = 41407 // Helper->self, 5.0s cast, range 12 circle
}

public enum TetherID : uint
{
    BeaconTimer = 312 // DeathwallHelper->RadiantBeacon
}

public enum SID : uint
{
    RoundelTimer = 2193 // none->RadiantRoundel, extra=0x36A
}

sealed class Shockwave(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.ShockwaveVisual1, (uint)AID.Shockwave, 0.8f, "Raidwide x3");
sealed class FearsomeGlint(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FearsomeGlint, new AOEShapeCone(60f, 45f.Degrees()));
sealed class FallingRock(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FallingRock, 10f);
sealed class Decompress(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Decompress, 12f);

sealed class AetherialRay(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeRect rect = new(28f, 6f); // extra safety margin because the predictions are not even close to pixel perfect

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.BeaconTimer)
        {
            var target = WorldState.Actors.Find(tether.Target);
            if (target is Actor t)
            {
                // cross seems to always rotate clockwise
                _aoes.Add(new(rect, Arena.Center, Angle.FromDirection(t.Position - CE114OnTheHunt.ArenaCenter) + 200f.Degrees(), WorldState.FutureTime(5.2d)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.AetherialRay)
        {
            _aoes.Clear();
        }
    }
}

sealed class BrightPulse(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(3);
    private static readonly AOEShapeCircle circle = new(13f); // extra safety margin because the predictions are not even close to pixel perfect

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.RoundelTimer && actor.OID == (uint)OID.RadiantRoundel)
        {
            var dir = actor.Position - CE114OnTheHunt.ArenaCenter;
            var angle = dir.LengthSq() < 225f ? 280f.Degrees() : 150f.Degrees();
            if (dir.OrthoR().Dot(actor.Rotation.ToDirection()) > 0f)
                angle = -angle;
            _aoes.Add(new(circle, CE114OnTheHunt.ArenaCenter + dir.Rotate(angle), default, WorldState.FutureTime(5.2d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.BrightPulse)
        {
            _aoes.Clear();
        }
    }
}

sealed class CE114OnTheHuntStates : StateMachineBuilder
{
    public CE114OnTheHuntStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<FearsomeGlint>()
            .ActivateOnEnter<FallingRock>()
            .ActivateOnEnter<Decompress>()
            .ActivateOnEnter<AetherialRay>()
            .ActivateOnEnter<BrightPulse>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 1018, NameID = 42)]
public sealed class CE114OnTheHunt(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter.Quantized(), new ArenaBoundsCircle(26f))
{
    public static readonly WPos ArenaCenter = new(636f, -54f);

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 30f);
}
