namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class DaemoniacBonds(BossModule module) : Components.UniformStackSpread(module, 4f, 6f)
{
    public int NumMechanics;
    private readonly List<Actor> _spreadTargets = [];
    private readonly List<Actor> _stackTargets = [];
    private DateTime _spreadResolve;
    private DateTime _stackResolve;

    public void Show()
    {
        if (_spreadResolve < _stackResolve)
            AddSpreads(_spreadTargets, _spreadResolve);
        else
            AddStacks(_stackTargets, _stackResolve);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_spreadResolve == default || _stackResolve == default)
            return;
        var stackHint = MinStackSize == 2 ? "Pairs" : "Groups";
        var orderHint = _spreadResolve > _stackResolve ? $"{stackHint} -> Spread" : $"Spread -> {stackHint}";
        hints.Add($"Debuff order: {orderHint}");
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.DaemoniacBonds:
                _spreadTargets.Add(actor);
                _spreadResolve = status.ExpireAt;
                break;
            case (uint)SID.DuodaemoniacBonds:
            case (uint)SID.TetradaemoniacBonds:
                MinStackSize = MaxStackSize = status.ID == (uint)SID.TetradaemoniacBonds ? 4 : 2;
                _stackTargets.Add(actor);
                _stackResolve = status.ExpireAt;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.DaemoniacBondsAOE:
                Spreads.Clear();
                NumMechanics = _spreadResolve < _stackResolve ? 1 : 2;
                if (NumMechanics == 1 && Stacks.Count == 0)
                    AddStacks(_stackTargets, _stackResolve);
                break;
            case (uint)AID.DuodaemoniacBonds:
            case (uint)AID.TetradaemoniacBonds:
                Stacks.Clear();
                NumMechanics = _stackResolve < _spreadResolve ? 1 : 2;
                if (NumMechanics == 1 && Spreads.Count == 0)
                    AddSpreads(_spreadTargets, _spreadResolve);
                break;
        }
    }
}
