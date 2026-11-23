namespace BossMod.Endwalker.Trial.T06Golbez;

public enum OID : uint
{
    Boss = 0x3F54, // x1
    ShadowDragon = 0x3F55, // x1, Dragon's Descent and Cauterize source
    GaleSphere = 0x3F56, // spawn during fight
    GolbezShadow = 0x3F5A, // x4
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 33867, // Boss->player, no cast, single-target
    Terrastorm = 33893, // Helper->self, 8.0s cast, range 16 circle
    CrescentBlade = 33899, // Boss->self, 5.0s cast, range 22 180-degree cone
    ArcticAssault = 33886, // Boss->self, 3.0s cast, single-target, visual
    ArcticAssaultAOE = 33888, // Helper->self, 4.0s cast, range 15 width 15 rect
    VoidMeteor = 33964, // Boss->self, 5.0s cast, single-target, visual
    VoidComet = 33966, // Helper->player, no cast, range 4 circle, x4 before Void Meteor
    VoidMeteorAOE = 33967, // Helper->player, no cast, range 6 circle, tankbuster
    LingeringSpark = 33895, // Boss->self, 3.0s cast, single-target, visual
    LingeringSparkAOE = 33897, // Helper->location, 3.0s cast, range 5 circle
    BindingCold = 33970, // Boss->self, 5.0s cast, range 40 circle, raidwide
    AzdajasShadow1 = 34558, // Boss->self, 5.0s cast, single-target, visual
    AzdajasShadow2 = 33910, // Boss->self, 4.0s cast, single-target, visual
    BlackFang = 33905, // Boss->self, 4.0s cast, single-target, visual
    BlackFangAOE = 33907, // Helper->self, 10.7s cast, range 40 circle, raidwide, after multiple small hits
    ShadowCrescent = 33927, // Boss->self, 5.0s cast, range 22 180-degree cone
    RisingBeaconVisual = 33931, // ShadowDragon->self, 5.0s cast, single-target, visual
    RisingBeacon = 33935, // Helper->self, 7.2s cast, range 10 circle
    RisingRing = 33936, // Helper->self, 7.2s cast, range 6-22 donut, not seen
    BurningShade = 33939, // Helper->player, 5.0s cast, range 5 circle, spread
    ImmolatingShade = 33941, // Helper->player, 6.0s cast, range 6 circle, stack
    Explosion1 = 33945, // Helper->self, 11.5s cast, range 4 circle, tower for 1
    Explosion2 = 33946, // Helper->self, 11.5s cast, range 4 circle, tower for 2
    Explosion3 = 33947, // Helper->self, 11.5s cast, range 4 circle, tower for 3
    MassiveExplosion = 33951, // Helper->self, no cast, range 40 circle, raidwide after a tower without enough soakers
    DragonsDescent = 33943, // ShadowDragon->self, no cast, range 45 circle, knockback from the marked player
    Cauterize = 33953, // ShadowDragon->self, 3.0s cast, range 50 width 12 rect
    DoubleMeteor = 33972, // Boss->self, 5.0s cast, single-target, visual
    DoubleMeteorAOE1 = 33974, // Helper->location, 6.0s cast, range 40 circle with falloff
    DoubleMeteorAOE2 = 34699, // Helper->location, 6.0s cast, range 40 circle with falloff
    GaleSphere = 33869, // Boss->self, 3.0s cast, single-target, visual
    GaleSpherePrepareN = 33871, // GolbezShadow->self, no cast, single-target, visual
    GaleSpherePrepareE = 33872, // GolbezShadow->self, no cast, single-target, visual
    GaleSpherePrepareW = 33873, // GolbezShadow->self, no cast, single-target, visual
    GaleSpherePrepareS = 33874, // GolbezShadow->self, no cast, single-target, visual
    GaleSphereAOE1 = 33876, // GaleSphere->self, 4.0s cast, range 30 width 5 rect
    GaleSphereAOE2 = 33877, // GaleSphere->self, 7.5s cast, range 30 width 5 rect
    GaleSphereAOE3 = 33878, // GaleSphere->self, 11.0s cast, range 30 width 5 rect
    GaleSphereAOE4 = 33879, // GaleSphere->self, 14.5s cast, range 30 width 5 rect
    VoidStardust1 = 33955, // Boss->self, 3.0s cast, single-target, visual
    VoidStardust2 = 33957, // Helper->self, 6.8s cast, range 6 circle, first pair
    VoidStardustRestVisual = 33959, // Helper->self, 1.0s cast, range 6 circle, resolves ~2.9s after it ends
    VoidStardustRestAOE = 33961, // Helper->self, no cast, range 6 circle
    EventideFall = 33922, // Boss->self, 7.0s cast, single-target, visual
    EventideFallMark = 33924, // Helper->player, no cast, marks both healers
    EventideFallVisual = 33923, // Boss->self, no cast, single-target, visual
    EventideFallAOE = 34483 // Helper->self, no cast, line stack for 4 aimed at a marked player
}

