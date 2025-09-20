namespace BossMod.Pathfinding;

// 'map' used for running pathfinding algorithms
// this is essentially a square grid representing an arena (or immediate neighbourhood of the player) where we rasterize forbidden/desired zones
// area covered by each pixel can be in one of the following states:
// - default: safe to traverse but non-goal
// - danger: unsafe to traverse after X seconds (X >= 0); instead of X, we store max 'g' value (distance travelled assuming constant speed) for which pixel is still considered unblocked
// - goal: destination with X priority (X > 0); 'default' is considered a goal with priority 0
// - goal and danger are mutually exclusive, 'danger' overriding 'goal' state
// typically we try to find a path to goal with highest priority; if that fails, try lower priorities; if no paths can be found (e.g. we're currently inside an imminent aoe) we find direct path to closest safe pixel

[SkipLocalsInit]
public sealed class Map
{
    public float Resolution; // pixel size, in world units
    public int Width; // always even
    public int Height; // always even
    public float[] PixelMaxG = []; // == MaxValue if not dangerous (TODO: consider changing to a byte per pixel?), < 0 if impassable
    public float[] PixelPriority = [];

    public WPos Center; // position of map center in world units
    public Angle Rotation; // rotation relative to world space (=> ToDirection() is equal to direction of local 'height' axis in world space)
    public WDir LocalZDivRes;

    public float MaxG; // maximal 'maxG' value of all blocked pixels
    public float MaxPriority; // maximal 'priority' value of all goal pixels

    // min-max bounds of 'interesting' area, default to (0,0) to (width-1,height-1)
    public int MinX;
    public int MinY;
    public int MaxX;
    public int MaxY;

    public Map() { }
    public Map(float resolution, WPos center, float worldHalfWidth, float worldHalfHeight, Angle rotation = default) => Init(resolution, center, worldHalfWidth, worldHalfHeight, rotation);

    public void Init(float resolution, WPos center, float worldHalfWidth, float worldHalfHeight, Angle rotation = default)
    {
        Resolution = resolution;
        Width = 2 * (int)MathF.Ceiling(worldHalfWidth / resolution);
        Height = 2 * (int)MathF.Ceiling(worldHalfHeight / resolution);

        var numPixels = Width * Height;
        if (PixelMaxG.Length < numPixels)
        {
            PixelMaxG = new float[numPixels];
        }
        new Span<float>(PixelMaxG, 0, numPixels).Fill(float.MaxValue); // fill is unconditional, can we avoid it by changing storage?..
        if (PixelPriority.Length < numPixels)
        {
            PixelPriority = new float[numPixels];
        }
        else
        {
            new Span<float>(PixelPriority, 0, numPixels).Clear();
        }

        Center = center;
        Rotation = rotation;
        LocalZDivRes = rotation.ToDirection() / Resolution;

        MaxG = 0f;
        MaxPriority = 0f;

        MinX = MinY = 0;
        MaxX = Width - 1;
        MaxY = Height - 1;
    }

