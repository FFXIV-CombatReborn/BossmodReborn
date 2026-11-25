namespace BossMod.Shadowbringers.Extreme.Ex4RubyWeapon;

public enum OID : uint
{
    Boss = 0x2CE2,
    RubyBit = 0x2CE3,
    NaelForm = 0x2CE6,
    RavensImageRage = 0x2CE7,
    RavensImageGrief = 0x2CE8,
    RavensImageHelper = 0x2DAE,
    Helper = 0x233C
}

public enum AID : uint
{
    Flexiclaw = 19090,
    Retract = 19096,
    Helicoclaw = 19097,
    Ravensflight = 19105,
    MagitekMeteor = 19121,
    MarkIIMagitekComet = 19126,
    NegativePersonae = 19133,
    OptimizedUltima = 19134,
    Ravensclaw = 19148,
    SpikeOfFlameLarge = 19149,
    SpikeOfFlameSmall = 19150,
    Liquefaction = 19151,
    Undermine = 19152,
    MagitekBit = 19153,
    MagitekRay = 19154,
    HelicoclawFirst1 = 19155,
    MagitekCharge = 19156,
    HomingLasers = 19158,
    HighPoweredHomingLasers = 19160,
    Screech = 19182,
    Landing = 19185,
    Burst = 19186,
    NegativeAffect = 19192,
    ChangeOfHeart = 19196,
    GreaterMemory = 19197,
    NegativeAura = 19198,
    RubyClaw = 19199,
    RubyClawRepeat = 19200,
    RubyFlare = 19201,
    RubyRay = 19202,
    Stamp = 19203,
    Outrage = 19204,
    CutAndRun = 19205,
    RubyDynamics = 19209,
    OptimizedUltimaEnrage = 19245,
    HelicoclawRest1 = 19247,
    HelicoclawFirst2 = 19716,
    HelicoclawRest2 = 19717,
    MeteorStream = 20072,
    MeteorStreamVisual = 20082,
    MeteorMine = 20105,
    LunarDynamo = 20144,
    IronChariot = 20145
}

public enum SID : uint
{
    UnwaveringWill = 2207,
    BlindToRage = 2208,
    BlindToGrief = 2209,
    PallOfRage = 2210,
    PallOfGrief = 2211
}

public enum IconID : uint
{
    MeteorStream = 224
}

sealed class Raidwides(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.OptimizedUltima, (uint)AID.RubyFlare, (uint)AID.Outrage, (uint)AID.ChangeOfHeart]);
sealed class OptimizedUltimaEnrage(BossModule module) : Components.CastHint(module, (uint)AID.OptimizedUltimaEnrage, "Enrage!", true);
sealed class Stamp(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Stamp);
sealed class RubyClaw(BossModule module) : Components.SingleTargetCast(module, (uint)AID.RubyClaw, "Tankbuster x5");
sealed class RubyRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RubyRay, new AOEShapeRect(40f, 8.5f));
sealed class Ravensclaw(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Ravensclaw, 8f);
sealed class SpikeOfFlameLarge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpikeOfFlameLarge, 8f);
sealed class SpikeOfFlameSmall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpikeOfFlameSmall, 6f);
sealed class Helicoclaw(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.HelicoclawFirst1, (uint)AID.HelicoclawFirst2], 6f);
sealed class MagitekRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagitekRay, new AOEShapeRect(45f, 1f));
sealed class HomingLasers(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.HomingLasers, 6f);
sealed class HighPoweredHomingLasers(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.HighPoweredHomingLasers, 6f, 4);
sealed class RubyDynamics(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RubyDynamics, new AOEShapeDonut(8.5f, 40f));
sealed class Liquefaction(BossModule module) : Components.CastHint(module, (uint)AID.Liquefaction, "Quicksand: get to stable ground!");
sealed class Undermine(BossModule module) : Components.CastHint(module, (uint)AID.Undermine, "Cracks become deadly: stay out of them!");
sealed class Ravensflight(BossModule module) : Components.CastHint(module, (uint)AID.Ravensflight, "Dodge the charges!");
sealed class CutAndRun(BossModule module) : Components.CastHint(module, (uint)AID.CutAndRun, "Spread to the side edges!");
sealed class MeteorStream(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.MeteorStream, (uint)AID.MeteorStream, 4f, 4.1d);
sealed class NegativeAura(BossModule module) : Components.CastGaze(module, (uint)AID.NegativeAura);
sealed class IronChariot(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IronChariot, 9f);
sealed class LunarDynamo(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LunarDynamo, new AOEShapeDonut(5f, 22f));
sealed class Screech(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Screech, 10f);
sealed class Burst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Burst, 6f);
sealed class Landing(BossModule module) : Components.CastHints(module, [(uint)AID.Landing, (uint)AID.MagitekCharge], "Proximity damage: move away from the impact!");
sealed class MagitekMeteor(BossModule module) : Components.CastHint(module, (uint)AID.MagitekMeteor, "Hide behind a comet!");
sealed class NegativeAffect(BossModule module) : Components.CastHint(module, (uint)AID.NegativeAffect, "Lead the heads into the opposite colored image!");

