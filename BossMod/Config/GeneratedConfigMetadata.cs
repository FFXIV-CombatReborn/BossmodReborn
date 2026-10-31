namespace BossMod;

internal sealed class ConfigFieldMetadata(string name, Type fieldType, bool serializable, Func<ConfigNode, object?> getter,
    Action<ConfigNode, object?> setter, SectionStartAttribute? sectionStart, PropertyDisplayAttribute? display, PropertyComboAttribute? combo,
    PropertySliderAttribute? slider, PropertyStringOrderAttribute? stringOrder, GroupDetailsAttribute? group, GroupPresetAttribute[] groupPresets, bool radio)
{
    public readonly string Name = name;
    public readonly Type FieldType = fieldType;
    public readonly bool Serializable = serializable;
    public readonly Func<ConfigNode, object?> Getter = getter;
    public readonly Action<ConfigNode, object?> Setter = setter;
    public readonly SectionStartAttribute? SectionStart = sectionStart;
    public readonly PropertyDisplayAttribute? Display = display;
    public readonly PropertyComboAttribute? Combo = combo;
    public readonly PropertySliderAttribute? Slider = slider;
    public readonly PropertyStringOrderAttribute? StringOrder = stringOrder;
    public readonly GroupDetailsAttribute? Group = group;
    public readonly GroupPresetAttribute[] GroupPresets = groupPresets;
    public readonly bool Radio = radio;
}

internal sealed class ConfigTypeMetadata
{
    public ConfigTypeMetadata(Type type, ConfigDisplayAttribute? display, ConfigFieldMetadata[] fields)
    {
        Type = type;
        Display = display;
        Fields = fields;

        var len = fields.Length;
        var fieldsByName = new Dictionary<string, ConfigFieldMetadata>(len, StringComparer.Ordinal);
        var serializableCount = 0;
        var displayCount = 0;
        for (var i = 0; i < len; ++i)
        {
            var field = fields[i];
            if (field.Serializable)
            {
                ++serializableCount;
            }
            if (field.Display != null)
            {
                ++displayCount;
            }
        }

        SerializableFields = new ConfigFieldMetadata[serializableCount];
        DisplayFields = new ConfigFieldMetadata[displayCount];
        var cSerializeable = 0;
        var cDisplay = 0;
        for (var i = 0; i < len; ++i)
        {
            var field = fields[i];
            if (field.Serializable)
            {
                SerializableFields[cSerializeable++] = field;
            }

            if (field.Display != null)
            {
                DisplayFields[cDisplay++] = field;
            }

            fieldsByName.Add(field.Name, field);
        }
        FieldsByName = fieldsByName;
    }

    public Type Type;
    public ConfigDisplayAttribute? Display;
    public ConfigFieldMetadata[] Fields;
    public ConfigFieldMetadata[] SerializableFields;
    public ConfigFieldMetadata[] DisplayFields;
    public Dictionary<string, ConfigFieldMetadata> FieldsByName;
}

// All accessors are generated direct field accesses
internal static partial class GeneratedConfigMetadata
{
    private static readonly Dictionary<Type, ConfigTypeMetadata> _byType = Build();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Dictionary<Type, ConfigTypeMetadata> Build()
    {
        Dictionary<Type, ConfigTypeMetadata> result = [];
        Register(result);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static partial void Register(Dictionary<Type, ConfigTypeMetadata> metadata);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConfigTypeMetadata Get(Type type)
        => _byType.GetValueOrDefault(type) ?? throw new ArgumentException($"No generated config metadata for {type.FullName}");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConfigTypeMetadata Get(ConfigNode node) => Get(node.GetType());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConfigTypeMetadata Get<T>() where T : ConfigNode => Get(typeof(T));
}
