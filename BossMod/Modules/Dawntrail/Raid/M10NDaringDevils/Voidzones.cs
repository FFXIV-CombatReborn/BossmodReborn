namespace BossMod.Dawntrail.Raid.M10NDaringDevils;

abstract class TrackedVoidzones(BossModule module, AOEShape shape, uint oid, bool hideUntilActivation = false) : Components.GenericAOEs(module)
{
    private const double PredictionTimeout = 3d;
    private readonly AOEShape _shape = shape;
    private readonly uint _oid = oid;
    private readonly bool _hideUntilActivation = hideUntilActivation;
    private readonly List<AOEInstance> _predicted = [];
    private readonly List<AOEInstance> _aoes = [];

    protected void Predict(WPos origin, Angle rotation, DateTime activation) => _predicted.Add(new(_shape, origin, rotation, activation));

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoes.Clear();
        var zones = Module.Enemies(_oid);
        var countZ = zones.Count;
        for (var i = 0; i < countZ; ++i)
        {
            var z = zones[i];
            if (z.EventState != 7)
            {
                _aoes.Add(new(_shape, z.Position, z.Rotation));
            }
        }
        var now = WorldState.CurrentTime;
        var countP = _predicted.Count;
        for (var i = 0; i < countP; ++i)
        {
            var p = _predicted[i];
            if (_hideUntilActivation && now < p.Activation || HasZoneAt(zones, p.Origin))
            {
                continue;
            }
            _aoes.Add(p);
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    private static bool HasZoneAt(List<Actor> zones, WPos pos)
    {
        var count = zones.Count;
        for (var i = 0; i < count; ++i)
        {
            var z = zones[i];
            if (z.EventState != 7 && z.Position.InCircle(pos, 1.5f))
            {
                return true;
            }
        }
        return false;
    }

    public override void Update()
    {
        var now = WorldState.CurrentTime;
        for (var i = _predicted.Count - 1; i >= 0; --i)
        {
            if (_predicted[i].Activation.AddSeconds(PredictionTimeout) < now)
            {
                _predicted.RemoveAt(i);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.DiversDare or (uint)AID.DiversDare1)
        {
            _predicted.Clear();
        }
    }
}
