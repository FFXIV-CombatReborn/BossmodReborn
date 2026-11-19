namespace BossMod.Dawntrail.Alliance.A21FaithboundKirin;

sealed class SynchronizedStrikeSmite(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> _aoes = [with(4)];
    private static readonly AOEShapeRect rectNarrow = new(60f, 5f), rectWide = new(60f, 16f);
    private Actor? leftArm;
    private Actor? rightArm;

    private int index;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        return aoes[..Math.Min(index, aoes.Length)];
    }

    public override void Update()
    {
        var count = _aoes.Count;
        if (count < 3)
        {
            index = count;
            return;
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        ref var aoe0 = ref aoes[0];
        ref var aoe2 = ref aoes[2];

        if (aoe2.Rotation == aoe0.Rotation)
        {
            index = 2;
            return;
        }
        var color = Colors.Danger;
        for (var i = 0; i < 2; ++i)
        {
            ref var aoe = ref aoes[i];
            aoe.Color = color;
        }
        index = count;
    }

    public override void OnActorCreated(Actor actor)
    {
        var id = actor.OID;
        if (id == (uint)OID.SculptedArm2)
        {
            leftArm = actor;
        }
        else if (id == (uint)OID.SculptedArm1)
        {
            rightArm = actor;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        var shape = spell.Action.ID switch
        {
            (uint)AID.SynchronizedStrike1 or (uint)AID.SynchronizedSequence or (uint)AID.SynchronizedStrikeTelegraph => rectNarrow,
            (uint)AID.SynchronizedSmite1 or (uint)AID.SynchronizedSmite2 or (uint)AID.SynchronizedStrike2 => rectWide,
            _ => null
        };
        if (shape != null)
        {
            var isPrediction = id == (uint)AID.SynchronizedStrike2;
            if (!isPrediction)
            {
                if (_aoes.Count > 1 && _aoes.Ref(0).Shape == shape && _aoes.Ref(1).Shape == shape)
                {
                    return;
                }
                var isTelegraph = id == (uint)AID.SynchronizedStrikeTelegraph;
                AddAOE(shape, null, isTelegraph ? 7.1d : default);
            }
            else
            {
                if (_aoes.Count > 2)
                {
                    return;
                }
                AddAOE(shape, leftArm, 5.1d);
                AddAOE(shape, rightArm, 5.1d);
            }
            void AddAOE(AOEShape shape, Actor? actor = null, double delay = default)
            => _aoes.Add(new(shape, actor == null ? spell.LocXZ : (actor.Position - 30f * actor.Rotation.Round(1f).ToDirection()).Quantized(), actor == null ? spell.Rotation : actor.Rotation, Module.CastFinishAt(spell, delay)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.SynchronizedStrike1 or (uint)AID.SynchronizedStrike2 or (uint)AID.SynchronizedSmite1 or (uint)AID.SynchronizedSmite2 or (uint)AID.SynchronizedSmite3 or (uint)AID.SynchronizedSmite4 or (uint)AID.SynchronizedSequence)
        {
            _aoes.RemoveAt(0);
        }
    }
}
