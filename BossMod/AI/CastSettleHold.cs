namespace BossMod.AI;

// after a move that interrupts a cast, hold off new casts until the mover has stood still for a short while
// the correction that forced the interrupt is often a single frame, and without a hold an external rotation (RSR) recasts the instant we stop,
// so anything that keeps forcing tiny corrections (e.g. riding the edge of a stack that follows a moving player) turns into a cast/interrupt loop
// shared by both movement systems (automovement ai and the autorotation movement module); the result is published via AIHints.HoldCasts
public sealed class CastSettleHold
{
    public const float SettleSeconds = 0.5f;

    private DateTime _until;

    public void Reset() => _until = default;

    // call once per frame; returns true while casts should be held
    // started is set on the frame an interrupting move opens a new hold (useful for logging)
    public bool Update(DateTime now, bool moving, bool interruptsCast, out bool started)
    {
        started = moving && interruptsCast && now >= _until;
        if (moving && (interruptsCast || now < _until))
        {
            _until = now.AddSeconds(SettleSeconds); // (re)start the settle window on every move while interrupting or still settling
        }
        return now < _until;
    }
}
