using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace BossMod.SourceGen;

public sealed class ConfigGenerator : IIncrementalGenerator
{
    private const string Category = "BossMod.SourceGen";
    private const int ChunkSize = 64;
    private const string ConfigNodeMetadataName = "BossMod.ConfigNode";
    private const string ConfigDisplayMetadataName = "BossMod.ConfigDisplayAttribute";
    private const string SectionStartMetadataName = "BossMod.SectionStartAttribute";
    private const string PropertyDisplayMetadataName = "BossMod.PropertyDisplayAttribute";
    private const string PropertyComboMetadataName = "BossMod.PropertyComboAttribute";
    private const string PropertySliderMetadataName = "BossMod.PropertySliderAttribute";
    private const string PropertyStringOrderMetadataName = "BossMod.PropertyStringOrderAttribute";
    private const string PropertyRadioMetadataName = "BossMod.PropertyRadioAttribute";
    private const string GroupDetailsMetadataName = "BossMod.GroupDetailsAttribute";
    private const string GroupPresetMetadataName = "BossMod.GroupPresetAttribute";
    private const string JsonIgnoreMetadataName = "System.Text.Json.Serialization.JsonIgnoreAttribute";

    private static readonly DiagnosticDescriptor MissingSymbol = new(
        "BMSG200", "Config source generation could not start", "Required symbol '{0}' was not found",
        Category, DiagnosticSeverity.Error, true);

    private static readonly DiagnosticDescriptor InvalidConfig = new(
        "BMSG201", "Config cannot be source generated", "Config '{0}' cannot be source generated: {1}",
        Category, DiagnosticSeverity.Error, true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
        => Register(context, SourceGenUtilities.DeclaredTypes(context));

    internal static void Register(
        IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<INamedTypeSymbol> declaredTypes)
    {
        // Project symbols to immutable value snapshots before Collect(). This is the
        // important incremental boundary: unchanged configs compare equal even when
        // Roslyn recreates symbols after unrelated edits elsewhere in the compilation.
        var configs = declaredTypes
            .Where(static type => IsPotentialConfig(type))
            .Select(static (type, _) => ConfigSnapshot.Create(type))
            .Collect();
        var frameworkState = context.CompilationProvider
            .Select(static (compilation, _) => ConfigFrameworkState.Create(compilation));

        context.RegisterImplementationSourceOutput(configs.Combine(frameworkState), static (productionContext, value) =>
        {
            if (value.Right.MissingSymbols != 0)
            {
                ReportMissingSymbols(productionContext, value.Right.MissingSymbols);
                return;
            }
            Generate(productionContext, value.Left, value.Right);
        });
    }

    private static bool IsPotentialConfig(INamedTypeSymbol type) => type.TypeKind == TypeKind.Class && !type.IsAbstract
            && SourceGenUtilities.InheritsFrom(type, ConfigNodeMetadataName);

    private static void Generate(SourceProductionContext context, ImmutableArray<ConfigSnapshot> snapshots, ConfigFrameworkState frameworkState)
    {
        var treeOrder = frameworkState.CreateSourcePathOrder();
        var configs = new List<ConfigSnapshot>(snapshots.Length);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var snapshot in snapshots)
        {
            // Partial declarations can surface the same symbol more than once.
            if (!seen.Add(snapshot.TypeName))
            {
                continue;
            }

            if (snapshot.TypeError is { } typeError)
            {
                Report(context, snapshot.TypeName, typeError.Location, typeError.Reason);
                continue;
            }

            var fields = new List<ConfigFieldSnapshot>(snapshot.Fields.Length);
            fields.AddRange(snapshot.Fields);
            fields.Sort((left, right) => CompareFieldOrder(left, right, treeOrder));

            var valid = true;
            foreach (var field in fields)
            {
                if (field.Error is { } error)
                {
                    Report(context, snapshot.TypeName, error.Location, error.Reason);
                    valid = false;
                }
            }
            if (valid)
            {
                configs.Add(snapshot.WithFields([.. fields]));
            }
        }

        configs.Sort(static (left, right) => StringComparer.Ordinal.Compare(left.TypeName, right.TypeName));
        context.AddSource("GeneratedConfigMetadata.g.cs", SourceText.From(Render(configs), Encoding.UTF8));
    }

