namespace BossMod;

// special distance function for precise positioning, finer than map resolution
// it's an inverted rect of a size equal to one grid cell, with a special adjustment if starting position is in the same cell, but farther than tolerance

[SkipLocalsInit]
public sealed class SDPrecisePosition : ShapeDistance
{
    private readonly float originX, originZ, dirX, dirZ, normalX, normalZ, cellSize;

    public SDPrecisePosition(WPos Origin, WDir Direction, float CellSize, WPos Starting, float Tolerance = default)
    {
        var starting = Starting;
        var origin = Origin;
        cellSize = CellSize;
        var dir = Direction;
        var tolerance = Tolerance;
        var delta = starting - origin;
        var dparr = delta.Dot(dir);
        if (dparr > tolerance && dparr <= cellSize)
        {
            origin -= cellSize * dir;
        }
        else if (dparr < -tolerance && dparr >= -cellSize)
        {
            origin += cellSize * dir;
        }

        var normal = dir.OrthoL();
        var dortho = delta.Dot(normal);
        if (dortho > tolerance && dortho <= cellSize)
        {
            origin -= cellSize * normal;
        }
        else if (dortho < -tolerance && dortho >= -cellSize)
        {
            origin += cellSize * normal;
        }
        originX = origin.X;
        originZ = origin.Z;
        dirX = dir.X;
        dirZ = dir.Z;
        normalX = normal.X;
        normalZ = normal.Z;
    }

    public override float Distance(WPos p)
    {
        var pXoriginX = p.X - originX;
        var pZoriginZ = p.Z - originZ;
        var distParr = pXoriginX * dirX + pZoriginZ * dirZ;
        var distOrtho = pXoriginX * normalX + pZoriginZ * normalZ;
        var distFront = distParr - cellSize;
        var distBack = -distParr - cellSize;
        var distLeft = distOrtho - cellSize;
        var distRight = -distOrtho - cellSize;

        var maxParr = distFront > distBack ? distFront : distBack;
        var maxOrtho = distLeft > distRight ? distLeft : distRight;

        return maxParr > maxOrtho ? maxParr : maxOrtho;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}
