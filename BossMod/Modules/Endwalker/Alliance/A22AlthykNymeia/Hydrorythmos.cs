namespace BossMod.Endwalker.Alliance.A22AlthykNymeia;

sealed class Hydrorythmos(BossModule module) : Components.GenericAOEs(module)
{
    private Angle _dir;
    private DateTime _activation;

    private readonly AOEShapeRect _shapeFirst = new(25f, 5f, 25f);
    private readonly AOEShapeRect _shapeRest = new(25f, 2.5f, 25f);

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = _aoesBuffer;
        aoes.Clear();
        if (NumCasts > 0)
        {
            var offset = (2.5f + ((NumCasts + 1) >> 1) * 5f) * _dir.ToDirection().OrthoL();
            aoes.Add(new(_shapeRest, Arena.Center + offset, _dir, _activation));
            aoes.Add(new(_shapeRest, Arena.Center - offset, _dir, _activation));
        }
        else if (_activation != default)
        {
            aoes.Add(new(_shapeFirst, Arena.Center, _dir, _activation));
        }
        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_activation == default)
        {
            return;
        }
        var center = Arena.Center;
        var normal = _dir.ToDirection().OrthoL();
        var limit = NumCasts == 0 ? 7f : 5f;
        hints.GoalZones.Add(p => Math.Abs((p - center).Dot(normal)) < limit ? 1f : 0f);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HydrorythmosFirst)
        {
            _dir = spell.Rotation;
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.HydrorythmosFirst or (uint)AID.HydrorythmosRest)
        {
            ++NumCasts;
            _activation = WorldState.FutureTime(2.1d);
        }
    }
}
