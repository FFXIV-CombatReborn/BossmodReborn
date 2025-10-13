namespace BossMod.Global.MaskedCarnivale.Stage22.Act1;

public enum OID : uint
{
    Boss = 0x26FC, //R=1.2
    BossAct2 = 0x26FE //R=3.75, needed for pullcheck, otherwise it activates additional modules in act2
}

public enum AID : uint
{
    Fulmination = 14901 // Boss->self, no cast, range 50+R circle, wipe if failed to kill grenade in one hit
}

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"The first act is easy. Kill the grenades in one hit each or they will wipe you.\nIf you gear is bad consider using 1000 Needles.\nFor the 2nd act you should bring Sticky Tongue. In the 2nd act you can start\nthe Final Sting combination at about 50%\nhealth left. (Off-guard->Bristle->Moonflute->Final Sting)");
    }
}

sealed class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"Kill the grenades in one hit each or they will wipe you. They got 543 HP.");
    }
}

sealed class Stage22Act1States : StateMachineBuilder
{
    public Stage22Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>()
            .Raw.Update = () => AllDeadOrDestroyed((uint)OID.Boss);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 632, NameID = 8122, SortOrder = 1)]
public sealed class Stage22Act1 : BossModule
{
    public Stage22Act1(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.Boss));
    }

    protected override bool CheckPull()
    {
        return !IsAnyActorTargetable((uint)OID.BossAct2) && Raid.Player()!.LastFrameMovement != default && IsAnyActorTargetable((uint)OID.Boss); // they die in one hit
    }
}
