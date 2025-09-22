namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE63WornToShadow;

public enum OID : uint
{
    Boss = 0x31D0, // R7.5
    AlkonostsShadow = 0x31D1, // R3.75-7.5
    VorticalOrb1 = 0x3239, // R0.5
    VorticalOrb2 = 0x323A, // R0.5
    VorticalOrb3 = 0x31D2, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target

    Stormcall = 24121, // Boss->self, 5.0s cast, single-target, visual
    Explosion = 24122, // VorticalOrb1/VorticalOrb2/VorticalOrb3->self, 1.5s cast, range 35 circle aoe
    BladedBeak = 24123, // Boss->player, 5.0s cast, single-target, tankbuster
    NihilitysSong = 24124, // Boss->self, 5.0s cast, single-target, visual
    NihilitysSongAOE = 24125, // Helper->self, no cast, ???, raidwide
    Fantod = 24126, // Boss->self, 2.0s cast, single-target, visual
    FantodAOE = 24127, // Helper->location, 3.0s cast, range 3 circle puddle
    Foreshadowing = 24128, // Boss->self, 5.0s cast, single-target, visual
    ShadowsCast = 24129, // AlkonostsShadow->Boss, 5.0s cast, single-target, visual (applies transfiguration status to caster and stores casted spell)
    FrigidPulse = 24130, // Boss->self, 5.0s cast, range 8-25 donut
    PainStorm = 24131, // Boss->self, 5.0s cast, range 36 130-degree cone aoe
    PainfulGust = 24132, // Boss->self, 5.0s cast, range 20 circle aoe
    ForeshadowingPulse = 24133, // AlkonostsShadow->self, 5.0s cast, range 8-25 donut
    ForeshadowingStorm = 24134, // AlkonostsShadow->self, 5.0s cast, range 36 130-degree cone aoe
    ForeshadowingGust = 24135 // AlkonostsShadow->self, 5.0s cast, range 20 circle aoe
}

public enum SID : uint
{
    OrbMovement = 2234 // none->VorticalOrb1/VorticalOrb2/VorticalOrb3, extra=0x1E (fast)/0x49 (slow)
}

sealed class Stormcall(BossModule module) : Components.GenericAOEs(module, (uint)AID.Explosion)
{
    private readonly List<AOEInstance> _aoes = new(3);
    private static readonly AOEShapeCircle circle = new(35f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.OrbMovement)
        {
            _aoes.Add(new(circle, Arena.Center + 30f * (actor.Position - Arena.Center).Normalized(), default, WorldState.FutureTime(status.Extra == 0x1E ? 9.7f : 19.9f), actorID: actor.InstanceID));
            if (_aoes.Count == 3)
                _aoes.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var count = _aoes.Count;
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].ActorID == id)
                {
                    _aoes[i] = new(circle, spell.LocXZ, default, Module.CastFinishAt(spell), actorID: id);
                    return;
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var count = _aoes.Count;
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

sealed class BladedBeak(BossModule module) : Components.SingleTargetCast(module, (uint)AID.BladedBeak);
sealed class NihilitysSong(BossModule module) : Components.RaidwideCast(module, (uint)AID.NihilitysSong);
sealed class Fantod(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FantodAOE, 3f);

sealed class Foreshadowing(BossModule module) : Components.GenericAOEs(module)
{
    private AOEShape? _bossShape;
    private readonly List<(Actor caster, AOEShape? shape)> _addAOEs = []; // shape is null if add starts cast slightly before boss
    private readonly List<AOEInstance> _aoes = new(5);

    private static readonly AOEShapeDonut _shapePulse = new(8f, 25f);
    private static readonly AOEShapeCone _shapeStorm = new(36f, 65f.Degrees());
    private static readonly AOEShapeCircle _shapeGust = new(20f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count == 5 ? CollectionsMarshal.AsSpan(_aoes)[..1] : CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FrigidPulse:
                StartBossCast(_shapePulse);
                break;
            case (uint)AID.PainStorm:
                StartBossCast(_shapeStorm);
                break;
            case (uint)AID.PainfulGust:
                StartBossCast(_shapeGust);
                break;
            case (uint)AID.ShadowsCast:
                _addAOEs.Add((caster, _bossShape)); // depending on timings, this might be null - will be updated when boss aoe starts
                InitIfReady();
                break;
        }
        void StartBossCast(AOEShape shape)
        {
            _bossShape = shape;
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
            var count = _addAOEs.Count;
            for (var i = 0; i < count; ++i)
            {
                var a = _addAOEs[i];
                if (a.shape == null)
                    _addAOEs[i] = (a.caster, shape);
            }
            InitIfReady();

        }
        void InitIfReady()
        {
            if (_bossShape == null)
                return;
            var count = _addAOEs.Count;
            if (count != 4)
                return;
            var act = Module.CastFinishAt(spell, 11.1f);
            for (var i = 0; i < count; ++i)
            {
                var a = _addAOEs[i];
                _aoes.Add(new(a.shape!, a.caster.Position.Quantized(), a.caster.Rotation, act, actorID: a.caster.InstanceID));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FrigidPulse:
            case (uint)AID.PainStorm:
            case (uint)AID.PainfulGust:
                _bossShape = null;
                RemoveAOE();
                break;
            case (uint)AID.ForeshadowingPulse:
            case (uint)AID.ForeshadowingStorm:
            case (uint)AID.ForeshadowingGust:
                RemoveAOE();
                _addAOEs.Clear();
                break;
        }
        void RemoveAOE()
        {
            var count = _aoes.Count;
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

sealed class CE63WornToShadowStates : StateMachineBuilder
{
    public CE63WornToShadowStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Stormcall>()
            .ActivateOnEnter<BladedBeak>()
            .ActivateOnEnter<NihilitysSong>()
            .ActivateOnEnter<Fantod>()
            .ActivateOnEnter<Foreshadowing>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 778, NameID = 28)] // bnpcname=9973
public sealed class CE63WornToShadow(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(-480f, -690f), 29.5f, 32)]);
    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 30f);
}
