using Lumina.Excel.Sheets;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;

namespace BossMod;

public sealed class ReplayRecorder : IDisposable
{
    public const int Version = 32;

    // approach copied from https://stackoverflow.com/a/2565861
    // we don't care about reversing this "encryption" step; all we care about is that each unique ContentID is mapped to a unique "random" ContentID
    // this is required for role assignment to work in anonymized replays (i.e. if we returned a constant instead, everyone would get the same role)
    private sealed class IdAnonymizer
    {
        private readonly byte[] _table = CreateTable();

        private static byte[] CreateTable()
        {
            var result = new byte[256];
            for (var i = 0; i < 256; ++i)
            {
                result[i] = (byte)i;
            }
            Random.Shared.Shuffle(result);
            return result;
        }

        public ulong Hide(ulong id)
        {
            ulong res = 0;
            var c = 0;
            for (var i = 0; i < 8; ++i)
            {
                var b = (byte)(id >> (i * 8));
                c = _table[b ^ c];
                res |= (ulong)(byte)c << (i * 8);
            }
            return res;
        }
    }

    public uint CFCID;
    public string LogPath;

    public abstract class Output : IDisposable
    {
        public bool Anonymous { get; set; }
        private readonly IdAnonymizer _anonymizer = new();
        private readonly Dictionary<ulong, string> _anonPlayerNames = [];

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
        public abstract void StartEntry(DateTime t);
        public abstract Output Emit();
        public abstract Output Emit(string v);
        public abstract Output Emit(FourCC v);
        public abstract Output Emit(float v, string format = "g9");
        public abstract Output Emit(double v, string format = "g17");
        public abstract Output Emit(Vector3 v);
        public abstract Output Emit(Angle v);
        public abstract Output Emit(bool v);
        public abstract Output Emit(sbyte v);
        public abstract Output Emit(short v);
        public abstract Output Emit(int v);
        public abstract Output Emit(long v);
        public abstract Output Emit(byte v, string format = "d");
        public abstract Output Emit(ushort v, string format = "d");
        public abstract Output Emit(uint v, string format = "d");
        public abstract Output Emit(ulong v, string format = "d");
        public abstract Output Emit(byte[] v);
        public abstract Output Emit(ActionID v);
        public abstract Output Emit(Class v);
        public abstract Output Emit(ActorStatus v);
        public abstract Output Emit(in ActionEffects v);
        public abstract Output Emit(List<ActorCastEvent.Target> v);
        public abstract Output EmitFloatPair(float t1, float t2);
        public abstract Output EmitActor(ulong instanceID);

        protected static string FormatAnonName(Class klass, int level)
        {
            var row = Service.LuminaRow<ClassJob>((byte)klass);
            var fakeName = row != null ? ModuleViewer.FixCase(row.Value.Name) : "";
            if (fakeName.Length == 0)
            {
                fakeName = "Adventurer";
            }
            return string.Create(CultureInfo.InvariantCulture, $"{fakeName} L{level}");
        }

        public Output EmitName(string name, ActorType Type, Class klass, int level)
        {
            return Anonymous && Type == ActorType.Player ? Emit(FormatAnonName(klass, level)) : Emit(name);
        }

        public Output EmitActorName(ulong instanceID, string name, ActorType type, Class klass, int level)
        {
            if (Anonymous && type == ActorType.Player)
            {
                var fakeName = FormatAnonName(klass, level);
                _anonPlayerNames[instanceID] = fakeName;
                return Emit(fakeName);
            }
            return Emit(name);
        }

        public Output EmitRename(ulong instanceID, string name)
        {
            return Anonymous && _anonPlayerNames.TryGetValue(instanceID, out var fakeName) ? Emit(fakeName) : Emit(name);
        }

        public Output EmitContentID(ulong contentID)
        {
            if (Anonymous)
            {
                contentID = _anonymizer.Hide(contentID);
            }
            return Emit(contentID, "X");
        }

        public abstract void EndEntry();
        public abstract void Flush();

        public virtual void WriteOp(WorldState.Operation op)
        {
            StartEntry(op.Timestamp);
            op.Write(this);
            EndEntry();
            if (op is ActorState.OpDestroy ad)
            {
                ForgetAnonPlayerName(ad.InstanceID);
            }
        }

        public virtual void FlushPending() { }

        public Output EmitFourCC(ReadOnlySpan<byte> v) => Emit(new FourCC(v));

        protected void ForgetAnonPlayerName(ulong instanceID) => _anonPlayerNames.Remove(instanceID);
    }

    public sealed class TextOutput(Stream dest, ActorState? actorLookup) : Output
    {
        private readonly StreamWriter _dest = new(dest);
        private DateTime _curEntry;

