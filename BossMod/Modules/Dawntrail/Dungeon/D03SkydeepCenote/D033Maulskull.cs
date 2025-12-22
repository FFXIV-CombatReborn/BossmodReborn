using BossMod.AI;
using BossMod.Components;

namespace BossMod.Dawntrail.Dungeon.D03SkydeepCenote.D033Maulskull;

public enum OID : uint
{
    Boss = 0x41C7, // R19.98
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 36678, // Boss->player, no cast, single-target

    StonecarverVisual1 = 36668, // Boss->self, 8.0s cast, single-target
    StonecarverVisual2 = 36669, // Boss->self, 8.0s cast, single-target
    StonecarverVisual3 = 36672, // Boss->self, no cast, single-target
    StonecarverVisual4 = 36673, // Boss->self, no cast, single-target
    StonecarverVisual5 = 36699, // Boss->self, no cast, single-target
    StonecarverVisual6 = 36700, // Boss->self, no cast, single-target
    Stonecarver1 = 36670, // Helper->self, 9.0s cast, range 40 width 20 rect
    Stonecarver2 = 36671, // Helper->self, 11.5s cast, range 40 width 20 rect
    Stonecarver3 = 36696, // Helper->self, 11.1s cast, range 40 width 20 rect
    Stonecarver4 = 36697, // Helper->self, 13.6s cast, range 40 width 20 rect

    Impact1 = 36677, // Helper->self, 7.0s cast, range 60 circle, knockback 18, away from origin
    Impact2 = 36667, // Helper->self, 9.0s cast, range 60 circle, knockback 18, away from origin
    Impact3 = 36707, // Helper->self, 8.0s cast, range 60 circle, knockback 20, away from origin

    SkullCrushVisual1 = 36674, // Boss->self, no cast, single-target
    SkullcrushVisual2 = 36675, // Boss->self, 5.0+2.0s cast, single-target
    SkullcrushVisual3 = 38664, // Boss->self, no cast, single-target
    Skullcrush1 = 36676, // Helper->self, 7.0s cast, range 10 circle
    Skullcrush2 = 36666, // Helper->self, 9.0s cast, range 10 circle

    Charcore = 36708, // Boss->self, no cast, single-target
    DestructiveHeat = 36709, // Helper->players, 7.0s cast, range 6 circle

    MaulworkFirstCenter = 36679, // Boss->self, 5.0s cast, single-target
    MaulworkFirstSides = 36681, // Boss->self, 5.0s cast, single-target
    MaulworkSecondSides = 36682, // Boss->self, 5.0s cast, single-target
    MaulworkSecondCenter = 36680, // Boss->self, 5.0s cast, single-target
    ShatterCenter = 36684, // Helper->self, 3.0s cast, range 40 width 20 rect
    ShatterLR1 = 36685, // Helper->self, 3.0s cast, range 45 width 22 rect
    ShatterLR2 = 36686, // Helper->self, 3.0s cast, range 45 width 22 rect
    Landing = 36683, // Helper->location, 3.0s cast, range 8 circle

    DeepThunderTower1 = 36688, // Helper->self, 9.0s cast, range 6 circle
    DeepThunderTower2 = 36689, // Helper->self, 11.0s cast, range 6 circle
    DeepThunderVisual1 = 36687, // Boss->self, 6.0s cast, single-target
    DeepThunderVisual2 = 36691, // Boss->self, no cast, single-target
    DeepThunderVisual3 = 36692, // Boss->self, no cast, single-target
    DeepThunderRepeat = 36690, // Helper->self, no cast, range 6 circle
    BigBurst = 36693, // Helper->self, no cast, range 60 circle, tower fail

    RingingBlows1 = 36694, // Boss->self, 7.0+2.0s cast, single-target
    RingingBlows2 = 36695, // Boss->self, 7.0+2.0s cast, single-target

    WroughtFireVisual = 39121, // Boss->self, 4.0+1.0s cast, single-target
    WroughtFire = 39122, // Helper->player, 5.0s cast, range 6 circle

    ColossalImpactVisual1 = 36704, // Boss->self, 6.0+2.0s cast, single-target
    ColossalImpactVisual2 = 36705, // Boss->self, 6.0+2.0s cast, single-target
    ColossalImpact = 36706, // Helper->self, 8.0s cast, range 10 circle

    BuildingHeat = 36710, // Helper->players, 7.0s cast, range 6 circle

    AshlayerVisual = 36711, // Boss->self, 3.0+2.0s cast, single-target
    Ashlayer = 36712 // Helper->self, no cast, range 60 circle
}

