namespace BossMod.Autorotation.MiscAI;

public sealed class GoToPositional(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Tracks
    {
        Positional
    }
    private static readonly Positional[] positionals = Enum.GetValues<Positional>();
    private static readonly string[] positionalNames = Enum.GetNames<Positional>();

    public static RotationModuleDefinition Definition()
    {
        RotationModuleDefinition def = new("Misc AI: Goes to specified positional", "Module for use with other rotation plugins.", "AI", "erdelf", RotationModuleQuality.Basic, new(~0ul), 1000);

        var track = def.Define(Tracks.Positional).As<Positional>("Positional", "Positional");
        for (var i = 0; i < 4; ++i)
        {
            ref readonly var positional = ref positionals[i];
            ref readonly var positionalName = ref positionalNames[i];
            track.AddOption(positional, positionalName);
        }
        return def;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (!Player.InCombat
            || Player.FindStatus((uint)ClassShared.AID.TrueNorth) != null
            || primaryTarget == null
            || primaryTarget is { Omnidirectional: true }
            || primaryTarget is { TargetID: var t, CastInfo: null, IsStrikingDummy: false } && t == Player.InstanceID)
        {
            return;
        }

        var positional = strategy.Option(Tracks.Positional).As<Positional>();
        if (positional == Positional.Any)
            return;

        //mainly from Basexan.UpdatePositionals
        var correct = positional switch
        {
            Positional.Flank => Math.Abs(primaryTarget.Rotation.ToDirection().Dot((Player.Position - primaryTarget.Position).Normalized())) < 0.7071067f,
            Positional.Rear => primaryTarget.Rotation.ToDirection().Dot((Player.Position - primaryTarget.Position).Normalized()) < -0.7071068f,
            _ => true
        };

        Hints.RecommendedPositional = (primaryTarget, positional, true, correct);
        Hints.GoalZones.Add(Hints.GoalSingleTarget(primaryTarget, positional));
    }
}
