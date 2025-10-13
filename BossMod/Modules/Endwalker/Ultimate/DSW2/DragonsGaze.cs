namespace BossMod.Endwalker.Ultimate.DSW2;

// used by two trio mechanics, in p2 and in p5
abstract class DragonsGaze(BossModule module, uint bossOID, double activationDelay) : Components.GenericGaze(module, (uint)AID.DragonsGazeAOE)
{
    public bool EnableHints;
    private readonly uint _bossOID = bossOID;
    private Actor? _boss;
    private WPos _eyePosition;
    private DateTime _activation;

    public bool Active => _boss != null;

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        if (_boss != null && NumCasts == 0)
        {
            var eyes = new Eye[2];
            eyes[0] = new(_eyePosition, _activation, range: EnableHints ? 10000f : default);
            eyes[1] = new(_boss.Position, _activation, range: EnableHints ? 10000f : default);
            return eyes;
        }
        return [];
    }

    public override void OnMapEffect(byte index, uint state)
    {
        // seen indices: 2 = E, 5 = SW, 6 = W => inferring 0=N, 1=NE, ... cw order
        if (index <= 0x07 && state == 0x00020001u)
        {
            if (_activation == default)
                _activation = WorldState.FutureTime(activationDelay);
            _boss = Module.Enemies(_bossOID)[0];
            _eyePosition = Arena.Center + 40f * (180f - index * 45f).Degrees().ToDirection();
        }
    }
}
