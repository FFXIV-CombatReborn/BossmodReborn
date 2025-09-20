namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN5TrinityAvowed;

sealed class ElementalImpact(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeCircle circle = new(20f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ElementalImpactVisual1 or (uint)AID.ElementalImpactVisual2)
            _aoes.Add(new(circle, spell.LocXZ, default, Module.CastFinishAt(spell, 0.3f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ElementalImpact1 or (uint)AID.ElementalImpact2 or (uint)AID.ElementalImpact3 or (uint)AID.ElementalImpact4)
        {
            _aoes.Clear();
        }
    }
}

sealed class ElementalImpactTemperature(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance>[] _aoes = new List<AOEInstance>[PartyState.MaxAllianceSize];
    private static readonly AOEShapeCircle circle = new(22f), circleInv = new(22f, true);
    private readonly PlayerTemperatures _temps = module.FindComponent<PlayerTemperatures>()!;
    private readonly List<AOEInstance>?[] aoePerTemp = new List<AOEInstance>?[5];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => slot is < 0 or > 23 ? [] : CollectionsMarshal.AsSpan(_aoes[slot]);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var temp = spell.Action.ID switch
        {
            (uint)AID.ChillBlast => 1u,
            (uint)AID.HeatedBlast => 3u,
            (uint)AID.FreezingBlast => 2u,
            (uint)AID.SearingBlast => 4u,
            _ => default
        };
        if (temp != default)
        {
            var safecolor = Colors.SafeFromAOE;
            var temps = _temps.Temperatures;
            var center = Arena.Center;
            for (var i = 0; i < 24; ++i)
            {
                var playertemp = temps[i];
                if (aoePerTemp[playertemp] != null)
                {
                    _aoes[i] = aoePerTemp[playertemp]!;
                    continue;
                }
                uint color = default;
                var shape = circle;
                if (playertemp != default && playertemp == temp)
                {
                    color = safecolor;
                    shape = circleInv;
                }
                if (_aoes[i] == null)
                    _aoes[i] = new(4);
                _aoes[i].Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), color));
                if (_aoes[i].Count != 4)
                    continue;
                if (playertemp == default)
                    aoePerTemp[playertemp] = _aoes[i];
                if (playertemp != default)
                {
                    if (aoePerTemp[playertemp] == null)
                    {
                        var aoes = CollectionsMarshal.AsSpan(_aoes[i]);

                        var safeShapes = new List<Polygon>(2);
                        var dangerShapes = new List<Polygon>(3);

                        for (var j = 0; j < 4; ++j)
                        {
                            ref var aoe = ref aoes[j];
                            var circle = new Polygon(aoe.Origin, 22f, 64);
                            if (aoe.Color == safecolor)
                            {
                                safeShapes.Add(circle);
                            }
                            else
                            {
                                dangerShapes.Add(circle);
                            }
                        }
                        if (safeShapes.Count == 2)
                        {
                            AOEShapeCustom xor = new([safeShapes[0]], shapes2: [safeShapes[1]], operand: OperandType.Xor);
                            AOEShapeCustom difference = new(dangerShapes, invertForbiddenZone: true);
                            var clipper = new PolygonClipper();
                            var combinedShapes = clipper.Difference(new PolygonClipper.Operand(xor.GetCombinedPolygon(center)),
                            new PolygonClipper.Operand(difference.GetCombinedPolygon(center)));
                            difference.Polygon = combinedShapes;
                            aoePerTemp[playertemp] = [new(difference, center, default, Module.CastFinishAt(spell), safecolor)];
                        }
                        else
                        {
                            aoePerTemp[playertemp] = [new(new AOEShapeCustom(safeShapes, dangerShapes, invertForbiddenZone: true), center, default, Module.CastFinishAt(spell), safecolor)];
                        }
                    }
                    _aoes[i] = aoePerTemp[playertemp]!;
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ChillBlast:
            case (uint)AID.HeatedBlast:
            case (uint)AID.FreezingBlast:
            case (uint)AID.SearingBlast:
                Array.Clear(_aoes);
                Array.Clear(aoePerTemp);
                break;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var aoes = ActiveAOEs(slot, actor);
        var len = aoes.Length;
        if (len == 0)
            return;
        var isInsideDanger = false;
        var isinsideCorrect = false;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(actor.Position))
            {
                if (aoe.Color == Colors.SafeFromAOE)
                {
                    isinsideCorrect = true;
                }
                else
                {
                    isInsideDanger = true;
                }
            }
        }

        if (isInsideDanger)
            hints.Add(WarningText);
        hints.Add("Get hit by correct AOE!", !isinsideCorrect);
    }
}
