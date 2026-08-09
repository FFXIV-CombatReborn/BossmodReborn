namespace BossMod.Dawntrail.Foray.FATE.NH104Stormcaller;

public enum OID : uint {
    Stormcaller = 0x4BEC,
    Helper = 0x233C,
    StormcallerHelper = 0x4BED, // R1.000, x0 (spawn during fight)
    BitingWind = 0x4C25, // R1.000, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 50854, // Stormcaller->player, no cast, single-target
    Teleport = 45587, // Stormcaller->location, no cast, single-target
    Stormcall = 47580, // Stormcaller->self, 3.0s cast, single-target
    FreefallTeleport = 47598, // Stormcaller->location, 4.0s cast, single-target
    Freefall = 47584, // Stormcaller->location, no cast, range 12 circle
    BitingScratch = 47588, // Stormcaller->self, 5.0s cast, range 40 90.000-degree cone
    Windage = 47583, // 4C25->self, 2.0s cast, range 7 circle

    FocusedTremor1 = 47587, // 4BED->location, 6.0s cast, range 10 circle
    FocusedTremor2 = 47586, // 4BED->location, 8.0s cast, range 10-20 donut
    FocusedTremor3 = 47585, // 4BED->location, 10.0s cast, range 20-30 donut

    FocusedTremor4 = 47594, // 4BED->location, 9.0s cast, range 10 circle
    FocusedTremor5 = 47593, // 4BED->location, 11.0s cast, range 10-20 donut
    FocusedTremor6 = 47592, // 4BED->location, 13.0s cast, range 20-30 donut

    FocusedTremor7 = 47597, // 4BED->location, 11.5s cast, range 10 circle
    FocusedTremor8 = 47596, // 4BED->location, 13.5s cast, range 10-20 donut
    FocusedTremor9 = 47595, // 4BED->location, 15.5s cast, range 20-30 donut
}

sealed class Windage(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Windage, new AOEShapeCircle(7.0f));
sealed class BitingScratch(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BitingScratch, new AOEShapeCone(40.0f, 45.0f.Degrees()));

// FreefallTeleport supplies the landing point before the boss jumps. Damage is dealt at that
// same point three times: ~0.23s, ~3.06s and ~5.84s after the teleport cast resolves.
sealed class FreefallSequence(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Shape = new(12f);
    private static readonly double[] ResolveOffsets = [0.23d, 3.06d, 5.84d];
    private readonly List<AOEInstance> _aoes = [with(3)];
    private readonly HashSet<uint> _seenGlobalSequences = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => _aoes.Count == 0 ? [] : CollectionsMarshal.AsSpan(_aoes)[..1];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.FreefallTeleport || spell.EventHappened)
            return;

        _aoes.Clear();
        _seenGlobalSequences.Clear();
        foreach (var offset in ResolveOffsets)
            _aoes.Add(new(Shape, spell.LocXZ, activation: Module.CastFinishAt(spell, offset), risky: false));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.Freefall
            || spell.GlobalSequence != 0 && !_seenGlobalSequences.Add(spell.GlobalSequence))
            return;

        ++NumCasts;
        if (_aoes.Count != 0)
            _aoes.RemoveAt(0);
    }

    public override void Update()
    {
        while (_aoes.Count != 0 && WorldState.CurrentTime > _aoes[0].Activation.AddSeconds(1d))
            _aoes.RemoveAt(0);
    }
}

// TODO improve colour display
sealed class FocusedTremor(BossModule module) : Components.ConcentricAOEs(module, _shapes) {
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 20f), new AOEShapeDonut(20f, 30f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID is (uint)AID.FocusedTremor1 or (uint)AID.FocusedTremor4 or (uint)AID.FocusedTremor7) {
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell) {
        if (Sequences.Count != 0) {
            var order = spell.Action.ID switch {
                (uint)AID.FocusedTremor1 or (uint)AID.FocusedTremor4 or (uint)AID.FocusedTremor7 => 0,
                (uint)AID.FocusedTremor2 or (uint)AID.FocusedTremor5 or (uint)AID.FocusedTremor8 => 1,
                (uint)AID.FocusedTremor3 or (uint)AID.FocusedTremor6 or (uint)AID.FocusedTremor9 => 2,
                _ => -1
            };

            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

[SkipLocalsInit]
sealed class GaleForceEncounterStates : StateMachineBuilder {
    public GaleForceEncounterStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<Windage>()
            .ActivateOnEnter<BitingScratch>()
            .ActivateOnEnter<FreefallSequence>()
            .ActivateOnEnter<FocusedTremor>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(GaleForceEncounterStates),
    ConfigType = null, // replace null with typeof(StormcallerConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.Stormcaller,
    Contributors = "KanoNoUta",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.ForayFATE,
    GroupID = 1093u,
    NameID = 2082u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class GaleForceEncounter(WorldState ws, Actor primary) : OpenWorldFate(ws, primary);
