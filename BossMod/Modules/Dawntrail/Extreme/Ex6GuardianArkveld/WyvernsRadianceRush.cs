namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

sealed class WyvernsRadianceRush(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(3);
    public int NumCharges;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WyvernsRadianceChargeTelegraph)
        {
            var dir = spell.LocXZ - caster.Position;
            var shape = new AOEShapeRect(dir.Length(), 6f);
            var pos = caster.Position.Quantized();
            var rot = Angle.FromDirection(dir);
            var first = NumCharges == 0;
            _aoes.Add(new(shape, pos, rot, Module.CastFinishAt(spell, first ? 6d : 2.6d), first ? Colors.Danger : default, shapeDistance: shape.Distance(pos, rot)));
            ++NumCharges;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RushFirst or (uint)AID.RushRest)
        {
            ++NumCasts;
            var count = _aoes.Count;
            if (count != 0)
            {
                _aoes.RemoveAt(0);
                if (count > 1)
                {
                    _aoes.Ref(0).Color = Colors.Danger;
                }
            }
        }
    }
}

sealed class WyvernsRadianceConcentric(BossModule module) : Components.ConcentricAOEs(module,
    [new AOEShapeCircle(8f), new AOEShapeDonut(8f, 14f), new AOEShapeDonut(14f, 20f), new AOEShapeDonut(20f, 26f)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WyvernsRadianceConcentric1)
        {
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.WyvernsRadianceConcentric1 => 0,
                (uint)AID.WyvernsRadianceConcentric2 => 1,
                (uint)AID.WyvernsRadianceConcentric3 => 2,
                (uint)AID.WyvernsRadianceConcentric4 => 3,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}
