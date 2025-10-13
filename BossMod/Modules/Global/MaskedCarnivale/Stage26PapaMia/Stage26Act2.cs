namespace BossMod.Global.MaskedCarnivale.Stage26.Act2;

public enum OID : uint
{
    Boss = 0x2C58, //R=3.6
    Thunderhead = 0x2C59, //R=1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target

    RawInstinct = 18604, // Boss->self, 3.0s cast, single-target
    BodyBlow = 18601, // Boss->player, 4.0s cast, single-target
    VoidThunderII = 18602, // Boss->location, 3.0s cast, range 4 circle
    LightningBolt = 18606, // Thunderhead->self, no cast, range 8 circle
    DadJoke = 18605, // Boss->self, no cast, range 25+R 120-degree cone, knockback 15, dir forward
    VoidThunderIII = 18603 // Boss->player, 4.0s cast, range 20 circle
}

public enum SID : uint
{
    CriticalStrikes = 1797, // Boss->Boss, extra=0x0
    Electrocution = 271 // Boss/2C59->player, extra=0x0
}

public enum IconID : uint
{
    BaitKnockback = 23 // player
}

sealed class Thunderhead(BossModule module) : Components.Voidzone(module, 8f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.Thunderhead);
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

sealed class DadJoke(BossModule module) : Components.GenericKnockback(module)
{
    private DateTime _activation;
    private readonly Thunderhead _aoe = module.FindComponent<Thunderhead>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_activation != default)
            return new Knockback[1] { new(Module.PrimaryActor.Position, 15f, _activation, direction: Angle.FromDirection(actor.Position - Module.PrimaryActor.Position), kind: Kind.DirForward) };
        return [];
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.BaitKnockback)
        {
            _activation = WorldState.FutureTime(5d);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.DadJoke)
        {
            _activation = default;
        }
    }

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

sealed class VoidThunderII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidThunderII, 4);
sealed class RawInstinct(BossModule module) : Components.CastHint(module, (uint)AID.RawInstinct, "Prepare to dispel buff");
sealed class VoidThunderIII(BossModule module) : Components.RaidwideCast(module, (uint)AID.VoidThunderIII, "Raidwide + Electrocution");
sealed class BodyBlow(BossModule module) : Components.SingleTargetCast(module, (uint)AID.BodyBlow, "Soft Tankbuster");

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"{Module.PrimaryActor.Name} will cast Raw Instinct, which causes all his hits to crit.\nUse Eerie Soundwave to dispel it.\n{Module.PrimaryActor.Name} is weak against earth and strong against lightning attacks.");
    }
}

sealed class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Module.PrimaryActor.FindStatus((uint)SID.CriticalStrikes) != null)
        {
            hints.Add($"Dispel {Module.PrimaryActor.Name} with Eerie Soundwave!");
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.FindStatus((uint)SID.Electrocution) != null)
        {
            hints.Add("Electrocution on you! Cleanse it with Exuviation.");
        }
    }
}

sealed class Stage26Act2States : StateMachineBuilder
{
    public Stage26Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RawInstinct>()
            .ActivateOnEnter<VoidThunderII>()
            .ActivateOnEnter<VoidThunderIII>()
            .ActivateOnEnter<BodyBlow>()
            .ActivateOnEnter<Thunderhead>()
            .ActivateOnEnter<DadJoke>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 695, NameID = 9231, SortOrder = 2)]
public sealed class Stage26Act2 : BossModule
{
    public Stage26Act2(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall)
    {
        ActivateComponent<Hints>();
    }
}
