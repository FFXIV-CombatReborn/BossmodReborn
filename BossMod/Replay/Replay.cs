namespace BossMod;

public sealed class Replay
{
    public sealed class History<T> : IEnumerable<(DateTime Key, T Value)>
    {
        public struct Enumerator(List<(DateTime Key, T Value)> list) : IEnumerator<(DateTime Key, T Value)>
        {
            private readonly List<(DateTime Key, T Value)> _list = list;
            private int _index = -1;

            public readonly (DateTime Key, T Value) Current => _list[_index];
            readonly object IEnumerator.Current => Current;
            public bool MoveNext() => ++_index < _list.Count;
            public void Reset() => _index = -1;
            public readonly void Dispose() { }
        }

        private readonly List<(DateTime Key, T Value)> _entries = [];

        public int Count => _entries.Count;
        public (DateTime Key, T Value) this[int index] => _entries[index];

        public T this[DateTime key]
        {
            set
            {
                var count = _entries.Count;
                if (count == 0 || key > _entries[count - 1].Key)
                {
                    _entries.Add((key, value));
                    return;
                }

                var span = CollectionsMarshal.AsSpan(_entries);
                if (key == span[count - 1].Key)
                {
                    span[count - 1] = (key, value);
                    return;
                }

                var idx = LowerBound(key);
                if (idx < count && span[idx].Key == key)
                {
                    span[idx] = (key, value);
                }
                else
                {
                    _entries.Insert(idx, (key, value));
                }
            }
        }

        public int LowerBound(DateTime test)
        {
            var span = CollectionsMarshal.AsSpan(_entries);
            int first = 0, size = span.Length;
            while (size > 0)
            {
                var step = size >> 1;
                var mid = first + step;
                if (span[mid].Key < test)
                {
                    first = mid + 1;
                    size -= step + 1;
                }
                else
                {
                    size = step;
                }
            }
            return first;
        }

        public int UpperBound(DateTime test)
        {
            var span = CollectionsMarshal.AsSpan(_entries);
            int first = 0, size = span.Length;
            while (size > 0)
            {
                var step = size >> 1;
                var mid = first + step;
                if (span[mid].Key <= test)
                {
                    first = mid + 1;
                    size -= step + 1;
                }
                else
                {
                    size = step;
                }
            }
            return first;
        }

        public T? At(DateTime t)
        {
            var next = UpperBound(t);
            return next == 0 ? default : _entries[next - 1].Value;
        }

        public void RemoveLast() => _entries.RemoveAt(_entries.Count - 1);
        public void TrimExcess() => _entries.TrimExcess();

        public Enumerator GetEnumerator() => new(_entries);
        IEnumerator<(DateTime Key, T Value)> IEnumerable<(DateTime Key, T Value)>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public record struct TimeRange(DateTime Start = default, DateTime End = default)
    {
        public readonly float Duration => (float)(End - Start).TotalSeconds;

        public override readonly string ToString() => $"{Duration:f2}";
        public readonly bool Contains(DateTime t) => t >= Start && t <= End;
        public readonly TimeSpan Distance(DateTime t) => t < Start ? Start - t : t > End ? t - End : default;
    }

    // note: if target is non-null, Location corresponds to target's position at cast start
    public sealed record class Cast(ActionID ID, float ExpectedCastTime, Participant? Target, Vector3 Location, Angle Rotation, bool Interruptible)
    {
        public TimeRange Time;
        public ClientAction? ClientAction;
    }

    public sealed record class ActionTarget(Participant Target, ActionEffects Effects)
    {
        public DateTime ConfirmationSource;
        public DateTime ConfirmationTarget;
    }

    // note: if main target is non-null, TargetPos corresponds to main target's position at cast event
    public sealed record class Action(ActionID ID, DateTime Timestamp, Participant Source, Participant? MainTarget, Vector3 TargetPos, float AnimationLock, uint SourceSequence, uint GlobalSequence, Angle Rotation)
    {
        public readonly List<ActionTarget> Targets = [];
        public ClientAction? ClientAction;
    }

