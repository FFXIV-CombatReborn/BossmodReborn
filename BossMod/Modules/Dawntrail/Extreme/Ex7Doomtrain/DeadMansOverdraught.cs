namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

sealed class DeadMansOverdraught(BossModule module) : Components.GenericStackSpread(module)
{
    private bool? partnerStack;
    public uint Counter;

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (partnerStack is bool stack)
        {
            hints.Add($"Stored: {(stack ? "Partner stack" : "Spread")}");
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.DeadMansOverdraughtSpread:
                partnerStack = false;
                break;
            case (uint)AID.DeadMansOverdraughtStack:
                partnerStack = true;
                break;
            case (uint)AID.DeadMansExpress:
            case (uint)AID.DeadMansWindpipeVisual:
                if (AutoActivate && Stacks.Count == 0 && Spreads.Count == 0)
                {
                    AddStackSpread(11.1d);
                }
                break;
        }
    }

    public bool AutoActivate;

    public void AddStackSpread(double delay)
    {
        if (partnerStack is bool kind)
        {
            var party = Raid.WithoutSlot(true, false, false);
            var len = party.Length;
            var activation = WorldState.FutureTime(delay);
            if (kind)
            {
                for (var i = 0; i < len; ++i)
                {
                    var p = party[i];
                    if (p.Class.IsSupport())
                    {
                        Stacks.Add(new(p, 5f, 2, 2, activation));
                    }
                }
            }
            else
            {
                for (var i = 0; i < len; ++i)
                {
                    Spreads.Add(new(party[i], 5f, activation));
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (partnerStack != null && spell.Action.ID is (uint)AID.Plasma or (uint)AID.HyperexplosivePlasma)
        {
            partnerStack = null;
            Stacks.Clear();
            Spreads.Clear();
            ++Counter;
        }
    }
}
