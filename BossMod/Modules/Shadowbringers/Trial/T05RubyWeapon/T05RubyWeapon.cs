namespace BossMod.Shadowbringers.Trial.T05RubyWeapon;

public enum OID : uint
{
    Boss = 0x2CDC,
    NaelForm = 0x2CDF,
    Helper = 0x233C,
    RavensImageHelper = 0x2DAD,
    RavensImageRage = 0x2CE0,
    RavensImageGrief = 0x2CE1,
    MarkIIComet = 0x2CDE,
    Comet = 0x2D9A
}

public enum AID : uint
{
    Flexiclaw = 19090,
    Ravensclaw = 19091,
    SpikeOfFlameLarge = 19092,
    SpikeOfFlameSmall = 19093,
    Liquefaction = 19094,
    Undermine = 19095,
    Retract = 19096,
    Helicoclaw = 19097,
    CutAndRun = 19100,
    RubyDynamics = 19104,
    Ravensflight = 19105,
    OptimizedUltima = 19112,
    OptimizedUltimaEnrage = 19113,
    MeteorProject = 19114,
    DalamudImpact = 19118,
    MagitekComet = 19120,
    MagitekMeteor = 19121,
    MagitekMeteorLOS = 19122,
    Burst = 19124,
    MarkIIMagitekComet = 19126,
    HeavyImpact = 19129,
    BradamanteVisual = 19131,
    Bradamante = 19132,
    NegativePersonae = 19133,
    RubyClaw = 19135,
    RubyClawRepeat = 19136,
    RubyFlare = 19137,
    HomingLasersVisual = 19138,
    HomingLasers = 19139,
    HighPoweredHomingLasersVisual = 19140,
    HighPoweredHomingLasers = 19141,
    RubyRay = 19142,
    Stamp = 19143,
    Outrage = 19144,
    HelicoclawFirst = 19714,
    HelicoclawRest = 19715,
    Landing = 20011,
    MeteorStream = 20071,
    MeteorStreamVisual = 20081
}

public enum SID : uint
{
    BlindToRage = 2208,
    BlindToGrief = 2209,
    PallOfRage = 2210,
    PallOfGrief = 2211
}

public enum IconID : uint
{
    HighPoweredHomingLasers = 62,
    HomingLasers = 139,
    MeteorStream = 224
}

sealed class OptimizedUltima(BossModule module) : Components.RaidwideCast(module, (uint)AID.OptimizedUltima);
sealed class Raidwides(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.RubyFlare, (uint)AID.Outrage]);
sealed class OptimizedUltimaEnrage(BossModule module) : Components.CastHint(module, (uint)AID.OptimizedUltimaEnrage, "Enrage!", true);
sealed class Stamp(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Stamp);
sealed class RubyClaw(BossModule module) : Components.SingleTargetCast(module, (uint)AID.RubyClaw, "Tankbuster x5");
sealed class RubyRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RubyRay, new AOEShapeRect(40f, 8.5f));
sealed class Ravensclaw(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Ravensclaw, 8f);
sealed class SpikeOfFlameLarge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpikeOfFlameLarge, 8f);
sealed class SpikeOfFlameSmall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpikeOfFlameSmall, 6f);
sealed class Helicoclaw(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.HelicoclawFirst, (uint)AID.HelicoclawRest], 6f);
sealed class HomingLasers(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.HomingLasers, (uint)AID.HomingLasers, 6f, 4.9d);
sealed class HighPoweredHomingLasers(BossModule module) : Components.StackWithIcon(module, (uint)IconID.HighPoweredHomingLasers, (uint)AID.HighPoweredHomingLasers, 6f, 5.5d, 4, maxCasts: 2);
sealed class RubyDynamics(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RubyDynamics, new AOEShapeDonut(8.5f, 40f));
sealed class Bradamante(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Bradamante, new AOEShapeRect(40f, 3f));
sealed class Liquefaction(BossModule module) : Components.CastHint(module, (uint)AID.Liquefaction, "Quicksand: stand on the cracks!");
sealed class Undermine(BossModule module) : Components.CastHint(module, (uint)AID.Undermine, "Cracks explode: move away from them!");
sealed class Ravensflight(BossModule module) : Components.CastHint(module, (uint)AID.Ravensflight, "Dodge the charges!");
sealed class MeteorStream(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.MeteorStream, (uint)AID.MeteorStream, 4f, 4.1d);
sealed class Landing(BossModule module) : Components.CastHint(module, (uint)AID.Landing, "Proximity damage: move away from the comets!");
sealed class Burst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Burst, 8f);

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

sealed class MagitekMeteor(BossModule module) : Components.GenericLineOfSightAOE(module, (uint)AID.MagitekMeteorLOS, 50f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Landing)
            Refresh(WorldState.FutureTime(10.5d));
        else if (spell.Action.ID == (uint)AID.MagitekMeteorLOS)
            Modify(null, []);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.Comet && Origin != null)
            Refresh(NextExplosion);
    }

    private void Refresh(DateTime activation)
    {
        var comets = Module.Enemies((uint)OID.Comet);
        var blockers = new List<(WPos, float)>(comets.Count);
        for (var i = 0; i < comets.Count; ++i)
        {
            var c = comets[i];
            if (!c.IsDestroyed)
                blockers.Add((c.Position, c.HitboxRadius));
        }
        Modify(Arena.Center, blockers, activation);
    }
}

sealed class T05RubyWeaponStates : StateMachineBuilder
{
    public T05RubyWeaponStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<OptimizedUltima>()
            .ActivateOnEnter<Raidwides>()
            .ActivateOnEnter<OptimizedUltimaEnrage>()
            .ActivateOnEnter<Stamp>()
            .ActivateOnEnter<RubyClaw>()
            .ActivateOnEnter<RubyRay>()
            .ActivateOnEnter<Ravensclaw>()
            .ActivateOnEnter<SpikeOfFlameLarge>()
            .ActivateOnEnter<SpikeOfFlameSmall>()
            .ActivateOnEnter<Helicoclaw>()
            .ActivateOnEnter<HomingLasers>()
            .ActivateOnEnter<HighPoweredHomingLasers>()
            .ActivateOnEnter<RubyDynamics>()
            .ActivateOnEnter<Bradamante>()
            .ActivateOnEnter<Liquefaction>()
            .ActivateOnEnter<Undermine>()
            .ActivateOnEnter<Ravensflight>()
            .ActivateOnEnter<MeteorStream>()
            .ActivateOnEnter<Landing>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<Personae>()
            .ActivateOnEnter<MagitekMeteor>()
            .Raw.Update = () =>
            {
                var nael = module.Enemies((uint)OID.NaelForm);
                return nael.Count != 0 && nael[0].IsDead || module.PrimaryActor.IsDestroyed && nael.Count == 0;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 717, NameID = 9250)]
public sealed class T05RubyWeapon(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f))
{
    private static readonly uint[] _enemies = [(uint)OID.NaelForm, (uint)OID.RavensImageRage, (uint)OID.RavensImageGrief, (uint)OID.MarkIIComet];

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            if (e.Actor.OID is (uint)OID.MarkIIComet or (uint)OID.RavensImageRage or (uint)OID.RavensImageGrief)
                e.Priority = 1;
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, _enemies);
    }
}
