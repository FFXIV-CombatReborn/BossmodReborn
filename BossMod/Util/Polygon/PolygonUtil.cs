namespace BossMod;

[SkipLocalsInit]
public static class PolygonUtil
{
    public static ReadOnlySpan<(WDir, WDir)> EnumerateEdges(ReadOnlySpan<WDir> contour)
    {
        var count = contour.Length;
        if (count == 0)
        {
            return [];
        }

        var result = new (WDir, WDir)[count];
        var prev = contour[count - 1];

        for (var i = 0; i < count; ++i)
        {
            var contourI = contour[i];
            result[i] = (prev, contourI);
            prev = contourI;
        }
        return result;
    }
}
