
using static BossMod.Components.Knockback;

namespace BossMod.Dawntrail.Savage.M1SBlackCat;

public class OneTwoPaw(BossModule module) : Components.GenericAOEs(module)
{
    public bool Done;
    public static readonly AOEShapeCone Shape = new(60, 90f.Degrees());
    private readonly List<AOEInstance> _aoes = [];
    private int DangerAOEs = 1;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.OneTwoPawLeftRight:
                _aoes.Add(new(Shape, caster.Position, 90.0f.Degrees(), Module.WorldState.FutureTime(1f)));
                _aoes.Add(new(Shape, caster.Position, -90.0f.Degrees(), Module.WorldState.FutureTime(5.7f)));
                break;
            case AID.OneTwoPawRightLeft:
                _aoes.Add(new(Shape, caster.Position, -90.0f.Degrees(), Module.WorldState.FutureTime(1f)));
                _aoes.Add(new(Shape, caster.Position, 90.0f.Degrees(), Module.WorldState.FutureTime(5.7f)));
                break;
            // TODO: since previous one two paw decides order, and actor position is known since soulshade, draw AoEs starting from soulshade?
            case AID.OneTwoPawSoulshadeLeftRightFirst:
                _aoes.Add(new(Shape, caster.Position, caster.Rotation + 90.0f.Degrees(), Module.WorldState.FutureTime(5.7f)));
                break;
            case AID.OneTwoPawSoulshadeLeftRightSecond:
                _aoes.Add(new(Shape, caster.Position, caster.Rotation + -90.0f.Degrees(), Module.WorldState.FutureTime(8.5f)));
                break;
            case AID.OneTwoPawSoulshadeRightLeftFirst:
                _aoes.Add(new(Shape, caster.Position, caster.Rotation + -90.0f.Degrees(), Module.WorldState.FutureTime(5.7f)));
                break;
            case AID.OneTwoPawSoulshadeRightLeftSecond:
                _aoes.Add(new(Shape, caster.Position, caster.Rotation + 90.0f.Degrees(), Module.WorldState.FutureTime(8.5f)));
                break;
        }
        _aoes.SortBy(x => x.Activation);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.OneTwoPawSoulshadeLeftRightFirst:
            case AID.OneTwoPawSoulshadeRightLeftFirst:
            case AID.OneTwoPawSoulshadeLeftRightSecond:
            case AID.OneTwoPawSoulshadeRightLeftSecond:
                _aoes.RemoveAt(0);
                if (_aoes.Count == 0)
                    Done = true;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.OneTwoPawLeftRightFirst:
            case AID.OneTwoPawRightLeftFirst:
            case AID.OneTwoPawLeftRightSecond:
            case AID.OneTwoPawRightLeftSecond:
                _aoes.RemoveAt(0);
                DangerAOEs = 2;
                break;
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        for (int i = 0; i < _aoes.Count; i++)
        {
            if (i < DangerAOEs)
            {
                yield return _aoes[i] with { Color = Colors.Danger };
            }
            else
            {
                yield return _aoes[i] with { Color = Colors.AOE, Risky = false };
            }
        }
    }
}

public class LeapingOneTwoPaw(BossModule module) : Components.GenericAOEs(module)
{
    public bool Done;
    private readonly List<AOEInstance> _aoes = [];
    private bool leapingLeft;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.LeapingOneTwoPawBCast)
            leapingLeft = true;
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.LeapingTarget)
        {
            if (leapingLeft)
            {
                _aoes.Add(new(OneTwoPaw.Shape, actor.Position, actor.Rotation + 90.0f.Degrees(), Module.WorldState.FutureTime(1f)));
                _aoes.Add(new(OneTwoPaw.Shape, actor.Position, actor.Rotation + -90.0f.Degrees(), Module.WorldState.FutureTime(5.7f)));
            }
            else
            {
                _aoes.Add(new(OneTwoPaw.Shape, actor.Position, actor.Rotation + -90.0f.Degrees(), Module.WorldState.FutureTime(1f)));
                _aoes.Add(new(OneTwoPaw.Shape, actor.Position, actor.Rotation + 90.0f.Degrees(), Module.WorldState.FutureTime(5.7f)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.LeapingOneTwoPawAFirst:
            case AID.LeapingOneTwoPawASecond:
            case AID.LeapingOneTwoPawBFirst:
            case AID.LeapingOneTwoPawBSecond:
                _aoes.RemoveAt(0);
                if (_aoes.Count == 0)
                    Done = true;
                break;
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        for (int i = 0; i < _aoes.Count; i++)
        {
            if (i < 1)
            {
                yield return _aoes[i] with { Color = Colors.Danger };
            }
            else
            {
                yield return _aoes[i] with { Color = Colors.AOE, Risky = false };
            }
        }
    }
}
