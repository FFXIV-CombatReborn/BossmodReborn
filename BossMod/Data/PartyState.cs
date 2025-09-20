namespace BossMod;

// state of the party/alliance/trust that player is part of; part of the world state structure
// solo player is considered to be in party of size 1
// after joining the party, member's slot never changes until leaving the party; this means that there could be intermediate gaps
// note that player could be in party without having actor in world (e.g. if he is in different zone)
// if player does not exist in world, party is always empty; otherwise player is always in slot 0
// in alliance, two 'other' groups use slots 8-15 and 16-23; alliance members don't have content-ID, but always have actor-ID
// in trust, buddies are considered party members with content-id 0 (but non-zero actor id, they are always in world)
// slots 24-63 are occupied by friendly NPCs, i.e. actors with type = Enemy who have the IsAlly and IsTargetable flags set
// certain modules need to treat NPCs as regular party members for the purpose of mechanic resolution
// we limit to 64 slots to facilitate a bitmask for the entire "party" state fitting inside one ulong
// party slot is considered 'empty' if both ids are 0
public sealed class PartyState
{
    public const int PlayerSlot = 0;
    public const int MaxPartySize = 8;
    public const int MaxAllianceSize = 24;
    public const int MaxAllies = 64;

    public struct Member(ulong contentId, ulong instanceId, bool inCutscene, string name)
    {
        public readonly ulong ContentId = contentId;
        public readonly ulong InstanceId = instanceId;
        public bool InCutscene = inCutscene;
        public readonly string Name = name;
        // note that a valid member can have 0 contentid (eg buddy) or 0 instanceid (eg player in a different zone)
        public readonly bool IsValid() => ContentId != default || InstanceId != default;

        public static bool operator ==(Member left, Member right) => left.ContentId == right.ContentId && left.InstanceId == right.InstanceId && left.InCutscene == right.InCutscene;
        public static bool operator !=(Member left, Member right) => left.ContentId != right.ContentId || left.InstanceId != right.InstanceId || left.InCutscene != right.InCutscene;

        public override readonly string ToString() => $"ContentID: {ContentId}, " + $"InstanceID: {InstanceId}, " + $"Name: {Name}";
        public readonly bool Equals(Member other) => this == other;
        public override readonly bool Equals(object? obj) => obj is Member other && Equals(other);
        public override readonly int GetHashCode() => (ContentId, InstanceId, InCutscene).GetHashCode();
    }
    public static readonly Member EmptySlot = new(default, default, false, "");

    public readonly Member[] Members = Utils.MakeArray(MaxAllies, EmptySlot);
    private readonly Actor?[] _actors = new Actor?[MaxAllies]; // transient

    public Actor? this[int slot] => (slot >= 0 && slot < _actors.Length) ? _actors[slot] : null; // bounds-checking accessor
    public Actor? Player() => this[PlayerSlot];

    public int LimitBreakCur;
    public int LimitBreakMax = 10000;

    public int LimitBreakLevel => LimitBreakMax > 0 ? LimitBreakCur / LimitBreakMax : 0;

    public PartyState(ActorState actorState)
    {
        void assign(ulong instanceID, Actor? actor)
        {
            var slot = FindSlot(instanceID);
            if (slot >= 0)
                _actors[slot] = actor;
        }
        actorState.Added.Subscribe(actor => assign(actor.InstanceID, actor));
        actorState.Removed.Subscribe(actor => assign(actor.InstanceID, null));
    }

    // select non-null and optionally alive raid members
    public Actor[] WithoutSlot(bool includeDead = false, bool excludeAlliance = false, bool excludeNPCs = false)
    {
        var limit = excludeNPCs && excludeAlliance ? MaxPartySize : excludeNPCs ? MaxAllianceSize : MaxAllies;
        var result = new Actor[limit];
        var count = 0;

        if (excludeAlliance)
        {
            for (var i = 0; i < MaxPartySize; ++i)
            {
                var player = _actors[i];
                if (player == null || !includeDead && player.IsDead)
                {
                    continue;
                }
                result[count++] = player;
            }
            if (!excludeNPCs)
            {
                for (var i = MaxAllianceSize; i < limit; ++i)
                {
                    var player = _actors[i];
                    if (player == null || !includeDead && player.IsDead)
                    {
                        continue;
                    }
                    result[count++] = player;
                }
            }
        }
        else
        {
            for (var i = 0; i < limit; ++i)
            {
                var player = _actors[i];
                if (player == null || !includeDead && player.IsDead)
                {
                    continue;
                }
                result[count++] = player;
            }
        }
        return result[..count];
    }

