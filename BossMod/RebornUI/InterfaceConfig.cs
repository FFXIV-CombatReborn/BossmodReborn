using RebornMaterial;

namespace BossMod.RebornUI;

[ConfigDisplay(Name = "Interface", Order = -2, Tags = ["theme", "accent", "scale", "size", "material", "animation", "motion"])]
public sealed class InterfaceConfig : ConfigNode, IM3Settings
{
    [SectionStart("Theme")]
    [PropertyDisplay("Accent color", tooltip: "The whole color scheme is built from this one color. Pick a preset, or the last swatch for your own color.")]
    public Color AccentColor = Color.FromRGBA(0x5BA7B7FF);

    [PropertyDisplay("Text size", tooltip: "Scales the text, on top of Dalamud's own font size.")]
    [PropertySlider(0.75f, 2f)]
    public float TextScale = 1f;

    [PropertyDisplay("Control size", tooltip: "Scales buttons, switches, fields and the other controls.")]
    [PropertySlider(0.75f, 2f)]
    public float ElementScale = 1f;

    [PropertyDisplay("Spacing", tooltip: "Scales the space around and between things, without resizing them.")]
    [PropertySlider(0.5f, 2f)]
    public float PaddingScale = 1f;

    [PropertyDisplay("Animation length", tooltip: "How long animations take, such as folding a window or opening a card. 0% turns them off.")]
    [PropertySlider(0f, 2f)]
    public float MotionScale = 1f;

    [PropertyDisplay("Compact layout", tooltip: "Tighter padding and shorter rows, so more fits on screen.")]
    public bool CompactSettings = false;

    [PropertyDisplay("Inline descriptions", tooltip: "Show each setting's description under its name. Turn it off to show descriptions on hover instead.")]
    public bool InlineDescriptions = true;

    [PropertyDisplay("Icons-only sidebar", tooltip: "Turn the sidebar into a narrow strip of icons. Hover an icon to see its name.")]
    public bool IconSidebar = false;

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

    float IM3Settings.UiMotionScale
    {
        get => MotionScale;
        set => MotionScale = value;
    }

    bool IM3Settings.UiIconSidebar
    {
        get => IconSidebar;
        set => IconSidebar = value;
    }

    bool IM3Settings.UiDescriptionTooltips => !InlineDescriptions;
}