    private static int CompareFieldOrder(ConfigFieldSnapshot left, ConfigFieldSnapshot right, IReadOnlyDictionary<string, int> treeOrder)
    {
        var leftOrder = treeOrder.TryGetValue(left.SourceTreeKey, out var l) ? l : int.MaxValue;
        var rightOrder = treeOrder.TryGetValue(right.SourceTreeKey, out var r) ? r : int.MaxValue;
        var byTree = leftOrder.CompareTo(rightOrder);
        if (byTree != 0)
        {
            return byTree;
        }
        var bySpan = left.SourceSpanStart.CompareTo(right.SourceSpanStart);
        return bySpan != 0 ? bySpan : StringComparer.Ordinal.Compare(left.Name, right.Name);
    }

    private static int GetMissingRequiredSymbols(Compilation compilation)
    {
        var result = 0;
        if (compilation.GetTypeByMetadataName(ConfigNodeMetadataName) == null)
        {
            result |= 1 << 0;
        }
        if (compilation.GetTypeByMetadataName(ConfigDisplayMetadataName) == null)
        {
            result |= 1 << 1;
        }
        if (compilation.GetTypeByMetadataName(PropertyDisplayMetadataName) == null)
        {
            result |= 1 << 2;
        }
        if (compilation.GetTypeByMetadataName(PropertyComboMetadataName) == null)
        {
            result |= 1 << 3;
        }
        if (compilation.GetTypeByMetadataName(PropertySliderMetadataName) == null)
        {
            result |= 1 << 4;
        }
        if (compilation.GetTypeByMetadataName(PropertyStringOrderMetadataName) == null)
        {
            result |= 1 << 5;
        }
        if (compilation.GetTypeByMetadataName(GroupDetailsMetadataName) == null)
        {
            result |= 1 << 6;
        }
        if (compilation.GetTypeByMetadataName(GroupPresetMetadataName) == null)
        {
            result |= 1 << 7;
        }
        if (compilation.GetTypeByMetadataName(JsonIgnoreMetadataName) == null)
        {
            result |= 1 << 8;
        }
        if (compilation.GetTypeByMetadataName(SectionStartMetadataName) == null)
        {
            result |= 1 << 9;
        }
        return result;
    }

