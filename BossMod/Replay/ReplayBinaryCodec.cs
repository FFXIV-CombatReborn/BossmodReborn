using System.IO;

namespace BossMod;

public enum ReplayOpCode : byte
{
    Invalid = 0,
    Version = 1,
    FrameStart = 2,
    UserMarker = 3,
    RSVData = 4,
    ZoneChange = 5,
    PvPAdd = 6,
    PvPRemove = 7,
    DirectorUpdate = 8,
    MapEffect = 9,
    LegacyMapEffect = 10,
    SystemLog = 11,
    WaymarkAdd = 12,
    WaymarkRemove = 13,
    SignAdd = 14,
    SignRemove = 15,
    ActorCreate = 16,
    ActorDestroy = 17,
    ActorRename = 18,
    ActorClassChange = 19,
    ActorMove = 20,
    ActorSizeChange = 21,
    ActorHPMP = 22,
    ActorTargetableAdd = 23,
    ActorTargetableRemove = 24,
    ActorAlly = 25,
    ActorDeadAdd = 26,
    ActorDeadRemove = 27,
    ActorCombatAdd = 28,
    ActorCombatRemove = 29,
    ActorAggroPlayer = 30,
    ActorModelState = 31,
    ActorEventState = 32,
    ActorTarget = 33,
    ActorMount = 34,
    ActorForay = 35,
    ActorTether = 36,
    ActorTetherAddLegacy = 37,
    ActorTetherRemoveLegacy = 38,
    CastStart = 39,
    CastEnd = 40,
    CastEvent = 41,
    EffectResult = 42,
    StatusAdd = 43,
    StatusRemove = 44,
    StatusUpdateLegacy = 45,
    IncomingEffectAdd = 46,
    IncomingEffectRemove = 47,
    ActorIcon = 48,
    ActorVFX = 49,
    ActorEventObjectState = 50,
    ActorEventObjectAnim = 51,
    ActorPlayActionTimeline = 52,
    ActorPlayActionTimelineSync = 53,
    ActorNpcYell = 54,
    ActorOpenTreasure = 55,
    ActorVisibility = 56,
    ActorRenderflags = 57,
    PartyModify = 58,
    PartyAddLegacy = 59,
    PartyLeaveLegacy = 60,
    PartyModLegacy = 61,
    PartyLimitBreak = 62,
    ClientActionRequest = 63,
    ClientActionReject = 64,
    ClientCountdownAdd = 65,
    ClientCountdownRemove = 66,
    ClientAnimationLock = 67,
    ClientCombo = 68,
    ClientPlayerStats = 69,
    ClientMoveSpeed = 70,
    ClientFlyingAdd = 71,
    ClientFlyingRemove = 72,
    ClientCooldown = 73,
    ClientDutyActions = 74,
    ClientBozjaHolster = 75,
    ClientBlueMageSpells = 76,
    ClientBeastmasterBeasts = 77,
    ClientClassJobLevels = 78,
    ClientActiveFate = 79,
    ClientActivePet = 80,
    ClientActiveCompanion = 81,
    ClientFocusTarget = 82,
    ClientForcedMovement = 83,
    ClientContentKVData = 84,
    ClientFateInfo = 85,
    ClientHate = 86,
    ClientProcTimers = 87,
    ClientInventory = 88,
    ClientFailedLoS = 89,
    DeepDungeonProgress = 90,
    DeepDungeonMap = 91,
    DeepDungeonParty = 92,
    DeepDungeonPomanders = 93,
    DeepDungeonChests = 94,
    DeepDungeonMagicite = 95,
    NetworkLegacyIDScramble = 96,
    NetworkIDScramble = 97,
    NetworkServerIPC = 98,
    FourCCEscape = 255,
}

public static class ReplayBinaryCodec
{
    public const float MoveQuantScale = 1000f;
    public const float MoveQuantInvScale = 0.001f;

    public readonly record struct QuantizedPosRot(int X, int Y, int Z, int Rot);

    public sealed class RecentEffectsCache
    {
        private readonly (uint GlobalSeq, int TargetIndex, ActionEffects Effects)[] _entries = new (uint, int, ActionEffects)[256];

        public void Record(uint globalSeq, int targetIndex, in ActionEffects effects)
        {
            if (globalSeq == 0)
            {
                return;
            }
            var slot = (int)((globalSeq * 31u + (uint)targetIndex) & 255u);
            _entries[slot] = (globalSeq, targetIndex, effects);
        }

        public bool TryGet(uint globalSeq, int targetIndex, out ActionEffects effects)
        {
            if (globalSeq != 0)
            {
                var slot = (int)((globalSeq * 31u + (uint)targetIndex) & 255u);
                ref readonly var e = ref _entries[slot];
                if (e.GlobalSeq == globalSeq && e.TargetIndex == targetIndex)
                {
                    effects = e.Effects;
                    return true;
                }
            }
            effects = default;
            return false;
        }
    }

