﻿namespace BossMod;

// allowed categories of targets for an action
[Flags]
public enum ActionTargets
{
    None = 0,
    Self = 1 << 0,
    Party = 1 << 1,
    Alliance = 1 << 2,
    Hostile = 1 << 3,
    Friendly = 1 << 4,
    OwnPet = 1 << 5,
    PartyPet = 1 << 6,
    Area = 1 << 7,
    Dead = 1 << 8,

    All = (1 << 9) - 1,
}

// some debuffs prevent specific categories of action - amnesia, silence, pacification, etc
public enum ActionCategory : byte
{
    None,
    Autoattack,
    Spell,
    Weaponskill,
    Ability,
    Item,
    DoLAbility,
    DoHAbility,
    Event,
    LimitBreak9,
    System10,
    System11,
    Mount,
    Special,
    ItemManipulation,
    LimitBreak15,
    Unk1,
    Artillery,
    Unk2
}

// used for BLM calculations and possibly BLU optimization
public enum ActionAspect : byte
{
    None,
    Fire,
    Ice,
    Wind,
    Earth,
    Thunder,
    Water,
    Physical
}

// this contains all information about player actions that we care about (for action tweaks, autorotation, etc)
// some of the data is available in sheets, however unfortunately quite a bit is hardcoded in game functions; it often uses current player data
// however, we need this information outside game (ie in uidev) and for different players of different classes/levels (ie for replay analysis)
// because of that, we need to reimplement a lot of the logic here - this has to be synchronized to the code whenever game changes
public sealed record class ActionDefinition(ActionID ID)
{
    public delegate bool ConditionDelegate(WorldState ws, Actor player, ActionQueue.Entry action, AIHints hints);
    public delegate Actor? SmartTargetDelegate(WorldState ws, Actor player, Actor? target, AIHints hints);
    public delegate Angle? TransformAngleDelegate(WorldState ws, Actor player, Actor? target, AIHints hints);

    public BitMask AllowedClasses = new(~0ul);
    public int MinLevel;
    public uint UnlockLink;
    public ActionTargets AllowedTargets;
    public float Range; // 0 for self-targeted abilities
    public float CastTime; // 0 for instant-cast; can be adjusted by a number of factors (TODO: add functor)
    public ActionCategory Category;
    public int MainCooldownGroup = -1;
    public int ExtraCooldownGroup = -1;
    public float Cooldown; // for single charge (if multi-charge action); can be adjusted by a number of factors (TODO: add functor)
    public ActionAspect Aspect; // useful for BLM and BLU
    public int MaxChargesBase; // baseline max-charges when action is unlocked
    public readonly List<(int Charges, int Level, uint UnlockLink)> MaxChargesOverride = []; // trait overrides for max-charges (applied in order)
    public bool IsRoleAction; // unlocked conditions are different for these
    public float InstantAnimLock = 0.6f; // animation lock if ability is instant-cast
    public float CastAnimLock = 0.1f; // animation lock if ability is non-instant
    public ConditionDelegate? ForbidExecute; // optional condition, if it returns true, action is not executed
    public SmartTargetDelegate? SmartTarget; // optional target transformation for 'smart targeting' feature
    public TransformAngleDelegate? TransformAngle; // optional facing angle transformation

    // note: this does *not* include quest-locked overrides
    // the way game works is - when you use first charge, total is set to cd*max-at-cap, and elapsed is set to cd*(max-at-level - 1)
    public int MaxChargesAtCap()
    {
        foreach (ref var o in MaxChargesOverride.AsSpan())
            if (LinkUnlocked(o.UnlockLink))
                return o.Charges;
        return MaxChargesBase;
    }

    public int MaxChargesAtLevel(int level)
    {
        foreach (ref var o in MaxChargesOverride.AsSpan())
            if (level >= o.Level && LinkUnlocked(o.UnlockLink))
                return o.Charges;
        return MaxChargesBase;
    }

    public bool IsMultiCharge => MaxChargesAtCap() > 1;
    public float CooldownAtFirstCharge => (MaxChargesAtCap() - 1) * Cooldown;
    public bool IsGCD => MainCooldownGroup == ActionDefinitions.GCDGroup || ExtraCooldownGroup == ActionDefinitions.GCDGroup;

