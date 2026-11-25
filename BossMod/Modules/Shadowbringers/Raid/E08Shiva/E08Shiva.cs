namespace BossMod.Shadowbringers.Raid.E08Shiva;

public enum OID : uint
{
    Boss = 0x2D73, // R4.130, Shiva
    Helper = 0x233C, // R0.500
    FrozenMirror = 0x2DA0, // R1.000
    Mothercrystal = 0x2D86, // R1.000
    EarthenAether = 0x2D78, // R0.960
    AqueousAether = 0x2D77, // R0.960
    ElectricAether = 0x2D76, // R0.960
    LuminousAether = 0x2E13, // R0.960
    Gaia = 0x2D96, // R1.000
    IceVeil = 0x2D83, // R5.000
}

public enum AID : uint
{
    AutoAttack = 19923, // Boss->player, no cast, single-target
    AbsoluteZero = 19927, // Boss->self, 5.0s cast, range 40 circle
    RedressHoly = 20303, // Boss->self, 4.0s cast, single-target
    ShiningArmor = 19953, // Helper->self, no cast, range 40 circle (gaze)
    ScytheKick = 19939, // Boss->self, 5.0s cast, range 4-20 donut
    RedressIce = 20302, // Boss->self, 4.0s cast, single-target
    ThinIce = 19992, // Helper->self, 5.0s cast, range 40 circle
    FrostArmor = 19952, // Helper->self, no cast, single-target
    DrivingFrost = 19932, // Boss->self, 5.0s cast, range 40 90-degree cone
    DoubleSlap = 19930, // Boss->player, 5.0s cast, single-target
    DiamondFrost = 19937, // Boss->self, 5.0s cast, range 40 circle
    FrigidWater = 19976, // Helper->location, no cast, range 40 circle
    IcicleImpact = 19978, // Helper->location, 8.0s cast, range 10 circle
    FrigidEruption = 19977, // Helper->location, 2.0s cast, range 5 circle
    HeavenlyStrike = 19928, // Boss->self, 5.0s cast, range 40 circle (knockback)
    MirrorMirror = 19924, // Boss->self, 3.0s cast, single-target
    BitingFrost = 19931, // Boss->self, 5.0s cast, range 30 270-degree cone
    ReflectedBitingFrost = 19966, // FrozenMirror->self, 5.0s cast, range 30 270-degree cone
    ReflectedDrivingFrost = 19967, // FrozenMirror->self, 5.0s cast, range 40 90-degree cone
    AxeKick = 19938, // Boss->self, 5.0s cast, range 16 circle
    ShivaJump = 19800, // Boss->location, no cast, single-target
    ShatteredWorld = 19945, // Boss->self, 10.0s cast, range 40 circle (phase transition)
    HeartAsunder = 19986, // Mothercrystal->location, no cast, range 20 circle
    Stoneskin = 19951, // EarthenAether->self, 4.0s cast, single-target
    ShockSpikes = 19950, // ElectricAether->self, no cast, single-target
    EarthenReach = 20090, // EarthenAether->Mothercrystal, no cast, single-target
    AetherReach = 20091, // Aqueous/Electric->Mothercrystal, no cast, single-target
    Rush = 20424, // LuminousAether->self, 5.0s cast, single-target
    LuminousReach = 20426, // LuminousAether->Mothercrystal, no cast, single-target
    RushHit = 20425, // LuminousAether->player, no cast, range 4 circle
    Skyfall = 19988, // Helper->location, no cast, range 100 circle (enrage)
    FrigidStone = 20070, // Helper->location, no cast, range 5 circle
    Skyfall2 = 19987, // Helper->location, no cast, range 100 circle
    Holy = 19949, // Boss->location, 8.0s cast, eight outer meteors
    Holy2 = 19948, // Boss->location, 8.0s cast, single center meteor
    HolyDivided = 19985, // Helper->location, no cast, individual meteor
    HolyHit = 19984, // Helper->location, no cast, center meteor resolve
    LightRampant = 19944, // Boss->self, 5.0s cast, range 40 circle
    ThePathOfLight = 19929, // Boss->self, 5.0s cast, single-target
    ThePathOfLightCone = 19979, // Helper->self, no cast, range 40 cone
    BrightHunger = 19980, // Helper->location, no cast, range 4 circle (towers)
    ReflectedAxeKick = 19968, // FrozenMirror->self, 5.0s cast, range 16 circle
    TwinStillness = 19933, // Boss->self, 5.0s cast, range 30 270-degree cone (then 90 behind)
    TwinSilence = 19934, // Boss->self, 5.0s cast, range 40 90-degree cone (then 270 front)
    TwinSecond = 19955, // Helper->self, 7.0s cast, range 40 90-degree cone (Twin Stillness follow-up telegraph)
    TwinStillnessSecond = 19936, // Boss->self, no cast, second hit
    AbsoluteZeroHit = 20008, // Boss->self, no cast, range 40 circle
    AbsoluteZeroHitTarget = 20009, // Helper->player, no cast, single-target
    EndlessIceAge = 19991, // IceVeil->self, 45.0s cast, range 40 circle (enrage)
}

