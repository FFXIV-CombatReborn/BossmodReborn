using Dalamud.Bindings.ImGui;

namespace BossMod.ReplayVisualization;

public sealed class ColumnPlayerActions : Timeline.ColumnGroup
{
    private struct CooldownGroup
    {
        public ColumnGenericHistory? Column;
        public DateTime Cursor;
        public DateTime ChargeCooldownEnd;
        public int ChargesOnCooldown;
        public int MaxCharges;
        public float ChargeCooldown;
        public ActionID CooldownAction;
    }

    private readonly ColumnGenericHistory _autoAttacks;
    private readonly ColumnGenericHistory _animLocks;
    private readonly CooldownGroup[] _cdGroups = new CooldownGroup[ClientState.NumCooldownGroups];
    private readonly ColumnSeparator _sep;
    private readonly Dictionary<ActionID, (int group, float cd)> _cooldownReductions = [];

    public ColumnPlayerActions(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player, Class playerClass)
        : base(timeline)
    {
        _autoAttacks = Add<ColumnGenericHistory>(new(timeline, tree, phaseBranches, "Auto attacks"));
        _animLocks = Add<ColumnGenericHistory>(new(timeline, tree, phaseBranches, "Abilities with animation locks"));
        _sep = Add(new ColumnSeparator(timeline));
        GetCooldownColumn(ActionDefinitions.GCDGroup, new()).Name = "GCD"; // make sure GCD column always exists and is before any others
        SetupClass(playerClass);

        var iCast = 0;
        var minTime = enc.Time.Start.AddSeconds(timeline.MinTime);
        for (var ai = 0; ai < replay.Actions.Count; ++ai)
        {
            var a = replay.Actions[ai];
            if (a.Timestamp < minTime)
            {
                continue;
            }
            if (a.Timestamp > enc.Time.End)
            {
                break;
            }
            if (a.Source != player)
            {
                continue;
            }
            // note: we assume autoattacks are never casted... in fact, I think only GCDs can be casted
            var actionName = $"{a.ID} -> {ReplayUtils.ParticipantString(a.MainTarget, a.Timestamp)} #{a.GlobalSequence}";
            if (a.ID == ActionDefinitions.IDAutoAttack || a.ID == ActionDefinitions.IDAutoShot)
            {
                AddAnimationLock(_autoAttacks, a, enc.Time.Start, a.Timestamp, actionName);
                _autoAttacks.AddHistoryEntryDot(enc.Time.Start, a.Timestamp, actionName, Colors.TextColor18).AddActionTooltip(a);
                continue;
            }

            while (iCast < player.Casts.Count && player.Casts[iCast].Time.End < a.Timestamp)
            {
                // add cast without event (interrupted, cancelled, whatever)
                AddUnfinishedCast(player.Casts[iCast++], enc.Time.Start);
            }

            var actionDef = ActionDefinitions.Instance[a.ID];
            DateTime effectStart;
            if (iCast < player.Casts.Count && player.Casts[iCast].Time.Start < a.Timestamp && player.Casts[iCast].ID == a.ID)
            {
                // casted action
                var cast = player.Casts[iCast++];
                var castName = $"{cast.ID} -> {ReplayUtils.ParticipantString(cast.Target, cast.Time.Start)}";
                _animLocks.AddHistoryEntryRange(enc.Time.Start, cast.Time, castName, Colors.TextColor9).AddCastTooltip(cast);
                if (actionDef?.MainCooldownGroup >= 0)
                {
                    StartCooldown(a.ID, actionDef, enc.Time.Start, cast.Time.Start);
                    GetCooldownColumn(actionDef.MainCooldownGroup, a.ID).AddHistoryEntryRange(enc.Time.Start, cast.Time, castName, Colors.TextColor9).AddCastTooltip(cast);
                    AdvanceCooldown(actionDef.MainCooldownGroup, enc.Time.Start, cast.Time.End, false);
                }
                effectStart = cast.Time.End;
            }
            else
            {
                if (actionDef?.MainCooldownGroup >= 0)
                {
                    StartCooldown(a.ID, actionDef, enc.Time.Start, a.ClientAction != null ? a.ClientAction.Requested : a.Timestamp);
                }
                effectStart = a.ClientAction != null ? a.ClientAction.Requested : a.Timestamp; // TODO: different depending on config...
            }

            AddAnimationLock(_animLocks, a, enc.Time.Start, effectStart, actionName);
            _animLocks.AddHistoryEntryDot(enc.Time.Start, a.Timestamp, actionName, actionDef != null ? (ActionConfirmed(a) ? Colors.TextColor1 : Colors.TextColor2) : Colors.TextColor3).AddActionTooltip(a);

            if (actionDef?.MainCooldownGroup >= 0)
            {
                var col = GetCooldownColumn(actionDef.MainCooldownGroup, a.ID);
                float effectDuration = 0;
                List<string> effectTooltip = [];
                var targets = a.Targets;
                var count = targets.Count;
                for (var i = 0; i < count; ++i)
                {
                    var t = targets[i];
                    if (t.Target.Type is ActorType.Player or ActorType.Buddy)
                    {
                        var effects = t.Effects.ValidEffects();
                        var len = effects.Length;
                        for (var j = 0; j < len; ++j)
                        {
                            ref readonly var eff = ref effects[j];
                            if (eff.Type is ActionEffectType.ApplyStatusEffectTarget or ActionEffectType.ApplyStatusEffectSource)
                            {
                                var source = eff.FromTarget ? t.Target : a.Source;
                                var target = eff.Type == ActionEffectType.ApplyStatusEffectTarget && !eff.AtSource ? t.Target : a.Source;
                                var statuses = replay.Statuses;
                                Replay.Status? status = null;
                                var countS = statuses.Count;
                                for (var k = 0; k < countS; ++k)
                                {
                                    var s = statuses[k];
                                    if (s.ID == eff.Value && s.Target == target && s.Source == source)
                                    {
                                        status = s;
                                        break;
                                    }
                                }
                                var duration = (float)((status?.Time.End ?? a.Timestamp) - a.Timestamp).TotalSeconds;
                                var delay = ((status?.Time.Start ?? a.Timestamp) - a.Timestamp).TotalSeconds;
                                effectDuration = Math.Max(effectDuration, duration);
                                effectTooltip.Add($"- effect: {Utils.StatusString(eff.Value)}, duration={(status != null ? status.Time : "???")}s, start-delay={delay:f3}s");
                            }
                        }
                    }
                }
                if (actionDef.MainCooldownGroup == ActionDefinitions.GCDGroup)
                {
                    effectDuration = Math.Min(effectDuration, 0.6f); // TODO: this is a hack, reconsider... the problem is that sometimes actions apply statuses that are then refreshed, that usually happens for gcds...
                }

                if (effectDuration > 0f)
                {
                    var e = col.AddHistoryEntryRange(enc.Time.Start, effectStart, effectDuration, actionName, Colors.TextColor10);
                    e.TooltipExtra = (res, _) => res.AddRange(effectTooltip);
                    AdvanceCooldown(actionDef.MainCooldownGroup, enc.Time.Start, effectStart.AddSeconds(effectDuration), false);
                }
                col.AddHistoryEntryDot(enc.Time.Start, a.Timestamp, actionName, Colors.TextColor1).AddActionTooltip(a);
            }

            // cooldown reduction
            if (_cooldownReductions.TryGetValue(a.ID, out var cdr))
            {
                AdvanceCooldown(cdr.group, enc.Time.Start, a.Timestamp, true);
                ref var data = ref _cdGroups[cdr.group];
                float actualReduction = 0;
                if (data.ChargesOnCooldown > 0)
                {
                    actualReduction = cdr.cd;
                    data.ChargeCooldownEnd = data.ChargeCooldownEnd.AddSeconds(-cdr.cd);
                    if (a.Timestamp >= data.ChargeCooldownEnd)
                    {
                        if (--data.ChargesOnCooldown > 0)
                        {
                            data.Column?.AddHistoryEntryLine(enc.Time.Start, data.Cursor, "", Colors.TextColor1);
                            data.ChargeCooldownEnd = data.ChargeCooldownEnd.AddSeconds(data.ChargeCooldown);
                        }
                        else
                        {
                            actualReduction -= (float)(a.Timestamp - data.ChargeCooldownEnd).TotalSeconds;
                        }
                    }
                }
                data.Column?.AddHistoryEntryDot(enc.Time.Start, a.Timestamp, $"CD reduced by {actualReduction:f1}/{cdr.cd:f1}s: {actionName}", actualReduction < cdr.cd ? Colors.TextColor2 : Colors.TextColor11).AddActionTooltip(a);
            }
        }

        // add remaining unfinished casts
        while (iCast < player.Casts.Count)
        {
            AddUnfinishedCast(player.Casts[iCast++], enc.Time.Start);
        }

        // add unfinished cooldowns
        for (var i = 0; i < _cdGroups.Length; ++i)
        {
            if (_cdGroups[i].ChargesOnCooldown > 0)
            {
                AdvanceCooldown(i, enc.Time.Start, DateTime.MaxValue, true);
            }
        }
    }