        protected override void Dispose(bool disposing) => _dest.Dispose();
        public override void StartEntry(DateTime t)
        {
            _curEntry = t;
            _dest.Write(t.ToString("O", CultureInfo.InvariantCulture));
        }
        public override Output Emit() => WriteEntry("");
        public override Output Emit(string v) => WriteEntry(ReplayBinaryCodec.EscapeTextField(v));
        public override Output Emit(FourCC v)
        {
            _dest.Write('|');
            Span<char> chars = stackalloc char[4];
            var raw = v.Value;
            chars[0] = (char)(byte)raw;
            chars[1] = (char)(byte)(raw >> 8);
            chars[2] = (char)(byte)(raw >> 16);
            chars[3] = (char)(byte)(raw >> 24);
            _dest.Write(chars);
            return this;
        }
        public override Output Emit(float v, string format) => WriteEntry(v.ToString(format, CultureInfo.InvariantCulture));
        public override Output Emit(double v, string format) => WriteEntry(v.ToString(format, CultureInfo.InvariantCulture));
        public override Output Emit(Vector3 v) => WriteEntry(string.Create(CultureInfo.InvariantCulture, $"{v.X:f3}/{v.Y:f3}/{v.Z:f3}"));
        public override Output Emit(Angle v) => WriteEntry(v.Deg.ToString("f3", CultureInfo.InvariantCulture));
        public override Output Emit(bool v) => WriteEntry(v ? "True" : "False");
        public override Output Emit(sbyte v) => WriteEntry(v.ToString(CultureInfo.InvariantCulture));
        public override Output Emit(short v) => WriteEntry(v.ToString(CultureInfo.InvariantCulture));
        public override Output Emit(int v) => WriteEntry(v.ToString(CultureInfo.InvariantCulture));
        public override Output Emit(long v) => WriteEntry(v.ToString(CultureInfo.InvariantCulture));
        public override Output Emit(byte v, string format = "d") => WriteEntry(v.ToString(format, CultureInfo.InvariantCulture));
        public override Output Emit(ushort v, string format = "d") => WriteEntry(v.ToString(format, CultureInfo.InvariantCulture));
        public override Output Emit(uint v, string format = "d") => WriteEntry(v.ToString(format, CultureInfo.InvariantCulture));
        public override Output Emit(ulong v, string format = "d") => WriteEntry(v.ToString(format, CultureInfo.InvariantCulture));
        public override Output Emit(byte[] v)
        {
            _dest.Write('|');
            Span<char> hex = stackalloc char[2];
            const string Digits = "0123456789ABCDEF";
            for (var i = 0; i < v.Length; ++i)
            {
                var b = v[i];
                hex[0] = Digits[b >> 4];
                hex[1] = Digits[b & 0xF];
                _dest.Write(hex);
            }
            return this;
        }
        public override Output Emit(ActionID v) => WriteEntry(ReplayBinaryCodec.EscapeTextField(v.ToString()));
        public override Output Emit(Class v) => WriteEntry(v.ToString());
        public override Output Emit(ActorStatus v)
        {
            var statusStr = ReplayBinaryCodec.EscapeTextField(Utils.StatusString(v.ID));
            var timeStr = Math.Max(0, (v.ExpireAt - _curEntry).TotalSeconds).ToString("f3", CultureInfo.InvariantCulture);
            return WriteEntry(statusStr).WriteEntry(v.Extra.ToString("X4", CultureInfo.InvariantCulture)).WriteEntry(timeStr).EmitActor(v.SourceID);
        }
        public override Output Emit(in ActionEffects v)
        {
            for (var i = 0; i < ActionEffects.MaxCount; ++i)
            {
                Emit(v[i], "X16");
            }
            return this;
        }
        public override Output Emit(List<ActorCastEvent.Target> v)
        {
            for (var ti = 0; ti < v.Count; ++ti)
            {
                var t = v[ti];
                EmitActor(t.ID);
                for (var i = 0; i < ActionEffects.MaxCount; ++i)
                {
                    if (t.Effects[i] != 0)
                    {
                        _dest.Write(string.Create(CultureInfo.InvariantCulture, $"!{t.Effects[i]:X16}"));
                    }
                }
            }
            return this;
        }
        public override Output EmitFloatPair(float t1, float t2) => WriteEntry(string.Create(CultureInfo.InvariantCulture, $"{t1:f3}/{t2:f3}"));
        public override Output EmitActor(ulong instanceID)
        {
            var actor = actorLookup?.Find(instanceID);
            if (actor == null)
            {
                return WriteEntry(string.Create(CultureInfo.InvariantCulture, $"{instanceID:X8}"));
            }
            var rawName = Anonymous && actor.Type == ActorType.Player ? FormatAnonName(actor.Class, actor.Level) : actor.Name;
            var safeName = ReplayBinaryCodec.EscapeTextField(rawName.Replace('/', '_').Replace('!', '_'));
            return WriteEntry(string.Create(CultureInfo.InvariantCulture, $"{actor.InstanceID:X8}/{actor.OID:X}/{safeName}/{actor.Type}/{actor.PosRot.X:f3}/{actor.PosRot.Y:f3}/{actor.PosRot.Z:f3}/{actor.Rotation.Deg:f3}deg"));
        }