    // for duty actions, the action definition always stores cooldown group 80, but in reality a different one might be used
    // note that this doesn't apply to phantom actions, which use the cdgroups 82-86
    public int ActualMainCooldownGroup(ReadOnlySpan<ClientState.DutyAction> dutyActions)
        => MainCooldownGroup == ActionDefinitions.DutyAction0CDGroup && dutyActions[0].Action != ID && dutyActions[1].Action == ID
            ? ActionDefinitions.DutyAction1CDGroup
            : MainCooldownGroup;

    // for multi-charge abilities, action is ready when elapsed >= single-charge cd; assume that if any multi-charge actions share cooldown group, they have same cooldown - otherwise dunno how it should work
    // note: GCDs with multiple charges can be affected OR unaffected by haste depending on how many charges the user currently has access to
    // the only separate-cooldown GCD that increases to >1 charge via trait is currently MCH Drill; others have multiple charges at unlock - SGE Phlegma, RPR Soul Slice, BLU Surpanakha
    public float MainReadyIn(ReadOnlySpan<Cooldown> cooldowns, ReadOnlySpan<ClientState.DutyAction> dutyActions)
    {
        if (MainCooldownGroup < 0)
            return 0;
        var cdg = cooldowns[ActualMainCooldownGroup(dutyActions)];
        return cdg.Total > 0 ? Math.Max(0, cdg.Total / MaxChargesAtCap() - cdg.Elapsed) : 0;
    }

    public float ExtraReadyIn(ReadOnlySpan<Cooldown> cooldowns) => ExtraCooldownGroup >= 0 ? cooldowns[ExtraCooldownGroup].Remaining : 0;
    public float ReadyIn(ReadOnlySpan<Cooldown> cooldowns, ReadOnlySpan<ClientState.DutyAction> dutyActions) => Math.Max(MainReadyIn(cooldowns, dutyActions), ExtraReadyIn(cooldowns));

    // return time until charges are capped (for multi-charge abilities; for single-charge this is equivalent to remaining cooldown)
    public float ChargeCapIn(ReadOnlySpan<Cooldown> cooldowns, ReadOnlySpan<ClientState.DutyAction> dutyActions, int level)
    {
        if (MainCooldownGroup < 0)
            return 0;
        var cdg = cooldowns[ActualMainCooldownGroup(dutyActions)];
        return cdg.Total > 0 ? Math.Max(0, MaxChargesAtLevel(level) * cdg.Total / MaxChargesAtCap() - cdg.Elapsed) : 0;
    }

    public bool IsUnlocked(WorldState ws, Actor player)
    {
        var checkLevel = IsRoleAction ? ws.Client.ClassJobLevel(player.Class) : player.Level;
        return AllowedClasses[(int)player.Class] && checkLevel >= MinLevel && LinkUnlocked(UnlockLink);
    }

    private static bool LinkUnlocked(uint link) => link == 0 || (ActionDefinitions.Instance.UnlockCheck?.Invoke(link) ?? true);
}

// database of all supported player-initiated actions
// note that it is associated to a specific worldstate, so that it can be used for things like action conditions
public sealed class ActionDefinitions : IDisposable
{
    private static readonly ActionTweaksConfig _config = Service.Config.Get<ActionTweaksConfig>();

    private readonly Lumina.Excel.ExcelSheet<Lumina.Excel.Sheets.Action> _actionsSheet = Service.LuminaSheet<Lumina.Excel.Sheets.Action>()!;
    private readonly Lumina.Excel.ExcelSheet<Lumina.Excel.Sheets.Item> _itemsSheet = Service.LuminaSheet<Lumina.Excel.Sheets.Item>()!;
    private readonly Lumina.Excel.ExcelSheet<Lumina.Excel.RawRow> _cjcSheet = Service.LuminaGameData!.Excel.GetSheet<Lumina.Excel.RawRow>(null, "ClassJobCategory")!;
    private readonly Lumina.Excel.ExcelSheet<Lumina.Excel.Sheets.Trait> _traitSheet = Service.LuminaSheet<Lumina.Excel.Sheets.Trait>()!;
    private readonly List<IDisposable> _classDefinitions;
    private readonly Dictionary<ActionID, ActionDefinition> _definitions = [];

