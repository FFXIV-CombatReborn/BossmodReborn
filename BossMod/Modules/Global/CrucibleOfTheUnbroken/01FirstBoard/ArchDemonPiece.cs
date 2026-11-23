namespace BossMod.Global.CrucibleOfTheUnbroken.FirstBoard.ArchDemonPiece;

public enum OID : uint
{
    ArchDemonPiece = 0x4B88, // R3.000, x?
    AbyssalLance = 0x4B89, // R1.500, x?
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 49682, // 4B88->player, no cast, single-target

    AbyssalChargeVisual = 46875, // 4B88->self, 3.0s cast, single-target : Summons Abyssal Lance to shoot lasers at pc.
    AbyssalCharge = 46876, // 4B89->self, 3.0s cast, range 40 width 4 rect
    DismemberVisual = 46877, // 4B88->self, 3.0s cast, single-target
    DismemberHelper = 46878, // 233C->self, 4.0s cast, single-target
    Dismember = 46879, // 233C->self, 4.5s cast, range 35 width 8 rect
    AbyssalTransfixion = 46880, // 4B88->self, 3.0s cast, single-target
    SwordAppearLarge = 46881, // 233C->self, no cast, single-target
    AbyssalTransfixion1 = 46882, // 233C->self, 3.7s cast, range 6 circle
    SwordAppearSmall = 46883, // 233C->self, no cast, single-target
    AbyssalTransfixion2 = 46884, // 233C->self, 3.5s cast, range 3 circle
    AbyssalSwing = 46885, // 4B88->self, no cast, single-target
    AbyssalSwing1 = 46886, // 233C->self, 5.7s cast, range 40 180.000-degree cone
}

sealed class AbyssalLanceAdds(BossModule module) : Components.AddsPointless(module, (uint)OID.AbyssalLance);

//AbyssalLance shoots narrow laser rectangles towards pc.
sealed class AbyssalCharge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AbyssalCharge, new AOEShapeRect(40f, 2f));

// Lasers that spread across the arena and fire off one after another; the next lane is highlighted so pc can step into lanes that already fired.
sealed class Dismember : Components.SimpleAOEs
{
    public Dismember(BossModule module) : base(module, (uint)AID.Dismember, new AOEShapeRect(35f, 4f), 4)
    {
        MaxDangerColor = 1;
    }
}

// Laser swords that appear above a r6 circle and step down into the ground.
sealed class AbyssalTransfixion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AbyssalTransfixion1, 6f);

// Laser swords that appear above the aoe circle and stab back down where pc was standing.
sealed class AbyssalTransfixion2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AbyssalTransfixion2, 3f);

// Laser axe swing that covers half the arena.
sealed class AbyssalSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AbyssalSwing1, new AOEShapeCone(40f, 90f.Degrees()));

sealed class ArchDemonPieceStates : StateMachineBuilder
{
    public ArchDemonPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AbyssalLanceAdds>()
            .ActivateOnEnter<AbyssalCharge>()
            .ActivateOnEnter<Dismember>()
            .ActivateOnEnter<AbyssalTransfixion>()
            .ActivateOnEnter<AbyssalTransfixion2>()
            .ActivateOnEnter<AbyssalSwing>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    PrimaryActorOID = (uint)OID.ArchDemonPiece,
    Contributors = "wen",
    GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken,
    GroupID = 1088u,
    NameID = 14533u,
    SortOrder = 2)]

public sealed class ArchDemonPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(520f, 0f), new ArenaBoundsRect(20f, 14.8f));
