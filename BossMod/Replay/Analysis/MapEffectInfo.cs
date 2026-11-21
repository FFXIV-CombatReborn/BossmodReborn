namespace BossMod.ReplayAnalysis;

sealed class MapEffectInfo
{
    private readonly Dictionary<byte, Dictionary<uint, List<(Replay r, Replay.Encounter enc, DateTime ts)>>> _data = [];

    public MapEffectInfo(List<Replay> replays, uint oid)
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
                foreach (var envc in replay.EncounterMapEffects(enc))
                {
                    _data.GetOrAdd(envc.Index).GetOrAdd(envc.State).Add((replay, enc, envc.Timestamp));
                }
            }
        }
    }

    public void Draw(UITree tree)
    {
        foreach (var ni in tree.Nodes(_data, kv => new(kv.Key.ToString("X2"))))
        {
            foreach (var ns in tree.Nodes(ni.Value, kv => new(kv.Key.ToString("X8"))))
            {
                tree.LeafNodes(ns.Value, t => $"{t.r.Path} @ {t.enc.Time.Start:O}+{(t.ts - t.enc.Time.Start).TotalSeconds:f4}");
            }
        }
    }
}
