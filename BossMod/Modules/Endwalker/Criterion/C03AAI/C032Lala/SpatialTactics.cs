namespace BossMod.Endwalker.VariantCriterion.C03AAI.C032Lala;

sealed class SpatialTactics(BossModule module) : Components.GenericAOEs(module)
{
    private readonly ArcaneArray? _array = module.FindComponent<ArcaneArray>();
    private readonly List<Actor> _fonts = [];
    private readonly int[] _remainingStacks = new int[4];

    private static readonly AOEShapeCross _shape = new(50f, 4f);

    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_array == null)
            return [];

        var wantMore = _remainingStacks[slot] > (NumCasts == 0 ? 1 : 0);
        var nextFonts = NextFonts();

        var count = 0;
        var countF = nextFonts.Count;
        var max = countF > 2 ? 2 : countF;
        var countarray = _array.SafeZoneCenters.Count;

        for (var i = 0; i < max; ++i)
        {
            var f = nextFonts[i];
            for (var j = 0; j < countarray; ++j)
            {
                if (_shape.Check(_array.SafeZoneCenters[j], f.actor))
                    ++count;
            }
        }

        if (count == 0)
            return [];

        var aoesLen = count;
        if (_aoesBuffer.Length < aoesLen)
            _aoesBuffer = new AOEInstance[aoesLen];
        var aoes = _aoesBuffer;
        var index = 0;

        for (var i = 0; i < max; ++i)
        {
            var f = nextFonts[i];
            for (var j = 0; j < countarray; ++j)
            {
                var p = _array.SafeZoneCenters[j];
                if (_shape.Check(p, f.actor))
                {
                    aoes[index++] = new(ArcaneArrayPlot.Shape, p, default, f.activation, wantMore ? Colors.SafeFromAOE : 0, !wantMore);
                }
            }
        }
        return aoes.AsSpan(0, aoesLen);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_remainingStacks[slot] > 0)
            hints.Add($"Remaining stacks: {_remainingStacks[slot]}", false);
        base.AddHints(slot, actor, hints);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.NArcaneFont or (uint)OID.SArcaneFont)
            _fonts.Add(actor);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.SubtractiveSuppressorBeta && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < _remainingStacks.Length)
            _remainingStacks[slot] = status.Extra;
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.SubtractiveSuppressorBeta && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < _remainingStacks.Length)
            _remainingStacks[slot] = 0;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.NRadiance2:
            case (uint)AID.SRadiance2:
                ++NumCasts;
                break;
            case (uint)AID.NInfernoDivide:
            case (uint)AID.SInfernoDivide:
                _fonts.Remove(caster);
                ++NumCasts;
                break;
        }
    }

    private DateTime FontActivation(Actor font) => _array?.AOEs.FirstOrDefault(aoe => aoe.Check(font.Position)).Activation.AddSeconds(0.2d) ?? default;

    private List<(Actor actor, DateTime activation)> NextFonts()
    {
        var fontActivations = new List<(Actor actor, DateTime activation)>();

        var count = _fonts.Count;
        for (var i = 0; i < count; ++i)
        {
            var font = _fonts[i];
            fontActivations.Add((font, FontActivation(font)));
        }
        fontActivations.Sort((x, y) => x.activation.CompareTo(y.activation));
        return fontActivations;
    }
}
