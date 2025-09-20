namespace BossMod.Dawntrail.Raid.M01NBlackCat;

sealed class OneTwoPaw(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private enum Pattern { None, WestEast, EastWest }
    private Pattern _currentPattern;
    private static readonly AOEShapeCone cone = new(60f, 90f.Degrees());
    private static readonly Angle[] angles = [Angle.AnglesCardinals[3], Angle.AnglesCardinals[0]];
    private Actor? helper;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (i == 0)
            {
                if (count == 2)
                    aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
            else
                aoe.Risky = false;
        }
        return aoes;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.LeapingAttacks)
        {
            helper = actor;
            InitIfReady();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.OneTwoPaw1:
            case (uint)AID.OneTwoPaw2:
            case (uint)AID.OneTwoPaw3:
            case (uint)AID.OneTwoPaw4:
                _aoes.Add(new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                if (_aoes.Count == 2)
                {
                    var aoes = CollectionsMarshal.AsSpan(_aoes);
                    ref var aoe1 = ref aoes[0];
                    ref var aoe2 = ref aoes[1];
                    if (aoe1.Activation > aoe2.Activation)
                    {
                        (aoe1, aoe2) = (aoe2, aoe1);
                    }
                }
                break;
            case (uint)AID.LeapingOneTwoPawVisual1:
            case (uint)AID.LeapingOneTwoPawVisual3:
                _currentPattern = Pattern.EastWest;
                InitIfReady();
                break;
            case (uint)AID.LeapingOneTwoPawVisual2:
            case (uint)AID.LeapingOneTwoPawVisual4:
                _currentPattern = Pattern.WestEast;
                InitIfReady();
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (helper != null)
            switch (spell.Action.ID)
            {
                case (uint)AID.LeapingBlackCatCrossingVisual1:
                case (uint)AID.LeapingBlackCatCrossingVisual2:
                case (uint)AID.LeapingBlackCatCrossingVisual3:
                case (uint)AID.LeapingBlackCatCrossingVisual4:
                    helper = null;
                    break;
            }
        else if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.OneTwoPaw1:
                case (uint)AID.OneTwoPaw2:
                case (uint)AID.OneTwoPaw3:
                case (uint)AID.OneTwoPaw4:
                case (uint)AID.LeapingOneTwoPaw1:
                case (uint)AID.LeapingOneTwoPaw2:
                case (uint)AID.LeapingOneTwoPaw3:
                case (uint)AID.LeapingOneTwoPaw4:
                    _currentPattern = Pattern.None;
                    _aoes.RemoveAt(0);
                    break;
            }
    }

    private void InitIfReady()
    {
        if (helper != null && _currentPattern != Pattern.None)
        {
            var angle = _currentPattern == Pattern.EastWest ? angles : angles.ReverseArray();
            _aoes.Add(new(cone, helper.Position, angle[0], WorldState.FutureTime(8.7d)));
            _aoes.Add(new(cone, helper.Position, angle[1], WorldState.FutureTime(10.7d)));
            _currentPattern = Pattern.None;
            helper = null;
        }
    }
}
