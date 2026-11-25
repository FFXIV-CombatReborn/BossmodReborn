namespace BossMod.Dawntrail.Unreal.Un5Shinryu;

// raidwides: the boss cast is a visual, the helper cast carries the aoe
class EarthenFury(BossModule module) : Components.RaidwideCast(module, (uint)AID.EarthenFury, "Raidwide, tiles crack");
class EarthenFurySmash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EarthenFurySmash, new AOEShapeRect(20f, 10f));
class Hellfire(BossModule module) : Components.RaidwideCast(module, (uint)AID.Hellfire, "Raidwide, stand in water");
class JudgmentBolt(BossModule module) : Components.RaidwideCast(module, (uint)AID.JudgmentBolt, "Raidwide, get out of water");
class DiamondDust(BossModule module) : Components.RaidwideCast(module, (uint)AID.DiamondDust, "Raidwide, thin ice after");
class IceStorm(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.IceStormVisual, (uint)AID.IceStormVisualP3]);
class DarkMatter(BossModule module) : Components.RaidwideCast(module, (uint)AID.DarkMatter, "Raidwide, floor breaks");
class Protostar(BossModule module) : Components.RaidwideCast(module, (uint)AID.Protostar);
class TailSpit(BossModule module) : Components.RaidwideCast(module, (uint)AID.TailSpit);
class Enrage(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.TidalWaveEnrage, (uint)AID.JudgmentBoltEnrage, (uint)AID.HellfireEnrage], "Enrage");

// knockbacks: tidal wave is a water spout on one edge pushing across, the other two are from the center.
// the big two are meant to be eaten with arm's length, the hint counts down to the 2s window
abstract class ArmsLengthKnockback(BossModule module, uint aid, float distance, Components.GenericKnockback.Kind kind = Components.GenericKnockback.Kind.AwayFromOrigin) : Components.SimpleKnockbacks(module, aid, distance, kind: kind)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (Casters.Count != 0)
        {
            var left = (Casters.Ref(0).Activation - WorldState.CurrentTime).TotalSeconds;
            hints.Add(left > 2d ? $"Arm's length at 2s ({left:f1}s)" : "ARM'S LENGTH NOW", left <= 2d);
        }
    }
}
class TidalWave(BossModule module) : ArmsLengthKnockback(module, (uint)AID.TidalWave, 35f, Kind.DirForward);
class AerialBlast(BossModule module) : ArmsLengthKnockback(module, (uint)AID.AerialBlast, 15f);
class SuperCyclone(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.SuperCyclone, 5f);

// wings. levinbolt has no marker, everyone gets a circle at the end of the 8s wing cast
class Levinbolt(BossModule module) : Components.UniformStackSpread(module, 0f, 5f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LevinboltVisual)
            AddSpreads(Raid.WithoutSlot(), Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Levinbolt)
            Spreads.Clear();
    }
}
class LevinboltAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LevinboltAOE, 5f);
class Hypernova(BossModule module) : Components.CastHint(module, (uint)AID.HypernovaVisual, "Stack together");
class IcicleAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.Icicle]);
class Spikesicle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spikesicle, new AOEShapeRect(64.5f, 5f), 3);
class SpikesicleKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Spikesicle, 10f, shape: new AOEShapeRect(64.5f, 5f), kind: Kind.DirForward, maxCasts: 3);

// icicle impact has no cast, the circle shows once the icicle lands and goes away with its spikesicle
class IcicleImpact(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.IcicleImpact)
            _aoes.Add(new(new AOEShapeCircle(6f), caster.Position.Quantized(), default, WorldState.CurrentTime));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Spikesicle)
            _aoes.RemoveAll(a => a.Origin == caster.Position.Quantized());
    }
}

