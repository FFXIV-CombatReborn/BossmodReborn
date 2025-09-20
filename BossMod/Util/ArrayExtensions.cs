namespace BossMod;

[SkipLocalsInit]
public static class ArrayExtensions
{
    public static T[] ReverseArray<T>(this T[] source)
    {
        var copy = (T[])source.Clone();
        Array.Reverse(copy);
        return copy;
    }
}
