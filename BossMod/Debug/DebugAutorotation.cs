using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;

namespace BossMod;

sealed class DebugAutorotation(RotationModuleManager autorot)
{
    private readonly UITree _tree = new();

    public void Draw()
    {
        var player = autorot.Bossmods.WorldState.Party[autorot.PlayerSlot];
        if (player == null)
            return;
        new AIHintsVisualizer(autorot.Hints, autorot.Bossmods.WorldState, player, 3f, default).Draw(_tree);

        if (ImGui.Button("Gaze!"))
            autorot.Hints.ForbiddenDirections.Add((player.Rotation, 45f.Degrees(), default));
    }
}