    public void DrawConfig(UITree tree)
    {
        if (ImGui.Button("Show all"))
        {
            foreach (var col in Columns)
            {
                col.Width = ColumnGenericHistory.DefaultWidth;
            }
        }

        ImGui.SameLine();
        if (ImGui.Button("Hide all"))
        {
            foreach (var col in Columns)
            {
                col.Width = 0;
            }
        }

        foreach (var col in Columns)
        {
            var visible = col.Width > 0;
            if (ImGui.Checkbox(col.Name, ref visible))
            {
                col.Width = visible ? ColumnGenericHistory.DefaultWidth : 0;
            }
        }

        var anyVisible = false;
        for (var i = 0; i < Columns.Count; ++i)
        {
            if (Columns[i].Width > 0)
            {
                anyVisible = true;
                break;
            }
        }
        _sep.Width = anyVisible ? 1 : 0;
    }

    private void SetupClass(Class playerClass)
    {
        switch (playerClass)
        {
            case Class.WAR:
                // make sure important damage cooldowns are in consistent order
                GetCooldownColumn(ActionID.MakeSpell(WAR.AID.Infuriate));
                GetCooldownColumn(ActionID.MakeSpell(WAR.AID.InnerRelease));
                GetCooldownColumn(ActionID.MakeSpell(WAR.AID.Upheaval));
                GetCooldownColumn(ActionID.MakeSpell(WAR.AID.Onslaught));
                // infuriate cooldown reductions
                var infCDG = ActionDefinitions.Instance.Spell(WAR.AID.Infuriate)!.MainCooldownGroup;
                _cooldownReductions[ActionID.MakeSpell(WAR.AID.InnerBeast)] = (infCDG, 5);
                _cooldownReductions[ActionID.MakeSpell(WAR.AID.FellCleave)] = (infCDG, 5);
                _cooldownReductions[ActionID.MakeSpell(WAR.AID.InnerChaos)] = (infCDG, 5);
                _cooldownReductions[ActionID.MakeSpell(WAR.AID.SteelCyclone)] = (infCDG, 5);
                _cooldownReductions[ActionID.MakeSpell(WAR.AID.Decimate)] = (infCDG, 5);
                _cooldownReductions[ActionID.MakeSpell(WAR.AID.ChaoticCyclone)] = (infCDG, 5);
                break;
        }
    }

