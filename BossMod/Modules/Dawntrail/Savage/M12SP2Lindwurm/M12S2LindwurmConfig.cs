namespace BossMod.Dawntrail.Savage.M12S2Lindwurm;

[ConfigDisplay(Order = 0x160, Parent = typeof(DawntrailConfig))]
public sealed class M12S2LindwurmConfig : ConfigNode
{
    // ============================================================
    // Replication 1 — Strategy Selection
    // ============================================================

    [PropertyDisplay("Replication 1 strategy")]
    public Replication1Strategy Rep1Strategy = Replication1Strategy.CloneRelative;

    public Replication1Effective GetReplication1()
        => new(Rep1Strategy);

    public enum Replication1Strategy
    {
        [PropertyDisplay("Clone Relative")]
        CloneRelative,

        [PropertyDisplay("DN")]
        DN,

        [PropertyDisplay("Static")]
        Static
    }

    public readonly struct Replication1Effective(Replication1Strategy strat)
    {
        public Replication1Strategy Strategy { get; } = strat;

        public bool IsCloneRelative => Strategy == Replication1Strategy.CloneRelative;
        public bool IsDN => Strategy == Replication1Strategy.DN;

        public bool IsStatic => Strategy == Replication1Strategy.Static;
    }

    // ============================================================
    // Replication 2 — Strategy Selection
    // ============================================================

    [PropertyDisplay("Replication 2 strategy")]
    public Replication2Strategy Rep2Strategy = Replication2Strategy.DN;

    // ============================================================
    // Replication 3 — Idyllic Dream
    // ============================================================

    [PropertyDisplay("Replication 3: clockspot → mechanic roles")]
    [GroupDetails(["N", "NE", "E", "SE", "S", "SW", "W", "NW"])]
    public Replication3Role[] Rep3Roles =
    [
        Replication3Role.Stack1,
        Replication3Role.Stack2,
        Replication3Role.Stack3,
        Replication3Role.Stack4,
        Replication3Role.Defam1,
        Replication3Role.Defam2,
        Replication3Role.Defam3,
        Replication3Role.Defam4
    ];

    [PropertyDisplay("Attempt to adjust Idyllic Dream tower logic to mistakes")]
    public bool IdyllicDreamAdjustToMistakes = true;

    [PropertyDisplay("Show Lindwurm's Portent positional hints based on towers taken")]
    public bool ShowLindwurmsPortentHints = true;

    // ============================================================
    // Effective Runtime Views (USED BY MODULE CODE)
    // ============================================================

    public Replication2Effective GetReplication2()
    {
        return Rep2Strategy switch
        {
            Replication2Strategy.DN => Replication2Effective.FromPreset(Replication2Presets.DN),
            Replication2Strategy.BananaCodex => Replication2Effective.FromPreset(Replication2Presets.BC),
            _ => Replication2Effective.FromPreset(Replication2Presets.DN)
        };
    }

    public Replication3Effective GetReplication3()
        => new(Rep3Roles);
}

//
// ============================================================
// Strategy Selection
// ============================================================
//

public enum Replication2Strategy
{
    [PropertyDisplay("DN (true north)")]
    DN,

    [PropertyDisplay("Banana Codex (west = north)")]
    BananaCodex
}

//
// ============================================================
// Clock Positions
// ============================================================
//

public enum Clockspot
{
    [PropertyDisplay("North")] N,
    [PropertyDisplay("Northeast")] NE,
    [PropertyDisplay("East")] E,
    [PropertyDisplay("Southeast")] SE,
    [PropertyDisplay("South")] S,
    [PropertyDisplay("Southwest")] SW,
    [PropertyDisplay("West")] W,
    [PropertyDisplay("Northwest")] NW
}

//
// ============================================================
// Replication 2 Roles
// ============================================================
//

public enum Replication2Role
{
    [PropertyDisplay("Boss")] Boss,
    [PropertyDisplay("None")] None,

    [PropertyDisplay("Cone (CW)")] Cone1,
    [PropertyDisplay("Cone (CCW)")] Cone2,

