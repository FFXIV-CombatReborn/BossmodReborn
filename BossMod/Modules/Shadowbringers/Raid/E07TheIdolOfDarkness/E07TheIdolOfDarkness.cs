namespace BossMod.Shadowbringers.Raid.E07TheIdolOfDarkness;

public enum OID : uint
{
    Boss = 0x2D2D, // R28.000, The Idol of Darkness
    Helper = 0x233C, // R0.500
    Voidgate = 0x18D6, // R0.500, red/blue portals
    UnforgivenIdolatryLight = 0x2D2E, // R3.000
    UnforgivenIdolatryDark = 0x2E0A, // R3.000
    Idolatry = 0x2D30, // R3.000, phase 2 add
    Idolatry2 = 0x2D31, // R3.000, phase 2 add
    BossPart1 = 0x2D3A, // R1.000, Part
    BossPart2 = 0x2D3B, // R1.000, Part
}

public enum AID : uint
{
    AutoAttack = 19536, // Helper->player, no cast, single-target
    EmptyWave = 19538, // Boss->self, 4.0s cast, range 100 circle
    UnshadowedStake = 19537, // Boss->self, 5.0s cast, single-target
    StygianStake = 19507, // Helper->player, no cast, range 100 width 6 rect
    WordsOfMotion = 19499, // Boss->self, 2.5s cast, single-target
    LightsCourseVisual = 19511, // UnforgivenIdolatryLight->self, no cast, single-target
    LightsCourse = 19516, // UnforgivenIdolatryLight->self, 7.0s cast, range 50 width 10 rect
    LightsCourseShort = 20067, // UnforgivenIdolatryLight->self, 7.0s cast, range 25 width 10 rect (into portal)
    LightsCoursePortal = 19518, // UnforgivenIdolatryLight->self, 7.0s cast, range 50 width 10 rect (into portal)
    LightsCourseNight = 19490, // UnforgivenIdolatryLight->self, 2.0s cast, range 50 width 10 rect
    LightsCourseExit = 19520, // UnforgivenIdolatryLight->self, no cast, range 55 width 10 rect (portal exit)
    BetwixtWorlds = 19704, // Boss->self, no cast, single-target (portals)
    AwayWithThee = 19513, // Boss->self, 3.0s cast, single-target
    AwayWithTheeAdds = 20094, // Boss->self, 3.0s cast, single-target (phase 2)
    FalseTwilight = 19545, // Boss->self, 3.0s cast, single-target
    StygianSword = 19541, // Helper->location, 4.0s cast, range 6 circle
    SilverSledge = 19540, // Helper->player, 5.0s cast, range 6 circle
    UnjoinedAspect = 19515, // Boss->self, 4.0s cast, single-target
    UnjoinedAspectApply = 19510, // Helper->self, no cast, single-target
    WordsOfNight = 19500, // Boss->self, 2.5s cast, single-target
    DarksCourseVisual = 19512, // UnforgivenIdolatryDark->self, no cast, single-target
    DarksCourse = 19517, // UnforgivenIdolatryDark->self, 7.0s cast, range 50 width 10 rect
    DarksCourseNight = 19491, // UnforgivenIdolatryDark->self, 2.0s cast, range 50 width 10 rect
    DarksCoursePortal = 19521, // UnforgivenIdolatryDark->self, 7.0s cast, range 50 width 10 rect (into portal)
    DarksCourseExit = 19523, // UnforgivenIdolatryDark->self, no cast, range 55 width 10 rect (portal exit)
    IdolatryAppear = 19514, // Idolatry/Idolatry2->self, no cast, single-target
    AutoAttackAdd = 19529, // Idolatry/Idolatry2->player, no cast, single-target
    StrengthInNumbersDonut = 19532, // Idolatry/Idolatry2->self, 11.0s cast, range 8-20 donut
    StrengthInNumbersCircle = 19533, // Idolatry/Idolatry2->self, 5.0s cast, range 12 circle
    SilverShot = 19602, // Helper->players, 8.0s cast, range 6 circle
    SilverShot2 = 20093, // Helper->players, 8.0s cast, range 6 circle
    OverwhelmingForce = 19534, // Idolatry->self, 3.0s cast, single-target (enrage)
    OverwhelmingForceAOE = 19535, // Idolatry->self, 3.0s cast, range 100 circle
    Explosion = 19531, // Idolatry2/Idolatry->self, 8.0s cast, range 100 circle (enrage)
    EmptyFlood = 20054, // Boss->self, 7.0s cast, single-target
    EmptyFloodAOE = 20052, // Helper->self, 7.0s cast, range 100 circle
    EmptyFloodAlt = 19539, // Boss->self, 7.0s cast, single-target
    BlackSmoke = 19542, // Helper->self, no cast, range 5 circle (tower soak)
    BlackBlaze = 19543, // Helper->self, no cast, range 100 circle (tower fail)
    SilverShotVisual = 20092, // Boss->self, 3.0s cast, single-target
    //_Ability_ = 19504, // Boss->self, no cast, single-target
    //_Ability_1 = 19722, // Boss->self, no cast, single-target
    //_Ability_2 = 19506, // Boss->self, no cast, single-target
}

