namespace BossMod;

// 64-bit mask; out-of-range accesses are safe and well-defined ('get' always returns 0, 'set' is no-op)
// this is often used e.g. to store per-player flags (in such case only 8 lowest bits are used for 'normal' raids, or 24 lowest for full alliances)
[SkipLocalsInit]
public struct BitMask(ulong raw)
{
    public bool this[int index]
    {
#pragma warning disable IDE0251 // for whatever reason, marking getter as readonly triggers CS0649 when bitmask is modified by a setter
        get => (Raw & MaskForBit(index)) != 0;
#pragma warning restore IDE0251
        set
        {
            if (value)
                Set(index);
            else
                Clear(index);
        }
    }

    public ulong Raw = raw;
    public void Reset() => Raw = default;
    public void Set(int index) => Raw |= MaskForBit(index);
    public void Clear(int index) => Raw &= ~MaskForBit(index);
    public void Toggle(int index) => Raw ^= MaskForBit(index);

    public readonly bool Any() => Raw != default;
    public readonly bool None() => Raw == default;
    public readonly int NumSetBits() => BitOperations.PopCount(Raw);
    public readonly int LowestSetBit() => BitOperations.TrailingZeroCount(Raw); // returns out-of-range value (64) if no bits are set
    public readonly int HighestSetBit() => 63 - BitOperations.LeadingZeroCount(Raw); // returns out-of-range value (-1) if no bits are set

    public readonly BitMask WithBit(int index) => new(Raw | MaskForBit(index));
    public readonly BitMask WithoutBit(int index) => new(Raw & ~MaskForBit(index));

    public static bool operator !=(BitMask a, BitMask b) => a.Raw != b.Raw;
    public static bool operator ==(BitMask a, BitMask b) => a.Raw == b.Raw;
    public static BitMask operator ~(BitMask a) => new(~a.Raw);
    public static BitMask operator &(BitMask a, BitMask b) => new(a.Raw & b.Raw);
    public static BitMask operator |(BitMask a, BitMask b) => new(a.Raw | b.Raw);
    public static BitMask operator ^(BitMask a, BitMask b) => new(a.Raw ^ b.Raw);
    public static BitMask operator <<(BitMask a, int bits) => new(a.Raw << bits);
    public static BitMask operator >>(BitMask a, int bits) => new(a.Raw >> bits);

    public static BitMask Build<A>(params A[] bits) where A : Enum
    {
        BitMask res = default;
        foreach (var bit in bits)
            res.Raw |= MaskForBit((byte)(object)bit);
        return res;
    }

    public static BitMask Build(params int[] bits)
    {
        BitMask res = default;
        foreach (var bit in bits)
            res.Raw |= MaskForBit(bit);
        return res;
    }

    public readonly int[] SetBits()
    {
        var v = Raw;
        var count = BitOperations.PopCount(v);
        if (count == 0)
            return [];

        var indices = new int[count];
        var index = 0;

        while (v != 0)
        {
            var bitIndex = BitOperations.TrailingZeroCount(v);
            indices[index++] = bitIndex;
            v &= ~(1ul << bitIndex);
        }

        return indices;
    }

    private static ulong MaskForBit(int index) => (uint)index < 64u ? (1ul << index) : default;

    public readonly bool Equals(BitMask other) => Raw == other.Raw;
    public override readonly bool Equals(object? obj) => obj is BitMask other && Equals(other);
    public override readonly int GetHashCode() => Raw.GetHashCode();

    public override readonly string ToString() => $"{Raw:X}";
}
