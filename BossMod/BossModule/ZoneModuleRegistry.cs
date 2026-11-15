namespace BossMod;

// attribute for defining zone module's metadata; it is required by each module to be loaded
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ZoneModuleInfoAttribute(BossModuleInfo.Maturity maturity, uint cfcId, uint territoryID = 0) : Attribute
{
    public BossModuleInfo.Maturity Maturity => maturity;
    public uint CFCID => cfcId;
    public uint TerritoryID => territoryID;
}

public static class ZoneModuleRegistry
{
    public record class Info(Type ModuleType, ZoneModuleInfoAttribute Desc, Func<WorldState, ZoneModule> Factory);

    private static readonly Dictionary<uint, Info> _modulesByCFC = [];

    static ZoneModuleRegistry() => GeneratedRegistries.RegisterZoneModules(Register);

    private static void Register(Info info)
    {
        if (_modulesByCFC.TryGetValue(info.Desc.CFCID, out var existingModule))
        {
            Service.Log($"[ZoneModuleRegistry] Two zone modules have same CFCID: {info.ModuleType.FullName} and {existingModule.ModuleType.FullName}");
            return;
        }

        _modulesByCFC[info.Desc.CFCID] = info;
    }

    public static IEnumerable<(uint CFCID, BossModuleInfo.Maturity Maturity)> RegisteredCFCs()
    {
        foreach (var (cfcId, info) in _modulesByCFC)
        {
            if (cfcId != 0)
            {
                yield return (cfcId, info.Desc.Maturity);
            }
        }
    }

    public static ZoneModule? CreateModule(WorldState ws, uint cfcId, BossModuleInfo.Maturity minMaturity) => cfcId != 0 && _modulesByCFC.TryGetValue(cfcId, out var info) && info.Desc.Maturity >= minMaturity ? info.Factory(ws) : null;
}
