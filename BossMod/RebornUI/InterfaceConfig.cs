using RebornMaterial;

namespace BossMod.RebornUI;

[ConfigDisplay(Name = "Interface", Order = -2, Tags = ["theme", "accent", "scale", "size", "material"])]
public sealed class InterfaceConfig : ConfigNode, IM3Settings
{
    [SectionStart("Theme")]
    [PropertyDisplay("Accent color", tooltip: "Every color in the BossMod Reborn windows is generated from this one. Grey or black colors fall back to the default.")]
    public Color AccentColor = Color.FromRGBA(0x5BA7B7FF);

    [SectionStart("Sizing")]
    [PropertyDisplay("Text size", tooltip: "Scales the text in the themed windows, on top of Dalamud's own font size.")]
    [PropertySlider(0.5f, 3f)]
    public float TextScale = 1f;

    [PropertyDisplay("Element size", tooltip: "Scales buttons, switches, fields and the other controls.")]
    [PropertySlider(0.5f, 2.5f)]
    public float ElementScale = 1f;

    [PropertyDisplay("Spacing", tooltip: "Scales the space around and between controls without changing their size.")]
    [PropertySlider(0f, 3f)]
    public float PaddingScale = 1f;

    [PropertyDisplay("Compact settings window", tooltip: "Uses tighter padding in the settings window, so more fits on screen.")]
    public bool CompactSettings = false;

    Vector4 IM3Settings.UiAccentColor
    {
        get => AccentColor.ToFloat4();
        set => AccentColor = Color.FromFloat4(value);
    }

    float IM3Settings.UiTextScale
    {
        get => TextScale;
        set => TextScale = value;
    }

    float IM3Settings.UiElementScale
    {
        get => ElementScale;
        set => ElementScale = value;
    }

    float IM3Settings.UiPaddingScale
    {
        get => PaddingScale;
        set => PaddingScale = value;
    }
}
