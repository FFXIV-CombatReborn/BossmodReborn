namespace BossMod.Endwalker.VariantCriterion.C03AAI.C033Statice;

class TrickReload(BossModule module) : BossComponent(module)
{
    public bool FirstStack;
    public int SafeSlice;
    public int NumLoads;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (SafeSlice > 0)
            hints.Add($"Order: {(FirstStack ? "stack" : "spread")} -> {SafeSlice} -> {(FirstStack ? "spread" : "stack")}");
        else if (NumLoads > 0)
            hints.Add($"Order: {(FirstStack ? "stack" : "spread")} -> ???");
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LockedAndLoaded:
                ++NumLoads;
                break;
            case (uint)AID.Misload:
                if (NumLoads == 0)
                    FirstStack = true;
                else if (SafeSlice == 0)
                    SafeSlice = NumLoads;
                ++NumLoads;
                break;
        }
    }
}

class Trapshooting(BossModule module) : Components.UniformStackSpread(module, 6f, 6, 4)
{
    public int NumResolves;
    private readonly TrickReload? _reload = module.FindComponent<TrickReload>();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NTrapshooting1 or (uint)AID.NTrapshooting2 or (uint)AID.STrapshooting1 or (uint)AID.STrapshooting2 && _reload != null)
        {
            var stack = NumResolves == 0 ? _reload.FirstStack : !_reload.FirstStack;
            if (stack)
            {
                var target = Raid.WithoutSlot(false, true, true).FirstOrDefault(); // TODO: dunno how target is selected...
                if (target != null)
                    AddStack(target, Module.CastFinishAt(spell, 4.1f));
            }
            else
            {
                AddSpreads(Raid.WithoutSlot(true, true, true), Module.CastFinishAt(spell, 4.1f));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.NTrapshootingStack:
            case (uint)AID.STrapshootingStack:
                if (Stacks.Count > 0)
                {
                    Stacks.Clear();
                    ++NumResolves;
                }
                break;
            case (uint)AID.NTrapshootingSpread:
            case (uint)AID.STrapshootingSpread:
                if (Spreads.Count > 0)
                {
                    Spreads.Clear();
                    ++NumResolves;
                }
                break;
        }
    }
}

abstract class TriggerHappy(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(40f, 30f.Degrees()));
class NTriggerHappy(BossModule module) : TriggerHappy(module, (uint)AID.NTriggerHappyAOE);
class STriggerHappy(BossModule module) : TriggerHappy(module, (uint)AID.STriggerHappyAOE);
