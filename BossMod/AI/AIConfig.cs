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
    public bool FollowDuringCombat = false;

    [PropertyDisplay("Follow during active boss module")]
    public bool FollowDuringActiveBossModule = false;

    [PropertyDisplay("Follow out of combat")]
    public bool FollowOutOfCombat = false;

    [PropertyDisplay("Follow target")]
    public bool FollowTarget = false;

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

    [PropertyDisplay("Idle while mounted")]
    public bool ForbidAIMovementMounted = false;
    [PropertyDisplay("Multiboxing mode: use party roles config to better support multiboxing.")]
    public bool MultiboxMode = false;

    [PropertyDisplay("Echo slash commands to chat")]
    public bool EchoToChat = true;

    public string? AIAutorotPresetName;
}
