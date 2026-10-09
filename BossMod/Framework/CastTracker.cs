namespace BossMod;

// tracks the player's recent casts and how they ended, so that a cast interrupted by movement (ours, manual or someone else's) can be told apart
// from one cancelled for another reason (e.g. an external rotation cancelling it); interrupted casts are logged, history is shown in the debug window
public sealed class CastTracker
{
    public enum Outcome { Casting, Completed, Interrupted, NoEffect }

    public sealed class Entry(ActionID action, uint sequence, DateTime start, WPos startPos)
    {
        public readonly ActionID Action = action;
        public readonly uint Sequence = sequence; // request sequence, matched against the action effect; 0 if unknown
        public readonly DateTime Start = start;
        public readonly WPos StartPos = startPos;
        public Outcome Outcome;
        public float Duration; // until effect or end, or elapsed so far while casting
        public float RemainingAtEnd; // cast time left when it ended without effect
        public int AutoMoveFrames; // frames where movement was injected by us (automovement / automatic movement module)
        public int UserMoveFrames; // frames with manual movement input that was not blocked
        public float Drift; // max distance from the position the cast started at

        public string LikelyCause => Outcome != Outcome.Interrupted ? ""
            : AutoMoveFrames > 0 ? "BMR movement"
            : UserMoveFrames > 0 ? "manual movement"
            : Drift > 0.05f ? "moved by something else"
            : "cancelled (no movement)";
    }

    public const int MaxEntries = 20;
    private const float NaturalEndThreshold = 0.15f; // a cast whose timer was at most this far from done when it disappeared ran out rather than being cut short

    public readonly List<Entry> History = [];
    private Entry? _current;
    private float _lastRemaining;

    public void Clear()
    {
        History.Clear();
        _current = null;
    }

    public void OnCastStart(ActionID action, uint sequence, WPos pos, float castRemaining)
    {
        if (_current != null)
        {
            Finish(_lastRemaining > NaturalEndThreshold ? Outcome.Interrupted : Outcome.NoEffect); // new request means the previous cast is gone
        }

        _current = new(action, sequence, DateTime.Now, pos);
        _lastRemaining = castRemaining;
        History.Add(_current);
        if (History.Count > MaxEntries)
        {
            History.RemoveAt(0);
        }
    }

    // note: the effect arrives before the client cast timer runs out (slidecast window), which is what marks the cast as completed
    public void OnEffect(uint sourceSequence)
    {
        if (_current != null && (_current.Sequence == 0 || _current.Sequence == sourceSequence))
        {
            Finish(Outcome.Completed);
        }
    }

    // call once per action manager update, after input for this frame was read
    public void Update(WPos pos, float castRemaining, bool autoMove, bool userMove)
    {
        if (_current == null)
        {
            return;
        }

        // count this frame's movement before checking whether the cast ended, so that the move that interrupted it is included
        _current.AutoMoveFrames += autoMove ? 1 : 0;
        _current.UserMoveFrames += userMove ? 1 : 0;
        _current.Drift = Math.Max(_current.Drift, (pos - _current.StartPos).Length());
        _current.Duration = (float)(DateTime.Now - _current.Start).TotalSeconds;

        if (castRemaining <= 0f)
        {
            Finish(_lastRemaining > NaturalEndThreshold ? Outcome.Interrupted : Outcome.NoEffect);
        }
        else
        {
            _lastRemaining = castRemaining;
        }
    }

    private void Finish(Outcome outcome)
    {
        var e = _current!;
        _current = null;
        e.Outcome = outcome;
        e.Duration = (float)(DateTime.Now - e.Start).TotalSeconds;
        if (outcome == Outcome.Interrupted)
        {
            e.RemainingAtEnd = _lastRemaining;
            Service.Log($"[AMEx] Cast {e.Action} interrupted after {e.Duration:f2}s ({e.RemainingAtEnd:f2}s left): autoMoveFrames={e.AutoMoveFrames}, userMoveFrames={e.UserMoveFrames}, drift={e.Drift:f2} -> likely {e.LikelyCause}");
        }
    }
}
