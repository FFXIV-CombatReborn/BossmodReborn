namespace BossMod.Global.MaskedCarnivale.Stage20.Act1;

public enum OID : uint
{
    Boss = 0x272A //R=4.5
}

public enum AID : uint
{
    Fungah = 14705, // Boss->self, no cast, range 8+R ?-degree cone, knockback 20 away from source
    Fireball = 14706, // Boss->location, 3.5s cast, range 8 circle
    Snort = 14704, // Boss->self, 7.0s cast, range 50+R circle, stun, knockback 30 away from source
    Fireball2 = 14707 // Boss->player, no cast, range 8 circle, 3 casts after snort
}

sealed class Fireball(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Fireball, 8f);
sealed class Snort(BossModule module) : Components.CastHint(module, (uint)AID.Snort, "Use Diamondback!");
sealed class SnortKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Snort, 30f, kind: Kind.AwayFromOrigin, stopAtWall: true);

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Diamondback and Flying Sardine are essential for this stage. The Final\nSting combo (Off-guard->Bristle->Moonflute->Final Sting) can make act 3\nincluding the achievement much easier. Ultros in act 2 and 3 is weak to\nfire.");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        hints.Add("Requirement for achievement: Don't kill any tentacles in act 3", false);
    }
}

sealed class Stage20Act1States : StateMachineBuilder
{
    public Stage20Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<Snort>()
            .ActivateOnEnter<SnortKB>()
            .ActivateOnEnter<Fireball>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 630, NameID = 3046, SortOrder = 1)]
public sealed class Stage20Act1 : BossModule
{
    public Stage20Act1(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
    {
        ActivateComponent<Hints>();
    }
}
