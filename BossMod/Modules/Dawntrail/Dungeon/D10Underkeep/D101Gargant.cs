using BossMod.Components;

namespace BossMod.Dawntrail.Dungeon.D10Underkeep.D101Gargant;

public enum OID : uint
{
    Boss = 0x4791, // R4.2
    SandSphere = 0x4792, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    ChillingChirp = 42547, // Boss->self, 5.0s cast, range 30 circle
    AlmightyRacket = 42546, // Boss->self, 4.0s cast, range 30 width 30 rect
    AerialAmbushVisual = 42542, // Boss->location, 3.0s cast, single-target
    AerialAmbush = 42543, // Helper->self, 3.5s cast, range 30 width 15 rect
    FoundationalDebris = 43161, // Helper->location, 6.0s cast, range 10 circle
    SedimentaryDebris = 43160, // Helper->players, 5.0s cast, range 5 circle, spread
    Earthsong = 42544, // Boss->self, 5.0s cast, range 30 circle
    SphereShatter1 = 42545, // SandSphere->self, 2.0s cast, range 6 circle
    SphereShatter2 = 43135, // SandSphere->self, 2.0s cast, range 6 circle
    TrapJaws = 42548 // Boss->player, 5.0s cast, single-target
}

sealed class AerialAmbush(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AerialAmbush, new AOEShapeRect(30f, 7.5f));
sealed class AlmightyRacket(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AlmightyRacket, new AOEShapeRect(30f, 15f));
sealed class FoundationalDebris(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FoundationalDebris, 10f);
sealed class SedimentaryDebris(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.SedimentaryDebris, 5f);
sealed class EarthsongChillingChirp(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.Earthsong, (uint)AID.ChillingChirp]);
sealed class TrapJaws(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.TrapJaws);