        public override void EndEntry()
        {
            _dest.WriteLine();
            _curEntry = default;
        }

        public override void Flush() => _dest.Flush();

        private TextOutput WriteEntry(string v)
        {
            _dest.Write('|');
            _dest.Write(v);
            return this;
        }
    }

    public sealed class BinaryOutput(Stream dest) : Output
    {
        private readonly BinaryWriter _dest = new(dest);

        private bool _compactV32Mode;
        private long _tsStartTicks;
        private ulong _qpf = TimeSpan.TicksPerSecond;
        private double _invQpf = 1.0 / TimeSpan.TicksPerSecond;
        private long _curFrameTicks;
        private bool _hasFirstFrame;
        private ulong _qpcStart;
        private ulong _lastQpc;
        private uint _lastFrameIndex;
        private ClientState.Gauge _lastGauge;
        private Angle _lastAzimuth;

        private WorldState.OpFrameStart? _pendingFrame;
        private float _accumDur;
        private float _accumDurRaw;

        private ulong _povActorId;
        private readonly Dictionary<ulong, ActorType> _actorTypes = [];
        private readonly Dictionary<ulong, ReplayBinaryCodec.QuantizedPosRot> _emittedPosQ = [];
        private readonly Dictionary<ulong, Vector4> _deferredMove = [];
        private readonly Dictionary<ulong, ActorHPMP> _lastHPMP = [];
        private uint _lastErSeq;
        private uint _lastCstGlobalSeq;
        private readonly ReplayBinaryCodec.RecentEffectsCache _recentEffects = new();

        public void EnableCompactV32(DateTime tsStart, ulong qpf)
        {
            _compactV32Mode = true;
            _tsStartTicks = tsStart.Ticks;
            _qpf = qpf != 0 ? qpf : TimeSpan.TicksPerSecond;
            _invQpf = 1.0 / _qpf;
            _curFrameTicks = _tsStartTicks;
        }

