﻿using BossMod.Autorotation;
using BossMod.Pathfinding;
using ImGuiNET;

namespace BossMod.AI;

public record struct Targeting(AIHints.Enemy Target, float PreferredRange = 2.6f, Positional PreferredPosition = Positional.Any, bool PreferTanking = false);

// constantly follow master
sealed class AIBehaviour(AIController ctrl, RotationModuleManager autorot, Preset? aiPreset) : IDisposable
{
    public WorldState WorldState => autorot.Bossmods.WorldState;
    public Preset? AIPreset = aiPreset;
    public float ForceMovementIn = float.MaxValue; // TODO: reconsider
    private static readonly AIConfig _config = Service.Config.Get<AIConfig>();
    private readonly NavigationDecision.Context _naviCtx = new();
    private NavigationDecision _naviDecision;
    private bool _afkMode;
    private bool _followMaster; // if true, our navigation target is master rather than primary target - this happens e.g. in outdoor or in dungeons during gathering trash
    private WPos _masterPrevPos;
    private WPos _masterMovementStart;
    private DateTime _masterLastMoved;
    private DateTime _navStartTime; // if current time is < this, navigation won't start

    public void Dispose()
    {
    }

    public async Task Execute(Actor player, Actor master)
    {
        ForceMovementIn = float.MaxValue;
        if (player.IsDead)
            return;

        // keep master in focus
        if (_config.FocusTargetLeader)
            FocusMaster(master);

        _afkMode = _config.AutoAFK && !master.InCombat && (WorldState.CurrentTime - _masterLastMoved).TotalSeconds > _config.AFKModeTimer;
        var gazeImminent = autorot.Hints.ForbiddenDirections.Count > 0 && autorot.Hints.ForbiddenDirections[0].activation <= WorldState.FutureTime(0.5f);
        var pyreticImminent = autorot.Hints.ImminentSpecialMode.mode == AIHints.SpecialMode.Pyretic && autorot.Hints.ImminentSpecialMode.activation <= WorldState.FutureTime(1);
        var misdirectionMode = autorot.Hints.ImminentSpecialMode.mode == AIHints.SpecialMode.Misdirection && autorot.Hints.ImminentSpecialMode.activation <= WorldState.CurrentTime;
        var forbidActions = _config.ForbidActions || _afkMode || gazeImminent || pyreticImminent;
        var hadNavi = _naviDecision.Destination != null;

        Targeting target = new();
        if (!forbidActions && AIPreset != null && (!_config.ForbidAIMovementMounted || _config.ForbidAIMovementMounted && player.MountId == 0))
        {
            target = SelectPrimaryTarget(player, master);
            if (_config.ManualTarget)
            {
                var t = autorot.WorldState.Actors.Find(player.TargetID);
                if (t != null)
                    target.Target = new AIHints.Enemy(t, 100, false);
                else
                    target = new();
            }
            if (target.Target != null || TargetIsForbidden(player.TargetID))
                autorot.Hints.ForcedTarget ??= target.Target?.Actor;
            AdjustTargetPositional(player, ref target);
        }

        var followTarget = _config.FollowTarget;
        _followMaster = (_config.FollowDuringCombat || !master.InCombat || (_masterPrevPos - _masterMovementStart).LengthSq() > 100) && (_config.FollowDuringActiveBossModule || autorot.Bossmods.ActiveModule?.StateMachine.ActiveState == null) && (_config.FollowOutOfCombat || master.InCombat);
        // note: if there are pending knockbacks, don't update navigation decision to avoid fucking up positioning
        if (player.PendingKnockbacks == 0)
        {
            var actorTarget = autorot.WorldState.Actors.Find(player.TargetID);
            (var naviDecision, target) = followTarget && actorTarget != null
                ? await BuildNavigationDecision(player, actorTarget, target).ConfigureAwait(true)
                : await BuildNavigationDecision(player, master, target).ConfigureAwait(true);
            _naviDecision = naviDecision;

            // There is a difference between having a small positive leeway and having a negative one for pathfinding, prefer to keep positive
            _naviDecision.LeewaySeconds = Math.Max(0, _naviDecision.LeewaySeconds - 0.1f);
        }

        var masterIsMoving = TrackMasterMovement(master);
        var moveWithMaster = masterIsMoving && (master == player || _followMaster);
        ForceMovementIn = moveWithMaster || gazeImminent || pyreticImminent ? 0 : _naviDecision.LeewaySeconds;

        if (_config.MoveDelay != 0 && !hadNavi && _naviDecision.Destination != null)
            _navStartTime = WorldState.FutureTime(_config.MoveDelay);

        if (!forbidActions)
        {
            autorot.Preset = target.Target != null ? AIPreset : null;
        }
        UpdateMovement(player, master, target, gazeImminent || pyreticImminent, misdirectionMode ? autorot.Hints.MisdirectionThreshold : default, !forbidActions ? autorot.Hints.ActionsToExecute : null);
    }