public enum SID : uint
{
    AstralEffect = 2238, // none->player, light aspect — soak dark
    UmbralEffect = 2239, // none->player, dark aspect — soak light
    ForwardWithThee = 2240, // none->player
    BackWithThee = 2241, // none->player
    LeftWithThee = 2242, // none->player
    RightWithThee = 2243, // none->player
}

public enum TetherID : uint
{
    UnshadowedStake = 37, // Boss->player
    Voidgate = 113, // Voidgate->Boss
}

sealed class EmptyWave(BossModule module) : Components.RaidwideCast(module, (uint)AID.EmptyWave);
sealed class EmptyFlood(BossModule module) : Components.RaidwideCast(module, (uint)AID.EmptyFloodAOE);

sealed class UnshadowedStake(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(100, 3), (uint)TetherID.UnshadowedStake, (uint)AID.StygianStake)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.UnshadowedStake)
        {
            var act = Module.CastFinishAt(spell);
            foreach (ref var b in CurrentBaits.AsSpan())
                b.Activation = act;
        }
    }
}

sealed class StygianSword(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StygianSword, 6);
sealed class SilverSledge(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.SilverSledge, 6f, 2, 4);

sealed class SilverShot(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.SilverShot, 6);
sealed class SilverShot2(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.SilverShot2, 6);

sealed class StrengthInNumbersDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StrengthInNumbersDonut, new AOEShapeDonut(8, 20))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // don't add hints if you have AwayWithThee cause they fight each other
        if (Module.FindComponent<AwayWithThee>()?.ActiveFor(actor) == true)
            return;
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

sealed class StrengthInNumbersCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StrengthInNumbersCircle, 12)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // don't add hints if you have AwayWithThee cause they fight each other
        if (Module.FindComponent<AwayWithThee>()?.ActiveFor(actor) == true)
            return;
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

sealed class Explosion(BossModule module) : Components.CastHint(module, (uint)AID.Explosion, "Kill adds!", true);

sealed class AwayWithThee(BossModule module) : Components.GenericKnockback(module)
{
    private readonly List<(Actor Target, uint Sid, DateTime Activation)> _sources = [];
    private const float Distance = 15f;
    private BlackSmoke? _towers;

    public bool ActiveFor(Actor actor)
    {
        for (var i = 0; i < _sources.Count; ++i)
        {
            if (_sources[i].Target == actor)
                return true;
        }
        return false;
    }

    private static WDir DirAt(uint sid, Angle rotation) => sid switch
    {
        (uint)SID.ForwardWithThee => rotation.ToDirection(),
        (uint)SID.BackWithThee => (rotation + 180.Degrees()).ToDirection(),
        (uint)SID.LeftWithThee => rotation.ToDirection().OrthoL(),
        (uint)SID.RightWithThee => rotation.ToDirection().OrthoR(),
        _ => default
    };