    public (int, Actor)[] WithSlot(bool includeDead = false, bool excludeAlliance = false, bool excludeNPCs = false)
    {
        var limit = excludeNPCs && excludeAlliance ? MaxPartySize : excludeNPCs ? MaxAllianceSize : MaxAllies;
        var result = new (int, Actor)[limit];
        var count = 0;

        if (excludeAlliance)
        {
            for (var i = 0; i < MaxPartySize; ++i)
            {
                var player = _actors[i];
                if (player == null || !includeDead && player.IsDead)
                {
                    continue;
                }
                result[count++] = (i, player);
            }
            if (!excludeNPCs)
            {
                for (var i = MaxAllianceSize; i < limit; ++i)
                {
                    var player = _actors[i];
                    if (player == null || !includeDead && player.IsDead)
                    {
                        continue;
                    }
                    result[count++] = (i, player);
                }
            }
        }
        else
        {
            for (var i = 0; i < limit; ++i)
            {
                var player = _actors[i];
                if (player == null || !includeDead && player.IsDead)
                {
                    continue;
                }
                result[count++] = (i, player);
            }
        }
        return result[..count];
    }

    // find a slot index containing specified player (by instance ID); returns -1 if not found
    public int FindSlot(ulong instanceID)
    {
        if (instanceID == default)
        {
            return -1;
        }
        var len = Members.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var m = ref Members[i];
            if (m.InstanceId == instanceID)
            {
                return i;
            }
        }
        return -1;
    }

    // find a slot index containing specified player (by name); returns -1 if not found
    public int FindSlot(ReadOnlySpan<char> name, StringComparison cmp = StringComparison.CurrentCultureIgnoreCase)
    {
        var length = Members.Length;
        for (var i = 0; i < length; ++i)
        {
            ref var m = ref Members[i];
            if (name.Equals(m.Name, cmp))
            {
                return i;
            }
        }
        return -1;
    }

    public List<WorldState.Operation> CompareToInitial()
    {
        var length = Members.Length;
        List<WorldState.Operation> ops = new(length + 1);
        for (var i = 0; i < length; ++i)
        {
            ref var m = ref Members[i];
            if (m.IsValid())
            {
                ops.Add(new OpModify(i, m));
            }
        }
        if (LimitBreakCur != 0 || LimitBreakMax != 10000)
        {
            ops.Add(new OpLimitBreakChange(LimitBreakCur, LimitBreakMax));
        }
        return ops;
    }

    // implementation of operations
    public Event<OpModify> Modified = new();
    public sealed class OpModify(int slot, Member member) : WorldState.Operation
    {
        public readonly int Slot = slot;
        public readonly Member Member = member;

        protected override void Exec(WorldState ws)
        {
            if (Slot >= 0 && Slot < ws.Party.Members.Length)
            {
                ref readonly var m = ref Member;
                ws.Party.Members[Slot] = m;
                ws.Party._actors[Slot] = ws.Actors.Find(m.InstanceId);
                ws.Party.Modified.Fire(this);
            }
            else
            {
                Service.Log($"[PartyState] Out-of-bounds slot {Slot}");
            }
        }
        public override void Write(ReplayRecorder.Output output)
        {
            ref readonly var m = ref Member;
            output.EmitFourCC("PAR "u8).Emit(Slot).Emit(m.ContentId, "X").Emit(m.InstanceId, "X8").Emit(m.InCutscene).Emit(m.Name);
        }
    }

    public Event<OpLimitBreakChange> LimitBreakChanged = new();
    public sealed class OpLimitBreakChange(int cur, int max) : WorldState.Operation
    {
        public readonly int Cur = cur;
        public readonly int Max = max;

        protected override void Exec(WorldState ws)
        {
            ws.Party.LimitBreakCur = Cur;
            ws.Party.LimitBreakMax = Max;
            ws.Party.LimitBreakChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("LB  "u8).Emit(Cur).Emit(Max);
    }
}