    private void AddUnfinishedCast(Replay.Cast cast, DateTime encStart)
    {
        var name = $"[unfinished] {cast.ID} -> {ReplayUtils.ParticipantString(cast.Target, cast.Time.Start)}";
        _animLocks.AddHistoryEntryRange(encStart, cast.Time, name, Colors.TextColor12).AddCastTooltip(cast);

        var castActionDef = ActionDefinitions.Instance[cast.ID];
        if (castActionDef?.MainCooldownGroup >= 0)
        {
            AdvanceCooldown(castActionDef.MainCooldownGroup, encStart, cast.Time.Start, true);
            GetCooldownColumn(castActionDef.MainCooldownGroup, cast.ID).AddHistoryEntryRange(encStart, cast.Time, name, Colors.TextColor12).AddCastTooltip(cast);
            AdvanceCooldown(castActionDef.MainCooldownGroup, encStart, cast.Time.End, false); // consider cooldown reset instead?..
        }
    }

    private void AddAnimationLock(ColumnGenericHistory col, Replay.Action action, DateTime encStart, DateTime lockStart, string name)
    {
        if (action.AnimationLock > 0)
        {
            var e = col.AddHistoryEntryRange(encStart, lockStart, action.AnimationLock, name, Colors.TextColor7);
            e.TooltipExtra = (res, t) =>
            {
                var elapsed = t - (lockStart - encStart).TotalSeconds;
                res.Add($"- anim lock: {action.AnimationLock - elapsed:f3} / {action.AnimationLock:f3}");
            };
        }
    }

    private ColumnGenericHistory GetCooldownColumn(int cooldownGroup, ActionID defaultAction)
        => _cdGroups[cooldownGroup].Column ??= AddBefore<ColumnGenericHistory>(new(Timeline, _autoAttacks.Tree, _autoAttacks.PhaseBranches, defaultAction.ToString()), _sep);
    private ColumnGenericHistory GetCooldownColumn(ActionID action) => GetCooldownColumn(ActionDefinitions.Instance[action]!.MainCooldownGroup, action);