    // the push direction is relative to wherever the player is facing when it resolves
    private readonly Knockback[] _oneKnockback = new Knockback[1];
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        for (var i = 0; i < _sources.Count; ++i)
        {
            var s = _sources[i];
            if (s.Target != actor)
                continue;
            var dir = DirAt(s.Sid, actor.Rotation);
            if (dir != default)
            {
                _oneKnockback[0] = new(actor.Position, Distance, s.Activation, null, dir.ToAngle(), Kind.DirForward, ignoreImmunes: true);
                return _oneKnockback;
            }
        }
        return [];
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.ForwardWithThee or (uint)SID.BackWithThee or (uint)SID.LeftWithThee or (uint)SID.RightWithThee)
        {
            _sources.Add((actor, status.ID, status.ExpireAt));
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.ForwardWithThee or (uint)SID.BackWithThee or (uint)SID.LeftWithThee or (uint)SID.RightWithThee)
        {
            for (var i = _sources.Count - 1; i >= 0; --i)
            {
                if (_sources[i].Target == actor)
                    _sources.RemoveAt(i);
            }
        }
    }

    // while the Black Smoke towers are up, the knockback has to land in one of them
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (base.DestinationUnsafe(slot, actor, pos))
            return true;
        _towers ??= Module.FindComponent<BlackSmoke>();
        if (_towers == null || _towers.Towers.Count == 0)
            return false;
        var towers = CollectionsMarshal.AsSpan(_towers.Towers);
        for (var i = 0; i < towers.Length; ++i)
        {
            if (towers[i].IsInside(pos))
                return false;
        }
        return true;
    }
}

