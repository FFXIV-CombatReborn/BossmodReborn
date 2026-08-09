namespace BossMod.Dawntrail.Foray.FATE.Thunderregnum;

public enum OID : uint {
    Cresceregina = 0x4D63,
    Helper = 0x233C,
    Cresceregina1 = 0x4EC4, // R0.500, x0 (spawn during fight)
    Cresceregina2 = 0x4EC3, // R0.500, x0 (spawn during fight)
    Cresceregina3 = 0x4EB1, // R0.500, x0 (spawn during fight)
    Cresceregina4 = 0x4D65, // R1.000, x0 (spawn during fight)
    BallOfLevin = 0x4D64, // R2.000, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 50539, // Cresceregina->player, no cast, single-target
    HighCaterwaul = 49499, // Cresceregina->self, 3.0s cast, single-target
    RegalFulguration = 49494, // Cresceregina->self, 5.0s cast, range 40 180.000-degree cone
    RegalFulguration1 = 49495, // Cresceregina->self, 5.0s cast, range 40 180.000-degree cone
    Thunderbolt = 49500, // 4EB1/4EC3/4EC4->location, 3.5s cast, range 10 circle
    NobleBlaster = 49501, // 4D64->self, 3.5s cast, range 50 width 5 rect
    ThunderboltPuddle = 49502, // 4D65->location, 5.0s cast, range 10 circle
    ThunderboltPuddle1 = 49919, // 4D65->location, 5.5s cast, range 10 circle
    ThunderboltPuddle2 = 49920, // 4D65->location, 6.0s cast, range 10 circle
    ThunderboltPuddle3 = 49921, // 4D65->location, 6.5s cast, range 10 circle
    ThunderboltPuddle4 = 49922, // 4D65->location, 7.0s cast, range 10 circle
    ThunderboltPuddle5 = 49923, // 4D65->location, 7.5s cast, range 10 circle
    ThunderboltPuddle6 = 49924, // 4D65->location, 8.0s cast, range 10 circle
    ThunderboltPuddle7 = 49925, // 4D65->location, 8.5s cast, range 10 circle
    ThunderboltPuddle8 = 49926, // 4D65->location, 9.0s cast, range 10 circle
}

sealed class RegalFulguration(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.RegalFulguration, (uint)AID.RegalFulguration1], new AOEShapeCone(40.0f, 90.0f.Degrees()));
sealed class Thunderbolt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Thunderbolt, 10f);
sealed class NobleBlaster(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NobleBlaster, new AOEShapeRect(50.0f, 2.5f));

sealed class ThunderboltPuddle(BossModule module) : Components.GenericAOEs(module) {
    private static readonly AOEShapeCircle Shape = new(10f);
    private readonly List<AOEInstance> _aoes = [];
    private readonly List<AOEInstance> _displayed = [with(9)];
    private readonly HashSet<uint> _seenGlobalSequences = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        switch ((AID)spell.Action.ID) {
            case AID.ThunderboltPuddle:
            case AID.ThunderboltPuddle1:
            case AID.ThunderboltPuddle2:
            case AID.ThunderboltPuddle3:
            case AID.ThunderboltPuddle4:
            case AID.ThunderboltPuddle5:
            case AID.ThunderboltPuddle6:
            case AID.ThunderboltPuddle7:
            case AID.ThunderboltPuddle8:
                if (!_aoes.Any(aoe => aoe.ActorID == caster.InstanceID))
                    // Start movement one second before the damage packet. The nine circles resolve
                    // in 0.5s steps; waiting for the raw finish time makes autorotation trail the
                    // visible sequence by roughly one circle.
                    _aoes.Add(new(Shape, spell.LocXZ, activation: Module.CastFinishAt(spell, -1f), actorID: caster.InstanceID,
                        shapeDistance: Shape.Distance(spell.LocXZ, default)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        switch ((AID)spell.Action.ID) {
            case AID.ThunderboltPuddle:
            case AID.ThunderboltPuddle1:
            case AID.ThunderboltPuddle2:
            case AID.ThunderboltPuddle3:
            case AID.ThunderboltPuddle4:
            case AID.ThunderboltPuddle5:
            case AID.ThunderboltPuddle6:
            case AID.ThunderboltPuddle7:
            case AID.ThunderboltPuddle8:
                if (spell.GlobalSequence != 0 && !_seenGlobalSequences.Add(spell.GlobalSequence))
                    break;
                ++NumCasts;
                _aoes.RemoveAll(aoe => aoe.ActorID == caster.InstanceID);
                break;
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        _displayed.Clear();
        _aoes.Sort((a, b) => a.Activation.CompareTo(b.Activation));
        if (_aoes.Count == 0)
            return CollectionsMarshal.AsSpan(_displayed);

        var riskyDeadline = _aoes[0].Activation.AddSeconds(0.2d);
        // All nine casts start together, so show the complete route immediately. Only the next
        // circle is risky; later circles are planning markers and must not constrain pathfinding.
        for (var i = 0; i < _aoes.Count; ++i) {
            var aoe = _aoes[i];
            aoe.Risky = aoe.Activation <= riskyDeadline;
            aoe.Color = aoe.Risky ? Colors.Danger : Colors.AOE;
            _displayed.Add(aoe);
        }
        return CollectionsMarshal.AsSpan(_displayed);
    }
}

[SkipLocalsInit]
sealed class ThunderregnumStates : StateMachineBuilder {
    public ThunderregnumStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<RegalFulguration>()
            .ActivateOnEnter<Thunderbolt>()
            .ActivateOnEnter<NobleBlaster>()
            .ActivateOnEnter<ThunderboltPuddle>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(ThunderregnumStates),
    ConfigType = null, // replace null with typeof(CrescereginaConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.Cresceregina,
    Contributors = "KanoNoUta",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.ForayFATE,
    GroupID = 1093u,
    NameID = 2084u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class Thunderregnum(WorldState ws, Actor primary) : OpenWorldFate(ws, primary);
