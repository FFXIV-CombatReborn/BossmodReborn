namespace BossMod.Shadowbringers.Raid.E06Garuda;

public enum OID : uint
{
    Boss = 0x2D0E, // R3.500, Garuda
    Helper = 0x233C, // R0.500
    Ifrit = 0x2D0F, // R3.000
    Raktapaksa = 0x2D10, // R3.500
    TumultuousNexus = 0x2D11, // R1.500
    GreatBallOfFire = 0x2D12, // R1.500
    IfritClone = 0x2DA1, // R3.000
    RaktapaksaClone = 0x2DA2, // R3.500
}

public enum AID : uint
{
    AutoAttackGaruda = 870, // Boss->player, no cast, single-target
    AutoAttackIfrit = 872, // Ifrit/Raktapaksa->player, no cast, single-target
    //_Ability_ = 19408, // Boss->location, no cast, single-target
    AirBumpVisual = 19409, // Boss->self, no cast, single-target
    OccludedFront = 19410, // Boss->self, 3.0s cast, single-target
    StrikeSpark = 19411, // Ifrit->self, 3.0s cast, single-target
    AirBumpVisualRaktapaksa = 19412, // Raktapaksa->self, no cast, single-target
    VacuumSlice = 19413, // Boss->self, 4.0s cast, range 42 width 8 rect
    IrresistiblePull = 19414, // Helper->self, 4.0s cast, single-target
    Superstorm = 19415, // Boss->self, 5.0s cast, range 40 circle
    Firestorm = 19416, // Helper->self, no cast, range 40 circle
    AirBump = 19417, // Helper->player, 5.5s cast, range 4 circle
    Thorns = 19418, // Helper->location, 4.0s cast, range 4 circle
    Downburst = 19419, // Helper->location, 4.0s cast, range 8 circle
    DownburstRaktapaksa = 19420, // Helper->location, 4.0s cast, range 8 circle
    FerostormAOE = 19421, // Helper->self, no cast, range 40 90-degree cone
    Ferostorm = 19422, // Boss->self, 5.0s cast, single-target
    FerostormVisual2 = 19423, // Boss->self, 5.0s cast, single-target
    StormOfFury = 19424, // Boss->self, 5.0s cast, range 11 circle
    StormOfFuryCone = 19425, // Helper->self, no cast, range 40 ?-degree cone
    Explosion = 19426, // TumultuousNexus->self, 1.0s cast, range 8 circle
    FerostormRaktapaksa = 19427, // Raktapaksa->self, 5.0s cast, single-target
    FerostormRaktapaksaRotated = 19428, // Raktapaksa->self, 5.0s cast, single-target
    FerostormRaktapaksaAOE = 19429, // Helper->self, no cast, range 40 90-degree cone
    StormOfFuryRaktapaksa = 19430, // Raktapaksa->self, 5.0s cast, range 11 circle
    StormOfFuryRaktapaksaCone = 19431, // Helper->self, no cast, range 40 ?-degree cone
    Touchdown = 19432, // Ifrit->location, 5.0s cast, range 40 circle
    HandsOfFlameCharge = 19433, // Ifrit/Raktapaksa->player, no cast, width 6 rect charge
    HandsOfHellCharge = 19434, // Ifrit/Raktapaksa->player, no cast, width 6 rect charge
    HandsOfHellCloneCharge = 19435, // IfritClone/RaktapaksaClone->player, no cast, width 6 rect charge
    HeatBurst = 19436, // Helper->self, 4.0s cast, range 37 width 80 rect
    InstantIncineration = 19437, // Ifrit->player, 5.0s cast, range 4 circle
    ConflagStrike = 19438, // Raktapaksa->self, 31.0s cast, range 40 270-degree cone
    HotFoot = 19439, // Ifrit->self, 5.0s cast, single-target
    SpikeOfFlame = 19440, // GreatBallOfFire->self, 1.0s cast, range 10 circle
    InfernoHowl = 19441, // Ifrit/Raktapaksa->self, 5.0s cast, range 40 circle
    RadiantPlume = 19442, // Helper->location, 5.0s cast, range 8 circle
    EruptionVisual = 19443, // Ifrit->self, 3.5s cast, single-target
    Eruption = 19444, // Helper->location, 3.5s cast, range 8 circle
    //HotFootKick = 19485, // Helper->GreatBallOfFire/TumultuousNexus, no cast, single-target
    HeatBurstVisual = 19486, // Helper->self, 3.5s cast, single-target
    DownburstKnockback = 19488, // Helper->location, 4.0s cast, range 40 circle
    HandsOfFlame = 19710, // Ifrit/Raktapaksa->self, 5.0s cast, single-target
    HandsOfHell = 19711, // Ifrit/Raktapaksa->self, 8.0s cast, single-target
    //_Weaponskill_3 = 19797, // Helper->self, no cast, single-target
    IrresistiblePullAttract = 20025, // Helper->self, 4.0s cast, range 21 width 42 rect
    IfritTeleport = 20376, // Ifrit->location, no cast, single-target
}