    // returns null if we're to be idle, otherwise target to attack
    private Targeting SelectPrimaryTarget(Actor player, Actor master)
    {
        if (AIManager.Instance?.Beh != null && autorot.Hints.InteractWithTarget is Actor interact)
            return new(new AIHints.Enemy(interact, 100, false), 3.5f);

        // we prefer not to switch targets unnecessarily, so start with current target - it could've been selected manually or by AI on previous frames
        // if current target is not among valid targets, clear it - this opens way for future target selection heuristics
        var targetId = autorot.Hints.ForcedTarget?.InstanceID ?? player.TargetID;
        var target = autorot.Hints.PriorityTargets.FirstOrDefault(e => e.Actor.InstanceID == targetId);

        // if we don't have a valid target yet, use some heuristics to select some 'ok' target to attack
        // try assisting master, otherwise (if player is own master, or if master has no valid target) just select closest valid target
        target ??= master != player ? autorot.Hints.PriorityTargets.FirstOrDefault(t => master.TargetID == t.Actor.InstanceID) : null;
        target ??= autorot.Hints.PriorityTargets.MinBy(e => (e.Actor.Position - player.Position).LengthSq());

        // if the previous line returned no target, there aren't any priority targets at all - give up
        if (target == null)
            return new();

        // TODO: rethink all this... ai module should set forced target if it wants to switch... figure out positioning and stuff
        // now give class module a chance to improve targeting
        // typically it would switch targets for multidotting, or to hit more targets with AOE
        // in case of ties, it should prefer to return original target - this would prevent useless switches
        var targeting = new Targeting(target!, player.Role is Role.Melee or Role.Tank ? 2.9f : 24.5f);

        var pos = autorot.Hints.RecommendedPositional;
        if (pos.Target != null && targeting.Target.Actor == pos.Target)
            targeting.PreferredPosition = pos.Pos;

        return /*autorot.SelectTargetForAI(targeting) ??*/ targeting;
    }

    private void AdjustTargetPositional(Actor player, ref Targeting targeting)
    {
        if (targeting.Target == null || targeting.PreferredPosition == Positional.Any)
            return; // nothing to adjust

        if (targeting.PreferredPosition == Positional.Front)
        {
            // 'front' is tank-specific positional; no point doing anything if we're not tanking target
            if (targeting.Target.Actor.TargetID != player.InstanceID)
                targeting.PreferredPosition = Positional.Any;
            return;
        }

        // if target-of-target is player, don't try flanking, it's probably impossible... - unless target is currently casting (TODO: reconsider?)
        // skip if targeting a dummy, they don't rotate
        if (targeting.Target.Actor.TargetID == player.InstanceID && targeting.Target.Actor.CastInfo == null && targeting.Target.Actor.NameID != 541)
            targeting.PreferredPosition = Positional.Any;
    }

