namespace BossMod;

// special distance function for precise positioning, finer than map resolution
// it's an inverted rect of a size equal to one grid cell, with a special adjustment if starting position is in the same cell, but farther than tolerance

[SkipLocalsInit]
public sealed class SDPrecisePosition : ShapeDistance
{
    private readonly float originX, originZ, dirX, dirZ, normalX, normalZ, cellSize;

    public SDPrecisePosition(WPos Origin, WDir Direction, float CellSize, WPos Starting, float Tolerance = default)
    {
        // Snap origin like your PrecisePosition(...)
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

    public override float Distance(in WPos p)
    {
        var px = p.X - originX;
        var pz = p.Z - originZ;

        var distParr = px * dirX + pz * dirZ;
        var distOrtho = px * normalX + pz * normalZ;

        var distFront = distParr - cellSize;
        var distBack = -distParr - cellSize;
        var distLeft = distOrtho - cellSize;
        var distRight = -distOrtho - cellSize;

        var maxParr = distFront > distBack ? distFront : distBack;
        var maxOrtho = distLeft > distRight ? distLeft : distRight;

        var regular = maxParr > maxOrtho ? maxParr : maxOrtho;

        return -regular;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var px = p.X - originX;
        var pz = p.Z - originZ;

        var parr = px * dirX + pz * dirZ;

        if (parr > cellSize || parr < -cellSize)
        {
            return true;
        }
        var ortho = px * normalX + pz * normalZ;
        return ortho > cellSize || ortho < -cellSize;
    }

    // always true since it is a tiny rect
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}