public enum TetherID : uint
{
    HandsOfFlame = 104, // player->Ifrit/Raktapaksa
    HandsOfHell = 106, // player->Ifrit/Raktapaksa
    StormOfFury = 109, // player->Boss/Raktapaksa
}

sealed class Ferostorm(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCone _shape = new(40, 45.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var act = Module.CastFinishAt(spell);
        var r = spell.Rotation;
        switch (spell.Action.ID)
        {
            case (uint)AID.Ferostorm:
            case (uint)AID.FerostormRaktapaksa:
                _aoes.Add(new(_shape, caster.Position, r - 45.Degrees(), act));
                _aoes.Add(new(_shape, caster.Position, r + 135.Degrees(), act));
                break;
            case (uint)AID.FerostormVisual2:
            case (uint)AID.FerostormRaktapaksaRotated:
                _aoes.Add(new(_shape, caster.Position, r + 45.Degrees(), act));
                _aoes.Add(new(_shape, caster.Position, r - 135.Degrees(), act));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.FerostormAOE or (uint)AID.FerostormRaktapaksaAOE)
        {
            ++NumCasts;
            _aoes.Clear();
        }
    }
}

sealed class Superstorm(BossModule module) : Components.RaidwideCast(module, (uint)AID.Superstorm);
sealed class InfernoHowl(BossModule module) : Components.RaidwideCast(module, (uint)AID.InfernoHowl);

sealed class AirBump(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.AirBump, 4, 2, 2);
sealed class Thorns(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Thorns, 4);

sealed class Downburst(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Downburst, (uint)AID.DownburstRaktapaksa], new AOEShapeCircle(8));
sealed class DownburstKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.DownburstKnockback, 7);

sealed class StormOfFury(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.StormOfFury, (uint)AID.StormOfFuryRaktapaksa], new AOEShapeCircle(11));
sealed class StormOfFuryTether(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(40, 15.Degrees()), (uint)TetherID.StormOfFury); // TODO: verify angle

sealed class VacuumSlice(BossModule module) : Components.GenericAOEs(module, (uint)AID.VacuumSlice)
{
    public static readonly AOEShapeRect Shape = new(42, 4, 42);
    private AOEInstance[] _aoe = [];

    public WPos? LineOrigin { get; private set; }
    public Angle? LineRotation { get; private set; }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            LineOrigin = caster.Position;
            LineRotation = spell.Rotation;
            _aoe = [new(Shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
            ++NumCasts;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        // idx 1: 0x00800040 / 0x00020001 appear, 0x02000004 / 0x00080004 clear
        if (index == 1 && state is 0x02000004 or 0x00080004)
        {
            _aoe = [];
            LineOrigin = null;
            LineRotation = null;
        }
    }
}

sealed class IrresistiblePull(BossModule module) : Components.GenericKnockback(module, (uint)AID.IrresistiblePullAttract)
{
    private VacuumSlice? _vacuum;
    public const float Distance = 5;
    private readonly List<Actor> _casters = [];

    public bool Active => _casters.Count > 0;

    public static WPos NearestOnLine(WPos p, WPos origin, Angle rotation)
    {
        var dir = rotation.ToDirection();
        return origin + (p - origin).Dot(dir) * dir;
    }

    public static WPos Predict(WPos p, WPos lineOrigin, Angle lineRotation)
    {
        var nearest = NearestOnLine(p, lineOrigin, lineRotation);
        var offset = nearest - p;
        var len = offset.Length();
        return len > 0.1f ? p + offset.Normalized() * Math.Min(Distance, len) : p;
    }