public enum SID : uint
{
    ThinIce = 911, // none->player
}

public enum IconID : uint
{
    Gaze = 214, // Boss
    Proximity = 87, // player -> FrigidWater
    Eruption = 197, // player -> FrigidEruption
    PathOfLight = 23, // player -> The Path of Light cone
}

public enum TetherID : uint
{
    Luminous = 84, // LuminousAether->Mothercrystal/player
}

sealed class AbsoluteZero(BossModule module) : Components.RaidwideCast(module, (uint)AID.AbsoluteZero);
sealed class DiamondFrost(BossModule module) : Components.RaidwideCast(module, (uint)AID.DiamondFrost);
sealed class ThinIceCast(BossModule module) : Components.RaidwideCast(module, (uint)AID.ThinIce, "Slippery ground soon");
sealed class DoubleSlap(BossModule module) : Components.SingleTargetCast(module, (uint)AID.DoubleSlap);
sealed class ScytheKick(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ScytheKick, new AOEShapeDonut(4, 20));
sealed class AxeKick(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.AxeKick, (uint)AID.ReflectedAxeKick], new AOEShapeCircle(16));
sealed class DrivingFrost(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.DrivingFrost, (uint)AID.ReflectedDrivingFrost], new AOEShapeCone(40, 45.Degrees()));
sealed class BitingFrost(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BitingFrost, (uint)AID.ReflectedBitingFrost], new AOEShapeCone(30, 135.Degrees()));
sealed class IcicleImpact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IcicleImpact, 10);
sealed class Holy(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle _shape = new(12);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.Holy)
            return;

        _aoes.Clear();
        var act = Module.CastFinishAt(spell);
        for (var i = 0; i < 8; ++i)
            _aoes.Add(new(_shape, E08Shiva.Phase1Center + 16 * (i * 45f).Degrees().ToDirection(), default, act));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Holy)
            _aoes.Clear();
    }
}

sealed class HolyCenter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Holy2, 15);
sealed class LightRampant(BossModule module) : Components.RaidwideCast(module, (uint)AID.LightRampant);
sealed class EndlessIceAge(BossModule module) : Components.CastHint(module, (uint)AID.EndlessIceAge, "Kill Ice Veil!", true);

sealed class ThePathOfLight(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(40, 30.Degrees()), (uint)IconID.PathOfLight, (uint)AID.ThePathOfLightCone, 6.0f);

sealed class BrightHunger(BossModule module) : Components.GenericTowers(module, (uint)AID.BrightHunger)
{
    private BitMask _forbidden; // prohibit path of light people from soaking

    public override void OnMapEffect(byte index, uint state)
    {
        if (state != 0x00020001)
            return;

        var (dir, min) = index switch
        {
            0x16 => (225.Degrees(), 1), // NW
            0x17 => (315.Degrees(), 1), // SW
            0x18 => (45.Degrees(), 1),  // SE
            0x19 => (135.Degrees(), 1), // NE
            0x1A => (270.Degrees(), 2), // W
            0x1B => (90.Degrees(), 2),  // E
            0x1C => (0.Degrees(), 4),   // S
            _ => default
        };
        if (min == 0)
            return;

        Towers.Add(new(Module.Center + 8 * dir.ToDirection(), 4, min, min, _forbidden, WorldState.FutureTime(8)));
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID != (uint)IconID.PathOfLight)
            return;
        _forbidden.Set(Raid.FindSlot(actor.InstanceID));
        UpdateForbidden();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ThePathOfLightCone)
        {
            foreach (var t in spell.Targets)
                _forbidden.Clear(Raid.FindSlot(t.ID));
            UpdateForbidden();
        }

        if (spell.Action.ID != (uint)AID.BrightHunger)
            return;
        ++NumCasts;
        for (var i = Towers.Count - 1; i >= 0; --i)
        {
            if (Towers[i].Position.AlmostEqual(spell.TargetXZ, 1f))
                Towers.RemoveAt(i);
        }
    }

    private void UpdateForbidden()
    {
        for (var i = 0; i < Towers.Count; ++i)
            Towers.Ref(i).ForbiddenSoakers = _forbidden;
    }
}

