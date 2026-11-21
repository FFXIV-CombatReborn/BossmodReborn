namespace BossMod.ReplayAnalysis;

class CommonEnumInfo
{
    protected Type? _oidType;

    protected string JoinStrings(IEnumerable<string> strings)
    {
        var s = string.Join('/', strings);
        return s.Length > 0 ? s : "none";
    }

    protected string OIDString(uint oid) => oid == 0 ? "player" : _oidType?.GeneratedEnumName(oid) ?? $"{oid:X}";

    protected string OIDListString(IEnumerable<uint> oids)
    {
        var sb = new StringBuilder();
        foreach (var oid in oids)
        {
            if (sb.Length > 0)
            {
                sb.Append('/');
            }
            sb.Append(OIDString(oid));
        }
        return sb.Length > 0 ? sb.ToString() : "none";
    }
}