sealed class LightsAndDarksCourse(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(AOEInstance AOE, bool Astral, bool PortalExit, int Wave)> _aoes = [];
    private readonly List<(WPos Pos, Angle Rot)> _pendingExitPortals = [];
    private readonly List<(WPos Pos, Angle Rot)> _exitPortals = [];
    private readonly List<(bool Astral, int Wave, DateTime Finish)> _pendingPortalEntries = [];
    private int _portalEntries;
    private int _nextWave;
    private DateTime _lastEntryFinish;

    private readonly List<AOEInstance> _visible = [];
    private static readonly AOEShapeRect _shape50 = new(50, 5);
    private static readonly AOEShapeRect _shape25 = new(25, 5);
    private static readonly AOEShapeRect _shape55 = new(55, 5);

    public override void Update()
    {
        var cutoff = WorldState.CurrentTime.AddSeconds(-0.5d); // I have no idea but sometimes they just don't resolve
        for (var i = _aoes.Count - 1; i >= 0; --i)
        {
            if (_aoes[i].AOE.Activation < cutoff)
                _aoes.RemoveAt(i);
        }

        if (_pendingPortalEntries.Count > 0 && _exitPortals.Count > 0)
        {
            // same-wave mixed, offset light first then dark
            _pendingPortalEntries.Sort(static (a, b) => a.Finish != b.Finish ? a.Finish.CompareTo(b.Finish) : b.Astral.CompareTo(a.Astral));
            for (var i = 0; i < _pendingPortalEntries.Count; ++i)
            {
                var e = _pendingPortalEntries[i];
                PredictPortalExits(e.Astral, e.Wave, e.Finish);
            }
            _pendingPortalEntries.Clear();
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var astral = actor.FindStatus((uint)SID.AstralEffect) != null;
        var umbral = actor.FindStatus((uint)SID.UmbralEffect) != null;
        // earliest activation among the lanes this player has to care about (light soaks dark, dark soaks light)
        var first = DateTime.MaxValue;
        for (var i = 0; i < _aoes.Count; ++i)
        {
            var a = _aoes[i];
            if (!(astral && !a.Astral || umbral && a.Astral) && a.AOE.Activation < first)
                first = a.AOE.Activation;
        }
        _visible.Clear();
        if (first == DateTime.MaxValue)
            return [];

        var window = first.AddSeconds(0.5d);
        var entranceUp = false;
        for (var i = 0; i < _aoes.Count; ++i)
        {
            var a = _aoes[i];
            if (!(astral && !a.Astral || umbral && a.Astral) && a.AOE.Activation <= window && !a.PortalExit)
                entranceUp = true;
        }
        for (var i = 0; i < _aoes.Count; ++i)
        {
            var a = _aoes[i];
            if (!(astral && !a.Astral || umbral && a.Astral) && a.AOE.Activation <= window && (!entranceUp || !a.PortalExit))
                _visible.Add(a.AOE);
        }
        _visible.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
        return CollectionsMarshal.AsSpan(_visible);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var astral = actor.FindStatus((uint)SID.AstralEffect) != null;
        var umbral = actor.FindStatus((uint)SID.UmbralEffect) != null;
        if ((astral || umbral) && _aoes.Count > 0)
            hints.Add(astral ? "Soak dark lane" : "Soak light lane");
        base.AddHints(slot, actor, hints);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BetwixtWorlds:
                _exitPortals.Clear();
                _exitPortals.AddRange(_pendingExitPortals);
                _pendingExitPortals.Clear();
                _pendingPortalEntries.Clear();
                _portalEntries = 0;
                _nextWave = 0;
                _lastEntryFinish = default;
                for (var i = _aoes.Count - 1; i >= 0; --i)
                {
                    if (_aoes[i].PortalExit)
                        _aoes.RemoveAt(i);
                }
                break;
            case (uint)AID.LightsCourseExit:
            case (uint)AID.DarksCourseExit:
                NumCasts++;
                RemoveClosestPortalExit(caster.Position);
                break;
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID != (uint)TetherID.Voidgate || source.OID != (uint)OID.Voidgate)
            return;
        // N/S are always entry voidgates, E/W are always exits. Just easier this way
        if (Math.Abs(source.Position.X - Arena.Center.X) <= 15)
            return;
        for (var i = 0; i < _pendingExitPortals.Count; ++i)
        {
            if (_pendingExitPortals[i].Pos.AlmostEqual(source.Position, 1f))
                return;
        }
        _pendingExitPortals.Add((source.Position, source.Rotation));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var aid = spell.Action.ID;
        if (aid is not ((uint)AID.LightsCourse or (uint)AID.LightsCoursePortal or (uint)AID.LightsCourseNight or (uint)AID.LightsCourseShort or (uint)AID.DarksCourse or (uint)AID.DarksCoursePortal or (uint)AID.DarksCourseNight))
            return;

        var activation = Module.CastFinishAt(spell);
        var wave = WaveFor(activation);
        switch (aid)
        {
            case (uint)AID.LightsCourse:
            case (uint)AID.LightsCoursePortal:
            case (uint)AID.LightsCourseNight:
                _aoes.Add((new(_shape50, caster.Position, spell.Rotation, activation), true, false, wave));
                break;
            case (uint)AID.LightsCourseShort:
                _aoes.Add((new(_shape25, caster.Position, spell.Rotation, activation), true, false, wave));
                break;
            case (uint)AID.DarksCourse:
            case (uint)AID.DarksCoursePortal:
            case (uint)AID.DarksCourseNight:
                _aoes.Add((new(_shape50, caster.Position, spell.Rotation, activation), false, false, wave));
                break;
        }

        if (aid is (uint)AID.LightsCourseShort or (uint)AID.LightsCoursePortal or (uint)AID.DarksCoursePortal)
            _pendingPortalEntries.Add((aid is not (uint)AID.DarksCoursePortal, wave, activation));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LightsCourse:
            case (uint)AID.LightsCourseShort:
            case (uint)AID.LightsCoursePortal:
            case (uint)AID.LightsCourseNight:
            case (uint)AID.DarksCourse:
            case (uint)AID.DarksCoursePortal:
            case (uint)AID.DarksCourseNight:
                for (var i = _aoes.Count - 1; i >= 0; --i)
                {
                    var a = _aoes[i];
                    if (!a.PortalExit && a.AOE.Origin.AlmostEqual(caster.Position, 1f) && a.AOE.Rotation.AlmostEqual(spell.Rotation, 0.1f))
                        _aoes.RemoveAt(i);
                }
                break;
        }
    }

    private void RemoveClosestPortalExit(WPos pos)
    {
        var best = -1;
        var bestDist = float.MaxValue;
        for (var i = 0; i < _aoes.Count; ++i)
        {
            if (!_aoes[i].PortalExit)
                continue;
            var d = (_aoes[i].AOE.Origin - pos).LengthSq();
            if (d < bestDist)
            {
                bestDist = d;
                best = i;
            }
        }
        if (best >= 0)
            _aoes.RemoveAt(best);
    }

    private int WaveFor(DateTime activation)
    {
        foreach (var a in _aoes)
            if (!a.PortalExit && Math.Abs((a.AOE.Activation - activation).TotalSeconds) < 0.5)
                return a.Wave;
        return _nextWave++;
    }

    private void PredictPortalExits(bool astral, int wave, DateTime castFinish)
    {
        if (_exitPortals.Count == 0)
            return;

        // words of night + multi-portal resolves faster than words of motion portal redirects
        var night = false;
        var party = Raid.WithoutSlot();
        for (var i = 0; i < party.Length; ++i)
        {
            var p = party[i];
            if (p.FindStatus((uint)SID.AstralEffect) != null || p.FindStatus((uint)SID.UmbralEffect) != null)
            {
                night = true;
                break;
            }
        }
        var delay = night ? 3.2f : 6.1f;
        if (_portalEntries > 0 && Math.Abs((castFinish - _lastEntryFinish).TotalSeconds) < 0.5)
            delay += 3.1f;
        _lastEntryFinish = castFinish;
        var act = castFinish.AddSeconds(delay);
        var sign = (_portalEntries % 2 == 0) ? 1 : -1;

        foreach (var (pos, rot) in _exitPortals)
        {
            // both facings emit +Z on the first entry. Second entry takes the other lane.
            var origin = new WPos(pos.X, pos.Z + 5f * sign);
            _aoes.Add((new(_shape55, origin, rot, act), astral, true, wave));
        }
        _portalEntries++;
    }
}