sealed class TwinStillnessSilence(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCone _wide = new(30, 135.Degrees());
    private static readonly AOEShapeCone _narrow = new(40, 45.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TwinStillness:
                _aoes.Add(new(_wide, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
                _aoes.Add(new(_narrow, caster.Position, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell, 2.1f)));
                break;
            case (uint)AID.TwinSilence:
                _aoes.Add(new(_narrow, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
                _aoes.Add(new(_wide, caster.Position, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell, 2.1f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.TwinStillness or (uint)AID.TwinSilence or (uint)AID.TwinSecond or (uint)AID.TwinStillnessSecond)
        {
            ++NumCasts;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }
}
sealed class FrigidEruption(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FrigidEruption, 5);
sealed class FrigidWater(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Proximity, (uint)AID.FrigidWater, 15, 5.2f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == SpreadAction)
            Spreads.Clear();
    }
}
sealed class HeavenlyStrike(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.HeavenlyStrike, 12);
sealed class Stoneskin(BossModule module) : Components.CastHint(module, (uint)AID.Stoneskin, "Interrupt Stoneskin!", true);

sealed class ShiningArmor(BossModule module) : Components.GenericGaze(module, (uint)AID.ShiningArmor)
{
    private DateTime _activation;

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
        => _activation != default ? new Eye[1] { new(Module.PrimaryActor.Position, _activation) } : [];

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RedressHoly)
            _activation = WorldState.FutureTime(5.1f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            NumCasts++;
            _activation = default;
        }
    }
}

// thin ice: every move slides the player forward by the distance stored in the status extra (in tenths of a yalm)
sealed class ThinIceHint(BossModule module) : Components.GenericKnockback(module)
{
    public static float SlideDistance(Actor actor) => actor.FindStatus((uint)SID.ThinIce) is ActorStatus s ? s.Extra / 10f : 0f;

    private readonly Knockback[] _oneKnockback = new Knockback[1];
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        var dist = SlideDistance(actor);
        return dist > 0f ? _oneKnockback.Single(new(actor.Position, dist, default, null, actor.Rotation, Kind.DirForward, ignoreImmunes: true)) : [];
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var dist = SlideDistance(actor);
        if (dist <= 0)
            return;
        hints.Add($"Slippery floor. Careful!");
        base.AddHints(slot, actor, hints);
    }
}

sealed class HeartAsunder(BossModule module) : Components.CastCounter(module, (uint)AID.HeartAsunder)
{
    private DateTime _next;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            NumCasts++;
            _next = WorldState.FutureTime(14);
        }
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (_next != default && _next > WorldState.CurrentTime)
            hints.Add($"Crystal pulse in {(_next - WorldState.CurrentTime).TotalSeconds:f1}s");
    }
}

sealed class CrystalBleed(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _shape = new(2f);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoes.Clear();
        if (!E08Shiva.InAddsPhase(Module))
            return [];
        var crystal = E08Shiva.NearestCrystal(Module, actor.Position);
        var crystals = Module.Enemies((uint)OID.Mothercrystal);
        for (var i = 0; i < crystals.Count; ++i)
        {
            var c = crystals[i];
            if (!c.IsDeadOrDestroyed && E08Shiva.OnSamePlatform(c.Position, crystal))
                _aoes.Add(new(_shape, c.Position));
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }
}

