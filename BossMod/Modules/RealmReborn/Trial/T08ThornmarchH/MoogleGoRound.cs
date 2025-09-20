namespace BossMod.RealmReborn.Trial.T08ThornmarchH;

class MoogleGoRound(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _casters = [];
    private static readonly AOEShape _shape = new AOEShapeCircle(20f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _casters.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = new AOEInstance[max];

        for (var i = 0; i < max; ++i)
        {
            var c = _casters[i];
            aoes[i] = new(_shape, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo!));
        }
        return aoes;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        // if there is a third cast, add a smaller shape to ensure people stay closer to eventual safespot
        if (_casters.Count > 2)
        {
            hints.AddForbiddenZone(new SDUnion([new SDInvertedCircle(_casters[0].Position, 23f), new SDCircle(_casters[2].Position, 10f)]), Module.CastFinishAt(_casters[1].CastInfo!));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.MoogleGoRoundBoss or (uint)AID.MoogleGoRoundAdd)
            _casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.MoogleGoRoundBoss or (uint)AID.MoogleGoRoundAdd)
            _casters.Remove(caster);
    }
}
