namespace BossMod;

public static class ArrayExtensions
{
    public static T[] ReverseArray<T>(this T[] source)
    {
        var copy = (T[])source.Clone();
        Array.Reverse(copy);
        return copy;
    }

    public static T[] Single<T>(this T[] buffer, T value)
    {
        buffer[0] = value;
        return buffer;
    }
}
