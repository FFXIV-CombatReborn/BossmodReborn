using BossMod.BST;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;

public sealed class CrucibleAI(RotationModuleManager manager, Actor player) : AIBase<CrucibleAI.Strategy>(manager, player)
{
    public struct Strategy
    {
        [Track("Snarl", InternalName = "Snarl", Action = AID.Snarl)]
        public Track<EnabledByDefault> Snarl;

        [Track("Challenge", InternalName = "Challenge", Action = AID.Challenge)]
        public Track<EnabledByDefault> Challenge;

        [Track("Auto-dispel", InternalName = "Auto-dispel", Action = AID.QuellingWave)]
        public Track<EnabledByDefault> Dispel;

        [Track("Auto-interrupt", InternalName = "Auto-interrupt", Action = AID.SoulCrush)]
        public Track<EnabledByDefault> Interrupt;
    }

    private const byte DispelKinship = 5;
    private const byte InterruptKinship = 7;

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Crucible AI", "Crucible of the Unbroken utilities", "AI (xan)", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.BST), 100).WithStrategies<Strategy>();
    }

    public override void Execute(in Strategy strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var gauge = World.Client.GetGauge<BeastmasterGauge>();
        var havePet = gauge.ActiveBattlehorn > 0;

        AIHints.Enemy? shirk = null, provoke = null, dispel = null, interrupt = null;
        var targets = Hints.PotentialTargets;
        var count = targets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = targets[i];
            if (e.PreferShirking)
            {
                shirk ??= e;
            }
            if (e.PreferProvoking)
            {
                provoke ??= e;
            }
            if (e.ShouldBeDispelled && e.Actor.PendingDispels.Count == 0)
            {
                dispel ??= e;
            }
            if (ShouldInterrupt(e) && e.Actor.CastInfo?.Interruptible == true)
            {
                interrupt ??= e;
            }
        }

        if (strategy.Snarl.IsEnabled() && havePet && shirk != null)
        {
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.Snarl), shirk.Actor, ActionQueue.Priority.Medium, forced: true);
        }

        if (strategy.Challenge.IsEnabled() && provoke != null && provoke.Actor.TargetID != Player.InstanceID)
        {
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.Challenge), provoke.Actor, ActionQueue.Priority.Medium, forced: true);
        }

        if (strategy.Dispel.IsEnabled() && dispel != null && gauge.KinshipClassification == DispelKinship)
        {
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.QuellingWave), dispel.Actor, ActionQueue.Priority.VeryHigh);
        }

        if (strategy.Interrupt.IsEnabled() && interrupt != null && gauge.KinshipClassification == InterruptKinship)
        {
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.SoulCrush), interrupt.Actor, ActionQueue.Priority.Medium, forced: true);
            Hints.GoalZones.Add(AIHints.GoalSingleTarget(interrupt.Actor.Position, interrupt.Actor.HitboxRadius + 3f + Player.HitboxRadius, 5f));
        }
    }
}
