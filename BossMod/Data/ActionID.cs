namespace BossMod;

// matches FFXIVClientStructs.FFXIV.Client.Game.ActionType, with some custom additions
public enum ActionType : byte
{
    None = 0,
    Spell = 1, // CS renamed that to Action
    Item = 2,
    KeyItem = 3,
    Ability = 4,
    General = 5, // CS renamed that to GeneralAction
    Buddy = 6,
    MainCommand = 7,
    Companion = 8,
    CraftAction = 9,
    PetAction = 11,
    Mount = 13,
    PvPAction = 14,
    Waymark = 15, // CS renamed that to FieldMarker
    ChocoboRaceAbility = 16,
    ChocoboRaceItem = 17,
    SquadronAction = 19, // CS renamed that to BgcArmyAction
    Ornament = 20,

    // below are custom additions, these aren't proper actions from game's point of view, but it makes sense for us to treat them as such
    BozjaHolsterSlot0 = 0xE0, // id = BozjaHolsterID, use from holster to replace duty action 0
    BozjaHolsterSlot1 = 0xE1, // id = BozjaHolsterID, use from holster to replace duty action 1
    Pomander = 0xE2, // id = PomanderID
    Magicite = 0xE3, // id = slot (1-3)
    Crucible = 0xE4, // id = CrucibleID
}

public enum Positional { Any, Flank, Rear, Front }

// high byte is type, low 3 bytes is ID
public readonly struct ActionID(uint raw) : IEquatable<ActionID>
{
    public readonly uint Raw = raw;
    public readonly ActionType Type => (ActionType)(Raw >> 24);
    public readonly uint ID = raw & 0x00FFFFFFu;

    public ActionID(ActionType type, uint id) : this(((uint)type << 24) | id) { }

    public static implicit operator bool(ActionID x) => x.Raw != default;
    public static bool operator ==(ActionID left, ActionID right) => left.Raw == right.Raw;
    public static bool operator !=(ActionID left, ActionID right) => left.Raw != right.Raw;
    public override readonly string ToString() => $"{Type} {ID} '{Name()}'";
    private static readonly Dictionary<uint, (float, string)> _spellCache = [];

    public readonly AID As<AID>() where AID : Enum => (AID)(object)ID;

    public readonly string Name() => Type switch
    {
        ActionType.Spell => GetSpellData(SpellId()).Name,
        ActionType.Item => $"{Service.LuminaRow<Lumina.Excel.Sheets.Item>(ID % 1000000u)?.Name ?? "<not found>"}{(ID > 1000000u ? " (HQ)" : "")}", // see Dalamud.Game.Text.SeStringHandling.Payloads.GetAdjustedId; TODO: id > 500000 is "collectible", >2000000 is "event" ??
        ActionType.BozjaHolsterSlot0 or ActionType.BozjaHolsterSlot1 => $"{(BozjaHolsterID)ID}",
        ActionType.PetAction => Service.LuminaRow<Lumina.Excel.Sheets.PetAction>(ID)?.Name.ToString() ?? "<not found>",
        ActionType.Crucible => Service.LuminaRow<Lumina.Excel.Sheets.XBMItem>(CrucibleItemID.GetXBMRow((CrucibleID)ID))?.Unknown2.ToString() ?? "<not found>",
        _ => ""
    };

    // see ActionManager.GetSpellIdForAction
    public readonly uint SpellId() => Type switch
    {
        ActionType.Spell => ID,
        ActionType.Item => Service.LuminaRow<Lumina.Excel.Sheets.Item>(ID % 500000u)?.ItemAction.ValueNullable?.Action.RowId ?? default,
        ActionType.KeyItem => Service.LuminaRow<Lumina.Excel.Sheets.EventItem>(ID)?.Action.RowId ?? default,
        ActionType.Ability => 2u, // 'interaction'
        ActionType.General => Service.LuminaRow<Lumina.Excel.Sheets.GeneralAction>(ID)?.Action.RowId ?? default, // note: duty action 1/2 (26/27) use special code
        ActionType.Mount => 4u, // 'mount'
        ActionType.Ornament => 20061u, // 'accessorize'
        _ => default
    };

    public readonly float CastTime() => Type switch
    {
        ActionType.Spell => GetSpellData(SpellId()).ExtraCastTime,
        _ => default
    };

    public readonly float CastTimeExtra() => GetSpellData(SpellId()).ExtraCastTime;

    public readonly bool IsCasted() => CastTime() > 0f;

    public static ActionID MakeSpell<AID>(AID id) where AID : Enum
    {
        var castID = (uint)(object)id;
        return castID != default ? new(ActionType.Spell, castID) : default;
    }

    public static ActionID MakeBozjaHolster(BozjaHolsterID id, int slot) => slot switch
    {
        0 => new(ActionType.BozjaHolsterSlot0, (uint)id),
        1 => new(ActionType.BozjaHolsterSlot1, (uint)id),
        _ => default
    };

    private static (float ExtraCastTime, string Name) GetSpellData(uint actionID)
    {
        if (_spellCache.TryGetValue(actionID, out var actionRow))
        {
            return actionRow;
        }

        var row = Service.LuminaRow<Lumina.Excel.Sheets.Action>(actionID);
        (float, string)? data;

        data = (row!.Value.ExtraCastTime100ms * 0.1f, row.Value.Name.ToString());
        return _spellCache[actionID] = data!.Value;
    }

    public readonly bool Equals(ActionID other) => this == other;
    public override readonly bool Equals(object? obj) => obj is ActionID other && Equals(other);
    public override readonly int GetHashCode() => Raw.GetHashCode();
}