        protected override void Dispose(bool disposing) => _dest.Dispose();
        public override void StartEntry(DateTime t) { }
        public override Output Emit() => this;
        public override Output Emit(string v)
        {
            _dest.Write(v);
            return this;
        }
        public override Output Emit(FourCC v)
        {
            if (_compactV32Mode)
            {
                if (ReplayBinaryCodec.TryGetOpCode(v, out var opCode))
                {
                    _dest.Write(opCode);
                }
                else
                {
                    _dest.Write((byte)ReplayOpCode.FourCCEscape);
                    _dest.Write(v.Value);
                }
            }
            else
            {
                _dest.Write(v.Value);
            }
            return this;
        }
        public override Output Emit(float v, string format)
        {
            _dest.Write(v);
            return this;
        }
        public override Output Emit(double v, string format)
        {
            _dest.Write(v);
            return this;
        }
        public override Output Emit(Vector3 v)
        {
            _dest.Write(v.X);
            _dest.Write(v.Y);
            _dest.Write(v.Z);
            return this;
        }
        public override Output Emit(Angle v)
        {
            _dest.Write(v.Rad);
            return this;
        }
        public override Output Emit(bool v)
        {
            _dest.Write(v);
            return this;
        }
        public override Output Emit(sbyte v)
        {
            _dest.Write(v);
            return this;
        }
        public override Output Emit(short v)
        {
            _dest.Write(v);
            return this;
        }
        public override Output Emit(int v)
        {
            _dest.Write(v);
            return this;
        }
        public override Output Emit(long v)
        {
            _dest.Write(v);
            return this;
        }
        public override Output Emit(byte v, string format = "d")
        {
            _dest.Write(v);
            return this;
        }
        public override Output Emit(ushort v, string format = "d")
        {
            _dest.Write(v);
            return this;
        }
        public override Output Emit(uint v, string format = "d")
        {
            _dest.Write(v);
            return this;
        }
        public override Output Emit(ulong v, string format = "d")
        {
            _dest.Write(v);
            return this;
        }
        public override Output Emit(byte[] v)
        {
            _dest.Write(v.Length);
            _dest.Write(v);
            return this;
        }
        public override Output Emit(ActionID v)
        {
            _dest.Write(v.Raw);
            return this;
        }
        public override Output Emit(Class v)
        {
            _dest.Write((byte)v);
            return this;
        }
        public override Output Emit(ActorStatus v)
        {
            if (_compactV32Mode)
            {
                ReplayBinaryCodec.WriteVarUInt(_dest, v.ID);
                _dest.Write(v.Extra);
                var deltaMs = (long)Math.Round((double)(v.ExpireAt.Ticks - _curFrameTicks) / TimeSpan.TicksPerMillisecond);
                ReplayBinaryCodec.WriteVarSInt(_dest, deltaMs);
                ReplayBinaryCodec.WriteCompactActorId(_dest, v.SourceID);
            }
            else
            {
                _dest.Write(v.ID);
                _dest.Write(v.Extra);
                _dest.Write(v.ExpireAt.Ticks);
                _dest.Write(v.SourceID);
            }
            return this;
        }
        public override Output Emit(in ActionEffects v)
        {
            if (_compactV32Mode)
            {
                ReplayBinaryCodec.WriteSparseActionEffects(_dest, v);
            }
            else
            {
                for (var i = 0; i < ActionEffects.MaxCount; ++i)
                {
                    _dest.Write(v[i]);
                }
            }
            return this;
        }
        public override Output Emit(List<ActorCastEvent.Target> v)
        {
            if (_compactV32Mode)
            {
                ReplayBinaryCodec.WriteVarUInt(_dest, (uint)v.Count);
                for (var i = 0; i < v.Count; ++i)
                {
                    var t = v[i];
                    ReplayBinaryCodec.WriteCompactActorId(_dest, t.ID);
                    ReplayBinaryCodec.WriteSparseActionEffects(_dest, t.Effects);
                }
            }
            else
            {
                _dest.Write(v.Count);
                foreach (var t in v)
                {
                    _dest.Write(t.ID);
                    for (var i = 0; i < ActionEffects.MaxCount; ++i)
                    {
                        _dest.Write(t.Effects[i]);
                    }
                }
            }
            return this;
        }
        public override Output EmitFloatPair(float t1, float t2)
        {
            _dest.Write(t1);
            _dest.Write(t2);
            return this;
        }
        public override Output EmitActor(ulong instanceID)
        {
            if (_compactV32Mode)
            {
                ReplayBinaryCodec.WriteCompactActorId(_dest, instanceID);
            }
            else
            {
                _dest.Write(instanceID);
            }
            return this;
        }
        public override void EndEntry() { }
        public override void Flush() => _dest.Flush();

        public override void FlushPending()
        {
            FlushPendingFrame();
            if (_deferredMove.Count == 0)
            {
                return;
            }
            var ids = new ulong[_deferredMove.Count];
            _deferredMove.Keys.CopyTo(ids, 0);
            for (var i = 0; i < ids.Length; ++i)
            {
                FlushDeferredMove(ids[i]);
            }
        }