sealed class Stonecarver(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(2);
    private static readonly AOEShapeRect rect = new(40f, 10f);
    private Impact2? _kb;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(AOEs);
        ref var aoe0 = ref aoes[0];
        aoe0.Risky = true;
        if (count > 1)
        {
            aoe0.Color = Colors.Danger;
        }

        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Stonecarver1:
            case (uint)AID.Stonecarver2:
            case (uint)AID.Stonecarver3:
            case (uint)AID.Stonecarver4:
                AOEs.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), risky: false));
                if (AOEs.Count == 2)
                {
                    _kb ??= Module.FindComponent<Impact2>();

                    var aoes = CollectionsMarshal.AsSpan(AOEs);
                    ref var aoe1 = ref aoes[0];
                    ref var aoe2 = ref aoes[1];
                    if (aoe1.Activation > aoe2.Activation)
                    {
                        (aoe1, aoe2) = (aoe2, aoe1);
                    }
                }
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (AOEs.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.Stonecarver1:
                case (uint)AID.Stonecarver2:
                case (uint)AID.Stonecarver3:
                case (uint)AID.Stonecarver4:
                    AOEs.RemoveAt(0);
                    break;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (AOEs.Count != 0)
        {
            hints.AddForbiddenZone(new SDInvertedRect(Arena.Center, new WDir(1f, default), 1.5f, 1.5f, 40f), _kb!.Casters.Count != 0 ? _kb.Casters.Ref(0).Activation : AOEs.Ref(0).Activation);
        }
    }
}

sealed class Shatter(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeRect rectCenter = new(40f, 10f), rectSides = new(45f, 11f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        if (aoes[0].Activation.AddSeconds(-6d) <= WorldState.CurrentTime)
        {
            return aoes;
        }
        return [];
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShapeRect rect, WPos pos, Angle rot) => _aoes.Add(new(rect, pos, rot, Module.CastFinishAt(spell, 15.1d)));
        switch (spell.Action.ID)
        {
            case (uint)AID.MaulworkFirstCenter:
            case (uint)AID.MaulworkSecondCenter:
                AddAOE(rectCenter, spell.LocXZ, spell.Rotation);
                break;
            case (uint)AID.MaulworkFirstSides:
            case (uint)AID.MaulworkSecondSides:
                var z = -453.025f;
                AddAOE(rectSides, new(91.539f, z), -17.004f.Degrees());
                AddAOE(rectSides, new(108.415f, z), 16.999f.Degrees());
                break;
            case (uint)AID.ShatterCenter:
            case (uint)AID.ShatterLR1:
            case (uint)AID.ShatterLR2:
                _aoes.Clear();
                break;
        }
    }
}

abstract class Impact(BossModule module, uint aid, float distance) : Components.SimpleKnockbacks(module, aid, distance, stopAfterWall: true)
{
    protected float halfWidth = 19f;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var dist = Distance;

            // square intentionally slightly smaller to prevent sus knockback
            hints.AddForbiddenZone(new SDKnockbackInAABBSquareAwayFromOrigin(Arena.Center, c.Origin, dist, halfWidth), c.Activation);
        }
    }
}

sealed class Impact1(BossModule module) : Impact(module, (uint)AID.Impact1, 18f);

sealed class Impact2(BossModule module) : Impact(module, (uint)AID.Impact2, 18f)
{
    private readonly Stonecarver _aoe = module.FindComponent<Stonecarver>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => _aoe.AOEs.Count != 0 && _aoe.AOEs.Ref(0).Check(pos) || !Arena.InBounds(pos);
}

sealed class Impact3(BossModule module) : Impact(module, (uint)AID.Impact3, 20f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == (uint)AID.BuildingHeat)
        {
            halfWidth = 14f;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (spell.Action.ID == (uint)AID.BuildingHeat)
        {
            halfWidth = 19f;
        }
    }
}

sealed class ColossalImpactSkullcrush(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ColossalImpact, (uint)AID.Skullcrush1, (uint)AID.Skullcrush2], 10f);

sealed class DestructiveHeat(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.DestructiveHeat, 6f)
{
    private readonly Impact1 _kb1 = module.FindComponent<Impact1>()!;
    private readonly Impact2 _kb2 = module.FindComponent<Impact2>()!;
    private readonly Impact3 _kb3 = module.FindComponent<Impact3>()!;
    private WPos origin;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Service.Config.Get<AIConfig>().MultiboxMode)
            return;

        if (Spreads.Count != 0)
        {
            if (_kb1.Casters.Count != 0)
            {
                origin = new(100f, -400f);
            }
            else if (_kb2.Casters.Count != 0)
            {
                origin = _kb2.Casters.Ref(0).Origin;
            }
            else if (_kb3.Casters.Count != 0)
            {
                origin = _kb3.Casters.Ref(0).Origin;
            }
            if (origin != default)
            {
                base.AddAIHints(slot, actor, assignment, hints);
                hints.AddForbiddenZone(new SDInvertedCircle(origin, 15f), Spreads.Ref(0).Activation);
            }
            else
            { }
        }
    }
}

