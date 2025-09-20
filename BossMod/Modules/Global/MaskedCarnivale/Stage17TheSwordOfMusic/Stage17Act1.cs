namespace BossMod.Global.MaskedCarnivale.Stage17.Act1;

public enum OID : uint
{
    Boss = 0x2720, //R=2.0
    RightClaw = 0x271F //R=2.0
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss/RightClaw->player, no cast, single-target
    TheHand = 14760, // RightClaw/Boss->self, 3.0s cast, range 6+R 120-degree cone, knockback away from source, dist 10
    Shred = 14759 // Boss/RightClaw->self, 2.5s cast, range 4+R width 4 rect, stuns player
}

sealed class TheHand(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheHand, new AOEShapeCone(8f, 60f.Degrees()));
sealed class Shred(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shred, new AOEShapeRect(6f, 2f));

sealed class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (!Module.PrimaryActor.IsDead)
            hints.Add($"{Module.PrimaryActor.Name} counters magical damage!");
        var rightClaws = Module.Enemies((uint)OID.RightClaw);
        var count = rightClaws.Count;
        if (count == 0)
            return;
        var claw = rightClaws[0];
        if (!claw.IsDead)
            hints.Add($"{claw.Name} counters physical damage!");
    }
}

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"The {Module.PrimaryActor.Name} counters magical attacks, the {Module.Enemies((uint)OID.RightClaw)[0].Name} counters physical\nattacks. If you have healing spells you can just tank the counter damage\nand kill them however you like anyway. All opponents in this stage are\nweak to lightning.\nThe Ram's Voice and Ultravibration combo can be used in Act 2.");
    }
}

sealed class Stage17Act1States : StateMachineBuilder
{
    public Stage17Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<Shred>()
            .ActivateOnEnter<TheHand>()
            .ActivateOnEnter<Hints2>()
            .Raw.Update = () => AllDeadOrDestroyed(Stage17Act1.Hands);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 627, NameID = 8115, SortOrder = 1)]
public sealed class Stage17Act1 : BossModule
{
    public Stage17Act1(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall)
    {
        ActivateComponent<Hints>();
    }
    public static readonly uint[] Hands = [(uint)OID.Boss, (uint)OID.RightClaw];

    protected override bool CheckPull() => IsAnyActorInCombat(Hands);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.RightClaw));
    }

    // protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    // {
    //     var count = hints.PotentialTargets.Count;
    //     for (var i = 0; i < count; ++i)
    //     {
    //         var e = hints.PotentialTargets[i];
    //         e.Priority = e.Actor.OID switch
    //         {
    //             (uint)OID.Boss or (uint)OID.RightClaw => 0, // TODO: ideally left claw should only be attacked with magical abilities and right claw should only be attacked with physical abilities
    //             _ => 0
    //         };
    //     }
    // }
}
