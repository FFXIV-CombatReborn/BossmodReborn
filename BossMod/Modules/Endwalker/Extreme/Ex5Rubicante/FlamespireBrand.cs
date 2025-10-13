namespace BossMod.Endwalker.Extreme.Ex5Rubicante;

class Welts(BossModule module) : Components.GenericStackSpread(module, true)
{
    public enum Mechanic { StackFlare, Spreads, Done }

    public Mechanic NextMechanic;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.BloomingWelt:
                Spreads.Add(new(actor, 15f));
                break;
            case (uint)SID.FuriousWelt:
                Stacks.Add(new(actor, 6f, 4, 4)); // TODO: verify flare falloff
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FuriousWelt:
                NextMechanic = Mechanic.Spreads;
                Stacks.Clear();
                Spreads.Clear();
                foreach (var t in Raid.WithoutSlot(false, true, true))
                    Spreads.Add(new(t, 6f));
                break;
            case (uint)AID.StingingWelt:
                NextMechanic = Mechanic.Done;
                Spreads.Clear();
                break;
        }
    }
}

class Flamerake(BossModule module) : Components.GenericAOEs(module)
{
    private Angle _offset;
    private DateTime _activation;

    private static readonly AOEShapeCross _first = new(20f, 6f);
    private static readonly AOEShapeRect _rest = new(8f, 20f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation == default)
            return [];

        if (NumCasts == 0)
        {
            return new AOEInstance[1] { new(_first, Arena.Center.Quantized(), _offset, _activation) };
        }
        else
        {
            var aoes = new AOEInstance[4];
            float offset = NumCasts == 1 ? 6 : 14;
            for (var i = 0; i < 4; ++i)
            {
                var dir = i * 90f.Degrees() + _offset;
                aoes[i] = new(_rest, (Arena.Center + offset * dir.ToDirection()).Quantized(), dir, _activation);
            }
            return aoes;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FlamerakeAOE11:
            case (uint)AID.FlamerakeAOE12:
                if (NumCasts == 0)
                {
                    ++NumCasts;
                    _activation = WorldState.FutureTime(2.1d);
                }
                break;
            case (uint)AID.FlamerakeAOE21:
            case (uint)AID.FlamerakeAOE22:
                if (NumCasts == 1)
                {
                    ++NumCasts;
                    _activation = WorldState.FutureTime(2.5d);
                }
                break;
            case (uint)AID.FlamerakeAOE31:
            case (uint)AID.FlamerakeAOE32:
                if (NumCasts == 2)
                {
                    ++NumCasts;
                    _activation = default;
                }
                break;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 4)
        {
            var act = WorldState.FutureTime(8.5d);
            switch (state)
            {
                // 00080004 when rotation ends
                case 0x00010001:
                case 0x00100010:
                    _offset = 45f.Degrees();
                    _activation = act;
                    break;
                case 0x00200020:
                case 0x00800080:
                    _offset = default;
                    _activation = act;
                    break;
            }
        }
    }
}
