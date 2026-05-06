namespace BossMod.AI;

/// <summary>
/// Threat-Level Movement Budgeting — ported from OmniDuty TimeBudgetNavigator.
/// <para><b>Problem solved:</b> BMR's movement logic is binary: either move or don't.
/// No awareness of HOW MUCH TIME the player has before a forbidden zone activates,
/// leading to either premature panic movement (DPS loss) or late dodges (death).</para>
/// <para><b>Solution:</b> Compute <c>SlackTime = TimeUntilActivation - TravelTime</c>
/// and assign one of 3 threat levels:
/// <list type="bullet">
///   <item><b>TL1 (Cruise, SlackTime &gt; AlertThreshold):</b> Optimize for DPS. Long casts OK.</item>
///   <item><b>TL2 (Alert, CriticalThreshold &lt; SlackTime ≤ AlertThreshold):</b> oGCDs only, suppress long casts.</item>
///   <item><b>TL3 (Critical, SlackTime ≤ CriticalThreshold):</b> Cancel casts, Sprint, survive.</item>
/// </list></para>
/// <para><b>Zero-Alloc:</b> All methods use primitive math. No LINQ, no heap allocations.</para>
/// </summary>
public enum ThreatLevel : byte
{
    /// <summary>No active threat or no forbidden zones. Normal rotation.</summary>
    None = 0,
    /// <summary>SlackTime > AlertThreshold. DPS optimization, long casts OK.</summary>
    Cruise = 1,
    /// <summary>CriticalThreshold &lt; SlackTime ≤ AlertThreshold. oGCDs only, suppress long casts.</summary>
    Alert = 2,
    /// <summary>SlackTime ≤ CriticalThreshold. Cancel casts, Sprint, direct dash.</summary>
    Critical = 3,
}

/// <summary>
/// Per-frame result of threat budget evaluation.
/// Struct to avoid heap allocation on the hot path.
/// </summary>
public struct ThreatBudgetResult
{
    /// <summary>Current threat level this frame.</summary>
    public ThreatLevel Threat;
    /// <summary>Computed slack time in seconds (TimeUntilActivation - TravelTime). float.MaxValue if no threat.</summary>
    public float SlackTime;
    /// <summary>Estimated travel time to safe destination in seconds.</summary>
    public float TravelTime;
    /// <summary>Distance to safe destination in yalms.</summary>
    public float DistToSafe;
}

/// <summary>
/// Static evaluator for threat budget. No instance state — pure function.
/// Called once per frame from AIBehaviour after NavigationDecision is computed.
/// </summary>
public static class ThreatBudget
{
    /// <summary>Minimum distance to consider for travel time computation. Below this, player is "arrived".</summary>
    private const float MinTravelDistance = 0.5f;

    /// <summary>
    /// Evaluate the movement threat budget for this frame.
    /// <para><b>Hot path:</b> Called every frame. Pure arithmetic, zero allocations.</para>
    /// </summary>
    /// <param name="destination">The pathfinding destination (from NavigationDecision). Null if no movement needed.</param>
    /// <param name="playerPos">Player's current world position.</param>
    /// <param name="leewaySeconds">LeewaySeconds from NavigationDecision (time before forbidden zone activates minus travel time already computed by pathfinder).</param>
    /// <param name="playerSpeed">Player's current movement speed in yalms/second.</param>
    /// <param name="alertThreshold">Config: seconds of slack above which we're in Cruise mode.</param>
    /// <param name="criticalThreshold">Config: seconds of slack at or below which we enter Critical mode.</param>
    /// <returns>Threat budget result for this frame.</returns>
    public static ThreatBudgetResult Evaluate(
        WPos? destination,
        WPos playerPos,
        float leewaySeconds,
        float playerSpeed,
        float alertThreshold,
        float criticalThreshold)
    {
        var result = new ThreatBudgetResult
        {
            Threat = ThreatLevel.None,
            SlackTime = float.MaxValue,
            TravelTime = 0f,
            DistToSafe = 0f,
        };

        // No destination → no threat
        if (!destination.HasValue)
            return result;

        // Compute distance
        var dx = destination.Value.X - playerPos.X;
        var dz = destination.Value.Z - playerPos.Z;
        var distSq = dx * dx + dz * dz;

        // Already at destination
        if (distSq < MinTravelDistance * MinTravelDistance)
            return result;

        result.DistToSafe = MathF.Sqrt(distSq);

        // Compute travel time using actual player speed
        var speed = playerSpeed > 0f ? playerSpeed : 6f;
        result.TravelTime = result.DistToSafe / speed;

        // SlackTime: how much margin we have after subtracting travel time
        // NavigationDecision.LeewaySeconds already factors in forbidden zone activation timing
        // It represents: "how long can the player stay in place before they MUST start moving"
        result.SlackTime = leewaySeconds;

        // Assign threat level
        if (result.SlackTime > alertThreshold)
        {
            result.Threat = ThreatLevel.Cruise;
        }
        else if (result.SlackTime > criticalThreshold)
        {
            result.Threat = ThreatLevel.Alert;
        }
        else
        {
            result.Threat = ThreatLevel.Critical;
        }

        return result;
    }
}