    [PropertyDisplay("Stack (CW)")] Stack1,
    [PropertyDisplay("Stack (CCW)")] Stack2,

    [PropertyDisplay("Defamation (CW)")] Defam1,
    [PropertyDisplay("Defamation (CCW)")] Defam2,
}

//
// ============================================================
// Replication 3 Roles
// ============================================================
//

public enum Replication3Role
{
    [PropertyDisplay("Stack N1")] Stack1,
    [PropertyDisplay("Stack N2")] Stack2,
    [PropertyDisplay("Stack S1")] Stack3,
    [PropertyDisplay("Stack S2")] Stack4,

    [PropertyDisplay("Defamation N1")] Defam1,
    [PropertyDisplay("Defamation N2")] Defam2,
    [PropertyDisplay("Defamation S1")] Defam3,
    [PropertyDisplay("Defamation S2")] Defam4,
}

//
// ============================================================
// Effective Runtime Containers
// ============================================================
//

public readonly struct Replication2Effective(Clockspot relativeNorth, Replication2Role[] roles)
{
    public Clockspot RelativeNorth { get; } = relativeNorth;
    public Replication2Role[] Roles { get; } = roles;

    public static Replication2Effective FromPreset(Replication2Preset p)
        => new(p.RelativeNorth, p.Roles);

    public Replication2Role GetRole(Clockspot spot)
        => Roles[(int)spot];
}

public readonly struct Replication3Effective(Replication3Role[] roles)
{
    public Replication3Role[] Roles { get; } = roles;

    public Replication3Role GetRole(Clockspot spot)
        => Roles[(int)spot];
}

//
// ============================================================
// Presets (Original BossMod Behavior Preserved)
// ============================================================
//

public readonly struct Replication2Preset(Clockspot relativeNorth, Replication2Role[] roles)
{
    public Clockspot RelativeNorth { get; } = relativeNorth;
    public Replication2Role[] Roles { get; } = roles;
}

public static class Replication2Presets
{
    // DN — true north
    public static readonly Replication2Preset DN = new(
        Clockspot.N,
        [
            Replication2Role.Boss,
            Replication2Role.Cone1,
            Replication2Role.Stack1,
            Replication2Role.Defam1,
            Replication2Role.None,
            Replication2Role.Defam2,
            Replication2Role.Stack2,
            Replication2Role.Cone2
        ]
    );

    // Banana Codex — west treated as north, stack/cone pairing differs
    public static readonly Replication2Preset BC = new(
        Clockspot.W,
        [
            Replication2Role.Boss,
            Replication2Role.Stack1,
            Replication2Role.Cone1,
            Replication2Role.Defam1,
            Replication2Role.None,
            Replication2Role.Defam2,
            Replication2Role.Cone2,
            Replication2Role.Stack2
        ]
    );
}

//
// ============================================================
// Other Shared Types (unchanged)
// ============================================================
//

public enum Cardinal { N, S }

public record struct ReplicatedCloneOrder(Cardinal Group, int Order)
{
    public override readonly string ToString() => $"{Group}{Order + 1}";
}

public enum Element
{
    None,
    Wind,
    Dark,
    Earth,
    Fire
}

//
// ============================================================
// Utility Extensions
// ============================================================
//

// false positive
#pragma warning disable CA1708

public static class WurmExtensions
{
    extension(Clockspot c)
    {
        public Angle Angle => AngleExtensions.Degrees(180 - (int)c * 45);

        public static Clockspot GetClosest(Angle input)
        {
            var values = Enum.GetValues<Clockspot>();
            var best = values[0];
            var bestDist = float.MaxValue;
            for (var i = 0; i < values.Length; ++i)
            {
                var d = Math.Abs(values[i].Angle.Rad - input.Rad);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = values[i];
                }
            }
            return best;
        }
    }

    extension(Replication2Role r)
    {
        public bool IsDefam => r is Replication2Role.None or Replication2Role.Defam1 or Replication2Role.Defam2;
        public bool IsStack => r is Replication2Role.Stack1 or Replication2Role.Stack2;
        public bool IsCone => r is Replication2Role.Cone1 or Replication2Role.Cone2;
    }
}
