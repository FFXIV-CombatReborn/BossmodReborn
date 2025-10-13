using System.Text.Json;
using System.Text.Json.Serialization;

namespace BossMod;

[JsonConverter(typeof(JsonColorConverter))]
[SkipLocalsInit]
public readonly struct Color(uint abgr)
{
    public readonly uint ABGR = abgr;
    public readonly uint R => ABGR & 0xFF;
    public readonly uint G => (ABGR >> 8) & 0xFF;
    public readonly uint B => (ABGR >> 16) & 0xFF;
    public readonly uint A => (ABGR >> 24) & 0xFF;

    private const float ToFloat = 1.0f / 255;

    public static Color FromComponents(uint r, uint g, uint b, uint a = 255) => new(((a & 0xFF) << 24) | ((b & 0xFF) << 16) | ((g & 0xFF) << 8) | (r & 0xFF));
    public static Color FromRGBA(uint rgba) => FromComponents(rgba >> 24, rgba >> 16, rgba >> 8, rgba);
    public static Color FromARGB(uint argb) => FromComponents(argb >> 16, argb >> 8, argb, argb >> 24);

    public static Color FromFloat4(Vector4 vec)
    {
        var r = Math.Clamp((uint)(vec.X * 255f), default, 255u);
        var g = Math.Clamp((uint)(vec.Y * 255f), default, 255u);
        var b = Math.Clamp((uint)(vec.Z * 255f), default, 255u);
        var a = Math.Clamp((uint)(vec.W * 255f), default, 255u);
        return FromComponents(r, g, b, a);
    }

    public readonly uint ToRGBA() => (R << 24) | (G << 16) | (B << 8) | A;
    public readonly uint ToARGB() => (A << 24) | (R << 16) | (G << 8) | B;
    public readonly Vector4 ToFloat4() => new Vector4(R, G, B, A) * ToFloat;
}

[SkipLocalsInit]
public class JsonColorConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        return str?.Length > 0 ? Color.FromRGBA(uint.Parse(str[1..], System.Globalization.NumberStyles.HexNumber)) : default;
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        writer.WriteStringValue($"#{value.ToRGBA():X8}");
    }
}

[SkipLocalsInit]
public static class Colors
{
    private static readonly ColorConfig _config = Service.Config.Get<ColorConfig>();

