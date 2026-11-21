namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME1TwoHeadedAevis;

sealed class BreathyDuet(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _lightning = [];
    private readonly List<Actor> _ice = [];
    private readonly Dictionary<ulong, List<WPos>> _clusters = [];
    private readonly AOEShapeCircle _circle = new(15f);
    private readonly AOEShapeCone _cone = new(45f, 30f.Degrees());
    private ulong _green = default;
    private ulong _blue = default;
    private DateTime _activation = default;

    // tether from boss helpers each to breathyduetactor to determine actor for cluster
    // icon position to determine position and order of cluster
    // icon and tether happen on same timestamp, either one can appear first
    // starts casting clusters while determining order
    // waves technically come out .1-.2s after cluster; will AI move into danger?
    // TODO: add/remove cone AOEs separately
    // 2nd tether Z-axis = safe spot CW, x-axis = safe spot CCW?

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_clusters.TryGetValue(_green, out var lightnings) && _clusters.TryGetValue(_blue, out var ices))
        {
            var aoes = _aoesBuffer;
            aoes.Clear();
            var activation = _activation.AddSeconds(NumCasts * 3.6d);

            if (lightnings.Count != 0)
            {
                var position = lightnings[0];
                aoes.Add(new(_circle, position, default, activation));
                AddCharmAOEs(aoes, _lightning, position, activation);
            }
            if (ices.Count != 0)
            {
                var position = ices[0];
                aoes.Add(new(_circle, position, default, activation));
                AddCharmAOEs(aoes, _ice, position, activation);
            }
            return CollectionsMarshal.AsSpan(aoes);
        }
        else
        {
            return [];
        }

        void AddCharmAOEs(List<AOEInstance> aoes, List<Actor> charmlist, WPos position, DateTime activation)
        {
            var charms = CollectionsMarshal.AsSpan(charmlist);
            var count = charms.Length;

            for (var i = 0; i < count; ++i)
            {
                ref var charm = ref charms[i];
                if (charm.Position.InCircle(position, 15f))
                {
                    aoes.Add(new(_cone, charm.Position, charm.Rotation, activation));
                }
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        switch (actor.OID)
        {
            case (uint)OID.CharmedLightning:
                _lightning.Add(actor);
                break;
            case (uint)OID.CharmedIce:
                _ice.Add(actor);
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (actor.OID == (uint)OID.BreathyDuetTarget)
        {
            if (iconID is (uint)IconID.Lockon1 or (uint)IconID.Lockon2 or (uint)IconID.Lockon3 or (uint)IconID.Lockon4)
            {
                var position = actor.Position;
                // source actor and target same
                if (_clusters.TryGetValue(targetID, out var value))
                {
                    value.Add(position);
                }
                else
                {
                    var pos = new List<WPos> { position };
                    _clusters[targetID] = pos;
                }
            }
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Tether)
        {
            if (source.OID is (uint)OID.GreenHead1 or (uint)OID.BlueHead1)
            {
                var targetID = tether.Target;
                var target = WorldState.Actors.Find(targetID);
                if (target?.OID == (uint)OID.BreathyDuetTarget)
                {
                    if (source.OID == (uint)OID.GreenHead1)
                    {
                        _green = targetID;
                    }
                    else if (source.OID == (uint)OID.BlueHead1)
                    {
                        _blue = targetID;
                    }
                }
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BreathyDuet)
        {
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LevinWave:
                _lightning.Remove(caster);
                break;
            case (uint)AID.IceWave:
                _ice.Remove(caster);
                break;
            case (uint)AID.LightningCluster1:
            case (uint)AID.LightningCluster2:
                ++NumCasts;
                if (_green != default && _clusters[_green].Count != 0)
                {
                    _clusters[_green].RemoveAt(0);
                }
                break;
            case (uint)AID.IceCluster1:
            case (uint)AID.IceCluster2:
                if (_blue != default && _clusters[_blue].Count != 0)
                {
                    _clusters[_blue].RemoveAt(0);
                }
                break;
        }
    }
}
