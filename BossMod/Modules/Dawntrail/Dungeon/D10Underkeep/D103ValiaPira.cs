using BossMod.Components;

namespace BossMod.Dawntrail.Dungeon.D10Underkeep.D103ValiaPira;

public enum OID : uint
{
    Boss = 0x478E, // R4.5
    CoordinateBit1 = 0x4789, // R1.0
    CoordinateBit2 = 0x47B5, // R1.0
    CoordinateTurret = 0x4793, // R1.0
    Orb = 0x478F, // R1.6
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 42526, // Boss->player, no cast, single-target

    EntropicSphere = 42525, // Boss->self, 5.0s cast, range 40 circle
    CoordinateMarch = 42513, // Boss->self, 4.0s cast, single-target
    BitAndOrbCollide = 42514, // Orb->CoordinateBit1/CoordinateBit2, no cast, single-target
    EnforcementRay = 42737, // CoordinateBit->self, 0.5s cast, range 36 width 9 cross
    OrderedFireVisual1 = 42508, // Boss->self, no cast, single-target
    OrderedFireVisual2 = 42509, // Helper->Boss, 1.0s cast, single-target
    Electray = 43130, // CoordinateTurret->self, 7.0s cast, range 40 width 9 rect
    ElectricFieldVisual = 42519, // Boss->self, 6.6+0,7s cast, single-target
    ConcurrentField = 42521, // Helper->self, 7.3s cast, range 26 50-degree cone
    ElectricField = 43261, // Helper->self, no cast, range 26 50-degree cone
    NeutralizeFrontLines = 42738, // Boss->self, 5.0s cast, range 30 180-degree cone
    HyperchargedLight = 42524, // Helper->player, 5.0s cast, range 5 circle, spread
    Bloodmarch = 42739, // Boss->self, 5.0s cast, single-target
    DeterrentPulse = 42540 // Boss->self/players, 5.0s cast, range 40 width 8 rect, line stack
}

public enum IconID : uint
{
    ElectricField = 586, // Boss->player
    DeterrentPulse = 525 // Boss->player
}

public enum TetherID : uint
{
    OrbTeleport = 282 // CoordinateBit2->Orb
}

sealed class EntropicSphere(BossModule module) : Components.RaidwideCast(module, (uint)AID.EntropicSphere);
sealed class Electray(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Electray, new AOEShapeRect(40f, 4.5f));
sealed class ConcurrentField(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ConcurrentField, new AOEShapeCone(26f, 25f.Degrees()));
sealed class ElectricField(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(26f, 25f.Degrees()), (uint)IconID.ElectricField, (uint)AID.ElectricField, 7.4d);
sealed class NeutralizeFrontLines(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NeutralizeFrontLines, new AOEShapeCone(30f, 90f.Degrees()));
sealed class HyperchargedLight(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.HyperchargedLight, 5f);
sealed class DeterrentPulse(BossModule module) : Components.LineStack(module, (uint)IconID.DeterrentPulse, (uint)AID.DeterrentPulse, 5.3d, 40f, 4f, 4, 4, 1, false);

