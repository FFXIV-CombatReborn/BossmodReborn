namespace BossMod.Endwalker.Savage.P11SThemis;

// note: currently we start showing stacks right after previous mechanic ends
sealed class InevitableLawSentence(BossModule module) : Components.GenericStackSpread(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.DivisiveOverrulingSoloLight:
            case (uint)AID.InnerLight:
            case (uint)AID.DivisiveOverrulingBossLight:
                AddPartyStacks();
                break;
            case (uint)AID.DivisiveOverrulingSoloDark:
            case (uint)AID.OuterDark:
            case (uint)AID.DivisiveOverrulingBossDark:
                AddPairStacks();
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.JuryOverrulingProteanLight:
            case (uint)AID.UpheldOverrulingAOELight:
                AddPartyStacks();
                break;
            case (uint)AID.JuryOverrulingProteanDark:
            case (uint)AID.UpheldOverrulingAOEDark:
                AddPairStacks();
                break;
            case (uint)AID.InevitableLaw:
            case (uint)AID.InevitableSentence:
                Stacks.Clear();
                break;
        }
    }

    private void AddPartyStacks()
    {
        Stacks.Clear();
        foreach (var t in Raid.WithoutSlot(true, true, true))
            if (t.Role == Role.Healer)
                Stacks.Add(new(t, 6, 4, 4, Module.StateMachine.NextTransitionWithFlag(StateMachine.StateHint.Raidwide)));
    }

    private void AddPairStacks()
    {
        Stacks.Clear();
        foreach (var t in Raid.WithoutSlot(true, true, true))
            if (t.Class.IsDD())
                Stacks.Add(new(t, 3, 2, 2, Module.StateMachine.NextTransitionWithFlag(StateMachine.StateHint.Raidwide)));
    }
}