// boss
class AkhMorn(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.AkhMorn, 4f);
class EarthBreath(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(80f, 30f.Degrees()), (uint)IconID.Earthshaker, (uint)AID.EarthBreathAOE, 5.6d);
class Dragonfist(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DragonfistAOE, 16f);
class GyreCharge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GyreCharge, new AOEShapeRect(100f, 30f));
class BurningChains(BossModule module) : Components.StretchTetherSingle(module, (uint)TetherID.BurningChains, 15f, null, (uint)AID.BurningChains);
class TailSlap(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.TailSlap1, (uint)AID.TailSlap2], new AOEShapeRect(40f, 10f));
class ThinIce(BossModule module) : Components.ThinIce(module, 30f, stopAtWall: false)
{
    // the generic "you are risking to slide" hint is useless here: on thin ice the only reason to move at all is a chain
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Mask[slot])
            hints.Add("DO NOT MOVE UNLESS H2 TETHER");
    }
}
// the reiryu have to be HEALED to full before dragonflight resolves, so they are drawn but never a target
class Dragonflight(BossModule module) : Components.CastHint(module, (uint)AID.Dragonflight, "Heal Reiryu to full before the cast ends")
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var r in Module.Enemies((uint)OID.Reiryu))
            if (hints.FindEnemy(r) is { } e)
                e.Priority = AIHints.Enemy.PriorityForbidden;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) => Arena.Actors(Module.Enemies((uint)OID.Reiryu), Colors.Vulnerable);
}

// tail marker. the game snapshots the marked player's position when the marker goes away (~9s), the tail spawns there
// 3s later and slaps 3s after that. drop spots follow the party's board by role, relative to the boss, which sits on one
// edge during phase 1: healer back right, dps front left, tank back left. the 4 on the board is just the default
// position marker and means nothing.
class TailMarker(BossModule module) : BossComponent(module)
{
    private Actor? _target;
    private DateTime _snapshot;

    private static (string name, float forward, float right) Drop(Actor actor) => actor.Role switch
    {
        Role.Tank => ("back left", -20f, -20f),
        Role.Healer => ("back right", -20f, 20f),
        _ => ("front left", 20f, -20f)
    };

    private WPos? DropTile()
    {
        var toBoss = Module.PrimaryActor.Position - Arena.Center;
        if (_target == null || toBoss.LengthSq() < 100f)
            return null; // boss not on an edge, no idea what front is
        var fwd = toBoss.Normalized();
        var (_, f, r) = Drop(_target);
        return Arena.Center + fwd * f + fwd.OrthoR() * r;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.TailMarker)
        {
            _target = actor;
            _snapshot = WorldState.FutureTime(9d);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Tail)
            _target = null;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_target == null)
            return;
        var left = Math.Max(0d, (_snapshot - WorldState.CurrentTime).TotalSeconds);
        var (name, _, _) = Drop(_target);
        if (actor == _target)
            hints.Add($"TAIL MARKER: drop at {name} ({left:f0}s)");
        else
            hints.Add($"Tail on {_target.Name}: stay off {name} tile", false);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor == _target && DropTile() is { } tile)
            hints.GoalZones.Add(AIHints.GoalSingleTarget(tile, 6f, 2f));
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_target != null && DropTile() is { } tile)
            Arena.ZoneRect(tile, new WDir(0f, 1f), 10f, 10f, 10f, pc == _target ? Colors.SafeFromAOE : Colors.AOE);
    }
}

// phase 2
class Cocoons(BossModule module) : Components.AddsMulti(module, [(uint)OID.Cocoon, (uint)OID.MassiveCocoon]);
class DragonAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.Ginryu, (uint)OID.Hakkinryu], 1);
class MeteorImpact(BossModule module) : Components.ProximityAOEs(module, (uint)AID.MeteorImpact, 10f); // proximity with range 60, drawn small so 5 of them stay readable on the platform
class MeteorImpact2(BossModule module) : Components.ProximityAOEs(module, (uint)AID.MeteorImpact2, 10f);
class Touchdown(BossModule module) : Components.ProximityAOEs(module, (uint)AID.TouchdownAOE, 14f);
class Fireball(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Fireball, 4f);
class BlazingTrail(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlazingTrail, new AOEShapeRect(15f, 5.5f));
class SpikedTail(BossModule module) : Components.SingleTargetCast(module, (uint)AID.SpikedTail);
class DeathSentence(BossModule module) : Components.SingleTargetCast(module, (uint)AID.DeathSentence);

