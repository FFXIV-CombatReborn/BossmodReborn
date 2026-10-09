using Dalamud.Game.ClientState.Conditions;

namespace BossMod.AI;

// utility for simulating user actions based on AI decisions:
// - navigation
// - using actions safely (without spamming, not in cutscenes, etc)
sealed class AIController(WorldState ws, ActionManagerEx amex, MovementOverride movement)
{
    public WPos? NaviTargetPos;
    public float? NaviTargetVertical;
    public bool AllowInterruptingCastByMovement; // explicit override (e.g. misdirection); normal leeway-based decision is made in Update
    public float NaviLeewaySeconds = float.MaxValue;
    public bool ForceCancelCastOtherAI;
    public bool ForceCancelCastMechanicAI;

    private readonly CastSettleHold _castHold = new();
    private readonly ActionManagerEx _amex = amex;
    private readonly MovementOverride _movement = movement;

    public bool IsVerticalAllowed => Service.Condition[ConditionFlag.InFlight];
    public Angle CameraFacing => (Camera.Instance?.CameraAzimuth ?? 0f).Radians() + 180f.Degrees();
    public Angle CameraAltitude => (Camera.Instance?.CameraAltitude ?? 0f).Radians();

    public void Clear()
    {
        NaviTargetPos = null;
        NaviTargetVertical = null;
        AllowInterruptingCastByMovement = false;
        NaviLeewaySeconds = float.MaxValue;
        ForceCancelCastOtherAI = false;
        ForceCancelCastMechanicAI = false;
        _castHold.Reset();
    }

    public void SetFocusTarget(Actor? actor)
    {
        if (Service.TargetManager.FocusTarget?.EntityId != actor?.InstanceID)
        {
            Service.TargetManager.FocusTarget = actor != null ? Service.ObjectTable.SearchById((uint)actor.InstanceID) : null;
        }
    }

    public void Update(Actor? player, AIHints hints, DateTime now)
    {
        if (player == null || player.IsDead || ws.Party.Members[PartyState.PlayerSlot].InCutscene)
        {
            return;
        }

        Vector3? desiredPosition = null;

        // TODO this checks whether movement keys are pressed, we need a better solution
        var moveRequested = _movement.IsMoveRequested();
        var castInProgress = player.CastInfo != null;
        // decide here, every frame, against the live client cast state - the AI behaviour runs async and only sees a previous frame's worldstate,
        // so a cast started this frame by an external plugin (e.g. RSR) looks like 'no cast', gets interrupted by a one-frame move, and the two lock into a cast/interrupt loop
        // if no cast has started yet but one is imminent, only an urgent dodge (no leeway left) is allowed to pre-empt it
        var castRemaining = _amex.CastTimeRemaining;
        var mustMoveNow = AllowInterruptingCastByMovement || (castRemaining > 0f ? NaviLeewaySeconds <= castRemaining - 0.5f : NaviLeewaySeconds <= 0f);
        var forbidMovement = moveRequested || _amex.MoveMightInterruptCast && !mustMoveNow;
        if (NaviTargetPos != null && !forbidMovement && (NaviTargetPos.Value - player.Position).LengthSq() > 0.001f)
        {
            desiredPosition = NaviTargetPos.Value.ToVec3(NaviTargetVertical != null && IsVerticalAllowed ? NaviTargetVertical.Value : player.PosRot.Y);
        }
        else
        {
            // note: |= rather than =, hints are cleared every frame and this must not wipe cancel requests already set by the hints builder or rotation modules
            hints.ForceCancelCastMechanic |= ForceCancelCastMechanicAI && castInProgress;
            hints.ForceCancelCastOther |= ForceCancelCastOtherAI && castInProgress;
        }

        var moving = desiredPosition != null;
        if (_castHold.Update(now, moving, moving && _amex.MoveMightInterruptCast, out var holdStarted))
        {
            if (holdStarted)
            {
                Service.Log($"[AI] Movement will interrupt cast: leeway={NaviLeewaySeconds:f2}, castRemaining={castRemaining:f2}, override={AllowInterruptingCastByMovement}, dist={(NaviTargetPos!.Value - player.Position).Length():f2} -> holding casts");
            }
            hints.HoldCasts = true;
            hints.MaxCastTime = 0f;
        }

        if (hints.ForcedMovement == null && desiredPosition != null)
        {
            hints.ForcedMovement = desiredPosition.Value - player.PosRot.XYZ();
        }
    }
}
