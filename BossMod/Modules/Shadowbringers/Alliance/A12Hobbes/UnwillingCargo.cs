namespace BossMod.Shadowbringers.Alliance.A12Hobbes;

sealed class UnwillingCargoRing(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(17.5f, 20f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => A12Hobbes.PlatformIndex(actor.Position) == 1 ? _aoe : [];

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x04)
        {
            if (state == 0x00020001u)
            {
                _aoe = [new(donut, UnwillingCargo.PlatformCenter)];
            }
            else if (state == 0x00080004u)
            {
                _aoe = [];
            }
        }
    }
}

sealed class UnwillingCargo(BossModule module) : Components.GenericKnockback(module, (uint)AID.UnwillingCargo, stopAtWall: true)
{
    public static readonly WPos PlatformCenter = A12Hobbes.PlatformCenters[1];
    private const float Distance = 15f;
    private const float SafeRadius = 17.5f;
    private static readonly AOEShapeRect full = new(20f, 20f, 20f);
    private static readonly AOEShapeRect strip = new(20f, 3.5f, 20f);

    private Knockback[] _kbs = [];
    private ShapeDistance? _forbidden;
    public DateTime Activation;

    public bool IsAffected(Actor actor) => Activation > WorldState.CurrentTime && OnPlatform(actor);
    private static bool OnPlatform(Actor actor) => actor.Position.InCircle(PlatformCenter, A12Hobbes.PlatformRadius);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => OnPlatform(actor) ? _kbs : [];

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !pos.InCircle(PlatformCenter, SafeRadius);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_forbidden != null && OnPlatform(actor) && !IsImmune(slot, Activation))
        {
            hints.AddForbiddenZone(_forbidden, Activation);
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x05)
        {
            switch (state)
            {
                case 0x00200020u: // blue
                    SetPattern(-1f, false);
                    break;
                case 0x01000100u: // pink
                    SetPattern(1f, false);
                    break;
                case 0x08000800u: // blue/pink strips
                    SetPattern(-1f, true);
                    break;
                case 0x40004000u: // pink/blue strips
                    SetPattern(1f, true);
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            _kbs = [];
            _forbidden = null;
        }
    }

    private void SetPattern(float sign, bool strips)
    {
        Activation = WorldState.FutureTime(6.1d);
        var dir = sign * 90f.Degrees();
        if (!strips)
        {
            _kbs = [new(PlatformCenter, Distance, Activation, full, dir, Kind.DirForward)];
            _forbidden = new SDInvertedCircle(PlatformCenter - Distance * dir.ToDirection(), SafeRadius);
            return;
        }

        _kbs = new Knockback[5];
        var zones = new ShapeDistance[5];
        for (var i = 0; i < 5; ++i)
        {
            var origin = PlatformCenter + new WDir(default, (i - 2) * 7f);
            var d = (i & 1) == 0 ? dir : -dir;
            _kbs[i] = new(origin, Distance, Activation, strip, d, Kind.DirForward);
            zones[i] = new SDIntersection([new SDRect(origin, d, 20f, 20f, 3.5f), new SDInvertedCircle(PlatformCenter - Distance * d.ToDirection(), SafeRadius)]);
        }
        _forbidden = new SDUnion(zones);
    }
}
