namespace BossMod.AI;

/// <summary>
/// Slidecast detection — ported from OmniDuty SlideCastEngine.
/// <para><b>Problem solved:</b> BMR forbids all movement during casts, even during the
/// final ~0.5s where the game allows free movement without interrupting the spell.
/// For a 2.5s GCD, that's 20% of cast time lost for positioning.</para>
/// <para><b>Solution:</b> Read the player's casting state via unsafe pointer to
/// <c>ActionManager.Instance()</c> and expose slidecast window detection.</para>
/// <para><b>Hot-path:</b> Single unsafe pointer read per call. Zero allocations. O(1).
/// No try/catch — uses pure null checks following BMR's WorldStateGameSync pattern.</para>
/// <para><b>RSR Compatibility:</b> This only controls when BMR allows its own AI movement.
/// RSR controls the rotation queue independently. RSR has its own slidecast logic and
/// both operate on the same underlying ActionManager state without conflict.</para>
/// </summary>
public static class SlideCastHelper
{
    /// <summary>
    /// Check if the player is currently in the slidecast window.
    /// <para>Returns <c>true</c> when: player is casting AND remaining cast time &lt; slideCastWindow.</para>
    /// <para>When this returns true, movement can begin without cancelling the in-progress cast.</para>
    /// </summary>
    /// <param name="slideCastWindow">Duration of the slidecast window in seconds (typically 0.5s).</param>
    /// <returns>True if in the slidecast window and movement is safe.</returns>
    public static unsafe bool IsInSlideCastWindow(float slideCastWindow)
    {
        var am = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
        if (am == null)
            return false;

        // No cast in progress
        if (am->CastTimeElapsed <= 0 || am->CastTimeTotal <= 0)
            return false;

        var remaining = am->CastTimeTotal - am->CastTimeElapsed;
        return remaining > 0 && remaining < slideCastWindow;
    }

    /// <summary>
    /// Check if a new GCD cast can be started and completed (or reach slidecast) before a movement deadline.
    /// <para>Use case: At TL2 Alert, instead of blanket MaxCastTime=0, we compute the maximum
    /// cast duration that fits within the available slack time minus the slidecast window.</para>
    /// </summary>
    /// <param name="slackTime">Available time before movement must begin.</param>
    /// <param name="slideCastWindow">The slidecast window duration in seconds.</param>
    /// <returns>Maximum castTime that would reach slidecast before the slack runs out. 0 if no cast fits.</returns>
    public static float MaxCastTimeForSlack(float slackTime, float slideCastWindow)
    {
        // The effective cast "lock" = castTime - slideCastWindow
        // So max castTime = slackTime + slideCastWindow
        // But we need at least slideCastWindow of slack to even start a cast
        if (slackTime <= 0)
            return 0f;
        return Math.Max(0f, slackTime + slideCastWindow);
    }

    /// <summary>
    /// Get the current remaining cast time. Returns 0 if not casting.
    /// </summary>
    public static unsafe float GetRemainingCastTime()
    {
        var am = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
        if (am == null || am->CastTimeTotal <= 0)
            return 0f;

        return Math.Max(0f, am->CastTimeTotal - am->CastTimeElapsed);
    }

    /// <summary>
    /// Get the current animation lock remaining in seconds. Returns 0 if no lock active.
    /// Used by GcdDriftEngine to avoid drifting during oGCD animation locks.
    /// </summary>
    public static unsafe float GetAnimationLockRemaining()
    {
        var am = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
        if (am == null)
            return 0f;

        return am->AnimationLock;
    }

    /// <summary>
    /// Get the current GCD recast remaining. Returns 0 if GCD is ready.
    /// Used by GcdDriftEngine to detect oGCD weave windows.
    /// </summary>
    public static unsafe float GetGcdRecastRemaining()
    {
        var am = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
        if (am == null)
            return 0f;

        // GCD group is recast group 58 (spell recast)
        var recastDetail = am->GetRecastGroupDetail(58);
        if (recastDetail == null)
            return 0f;

        var remaining = recastDetail->Total - recastDetail->Elapsed;
        return remaining > 0 ? remaining : 0f;
    }
}