// cocoon markers: 5 players get a spread icon, 8s later meteor cocoons spawn where they stand and start casting.
// cleared on time, the cast start is only a backup
class CocoonSpread(BossModule module) : Components.UniformStackSpread(module, 0f, 5f)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.CocoonSpread)
            AddSpread(actor, WorldState.FutureTime(8d));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MeteorImpact2)
            Spreads.Clear();
    }

    public override void Update()
    {
        base.Update();
        Spreads.RemoveAll(s => s.Activation < WorldState.CurrentTime);
    }
}

// phase 3
class TeraSlash(BossModule module) : Components.SingleTargetCast(module, (uint)AID.TeraSlash);
class AtomicRay(BossModule module) : Components.StackWithIcon(module, (uint)IconID.AtomicRay, (uint)AID.AtomicRay, 6f, 5.7d, 2, 2, 4)
{
    // stacks that were not matched to a hit (dead partner, odd marker count) stayed on screen forever, drop them once resolved
    public override void Update()
    {
        base.Update();
        Stacks.RemoveAll(s => s.Activation.AddSeconds(1d) < WorldState.CurrentTime);
    }
}
class AetherialRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AetherialRay, new AOEShapeRect(100f, 1.5f));
class Wormwail(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Wormwail, new AOEShapeDonut(10f, 50f)); // inner radius from replay hits (safe at 8.4, hit at 10.2 with movement lag)
class BenightingBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BenightingBreath, new AOEShapeCone(50f, 45f.Degrees())); // half-angle between 44 and 50 per replay hits

sealed class AkhRhaiBait(BossModule module) : Components.UniformStackSpread(module, default, 4f, raidwideOnResolve: false)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AkhMorn)
        {
            var party = Raid.WithoutSlot();
            var activation = Module.CastFinishAt(spell, 2.1d);
            for (var i = 0; i < party.Length; ++i)
            {
                var p = party[i];
                if (p.Role != Role.Tank && p.InstanceID != spell.TargetID)
                    AddSpread(p, activation);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.AkhRhai)
            Spreads.Clear();
    }
}

sealed class AkhRhai(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(4f);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AkhRhai or (uint)AID.AkhRhai2)
        {
            ++NumCasts;
            var pos = spell.TargetXZ;
            var aoe = new AOEInstance(circle, pos, default, WorldState.FutureTime(0.6d));
            var count = _aoes.Count;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].Origin.AlmostEqual(pos, 0.5f))
                {
                    _aoes[i] = aoe;
                    return;
                }
            }
            _aoes.Add(aoe);
        }
    }

    public override void Update()
    {
        var now = WorldState.CurrentTime;
        _aoes.RemoveAll(a => a.Activation.AddSeconds(0.5d) < now);
    }
}

