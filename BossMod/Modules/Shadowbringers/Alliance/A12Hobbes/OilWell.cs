namespace BossMod.Shadowbringers.Alliance.A12Hobbes;

sealed class OilWell(BossModule module) : Components.GenericAOEs(module, (uint)AID.OilWell)
{
    private static readonly WPos platformCenter = A12Hobbes.PlatformCenters[2];
    private static readonly WPos[] pads = MakePads();
    private static readonly AOEShapeCircle circle = new(6f);
    private static readonly SDUnion padsShape = new([new SDCircle(pads[0], 6f), new SDCircle(pads[1], 6f), new SDCircle(pads[2], 6f), new SDCircle(pads[3], 6f)]);
    private static readonly SDInvertedUnion padsShapeInverted = new([new SDCircle(pads[0], 6f), new SDCircle(pads[1], 6f), new SDCircle(pads[2], 6f), new SDCircle(pads[3], 6f)]);
    private readonly List<AOEInstance> _aoes = [with(4)];
    private DateTime _activation;
    private bool _inverted;

    private static WPos[] MakePads()
    {
        var rot = -120f.Degrees();
        return [platformCenter + new WDir(8f, 8f).Rotate(rot), platformCenter + new WDir(-8f, 8f).Rotate(rot),
            platformCenter + new WDir(-8f, -8f).Rotate(rot), platformCenter + new WDir(8f, -8f).Rotate(rot)];
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => Relevant(actor) ? CollectionsMarshal.AsSpan(_aoes) : [];

    private static bool Relevant(Actor a) => a.Position.InCircle(platformCenter, A12Hobbes.PlatformRadius);

    private static bool OnPad(WPos pos)
    {
        for (var i = 0; i < 4; ++i)
        {
            if (pos.InCircle(pads[i], 6f))
            {
                return true;
            }
        }
        return false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoes.Count != 0 && Relevant(actor))
        {
            hints.AddForbiddenZone(_inverted ? padsShapeInverted : padsShape, _activation);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoes.Count == 0 || !Relevant(actor))
        {
            return;
        }
        if (_inverted)
        {
            if (!OnPad(actor.Position))
            {
                hints.Add("Go to platform!");
            }
        }
        else if (OnPad(actor.Position))
        {
            hints.Add(WarningText);
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x06)
        {
            switch (state)
            {
                case 0x01000080u:
                    Activate(false);
                    break;
                case 0x00200010u:
                    Activate(true);
                    break;
            }
        }
    }

    private void Activate(bool inverted)
    {
        _activation = WorldState.FutureTime(4.2d);
        _inverted = inverted;
        _aoes.Clear();
        for (var i = 0; i < 4; ++i)
        {
            _aoes.Add(new(circle, pads[i], default, _activation, inverted ? Colors.SafeFromAOE : default, !inverted));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            _aoes.Clear();
        }
    }
}
