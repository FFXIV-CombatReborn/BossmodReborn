namespace BossMod.Dawntrail.Savage.M1SBlackCat;

public enum OID : uint
{
    Boss = 0x4329, // R3.993
    CopyCat = 0x432A,
    LeapingTarget = 0x432B,
    Soulshade = 0x432C,
    Helper = 0x233C
}
public enum AID : uint
{
    AutoAttack = 39152, // Boss->player, no cast, single-target
    Teleport = 37640, // Boss->location, no cast, single-target

    QuadrupleCrossingStart = 37948, // Boss->self, 5.0s cast
    QuadrupleCrossingStep = 37949, // Boss->self, 1.0s cast
    QuadrupleCrossingFinish = 37950, // Boss->self, 1.0s cast
    QuadrupleCrossingAOE = 37951, // Helper->self, no cast
    QuadrupleCrossingAOERepeat = 37952, // Helper->self, no cast

    BiscuitMakerFirst = 38037, // Boss->player, 5.0s cast, single-target, tankbuster
    BiscuitMakerSecond = 38038, // Boss->player, no cast, single-target, tankbuster, 2.0s after BiscuitMakerFirst

    NineLives = 37985, //?

    OneTwoPawLeftRight = 37942, // Boss->self, 5.0s cast
    OneTwoPawLeftRightFirst = 37943, // Helper->self, no cast, 180 degree
    OneTwoPawLeftRightSecond = 37944, // Helper->self, no cast, 180 degree
    OneTwoPawRightLeft = 37945, // Boss->self, 5.0s cast
    OneTwoPawRightLeftFirst = 37947, // Helper->self, no cast, 180 degree
    OneTwoPawRightLeftSecond = 37946, // Helper->self, no cast, 180 degree

    OneTwoPawSoulshadeLeftRight = 37991, // Soulshade->self, 5.0s cast
    OneTwoPawSoulshadeLeftRightFirst = 37993, // Helper->self, no cast, 180 degree
    OneTwoPawSoulshadeLeftRightSecond = 37992, // Helper->self, no cast, 180 degree
    OneTwoPawSoulshadeRightLeft = 37988, // Soulshade->self, 5.0s cast
    OneTwoPawSoulshadeRightLeftFirst = 37989, // Helper->self, no cast, 180 degree
    OneTwoPawSoulshadeRightLeftSecond = 37990, // Helper->self, no cast, 180 degree

    QuadrupleSwipe = 37981,
    QuadrupleSwipeAOE = 37982,
    DoubleSwipe = 37983,
    DoubleSwipeAOE = 37984,

    Soulshade = 37986,

    LeapingQuadrupleCrossingCastA = 37975,
    LeapingQuadrupleCrossingCastB = 38959,
    LeapingQuadrupleCrossingStart = 37976,
    LeapingQuadrupleCrossingStep = 37977,
    LeapingQuadrupleCrossingFinish = 37978,
    LeapingQuadrupleCrossingAOE = 37979,
    LeapingQuadrupleCrossingAOERepeat = 37980,

    BloodyScratch = 38036,

    Mouser = 37953,
    MouserTelegraphFirst = 37955,
    MouserTelegraphSecond = 39276,

    Copycat = 37957,

    ShockwaveCast = 37963,
    Shockwave = 37964,

    LeapingOneTwoPawACast = 37965,
    LeapingOneTwoPawAMove = 37969,
    LeapingOneTwoPawAFirst = 37970,
    LeapingOneTwoPawASecond = 37971,
    LeapingOneTwoPawBCast = 37968,
    LeapingOneTwoPawBMove = 37972,
    LeapingOneTwoPawBFirst = 37974,
    LeapingOneTwoPawBSecond = 37973
}

public enum SID : uint
{
    SlashingResistanceDown = 3130
}