    public IEnumerable<ActionDefinition> Definitions => _definitions.Values;
    public ActionDefinition? this[ActionID action] => _definitions.GetValueOrDefault(action);
    public ActionDefinition? Spell<AID>(AID aid) where AID : Enum => _definitions.GetValueOrDefault(ActionID.MakeSpell(aid));

    public const int GCDGroup = 57;
    public const int PotionCDGroup = 58;
    public const int DutyAction0CDGroup = 80;
    public const int DutyAction1CDGroup = 81;

    public static readonly ActionID IDSprint = new(ActionType.Spell, 3);
    public static readonly ActionID IDAutoAttack = new(ActionType.Spell, 7);
    public static readonly ActionID IDAutoShot = new(ActionType.Spell, 8);
    public static readonly ActionID IDPotionStr = new(ActionType.Item, 1045995); // hq grade 3 gemdraught of strength
    public static readonly ActionID IDPotionDex = new(ActionType.Item, 1045996); // hq grade 3 gemdraught of dexterity
    public static readonly ActionID IDPotionVit = new(ActionType.Item, 1045997); // hq grade 3 gemdraught of vitality
    public static readonly ActionID IDPotionInt = new(ActionType.Item, 1045998); // hq grade 3 gemdraught of intelligence
    public static readonly ActionID IDPotionMnd = new(ActionType.Item, 1045999); // hq grade 3 gemdraught of mind

    // content specific consumables
    public static readonly ActionID IDPotionSustaining = new(ActionType.Item, 20309);
    public static readonly ActionID IDPotionMax = new(ActionType.Item, 1013637);
    public static readonly ActionID IDPotionEmpyrean = new(ActionType.Item, 23163);
    public static readonly ActionID IDPotionSuper = new(ActionType.Item, 1023167);
    public static readonly ActionID IDPotionOrthos = new(ActionType.Item, 38944);
    public static readonly ActionID IDPotionHyper = new(ActionType.Item, 1038956);
    public static readonly ActionID IDPotionEureka = new(ActionType.Item, 22306);

    // special general actions that we support
    public static readonly ActionID IDGeneralLimitBreak = new(ActionType.General, 3);
    public static readonly ActionID IDGeneralSprint = new(ActionType.General, 4);
    public static readonly ActionID IDGeneralDuty1 = new(ActionType.General, 26);
    public static readonly ActionID IDGeneralDuty2 = new(ActionType.General, 27);

    public static readonly ActionDefinitions Instance = new();

    public Func<uint, bool>? UnlockCheck;

    private ActionDefinitions()
    {
        _classDefinitions = [
            new ClassShared.Definitions(this),
            new PLD.Definitions(this),
            new WAR.Definitions(this),
            new DRK.Definitions(this),
            new GNB.Definitions(this),
            new WHM.Definitions(this),
            new SCH.Definitions(this),
            new AST.Definitions(this),
            new SGE.Definitions(this),
            new MNK.Definitions(this),
            new DRG.Definitions(this),
            new NIN.Definitions(this),
            new SAM.Definitions(this),
            new RPR.Definitions(this),
            new BRD.Definitions(this),
            new MCH.Definitions(this),
            new DNC.Definitions(this),
            new BLM.Definitions(this),
            new SMN.Definitions(this),
            new RDM.Definitions(this),
            new BLU.Definitions(this),
            new PCT.Definitions(this),
            new VPR.Definitions(this),
            new Roleplay.Definitions(this),
        ];

        // items (TODO: more generic approach is needed...)
        RegisterPotion(IDPotionStr);
        RegisterPotion(IDPotionDex);
        RegisterPotion(IDPotionVit);
        RegisterPotion(IDPotionInt);
        RegisterPotion(IDPotionMnd);

        RegisterPotion(IDPotionSustaining, 1.1f);
        RegisterPotion(IDPotionMax, 1.1f);
        RegisterPotion(IDPotionEmpyrean, 1.1f);
        RegisterPotion(IDPotionSuper, 1.1f);
        RegisterPotion(IDPotionOrthos, 1.1f);
        RegisterPotion(IDPotionHyper, 1.1f);
        RegisterPotion(IDPotionEureka, 1.1f);

        // special content actions - bozja, deep dungeons, etc
        for (var i = BozjaHolsterID.None + 1; i < BozjaHolsterID.Count; ++i)
            RegisterBozja(i);
        for (var i = PomanderID.Safety; i < PomanderID.Count; ++i)
            RegisterDeepDungeon(new(ActionType.Pomander, (uint)i));
        for (var i = 1u; i <= 3; ++i)
            RegisterDeepDungeon(new(ActionType.Magicite, i));

        foreach (var act in typeof(EurekaActionID).GetEnumValues())
            if ((uint)act > 0)
                RegisterSpell((EurekaActionID)act);

        for (var i = 1u; i <= 6u; ++i)
        {
            var petAction = new ActionID(ActionType.PetAction, i);
            var def = new ActionDefinition(petAction)
            {
                CastAnimLock = 0,
                InstantAnimLock = 0,
            };
            if (i == 3) // PetAction 3 "Place" is area-targeted
            {
                def.Range = 30;
                def.AllowedTargets = ActionTargets.Area;
            }
            Register(def.ID, def);
        }
    }