sealed class Landing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Landing, 8f);

abstract class DeepThunder(BossModule module, uint aid) : Components.CastTowers(module, aid, 6f, 4, 4);
sealed class DeepThunder1(BossModule module) : DeepThunder(module, (uint)AID.DeepThunderTower1);
sealed class DeepThunder2(BossModule module) : DeepThunder(module, (uint)AID.DeepThunderTower2);

sealed class WroughtFire(BossModule module) : Components.BaitAwayCast(module, (uint)AID.WroughtFire, 6f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);
sealed class BuildingHeat(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.BuildingHeat, 6f, 4, 4);
sealed class Ashlayer(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.AshlayerVisual, (uint)AID.Ashlayer, 2.2d);

sealed class MultiboxSupport(BossModule module) : MultiboxComponent(module)
{
    private readonly DestructiveHeat _destructiveHeat = module.FindComponent<DestructiveHeat>()!;
    private readonly Impact1 _kb1 = module.FindComponent<Impact1>()!;
    private readonly Impact2 _kb2 = module.FindComponent<Impact2>()!;
    private readonly Impact3 _kb3 = module.FindComponent<Impact3>()!;
    private readonly WroughtFire _wroughtFire = module.FindComponent<WroughtFire>()!;

    private enum MechanicState { None, DestructiveHeat, WroughtFire }
    private MechanicState _currentMechanic;
    private DateTime _destructiveHeatActivation;
    private DateTime _wroughtFireActivation;
    private readonly Dictionary<PartyRolesConfig.Assignment, WPos> partyDestructiveHeatHintPos = [];
    private WPos _lastKnockbackOrigin;
    private Angle _knockbackCornerAngle;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Impact1:
                _lastKnockbackOrigin = _kb1.Casters.Ref(0).Origin;
                break;
            case AID.Impact2:
                _lastKnockbackOrigin = _kb2.Casters.Ref(0).Origin;
                break;
            case AID.Impact3:
                _lastKnockbackOrigin = _kb3.Casters.Ref(0).Origin;
                break;
            case AID.DestructiveHeat:
                _currentMechanic = MechanicState.DestructiveHeat;
                _destructiveHeatActivation = Module.CastFinishAt(spell);
                break;
            case AID.WroughtFire:
                _currentMechanic = MechanicState.WroughtFire;
                _wroughtFireActivation = Module.CastFinishAt(spell);
                break;
        }

        var arenaCenter = Module.Center;
        var knockbackDir = _lastKnockbackOrigin - arenaCenter;
        _knockbackCornerAngle = Angle.FromDirection(-knockbackDir);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.DestructiveHeat)
            _currentMechanic = MechanicState.None;
        else if (spell.Action.ID == (uint)AID.WroughtFire)
            _currentMechanic = MechanicState.None;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_config.MultiboxMode || assignment == PartyRolesConfig.Assignment.Unassigned)
            return;

        if (WorldState.CurrentTime > _destructiveHeatActivation)
            partyDestructiveHeatHintPos.Clear();

        switch (_currentMechanic)
        {
            case MechanicState.DestructiveHeat:
                AddDestructiveHeatHints(slot, actor, assignment, hints);
                break;
            case MechanicState.WroughtFire:
                AddWroughtFireHints(slot, actor, assignment, hints);
                break;
            case MechanicState.None:
                AddGenericMTNorthHint(slot, actor, assignment, hints);
                break;
        }

        if (_currentMechanic is not MechanicState.WroughtFire)
            AddGenericMTNorthHint(slot, actor, assignment, hints);
    }

    private void AddWroughtFireHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (assignment == PartyRolesConfig.Assignment.MT && actor.InstanceID == Raid.Player()?.InstanceID)
        {
            var boss = Module.PrimaryActor;
            var bossPos = boss.Position;
            var bossRotation = boss.Rotation;
            var mtPosition = bossPos + 3f * bossRotation.ToDirection();
            AddGenericGoalDestination(hints, mtPosition);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!_config.MultiboxMode)
            return;

        foreach (var kvp in partyDestructiveHeatHintPos)
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
                Arena.ZoneCircle(kvp.Value.Quantized(), _destructiveHeat.SpreadRadius, Colors.Safe);
                Arena.ZoneCircle(kvp.Value.Quantized(), 0.5f, color);
            }
        }
    }

    private void AddDestructiveHeatHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var spreadRadius = _destructiveHeat.SpreadRadius;

        var activeRoles = new List<PartyRolesConfig.Assignment>();
        for (var index = 0; index < Raid.Members.Count(); ++index)
        {
            var role = _prc[Raid.Members[index].ContentId];
            if (Raid.Members[index].IsValid() && role != PartyRolesConfig.Assignment.Unassigned)
            {
                activeRoles.Add(role);
            }
        }

        var positions = AssignDestructiveHeatPositions(activeRoles, spreadRadius);
        if (actor.InstanceID == Raid.Player()?.InstanceID)
        {
            if (positions.TryGetValue(assignment, out var assignedPos))
            {
                AddGenericGoalDestination(hints, assignedPos);
                partyDestructiveHeatHintPos.Clear();
                partyDestructiveHeatHintPos[assignment] = assignedPos;
            }
        }

        // debug visualization
        partyDestructiveHeatHintPos.Clear();
        foreach (var kvp in positions)
        {
            if (partyDestructiveHeatHintPos.ContainsKey(kvp.Key))
            {
                partyDestructiveHeatHintPos[kvp.Key] = kvp.Value;
            }
            else
            {
                partyDestructiveHeatHintPos.Add(kvp.Key, kvp.Value);
            }
        }
    }

    private Dictionary<PartyRolesConfig.Assignment, WPos> AssignDestructiveHeatPositions(List<PartyRolesConfig.Assignment> activeRoles, float spreadRadius)
    {
        var boss = Module.PrimaryActor;
        var bossPos = boss.Position;
        var positions = new Dictionary<PartyRolesConfig.Assignment, WPos>();

        if (activeRoles.Count == 0)
            return positions;

        var melees = activeRoles.Where(r => r is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT or PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2).OrderBy(r => (int)r).ToList();
        var ranged = activeRoles.Where(r => r is PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.R2 or PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.H2).OrderBy(r => (int)r).ToList();

        var arenaCenter = Module.Center;
        float lateralOffset = spreadRadius * 1.1f;
        spreadRadius *= 1.1f;

        var cornerDirection = _knockbackCornerAngle.ToDirection();
        float distanceToCorner = 20f * 1.414f; // sqrt(2) * arena radius for diagonal distance
        var cornerPos = arenaCenter + distanceToCorner * cornerDirection;
        var safeOffsets = new List<WPos>();
        for (int row = 0; row < 2; row++)
        {
            float distanceFromCorner = (2.0f * spreadRadius) + (row * spreadRadius);
            var basePos = cornerPos - distanceFromCorner * cornerDirection;
            var leftLateralOffset = lateralOffset * (_knockbackCornerAngle + 90.Degrees()).ToDirection();
            var rightLateralOffset = lateralOffset * (_knockbackCornerAngle - 90.Degrees()).ToDirection();

            safeOffsets.Add(basePos + leftLateralOffset);
            safeOffsets.Add(basePos + rightLateralOffset);
        }

        int positionIndex = 0;
        foreach (var melee in melees)
        {
            if (positionIndex < safeOffsets.Count)
                positions[melee] = safeOffsets[positionIndex++];
        }

        foreach (var role in ranged)
        {
            if (positionIndex < safeOffsets.Count)
                positions[role] = safeOffsets[positionIndex++];
        }

        return positions;
    }
}

sealed class D033MaulskullStates : StateMachineBuilder
{
    public D033MaulskullStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Stonecarver>()
            .ActivateOnEnter<Impact1>()
            .ActivateOnEnter<Impact2>()
            .ActivateOnEnter<Impact3>()
            .ActivateOnEnter<ColossalImpactSkullcrush>()
            .ActivateOnEnter<DestructiveHeat>()
            .ActivateOnEnter<Landing>()
            .ActivateOnEnter<Shatter>()
            .ActivateOnEnter<DeepThunder1>()
            .ActivateOnEnter<DeepThunder2>()
            .ActivateOnEnter<WroughtFire>()
            .ActivateOnEnter<BuildingHeat>()
            .ActivateOnEnter<Ashlayer>()
            .ActivateOnEnter<MultiboxSupport>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 829, NameID = 12728, MultiboxSupport = true)]
public sealed class D033Maulskull(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, -430f), new ArenaBoundsSquare(20f));
