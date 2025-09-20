namespace BossMod.Dawntrail.FATE.MicaTheMagicalMu;

public enum OID : uint
{
    Boss = 0x43EB, // R7.999

    CardHelper = 0x43ED, // R0.5
    MagicalHoop = 0x43F4, // R1.0
    Card1 = 0x43EE, // R1.0
    Card2 = 0x43EF, // R1.0
    Card3 = 0x43F0, // R1.0
    Card4 = 0x43F1, // R1.0
    Card5 = 0x43F2, // R1.0
    Card6 = 0x43F3, // R1.0
    Helper = 0x43EC
}

public enum AID : uint
{
    AutoAttack = 38709, // Boss->player, no cast, single-target
    Teleport = 38708, // Boss->location, no cast, single-target

    DealDrawStart = 38864, // Boss->location, no cast, single-target, visual (???)
    Deal = 38665, // Boss->self, 4.0s cast, single-target, visual (show pattern)
    DrawFirst1 = 38666, // Boss->self, 4.0s cast, single-target
    DrawFirst2 = 38667, // Boss->self, 4.0s cast, single-target
    DrawFirst3 = 38668, // Boss->self, 4.0s cast, single-target
    DrawFirst4 = 38669, // Boss->self, 4.0s cast, single-target
    DrawFirst5 = 38670, // Boss->self, 4.0s cast, single-target
    DrawFirst6 = 38671, // Boss->self, 4.0s cast, single-target
    DrawRest1 = 38672, // Boss->self, 1.5s cast, single-target
    DrawRest2 = 38673, // Boss->self, 1.5s cast, single-target
    DrawRest3 = 38674, // Boss->self, 1.5s cast, single-target
    DrawRest4 = 38675, // Boss->self, 1.5s cast, single-target
    DrawRest5 = 38676, // Boss->self, 1.5s cast, single-target
    DrawRest6 = 38677, // Boss->self, 1.5s cast, single-target
    CardTrick = 38678, // Boss->self, 4.0+0.5s cast, single-target, visual (explode squares)
    CardTrickAOEReal = 38679, // Helper->location, 1.5s cast, range 20 width 14 rect
    CardTrickAOEFake = 39156, // Helper->location, 1.5s cast, range 20 width 14 rect, visual (correct square)
    CardResolve1 = 38680, // CardHelper->location, no cast, range 40 width 42 rect, visual (remove card-drawn status)
    CardResolve2 = 38681, // CardHelper->location, no cast, range 40 width 42 rect, visual (remove card-drawn status)
    CardResolve3 = 38682, // CardHelper->location, no cast, range 40 width 42 rect, visual (remove card-drawn status)
    CardResolve4 = 38683, // CardHelper->location, no cast, range 40 width 42 rect, visual (remove card-drawn status)
    CardResolve5 = 38684, // CardHelper->location, no cast, range 40 width 42 rect, visual (remove card-drawn status)
    CardResolve6 = 38685, // CardHelper->location, no cast, range 40 width 42 rect, visual (remove card-drawn status)

    BewitchingBall = 38686, // Boss->self, 4.0s cast, single-target, visual (prepare for next mechanic)
    RollingStarlightFirst = 38687, // Boss->location, 7.0s cast, width 10 rect charge
    RollingStarlightRest = 38688, // Boss->location, no cast, width 10 rect charge
    RollingStarlightVisual1 = 38996, // Helper->location, 2.0s cast, range 20 width 10 rect
    RollingStarlightVisual2 = 38997, // Helper->location, 2.0s cast, range 29 width 10 rect
    RollingStarlightVisual3 = 38998, // Helper->location, 2.0s cast, range 43 width 10 rect
    RollingStarlightVisual4 = 38999, // Helper->location, 2.0s cast, range 52 width 10 rect
    BewitchingBallEnd = 38690, // Boss->self, no cast, single-target, visual (clear the status)

