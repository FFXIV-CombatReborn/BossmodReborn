namespace BossMod.Endwalker.Trial.T03Endsinger;

public enum OID : uint
{
    Boss = 0x34F3, // The Endsinger
    Endsinger2 = 0x34F5, // Epigonoi heads, source of Misery
    Endsinger3 = 0x34F6, // The Endsinger
    Kakodaimon = 0x34F7,
    DoomedStars = 0x34F8,
    FeatherOfDespair = 0x34FA,
    Helper = 0x233C
}

public enum AID : uint
{
    Elegeia = 26156, // Boss->self, 5.0s cast, raidwide
    ElegeiaAOE = 26618, // Helper->self, no cast, range 40 circle
    ElegeiaUnforgotten = 26242, // Boss->self, 5.0s cast, raidwide
    StellarCollision = 26158, // DoomedStars->self, 7.0s cast, range 30 circle
    StellarCollisionRect = 26164, // DoomedStars->self, 3.0s cast, range 80 width 22 rect
    FatalismStellarCollision = 26171, // ?->self, 2.0s cast, range 30 circle
    Fatalism1 = 26162, // Boss->self, 3.0s cast, single-target
    Galaxias = 27753, // Boss->self, 5.0s cast, single-target
    GalaxiasAOE = 27754, // Helper->location, 5.0s cast, range 40 circle, knockback from center
    ElenchosCenter = 26180, // Boss->self, 6.0s cast, range 40 width 14 rect
    ElenchosSides = 26178, // Boss->self, 6.0s cast, single-target
    ElenchosSidesAOE = 26179, // Helper->self, 6.0s cast, range 40 width 13 rect
    DeathsEmbrace = 26185, // Boss->self, 6.0s cast, single-target
    DeathsEmbraceAOE = 26187, // Boss->self, no cast, range 45 cone
    Misery = 26184, // Helper->self, no cast, circle growing every ~1.07s for 14 pulses, last ~12
    Pharmakon = 26188, // FeatherOfDespair->self, 2.0s cast, range 6 circle
    Aporrhoia = 26173, // Boss->self, 3.0s cast, single-target
    AporrhoiaLaser = 26174, // ?->self, 5.0s cast, range 40 width 5 rect
    FatalismLaser = 26175, // ?->self, 4.0s cast, range 40 width 5 rect
    Hubris = 26194, // Boss->self, 5.0s cast, single-target
    HubrisAOE = 26195, // Helper->player, 5.0s cast, range 6 circle
    Ekstasis = 26181, // Boss->self, 3.0s cast, single-target
    Epigonoi = 26182, // ?->location, 3.0s cast, range 6 circle
    Interstellar = 26189, // Boss->self, 3.0s cast, single-target
    InterstellarAOE = 26619, // Helper->self, 5.0s cast, range 40 width 28 rect
    Nemesis1 = 26190, // Helper->player, 5.0s cast, range 6 circle
    Nemesis2 = 26196, // Boss->self, 5.0s cast, single-target
    Nemesis2AOE = 26197, // Helper->player, 5.0s cast, range 6 circle
    Planetes = 27480, // Boss->self, 4.5s cast, single-target
    MeteorRadiant = 26198, // Kakodaimon->self, 3.0s cast, single-target
    Crash = 26199, // ?->location, 9.0s cast, range 15 circle
    MeteorOutburst = 26204, // Kakodaimon->self, 5.0s cast, single-target
    DeadStarSpread = 26205, // ?->player, 5.0s cast, range 6 circle
    Katastrophe = 26203, // Kakodaimon->self, 5.0s cast, range 40 circle
    UltimateFate = 27481, // Boss->self, 5.0s cast, range 40 circle
    Fatalism2 = 27483, // Boss->self, 3.0s cast, single-target
    Telos = 26206, // Boss->self, 5.0s cast, range 40 circle
    Telomania = 26207, // Boss->self, 3.0s cast, single-target
    TelomaniaSmall = 26208, // ?->self, no cast, range 40 circle
    TelomaniaBig = 26209, // ?->self, no cast, range 40 circle
    DeadStar = 24142 // ?->location, 3.0s cast, range 6 circle
}

public enum TetherID : uint
{
    Fatalism = 166 // Boss->DoomedStars
}

sealed class Raidwides(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.Elegeia, (uint)AID.ElegeiaUnforgotten, (uint)AID.Katastrophe, (uint)AID.Telos]);
sealed class UltimateFate(BossModule module) : Components.RaidwideCast(module, (uint)AID.UltimateFate, "Raidwide, tank LB3!");
sealed class Telomania(BossModule module) : Components.RaidwideCast(module, (uint)AID.Telomania, "Raidwide x5");
sealed class StellarCollision(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.StellarCollision, (uint)AID.FatalismStellarCollision], 30f);
sealed class StellarCollisionRect(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StellarCollisionRect, new AOEShapeRect(80f, 11f));