        public override void WriteOp(WorldState.Operation op)
        {
            if (!_compactV32Mode)
            {
                base.WriteOp(op);
                return;
            }

            if (op is WorldState.OpFrameStart fs)
            {
                HandleFrameStart(fs);
                return;
            }

            if (op is ActorState.OpMove mv)
            {
                HandleActorMove(mv);
                return;
            }

            // Flush stuffso timestamps remain exact
            FlushPendingFrame();

            // Force-flush deferred non-POV player positions prior to combat/lifecycle events involving those actors
            switch (op)
            {
                case ActorState.OpCastEvent ce:
                    FlushDeferredMove(ce.InstanceID);
                    FlushDeferredMove(ce.Value.MainTargetID);
                    for (var i = 0; i < ce.Value.Targets.Count; ++i)
                    {
                        FlushDeferredMove(ce.Value.Targets[i].ID);
                    }
                    WriteCastEvent(ce);
                    return;

                case ActorState.OpIncomingEffect aie:
                    FlushDeferredMove(aie.InstanceID);
                    if (aie.Value.GlobalSequence != 0)
                    {
                        FlushDeferredMove(aie.Value.SourceInstanceID);
                        WriteIncomingEffectAdd(aie);
                    }
                    else
                    {
                        aie.Write(this);
                    }
                    return;

                case ActorState.OpHPMP hp:
                    WriteHPMP(hp);
                    return;

                case ActorState.OpEffectResult er:
                    WriteEffectResult(er);
                    return;

                case ActorState.OpCreate ac:
                    ac.Write(this);
                    _actorTypes[ac.InstanceID] = ac.Type;
                    if (ac.SpawnIndex == 0 && ac.Type == ActorType.Player)
                    {
                        _povActorId = ac.InstanceID;
                    }
                    if (ReplayBinaryCodec.TryQuantize(ac.PosRot, out var initQ))
                    {
                        _emittedPosQ[ac.InstanceID] = initQ;
                    }
                    else
                    {
                        _emittedPosQ.Remove(ac.InstanceID);
                    }
                    _deferredMove.Remove(ac.InstanceID);
                    _lastHPMP[ac.InstanceID] = ac.HPMP;
                    return;

                case ActorState.OpDestroy ad:
                    FlushDeferredMove(ad.InstanceID);
                    ad.Write(this);
                    _actorTypes.Remove(ad.InstanceID);
                    _emittedPosQ.Remove(ad.InstanceID);
                    _deferredMove.Remove(ad.InstanceID);
                    _lastHPMP.Remove(ad.InstanceID);
                    ForgetAnonPlayerName(ad.InstanceID);
                    return;

                case PartyState.OpModify pm:
                    if (pm.Slot == PartyState.PlayerSlot && pm.Member.InstanceId != 0)
                    {
                        _povActorId = pm.Member.InstanceId;
                        FlushDeferredMove(_povActorId);
                    }
                    pm.Write(this);
                    return;

                case ActorState.OpCastInfo ci:
                    FlushDeferredMove(ci.InstanceID);
                    if (ci.Value != null)
                    {
                        FlushDeferredMove(ci.Value.TargetID);
                    }
                    ci.Write(this);
                    return;

                case ActorState.OpStatus st:
                    FlushDeferredMove(st.InstanceID);
                    if (st.Value.ID != 0)
                    {
                        FlushDeferredMove(st.Value.SourceID);
                    }
                    st.Write(this);
                    return;

                case ActorState.OpIcon ic:
                    FlushDeferredMove(ic.InstanceID);
                    FlushDeferredMove(ic.TargetID);
                    ic.Write(this);
                    return;

                case ActorState.OpVFX vfx:
                    FlushDeferredMove(vfx.InstanceID);
                    FlushDeferredMove(vfx.TargetID);
                    vfx.Write(this);
                    return;

                case ActorState.OpTether th:
                    FlushDeferredMove(th.InstanceID);
                    FlushDeferredMove(th.Value.Target);
                    th.Write(this);
                    return;

                case ActorState.OpDead dd:
                    FlushDeferredMove(dd.InstanceID);
                    dd.Write(this);
                    return;

                case ActorState.OpTargetable tg:
                    FlushDeferredMove(tg.InstanceID);
                    tg.Write(this);
                    return;

                default:
                    op.Write(this);
                    return;
            }
        }

        private void HandleFrameStart(WorldState.OpFrameStart fs)
        {
            if (_pendingFrame != null && _accumDur + fs.Frame.Duration > 0.1f)
            {
                FlushPendingFrame();
            }

            var isIdleCompatible = _hasFirstFrame &&
                fs.Frame.QPC >= _lastQpc &&
                fs.Frame.TickSpeedMultiplier == 1f &&
                fs.Frame.DurationRaw == fs.Frame.Duration &&
                fs.GaugePayload.Low == _lastGauge.Low &&
                fs.GaugePayload.High == _lastGauge.High &&
                fs.CameraAzimuth == _lastAzimuth;

            _accumDur += fs.Frame.Duration;
            _accumDurRaw += fs.Frame.DurationRaw;
            _pendingFrame = fs;

            if (!isIdleCompatible)
            {
                FlushPendingFrame();
            }
        }

