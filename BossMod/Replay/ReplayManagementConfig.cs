namespace BossMod;

[ConfigDisplay(Name = "Replays", Order = 0)]
public sealed class ReplayManagementConfig : ConfigNode
{
    [PropertyDisplay("Show replay management UI")]
    public bool ShowUI = false;

    [PropertyDisplay("Show chat alert when a duty with no module is entered or recorded")]
    public bool ImportantDutyAlert = true;

    [PropertyDisplay("Auto record replays on duty start/end or outdoor module start/end")]
    public bool AutoRecord = false;

    [PropertyDisplay("Auto record in Duty Recorder replays", depends: nameof(AutoRecord))]
    public bool AutoARR = false;

    [PropertyDisplay("Anonymize replays", tooltip: "If this option is disabled, replays will contain personally identifying information for your character and any other player you see during the recording - specifically, names and content IDs.")]
    public bool Anonymize = false;

    [PropertyDisplay("Max replays to keep before removal")]
    [PropertySlider(0, 1000)]
    public int MaxReplays = 0;

    [PropertyDisplay("Record and store server packets in the replay")]
    public bool RecordServerPackets = false;

    [PropertyDisplay("Filter noisy server packets (movement, HP/MP, inventory) when recording", depends: nameof(RecordServerPackets))]
    public bool FilterNoisyServerPackets = true;

    [PropertyDisplay("Dump server packets into dalamud.log")]
    public bool DumpServerPackets = false;

    [PropertyDisplay("Ignore packets for other players when dumping to dalamud.log")]
    public bool DumpServerPacketsPlayerOnly = false;

    [PropertyDisplay("Dump client packets into dalamud.log")]
    public bool DumpClientPackets = false;

    [PropertyDisplay("Format for recorded logs")]
    public ReplayLogFormat WorldLogFormat = ReplayLogFormat.BinaryCompressed;
    public string ReplayFolder = "";
}
