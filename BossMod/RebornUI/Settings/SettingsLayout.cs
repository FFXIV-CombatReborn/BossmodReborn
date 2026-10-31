using BossMod.AI;
using BossMod.Autorotation;
using BossMod.ClassShared;
using BossMod.Pathfinding;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using RebornMaterial;

namespace BossMod.RebornUI;

internal static class SettingsLayout
{
    public static readonly Action<ConfigNode, UITree, WorldState> NoCustom = static (_, _, _) => { };

    public sealed record class Node
    {
        public string? Name { get; init; }
        public FontAwesomeIcon Icon { get; init; }
        public string? Description { get; init; }
        public string[] Tags { get; init; } = [];
        public (M3Severity Severity, FontAwesomeIcon Icon, string Text)? Notice { get; init; }
        public Section[]? Sections { get; init; }
        public Dictionary<string, Field> Fields { get; init; } = [];
        public Action<ConfigNode, UITree, WorldState>? Custom { get; init; }
        public Action<ConfigNode, string, UITree, WorldState>? SectionExtras { get; init; }
    }

    public sealed record class Section(string Label, FontAwesomeIcon Icon, string[] Fields, string? StripPrefix = null);

    public sealed record class Field(string? Label = null, string? Tooltip = null, string? Depends = null, string? Format = null, string[]? Options = null, float? Min = null, float? Max = null);

    private static readonly string[] _maturityOptions = ["Dummy (dev work only)", "Work in progress", "Contributed", "Verified", "AI support"];

    public static Node? For(Type type) => _nodes.GetValueOrDefault(type);

