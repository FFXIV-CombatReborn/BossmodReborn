namespace BossMod.Shadowbringers.Ultimate.TEA;

// TODO: consider drawing tethers & shared sentence?..
sealed class P4FateCalibrationBetaDebuffs(BossModule module) : P4ForcedMarchDebuffs(module)
{
    private enum Color { Unknown, Light, Dark }

    private readonly P4FateProjection? _proj = module.FindComponent<P4FateProjection>();
    private readonly Color[] _colors = new Color[PartyState.MaxPartySize];
    private readonly int[] _farTethers = [-1, -1];
    private readonly int[] _nearTethers = [-1, -1];
    private int _sharedSentence = -1;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.FateCalibrationBeta:
                var fcbSlot = _proj?.ProjectionOwner(actor.InstanceID) ?? -1;
                if (fcbSlot >= 0)
                {
                    _colors[fcbSlot] = status.Extra switch
                    {
                        0x84 => Color.Light,
                        0x85 => Color.Dark,
                        _ => Color.Unknown
                    };
                }
                break;
            case (uint)SID.ContactProhibitionOrdained:
            case (uint)SID.ContactRegulationOrdained:
            case (uint)SID.EscapeProhibitionOrdained:
            case (uint)SID.EscapeDetectionOrdained:
                Done = true;
                break;
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        switch (tether.ID)
        {
            case (uint)TetherID.RestrainingOrder:
                _farTethers[0] = _proj?.ProjectionOwner(source.InstanceID) ?? -1;
                _farTethers[1] = _proj?.ProjectionOwner(tether.Target) ?? -1;
                break;
            case (uint)TetherID.HouseArrest:
                _nearTethers[0] = _proj?.ProjectionOwner(source.InstanceID) ?? -1;
                _nearTethers[1] = _proj?.ProjectionOwner(tether.Target) ?? -1;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FateCalibrationAlphaSharedSentence:
                _sharedSentence = _proj?.ProjectionOwner(spell.MainTargetID) ?? -1;
                // note: at this point, we can distinguish light beacon vs light untethered (shared sentence hits untethered), but not darks
                InitColor(Color.Light, GuessLightBeacon());
                break;
            case (uint)AID.FateCalibrationBetaKillBeaconSpread:
            case (uint)AID.FateCalibrationBetaKillBeaconStack:
                // these are always cast at two beacons
                var slot = _proj?.ProjectionOwner(spell.MainTargetID) ?? -1;
                if (slot >= 0)
                    InitColor(_colors[slot], slot);
                break;
        }
    }

    protected override WDir SafeSpotDirection(int slot) => Debuffs[slot] switch
    {
        Debuff.LightBeacon => new(-8f, -16f), // between N and NW
        Debuff.LightFollow => new(11f, -2f), // slightly N from E
        Debuff.DarkBeacon => new(14f, default), // E
        _ => new(11f, _nearTethers.Contains(slot) ? -2 : _farTethers.Contains(slot) ? +2f : default), // around E, depending on tether
    };

    private int GuessLightBeacon()
    {
        for (var i = 0; i < PartyState.MaxPartySize; ++i)
            if (_colors[i] == Color.Light && i != _sharedSentence && !_farTethers.Contains(i) && !_nearTethers.Contains(i))
                return i;
        return -1;
    }

    private void InitColor(Color color, int beacon)
    {
        for (var i = 0; i < PartyState.MaxPartySize; ++i)
        {
            if (_colors[i] != color)
                continue;

            if (i == beacon)
            {
                Debuffs[i] = color == Color.Light ? Debuff.LightBeacon : Debuff.DarkBeacon;
                if (color == Color.Light)
                    LightBeacon = Raid[i];
                else
                    DarkBeacon = Raid[i];
            }
            else
            {
                Debuffs[i] = color == Color.Light ? Debuff.LightFollow : Debuff.DarkFollow;
            }
        }
    }
}

sealed class P4FateCalibrationBetaJJump(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private bool _enabled;
    private readonly List<Actor> _jumpers = [];

    private readonly AOEShapeCircle _shape = new(10f);

    public void Show() => _enabled = true;

    public override void Update()
    {
        CurrentBaits.Clear();
        if (!_enabled)
            return;

        foreach (var source in _jumpers)
        {
            var target = Raid.WithoutSlot(false, true, true).FarthestFrom(source.Position);
            if (target != null)
                CurrentBaits.Add(new(source, target, _shape));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FateCalibrationBetaJump:
                _jumpers.Add(caster);
                break;
            case (uint)AID.FateCalibrationBetaJJump:
                ++NumCasts;
                _jumpers.Remove(caster);
                break;
        }
    }
}

sealed class P4FateCalibrationBetaOpticalSight(BossModule module) : Components.UniformStackSpread(module, 6f, 6f, 4, 4)
{
    private enum Mechanic { Unknown, Stack, Spread }

    public bool Done;
    private Mechanic _mechanic;
    private readonly List<Actor> _stackTargets = [];

    public void Show()
    {
        switch (_mechanic)
        {
            case Mechanic.Stack:
                AddStacks(_stackTargets, WorldState.FutureTime(6.1d));
                break;
            case Mechanic.Spread:
                AddSpreads(Raid.WithoutSlot(true, true, true), WorldState.FutureTime(6.1d));
                break;
        }
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (_mechanic != Mechanic.Unknown)
            hints.Add($"{_mechanic} after jumps");
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FateCalibrationBetaKillBeaconSpread:
                _mechanic = Mechanic.Spread;
                break;
            case (uint)AID.FateCalibrationBetaKillBeaconStack:
                _mechanic = Mechanic.Stack;
                var target = Raid[Module.FindComponent<P4FateProjection>()?.ProjectionOwner(spell.MainTargetID) ?? -1];
                if (target != null)
                    _stackTargets.Add(target);
                break;
            case (uint)AID.IndividualReprobation:
                Spreads.Clear();
                Done = true;
                break;
            case (uint)AID.CollectiveReprobation:
                Stacks.Clear();
                Done = true;
                break;
        }
    }
}

sealed class P4FateCalibrationBetaRadiantSacrament(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? _caster;
    private bool _enabled;

    private readonly AOEShapeDonut _shape = new(8f, 60f);

    public void Show() => _enabled = true;

    private readonly AOEInstance[] _oneAOEInstance = new AOEInstance[1];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_enabled && _caster != null)
        {
            _oneAOEInstance[0] = new(_shape, _caster.Position);
            return _oneAOEInstance;
        }
        return [];
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FateCalibrationBetaDonut:
                _caster = caster;
                break;
            case (uint)AID.FateCalibrationBetaRadiantSacrament:
                ++NumCasts;
                _caster = null;
                break;
        }
    }
}
