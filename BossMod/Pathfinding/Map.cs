﻿using System.Runtime.InteropServices;

namespace BossMod.Pathfinding;

// 'map' used for running pathfinding algorithms
// this is essentially a square grid representing an arena (or immediate neighbourhood of the player) where we rasterize forbidden/desired zones
// area covered by each pixel can be in one of the following states:
// - default: safe to traverse but non-goal
// - danger: unsafe to traverse after X seconds (X >= 0); instead of X, we store max 'g' value (distance travelled assuming constant speed) for which pixel is still considered unblocked
// - goal: destination with X priority (X > 0); 'default' is considered a goal with priority 0
// - goal and danger are mutually exclusive, 'danger' overriding 'goal' state
// typically we try to find a path to goal with highest priority; if that fails, try lower priorities; if no paths can be found (e.g. we're currently inside an imminent aoe) we find direct path to closest safe pixel
public class Map
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Pixel
    {
        public float MaxG; // MaxValue if not dangerous
        public int Priority; // >0 if goal
    }

    public float Resolution; // pixel size, in world units
    public int Width; // always even
    public int Height; // always even
    public Pixel[] Pixels = [];
    private const float Epsilon = 1e-5f;
    private const float HalfPixel = 0.5f;

    public WPos Center; // position of map center in world units
    public Angle Rotation; // rotation relative to world space (=> ToDirection() is equal to direction of local 'height' axis in world space)
    private WDir LocalZDivRes;

    public float MaxG; // maximal 'maxG' value of all blocked pixels
    public int MaxPriority; // maximal 'priority' value of all goal pixels

    //public float Speed = 6; // used for converting activation time into max g-value: num world units that player can move per second

    public Pixel this[int x, int y] => InBounds(x, y) ? Pixels[y * Width + x] : new() { MaxG = float.MaxValue, Priority = 0 };

    public Map() { }
    public Map(float resolution, WPos center, float worldHalfWidth, float worldHalfHeight, Angle rotation = new()) => Init(resolution, center, worldHalfWidth, worldHalfHeight, rotation);

    public void Init(float resolution, WPos center, float worldHalfWidth, float worldHalfHeight, Angle rotation = new())
    {
        Resolution = resolution;
        var res = 1 / resolution;
        Width = 2 * (int)MathF.Ceiling(worldHalfWidth * res);
        Height = 2 * (int)MathF.Ceiling(worldHalfHeight * res);

        var numPixels = Width * Height;
        if (Pixels.Length < numPixels)
            Pixels = new Pixel[numPixels];
        Array.Fill(Pixels, new Pixel { MaxG = float.MaxValue, Priority = 0 }, 0, numPixels);

        Center = center;
        Rotation = rotation;
        LocalZDivRes = rotation.ToDirection() * res;

        MaxG = 0;
        MaxPriority = 0;
    }

    public void Init(Map source, WPos center)
    {
        Resolution = source.Resolution;
        Width = source.Width;
        Height = source.Height;

        var numPixels = Width * Height;
        if (Pixels.Length < numPixels)
            Pixels = new Pixel[numPixels];
        Array.Copy(source.Pixels, Pixels, numPixels);

        Center = center;
        Rotation = source.Rotation;
        LocalZDivRes = source.LocalZDivRes;

        MaxG = source.MaxG;
        MaxPriority = source.MaxPriority;
    }

    public Vector2 WorldToGridFrac(WPos world)
    {
        var offset = world - Center;
        var x = offset.Dot(LocalZDivRes.OrthoL());
        var y = offset.Dot(LocalZDivRes);
        return new(Width * HalfPixel + x, Height * HalfPixel + y);
    }

    public (int x, int y) FracToGrid(Vector2 frac) => ((int)MathF.Floor(frac.X), (int)MathF.Floor(frac.Y));
    public (int x, int y) WorldToGrid(WPos world) => FracToGrid(WorldToGridFrac(world));
    public (int x, int y) ClampToGrid((int x, int y) pos) => (Math.Clamp(pos.x, 0, Width - 1), Math.Clamp(pos.y, 0, Height - 1));
    public bool InBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    public WPos GridToWorld(int gx, int gy, float fx, float fy)
    {
        var rsq = Resolution * Resolution; // since we then multiply by _localZDivRes, end result is same as * res * rotation.ToDir()
        var ax = (gx - Width * HalfPixel + fx) * rsq;
        var az = (gy - Height * HalfPixel + fy) * rsq;
        return Center + ax * LocalZDivRes.OrthoL() + az * LocalZDivRes;
    }

    // block all pixels for which function returns value smaller than threshold ('inside' shape + extra cushion)
    public void BlockPixelsInside(Func<WPos, float> shape, float maxG, float threshold)
    {
        MaxG = Math.Max(MaxG, maxG);
        ref var pixels = ref Pixels;
        var width = Width;
        var height = Height;
        var resolution = Resolution;
        var dir = Rotation.ToDirection();
        var dx = dir.OrthoL() * resolution;
        var dy = dir * resolution;
        var startPos = Center - (width * HalfPixel - HalfPixel) * dx - (height * HalfPixel - HalfPixel) * dy;

        Parallel.For(0, height, y =>
        {
            var posY = startPos + y * dy;
            var rowBaseIndex = y * width;
            for (var x = 0; x < width; ++x)
            {
                ref var pixel = ref Pixels[rowBaseIndex + x];
                if (pixel.MaxG == -1) // pixel is outside of walkable arena bounds, no need to test or modify
                    continue;
                var pos = posY + x * dx;
                if (shape(pos) <= threshold)
                {
                    ref var pixelMaxG = ref pixel.MaxG;
                    pixelMaxG = pixelMaxG < maxG ? pixelMaxG : maxG;
                }
            }
        });
    }

    private static readonly (float, float)[] offsets =
            [
                (Epsilon, Epsilon),
                (Epsilon, 1 - Epsilon),
                (1 - Epsilon, Epsilon),
                (1 - Epsilon, 1 - Epsilon)
            ];

    // for testing 4 points per pixel for increased accuracy to rasterize circle and rectangle arena bounds
    public void BlockPixelsInside2(Func<WPos, float> shape, float maxG)
    {
        MaxG = Math.Max(MaxG, maxG);
        var width = Width;
        var height = Height;
        ref var pixels = ref Pixels;
        var resolution = Resolution;
        var dir = Rotation.ToDirection();
        var dx = dir.OrthoL() * resolution;
        var dy = dir * resolution;
        var startPos = Center - width * HalfPixel * dx - height * HalfPixel * dy;

        Parallel.For(0, height, y =>
        {
            var posY = startPos + y * dy;
            var rowBaseIndex = y * width;

            for (var x = 0; x < width; ++x)
            {
                var posBase = posY + x * dx;
                var isBlocked = false;
                for (var i = 0; i < 4; ++i)
                {
                    ref var points = ref offsets[i];
                    var pos = posBase + points.Item1 * dx + points.Item2 * dy;

                    if (shape(pos) <= 0)
                    {
                        isBlocked = true;
                        break;
                    }
                }
                if (isBlocked)
                {
                    ref var pixelMaxG = ref Pixels[rowBaseIndex + x].MaxG;
                    pixelMaxG = pixelMaxG < maxG ? pixelMaxG : maxG;
                }
            }
        });
    }

    public int AddGoal(int x, int y, int deltaPriority)
    {
        ref var pixel = ref Pixels[y * Width + x];
        pixel.Priority += deltaPriority;
        MaxPriority = Math.Max(MaxPriority, pixel.Priority);
        return pixel.Priority;
    }

    public int AddGoal(Func<WPos, float> shape, float threshold, int minPriority, int deltaPriority)
    {
        var maxAdjustedPriority = minPriority;
        Parallel.For(0, Height, y =>
        {
            var rowBaseIndex = y * Width;
            for (var x = 0; x < Width; ++x)
            {
                var i = rowBaseIndex + x;
                if (shape(GridToWorld(x, y, HalfPixel, HalfPixel)) <= threshold)
                {
                    ref var pixel = ref Pixels[i];
                    if (pixel.Priority >= minPriority)
                    {
                        pixel.Priority += deltaPriority;
                        maxAdjustedPriority = Math.Max(maxAdjustedPriority, pixel.Priority);
                    }
                }
            }
        });
        MaxPriority = Math.Max(MaxPriority, maxAdjustedPriority);
        return maxAdjustedPriority;
    }

    public List<(int x, int y, int priority)> Goals()
    {
        var result = new List<(int x, int y, int priority)>(Width * Height);
        var index = 0;
        for (var y = 0; y < Height; ++y)
        {
            for (var x = 0; x < Width; ++x)
            {
                if (Pixels[index].MaxG == float.MaxValue)
                    result.Add((x, y, Pixels[index].Priority));
                ++index;
            }
        }
        return result;
    }

    public (int x, int y, WPos center)[] EnumeratePixels()
    {
        var result = new (int x, int y, WPos center)[(Width * Height)];
        var rsq = Resolution * Resolution; // since we then multiply by _localZDivRes, end result is same as * res * rotation.ToDir()
        var dx = LocalZDivRes.OrthoL() * rsq;
        var dy = LocalZDivRes * rsq;
        var cy = Center + (-Width * HalfPixel + HalfPixel) * dx + (-Height * HalfPixel + HalfPixel) * dy;
        var index = 0;
        for (var y = 0; y < Height; ++y)
        {
            var cx = cy;
            for (var x = 0; x < Width; ++x)
            {
                result[index++] = (x, y, cx);
                cx += dx;
            }
            cy += dy;
        }
        return result;
    }

    // enumerate pixels along line starting from (x1, y1) to (x2, y2); first is not returned, last is returned
    public List<(int x, int y)> EnumeratePixelsInLine(int x1, int y1, int x2, int y2)
    {
        var estimatedLength = Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1)) + 1;
        var result = new List<(int x, int y)>(estimatedLength);
        int dx = Math.Abs(x2 - x1), sx = x1 < x2 ? 1 : -1;
        int dy = -Math.Abs(y2 - y1), sy = y1 < y2 ? 1 : -1;
        int err = dx + dy, e2;

        while (true)
        {
            result.Add((x1, y1));
            if (x1 == x2 && y1 == y2)
                break;
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
        }
        return result;
    }
}
