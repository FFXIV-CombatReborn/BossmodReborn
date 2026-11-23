namespace BossMod.Endwalker.Quest.MSQ.Endwalker;

sealed class AkhMorn(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private DateTime _activation;
    private static readonly AOEShapeCircle circle = new(4f);

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (_activation != default)
            hints.Add($"Tankbuster x{NumExpectedCasts()}");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activation != default)
            hints.AddPredictedDamage(new(1), _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AkhMorn)
        {
            if (Raid.Player() is Actor player)
                CurrentBaits.Add(new(Module.PrimaryActor, player, circle));
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AkhMorn)
            ++NumCasts;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.AkhMornVisual)
        {
            ++NumCasts;
            if (NumCasts == NumExpectedCasts())
            {
                CurrentBaits.Clear();
                NumCasts = 0;
                _activation = default;
            }
        }
    }

    private int NumExpectedCasts() => Module.PrimaryActor.IsDead ? 8 : 6;
}
