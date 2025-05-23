﻿using System.Runtime.InteropServices;

namespace BossMod;

public enum ActionEffectType : byte
{
    Nothing = 0,
    Miss = 1,
    FullResist = 2,
    Damage = 3,
    Heal = 4,
    BlockedDamage = 5,
    ParriedDamage = 6,
    Invulnerable = 7,
    NoEffectText = 8,
    FailMissingStatus = 9,
    MpLoss = 10, // 0x0A
    MpGain = 11, // 0x0B
    TpLoss = 12, // 0x0C
    TpGain = 13, // 0x0D
    ApplyStatusEffectTarget = 14, // 0x0E - dissector calls this "GpGain"
    ApplyStatusEffectSource = 15, // 0x0F
    RecoveredFromStatusEffect = 16, // 0x10
    LoseStatusEffectTarget = 17, // 0x11
    LoseStatusEffectSource = 18, // 0x12
    //Unknown_13 = 19, // 0x13 - sometimes part of pvp Purify & Empyrean Rain spells, related to afflictions removal?..
    StatusNoEffect = 20, // 0x14
    ThreatPosition = 24, // 0x18 - provoke
    EnmityAmountUp = 25, // 0x19 - ? summons
    EnmityAmountDown = 26, // 0x1A
    StartActionCombo = 27, // 0x1B
    Retaliation = 28, // 0x1C - 'vengeance' has value = 7, 'arms length' has value = 0
    Knockback = 31, // 0x1F
    Attract1 = 32, // 0x20
    Attract2 = 33, // 0x21
    AttractCustom1 = 34, // 0x22
    AttractCustom2 = 35, // 0x23
    AttractCustom3 = 36, // 0x24
    Mount = 39, // 0x27
    ReviveLB = 50, // 0x32 - heal lb3 revive with full hp; seen value == 1
    FullResistStatus = 55, // 0x37 - full resist status (e.g. 9 = resist 'arms length' slow, 2 = resist 'low blow' stun)
    //Unknown_39 = 57, // 0x39 - 'you have been sentenced to death!' message
    VFX = 59, // 0x3B
    //Unknown_3D = 61, // 0x3D - was called 'gauge', but i think it's incorrect
    Resource = 62, // 0x3E - value 0x34 = gain war gauge (amount == hitSeverity)
    SetModelState = 73, // 0x49 - value == model state
    SetHP = 74, // 0x4A - e.g. zodiark's kokytos
    PartialInvulnerable = 75, // 0x4B
    Interrupt = 76, // 0x4C
}

public enum DamageType
{
    Unknown,
    Slashing,
    Piercing,
    Blunt,
    Shot,
    Magic,
    Breath,
    Physical,
    LimitBreak,
}

public enum DamageElementType
{
    Unknown,
    Fire,
    Ice,
    Air,
    Earth,
    Lightning,
    Water,
    Unaspected,
}

public enum KnockbackDirection
{
    AwayFromSource = 0, // direction = target-source
    Arg = 1, // direction = arg.degrees()
    Random = 2, // direction = random(0, 2pi)
    SourceForward = 3, // direction = src.direction
    SourceRight = 4, // direction = src.direction - pi/2
    SourceLeft = 5, // direction = src.direction + pi/2
    AwayFromSource2 = 6, // direction = target-source
}

public enum ActionResourceType
{
    WARGauge = 0x34,
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct ActionEffect
{
    public ActionEffectType Type;
    public byte Param0;
    public byte Param1;
    public byte Param2;
    public byte Param3;
    public byte Param4;
    public ushort Value;

    public readonly bool FromTarget => (Param4 & 0x20) != default;
    public readonly bool AtSource => (Param4 & 0x80) != default;
    public readonly DamageType DamageType => (DamageType)(Param1 & 0x0F); // for various damage effects
    public readonly DamageElementType DamageElement => (DamageElementType)(Param1 >> 4); // for various damage effects
    public readonly int DamageHealValue => Value + ((Param4 & 0x40) != default ? Param3 * 0x10000 : default); // for damage/heal effects
}

// TODO: convert to inline array
public unsafe struct ActionEffects : IEnumerable<ActionEffect>
{
    public const int MaxCount = 8;

