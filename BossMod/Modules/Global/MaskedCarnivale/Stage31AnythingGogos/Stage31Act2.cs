namespace BossMod.Global.MaskedCarnivale.Stage31.Act2;

public enum OID : uint
{
    Boss = 0x30F7, //R2.0
    Maelstrom = 0x30F9, //R1.0
    Voidzone = 0x1E9684,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target

    Teleport = 23108, // Boss->location, no cast, single-target
    GogoFireIII = 23117, // Boss->self, 3.0s cast, range 60 circle, applies pyretic
    GogoBlizzardIII = 23118, // Boss->self, 3.0s cast, range 6+R circle
    GogoThunderIII = 23119, // Boss->location, 3.0s cast, range 6 circle
    GogoFlare = 23128, // Boss->player, 6.0s cast, range 80 circle
    GogoHoly = 23130, // Boss->player, 6.0s cast, range 6 circle, unavoidable, basically a raidwide
    GogoMeteorVisual = 23120, // Boss->self, 3.0s cast, single-target
    GogoMeteorVisual2 = 22023, // Boss->self, no cast, single-target
    GogoMeteor1 = 23121, // Helper->location, 3.0s cast, range 5 circle
    GogoMeteor2 = 23123, // Helper->location, 12.0s cast, range 100 circle, damage fall off AOE, ca. 16 distance seems fine
    GogoMeteor3 = 23131, // Helper->location, 4.0s cast, range 8 circle
    GogoMeteor4 = 23122, // Helper->location, 11.0s cast, range 8 circle
    GogoMeteor5 = 23129, // Helper->location, 7.0s cast, range 100 circle, damage fall off AOE, ca. 16 distance seems fine
    GogoMeteor6 = 23124, // Helper->location, 10.0s cast, range 100 circle, wipe without diamondback
    Charybdis = 20055, // Boss->self, 3.0s cast, single-target
    Charybdis2 = 20056, // Helper->self, 4.0s cast, range 8 circle
    Icestorm = 23126, // Boss->self, 3.0s cast, single-target
    Icestorm2 = 23127, // Helper->self, no cast, range 60 circle, applies frostbite + heavy, should be removed with exuviation
    AetherialPull = 23125 // Maelstrom->self, 1.0s cast, range 8 circle, pull 40 between centers
}

public enum SID : uint
{
    Pyretic = 960, // Boss->player, extra=0x0
    Frostbite = 268, // Helper->player, extra=0x0
    Heavy = 1107 // Helper->player, extra=0x50
}

sealed class Maelstrom(BossModule module) : Components.Voidzone(module, 8f, GetMaelstrom)
{
    private static List<Actor> GetMaelstrom(BossModule module) => module.Enemies((uint)OID.Maelstrom);
}

sealed class GogoFlareHoly(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.GogoFlare, (uint)AID.GogoHoly]);
sealed class GogoMeteorSmall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GogoMeteor1, 5f);
sealed class GogoMeteorMedCharybdis(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.GogoMeteor3, (uint)AID.GogoMeteor4, (uint)AID.Charybdis], 8f);
sealed class GogoMeteorLarge(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.GogoMeteor2, (uint)AID.GogoMeteor5], 16f);
sealed class GogoMeteorBig(BossModule module) : Components.RaidwideCast(module, (uint)AID.GogoMeteor6, "Use Diamondback!");
sealed class Icestorm(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.Icestorm, (uint)AID.Icestorm2, 0.9d, "Raidwide + Frostbite + Heavy");
sealed class ThunderIII(BossModule module) : Components.VoidzoneAtCastTarget(module, 6f, (uint)AID.GogoThunderIII, GetVoidzones, 0.8d)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.Voidzone);
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

sealed class GogoBlizzardIII(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCircle circle = new(8f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GogoFireIII)
        {
            _aoe = [new(circle, spell.LocXZ, default, Module.CastFinishAt(spell, 5.1d))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GogoBlizzardIII)
        {
            _aoe = [];
        }
    }
}

sealed class GogoFireIIIHint(BossModule module) : Components.CastHint(module, (uint)AID.GogoFireIII, "Pyretic, dodge AOE then stop everything!");

sealed class Pyretic(BossModule module) : Components.StayMove(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GogoFireIII)
        {
            Array.Fill(PlayerStates, new(Requirement.Stay, Module.CastFinishAt(spell)));
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Pyretic)
        {
            Array.Clear(PlayerStates);
        }
    }
}

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.FindStatus((uint)SID.Heavy) != null || actor.FindStatus((uint)SID.Frostbite) != null)
        {
            hints.Add($"Cleanse debuffs!");
        }
    }
}

sealed class Stage31Act2States : StateMachineBuilder
{
    public Stage31Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Maelstrom>()
            .ActivateOnEnter<GogoFlareHoly>()
            .ActivateOnEnter<GogoMeteorSmall>()
            .ActivateOnEnter<GogoMeteorMedCharybdis>()
            .ActivateOnEnter<GogoMeteorLarge>()
            .ActivateOnEnter<GogoMeteorBig>()
            .ActivateOnEnter<ThunderIII>()
            .ActivateOnEnter<Icestorm>()
            .ActivateOnEnter<GogoBlizzardIII>()
            .ActivateOnEnter<GogoFireIIIHint>()
            .ActivateOnEnter<Pyretic>()
            .ActivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 754, NameID = 9908, SortOrder = 2)]
public sealed class Stage31Act2(WorldState ws, Actor primary) : BossModule(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall);
