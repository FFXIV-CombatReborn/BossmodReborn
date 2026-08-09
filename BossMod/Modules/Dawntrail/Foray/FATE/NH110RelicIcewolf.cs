namespace BossMod.Dawntrail.Foray.FATE.NH110RelicIcewolf;

public enum OID : uint
{
    RelicIcewolf = 0x4D5E, // R6.750
    IcePillar = 0x4D5F,
    BlizzardHelper = 0x4D60,
    EndlessFrostHelper = 0x4DA0,
}

public enum AID : uint
{
    AutoAttack = 50536, // RelicIcewolf->player, no cast, single-target
    StormWithin = 49756, // RelicIcewolf->self, 5.0s cast, range 10 circle
    StormWithout = 49757, // RelicIcewolf->self, 5.0s cast, range 10-40 donut
    IcePillarVisual = 49758, // RelicIcewolf->self, 3.0s cast, single-target
    Rush = 49759, // IcePillar->self, 4.0s cast, range 80 width 4 rect
    AgeOfEndlessFrostVisual = 49760, // RelicIcewolf->self, 3.0s cast, single-target
    AgeOfEndlessFrost = 49761, // EndlessFrostHelper->self, 3.0s cast, range 40 60-degree cone
    RoaringBlizzard = 49765, // RelicIcewolf->self, 5.0s cast, range 50 60-degree cone
    StormWithinAOE = 49766, // BlizzardHelper->self, no cast, range 10 circle
    StormWithoutAOE = 49767, // BlizzardHelper->self, no cast, range 10-40 donut
    IcePillar = 49770, // IcePillar->self, 3.0s cast, range 4 circle
}

// The visible boss cast resolves one second before the helper damage event. Keep the
// telegraph active through the real hit instead of clearing it at cast finish.
sealed class StormWithinWithout(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Circle = new(10f);
    private static readonly AOEShapeDonut Donut = new(10f, 40f);
    private AOEInstance[] _aoe = [];
    private AID _sourceCast;
    private AID _expectedResolve;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.StormWithin:
                _aoe = [new(Circle, caster.Position, activation: Module.CastFinishAt(spell, 1d))];
                _sourceCast = AID.StormWithin;
                _expectedResolve = AID.StormWithinAOE;
                break;
            case AID.StormWithout:
                _aoe = [new(Donut, caster.Position, activation: Module.CastFinishAt(spell, 1d))];
                _sourceCast = AID.StormWithout;
                _expectedResolve = AID.StormWithoutAOE;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == _sourceCast && !spell.EventHappened)
            Clear();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == _expectedResolve)
            Clear();
    }

    public override void Update()
    {
        if (_aoe.Length != 0 && WorldState.CurrentTime > _aoe[0].Activation.AddSeconds(1d))
            Clear();
    }

    private void Clear()
    {
        _aoe = [];
        _sourceCast = default;
        _expectedResolve = default;
    }
}

sealed class IcePillar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IcePillar, new AOEShapeCircle(4f));
sealed class Rush(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Rush, new AOEShapeRect(80f, 2f));
sealed class AgeOfEndlessFrost(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AgeOfEndlessFrost, new AOEShapeCone(40f, 30f.Degrees()))
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        Casters.Sort((left, right) => left.Activation.CompareTo(right.Activation));
        var aoes = CollectionsMarshal.AsSpan(Casters);
        if (aoes.Length == 0)
            return aoes;

        // Helpers announce two three-cone waves together. Keep the second wave visible without
        // combining it with the first into a fake full-circle danger zone.
        var riskyDeadline = aoes[0].Activation.AddSeconds(0.2d);
        foreach (ref var aoe in aoes)
        {
            aoe.Risky = aoe.Activation <= riskyDeadline;
            aoe.Color = aoe.Risky ? Colors.Danger : Colors.AOE;
        }
        return aoes;
    }
}
sealed class RoaringBlizzard(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RoaringBlizzard, new AOEShapeCone(50f, 30f.Degrees()));

[SkipLocalsInit]
sealed class RelicIcewolfStates : StateMachineBuilder
{
    public RelicIcewolfStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StormWithinWithout>()
            .ActivateOnEnter<IcePillar>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<AgeOfEndlessFrost>()
            .ActivateOnEnter<RoaringBlizzard>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(RelicIcewolfStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    PrimaryActorOID = (uint)OID.RelicIcewolf,
    Contributors = "KanoNoUta",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.ForayFATE,
    GroupID = 1093u,
    NameID = 2080u,
    SortOrder = 1)]
[SkipLocalsInit]
public sealed class RelicIcewolf(WorldState ws, Actor primary) : OpenWorldFate(ws, primary);