    public void Dispose()
    {
        foreach (var c in _classDefinitions)
            c.Dispose();
    }

    // smart targeting utility: return target (if friendly) or null (otherwise)
    public static Actor? SmartTargetFriendly(Actor? primaryTarget) => (primaryTarget?.IsAlly ?? false) ? primaryTarget : null;

    // smart targeting utility: return target (if friendly) or other tank (if available) or null (otherwise)
    public static Actor? FindCoTank(WorldState ws, Actor player) => ws.Party.WithoutSlot().Exclude(player).FirstOrDefault(a => a.Role == Role.Tank);
    public static Actor? SmartTargetCoTank(WorldState ws, Actor player, Actor? primaryTarget, AIHints hints) => SmartTargetFriendly(primaryTarget) ?? FindCoTank(ws, player);

    // smart targeting utility: return target (if friendly) or any esunable player (if any) or self (otherwise)
    public static Actor? FindEsunaTarget(WorldState ws) => ws.Party.WithoutSlot().FirstOrDefault(p => p.Statuses.Any(s => Utils.StatusIsRemovable(s.ID)));
    public static Actor? SmartTargetEsunable(WorldState ws, Actor player, Actor? primaryTarget, AIHints hints) => SmartTargetFriendly(primaryTarget) ?? FindEsunaTarget(ws) ?? player;

    public static bool DashToTargetCheck(WorldState _, Actor player, ActionQueue.Entry action, AIHints hints)
    {
        var target = action.Target;
        if (target == null || !_config.DashSafety)
            return false;

        // if there are pending knockbacks, god only knows where we would be sent after using a gapcloser
        // note that once the knockback is actually active and not pending, we can probably cancel it with a dash
        if (player.PendingKnockbacks.Count > 0)
            return true;

        var dist = player.DistanceToHitbox(target);
        var dir = player.DirectionTo(target).Normalized();
        var src = player.Position;

        return IsDashDangerous(src, src + dir * MathF.Max(0, dist), hints);
    }

    public static bool DashToPositionCheck(WorldState _, Actor player, ActionQueue.Entry action, AIHints hints)
    {
        if (action.TargetPos == default || !_config.DashSafety || !_config.DashSafetyExtra)
            return false;

        if (player.PendingKnockbacks.Count > 0)
            return true;

        return IsDashDangerous(player.Position, new WPos(action.TargetPos.XZ()), hints);
    }

    public static ActionDefinition.ConditionDelegate DashFixedDistanceCheck(float range, bool backwards = false)
        => (ws, player, act, hints) =>
        {
            if (!_config.DashSafety || !_config.DashSafetyExtra)
                return false;

            if (player.PendingKnockbacks.Count != 0)
                return true;

            var dir = act.FacingAngle ?? player.Rotation;

            var dest = player.Position + dir.ToDirection() * range * (backwards ? -1f : 1f);

            return IsDashDangerous(player.Position, dest, hints);
        };