sealed class WaterPuddles(BossModule module) : BossComponent(module)
{
    private const float Radius = 5f;
    private DateTime _avoid, _seek, _stack;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LevinboltVisual:
            case (uint)AID.JudgmentBolt:
                _avoid = Module.CastFinishAt(spell);
                break;
            case (uint)AID.Hellfire:
                _seek = Module.CastFinishAt(spell);
                break;
            case (uint)AID.HypernovaVisual:
                _stack = Module.CastFinishAt(spell, 0.1d);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LevinboltVisual:
            case (uint)AID.JudgmentBolt:
                _avoid = default;
                break;
            case (uint)AID.Hellfire:
                _seek = default;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Hypernova)
            _stack = default;
    }

    private List<Actor> Puddles()
    {
        var puddles = Module.Enemies((uint)OID.WaterPuddle);
        var count = puddles.Count;
        List<Actor> res = [with(count)];
        for (var i = 0; i < count; ++i)
        {
            var p = puddles[i];
            if (p.EventState != 7)
                res.Add(p);
        }
        return res;
    }

    private static Actor? Closest(List<Actor> puddles, WPos pos)
    {
        Actor? best = null;
        var bestDist = float.MaxValue;
        var count = puddles.Count;
        for (var i = 0; i < count; ++i)
        {
            var p = puddles[i];
            var d = (p.Position - pos).LengthSq();
            if (d < bestDist)
            {
                bestDist = d;
                best = p;
            }
        }
        return best;
    }

    private Actor? Target(List<Actor> puddles, Actor actor) => _stack != default ? Closest(puddles, Arena.Center) : _seek != default ? Closest(puddles, actor.Position) : null;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var puddles = Puddles();
        if (_avoid != default)
        {
            var count = puddles.Count;
            for (var i = 0; i < count; ++i)
                hints.AddForbiddenZone(new SDCircle(puddles[i].Position, Radius + 1f), _avoid);
        }
        else if (Target(puddles, actor) is Actor target)
        {
            hints.AddForbiddenZone(new SDInvertedCircle(target.Position, Radius - 1.5f), _stack != default ? _stack : _seek);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var puddles = Puddles();
        var count = puddles.Count;
        if (count == 0)
            return;
        var inWater = false;
        for (var i = 0; i < count; ++i)
        {
            if (actor.Position.InCircle(puddles[i].Position, Radius))
            {
                inWater = true;
                break;
            }
        }
        if (_avoid != default && inWater)
            hints.Add("Get out of water!");
        else if (_stack != default && Closest(puddles, Arena.Center) is Actor puddle && !actor.Position.InCircle(puddle.Position, Radius))
            hints.Add("Stack in the water!");
        else if (_seek != default && !inWater)
            hints.Add("Stand in water!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var puddles = Puddles();
        var stackPuddle = _stack != default ? Closest(puddles, Arena.Center) : null;
        var count = puddles.Count;
        for (var i = 0; i < count; ++i)
        {
            var p = puddles[i];
            if (_avoid != default)
                Arena.ZoneCircle(p.Position, Radius, Colors.AOE);
            else if (_seek != default || p == stackPuddle)
                Arena.ZoneCircle(p.Position, Radius, Colors.SafeFromAOE);
            else
                Arena.AddCircleUnfilled(p.Position, Radius, Colors.Object);
        }
    }
}

sealed class Paralysis(BossModule module) : BossComponent(module)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!actor.Class.CanEsuna())
            return;
        var party = Raid.WithoutSlot();
        for (var i = 0; i < party.Length; ++i)
        {
            var member = party[i];
            if (member.FindStatus((uint)SID.Paralysis) != null && member.PendingDispels.Count == 0)
                hints.Add($"Cleanse {member.Name}!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var party = Raid.WithSlot();
        for (var i = 0; i < party.Length; ++i)
        {
            var (partySlot, member) = party[i];
            if (member.FindStatus((uint)SID.Paralysis) != null)
                hints.ShouldCleanse.Set(partySlot);
        }
    }
}

sealed class DarkMatterPush(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        var hp = Module.PrimaryActor.HPMP;
        if (hp.MaxHP == 0 || Module.StateMachine.ActivePhaseIndex != 0 || Module.StateMachine.TimeSincePhaseEnter < 240f)
            return;
        var pct = 100f * hp.CurHP / hp.MaxHP;
        if (pct > 45f)
            hints.Add($"Push to 45% for Dark Matter (now {pct:f1}%)");
    }
}

sealed class TouchdownSpot(BossModule module) : BossComponent(module)
{
    private static readonly WPos spot = new(default, -28f);
    private bool _done;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.TouchdownAOE)
            _done = true;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_done)
            hints.AddForbiddenZone(new SDInvertedCircle(spot, 2f));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_done && !actor.Position.InCircle(spot, 2f))
            hints.Add("Go to the north edge, Touchdown throws you onto the platform!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!_done)
            Arena.AddCircleUnfilled(spot, 2f, Colors.Safe, 2f);
    }
}

