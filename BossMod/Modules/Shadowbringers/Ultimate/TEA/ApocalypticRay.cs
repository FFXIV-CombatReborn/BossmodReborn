namespace BossMod.Shadowbringers.Ultimate.TEA;

abstract class ApocalypticRay(BossModule module, bool faceCenter) : Components.GenericAOEs(module)
{
    public Actor? Source;
    private readonly bool _faceCenter = faceCenter;
    private Angle _rotation;
    private DateTime _activation;

    private readonly AOEShapeCone _shape = new(25.5f, 45f.Degrees());

    private readonly AOEInstance[] _oneAOEInstance = new AOEInstance[1];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Source != null)
        {
            _oneAOEInstance[0] = new(_shape, Source.Position, _rotation, _activation);
            return _oneAOEInstance;
        }
        return [];
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ApocalypticRay:
                Source = caster;
                if (_faceCenter)
                {
                    _rotation = Angle.FromDirection(Arena.Center - caster.Position);
                }
                else
                {
                    var target = WorldState.Actors.Find(caster.TargetID);
                    _rotation = target != null ? Angle.FromDirection(target.Position - caster.Position) : caster.Rotation; // this seems to be how it is baited
                }
                _activation = WorldState.FutureTime(0.6d);
                break;
            case (uint)AID.ApocalypticRayAOE:
                ++NumCasts;
                _activation = WorldState.FutureTime(1.1d);
                _rotation = caster.Rotation; // fix possible mistake
                break;
        }
    }
}

sealed class P2ApocalypticRay(BossModule module) : ApocalypticRay(module, false);
sealed class P3ApocalypticRay(BossModule module) : ApocalypticRay(module, true);
