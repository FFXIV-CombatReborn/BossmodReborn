namespace BossMod.Endwalker.Unreal.Un1Ultima;

// both phases use radiant plumes
sealed class TitanIfrit(BossModule module) : BossComponent(module)
{
    private readonly List<(Actor, AOEShapeCircle)> _activeSimpleAOEs = [];
    private readonly List<Actor> _crimsonCyclone = [];

    private readonly AOEShapeCircle _aoeRadiantPlume = new(8f);
    private readonly AOEShapeCircle _aoeWeightOfLand = new(6f);
    private readonly AOEShapeCircle _aoeEruption = new(8f);
    private readonly AOEShapeRect _aoeCrimsonCyclone = new(38f, 6f);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        for (var i = 0; i < _activeSimpleAOEs.Count; ++i)
        {
            var (caster, shape) = _activeSimpleAOEs[i];
            if (caster.CastInfo is ActorCastInfo ci && shape.Check(actor.Position, ci.LocXZ))
            {
                hints.Add("GTFO from aoe!");
                return;
            }
        }
        for (var i = 0; i < _crimsonCyclone.Count; ++i)
        {
            if (_aoeCrimsonCyclone.Check(actor.Position, _crimsonCyclone[i]))
            {
                hints.Add("GTFO from aoe!");
                return;
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var (a, aoe) in _activeSimpleAOEs)
            aoe.Draw(Arena, a.CastInfo!.LocXZ);
        foreach (var a in _crimsonCyclone)
            _aoeCrimsonCyclone.Draw(Arena, a);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.RadiantPlume:
                _activeSimpleAOEs.Add((caster, _aoeRadiantPlume));
                break;
            case (uint)AID.WeightOfTheLand:
                _activeSimpleAOEs.Add((caster, _aoeWeightOfLand));
                break;
            case (uint)AID.Eruption:
                _activeSimpleAOEs.Add((caster, _aoeEruption));
                break;
            case (uint)AID.CrimsonCyclone:
                _crimsonCyclone.Add(caster);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.RadiantPlume:
            case (uint)AID.WeightOfTheLand:
            case (uint)AID.Eruption:
                _activeSimpleAOEs.RemoveAll(e => e.Item1 == caster);
                break;
            case (uint)AID.CrimsonCyclone:
                _crimsonCyclone.Remove(caster);
                break;
        }
    }
}
