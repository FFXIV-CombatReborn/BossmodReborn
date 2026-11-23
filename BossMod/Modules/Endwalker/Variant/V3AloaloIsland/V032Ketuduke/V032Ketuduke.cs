namespace BossMod.Endwalker.VariantCriterion.V03Aloalo.V032Ketuduke;

public enum OID : uint
{
    Boss = 0x4091, // R8.000, x1
    Helper = 0x233C, // R0.500, x?, Helper type
    KnockbackHelper = 0x40B2, // R0.500, x?
    SpringCrystalSphere = 0x4092, // R4.200, x?, polyhedron crystals (point-blank Saturate)
    SpringCrystalRect = 0x4093, // R4.200, x?, flat crystals (line Saturate)
    AiryBubble = 0x4095, // R1.300, x?, Blowing Bubbles
    ZealBlindZozone = 0x4096, // R0.500, x?, route 1/2 NPC
    SummonedApa = 0x4113, // R2.880, x?, Water III / Wavefoam
    SummonedAnila = 0x4097, // R2.400, x?, route 2 Fluke Typhoon
    WavefoamBubble = 0x4094, // R1.200, x?, route 2 stand-in bubbles
    BubbleStrewer = 0x1EB936, // R0.500, EventObj type, Strewn Bubbles lines
    AloaloZaratan = 0x409A, // R1.120, x?, route 3 sand pile snakes
    AloaloOgrebon = 0x4098, // R2.340, x?, route 4 stone pile fish
}

public enum AID : uint
{
    AutoAttack = 35448, // Boss->player, no cast, single-target
    Teleport = 35447, // Boss->location, no cast, single-target

    TidalRoar = 35493, // Boss->self, 5.0s cast, single-target, visual
    TidalRoarAOE = 35494, // Helper->self, no cast, range 100 circle, raidwide

    SpringCrystals = 35449, // Boss->self, 2.2+0.8s cast, single-target, visual
    SpringCrystalSphereAppear = 35450, // SpringCrystalSphere->self, no cast, single-target, visual
    SaturateSphere = 35452, // SpringCrystalSphere->self, 3.0s cast, range 8 circle
    SaturateSphereFast = 35453, // SpringCrystalSphere->self, 1.0s cast, range 8 circle
    SpringCrystalRectAppear = 35451, // SpringCrystalRect->self, no cast, single-target, visual
    SaturateRect = 35454, // SpringCrystalRect->self, 3.0s cast, range 76 width 10 rect
    SaturateRectFast = 35455, // SpringCrystalRect->self, 1.0s cast, range 76 width 10 rect

    BubbleNet = 35456, // Boss->self, 4.1+0.9s cast, single-target, visual
    BubbleNetAOE = 35457, // Helper->self, 5.0s cast, range 65 circle, raidwide

    FlukeTyphoon = 35460, // Boss->self, 3.0s cast, single-target, visual (move bubbled crystal)
    FlukeTyphoonAOE = 35461, // Helper->self, 5.0s cast, range 40 width 40 rect

    StrewnBubbles = 35462, // Boss->self, 2.2+0.8s cast, single-target, visual
    SphereShatter = 35463, // Helper->self, no cast, range 20 width 10 rect

    RecedingTwintides = 35485, // Boss->self, 5.0s cast, range 14 circle
    FarTide = 35488, // Boss->self, 1.5s cast, range 8-60 donut
    EncroachingTwintides = 35487, // Boss->self, 5.0s cast, range 8-60 donut
    NearTide = 35486, // Boss->self, 1.5s cast, range 14 circle

    Hydrobomb = 35489, // Boss->self, 2.2+0.8s cast, single-target, visual
    HydrobombAOE = 35490, // Helper->location, 3.0s cast, range 5 circle

    BlowingBubbles = 35464, // Boss->self, 2.2+0.8s cast, single-target, visual

    Hydroblast = 35491, // Boss->player, 5.0s cast, single-target, tankbuster
    HydroblastAOE = 35492, // Helper->player, no cast, range 5 circle

    Summon = 35470, // ZealBlindZozone->self, 3.0s cast, single-target
    WaterIII = 36116, // SummonedApa->self, 10.0s cast, single-target, proximity
    WaterIIIApply = 36125, // SummonedApa->player, no cast, single-target

