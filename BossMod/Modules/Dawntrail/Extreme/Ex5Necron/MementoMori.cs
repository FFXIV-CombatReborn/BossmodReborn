namespace BossMod.Dawntrail.Extreme.Ex5Necron;

sealed class MementoMori(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeRect rect = new(100f, 6f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.MementoMori1 or (uint)AID.MementoMori2)
        {
            _aoe = [new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.MementoMori1 or (uint)AID.MementoMori2)
        {
            ++NumCasts;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x18)
        {
            if (state == 0x00020001u)
            {
                Arena.Bounds = Trial.T05Necron.Necron.SplitArena;
                _aoe = [];
            }
            else if (state == 0x00080004u)
            {
                Arena.Bounds = new ArenaBoundsRect(18f, 15f);
            }
        }
    }
}

sealed class SmiteOfGloom(BossModule module) : Components.GenericStackSpread(module, true, true)
{
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.MementoMori1 or (uint)AID.MementoMori2)
        {
            var party = Raid.WithoutSlot(true, false, false);
            var len = party.Length;
            Spreads.Capacity = 8;
            var act = WorldState.FutureTime(7.1d);
            for (var i = 0; i < len; ++i)
            {
                Spreads.Add(new(party[i], 10f, act));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SmiteOfGloom)
        {
            ++NumCasts;
        }
    }
}

sealed class ChokingGraspMM(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(5);
    public bool isRisky = true;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.IcyHands1)
        {
            _aoes.Add(new(ChokingGrasp.Rect, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(11.1d), risky: isRisky));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ChokingGraspAOE1)
        {
            ++NumCasts;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (!isRisky && spell.Action.ID == (uint)AID.ChokingGraspAOE1)
        {
            var count = _aoes.Count;
            var aoes = CollectionsMarshal.AsSpan(_aoes);

            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                aoe.Risky = true;
            }
        }
    }
}
