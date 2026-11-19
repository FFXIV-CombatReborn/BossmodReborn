namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

sealed class Shockwave(BossModule module) : Components.RaidwideInstant(module, (uint)AID.Shockwave, 5.7d)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.ShockwaveVisual)
        {
            Activation = WorldState.FutureTime(Delay);
        }
    }
}

sealed class DerailmentSiege(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DerailmentSiegeVisual5 or (uint)AID.DerailmentSiegeVisual6 or (uint)AID.DerailmentSiegeVisual7 or (uint)AID.DerailmentSiegeVisual8)
        {
            Towers.Add(new(spell.LocXZ, 5f, 1, PartyState.MaxPartySize, activation: Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.DerailmentSiegeVisual5:
            case (uint)AID.DerailmentSiegeVisual6:
            case (uint)AID.DerailmentSiegeVisual7:
            case (uint)AID.DerailmentSiegeVisual8:
                ++NumCasts;
                Towers.Clear();
                break;
            case (uint)AID.DerailmentSiege1:
                ++NumCasts;
                break;
        }
    }
}

sealed class Derail(BossModule module) : Components.CastCounter(module, (uint)AID.Derail1);

sealed class Launchpad(BossModule module) : BossComponent(module)
{
    private WPos? _pad;
    private const float Radius = 2f;

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001u && index is >= 0x0A and <= 0x0C)
        {
            _pad = new(100f, 212.5f + 50f * (index - 0x0A));
        }
    }

    private bool Relevant(Actor actor) => _pad is WPos pad && actor.PosRot.Z < pad.Z + Radius;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Relevant(pc))
        {
            Arena.ZoneCircle(_pad!.Value, Radius, Colors.SafeFromAOE);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Relevant(actor))
        {
            hints.Add("Get to launchpad!", !actor.Position.InCircle(_pad!.Value, Radius));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Relevant(actor))
        {
            hints.AddForbiddenZone(new SDInvertedCircle(_pad!.Value, Radius), DateTime.MaxValue);
        }
    }
}