    private static readonly Dictionary<FourCC, byte> _fourCCToOp = [];
    private static readonly FourCC[] _opToFourCC = new FourCC[256];

    static ReplayBinaryCodec()
    {
        Register(ReplayOpCode.Version, "VER "u8);
        Register(ReplayOpCode.FrameStart, "FRAM"u8);
        Register(ReplayOpCode.UserMarker, "UMRK"u8);
        Register(ReplayOpCode.RSVData, "RSV "u8);
        Register(ReplayOpCode.ZoneChange, "ZONE"u8);
        Register(ReplayOpCode.PvPAdd, "PVP+"u8);
        Register(ReplayOpCode.PvPRemove, "PVP-"u8);
        Register(ReplayOpCode.DirectorUpdate, "DIRU"u8);
        Register(ReplayOpCode.MapEffect, "ENVC"u8);
        Register(ReplayOpCode.LegacyMapEffect, "LEME"u8);
        Register(ReplayOpCode.SystemLog, "SLOG"u8);
        Register(ReplayOpCode.WaymarkAdd, "WAY+"u8);
        Register(ReplayOpCode.WaymarkRemove, "WAY-"u8);
        Register(ReplayOpCode.SignAdd, "SGN+"u8);
        Register(ReplayOpCode.SignRemove, "SGN-"u8);
        Register(ReplayOpCode.ActorCreate, "ACT+"u8);
        Register(ReplayOpCode.ActorDestroy, "ACT-"u8);
        Register(ReplayOpCode.ActorRename, "NAME"u8);
        Register(ReplayOpCode.ActorClassChange, "CLSR"u8);
        Register(ReplayOpCode.ActorMove, "MOVE"u8);
        Register(ReplayOpCode.ActorSizeChange, "ACSZ"u8);
        Register(ReplayOpCode.ActorHPMP, "HP  "u8);
        Register(ReplayOpCode.ActorTargetableAdd, "ATG+"u8);
        Register(ReplayOpCode.ActorTargetableRemove, "ATG-"u8);
        Register(ReplayOpCode.ActorAlly, "ALLY"u8);
        Register(ReplayOpCode.ActorDeadAdd, "DIE+"u8);
        Register(ReplayOpCode.ActorDeadRemove, "DIE-"u8);
        Register(ReplayOpCode.ActorCombatAdd, "COM+"u8);
        Register(ReplayOpCode.ActorCombatRemove, "COM-"u8);
        Register(ReplayOpCode.ActorAggroPlayer, "NENP"u8);
        Register(ReplayOpCode.ActorModelState, "MDLS"u8);
        Register(ReplayOpCode.ActorEventState, "EVTS"u8);
        Register(ReplayOpCode.ActorTarget, "TARG"u8);
        Register(ReplayOpCode.ActorMount, "MNTD"u8);
        Register(ReplayOpCode.ActorForay, "FORA"u8);
        Register(ReplayOpCode.ActorTether, "TETH"u8);
        Register(ReplayOpCode.ActorTetherAddLegacy, "TET+"u8);
        Register(ReplayOpCode.ActorTetherRemoveLegacy, "TET-"u8);
        Register(ReplayOpCode.CastStart, "CST+"u8);
        Register(ReplayOpCode.CastEnd, "CST-"u8);
        Register(ReplayOpCode.CastEvent, "CST!"u8);
        Register(ReplayOpCode.EffectResult, "ER  "u8);
        Register(ReplayOpCode.StatusAdd, "STA+"u8);
        Register(ReplayOpCode.StatusRemove, "STA-"u8);
        Register(ReplayOpCode.StatusUpdateLegacy, "STA!"u8);
        Register(ReplayOpCode.IncomingEffectAdd, "AIE+"u8);
        Register(ReplayOpCode.IncomingEffectRemove, "AIE-"u8);
        Register(ReplayOpCode.ActorIcon, "ICON"u8);
        Register(ReplayOpCode.ActorVFX, "VFX "u8);
        Register(ReplayOpCode.ActorEventObjectState, "ESTA"u8);
        Register(ReplayOpCode.ActorEventObjectAnim, "EANM"u8);
        Register(ReplayOpCode.ActorPlayActionTimeline, "PATE"u8);
        Register(ReplayOpCode.ActorPlayActionTimelineSync, "PATS"u8);
        Register(ReplayOpCode.ActorNpcYell, "NYEL"u8);
        Register(ReplayOpCode.ActorOpenTreasure, "OPNT"u8);
        Register(ReplayOpCode.ActorVisibility, "AVIS"u8);
        Register(ReplayOpCode.ActorRenderflags, "RFLG"u8);
        Register(ReplayOpCode.PartyModify, "PAR "u8);
        Register(ReplayOpCode.PartyAddLegacy, "PAR+"u8);
        Register(ReplayOpCode.PartyLeaveLegacy, "PAR-"u8);
        Register(ReplayOpCode.PartyModLegacy, "PAR!"u8);
        Register(ReplayOpCode.PartyLimitBreak, "LB  "u8);
        Register(ReplayOpCode.ClientActionRequest, "CLAR"u8);
        Register(ReplayOpCode.ClientActionReject, "CLRJ"u8);
        Register(ReplayOpCode.ClientCountdownAdd, "CDN+"u8);
        Register(ReplayOpCode.ClientCountdownRemove, "CDN-"u8);
        Register(ReplayOpCode.ClientAnimationLock, "CLAL"u8);
        Register(ReplayOpCode.ClientCombo, "CLCB"u8);
        Register(ReplayOpCode.ClientPlayerStats, "CLST"u8);
        Register(ReplayOpCode.ClientMoveSpeed, "CLMV"u8);
        Register(ReplayOpCode.ClientFlyingAdd, "FLY+"u8);
        Register(ReplayOpCode.ClientFlyingRemove, "FLY-"u8);
        Register(ReplayOpCode.ClientCooldown, "CLCD"u8);
        Register(ReplayOpCode.ClientDutyActions, "CLDA"u8);
        Register(ReplayOpCode.ClientBozjaHolster, "CLBH"u8);
        Register(ReplayOpCode.ClientBlueMageSpells, "CBLU"u8);
        Register(ReplayOpCode.ClientBeastmasterBeasts, "CBST"u8);
        Register(ReplayOpCode.ClientClassJobLevels, "CLVL"u8);
        Register(ReplayOpCode.ClientActiveFate, "CLAF"u8);
        Register(ReplayOpCode.ClientActivePet, "CPET"u8);
        Register(ReplayOpCode.ClientActiveCompanion, "CHOC"u8);
        Register(ReplayOpCode.ClientFocusTarget, "CLFT"u8);
        Register(ReplayOpCode.ClientForcedMovement, "CLFD"u8);
        Register(ReplayOpCode.ClientContentKVData, "CLKV"u8);
        Register(ReplayOpCode.ClientFateInfo, "FATE"u8);
        Register(ReplayOpCode.ClientHate, "HATE"u8);
        Register(ReplayOpCode.ClientProcTimers, "CLPR"u8);
        Register(ReplayOpCode.ClientInventory, "INVT"u8);
        Register(ReplayOpCode.ClientFailedLoS, "FLOS"u8);
        Register(ReplayOpCode.DeepDungeonProgress, "DDPG"u8);
        Register(ReplayOpCode.DeepDungeonMap, "DDMP"u8);
        Register(ReplayOpCode.DeepDungeonParty, "DDPT"u8);
        Register(ReplayOpCode.DeepDungeonPomanders, "DDIT"u8);
        Register(ReplayOpCode.DeepDungeonChests, "DDCT"u8);
        Register(ReplayOpCode.DeepDungeonMagicite, "DDMG"u8);
        Register(ReplayOpCode.NetworkLegacyIDScramble, "IPCI"u8);
        Register(ReplayOpCode.NetworkIDScramble, "IPCX"u8);
        Register(ReplayOpCode.NetworkServerIPC, "IPCS"u8);

        static void Register(ReplayOpCode op, ReadOnlySpan<byte> tag)
        {
            var fcc = new FourCC(tag);
            _fourCCToOp[fcc] = (byte)op;
            _opToFourCC[(byte)op] = fcc;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetOpCode(FourCC tag, out byte op) => _fourCCToOp.TryGetValue(tag, out op);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FourCC GetFourCC(byte op) => _opToFourCC[op];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteVarUInt(BinaryWriter w, ulong v)
    {
        while (v >= 0x80u)
        {
            w.Write((byte)((v & 0x7Fu) | 0x80u));
            v >>= 7;
        }
        w.Write((byte)v);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ReadVarUInt(BinaryReader r)
    {
        ulong res = 0;
        var shift = 0;
        while (true)
        {
            var b = r.ReadByte();
            if (shift > 63 || shift == 63 && b > 1)
            {
                throw new InvalidDataException("Corrupt varint in replay stream");
            }
            res |= (ulong)(b & 0x7Fu) << shift;
            if ((b & 0x80u) == 0)
            {
                return res;
            }
            shift += 7;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteVarSInt(BinaryWriter w, long v) => WriteVarUInt(w, (ulong)((v << 1) ^ (v >> 63)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ReadVarSInt(BinaryReader r)
    {
        var u = ReadVarUInt(r);
        return (long)((u >> 1) ^ (ulong)-(long)(u & 1u));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteCompactActorId(BinaryWriter w, ulong id)
    {
        if (id <= 0xFFFFFFFEul)
        {
            w.Write((uint)id);
        }
        else
        {
            w.Write(0xFFFFFFFFu);
            w.Write(id);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ReadCompactActorId(BinaryReader r)
    {
        var lo = r.ReadUInt32();
        return lo != 0xFFFFFFFFu ? lo : r.ReadUInt64();
    }

    public static void WriteSparseActionEffects(BinaryWriter w, in ActionEffects v)
    {
        byte mask = 0;
        for (var i = 0; i < ActionEffects.MaxCount; ++i)
        {
            if (v[i] != 0)
            {
                mask |= (byte)(1 << i);
            }
        }
        w.Write(mask);
        if (mask != 0)
        {
            for (var i = 0; i < ActionEffects.MaxCount; ++i)
            {
                if ((mask & (1 << i)) != 0)
                {
                    w.Write(v[i]);
                }
            }
        }
    }

    public static ActionEffects ReadSparseActionEffects(BinaryReader r)
    {
        var effects = new ActionEffects();
        var mask = r.ReadByte();
        if (mask != 0)
        {
            for (var i = 0; i < ActionEffects.MaxCount; ++i)
            {
                if ((mask & (1 << i)) != 0)
                {
                    effects[i] = r.ReadUInt64();
                }
            }
        }
        return effects;
    }

    public static bool EffectsEqual(in ActionEffects a, in ActionEffects b)
    {
        for (var i = 0; i < ActionEffects.MaxCount; ++i)
        {
            if (a[i] != b[i])
            {
                return false;
            }
        }
        return true;
    }

    public static bool TryQuantize(Vector4 v, out QuantizedPosRot q)
    {
        if (!float.IsFinite(v.X) || !float.IsFinite(v.Y) || !float.IsFinite(v.Z) || !float.IsFinite(v.W) ||
            MathF.Abs(v.X) > 2_000_000f || MathF.Abs(v.Y) > 2_000_000f || MathF.Abs(v.Z) > 2_000_000f || MathF.Abs(v.W) > 2_000_000f)
        {
            q = default;
            return false;
        }
        q = new((int)MathF.Round(v.X * MoveQuantScale), (int)MathF.Round(v.Y * MoveQuantScale), (int)MathF.Round(v.Z * MoveQuantScale), (int)MathF.Round(v.W * MoveQuantScale));
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Dequantize(in QuantizedPosRot q)
        => new(q.X * MoveQuantInvScale, q.Y * MoveQuantInvScale, q.Z * MoveQuantInvScale, q.Rot * MoveQuantInvScale);

    private static readonly System.Buffers.SearchValues<char> _escapeSearchChars = System.Buffers.SearchValues.Create("\\|\r\n");

    public static string EscapeTextField(string v)
    {
        var idx = v.AsSpan().IndexOfAny(_escapeSearchChars);
        if (idx < 0)
        {
            return v;
        }
        var sb = new StringBuilder(v.Length + 8);
        sb.Append(v, 0, idx);
        for (var i = idx; i < v.Length; ++i)
        {
            switch (v[i])
            {
                case '\\': sb.Append(@"\\"); break;
                case '|': sb.Append(@"\|"); break;
                case '\r': sb.Append(@"\r"); break;
                case '\n': sb.Append(@"\n"); break;
                default: sb.Append(v[i]); break;
            }
        }
        return sb.ToString();
    }

    public static string UnescapeTextField(string v)
    {
        var idx = v.IndexOf('\\');
        if (idx < 0)
        {
            return v;
        }
        var sb = new StringBuilder(v.Length);
        sb.Append(v, 0, idx);
        for (var i = idx; i < v.Length; ++i)
        {
            var c = v[i];
            if (c == '\\' && i + 1 < v.Length)
            {
                switch (v[++i])
                {
                    case '\\': sb.Append('\\'); break;
                    case '|': sb.Append('|'); break;
                    case 'r': sb.Append('\r'); break;
                    case 'n': sb.Append('\n'); break;
                    default: sb.Append(v[i]); break;
                }
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    public static void SplitUnescapedPipe(string line, List<string> dst)
    {
        dst.Clear();
        var start = 0;
        var escaped = false;
        for (var i = 0; i < line.Length; ++i)
        {
            var c = line[i];
            if (escaped)
            {
                escaped = false;
            }
            else if (c == '\\')
            {
                escaped = true;
            }
            else if (c == '|')
            {
                dst.Add(line[start..i]);
                start = i + 1;
            }
        }
        dst.Add(line[start..]);
    }
}