sealed class EnforcementRay(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCross cross = new(36f, 4.5f);
    private readonly List<AOEInstance> _aoes = new(3);
    private bool teleported;
    private readonly List<WPos> startingpositions = new(2);
    private WPos center;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11DA && !teleported)
        {
            _aoes.Add(new(cross, actor.Position.Quantized()));
            if (center != default)
                UpdateAOEs();
        }
        else if (id == 0x11D5)
        {
            startingpositions.Add(actor.Position);
            if (startingpositions.Count == 2)
            {
                var pos0 = startingpositions[0];
                var pos1 = startingpositions[1];
                center = new((pos0.X + pos1.X) * 0.5f, (pos0.Z + pos1.Z) * 0.5f);
                UpdateAOEs();
            }
        }
        void UpdateAOEs()
        {
            var count = _aoes.Count;
            if (count > 1)
            {
                SortAOEs();
                var aoes = CollectionsMarshal.AsSpan(_aoes);

                var activationTime = NumCasts switch
                {
                    0 => 8.3d,
                    1 => 11.4d,
                    _ => 7.3d
                };

                aoes[0].Activation = WorldState.FutureTime(activationTime);

                if (NumCasts == 0)
                    aoes[1].Activation = WorldState.FutureTime(11.4d);
            }
        }
    }

    private void SortAOEs()
    {
        var count = _aoes.Count;
        if (count < 2)
            return;

        var aoes = CollectionsMarshal.AsSpan(_aoes);
        if ((aoes[0].Origin - center).LengthSq() > (aoes[1].Origin - center).LengthSq())
            (aoes[0], aoes[1]) = (aoes[1], aoes[0]);
        if (count == 3)
        {
            if ((aoes[0].Origin - center).LengthSq() > (aoes[2].Origin - center).LengthSq())
                (aoes[0], _aoes[2]) = (aoes[2], aoes[0]);

            if ((aoes[1].Origin - center).LengthSq() > (aoes[2].Origin - center).LengthSq())
                (aoes[1], aoes[2]) = (aoes[2], aoes[1]);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.CoordinateBit2 && tether.ID == (uint)TetherID.OrbTeleport)
        {
            teleported = true;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var len = aoes.Length;
            var position = WorldState.Actors.Find(tether.Target)?.Position;
            if (position is not WPos pos)
                return;
            for (var i = 0; i < len; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.Origin.AlmostEqual(pos, 1f))
                {
                    aoe.Origin = source.Position.Quantized();
                    break;
                }
            }
            SortAOEs();

            double[] timings = NumCasts == 1 ? [2.8, 7.9] : [0.6, 7.9, 14.4];
            var lenT = timings.Length;
            var max = lenT > len ? len : lenT;
            for (var i = 0; i < max; ++i)
            {
                aoes[i].Activation = WorldState.FutureTime(timings[i]);
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EnforcementRay)
        {
            var count = _aoes.Count;
            var pos = spell.LocXZ;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].Origin.AlmostEqual(pos, 1f))
                {
                    _aoes.RemoveAt(i);
                    if (_aoes.Count == 0)
                    {
                        teleported = false;
                        center = default;
                        startingpositions.Clear();
                        ++NumCasts;
                    }
                    return;
                }
            }
        }
    }
}

sealed class MultiboxSupport(BossModule module) : MultiboxComponent(module)
{
    private readonly ConcurrentField _concurrentField = module.FindComponent<ConcurrentField>()!;
    private readonly ElectricField _electricField = module.FindComponent<ElectricField>()!;
    private readonly HyperchargedLight _hyperchargedLight = module.FindComponent<HyperchargedLight>()!;
    private readonly NeutralizeFrontLines _neutralizeFrontLines = module.FindComponent<NeutralizeFrontLines>()!;

