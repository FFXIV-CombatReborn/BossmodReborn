namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

sealed class WyvernsRadianceCrackedCrystal(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(6);
    private readonly AOEShapeCircle circleSmall = new(6f), circleBig = new(12f);
    private readonly List<Actor> bigCrystals = module.Enemies((uint)OID.CrackedCrystal1);
    private readonly List<Actor> smallCrystals = module.Enemies((uint)OID.CrackedCrystal2);
    private bool aoesAdded;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (aoesAdded)
        {
            return;
        }
        var delay = spell.Action.ID switch
        {
            (uint)AID.WyvernsVengeanceFirst1 => 1.6d,
            (uint)AID.WyvernsVengeanceFirst2 => 2.7d,
            _ => default
        };
        if (delay != default)
        {
            var act = Module.CastFinishAt(spell, delay); // delay is slightly longer (1s) for each additional wave, but I doubt it matters. if we want exact activation times for all aoes we need to calculate when the exaflares intersects the crystals.
            AddAOEs(act);
        }
    }

    private void AddAOEs(DateTime activation)
    {
        var countB = bigCrystals.Count;
        for (var i = 0; i < countB; ++i)
        {
            AddAOE(circleBig, bigCrystals[i].Position.Quantized());
        }
        var countS = smallCrystals.Count;
        for (var i = 0; i < countS; ++i)
        {
            AddAOE(circleSmall, smallCrystals[i].Position.Quantized());
        }
        aoesAdded = true;
        void AddAOE(AOEShapeCircle shape, WPos origin) => _aoes.Add(new(shape, origin, default, activation, shapeDistance: shape.Distance(origin, default)));
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (!aoesAdded && iconID == (uint)IconID.WyvernsWeal)
        {
            AddAOEs(WorldState.FutureTime(8.1d));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WyvernsRadianceCrackedCrystalBig or (uint)AID.WyvernsRadianceCrackedCrystalSmall)
        {
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            ++NumCasts;
            var len = aoes.Length;
            var pos = caster.Position;
            for (var i = 0; i < len; ++i)
            {
                if (pos.AlmostEqual(aoes[i].Origin, 1f))
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

sealed class WyvernsVengeance(BossModule module) : Components.Exaflare(module, 6f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id is (uint)AID.WyvernsVengeanceFirst1 or (uint)AID.WyvernsVengeanceFirst2)
        {
            Lines.Add(new(caster.Position, 8f * spell.Rotation.Round(1f).ToDirection(), Module.CastFinishAt(spell), 1.1d, id == (uint)AID.WyvernsVengeanceFirst1 ? 3 : 6, 3));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id is (uint)AID.WyvernsVengeanceFirst1 or (uint)AID.WyvernsVengeanceFirst2 or (uint)AID.WyvernsVengeanceRest1 or (uint)AID.WyvernsVengeanceRest2)
        {
            var count = Lines.Count;
            var isFirst = id is (uint)AID.WyvernsVengeanceFirst1 or (uint)AID.WyvernsVengeanceFirst2;
            var loc = isFirst ? caster.Position : spell.TargetXZ;
            if (isFirst)
            {
                ++NumCasts;
            }
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(loc, 1f))
                {
                    AdvanceLine(line, loc);
                    if (line.ExplosionsLeft == 0)
                    {
                        Lines.RemoveAt(i);
                    }
                    return;
                }
            }
        }
    }
}
