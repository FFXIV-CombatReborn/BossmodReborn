namespace BossMod.Shadowbringers.Alliance.A13GoliathTank;

public enum OID : uint
{
    Boss = 0x2C7E,
    FlightUnit = 0x2C80,
    Helper = 0x233C,
    MediumExploder = 0x2C7F // R1.200, x0 (spawn during fight)
}

public enum AID : uint
{
    EnergyRingCast = 18738, // Boss->self, 3.0s cast, single-target
    EnergyRingVisual1 = 18740, // Boss->self, no cast, single-target
    EnergyRingVisual2 = 18741, // Boss->self, no cast, single-target
    EnergyRingVisual3 = 18742, // Boss->self, no cast, single-target
    EnergyRingVisual4 = 18739, // Boss->self, no cast, single-target
    EnergyRing1 = 18743, // Helper->self, 4.0s cast, range 12 circle
    EnergyRing2 = 18744, // Helper->self, 4.0s cast, range 12-24 donut
    EnergyRing3 = 18745, // Helper->self, 4.0s cast, range 24-36 donut
    EnergyRing4 = 18746, // Helper->self, 4.0s cast, range 36-48 donut
    ConvenientSelfDestruction = 18748, // MediumExploder->self, no cast, range 10 circle
    LaserTurret = 18747, // Boss->self, 4.0s cast, range 85 width 10 rect
    AutoAttackUnit = 18189, // FlightUnit->player, no cast, single-target
    FlightDash1 = 18749, // FlightUnit->self, no cast, single-target
    FlightDash2 = 18751, // FlightUnit->location, no cast, single-target
    AreaBombingManeuver = 18754, // FlightUnit->self, 3.0s cast, single-target
    BallisticImpact = 18755, // Helper->location, 1.0s cast, range 4 circle
    A360DegreeBombingManeuver = 18753, // FlightUnit->self, 5.0s cast, range 100 circle
    LightfastBlade = 18752 // FlightUnit->self, 5.0s cast, range 48 180-degree cone
}

public enum IconID : uint
{
    Spread = 189, // MediumExploder->self
    Marker = 23 // player->self
}

public enum TetherID : uint
{
    Generic = 17 // MediumExploder/player->player/FlightUnit
}

sealed class A360DegreeBombingManeuver(BossModule module) : Components.RaidwideCast(module, (uint)AID.A360DegreeBombingManeuver);
sealed class LaserTurret(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LaserTurret, new AOEShapeRect(85f, 5f));
sealed class LightfastBlade(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightfastBlade, new AOEShapeCone(48f, 90f.Degrees()));

sealed class EnergyRing(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(12f), new AOEShapeDonut(12f, 24f), new AOEShapeDonut(24f, 36f), new AOEShapeDonut(36f, 48f)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EnergyRing1)
            AddSequence(caster.Position, Module.CastFinishAt(spell));
    }

    public override void Update()
    {
        if (Module.PrimaryActor.IsDeadOrDestroyed)
            Sequences.Clear();
        base.Update();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var index = spell.Action.ID switch
        {
            (uint)AID.EnergyRing1 => 0,
            (uint)AID.EnergyRing2 => 1,
            (uint)AID.EnergyRing3 => 2,
            (uint)AID.EnergyRing4 => 3,
            _ => -1
        };
        if (index >= 0)
            AdvanceSequence(index, caster.Position, WorldState.FutureTime(2d));
    }
}

sealed class ConvenientSelfDestruction(BossModule module) : Components.GenericBaitAway(module, (uint)AID.ConvenientSelfDestruction, centerAtTarget: true)
{
    private static readonly AOEShapeCircle _shape = new(10f);

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.MediumExploder && WorldState.Actors.Find(tether.Target) is Actor target)
            CurrentBaits.Add(new(source, target, _shape, WorldState.FutureTime(9.1d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != WatchedAction)
            return;
        ++NumCasts;
        for (var i = CurrentBaits.Count - 1; i >= 0; --i)
        {
            if (CurrentBaits[i].Source == caster)
                CurrentBaits.RemoveAt(i);
        }
    }
}

sealed class AreaBombingBait(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeRect _shape = new(60f, 4f);
    private static readonly AOEShapeCircle _keepAway = new(8f);

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (WorldState.Actors.Find(tether.Target) is Actor target && target.OID == (uint)OID.FlightUnit)
            CurrentBaits.Add(new(target, source, _shape, WorldState.FutureTime(5.1d)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BallisticImpact)
            CurrentBaits.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var baits = ActiveBaitsOn(actor);
        if (baits.Count != 0)
            hints.AddForbiddenZone(_keepAway, baits[0].Source.Position, default, baits[0].Activation);
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

sealed class AreaBombingExa(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(4f), (uint)AID.BallisticImpact)
{
    private int IndexOfLine(Angle rotation)
    {
        for (var i = 0; i < Lines.Count; ++i)
        {
            if (Lines[i].Rotation.AlmostEqual(rotation, 0.05f))
                return i;
        }
        return -1;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction && IndexOfLine(caster.Rotation) < 0)
            Lines.Add(new(caster.Position, caster.Rotation.ToDirection() * 4f, Module.CastFinishAt(spell), 1.05d, 7, 4, caster.Rotation));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != WatchedAction)
            return;
        ++NumCasts;
        var l = IndexOfLine(caster.Rotation);
        if (l >= 0)
        {
            AdvanceLine(Lines[l], caster.Position);
            if (Lines[l].ExplosionsLeft <= 0)
                Lines.RemoveAt(l);
        }
    }
}

sealed class A13GoliathTankStates : StateMachineBuilder
{
    public A13GoliathTankStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EnergyRing>()
            .ActivateOnEnter<ConvenientSelfDestruction>()
            .ActivateOnEnter<LaserTurret>()
            .ActivateOnEnter<AreaBombingBait>()
            .ActivateOnEnter<AreaBombingExa>()
            .ActivateOnEnter<A360DegreeBombingManeuver>()
            .ActivateOnEnter<LightfastBlade>()
            .Raw.Update = () =>
            {
                if (!module.PrimaryActor.IsDeadOrDestroyed)
                    return false;
                foreach (var f in module.Enemies((uint)OID.FlightUnit))
                {
                    if (!f.IsDeadOrDestroyed && f.HPMP.CurHP != 1)
                        return false;
                }
                return true;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 700, NameID = 9138, SortOrder = 4)]
public sealed class A13GoliathTank(WorldState ws, Actor primary) : BossModule(ws, primary, new(-780f, 555f), new ArenaBoundsCircle(30f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.FlightUnit));
    }
}
