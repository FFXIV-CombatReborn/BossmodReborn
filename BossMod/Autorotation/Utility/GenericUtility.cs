﻿namespace BossMod.Autorotation;

// base class that simplifies implementation of utility modules - these are really only useful for planning support
public abstract class GenericUtility(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum SimpleOption { None, Use }
    public enum LBOption { None, LB3, LB2, LB1, LB2Only, LB1Only, LB12 }

    protected static void DefineSimpleConfig<Index, AID>(RotationModuleDefinition def, Index expectedIndex, string internalName, string displayName, int uiPriority, AID aid, float effect = 0)
        where Index : Enum
        where AID : Enum
    {
        var adefs = ActionDefinitions.Instance;
        var action = ActionID.MakeSpell(aid);
        var adata = adefs[action]!;
        var cfg = def.Define(expectedIndex).As<SimpleOption>(internalName, displayName, uiPriority);
        cfg.AddOption(SimpleOption.None, new(0x80ffffff, ActionTargets.None, "None", "Do not use automatically"));
        cfg.AddOption(SimpleOption.Use, new(0x8000ffff, adata.AllowedTargets, "Use", $"Use {action.Name()}", adata.Cooldown, effect, adefs.ActionMinLevel(action)));
        cfg.AddAssociatedAction(aid);
    }

    protected static StrategyConfig DefineLimitBreak<Index>(RotationModuleDefinition def, Index expectedIndex, ActionTargets allowedTargets, float effectLB1 = 0, float effectLB2 = 0, float effectLB3 = 0) where Index : Enum
    {
        // note: it assumes that effect durations are either 0's or correspond to tank LB (so lb2 > lb1 > lb3)
        var lb = def.Define(expectedIndex).As<LBOption>("LB");
        lb.AddOption(LBOption.None, new(0x80ffffff, ActionTargets.None, "None", "Do not use automatically"));
        lb.AddOption(LBOption.LB3, new(0x8000ff00, allowedTargets, "LB3", "Use LB3 if available", Effect: effectLB3));
        lb.AddOption(LBOption.LB2, new(0x8000ff40, allowedTargets, "LB2", "Use LB2/3 if available", Effect: effectLB3));
        lb.AddOption(LBOption.LB1, new(0x8000ff80, allowedTargets, "LB1", "Use any LB if available", Effect: effectLB3));
        lb.AddOption(LBOption.LB2Only, new(0x80ffff00, allowedTargets, "LB2Only", "Use LB2 if available, but not LB3", Effect: effectLB2));
        lb.AddOption(LBOption.LB1Only, new(0x80ffff40, allowedTargets, "LB1Only", "Use LB1 if available, but not LB2+", Effect: effectLB1));
        lb.AddOption(LBOption.LB12, new(0x80ffff80, allowedTargets, "LB12", "Use LB1/2 if available, but not LB3", Effect: effectLB1));
        return lb;
    }

    protected void ExecuteSimple<AID>(in StrategyValues.OptionRef opt, AID aid, Actor? target) where AID : Enum
    {
        if (opt.As<SimpleOption>() == SimpleOption.Use)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), ResolveTargetOverride(opt.Value) ?? target, opt.Priority());
    }

    // returns 0 if not needed, or current LB level
    protected int LBLevelToExecute(LBOption option)
    {
        // note: limit break's animation lock is very long, so we're guaranteed to delay next gcd at least a bit => priority has to be higher than gcd
        // note: while we could arguably delay that until right after next gcd, it's too risky, let the user deal with it by planning carefully...
        if (option == LBOption.None)
            return 0;
        var curLevel = World.Party.LimitBreakMax > 0 ? World.Party.LimitBreakCur / World.Party.LimitBreakMax : 0;
        return option switch
        {
            LBOption.LB3 => curLevel == 3,
            LBOption.LB2 => curLevel >= 2,
            LBOption.LB1 => curLevel >= 1,
            LBOption.LB2Only => curLevel == 2,
            LBOption.LB1Only => curLevel == 1,
            LBOption.LB12 => curLevel is 1 or 2,
            _ => false
        } ? curLevel : 0;
    }
}