    private readonly Knockback[] _oneKnockback = new Knockback[1];
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_casters.Count == 0)
            return [];
        _vacuum ??= Module.FindComponent<VacuumSlice>();
        if (_vacuum?.LineOrigin is not WPos origin || _vacuum.LineRotation is not Angle rot)
            return [];

        // pulls toward the vacuum line
        {
            _oneKnockback[0] = new(NearestOnLine(actor.Position, origin, rot), Distance, Module.CastFinishAt(_casters[0].CastInfo), kind: Kind.TowardsOrigin);
            return _oneKnockback;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            _casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            _casters.Remove(caster);
    }
}

sealed class Explosion(BossModule module) : Components.GenericAOEs(module, (uint)AID.Explosion)
{
    private readonly List<Actor> _orbs = [];
    private readonly List<AOEInstance> _aoes = [];
    private DateTime _activation;
    private static readonly AOEShapeCircle _shape = new(8f);
    private IrresistiblePull? _pull;
    private VacuumSlice? _vacuum;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoes.Clear();
        if (_activation == default || _orbs.Count == 0)
            return [];

        _pull ??= Module.FindComponent<IrresistiblePull>();
        _vacuum ??= Module.FindComponent<VacuumSlice>();
        // orbs get pulled toward the vacuum line too, so show where they will end up
        var predict = _pull?.Active == true && _vacuum?.LineOrigin is not null;

        for (var i = 0; i < _orbs.Count; ++i)
        {
            var orb = _orbs[i];
            var pos = predict ? IrresistiblePull.Predict(orb.Position, _vacuum!.LineOrigin!.Value, _vacuum.LineRotation!.Value) : orb.Position;
            _aoes.Add(new(_shape, pos, default, _activation));
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.TumultuousNexus)
        {
            _orbs.Add(actor);
            if (_activation == default)
                _activation = WorldState.FutureTime(9.5f);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.IrresistiblePullAttract:
                _activation = Module.CastFinishAt(spell, 1.2f);
                break;
            case (uint)AID.Explosion:
                _activation = Module.CastFinishAt(spell);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            _orbs.Remove(caster);
            if (_orbs.Count == 0)
                _activation = default;
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (_orbs.Remove(actor) && _orbs.Count == 0)
            _activation = default;
    }
}

sealed class Touchdown(BossModule module) : Components.ProximityAOEs(module, (uint)AID.Touchdown, 15);

sealed class HandsOfFlame(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(40, 3), (uint)TetherID.HandsOfFlame, (uint)AID.HandsOfFlameCharge)
{
    public override void Update()
    {
        // the charge goes exactly to the tether target, so stretch the rect to it (only reallocate when it actually changes)
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        for (var i = 0; i < baits.Length; ++i)
        {
            ref var b = ref baits[i];
            if (b.Shape is AOEShapeRect shape)
            {
                var length = (b.Target.Position - b.Source.Position).Length();
                if (Math.Abs(shape.LengthFront - length) > 0.1f)
                    b.Shape = new AOEShapeRect(length, shape.HalfWidth);
            }
        }
    }
}

sealed class HandsOfHell(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(40, 3), (uint)TetherID.HandsOfHell)
{
    public override void Update()
    {
        // the charge goes exactly to the tether target, so stretch the rect to it (only reallocate when it actually changes)
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        for (var i = 0; i < baits.Length; ++i)
        {
            ref var b = ref baits[i];
            if (b.Shape is AOEShapeRect shape)
            {
                var length = (b.Target.Position - b.Source.Position).Length();
                if (Math.Abs(shape.LengthFront - length) > 0.1f)
                    b.Shape = new AOEShapeRect(length, shape.HalfWidth);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.HandsOfHellCharge or (uint)AID.HandsOfHellCloneCharge)
        {
            ++NumCasts;
            for (var i = CurrentBaits.Count - 1; i >= 0; --i)
            {
                var b = CurrentBaits[i];
                if (b.Source == caster || b.Target.InstanceID == spell.MainTargetID)
                    CurrentBaits.RemoveAt(i);
            }
        }
    }
}

sealed class InstantIncineration(BossModule module) : Components.SingleTargetCast(module, (uint)AID.InstantIncineration);
sealed class Eruption(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Eruption, 8);
sealed class RadiantPlume(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RadiantPlume, 8);
sealed class HeatBurst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeatBurst, new AOEShapeRect(37, 40));
sealed class ConflagStrike(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ConflagStrike, new AOEShapeCone(40, 135.Degrees()));