sealed class SphereShatter(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(6f);
    private readonly List<AOEInstance> _aoes = new(13);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var deadline = aoes[0].Activation.AddSeconds(1d);

        var index = 0;
        while (index < count)
        {
            ref var aoe = ref aoes[index];
            if (aoe.Activation >= deadline)
            {
                break;
            }
            ++index;
        }

        return aoes[..index];
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.SandSphere)
        {
            _aoes.Add(new(circle, actor.Position.Quantized(), default, WorldState.FutureTime(7.9d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.SphereShatter1 or (uint)AID.SphereShatter2)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class MultiboxSupport(BossModule module) : MultiboxComponent(module)
{
    private readonly SedimentaryDebris _sedimentaryDebris = module.FindComponent<SedimentaryDebris>()!;
    private readonly AerialAmbush _aerialAmbush = module.FindComponent<AerialAmbush>()!;

    private enum MechanicState { None, SedimentaryDebris, AerialAmbush }
    private MechanicState _currentMechanic;
    private DateTime _sedimentaryDebrisActivation;
    private DateTime _aerialAmbushActivation;
    private readonly Dictionary<PartyRolesConfig.Assignment, WPos> partyAerialAmbushHintPos = [];
    private readonly Dictionary<PartyRolesConfig.Assignment, WPos> partySedimentaryDebrisHintPos = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SedimentaryDebris:
                _currentMechanic = MechanicState.SedimentaryDebris;
                _sedimentaryDebrisActivation = Module.CastFinishAt(spell, 5.5f);
                break;
            case AID.AerialAmbushVisual:
                _currentMechanic = MechanicState.AerialAmbush;
                _aerialAmbushActivation = Module.CastFinishAt(spell, 3.5f);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is ((uint)AID.SedimentaryDebris) or ((uint)AID.AerialAmbush))
            _currentMechanic = MechanicState.None;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_config.MultiboxMode || assignment == PartyRolesConfig.Assignment.Unassigned)
            return;

        AddGenericMTNorthHint(slot, actor, assignment, hints);

        if (WorldState.CurrentTime > _sedimentaryDebrisActivation)
            partySedimentaryDebrisHintPos.Clear();

        if (WorldState.CurrentTime > _aerialAmbushActivation)
            partyAerialAmbushHintPos.Clear();

        switch (_currentMechanic)
        {
            case MechanicState.SedimentaryDebris:
                AddSedimentaryDebrisHints(slot, actor, assignment, hints);
                break;
            case MechanicState.AerialAmbush:
                AddAerialAmbushHints(slot, actor, assignment, hints);
                break;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!_config.MultiboxMode)
            return;

        foreach (var kvp in partySedimentaryDebrisHintPos)
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
                Arena.ZoneCircle(kvp.Value.Quantized(), _sedimentaryDebris.SpreadRadius, Colors.Safe);
                Arena.ZoneCircle(kvp.Value.Quantized(), 0.5f, color);
            }
        }

        foreach (var kvp in partyAerialAmbushHintPos)
        {
            uint color = kvp.Key switch
            {
                PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT => Colors.Tank,
                PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2 => Colors.Melee,
                _ => 0
            };

            if (color != 0)
            {
                Arena.ZoneCircle(kvp.Value.Quantized(), 0.5f, color);
            }
        }
    }

    private void AddAerialAmbushHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // Only position melees - ranged can stay wherever safe
        var isMelee = assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT
                                  or PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2;

        //if (!isMelee)
        //    return;

        if (_aerialAmbush.ActiveCasters.Length == 0)
            return;

        var aoe = _aerialAmbush.ActiveCasters[0];
        var rectLength = 30f; // From AOEShapeRect(30f, 7.5f)

        // where boss will appear
        var endPosition = aoe.Origin + rectLength * aoe.Rotation.ToDirection();

        // which side is closer to the actor's current position?
        var leftSide = endPosition + 3f * (aoe.Rotation + 90.Degrees()).ToDirection();
        var rightSide = endPosition + 3f * (aoe.Rotation - 90.Degrees()).ToDirection();

        var distToLeft = (leftSide - actor.Position).LengthSq();
        var distToRight = (rightSide - actor.Position).LengthSq();

        var targetPosition = distToLeft < distToRight ? leftSide : rightSide;

        if (actor.InstanceID == Raid.Player()?.InstanceID)
        {
            AddGenericGoalDestination(hints, targetPosition);
            partyAerialAmbushHintPos.Clear();
            partyAerialAmbushHintPos[assignment] = targetPosition;
        }

        // debug visualization
        partyAerialAmbushHintPos[assignment] = targetPosition;
    }

    private void AddSedimentaryDebrisHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var boss = Module.PrimaryActor;
        var bossPos = boss.Position;
        var spreadRadius = _sedimentaryDebris.SpreadRadius + 1.5f;

        var activeRoles = new List<PartyRolesConfig.Assignment>();
        for (var index = 0; index < Raid.Members.Count(); ++index)
        {
            var role = _prc[Raid.Members[index].ContentId];
            if (Raid.Members[index].IsValid() && role != PartyRolesConfig.Assignment.Unassigned)
            {
                activeRoles.Add(role);
            }
        }

        var positions = AssignSedimentaryDebrisPositions(activeRoles, spreadRadius);
        if (actor.InstanceID == Raid.Player()?.InstanceID)
        {
            if (positions.TryGetValue(assignment, out var assignedPos))
            {
                AddGenericGoalDestination(hints, assignedPos);
                partySedimentaryDebrisHintPos.Clear();
                partySedimentaryDebrisHintPos[assignment] = assignedPos;
            }
        }

        // debug visualization
        partySedimentaryDebrisHintPos.Clear();
        foreach (var kvp in positions)
        {
            if (partySedimentaryDebrisHintPos.ContainsKey(kvp.Key))
            {
                partySedimentaryDebrisHintPos[kvp.Key] = kvp.Value;
            }
            else
            {
                partySedimentaryDebrisHintPos.Add(kvp.Key, kvp.Value);
            }
        }
    }

    private Dictionary<PartyRolesConfig.Assignment, WPos> AssignSedimentaryDebrisPositions(List<PartyRolesConfig.Assignment> activeRoles, float spreadRadius)
    {
        var boss = Module.PrimaryActor;
        var bossPos = boss.Position;
        var positions = new Dictionary<PartyRolesConfig.Assignment, WPos>();
        WDir[] safeOffset = [];

        if (activeRoles.Count == 0)
        {
            Service.Log("No roles??");
            return positions;
        }

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

        // Sort these lists too for deterministic order
        var melees = activeRoles.Where(r => r is PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2 or PartyRolesConfig.Assignment.OT).OrderBy(r => (int)r).ToList();
        var ranged = activeRoles.Where(r => r is PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.R2 or PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.H2).OrderBy(r => (int)r).ToList();


        // rest of your code remains the same...
        // orient ourselves using foundational debris
        var foundationalDebris = Module.FindComponent<FoundationalDebris>();
        var arenaCenter = Module.Center;
        Angle relativeNorth = Angle.FromDirection(bossPos - arenaCenter); // boss relative

        if (foundationalDebris != null && foundationalDebris.Casters.Count == 3)
        {
            // treat lone aoe as north
            spreadRadius = spreadRadius * 2;
            var debrisPositions = foundationalDebris.Casters.Select(c => c.Origin).ToList();
            var loneDebris = FindLoneDebris(debrisPositions, arenaCenter);
            if (loneDebris != null)
            {
                relativeNorth = Angle.FromDirection(loneDebris.Value - arenaCenter);
                safeOffset =
                [
                    (boss.HitboxRadius) * (relativeNorth + Cardinal.South).ToDirection(),
                    (boss.HitboxRadius) * (relativeNorth + Cardinal.East).ToDirection(),
                    (boss.HitboxRadius) * (relativeNorth + Cardinal.West).ToDirection(),
                    // specific numbers to fit the bat shape of the safe zone
                    spreadRadius * (relativeNorth + 55.Degrees()).ToDirection(),
                    spreadRadius * (relativeNorth - 55.Degrees()).ToDirection(),
                    spreadRadius * (relativeNorth + 95.Degrees()).ToDirection(),
                    spreadRadius * (relativeNorth - 95.Degrees()).ToDirection()
                ];
            }
        }

        // no foundational debris?
        WPos center = Arena.Center;
        if (safeOffset.Count() == 0)
        {
            safeOffset =
            [
                (boss.HitboxRadius) * relativeNorth.ToDirection(),
                (boss.HitboxRadius) * (relativeNorth + Cardinal.East).ToDirection(),
                (boss.HitboxRadius) * (relativeNorth + Cardinal.South).ToDirection(),
                (boss.HitboxRadius) * (relativeNorth + Cardinal.West).ToDirection(),
            ];
            center = bossPos;
        }

        int positionIndex = 0;

        // MT priority to relative south
        if (activeRoles.Contains(PartyRolesConfig.Assignment.MT))
        {
            positions[PartyRolesConfig.Assignment.MT] = center + safeOffset[positionIndex++];
        }

        // melee priority to center
        foreach (var role in melees)
        {
            positions[role] = center + safeOffset[positionIndex++];
        }

        foreach (var role in ranged)
        {
            positions[role] = center + safeOffset[positionIndex++];
        }

        return positions;
    }

    private WPos? FindLoneDebris(List<WPos> debrisPositions, WPos arenaCenter)
    {
        if (debrisPositions.Count != 3)
            return null;

        // find which debris is furthest from the others
        float maxMinDistance = 0;
        WPos? loneDebris = null;

        foreach (var debris in debrisPositions)
        {
            float minDistanceToOther = float.MaxValue;
            foreach (var other in debrisPositions)
            {
                if (debris == other)
                    continue;
                var dist = (debris - other).LengthSq();
                if (dist < minDistanceToOther)
                    minDistanceToOther = dist;
            }

            if (minDistanceToOther > maxMinDistance)
            {
                maxMinDistance = minDistanceToOther;
                loneDebris = debris;
            }
        }

        return loneDebris;
    }
}

sealed class D101GargantStates : StateMachineBuilder
{
    public D101GargantStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AerialAmbush>()
            .ActivateOnEnter<AlmightyRacket>()
            .ActivateOnEnter<FoundationalDebris>()
            .ActivateOnEnter<SedimentaryDebris>()
            .ActivateOnEnter<EarthsongChillingChirp>()
            .ActivateOnEnter<TrapJaws>()
            .ActivateOnEnter<SphereShatter>()
            .ActivateOnEnter<MultiboxSupport>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1027, NameID = 13753, MultiboxSupport = true)]
public sealed class D101Gargant(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(-248f, 122f), 14.5f, 72)]);
}
