namespace BossMod.Dawntrail.Alliance.A11Prishe;

sealed class BanishStorm(BossModule module) : Components.Exaflare(module, 6f)
{
    public bool Done;

    private static readonly WPos[] positions = [new(815f, 415f), new(800f, 385f), new(785f, 400f), new(785f, 385f), new(815f, 400f), new(800f, 415f)];
    private static readonly WDir[] directions =
    [
        4f * (-0.003f).Degrees().ToDirection(),
        4f * 119.997f.Degrees().ToDirection(),
        4f * (-120.003f).Degrees().ToDirection(),
        4f * 180f.Degrees().ToDirection(),
        4f * (-60.005f).Degrees().ToDirection(),
        4f * 60f.Degrees().ToDirection(),
        4f * 89.999f.Degrees().ToDirection(),
        4f * (-150.001f).Degrees().ToDirection(),
        4f * (-30.001f).Degrees().ToDirection(),
        4f * (-90.004f).Degrees().ToDirection(),
        4f * 29.996f.Degrees().ToDirection(),
        4f * 149.996f.Degrees().ToDirection()
    ];
    private static readonly Dictionary<byte, (int position, int[] directions, int[] numExplosions)> LineConfigs = new()
    {
        { 0x0A, (0, [0, 1, 2], [5, 5, 14]) },
        { 0x34, (0, [0, 1, 2], [5, 5, 14]) },
        { 0x0D, (0, [3, 5, 4], [13, 5, 9]) },
        { 0x05, (3, [0, 1, 2], [13, 9, 5]) },
        { 0x02, (3, [3, 5, 4], [5, 14, 5]) },
        { 0x32, (3, [3, 5, 4], [5, 14, 5]) },
        { 0x0B, (1, [3, 4, 5], [5, 10, 10]) },
        { 0x35, (1, [3, 4, 5], [5, 10, 10]) },
        { 0x08, (1, [0, 2, 1], [13, 9, 9]) },
        { 0x09, (2, [9, 11, 10], [5, 10, 10]) },
        { 0x0C, (2, [6, 7, 8], [13, 9, 9]) },
        { 0x03, (4, [6, 7, 8], [5, 10, 10]) },
        { 0x06, (4, [9, 11, 10], [13, 9, 9]) },
        { 0x07, (5, [0, 1, 2], [5, 10, 10]) },
        { 0x33, (5, [0, 1, 2], [5, 10, 10]) },
        { 0x04, (5, [3, 5, 4], [13, 9, 9]) }
    };

    public override void OnMapEffect(byte index, uint state)
    {
        if (LineConfigs.TryGetValue(index, out var config))
        {
            if (state == 0x00020001u) // rod appear
            {
                var activation1 = WorldState.FutureTime(9.1d);
                var activation2 = WorldState.FutureTime(9.8d);

                for (var i = 0; i < 3; ++i)
                {
                    Lines.Add(new(next: positions[config.position] + (i > 0 ? directions[config.directions[i]] : default), directions[config.directions[i]],
                    i == 0 ? activation1 : activation2, 0.7d, config.numExplosions[i], config.numExplosions[i]));
                }
            }
            else if (state == 0x00080004u) // rod disappear
            {
                Done = true;
            }
            // 0x00200010 - aoe direction indicator appear
            // 0x00800040 - aoe direction indicator disappear
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Banish)
        {
            ++NumCasts;
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                        Lines.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