    TidalWave = 36113, // Boss->self, 2.2+0.8s cast, single-target, visual
    TidalWaveAOE = 36114, // Helper->self, 6.0s cast, range 46 width 46 rect, knockback
    Hydrosurge = 35467, // Boss->self, 5.2+0.8s cast, single-target, visual
    HydrosurgeAOE = 35468, // Helper->self, 6.0s cast, single-target, starts persistent mid-line
    Wavefoam = 36115, // SummonedApa->self, 3.0s cast, single-target, spawn WavefoamBubble
    FlukeTyphoonAnila = 35471, // SummonedAnila->self, 8.0s cast, range 40 width 40 rect, knock bubbled players across

    Roar = 35474, // Boss->self, 5.0s cast, single-target, summon Zaratan
    BubbleNetSand = 35475, // Boss->self, 4.1+0.9s cast, single-target, visual (bubble one Zaratan)
    BubbleNetSandAOE = 35476, // Helper->self, 5.0s cast, range 65 circle, raidwide
    Updraft = 36111, // Boss->self, 6.5+0.5s cast, single-target, elevate bubbled Zaratan
    UpdraftAOE = 36112, // Helper->self, 7.0s cast, range 35 circle, visual
    HundredLashingsVisual = 35480, // AloaloZaratan->self, 14.4+1.1s cast, single-target, visual
    HundredLashings = 35479, // AloaloZaratan->self, 15.5s cast, range 60 180-degree cone (ground)
    HundredLashingsElevated = 35481, // Helper->self, 15.5s cast, single-target, elevated (misses)

    BubbleRiptide = 35458, // WavefoamBubble->player, no cast, single-target, pull into bubble
    BubbleFetters = 35459, // WavefoamBubble->player, no cast, single-target, bind in bubble
    UpdraftStone = 35477, // Boss->self, 6.0s cast, single-target, elevate bubble
    UpdraftStoneAOE = 35478, // Helper->self, 8.0s cast, range 35 circle, visual
    AerialShock = 35483, // AloaloOgrebon->self, 8.6+1.1s cast, single-target, visual
    AerialShockAOE = 35484, // Helper->self, 9.7s cast, range 35 circle

    Riptide = 35465, // AiryBubble->player, no cast, single-target
    Fetters = 35466, // AiryBubble->player, no cast, single-target
}

public enum SID : uint
{
    Bubble = 3745, // on bubbled spring crystals / Zaratan / Ogrebon (extra=0xC8)
    BubbleNet = 3744, // WavefoamBubble->player, extra=0x64
}

public enum TetherID : uint
{
    WaterIIIGood = 1, // Apa->player, stretched
    WaterIIIStretch = 57, // Apa->player, too close
}

sealed class TidalRoar(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.TidalRoar, (uint)AID.TidalRoarAOE, 1);
sealed class BubbleNet(BossModule module) : Components.RaidwideCast(module, (uint)AID.BubbleNetAOE);
sealed class BubbleNetSand(BossModule module) : Components.RaidwideCast(module, (uint)AID.BubbleNetSandAOE);
sealed class Hydrobomb(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HydrobombAOE, 5);
sealed class Hydroblast(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Hydroblast);
sealed class Roar(BossModule module) : Components.CastHint(module, (uint)AID.Roar, "Zaratan adds spawning");

sealed class WaterIII(BossModule module) : Components.GenericAOEs(module, (uint)AID.WaterIII)
{
    private readonly List<Actor> _casters = [];
    private readonly List<(Actor Apa, Actor Target, bool Stretched)> _tethers = [];

