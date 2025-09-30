namespace BossMod.Dawntrail.Trial.T05Necron;

sealed class MementoMori(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeRect rect = new(37f, 6f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MementoMori)
        {
            _aoe = [new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x18)
        {
            if (state == 0x00020001u)
            {
                Arena.Bounds = Necron.SplitArena;
                _aoe = [];
            }
            else if (state == 0x00080004u)
            {
                Arena.Bounds = new ArenaBoundsRect(18f, 15f);
            }
        }
    }
}

sealed class ChokingGrasp(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(5);
    private static readonly AOEShapeRect rect = new(24f, 3f);
    private bool fearOfDeath;
    private bool mementoMori;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if ((fearOfDeath || mementoMori) && actor.OID == (uint)OID.IcyHands1)
        {
            _aoes.Add(new(rect, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(fearOfDeath ? 13.7d : 11.1d)));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FearOfDeath:
                fearOfDeath = true;
                AddAOEs(13.7d);
                break;
            case (uint)AID.MementoMori:
                mementoMori = true;
                AddAOEs(11.1d);
                break;
            case (uint)AID.ChokingGrasp when caster.OID != (uint)OID.IcyHands1:
                _aoes.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                break;
        }
        void AddAOEs(double delay)
        {
            var enemies = Module.Enemies((uint)OID.IcyHands1);
            var count = enemies.Count;
            var act = WorldState.FutureTime(delay);
            for (var i = 0; i < count; ++i)
            {
                var e = enemies[i];
                _aoes.Add(new(rect, e.Position.Quantized(), e.Rotation, act));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ChokingGrasp)
        {
            if (caster.OID == (uint)OID.IcyHands1)
            {
                _aoes.Clear();
                mementoMori = fearOfDeath = false;
            }
            else if (_aoes.Count != 0)
            {
                _aoes.RemoveAt(0);
            }
        }
    }
}