        private void FlushPendingFrame()
        {
            var pf = _pendingFrame;
            if (pf == null)
            {
                return;
            }

            var dur = _accumDur;
            var durRaw = _accumDurRaw;
            _pendingFrame = null;
            _accumDur = 0f;
            _accumDurRaw = 0f;

            var qpc = pf.Frame.QPC;
            var absQpc = !_hasFirstFrame || qpc < _lastQpc;
            byte flags = 0;
            if (pf.GaugePayload.Low != _lastGauge.Low)
                flags |= 1 << 0;
            if (pf.GaugePayload.High != _lastGauge.High)
                flags |= 1 << 1;
            if (!_hasFirstFrame || pf.Frame.Index != _lastFrameIndex + 1)
                flags |= 1 << 2;
            if (durRaw != dur)
                flags |= 1 << 3;
            if (pf.Frame.TickSpeedMultiplier != 1f)
                flags |= 1 << 4;
            if (pf.CameraAzimuth != _lastAzimuth)
                flags |= 1 << 5;
            if (absQpc)
                flags |= 1 << 6;

            _dest.Write((byte)ReplayOpCode.FrameStart);
            _dest.Write(flags);
            if (absQpc)
            {
                _dest.Write(qpc);
            }
            else
            {
                ReplayBinaryCodec.WriteVarUInt(_dest, qpc - _lastQpc);
            }
            _dest.Write(dur);
            if ((flags & (1 << 0)) != 0)
                _dest.Write(pf.GaugePayload.Low);
            if ((flags & (1 << 1)) != 0)
                _dest.Write(pf.GaugePayload.High);
            if ((flags & (1 << 2)) != 0)
                ReplayBinaryCodec.WriteVarUInt(_dest, pf.Frame.Index);
            if ((flags & (1 << 3)) != 0)
                _dest.Write(durRaw);
            if ((flags & (1 << 4)) != 0)
                _dest.Write(pf.Frame.TickSpeedMultiplier);
            if ((flags & (1 << 5)) != 0)
                _dest.Write(pf.CameraAzimuth.Rad);

            if (!_hasFirstFrame)
            {
                _hasFirstFrame = true;
                _qpcStart = qpc;
                _curFrameTicks = _tsStartTicks;
            }
            else
            {
                _curFrameTicks = qpc >= _qpcStart ? new DateTime(_tsStartTicks).AddSeconds((qpc - _qpcStart) * _invQpf).Ticks : _tsStartTicks;
            }

            _lastQpc = qpc;
            _lastFrameIndex = pf.Frame.Index;
            _lastGauge = pf.GaugePayload;
            _lastAzimuth = pf.CameraAzimuth;
        }

        private void HandleActorMove(ActorState.OpMove mv)
        {
            var id = mv.InstanceID;
            var pos = mv.PosRot;
            if (!ReplayBinaryCodec.TryQuantize(pos, out var q) || !_emittedPosQ.TryGetValue(id, out var prevQ))
            {
                FlushPendingFrame();
                WriteRawMove(id, pos);
                if (ReplayBinaryCodec.TryQuantize(pos, out var seeded))
                {
                    _emittedPosQ[id] = seeded;
                }
                else
                {
                    _emittedPosQ.Remove(id);
                }
                _deferredMove.Remove(id);
                return;
            }

            var dx = q.X - prevQ.X;
            var dy = q.Y - prevQ.Y;
            var dz = q.Z - prevQ.Z;
            var dr = q.Rot - prevQ.Rot;
            if (dx == 0 && dy == 0 && dz == 0 && dr == 0)
            {
                _deferredMove.Remove(id);
                return;
            }

            if (id != _povActorId && _actorTypes.TryGetValue(id, out var actorType) && actorType == ActorType.Player)
            {
                var distSqMm = (long)dx * dx + (long)dy * dy + (long)dz * dz;
                var drWrap = dr;
                if (drWrap > 3142)
                    drWrap -= 6283;
                else if (drWrap < -3142)
                    drWrap += 6283;
                if (distSqMm < 10_000 && Math.Abs(drWrap) < 50)
                {
                    _deferredMove[id] = pos;
                    return;
                }
            }

            FlushPendingFrame();
            WriteDeltaMove(id, dx, dy, dz, dr);
            _emittedPosQ[id] = q;
            _deferredMove.Remove(id);
        }

        private void FlushDeferredMove(ulong id)
        {
            if (id == 0 || id == 0xE0000000 || !_deferredMove.Remove(id, out var pos))
            {
                return;
            }

            if (!ReplayBinaryCodec.TryQuantize(pos, out var q) || !_emittedPosQ.TryGetValue(id, out var prevQ))
            {
                WriteRawMove(id, pos);
                if (ReplayBinaryCodec.TryQuantize(pos, out var seeded))
                {
                    _emittedPosQ[id] = seeded;
                }
                else
                {
                    _emittedPosQ.Remove(id);
                }
                return;
            }

            var dx = q.X - prevQ.X;
            var dy = q.Y - prevQ.Y;
            var dz = q.Z - prevQ.Z;
            var dr = q.Rot - prevQ.Rot;
            if (dx == 0 && dy == 0 && dz == 0 && dr == 0)
            {
                return;
            }

            WriteDeltaMove(id, dx, dy, dz, dr);
            _emittedPosQ[id] = q;
        }

        private void WriteRawMove(ulong id, Vector4 pos)
        {
            _dest.Write((byte)ReplayOpCode.ActorMove);
            ReplayBinaryCodec.WriteCompactActorId(_dest, id);
            _dest.Write((byte)0x10);
            _dest.Write(pos.X);
            _dest.Write(pos.Y);
            _dest.Write(pos.Z);
            _dest.Write(pos.W);
        }

