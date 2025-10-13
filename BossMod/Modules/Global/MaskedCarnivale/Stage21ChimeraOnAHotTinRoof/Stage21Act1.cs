namespace BossMod.Global.MaskedCarnivale.Stage21.Act1;

public enum OID : uint
{
    Boss = 0x272F //R=0.45
}

public enum AID : uint
{
    Blizzard = 14267, // Boss->player, 1.0s cast, single-target
    VoidBlizzard = 15063, // Boss->player, 6.0s cast, single-target
    Icefall = 15064 // Boss->location, 2.5s cast, range 5 circle
}

sealed class Icefall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Icefall, 5f);
sealed class VoidBlizzard(BossModule module) : Components.CastInterruptHint(module, (uint)AID.VoidBlizzard);

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("The first act is fairly easy. Interrupt the Void Blizzards with Flying\nSardine and most of the danger is gone. The Imps are weak against fire spells.\nIn the 2nd act you can start the Final Sting combination at about 50%\nhealth left. (Off-guard->Bristle->Moonflute->Final Sting)");
    }
}

sealed class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("The imps are weak to fire spells and strong against ice.\nInterrupt Void Blizzard with Flying Sardine.");
    }
}

sealed class Stage21Act1States : StateMachineBuilder
{
    public Stage21Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VoidBlizzard>()
            .ActivateOnEnter<Icefall>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>()
            .Raw.Update = () => AllDeadOrDestroyed((uint)OID.Boss);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 631, NameID = 8120, SortOrder = 1)]
public sealed class Stage21Act1 : BossModule
{
    public Stage21Act1(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.Boss));
    }

    protected override bool CheckPull() => IsAnyActorInCombat((uint)OID.Boss);
}