    private const float MinStretch = 20;
    private static readonly AOEShapeCircle _tooClose = new(MinStretch);

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoesBuffer.Clear();
        foreach (var c in _casters)
            _aoesBuffer.Add(new(_tooClose, c.Position, default, Module.CastFinishAt(c.CastInfo)));
        return CollectionsMarshal.AsSpan(_aoesBuffer);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            _casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            _casters.RemoveAll(c => c == caster);
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID is not ((uint)TetherID.WaterIIIGood or (uint)TetherID.WaterIIIStretch))
            return;
        if (source.OID != (uint)OID.SummonedApa || WorldState.Actors.Find(tether.Target) is not { } target)
            return;

        _tethers.RemoveAll(t => t.Apa == source || t.Target == target);
        _tethers.Add((source, target, tether.ID == (uint)TetherID.WaterIIIGood));
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.SummonedApa)
            _tethers.RemoveAll(t => t.Apa == source);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var t in _tethers)
        {
            if (!(t.Target == actor))
                continue;
            if (!t.Stretched)
                hints.Add("Stretch tether!");
            else
                hints.Add("Tether is stretched!", false);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var t in _tethers)
        {
            if (!(t.Target == actor && !t.Stretched))
                continue;
            var activation = t.Apa.CastInfo != null ? Module.CastFinishAt(t.Apa.CastInfo) : default;
            hints.AddForbiddenZone(_tooClose.Distance(t.Apa.Position, default), activation);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var t in _tethers)
        {
            var color = t.Stretched ? Colors.Safe : Colors.Danger;
            Arena.AddLine(t.Apa.Position, t.Target.Position, color);
        }
    }
}

sealed class SpringCrystals(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(ulong InstanceID, AOEInstance AOE)> _aoes = [];

    private static readonly AOEShapeCircle _sphere = new(8);
    private static readonly AOEShapeRect _rect = new(38, 5, 38);

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoesBuffer.Clear();
        // make not risky for hints if Hydrosurge is active so AI doesn't try to cross the midline
        var soften = Module.FindComponent<Hydrosurge>()?.Active == true;
        foreach (var a in _aoes)
            _aoesBuffer.Add(soften ? a.AOE with { Color = Colors.AOE, Risky = false } : a.AOE);
        return CollectionsMarshal.AsSpan(_aoesBuffer);
    }

    public override void OnActorCreated(Actor actor)
    {
        AOEShape? shape = actor.OID switch
        {
            (uint)OID.SpringCrystalSphere => _sphere,
            (uint)OID.SpringCrystalRect => _rect,
            _ => null
        };
        if (shape == null)
            return;

        _aoes.Add((actor.InstanceID, new(shape, actor.Position, actor.Rotation, WorldState.FutureTime(15))));
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID != (uint)SID.Bubble)
            return;
        if (actor.OID is not ((uint)OID.SpringCrystalSphere or (uint)OID.SpringCrystalRect))
            return;

        var index = _aoes.FindIndex(a => a.InstanceID == actor.InstanceID);
        if (index < 0)
        {
            ReportError($"Failed to find crystal AOE for bubbled {actor}");
            return;
        }

        ref var entry = ref _aoes.Ref(index);
        // bubble crystals move +20 towards center
        var origin = entry.AOE.Origin + new WDir(entry.AOE.Origin.X < Module.Center.X ? 20 : -20, 0);
        entry = (entry.InstanceID, entry.AOE with { Origin = origin });
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is not ((uint)AID.SaturateSphere or (uint)AID.SaturateSphereFast or (uint)AID.SaturateRect or (uint)AID.SaturateRectFast))
            return;

        var index = _aoes.FindIndex(a => a.InstanceID == caster.InstanceID);
        if (index < 0)
            return;

        ref var entry = ref _aoes.Ref(index);
        // update from actor position
        entry = (entry.InstanceID, entry.AOE with { Origin = caster.Position, Rotation = spell.Rotation, Activation = Module.CastFinishAt(spell) });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is not ((uint)AID.SaturateSphere or (uint)AID.SaturateSphereFast or (uint)AID.SaturateRect or (uint)AID.SaturateRectFast))
            return;

        _aoes.RemoveAll(a => a.InstanceID == caster.InstanceID);
        ++NumCasts;
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID is (uint)OID.SpringCrystalSphere or (uint)OID.SpringCrystalRect)
            _aoes.RemoveAll(a => a.InstanceID == actor.InstanceID);
    }
}

