namespace BossMod.Components;

// generic component that is 'active' when any actor casts specific spell

[SkipLocalsInit]
public class CastHint(BossModule module, uint aid, string hint, bool showCastTimeLeft = false) : CastCounter(module, aid)
{
    public string Hint = hint;
    public readonly bool ShowCastTimeLeft = showCastTimeLeft; // if true, show cast time left until next instance
    public readonly List<Actor> Casters = [];

    public bool Active => Casters.Count > 0;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Active && Hint.Length > 0)
            hints.Add(ShowCastTimeLeft ? $"{Hint} {Casters[0].CastInfo?.NPCRemainingTime ?? 0:f1}s left" : Hint);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            Casters.Remove(caster);
    }
}

[SkipLocalsInit]
public class CastHints(BossModule module, uint[] aids, string hint, bool showCastTimeLeft = false) : CastHint(module, default, hint, showCastTimeLeft)
{
    private readonly uint[] AIDs = aids;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var len = AIDs.Length;
        for (var i = 0; i < len; ++i)
            if (spell.Action.ID == AIDs[i])
                Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var len = AIDs.Length;
        for (var i = 0; i < len; ++i)
            if (spell.Action.ID == AIDs[i])
                Casters.Remove(caster);
    }
}

[SkipLocalsInit]
public class CastInterruptHint : CastHint
{
    public readonly bool CanBeInterrupted;
    public readonly bool CanBeStunned;
    public readonly bool ShowNameInHint; // important if there are several targets
    public readonly string HintExtra;

    public CastInterruptHint(BossModule module, uint aid, bool canBeInterrupted = true, bool canBeStunned = false, string hintExtra = "", bool showNameInHint = false) : base(module, aid, "")
    {
        CanBeInterrupted = canBeInterrupted;
        CanBeStunned = canBeStunned;
        ShowNameInHint = showNameInHint;
        HintExtra = hintExtra;
        UpdateHint();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Casters.Count;
        for (var i = 0; i < count; ++i)
        {
            var c = Casters[i];
            var e = hints.FindEnemy(c);
            if (e != null)
            {
                e.ShouldBeInterrupted |= CanBeInterrupted;
                e.ShouldBeStunned |= CanBeStunned;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (ShowNameInHint && spell.Action.ID == WatchedAction)
            UpdateHint();
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (ShowNameInHint && spell.Action.ID == WatchedAction)
            UpdateHint();
    }

    private void UpdateHint()
    {
        if (!CanBeInterrupted && !CanBeStunned)
            return;
        var actionStr = !CanBeStunned ? "Interrupt" : !CanBeInterrupted ? "Stun" : "Interrupt/stun";
        var nameStr = ShowNameInHint && Casters.Count == 1 ? " " + Casters[0].Name : "";
        Hint = $"{actionStr}{nameStr}!";
        if (HintExtra.Length > 0)
            Hint += $" ({HintExtra})";
    }
}
