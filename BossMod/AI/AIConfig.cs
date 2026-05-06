namespace BossMod.AI;

[ConfigDisplay(Name = "AI configuration (AI is very experimental, use at your own risk!)", Order = 7)]
sealed class AIConfig : ConfigNode
{
    [PropertyDisplay("Show status in DTR bar")]
    public bool ShowDTR = false;

    [PropertyDisplay("Show AI interface")]
    public bool DrawUI = false;

    [PropertyDisplay("Focus target master")]
    public bool FocusTargetMaster = false;

    [PropertyDisplay("Broadcast keypresses to other windows", tooltip: "Can cause hitching on some computers. Only enable it if it is actually needed! It is only useful for multiboxers.")]
    public bool BroadcastToSlaves = false;

    [PropertyDisplay("Follow party slot")]
    public int FollowSlot = 0;

    [PropertyDisplay("Forbid actions")]
    public bool ForbidActions = false;

    [PropertyDisplay("Manual targeting")]
    public bool ManualTarget = false;

    [PropertyDisplay("Forbid movement")]
    public bool ForbidMovement = false;

    [PropertyDisplay("Follow during combat")]
    public bool FollowDuringCombat = true;

    [PropertyDisplay("Follow during active boss module")]
    public bool FollowDuringActiveBossModule = true;

    [PropertyDisplay("Follow out of combat")]
    public bool FollowOutOfCombat = false;

    [PropertyDisplay("Follow target")]
    public bool FollowTarget = true;

    [PropertyDisplay("Desired positional when following target")]
    [PropertyCombo(["Any", "Flank", "Rear", "Front"])]
    public Positional DesiredPositional = Positional.Any;

    [PropertyDisplay("Max distance to slot")]
    public float MaxDistanceToSlot = 1f;

    [PropertyDisplay("Max distance to target")]
    public float MaxDistanceToTarget = 2.6f;

    [PropertyDisplay("Minimum distance to hitbox")]
    public float MinDistance = default;

    [PropertyDisplay("Preferred distance to forbidden zones")]
    public float PreferredDistance = default;

    [PropertyDisplay("Enable auto AFK", tooltip: "Enables auto AFK if out of combat. While AFK AI will not use autorotation or target anything")]
    public bool AutoAFK = false;

    [PropertyDisplay("Auto AFK timer", tooltip: "Time in seconds out of combat until AFK mode enables. Any movement will reset timer or disable AFK mode if already active.")]
    public float AFKModeTimer = 10f;

    [PropertyDisplay("Disable loading obstacle maps", tooltip: "Might be required to be enabled for some some content such as deep dungeons.")]
    public bool DisableObstacleMaps = false;

    [PropertyDisplay("Movement decision delay", tooltip: "Only change this at your own risk and keep this value low! Too high and it won't move in time for some mechanics. Make sure to readjust the value for different content.")]
    public double MoveDelay = default;

    [PropertyDisplay("Move delay variance %", tooltip: "Randomizes the move delay by this percentage to simulate human reaction time. Recommended: 20. Set to 0 to disable.")]
    public float MoveDelayVariance = 0f;

    [PropertyDisplay("Distance variance", tooltip: "Adds a random +/- offset to minimum and preferred distances. Recommended: 0.05. Set to 0 to disable.")]
    public float DistanceVariance = 0f;

    [PropertyDisplay("Idle while mounted")]
    public bool ForbidAIMovementMounted = false;

    [PropertyDisplay("Echo slash commands to chat")]
    public bool EchoToChat = true;

    public string? AIAutorotPresetName;

    // ═══════ OmniDuty-Ported Movement Intelligence ═══════
    // All features default to OFF. Existing behavior is preserved unless explicitly enabled.

    [PropertyDisplay("Enable threat-level movement budgeting", tooltip: "Replaces binary move/don't-move with 3-tier threat system (Cruise/Alert/Critical) based on time-to-safety vs distance. Uses existing BMR LeewaySeconds data.")]
    public bool EnableThreatBudget = false;

    [PropertyDisplay("Threat budget — Alert threshold (seconds)", tooltip: "Below this SlackTime, suppress long casts (oGCDs only). Works via MaxCastTime=0 which RSR reads via IPC. Default: 8.0")]
    public float ThreatAlertThreshold = 8.0f;

    [PropertyDisplay("Threat budget — Critical threshold (seconds)", tooltip: "Below this SlackTime, cancel casts, Sprint, direct dash. Default: 2.0")]
    public float ThreatCriticalThreshold = 2.0f;

    [PropertyDisplay("Enable slidecast optimization", tooltip: "Detects the ~0.5s slidecast window at the end of casts and allows AI movement during that time. The cast still completes — zero DPS loss. Independent from RSR's own slidecast logic.")]
    public bool EnableSlideCast = false;

    [PropertyDisplay("Slidecast window (seconds)", tooltip: "Duration of the slidecast window. 0.5 is conservative, 0.4 works on high-latency. Default: 0.5")]
    public float SlideCastWindow = 0.5f;

    [PropertyDisplay("Enable inter-GCD drift", tooltip: "Gradually drift toward safe zone between GCD casts instead of standing still. Only drifts when: not in animation lock, not during oGCD weave windows, not casting. RSR-safe.")]
    public bool EnableGcdDrift = false;

    [PropertyDisplay("Enable emergency evasion sprint", tooltip: "Auto-queue Sprint when threat level is Critical and safe zone is >3 yalms away. If disabled, sprint logic reverts to legacy behavior.")]
    public bool EnableEmergencySprint = false;

    [PropertyDisplay("Draw AI waypoint in world", tooltip: "Draws a circle at the AI navigation destination and a line from player to it. Color changes by threat level: green=Cruise, yellow=Alert, red=Critical.")]
    public bool DrawWaypoint = false;

    [PropertyDisplay("Target position anti-jitter", tooltip: "Stabilizes the target position used for distance calculations (min distance and max distance to target). Ignores boss movements smaller than 0.5 yalms (e.g. auto-attack animations). Prevents micro-stutter at max melee range.")]
    public bool EnableTargetAntiJitter = false;
}
