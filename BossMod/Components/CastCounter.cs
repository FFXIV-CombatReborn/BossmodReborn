namespace BossMod.Components;

// generic component that counts specified casts

[SkipLocalsInit]
public class CastCounter(BossModule module, uint aid) : BossComponent(module)
{
    public readonly uint WatchedAction = aid;
    public int NumCasts;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
        }
    }
}

[SkipLocalsInit]
public class CastCounterMulti(BossModule module, uint[] aids) : BossComponent(module)
{
    public readonly uint[] WatchedActions = aids;
    public int NumCasts;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var len = WatchedActions.Length;
        var id = spell.Action.ID;
        for (var i = 0; i < len; ++i)
        {
            if (id == WatchedActions[i])
            {
                ++NumCasts;
                return;
            }
        }
    }
}
