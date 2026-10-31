using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using RebornMaterial;

namespace BossMod.RebornUI;

internal sealed partial class SettingsWindow
{
    private static readonly (string, string)[] _availableAICommands =
    [
        ( "on", "Enables the AI." ),
        ( "off", "Disables the AI." ),
        ( "toggle", "Toggles the AI on/off." ),
        ( "targetmaster", "Toggles the focus on target leader." ),
        ( "follow slotX", "Follows the specified slot, eg. Slot1." ),
        ( "follow name", "Follows the specified party member by name." ),
        ( "ui", "Toggles the AI menu." ),
        ( "forbidactions", "Toggles the forbidding of actions. (only for autorotation)" ),
        ( "forbidactions on/off", "Sets forbid actions to on or off. (only for autorotation)" ),
        ( "forbidmovement", "Toggles the forbidding of movement." ),
        ( "forbidmovement on/off", "Sets forbid movement to on or off." ),
        ( "idlewhilemounted", "Toggles the idling while mounted." ),
        ( "idlewhilemounted on/off", "Sets idle while mounted to on or off." ),
        ( "followcombat", "Toggles following during combat." ),
        ( "followcombat on/off", "Sets following during combat to on or off." ),
        ( "followmodule", "Toggles following during active boss module." ),
        ( "followmodule on/off", "Sets following during active boss module to on or off." ),
        ( "followoutofcombat", "Toggles following during out of combat." ),
        ( "followoutofcombat on/off", "Sets following target out of combat to on or off." ),
        ( "followtarget", "Toggles following targets during combat." ),
        ( "followtarget on/off", "Sets following target during combat to on or off." ),
        ( "positional X", "Switch to positional when following targets. (any, rear, flank, front)" ),
        ( "maxdistancetarget X", "Sets max distance to target. (default = 2.6)" ),
        ( "maxdistanceslot X", "Sets max distance to slot. (default = 1)" ),
        ( "mindistance X", "Sets min distance to hitbox. (default = 0)" ),
        ( "prefdistance X", "Sets preferred distance to forbidden zones. (default = 0)" ),
        ( "movedelay X", "Sets AI movement decision delay. (default = 0)" ),
        ( "obstaclemaps", "Toggles loading obstacle maps." ),
        ( "obstaclemaps on/off", "Sets the loading of obstacle maps to on or off." ),
        ( "setpresetname X", "Sets an autorotation preset for the AI, eg. setpresetname vbm default." )
    ];

    private static readonly (string, string)[] _autorotationCommands =
    [
        ( "ar clear", "Clear current preset; autorotation will do nothing unless plan is active" ),
        ( "ar disable", "Force disable autorotation; no actions will be executed automatically even if plan is active." ),
        ( "ar set Preset", "Start executing specified preset." ),
        ( "ar toggle", "Force disable autorotation if not already; otherwise clear overrides." ),
        ( "ar toggle Preset", "Start executing specified preset unless it's already active; clear otherwise" ),
        ( "ar ui", "Toggle autorotation ui." ),
    ];

    private static readonly (string, string)[] _availableOtherCommands =
    [
        ( "", "Opens this window." ),
        ( "restorerotation", "Toggle restore character orientation after action use setting." ),
        ( "resetcolors", "Resets all colors to their default values." ),
        ( "d", "Opens the debug menu." ),
        ( "r", "Opens the replay menu." ),
        ( "r on/off", "Starts/stops recording a replay." ),
        ( "gc", "Triggers the garbage collection." ),
        ( "radar", "Toggles radar display." ),
        ( "radar on/off", "Sets radar display to on or off." ),
        ( "radar reset", "Moves the radar window back to the middle of the screen." ),
        ( "cfg", "Lists all configs; /bmr cfg <config> <field> <value> changes a setting." )
    ];

    private static void DrawCommands()
    {
        DrawCommandCard("cmd_general", "General", FontAwesomeIcon.Terminal, "/bmr", _availableOtherCommands, 0);
        DrawCommandCard("cmd_ai", "AI", FontAwesomeIcon.Robot, "/bmrai", _availableAICommands, 1);
        DrawCommandCard("cmd_ar", "Autorotation", FontAwesomeIcon.Bolt, "/bmr", _autorotationCommands, 2);
    }

    private static void DrawCommandCard(string id, string title, FontAwesomeIcon icon, string prefix, (string, string)[] commands, int index)
    {
        using var card = M3Card.Begin(id, title, icon, M3.AccentFor(index), subtitle: $"Start with {prefix}");
        using var scope = ImRaii.PushId(id);
        var button = M3Widgets.IconButtonSize;
        foreach (var (args, description) in commands)
        {
            var command = args.Length > 0 ? $"{prefix} {args}" : prefix;
            var row = M3SettingRow.Begin(command, description, new(button, button), M3.Scheme.Primary);
            ImGui.SetCursorScreenPos(row.ControlPosition);
            if (M3Widgets.IconButton($"##{command}", FontAwesomeIcon.Copy, "Copy to clipboard"))
            {
                ImGui.SetClipboardText(command);
                M3Snackbar.Show($"Copied {command}");
            }
            M3SettingRow.End(row);
        }
    }
}
