using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;

namespace BossMod;

public static class UICombo
{
    public static string EnumString<T>(T value) where T : struct, Enum => GeneratedEnumMetadata.DisplayName(value);
    public static string EnumString(Type type, Enum value) => GeneratedEnumMetadata.DisplayName(type, value);

    public static bool Enum<T>(string label, ref T value, Func<T, string>? print = null, Func<T, bool>? filter = null) where T : struct, Enum
    {
        var values = GeneratedEnumMetadata.Values<T>();
        var current = GeneratedEnumMetadata.IndexOf(value);
        if (current < 0)
        {
            current = 0;
        }

        print ??= static item => EnumString(item);
        filter ??= static _ => true;
        if (!EnumIndexCore(label, values.Length, ref current, index => print(values[index]), index => filter(values[index])))
        {
            return false;
        }
        value = values[current];
        return true;
    }

    public static bool Enum(string label, Type type, ref Enum value, Func<Enum, string>? print = null, Func<Enum, bool>? filter = null)
    {
        var values = GeneratedEnumMetadata.Values(type);
        var current = GeneratedEnumMetadata.IndexOf(type, value);
        if (current < 0)
        {
            current = 0;
        }

        print ??= item => EnumString(type, item);
        filter ??= static _ => true;
        if (!EnumIndexCore(label, values.Length, ref current, index => print((Enum)values.GetValue(index)!), index => filter((Enum)values.GetValue(index)!)))
        {
            return false;
        }
        value = (Enum)values.GetValue(current)!;
        return true;
    }

    public static bool EnumIndex(string label, Type type, ref int value, Func<int, string>? print = null, Func<int, bool>? filter = null)
    {
        var values = GeneratedEnumMetadata.Values(type);
        print ??= index => EnumString(type, (Enum)values.GetValue(index)!);
        filter ??= static _ => true;
        return EnumIndexCore(label, values.Length, ref value, print, filter);
    }

    private static bool EnumIndexCore(string label, int count, ref int value, Func<int, string> print, Func<int, bool> filter)
    {
        var result = false;
        var width = 300f * ImGuiHelpers.GlobalScale;
        ImGui.SetNextItemWidth(width);

        var currentLabel = print(value);
        var showLabelPopup = ImGui.CalcTextSize(currentLabel).X > width;
        if (ImGui.BeginCombo($"###{label}", currentLabel))
        {
            showLabelPopup = false;
            for (var i = 0; i < count; ++i)
            {
                if (!filter(i))
                {
                    continue;
                }
                if (ImGui.Selectable(print(i), i == value))
                {
                    value = i;
                    result = true;
                }
            }
            ImGui.EndCombo();
        }
        if (showLabelPopup && ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(currentLabel);
        }

        if (!label.StartsWith('#'))
        {
            ImGui.SameLine();
            ImGui.Text(label);
        }
        return result;
    }

    public static bool Radio(Type type, ref int value, bool oneLine, Func<int, string>? print = null)
    {
        var values = GeneratedEnumMetadata.Values(type);
        print ??= index => EnumString(type, (Enum)values.GetValue(index)!);
        var original = value;
        var result = false;
        var len = values.Length;
        for (var i = 0; i < len; ++i)
        {
            if (ImGui.RadioButton(print(i), i == value))
            {
                value = i;
                result = i != original;
            }
            if (oneLine && i + 1 < len)
            {
                ImGui.SameLine();
            }
        }
        return result;
    }

    public static bool Int(string label, string[] values, ref int value)
    {
        var result = false;
        ImGui.SetNextItemWidth(200f);
        var len = values.Length;
        if (ImGui.BeginCombo(label, value < len ? values[value] : value.ToString()))
        {
            for (var i = 0; i < len; ++i)
            {
                if (ImGui.Selectable(values[i], value == i))
                {
                    value = i;
                    result = true;
                }
            }
            ImGui.EndCombo();
        }
        return result;
    }

    public static bool Bool(string label, string[] values, ref bool value)
    {
        var raw = value ? 1 : 0;
        if (!Int(label, values, ref raw))
        {
            return false;
        }
        value = raw != 0;
        return true;
    }
}
