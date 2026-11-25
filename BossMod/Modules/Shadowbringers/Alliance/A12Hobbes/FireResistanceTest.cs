namespace BossMod.Shadowbringers.Alliance.A12Hobbes;

sealed class FireResistanceTest(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect half = new(22f, 21f, 2f);
    private static readonly AOEShapeCone coneInside = new(70f, 15f.Degrees());
    private static readonly AOEShapeCone coneOutside = new(70f, 30f.Degrees());
    private readonly List<AOEInstance> _aoes = [with(2)];
    private int _platform = -1;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => A12Hobbes.PlatformIndex(actor.Position) == _platform ? CollectionsMarshal.AsSpan(_aoes) : [];

    public override void OnMapEffect(byte index, uint state)
    {
        var platform = index switch
        {
            0x07 => 0,
            0x09 => 1,
            0x0A => 2,
            _ => -1
        };
        if (platform < 0)
        {
            return;
        }

        var center = A12Hobbes.PlatformCenters[platform];
        var arenaCenter = A12Hobbes.ArenaCenter;
        var activation = WorldState.FutureTime(10.0d);
        switch (state)
        {
            case 0x00100010u: // inside
                _platform = platform;
                _aoes.Clear();
                var dir = (arenaCenter - center).Normalized() * 15f;
                _aoes.Add(new(coneInside, arenaCenter + dir, Angle.FromDirection(center - arenaCenter), activation));
                break;
            case 0x00800080u: // outside
                _platform = platform;
                _aoes.Clear();
                var platformEdge = (arenaCenter - center).Normalized() * A12Hobbes.PlatformRadius + center;
                var angle = Angle.FromDirection(center - arenaCenter);
                _aoes.Add(new(coneOutside, platformEdge, angle + 55f.Degrees(), activation));
                _aoes.Add(new(coneOutside, platformEdge, angle - 55f.Degrees(), activation));
                break;
            case 0x04000400u: // CCW
                _platform = platform;
                _aoes.Clear();
                _aoes.Add(new(half, center, Angle.FromDirection(arenaCenter - center) - 85f.Degrees(), activation));
                break;
            case 0x20002000u: // CW
                _platform = platform;
                _aoes.Clear();
                _aoes.Add(new(half, center, Angle.FromDirection(arenaCenter - center) + 85f.Degrees(), activation));
                break;
            case 0x00400004u:
            case 0x02000004u:
            case 0x10000004u:
            case 0x80000004u:
                // bodge idk, mapeffect is generally close enough to the actual cast
                _platform = -1;
                _aoes.Clear();
                break;
        }
    }
}
