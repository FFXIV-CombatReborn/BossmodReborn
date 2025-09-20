namespace BossMod.Global.MaskedCarnivale.Stage04.Act1;

public enum OID : uint
{
    Boss = 0x25C8, //R=1.65
    Bat = 0x25D2, //R=0.4
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    AutoAttack2 = 6499, // Bat->player, no cast, single-target
    BloodDrain = 14360, // Bat->player, no cast, single-target
    SanguineBite = 14361, // Boss->self, no cast, range 3+R width 2 rect
}

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Trivial act. Enemies here are weak to lightning and fire.\nIn Act 2 the Ram's Voice and Ultravibration combo can be useful.\nFlying Sardine for interrupts can be beneficial.");
    }
}

sealed class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Bats are weak to lightning.\nThe wolf is weak to fire.");
    }
}

sealed class Stage04Act1States : StateMachineBuilder
{
    public Stage04Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>()
            .Raw.Update = () => AllDeadOrDestroyed(Stage04Act1.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 614, NameID = 8086, SortOrder = 1)]
public sealed class Stage04Act1 : BossModule
{
    public Stage04Act1(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
    {
        ActivateComponent<Hints>();
    }
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.Bat];

    protected override bool CheckPull() => IsAnyActorInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Bat));
    }
}
