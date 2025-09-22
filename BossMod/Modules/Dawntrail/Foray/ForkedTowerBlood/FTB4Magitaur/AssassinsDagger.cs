namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB4Magitaur;

sealed class AssassinsDagger(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(18);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        var max = count > 6 ? 6 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        if (count > 3)
        {
            var color = Colors.Danger;
            for (var i = 0; i < 3; ++i)
            {
                aoes[i].Color = color;
            }
        }
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AssassinsDaggerFirst)
        {
            var offset = -50f.Degrees();
            var dir = spell.LocXZ - caster.Position;
            var rect = new AOEShapeRect(dir.Length(), 3f);
            var angle = Angle.FromDirection(dir);
            var pos = caster.Position.Quantized();
            var act = Module.CastFinishAt(spell);

            for (var i = 0; i < 6; ++i)
            {
                _aoes.Add(new(rect, pos, angle + i * offset, act.AddSeconds(i * 3.9d)));
            }
            if (_aoes.Count == 18)
            {
                _aoes.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AssassinsDaggerFirst or (uint)AID.AssassinsDaggerRepeat or (uint)AID.AssassinsDaggerLast)
        {
            if (++NumCasts % 6 == 0)
            {
                _aoes.RemoveRange(0, 3);
            }
        }
    }
}