sealed class StrewnBubbles(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(20, 5);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count > 4 ? CollectionsMarshal.AsSpan(_aoes)[..4] : CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.BubbleStrewer)
            _aoes.Add(new(_shape, actor.Position, actor.Rotation, WorldState.FutureTime(5.8f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.SphereShatter)
            return;

        var count = _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
        if (count != 1)
            ReportError($"{spell.Action} removed {count} aoes at {caster.Position}");
        ++NumCasts;
    }
}

sealed class Twintides(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shapeOut = new(14);
    private static readonly AOEShapeDonut _shapeIn = new(8, 60);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count > 1 ? CollectionsMarshal.AsSpan(_aoes)[..1] : CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.RecedingTwintides:
                _aoes.Add(new(_shapeOut, caster.Position, default, Module.CastFinishAt(spell)));
                _aoes.Add(new(_shapeIn, caster.Position, default, Module.CastFinishAt(spell, 3.6f)));
                break;
            case (uint)AID.EncroachingTwintides:
                _aoes.Add(new(_shapeIn, caster.Position, default, Module.CastFinishAt(spell)));
                _aoes.Add(new(_shapeOut, caster.Position, default, Module.CastFinishAt(spell, 3.6f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RecedingTwintides or (uint)AID.EncroachingTwintides or (uint)AID.FarTide or (uint)AID.NearTide)
        {
            ++NumCasts;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }
}

sealed class BlowingBubbles(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _bubbles = [];

    private static readonly AOEShapeCircle _shape = new(5);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.AiryBubble)
            _bubbles.Add(actor);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.AiryBubble)
            _bubbles.Remove(actor);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var a in _bubbles)
            _shape.Draw(Arena, a);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var a in _bubbles)
            hints.AddForbiddenZone(_shape.Distance(a.Position, a.Rotation));
    }
}

sealed class TidalWave(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.TidalWaveAOE, 27, ignoreImmunes: true, shape: new AOEShapeRect(46, 23), kind: Kind.DirForward)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in Casters)
        {
            if (IsImmune(slot, c.Activation))
                continue;
            // stay near wave origin
            var origin = c.Origin;
            var activation = c.Activation;
            hints.AddForbiddenZone(new SDInvertedCircle(origin, 8), activation);
            hints.GoalZones.Add(AIHints.GoalProximity(origin, 4, 60));
        }
    }
}

sealed class Hydrosurge(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeRect _shape = new(20, 5, 20);

    public bool Active => _aoe != null;

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoesBuffer.Clear();
        if (_aoe is { } aoe)
            _aoesBuffer.Add(aoe);
        return CollectionsMarshal.AsSpan(_aoesBuffer);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HydrosurgeAOE)
            _aoe = new(_shape, caster.Position, spell.Rotation, WorldState.CurrentTime);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.FlukeTyphoonAnila)
            _aoe = null;
    }
}

sealed class WavefoamBubbles(BossModule module) : Components.GenericTowers(module)
{
    private bool _ogrebonBubbled;

    public override void Update()
    {
        Actor? ogrebon = null;
        foreach (var o in Module.Enemies((uint)OID.AloaloOgrebon))
        {
            if (!o.IsDeadOrDestroyed)
            {
                ogrebon = o;
                break;
            }
        }
        if (ogrebon == null)
        {
            _ogrebonBubbled = false;
            return;
        }

        _ogrebonBubbled = ogrebon.FindStatus((uint)SID.Bubble) != null;
        if (_ogrebonBubbled)
            Towers.Clear();
        else
        {
            foreach (var b in Module.Enemies((uint)OID.WavefoamBubble))
            {
                var known = false;
                for (var i = 0; i < Towers.Count; ++i)
                {
                    if (Towers[i].Position.AlmostEqual(b.Position, 1f))
                    {
                        known = true;
                        break;
                    }
                }
                if (!known)
                    Towers.Add(new(b.Position, 2, activation: WorldState.FutureTime(8)));
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID != (uint)OID.WavefoamBubble)
            return;
        // if fish is bubbled, don't soak towers
        if (Module.Enemies((uint)OID.AloaloOgrebon).Any(o => !o.IsDeadOrDestroyed && o.FindStatus((uint)SID.Bubble) != null))
            return;
        Towers.Add(new(actor.Position, 2, activation: WorldState.FutureTime(12)));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.WavefoamBubble)
            Towers.RemoveAll(t => t.Position.AlmostEqual(actor.Position, 1));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.FlukeTyphoonAnila or (uint)AID.AerialShockAOE or (uint)AID.UpdraftStoneAOE)
            Towers.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_ogrebonBubbled)
        {
            foreach (var b in Module.Enemies((uint)OID.WavefoamBubble))
                hints.AddForbiddenZone(new SDCircle(b.Position, 2));
            return;
        }
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