sealed class HotFoot(BossModule module) : Components.GenericAOEs(module, (uint)AID.SpikeOfFlame)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle _shape = new(10);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    private void Predict(WPos ifritPos, DateTime activation, bool refreshActivation = false)
    {
        var orbs = Module.Enemies((uint)OID.GreatBallOfFire);
        if (_aoes.Count == 0)
        {
            // ifrit tps next to the orb that gets kicked to center
            Actor? kicked = null;
            var best = float.MaxValue;
            for (var i = 0; i < orbs.Count; ++i)
            {
                var o = orbs[i];
                if (o.IsDeadOrDestroyed)
                    continue;
                var d = (o.Position - ifritPos).LengthSq();
                if (d < best)
                {
                    best = d;
                    kicked = o;
                }
            }
            if (kicked == null)
                return;
            for (var i = 0; i < orbs.Count; ++i)
            {
                var orb = orbs[i];
                if (!orb.IsDeadOrDestroyed)
                    _aoes.Add(new(_shape, orb == kicked ? Arena.Center : orb.Position, default, activation));
            }
        }
        else if (refreshActivation)
        {
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < aoes.Length; ++i)
                aoes[i].Activation = activation;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.IfritTeleport:
                if (_aoes.Count == 0) // ignore departure teleport
                    Predict(spell.TargetXZ, WorldState.FutureTime(6.5f));
                break;
            case (uint)AID.SpikeOfFlame:
                ++NumCasts;
                for (var i = _aoes.Count - 1; i >= 0; --i)
                {
                    if (_aoes[i].Origin.AlmostEqual(caster.Position, 1.5f))
                        _aoes.RemoveAt(i);
                }
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HotFoot)
            Predict(caster.Position, Module.CastFinishAt(spell, 1.5f), refreshActivation: true);
    }
}

sealed class E06GarudaStates : StateMachineBuilder
{
    private readonly E06Garuda _module;

    public E06GarudaStates(E06Garuda module) : base(module)
    {
        _module = module;
        TrivialPhase()
            .ActivateOnEnter<Ferostorm>()
            .ActivateOnEnter<Superstorm>()
            .ActivateOnEnter<InfernoHowl>()
            .ActivateOnEnter<AirBump>()
            .ActivateOnEnter<Thorns>()
            .ActivateOnEnter<Downburst>()
            .ActivateOnEnter<DownburstKnockback>()
            .ActivateOnEnter<StormOfFury>()
            .ActivateOnEnter<StormOfFuryTether>()
            .ActivateOnEnter<VacuumSlice>()
            .ActivateOnEnter<IrresistiblePull>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<Touchdown>()
            .ActivateOnEnter<HandsOfFlame>()
            .ActivateOnEnter<HandsOfHell>()
            .ActivateOnEnter<InstantIncineration>()
            .ActivateOnEnter<Eruption>()
            .ActivateOnEnter<RadiantPlume>()
            .ActivateOnEnter<HeatBurst>()
            .ActivateOnEnter<ConflagStrike>()
            .ActivateOnEnter<HotFoot>()
            .Raw.Update = () => _module.SeenRaktapaksa && (_module.Raktapaksa()?.IsDeadOrDestroyed ?? true);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "croizat (ported from upstream and modified by LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 719, NameID = 9287, PrimaryActorOID = (uint)OID.Boss)]
public sealed class E06Garuda(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(19.5f))
{
    private Actor? _ifrit;
    private Actor? _raktapaksa;

    public bool SeenRaktapaksa { get; private set; }
    public Actor? Ifrit() => _ifrit;
    public Actor? Raktapaksa() => _raktapaksa;

    protected override void UpdateModule()
    {
        _ifrit ??= FirstTargetable((uint)OID.Ifrit);
        var rakta = FirstTargetable((uint)OID.Raktapaksa);
        if (rakta != null)
        {
            _raktapaksa = rakta;
            SeenRaktapaksa = true;
        }
    }

    private Actor? FirstTargetable(uint oid)
    {
        var actors = Enemies(oid);
        for (var i = 0; i < actors.Count; ++i)
        {
            if (actors[i].IsTargetable)
                return actors[i];
        }
        return null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_ifrit);
        Arena.Actor(_raktapaksa);
    }
}