        private void WriteDeltaMove(ulong id, int dx, int dy, int dz, int dr)
        {
            byte mask = 0;
            if (dx != 0)
                mask |= 1 << 0;
            if (dy != 0)
                mask |= 1 << 1;
            if (dz != 0)
                mask |= 1 << 2;
            if (dr != 0)
                mask |= 1 << 3;
            _dest.Write((byte)ReplayOpCode.ActorMove);
            ReplayBinaryCodec.WriteCompactActorId(_dest, id);
            _dest.Write(mask);
            if (dx != 0)
                ReplayBinaryCodec.WriteVarSInt(_dest, dx);
            if (dy != 0)
                ReplayBinaryCodec.WriteVarSInt(_dest, dy);
            if (dz != 0)
                ReplayBinaryCodec.WriteVarSInt(_dest, dz);
            if (dr != 0)
                ReplayBinaryCodec.WriteVarSInt(_dest, dr);
        }

        private void WriteHPMP(ActorState.OpHPMP op)
        {
            _lastHPMP.TryGetValue(op.InstanceID, out var prev);
            ref readonly var cur = ref op.HPMP;
            byte mask = 0;
            if (cur.CurHP != prev.CurHP)
                mask |= 1 << 0;
            if (cur.MaxHP != prev.MaxHP)
                mask |= 1 << 1;
            if (cur.Shield != prev.Shield)
                mask |= 1 << 2;
            if (cur.CurMP != prev.CurMP)
                mask |= 1 << 3;
            if (cur.MaxMP != prev.MaxMP)
                mask |= 1 << 4;

            _dest.Write((byte)ReplayOpCode.ActorHPMP);
            ReplayBinaryCodec.WriteCompactActorId(_dest, op.InstanceID);
            _dest.Write(mask);
            if ((mask & (1 << 0)) != 0)
                ReplayBinaryCodec.WriteVarUInt(_dest, cur.CurHP);
            if ((mask & (1 << 1)) != 0)
                ReplayBinaryCodec.WriteVarUInt(_dest, cur.MaxHP);
            if ((mask & (1 << 2)) != 0)
                ReplayBinaryCodec.WriteVarUInt(_dest, cur.Shield);
            if ((mask & (1 << 3)) != 0)
                ReplayBinaryCodec.WriteVarUInt(_dest, cur.CurMP);
            if ((mask & (1 << 4)) != 0)
                ReplayBinaryCodec.WriteVarUInt(_dest, cur.MaxMP);
            _lastHPMP[op.InstanceID] = cur;
        }

        private void WriteEffectResult(ActorState.OpEffectResult op)
        {
            var dseq = (long)op.Seq - _lastErSeq;
            _lastErSeq = op.Seq;
            _dest.Write((byte)ReplayOpCode.EffectResult);
            ReplayBinaryCodec.WriteCompactActorId(_dest, op.InstanceID);
            ReplayBinaryCodec.WriteVarSInt(_dest, dseq);
            ReplayBinaryCodec.WriteVarSInt(_dest, op.TargetIndex);
        }

        private void WriteCastEvent(ActorState.OpCastEvent op)
        {
            ref readonly var v = ref op.Value;
            var dseq = (long)v.GlobalSequence - _lastCstGlobalSeq;
            _lastCstGlobalSeq = v.GlobalSequence;
            _dest.Write((byte)ReplayOpCode.CastEvent);
            ReplayBinaryCodec.WriteCompactActorId(_dest, op.InstanceID);
            _dest.Write(v.Action.Raw);
            ReplayBinaryCodec.WriteCompactActorId(_dest, v.MainTargetID);
            _dest.Write(v.AnimationLockTime);
            ReplayBinaryCodec.WriteVarUInt(_dest, v.MaxTargets);
            _dest.Write(v.TargetPos.X);
            _dest.Write(v.TargetPos.Y);
            _dest.Write(v.TargetPos.Z);
            ReplayBinaryCodec.WriteVarSInt(_dest, dseq);
            ReplayBinaryCodec.WriteVarUInt(_dest, v.SourceSequence);
            _dest.Write(v.Rotation.Rad);

            var targets = v.Targets;
            ReplayBinaryCodec.WriteVarUInt(_dest, (uint)targets.Count);
            for (var i = 0; i < targets.Count; ++i)
            {
                var t = targets[i];
                ReplayBinaryCodec.WriteCompactActorId(_dest, t.ID);
                ReplayBinaryCodec.WriteSparseActionEffects(_dest, t.Effects);
                _recentEffects.Record(v.GlobalSequence, i, t.Effects);
            }
        }