sealed class FlukeTyphoonAnila(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.FlukeTyphoonAnila, 20, ignoreImmunes: true, shape: new AOEShapeRect(40, 20), kind: Kind.DirForward);

sealed class AerialShock(BossModule module) : Components.GenericAOEs(module, (uint)AID.AerialShockAOE)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCircle _shape = new(35);

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoesBuffer.Clear();
        if (_aoe is not { } aoe)
            return CollectionsMarshal.AsSpan(_aoesBuffer);
        var ogrebonBubbled = false;
        foreach (var o in Module.Enemies((uint)OID.AloaloOgrebon))
            ogrebonBubbled |= o.FindStatus((uint)SID.Bubble) != null;
        _aoesBuffer.Add(ogrebonBubbled ? aoe with { Color = Colors.SafeFromAOE, Risky = false } : aoe);
        return CollectionsMarshal.AsSpan(_aoesBuffer);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            _aoe = new(_shape, caster.Position, default, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            _aoe = null;
    }
}

sealed class HundredLashings(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor Caster, AOEInstance AOE, bool Bubbled)> _aoes = [];
    private static readonly AOEShapeCone _shape = new(60, 90.Degrees());

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoesBuffer.Clear();
        foreach (var a in _aoes)
        {
            // bubbled half safe
            _aoesBuffer.Add(a.Bubbled ? (a.AOE with { Color = Colors.SafeFromAOE, Risky = false }) : a.AOE);
        }
        return CollectionsMarshal.AsSpan(_aoesBuffer);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.HundredLashings)
            return;
        var bubbled = caster.FindStatus((uint)SID.Bubble) != null;
        _aoes.Add((caster, new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)), bubbled));
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID != (uint)SID.Bubble || actor.OID != (uint)OID.AloaloZaratan)
            return;
        var i = _aoes.FindIndex(a => a.Caster == actor);
        if (i >= 0)
        {
            var e = _aoes[i];
            _aoes[i] = (e.Caster, e.AOE, true);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.HundredLashings or (uint)AID.HundredLashingsVisual or (uint)AID.HundredLashingsElevated)
            _aoes.RemoveAll(a => a.Caster == caster || a.Caster.InstanceID == caster.InstanceID);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.HundredLashings or (uint)AID.HundredLashingsElevated)
        {
            _aoes.RemoveAll(a => a.Caster == caster || a.AOE.Origin.AlmostEqual(caster.Position, 1));
            ++NumCasts;
        }
    }
}

sealed class V032KetudukeStates : StateMachineBuilder
{
    public V032KetudukeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TidalRoar>()
            .ActivateOnEnter<BubbleNet>()
            .ActivateOnEnter<BubbleNetSand>()
            .ActivateOnEnter<SpringCrystals>()
            .ActivateOnEnter<StrewnBubbles>()
            .ActivateOnEnter<Hydrobomb>()
            .ActivateOnEnter<Hydroblast>()
            .ActivateOnEnter<WaterIII>()
            .ActivateOnEnter<Twintides>()
            .ActivateOnEnter<BlowingBubbles>()
            .ActivateOnEnter<TidalWave>()
            .ActivateOnEnter<Hydrosurge>()
            .ActivateOnEnter<WavefoamBubbles>()
            .ActivateOnEnter<FlukeTyphoonAnila>()
            .ActivateOnEnter<Roar>()
            .ActivateOnEnter<HundredLashings>()
            .ActivateOnEnter<AerialShock>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 961, NameID = 12605, Contributors = "croizat (ported from upstream by LTS)")]
public sealed class V032Ketuduke(WorldState ws, Actor primary) : BossModule(ws, primary, new(-790, -395), new ArenaBoundsSquare(20));
