namespace BossMod;

// clipping shapes to bounds and triangulating them is a serious time sink, so we want to cache that
// to avoid requiring tracking cache lifetime by users, we use a heuristic - we assume that if something isn't drawn for a frame, it's no longer relevant
[SkipLocalsInit]
public sealed class TriangulationCache
{
    private class CacheEntry(List<RelTriangle>? triangulation)
    {
        public List<RelTriangle>? Triangulation = triangulation;
        public bool IsCurrent = true;
    }

    private readonly Dictionary<int, CacheEntry> _cache = [];

    public static int GetKeyHash(int keyType, params object[] keyParts)
    {
        var hash = keyType;
        var len = keyParts.Length;
        for (var i = 0; i < len; ++i)
        {
            hash = hash * 31 + keyParts[i].GetHashCode();
        }
        return hash;
    }

    public ref List<RelTriangle>? this[int keyType, params object[] keyParts]
    {
        get
        {
            var keyHash = GetKeyHash(keyType, keyParts);
            if (!_cache.TryGetValue(keyHash, out var entry))
            {
                entry = new CacheEntry(null);
                _cache[keyHash] = entry;
                return ref entry.Triangulation;
            }

            entry.IsCurrent = true;
            return ref entry.Triangulation;
        }
    }

    public void NextFrame()
    {
        var keysToRemove = new List<int>(_cache.Count);

        foreach (var kvp in _cache)
        {
            if (!kvp.Value.IsCurrent)
                keysToRemove.Add(kvp.Key);
            else
                kvp.Value.IsCurrent = false;
        }
        var count = keysToRemove.Count;
        for (var i = 0; i < count; ++i)
            _cache.Remove(keysToRemove[i]);
    }

    public void Invalidate()
    {
        _cache.Clear();
    }
}