sealed class TailSpitKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.TailSpit, 15f)
{
    private static readonly WPos spot = new WPos(default, -80f) + 6.5f * (-135f).Degrees().ToDirection();

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => false;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
            hints.AddForbiddenZone(new SDInvertedCircle(spot, 1.5f), Casters.Ref(0).Activation);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Casters.Count != 0 && !actor.Position.InCircle(spot, 1.5f))
            hints.Add("Stand north-west of the center, the knockback throws you onto the tail!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (Casters.Count != 0)
            Arena.AddCircleUnfilled(spot, 1.5f, Colors.Safe, 2f);
    }
}
sealed class TailSpitSmall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TailSpitSmall, 5f);

sealed class IceStormFreeze(BossModule module) : Components.StayMove(module)
{
    private DateTime _iceStorm;
    private Requirement _requirement;

    private void Refresh()
    {
        if (_iceStorm == default || _requirement == Requirement.None)
            return;
        var party = Raid.WithSlot();
        for (var i = 0; i < party.Length; ++i)
            PlayerStates[party[i].Item1] = new(_requirement, _iceStorm);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.IceStormVisualP3:
                _iceStorm = Module.CastFinishAt(spell, 0.6d);
                break;
            case (uint)AID.HypernovaVisualP3:
                _requirement = Requirement.Stay2;
                break;
            case (uint)AID.LevinboltVisualP3:
                _requirement = Requirement.Move;
                break;
            default:
                return;
        }
        Refresh();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.IceStorm && _iceStorm != default)
        {
            _iceStorm = default;
            _requirement = Requirement.None;
            Array.Fill(PlayerStates, default);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (PlayerStates[slot].Requirement == Requirement.Stay2)
            hints.Add("Stand still for Ice Storm, Hypernova thaws you", false);
        else if (PlayerStates[slot].Requirement == Requirement.Move)
            hints.Add("Keep moving through Ice Storm, Levinbolt drops under you", false);
    }
}

sealed class HypernovaP3(BossModule module) : Components.CastHint(module, (uint)AID.HypernovaVisualP3, "Stack together")
{
    private WPos StackSpot()
    {
        var party = Raid.WithoutSlot();
        var len = party.Length;
        if (len == 0)
            return Arena.Center;
        var sum = new WDir();
        for (var i = 0; i < len; ++i)
            sum += party[i].Position - Arena.Center;
        return Arena.Center + sum / len;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
            hints.AddForbiddenZone(new SDInvertedCircle(StackSpot(), 2f), Module.CastFinishAt(Casters[0].CastInfo));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Casters.Count != 0)
            Arena.AddCircleUnfilled(StackSpot(), 2f, Colors.Safe);
    }
}

sealed class LevinboltP3Hint(BossModule module) : Components.UniformStackSpread(module, default, 5f, raidwideOnResolve: false)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LevinboltVisualP3)
            AddSpreads(Raid.WithoutSlot(), Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LevinboltVisualP3)
            Spreads.Clear();
    }
}

sealed class DoomPuddle(BossModule module) : BossComponent(module)
{
    private const float Radius = 14f;
    private static readonly WPos back = new(2f, -8f);
    private readonly HashSet<ulong> _activePads = [];
    private static readonly uint[] padOIDs = [(uint)OID.JumpPadWest, (uint)OID.JumpPadEast];

    private bool OnBack(Actor actor) => actor.PosRot.Y > Module.PrimaryActor.PosRot.Y + 4f;
    private bool WormwailActive => Module.PrimaryActor.CastInfo?.IsSpell(AID.Wormwail) == true;