    private static readonly Dictionary<Type, Node> _nodes = new()
    {
        [typeof(InterfaceConfig)] = new()
        {
            Icon = FontAwesomeIcon.PaintBrush,
            Description = "Accent color and sizing of the BossMod Reborn windows",
            Sections =
            [
                new("Theme", FontAwesomeIcon.Palette, [nameof(InterfaceConfig.AccentColor)]),
                new("Sizing", FontAwesomeIcon.TextHeight, [nameof(InterfaceConfig.TextScale), nameof(InterfaceConfig.ElementScale), nameof(InterfaceConfig.PaddingScale), nameof(InterfaceConfig.CompactSettings)]),
            ],
            Fields = new()
            {
                [nameof(InterfaceConfig.TextScale)] = new(Format: "%.2fx"),
                [nameof(InterfaceConfig.ElementScale)] = new(Format: "%.2fx"),
                [nameof(InterfaceConfig.PaddingScale)] = new(Format: "%.2fx"),
            },
        },

        [typeof(BossModuleConfig)] = new()
        {
            Name = "Radar and hints",
            Icon = FontAwesomeIcon.Bullseye,
            Description = "The radar window, the hints shown with it, and what boss modules are allowed to do",
            Tags = ["boss modules and radar", "radar", "hints", "arena", "3d"],
            Sections =
            [
                new("Boss modules", FontAwesomeIcon.PuzzlePiece, [nameof(BossModuleConfig.MinMaturity), nameof(BossModuleConfig.AllowAutomaticActions), nameof(BossModuleConfig.MaxLoadDistance)]),
                new("Radar window", FontAwesomeIcon.WindowMaximize,
                [
                    nameof(BossModuleConfig.EnableRadar), nameof(BossModuleConfig.ShowDemo), nameof(BossModuleConfig.Lock), nameof(BossModuleConfig.TrishaMode),
                    nameof(BossModuleConfig.OpaqueArenaBackground), nameof(BossModuleConfig.ArenaScale), nameof(BossModuleConfig.RotateArena), nameof(BossModuleConfig.FlipArena),
                    nameof(BossModuleConfig.SlackForRotations),
                ]),
                new("Radar contents", FontAwesomeIcon.MapMarkedAlt,
                [
                    nameof(BossModuleConfig.ShowBorder), nameof(BossModuleConfig.ShowBorderRisk), nameof(BossModuleConfig.ShowOutlinesAndShadows), nameof(BossModuleConfig.ThicknessScale),
                    nameof(BossModuleConfig.ActorScale), nameof(BossModuleConfig.ShowCardinals), nameof(BossModuleConfig.CardinalsFontSize), nameof(BossModuleConfig.ShowWaymarks),
                    nameof(BossModuleConfig.WaymarkFontSize), nameof(BossModuleConfig.ShowSigns), nameof(BossModuleConfig.ShowIrrelevantPlayers), nameof(BossModuleConfig.ColorPlayersBasedOnRole),
                    nameof(BossModuleConfig.ShowFocusTargetPlayer), nameof(BossModuleConfig.ShowMeleeRangeIndicator),
                ]),
                new("Hints", FontAwesomeIcon.CommentDots,
                [
                    nameof(BossModuleConfig.ShowPrePullHints), nameof(BossModuleConfig.HintsInSeparateWindow), nameof(BossModuleConfig.HintsInSeparateWindowTransparent),
                    nameof(BossModuleConfig.ShowMechanicTimers), nameof(BossModuleConfig.ShowGlobalHints), nameof(BossModuleConfig.ShowPlayerHints),
                ]),
                new("3D world projection", FontAwesomeIcon.Cube,
                [
                    nameof(BossModuleConfig.ProjectRadarInto3DWorld), nameof(BossModuleConfig.ShowActorTrianglesIn3DWorld), nameof(BossModuleConfig.EnableArenaOutlineIn3DWorld),
                    nameof(BossModuleConfig.EnableTextIconBillboards), nameof(BossModuleConfig.BillboardHeightOffset), nameof(BossModuleConfig.TextBillboardFontSize),
                    nameof(BossModuleConfig.IconBillboardFontSize),
                ]),
                new("Screen and world", FontAwesomeIcon.Desktop, [nameof(BossModuleConfig.ShowScreenRiskBorder), nameof(BossModuleConfig.ScreenRiskBorderIntensity), nameof(BossModuleConfig.ShowWorldArrows)]),
            ],
            Fields = new()
            {
                [nameof(BossModuleConfig.MinMaturity)] = new(Options: _maturityOptions),
                [nameof(BossModuleConfig.MaxLoadDistance)] = new(Format: "%.0f y"),
                [nameof(BossModuleConfig.ArenaScale)] = new(Format: "%.2fx"),
                [nameof(BossModuleConfig.SlackForRotations)] = new(Format: "%.2fx"),
                [nameof(BossModuleConfig.ThicknessScale)] = new(Format: "%.2fx"),
                [nameof(BossModuleConfig.ActorScale)] = new(Format: "%.2fx"),
                [nameof(BossModuleConfig.ShowBorderRisk)] = new(Depends: nameof(BossModuleConfig.ShowBorder)),
                [nameof(BossModuleConfig.HintsInSeparateWindowTransparent)] = new(Depends: nameof(BossModuleConfig.HintsInSeparateWindow)),
                [nameof(BossModuleConfig.BillboardHeightOffset)] = new(Depends: nameof(BossModuleConfig.EnableTextIconBillboards), Format: "%.1f y"),
                [nameof(BossModuleConfig.TextBillboardFontSize)] = new(Depends: nameof(BossModuleConfig.EnableTextIconBillboards)),
                [nameof(BossModuleConfig.IconBillboardFontSize)] = new(Depends: nameof(BossModuleConfig.EnableTextIconBillboards)),
            },

            Custom = NoCustom,
            SectionExtras = static (_, section, _, _) =>
            {
                if (section == "Radar window")
                {
                    ButtonRow("##recenter", "Radar window position", "Moves the radar window back to the middle of the screen. Also available as /bmr radar reset.", "Recenter", FontAwesomeIcon.Crosshairs,
                        () => Service.BossModWindow?.RecenterWindow());
                }
            },
        },

        [typeof(ModuleConfig)] = new()
        {
            Name = "Encounter settings",
            Icon = FontAwesomeIcon.SlidersH,
            Description = "Strategies, assignments and hints for individual fights",
            Tags = ["encounter-specific settings", "strategy"],
            Notice = (M3Severity.Info, FontAwesomeIcon.InfoCircle, "Strategies and hints for individual encounters. The same settings open from the cog button next to each fight in Supported fights."),
        },

        [typeof(PartyRolesConfig)] = new()
        {
            Name = "Party roles",
            Icon = FontAwesomeIcon.Users,
            Description = "Which party member plays which role, for encounter strategies that need it",
            Tags = ["party roles assignment", "mt", "ot", "h1", "h2"],
            Custom = static (node, _, ws) => PartyRolesPanel.Draw((PartyRolesConfig)node, ws),
        },

        [typeof(ColorConfig)] = new()
        {
            Name = "Colors",
            Icon = FontAwesomeIcon.Palette,
            Description = "Colors used on the radar, in the cooldown planner and in debug views. Every color can be reset to its default",
            Tags = ["color scheme", "colour"],
            Sections =
            [
                new("Arena", FontAwesomeIcon.Bullseye,
                [
                    nameof(ColorConfig.ArenaBackground), nameof(ColorConfig.ArenaBorder), nameof(ColorConfig.ArenaAOE), nameof(ColorConfig.ArenaSafeFromAOE), nameof(ColorConfig.ArenaDanger),
                    nameof(ColorConfig.ArenaSafe), nameof(ColorConfig.ArenaEnemy), nameof(ColorConfig.ArenaObject), nameof(ColorConfig.ArenaTrap), nameof(ColorConfig.ArenaLight),
                    nameof(ColorConfig.ArenaVulnerable), nameof(ColorConfig.ArenaFutureVulnerable), nameof(ColorConfig.ArenaMeleeRangeIndicator), nameof(ColorConfig.ArenaOther),
                    nameof(ColorConfig.Shadows),
                ], "Arena: "),
                new("Players", FontAwesomeIcon.Users,
                [
                    nameof(ColorConfig.ArenaPC), nameof(ColorConfig.ArenaPlayerInteresting), nameof(ColorConfig.ArenaPlayerGeneric), nameof(ColorConfig.ArenaPlayerReallyGeneric),
                    nameof(ColorConfig.ArenaPlayerGenericTank), nameof(ColorConfig.ArenaPlayerGenericHealer), nameof(ColorConfig.ArenaPlayerGenericMelee),
                    nameof(ColorConfig.ArenaPlayerGenericCaster), nameof(ColorConfig.ArenaPlayerGenericPhysRanged), nameof(ColorConfig.ArenaPlayerGenericFocus),
                ], "Arena: "),
                new("Markers", FontAwesomeIcon.MapMarkerAlt,
                [
                    nameof(ColorConfig.WaymarkA), nameof(ColorConfig.WaymarkB), nameof(ColorConfig.WaymarkC), nameof(ColorConfig.WaymarkD), nameof(ColorConfig.Waymark1),
                    nameof(ColorConfig.Waymark2), nameof(ColorConfig.Waymark3), nameof(ColorConfig.Waymark4), nameof(ColorConfig.CardinalN), nameof(ColorConfig.CardinalE),
                    nameof(ColorConfig.CardinalS), nameof(ColorConfig.CardinalW), nameof(ColorConfig.PositionalColors),
                ]),
                new("Cooldown planner", FontAwesomeIcon.CalendarAlt,
                [
                    nameof(ColorConfig.PlannerBackground), nameof(ColorConfig.PlannerBackgroundHighlight), nameof(ColorConfig.PlannerCooldown), nameof(ColorConfig.PlannerFallback),
                    nameof(ColorConfig.PlannerEffect), nameof(ColorConfig.PlannerWindow),
                ], "Planner: "),
                new("Interface and debug", FontAwesomeIcon.Bug,
                [
                    nameof(ColorConfig.ButtonPushColor), nameof(ColorConfig.TextColors), nameof(ColorConfig.CollisionColors), nameof(ColorConfig.PathfindingColors),
                    nameof(ColorConfig.PathfindingColors5), nameof(ColorConfig.PathfindingColors6), nameof(ColorConfig.PathfindingColors7), nameof(ColorConfig.PathfindingColors8),
                ]),
            ],
        },

        [typeof(ActionTweaksConfig)] = new()
        {
            Icon = FontAwesomeIcon.Wrench,
            Description = "Changes to how actions are queued, targeted and executed, for manual play and autorotation alike",
            Sections =
            [
                new("Actions", FontAwesomeIcon.Bolt,
                [
                    nameof(ActionTweaksConfig.RemoveAnimationLockDelay), nameof(ActionTweaksConfig.AnimationLockDelayMax), nameof(ActionTweaksConfig.RemoveCooldownDelay),
                    nameof(ActionTweaksConfig.UseManualQueue), nameof(ActionTweaksConfig.AutoAutos), nameof(ActionTweaksConfig.AutoDismount), nameof(ActionTweaksConfig.CancelCastOnDeadTarget),
                ]),
                new("Movement", FontAwesomeIcon.Running,
                [
                    nameof(ActionTweaksConfig.PreventMovingWhileCasting), nameof(ActionTweaksConfig.MoveEscapeHatch), nameof(ActionTweaksConfig.PyreticThreshold),
                    nameof(ActionTweaksConfig.MisdirectionThreshold), nameof(ActionTweaksConfig.RestoreRotation), nameof(ActionTweaksConfig.DashSafety), nameof(ActionTweaksConfig.DashSafetyExtra),
                ]),
                new("Targeting", FontAwesomeIcon.Crosshairs, [nameof(ActionTweaksConfig.PreferMouseover), nameof(ActionTweaksConfig.PreventForbiddenTargets), nameof(ActionTweaksConfig.GTMode)]),
            ],
            Fields = new()
            {
                [nameof(ActionTweaksConfig.AnimationLockDelayMax)] = new(Depends: nameof(ActionTweaksConfig.RemoveAnimationLockDelay)),
                [nameof(ActionTweaksConfig.MoveEscapeHatch)] = new(Tooltip: "While this is held, you can move even though movement while casting is prevented.", Depends: nameof(ActionTweaksConfig.PreventMovingWhileCasting)),
                [nameof(ActionTweaksConfig.PyreticThreshold)] = new("Pyretic safety threshold", "Prevents movement and action execution when pyretic-like mechanics are imminent. Set to 0 to disable, otherwise increase the threshold depending on your ping.", Format: "%.2f s"),
                [nameof(ActionTweaksConfig.MisdirectionThreshold)] = new("Auto misdirection threshold", "Prevents movement under misdirection if the angle between normal and misdirected movement is greater than this threshold. Set to 180 to disable.", Format: "%.0f°"),
                [nameof(ActionTweaksConfig.DashSafetyExtra)] = new(Depends: nameof(ActionTweaksConfig.DashSafety)),
            },

            Custom = NoCustom,
            SectionExtras = static (node, section, _, _) =>
            {
                if (section == "Targeting")
                {
                    SmartTargetingRow((ActionTweaksConfig)node);
                }
            },
        },

        [typeof(SmartRotationConfig)] = new() { Icon = FontAwesomeIcon.Compass },
        [typeof(OutOfCombatActionsConfig)] = new()
        {
            Icon = FontAwesomeIcon.Walking,
            Fields = new() { [nameof(OutOfCombatActionsConfig.AutoPeloton)] = new(Depends: nameof(OutOfCombatActionsConfig.Enabled)) },
        },
        [typeof(SharedActionsConfig)] = new() { Icon = FontAwesomeIcon.ExchangeAlt },

        [typeof(AIConfig)] = new()
        {
            Icon = FontAwesomeIcon.Robot,
            Description = "The AI that follows a party member and moves to the safe spots the boss modules find",
            Tags = ["ai", "follow"],
            Sections =
            [
                new("Interface", FontAwesomeIcon.Desktop, [nameof(AIConfig.DrawUI), nameof(AIConfig.ShowDTR), nameof(AIConfig.EchoToChat)]),
                new("Following", FontAwesomeIcon.Users,
                [
                    nameof(AIConfig.FollowSlot), nameof(AIConfig.FocusTargetMaster), nameof(AIConfig.FollowDuringCombat), nameof(AIConfig.FollowDuringActiveBossModule),
                    nameof(AIConfig.FollowOutOfCombat), nameof(AIConfig.MaxDistanceToSlot), nameof(AIConfig.FollowTarget), nameof(AIConfig.DesiredPositional),
                    nameof(AIConfig.FollowRSRDesiredPositional), nameof(AIConfig.MaxDistanceToTarget), nameof(AIConfig.MinDistance), nameof(AIConfig.PreferredDistance),
                ]),
                new("Restrictions", FontAwesomeIcon.Ban, [nameof(AIConfig.ForbidActions), nameof(AIConfig.ForbidMovement), nameof(AIConfig.ManualTarget), nameof(AIConfig.ForbidAIMovementMounted)]),
                new("Behaviour", FontAwesomeIcon.SlidersH, [nameof(AIConfig.AutoAFK), nameof(AIConfig.AFKModeTimer), nameof(AIConfig.MoveDelay), nameof(AIConfig.DisableObstacleMaps), nameof(AIConfig.BroadcastToSlaves)]),
            ],
            Fields = new()
            {
                [nameof(AIConfig.FollowSlot)] = new(Tooltip: "Party slot of the player to follow, from 0 (usually yourself) to 7.", Min: 0, Max: 7),
                [nameof(AIConfig.MaxDistanceToSlot)] = new(Tooltip: "How close to stay to the followed player."),
                [nameof(AIConfig.FollowTarget)] = new(Tooltip: "In combat, follow the target instead of the followed player, at the distances below."),
                [nameof(AIConfig.DesiredPositional)] = new(Depends: nameof(AIConfig.FollowTarget)),
                [nameof(AIConfig.FollowRSRDesiredPositional)] = new(Depends: nameof(AIConfig.FollowTarget)),
                [nameof(AIConfig.MaxDistanceToTarget)] = new(Depends: nameof(AIConfig.FollowTarget)),
                [nameof(AIConfig.ManualTarget)] = new(Tooltip: "Allows manual targeting with an active AI autorotation."),
                [nameof(AIConfig.AFKModeTimer)] = new(Depends: nameof(AIConfig.AutoAFK)),
            },
        },

        [typeof(AutorotationConfig)] = new()
        {
            Name = "Autorotation",
            Icon = FontAwesomeIcon.Bolt,
            Description = "When BossMod's own autorotation runs and stops. Unsupported by Combat Reborn",
            Tags = ["unsupported", "rotation"],
            Notice = (M3Severity.Warning, FontAwesomeIcon.ExclamationTriangle, "BossMod's own autorotation is not supported by Combat Reborn. Please don't ask for help with it on the Combat Reborn Discord."),
            Sections =
            [
                new("Interface", FontAwesomeIcon.Desktop,
                [
                    nameof(AutorotationConfig.ShowUI), nameof(AutorotationConfig.ShowDTR), nameof(AutorotationConfig.HideDefaultPresets), nameof(AutorotationConfig.ShowPositionals),
                    nameof(AutorotationConfig.FollowRSRDesiredPositional),
                ]),
                new("Stopping automatically", FontAwesomeIcon.StopCircle,
                [
                    nameof(AutorotationConfig.ClearPresetOnDeath), nameof(AutorotationConfig.ClearPresetOnCombatEnd), nameof(AutorotationConfig.ClearPresetOnLuring),
                    nameof(AutorotationConfig.ClearForceDisableOnCombatEnd),
                ]),
                new("Pull safety", FontAwesomeIcon.ShieldAlt, [nameof(AutorotationConfig.EarlyPullThreshold), nameof(AutorotationConfig.PlannedPullSafety)]),
            ],
            Fields = new() { [nameof(AutorotationConfig.EarlyPullThreshold)] = new(Format: "%.1f s") },
        },

        [typeof(ZoneModuleConfig)] = new()
        {
            Name = "Duty automation",
            Icon = FontAwesomeIcon.Route,
            Description = "Zone modules that can play through quest battles and solo duties",
            Tags = ["full duty automation", "zone", "quest"],
            Sections =
            [
                new("General", FontAwesomeIcon.None, [nameof(ZoneModuleConfig.MinMaturity), nameof(ZoneModuleConfig.EnableQuestBattles), nameof(ZoneModuleConfig.UseDash)]),
                new("Display", FontAwesomeIcon.WindowMaximize, [nameof(ZoneModuleConfig.ShowWaypoints), nameof(ZoneModuleConfig.Lock), nameof(ZoneModuleConfig.TransparentMode)]),
            ],
            Fields = new() { [nameof(ZoneModuleConfig.MinMaturity)] = new(Options: _maturityOptions) },
        },

        [typeof(ReplayManagementConfig)] = new()
        {
            Icon = FontAwesomeIcon.Film,
            Description = "Recording fights for bug reports, module development and cooldown planning",
            Tags = ["record", "log"],
            Sections =
            [
                new("Recording", FontAwesomeIcon.Video,
                [
                    nameof(ReplayManagementConfig.ShowUI), nameof(ReplayManagementConfig.ImportantDutyAlert), nameof(ReplayManagementConfig.AutoRecord), nameof(ReplayManagementConfig.AutoARR),
                    nameof(ReplayManagementConfig.Anonymize), nameof(ReplayManagementConfig.MaxReplays), nameof(ReplayManagementConfig.WorldLogFormat),
                ]),
                new("Packets", FontAwesomeIcon.NetworkWired,
                [
                    nameof(ReplayManagementConfig.RecordServerPackets), nameof(ReplayManagementConfig.DumpServerPackets), nameof(ReplayManagementConfig.DumpServerPacketsPlayerOnly),
                    nameof(ReplayManagementConfig.DumpClientPackets),
                ]),
            ],
            Fields = new()
            {
                [nameof(ReplayManagementConfig.MaxReplays)] = new(Tooltip: "0 keeps every replay."),
                [nameof(ReplayManagementConfig.DumpServerPacketsPlayerOnly)] = new(Depends: nameof(ReplayManagementConfig.DumpServerPackets)),
            },
        },

        [typeof(DeveloperConfig)] = new()
        {
            Name = "Developer",
            Icon = FontAwesomeIcon.Code,
            Description = "Settings for people working on BossMod Reborn itself",
            Tags = ["developer settings", "obstacle"],
            Fields = new() { [nameof(DeveloperConfig.MapSourcePath)] = new(Depends: nameof(DeveloperConfig.MapLoadFromSource)) },
        },
    };

