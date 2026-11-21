namespace BossMod;

public sealed class NetworkState
{
    public readonly struct ServerIPC(Network.ServerIPC.PacketID id, ushort opcode, uint epoch, uint sourceServerActor, DateTime sendTimestamp, byte[] payload)
    {
        public readonly Network.ServerIPC.PacketID ID = id;
        public readonly ushort Opcode = opcode;
        public readonly uint Epoch = epoch;
        public readonly uint SourceServerActor = sourceServerActor;
        public readonly DateTime SendTimestamp = sendTimestamp;
        public readonly byte[] Payload = payload;
    };

    public readonly struct IDScrambleFields(uint gameSessionRandom, uint zoneRandom, uint key0, uint key1, uint key2) : IEquatable<IDScrambleFields>
    {
        public readonly uint GameSessionRandom = gameSessionRandom;
        public readonly uint ZoneRandom = zoneRandom;
        public readonly uint Key0 = key0;
        public readonly uint Key1 = key1;
        public readonly uint Key2 = key2;

        public static bool operator ==(IDScrambleFields left, IDScrambleFields right) => left.GameSessionRandom == right.GameSessionRandom && left.ZoneRandom == right.ZoneRandom && left.Key0 == right.Key0 && left.Key1 == right.Key1 && left.Key2 == right.Key2;
        public static bool operator !=(IDScrambleFields left, IDScrambleFields right) => left.GameSessionRandom != right.GameSessionRandom || left.ZoneRandom != right.ZoneRandom || left.Key0 != right.Key0 || left.Key1 != right.Key1 || left.Key2 != right.Key2;

        public readonly uint Decode(ushort opcode, uint value)
        {
            var key = (new uint[] { Key0, Key1, Key2 })[opcode % 3];
            return value - (key - GameSessionRandom - ZoneRandom);
        }

        public readonly bool Equals(IDScrambleFields other) => this == other;
        public override readonly bool Equals(object? obj) => obj is IDScrambleFields other && Equals(other);
        public override readonly int GetHashCode() => HashCode.Combine(GameSessionRandom, ZoneRandom, Key0, Key1, Key2);
    }

    public IDScrambleFields IDScramble;

    public List<WorldState.Operation> CompareToInitial() => IDScramble != default ? [new OpIDScramble(IDScramble)] : [];

    public Event<OpLegacyIDScramble> LegacyIDScrambleChanged = new();
    public sealed class OpLegacyIDScramble(uint value) : WorldState.Operation
    {
        public readonly uint Value = value;

        protected override void Exec(WorldState ws) => ws.Network.LegacyIDScrambleChanged.Fire(this);
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("IPCI"u8).Emit(Value);
    }

    public Event<OpIDScramble> IDScrambleChanged = new();
    public sealed class OpIDScramble(IDScrambleFields fields) : WorldState.Operation
    {
        public readonly IDScrambleFields Fields = fields;

        protected override void Exec(WorldState ws)
        {
            ws.Network.IDScramble = Fields;
            ws.Network.IDScrambleChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            ref readonly var f = ref Fields;
            output.EmitFourCC("IPCX"u8)
            .Emit(f.GameSessionRandom)
            .Emit(f.ZoneRandom)
            .Emit(f.Key0)
            .Emit(f.Key1)
            .Emit(f.Key2);
        }
    }

    public Event<OpServerIPC> ServerIPCReceived = new();
    public sealed class OpServerIPC(ServerIPC packet) : WorldState.Operation
    {
        public readonly ServerIPC Packet = packet;

        protected override void Exec(WorldState ws) => ws.Network.ServerIPCReceived.Fire(this);
        public override void Write(ReplayRecorder.Output output)
        {
            ref readonly var p = ref Packet;
            output.EmitFourCC("IPCS"u8)
            .Emit((int)p.ID)
            .Emit(p.Opcode)
            .Emit(p.Epoch)
            .Emit(p.SourceServerActor, "X8")
            .Emit(p.SendTimestamp.Ticks)
            .Emit(p.Payload);
        }
    }
}
