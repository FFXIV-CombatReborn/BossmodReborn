namespace BossMod.Shadowbringers.Raid.E04Titan;

public enum OID : uint
{
    Boss = 0x298C, // R4.999
    Helper = 0x233C, // R0.500
    MassiveBoulder = 0x298E, // R2.210
    BombBoulder = 0x298D, // R1.300-3.900
}

public enum AID : uint
{
    AutoAttack = 16701, // Boss->player, no cast, single-target
    VoiceOfTheLand = 16631, // Boss->self, 4.0s cast, range 100 circle
    EvilEarth = 16622, // Boss->self, 4.1s cast, single-target
    EvilEarthAOE = 16623, // Helper->self, 5.0s cast, range 10 width 10 rect
    Aftershock = 16820, // Helper->self, 5.0s cast, range 10 width 10 rect
    WeightOfTheLand = 16618, // Boss->self, 2.1s cast, single-target
    WeightOfTheLandAOE = 16619, // Helper->self, 3.0s cast, range 10 width 10 rect
    Stonecrusher = 16633, // Boss->player, 5.0s cast, range 5 circle
    CrumblingDown = 16625, // Boss->self, 3.0s cast, single-target
    CrumblingDownAOE = 16626, // MassiveBoulder->self, 5.0s cast, range 60 circle
    SeismicWave = 16627, // Boss->self, 4.0s cast, range 100 circle
    Geocrush = 16630, // Boss->location, 4.0s cast, range 100 circle
    EarthenGauntlets = 16614, // Boss->self, no cast, single-target
    MassiveLandslideAOE1 = 16636, // Helper->self, 3.0s cast, range 42 width 22 rect
    MassiveLandslideAOE2 = 16635, // Helper->self, 3.0s cast, range 42 width 22 rect
    MassiveLandslide = 16634, // Boss->self, 3.0s cast, single-target
    AftershockLandslide = 16624, // Helper->self, 3.0s cast, range 10 width 10 rect
    EarthenArmor = 16615, // Boss->self, no cast, single-target
    BombBoulders = 16620, // Boss->self, 3.0s cast, single-target
    Bury = 16705, // BombBoulder->self, no cast, range 3+R circle
    CobaltBomb = 16628, // Boss->self, 3.0s cast, single-target
    Explosion = 16621, // BombBoulder->self, 6.0s cast, range 10 circle
    CobaltExplosion = 16629, // BombBoulder->self, 6.0s cast, range 22 circle
    EarthenWheels = 16616, // Boss->self, no cast, single-target
    FaultZoneAOE = 16643, // Helper->self, 3.0s cast, range 42 width 52 rect
    FaultZone = 16642, // Boss->location, 3.0s cast, ???
    EarthenArmorEnd = 16617, // Boss->self, no cast, single-target
    EarthenFury = 16632, // Boss->self, 6.0s cast, single-target
    EarthenFuryAOE = 17382, // Helper->self, 7.5s cast, range 100 circle
    FaultLine = 16641, // Boss->location, no cast, width 10 rect charge
    Magnitude50 = 16644, // Boss->self, 6.0s cast, range 5-40 donut
    Landslide = 16637, // Boss->location, 4.4s cast, visual (jump)
    LandslideAOE = 16638, // Helper->self, 5.0s cast, range 32 width 20 cross
    LeftwardLandslide = 16639, // Boss->self, 3.0s cast, range 21 width 77 rect
    RightwardLandslide = 16640, // Boss->self, 3.0s cast, range 21 width 77 rect
}

sealed class VoiceOfTheLand(BossModule module) : Components.RaidwideCast(module, (uint)AID.VoiceOfTheLand);
sealed class EarthenFury(BossModule module) : Components.RaidwideCast(module, (uint)AID.EarthenFuryAOE);

sealed class EvilEarth(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.EvilEarthAOE, (uint)AID.Aftershock], new AOEShapeRect(10, 5));

sealed class WeightOfTheLand(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WeightOfTheLandAOE, new AOEShapeRect(10, 5));

sealed class Stonecrusher(BossModule module) : Components.BaitAwayCast(module, (uint)AID.Stonecrusher, new AOEShapeCircle(5), centerAtTarget: true);

sealed class CrumblingDown(BossModule module) : Components.ProximityAOEs(module, (uint)AID.CrumblingDownAOE, 20);

