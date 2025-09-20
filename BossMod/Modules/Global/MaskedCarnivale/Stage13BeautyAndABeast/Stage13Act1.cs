namespace BossMod.Global.MaskedCarnivale.Stage13.Act1;

public enum OID : uint
{
    Boss = 0x26F5, //R=1.4
    Vodoriga = 0x26F6, //R=1.2
}

public enum AID : uint
{
    Attack = 6497, // Boss/Vodoriga->player, no cast, single-target
    Mow = 14879, // Boss->self, 3.0s cast, range 6+R 120-degree cone
}

sealed class Mow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mow, new AOEShapeCone(7.4f, 60f.Degrees()));

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("The first act is trivial, almost anything will work.\nFor act 2 having Flying Sardine is recommended.");
    }
}

sealed class Stage13Act1States : StateMachineBuilder
{
    public Stage13Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<Mow>()
            .Raw.Update = () => AllDeadOrDestroyed(Stage13Act1.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 623, NameID = 8104, SortOrder = 1)]
public sealed class Stage13Act1 : BossModule
{
    public Stage13Act1(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
    {
        ActivateComponent<Hints>();
    }
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.Vodoriga];

    protected override bool CheckPull() => IsAnyActorInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Vodoriga));
    }
}
