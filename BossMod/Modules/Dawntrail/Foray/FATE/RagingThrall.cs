namespace BossMod.Dawntrail.Foray.FATE.RagingThrall;

public enum OID : uint {
    Machetaur = 0x4C26,
    Helper = 0x233C,
    Machetaur1 = 0x4C27, // R1.000, x0 (spawn during fight)
    Machetaur2 = 0x4C52, // R0.500, x0 (spawn during fight)
    Machetaur3 = 0x4EBF, // R0.500, x0 (spawn during fight)
    Machetaur4 = 0x4EC0, // R0.500, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 50534, // Machetaur->player, no cast, single-target
    FocusedTremorCast = 47606, // Machetaur->self, 3.0s cast, single-target
    FocusedTremor = 48374, // Machetaur1->self, 2.2s cast, range 30 circle

    FocusedTremorInner = 47607, // Machetaur1->location, 6.0s cast, range 10 circle
    FocusedTremorMiddle = 47608, // Machetaur1->location, 8.0s cast, range 10-20 donut
    FocusedTremorOuter = 47609, // Machetaur1->location, 10.0s cast, range 20-30 donut

    BruntOfTheBattlefieldCast = 47610, // Machetaur->self, 3.0s cast, single-target
    BruntOfTheBattlefield = 48373, // Machetaur1->self, 4.5s cast, range 10 circle
    Uplift = 47611, // Machetaur2/Machetaur3/Machetaur1/Machetaur4->location, 3.0s cast, range 6 circle

    OctupleSwipeVisual = 47600, // Machetaur->self, 10.0s cast, visual
    OctupleSwipeTelegraph = 47601, // Machetaur1->self, 1.0s cast, range 40 90-degree cone, eight directions in order
    OctupleSwipe1 = 47604, // Machetaur->self, no cast, range 40 90-degree cone
    OctupleSwipe2 = 47605, // Machetaur->self, no cast, range 40 90-degree cone
    OctupleSwipe3 = 47602, // Machetaur->self, no cast, range 40 90-degree cone
}

sealed class FocusedTremor(BossModule module) : Components.RaidwideCast(module, (uint)AID.FocusedTremor);
sealed class BruntOfTheBattlefield(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BruntOfTheBattlefield, 10f);
sealed class Uplift(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Uplift, 6f);

// Eight short helper casts record the directions during the long visual cast. The boss then
// executes those cones in the same order, one every ~2.1s.
sealed class OctupleSwipe(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone Shape = new(40f, 45f.Degrees());
    private readonly List<AOEInstance> _aoes = [with(8)];
    private readonly HashSet<uint> _seenGlobalSequences = [];
    private DateTime _firstActivation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        PruneExpired();
        var count = Math.Min(_aoes.Count, 2);
        return count == 0 ? [] : CollectionsMarshal.AsSpan(_aoes)[..count];
    }

    public override void Update() => PruneExpired();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.OctupleSwipeVisual:
                if (!spell.EventHappened)
                {
                    _aoes.Clear();
                    _firstActivation = Module.CastFinishAt(spell, 0.55d);
                }
                break;
            case (uint)AID.OctupleSwipeTelegraph:
                if (spell.EventHappened || _aoes.Count >= 8)
                    break;
                var order = _aoes.Count;
                var activation = order == 0
                    ? (_firstActivation > WorldState.CurrentTime ? _firstActivation : Module.CastFinishAt(spell, 7.6d))
                    : _aoes[0].Activation.AddSeconds(order * 2.1d);
                _aoes.Add(new(Shape, caster.Position, spell.Rotation, activation, order == 0 ? Colors.Danger : Colors.AOE, order == 0, shapeDistance: Shape.Distance(caster.Position, spell.Rotation)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is not ((uint)AID.OctupleSwipe1) and not ((uint)AID.OctupleSwipe2) and not ((uint)AID.OctupleSwipe3)
            || spell.GlobalSequence != 0 && !_seenGlobalSequences.Add(spell.GlobalSequence))
            return;

        ++NumCasts;
        if (_aoes.Count != 0)
        {
            _aoes.RemoveAt(0);
            MarkNextDanger();
        }
    }

    private void PruneExpired()
    {
        var removed = false;
        while (_aoes.Count != 0 && WorldState.CurrentTime > _aoes[0].Activation.AddSeconds(1d))
        {
            _aoes.RemoveAt(0);
            removed = true;
        }
        if (removed)
            MarkNextDanger();
    }

    private void MarkNextDanger()
    {
        if (_aoes.Count != 0)
        {
            _aoes.Ref(0).Color = Colors.Danger;
            _aoes.Ref(0).Risky = true;
        }
    }
}

// TODO make it a sequence one instead if its always a single one
sealed class FocusedTremorCircle(BossModule module) : Components.GenericAOEs(module) {
    private readonly List<AOEInstance> aoes = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.FocusedTremorInner) {
            aoes.Add(new(new AOEShapeCircle(10), caster.Position, caster.Rotation, Module.CastFinishAt(spell)));
        }

        if (spell.Action.ID == (uint)AID.FocusedTremorMiddle) {
            aoes.Add(new(new AOEShapeDonut(10, 20), caster.Position, caster.Rotation, Module.CastFinishAt(spell)));
        }

        if (spell.Action.ID == (uint)AID.FocusedTremorOuter) {
            aoes.Add(new(new AOEShapeDonut(20, 30), caster.Position, caster.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID is (uint)AID.FocusedTremorInner or (uint)AID.FocusedTremorMiddle or (uint)AID.FocusedTremorOuter) {
            aoes.Sort((a, b) => a.Activation.CompareTo(b.Activation));
            if (aoes.Count > 0) {
                aoes.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        int show = 0;
        var incomingAOEs = aoes.OrderBy(a => a.Activation).Take(2).ToList();

        foreach (ref var aoe in CollectionsMarshal.AsSpan(incomingAOEs)) {
            aoe.Color = show == 0 ? Colors.Danger : Colors.AOE;
            aoe.Risky = show == 0;
            show++;
        }

        return CollectionsMarshal.AsSpan(incomingAOEs);
    }
}

[SkipLocalsInit]
sealed class RagingThrallStates : StateMachineBuilder {
    public RagingThrallStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<OctupleSwipe>()
            .ActivateOnEnter<FocusedTremor>()
            .ActivateOnEnter<FocusedTremorCircle>()
            .ActivateOnEnter<BruntOfTheBattlefield>()
            .ActivateOnEnter<Uplift>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(RagingThrallStates),
    ConfigType = null, // replace null with typeof(MachetaurConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.Machetaur,
    Contributors = "KanoNoUta",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.ForayFATE,
    GroupID = 1093u,
    NameID = 2074u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class Machetaur(WorldState ws, Actor primary) : OpenWorldFate(ws, primary);
