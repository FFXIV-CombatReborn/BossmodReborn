namespace BossMod.Dawntrail.Savage.M01SBlackCat;

sealed class OneTwoPawBoss(BossModule module) : Components.GenericAOEs(module)
{
    private readonly PredaceousPounce? _pounce = module.FindComponent<PredaceousPounce>();
    private readonly List<AOEInstance> _aoes = new(2);

    private static readonly AOEShapeCone _shape = new(100, 90.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        var pounce = _pounce != null && _pounce.AOEs.Count != 0;
        var max = count > 2 ? 2 : count;
        var adj = pounce ? 1 : max;

        var aoes = CollectionsMarshal.AsSpan(_aoes)[..adj];
        for (var i = 0; i < adj; ++i)
        {
            ref var aoe = ref aoes[i];
            if (i == 0)
            {
                if (count == 2)
                    aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
            else
                aoe.Risky = false;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.OneTwoPawBossAOERFirst:
            case (uint)AID.OneTwoPawBossAOELSecond:
            case (uint)AID.OneTwoPawBossAOELFirst:
            case (uint)AID.OneTwoPawBossAOERSecond:
                _aoes.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                if (_aoes.Count == 2)
                {
                    var aoes = CollectionsMarshal.AsSpan(_aoes);
                    ref var aoe1 = ref aoes[0];
                    ref var aoe2 = ref aoes[1];
                    if (aoe1.Activation > aoe2.Activation)
                    {
                        (aoe1, aoe2) = (aoe2, aoe1);
                    }
                }
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.OneTwoPawBossAOERFirst:
            case (uint)AID.OneTwoPawBossAOELSecond:
            case (uint)AID.OneTwoPawBossAOELFirst:
            case (uint)AID.OneTwoPawBossAOERSecond:
                ++NumCasts;
                if (_aoes.Count != 0)
                {
                    _aoes.RemoveAt(0);
                }
                break;
        }
    }
}

sealed class OneTwoPawShade(BossModule module) : Components.GenericAOEs(module)
{
    private Angle _firstDirection;
    private readonly List<AOEInstance> _aoes = new(4);

    private static readonly AOEShapeCone _shape = new(100f, 90f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var dir = spell.Action.ID switch
        {
            (uint)AID.OneTwoPawBossRL => Angle.AnglesCardinals[0],
            (uint)AID.OneTwoPawBossLR => Angle.AnglesCardinals[3],
            _ => default
        };
        if (dir != default)
            _firstDirection = dir;
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (_aoes.Count < 4 && tether.ID == (uint)TetherID.Soulshade)
        {
            var pos = source.Position.Quantized();
            _aoes.Add(new(_shape, pos, source.Rotation + _firstDirection, WorldState.FutureTime(20.3d)));
            _aoes.Add(new(_shape, pos, source.Rotation - _firstDirection, WorldState.FutureTime(23.3d)));
            _aoes.Sort((a, b) => a.Activation.CompareTo(b.Activation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.OneTwoPawShadeAOERFirst:
            case (uint)AID.OneTwoPawShadeAOELSecond:
            case (uint)AID.OneTwoPawShadeAOELFirst:
            case (uint)AID.OneTwoPawShadeAOERSecond:
                ++NumCasts;
                if (_aoes.Count != 0)
                    _aoes.RemoveAt(0);
                break;
        }
    }
}

sealed class LeapingOneTwoPaw(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(2);
    private Angle _leapDirection;
    private Angle _firstDirection;
    private Actor? _clone;

    private static readonly AOEShapeCone _shape = new(100f, 90f.Degrees());
    private static readonly Angle[] angles = [Angle.AnglesCardinals[3], Angle.AnglesCardinals[0]];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];

        var aoes = CollectionsMarshal.AsSpan(AOEs);
        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (i == 0)
            {
                if (count == 2)
                    aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
            else
                aoe.Risky = false;
        }
        return aoes;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_clone != null && NumCasts >= AOEs.Count)
            Arena.Actor(_clone.Position + 10f * (_clone.Rotation + _leapDirection).ToDirection(), _clone.Rotation, Colors.Object);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var (leapDir, firstDir) = spell.Action.ID switch
        {
            (uint)AID.LeapingOneTwoPawBossLRL => (angles[0], angles[1]),
            (uint)AID.LeapingOneTwoPawBossLLR => (angles[0], angles[0]),
            (uint)AID.LeapingOneTwoPawBossRRL => (angles[1], angles[1]),
            (uint)AID.LeapingOneTwoPawBossRLR => (angles[1], angles[0]),
            _ => default
        };
        if (leapDir == default)
            return;

        _leapDirection = leapDir;
        _firstDirection = firstDir;
        StartMechanic(caster.Position, spell.Rotation, Module.CastFinishAt(spell, 1.8f));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LeapingOneTwoPawBossAOERFirst:
            case (uint)AID.LeapingOneTwoPawBossAOELSecond:
            case (uint)AID.LeapingOneTwoPawBossAOELFirst:
            case (uint)AID.LeapingOneTwoPawBossAOERSecond:
            case (uint)AID.LeapingOneTwoPawShadeAOERFirst:
            case (uint)AID.LeapingOneTwoPawShadeAOELSecond:
            case (uint)AID.LeapingOneTwoPawShadeAOELFirst:
            case (uint)AID.LeapingOneTwoPawShadeAOERSecond:
                ++NumCasts;
                if (AOEs.Count != 0)
                    AOEs.RemoveAt(0);
                break;
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Soulshade)
        {
            if (_clone == null)
            {
                _clone = source;
            }
            else if (_clone == source)
            {
                // note: if this is second mechanic, we could activate it slightly earlier than tethers appear, as soon as first mechanic ends
                StartMechanic(source.Position, source.Rotation, WorldState.FutureTime(16));
            }
            // else: second clone being tethered, wait...
        }
    }

    private void StartMechanic(WPos position, Angle rotation, DateTime activation)
    {
        var origin = position + 10f * (rotation + _leapDirection).ToDirection();
        AOEs.Add(new(_shape, origin, rotation + _firstDirection, activation));
        AOEs.Add(new(_shape, origin, rotation - _firstDirection, activation.AddSeconds(2d)));
    }
}