    public static ActionDefinition.ConditionDelegate BackdashCheck(float range)
         => (ws, player, act, hints) =>
        {
            if (act.Target == null || !_config.DashSafety || !_config.DashSafetyExtra)
                return false;

            if (player.PendingKnockbacks.Count > 0)
                return true;

            var dir = act.Target.DirectionTo(player).Normalized();

            return IsDashDangerous(player.Position, player.Position + dir * range, hints);
        };

    // check if dashing to target will put the player inside a forbidden zone
    public static bool IsDashDangerous(WPos from, WPos to, AIHints hints)
    {
        var center = hints.PathfindMapCenter;
        if (!hints.PathfindMapBounds.Contains(to - center))
            return true;

        // if arena is a weird shape, try to ensure player won't dash out of it
        if (from != to && hints.PathfindMapBounds is ArenaBoundsCustom && hints.PathfindMapBounds.IntersectRay(from - center, to - from) is >= 0 and < float.MaxValue)
            return true;

        return hints.ForbiddenZones.Any(d => d.shapeDistance(to) < 0f);
    }

    public BitMask SpellAllowedClasses(Lumina.Excel.Sheets.Action data)
    {
        BitMask res = default;
        var cjc = _cjcSheet?.GetRowOrDefault(data.ClassJobCategory.RowId);
        if (cjc != null)
            for (var i = 1; i < _cjcSheet!.Columns.Count; ++i)
                res[i - 1] = cjc.Value.ReadBoolColumn(i);
        return res;
    }
    public BitMask SpellAllowedClasses(uint spellId) => SpellAllowedClasses(ActionData(spellId));
    public BitMask ActionAllowedClasses(ActionID aid) => aid.Type == ActionType.Spell ? SpellAllowedClasses(aid.ID) : new(~0ul);

    public int SpellMinLevel(Lumina.Excel.Sheets.Action data) => data.ClassJobLevel;
    public int SpellMinLevel(uint spellId) => SpellMinLevel(ActionData(spellId));
    public int ActionMinLevel(ActionID aid) => aid.Type == ActionType.Spell ? SpellMinLevel(aid.ID) : 0;

    public uint SpellUnlockLink(Lumina.Excel.Sheets.Action data) => data.UnlockLink.RowId;
    public uint SpellUnlockLink(uint spellId) => SpellUnlockLink(ActionData(spellId));
    public uint ActionUnlockLink(ActionID aid) => aid.Type == ActionType.Spell ? SpellUnlockLink(aid.ID) : 0;

    // see ActionManager.CanUseActionOnTarget
    public ActionTargets SpellAllowedTargets(Lumina.Excel.Sheets.Action data)
    {
        var res = ActionTargets.None;
        if (data.CanTargetSelf)
            res |= ActionTargets.Self;
        if (data.CanTargetParty)
            res |= ActionTargets.Party;
        if (data.CanTargetAlliance)
            res |= ActionTargets.Alliance;
        if (data.CanTargetHostile)
            res |= ActionTargets.Hostile;
        if (data.CanTargetAlly)
            res |= ActionTargets.Friendly;
        if (data.CanTargetOwnPet)
            res |= ActionTargets.OwnPet;
        if (data.CanTargetPartyPet)
            res |= ActionTargets.PartyPet;
        if (data.TargetArea)
            res |= ActionTargets.Area;
        if (data.DeadTargetBehaviour == 1)
            res |= ActionTargets.Dead;
        return res;
    }
    public ActionTargets SpellAllowedTargets(uint spellId) => SpellAllowedTargets(ActionData(spellId));
    public ActionTargets ActionAllowedTargets(ActionID aid) => SpellAllowedTargets(aid.SpellId());

    // see ActionManager.GetActionRange
    // note that actions with range == -1 use data from equipped weapon; currently all weapons for phys-ranged classes have range 25, others have range 3
    public int SpellRange(Lumina.Excel.Sheets.Action data, bool isPhysRanged = false)
    {
        if ((SCH.AID)data.RowId is SCH.AID.PetEmbrace or SCH.AID.PetFeyUnion or SCH.AID.PetSeraphicVeil)
            return 30; // these are hardcoded
        var range = data.Range;
        return range >= 0 ? range : isPhysRanged ? 25 : 3;
    }
    public int SpellRange(uint spellId, bool isPhysRanged = false) => SpellRange(ActionData(spellId), isPhysRanged);
    public int ActionRange(ActionID aid, bool isPhysRanged = false) => SpellRange(aid.SpellId(), isPhysRanged);

