namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

sealed class WheelOfFireflight(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone _cone = new(40f, 90f.Degrees());
    private readonly List<AOEInstance> _predicted = [];
    private WheelRectStart? _start;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_predicted);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        float deg;
        switch (iconID)
        {
            case (uint)IconID.LeftToRight8Sec:
            case (uint)IconID.RightToRight8Sec:
                deg = -90f;
                break;
            case (uint)IconID.LeftToLeft8Sec:
            case (uint)IconID.RightToLeft8Sec:
                deg = 90f;
                break;
            default:
                return;
        }
        _start ??= Module.FindComponent<WheelRectStart>();
        if (_start == null)
        {
            return;
        }
        var casters = CollectionsMarshal.AsSpan(_start.Casters);
        var len = casters.Length;
        var pos = actor.Position;
        for (var i = 0; i < len; ++i)
        {
            ref var c = ref casters[i];
            if (c.Origin.AlmostEqual(pos, 0.5f))
            {
                _predicted.Add(new(_cone, c.Origin, c.Rotation + deg.Degrees(), c.Activation.AddSeconds(10d)));
                return;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.WheelOfFireflightLTRBack or (uint)AID.WheelOfFireflightLTRFront or (uint)AID.WheelOfFireflightRTLFront or (uint)AID.WheelOfFireflightRTLBack)
        {
            ++NumCasts;
            _predicted.Clear();
        }
    }
}

sealed class Kindling(BossModule module) : Components.UniformStackSpread(module, 6f, 6f)
{
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ScatteredKindlingCast:
                AddSpreads(Raid.WithoutSlot(false, true, true), Module.CastFinishAt(spell));
                break;
            case (uint)AID.KindledFlame1Cast:
                var party = Raid.WithoutSlot(false, true, true);
                var len = party.Length;
                var activation = Module.CastFinishAt(spell);
                var added = 0;
                for (var i = 0; i < len && added < 2; ++i)
                {
                    if (party[i].Class.IsSupport())
                    {
                        AddStack(party[i], activation);
                        ++added;
                    }
                }
                for (var i = 0; i < len && added < 2; ++i)
                {
                    if (!party[i].Class.IsSupport())
                    {
                        AddStack(party[i], activation);
                        ++added;
                    }
                }
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ScatteredKindling or (uint)AID.KindledFlame1)
        {
            ++NumCasts;
            Spreads.Clear();
            Stacks.Clear();
        }
    }
}
