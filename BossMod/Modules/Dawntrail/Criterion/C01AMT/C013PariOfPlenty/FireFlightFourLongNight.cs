namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

sealed class FourLongNightsSide(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone _cone = new(40f, 90f.Degrees());
    private static readonly int[] _rotateL = [3, 0, 1, 2];
    private static readonly int[] _rotateR = [1, 2, 3, 0];
    private static readonly int[] _invert = [2, 3, 0, 1];
    private static readonly Angle[] _angles = [180f.Degrees(), 90f.Degrees(), default, -90f.Degrees()];
    private static readonly string[] _names = ["N", "E", "S", "W"];
    private readonly List<AOEInstance> _aoes = [];
    private readonly List<int> _sides = [];
    private string _hint = "";
    private int _prev;
    private DateTime _start;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (_hint.Length != 0)
        {
            hints.Add(_hint);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FireflightFourLongNightsE:
            case (uint)AID.FireflightFourLongNightsW:
                SetStart(Module.CastFinishAt(spell, 2.2d));
                break;
            case (uint)AID.CharmdFlightFourNightsE:
            case (uint)AID.CharmdFlightFourNightsW:
                SetStart(Module.CastFinishAt(spell, 2.1d));
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch (iconID)
        {
            case (uint)IconID.LeftToRight3Sec:
                AddSide(3, _rotateR);
                break;
            case (uint)IconID.RightToRight3Sec:
                AddSide(1, _rotateR);
                break;
            case (uint)IconID.LeftToLeft3Sec:
                AddSide(3, _rotateL);
                break;
            case (uint)IconID.RightToLeft3Sec:
                AddSide(1, _rotateL);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.WheelOfFireflightLTRBack or (uint)AID.WheelOfFireflightLTRFront or (uint)AID.WheelOfFireflightRTLFront or (uint)AID.WheelOfFireflightRTLBack)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
            {
                _aoes.RemoveAt(0);
                _sides.RemoveAt(0);
                UpdateHint();
            }
        }
    }

    private void SetStart(DateTime start)
    {
        _start = start;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            aoes[i].Activation = start.AddSeconds(3d * i);
        }
    }

    private void AddSide(int initial, int[] rotation)
    {
        if (_aoes.Count == 0 && NumCasts == 0)
        {
            _prev = initial;
        }
        var side = rotation[_prev];
        _aoes.Add(new(_cone, Module.PrimaryActor.Position, _angles[side], _start.AddSeconds(3d * (_aoes.Count + NumCasts))));
        _sides.Add(side);
        _prev = _invert[_prev];
        UpdateHint();
    }

    private void UpdateHint()
    {
        var count = _sides.Count;
        if (count == 0)
        {
            _hint = "";
            return;
        }
        var sb = new StringBuilder("Side: ");
        for (var i = 0; i < count; ++i)
        {
            if (i != 0)
            {
                sb.Append(", ");
            }
            sb.Append(_names[_sides[i]]);
        }
        _hint = sb.ToString();
    }
}

sealed class FourLongNightsBait(BossModule module) : Components.UniformStackSpread(module, 3f, 3f, 3)
{
    private readonly List<(bool Close, DateTime Activation)> _baits = [];
    private string _hint = "";
    private DateTime _start;
    public int NumStacks;
    public int NumSpreads;

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (_hint.Length != 0)
        {
            hints.Add(_hint);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FireflightFourLongNightsE or (uint)AID.FireflightFourLongNightsW)
        {
            _start = Module.CastFinishAt(spell, 2.2d);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.WitchHunt && status.Extra is 0x3F4 or 0x3F5)
        {
            _baits.Add((status.Extra == 0x3F4, _start.AddSeconds(3d * (_baits.Count + NumSpreads))));
            UpdateHint();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FellFirework:
                ++NumSpreads;
                Advance();
                break;
            case (uint)AID.FireWell:
                ++NumStacks;
                Advance();
                break;
        }
    }

    public override void Update()
    {
        Stacks.Clear();
        Spreads.Clear();
        if (_baits.Count == 0)
        {
            return;
        }
        var party = Raid.WithoutSlot(false, true, true);
        var len = party.Length;
        if (len < 2)
        {
            return;
        }
        var origin = Module.PrimaryActor.Position;
        Actor closest = party[0], farthest = party[0];
        float minDist = float.MaxValue, maxDist = float.MinValue;
        for (var i = 0; i < len; ++i)
        {
            var p = party[i];
            var dist = (p.Position - origin).LengthSq();
            if (dist < minDist)
            {
                minDist = dist;
                closest = p;
            }
            if (dist > maxDist)
            {
                maxDist = dist;
                farthest = p;
            }
        }
        var (close, activation) = _baits[0];
        AddSpread(close ? closest : farthest, activation);
        AddStack(close ? farthest : closest, activation);
    }

    private void Advance()
    {
        if (NumStacks == NumSpreads && _baits.Count != 0)
        {
            _baits.RemoveAt(0);
            UpdateHint();
        }
    }

    private void UpdateHint()
    {
        var count = _baits.Count;
        if (count == 0)
        {
            _hint = "";
            return;
        }
        var sb = new StringBuilder("Bait: ");
        for (var i = 0; i < count; ++i)
        {
            if (i != 0)
            {
                sb.Append(", ");
            }
            sb.Append(_baits[i].Close ? "Close" : "Far");
        }
        _hint = sb.ToString();
    }
}
