namespace BossMod.Dawntrail.Alliance.A22UltimaOmega;

sealed class GuidedMissile(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GuidedMissile, 6f);

sealed class TrajectoryProjection(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private DateTime activation;
    private readonly AOEShapeCircle circle = new(6);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch (iconID)
        {
            case (uint)IconID.GuidedMissileEast:
                AddBait(new(5f, default));
                break;
            case (uint)IconID.GuidedMissileWest:
                AddBait(new(-5f, default));
                break;
            case (uint)IconID.GuidedMissileSouth:
                AddBait(new(default, 5f));
                break;
            case (uint)IconID.GuidedMissileNorth:
                AddBait(new(default, -5f));
                break;
        }
        void AddBait(WDir offset) => CurrentBaits.Add(new(Module.PrimaryActor, actor, circle, activation, offset: offset));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TrajectoryProjection:
                activation = Module.CastFinishAt(spell, 9.8d);
                break;
            case (uint)AID.GuidedMissile:
                CurrentBaits.Clear();
                break;
        }
    }
}
