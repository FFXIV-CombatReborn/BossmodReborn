namespace BossMod.ReplayAnalysis;

sealed class DirectorInfo
{
    private readonly Dictionary<uint, List<(Replay r, Replay.Encounter enc, uint[] prms, DateTime ts)>> _data = [];

    public DirectorInfo(List<Replay> replays, uint oid)
    {
        var moduleInfo = BossModuleRegistry.FindByOID(oid);
        foreach (var replay in replays)
        {
            for (var i = 0; i < replay.Encounters.Count; ++i)
            {
                var enc = replay.Encounters[i];
                if (enc.OID != oid)
                {
                    continue;
                }
                foreach (var diru in replay.EncounterDirectorUpdates(enc))
                {
                    _data.GetOrAdd(diru.UpdateID).Add((replay, enc, [diru.Param1, diru.Param2, diru.Param3, diru.Param4], diru.Timestamp));
                }
            }
        }
    }

    public void Draw(UITree tree)
    {
        foreach (var ni in tree.Nodes(_data, kv => new(kv.Key.ToString("X8"))))
        {
            tree.LeafNodes(ni.Value, t => $"[{string.Join(", ", t.prms)}] {t.r.Path} @ {t.enc.Time.Start:O}+{(t.ts - t.enc.Time.Start).TotalSeconds:f4}");
        }
    }
}
