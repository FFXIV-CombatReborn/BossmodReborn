using BossMod.Components;

namespace BossMod.Dawntrail.Dungeon.D10Underkeep.D102SoldierS0;

public enum OID : uint
{
    Boss = 0x47AD, // R2.76
    SoldierS0Clone = 0x47AE, // R2.76
    AddBlock = 0x47AF, // R3.2
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Teleport = 42576, // Boss->location, no cast, single-target

    FieldOfScorn = 42579, // Boss->self, 5.0s cast, range 45 circle
    ThunderousSlash = 43136, // Boss->player, 5.0s cast, single-target

    SectorBisectorVisual1 = 42562, // Boss->self, 5.0s cast, single-target, cleave left
    SectorBisectorVisual2 = 42563, // Boss->self, 5.0s cast, single-target, cleave right
    SectorBisectorVisualClone1 = 42568, // SoldierS0Clone->self, no cast, single-target
    SectorBisectorVisualClone2 = 43163, // SoldierS0Clone->self, no cast, single-target
    SectorBisectorVisualClone3 = 42564, // SoldierS0Clone->self, no cast, single-target
    SectorBisectorVisualClone4 = 42569, // SoldierS0Clone->self, no cast, single-target
    SectorBisectorVisualClone5 = 43164, // SoldierS0Clone->self, no cast, single-target
    SectorBisectorVisualClone6 = 42565, // SoldierS0Clone->self, no cast, single-target
    SectorBisector1 = 42566, // Helper->self, 0.5s cast, range 45 180-degree cone, cleave left
    SectorBisector2 = 42567, // Helper->self, 0.5s cast, range 45 180-degree cone, cleave right

    OrderedFireVisual = 42572, // Boss->self, 2.0+2,0s cast, single-target
    OrderedFire = 42573, // AddBlock->self, 5.0s cast, range 55 width 8 rect
    StaticForceVisual = 42574, // Boss->self, 5.0s cast, single-target
    StaticForce = 42575, // Helper->self, no cast, range 60 30-degree cone
    ElectricExcessVisual = 42570, // Boss->self, 4.0+1,0s cast, single-target
    ElectricExcess = 43139 // Helper->players, 5.0s cast, range 6 circle, spread
}

public enum TetherID : uint
{
    BisectorInitial = 313, // SoldierS0Clone->SoldierS0Clone
    BisectorEnd = 327 // SoldierS0Clone->SoldierS0Clone
}

public enum IconID : uint
{
    StaticForce = 591 // Boss->players
}

sealed class FieldOfScorn(BossModule module) : Components.RaidwideCast(module, (uint)AID.FieldOfScorn);
sealed class ThunderousSlash(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ThunderousSlash);
sealed class OrderedFire(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OrderedFire, new AOEShapeRect(55f, 4f));
sealed class ElectricExcess(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.ElectricExcess, 6f);
sealed class StaticForce(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60f, 15f.Degrees()), (uint)IconID.StaticForce, (uint)AID.StaticForce, 5.1d);

