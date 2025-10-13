namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

[ConfigDisplay(Order = 0x150, Parent = typeof(DawntrailConfig))]
public class Ex6GuardianArkveldConfig : ConfigNode
{
    public enum LimitCutStrategy
    {
        [PropertyDisplay("Take turns at cardinals (1 W/E)")]
        Circle,
        [PropertyDisplay("Even numbers north, odd numbers south")]
        EvenNorth,
    }

    [PropertyDisplay("Limit cut positioning hints")]
    public LimitCutStrategy LimitCutHints = LimitCutStrategy.Circle;
}