sealed class BlackSmoke(BossModule module) : Components.GenericTowers(module, (uint)AID.BlackSmoke)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (state != 0x00200010)
            return;
        var pos = index switch
        {
            0x0E => new WPos(100, 93),
            0x0F => new WPos(100, 107),
            _ => default
        };
        if (pos == default)
            return;
        for (var i = 0; i < Towers.Count; ++i)
        {
            if (Towers[i].Position.AlmostEqual(pos, 1f))
                return;
        }
        Towers.Add(new(pos, 5f, activation: WorldState.FutureTime(15.1d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.BlackSmoke or (uint)AID.BlackBlaze)
        {
            NumCasts++;
            Towers.Clear();
        }
    }
}

sealed class IdolatryAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.Idolatry, (uint)OID.Idolatry2]);

sealed class E07TheIdolOfDarknessStates : StateMachineBuilder
{
    public E07TheIdolOfDarknessStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EmptyWave>()
            .ActivateOnEnter<EmptyFlood>()
            .ActivateOnEnter<UnshadowedStake>()
            .ActivateOnEnter<LightsAndDarksCourse>()
            .ActivateOnEnter<AwayWithThee>()
            .ActivateOnEnter<StygianSword>()
            .ActivateOnEnter<SilverSledge>()
            .ActivateOnEnter<SilverShot>()
            .ActivateOnEnter<SilverShot2>()
            .ActivateOnEnter<StrengthInNumbersDonut>()
            .ActivateOnEnter<StrengthInNumbersCircle>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<BlackSmoke>()
            .ActivateOnEnter<IdolatryAdds>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "croizat (ported from upstream and modified by LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 726, NameID = 9298)]
public sealed class E07TheIdolOfDarkness(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));