    private fixed ulong _effects[MaxCount];

    public ulong this[int index]
    {
        get => _effects[index];
        set => _effects[index] = value;
    }

    public IEnumerator<ActionEffect> GetEnumerator()
    {
        for (var i = 0; i < 8; ++i)
        {
            var eff = Build(i);
            if (eff.Type != ActionEffectType.Nothing)
                yield return eff;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private unsafe ActionEffect Build(int index)
    {
        fixed (ulong* p = _effects)
            return *(ActionEffect*)(p + index);
    }
}

public static class ActionEffectParser
{
    public static string DescribeFields(ActionEffect eff)
    {
        // note: for all effects, bit 7 of param4 means "applied to source instead of target"
        // note: for all effects, bit 5 of param4 means "originate from target instead of source"
        var res = new StringBuilder();
        switch (eff.Type)
        {
            case ActionEffectType.Damage:
            case ActionEffectType.BlockedDamage:
            case ActionEffectType.ParriedDamage:
                // param0: bits0-4 (0x1F) = some kind of vfx, bit5 = crit, bit6 = direct hit
                // param1: damage/element type
                // param2: bonus percent in log (purely visual)
                // param3: third (high) byte of value (if bit 6 in param4 is set), 0 otherwise
                // param4: bit1 = ? (seen when part of damage is absorbed by BLM manaward), bit2 = partial absorb? (seen when part of damage is absorbed by SMN succor), bit4 = immune (e.g. because of transcendent after raise),
                //         bit5 = originating from target (e.g. retaliation damage from vengeance), bit 6 = large value, bit 7 = applied to source, others unused
                res.Append($"amount={eff.DamageHealValue} {eff.DamageType} {eff.DamageElement} ({(sbyte)eff.Param2}% bonus)");
                if ((eff.Param0 & 0x20) != default)
                    res.Append(", crit");
                if ((eff.Param0 & 0x40) != default)
                    res.Append(", dhit");
                if ((eff.Param4 & 2) != default)
                    res.Append(", manaward absorb?");
                if ((eff.Param4 & 4) != default)
                    res.Append(", partially absorbed?");
                if ((eff.Param4 & 0x10) != default)
                    res.Append(", immune");
                break;
            case ActionEffectType.Heal:
                // param0: bit0 = lifedrain? (e.g. melee bloodbath, SCH energy drain, etc. - also called "absorb"), bit1 = nascent flash?, others unused
                // param1: bit5 = crit, others unused
                // param2: unused
                // param3: third (high) byte of value (if bit 6 in param4 is set), 0 otherwise
                // param4: bit 6 = large value, bit 7 = applied to source, others unused
                res.Append($"amount={eff.DamageHealValue}");
                if ((eff.Param1 & 0x20) != default)
                    res.Append(", crit");
                if ((eff.Param0 & 1) != default)
                    res.Append(", lifedrain?");
                if ((eff.Param0 & 2) != default)
                    res.Append(", nascent flash?");
                break;
            case ActionEffectType.Invulnerable:
                // value: either 0 or status id
                if (eff.Value != default)
                    res.Append($"status {Utils.StatusString(eff.Value)}");
                break;
            case ActionEffectType.MpGain:
            case ActionEffectType.TpGain:
                res.Append($"amount={eff.Value}");
                break;
            case ActionEffectType.ApplyStatusEffectTarget:
            case ActionEffectType.ApplyStatusEffectSource:
                // param0/1: ??? (seen full range of values)
                // param2: low byte of 'extra' (don't know where high byte is...)
                // param3: unused
                // param4: bit5 = retaliation, bit 7 = applied to source, others unused
                res.Append($"{Utils.StatusString(eff.Value)} (xx{eff.Param2:X2})");
                break;
            case ActionEffectType.RecoveredFromStatusEffect:
                // param0: low byte of 'extra' (don't know where high byte is...)
                // param1-4: unused (except source bit in param4)
                res.Append($"{Utils.StatusString(eff.Value)} (xx{eff.Param0:X2})");
                break;
            case ActionEffectType.LoseStatusEffectTarget:
            case ActionEffectType.LoseStatusEffectSource:
            case ActionEffectType.StatusNoEffect:
                res.Append(Utils.StatusString(eff.Value));
                break;
            case ActionEffectType.StartActionCombo:
                res.Append($"aid={eff.Value}");
                break;
            case ActionEffectType.Knockback:
                var kbData = Service.LuminaRow<Lumina.Excel.Sheets.Knockback>(eff.Value);
                res.Append($"row={eff.Value}, dist={kbData?.Distance}+{eff.Param0}, dir={(KnockbackDirection?)kbData?.Direction}{(kbData?.Direction == (byte)KnockbackDirection.Arg ? $" ({kbData?.DirectionArg}deg)" : "")}, speed={kbData?.Speed}");
                break;
            case ActionEffectType.Attract1:
            case ActionEffectType.Attract2:
                var attrData = Service.LuminaRow<Lumina.Excel.Sheets.Attract>(eff.Value);
                res.Append($"row={eff.Value}, dist<={attrData?.MaxDistance} up to {attrData?.MinRemainingDistance} between {(attrData?.UseDistanceBetweenHitboxes == true ? "hitboxes" : "centers")}, dir={attrData?.Direction}, speed={attrData?.Speed}");
                break;
            case ActionEffectType.AttractCustom1:
            case ActionEffectType.AttractCustom2:
            case ActionEffectType.AttractCustom3:
                res.Append($"dist={eff.Value} (min={eff.Param1}), speed={eff.Param0}");
                break;
            case ActionEffectType.Mount:
                res.Append($"{eff.Value} '{Service.LuminaRow<Lumina.Excel.Sheets.Mount>(eff.Value)?.Singular}'");
                break;
            case ActionEffectType.FullResistStatus:
                res.Append(Utils.StatusString(eff.Value));
                break;
            case ActionEffectType.Resource:
                switch ((ActionResourceType)eff.Value)
                {
                    case ActionResourceType.WARGauge:
                        res.Append($"WAR Gauge: {eff.Param0}");
                        break;
                }
                break;
            case ActionEffectType.SetHP:
                res.Append($"value={eff.Value}");
                break;
        }
        return res.ToString();
    }

    public static string DescribeUnknown(ActionEffect eff)
    {
        switch (eff.Type)
        {
            case ActionEffectType.Miss:
            case ActionEffectType.FullResist:
            case ActionEffectType.FailMissingStatus: // e.g. deployment tactics or bane when target doesn't have required status
            case ActionEffectType.Interrupt:
                // so far never seen any non-zero params
                return eff.Param0 != default || eff.Param1 != default || eff.Param2 != default || eff.Param3 != default || eff.Param4 != default || eff.Value != default ? "non-zero params" : "";
            case ActionEffectType.NoEffectText: // e.g. taunt immune
                // so far never seen any non-zero params, except for 'source' flag
                return eff.Param0 != default || eff.Param1 != default || eff.Param2 != default || eff.Param3 != default || (eff.Param4 & ~0x80) != default || eff.Value != default ? "non-zero params" : "";
            case ActionEffectType.Damage:
            case ActionEffectType.BlockedDamage:
            case ActionEffectType.ParriedDamage:
                return (eff.Param0 & ~0x60) != default ? $"param0={eff.Param0 & ~0x60:X2}"
                    : eff.Param3 != default && (eff.Param4 & 0x40) == default ? "non-zero param3 while large-value bit is unset"
                    : (eff.Param4 & ~0xF0) != default ? $"param4={eff.Param4 & ~0xF0:X2}"
                    : (eff.Param4 & 0x10) != default && eff.Value != default ? $"immune bit set but value is non-zero"
                    : "";
            case ActionEffectType.Heal:
                return (eff.Param0 & ~3) != default ? $"param0={eff.Param0 & ~3:X2}"
                    : (eff.Param1 & ~0x20) != default ? $"param1={eff.Param1 & ~0x20:X2}"
                    : eff.Param2 != default ? $"param2={eff.Param2}"
                    : eff.Param3 != default && (eff.Param4 & 0x40) == default ? "non-zero param3 while large-value bit is unset"
                    : (eff.Param4 & ~0xC0) != 0 ? $"param4={eff.Param4 & ~0xC0:X2}"
                    : eff.Param0 != default && (eff.Param4 & 0x80) == default ? "lifedrain bits set while source bit is unset"
                    : "";
            case ActionEffectType.Invulnerable:
            case ActionEffectType.MpGain:
            case ActionEffectType.TpGain:
            case ActionEffectType.StartActionCombo:
            case ActionEffectType.FullResistStatus:
            case ActionEffectType.SetHP:
                // so far only seen 'source' flag and non-zero values
                return eff.Param0 != default || eff.Param1 != default || eff.Param2 != default || eff.Param3 != default || (eff.Param4 & ~0x80) != default ? "non-zero params" : "";
            case ActionEffectType.ApplyStatusEffectTarget:
            case ActionEffectType.ApplyStatusEffectSource:
                if (eff.Param3 != default || (eff.Param4 & ~0xA0) != default)
                    return "non-zero param3/4";
                else
                    return "TODO investigate param0/1";// $"{Utils.StatusString(eff.Value)} {eff.Param0:X2}{eff.Param1:X2}"; - these are often non-zero, but I have no idea what they mean...
            case ActionEffectType.RecoveredFromStatusEffect:
            case ActionEffectType.LoseStatusEffectTarget:
            case ActionEffectType.LoseStatusEffectSource:
            case ActionEffectType.StatusNoEffect:
                if (eff.Param1 != default || eff.Param2 != default || eff.Param3 != default || (eff.Param4 & ~0x80) != default)
                    return "non-zero params";
                else if (eff.Param0 != default)
                    return $"param0={eff.Param0}"; // this has some meaning, TODO investigate
                else
                    return "";
            case ActionEffectType.ThreatPosition:
            case ActionEffectType.EnmityAmountUp:
                if (eff.Param0 != default || eff.Param1 != default || eff.Param2 != default || eff.Param3 != default || eff.Param4 != default)
                    return "non-zero params";
                else
                    return $"value={eff.Value}"; // this has some meaning, TODO investigate
            case ActionEffectType.Retaliation:
                if (eff.Param1 != default || eff.Param2 != default || eff.Param3 != default || eff.Param4 != default)
                    return "non-zero params";
                else
                    return $"param0={eff.Param0}, value={eff.Value}"; // this has some meaning, TODO investigate
            case ActionEffectType.Knockback:
                return eff.Param1 != default || eff.Param2 != default || eff.Param3 != default || eff.Param4 != default ? "non-zero params" : "";
            case ActionEffectType.Attract1:
            case ActionEffectType.Attract2:
                return eff.Param0 != default || eff.Param1 != default || eff.Param2 != default || eff.Param3 != default || eff.Param4 != default ? "non-zero params" : "";
            case ActionEffectType.AttractCustom1:
            case ActionEffectType.AttractCustom2:
            case ActionEffectType.AttractCustom3:
                return eff.Param2 != default || eff.Param3 != default || eff.Param4 != default ? "non-zero params" : "";
            case ActionEffectType.Mount:
                if (eff.Param1 != default || eff.Param2 != default || eff.Param3 != default || eff.Param4 != default)
                    return "non-zero params";
                else
                    return $"param0={eff.Param0}"; // 0 or 1, TODO investigate
            case ActionEffectType.ReviveLB:
                return eff.Param0 != default || eff.Param1 != default || eff.Param2 != default || eff.Param3 != default || eff.Param4 != default || eff.Value != 1 ? "unknown payload" : "";
            case ActionEffectType.Resource:
                return (ActionResourceType)eff.Value switch
                {
                    ActionResourceType.WARGauge => eff.Param1 != default || eff.Param2 != default || eff.Param3 != default || eff.Param4 != default ? "non-zero params" : "",
                    _ => $"unknown value {eff.Value}",
                };
            default:
                return $"unknown type";
        }
    }
}
