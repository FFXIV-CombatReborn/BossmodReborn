namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS6TrinityAvowed;

class FreedomOfBozja : TemperatureAOE
{
    private readonly List<(Actor orb, int temperature)> _orbs = [];
    private readonly DateTime _activation;
    private readonly bool _risky;

    private static readonly AOEShapeCircle _shape = new(22);

    public FreedomOfBozja(BossModule module, bool risky) : base(module)
    {
        _risky = risky;
        _activation = WorldState.FutureTime(10d);
        InitOrb((uint)OID.SwirlingOrb, -1);
        InitOrb((uint)OID.TempestuousOrb, -2);
        InitOrb((uint)OID.BlazingOrb, +1);
        InitOrb((uint)OID.RoaringOrb, +2);
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var playerTemp = Temperature(actor);
        var count = _orbs.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var o = _orbs[i];
            aoes[i] = new(_shape, o.orb.Position, o.orb.Rotation, _activation, o.temperature == -playerTemp ? Colors.SafeFromAOE : 0, _risky);
        }
        return aoes;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ChillBlast1 or (uint)AID.FreezingBlast1 or (uint)AID.HeatedBlast1 or (uint)AID.SearingBlast1 or (uint)AID.ChillBlast2
        or (uint)AID.FreezingBlast2 or (uint)AID.HeatedBlast2 or (uint)AID.SearingBlast2)
            ++NumCasts;
    }

    public bool ActorUnsafeAt(Actor actor, WPos pos)
    {
        var playerTemp = Temperature(actor);
        var count = _orbs.Count;
        for (var i = 0; i < count; ++i)
        {
            var o = _orbs[i];
            var shapeCheck = _shape.Check(pos, o.orb.Position);
            var tempCheck = o.temperature == -playerTemp;

            if (shapeCheck != tempCheck)
                return true;
        }
        return false;
    }

    private void InitOrb(uint oid, int temp)
    {
        var orbs = Module.Enemies(oid);
        var orb = orbs.Count != 0 ? orbs[0] : null;
        if (orb != null)
            _orbs.Add((orb, temp));
    }
}

sealed class FreedomOfBozja1(BossModule module) : FreedomOfBozja(module, false);

sealed class QuickMarchStaff1(BossModule module) : QuickMarch(module)
{
    private readonly FreedomOfBozja1? _freedom = module.FindComponent<FreedomOfBozja1>();

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !Arena.InBounds(pos) || (_freedom?.ActorUnsafeAt(actor, pos) ?? false);
}

sealed class FreedomOfBozja2(BossModule module) : FreedomOfBozja(module, true);
