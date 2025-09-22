namespace BossMod;

[SkipLocalsInit]
public readonly struct Edge(float ax, float ay, float dx, float dy)
{
    private const float Epsilon = 1e-8f;

    public readonly float Ax = ax, Ay = ay, Dx = dx, Dy = dy, InvLengthSq = 1f / (dx * dx + dy * dy + Epsilon);
}

[SkipLocalsInit]
public sealed class SpatialIndex
{
    private int[][] _grid = [];
    private readonly Edge[] _edges;
    private readonly int _minX, _minY, _gridWidth, _gridHeight;
    private const float InvGridSize = 1f / 5f;

    public SpatialIndex(Edge[] edges)
    {
        _edges = edges;
        ComputeGridBounds(out _minX, out _minY, out _gridWidth, out _gridHeight);
        BuildIndex();
    }

    private void ComputeGridBounds(out int minX, out int minY, out int gridWidth, out int gridHeight)
    {
        minX = minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;
        var len = _edges.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var edge = ref _edges[i];
            var ax = edge.Ax;
            var ay = edge.Ay;
            var bx = ax + edge.Dx;
            var by = ay + edge.Dy;

            var ex0 = (int)MathF.Floor(Math.Min(ax, bx) * InvGridSize);
            var ex1 = (int)MathF.Floor(Math.Max(ax, bx) * InvGridSize);
            var ey0 = (int)MathF.Floor(Math.Min(ay, by) * InvGridSize);
            var ey1 = (int)MathF.Floor(Math.Max(ay, by) * InvGridSize);

            minX = Math.Min(minX, ex0);
            minY = Math.Min(minY, ey0);
            maxX = Math.Max(maxX, ex1);
            maxY = Math.Max(maxY, ey1);
        }

        gridWidth = maxX - minX + 1;
        gridHeight = maxY - minY + 1;
    }

    private void BuildIndex()
    {
        var cellCount = _gridWidth * _gridHeight;
        var grid = new List<int>[cellCount];
        for (var i = 0; i < cellCount; ++i)
        {
            grid[i] = [];
        }
        var len = _edges.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var edge = ref _edges[i];
            var edgeAx = edge.Ax;
            var edgeAy = edge.Ay;
            var edgeAxDx = edgeAx + edge.Dx;
            var edgeAydy = edgeAy + edge.Dy;
            var minX = Math.Min(edgeAx, edgeAxDx);
            var maxX = Math.Max(edgeAx, edgeAxDx);
            var minY = Math.Min(edgeAy, edgeAydy);
            var maxY = Math.Max(edgeAy, edgeAydy);

            var x0 = (int)MathF.Floor(minX * InvGridSize) - _minX;
            var x1 = (int)MathF.Floor(maxX * InvGridSize) - _minX;
            var y0 = (int)MathF.Floor(minY * InvGridSize) - _minY;
            var y1 = (int)MathF.Floor(maxY * InvGridSize) - _minY;

            for (var y = y0; y <= y1; ++y)
            {
                var rowIndex = y * _gridWidth;
                for (var x = x0; x <= x1; ++x)
                {
                    grid[rowIndex + x].Add(i);
                }
            }
        }

        _grid = new int[cellCount][];
        for (var i = 0; i < cellCount; ++i)
        {
            _grid[i] = [.. grid[i]];
        }
    }

    public int[] Query(float px, float py)
    {
        var cellX = (int)MathF.Floor(px * InvGridSize) - _minX;
        var cellY = (int)MathF.Floor(py * InvGridSize) - _minY;

        return (uint)cellX >= _gridWidth || (uint)cellY >= _gridHeight ? [] : _grid[cellY * _gridWidth + cellX];
    }
}
