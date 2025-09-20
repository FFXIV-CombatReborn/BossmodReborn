namespace BossMod.Global.MaskedCarnivale.Stage02.Act2;

public enum OID : uint
{
    Boss = 0x25C1, //R1.8
    Flan = 0x25C5, //R1.8
    Licorice = 0x25C3 //R=1.8
}

public enum AID : uint
{
    Water = 14271, // Flan->player, 1.0s cast, single-target
    Stone = 14270, // Licorice->player, 1.0s cast, single-target
    Blizzard = 14267, // Boss->player, 1.0s cast, single-target
    GoldenTongue = 14265 // Flan/Licorice/Boss->self, 5.0s cast, single-target
}

sealed class GoldenTongue(BossModule module) : Components.CastInterruptHint(module, (uint)AID.GoldenTongue);

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Gelato is weak to fire spells.\nFlan is weak to lightning spells.\nLicorice is weak to water spells.");
    }
}

sealed class Stage02Act2States : StateMachineBuilder
{
    public Stage02Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GoldenTongue>()
            .ActivateOnEnter<Hints>()
            .Raw.Update = () => AllDeadOrDestroyed(Stage02Act2.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 612, NameID = 8079, SortOrder = 2)]
public sealed class Stage02Act2(WorldState ws, Actor primary) : BossModule(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
{
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.Flan, (uint)OID.Licorice];

    protected override bool CheckPull() => IsAnyActorInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Flan));
        Arena.Actors(Enemies((uint)OID.Licorice));
    }
}