sealed class FatalismStars(BossModule module) : Components.GenericAOEs(module, (uint)AID.FatalismStellarCollision)
{
    private static readonly AOEShapeCircle _shape = new(30f);
    private readonly Dictionary<ulong, WPos> _recorded = [];
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.StellarCollision)
            _recorded[caster.InstanceID] = caster.Position;
        else if (spell.Action.ID == WatchedAction && _aoes.Count != 0)
            _aoes.RemoveAt(0);
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Fatalism && WorldState.Actors.Find(tether.Target) is Actor star)
            _aoes.Add(new(_shape, _recorded.GetValueOrDefault(star.InstanceID, star.Position), default, WorldState.FutureTime(19.6d)));
    }
}

sealed class Galaxias(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.GalaxiasAOE, 13f);
sealed class ElenchosCenter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElenchosCenter, new AOEShapeRect(40f, 7f));
sealed class ElenchosSides(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElenchosSidesAOE, new AOEShapeRect(40f, 6.5f, 40f));

sealed class DeathsEmbrace(BossModule module) : Components.GenericBaitAway(module, (uint)AID.DeathsEmbraceAOE)
{
    private static readonly AOEShapeCone _shape = new(45f, 5f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DeathsEmbrace)
        {
            var activation = Module.CastFinishAt(spell, 1.2d);
            var party = Raid.WithoutSlot();
            for (var i = 0; i < party.Length; ++i)
                CurrentBaits.Add(new(caster, party[i], _shape, activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}

sealed class Pharmakon(BossModule module) : Components.GenericAOEs(module, (uint)AID.Pharmakon)
{
    private static readonly AOEShapeCircle _shape = new(6f);
    private readonly List<(Actor Feather, DateTime Activation)> _feathers = [];
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoes.Clear();
        for (var i = 0; i < _feathers.Count; ++i)
        {
            var (feather, activation) = _feathers[i];
            _aoes.Add(new(_shape, feather.Position, default, activation));
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.FeatherOfDespair)
            _feathers.Add((actor, WorldState.FutureTime(6.7d)));
    }

    public override void OnActorDestroyed(Actor actor) => Remove(actor);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            Remove(caster);
        }
    }

    private void Remove(Actor feather)
    {
        for (var i = _feathers.Count - 1; i >= 0; --i)
        {
            if (_feathers[i].Feather == feather)
                _feathers.RemoveAt(i);
        }
    }
}

sealed class Misery(BossModule module) : Components.GenericAOEs(module, (uint)AID.Misery)
{
    private static readonly AOEShapeCircle _shape = new(12.5f);
    private readonly List<AOEInstance> _aoes = [];
    private DateTime _activation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoes.Clear();
        var heads = Module.Enemies((uint)OID.Endsinger2);
        for (var i = 0; i < heads.Count; ++i)
            _aoes.Add(new(_shape, heads[i].Position, default, _activation));
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Endsinger2 && _activation < WorldState.CurrentTime)
            _activation = WorldState.FutureTime(6.3d);
    }
}

sealed class Lasers(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.AporrhoiaLaser, (uint)AID.FatalismLaser], new AOEShapeRect(40f, 2.5f));
sealed class Hubris(BossModule module) : Components.BaitAwayCast(module, (uint)AID.HubrisAOE, new AOEShapeCircle(6f), true);
sealed class Epigonoi(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Epigonoi, 6f);
sealed class Interstellar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.InterstellarAOE, new AOEShapeRect(40f, 14f));
sealed class Nemesis1(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Nemesis1, 6f);
sealed class Nemesis2(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Nemesis2AOE, 6f);
sealed class Crash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Crash, 15f);
sealed class DeadStarSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.DeadStarSpread, 6f);
sealed class DeadStar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeadStar, 6f);

sealed class T03EndsingerStates : StateMachineBuilder
{
    public T03EndsingerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Raidwides>()
            .ActivateOnEnter<UltimateFate>()
            .ActivateOnEnter<Telomania>()
            .ActivateOnEnter<StellarCollision>()
            .ActivateOnEnter<StellarCollisionRect>()
            .ActivateOnEnter<FatalismStars>()
            .ActivateOnEnter<Galaxias>()
            .ActivateOnEnter<ElenchosCenter>()
            .ActivateOnEnter<ElenchosSides>()
            .ActivateOnEnter<DeathsEmbrace>()
            .ActivateOnEnter<Pharmakon>()
            .ActivateOnEnter<Misery>()
            .ActivateOnEnter<Lasers>()
            .ActivateOnEnter<Hubris>()
            .ActivateOnEnter<Epigonoi>()
            .ActivateOnEnter<Interstellar>()
            .ActivateOnEnter<Nemesis1>()
            .ActivateOnEnter<Nemesis2>()
            .ActivateOnEnter<Crash>()
            .ActivateOnEnter<DeadStarSpread>()
            .ActivateOnEnter<DeadStar>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Kagekazu (ported from upstream and improved by LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 796, NameID = 10448)]
public sealed class T03Endsinger(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Endsinger2));
        Arena.Actors(Enemies((uint)OID.Endsinger3));
        Arena.Actors(Enemies((uint)OID.Kakodaimon));
    }
}