    // see ActionManager.GetCastTimeAdjusted
    public float SpellBaseCastTime(Lumina.Excel.Sheets.Action data) => data.Cast100ms * 0.1f;
    public float SpellBaseCastTime(uint spellId) => SpellBaseCastTime(ActionData(spellId));
    public float ActionBaseCastTime(ActionID aid) => aid.Type switch
    {
        ActionType.Item => ItemData(aid.ID).CastTimeSeconds/* ?? 2*/,
        ActionType.KeyItem => Service.LuminaRow<Lumina.Excel.Sheets.EventItem>(aid.ID)?.CastTime ?? 0,
        ActionType.Spell or ActionType.Mount or ActionType.Ornament => SpellBaseCastTime(aid.SpellId()),
        _ => 0
    };

    public int SpellMainCDGroup(Lumina.Excel.Sheets.Action data) => data.CooldownGroup - 1;
    public int SpellMainCDGroup(uint spellId) => SpellMainCDGroup(ActionData(spellId));
    public int ActionMainCDGroup(ActionID aid) => SpellMainCDGroup(aid.SpellId());

    public int SpellExtraCDGroup(Lumina.Excel.Sheets.Action data) => data.AdditionalCooldownGroup - 1;
    public int SpellExtraCDGroup(uint spellId) => SpellExtraCDGroup(ActionData(spellId));
    public int ActionExtraCDGroup(ActionID aid) => aid.Type == ActionType.Spell ? SpellExtraCDGroup(aid.ID) : -1; // see ActionManager.GetAdditionalRecastGroup - always -1 for non-spells

    // see ActionManager.GetAdjustedRecastTime
    public float SpellBaseCooldown(Lumina.Excel.Sheets.Action data) => data.Recast100ms * 0.1f;
    public float SpellBaseCooldown(uint spellId) => SpellBaseCooldown(ActionData(spellId));
    public float ActionBaseCooldown(ActionID aid) => aid.Type switch
    {
        ActionType.Spell => SpellBaseCooldown(aid.ID),
        ActionType.Item => ItemData(aid.ID).Cooldowns * (aid.ID > 1000000u ? 0.9f : 1.0f) /*?? 5*/,
        _ => 5,
    };

    public ActionAspect SpellAspect(Lumina.Excel.Sheets.Action data) => BossMod.ActionAspect.None;
    public ActionAspect SpellAspect(uint spellId) => SpellAspect(ActionData(spellId));
    public ActionAspect ActionAspect(ActionID aid) => aid.Type switch
    {
        ActionType.Spell => SpellAspect(aid.ID),
        _ => 0
    };

    // the vast majority of chargeless actions have MaxCharges=0, even though they logically have 1
    public int SpellBaseMaxCharges(Lumina.Excel.Sheets.Action data) => Math.Max(data.MaxCharges, (byte)1);
    public int SpellBaseMaxCharges(uint spellId) => SpellBaseMaxCharges(ActionData(spellId));
    public int ActionBaseMaxCharges(ActionID aid) => SpellBaseMaxCharges(aid.SpellId());

    private Lumina.Excel.Sheets.Action ActionData(uint id) => _actionsSheet.GetRow(id);
    private Lumina.Excel.Sheets.Item ItemData(uint id) => _itemsSheet.GetRow(id % 500000);
    private Lumina.Excel.Sheets.Trait TraitData(uint id) => _traitSheet.GetRow(id);

