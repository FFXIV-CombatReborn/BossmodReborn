namespace BossMod.Shadowbringers.Foray.Skirmish.OfSteelAndFlame;

public enum OID : uint
{
    Boss = 0x2FAE,
    Helper = 0x233C,
    SartauvoirTheInferno = 0x2FAD,
    SartHelper = 0x2EA1
}

public enum AID : uint
{
    Exhaust = 21556, // Boss->self, 4.0s cast, range 40 width 10 rect
    MagitekRay = 21558, // Boss->location, 3.5s cast, range 6 circle
    ThermalShock = 21022, // SartHelper->self, 7.0s cast, range 30 circle
    GrandSword = 21553, // Boss->self, 9.0s cast, range 27 120-degree cone
    GrandSwordInstant = 21555, // Boss->self, 0.5s cast, range 27 ?-degree cone
    TridirectionalFlame = 20927, // SartHelper->self, 3.0s cast, range 60 width 8 rect
    PyreticEruption = 20929, // SartHelper->location, 3.0s cast, range 8 circle
    Pyroscatter = 20930, // SartHelper->location, 3.0s cast, range 8 circle
    Pyroburst = 20931, // SartHelper->self, 4.0s cast, range 10 circle
    GrandCrossflame1 = 20928, // SartHelper->self, 4.5s cast, range 40 width 18 cross
    AtomicRay = 21559 // Boss->self, 8.0s cast, range 40 circle
}

sealed class TridirectionalFlame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TridirectionalFlame, new AOEShapeRect(60f, 4f));
sealed class Exhaust(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Exhaust, new AOEShapeRect(40f, 5f));
sealed class MagitekRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagitekRay, 6f);

// 10y is the lethal core; anyone within 30y still takes damage
sealed class ThermalShock(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThermalShock, 10f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Casters.Count != 0)
        {
            var c = Casters[0];
            hints.AddPredictedDamage(Raid.WithSlot().MaskInRadius(c.Origin, 30f), c.Activation);
        }
    }
}

sealed class PyreticEruption(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PyreticEruption, 8f);
sealed class Pyroscatter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Pyroscatter, 8f);
sealed class Pyroburst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Pyroburst, 10f);
sealed class GrandCrossflame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrandCrossflame1, new AOEShapeCross(40f, 9f));
sealed class AtomicRay(BossModule module) : Components.RaidwideCast(module, (uint)AID.AtomicRay);

// rotating 120-degree cones: 3 hits the first time, 6 afterwards
sealed class GrandSword(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCone _shape = new(27f, 60f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GrandSword)
            Sequences.Add(new(_shape, caster.Position, caster.Rotation, -120f.Degrees(), Module.CastFinishAt(spell), 2.8d, NumCasts > 0 ? 6 : 3));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.GrandSword or (uint)AID.GrandSwordInstant)
        {
            ++NumCasts;
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);
        }
    }
}

sealed class CacusStates : StateMachineBuilder
{
    public CacusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Exhaust>()
            .ActivateOnEnter<MagitekRay>()
            .ActivateOnEnter<ThermalShock>()
            .ActivateOnEnter<GrandSword>()
            .ActivateOnEnter<TridirectionalFlame>()
            .ActivateOnEnter<PyreticEruption>()
            .ActivateOnEnter<Pyroscatter>()
            .ActivateOnEnter<Pyroburst>()
            .ActivateOnEnter<GrandCrossflame>()
            .ActivateOnEnter<AtomicRay>()
            .Raw.Update = () =>
            {
                if (!module.PrimaryActor.IsDeadOrDestroyed)
                    return false;
                foreach (var e in module.Enemies((uint)OID.SartauvoirTheInferno))
                {
                    if (!(e.HPRatio < 1f && !e.IsTargetable))
                        return false;
                }
                return true;
            };
    }
}

// ported from upstream, which marks it incomplete
[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "ported from upstream", GroupType = BossModuleInfo.GroupType.ForayFATE, GroupID = 735u, NameID = 1626u)] // bozja skirmishes are keyed by zone + fate id here (upstream: Fate 1626, boss name 9634)
public sealed class Cacus(WorldState ws, Actor primary) : BossModule(ws, primary, new(-234.1f, 260.1f), new ArenaBoundsCircle(50f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.SartauvoirTheInferno));
    }
}