    public void Init(Map source, WPos center)
    {
        Resolution = source.Resolution;
        Width = source.Width;
        Height = source.Height;

        var numPixels = Width * Height;
        if (PixelMaxG.Length < numPixels)
            PixelMaxG = new float[numPixels];
        Array.Copy(source.PixelMaxG, PixelMaxG, numPixels);
        if (PixelPriority.Length < numPixels)
            PixelPriority = new float[numPixels];
        Array.Copy(source.PixelPriority, PixelPriority, numPixels);

        Center = center;
        Rotation = source.Rotation;
        LocalZDivRes = source.LocalZDivRes;

        MaxG = source.MaxG;
        MaxPriority = source.MaxPriority;

        MinX = source.MinX;
        MinY = source.MinY;
        MaxX = source.MaxX;
        MaxY = source.MaxY;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 WorldToGridFrac(WPos world)
    {
        var offset = world - Center;
        var x = offset.Dot(LocalZDivRes.OrthoL());
        var y = offset.Dot(LocalZDivRes);
        return new((Width >> 1) + x, (Height >> 1) + y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GridToIndex(int x, int y) => y * Width + x;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GridToIndex((int x, int y) p) => GridToIndex(p.x, p.y);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int x, int y) IndexToGrid(int index) => (index % Width, index / Width);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int x, int y) FracToGrid(Vector2 frac) => ((int)MathF.Floor(frac.X), (int)MathF.Floor(frac.Y));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int x, int y) WorldToGrid(WPos world) => FracToGrid(WorldToGridFrac(world));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int x, int y) ClampToGrid((int x, int y) pos) => (Math.Clamp(pos.x, 0, Width - 1), Math.Clamp(pos.y, 0, Height - 1));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool InBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public WPos GridToWorld(int gx, int gy, float fx, float fy)
    {
        var rsq = Resolution * Resolution; // since we then multiply by _localZDivRes, end result is same as * res * rotation.ToDir()
        var ax = (gx - (Width >> 1) + fx) * rsq;
        var az = (gy - (Height >> 1) + fy) * rsq;
        return Center + ax * LocalZDivRes.OrthoL() + az * LocalZDivRes;
    }

    // block all pixels for which function returns value smaller than threshold ('inside' shape + extra cushion)
    public void BlockPixelsInside(ShapeDistance shape, float maxG, float threshold)
    {
        MaxG = Math.Max(MaxG, maxG);
        var width = Width;
        var height = Height;
        var resolution = Resolution;
        var dir = Rotation.ToDirection();
        var dx = dir.OrthoL() * resolution;
        var dy = dir * resolution;
        var startPos = Center - ((width >> 1) - 0.5f) * dx - ((height >> 1) - 0.5f) * dy;
        var maxG_ = maxG;
        var threshold_ = threshold;
        var shape_ = shape;

        for (var y = 0; y < height; ++y)
        {
            var posY = startPos + y * dy;
            var rowBaseIndex = y * width;
            for (var x = 0; x < width; ++x)
            {
                var pos = posY + x * dx;
                if (shape_.Distance(pos) <= threshold_)
                {
                    PixelMaxG[rowBaseIndex + x] = maxG_;
                }
            }
        }
    }

    public (int x, int y, WPos center)[] EnumeratePixels()
    {
        var width = Width;
        var height = Height;
        var result = new (int x, int y, WPos center)[(width * height)];
        var rsq = Resolution * Resolution; // since we then multiply by _localZDivRes, end result is same as * res * rotation.ToDir()
        var dx = LocalZDivRes.OrthoL() * rsq;
        var dy = LocalZDivRes * rsq;
        var cy = Center + (-(width >> 1) + 0.5f) * dx + (-(height >> 1) + 0.5f) * dy;
        var index = 0;
        for (var y = 0; y < height; ++y)
        {
            var cx = cy;
            for (var x = 0; x < width; ++x)
            {
                result[index++] = (x, y, cx);
                cx += dx;
            }
            cy += dy;
        }
        return result;
    }

    // enumerate pixels along line starting from (x1, y1) to (x2, y2); first is not returned, last is returned
    public (int x, int y)[] EnumeratePixelsInLine(int x1, int y1, int x2, int y2)
    {
        var absDx = Math.Abs(x2 - x1);
        var absDy = Math.Abs(y2 - y1);
        var estimatedLength = Math.Max(absDx, absDy);

        var result = new (int x, int y)[estimatedLength];

        int dx = absDx, sx = x1 < x2 ? 1 : -1;
        int dy = -absDy, sy = y1 < y2 ? 1 : -1;
        int err = dx + dy, e2;

        for (var i = 0; i < estimatedLength; ++i)
        {
            e2 = 2 * err;
            if (e2 >= dy)
            {
                err += dy;
                x1 += sx;
            }
            if (e2 <= dx)
            {
                err += dx;
                y1 += sy;
            }

            result[i] = (x1, y1);
        }

        return result;
    }
}
