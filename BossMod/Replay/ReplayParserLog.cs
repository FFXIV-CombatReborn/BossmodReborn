using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace BossMod;

public sealed class ReplayParserLog : IDisposable
{
    abstract class Input : IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) { }
        public abstract FourCC NextEntry();
        public abstract bool CanRead();
        public abstract void ReadVoid();
        public abstract string ReadString();
        public abstract float ReadFloat();
        public abstract double ReadDouble();
        public abstract Vector3 ReadVec3();
        public abstract Angle ReadAngle();
        public abstract bool ReadBool();
        public abstract sbyte ReadSByte();
        public abstract short ReadShort();
        public abstract int ReadInt();
        public abstract long ReadLong();
        public abstract byte ReadByte(bool hex);
        public abstract ushort ReadUShort(bool hex);
        public abstract uint ReadUInt(bool hex);
        public abstract ulong ReadULong(bool hex);
        public abstract byte[] ReadBytes();
        public abstract ActionID ReadAction();
        public abstract Class ReadClass();
        public abstract ActorStatus ReadStatus();
        public abstract ActionEffects ReadActionEffects();
        public abstract void ReadTargets(List<ActorCastEvent.Target> list);
        public abstract (float, float) ReadFloatPair();
        public abstract (DateTime, float) ReadTimePair();
        public abstract ulong ReadActorID();
    }

    sealed class TextInput(Stream stream) : Input
    {
        public DateTime Timestamp;
        public bool EscapedStrings;
        private readonly StreamReader _input = new(stream);
        private readonly List<string> _escapedParts = [];
        private string[] _line = [];
        private int _lineCount;
        private int _nextPayload;

        protected override void Dispose(bool disposing)
        {
            _input.Dispose();
            base.Dispose(disposing);
        }

        public override FourCC NextEntry()
        {
            while (_input.ReadLine() is var line && line != null)
            {
                if (line.Length == 0 || line[0] == '#')
                {
                    continue; // empty line or comment
                }

                string tag;
                string tsStr;
                if (EscapedStrings)
                {
                    ReplayBinaryCodec.SplitUnescapedPipe(line, _escapedParts);
                    if (_escapedParts.Count < 2)
                    {
                        continue; // invalid string
                    }
                    tsStr = _escapedParts[0];
                    tag = _escapedParts[1];
                    _lineCount = _escapedParts.Count;
                }
                else
                {
                    _line = line.Split('|');
                    if (_line.Length < 2)
                    {
                        continue; // invalid string
                    }
                    tsStr = _line[0];
                    tag = _line[1];
                    _lineCount = _line.Length;
                }

                if (tag.Length != 4)
                {
                    continue; // invalid tag
                }

                Timestamp = DateTime.Parse(tsStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                _nextPayload = 2;
                Span<byte> fourcc = [(byte)tag[0], (byte)tag[1], (byte)tag[2], (byte)tag[3]];
                return new(fourcc);
            }
            return default;
        }

        public override bool CanRead() => _nextPayload < _lineCount;
        public override void ReadVoid() => ++_nextPayload;

        private string ReadRawToken() => EscapedStrings ? _escapedParts[_nextPayload++] : _line[_nextPayload++];

        public override string ReadString()
        {
            var raw = ReadRawToken();
            return EscapedStrings ? ReplayBinaryCodec.UnescapeTextField(raw) : raw;
        }

        public override float ReadFloat() => float.Parse(ReadRawToken(), CultureInfo.InvariantCulture);
        public override double ReadDouble() => double.Parse(ReadRawToken(), CultureInfo.InvariantCulture);
        public override Vector3 ReadVec3()
        {
            var parts = ReadRawToken().Split('/');
            return new(float.Parse(parts[0], CultureInfo.InvariantCulture), float.Parse(parts[1], CultureInfo.InvariantCulture), float.Parse(parts[2], CultureInfo.InvariantCulture));
        }
        public override Angle ReadAngle() => ReadFloat().Degrees();
        public override bool ReadBool() => bool.Parse(ReadRawToken());
        public override sbyte ReadSByte() => sbyte.Parse(ReadRawToken(), CultureInfo.InvariantCulture);
        public override short ReadShort() => short.Parse(ReadRawToken(), CultureInfo.InvariantCulture);
        public override int ReadInt() => int.Parse(ReadRawToken(), CultureInfo.InvariantCulture);
        public override long ReadLong() => long.Parse(ReadRawToken(), CultureInfo.InvariantCulture);
        public override byte ReadByte(bool hex) => byte.Parse(ReadRawToken(), hex ? NumberStyles.HexNumber : NumberStyles.Number, CultureInfo.InvariantCulture);
        public override ushort ReadUShort(bool hex) => ushort.Parse(ReadRawToken(), hex ? NumberStyles.HexNumber : NumberStyles.Number, CultureInfo.InvariantCulture);
        public override uint ReadUInt(bool hex) => uint.Parse(ReadRawToken(), hex ? NumberStyles.HexNumber : NumberStyles.Number, CultureInfo.InvariantCulture);
        public override ulong ReadULong(bool hex) => ulong.Parse(ReadRawToken(), hex ? NumberStyles.HexNumber : NumberStyles.Number, CultureInfo.InvariantCulture);
        public override byte[] ReadBytes()
        {
            var str = ReadRawToken();
            var res = new byte[str.Length >> 1];
            for (var i = 0; i < res.Length; ++i)
            {
                res[i] = byte.Parse(str.AsSpan()[(2 * i)..(2 * i + 2)], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return res;
        }
        public override ActionID ReadAction()
        {
            var parts = ReadString().Split(' ');
            var type = parts.Length > 0 ? GeneratedEnumMetadata.Parse<ActionType>(parts[0]) : ActionType.None;
            var id = parts.Length > 1 ? uint.Parse(parts[1], CultureInfo.InvariantCulture) : 0;
            return new(type, id);
        }
        public override Class ReadClass() => GeneratedEnumMetadata.Parse<Class>(ReadRawToken());
        public override ActorStatus ReadStatus()
        {
            var sid = ReadString();
            var sep = sid.IndexOf(' ', StringComparison.Ordinal);
            return new(
                uint.Parse(sep >= 0 ? sid.AsSpan(0, sep) : sid.AsSpan(), CultureInfo.InvariantCulture),
                ushort.Parse(ReadRawToken(), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                Timestamp.AddSeconds(ReadFloat()),
                ReadActorID());
        }
        public override ActionEffects ReadActionEffects()
        {
            var effects = new ActionEffects();
            for (var i = 0; i < ActionEffects.MaxCount; ++i)
            {
                effects[i] = ReadULong(true);
            }

            return effects;
        }
        public override void ReadTargets(List<ActorCastEvent.Target> list)
        {
            while (CanRead())
            {
                var parts = ReadRawToken().Split('!');
                var firstEffect = parts.Length;
                while (firstEffect > 1 && parts.Length - (firstEffect - 1) <= ActionEffects.MaxCount && parts[firstEffect - 1].Length == 16 &&
                       ulong.TryParse(parts[firstEffect - 1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _))
                {
                    --firstEffect;
                }
                var effects = new ActionEffects();
                for (var j = firstEffect; j < parts.Length; ++j)
                {
                    effects[j - firstEffect] = ulong.Parse(parts[j], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                }

                list.Add(new(ParseActorID(parts[0]), effects));
            }
        }
        public override (float, float) ReadFloatPair()
        {
            var parts = ReadRawToken().Split('/');
            return (float.Parse(parts[0], CultureInfo.InvariantCulture), float.Parse(parts[1], CultureInfo.InvariantCulture));
        }
        public override (DateTime, float) ReadTimePair()
        {
            var parts = ReadRawToken().Split('/');
            return (Timestamp.AddSeconds(float.Parse(parts[0], CultureInfo.InvariantCulture)), float.Parse(parts[1], CultureInfo.InvariantCulture));
        }
        public override ulong ReadActorID() => ParseActorID(ReadRawToken());

        private static ulong ParseActorID(string actor)
        {
            var sep = actor.IndexOf('/', StringComparison.Ordinal);
            return ulong.Parse(sep >= 0 ? actor.AsSpan(0, sep) : actor.AsSpan(), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }
    }

    sealed class BinaryInput(Stream stream) : Input
    {
        private readonly BinaryReader _input = new(stream);

        public bool CompactV32Mode;
        public long CurFrameTicks;
        public ulong LastQpc;
        public uint LastFrameIndex;
        public ClientState.Gauge LastGauge;
        public Angle LastAzimuth;
        public uint LastErSeq;
        public uint LastCstGlobalSeq;
        public readonly Dictionary<ulong, ReplayBinaryCodec.QuantizedPosRot> LastPosQ = [];
        public readonly Dictionary<ulong, ActorHPMP> LastHPMP = [];
        public readonly ReplayBinaryCodec.RecentEffectsCache RecentEffects = new();

        public BinaryReader RawReader => _input;

        protected override void Dispose(bool disposing)
        {
            _input.Dispose();
            base.Dispose(disposing);
        }

        public override FourCC NextEntry()
        {
            if (CompactV32Mode)
            {
                var b = _input.BaseStream.ReadByte();
                if (b < 0)
                {
                    return default;
                }
                if (b == (byte)ReplayOpCode.FourCCEscape)
                {
                    return new(_input.ReadUInt32());
                }
                var tag = ReplayBinaryCodec.GetFourCC((byte)b);
                if (tag == default)
                {
                    throw new InvalidDataException($"Unknown compact replay opcode 0x{b:X2}");
                }
                return tag;
            }

            try
            {
                return new(_input.ReadUInt32());
            }
            catch (EndOfStreamException)
            {
                return default;
            }
        }

        public override bool CanRead() => true;
        public override void ReadVoid() { }
        public override string ReadString() => _input.ReadString();
        public override float ReadFloat() => _input.ReadSingle();
        public override double ReadDouble() => _input.ReadDouble();
        public override Vector3 ReadVec3() => new(_input.ReadSingle(), _input.ReadSingle(), _input.ReadSingle());
        public override Angle ReadAngle() => _input.ReadSingle().Radians();
        public override bool ReadBool() => _input.ReadBoolean();
        public override sbyte ReadSByte() => _input.ReadSByte();
        public override short ReadShort() => _input.ReadInt16();
        public override int ReadInt() => _input.ReadInt32();
        public override long ReadLong() => _input.ReadInt64();
        public override byte ReadByte(bool hex) => _input.ReadByte();
        public override ushort ReadUShort(bool hex) => _input.ReadUInt16();
        public override uint ReadUInt(bool hex) => _input.ReadUInt32();
        public override ulong ReadULong(bool hex) => _input.ReadUInt64();
        public override byte[] ReadBytes() => _input.ReadBytes(_input.ReadInt32());
        public override ActionID ReadAction() => new(_input.ReadUInt32());
        public override Class ReadClass() => (Class)_input.ReadByte();
        public override ActorStatus ReadStatus()
        {
            if (CompactV32Mode)
            {
                var id = (uint)ReplayBinaryCodec.ReadVarUInt(_input);
                var extra = _input.ReadUInt16();
                var deltaMs = ReplayBinaryCodec.ReadVarSInt(_input);
                var ticks = Math.Clamp(CurFrameTicks + deltaMs * TimeSpan.TicksPerMillisecond, 0L, DateTime.MaxValue.Ticks);
                var source = ReplayBinaryCodec.ReadCompactActorId(_input);
                return new(id, extra, new(ticks), source);
            }
            return new(_input.ReadUInt32(), _input.ReadUInt16(), new(_input.ReadInt64()), _input.ReadUInt64());
        }
        public override ActionEffects ReadActionEffects()
        {
            if (CompactV32Mode)
            {
                return ReplayBinaryCodec.ReadSparseActionEffects(_input);
            }

            var effects = new ActionEffects();
            for (var i = 0; i < ActionEffects.MaxCount; ++i)
            {
                effects[i] = _input.ReadUInt64();
            }

            return effects;
        }
        public override void ReadTargets(List<ActorCastEvent.Target> list)
        {
            var count = CompactV32Mode ? (int)ReplayBinaryCodec.ReadVarUInt(_input) : _input.ReadInt32();
            list.Capacity = count;
            for (var i = 0; i < count; ++i)
            {
                list.Add(new(ReadActorID(), ReadActionEffects()));
            }
        }
        public override (float, float) ReadFloatPair() => (_input.ReadSingle(), _input.ReadSingle());
        public override (DateTime, float) ReadTimePair() => (new(_input.ReadInt64()), _input.ReadSingle());
        public override ulong ReadActorID() => CompactV32Mode ? ReplayBinaryCodec.ReadCompactActorId(_input) : _input.ReadUInt64();
    }

    public static Replay Parse(string path, ref float progress, CancellationToken cancel)
    {
        try
        {
            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            Input? input = null;
            var inputOwnsStream = false;
            try
            {
                Span<byte> header = stackalloc byte[4];
                if (fileStream.Read(header) == header.Length)
                {
                    var headerMagic = new FourCC(header);
                    if (headerMagic == ReplayLogFormatMagic.CompressedBinary)
                    {
                        var decompressStream = new BrotliStream(fileStream, CompressionMode.Decompress, false);
                        input = new BinaryInput(new BufferedStream(decompressStream, 65536));
                    }
                    else if (headerMagic == ReplayLogFormatMagic.RawBinary)
                    {
                        input = new BinaryInput(new BufferedStream(fileStream, 65536));
                    }
                }
                if (input == null)
                {
                    fileStream.Seek(0, SeekOrigin.Begin);
                    input = new TextInput(fileStream);
                }
                inputOwnsStream = true;
            }
            finally
            {
                if (!inputOwnsStream)
                {
                    fileStream.Dispose();
                }
            }

            using (input)
            using (ReplayBuilder builder = new(path))
            using (ReplayParserLog parser = new(input, builder))
            {
                var streamInvLength = fileStream.Length > 0 ? 1.0f / fileStream.Length : 0f;
                var curOp = 0;
                try
                {
                    while (parser.ParseLine())
                    {
                        if ((++curOp & 0x3ff) == 0)
                        {
                            progress = fileStream.Position * streamInvLength;
                            if (cancel.IsCancellationRequested)
                            {
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex) when (ex is EndOfStreamException or InvalidDataException)
                {
                    Service.Log($"Replay {path} truncated near EOF ({ex.Message}); returning partial replay");
                }
                return cancel.IsCancellationRequested ? new() : builder.Finish();
            }
        }
        catch (Exception e)
        {
            Service.Log($"Failed to read {path}: {e}");
            return new();
        }
    }

    private readonly Input _input;
    private readonly ReplayBuilder _builder;
    private readonly Dictionary<FourCC, Func<WorldState.Operation?>> _dispatch;
    private int _version;
    private DateTime _tsStart;
    private ulong _qpcStart;
    private uint _legacyFrameIndex;
    private DateTime _legacyPrevTS;
    private double _invQPF = 1.0 / TimeSpan.TicksPerSecond;

    private ReplayParserLog(Input input, ReplayBuilder builder)
    {
        _input = input;
        _builder = builder;
        _dispatch = new()
        {
            [new("VER "u8)] = ParseVersion,
            [new("FRAM"u8)] = ParseFrameStart,
            [new("UMRK"u8)] = ParseUserMarker,
            [new("RSV "u8)] = ParseRSVData,
            [new("ZONE"u8)] = ParseZoneChange,
            [new("PVP+"u8)] = () => new WorldState.OpPvPArea(true),
            [new("PVP-"u8)] = () => new WorldState.OpPvPArea(false),
            [new("DIRU"u8)] = ParseDirectorUpdate,
            [new("ENVC"u8)] = ParseMapEffect,
            [new("LEME"u8)] = ParseLegacyMapEffect,
            [new("SLOG"u8)] = ParseSystemLog,
            [new("WAY+"u8)] = () => ParseWaymarkChange(true),
            [new("WAY-"u8)] = () => ParseWaymarkChange(false),
            [new("SGN+"u8)] = () => ParseSignChange(true),
            [new("SGN-"u8)] = () => ParseSignChange(false),
            [new("ACT+"u8)] = ParseActorCreate,
            [new("ACT-"u8)] = ParseActorDestroy,
            [new("NAME"u8)] = ParseActorRename,
            [new("CLSR"u8)] = ParseActorClassChange,
            [new("MOVE"u8)] = ParseActorMove,
            [new("ACSZ"u8)] = ParseActorSizeChange,
            [new("HP  "u8)] = ParseActorHPMP,
            [new("ATG+"u8)] = () => ParseActorTargetable(true),
            [new("ATG-"u8)] = () => ParseActorTargetable(false),
            [new("ALLY"u8)] = ParseActorAlly,
            [new("DIE+"u8)] = () => ParseActorDead(true),
            [new("DIE-"u8)] = () => ParseActorDead(false),
            [new("COM+"u8)] = () => ParseActorCombat(true),
            [new("COM-"u8)] = () => ParseActorCombat(false),
            [new("NENP"u8)] = ParseActorAggroPlayer,
            [new("MDLS"u8)] = ParseActorModelState,
            [new("EVTS"u8)] = ParseActorEventState,
            [new("TARG"u8)] = ParseActorTarget,
            [new("MNTD"u8)] = ParseActorMount,
            [new("FORA"u8)] = ParseActorForay,
            [new("TETH"u8)] = () => ParseActorTether(true),
            [new("TET+"u8)] = () => ParseActorTether(true), // legacy (up to v4)
            [new("TET-"u8)] = () => ParseActorTether(false), // legacy (up to v4)
            [new("CST+"u8)] = () => ParseActorCastInfo(true),
            [new("CST-"u8)] = () => ParseActorCastInfo(false),
            [new("CST!"u8)] = ParseActorCastEvent,
            [new("ER  "u8)] = ParseActorEffectResult,
            [new("STA+"u8)] = () => ParseActorStatus(true),
            [new("STA-"u8)] = () => ParseActorStatus(false),
            [new("STA!"u8)] = () => ParseActorStatus(true),
            [new("AIE+"u8)] = () => ParseActorIncomingEffect(true),
            [new("AIE-"u8)] = () => ParseActorIncomingEffect(false),
            [new("ICON"u8)] = ParseActorIcon,
            [new("VFX "u8)] = ParseActorVFX,
            [new("ESTA"u8)] = ParseActorEventObjectStateChange,
            [new("EANM"u8)] = ParseActorEventObjectAnimation,
            [new("PATE"u8)] = ParseActorPlayActionTimelineEvent,
            [new("PATS"u8)] = ParseActorPlayActionTimelineSync,
            [new("NYEL"u8)] = ParseActorEventNpcYell,
            [new("OPNT"u8)] = ParseActorEventOpenTreasure,
            [new("AVIS"u8)] = ParseActorVisibility,
            [new("PAR "u8)] = ParsePartyModify,
            [new("PAR+"u8)] = ParsePartyModify, // legacy (up to v3)
            [new("PAR-"u8)] = ParsePartyLeave, // legacy (up to v3)
            [new("PAR!"u8)] = ParsePartyModify, // legacy (up to v3)
            [new("LB  "u8)] = ParsePartyLimitBreak,
            [new("CLAR"u8)] = ParseClientActionRequest,
            [new("CLRJ"u8)] = ParseClientActionReject,
            [new("CDN+"u8)] = () => ParseClientCountdown(true),
            [new("CDN-"u8)] = () => ParseClientCountdown(false),
            [new("CLAL"u8)] = ParseClientAnimationLock,
            [new("CLCB"u8)] = ParseClientCombo,
            [new("CLST"u8)] = ParseClientPlayerStats,
            [new("CLMV"u8)] = ParseClientMovespeed,
            [new("FLY+"u8)] = () => new ClientState.OpFlyingChange(true),
            [new("FLY-"u8)] = () => new ClientState.OpFlyingChange(false),
            [new("CLCD"u8)] = ParseClientCooldown,
            [new("CLDA"u8)] = ParseClientDutyActions,
            [new("CLBH"u8)] = ParseClientBozjaHolster,
            [new("CBLU"u8)] = ParseClientBlueMageSpells,
            [new("CBST"u8)] = ParseClientBeastmasterBeasts,
            [new("CLVL"u8)] = ParseClientClassJobLevels,
            [new("CLAF"u8)] = ParseClientActiveFate,
            [new("CPET"u8)] = ParseClientActivePet,
            [new("CHOC"u8)] = ParseClientActiveCompanion,
            [new("CLFT"u8)] = ParseClientFocusTarget,
            [new("CLFD"u8)] = ParseClientForcedMovementDirection,
            [new("CLKV"u8)] = ParseClientContentKVData,
            [new("FATE"u8)] = ParseClientFateInfo,
            [new("HATE"u8)] = ParseClientHateInfo,
            [new("CLPR"u8)] = ParseClientProcTimers,
            [new("INVT"u8)] = ParseClientInventory,
            [new("FLOS"u8)] = ParseClientFailedLoS,
            [new("DDPG"u8)] = ParseDeepDungeonProgress,
            [new("DDMP"u8)] = ParseDeepDungeonMap,
            [new("DDPT"u8)] = ParseDeepDungeonParty,
            [new("DDIT"u8)] = ParseDeepDungeonPomanders,
            [new("DDCT"u8)] = ParseDeepDungeonChests,
            [new("DDMG"u8)] = ParseDeepDungeonMagicite,
            [new("IPCI"u8)] = ParseNetworkLegacyIDScramble,
            [new("IPCX"u8)] = ParseNetworkIDScramble,
            [new("IPCS"u8)] = ParseNetworkServerIPC,
            [new("RFLG"u8)] = ParseRenderflags,
        };
    }

    public void Dispose() => _input.Dispose();

    private bool ParseLine()
    {
        var tag = _input.NextEntry();
        if (tag == default)
        {
            return false; // end of replay
        }

        if (_version is > 0 and < 5 && _input is TextInput ti && _legacyPrevTS < ti.Timestamp)
        {
            _builder.AddOp(new WorldState.OpFrameStart(new() { Timestamp = ti.Timestamp }, default, default, default));
            _legacyPrevTS = ti.Timestamp;
        }

        if (!_dispatch.TryGetValue(tag, out var parse))
        {
            throw new InvalidOperationException($"Replay contains unsupported tag {tag}");
        }

        var op = parse();
        if (op != null)
        {
            _builder.AddOp(op);
        }

        return true;
    }

    private WorldState.Operation? ParseVersion()
    {
        _version = _input.ReadInt();
        if (_version < 2)
        {
            throw new InvalidOperationException($"Version {_version} is too old and is no longer supported, sorry");
        }

        var qpf = _version >= 10 ? _input.ReadULong(false) : (ulong)TimeSpan.TicksPerSecond; // newer windows versions have 10mhz qpc frequency
        var gameVersion = _version >= 11 ? _input.ReadString() : "old";
        _tsStart = _input is TextInput ti ? ti.Timestamp : new(_input.ReadLong());
        _invQPF = 1.0 / (qpf != 0 ? qpf : TimeSpan.TicksPerSecond);
        if (_version >= 32)
        {
            if (_input is BinaryInput bi)
            {
                bi.CompactV32Mode = true;
                bi.CurFrameTicks = _tsStart.Ticks;
            }
            else if (_input is TextInput tiEsc)
            {
                tiEsc.EscapedStrings = true;
            }
        }
        _builder.Start(qpf, gameVersion);
        return null;
    }

    private WorldState.OpFrameStart ParseFrameStart()
    {
        if (_version >= 32 && _input is BinaryInput bi)
        {
            var r = bi.RawReader;
            var flags = r.ReadByte();
            var frame = new FrameState();
            frame.QPC = (flags & (1 << 6)) != 0 ? r.ReadUInt64() : bi.LastQpc + ReplayBinaryCodec.ReadVarUInt(r);
            frame.Duration = r.ReadSingle();
            var gLo = (flags & (1 << 0)) != 0 ? r.ReadUInt64() : bi.LastGauge.Low;
            var gHi = (flags & (1 << 1)) != 0 ? r.ReadUInt64() : bi.LastGauge.High;
            frame.Index = (flags & (1 << 2)) != 0 ? (uint)ReplayBinaryCodec.ReadVarUInt(r) : bi.LastFrameIndex + 1;
            frame.DurationRaw = (flags & (1 << 3)) != 0 ? r.ReadSingle() : frame.Duration;
            frame.TickSpeedMultiplier = (flags & (1 << 4)) != 0 ? r.ReadSingle() : 1f;
            var azimuth = (flags & (1 << 5)) != 0 ? r.ReadSingle().Radians() : bi.LastAzimuth;

            if (_qpcStart != 0 && frame.QPC >= _qpcStart)
            {
                var timeDiff = (frame.QPC - _qpcStart) * _invQPF;
                frame.Timestamp = timeDiff >= 0 ? _tsStart.AddSeconds(timeDiff) : _tsStart;
            }
            else
            {
                if (_qpcStart == 0)
                {
                    _qpcStart = frame.QPC;
                }
                frame.Timestamp = _tsStart;
            }

            var gauge = new ClientState.Gauge(gLo, gHi);
            bi.LastQpc = frame.QPC;
            bi.LastFrameIndex = frame.Index;
            bi.LastGauge = gauge;
            bi.LastAzimuth = azimuth;
            bi.CurFrameTicks = frame.Timestamp.Ticks;
            _legacyPrevTS = frame.Timestamp;
            return new(frame, default, gauge, azimuth);
        }

        var legacyFrame = new FrameState();
        var prevUpdateTime = TimeSpan.FromMilliseconds(_input.ReadDouble());
        _input.ReadVoid();
        var legacyGauge = new ClientState.Gauge(
            _input.CanRead() ? _input.ReadULong(true) : 0,
            _version >= 18 ? _input.ReadULong(true) : 0
        );

        if (_version >= 10)
        {
            legacyFrame.QPC = _input.ReadULong(false);
            legacyFrame.Index = _input.ReadUInt(false);
            legacyFrame.DurationRaw = _input.ReadFloat();
            legacyFrame.Duration = _input.ReadFloat();
            legacyFrame.TickSpeedMultiplier = _input.ReadFloat();

            if (_qpcStart != 0 && legacyFrame.QPC >= _qpcStart)
            {
                var timeDiff = (legacyFrame.QPC - _qpcStart) * _invQPF;
                if (timeDiff >= 0)
                {
                    legacyFrame.Timestamp = _tsStart.AddSeconds(timeDiff);
                }
                else
                {
                    legacyFrame.Timestamp = _tsStart;
                    Service.Log($"Invalid time difference: {timeDiff}. Using default timestamp.");
                }
            }
            else
            {
                if (_qpcStart == 0)
                {
                    _qpcStart = legacyFrame.QPC;
                    legacyFrame.Timestamp = _tsStart;
                }
                else
                {
                    legacyFrame.Timestamp = _tsStart;
                    Service.Log($"Invalid QPC values: _qpcStart={_qpcStart}, frame.QPC={legacyFrame.QPC}");
                }
            }
        }
        else if (_input is TextInput ti)
        {
            legacyFrame.Timestamp = ti.Timestamp;
            legacyFrame.QPC = (ulong)(ti.Timestamp - _tsStart).Ticks;
            legacyFrame.Index = ++_legacyFrameIndex;
            legacyFrame.Duration = legacyFrame.DurationRaw = (float)(ti.Timestamp - _legacyPrevTS).TotalSeconds;
            legacyFrame.TickSpeedMultiplier = 1;
        }

        _legacyPrevTS = legacyFrame.Timestamp;

        return new(legacyFrame, prevUpdateTime, legacyGauge, _version >= 16 ? _input.ReadAngle() : default);
    }

    private WorldState.OpUserMarker ParseUserMarker() => new(_input.ReadString());
    private WorldState.OpRSVData ParseRSVData() => new(_input.ReadString(), _input.ReadString());
    private WorldState.OpZoneChange ParseZoneChange() => new(_input.ReadUShort(false), _version >= 13 ? _input.ReadUShort(false) : (ushort)0);
    private WorldState.OpDirectorUpdate ParseDirectorUpdate()
        => new(_input.ReadUInt(true), _input.ReadUInt(true), _input.ReadUInt(true), _input.ReadUInt(true), _input.ReadUInt(true), _input.ReadUInt(true));

    private WorldState.OpMapEffect ParseMapEffect()
    {
        // director id field is removed in v11
        if (_version < 11)
        {
            _input.ReadUInt(true);
        }

        return new(_input.ReadByte(true), _input.ReadUInt(true));
    }

    private WorldState.OpLegacyMapEffect ParseLegacyMapEffect() => new(_input.ReadByte(true), _input.ReadByte(true), _input.ReadBytes());

    private WorldState.OpSystemLogMessage ParseSystemLog()
    {
        var id = _input.ReadUInt(false);
        var argCount = _input.ReadInt();
        var args = new int[argCount];
        for (var i = 0; i < argCount; ++i)
        {
            args[i] = _input.ReadInt();
        }

        return new(id, args);
    }

    private ClientState.OpFateInfo ParseClientFateInfo() => new(_input.ReadUInt(false), new(_input.ReadLong()));

    private ClientState.OpHateChange ParseClientHateInfo()
    {
        var primary = _input.ReadActorID();
        var targets = new ClientState.Hate[ClientState.NumHateTargets];
        var haterCount = _input.ReadInt();
        for (var i = 0; i < haterCount; ++i)
        {
            var id = _input.ReadActorID();
            var enmity = _input.ReadInt();
            if (i < targets.Length)
            {
                targets[i] = new(id, enmity);
            }
        }

        return new(primary, targets);
    }

    private ClientState.OpProcTimersChange ParseClientProcTimers() => new([_input.ReadFloat(), _input.ReadFloat(), _input.ReadFloat(), _input.ReadFloat()]);

    private ClientState.OpInventoryChange ParseClientInventory() => new(_input.ReadUInt(false), _input.ReadUInt(false));
    private ClientState.OpActionFailedLoS ParseClientFailedLoS() => new(_input.ReadUInt(false), _input.ReadActorID());

    private WaymarkState.OpWaymarkChange ParseWaymarkChange(bool set)
        => new(_version < 10 ? GeneratedEnumMetadata.Parse<Waymark>(_input.ReadString()) : (Waymark)_input.ReadByte(false), set ? _input.ReadVec3() : null);

    private WaymarkState.OpSignChange ParseSignChange(bool set) => new((Sign)_input.ReadByte(false), set ? _input.ReadActorID() : 0);

    private ActorState.OpCreate ParseActorCreate()
    {
        if (_version < 10)
        {
            var parts = _input.ReadString().Split('/');
            var cls = _input.ReadClass();
            var targetable = _input.ReadBool();
            var radius = _input.ReadFloat();
            var owner = _input.CanRead() ? _input.ReadActorID() : 0;
            var hpmp = _input.CanRead() ? ReadActorHPMP() : default;
            var ally = _input.CanRead() && _input.ReadBool();
            var spawnIndex = _input.CanRead() ? _input.ReadInt() : -1;
            return new
            (
                ulong.Parse(parts[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                uint.Parse(parts[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                spawnIndex,
                0,
                parts[2],
                0,
                parts[3] == "Unknown" ? ActorType.Part : GeneratedEnumMetadata.Parse<ActorType>(parts[3]),
                cls,
                0,
                new(float.Parse(parts[4], CultureInfo.InvariantCulture), float.Parse(parts[5], CultureInfo.InvariantCulture), float.Parse(parts[6], CultureInfo.InvariantCulture), float.Parse(parts[7], CultureInfo.InvariantCulture).Degrees().Rad),
                radius,
                hpmp,
                targetable,
                ally,
                owner,
                0,
                0
            );
        }
        else
        {
            var op = new ActorState.OpCreate
            (
                _input.ReadULong(true),
                _input.ReadUInt(true),
                _input.ReadInt(),
                _version >= 26 ? _input.ReadUInt(true) : 0,
                _input.ReadString(),
                _version >= 13 ? _input.ReadUInt(false) : 0,
                (ActorType)_input.ReadUShort(true),
                _input.ReadClass(),
                _version < 12 ? 0 : _input.ReadInt(),
                new(_input.ReadVec3(), _input.ReadAngle().Rad),
                _input.ReadFloat(),
                new(_input.ReadUInt(false), _input.ReadUInt(false), _input.ReadUInt(false), _input.ReadUInt(false), _version >= 24 ? _input.ReadUInt(false) : 10000),
                _input.ReadBool(),
                _input.ReadBool(),
                _input.ReadActorID(),
                _version >= 14 ? _input.ReadUInt(false) : 0,
                _version >= 27 ? _input.ReadInt() : 0
            );
            if (_version >= 32 && _input is BinaryInput bi)
            {
                if (ReplayBinaryCodec.TryQuantize(op.PosRot, out var q))
                {
                    bi.LastPosQ[op.InstanceID] = q;
                }
                else
                {
                    bi.LastPosQ.Remove(op.InstanceID);
                }
                bi.LastHPMP[op.InstanceID] = op.HPMP;
            }
            return op;
        }
    }

    private ActorState.OpDestroy ParseActorDestroy()
    {
        var id = _input.ReadActorID();
        if (_version >= 32 && _input is BinaryInput bi)
        {
            bi.LastPosQ.Remove(id);
            bi.LastHPMP.Remove(id);
        }
        return new(id);
    }

    private ActorState.OpRename ParseActorRename()
    {
        if (_version < 10)
        {
            var parts = _input.ReadString().Split('/');
            return new(ulong.Parse(parts[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture), parts[2], 0);
        }
        else
        {
            return new(_input.ReadULong(true), _input.ReadString(), _version >= 13 ? _input.ReadUInt(false) : 0);
        }
    }

    private ActorState.OpClassChange ParseActorClassChange()
    {
        var instanceID = _input.ReadActorID();
        _input.ReadVoid();
        return new(instanceID, _input.ReadClass(), _version < 12 ? 0 : _input.ReadInt());
    }

    private ActorState.OpMove ParseActorMove()
    {
        if (_version < 10)
        {
            var parts = _input.ReadString().Split('/');
            return new(ulong.Parse(parts[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture), new(float.Parse(parts[4], CultureInfo.InvariantCulture), float.Parse(parts[5], CultureInfo.InvariantCulture), float.Parse(parts[6], CultureInfo.InvariantCulture), float.Parse(parts[7], CultureInfo.InvariantCulture).Degrees().Rad));
        }

        if (_version >= 32 && _input is BinaryInput bi)
        {
            var id = bi.ReadActorID();
            var r = bi.RawReader;
            var mask = r.ReadByte();
            if ((mask & 0x10) != 0)
            {
                var posRot = new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                if (ReplayBinaryCodec.TryQuantize(posRot, out var seeded))
                {
                    bi.LastPosQ[id] = seeded;
                }
                else
                {
                    bi.LastPosQ.Remove(id);
                }
                return new(id, posRot);
            }

            var prevQ = bi.LastPosQ.GetValueOrDefault(id);
            var qx = prevQ.X + ((mask & (1 << 0)) != 0 ? (int)ReplayBinaryCodec.ReadVarSInt(r) : 0);
            var qy = prevQ.Y + ((mask & (1 << 1)) != 0 ? (int)ReplayBinaryCodec.ReadVarSInt(r) : 0);
            var qz = prevQ.Z + ((mask & (1 << 2)) != 0 ? (int)ReplayBinaryCodec.ReadVarSInt(r) : 0);
            var qr = prevQ.Rot + ((mask & (1 << 3)) != 0 ? (int)ReplayBinaryCodec.ReadVarSInt(r) : 0);
            var nextQ = new ReplayBinaryCodec.QuantizedPosRot(qx, qy, qz, qr);
            bi.LastPosQ[id] = nextQ;
            return new(id, ReplayBinaryCodec.Dequantize(nextQ));
        }

        return new(_input.ReadULong(true), new(_input.ReadVec3(), _input.ReadAngle().Rad));
    }

    private ActorState.OpSizeChange ParseActorSizeChange() => new(_input.ReadActorID(), _input.ReadFloat());

    private ActorState.OpHPMP ParseActorHPMP()
    {
        if (_version >= 32 && _input is BinaryInput bi)
        {
            var id = bi.ReadActorID();
            var r = bi.RawReader;
            var mask = r.ReadByte();
            var hpmp = bi.LastHPMP.GetValueOrDefault(id);
            if ((mask & (1 << 0)) != 0) hpmp.CurHP = (uint)ReplayBinaryCodec.ReadVarUInt(r);
            if ((mask & (1 << 1)) != 0) hpmp.MaxHP = (uint)ReplayBinaryCodec.ReadVarUInt(r);
            if ((mask & (1 << 2)) != 0) hpmp.Shield = (uint)ReplayBinaryCodec.ReadVarUInt(r);
            if ((mask & (1 << 3)) != 0) hpmp.CurMP = (uint)ReplayBinaryCodec.ReadVarUInt(r);
            if ((mask & (1 << 4)) != 0) hpmp.MaxMP = (uint)ReplayBinaryCodec.ReadVarUInt(r);
            bi.LastHPMP[id] = hpmp;
            return new(id, hpmp);
        }
        return new(_input.ReadActorID(), ReadActorHPMP());
    }

    private ActorState.OpTargetable ParseActorTargetable(bool targetable) => new(_input.ReadActorID(), targetable);
    private ActorState.OpAlly ParseActorAlly() => new(_input.ReadActorID(), _input.ReadBool());
    private ActorState.OpDead ParseActorDead(bool dead) => new(_input.ReadActorID(), dead);
    private ActorState.OpCombat ParseActorCombat(bool value) => new(_input.ReadActorID(), value);
    private ActorState.OpAggroPlayer ParseActorAggroPlayer() => new(_input.ReadActorID(), _input.ReadBool());
    private ActorState.OpModelState ParseActorModelState()
        => new(_input.ReadActorID(), new(_input.ReadByte(false), _input.CanRead() ? _input.ReadByte(false) : (byte)0, _input.CanRead() ? _input.ReadByte(false) : (byte)0));
    private ActorState.OpEventState ParseActorEventState() => new(_input.ReadActorID(), _input.ReadByte(false));
    private ActorState.OpTarget ParseActorTarget() => new(_input.ReadActorID(), _input.ReadActorID());
    private ActorState.OpMount ParseActorMount() => new(_input.ReadActorID(), _input.ReadUInt(false));
    private ActorState.OpForayInfo ParseActorForay() => new(_input.ReadActorID(), new(_input.ReadByte(false), _input.ReadByte(false)));
    private ActorState.OpTether ParseActorTether(bool tether) => new(_input.ReadActorID(), tether ? new(_input.ReadUInt(false), _input.ReadActorID()) : default);
    private ActorState.OpRenderflags ParseRenderflags() => new(_input.ReadActorID(), _input.ReadInt());

    private ActorState.OpCastInfo ParseActorCastInfo(bool start)
    {
        var instanceID = _input.ReadActorID();
        ActorCastInfo? cast = null;
        if (start)
        {
            var action = _input.ReadAction();
            var target = _input.ReadActorID();
            var loc = _input.ReadVec3();
            float elapsedTime, totalTime;
            if (_version >= 17)
            {
                (elapsedTime, totalTime) = _input.ReadFloatPair();
            }
            else
            {
                (var finishAt, totalTime) = _input.ReadTimePair();
                elapsedTime = totalTime + action.CastTimeExtra() - (float)(finishAt - _legacyPrevTS).TotalSeconds;
            }
            cast = new()
            {
                Action = action,
                TargetID = target,
                Location = loc,
                ElapsedTime = elapsedTime,
                TotalTime = totalTime,
                Interruptible = _input.CanRead() && _input.ReadBool(),
                Rotation = _input.CanRead() ? _input.ReadAngle() : default,
            };
        }
        return new(instanceID, cast);
    }

    private ActorState.OpCastEvent ParseActorCastEvent()
    {
        if (_version >= 32 && _input is BinaryInput bi)
        {
            var r = bi.RawReader;
            var actor = bi.ReadActorID();
            var action = bi.ReadAction();
            var mainTarget = bi.ReadActorID();
            var animLock = r.ReadSingle();
            var maxTargets = (uint)ReplayBinaryCodec.ReadVarUInt(r);
            var targetPos = bi.ReadVec3();
            var dseq = ReplayBinaryCodec.ReadVarSInt(r);
            var globalSeq = (uint)(bi.LastCstGlobalSeq + dseq);
            bi.LastCstGlobalSeq = globalSeq;
            var sourceSeq = (uint)ReplayBinaryCodec.ReadVarUInt(r);
            var rot = bi.ReadAngle();
            var value = new ActorCastEvent(action, mainTarget, animLock, maxTargets, targetPos, globalSeq, sourceSeq, rot);
            bi.ReadTargets(value.Targets);
            for (var i = 0; i < value.Targets.Count; ++i)
            {
                bi.RecentEffects.Record(globalSeq, i, value.Targets[i].Effects);
            }
            return new(actor, value);
        }

        var legacyActor = _input.ReadActorID();
        var legacyValue = new ActorCastEvent(_input.ReadAction(), _input.ReadActorID(), _input.ReadFloat(), _input.ReadUInt(false),
            _version >= 6 ? _input.ReadVec3() : default,
            _version >= 7 ? _input.ReadUInt(false) : 0,
            _version >= 9 ? _input.ReadUInt(false) : 0,
            _version >= 21 ? _input.ReadAngle() : default);
        _input.ReadTargets(legacyValue.Targets);
        return new(legacyActor, legacyValue);
    }

    private ActorState.OpEffectResult ParseActorEffectResult()
    {
        if (_version >= 32 && _input is BinaryInput bi)
        {
            var r = bi.RawReader;
            var actor = bi.ReadActorID();
            var dseq = ReplayBinaryCodec.ReadVarSInt(r);
            var seq = (uint)(bi.LastErSeq + dseq);
            bi.LastErSeq = seq;
            var targetIndex = (int)ReplayBinaryCodec.ReadVarSInt(r);
            return new(actor, seq, targetIndex);
        }
        return new(_input.ReadActorID(), _input.ReadUInt(false), _input.ReadInt());
    }

    private ActorState.OpStatus ParseActorStatus(bool gainOrUpdate) => new(_input.ReadActorID(), _input.ReadInt(), gainOrUpdate ? _input.ReadStatus() : default);

    private ActorState.OpIncomingEffect ParseActorIncomingEffect(bool add)
    {
        if (add && _version >= 32 && _input is BinaryInput bi)
        {
            var r = bi.RawReader;
            var actor = bi.ReadActorID();
            var matched = r.ReadByte() != 0;
            var idx = (int)ReplayBinaryCodec.ReadVarSInt(r);
            var gseq = (uint)ReplayBinaryCodec.ReadVarUInt(r);
            var tidx = (int)ReplayBinaryCodec.ReadVarSInt(r);
            var src = bi.ReadActorID();
            var act = bi.ReadAction();
            ActionEffects effects;
            if (matched)
            {
                if (!bi.RecentEffects.TryGet(gseq, tidx, out effects))
                {
                    throw new InvalidDataException($"Missing cached ActionEffects for AIE+ seq={gseq} idx={tidx}");
                }
            }
            else
            {
                effects = ReplayBinaryCodec.ReadSparseActionEffects(r);
            }
            return new(actor, idx, new(gseq, tidx, src, act, effects));
        }

        return new(_input.ReadActorID(), _input.ReadInt(), add ? new(_input.ReadUInt(false), _input.ReadInt(), _input.ReadActorID(), _input.ReadAction(), _input.ReadActionEffects()) : default);
    }

    private ActorState.OpIcon ParseActorIcon() => new(_input.ReadActorID(), _input.ReadUInt(false), _version >= 22 ? _input.ReadActorID() : 0);
    private ActorState.OpVFX ParseActorVFX() => new(_input.ReadActorID(), _input.ReadUInt(false), _input.ReadActorID());
    private ActorState.OpEventObjectStateChange ParseActorEventObjectStateChange() => new(_input.ReadActorID(), _input.ReadUShort(true));
    private ActorState.OpEventObjectAnimation ParseActorEventObjectAnimation() => new(_input.ReadActorID(), _input.ReadUShort(true), _input.ReadUShort(true));
    private ActorState.OpPlayActionTimelineEvent ParseActorPlayActionTimelineEvent() => new(_input.ReadActorID(), _input.ReadUShort(true));
    private ActorState.OpPlayActionTimelineSync ParseActorPlayActionTimelineSync()
    {
        var owner = _input.ReadActorID();
        var count = _input.ReadInt();
        List<(ulong, ushort)> actions = [];
        for (var i = 0; i < count; ++i)
        {
            actions.Add((_input.ReadActorID(), _input.ReadUShort(true)));
        }

        return new(owner, actions);
    }
    private ActorState.OpEventNpcYell ParseActorEventNpcYell() => new(_input.ReadActorID(), _input.ReadUShort(false));
    private ActorState.OpEventOpenTreasure ParseActorEventOpenTreasure() => new(_input.ReadActorID());

    private ActorState.OpVisibility ParseActorVisibility() => new(_input.ReadActorID(), Visibility.Decode((char)_input.ReadUShort(false)));
    private PartyState.OpModify ParsePartyModify()
    {
        var slot = _input.ReadInt();
        var cid = _input.ReadULong(true);
        var iid = _input.ReadULong(true);
        var cs = _version >= 15 && _input.ReadBool();
        if (_version is >= 15 and < 31)
            _input.ReadString(); // member name, not stored here anymore (just look them up in worldstate)
        return new(slot, new(cid, iid, cs));
    }
    private PartyState.OpModify ParsePartyLeave() => new(_input.ReadInt(), new(0, 0, false));
    private PartyState.OpLimitBreakChange ParsePartyLimitBreak() => new(_input.ReadInt(), _input.ReadInt());

    private ClientState.OpActionRequest ParseClientActionRequest()
    {
        var action = _input.ReadAction();
        var targetId = _input.ReadActorID();
        var targetPos = _input.ReadVec3();
        var sequence = _input.ReadUInt(false);
        var animLock = _input.ReadFloat();
        var (castElapsed, castTotal) = _input.ReadFloatPair();
        var (recastElapsed, recastTotal) = _input.ReadFloatPair();

        return new(new(action, targetId, targetPos, sequence, animLock, castElapsed, castTotal, recastElapsed, recastTotal));
    }

    private ClientState.OpActionReject ParseClientActionReject()
    {
        var action = _input.ReadAction();
        var sourceSequence = _input.ReadUInt(false);
        var (recastElapsed, recastTotal) = _input.ReadFloatPair();
        var logMessageID = _input.ReadUInt(false);
        return new(new(action, sourceSequence, recastElapsed, recastTotal, logMessageID));
    }

    private ClientState.OpCountdownChange ParseClientCountdown(bool start) => new(start ? _input.ReadFloat() : null);
    private ClientState.OpAnimationLockChange ParseClientAnimationLock() => new(_input.ReadFloat());
    private ClientState.OpComboChange ParseClientCombo() => new(new(_input.ReadUInt(false), _input.ReadFloat()));
    private ClientState.OpPlayerStatsChange ParseClientPlayerStats() => new(new(_input.ReadInt(), _input.ReadInt(), _input.ReadInt()));
    private ClientState.OpMoveSpeedChange ParseClientMovespeed() => new(_input.ReadFloat());

    private ClientState.OpCooldown ParseClientCooldown()
    {
        var reset = _input.ReadBool();
        List<(int, Cooldown)> cooldowns = [];
        cooldowns.Capacity = _input.ReadByte(false);
        for (var i = 0; i < cooldowns.Capacity; ++i)
        {
            cooldowns.Add((_input.ReadByte(false), new(_input.ReadFloat(), _input.ReadFloat())));
        }

        if (_version < 19)
        {
            foreach (ref var cd in cooldowns.AsSpan())
            {
                cd.Item2.Elapsed = cd.Item2.Total - cd.Item2.Elapsed; // there was a mistake before v19 where remaining was saved instead of elapsed
            }
        }

        return new(reset, cooldowns);
    }

    private ClientState.OpDutyActionsChange ParseClientDutyActions()
    {
        if (_version < 25)
        {
            var slot0 = new ClientState.DutyAction(_input.ReadAction(), _version >= 20 ? _input.ReadByte(false) : (byte)1, _version >= 20 ? _input.ReadByte(false) : (byte)1);
            var slot1 = new ClientState.DutyAction(_input.ReadAction(), _version >= 20 ? _input.ReadByte(false) : (byte)1, _version >= 20 ? _input.ReadByte(false) : (byte)1);
            return new([slot0, slot1]);
        }

        var count = _input.ReadByte(false);
        var actions = new ClientState.DutyAction[count];
        for (var i = 0; i < count; ++i)
        {
            actions[i] = new ClientState.DutyAction(_input.ReadAction(), _input.ReadByte(false), _input.ReadByte(false));
        }

        return new(actions);
    }

    private ClientState.OpBozjaHolsterChange ParseClientBozjaHolster()
    {
        List<(BozjaHolsterID, byte)> contents = [];
        contents.Capacity = _input.ReadByte(false);
        for (var i = 0; i < contents.Capacity; ++i)
        {
            contents.Add(((BozjaHolsterID)_input.ReadByte(false), _input.ReadByte(false)));
        }

        return new(contents);
    }

    private ClientState.OpBlueMageSpellsChange ParseClientBlueMageSpells()
    {
        var contents = new uint[ClientState.NumBlueMageSpells];
        var count = _input.ReadByte(false);
        for (var i = 0; i < count; ++i)
        {
            var spell = _input.ReadUInt(false);
            if (i < contents.Length)
            {
                contents[i] = spell;
            }
        }

        return new(contents);
    }

    private ClientState.OpBeastmasterBeastsChanged ParseClientBeastmasterBeasts()
    {
        var contents = new byte[ClientState.NumBeastmasterBeasts];
        var count = _input.ReadByte(false);
        for (var i = 0; i < count; ++i)
        {
            var beast = _input.ReadByte(false);
            if (i < contents.Length)
            {
                contents[i] = beast;
            }
        }
        return new(contents);
    }

    private ClientState.OpClassJobLevelsChange ParseClientClassJobLevels()
    {
        var contents = new short[_input.ReadByte(false)];
        for (var i = 0; i < contents.Length; ++i)
        {
            contents[i] = _input.ReadShort();
        }

        return new(contents);
    }

    private ClientState.OpActiveFateChange ParseClientActiveFate() => new(new(_input.ReadUInt(false), _input.ReadVec3(), _input.ReadFloat(), _version >= 27 ? _input.ReadByte(false) : default, _version >= 27 ? _input.ReadByte(false) : default, _version >= 29 ? _input.ReadUInt(false) : default));
    private ClientState.OpActivePetChange ParseClientActivePet() => new(new(_input.ReadULong(true), _input.ReadByte(false), _input.ReadByte(false)));
    private ClientState.OpActiveCompanionChange ParseClientActiveCompanion() => new(new(_input.ReadULong(true), _input.ReadByte(false), _input.ReadFloat(), _version >= 30 && _input.ReadBool()));
    private ClientState.OpFocusTargetChange ParseClientFocusTarget() => new(_input.ReadULong(true));
    private ClientState.OpForcedMovementDirectionChange ParseClientForcedMovementDirection() => new(_input.ReadAngle());
    private ClientState.OpContentKVDataChange ParseClientContentKVData() => new([
        _input.ReadUInt(false),
        _input.ReadUInt(false),
        _input.ReadUInt(false),
        _input.ReadUInt(false),
        _input.ReadUInt(false),
        _input.ReadUInt(false),
    ]);

    private DeepDungeonState.OpProgressChange ParseDeepDungeonProgress() => new((DeepDungeonState.DungeonType)_input.ReadByte(false), new(_input.ReadByte(false), _input.ReadByte(false), _input.ReadByte(false), _input.ReadByte(false), _input.ReadByte(false), _input.ReadByte(false), _input.ReadByte(false), _input.ReadByte(false), _version >= 29 && _input.ReadBool()));
    private DeepDungeonState.OpMapDataChange ParseDeepDungeonMap()
    {
        var rooms = new FFXIVClientStructs.FFXIV.Client.Game.InstanceContent.InstanceContentDeepDungeon.RoomFlags[DeepDungeonState.NumRooms];
        if (_version < 23)
        {
            var raw = _input.ReadBytes();
            Array.Copy(raw, rooms, raw.Length);
        }
        else
        {
            for (var i = 0; i < rooms.Length; ++i)
            {
                var roomRaw = _version >= 29 ? _input.ReadUShort(true) : _input.ReadByte(true);
                rooms[i] = (FFXIVClientStructs.FFXIV.Client.Game.InstanceContent.InstanceContentDeepDungeon.RoomFlags)roomRaw;
            }
        }
        return new(rooms);
    }
    private DeepDungeonState.OpPartyStateChange ParseDeepDungeonParty()
    {
        var pt = new DeepDungeonState.PartyMember[DeepDungeonState.NumPartyMembers];
        for (var i = 0; i < pt.Length; ++i)
        {
            pt[i] = new(_input.ReadActorID(), _input.ReadByte(false));
        }

        return new(pt);
    }
    private DeepDungeonState.OpPomandersChange ParseDeepDungeonPomanders()
    {
        var it = new DeepDungeonState.PomanderState[DeepDungeonState.NumPomanderSlots];
        for (var i = 0; i < it.Length; ++i)
        {
            it[i] = new(_input.ReadByte(false), _input.ReadByte(true));
        }

        return new(it);
    }
    private DeepDungeonState.OpChestsChange ParseDeepDungeonChests()
    {
        var ct = new DeepDungeonState.Chest[DeepDungeonState.NumChests];
        for (var i = 0; i < ct.Length; ++i)
        {
            ct[i] = new(_input.ReadByte(false), _input.ReadByte(false));
        }

        return new(ct);
    }
    private DeepDungeonState.OpMagiciteChange ParseDeepDungeonMagicite() => new(_input.ReadBytes());

    private NetworkState.OpLegacyIDScramble ParseNetworkLegacyIDScramble() => new(_input.ReadUInt(false));
    private NetworkState.OpIDScramble ParseNetworkIDScramble() => new(new(_input.ReadUInt(false), _input.ReadUInt(false), _input.ReadUInt(false), _input.ReadUInt(false), _input.ReadUInt(false)));
    private NetworkState.OpServerIPC ParseNetworkServerIPC() => new(new((Network.ServerIPC.PacketID)_input.ReadInt(), _input.ReadUShort(false), _input.ReadUInt(false), _input.ReadUInt(true), new(_input.ReadLong()), _input.ReadBytes()));

    private ActorHPMP ReadActorHPMP()
    {
        if (_version < 10)
        {
            var parts = _input.ReadString().Split('/');
            return new(uint.Parse(parts[0], CultureInfo.InvariantCulture), uint.Parse(parts[1], CultureInfo.InvariantCulture), parts.Length > 2 ? uint.Parse(parts[2], CultureInfo.InvariantCulture) : 0, parts.Length > 3 ? uint.Parse(parts[3], CultureInfo.InvariantCulture) : 0, 10000);
        }
        else
        {
            return new(_input.ReadUInt(false), _input.ReadUInt(false), _input.ReadUInt(false), _input.ReadUInt(false), _version >= 24 ? _input.ReadUInt(false) : 10000);
        }
    }
}
