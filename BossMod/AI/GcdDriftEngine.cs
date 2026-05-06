namespace BossMod.AI;

/// <summary>
/// GCD-Interleave Drift — ported from OmniDuty GcdInterleaveMover.
/// <para><b>Problem solved:</b> Binary movement: player stands still casting for the entire
/// mechanic window, then panics at the last second. No gradual approach.</para>
/// <para><b>Solution:</b> During inter-GCD windows (player not casting, no animation lock,
/// GCD still on recast), subtly drift toward the safe destination.</para>
/// <para><b>RSR Safety Mitigations:</b>
/// <list type="number">
///   <item>No drift during animation lock (oGCD weave timing preserved)</item>
///   <item>No drift when GCD recast &lt; 0.7s (oGCD weave window preserved)</item>
///   <item>No drift for melee/tank roles (positional breakage prevention)</item>
///   <item>Separate toggle from other features</item>
/// </list></para>
/// <para><b>Zero-Alloc:</b> All methods use primitive math. No LINQ, no heap allocations.</para>
/// </summary>
public sealed class GcdDriftEngine
{
    // ═══════ CONSTANTS ═══════
    /// <summary>Minimum distance to bother drifting (below this we're "arrived").</summary>
    private const float MinDriftDistance = 1.5f;
    /// <summary>Max drift per inter-GCD window in yalms at TL1 Cruise (subtle).</summary>
    private const float MaxDriftPerCycleTL1 = 2.5f;
    /// <summary>Max drift per inter-GCD window in yalms at TL2 Alert (aggressive).</summary>
    private const float MaxDriftPerCycleTL2 = 5.0f;
    /// <summary>Jitter magnitude (yalms) to avoid robotic straight-line pathing.</summary>
    private const float JitterMagnitude = 0.4f;
    /// <summary>Minimum GCD recast remaining to allow drift (preserves oGCD weave window).</summary>
    private const float MinGcdRecastForDrift = 0.7f;
    /// <summary>How often to regenerate jitter (seconds) for natural feel.</summary>
    private const double JitterIntervalSeconds = 1.5;

    // ═══════ STATE ═══════
    /// <summary>True during inter-GCD drift this frame.</summary>
    public bool IsDrifting { get; private set; }
    /// <summary>The computed drift target this frame.</summary>
    public WPos? DriftTarget { get; private set; }

    // Internal state
    private readonly Random _rng = new();
    private float _jitterX;
    private float _jitterZ;
    private DateTime _lastJitterTime;

    /// <summary>
    /// Evaluate whether to drift this frame.
    /// <para>Returns the drift target if drifting, null if not.</para>
    /// </summary>
    /// <param name="playerPos">Player's current position.</param>
    /// <param name="destination">Pathfinding destination (from NavigationDecision).</param>
    /// <param name="threat">Current threat level from ThreatBudget.</param>
    /// <param name="playerIsCasting">Whether the player has an active cast.</param>
    /// <param name="slideCastWindow">Slidecast window duration for slidecast check.</param>
    /// <param name="enableSlideCast">Whether slidecast detection is enabled.</param>
    /// <param name="currentTime">Current world time for jitter timing.</param>
    /// <param name="playerRole">Player's combat role. Drift is disabled for Melee and Tank to prevent positional breakage.</param>
    /// <returns>Drift target position, or null if not drifting this frame.</returns>
    public WPos? Evaluate(
        WPos playerPos,
        WPos? destination,
        ThreatLevel threat,
        bool playerIsCasting,
        float slideCastWindow,
        bool enableSlideCast,
        DateTime currentTime,
        Role playerRole)
    {
        // Reset per-frame state
        IsDrifting = false;
        DriftTarget = null;

        // Gate: need a destination
        if (!destination.HasValue)
            return null;

        // Gate: only drift at TL1 (Cruise) or TL2 (Alert)
        // TL3: BMR pathfinding drives directly. TL_None: no threat, no destination.
        if (threat is ThreatLevel.Critical or ThreatLevel.None)
            return null;

        // ═══════ RSR SAFETY MITIGATION 3: No drift for melee/tank roles ═══════
        // Melee DPS and tanks need precise positionals (Rear/Flank).
        // Drifting could move them out of the correct arc mid-combo.
        if (playerRole is Role.Melee or Role.Tank)
            return null;

        // Gate: check distance
        var dx = destination.Value.X - playerPos.X;
        var dz = destination.Value.Z - playerPos.Z;
        var distSq = dx * dx + dz * dz;
        if (distSq < MinDriftDistance * MinDriftDistance)
            return null;

        // ═══════ RSR SAFETY MITIGATION 1: No drift during animation lock ═══════
        if (SlideCastHelper.GetAnimationLockRemaining() > 0.05f)
            return null;

        // ═══════ RSR SAFETY MITIGATION 2: No drift during oGCD weave window ═══════
        // If GCD recast is < 0.7s, RSR may be about to weave an oGCD — don't interfere
        var gcdRecast = SlideCastHelper.GetGcdRecastRemaining();
        if (gcdRecast > 0f && gcdRecast < MinGcdRecastForDrift)
            return null;

        // ═══════ DRIFT WINDOW CHECK ═══════
        // Only drift when NOT casting a stationary spell.
        // During slidecast window (last 0.5s of cast) → can also drift.
        if (playerIsCasting)
        {
            if (enableSlideCast && SlideCastHelper.IsInSlideCastWindow(slideCastWindow))
            {
                // In slidecast window — safe to drift
            }
            else
            {
                return null; // Casting and not in slidecast — don't move
            }
        }

        // ═══════ COMPUTE DRIFT TARGET ═══════
        var dist = MathF.Sqrt(distSq);
        var maxDrift = threat == ThreatLevel.Cruise ? MaxDriftPerCycleTL1 : MaxDriftPerCycleTL2;

        // Direction toward destination
        var invDist = 1f / dist;
        var dirX = dx * invDist;
        var dirZ = dz * invDist;

        // Clamp drift distance
        var driftDist = MathF.Min(maxDrift, dist);
        var driftX = playerPos.X + dirX * driftDist;
        var driftZ = playerPos.Z + dirZ * driftDist;

        // ═══════ HUMANIZATION JITTER ═══════
        if ((currentTime - _lastJitterTime).TotalSeconds > JitterIntervalSeconds)
        {
            _jitterX = ((float)_rng.NextDouble() - 0.5f) * 2f * JitterMagnitude;
            _jitterZ = ((float)_rng.NextDouble() - 0.5f) * 2f * JitterMagnitude;
            _lastJitterTime = currentTime;
        }

        var driftTarget = new WPos(driftX + _jitterX, driftZ + _jitterZ);

        // ═══════ ACTIVATE DRIFT ═══════
        IsDrifting = true;
        DriftTarget = driftTarget;
        return driftTarget;
    }

    /// <summary>Reset state on combat end / zone change.</summary>
    public void Reset()
    {
        IsDrifting = false;
        DriftTarget = null;
        _lastJitterTime = default;
    }
}