    private async Task<(NavigationDecision decision, Targeting updatedTargeting)> BuildNavigationDecision(Actor player, Actor master, Targeting targeting)
    {
        if (_config.ForbidMovement || _config.ForbidAIMovementMounted && player.MountId != 0 || autorot.Hints.ImminentSpecialMode.mode == AIHints.SpecialMode.NoMovement && autorot.Hints.ImminentSpecialMode.activation <= WorldState.FutureTime(1))
            return (new NavigationDecision { LeewaySeconds = float.MaxValue }, targeting);

        if (_followMaster && (AIPreset == null || _config.OverrideAutorotation))
        {
            var target = autorot.WorldState.Actors.Find(player.TargetID);
            if (!_config.FollowTarget || _config.FollowTarget && target == null)
            {
                var decision = await Task.Run(() => NavigationDecision.Build(_naviCtx, WorldState, autorot.Hints, player, master.Position, _config.MaxDistanceToSlot, new(), Positional.Any)).ConfigureAwait(true);
                return (decision, targeting);
            }
            if (_config.FollowTarget && target != null)
            {
                var positional = _config.DesiredPositional;
                if (positional is not Positional.Any and not Positional.Front && target.TargetID == player.InstanceID && target.CastInfo == null && target.NameID != 541) // if player is target, rear/flank is usually impossible unless target is casting
                    positional = Positional.Any;
                var decision = await Task.Run(() => NavigationDecision.Build(_naviCtx, WorldState, autorot.Hints, player, target.Position, target.HitboxRadius + (positional != Positional.Any ? 2.6f : _config.MaxDistanceToTarget), target.Rotation, positional)).ConfigureAwait(true);
                return (decision, targeting);
            }
        }
        if (targeting.Target == null)
        {
            var decision = await Task.Run(() => NavigationDecision.Build(_naviCtx, autorot.WorldState, autorot.Hints, player, null, 0, new(), Positional.Any)).ConfigureAwait(true);
            return (decision, targeting);
        }

        var adjRange = targeting.PreferredRange + player.HitboxRadius + targeting.Target.Actor.HitboxRadius;

        if (targeting.PreferTanking)
        {
            var desiredToTarget = targeting.Target.Actor.Position - targeting.Target.DesiredPosition;

            if (desiredToTarget.LengthSq() > 4 /*&& (_autorot.ClassActions?.GetState().GCD ?? 0) > 0.5f*/)
            {
                var decision = await Task.Run(() =>
                {
                    var dest = autorot.Hints.ClampToBounds(targeting.Target.DesiredPosition - adjRange * desiredToTarget.Normalized());
                    return NavigationDecision.Build(_naviCtx, WorldState, autorot.Hints, player, dest, 0.5f, new(), Positional.Any);
                }).ConfigureAwait(true);

                return (decision, targeting);
            }
        }
        var adjRotation = targeting.PreferTanking ? targeting.Target.DesiredRotation : targeting.Target.Actor.Rotation;
        var pos = autorot.Hints.RecommendedPositional.Target?.Position;
        var fdecision = await Task.Run(() =>
            NavigationDecision.Build(_naviCtx, WorldState, autorot.Hints, player, pos == null ? autorot.WorldState.Actors.Find(player.TargetID)?.Position : pos, adjRange, adjRotation, autorot.Hints.RecommendedPositional.Pos)
        ).ConfigureAwait(true);

        return (fdecision, targeting);
    }
    private void FocusMaster(Actor master)
    {
        var masterChanged = Service.TargetManager.FocusTarget?.EntityId != master.InstanceID;
        if (masterChanged)
        {
            ctrl.SetFocusTarget(master);
            _masterPrevPos = _masterMovementStart = master.Position;
            _masterLastMoved = WorldState.CurrentTime.AddSeconds(-1);
        }
    }

    private bool TrackMasterMovement(Actor master)
    {
        // keep track of master movement
        // idea is that if master is moving forward (e.g. running in outdoor or pulling trashpacks in dungeon), we want to closely follow and not stop to cast
        var masterIsMoving = true;
        if (master.Position != _masterPrevPos)
        {
            _masterLastMoved = WorldState.CurrentTime;
            _masterPrevPos = master.Position;
        }
        else if ((WorldState.CurrentTime - _masterLastMoved).TotalSeconds > 0.5f)
        {
            // master has stopped, consider previous movement finished
            _masterMovementStart = _masterPrevPos;
            masterIsMoving = false;
        }
        // else: don't consider master to have stopped moving unless he's standing still for some small time

        return masterIsMoving;
    }

