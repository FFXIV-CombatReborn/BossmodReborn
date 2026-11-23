namespace BossMod.Endwalker.Trial.T05Rubicante;

public enum OID : uint
{
    Boss = 0x3AEE, // R10.000, x1
    Helper = 0x233C, // Helper type
    RubicanteMirage = 0x3D85, // x8, Scalding Fleet source
    CircleOfPurgatory = 0x3D86, // x8, outer ring triangles
    CircleOfPurgatoryInner = 0x3D95, // x1
    CircleOfPurgatoryMiddle = 0x3D96, // x1
    CircleOfPurgatoryOuter = 0x3D97, // x1
    CircleOfPurgatoryFireball = 0x3D98, // x2
    GreaterFlamesent = 0x3D88, // spawn during fight
    FlamesentHeat = 0x3D89, // spawn during fight, Shattering Heat
    FlamesentWind = 0x3D8A, // spawn during fight, Ghastly Wind
    FlamesentFlame = 0x3D8B // spawn during fight, Ghastly Flame
}

public enum AID : uint
{
    Inferno1 = 31978, // Boss->self, 5.0s cast, range 60 circle, raidwide
    ShatteringHeat1 = 31979, // Boss->player, 5.0s cast, range 4 circle, tankbuster
    HopeAbandonYe = 32550, // Boss->self, no cast, single-target, visual
    ArcaneRevelation = 31934, // Boss->self, no cast, single-target, visual
    NinthCircle = 32802, // Boss->self, no cast, single-target, visual
    OrdealOfPurgation1 = 31940, // Boss->self, 10.0s cast, single-target, players are shackled as it ends
    OrdealOfPurgation2 = 33000, // Boss->self, 10.0s cast, single-target, players are shackled as it ends
    FieryExpiation = 31941, // CircleOfPurgatory->self, 2.0s cast, range 40 60-degree cone, from the edge
    FieryExpiationRect = 31942, // CircleOfPurgatory->self, 2.0s cast, range 20 width 40 rect, not seen
    ArchInferno = 31943, // Boss->self, 6.0s cast, range 5 circle
    ArchInfernoTick = 31944, // Helper->self, no cast, range 5 circle, ticks until Radial Flagration
    InfernoDevilFirst = 31945, // Helper->self, 6.0s cast, range 10 circle
    InfernoDevilRest = 31946, // Helper->self, 5.0s cast, range 10 circle
    InfernalSlaughter = 31950, // Boss->self, no cast, single-target, visual
    Hellscathe = 31951, // Helper->location, 4.0s cast, range 5 circle
    Conflagration = 31947, // Boss->self, 5.0s cast, range 40 width 10 rect, from the edge
    RadialFlagration = 31948, // Boss->self, 5.0s cast, single-target, visual
    RadialFlagrationAOE = 31949, // Helper->self, 6.0s cast, range 21 30-degree cone, x8
    GhastlyTorch = 31956, // GreaterFlamesent->self, 4.0s cast, range 60 circle, raidwide
    GhastlyFlame = 31959, // FlamesentFlame->self, 4.0s cast, single-target, visual
    GhastlyFlameAOE = 31960, // Helper->location, 4.0s cast, range 5 circle
    GhastlyWind = 31958, // FlamesentWind->self, 5.0s cast, range 40 60-degree cone
    ShatteringHeat2 = 31957, // FlamesentHeat->player, 5.0s cast, range 3 circle, tankbuster
    BlazingRapture = 31954, // Helper->self, 13.7s cast, range 60 circle, raidwide
    Inferno2 = 31962, // Helper->player, 5.0s cast, range 5 circle, spread
    ExplosivePyre = 31963, // Boss->player, no cast, range 2 circle, auto-attack
    Flamerake = 31965, // Boss->self, 6.0s cast, single-target, visual
    Flamerake1 = 31967, // Helper->self, 1.8s cast, range 40 width 12 rect, from the edge
    Flamerake2 = 31968, // Helper->self, 2.8s cast, range 8 width 40 rect
    Flamerake3 = 31969, // Helper->self, 3.8s cast, range 8 width 40 rect
    Soulscald = 31974, // Boss->self, 5.0s cast, single-target, visual
    ScaldingFleetMark = 31939, // Helper->player, no cast, marks every player for Scalding Fleet
    ScaldingFleetFirst = 31976, // RubicanteMirage->location, no cast, range 40 width 6 rect, from the boss through each player
    ScaldingFleetSecond = 31977, // RubicanteMirage->location, 3.0s cast, range 60 width 6 rect
    Dualfire = 32149, // Boss->self, 5.0s cast, single-target, visual
    DualfireAOE = 32150, // Helper->self, no cast, range 60 cone, on both tanks
    SweepingImmolationSpread = 31970, // Boss->self, 5.0s cast, range 20 180-degree cone
    SweepingImmolationStack = 31971, // Boss->self, 5.0s cast, range 20 180-degree cone, not seen
    PartialImmolation = 31972, // Helper->player, 5.5s cast, range 5 circle, spread
    TotalImmolation = 31973 // Helper->player, 5.5s cast, range 6 circle, stack, not seen
}

