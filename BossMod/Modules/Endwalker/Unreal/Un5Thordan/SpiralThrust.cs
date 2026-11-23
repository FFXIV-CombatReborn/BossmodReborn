namespace BossMod.Endwalker.Unreal.Un5Thordan;

abstract class SpiralThrust(BossModule module, float predictionDelay) : Components.GenericAOEs(module, (uint)AID.SpiralThrust)
{
    private float _predictionDelay = predictionDelay;
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(54.2f, 6);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            if (_predictionDelay > 0)
            {
                _predictionDelay = 0;
                _aoes.Clear();
            }
            _aoes.Add(new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.KnightAppear:
                if (caster.OID is (uint)OID.Vellguine or (uint)OID.Paulecrain or (uint)OID.Ignasse && (caster.Position - Module.Center).LengthSq() > 25 * 25)
                {
                    // prediction
                    _aoes.Add(new(_shape, caster.Position, Angle.FromDirection(Module.Center - caster.Position), WorldState.FutureTime(_predictionDelay), risky: false));
                }
                break;
            case (uint)AID.SpiralThrust:
                ++NumCasts;
                break;
        }
    }
}

sealed class SpiralThrust1(BossModule module) : SpiralThrust(module, 10);
sealed class SpiralThrust2(BossModule module) : SpiralThrust(module, 12.1f);
