namespace BossMod.Dawntrail.Savage.M04SWickedThunder;

sealed class WideningNarrowingWitchHunt(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [with(4)];

    private static readonly AOEShapeCircle _shapeOut = new(10f);
    private static readonly AOEShapeDonut _shapeIn = new(10f, 60f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs.Count != 0 ? CollectionsMarshal.AsSpan(AOEs)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var (first, second) = spell.Action.ID switch
        {
            (uint)AID.WideningWitchHunt => (_shapeOut, _shapeIn),
            (uint)AID.NarrowingWitchHunt => (_shapeIn, _shapeOut),
            _ => ((AOEShape?)null, (AOEShape?)null)
        };
        if (first != null && second != null)
        {
            var pos = spell.LocXZ;
            AOEs.Add(new(first, pos, default, Module.CastFinishAt(spell, 1.1f)));
            AOEs.Add(new(second, pos, default, Module.CastFinishAt(spell, 4.6f)));
            AOEs.Add(new(first, pos, default, Module.CastFinishAt(spell, 8.1f)));
            AOEs.Add(new(second, pos, default, Module.CastFinishAt(spell, 11.6f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.LightningVortexAOE or (uint)AID.ThunderingAOE)
        {
            ++NumCasts;
            if (AOEs.Count != 0)
                AOEs.RemoveAt(0);
        }
    }
}

sealed class WideningNarrowingWitchHuntBait(BossModule module) : Components.GenericBaitAway(module, (uint)AID.WitchHuntAOE, centerAtTarget: true)
{
    public enum Mechanic { None, Near, Far }

    public Mechanic CurMechanic;
    private DateTime _activation;

    private static readonly AOEShapeCircle _shape = new(6f);

    private (Actor actor, float distSq)[] _distances = new (Actor, float)[PartyState.MaxPartySize];

    public override void Update()
    {
        if (CurMechanic == Mechanic.None)
            return;

        CurrentBaits.Clear();

        var party = Raid.WithoutSlot(false, true, true);
        var len = party.Length;

        if (_distances.Length < len)
            _distances = new (Actor, float)[len];
        var distances = _distances;
        var center = Arena.Center;

        for (var i = 0; i < len; ++i)
        {
            var p = party[i];
            var distSq = (p.Position - center).LengthSq();
            distances[i] = (p, distSq);
        }

        var isNear = CurMechanic == Mechanic.Near;

        var targets = Math.Min(2, len);
        for (var i = 0; i < targets; ++i)
        {
            var selIdx = i;
            for (var j = i + 1; j < len; ++j)
            {
                var distJSq = distances[j].distSq;
                var distSelIdx = distances[selIdx].distSq;
                var isBetter = isNear ? distJSq < distSelIdx : distJSq > distSelIdx;
                if (isBetter)
                    selIdx = j;
            }

            if (selIdx != i)
            {
                (distances[selIdx], distances[i]) = (distances[i], distances[selIdx]);
            }

            CurrentBaits.Add(new(Module.PrimaryActor, distances[i].actor, _shape, _activation));
        }
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (CurMechanic != Mechanic.None)
            hints.Add($"Next bait: {CurMechanic}");
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Marker && CurMechanic == Mechanic.None)
        {
            _activation = status.ExpireAt.AddSeconds(12.2d);
            CurMechanic = status.Extra switch
            {
                0x2F6 => Mechanic.Near,
                0x2F7 => Mechanic.Far,
                _ => Mechanic.None
            };
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            ForbiddenPlayers[Raid.FindSlot(spell.MainTargetID)] = true;
            _activation = WorldState.FutureTime(3.5d);
            if ((NumCasts & 1) == 0)
                CurMechanic = CurMechanic == Mechanic.Near ? Mechanic.Far : Mechanic.Near;
        }
    }
}