    // registration for different kinds of actions
    public void RegisterSpell(ActionID aid, bool isPhysRanged = false, float instantAnimLock = 0.6f, float castAnimLock = 0.1f)
    {
        var data = ActionData(aid.ID);
        var def = new ActionDefinition(aid)
        {
            AllowedClasses = SpellAllowedClasses(data),
            MinLevel = SpellMinLevel(data),
            UnlockLink = SpellUnlockLink(data),
            AllowedTargets = SpellAllowedTargets(data),
            Range = SpellRange(data, isPhysRanged),
            CastTime = SpellBaseCastTime(data),
            MainCooldownGroup = SpellMainCDGroup(data),
            ExtraCooldownGroup = SpellExtraCDGroup(data),
            Cooldown = SpellBaseCooldown(data),
            Aspect = SpellAspect(data),
            MaxChargesBase = SpellBaseMaxCharges(data),
            InstantAnimLock = instantAnimLock,
            CastAnimLock = castAnimLock,
            IsRoleAction = data.IsRoleAction,
            Category = (ActionCategory)data.ActionCategory.RowId
        };
        Register(aid, def);
    }

    public void RegisterSpell<AID>(AID aid, bool isPhysRanged = false, float instantAnimLock = 0.6f, float castAnimLock = 0.1f) where AID : Enum
        => RegisterSpell(ActionID.MakeSpell(aid), isPhysRanged, instantAnimLock, castAnimLock);

    private void Register(ActionID aid, ActionDefinition definition) => _definitions.Add(aid, definition);

    private void RegisterPotion(ActionID aid, float animLock = 0.6f)
    {
        var baseId = aid.ID % 500000;
        var item = ItemData(baseId);
        var itemAction = item.ItemAction.Value;
        var spellId = itemAction.Type;
        var cdgroup = SpellMainCDGroup(spellId);
        float cooldown = item.Cooldowns;
        var targets = SpellAllowedTargets(spellId);
        var range = SpellRange(spellId);
        var castTime = item.CastTimeSeconds /*?? 2*/;
        var aidNQ = new ActionID(ActionType.Item, baseId);
        _definitions[aidNQ] = new(aidNQ)
        {
            AllowedTargets = targets,
            Range = range,
            CastTime = castTime,
            MainCooldownGroup = cdgroup,
            Cooldown = cooldown,
            InstantAnimLock = animLock,
        };
        var aidHQ = new ActionID(ActionType.Item, baseId + 1000000u);
        _definitions[aidHQ] = new(aidHQ)
        {
            AllowedTargets = targets,
            Range = range,
            CastTime = castTime,
            MainCooldownGroup = cdgroup,
            Cooldown = cooldown * 0.9f,
            InstantAnimLock = animLock
        };
    }

    private void RegisterBozja(BozjaHolsterID id)
    {
        var normalAction = BozjaActionID.GetNormal(id);
        var isItem = normalAction == BozjaActionID.GetHolster(id);
        RegisterSpell(normalAction, instantAnimLock: isItem ? 1.1f : 0.6f);
        if (!isItem)
        {
            var aid1 = ActionID.MakeBozjaHolster(id, 0);
            _definitions[aid1] = new(aid1) { AllowedTargets = ActionTargets.Self, InstantAnimLock = 2.1f };
            var aid2 = ActionID.MakeBozjaHolster(id, 1);
            _definitions[aid2] = new(aid2) { AllowedTargets = ActionTargets.Self, InstantAnimLock = 2.1f };
        }

        if (id == BozjaHolsterID.LostSeraphStrike)
            _definitions[normalAction].ForbidExecute = DashToTargetCheck;
    }

    private void RegisterDeepDungeon(ActionID id)
    {
        _definitions[id] = new(id) { AllowedTargets = ActionTargets.Self, InstantAnimLock = 2.1f };
    }

    // hardcoded mechanic implementations
    public void RegisterChargeIncreaseTrait(ActionID aid, uint traitId)
    {
        var trait = TraitData(traitId)!;
        _definitions[aid].MaxChargesOverride.Add((trait.Value, trait.Level, trait.Quest.RowId));
        _definitions[aid].MaxChargesOverride.Sort((b, a) => a.Level.CompareTo(b.Level));
    }
    public void RegisterChargeIncreaseTrait<AID, TraitID>(AID aid, TraitID traitId) where AID : Enum where TraitID : Enum => RegisterChargeIncreaseTrait(ActionID.MakeSpell(aid), (uint)(object)traitId);
}