sealed class Personae(BossModule module) : Components.GenericInvincible(module)
{
    private readonly List<Actor> _forbidden = [];

    protected override ReadOnlySpan<Actor> ForbiddenTargets(int slot, Actor actor)
    {
        _forbidden.Clear();
        var rage = actor.FindStatus((uint)SID.PallOfRage) != null;
        var grief = actor.FindStatus((uint)SID.PallOfGrief) != null;
        if (!rage && !grief)
            return [];
        AddImages(Module.Enemies((uint)OID.RavensImageRage), rage, grief);
        AddImages(Module.Enemies((uint)OID.RavensImageGrief), rage, grief);
        return CollectionsMarshal.AsSpan(_forbidden);
    }

    private void AddImages(List<Actor> images, bool rage, bool grief)
    {
        for (var i = 0; i < images.Count; ++i)
        {
            var a = images[i];
            if (rage && a.FindStatus((uint)SID.BlindToRage) != null || grief && a.FindStatus((uint)SID.BlindToGrief) != null)
                _forbidden.Add(a);
        }
    }
}

sealed class Ex4RubyWeaponStates : StateMachineBuilder
{
    public Ex4RubyWeaponStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Raidwides>()
            .ActivateOnEnter<OptimizedUltimaEnrage>()
            .ActivateOnEnter<Stamp>()
            .ActivateOnEnter<RubyClaw>()
            .ActivateOnEnter<RubyRay>()
            .ActivateOnEnter<Ravensclaw>()
            .ActivateOnEnter<SpikeOfFlameLarge>()
            .ActivateOnEnter<SpikeOfFlameSmall>()
            .ActivateOnEnter<Helicoclaw>()
            .ActivateOnEnter<MagitekRay>()
            .ActivateOnEnter<HomingLasers>()
            .ActivateOnEnter<HighPoweredHomingLasers>()
            .ActivateOnEnter<RubyDynamics>()
            .ActivateOnEnter<Liquefaction>()
            .ActivateOnEnter<Undermine>()
            .ActivateOnEnter<Ravensflight>()
            .ActivateOnEnter<CutAndRun>()
            .ActivateOnEnter<MeteorStream>()
            .ActivateOnEnter<NegativeAura>()
            .ActivateOnEnter<IronChariot>()
            .ActivateOnEnter<LunarDynamo>()
            .ActivateOnEnter<Screech>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<Landing>()
            .ActivateOnEnter<MagitekMeteor>()
            .ActivateOnEnter<NegativeAffect>()
            .ActivateOnEnter<Personae>()
            .Raw.Update = () =>
            {
                var nael = module.Enemies((uint)OID.NaelForm);
                return nael.Count != 0 && nael[0].IsDead || module.PrimaryActor.IsDestroyed && nael.Count == 0;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 718, NameID = 9250, PlanLevel = 80)]
public sealed class Ex4RubyWeapon(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f))
{
    private static readonly uint[] _enemies = [(uint)OID.NaelForm, (uint)OID.RavensImageRage, (uint)OID.RavensImageGrief];

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            if (e.Actor.OID is (uint)OID.RavensImageRage or (uint)OID.RavensImageGrief)
                e.Priority = 1;
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, _enemies);
    }
}