sealed class ThirdRail(BossModule module) : Components.GenericAOEs(module, (uint)AID.ThirdRail)
{
    private static readonly AOEShapeCircle circle = new(4f);
    private readonly List<AOEInstance> _aoes = [with(8)];
    private readonly List<bool> _high = [with(8)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var len = aoes.Length;
        var actorHigh = actor.PosRot.Y > 4f;
        for (var i = 0; i < len; ++i)
        {
            aoes[i].Risky = _high[i] == actorHigh;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _aoes.Add(new(circle, spell.LocXZ, default, Module.CastFinishAt(spell), actorID: caster.InstanceID));
            _high.Add(spell.Location.Y > 4f);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var count = _aoes.Count;
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    _high.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

sealed class ThirdRailBait(BossModule module) : BossComponent(module)
{
    private bool active = true;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (active)
        {
            Arena.ZoneCircleOutline(pc.Position, 4f, Colors.Danger);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ThirdRail)
        {
            active = false;
        }
    }
}

sealed class ZoomCounter(BossModule module) : Components.CastCounter(module, (uint)AID.Zoom);

sealed class Breathlight(BossModule module) : Components.GenericAOEs(module)
{
    private readonly ArenaChanges _arena = module.FindComponent<ArenaChanges>()!;
    private readonly List<(bool Air, DateTime Activation)> _casts = [with(2)];
    private AOEInstance[] _aoe = [];
    public bool Draw = true;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Draw ? _aoe : [];

    private void UpdateAOE()
    {
        if (_casts.Count == 0)
        {
            _aoe = [];
            return;
        }
        var (air, activation) = _casts[0];
        _aoe = [new(air ? _arena.AirShape : _arena.GroundShape, Arena.Center, default, activation)];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ThunderousBreathFirstVisual:
                AddCasts(false);
                break;
            case (uint)AID.HeadlightFirstVisual:
                AddCasts(true);
                break;
        }
        void AddCasts(bool airFirst)
        {
            _casts.Add((airFirst, Module.CastFinishAt(spell)));
            _casts.Add((!airFirst, Module.CastFinishAt(spell, 2.5d)));
            UpdateAOE();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.Headlight or (uint)AID.ThunderousBreath)
        {
            ++NumCasts;
            if (_casts.Count != 0)
            {
                _casts.RemoveAt(0);
                UpdateAOE();
            }
        }
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (!Draw && _casts.Count != 0)
        {
            hints.Add(_casts[0].Air ? "Next: ground safe" : "Next: platform safe");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_casts.Count != 0 && _casts[0].Air)
        {
            var portals = _arena.Portals;
            var count = portals.Count;
            var act = _casts[0].Activation;
            for (var i = 0; i < count; ++i)
            {
                hints.AddForbiddenZone(new SDCircle(portals[i].Entrance, 1.5f), act);
            }
        }
    }
}

sealed class Plummet(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Plummet, 8f);

sealed class HailOfThunder(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(16f);
    private static readonly WDir[] sources = [new(default, -10f), new(-4.8f, default), new(default, 10f), new(4.8f, default)];
    private readonly List<Actor> _indicators = module.Enemies((uint)OID.ArcaneRevelation);
    private AOEInstance[] _aoe = [];
    private DateTime _nextActivation;
    private int _advance;
    private int _lastPos;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void Update()
    {
        if (_advance == 0)
        {
            return;
        }
        if (_indicators.Count == 0)
        {
            ReportError("Indicator not found, can't predict AOE");
            _advance = 0;
            return;
        }
        var indicator = _indicators[0];
        var movement = indicator.LastFrameMovement;
        if (movement == default)
        {
            return;
        }
        var center = Arena.Center;
        var sign = MathF.Sign(movement.OrthoL().Dot(indicator.DirectionTo(center)));
        _lastPos = ((_lastPos + _advance * sign) % 4 + 4) % 4;
        var pos = (center + sources[_lastPos]).Quantized();
        _aoe = [new(circle, pos, default, _nextActivation, shapeDistance: circle.Distance(pos, default))];
        _advance = 0;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.HailOfThunderVisual2:
                Predict(2, 7.5d);
                break;
            case (uint)AID.HailOfThunderVisual3:
                Predict(3, 10.5d);
                break;
            case (uint)AID.HailOfThunderVisual1:
                Predict(4, 13.4d);
                break;
            case (uint)AID.HailOfThunder1:
                _advance = 0;
                _aoe = [];
                ++NumCasts;
                break;
        }
        void Predict(int steps, double delay)
        {
            _advance = steps;
            _nextActivation = WorldState.FutureTime(delay);
        }
    }
}

sealed class DesignatedConductor(BossModule module) : Components.GenericStackSpread(module)
{
    private BitMask _conductors;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.DesignatedConductor)
        {
            _conductors.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.DesignatedConductor)
        {
            _conductors.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.HailOfThunderVisual2:
                Predict(7.5d);
                break;
            case (uint)AID.HailOfThunderVisual3:
                Predict(10.5d);
                break;
            case (uint)AID.HailOfThunderVisual1:
                Predict(13.4d);
                break;
            case (uint)AID.HyperconductivePlasma:
                if (Stacks.Count != 0)
                {
                    Stacks.RemoveAt(0);
                }
                break;
        }
        void Predict(double delay)
        {
            var party = Raid.WithSlot(false, true, true);
            var len = party.Length;
            var act = WorldState.FutureTime(delay);
            for (var i = 0; i < len; ++i)
            {
                ref readonly var p = ref party[i];
                if (_conductors[p.Item1])
                {
                    Stacks.Add(new(p.Item2, 13f, 3, PartyState.MaxPartySize, act));
                }
            }
        }
    }
}