    private bool PuddleActive()
    {
        var puddles = Module.Enemies((uint)OID.DoomPuddle);
        var count = puddles.Count;
        for (var i = 0; i < count; ++i)
        {
            if (puddles[i].EventState != 7)
                return true;
        }
        return false;
    }

    private Actor? ClosestPad(Actor actor)
    {
        Actor? best = null;
        var bestDist = float.MaxValue;
        foreach (var oid in padOIDs)
        {
            var pads = Module.Enemies(oid);
            var count = pads.Count;
            for (var i = 0; i < count; ++i)
            {
                var p = pads[i];
                if (!_activePads.Contains(p.InstanceID))
                    continue;
                var d = (p.Position - actor.Position).LengthSq();
                if (d < bestDist)
                {
                    bestDist = d;
                    best = p;
                }
            }
        }
        return best;
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID is (uint)OID.JumpPadWest or (uint)OID.JumpPadEast)
        {
            if (state == 0x00040008u)
                _activePads.Add(actor.InstanceID);
            else if (state == 0x00010002u)
                _activePads.Remove(actor.InstanceID);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (OnBack(actor))
        {
            if (_activePads.Count != 0)
                hints.AddForbiddenZone(new SDInvertedCircle(back, 4f));
            return;
        }
        if (!WormwailActive && PuddleActive())
            hints.AddForbiddenZone(new SDCircle(Module.PrimaryActor.Position, Radius));
        if (ClosestPad(actor) is Actor pad)
            hints.AddForbiddenZone(new SDInvertedCircle(pad.Position, 2f));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (OnBack(actor))
            return;
        if (actor.FindStatus((uint)SID.Doom) != null)
            hints.Add("Doom: get out of the puddle!");
        if (ClosestPad(actor) != null)
            hints.Add("Jump onto Shinryu's back from a pad!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!OnBack(pc) && !WormwailActive && PuddleActive())
            Arena.ZoneCircle(Module.PrimaryActor.Position, Radius, Colors.AOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (OnBack(pc))
            return;
        if (WormwailActive && PuddleActive())
            Arena.AddCircleUnfilled(Module.PrimaryActor.Position, Radius, Colors.Danger);
        foreach (var oid in padOIDs)
        {
            var pads = Module.Enemies(oid);
            var count = pads.Count;
            for (var i = 0; i < count; ++i)
            {
                var p = pads[i];
                if (_activePads.Contains(p.InstanceID))
                    Arena.AddCircleUnfilled(p.Position, 2f, Colors.Safe, 2f);
            }
        }
    }
}

// three arenas: the 3x3 tile platform (y=-380), the 60 square below during phase 2 (y=620, z -100 to -40),
// the 80 square after shatter (y=620, centered). edges measured from the 2026-10-18 replay. the boss y tells the first apart, shatter/dark matter the other two.
class ArenaChanges(BossModule module) : BossComponent(module)
{
    private int _arena = -1;
    private bool _p3, _touchdown;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DarkMatter)
            _p3 = false;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Shatter)
            _p3 = true;
        else if (spell.Action.ID == (uint)AID.TouchdownAOE)
            _touchdown = true;
    }

    public override void Update()
    {
        var arena = _p3 ? 2 : _touchdown && Module.PrimaryActor.PosRot.Y >= 0f ? 1 : 0;
        if (arena == _arena)
            return;
        _arena = arena;
        switch (arena)
        {
            case 0:
                Arena.Center = default;
                Arena.Bounds = new ArenaBoundsSquare(30f, mapResolution: 0.3f);
                break;
            case 1:
                Arena.Center = new(0f, -70f);
                Arena.Bounds = new ArenaBoundsSquare(30f);
                break;
            default:
                Arena.Center = default;
                Arena.Bounds = new ArenaBoundsSquare(40f);
                break;
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Le-Vagabond (parts ported from upstream and modified by LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1118, NameID = 5640, PlanLevel = 100)]
public class Un5Shinryu(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsSquare(30f, mapResolution: 0.3f));
