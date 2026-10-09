using BossMod.AI;
using BossMod.Autorotation.MiscAI;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BossMod;

// diagnostics for interactions between BMR movement and casts started by us or an external rotation (RSR):
// which movement system is active, what each side believes the cast state is, what we report over ipc and how recent casts ended
// also hosts the fake aoe test that forces cast-interrupting dodges to check that no cast/interrupt loop forms
sealed class DebugCastMovement(WorldState ws, AIHints hints, ActionManagerEx amex, MovementOverride move, RotationSolverRebornModule rsr)
{
    public void Draw()
    {
        DrawState();
        ImGui.Separator();
        DrawFakeAoe();
        ImGui.Separator();
        DrawHistory();
    }

    private void DrawState()
    {
        var aiActive = AIManager.Instance?.Beh != null;
        var autorotMovement = NormalMovement.Instance != null;
        var system = aiActive ? "Automovement (AI)" : autorotMovement ? "Autorotation (Automatic movement module)" : "none";
        ImGui.TextUnformatted($"Movement system: {system}{(aiActive && autorotMovement ? " - automatic movement module is loaded but idle while automovement is on" : "")}");

        var rsrInstalled = rsr.IsInstalled;
        ImGui.TextUnformatted($"RSR: installed={rsrInstalled}, autorotation active={rsrInstalled && rsr.IsAutorotationActive}");
        ImGui.TextUnformatted($"Prevent movement while casting: config={ActionManagerEx.Config.PreventMovingWhileCasting}, effective={amex.PreventMovingWhileCasting}, input blocked now={move.MovementBlocked}");

        // live client state vs worldstate: the worldstate only shows our own casts once the server has confirmed them
        var castRemaining = amex.CastTimeRemaining;
        ImGui.TextUnformatted($"Cast (live): {(castRemaining > 0f ? $"{amex.CastAction}, {castRemaining:f2}s left" : "none")}, move might interrupt={amex.MoveMightInterruptCast}");
        var player = ws.Party.Player();
        var cast = player?.CastInfo;
        ImGui.TextUnformatted($"Cast (worldstate): {(cast != null ? $"{cast.Action}, {cast.RemainingTime:f2}s left, effect happened={cast.EventHappened}" : "none")}");

        var forced = hints.ForcedMovement;
        ImGui.TextUnformatted($"Hints: forced movement={(forced != null ? $"{forced.Value.Length():f2}y" : "none")}, hold casts={hints.HoldCasts}, max cast time={Seconds(hints.MaxCastTime)}, force cancel mechanic/other={hints.ForceCancelCastMechanic}/{hints.ForceCancelCastOther}");
        ImGui.TextUnformatted($"IPC Movement.IsMoving (what RSR sees): {forced != null || hints.HoldCasts}");

        if (AIManager.Instance is { } ai)
        {
            var ctrl = ai.Controller;
            var dest = ctrl.NaviTargetPos is { } pos && player != null ? $"{(pos - player.Position).Length():f2}y away" : "none";
            ImGui.TextUnformatted($"Automovement: destination={dest}, leeway={Seconds(ctrl.NaviLeewaySeconds)}, override={ctrl.AllowInterruptingCastByMovement}, force cancel mechanic/other={ctrl.ForceCancelCastMechanicAI}/{ctrl.ForceCancelCastOtherAI}");
        }

        ImGui.TextUnformatted($"Input: user={Dir(move.UserMove)}, actual={Dir(move.ActualMove)}, desired direction={(move.DesiredDirection is { } d ? Utils.Vec3String(d) : "none")}");
    }

    private void DrawFakeAoe()
    {
        ImGui.TextWrapped("Fake AOE: repeatedly drops a circle under the player so the active movement system has to dodge with no leeway. With RSR casting, expect one interrupted cast per drop followed by an instant - never back-to-back interrupts.");

        var enabled = FakeAoeTest.Enabled;
        if (ImGui.Checkbox("Enable fake AOE under player", ref enabled))
        {
            FakeAoeTest.Enabled = enabled;
            FakeAoeTest.Reset();
        }
        ImGui.DragFloat("Radius##fakeaoe", ref FakeAoeTest.Radius, 0.1f, 1f, 15f, "%.1f y");
        ImGui.DragFloat("Activation delay##fakeaoe", ref FakeAoeTest.ActivationDelay, 0.05f, 0f, 10f, "%.2f s");
        ImGui.DragFloat("Re-drop interval##fakeaoe", ref FakeAoeTest.Interval, 0.1f, 0.5f, 30f, "%.1f s");

        if (FakeAoeTest.Enabled)
        {
            var now = ws.CurrentTime;
            var toActivation = (FakeAoeTest.Activation - now).TotalSeconds;
            ImGui.TextUnformatted($"Drops: {FakeAoeTest.Drops}, current at {Pos(FakeAoeTest.Center)}, {(toActivation > 0d ? $"activates in {toActivation:f2}s" : "active")}, next drop in {(FakeAoeTest.NextDrop - now).TotalSeconds:f2}s");
        }
    }

    private void DrawHistory()
    {
        var tracker = amex.CastTracker;
        var history = tracker.History;
        var count = history.Count;
        int interrupted = 0, streak = 0, longestStreak = 0;
        for (var i = 0; i < count; ++i)
        {
            if (history[i].Outcome == CastTracker.Outcome.Interrupted)
            {
                ++interrupted;
                longestStreak = Math.Max(longestStreak, ++streak);
            }
            else if (history[i].Outcome != CastTracker.Outcome.Casting)
            {
                streak = 0;
            }
        }
        // casts interrupted back to back are the signature of a cast/interrupt loop
        ImGui.TextUnformatted($"Recent casts: {count}, interrupted: {interrupted}, longest back-to-back interrupt streak: {longestStreak}{(longestStreak >= 3 ? " (might be a loop)" : "")}");
        ImGui.SameLine();
        if (ImGui.Button("Clear##casthistory"))
        {
            tracker.Clear();
            return; // count above is stale now; the table is drawn again next frame
        }

        using var table = ImRaii.Table("cast_history", 9, ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit);
        if (!table)
        {
            return;
        }

        ImGui.TableSetupColumn("Action");
        ImGui.TableSetupColumn("Ago");
        ImGui.TableSetupColumn("Duration");
        ImGui.TableSetupColumn("Outcome");
        ImGui.TableSetupColumn("Left");
        ImGui.TableSetupColumn("BMR move frames");
        ImGui.TableSetupColumn("Manual move frames");
        ImGui.TableSetupColumn("Drift");
        ImGui.TableSetupColumn("Likely cause");
        ImGui.TableHeadersRow();

        var now = DateTime.Now;
        for (var i = count - 1; i >= 0; --i) // newest first
        {
            var e = history[i];
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(e.Action.ToString());
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{(now - e.Start).TotalSeconds:f1}s");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{e.Duration:f2}s");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(e.Outcome.ToString());
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(e.Outcome == CastTracker.Outcome.Interrupted ? $"{e.RemainingAtEnd:f2}s" : "");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(e.AutoMoveFrames.ToString());
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(e.UserMoveFrames.ToString());
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{e.Drift:f2}y");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(e.LikelyCause);
        }
    }

    private static string Seconds(float s) => s >= 1e6f ? "inf" : $"{s:f2}s";
    private static string Dir(WDir d) => d == default ? "none" : $"({d.X:f2}, {d.Z:f2})";
    private static string Pos(WPos p) => $"({p.X:f1}, {p.Z:f1})";
}