    private static void ButtonRow(string id, string label, string supporting, string button, FontAwesomeIcon icon, Action click)
    {
        var width = M3Widgets.ButtonWidth(icon, button);
        var row = M3SettingRow.Begin(label, supporting, new(width, M3Widgets.ButtonHeight));
        ImGui.SetCursorScreenPos(row.ControlPosition);
        if (M3Widgets.Button(id, button, M3ButtonStyle.Tonal, icon))
        {
            click();
        }
        M3SettingRow.End(row);
    }

    private static void SmartTargetingRow(ActionTweaksConfig config)
    {
        var rsrEnabled = ActionTweaksConfig.IsRSREnabled();
        const string supporting = "If the usual (mouseover/primary) target is not valid for an action, select the next best target automatically (e.g. co-tank for Shirk).";
        var row = M3SettingRow.Begin("Smart ability targeting", rsrEnabled ? $"{supporting}\nDo not use with Rotation Solver Reborn, which is currently loaded." : $"{supporting}\nDo not use with Rotation Solver Reborn.",
            M3Widgets.SwitchSize(), rsrEnabled ? M3.Scheme.Error : null);
        ImGui.SetCursorScreenPos(row.ControlPosition);
        if (M3Widgets.Switch("##SmartTargeting", ref config.SmartTargeting))
        {
            config.Modified.Fire();
        }
        M3SettingRow.End(row);
    }
}
