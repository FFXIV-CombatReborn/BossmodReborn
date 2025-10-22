using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Linq;

namespace BossMod;

public sealed class ConfigRoot
{
    public Event Modified = new();
    public readonly Dictionary<Type, ConfigNode> _nodes = [];
    public List<ConfigNode> Nodes => [.. _nodes.Values];

    public void Initialize()
    {
        foreach (var t in Utils.GetDerivedTypes<ConfigNode>(Assembly.GetExecutingAssembly()).Where(t => !t.IsAbstract))
        {
            if (t == typeof(RSRIntegrationConfig) && !IsRSRInstalled())
            {
                // Skip adding RSR integration settings if Rotation Solver Reborn is not installed/loaded
                continue;
            }

            if (Activator.CreateInstance(t) is not ConfigNode inst)
            {
                Service.Log($"[Config] Failed to create an instance of {t}");
                continue;
            }
            inst.Modified.Subscribe(Modified.Fire);
            _nodes[t] = inst;
        }
    }

    private static bool IsRSRInstalled()
    {
        try
        {
            const string rsrName = "Rotation Solver Reborn";
            var plugins = Service.PluginInterface.InstalledPlugins;
            foreach (var p in plugins)
            {
                if (p.IsLoaded && (p.Name.Equals(rsrName, StringComparison.OrdinalIgnoreCase) || p.InternalName.Equals(rsrName, StringComparison.OrdinalIgnoreCase)))
                    return true;
            }
        }
        catch
        {
            // ignore errors and assume not installed
        }
        return false;
    }

    public T Get<T>() where T : ConfigNode => (T)_nodes[typeof(T)];
    public T Get<T>(Type derived) where T : ConfigNode => (T)_nodes[derived];

    public ConfigListener<T> GetAndSubscribe<T>(Action<T> modified) where T : ConfigNode => new(Get<T>(), modified);

    public void LoadFromFile(FileInfo file)
    {
        try
        {
            var data = ConfigConverter.Schema.Load(file);
            using var json = data.document;
            var ser = Serialization.BuildSerializationOptions();
            foreach (var jconfig in data.payload.EnumerateObject())
            {
                var type = Type.GetType(jconfig.Name);
                var node = type != null ? _nodes.GetValueOrDefault(type) : null;
                node?.Deserialize(jconfig.Value, ser);
            }
        }
        catch (Exception e)
        {
            Service.Log($"Failed to load config from {file.FullName}: {e}");
        }
    }

    public void SaveToFile(FileInfo file)
    {
        try
        {
            var ser = Serialization.BuildSerializationOptions();
            var serializedNodes = new ConcurrentDictionary<Type, string>();

            Parallel.ForEach(_nodes, entry =>
            {
                using var ms = new MemoryStream();
                using var tempWriter = new Utf8JsonWriter(ms);
                entry.Value.Serialize(tempWriter, ser);
                tempWriter.Flush();
                serializedNodes[entry.Key] = Encoding.UTF8.GetString(ms.ToArray());
            });

            using var stream = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.None);
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });

            writer.WriteStartObject();
            writer.WriteNumber("Version", ConfigConverter.Schema.CurrentVersion);
            writer.WritePropertyName("Payload");
            writer.WriteStartObject();

            foreach (var (t, jsonStr) in serializedNodes)
            {
                writer.WritePropertyName(t.FullName!);
                writer.WriteRawValue(jsonStr);
            }

            writer.WriteEndObject();
            writer.WriteEndObject();
        }
        catch (Exception e)
        {
            Service.Log($"Failed to save config to {file.FullName}: {e}");
        }
    }

    public List<string> ConsoleCommand(ReadOnlySpan<string> args, bool save = true)
    {
        List<string> result = [];
        if (args.Length == 0)
        {
            result.Add("Usage: /bmr cfg <config-type> <field> <value>");
            result.Add("Both config-type and field can be shortened. Valid config-types:");
            foreach (var t in _nodes.Keys)
                result.Add($"- {t.Name}");
        }
        else
        {
            List<ConfigNode> matchingNodes = [];
            foreach (var (t, n) in _nodes)
                if (t.Name.Contains(args[0], StringComparison.CurrentCultureIgnoreCase))
                    matchingNodes.Add(n);
            if (matchingNodes.Count == 0)
            {
                result.Add("Config type not found. Valid types:");
                foreach (var t in _nodes.Keys)
                    result.Add($"- {t.Name}");
            }
            else if (matchingNodes.Count > 1)
            {
                result.Add("Ambiguous config type, pass longer pattern. Matches:");
                foreach (var n in matchingNodes)
                    result.Add($"- {n.GetType().Name}");
            }
            else if (args.Length == 1)
            {
                result.Add("Usage: /bmr cfg <config-type> <field> <value>");
                result.Add($"Valid fields for {matchingNodes[0].GetType().Name}:");
                foreach (var f in matchingNodes[0].GetType().GetFields().Where(f => f.GetCustomAttribute<PropertyDisplayAttribute>() != null))
                    result.Add($"- {f.Name}");
            }
            else
            {
                List<FieldInfo> matchingFields = [];
                foreach (var f in matchingNodes[0].GetType().GetFields().Where(f => f.GetCustomAttribute<PropertyDisplayAttribute>() != null))
                    if (f.Name.Contains(args[1], StringComparison.CurrentCultureIgnoreCase))
                        matchingFields.Add(f);
                if (matchingFields.Count == 0)
                {
                    result.Add($"Field not found {args[1]}, Valid fields:");
                    foreach (var f in matchingNodes[0].GetType().GetFields().Where(f => f.GetCustomAttribute<PropertyDisplayAttribute>() != null))
                        result.Add($"- {f.Name}");
                }
                else if (matchingFields.Count > 1)
                {
                    result.Add("Ambiguous field name, pass longer pattern. Matches:");
                    foreach (var f in matchingFields)
                        result.Add($"- {f.Name}");
                }
                /*else if (args.Count == 2)
                {
                    result.Add("Usage: /bmr cfg <config-type> <field> <value>");
                    result.Add($"Type of {matchingNodes[0].GetType().Name}.{matchingFields[0].Name} is {matchingFields[0].FieldType.Name}");
                }*/
                else
                {
                    try
                    {
                        if (args.Length == 2)
                            result.Add(matchingFields[0].GetValue(matchingNodes[0])?.ToString() ?? $"Failed to get value of '{args[2]}'");
                        else
                        {
                            var val = FromConsoleString(args[2], matchingFields[0].FieldType);
                            if (val == null)
                            {
                                result.Add($"Failed to convert '{args[2]}' to {matchingFields[0].FieldType}");
                            }
                            else
                            {
                                matchingFields[0].SetValue(matchingNodes[0], val);
                                if (save)
                                    matchingNodes[0].Modified.Fire();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (args.Length == 2)
                            result.Add($"Failed to get value of {matchingNodes[0].GetType().Name}.{matchingFields[0].Name} : {e}");
                        else
                            result.Add($"Failed to set {matchingNodes[0].GetType().Name}.{matchingFields[0].Name} to {args[2]}: {e}");
                    }
                }
            }
        }
        return result;
    }

    private object? FromConsoleString(string str, Type t)
        => t == typeof(bool) ? bool.Parse(str)
        : t == typeof(float) ? float.Parse(str)
        : t == typeof(int) ? int.Parse(str)
        : t.IsAssignableTo(typeof(Enum)) ? Enum.Parse(t, str)
        : null;
}