sealed class SeismicWave(BossModule module) : Components.GenericLineOfSightAOE(module, (uint)AID.SeismicWave, 100, false)
{
    private DateTime _activation;

    private void Refresh()
    {
        // intact boulders block the wave
        var boulders = Module.Enemies((uint)OID.MassiveBoulder);
        var blockers = new List<(WPos, float)>(boulders.Count);
        for (var i = 0; i < boulders.Count; ++i)
        {
            var b = boulders[i];
            if (!b.IsDeadOrDestroyed && b.ModelState.AnimState2 == 0)
                blockers.Add((b.Position, 1.5f));
        }
        if (blockers.Count == 0)
        {
            Modify(null, []);
            return;
        }
        var activation = Module.PrimaryActor.CastInfo?.Action.ID == (uint)AID.SeismicWave ? Module.CastFinishAt(Module.PrimaryActor.CastInfo) : _activation;
        Modify(Module.PrimaryActor.Position, blockers, activation);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CrumblingDownAOE)
        {
            _activation = WorldState.FutureTime(8);
            Refresh();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            Refresh();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            NumCasts++;
            _activation = default;
            Modify(null, []);
        }
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.MassiveBoulder)
            Refresh();
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.MassiveBoulder)
            Refresh();
    }
}

sealed class Geocrush(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Geocrush, 20);

sealed class MassiveLandslide(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.MassiveLandslideAOE1, (uint)AID.MassiveLandslideAOE2], new AOEShapeRect(42, 11));
sealed class AftershockLandslide(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AftershockLandslide, new AOEShapeRect(10, 5));

sealed class Landslide(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LandslideAOE, new AOEShapeCross(32, 10));
sealed class SideLandslide(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LeftwardLandslide, (uint)AID.RightwardLandslide], new AOEShapeRect(21, 38.5f));

sealed class Explosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion, 10);
sealed class CobaltExplosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CobaltExplosion, 22);

sealed class FaultZoneCharge(BossModule module) : Components.ChargeAOEs(module, (uint)AID.FaultZone, 5f);
sealed class FaultZoneKnockback(BossModule module) : Components.GenericKnockback(module, (uint)AID.FaultZoneAOE)
{
    private WPos _from;
    private WPos _to;
    private DateTime _activation;

    private static WPos NearestOnSegment(WPos p, WPos a, WPos b)
    {
        var ab = b - a;
        var lenSq = ab.LengthSq();
        if (lenSq < 0.01f)
            return a;
        var t = Math.Clamp((p - a).Dot(ab) / lenSq, 0, 1);
        return a + t * ab;
    }

    // knocked away from the nearest point of the charge line
    private readonly Knockback[] _oneKnockback = new Knockback[1];
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
        => _activation == default ? [] : _oneKnockback.Single(new(NearestOnSegment(actor.Position, _from, _to), 15f, _activation));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FaultZone:
                _from = caster.Position;
                _to = spell.LocXZ;
                break;
            case (uint)AID.FaultZoneAOE:
                _activation = Module.CastFinishAt(spell);
                if (_from == default)
                {
                    _from = Module.PrimaryActor.Position;
                    _to = Module.PrimaryActor.Position + 40f * spell.Rotation.ToDirection();
                }
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FaultZoneAOE)
        {
            _activation = default;
            _from = default;
            _to = default;
            ++NumCasts;
        }
    }
}

sealed class Magnitude50(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Magnitude50, new AOEShapeDonut(5, 40));

sealed class E04TitanStates : StateMachineBuilder
{
    public E04TitanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VoiceOfTheLand>()
            .ActivateOnEnter<EarthenFury>()
            .ActivateOnEnter<EvilEarth>()
            .ActivateOnEnter<WeightOfTheLand>()
            .ActivateOnEnter<Stonecrusher>()
            .ActivateOnEnter<CrumblingDown>()
            .ActivateOnEnter<SeismicWave>()
            .ActivateOnEnter<Geocrush>()
            .ActivateOnEnter<MassiveLandslide>()
            .ActivateOnEnter<AftershockLandslide>()
            .ActivateOnEnter<Landslide>()
            .ActivateOnEnter<SideLandslide>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<CobaltExplosion>()
            .ActivateOnEnter<FaultZoneCharge>()
            .ActivateOnEnter<FaultZoneKnockback>()
            .ActivateOnEnter<Magnitude50>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "croizat (ported from upstream and modified by LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 689, NameID = 8350)]
public sealed class E04Titan(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsRect(20, 20));
