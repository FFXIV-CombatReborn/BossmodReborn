namespace BossMod.Dawntrail.Savage.M01SBlackCat;

// same component covers normal, leaping and leaping clone versions
sealed class QuadrupleCrossingProtean(BossModule module) : Components.GenericBaitAway(module)
{
    public WPos? Origin;
    private DateTime _activation;
    private Actor? _clone;
    private Angle _jumpDirection;

    private static readonly AOEShapeCone cone = new(100f, 22.5f.Degrees());

    private (Actor actor, float distSq)[] _distances = new (Actor, float)[PartyState.MaxPartySize];

    public override void Update()
    {
        CurrentBaits.Clear();
        if (Origin is WPos origin && _activation != default)
        {
            var party = Raid.WithoutSlot(false, true, true);
            var len = party.Length;

            if (_distances.Length < len)
                _distances = new (Actor, float)[len];
            var distances = _distances;

            for (var i = 0; i < len; ++i)
            {
                var p = party[i];
                var distSq = (p.Position - origin).LengthSq();
                distances[i] = (p, distSq);
            }

            var targets = Math.Min(4, len);
            for (var i = 0; i < targets; ++i)
            {
                var selIdx = i;
                for (var j = i + 1; j < len; ++j)
                {
                    if (distances[j].distSq < distances[selIdx].distSq)
                        selIdx = j;
                }

                if (selIdx != i)
                {
                    (distances[selIdx], distances[i]) = (distances[i], distances[selIdx]);
                }

                CurrentBaits.Add(new(origin, distances[i].actor, cone, _activation));
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (_clone != null && CurrentBaits.Count == 0)
            Arena.Actor(_clone.Position + 10f * (_clone.Rotation + _jumpDirection).ToDirection(), _clone.Rotation, Colors.Object);
    }

    public override void OnActorCreated(Actor actor)
    {
        // note: tether target is created after boss is tethered...
        if (actor.OID == (uint)OID.LeapTarget && Module.PrimaryActor.Tether.Target == actor.InstanceID)
        {
            Origin = actor.Position;
            _jumpDirection = Angle.FromDirection(actor.Position - Module.PrimaryActor.Position) - (Module.PrimaryActor.CastInfo?.Rotation ?? Module.PrimaryActor.Rotation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.QuadrupleCrossingFirst:
                Origin = spell.LocXZ;
                _activation = Module.CastFinishAt(spell, 0.8d);
                break;
            case (uint)AID.LeapingQuadrupleCrossingBossL:
            case (uint)AID.LeapingQuadrupleCrossingBossR:
                // origin will be set to leap target when it's created
                _activation = Module.CastFinishAt(spell, 1.8d);
                break;
            case (uint)AID.NailchipperAOE:
                if (NumCasts == 8)
                    ForbiddenPlayers.Set(Raid.FindSlot(spell.TargetID));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.QuadrupleCrossingProtean or (uint)AID.LeapingQuadrupleCrossingBossProtean or (uint)AID.LeapingQuadrupleCrossingShadeProtean)
        {
            if (NumCasts == 8)
            {
                ForbiddenPlayers = default; // third set => clear nailchippers
            }

            _activation = WorldState.FutureTime(3d);
            var targets = CollectionsMarshal.AsSpan(spell.Targets);
            var len = targets.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var targ = ref targets[i];
                ForbiddenPlayers.Set(Raid.FindSlot(targ.ID));
            }

            if (++NumCasts is 8 or 16)
            {
                Origin = null;
                ForbiddenPlayers = default;
            }
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (NumCasts < 8 || tether.ID != (uint)TetherID.Soulshade)
            return; // not relevant tether

        if (_clone == null)
        {
            _clone = source;
        }
        else if (_clone == source)
        {
            var origin = source.Position + 10f * (source.Rotation + _jumpDirection).ToDirection();
            Origin = new(origin.X, origin.Z);
            _activation = WorldState.FutureTime(17d);
        }
    }
}

sealed class QuadrupleCrossingAOE(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(8)];
    private bool ready;
    private static readonly AOEShapeCone _shape = new(100f, 22.5f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!ready)
            return [];
        var count = _aoes.Count;
        var max = count > 4 ? 4 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        for (var i = 0; i < max; ++i)
        {
            aoes[i].Risky = true;
        }
        return aoes;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.QuadrupleCrossingProtean:
            case (uint)AID.LeapingQuadrupleCrossingBossProtean:
            case (uint)AID.LeapingQuadrupleCrossingShadeProtean:
                _aoes.Add(new(_shape, caster.Position.Quantized(), caster.Rotation, WorldState.FutureTime(5.9d), _aoes.Count < 4 ? Colors.Danger : default, risky: false));
                break;
            case (uint)AID.QuadrupleCrossingAOE:
            case (uint)AID.LeapingQuadrupleCrossingBossAOE:
            case (uint)AID.LeapingQuadrupleCrossingShadeAOE:
                ++NumCasts;
                if (_aoes.Count != 0)
                    _aoes.RemoveAt(0);
                break;
        }
        if (_aoes.Count == 8)
            ready = true;
    }
}