sealed class LuminousRush(BossModule module) : BossComponent(module)
{
    private readonly List<(Actor Source, Actor Target)> _tethers = [];

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID != (uint)TetherID.Luminous || source.OID != (uint)OID.LuminousAether)
            return;
        RemoveTethers(source);
        if (WorldState.Actors.Find(tether.Target) is { } target)
            _tethers.Add((source, target));
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Luminous)
            RemoveTethers(source);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RushHit or (uint)AID.LuminousReach)
            RemoveTethers(caster);
    }

    private void RemoveTethers(Actor source)
    {
        for (var i = _tethers.Count - 1; i >= 0; --i)
        {
            if (_tethers[i].Source == source)
                _tethers.RemoveAt(i);
        }
    }

    private bool TetheredToCrystal(Actor source)
    {
        for (var i = 0; i < _tethers.Count; ++i)
        {
            var t = _tethers[i];
            if (t.Source == source && t.Target.OID == (uint)OID.Mothercrystal)
                return true;
        }
        return false;
    }

    private Actor? LocalLuminous(Actor actor)
    {
        if (!E08Shiva.InAddsPhase(Module))
            return null;
        var crystal = E08Shiva.NearestCrystal(Module, actor.Position);
        if (!E08Shiva.OnSamePlatform(actor.Position, crystal))
            return null;
        foreach (var (src, tgt) in _tethers)
        {
            if (!E08Shiva.OnSamePlatform(src.Position, crystal))
                continue;
            if (tgt.OID == (uint)OID.Mothercrystal || tgt == actor) // need to grab, or already holding: stay still so autorot doesn't drop it
                return src;
        }
        return null;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (LocalLuminous(actor) is Actor src && TetheredToCrystal(src))
            hints.Add("Grab tether!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var crystal = E08Shiva.NearestCrystal(Module, pc.Position);
        foreach (var (src, tgt) in _tethers)
        {
            if (!E08Shiva.OnSamePlatform(src.Position, crystal))
                continue;
            Arena.AddLine(src.Position, tgt.Position, tgt.OID == (uint)OID.Mothercrystal ? Colors.Danger : Colors.Safe);
            Arena.Actor(src);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (LocalLuminous(actor) is not { } src)
            return;

        // high weight because autorot will keep taking you off the tether
        hints.GoalZones.Add(AIHints.GoalProximity(src.Position, 25, 1000));
    }
}

sealed class ShatteredWorld(BossModule module) : Components.RaidwideCast(module, (uint)AID.ShatteredWorld)
{
    public override void Update()
    {
        var e08 = (E08Shiva)Module;
        var focus = Raid.Player();
        if (focus == null)
        {
            var party = Raid.WithoutSlot();
            if (party.Length == 0)
                return;
            focus = party[0];
        }

        if (!e08.AddsPhase)
        {
            foreach (var c in E08Shiva.CrystalCenters)
            {
                if ((focus.Position - c).LengthSq() < 12 * 12)
                {
                    e08.AddsPhase = true;
                    break;
                }
            }
        }

        if (e08.AddsPhase)
        {
            // back to main arena after adds
            if (focus.Position.InCircle(E08Shiva.Phase1Center, E08Shiva.Phase1Bounds.Radius + 5))
            {
                e08.AddsPhase = false;
                Module.Arena.Center = E08Shiva.Phase1Center;
                Module.Arena.Bounds = E08Shiva.Phase1Bounds;
                return;
            }

            var crystal = E08Shiva.NearestCrystal(Module, focus.Position);
            Module.Arena.Center = crystal;
            Module.Arena.Bounds = E08Shiva.LocalPlatformBounds;
        }
    }
}

sealed class AetherAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.EarthenAether, (uint)OID.AqueousAether, (uint)OID.ElectricAether, (uint)OID.LuminousAether], 1)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var crystal = E08Shiva.NearestCrystal(Module, pc.Position);
        var actors = ActiveActors;
        for (var i = 0; i < actors.Count; ++i)
        {
            if (E08Shiva.OnSamePlatform(actors[i].Position, crystal))
                Arena.Actor(actors[i]);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var crystal = E08Shiva.NearestCrystal(Module, actor.Position);
        var targets = hints.PotentialTargets;
        for (var i = 0; i < targets.Count; ++i)
        {
            var e = targets[i];
            if (Array.IndexOf(OIDs, e.Actor.OID) < 0)
                continue;
            if (!E08Shiva.OnSamePlatform(e.Actor.Position, crystal))
            {
                e.Priority = AIHints.Enemy.PriorityPointless;
                continue;
            }
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.LuminousAether => 3,
                (uint)OID.EarthenAether => 2,
                _ => 1
            };
        }
    }
}

