namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

[ConfigDisplay(Order = 0x130, Parent = typeof(DawntrailConfig))]
public sealed class M08SHowlingBladeConfig() : ConfigNode()
{
    [PropertyDisplay("Show platform numbers")]
    public bool ShowPlatformNumbers = true;

    [PropertyDisplay("Platform number colors:")]
    public Color[] PlatformNumberColors = [new(0xffffffff), new(0xffffffff), new(0xffffffff), new(0xffffffff), new(0xffffffff)];

    [PropertyDisplay("Platform number font size")]
    [PropertySlider(0.1f, 100, Speed = 1)]
    public float PlatformNumberFontSize = 22;

    public enum ReignStrategy
    {
        [PropertyDisplay("Show both safespots for current role")]
        Any,
        [PropertyDisplay("Assume G1 left, G2 right when looking at boss from arena center")]
        Standard,
        [PropertyDisplay("Assume G1 right, G2 left when looking at boss from arena center")]
        Inverse,
        [PropertyDisplay("None")]
        Disabled
    }

    [PropertyDisplay("Revolutionary/Eminent Reign positioning hints")]
    public ReignStrategy ReignHints = ReignStrategy.Standard;

    [PropertyDisplay("Show Rinon/Toxic Friends tower spots for Lone Wolf's Lament")]
    public bool LoneWolfsLamentHints = true;
}