    MagicalHat = 38691, // Boss->self, 4.0+0.8s cast, single-target, visual (create hoops)
    TwinkleToss = 38692, // MagicalHoop->self, 3.0s cast, range 42 width 5 rect

    FlourishingBow = 38696, // Boss->self, 4.5+0.5s cast, single-target, visual (in/out)
    TwinklingFlourishLong = 38697, // Helper->location, 7.0s cast, range 10 circle
    TwinklingFlourishShort = 38698, // Helper->location, 5.0s cast, range 10 circle
    TwinklingRingLong = 38700, // Helper->location, 7.0s cast, range 10-30 donut
    TwinklingRingShort = 38701, // Helper->location, 5.0s cast, range 10-30 donut
    DoubleMisdirect = 38693, // Boss->self, 4.5+0.5s cast, single-target, visual (pizzas)
    DoubleMisdirectAOELong = 38694, // Helper->location, 7.0s cast, range 40 60-degree cone
    DoubleMisdirectAOEShort = 38695, // Helper->location, 5.0s cast, range 40 60-degree cone
    RoundOfApplause = 38699, // Boss->self, 4.5+0.5s cast, single-target, visual (in/out + pizzas)

    Shimmerstorm = 38702, // Boss->self, 2.5+0.5s cast, single-target, visual (puddles)
    ShimmerstormAOE = 38703, // Helper->location, 3.0s cast, range 6 circle
    Shimmerstrike = 38704, // Boss->self, 4.5+0.5s cast, single-target, visual (tankbuster)
    ShimmerstrikeAOE = 38705, // Helper->players, 5.0s cast, range 6 circle
    SparkOfImagination = 38706, // Boss->self, 4.5+0.5s cast, single-target, visual (raidwide)
    SparkOfImaginationAOE = 38707, // Helper->location, 4.0s cast, range 35 circle, raidwide
    End = 38710 // Boss->self, no cast, single-target, visual (end fight)
}