sealed class SectorBisector(BossModule module) : Components.GenericAOEs(module)
{
    // this solution looks a bit complex and confusing, but that is because the pretty and easy solution of just using the tether order only works with good ping + fps
    // at higher latencies the time stamps merge together and tethers start to appear in random order in the logs...
    private static readonly AOEShapeCone cone = new(45f, 90f.Degrees());
    private AOEInstance[] _aoe = [];
    private readonly List<(Actor source, Actor target)> tethers = new(8);
    private int cloneCount;
    private bool direction; // false = left, true = right
    private bool active;
    private Actor? firstClone;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.SoldierS0Clone)
        {
            if (modelState == 5)
            {
                direction = false;
            }
            else if (modelState == 6)
            {
                direction = true;
            }
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.BisectorInitial)
        {
            ++cloneCount;
        }
        else if (tether.ID == (uint)TetherID.BisectorEnd)
        {
            tethers.Add((source, WorldState.Actors.Find(tether.Target)!));
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (active && tether.ID == (uint)TetherID.BisectorEnd)
        {
            var count = tethers.Count;
            for (var i = 0; i < count; ++i)
            {
                if (tethers[i].source == source)
                {
                    tethers.RemoveAt(i);
                    break;
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.SectorBisector1 or (uint)AID.SectorBisector2)
        {
            _aoe = [];
            cloneCount = 0;
            tethers.Clear();
            firstClone = null;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (firstClone == null && spell.Action.ID is (uint)AID.SectorBisectorVisualClone2 or (uint)AID.SectorBisectorVisualClone5)
        {
            active = true;
            firstClone = caster;
        }
    }

    public override void Update()
    {
        if (active && firstClone != null && _aoe.Length == 0 && tethers.Count < cloneCount)
        {
            var count = tethers.Count;
            for (var i = 0; i < count; ++i)
            {
                var tether = tethers[i];
                if (tether.source == firstClone)
                {
                    active = false;
                    _aoe = [new(cone, tether.target.Position.Quantized(), tether.target.Rotation + (direction ? -1f : 1f) * 90f.Degrees(), WorldState.FutureTime(cloneCount == 6 ? 4.2d : 5.9d))];
                    return;
                }
            }
        }
    }
}

sealed class MultiboxSupport(BossModule module) : MultiboxComponent(module)
{
    private readonly OrderedFire _orderedFire = module.FindComponent<OrderedFire>()!;
    private readonly StaticForce _staticForce = module.FindComponent<StaticForce>()!;
    private readonly ElectricExcess _electricExcess = module.FindComponent<ElectricExcess>()!;

    private enum MechanicState { None, OrderedFire, StaticForce, ElectricExcess }
    private MechanicState _currentMechanic;
    private DateTime? _orderedFireActivation = null;
    private DateTime _electricExcessActivation;
    private WPos? _lastSafeQuad;
    private readonly Dictionary<PartyRolesConfig.Assignment, WPos> partyElectricExcessHintPos = [];
    private readonly Dictionary<PartyRolesConfig.Assignment, WPos> partyOrderedFireHintPos = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.OrderedFireVisual:
                _currentMechanic = MechanicState.OrderedFire;
                _orderedFireActivation = Module.CastFinishAt(spell, 5);
                break;
            case AID.StaticForce:
                _currentMechanic = MechanicState.StaticForce;
                break;
            case AID.ElectricExcessVisual:
                _currentMechanic = MechanicState.ElectricExcess;
                _electricExcessActivation = Module.CastFinishAt(spell, 5.5f);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is ((uint)AID.OrderedFire) or ((uint)AID.StaticForce) or ((uint)AID.ElectricExcess))
            _currentMechanic = MechanicState.None;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_config.MultiboxMode || assignment == PartyRolesConfig.Assignment.Unassigned)
            return;

        if (_currentMechanic is not MechanicState.OrderedFire and not MechanicState.StaticForce)
        {
            AddGenericMTNorthHint(slot, actor, assignment, hints);
        }

        if (WorldState.CurrentTime > _electricExcessActivation)
        {
            partyElectricExcessHintPos.Clear();
        }

        if (WorldState.CurrentTime > _orderedFireActivation)
        {
            partyOrderedFireHintPos.Clear();
        }

        if (_currentMechanic is MechanicState.OrderedFire or MechanicState.StaticForce || WorldState.CurrentTime < _orderedFireActivation?.AddSeconds(5))
        {
            AddOrderedFireHints(slot, actor, assignment, hints);
        }
        else if (_currentMechanic is MechanicState.ElectricExcess)
        {
            AddElectricExcessHints(slot, actor, assignment, hints);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!_config.MultiboxMode)
            return;

        foreach (var kvp in partyElectricExcessHintPos)
        {
            uint color = kvp.Key switch
            {
                PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT => Colors.Tank,
                PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2 => Colors.Melee,
                PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.R2 => Colors.Caster,
                PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.H2 => Colors.Healer,
                _ => 0
            };
            uint color2 = Colors.Safe;

            if (color != 0)
            {
                Arena.ZoneCircle(kvp.Value.Quantized(), _electricExcess.SpreadRadius, color2);
                Arena.ZoneCircle(kvp.Value.Quantized(), 0.5f, color);
            }
        }

        foreach (var kvp in partyOrderedFireHintPos)
        {
            uint color = kvp.Key switch
            {
                PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT => Colors.Tank,
                PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2 => Colors.Melee,
                PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.R2 => Colors.Caster,
                PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.H2 => Colors.Healer,
                _ => 0
            };
            uint color2 = Colors.Safe;

            if (color != 0)
            {
                Angle angle = Angle.FromDirection((kvp.Value - Module.PrimaryActor.Position).Normalized());
                Arena.ZoneCone(Module.PrimaryActor.Position.Quantized(), 0, 60, angle, 15.Degrees(), color);
                Arena.ZoneCircle(kvp.Value.Quantized(), 0.5f, color2);
            }
        }
    }

    private void AddElectricExcessHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var spreadRadius = _electricExcess.SpreadRadius;

        var activeRoles = new List<PartyRolesConfig.Assignment>();
        for (var index = 0; index < Raid.Members.Count(); ++index)
        {
            var role = _prc[Raid.Members[index].ContentId];
            if (Raid.Members[index].IsValid() && role != PartyRolesConfig.Assignment.Unassigned)
            {
                activeRoles.Add(role);
            }
        }

        var boss = Module.PrimaryActor;
        var bossPos = boss.Position;

        var relativePositions = GenericSpreadAroundBoss(activeRoles, spreadRadius, false, 180.Degrees());
        var positions = new Dictionary<PartyRolesConfig.Assignment, WPos>();
        foreach (var kvp in relativePositions)
        {
            positions[kvp.Key] = bossPos + kvp.Value;
        }

        if (actor.InstanceID == Raid.Player()?.InstanceID)
        {
            if (positions.TryGetValue(assignment, out var assignedPos))
            {
                AddGenericGoalDestination(hints, assignedPos);
            }
        }

        // debug visualization
        partyElectricExcessHintPos.Clear();
        foreach (var kvp in positions)
        {
            partyElectricExcessHintPos[kvp.Key] = kvp.Value;
        }
    }

    private void AddOrderedFireHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var activeRoles = new List<PartyRolesConfig.Assignment>();
        for (var index = 0; index < Raid.Members.Count(); ++index)
        {
            var role = _prc[Raid.Members[index].ContentId];
            if (Raid.Members[index].IsValid() && role != PartyRolesConfig.Assignment.Unassigned)
            {
                activeRoles.Add(role);
            }
        }

        var safeQuadrant = GetOrderedFireSafeQuadrant();
        if (safeQuadrant == null)
        {
            if (_lastSafeQuad == null)
                return;
            safeQuadrant = _lastSafeQuad;
        }
        else
        {
            _lastSafeQuad = (WPos)safeQuadrant;
        }

        bool orderedFireResolved = _orderedFire.NumCasts > 0 || WorldState.CurrentTime > _orderedFireActivation || _currentMechanic == MechanicState.StaticForce;
        var positions = AssignOrderedFirePositions(activeRoles, safeQuadrant.Value, orderedFireResolved);
        if (actor.InstanceID == Raid.Player()?.InstanceID)
        {
            if (positions.TryGetValue(assignment, out var assignedPos))
            {
                AddGenericGoalDestination(hints, assignedPos);
            }
        }

        // debug visualization
        partyOrderedFireHintPos.Clear();
        foreach (var kvp in positions)
        {
            if (partyOrderedFireHintPos.ContainsKey(kvp.Key))
            {
                partyOrderedFireHintPos[kvp.Key] = kvp.Value;
            }
            else
            {
                partyOrderedFireHintPos.Add(kvp.Key, kvp.Value);
            }
        }
    }

    private WPos? GetOrderedFireSafeQuadrant()
    {
        var activeAOEs = _orderedFire.ActiveCasters;
        if (activeAOEs.Length == 0)
            return null;

        var arenaCenter = Module.Center;
        float quarterR = Arena.Bounds.Radius / 4;
        var quadrants = new[]
        {
            arenaCenter + new WDir(quarterR, quarterR),
            arenaCenter + new WDir(quarterR, -quarterR),
            arenaCenter + new WDir(-quarterR, -quarterR),
            arenaCenter + new WDir(-quarterR, quarterR),
        };

        // check which quadrant is not covered by the rectangles
        foreach (var quadrant in quadrants)
        {
            bool safe = true;
            foreach (var aoe in activeAOEs)
            {
                if (aoe.Shape.Check(quadrant, aoe.Origin, aoe.Rotation))
                {
                    safe = false;
                    break;
                }
            }
            if (safe)
                return quadrant;
        }

        return null;
    }

    private Dictionary<PartyRolesConfig.Assignment, WPos> AssignOrderedFirePositions(List<PartyRolesConfig.Assignment> activeRoles, WPos safeCenter, bool orderedFireResolved)
    {
        Actor? boss = Module.PrimaryActor;
        WPos bossPos = boss.Position;
        Dictionary<PartyRolesConfig.Assignment, WPos> positions = [];

        activeRoles = [.. activeRoles.OrderBy(r => (int)r)];
        foreach (var role in activeRoles)
        {
            if (positions.ContainsKey(role))
            {
                Service.Log("Duplicate party assignment! Fix this!!");
            }
            else
            {
                positions.Add(role, new());
            }
        }

        var tanks = activeRoles.Where(r => r is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT).OrderBy(r => (int)r).ToList();
        var melees = activeRoles.Where(r => r is PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2).OrderBy(r => (int)r).ToList();
        var ranged = activeRoles.Where(r => r is PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.R2 or PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.H2).ToList();

        Angle angle = Angle.FromDirection(safeCenter - bossPos);
        Angle[] angleOffsets =
        [
            angle,
            angle - 24.5f.Degrees(),
            angle + 24.5f.Degrees(),
            angle + 55.0f.Degrees(),
            angle - 55.0f.Degrees()
        ];

        float mtDistance = 2.0f;
        float meleeDistance = 5.0f;
        float rangedDistance = 6.0f;
        int positionIndex = 0;

        // MT special handling
        if (tanks.Contains(PartyRolesConfig.Assignment.MT))
        {
            if (!orderedFireResolved)
            {
                // under boss until OrderedFire resolves
                positions[PartyRolesConfig.Assignment.MT] = bossPos + (mtDistance * angle.ToDirection());
            }
            else
            {
                // behind boss
                positions[PartyRolesConfig.Assignment.MT] = bossPos + (meleeDistance * (angle + 180.Degrees()).ToDirection());
            }
        }

        // melee priority to center
        if (melees.Contains(PartyRolesConfig.Assignment.M1))
        {
            positions[PartyRolesConfig.Assignment.M1] = bossPos + (meleeDistance * angleOffsets[positionIndex++].ToDirection());
        }
        if (melees.Contains(PartyRolesConfig.Assignment.M2))
        {
            positions[PartyRolesConfig.Assignment.M2] = bossPos + (meleeDistance * angleOffsets[positionIndex++].ToDirection());
        }
        if (tanks.Contains(PartyRolesConfig.Assignment.OT))
        {
            positions[PartyRolesConfig.Assignment.OT] = bossPos + (meleeDistance * angleOffsets[positionIndex++].ToDirection());
        }

        foreach (var r in ranged)
        {
            if (positionIndex < angleOffsets.Length)
            {
                positions[r] = bossPos + rangedDistance * angleOffsets[positionIndex++].ToDirection();
            }
        }

        return positions;
    }
}

sealed class D102SoldierS0States : StateMachineBuilder
{
    public D102SoldierS0States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FieldOfScorn>()
            .ActivateOnEnter<ThunderousSlash>()
            .ActivateOnEnter<OrderedFire>()
            .ActivateOnEnter<ElectricExcess>()
            .ActivateOnEnter<StaticForce>()
            .ActivateOnEnter<SectorBisector>()
            .ActivateOnEnter<MultiboxSupport>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1027u, NameID = 13757u, MultiboxSupport = true)]
public sealed class D102SoldierS0(WorldState ws, Actor primary) : BossModule(ws, primary, new(default, -182f), new ArenaBoundsSquare(15.5f));