    // note: actors are sometimes destroyed and recreated (e.g. due to object size limits); we combine them into single participant structure with multiple 'instances'
    // sometimes we could even have participants that never exist as actors (but are referenced by other actions/events/etc.)
    // on the other hand, sometimes instance ids are reused for clearly different actors (e.g. different OIDs) - we try to separate them into different participants
    // we use a heuristic to distinguish actor destruction/recreation vs instance id reuse
    public sealed record class Participant(ulong InstanceID)
    {
        public uint OID;
        public ActorType Type;
        public ulong OwnerID;
        public uint ZoneID;
        public uint CFCID;
        public uint LayoutID;
        public TimeRange EffectiveExistence;
        public readonly List<TimeRange> WorldExistence = []; // sorted by time, non-overlapping ranges
        public readonly History<(string name, uint id)> NameHistory = new();
        public readonly History<bool> TargetableHistory = new();
        public readonly History<bool> DeadHistory = new();
        public readonly History<bool> AllyHistory = new();
        public readonly History<Vector4> PosRotHistory = new();
        public readonly History<ActorHPMP> HPMPHistory = new();
        public readonly List<Cast> Casts = [];
        public readonly History<uint> EventObjectAnimation = new();
        public readonly History<byte> EventState = new();
        public readonly History<ushort> ActionTimeline = new();
        public float MinRadius = float.MaxValue;
        public float MaxRadius = float.MinValue;
        public bool HasAnyActions;
        public bool HasAnyStatuses;
        public bool IsTargetOfAnyActions;

        public bool ExistsInWorldAt(DateTime t)
        {
            for (var i = 0; i < WorldExistence.Count; ++i)
            {
                if (WorldExistence[i].Contains(t))
                {
                    return true;
                }
            }
            return false;
        }

        public (string? name, uint id) NameAt(DateTime t) => NameHistory.At(t);
        public bool TargetableAt(DateTime t) => TargetableHistory.At(t);
        public bool AllyAt(DateTime t) => AllyHistory.At(t);
        public bool DeadAt(DateTime t) => DeadHistory.At(t);
        public Vector4 PosRotAt(DateTime t) => PosRotHistory.At(t);
        public ActorHPMP HPMPAt(DateTime t) => HPMPHistory.At(t);