sealed class Draw(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor>[] _cards = [
        module.Enemies((uint)OID.Card1),
        module.Enemies((uint)OID.Card2),
        module.Enemies((uint)OID.Card3),
        module.Enemies((uint)OID.Card4),
        module.Enemies((uint)OID.Card5),
        module.Enemies((uint)OID.Card6),
    ];
    private AOEInstance[] _aoes = [];
    private readonly List<int> cachedCardIndices = new(2);

    private static readonly AOEShapeRect _shape = new(20f, 7f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.DrawFirst1:
            case (uint)AID.DrawRest1:
                AddAOEs(0);
                break;
            case (uint)AID.DrawFirst2:
            case (uint)AID.DrawRest2:
                AddAOEs(1);
                break;
            case (uint)AID.DrawFirst3:
            case (uint)AID.DrawRest3:
                AddAOEs(2);
                break;
            case (uint)AID.DrawFirst4:
            case (uint)AID.DrawRest4:
                AddAOEs(3);
                break;
            case (uint)AID.DrawFirst5:
            case (uint)AID.DrawRest5:
                AddAOEs(4);
                break;
            case (uint)AID.DrawFirst6:
            case (uint)AID.DrawRest6:
                AddAOEs(5);
                break;
            case (uint)AID.CardTrick:
                var activation = Module.CastFinishAt(spell, 0.6d);
                if (_aoes.Length != 0)
                {
                    for (var i = 0; i < 5; ++i)
                    {
                        ref var aoe = ref _aoes[i];
                        aoe.Activation = activation;
                        aoe.Risky = true;
                    }
                }
                break;
        }
    }

    private void AddAOEs(int safeZoneIndex)
    {
        if (_aoes.Length != 0)
        {
            cachedCardIndices.Add(safeZoneIndex);
            return;
        }
        _aoes = new AOEInstance[5];
        var index = 0;
        var dir = new WDir(default, -10f);
        for (var i = 0; i < 6; ++i)
        {
            if (i != safeZoneIndex)
            {
                var cards = _cards[i];
                if (cards.Count != 0)
                {
                    _aoes[index++] = new(_shape, (cards[0].Position + dir).Quantized(), Angle.AnglesCardinals[1], risky: false);
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CardTrickAOEFake)
        {
            _aoes = [];
            if (cachedCardIndices.Count != 0)
            {
                AddAOEs(cachedCardIndices[0]);
                cachedCardIndices.RemoveAt(0);
            }
        }
    }
}

sealed class FlourishingBow(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);

    private static readonly AOEShapeCircle _shapeOut = new(10f);
    private static readonly AOEShapeDonut _shapeIn = new(10f, 30f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.TwinklingFlourishLong or (uint)AID.TwinklingFlourishShort => _shapeOut,
            (uint)AID.TwinklingRingLong or (uint)AID.TwinklingRingShort => _shapeIn,
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            if (_aoes.Count > 1)
            {
                var aoes = CollectionsMarshal.AsSpan(_aoes);
                ref var aoe1 = ref aoes[0];
                ref var aoe2 = ref aoes[1];
                if (aoe1.Activation > aoe2.Activation)
                {
                    (aoe1, aoe2) = (aoe2, aoe1);
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.TwinklingFlourishLong or (uint)AID.TwinklingFlourishShort or (uint)AID.TwinklingRingLong or (uint)AID.TwinklingRingShort)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class DoubleMisdirect(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.DoubleMisdirectAOELong, (uint)AID.DoubleMisdirectAOEShort], new AOEShapeCone(40f, 30f.Degrees()), 3, 6);

sealed class RollingStarlight(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(7);
    private int lastVersion;
    private static readonly AOEShapeRect[] rects = [new(20f, 5f), new(29f, 5f), new(43f, 5f), new(52f, 5f)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void Update()
    {
        var count = _aoes.Count;
        if (lastVersion != count)
        {
            if (count > 1)
            {
                ref var aoe = ref _aoes.Ref(0);
                aoe.Color = Colors.Danger;
            }
            lastVersion = count;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = spell.Action.ID switch
        {
            (uint)AID.RollingStarlightVisual1 => rects[0],
            (uint)AID.RollingStarlightVisual2 => rects[1],
            (uint)AID.RollingStarlightVisual3 => rects[2],
            (uint)AID.RollingStarlightVisual4 => rects[3],
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 5d + _aoes.Count * 0.6d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.RollingStarlightFirst or (uint)AID.RollingStarlightRest)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class MagicalHat(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TwinkleToss, new AOEShapeRect(42f, 2.5f), 4);
sealed class Shimmerstorm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ShimmerstormAOE, 6f);
sealed class Shimmerstrike(BossModule module) : Components.BaitAwayCast(module, (uint)AID.ShimmerstrikeAOE, 6f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);
sealed class SparkOfImagination(BossModule module) : Components.RaidwideCast(module, (uint)AID.SparkOfImaginationAOE);

sealed class MicaTheMagicalMuStates : StateMachineBuilder
{
    public MicaTheMagicalMuStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Draw>()
            .ActivateOnEnter<FlourishingBow>()
            .ActivateOnEnter<DoubleMisdirect>()
            .ActivateOnEnter<RollingStarlight>()
            .ActivateOnEnter<MagicalHat>()
            .ActivateOnEnter<Shimmerstorm>()
            .ActivateOnEnter<Shimmerstrike>()
            .ActivateOnEnter<SparkOfImagination>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Fate, GroupID = 1922, NameID = 13049)]
public sealed class MicaTheMagicalMu(WorldState ws, Actor primary) : BossModule(ws, primary, new(791f, 593f), new ArenaBoundsRect(20.5f, 19.5f))
{
    protected override bool CheckPull() => base.CheckPull() && (Center - Raid.Player()!.Position).LengthSq() < 420f;
}