    private enum MechanicState { None, ConcurrentField, HyperchargedLight }
    private MechanicState _currentMechanic;
    private DateTime _concurrentFieldActivation;
    private DateTime _hyperchargedLightActivation;
    private readonly Dictionary<PartyRolesConfig.Assignment, WPos> partyConcurrentFieldHintPos = [];
    private readonly Dictionary<PartyRolesConfig.Assignment, WPos> partyHyperchargedLightHintPos = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ConcurrentField:
                _currentMechanic = MechanicState.ConcurrentField;
                _concurrentFieldActivation = Module.CastFinishAt(spell, 7.8f);
                break;
            case AID.HyperchargedLight:
                _currentMechanic = MechanicState.HyperchargedLight;
                _hyperchargedLightActivation = Module.CastFinishAt(spell);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is ((uint)AID.ElectricField) or ((uint)AID.HyperchargedLight))
            _currentMechanic = MechanicState.None;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_config.MultiboxMode || assignment == PartyRolesConfig.Assignment.Unassigned)
            return;

        AddGenericMTNorthHint(slot, actor, assignment, hints);

        if (WorldState.CurrentTime > _concurrentFieldActivation.AddSeconds(3))
            partyConcurrentFieldHintPos.Clear();

        if (WorldState.CurrentTime > _hyperchargedLightActivation.AddSeconds(3))
            partyHyperchargedLightHintPos.Clear();

        switch (_currentMechanic)
        {
            case MechanicState.ConcurrentField:
                AddConcurrentFieldHints(slot, actor, assignment, hints);
                break;
            case MechanicState.HyperchargedLight:
                AddHyperchargedLightHints(slot, actor, assignment, hints);
                break;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!_config.MultiboxMode)
            return;

        foreach (var kvp in partyConcurrentFieldHintPos)
        {
            uint color = kvp.Key switch
            {
                PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT => Colors.Tank,
                PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2 => Colors.Melee,
                PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.R2 => Colors.Caster,
                PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.H2 => Colors.Healer,
                _ => 0
            };

            if (color != 0)
            {
                Arena.ZoneCircle(kvp.Value.Quantized(), 0.5f, color);
            }
        }

        foreach (var kvp in partyHyperchargedLightHintPos)
        {
            uint color = kvp.Key switch
            {
                PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT => Colors.Tank,
                PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2 => Colors.Melee,
                PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.R2 => Colors.Caster,
                PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.H2 => Colors.Healer,
                _ => 0
            };

            if (color != 0)
            {
                Arena.ZoneCircle(kvp.Value.Quantized(), _hyperchargedLight.SpreadRadius, Colors.SafeFromAOE);
                Arena.ZoneCircle(kvp.Value.Quantized(), 0.5f, color);
            }
        }
    }

    private void AddConcurrentFieldHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
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

        var positions = AssignConcurrentFieldPositions(activeRoles);
        if (actor.InstanceID == Raid.Player()?.InstanceID)
        {
            if (positions.TryGetValue(assignment, out var assignedPos))
            {
                AddGenericGoalDestination(hints, assignedPos); // Bug?: AI will sometimes not move to safe spot?
            }
        }

        // debug visualization
        partyConcurrentFieldHintPos.Clear();
        foreach (var kvp in positions)
        {
            if (partyConcurrentFieldHintPos.ContainsKey(kvp.Key))
            {
                partyConcurrentFieldHintPos[kvp.Key] = kvp.Value;
            }
            else
            {
                partyConcurrentFieldHintPos.Add(kvp.Key, kvp.Value);
            }
        }
    }

    private struct SimpleCone(Angle center, Angle left, Angle right)
    {
        public Angle Center = center;
        public Angle Left = left;
        public Angle Right = right;
    }

    private Dictionary<PartyRolesConfig.Assignment, WPos> AssignConcurrentFieldPositions(List<PartyRolesConfig.Assignment> activeRoles)
    {
        var positions = new Dictionary<PartyRolesConfig.Assignment, WPos>();
        var arenaCenter = Module.Center;

        if (activeRoles.Count == 0 || _concurrentField.Casters.Count == 0)
            return positions;

        activeRoles = [.. activeRoles.OrderBy(r => (int)r)];

        List<SimpleCone> cones = [];
        List<SimpleCone> conesNoOverlap = [];
        for (int index = 0; index < _concurrentField.Casters.Count; index++)
        {
            var reference = _concurrentField.Casters[index];
            var halfAngle = 25f.Degrees();
            cones.Add(new SimpleCone(reference.Rotation, reference.Rotation - halfAngle, reference.Rotation + halfAngle));
        }
        cones = [.. cones.OrderBy(c => c.Center.Rad)];

        // merge overlapping cones
        for (int index = 0; index < cones.Count; index++)
        {
            var currentCone = cones[index];
            var mergedLeft = currentCone.Left;
            var mergedRight = currentCone.Right;

            while (index + 1 < cones.Count)
            {
                var nextCone = cones[index + 1];

                bool overlaps = false;
                if (mergedRight.Rad > mergedLeft.Rad)
                {
                    overlaps = nextCone.Left.Rad <= mergedRight.Rad;
                }
                else // wraparound
                {
                    overlaps = nextCone.Left.Rad <= mergedRight.Rad || nextCone.Left.Rad >= mergedLeft.Rad;
                }

                if (overlaps)
                {
                    if (nextCone.Right.Rad > mergedRight.Rad || nextCone.Right.Rad < mergedLeft.Rad)
                        mergedRight = nextCone.Right;
                    index++;
                }
                else
                {
                    break;
                }
            }

            Angle mergedCenter;
            if (mergedRight.Rad > mergedLeft.Rad)
                mergedCenter = mergedLeft + (mergedRight - mergedLeft) / 2;
            else // Wraparound
                mergedCenter = mergedLeft + (mergedRight + 360.Degrees() - mergedLeft) / 2;

            conesNoOverlap.Add(new SimpleCone(mergedCenter, mergedLeft, mergedRight));
        }

        List<SimpleCone> safeAreas = [];
        for (int index = 0; index < conesNoOverlap.Count; index++)
        {
            var currentCone = conesNoOverlap[index];
            var nextCone = conesNoOverlap[(index + 1) % conesNoOverlap.Count];

            var safeLeft = currentCone.Right;
            var safeRight = nextCone.Left;

            float width;
            if (safeRight.Rad > safeLeft.Rad)
                width = (safeRight - safeLeft).Rad;
            else // wraparound
                width = (360.Degrees() - safeLeft).Rad + safeRight.Rad;

            if (width > 0)
            {
                Angle safeCenter;
                if (safeRight.Rad > safeLeft.Rad)
                    safeCenter = safeLeft + (safeRight - safeLeft) / 2;
                else // wraparound
                    safeCenter = safeLeft + (safeRight + 360.Degrees() - safeLeft) / 2;

                safeAreas.Add(new SimpleCone(safeCenter, safeLeft, safeRight));
            }
        }

        List<WPos> safeSpots = [];
        float safeDistance = 6f;

        foreach (var area in safeAreas)
        {
            float width;
            if (area.Right.Rad > area.Left.Rad)
                width = (area.Right - area.Left).Rad;
            else
                width = (360.Degrees() - area.Left).Rad + area.Right.Rad;

            if (width > 50.Degrees().Rad) // large area = 3 spots
            {
                safeSpots.Add(arenaCenter + safeDistance * (area.Left + 1.0f.Degrees()).ToDirection());
                safeSpots.Add(arenaCenter + safeDistance * area.Center.ToDirection());
                safeSpots.Add(arenaCenter + safeDistance * (area.Right - 1.0f.Degrees()).ToDirection());
            }
            else if (width > 25.Degrees().Rad) // medium area = 2 spots
            {
                safeSpots.Add(arenaCenter + safeDistance * (area.Left + 1.0f.Degrees()).ToDirection());
                safeSpots.Add(arenaCenter + safeDistance * (area.Right - 1.0f.Degrees()).ToDirection());
            }
            else if (width > 0) // small area = 1 spot
            {
                safeSpots.Add(arenaCenter + safeDistance * area.Center.ToDirection());
            }
        }

        // sort safe spots by angle
        safeSpots = [.. safeSpots.OrderBy(spot =>
        {
            var angle = Angle.FromDirection(spot - arenaCenter).Rad;
            if (angle < 0) angle += 2 * MathF.PI;
            return angle;
        })];

        for (int index = 0; index < Math.Min(activeRoles.Count, safeSpots.Count); index++)
        {
            positions[activeRoles[index]] = safeSpots[index];
        }

        return positions;
    }

    private void AddHyperchargedLightHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var boss = Module.PrimaryActor;
        var bossPos = boss.Position;

        var activeRoles = new List<PartyRolesConfig.Assignment>();
        for (var index = 0; index < Raid.Members.Count(); ++index)
        {
            var role = _prc[Raid.Members[index].ContentId];
            if (Raid.Members[index].IsValid() && role != PartyRolesConfig.Assignment.Unassigned)
            {
                activeRoles.Add(role);
            }
        }

        var positions = AssignHyperchargedLightPositions(activeRoles, _hyperchargedLight.SpreadRadius);
        if (actor.InstanceID == Raid.Player()?.InstanceID)
        {
            if (positions.TryGetValue(assignment, out var assignedPos))
            {
                AddGenericGoalDestination(hints, assignedPos);
            }
        }

        // debug visualization
        partyHyperchargedLightHintPos.Clear();
        foreach (var kvp in positions)
        {
            partyHyperchargedLightHintPos[kvp.Key] = kvp.Value;
        }
    }

    private Dictionary<PartyRolesConfig.Assignment, WPos> AssignHyperchargedLightPositions(List<PartyRolesConfig.Assignment> activeRoles, float spreadRadius)
    {
        var boss = Module.PrimaryActor;
        var bossPos = boss.Position;
        var positions = new Dictionary<PartyRolesConfig.Assignment, WPos>();

        if (activeRoles.Count == 0)
            return positions;

        activeRoles = [.. activeRoles.OrderBy(r => (int)r)];

        var melees = activeRoles.Where(r => r is PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2 or PartyRolesConfig.Assignment.OT).OrderBy(r => (int)r).ToList();
        var ranged = activeRoles.Where(r => r is PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.R2 or PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.H2).OrderBy(r => (int)r).ToList();

        List<WDir> safeOffsets;

        if (activeRoles.Contains(PartyRolesConfig.Assignment.MT) && melees.Count == 2)
        {
            // 3 melee party: pack melees in triangle formation
            safeOffsets =
            [
                spreadRadius * (boss.Rotation + Cardinal.South).ToDirection(),
                spreadRadius * (boss.Rotation + Cardinal.East + 2.Degrees()).ToDirection(),
                spreadRadius * (boss.Rotation + Cardinal.West - 2.Degrees()).ToDirection()
            ];
        }
        else if (activeRoles.Contains(PartyRolesConfig.Assignment.MT) && melees.Count == 3)
        {
            // all melee party: MT will tank the frontlines damage to allow for more overall melee uptime
            safeOffsets =
            [
                spreadRadius * (boss.Rotation + Cardinal.North).ToDirection(),
                spreadRadius * (boss.Rotation + Cardinal.East + 2.Degrees()).ToDirection(),
                spreadRadius * (boss.Rotation + Cardinal.West - 2.Degrees()).ToDirection(),
                spreadRadius * (boss.Rotation + Cardinal.South).ToDirection()
            ];
        }
        else
        {
            // otherwise pack party in trapezoid formation
            safeOffsets =
            [
                (0.1f + spreadRadius / 2) * (boss.Rotation + Cardinal.East + 2.Degrees()).ToDirection(),
                (0.1f + spreadRadius / 2) * (boss.Rotation + Cardinal.West - 2.Degrees()).ToDirection()
            ];
        }

        safeOffsets.AddRange([
            1.5f * spreadRadius * (boss.Rotation + 135.Degrees()).ToDirection(),
            1.5f * spreadRadius * (boss.Rotation - 135.Degrees()).ToDirection(),
            2.5f * spreadRadius * (boss.Rotation + Cardinal.South).ToDirection()
        ]);

        int positionIndex = 0;

        if (activeRoles.Contains(PartyRolesConfig.Assignment.MT))
        {
            positions[PartyRolesConfig.Assignment.MT] = bossPos + safeOffsets[positionIndex++];
        }

        foreach (var role in melees)
        {
            if (positionIndex < safeOffsets.Count)
                positions[role] = bossPos + safeOffsets[positionIndex++];
        }

        foreach (var role in ranged)
        {
            if (positionIndex < safeOffsets.Count)
                positions[role] = bossPos + safeOffsets[positionIndex++];
        }

        return positions;
    }
}

sealed class D103ValiaPiraStates : StateMachineBuilder
{
    public D103ValiaPiraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EntropicSphere>()
            .ActivateOnEnter<Electray>()
            .ActivateOnEnter<ConcurrentField>()
            .ActivateOnEnter<ElectricField>()
            .ActivateOnEnter<NeutralizeFrontLines>()
            .ActivateOnEnter<HyperchargedLight>()
            .ActivateOnEnter<DeterrentPulse>()
            .ActivateOnEnter<EnforcementRay>()
            .ActivateOnEnter<MultiboxSupport>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1027, NameID = 13749, MultiboxSupport = true)]
public sealed class D103ValiaPira(WorldState ws, Actor primary) : BossModule(ws, primary, new(default, -331f), new ArenaBoundsSquare(17.5f));