    private void AddCooldownRange(ref CooldownGroup data, DateTime encStart, DateTime rangeEnd)
    {
        if (data.Column == null)
        {
            return;
        }

        var maxCharges = data.MaxCharges;
        var chargesOnCooldown = data.ChargesOnCooldown;
        var chargeCooldown = data.ChargeCooldown;
        var chargeCooldownEnd = data.ChargeCooldownEnd;
        var startCD = (chargeCooldownEnd - data.Cursor).TotalSeconds;
        var endCD = (chargeCooldownEnd - rangeEnd).TotalSeconds;
        var width = maxCharges > 0 ? (float)chargesOnCooldown / maxCharges : 1;
        var e = data.Column.AddHistoryEntryRange(encStart, data.Cursor, rangeEnd, data.CooldownAction.ToString(), Colors.TextColor7, width);
        e.TooltipExtra = (res, t) =>
        {
            res.Add($"- remaining: {(chargeCooldownEnd - encStart).TotalSeconds - t:f3}s / {chargeCooldown:f3}s ({maxCharges - chargesOnCooldown}/{maxCharges} charges)");
            res.Add($"- start CD: {startCD:f3}s / {chargeCooldown:f3}s");
            res.Add($"- end CD: {endCD:f3}s / {chargeCooldown:f3}s");
        };
    }

    private void AdvanceCooldown(int cdGroup, DateTime encStart, DateTime timestamp, bool addRanges)
    {
        ref var data = ref _cdGroups[cdGroup];

        while (data.ChargesOnCooldown > 0 && timestamp >= data.ChargeCooldownEnd)
        {
            // next charge is fully finished
            if (addRanges)
            {
                AddCooldownRange(ref data, encStart, data.ChargeCooldownEnd);
            }

            data.Cursor = data.ChargeCooldownEnd;
            if (--data.ChargesOnCooldown > 0)
            {
                data.Column?.AddHistoryEntryLine(encStart, data.Cursor, "", Colors.TextColor1);
                data.ChargeCooldownEnd = data.Cursor.AddSeconds(data.ChargeCooldown);
            }
        }

        // add partially finished cooldown: we're either just spending another charge, or we're starting new cooldown slightly earlier than expected (due to high sks for gcd, or due to client-server timing differences)
        if (data.ChargesOnCooldown > 0)
        {
            // assertion: timestamp < data.ChargeCooldownEnd
            if (addRanges)
            {
                AddCooldownRange(ref data, encStart, timestamp);
            }
        }

        data.Cursor = timestamp;
    }

    private void StartCooldown(ActionID aid, ActionDefinition actionDef, DateTime encStart, DateTime timestamp)
    {
        AdvanceCooldown(actionDef.MainCooldownGroup, encStart, timestamp, true);
        ref var data = ref _cdGroups[actionDef.MainCooldownGroup];
        if (data.ChargesOnCooldown == 0)
        {
            // off cd -> cd
            data.ChargeCooldownEnd = data.Cursor.AddSeconds(actionDef.Cooldown);
            data.ChargesOnCooldown = 1;
            data.MaxCharges = actionDef.MaxChargesAtCap(); // TODO: at player level
        }
        else if (data.ChargesOnCooldown < actionDef.MaxChargesAtCap()) // TODO: at player level
        {
            // just spend new charge
            ++data.ChargesOnCooldown;
        }
        else
        {
            // already at max charges, assume previous cooldown is slightly smaller than expected
            var deficit = (data.ChargeCooldownEnd - data.Cursor).TotalSeconds;
            var e = data.Column!.AddHistoryEntryLine(encStart, timestamp, aid.ToString(), Colors.TextColor1);
            e.TooltipExtra = (res, _) => res.Add($"- cooldown {deficit:f1}s smaller than expected");
            data.ChargeCooldownEnd = data.Cursor.AddSeconds(actionDef.Cooldown);
        }
        data.ChargeCooldown = actionDef.Cooldown;
        data.CooldownAction = aid;
    }

    private static bool ActionConfirmed(Replay.Action a)
    {
        if (a.Targets.Count == 0)
        {
            return true;
        }
        for (var i = 0; i < a.Targets.Count; ++i)
        {
            var t = a.Targets[i];
            if (t.ConfirmationSource != default || t.ConfirmationTarget != default)
            {
                return true;
            }
        }
        return false;
    }
}