public enum IconID : uint
{
    VoidMeteor = 344, // player
    DragonsDescent = 476 // player
}

public enum TetherID : uint
{
    Cauterize = 17 // ShadowDragon->player
}

sealed class Terrastorm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Terrastorm, 16f);
sealed class Crescents(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.CrescentBlade, (uint)AID.ShadowCrescent], new AOEShapeCone(22f, 90f.Degrees()));
sealed class ArcticAssault(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArcticAssaultAOE, new AOEShapeRect(15f, 7.5f));
sealed class LingeringSpark(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LingeringSparkAOE, 5f);
sealed class BindingCold(BossModule module) : Components.RaidwideCast(module, (uint)AID.BindingCold);
sealed class BlackFang(BossModule module) : Components.RaidwideCast(module, (uint)AID.BlackFangAOE, "Raidwide x3");
sealed class RisingBeacon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RisingBeacon, 10f);
sealed class RisingRing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RisingRing, new AOEShapeDonut(6f, 22f));
sealed class BurningShade(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.BurningShade, 5f);
sealed class ImmolatingShade(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.ImmolatingShade, 6f, 4);
sealed class DoubleMeteor(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.DoubleMeteorAOE1, (uint)AID.DoubleMeteorAOE2], 15f);
sealed class VoidMeteor(BossModule module) : Components.BaitAwayIcon(module, 6f, (uint)IconID.VoidMeteor, (uint)AID.VoidMeteorAOE, 9.1d, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

sealed class DragonsDescent(BossModule module) : Components.GenericKnockback(module, (uint)AID.DragonsDescent)
{
    private Actor? _source;
    private DateTime _activation;
    private readonly Knockback[] _kb = new Knockback[1];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
        => _source != null && _source != actor ? _kb.Single(new(_source.Position, 10f, _activation)) : [];

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.DragonsDescent)
        {
            _source = actor;
            _activation = WorldState.FutureTime(8.1d);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            _source = null;
        }
    }
}

abstract class Explosion(BossModule module, uint aid, int minSoakers) : Components.CastTowers(module, aid, 4f, minSoakers, PartyState.MaxPartySize)
{
    private BitMask _forbidden;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.DragonsDescent)
            _forbidden.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        var towers = CollectionsMarshal.AsSpan(Towers);
        for (var i = 0; i < towers.Length; ++i)
            towers[i].ForbiddenSoakers = _forbidden;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (Towers.Count == 0)
            _forbidden.Reset();
    }
}

sealed class Explosion1(BossModule module) : Explosion(module, (uint)AID.Explosion1, 1);
sealed class Explosion2(BossModule module) : Explosion(module, (uint)AID.Explosion2, 2);
sealed class Explosion3(BossModule module) : Explosion(module, (uint)AID.Explosion3, 3);

sealed class CauterizeBait(BossModule module) : Components.GenericBaitAway(module, (uint)AID.Cauterize)
{
    private static readonly AOEShapeRect _shape = new(50f, 6f);

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Cauterize && WorldState.Actors.Find(tether.Target) is Actor target)
            CurrentBaits.Add(new(source, target, _shape, WorldState.FutureTime(8.1d)));
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Cauterize)
            CurrentBaits.Clear();
    }
}

sealed class Cauterize(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Cauterize, new AOEShapeRect(50f, 6f));

sealed class GaleSphere(BossModule module) : Components.GenericAOEs(module)
{
    private enum Side { S, E, N, W }

    private static readonly AOEShapeRect _shape = new(30f, 2.5f);
    private readonly List<Side> _sides = [];
    private readonly List<Actor>[] _spheres = [[], [], [], []];
    private readonly List<AOEInstance> _aoes = [];