    public static uint Background => _config.ArenaBackground.ABGR;
    public static uint Border => _config.ArenaBorder.ABGR;
    public static uint AOE => _config.ArenaAOE.ABGR;
    public static uint SafeFromAOE => _config.ArenaSafeFromAOE.ABGR;
    public static uint Danger => _config.ArenaDanger.ABGR;
    public static uint Safe => _config.ArenaSafe.ABGR;
    public static uint Trap => _config.ArenaTrap.ABGR;
    public static uint PC => _config.ArenaPC.ABGR;
    public static uint Enemy => _config.ArenaEnemy.ABGR;
    public static uint Object => _config.ArenaObject.ABGR;
    public static uint PlayerInteresting => _config.ArenaPlayerInteresting.ABGR;
    public static uint PlayerGeneric => _config.ArenaPlayerGeneric.ABGR;
    public static uint Vulnerable => _config.ArenaVulnerable.ABGR;
    public static uint FutureVulnerable => _config.ArenaFutureVulnerable.ABGR;
    public static uint MeleeRangeIndicator => _config.ArenaMeleeRangeIndicator.ABGR;
    public static uint Other1 => _config.ArenaOther[0].ABGR;
    public static uint Other2 => _config.ArenaOther[1].ABGR;
    public static uint Other3 => _config.ArenaOther[2].ABGR;
    public static uint Other4 => _config.ArenaOther[3].ABGR;
    public static uint Other5 => _config.ArenaOther[4].ABGR;
    public static uint Other6 => _config.ArenaOther[5].ABGR;
    public static uint Other7 => _config.ArenaOther[6].ABGR;
    public static uint Other8 => _config.ArenaOther[7].ABGR;
    public static uint Other9 => _config.ArenaOther[8].ABGR;
    public static uint Shadows => _config.Shadows.ABGR;
    public static uint CardinalN => _config.CardinalN.ABGR;
    public static uint CardinalE => _config.CardinalE.ABGR;
    public static uint CardinalS => _config.CardinalS.ABGR;
    public static uint CardinalW => _config.CardinalW.ABGR;
    public static uint WaymarkA => _config.WaymarkA.ABGR;
    public static uint WaymarkB => _config.WaymarkB.ABGR;
    public static uint WaymarkC => _config.WaymarkC.ABGR;
    public static uint WaymarkD => _config.WaymarkD.ABGR;
    public static uint Waymark1 => _config.Waymark1.ABGR;
    public static uint Waymark2 => _config.Waymark2.ABGR;
    public static uint Waymark3 => _config.Waymark3.ABGR;
    public static uint Waymark4 => _config.Waymark4.ABGR;
    public static uint ButtonPushColor1 => _config.ButtonPushColor[0].ABGR;
    public static uint ButtonPushColor2 => _config.ButtonPushColor[1].ABGR;
    public static uint ButtonPushColor3 => _config.ButtonPushColor[2].ABGR;
    public static uint ButtonPushColor4 => _config.ButtonPushColor[3].ABGR;
    public static uint ButtonPushColor5 => _config.ButtonPushColor[4].ABGR;
    public static uint ButtonPushColor6 => _config.ButtonPushColor[5].ABGR;
    public static uint TextColor1 => _config.TextColors[0].ABGR;
    public static uint TextColor2 => _config.TextColors[1].ABGR;
    public static uint TextColor3 => _config.TextColors[2].ABGR;
    public static uint TextColor4 => _config.TextColors[3].ABGR;
    public static uint TextColor5 => _config.TextColors[4].ABGR;
    public static uint TextColor6 => _config.TextColors[5].ABGR;
    public static uint TextColor7 => _config.TextColors[6].ABGR;
    public static uint TextColor8 => _config.TextColors[7].ABGR;
    public static uint TextColor9 => _config.TextColors[8].ABGR;
    public static uint TextColor10 => _config.TextColors[9].ABGR;
    public static uint TextColor11 => _config.TextColors[10].ABGR;
    public static uint TextColor12 => _config.TextColors[11].ABGR;
    public static uint TextColor13 => _config.TextColors[12].ABGR;
    public static uint TextColor14 => _config.TextColors[13].ABGR;
    public static uint TextColor15 => _config.TextColors[14].ABGR;
    public static uint TextColor16 => _config.TextColors[15].ABGR;
    public static uint TextColor17 => _config.TextColors[16].ABGR;
    public static uint TextColor18 => _config.TextColors[17].ABGR;
    public static uint PositionalColor1 => _config.PositionalColors[0].ABGR;
    public static uint PositionalColor2 => _config.PositionalColors[1].ABGR;
    public static uint PositionalColor3 => _config.PositionalColors[2].ABGR;
    public static uint PositionalColor4 => _config.PositionalColors[3].ABGR;
    public static uint Tank => _config.ArenaPlayerGenericTank.ABGR;
    public static uint Healer => _config.ArenaPlayerGenericHealer.ABGR;
    public static uint Melee => _config.ArenaPlayerGenericMelee.ABGR;
    public static uint Caster => _config.ArenaPlayerGenericCaster.ABGR;
    public static uint PhysRanged => _config.ArenaPlayerGenericPhysRanged.ABGR;
    public static uint Focus => _config.ArenaPlayerGenericFocus.ABGR;
    public static uint CollisionColor1 => _config.CollisionColors[0].ABGR;
    public static uint CollisionColor2 => _config.CollisionColors[1].ABGR;
    public static uint CollisionColor3 => _config.CollisionColors[2].ABGR;
    public static uint PathfindingColor1 => _config.PathfindingColors[0].ABGR;
    public static uint PathfindingColor2 => _config.PathfindingColors[1].ABGR;
    public static uint PathfindingColor3 => _config.PathfindingColors[2].ABGR;
    public static uint PathfindingColor4 => _config.PathfindingColors[3].ABGR;
    public static uint PathfindingColor5 => _config.PathfindingColors5.ABGR;
    public static uint PathfindingColor6 => _config.PathfindingColors6.ABGR;
    public static uint PathfindingColor7 => _config.PathfindingColors7.ABGR;
    public static uint PathfindingColor8 => _config.PathfindingColors8.ABGR;
}
