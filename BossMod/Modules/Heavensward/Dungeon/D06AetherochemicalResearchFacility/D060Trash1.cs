namespace BossMod.Heavensward.Dungeon.D06AetherochemicalResearchFacility.D060Trash1;

public enum OID : uint
{
    Boss = 0xFA5, // R3.0
    ClockworkHunter = 0xF48, // R1.25
    ClockworkAvenger = 0xF49, // R1.5
    ScrambledEngineer = 0xF59, // R1.225
    ScrambledPaladin = 0xF5A, // R1.225
    ScrambledIronClaw = 0xFA4, // R1.5
    ScrambledIronGiant = 0xFA3 // R2.3
}

public enum AID : uint
{
    AutoAttack1 = 870, // ClockworkHunter/ClockworkAvenger/ScrambledIronGiant->player, no cast, single-target
    AutoAttack2 = 872, // ScrambledPaladin/ScrambledIronClaw/ScrambledEngineer->player, no cast, single-target

    AetherochemicalAmplification = 4550, // Boss->player, no cast, single-target
    PassiveInfraredGuidanceSystem = 4549, // Boss->players, no cast, range 6 circle

    Shred = 608, // ScrambledIronClaw->player, no cast, single-target
    Headspin1 = 4665, // ScrambledPaladin->self, no cast, range 4+R circle
    Headspin2 = 4664, // ScrambledEngineer->self, no cast, range 4+R circle
    GrandSword = 33030, // ScrambledIronGiant->self, 3.0s cast, range 16 120-degree cone
    TheHand = 609, // ScrambledIronClaw->self, 3.0s cast, range 6+R 120-degree cone
    DefensiveManeuvers = 607 // ScrambledIronClaw->self, 3.0s cast, single-target, apply stoneskin
}

class PassiveInfraredGuidanceSystem(BossModule module) : Components.Cleave(module, (uint)AID.PassiveInfraredGuidanceSystem, new AOEShapeCircle(6f), [(uint)OID.Boss], originAtTarget: true);

abstract class HeadSpin(BossModule module, uint aid, OID oid) : Components.Cleave(module, aid, new AOEShapeCircle(5.225f), [(uint)oid])
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.Role is not Role.Melee and not Role.Tank)
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor.Role is not Role.Melee and not Role.Tank)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (pc.Role is not Role.Melee and not Role.Tank)
            base.DrawArenaForeground(pcSlot, pc);
    }
}
class Headspin1(BossModule module) : HeadSpin(module, (uint)AID.Headspin1, OID.ScrambledPaladin);
class Headspin2(BossModule module) : HeadSpin(module, (uint)AID.Headspin2, OID.ScrambledEngineer);

class GrandSword(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrandSword, new AOEShapeCone(16f, 60f.Degrees()));
class TheHand(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheHand, new AOEShapeCone(7.5f, 60f.Degrees()));

class D060EnforcementDroid210States : StateMachineBuilder
{
    public D060EnforcementDroid210States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PassiveInfraredGuidanceSystem>()
            .Raw.Update = () => AllDeadOrDestroyed(D060Trash1.TrashP1);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 38, NameID = 4390, SortOrder = 1)]
public class D060EnforcementDroid210(WorldState ws, Actor primary) : D060Trash1(ws, primary);

class D060ScrambledIronGiantStates : StateMachineBuilder
{
    public D060ScrambledIronGiantStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Headspin1>()
            .ActivateOnEnter<Headspin2>()
            .ActivateOnEnter<GrandSword>()
            .ActivateOnEnter<TheHand>()
            .Raw.Update = () => AllDeadOrDestroyed(D060Trash1.TrashP2);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.ScrambledIronGiant, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 38, NameID = 4337, SortOrder = 2)]
public class D060ScrambledIronGiant(WorldState ws, Actor primary) : D060Trash1(ws, primary);

public abstract class D060Trash1(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(-161.42f, -387.38f), new(-160.94f, -387.16f), new(-160.94f, -357.32f), new(-161.56f, -357.04f), new(-168.47f, -357.05f),
    new(-168.92f, -356.77f), new(-173.49f, -356.49f), new(-182.88f, -356.52f), new(-183.27f, -357), new(-191.29f, -357.04f),
    new(-191.6f, -357.44f), new(-191.59f, -364.77f), new(-191.74f, -365.29f), new(-192.26f, -365.39f), new(-208.3f, -365.39f),
    new(-208.56f, -366.03f), new(-208.63f, -376.44f), new(-208.68f, -377.15f), new(-208.91f, -379.31f), new(-191.94f, -379.47f),
    new(-191.32f, -379.56f), new(-191.13f, -387.25f), new(-161.42f, -387.38f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);

    public static readonly uint[] TrashP1 = [(uint)OID.Boss, (uint)OID.ClockworkHunter, (uint)OID.ClockworkAvenger];
    public static readonly uint[] TrashP2 = [(uint)OID.ScrambledEngineer, (uint)OID.ScrambledIronClaw, (uint)OID.ScrambledIronGiant, (uint)OID.ScrambledPaladin];
    public static readonly uint[] Trash = [.. TrashP1, .. TrashP2];

    protected override bool CheckPull() => IsAnyActorInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Trash);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            hints.PotentialTargets[i].Priority = 0;
        }
    }
}