    private int Current()
    {
        for (var i = 0; i < _spheres.Length; ++i)
        {
            if (_spheres[i].Count != 0)
                return i;
        }
        return -1;
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var order = Current();
        if (order < 0)
            return [];
        _aoes.Clear();
        var spheres = _spheres[order];
        for (var i = 0; i < spheres.Count; ++i)
        {
            var s = spheres[i];
            if (s.CastInfo is ActorCastInfo cast)
                _aoes.Add(new(_shape, s.Position, cast.Rotation, Module.CastFinishAt(cast)));
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (_sides.Count != 0)
            hints.Add($"Order: {string.Join(" -> ", _sides)}");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var order = CastOrder(spell.Action.ID);
        if (order >= 0)
            _spheres[order].Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var order = CastOrder(spell.Action.ID);
        if (order < 0)
            return;
        _spheres[order].Remove(caster);
        if (_sides.Count != 0 && _spheres[order].Count == 0)
            _sides.RemoveAt(0);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.GaleSpherePrepareN:
                _sides.Add(Side.N);
                break;
            case (uint)AID.GaleSpherePrepareE:
                _sides.Add(Side.E);
                break;
            case (uint)AID.GaleSpherePrepareW:
                _sides.Add(Side.W);
                break;
            case (uint)AID.GaleSpherePrepareS:
                _sides.Add(Side.S);
                break;
        }
    }

    private static int CastOrder(uint aid) => aid switch
    {
        (uint)AID.GaleSphereAOE1 => 0,
        (uint)AID.GaleSphereAOE2 => 1,
        (uint)AID.GaleSphereAOE3 => 2,
        (uint)AID.GaleSphereAOE4 => 3,
        _ => -1
    };
}

sealed class VoidStardust(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _shape = new(6f);
    private readonly List<(WPos Pos, DateTime Activation)> _puddles = [];
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoes.Clear();
        var count = _puddles.Count;
        var upcomingEnd = Math.Min(count, NumCasts + 12);
        for (var i = NumCasts + 2; i < upcomingEnd; ++i)
            _aoes.Add(new(_shape, _puddles[i].Pos, default, _puddles[i].Activation));
        var currentEnd = Math.Min(count, NumCasts + 2);
        for (var i = NumCasts; i < currentEnd; ++i)
            _aoes.Add(new(_shape, _puddles[i].Pos, default, _puddles[i].Activation, Colors.Danger));
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.VoidStardust2:
                _puddles.Add((caster.Position, Module.CastFinishAt(spell)));
                break;
            case (uint)AID.VoidStardustRestVisual:
                _puddles.Add((caster.Position, Module.CastFinishAt(spell, 2.9d)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.VoidStardust2 or (uint)AID.VoidStardustRestAOE)
            ++NumCasts;
    }
}

sealed class EventideFall(BossModule module) : Components.UniformStackSpread(module, 3f, default, 4, 4)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.EventideFallMark:
                if (WorldState.Actors.Find(spell.MainTargetID) is Actor target)
                    AddStack(target, WorldState.FutureTime(8.6d));
                break;
            case (uint)AID.EventideFallAOE:
                var targets = CollectionsMarshal.AsSpan(spell.Targets);
                for (var i = Stacks.Count - 1; i >= 0; --i)
                {
                    var id = Stacks[i].Target.InstanceID;
                    for (var j = 0; j < targets.Length; ++j)
                    {
                        if (targets[j].ID == id)
                        {
                            Stacks.RemoveAt(i);
                            break;
                        }
                    }
                }
                break;
        }
    }
}

sealed class T06GolbezStates : StateMachineBuilder
{
    public T06GolbezStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Terrastorm>()
            .ActivateOnEnter<Crescents>()
            .ActivateOnEnter<ArcticAssault>()
            .ActivateOnEnter<LingeringSpark>()
            .ActivateOnEnter<BindingCold>()
            .ActivateOnEnter<BlackFang>()
            .ActivateOnEnter<RisingBeacon>()
            .ActivateOnEnter<RisingRing>()
            .ActivateOnEnter<BurningShade>()
            .ActivateOnEnter<ImmolatingShade>()
            .ActivateOnEnter<DoubleMeteor>()
            .ActivateOnEnter<VoidMeteor>()
            .ActivateOnEnter<DragonsDescent>()
            .ActivateOnEnter<Explosion1>()
            .ActivateOnEnter<Explosion2>()
            .ActivateOnEnter<Explosion3>()
            .ActivateOnEnter<CauterizeBait>()
            .ActivateOnEnter<Cauterize>()
            .ActivateOnEnter<GaleSphere>()
            .ActivateOnEnter<VoidStardust>()
            .ActivateOnEnter<EventideFall>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Kagekazu (ported from upstream and improved by LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 949, NameID = 12365)]
public sealed class T06Golbez(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(15f));
