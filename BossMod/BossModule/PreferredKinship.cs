using System.Reflection;
using System.Text.Json;

namespace BossMod;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class PreferredKinshipAttribute(BST.Kinship kinship, string reason) : Attribute
{
    public BST.Kinship Kinship => kinship;
    public string Reason => reason;
}

public static class CruciblePreferredKinships
{
    public readonly record struct Preference(int Kinship, string Reason);
    public sealed record class Entry(string Board, int BoardOrder, uint GroupID, string Opponent, int SortOrder, string ModuleType, Preference[] Kinships);

    private static List<Entry>? _entries;
    private static string? _json;

    public static IReadOnlyList<Entry> Entries => _entries ??= Build();

    public static string Json() => _json ??= JsonSerializer.Serialize(Entries);

    public static Preference[] ForModule(Type moduleType)
    {
        var attrs = moduleType.GetCustomAttributes<PreferredKinshipAttribute>(false);
        var result = new List<Preference>();
        foreach (var a in attrs)
        {
            result.Add(new((int)a.Kinship, a.Reason));
        }
        return [.. result];
    }

    private static List<Entry> Build()
    {
        var entries = new List<Entry>();
        foreach (var info in BossModuleRegistry.RegisteredModules.Values)
        {
            if (info.GroupType != BossModuleInfo.GroupType.CrucibleOfTheUnbroken)
            {
                continue;
            }

            var (board, boardOrder) = BoardName(info.GroupID);
            var opponent = info.NameID != 0 ? ModuleViewer.BNpcName(info.NameID) : info.ModuleType.Name;
            entries.Add(new(board, boardOrder, info.GroupID, opponent, info.SortOrder, info.ModuleType.FullName ?? info.ModuleType.Name, ForModule(info.ModuleType)));
        }

        entries.Sort(static (a, b) => a.BoardOrder != b.BoardOrder ? a.BoardOrder.CompareTo(b.BoardOrder) : a.SortOrder.CompareTo(b.SortOrder));
        return entries;
    }

    private static (string name, int order) BoardName(uint cfcId)
    {
        if (Service.LuminaRow<Lumina.Excel.Sheets.ContentFinderCondition>(cfcId) is not { } row)
        {
            return ($"Board {cfcId}", (int)cfcId);
        }

        var name = row.Name.ToString();
        const string suffix = " of the Unbroken";
        if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            name = name[..^suffix.Length];
        }

        var shortCode = row.ShortCode.ToString();
        var order = shortCode.Length > 3 && int.TryParse(shortCode.AsSpan(3), out var parsed) ? parsed : (int)cfcId;
        return (name, order);
    }
}
