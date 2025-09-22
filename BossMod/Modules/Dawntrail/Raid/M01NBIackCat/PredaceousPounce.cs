namespace BossMod.Dawntrail.Raid.M01NBlackCat;

sealed class PredaceousPounce(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(12);
    private static readonly AOEShapeCircle circle = new(11f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        var aoes = CollectionsMarshal.AsSpan(_aoes);
        if (count >= 3)
        {
            for (var i = 0; i < 2; ++i)
            {
                aoes[i].Color = Colors.Danger;
            }
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShape shape, WPos position, Angle rotation = default)
        {
            _aoes.Add(new(shape, position, rotation, Module.CastFinishAt(spell)));
            var count = _aoes.Count;
            if (count == 12)
            {
                _aoes.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
                var aoes = CollectionsMarshal.AsSpan(_aoes);
                for (var i = 0; i < count; ++i)
                    aoes[i].Activation = WorldState.FutureTime(13.5d + i * 0.5d);
            }
        }
        switch (spell.Action.ID)
        {
            case (uint)AID.PredaceousPounceTelegraphCharge1:
            case (uint)AID.PredaceousPounceTelegraphCharge2:
            case (uint)AID.PredaceousPounceTelegraphCharge3:
            case (uint)AID.PredaceousPounceTelegraphCharge4:
            case (uint)AID.PredaceousPounceTelegraphCharge5:
            case (uint)AID.PredaceousPounceTelegraphCharge6:
                var dir = spell.LocXZ - caster.Position;
                AddAOE(new AOEShapeRect(dir.Length(), 3f), caster.Position, Angle.FromDirection(dir));
                break;
            case (uint)AID.PredaceousPounceTelegraphCircle1:
            case (uint)AID.PredaceousPounceTelegraphCircle2:
            case (uint)AID.PredaceousPounceTelegraphCircle3:
            case (uint)AID.PredaceousPounceTelegraphCircle4:
            case (uint)AID.PredaceousPounceTelegraphCircle5:
            case (uint)AID.PredaceousPounceTelegraphCircle6:
                AddAOE(circle, spell.LocXZ);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.PredaceousPounceCharge1:
                case (uint)AID.PredaceousPounceCharge2:
                case (uint)AID.PredaceousPounceCircle1:
                case (uint)AID.PredaceousPounceCircle2:
                    _aoes.RemoveAt(0);
                    break;
            }
    }
}
