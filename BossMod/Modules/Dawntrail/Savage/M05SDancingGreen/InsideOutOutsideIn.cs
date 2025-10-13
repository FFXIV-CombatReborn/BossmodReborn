namespace BossMod.Dawntrail.Savage.M05SDancingGreen;

sealed class InsideOutOutsideIn(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCircle circle = new(7f);
    private readonly AOEShapeDonut donut = new(5f, 40f);
    public readonly List<AOEInstance> AOEs = new(2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs.Count != 0 ? CollectionsMarshal.AsSpan(AOEs)[..1] : [];

    public override void AddGlobalHints(GlobalHints hints)
    {
        var count = AOEs.Count;
        if (count > 0)
        {
            var sb = new StringBuilder(9);
            var aoes = CollectionsMarshal.AsSpan(AOEs);
            for (var i = 0; i < count; ++i)
            {
                var shapeHint = aoes[i].Shape switch
                {
                    AOEShapeCircle => "Out",
                    AOEShapeDonut => "In",
                    _ => ""
                };
                sb.Append(shapeHint);

                if (i < count - 1)
                    sb.Append(" -> ");
            }
            hints.Add(sb.ToString());
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape[] shapes = spell.Action.ID switch
        {
            (uint)AID.InsideOutVisual1 => [circle, donut],
            (uint)AID.OutsideInVisual1 => [donut, circle],
            _ => []
        };
        if (shapes.Length != 0)
        {
            AddAOE(shapes[0], 0.1d);
            AddAOE(shapes[1], 2.6d);
        }
        void AddAOE(AOEShape shape, double delay)
        => AOEs.Add(new(shape, spell.LocXZ, default, Module.CastFinishAt(spell, delay)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (AOEs.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.InsideOuDonut:
                case (uint)AID.InsideOutCircle:
                case (uint)AID.OutsideInCircle:
                case (uint)AID.OutsideInDonut:
                    AOEs.RemoveAt(0);
                    break;
            }
        }
    }
}