public enum SID : uint
{
    Pattern = 2056 // none->CircleOfPurgatoryInner/CircleOfPurgatoryMiddle/CircleOfPurgatoryOuter, extra=0x21E/0x221/0x222
}

public enum IconID : uint
{
    Dualfire = 230 // player
}

sealed class Raidwides(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.Inferno1, (uint)AID.GhastlyTorch, (uint)AID.BlazingRapture]);
sealed class ShatteringHeat(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.ShatteringHeat1, 4f);

sealed class OrdealOfPurgation(BossModule module) : Components.GenericAOEs(module, (uint)AID.FieryExpiation)
{
    private static readonly AOEShapeCone _shape = new(40f, 30f.Degrees());
    private readonly AOEInstance[] _aoes = new AOEInstance[2];

    private int _dirInner;
    private int _dirInnerExtra;
    private int _midIncrement;
    private int _midDecrement;
    private DateTime _activation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation == default)
            return [];
        _aoes[0] = AOEFromDirection(_dirInner);
        if (_dirInnerExtra == _dirInner)
            return _aoes.AsSpan(0, 1);
        _aoes[1] = AOEFromDirection(_dirInnerExtra);
        return _aoes;
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID != (uint)SID.Pattern)
            return;
        switch (actor.OID)
        {
            case (uint)OID.CircleOfPurgatoryInner:
                _dirInner = AngleToDirectionIndex(actor.Rotation);
                _dirInnerExtra = status.Extra switch
                {
                    0x21F => AngleToDirectionIndex(actor.Rotation - 90f.Degrees()),
                    0x220 => AngleToDirectionIndex(actor.Rotation + 180f.Degrees()),
                    _ => _dirInner
                };
                break;
            case (uint)OID.CircleOfPurgatoryMiddle:
                _midIncrement = AngleToDirectionIndex(actor.Rotation - 135f.Degrees());
                _midDecrement = AngleToDirectionIndex(actor.Rotation + 135f.Degrees());
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.OrdealOfPurgation1:
            case (uint)AID.OrdealOfPurgation2:
                _activation = Module.CastFinishAt(spell);
                break;
            case (uint)AID.FieryExpiation:
                var aoes = ActiveAOEs(default, caster);
                var matched = false;
                for (var i = 0; i < aoes.Length; ++i)
                    matched |= aoes[i].Origin.AlmostEqual(spell.LocXZ, 1f);
                if (!matched)
                    ReportError($"Predicted fireball exit does not match the cast at {spell.LocXZ}");
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            _activation = default;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        var dir = state switch
        {
            0x00020001u => -1,
            0x00200010u => +1,
            _ => 0
        };
        if (dir == 0)
            return;
        switch (index)
        {
            case 1:
                _dirInner = NormalizeDirectionIndex(_dirInner + dir);
                _dirInnerExtra = NormalizeDirectionIndex(_dirInnerExtra + dir);
                break;
            case 2:
                _midIncrement = NormalizeDirectionIndex(_midIncrement + dir);
                _midDecrement = NormalizeDirectionIndex(_midDecrement + dir);
                break;
        }
    }

    private static int NormalizeDirectionIndex(int index) => index & 7;
    private static int AngleToDirectionIndex(Angle rotation) => NormalizeDirectionIndex((int)(Math.Round(rotation.Deg / 45f) + 4));
    private static Angle DirectionIndexToAngle(int index) => (index - 4) * 45f.Degrees();

    private int TransformByMiddle(int index)
    {
        if (index == _midIncrement)
            return NormalizeDirectionIndex(index + 1);
        if (index == _midDecrement)
            return NormalizeDirectionIndex(index - 1);
        return index;
    }

    private AOEInstance AOEFromDirection(int index)
    {
        var dir = DirectionIndexToAngle(TransformByMiddle(index));
        return new(_shape, Arena.Center + 20f * dir.ToDirection(), dir + 180f.Degrees(), _activation);
    }
}

