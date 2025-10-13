namespace BossMod.Global.MaskedCarnivale.Stage28;

public enum OID : uint
{
    Boss = 0x2CD2, //R=2.6
    UndeadSerf1 = 0x2CD4, // R=0.5
    UndeadSerf2 = 0x2CD3, // R=0.5
    UndeadSoldier = 0x2CD5, // R=0.5
    UndeadWarrior = 0x2CD6, // R=1.9
    UndeadGravekeeper = 0x2CD7, // R=0.75
    NecrobaneVoidzone = 0x1EA9FA,
    MagitekExplosive = 0x2CEC, //R=0.8
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 19052, // Boss->player, no cast, single-target
    AutoAttack2 = 6499, // UndeadSerf1/UndeadWarrior/UndeadGravekeeper->player, no cast, single-target
    AutoAttack3 = 19068, // UndeadSoldier->player, no cast, single-target

    DoomImpending = 19051, // Boss->self, 8.0s cast, range 80 circle, heal to full before cast ends
    MarchOfTheDraugar = 19057, // Boss->self, 3.0s cast, single-target, summons 4 different sets of adds
    DeathThroes = 19061, // UndeadSerf2->player, no cast, single-target, pull 20 between hitboxes, player becomes unable to move
    DeathThroesRecover = 19062, // UndeadSerf2->self, no cast, range 100 circle
    Necrobane = 19059, // Boss->location, 4.0s cast, range 6 circle
    MegaDeath = 19055, // Boss->self, 6.0s cast, range 80 circle, deadly if not in voidzone
    HelblarShriek = 19084, // Boss->self, 4.0s cast, range 50 circle, high dmg if in Necrobane voidzone
    FireIII = 19069, // UndeadSoldier->player, 5.0s cast, single-target
    FuneralPyre = 19056, // Boss->self, 6.0s cast, range 40 circle, deadly if in Necrobane voidzone
    ButterflyFloat = 19065, // UndeadWarrior->player, 3.0s cast, single-target
    GlassPunch = 19063, // UndeadWarrior->self, no cast, range 6+R 90-degree cone
    Brainstorm = 19054, // Boss->self, 4.0s cast, range 60 circle, forced march debuffs
    BilrostSquallVisual = 19081, // Boss->self, 9.0s cast, single-target
    BilrostSquall = 19082, // Helper->location, 9.0s cast, range 10 circle
    NailInTheCoffin = 19066, // UndeadGravekeeper->player, no cast, single-target
    VengefulSoul = 19067, // UndeadGravekeeper->location, 3.0s cast, range 6 circle
    Cackle = 19053, // Boss->player, 4.0s cast, single-target, high dmg, interruptible
    Catapult = 19064 // UndeadWarrior->location, 3.0s cast, range 6 circle
}

public enum SID : uint
{
    RightFace = 1961, // Boss->player, extra=0x0
    LeftFace = 1960, // Boss->player, extra=0x0
    ForwardMarch = 1958, // Boss->player, extra=0x0
    AboutFace = 1959 // Boss->player, extra=0x0
}

sealed class DoomImpending(BossModule module) : Components.CastHint(module, (uint)AID.DoomImpending, "Heal to full before cast ends!");
sealed class MarchOfTheDraugar(BossModule module) : Components.CastHint(module, (uint)AID.MarchOfTheDraugar, "Summons adds! (Kill with fire!)");
sealed class NecrobaneVoidzone(BossModule module) : Components.PersistentInvertibleVoidzoneByCast(module, 6f, GetVoidzones, (uint)AID.MegaDeath)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.NecrobaneVoidzone);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
            {
                voidzones[index++] = z;
            }
        }
        return voidzones[..index];
    }
}

sealed class NecrobaneCatapultVengefulSoul(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Necrobane, (uint)AID.Catapult, (uint)AID.VengefulSoul], 6f);
sealed class HelblarShriekFuneralPyre(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.HelblarShriek, (uint)AID.FuneralPyre]);
sealed class BilrostSquall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BilrostSquall, 10f);
sealed class Cackle(BossModule module) : Components.CastInterruptHint(module, (uint)AID.Cackle);

sealed class Brainstorm(BossModule module) : Components.StatusDrivenForcedMarch(module, 2f, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace)
{
    private readonly BilrostSquall _aoe = module.FindComponent<BilrostSquall>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            if (aoes[i].Check(pos))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }
}

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"{Module.PrimaryActor.Name} will spawn upto 4 waves of adds which are weak to fire.\nA way to quickly heal yourself to full is mandatory and a ranged fire\nability such as Mustard Bomb and Flying Sardine for interrupts\nare highly recommended.");
    }
}

sealed class Stage28States : StateMachineBuilder
{
    public Stage28States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Cackle>()
            .ActivateOnEnter<BilrostSquall>()
            .ActivateOnEnter<Brainstorm>()
            .ActivateOnEnter<NecrobaneCatapultVengefulSoul>()
            .ActivateOnEnter<HelblarShriekFuneralPyre>()
            .ActivateOnEnter<NecrobaneVoidzone>()
            .ActivateOnEnter<MarchOfTheDraugar>()
            .ActivateOnEnter<DoomImpending>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 697, NameID = 9233)]
public sealed class Stage28 : BossModule
{
    public Stage28(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall)
    {
        ActivateComponent<Hints>();
    }
    private static readonly uint[] adds = [(uint)OID.UndeadSerf1, (uint)OID.UndeadSerf2, (uint)OID.UndeadGravekeeper, (uint)OID.UndeadSoldier, (uint)OID.UndeadWarrior];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, adds);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Boss => 0,
                _ => 1
            };
        }
    }
}
