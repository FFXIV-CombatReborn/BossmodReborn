using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;

namespace BossMod;

// attribute that specifies how config node should be shown in the UI
[AttributeUsage(AttributeTargets.Class)]
public sealed class ConfigDisplayAttribute : Attribute
{
    public string? Name { get; set; }
    public int Order { get; set; }
    public Type? Parent { get; set; }
    public string[]? Tags { get; set; }
}

// attribute that specifies how config node field or enumeration value is shown in the UI
[AttributeUsage(AttributeTargets.Field)]
public sealed class PropertyDisplayAttribute(string label, uint color = default, string tooltip = "", bool separator = false, string[]? tags = null) : Attribute
{
    public string Label { get; } = label;
    public uint Color { get; } = color == default ? Colors.TextColor1 : color;
    public string Tooltip { get; } = tooltip;
    public bool Separator { get; } = separator;
    public string[] Tags { get; } = tags ?? [];
}

// attribute that specifies combobox should be used for displaying int/bool property
[AttributeUsage(AttributeTargets.Field)]
public sealed class PropertyComboAttribute(string[] values) : Attribute
{
    public string[] Values { get; } = values;

#pragma warning disable CA1019 // this is just a shorthand
    public PropertyComboAttribute(string falseText, string trueText) : this([falseText, trueText]) { }
#pragma warning restore CA1019
}

// attribute that specifies slider should be used for displaying float/int property
[AttributeUsage(AttributeTargets.Field)]
public sealed class PropertySliderAttribute(float min, float max) : Attribute
{
    public float Speed { get; set; } = 1f;
    public float Min { get; } = min;
    public float Max { get; } = max;
    public bool Logarithmic { get; set; }
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class PropertyStringOrderAttribute(string[] values) : Attribute
{
    public string[] Values { get; } = values;
}

// base class for configuration nodes
public abstract class ConfigNode
{
    // event fired when configuration node was modified; should be fired by anyone making any modifications
    // root subscribes to modification event to save updated configuration
    [JsonIgnore]
    public Event Modified = new();

    // draw custom contents; override this for complex config nodes
    public virtual void DrawCustom(UITree tree, WorldState ws) { }

    private static readonly ConcurrentDictionary<Type, FieldInfo[]> _fieldsCache = [];

    protected static FieldInfo[] GetSerializableFields(Type t)
    {
        if (_fieldsCache.TryGetValue(t, out var cachedFields))
            return cachedFields;

        var fields = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var len = fields.Length;
        var discoveredFields = new FieldInfo[len];
        var index = 0;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var field = ref fields[i];
            if (!field.IsStatic && !field.IsDefined(typeof(JsonIgnoreAttribute), false))
            {
                discoveredFields[index++] = field;
            }
        }

        return _fieldsCache[t] = discoveredFields[..index];
    }

    // deserialize fields from json; default implementation should work fine for most cases
    public virtual void Deserialize(JsonElement j, JsonSerializerOptions ser)
    {
        var type = GetType();
        foreach (var jfield in j.EnumerateObject())
        {
            var field = type.GetField(jfield.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                if (field.IsStatic)
                    continue;
                if (field.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                    continue;

                var value = jfield.Value.Deserialize(field.FieldType, ser);
                if (value != null)
                {
                    field.SetValue(this, value);
                }
            }
        }
    }

    // serialize node to json;
    public virtual void Serialize(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        var fields = GetSerializableFields(GetType());
        var len = fields.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var field = ref fields[i];
            var fieldValue = field.GetValue(this);

            writer.WritePropertyName(field.Name);
            if (fieldValue is ConfigNode subNode)
            {
                subNode.Serialize(writer, options);
            }
            else
            {
                JsonSerializer.Serialize(writer, fieldValue, field.FieldType, options);
            }
        }

        writer.WriteEndObject();
    }
}

// utility to simplify listening for config modifications; callback is executed immediately for initial state
public sealed class ConfigListener<T>(T data, Action<T> modified) : IDisposable where T : ConfigNode
{
    public readonly T Data = data;
    private readonly EventSubscription _listener = data.Modified.ExecuteAndSubscribe(() => modified(data));

    public void Dispose() => _listener.Dispose();
}