sealed class E08ShivaStates : StateMachineBuilder
{
    public E08ShivaStates(BossModule module) : base(module)
    {
        var phase = TrivialPhase()
            .ActivateOnEnter<AbsoluteZero>()
            .ActivateOnEnter<ShiningArmor>()
            .ActivateOnEnter<ScytheKick>()
            .ActivateOnEnter<ThinIceCast>()
            .ActivateOnEnter<ThinIceHint>()
            .ActivateOnEnter<DrivingFrost>()
            .ActivateOnEnter<DoubleSlap>()
            .ActivateOnEnter<DiamondFrost>()
            .ActivateOnEnter<IcicleImpact>()
            .ActivateOnEnter<FrigidWater>()
            .ActivateOnEnter<FrigidEruption>()
            .ActivateOnEnter<HeavenlyStrike>()
            .ActivateOnEnter<BitingFrost>()
            .ActivateOnEnter<AxeKick>()
            .ActivateOnEnter<Holy>()
            .ActivateOnEnter<HolyCenter>()
            .ActivateOnEnter<LightRampant>()
            .ActivateOnEnter<ThePathOfLight>()
            .ActivateOnEnter<BrightHunger>()
            .ActivateOnEnter<TwinStillnessSilence>()
            .ActivateOnEnter<EndlessIceAge>()
            .ActivateOnEnter<ShatteredWorld>()
            .ActivateOnEnter<HeartAsunder>()
            .ActivateOnEnter<CrystalBleed>()
            .ActivateOnEnter<Stoneskin>()
            .ActivateOnEnter<LuminousRush>()
            .ActivateOnEnter<AetherAdds>();
        phase.Raw.Update = () =>
        {
            if (Module.PrimaryActor.IsDestroyed)
                return true;
            var veils = Module.Enemies((uint)OID.IceVeil);
            for (var i = 0; i < veils.Count; ++i)
            {
                if (veils[i].IsDead || veils[i].HPMP.CurHP == 0)
                    return true;
            }
            return false;
        };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "croizat (ported from upstream and modified by LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 728, NameID = 9353)]
public sealed class E08Shiva(WorldState ws, Actor primary) : BossModule(ws, primary, Phase1Center, Phase1Bounds)
{
    public static readonly WPos Phase1Center = new(100, 100);
    public static readonly ArenaBoundsCircle Phase1Bounds = new(20);

    // showing both at once is way too small to be that useful tbh
    public static readonly WPos[] CrystalCenters = [new(67, 168), new(133, 168)];
    public const float PlatformRadius = 9.5f;
    public static readonly ArenaBoundsCircle LocalPlatformBounds = new(PlatformRadius);

    public bool AddsPhase;

    public static bool InAddsPhase(BossModule module) => module is E08Shiva { AddsPhase: true };
    public static bool OnSamePlatform(WPos pos, WPos crystal) => (pos - crystal).LengthSq() < 12 * 12;

    public static WPos NearestCrystal(BossModule module, WPos pos)
    {
        var best = CrystalCenters[0];
        var bestDist = float.MaxValue;
        foreach (var c in CrystalCenters)
        {
            var d = (c - pos).LengthSq();
            if (d < bestDist)
            {
                bestDist = d;
                best = c;
            }
        }

        var crystals = module.Enemies((uint)OID.Mothercrystal);
        for (var i = 0; i < crystals.Count; ++i)
        {
            var m = crystals[i];
            if (m.IsDeadOrDestroyed)
                continue;
            var d = (m.Position - pos).LengthSq();
            if (d < bestDist)
            {
                bestDist = d;
                best = m.Position;
            }
        }
        return best;
    }

    public override Actor? GetDefaultTarget(int slot)
    {
        var veils = Enemies((uint)OID.IceVeil);
        for (var i = 0; i < veils.Count; ++i)
        {
            var v = veils[i];
            if (v.IsTargetable && !v.IsDeadOrDestroyed)
                return v;
        }
        return base.GetDefaultTarget(slot);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.IceVeil));
        Arena.Actors(Enemies((uint)OID.FrozenMirror), Colors.Object);
        if (AddsPhase)
        {
            var crystal = NearestCrystal(this, pc.Position);
            var crystals = Enemies((uint)OID.Mothercrystal);
            for (var i = 0; i < crystals.Count; ++i)
            {
                if (OnSamePlatform(crystals[i].Position, crystal))
                    Arena.Actor(crystals[i], Colors.Object);
            }
        }
        else
        {
            Arena.Actors(Enemies((uint)OID.Mothercrystal), Colors.Object);
        }
    }
}