        private void WriteIncomingEffectAdd(ActorState.OpIncomingEffect op)
        {
            ref readonly var v = ref op.Value;
            var matched = v.GlobalSequence != 0 &&
                _recentEffects.TryGet(v.GlobalSequence, v.TargetIndex, out var cached) &&
                ReplayBinaryCodec.EffectsEqual(cached, v.Effects);

            _dest.Write((byte)ReplayOpCode.IncomingEffectAdd);
            ReplayBinaryCodec.WriteCompactActorId(_dest, op.InstanceID);
            _dest.Write(matched ? (byte)1 : (byte)0);
            ReplayBinaryCodec.WriteVarSInt(_dest, op.Index);
            ReplayBinaryCodec.WriteVarUInt(_dest, v.GlobalSequence);
            ReplayBinaryCodec.WriteVarSInt(_dest, v.TargetIndex);
            ReplayBinaryCodec.WriteCompactActorId(_dest, v.SourceInstanceID);
            _dest.Write(v.Action.Raw);
            if (!matched)
            {
                ReplayBinaryCodec.WriteSparseActionEffects(_dest, v.Effects);
            }
        }
    }

    private readonly WorldState _ws;
    private readonly Output _logger = null!;
    private readonly EventSubscription _subscription = null!;
    private long _lastFlushTicks;

    public ReplayRecorder(WorldState ws, ReplayLogFormat format, bool logInitialState, DirectoryInfo targetDirectory, string logPrefix, bool anon)
    {
        _ws = ws;
        CFCID = _ws.CurrentCFCID;
        LogPath = Path.Combine(targetDirectory.FullName, string.Create(CultureInfo.InvariantCulture, $"{logPrefix}_{_ws.CurrentTime:yyyy_MM_dd_HH_mm_ss}.log"));

        FileStream? rawFile = null;
        BrotliStream? brotli = null;
        try
        {
            rawFile = new FileStream(LogPath, FileMode.Create, FileAccess.Write, FileShare.Read);
            switch (format)
            {
                case ReplayLogFormat.BinaryCompressed:
                    WriteHeader(rawFile, ReplayLogFormatMagic.CompressedBinary);
                    brotli = new BrotliStream(rawFile, new BrotliCompressionOptions { Quality = 6 }, leaveOpen: false);
                    _logger = new BinaryOutput(new BackgroundWriteStream(brotli, rawFile));
                    break;
                case ReplayLogFormat.BinaryUncompressed:
                    WriteHeader(rawFile, ReplayLogFormatMagic.RawBinary);
                    _logger = new BinaryOutput(new BackgroundWriteStream(rawFile));
                    break;
                case ReplayLogFormat.TextCondensed:
                    _logger = new TextOutput(new BackgroundWriteStream(rawFile), null);
                    break;
                case ReplayLogFormat.TextVerbose:
                    _logger = new TextOutput(new BackgroundWriteStream(rawFile), _ws.Actors);
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(format), (int)format, typeof(ReplayLogFormat));
            }
            // BackgroundWriteStream now owns brotli/rawFile
            brotli = null;
            rawFile = null;

            _logger.Anonymous = anon;

            // log header
            _logger.StartEntry(_ws.CurrentTime);
            _logger.EmitFourCC("VER "u8).Emit(Version).Emit(_ws.QPF).Emit(_ws.GameVersion);
            if (_logger is BinaryOutput bo)
            {
                _logger.Emit(_ws.CurrentTime.Ticks);
                _logger.EndEntry();
                bo.EnableCompactV32(_ws.CurrentTime, _ws.QPF);
            }
            else
            {
                _logger.EndEntry();
            }

            _lastFlushTicks = _ws.CurrentTime.Ticks;

            if (logInitialState)
            {
                foreach (var op in _ws.CompareToInitial())
                {
                    op.Timestamp = _ws.CurrentTime;
                    Log(op);
                }
            }

            // log changes
            _subscription = _ws.Modified.Subscribe(Log);
        }
        catch
        {
            _subscription?.Dispose();
            _logger?.Dispose();
            brotli?.Dispose();
            rawFile?.Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        _subscription.Dispose();
        _logger.FlushPending();
        _logger.Dispose();
    }

    private unsafe void WriteHeader(Stream stream, FourCC magic)
    {
        Span<byte> buf = stackalloc byte[4];
        fixed (byte* raw = &buf[0])
        {
            *(uint*)raw = magic.Value;
            stream.Write(buf);
        }
    }

    private void Log(WorldState.Operation op)
    {
        _logger.WriteOp(op);
        if (op is WorldState.OpFrameStart fs && fs.Frame.Timestamp.Ticks - _lastFlushTicks >= 5 * TimeSpan.TicksPerSecond)
        {
            _lastFlushTicks = fs.Frame.Timestamp.Ticks;
            _logger.Flush();
        }
    }
}