    private static void ReportMissingSymbols(SourceProductionContext context, int missing)
    {
        if ((missing & (1 << 0)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingSymbol, Location.None, ConfigNodeMetadataName));
        }
        if ((missing & (1 << 1)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingSymbol, Location.None, ConfigDisplayMetadataName));
        }
        if ((missing & (1 << 2)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingSymbol, Location.None, PropertyDisplayMetadataName));
        }
        if ((missing & (1 << 3)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingSymbol, Location.None, PropertyComboMetadataName));
        }
        if ((missing & (1 << 4)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingSymbol, Location.None, PropertySliderMetadataName));
        }
        if ((missing & (1 << 5)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingSymbol, Location.None, PropertyStringOrderMetadataName));
        }
        if ((missing & (1 << 6)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingSymbol, Location.None, GroupDetailsMetadataName));
        }
        if ((missing & (1 << 7)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingSymbol, Location.None, GroupPresetMetadataName));
        }
        if ((missing & (1 << 8)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingSymbol, Location.None, JsonIgnoreMetadataName));
        }
        if ((missing & (1 << 9)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingSymbol, Location.None, SectionStartMetadataName));
        }
    }

    private static string Render(IReadOnlyList<ConfigSnapshot> configs)
    {
        var fieldCount = 0;
        var count = configs.Count;
        for (var i = 0; i < count; ++i)
        {
            fieldCount += configs[i].Fields.Length;
        }
        var sb = new StringBuilder(Math.Max(1024, fieldCount * 640));
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("namespace BossMod;");
        sb.AppendLine();
        sb.AppendLine("internal static partial class GeneratedConfigMetadata");
        sb.AppendLine("{");
        sb.AppendLine("    private static partial void Register(global::System.Collections.Generic.Dictionary<global::System.Type, ConfigTypeMetadata> metadata)");
        sb.AppendLine("    {");
        var chunkCount = (configs.Count + ChunkSize - 1) / ChunkSize;
        for (var chunk = 0; chunk < chunkCount; ++chunk)
        {
            sb.Append("        RegisterChunk").Append(chunk).AppendLine("(metadata);");
        }
        sb.AppendLine("    }");

        for (var chunk = 0; chunk < chunkCount; ++chunk)
        {
            sb.AppendLine();
            sb.Append("    private static void RegisterChunk").Append(chunk).AppendLine("(global::System.Collections.Generic.Dictionary<global::System.Type, ConfigTypeMetadata> metadata)");
            sb.AppendLine("    {");
            var end = Math.Min(count, (chunk + 1) * ChunkSize);
            for (var i = chunk * ChunkSize; i < end; ++i)
            {
                RenderConfig(sb, configs[i]);
            }
            sb.AppendLine("    }");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static void RenderConfig(StringBuilder sb, ConfigSnapshot config)
    {
        var typeName = config.TypeName;
        sb.Append("        metadata.Add(typeof(").Append(typeName).Append("), new ConfigTypeMetadata(typeof(").Append(typeName).Append("), ")
            .Append(config.DisplayExpression).AppendLine(", new ConfigFieldMetadata[]");
        sb.AppendLine("        {");
        var fields = config.Fields;
        var len = fields.Length;
        for (var i = 0; i < len; ++i)
        {
            var field = fields[i];
            sb.Append("            new ConfigFieldMetadata(").Append(SourceGenUtilities.PrimitiveLiteral(field.Name)).Append(", typeof(").Append(field.TypeName).Append("), ").Append(field.Serializable ? "true" : "false").Append(", ")
                .Append("static node => ((").Append(typeName).Append(")node).").Append(field.EscapedName).Append(", ")
                .Append("static (node, value) => ((").Append(typeName).Append(")node).").Append(field.EscapedName).Append(" = (").Append(field.TypeName).Append(")value!, ")
                .Append(field.SectionStartExpression).Append(", ")
                .Append(field.PropertyDisplayExpression).Append(", ")
                .Append(field.PropertyComboExpression).Append(", ")
                .Append(field.PropertySliderExpression).Append(", ")
                .Append(field.PropertyStringOrderExpression).Append(", ")
                .Append(field.GroupDetailsExpression).Append(", ")
                .Append(field.PresetArrayExpression).Append(", ")
                .Append(field.Radio ? "true" : "false").AppendLine("),");
        }
        sb.AppendLine("        }));");
    }

    private static string AttributeExpression(AttributeData? attribute)
    {
        var sb = new StringBuilder(128);
        SourceGenUtilities.AppendAttributeExpression(sb, attribute);
        return sb.ToString();
    }

    private static string PresetArrayExpression(ImmutableArray<AttributeData> attributes)
    {
        var sb = new StringBuilder(128);
        var found = false;
        var len = attributes.Length;
        for (var i = 0; i < len; ++i)
        {
            if (attributes[i].AttributeClass is not { } attributeClass || !SourceGenUtilities.HasMetadataName(attributeClass, GroupPresetMetadataName))
            {
                continue;
            }
            if (found)
            {
                sb.Append(", ");
            }
            else
            {
                sb.Append("new GroupPresetAttribute[] { ");
                found = true;
            }
            SourceGenUtilities.AppendAttributeExpression(sb, attributes[i]);
        }
        if (found)
        {
            sb.Append(" }");
        }
        else
        {
            sb.Append("[]");
        }
        return sb.ToString();
    }

    private static void Report(SourceProductionContext context, string typeName, DiagnosticLocation location, string reason)
        => context.ReportDiagnostic(Diagnostic.Create(InvalidConfig, location.ToLocation(), typeName, reason));

    private sealed class ConfigFrameworkState : IEquatable<ConfigFrameworkState>
    {
        private ConfigFrameworkState(int missingSymbols, string[] sourcePaths)
        {
            MissingSymbols = missingSymbols;
            SourcePaths = sourcePaths;
        }

        public readonly int MissingSymbols;
        private readonly string[] SourcePaths;

        public static ConfigFrameworkState Create(Compilation compilation)
        {
            var sourcePaths = new List<string>();
            foreach (var tree in compilation.SyntaxTrees)
            {
                sourcePaths.Add(SourceGenUtilities.SourceTreeKey(tree));
            }
            return new ConfigFrameworkState(GetMissingRequiredSymbols(compilation), [.. sourcePaths]);
        }

        public Dictionary<string, int> CreateSourcePathOrder()
        {
            var len = SourcePaths.Length;
            var result = new Dictionary<string, int>(len, StringComparer.Ordinal);

            for (var i = 0; i < len; ++i)
            {
                var s = SourcePaths[i];
                if (!result.ContainsKey(s))
                {
                    result.Add(s, i);
                }
            }
            return result;
        }

        public bool Equals(ConfigFrameworkState? other)
        {
            if (other == null || MissingSymbols != other.MissingSymbols || SourcePaths.Length != other.SourcePaths.Length)
            {
                return false;
            }
            var len = SourcePaths.Length;
            for (var i = 0; i < len; ++i)
            {
                if (!StringComparer.Ordinal.Equals(SourcePaths[i], other.SourcePaths[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(object? obj) => obj is ConfigFrameworkState other && Equals(other);

        public override int GetHashCode()
        {
            var hash = MissingSymbols;
            var len = SourcePaths.Length;
            for (var i = 0; i < len; ++i)
            {
                hash = hash * 31 + StringComparer.Ordinal.GetHashCode(SourcePaths[i]);
            }
            return hash;
        }
    }

    private sealed class ConfigSnapshot : IEquatable<ConfigSnapshot>
    {
        private ConfigSnapshot(string typeName, string displayExpression, ConfigFieldSnapshot[] fields, ConfigError? typeError)
        {
            TypeName = typeName;
            DisplayExpression = displayExpression;
            Fields = fields;
            TypeError = typeError;
        }

        public readonly string TypeName;
        public readonly string DisplayExpression;
        public readonly ConfigFieldSnapshot[] Fields;
        public readonly ConfigError? TypeError;

        public static ConfigSnapshot Create(INamedTypeSymbol type)
        {
            var typeName = SourceGenUtilities.TypeName(type);
            ConfigError? typeError = null;
            if (!SourceGenUtilities.CanEmitClosedType(type))
            {
                typeError = new ConfigError(DiagnosticLocation.From(SourceGenUtilities.FirstSourceLocation(type)), "the type is inaccessible or contains unbound generic parameters");
            }

            var fields = new List<ConfigFieldSnapshot>();
            for (var current = type; current != null && !SourceGenUtilities.HasMetadataName(current, ConfigNodeMetadataName); current = current.BaseType)
            {
                var members = current.GetMembers();
                var len = members.Length;
                for (var i = 0; i < len; ++i)
                {
                    if (members[i] is not IFieldSymbol field || field.IsStatic || field.IsImplicitlyDeclared)
                    {
                        continue;
                    }
                    fields.Add(ConfigFieldSnapshot.Create(field));
                }
            }

            return new ConfigSnapshot(typeName, AttributeExpression(SourceGenUtilities.Attribute(type.GetAttributes(), ConfigDisplayMetadataName)), [.. fields], typeError);
        }

        public ConfigSnapshot WithFields(ConfigFieldSnapshot[] fields) => new(TypeName, DisplayExpression, fields, TypeError);

        public bool Equals(ConfigSnapshot? other)
        {
            if (other == null || TypeName != other.TypeName || DisplayExpression != other.DisplayExpression || !Equals(TypeError, other.TypeError) || Fields.Length != other.Fields.Length)
            {
                return false;
            }
            var len = Fields.Length;
            for (var i = 0; i < len; ++i)
            {
                if (!Fields[i].Equals(other.Fields[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(object? obj) => obj is ConfigSnapshot other && Equals(other);
        public override int GetHashCode()
        {
            var hash = StringComparer.Ordinal.GetHashCode(TypeName);
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(DisplayExpression);
            hash = hash * 31 + (TypeError?.GetHashCode() ?? 0);
            var len = Fields.Length;
            for (var i = 0; i < len; ++i)
            {
                hash = hash * 31 + Fields[i].GetHashCode();
            }
            return hash;
        }
    }

    private sealed class ConfigFieldSnapshot : IEquatable<ConfigFieldSnapshot>
    {
        private ConfigFieldSnapshot(string name, string escapedName, string typeName, bool serializable, string sectionStartExpression, string propertyDisplayExpression,
            string propertyComboExpression, string propertySliderExpression, string propertyStringOrderExpression, string groupDetailsExpression,
            string presetArrayExpression, bool radio, string sourceTreeKey, int sourceSpanStart, ConfigError? error)
        {
            Name = name;
            EscapedName = escapedName;
            TypeName = typeName;
            Serializable = serializable;
            SectionStartExpression = sectionStartExpression;
            PropertyDisplayExpression = propertyDisplayExpression;
            PropertyComboExpression = propertyComboExpression;
            PropertySliderExpression = propertySliderExpression;
            PropertyStringOrderExpression = propertyStringOrderExpression;
            GroupDetailsExpression = groupDetailsExpression;
            PresetArrayExpression = presetArrayExpression;
            Radio = radio;
            SourceTreeKey = sourceTreeKey;
            SourceSpanStart = sourceSpanStart;
            Error = error;
        }

        public readonly string Name;
        public readonly string EscapedName;
        public readonly string TypeName;
        public readonly bool Serializable;
        public readonly string SectionStartExpression;
        public readonly string PropertyDisplayExpression;
        public readonly string PropertyComboExpression;
        public readonly string PropertySliderExpression;
        public readonly string PropertyStringOrderExpression;
        public readonly string GroupDetailsExpression;
        public readonly string PresetArrayExpression;
        public readonly bool Radio;
        public readonly string SourceTreeKey;
        public readonly int SourceSpanStart;
        public readonly ConfigError? Error;

        public static ConfigFieldSnapshot Create(IFieldSymbol field)
        {
            var attributes = field.GetAttributes();
            ConfigError? error = null;
            if (!SourceGenUtilities.CanReference(field))
            {
                error = new ConfigError(DiagnosticLocation.From(SourceGenUtilities.FirstSourceLocation(field)), $"field '{field.Name}' is not accessible from generated code");
            }
            else if (!SourceGenUtilities.CanAssign(field))
            {
                error = new ConfigError(DiagnosticLocation.From(SourceGenUtilities.FirstSourceLocation(field)), $"field '{field.Name}' is readonly and cannot be deserialized without reflection");
            }

            var location = SourceGenUtilities.FirstSourceLocation(field);
            return new ConfigFieldSnapshot(
                field.Name,
                SourceGenUtilities.EscapeIdentifier(field.Name),
                SourceGenUtilities.TypeName(field.Type),
                SourceGenUtilities.Attribute(attributes, JsonIgnoreMetadataName) == null,
                AttributeExpression(SourceGenUtilities.Attribute(attributes, SectionStartMetadataName)),
                AttributeExpression(SourceGenUtilities.Attribute(attributes, PropertyDisplayMetadataName)),
                AttributeExpression(SourceGenUtilities.Attribute(attributes, PropertyComboMetadataName)),
                AttributeExpression(SourceGenUtilities.Attribute(attributes, PropertySliderMetadataName)),
                AttributeExpression(SourceGenUtilities.Attribute(attributes, PropertyStringOrderMetadataName)),
                AttributeExpression(SourceGenUtilities.Attribute(attributes, GroupDetailsMetadataName)),
                PresetArrayExpression(attributes),
                SourceGenUtilities.Attribute(attributes, PropertyRadioMetadataName) != null,
                location?.SourceTree is { } tree ? SourceGenUtilities.SourceTreeKey(tree) : string.Empty,
                location?.IsInSource == true ? location.SourceSpan.Start : int.MaxValue,
                error);
        }

        public bool Equals(ConfigFieldSnapshot? other)
            => other is not null && Name == other.Name && EscapedName == other.EscapedName && TypeName == other.TypeName && Serializable == other.Serializable
                && SectionStartExpression == other.SectionStartExpression && PropertyDisplayExpression == other.PropertyDisplayExpression && PropertyComboExpression == other.PropertyComboExpression
                && PropertySliderExpression == other.PropertySliderExpression && PropertyStringOrderExpression == other.PropertyStringOrderExpression
                && GroupDetailsExpression == other.GroupDetailsExpression && PresetArrayExpression == other.PresetArrayExpression && Radio == other.Radio
                && SourceTreeKey == other.SourceTreeKey && SourceSpanStart == other.SourceSpanStart && Equals(Error, other.Error);

        public override bool Equals(object? obj) => obj is ConfigFieldSnapshot other && Equals(other);
        public override int GetHashCode()
        {
            var hash = StringComparer.Ordinal.GetHashCode(Name);
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(TypeName);
            hash = hash * 31 + Serializable.GetHashCode();
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(SectionStartExpression);
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(PropertyDisplayExpression);
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(PropertyComboExpression);
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(PropertySliderExpression);
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(PropertyStringOrderExpression);
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(GroupDetailsExpression);
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(PresetArrayExpression);
            hash = hash * 31 + Radio.GetHashCode();
            hash = hash * 31 + StringComparer.Ordinal.GetHashCode(SourceTreeKey);
            hash = hash * 31 + SourceSpanStart;
            hash = hash * 31 + (Error?.GetHashCode() ?? 0);
            return hash;
        }
    }

    private sealed class ConfigError(DiagnosticLocation location, string reason) : IEquatable<ConfigError>
    {
        public readonly DiagnosticLocation Location = location;
        public readonly string Reason = reason;
        public bool Equals(ConfigError? other) => other is not null && Location.Equals(other.Location) && Reason == other.Reason;
        public override bool Equals(object? obj) => obj is ConfigError other && Equals(other);
        public override int GetHashCode() => Location.GetHashCode() * 31 + StringComparer.Ordinal.GetHashCode(Reason);
    }

    private readonly struct DiagnosticLocation : IEquatable<DiagnosticLocation>
    {
        private DiagnosticLocation(string path, TextSpan span, LinePositionSpan lineSpan, bool hasSource)
        {
            Path = path;
            Span = span;
            LineSpan = lineSpan;
            HasSource = hasSource;
        }
        private readonly string Path;
        private readonly TextSpan Span;
        private readonly LinePositionSpan LineSpan;
        private readonly bool HasSource;

        public static DiagnosticLocation From(Location? location)
        {
            if (location == null || !location.IsInSource)
            {
                return default;
            }
            var line = location.GetLineSpan();
            return new DiagnosticLocation(line.Path ?? string.Empty, location.SourceSpan, line.Span, true);
        }
        public readonly Location ToLocation() => HasSource ? Location.Create(Path, Span, LineSpan) : Location.None;
        public readonly bool Equals(DiagnosticLocation other) => HasSource == other.HasSource && Path == other.Path && Span.Equals(other.Span) && LineSpan.Equals(other.LineSpan);
        public override readonly bool Equals(object? obj) => obj is DiagnosticLocation other && Equals(other);
        public override readonly int GetHashCode() => (((HasSource ? 1 : 0) * 31 + StringComparer.Ordinal.GetHashCode(Path ?? string.Empty)) * 31 + Span.GetHashCode()) * 31 + LineSpan.GetHashCode();
    }
}
