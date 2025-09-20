namespace BossMod.Endwalker.VariantCriterion.C03AAI.C032Lala;

class ArcaneArrayPlot : Components.GenericAOEs
{
    public List<AOEInstance> AOEs = [];
    public List<WPos> SafeZoneCenters = [];

    public static readonly AOEShapeRect Shape = new(4, 4, 4);

    public ArcaneArrayPlot(BossModule module) : base(module)
    {
        for (var z = -16; z <= 16; z += 8)
            for (var x = -16; x <= 16; x += 8)
                SafeZoneCenters.Add(Arena.Center + new WDir(x, z));
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NBrightPulseFirst or (uint)AID.NBrightPulseRest or (uint)AID.SBrightPulseFirst or (uint)AID.SBrightPulseRest)
            ++NumCasts;
    }

    public void AddAOE(WPos pos, DateTime activation)
    {
        AOEs.Add(new(Shape, pos, default, activation));
        SafeZoneCenters.RemoveAll(c => Shape.Check(c, pos, default));
    }

    protected void Advance(ref WPos pos, ref DateTime activation, WDir offset)
    {
        AddAOE(pos, activation);
        activation = activation.AddSeconds(1.2d);
        pos += offset;
    }
}

class ArcaneArray(BossModule module) : ArcaneArrayPlot(module)
{
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ArrowBright)
        {
            var activation = WorldState.FutureTime(4.6d);
            var pos = actor.Position;
            var offset = 8 * actor.Rotation.ToDirection();
            for (var i = 0; i < 5; ++i)
            {
                Advance(ref pos, ref activation, offset);
            }
            pos -= offset;
            pos += Arena.InBounds(pos + offset.OrthoL()) ? offset.OrthoL() : offset.OrthoR();
            for (var i = 0; i < 5; ++i)
            {
                Advance(ref pos, ref activation, -offset);
            }

            if (AOEs.Count > 10)
                AOEs.Sort((a, b) => a.Activation.CompareTo(b.Activation));
        }
    }
}

class ArcanePlot(BossModule module) : ArcaneArrayPlot(module)
{
    public override void OnActorCreated(Actor actor)
    {
        switch (actor.OID)
        {
            case (uint)OID.ArrowBright:
                AddLine(actor, WorldState.FutureTime(4.6d), false);
                break;
            case (uint)OID.ArrowDim:
                AddLine(actor, WorldState.FutureTime(8.2d), true);
                break;
        }
    }

    private void AddLine(Actor actor, DateTime activation, bool preAdvance)
    {
        var pos = actor.Position;
        var offset = 8f * actor.Rotation.ToDirection();
        if (preAdvance)
            pos += offset;

        do
        {
            Advance(ref pos, ref activation, offset);
        }
        while (Arena.InBounds(pos));

        AOEs.Sort((a, b) => a.Activation.CompareTo(b.Activation));
    }
}