    private void UpdateMovement(Actor player, Actor master, Targeting target, bool gazeOrPyreticImminent, Angle misdirectionAngle, ActionQueue? queueForSprint)
    {
        if (gazeOrPyreticImminent)
        {
            // gaze or pyretic imminent, drop any movement - we should have moved to safe zone already...
            ctrl.NaviTargetPos = null;
            ctrl.NaviTargetVertical = null;
            ctrl.ForceCancelCast = true;
        }
        else if (misdirectionAngle != default && _naviDecision.Destination != null)
        {
            ctrl.NaviTargetPos = _naviDecision.NextTurn == 0 ? _naviDecision.Destination
                : player.Position + (_naviDecision.Destination.Value - player.Position).Rotate(_naviDecision.NextTurn > 0 ? -misdirectionAngle : misdirectionAngle);
            ctrl.AllowInterruptingCastByMovement = true;

            // debug
            //void drawLine(WPos from, WPos to, uint color) => Camera.Instance!.DrawWorldLine(new(from.X, player.PosRot.Y, from.Z), new(to.X, player.PosRot.Y, to.Z), color);
            //var toDest = _naviDecision.Destination.Value - player.Position;
            //drawLine(player.Position, _naviDecision.Destination.Value, 0xff00ff00);
            //drawLine(_naviDecision.Destination.Value, _naviDecision.Destination.Value + toDest.Normalized().OrthoL(), 0xff00ff00);
            //drawLine(player.Position, ctrl.NaviTargetPos.Value, 0xff00ffff);
        }
        else
        {
            var toDest = _naviDecision.Destination != null ? _naviDecision.Destination.Value - player.Position : new();
            var distSq = toDest.LengthSq();

            ctrl.NaviTargetPos = _config.MoveDelay == 0 ? _naviDecision.Destination : WorldState.CurrentTime >= _navStartTime ? _naviDecision.Destination : null;
            ctrl.NaviTargetVertical = master != player ? master.PosRot.Y : null;
            ctrl.AllowInterruptingCastByMovement = player.CastInfo != null && _naviDecision.LeewaySeconds <= player.CastInfo.RemainingTime - 0.5;
            ctrl.ForceCancelCast = false;

            //var cameraFacing = _ctrl.CameraFacing;
            //var dot = cameraFacing.Dot(_ctrl.TargetRot.Value);
            //if (dot < -0.707107f)
            //    _ctrl.TargetRot = -_ctrl.TargetRot.Value;
            //else if (dot < 0.707107f)
            //    _ctrl.TargetRot = cameraFacing.OrthoL().Dot(_ctrl.TargetRot.Value) > 0 ? _ctrl.TargetRot.Value.OrthoR() : _ctrl.TargetRot.Value.OrthoL();

            // sprint, if not in combat and far enough away from destination
            if (player.InCombat ? _naviDecision.LeewaySeconds <= 0 && distSq > 25 : player != master && distSq > 400)
            {
                queueForSprint?.Push(ActionDefinitions.IDSprint, player, ActionQueue.Priority.Minimal + 100);
            }
        }
    }

    public void DrawDebug()
    {
        var player = autorot.WorldState.Party.Player();
        var dist = _naviDecision.Destination != null && player != null ? (_naviDecision.Destination.Value - player.Position).Length() : 0;
        var algo = WorldState.CurrentTime <= _navStartTime ? _naviDecision.DecisionType = NavigationDecision.Decision.Waiting : _naviDecision.DecisionType;
        ImGui.TextUnformatted($"Max-cast={Math.Min(ForceMovementIn, 1000):f3}, afk={_afkMode}, follow={_followMaster}, algo={algo} {_naviDecision.Destination} (d={dist:f3}), master standing for {Math.Clamp((WorldState.CurrentTime - _masterLastMoved).TotalSeconds, 0, 1000):f1}");
    }

    private bool TargetIsForbidden(ulong actorId) => autorot.Hints.ForbiddenTargets.Any(e => e.Actor.InstanceID == actorId);
}