sealed class FieryExpiationRect(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FieryExpiationRect, new AOEShapeRect(20f, 20f));

sealed class ArchInferno(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _shape = new(5f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ArchInferno)
            _aoe = [new(_shape, caster.Position, default, Module.CastFinishAt(spell))];
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.RadialFlagrationAOE)
            _aoe = [];
    }
}

sealed class InfernoDevil(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.InfernoDevilFirst, (uint)AID.InfernoDevilRest], 10f);
sealed class Hellscathe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hellscathe, 5f);
sealed class Conflagration(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Conflagration, new AOEShapeRect(40f, 5f));
sealed class RadialFlagration(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RadialFlagrationAOE, new AOEShapeCone(21f, 15f.Degrees()));

sealed class Flamesents(BossModule module) : Components.AddsMulti(module, [(uint)OID.GreaterFlamesent, (uint)OID.FlamesentHeat, (uint)OID.FlamesentWind, (uint)OID.FlamesentFlame]);
sealed class GhastlyFlame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GhastlyFlameAOE, 5f);
sealed class GhastlyWind(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GhastlyWind, new AOEShapeCone(40f, 30f.Degrees()));
sealed class ShatteringHeatAdd(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.ShatteringHeat2, 3f);

sealed class Inferno2(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Inferno2, 5f);
sealed class Flamerake1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Flamerake1, new AOEShapeRect(40f, 6f));
sealed class Flamerake23(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Flamerake2, (uint)AID.Flamerake3], new AOEShapeRect(8f, 20f));

sealed class ScaldingFleetFirst(BossModule module) : Components.GenericBaitAway(module, (uint)AID.ScaldingFleetFirst)
{
    private static readonly AOEShapeRect _shape = new(40f, 3f);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ScaldingFleetMark && WorldState.Actors.Find(spell.MainTargetID) is Actor target)
        {
            CurrentBaits.Add(new(Module.PrimaryActor, target, _shape, WorldState.FutureTime(5.1d)));
        }
        else if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}

sealed class ScaldingFleetSecond(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ScaldingFleetSecond, new AOEShapeRect(60f, 3f));
sealed class Dualfire(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60f, 60f.Degrees()), (uint)IconID.Dualfire, (uint)AID.DualfireAOE, 6d, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);
sealed class SweepingImmolation(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.SweepingImmolationSpread, (uint)AID.SweepingImmolationStack], new AOEShapeCone(20f, 90f.Degrees()));
sealed class PartialTotalImmolation(BossModule module) : Components.CastStackSpread(module, (uint)AID.TotalImmolation, (uint)AID.PartialImmolation, 6f, 5f, 8, 8);

sealed class T05RubicanteStates : StateMachineBuilder
{
    public T05RubicanteStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Raidwides>()
            .ActivateOnEnter<ShatteringHeat>()
            .ActivateOnEnter<OrdealOfPurgation>()
            .ActivateOnEnter<FieryExpiationRect>()
            .ActivateOnEnter<ArchInferno>()
            .ActivateOnEnter<InfernoDevil>()
            .ActivateOnEnter<Hellscathe>()
            .ActivateOnEnter<Conflagration>()
            .ActivateOnEnter<RadialFlagration>()
            .ActivateOnEnter<Flamesents>()
            .ActivateOnEnter<GhastlyFlame>()
            .ActivateOnEnter<GhastlyWind>()
            .ActivateOnEnter<ShatteringHeatAdd>()
            .ActivateOnEnter<Inferno2>()
            .ActivateOnEnter<Flamerake1>()
            .ActivateOnEnter<Flamerake23>()
            .ActivateOnEnter<ScaldingFleetFirst>()
            .ActivateOnEnter<ScaldingFleetSecond>()
            .ActivateOnEnter<Dualfire>()
            .ActivateOnEnter<SweepingImmolation>()
            .ActivateOnEnter<PartialTotalImmolation>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Kagekazu (ported from upstream and improved by LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 886, NameID = 12054)]
public sealed class T05Rubicante(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f));
