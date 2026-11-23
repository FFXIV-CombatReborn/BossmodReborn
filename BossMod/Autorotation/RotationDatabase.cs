using System.IO;

namespace BossMod.Autorotation;

public sealed class RotationDatabase
{
    public readonly PresetDatabase Presets;
    public readonly PlanDatabase Plans;

    public RotationDatabase(DirectoryInfo rootPath, FileInfo rebornPresets, FileInfo vbmPresets)
    {
        if (!rootPath.Exists)
            rootPath.Create();
        Presets = new(rootPath.FullName + "/presets", rebornPresets, vbmPresets);
        Plans = new(rootPath.FullName + "/plans");
    }
}