        public bool WasAlly
        {
            get
            {
                for (var i = 0; i < AllyHistory.Count; ++i)
                {
                    if (AllyHistory[i].Value)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public void TrimExcess()
        {
            WorldExistence.TrimExcess();
            NameHistory.TrimExcess();
            TargetableHistory.TrimExcess();
            DeadHistory.TrimExcess();
            AllyHistory.TrimExcess();
            PosRotHistory.TrimExcess();
            HPMPHistory.TrimExcess();
            Casts.TrimExcess();
            EventObjectAnimation.TrimExcess();
            EventState.TrimExcess();
            ActionTimeline.TrimExcess();
        }
    }

    public sealed record class Status(uint ID, int Index, Participant Target, Participant? Source, float InitialDuration, ushort StartingExtra)
    {
        public TimeRange Time;
    }

    public sealed record class Tether(uint ID, Participant Source, Participant Target)
    {
        public TimeRange Time;
    }

    public sealed record class Icon(uint ID, Participant Source, Participant? Target, DateTime Timestamp);

    public sealed record class DirectorUpdate(uint DirectorID, uint UpdateID, uint Param1, uint Param2, uint Param3, uint Param4, DateTime Timestamp);

    public sealed record class MapEffect(byte Index, uint State, DateTime Timestamp);

    public sealed record class ClientAction(ActionID ID, uint SourceSequence, Participant? Target, Vector3 TargetPos, DateTime Requested)
    {
        public DateTime Rejected;
        public Cast? Cast;
        public Action? Action;
    }

    public sealed record class EncounterPhase(int ID, uint LastStateID, DateTime Exit);

    public sealed record class EncounterState(uint ID, string Name, string Comment, float ExpectedDuration, List<uint> ExpectedSuccessors, DateTime Exit)
    {
        public string FullName => $"{ID:X} '{Name}' ({Comment})";
    }

    public sealed record class EncounterError(DateTime Timestamp, Type? CompType, string Message);

    public sealed record class Encounter(ulong InstanceID, uint OID, ushort Zone)
    {
        public float CountdownOnPull = 10000;
        public TimeRange Time; // pull to deactivation
        public readonly List<EncounterPhase> Phases = [];
        public readonly List<EncounterState> States = [];
        public readonly List<EncounterError> Errors = [];
        public readonly Dictionary<uint, List<Participant>> ParticipantsByOID = []; // key = oid
        public readonly List<(Participant p, Class cls, int level)> PartyMembers = [];
        public int FirstAction;
        public int FirstStatus;
        public int FirstTether;
        public int FirstIcon;
        public int FirstDirectorUpdate;
        public int FirstEnvControl;
    }

    public string Path = "";
    public ulong QPF = TimeSpan.TicksPerSecond;
    public string GameVersion = "";
    public readonly List<WorldState.Operation> Ops = [];
    public readonly List<Participant> Participants = [];
    private readonly Dictionary<ulong, List<Participant>> _participantsById = [];
    public readonly List<Action> Actions = [];
    public readonly List<Status> Statuses = [];
    public readonly List<Tether> Tethers = [];
    public readonly List<Icon> Icons = [];
    public readonly List<DirectorUpdate> DirectorUpdates = [];
    public readonly List<MapEffect> MapEffects = [];
    public readonly List<ClientAction> ClientActions = [];
    public readonly List<Encounter> Encounters = [];
    public readonly List<(DateTime Timestamp, string Text)> UserMarkers = [];

    public void AddParticipant(Participant p)
    {
        Participants.Add(p);
        _participantsById.GetOrAdd(p.InstanceID).Add(p);
    }

    public IEnumerable<Action> EncounterActions(Encounter e)
    {
        for (var i = e.FirstAction; i < Actions.Count && Actions[i].Timestamp <= e.Time.End; ++i)
        {
            yield return Actions[i];
        }
    }

    public IEnumerable<Status> EncounterStatuses(Encounter e)
    {
        for (var i = e.FirstStatus; i < Statuses.Count && Statuses[i].Time.Start <= e.Time.End; ++i)
        {
            yield return Statuses[i];
        }
    }

    public IEnumerable<Tether> EncounterTethers(Encounter e)
    {
        for (var i = e.FirstTether; i < Tethers.Count && Tethers[i].Time.Start <= e.Time.End; ++i)
        {
            yield return Tethers[i];
        }
    }

    public IEnumerable<Icon> EncounterIcons(Encounter e)
    {
        for (var i = e.FirstIcon; i < Icons.Count && Icons[i].Timestamp <= e.Time.End; ++i)
        {
            yield return Icons[i];
        }
    }

    public IEnumerable<DirectorUpdate> EncounterDirectorUpdates(Encounter e)
    {
        for (var i = e.FirstDirectorUpdate; i < DirectorUpdates.Count && DirectorUpdates[i].Timestamp <= e.Time.End; ++i)
        {
            yield return DirectorUpdates[i];
        }
    }

    public IEnumerable<MapEffect> EncounterMapEffects(Encounter e)
    {
        for (var i = e.FirstEnvControl; i < MapEffects.Count && MapEffects[i].Timestamp <= e.Time.End; ++i)
        {
            yield return MapEffects[i];
        }
    }

    public Participant? FindParticipant(ulong instanceID, DateTime t)
    {
        if (!_participantsById.TryGetValue(instanceID, out var list))
        {
            return null;
        }
        for (var i = 0; i < list.Count; ++i)
        {
            var p = list[i];
            if (p.EffectiveExistence.Contains(t))
            {
                return p;
            }
        }
        return null;
    }
}
