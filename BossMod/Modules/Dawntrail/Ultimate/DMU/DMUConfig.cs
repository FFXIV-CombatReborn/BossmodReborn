namespace BossMod.Dawntrail.Ultimate.DMU;

[ConfigDisplay(Order = 0x400, Parent = typeof(DawntrailConfig))]
public sealed class DMUConfig : ConfigNode {
    [SectionStart("Strategy Settings")]
    [PropertyDisplay("P1 Graven Image 1 strategy")]
    public P1GravenImage1Strategy P1GravenImage1 = P1GravenImage1Strategy.GravenImage1LPDU;

    public enum P1GravenImage1Strategy {
        [PropertyDisplay("None")]
        GravenImage1None,

        [PropertyDisplay("LPDU / Kefka Bin (NA)")]
        GravenImage1LPDU,
    }

    [PropertyDisplay("P1 Wave Cannon conga priority", tooltip: "Conga order is West to East")]
    [GroupDetails(["1", "2", "3", "4", "5", "6", "7", "8"])]
    [GroupPreset("HHTTMMRR", [3, 2, 1, 0, 4, 5, 6, 7])]
    public GroupAssignmentUnique P1WaveCannonAssignment = new() { Assignments = [3, 2, 1, 0, 4, 5, 6, 7] };

    [PropertyDisplay("P1 Wave Cannon safe spot hints", tooltip: "Will display roughly where to stand for your bait")]
    public bool P1WaveCannonHints = false;

    public enum P1DoubleTroubleStrategy {
        [PropertyDisplay("None")]
        DoubleTroubleNone,

        [PropertyDisplay("LPDU")]
        DoubleTroubleLPDU,

        [PropertyDisplay("Kefka Bin (NA)")]
        DoubleTroubleKefkaBin
    }

    [PropertyDisplay("P1 DoubleTrouble strategy")]
    public P1DoubleTroubleStrategy P1DoubleTrouble = P1DoubleTroubleStrategy.DoubleTroubleLPDU;

    [PropertyDisplay("P1 DoubleTrouble Knockback safe spot hints", tooltip: "Will display roughly where to stand for the stack, but you may still need to adjust")]
    public bool P1DoubleTroubleKnockbackHints = false;

    public enum P1GravenImage2Strategy {
        [PropertyDisplay("None")]
        GravenImage2None,

        [PropertyDisplay("LPDU - Uptime Graven Image 2")]
        GravenImage2LPDUUptime,

        [PropertyDisplay("Kefka Bin (NA)")]
        GravenImage2KefkaBin,
    }

    [PropertyDisplay("P1 Graven Image 2 strategy")]
    public P1GravenImage2Strategy P1GravenImage2 = P1GravenImage2Strategy.GravenImage2LPDUUptime;

    public enum P1TeleTrouncingStrategy {
        [PropertyDisplay("None")]
        TeleTrouncingNone,

        [PropertyDisplay("Modified Xolo")]
        Modified_Xolo,

        [PropertyDisplay("Freaky arrow CW box (Merry Go Round)")]
        Freaky_Arrow,
    }

    [PropertyDisplay("P1 TeleTrouncing strategy")]
    public P1TeleTrouncingStrategy P1TeleTrouncing = P1TeleTrouncingStrategy.Freaky_Arrow;

    [PropertyDisplay("P1 TeleTrouncing Static Spots")]
    public bool P1TeleTrouncingStaticSpots = true;

    [PropertyDisplay("P1 Graven Image 3 strategy")]
    public P1GravenImage3Strategy P1GravenImage3 = P1GravenImage3Strategy.GravenImage3LPDU;

    public enum P1GravenImage3Strategy {
        [PropertyDisplay("None")]
        GravenImage3None,

        [PropertyDisplay("LPDU")]
        GravenImage3LPDU,

        [PropertyDisplay("Kefka Bin (NA)")]
        GravenImage3KefkaBin,
    }

    public enum P2ForsakenStrategy {
        [PropertyDisplay("EU meow braindead strategy using markerless")]
        Meow_Markerless,

        [PropertyDisplay("EU meow braindead strategy using DN ZENITH markers")]
        Meow_DN_ZENITH_Markers,

        [PropertyDisplay("NA Kroxy-Rinon (341 Melee Flex)")]
        Kroxy_Rinon_Melee_Flex,
    }

    [PropertyDisplay("P2 Forsaken strategy")]
    public P2ForsakenStrategy P2Forsaken = P2ForsakenStrategy.Meow_Markerless;

    [SectionStart("AI Settings")]
    [PropertyDisplay("P1 RevoltingRuinIII always around true north?", tooltip: "Only used by AI - Doesn't do anything at the moment")]
    public bool P1RevoltingRuinIIIAlwaysAroundTrueNorth = true;

    [PropertyDisplay("P1 RevoltingRuinIII 1st bait", tooltip: "Only used by AI - Doesn't do anything at the moment")]
    [PropertyCombo("MT", "OT")]
    public bool P1RevoltingRuinIIIBait1OT = false;

    [PropertyDisplay("P1 HyperDrive always around true north?", tooltip: "Only used by AI - Doesn't do anything at the moment")]
    public bool P1HyperDriveAlwaysAroundTrueNorth = true;

    [PropertyDisplay("P1 HyperDrive 1st bait", tooltip: "Only used by AI - Doesn't do anything at the moment")]
    [PropertyCombo("MT", "OT")]
    public bool P1HyperDriveBait1OT = false;

    [SectionStart("Debug Settings")]
    [PropertyDisplay("P1 Graven Image 1 knockback additional hints", tooltip: "Only used for debugging - H1 & R1 roles will still have to move a fair amount " +
                                                                              " if north is safe, other roles will only have to do slight adjustments")]
    public bool P1GravenImage1KnockbackAdditionalHints = false;
}
