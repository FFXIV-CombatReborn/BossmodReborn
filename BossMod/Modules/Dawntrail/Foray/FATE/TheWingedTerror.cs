namespace BossMod.Dawntrail.Foray.FATE.TheWingedTerror;

public enum OID : uint
{
    Boss = 0x46C1, // R4.68
    Petrifog1 = 0x46C2, // R1.3
    Petrifog2 = 0x4822, // R1.3
    Petrifog3 = 0x4821, // R1.3
    Petrifog4 = 0x4820, // R1.3
    Petrifog5 = 0x481E, // R1.3
    Petrifog6 = 0x481F, // R1.3
    Petrifog7 = 0x481D // R1.3
}

public enum AID : uint
{
    AutoAttack = 42900, // Boss->player, no cast, single-target
    Teleport = 44481, // Boss->location, no cast, single-target

    GaleCannon = 41274, // Boss->self, 5.0s cast, range 40 width 10 rect
    Petrisphere = 41272, // Boss->self, 4.0s cast, single-target
    SphereShatter = 41273 // Petrifog1/Petrifog6/Petrifog5/Petrifog7/Petrifog2/Petrifog3/Petrifog4->self, 2.0s cast, range 7 circle
}

sealed class GaleCannon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GaleCannon, new AOEShapeRect(40f, 5f));
sealed class SphereShatter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SphereShatter, 7f);

sealed class TheWingedTerrorStates : StateMachineBuilder
{
    public TheWingedTerrorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GaleCannon>()
            .ActivateOnEnter<SphereShatter>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.ForayFATE, GroupID = 1018, NameID = 1965)]
public sealed class TheWingedTerror(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
