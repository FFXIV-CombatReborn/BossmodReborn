namespace BossMod.Shadowbringers.Extreme.Ex3Hades;

public enum OID : uint
{
    Boss = 0x2C1C,
    HadesGiant = 0x2C1D,
    HadesFinal = 0x2C1E,
    Helper = 0x233C,
    ShadowOfTheAncients = 0x2C23,
    NabrialesShade = 0x2C24,
    LahabreasShade = 0x2C25,
    IgeyorhmsShade = 0x2C26,
    AscianPrimesShade = 0x2C27,
    AetherialGaol = 0x2C28,
    CombinedShades = 0x2C57,
    CometMarker = 0x1EAE95
}

public enum AID : uint
{
    AncientDouble = 18341,
    RavenousAssault = 18342,
    RavenousAssaultRepeat = 18343,
    ShadowSpreadVisual = 18344,
    ShadowSpreadFirst = 18345,
    ShadowSpreadSecond = 18346,
    BadFaithVisual1 = 18347,
    BadFaithVisual2 = 18348,
    BadFaith1 = 18349,
    BadFaith2 = 18350,
    BadFaith3 = 18351,
    BadFaith4 = 18352,
    BrokenFaithVisual = 18353,
    BrokenFaith = 18354,
    ArcaneUtterance = 18355,
    ArcaneControl = 18356,
    MagicChakram = 18357,
    MagicSpear = 18358,
    AncientDarkIV = 18359,
    QuakeIII = 18360,
    CometFail = 18361,
    DarkII = 18362,
    AncientDarkness = 18363,
    AncientWaterIII = 18364,
    BlizzardSphere = 18365,
    FireSphere = 18366,
    Annihilation = 18367,
    FireIV = 18370,
    BlizzardIV = 18371,
    DarkFreezeVisual = 18372,
    DarkFreeze = 18373,
    DarkFlameVisual = 18374,
    DarkFlame = 18375,
    UniversalManipulation = 18376,
    ForkedLightning = 18377,
    AncientCircle = 18378,
    DeathShriek = 18379,
    Blight = 18380,
    MegiddoFlameVisual = 18381,
    MegiddoFlame = 18383,
    ShadowFlare = 18384,
    HeightOfChaos = 18385,
    AncientEruptionVisual = 18386,
    AncientEruption = 18387,
    ShadowFlareEnrage = 18388,
    AgainTheMajestic = 18389,
    CaptivityVisual = 18390,
    Captivity = 18391,
    ChorusOfTheLost = 18395,
    CometFailGiant = 18397,
    AgainTheMartyr = 18398,
    WailOfTheLost = 18401,
    AgainTheAbyssalCelebrant = 18402,
    Bident = 18403,
    BidentShadowSpread = 18404,
    BidentShadowSpreadRepeat = 18405,
    NetherBlast = 18406,
    BidentMegiddoFlame = 18408,
    DarkSeal = 18409,
    ShadowStream = 18410,
    PolydegmonsPurgationVisual = 18411,
    PolydegmonsPurgation1 = 18412,
    PolydegmonsPurgation2 = 18413,
    Titanomachy = 18415,
    DarkCurrentVisual = 18416,
    DarkCurrentFirst = 18417,
    DarkCurrentRest = 18418,
    Gigantomachy = 18419,
    QuadrastrikeVisual = 18420,
    QuadrastrikeRaidwide1 = 18421,
    QuadrastrikeTower = 18422,
    GigantomachyEnrage = 18425,
    ShadowFlareRaidwide = 18428,
    ShadowFlareRaidwideEnrage = 18429,
    LifeInCaptivity = 18760,
    BlackCauldron = 18762,
    Comet = 18769,
    CometGiant = 18770
}

public enum SID : uint
{
    AncientDouble = 2201,
    BurningBrand = 2128,
    FreezingBrand = 2129,
    AncientCircle = 2110,
    ForkedLightning = 587,
    CursedShriek = 452,
    BeyondDeath = 1382,
    Doom = 1769
}

public enum IconID : uint
{
    Captivity = 120
}

public enum TetherID : uint
{
    DarkII = 17,
    DarkFlame = 96,
    DarkFreeze = 97
}

sealed class Raidwides(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.Annihilation, (uint)AID.BlizzardSphere, (uint)AID.FireSphere, (uint)AID.UniversalManipulation,
    (uint)AID.Blight, (uint)AID.ShadowFlare, (uint)AID.ShadowFlareRaidwide, (uint)AID.Titanomachy, (uint)AID.LifeInCaptivity, (uint)AID.ChorusOfTheLost, (uint)AID.Gigantomachy]);
sealed class Enrages(BossModule module) : Components.CastHints(module, [(uint)AID.AncientDarkIV, (uint)AID.ShadowFlareEnrage, (uint)AID.ShadowFlareRaidwideEnrage, (uint)AID.GigantomachyEnrage], "Enrage!", true);
sealed class QuakeIII(BossModule module) : Components.CastHint(module, (uint)AID.QuakeIII, "DPS check: push Nabriales below 50%!", true);
sealed class BadFaith(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BadFaith1, (uint)AID.BadFaith2, (uint)AID.BadFaith3, (uint)AID.BadFaith4], new AOEShapeRect(20f, 10f));
sealed class ShadowSpreadSecond(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ShadowSpreadSecond, new AOEShapeCone(40f, 15f.Degrees()));
sealed class BidentShadowSpread(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BidentShadowSpread, new AOEShapeCone(40f, 15f.Degrees()));
sealed class RavenousAssault(BossModule module) : Components.BaitAwayCast(module, (uint)AID.RavenousAssault, new AOEShapeCircle(5f), true, true, true);
sealed class HeightOfChaos(BossModule module) : Components.BaitAwayCast(module, (uint)AID.HeightOfChaos, new AOEShapeCircle(5f), true, true, true);
sealed class DarkII(BossModule module) : Components.BaitAwayCast(module, (uint)AID.DarkII, new AOEShapeCone(51.25f, 15f.Degrees()));
sealed class AncientDarkness(BossModule module) : Components.BaitAwayCast(module, (uint)AID.AncientDarkness, new AOEShapeCircle(5f), true);
sealed class AncientWaterIII(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.AncientWaterIII, 6f, 2, 4);
sealed class Tankbusters(BossModule module) : Components.SingleTargetCasts(module, [(uint)AID.FireIV, (uint)AID.BlizzardIV]);
sealed class DarkFlame(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCircle(6f), (uint)TetherID.DarkFlame, (uint)AID.DarkFlame, centerAtTarget: true);
sealed class DarkFreeze(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCircle(6f), (uint)TetherID.DarkFreeze, (uint)AID.DarkFreeze, centerAtTarget: true);
sealed class ForkedLightning(BossModule module) : Components.StatusStackSpread(module, default, (uint)SID.ForkedLightning, default, 8f);
sealed class MegiddoFlame(BossModule module) : Components.CastHint(module, (uint)AID.MegiddoFlameVisual, "Line stacks on healers");
sealed class AncientEruption(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AncientEruption, 5f);
sealed class Captivity(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(8f), (uint)IconID.Captivity, (uint)AID.Captivity, 5.1d, true);
sealed class WailOfTheLost(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.WailOfTheLost, 15f);
sealed class StreamAndPurgation(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ShadowStream, (uint)AID.PolydegmonsPurgation1, (uint)AID.PolydegmonsPurgation2], new AOEShapeRect(100f, 8f));
sealed class QuadrastrikeTowers(BossModule module) : Components.CastTowers(module, (uint)AID.QuadrastrikeTower, 5f);
sealed class Quadrastrike(BossModule module) : Components.CastHint(module, (uint)AID.QuadrastrikeVisual, "Raidwide, tank towers, then raidwide + bleed");

sealed class ShadowSpreadFirst(BossModule module) : Components.GenericBaitAway(module, (uint)AID.ShadowSpreadFirst)
{
    private static readonly AOEShapeCone _cone = new(40f, 15f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.ShadowSpreadVisual)
            return;
        var party = Raid.WithoutSlot();
        var activation = Module.CastFinishAt(spell, 1d);
        for (var i = 0; i < party.Length; ++i)
            CurrentBaits.Add(new(caster, party[i], _cone, activation));
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

sealed class CometTowers(BossModule module) : Components.GenericTowers(module)
{
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.CometMarker)
            Towers.Add(new(actor.Position, 4f, 1, 1, activation: WorldState.FutureTime(5.7d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is not ((uint)AID.Comet or (uint)AID.CometGiant or (uint)AID.CometFail or (uint)AID.CometFailGiant))
            return;
        ++NumCasts;
        var pos = spell.TargetXZ;
        for (var i = Towers.Count - 1; i >= 0; --i)
        {
            if (Towers[i].Position.AlmostEqual(pos, 1f))
                Towers.RemoveAt(i);
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID != (uint)OID.CometMarker)
            return;
        for (var i = Towers.Count - 1; i >= 0; --i)
        {
            if (Towers[i].Position.AlmostEqual(actor.Position, 1f))
                Towers.RemoveAt(i);
        }
    }
}

sealed class Brands(BossModule module) : Components.GenericInvincible(module)
{
    private readonly List<Actor> _forbidden = [];

    protected override ReadOnlySpan<Actor> ForbiddenTargets(int slot, Actor actor)
    {
        _forbidden.Clear();
        if (actor.FindStatus((uint)SID.BurningBrand) != null)
            _forbidden.AddRange(Module.Enemies((uint)OID.LahabreasShade));
        if (actor.FindStatus((uint)SID.FreezingBrand) != null)
            _forbidden.AddRange(Module.Enemies((uint)OID.IgeyorhmsShade));
        return CollectionsMarshal.AsSpan(_forbidden);
    }
}

sealed class AncientCircle(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut _donut = new(10f, 20f);
    private readonly List<(Actor Owner, DateTime Expire)> _circles = [];
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoes.Clear();
        if (actor.FindStatus((uint)SID.BeyondDeath) != null)
            return [];
        for (var i = 0; i < _circles.Count; ++i)
        {
            var c = _circles[i];
            if (c.Owner != actor)
                _aoes.Add(new(_donut, c.Owner.Position, default, c.Expire));
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.AncientCircle)
            _circles.Add((actor, status.ExpireAt));
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID != (uint)SID.AncientCircle)
            return;
        for (var i = _circles.Count - 1; i >= 0; --i)
        {
            if (_circles[i].Owner == actor)
                _circles.RemoveAt(i);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_circles.Count != 0 && actor.FindStatus((uint)SID.BeyondDeath) != null)
            hints.Add("Get hit by one Ancient Circle!");
        else
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_circles.Count != 0 && actor.FindStatus((uint)SID.BeyondDeath) != null)
        {
            (Actor Owner, DateTime Expire)? best = null;
            var bestDist = float.MaxValue;
            for (var i = 0; i < _circles.Count; ++i)
            {
                var d = (_circles[i].Owner.Position - actor.Position).LengthSq();
                if (d < bestDist)
                {
                    bestDist = d;
                    best = _circles[i];
                }
            }
            if (best is var (owner, _))
                hints.GoalZones.Add(AIHints.GoalDonut(owner.Position, 11f, 19f, 5f));
        }
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

sealed class CursedShriek(BossModule module) : Components.GenericGaze(module)
{
    private readonly List<(Actor Owner, DateTime Expire)> _shrieks = [];
    private readonly List<Eye> _eyes = [];

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        _eyes.Clear();
        for (var i = 0; i < _shrieks.Count; ++i)
        {
            var s = _shrieks[i];
            if (s.Owner != actor)
                _eyes.Add(new(s.Owner.Position, s.Expire));
        }
        return CollectionsMarshal.AsSpan(_eyes);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.CursedShriek)
            _shrieks.Add((actor, status.ExpireAt));
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID != (uint)SID.CursedShriek)
            return;
        for (var i = _shrieks.Count - 1; i >= 0; --i)
        {
            if (_shrieks[i].Owner == actor)
                _shrieks.RemoveAt(i);
        }
    }
}

sealed class DarkCurrent(BossModule module) : Components.Exaflare(module, 6f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DarkCurrentFirst)
            Lines.Add(new(spell.LocXZ, 10f * spell.Rotation.ToDirection(), Module.CastFinishAt(spell), 1.46d, 5, 2));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is not ((uint)AID.DarkCurrentFirst or (uint)AID.DarkCurrentRest))
            return;
        var pos = caster.Position;
        for (var i = 0; i < Lines.Count; ++i)
        {
            var line = Lines[i];
            if (line.Next.AlmostEqual(pos, 1f))
            {
                AdvanceLine(line, pos);
                if (line.ExplosionsLeft == 0)
                    Lines.RemoveAt(i);
                return;
            }
        }
    }
}

sealed class Ex3HadesStates : StateMachineBuilder
{
    public Ex3HadesStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Raidwides>()
            .ActivateOnEnter<Enrages>()
            .ActivateOnEnter<QuakeIII>()
            .ActivateOnEnter<BadFaith>()
            .ActivateOnEnter<ShadowSpreadFirst>()
            .ActivateOnEnter<ShadowSpreadSecond>()
            .ActivateOnEnter<BidentShadowSpread>()
            .ActivateOnEnter<RavenousAssault>()
            .ActivateOnEnter<HeightOfChaos>()
            .ActivateOnEnter<DarkII>()
            .ActivateOnEnter<AncientDarkness>()
            .ActivateOnEnter<AncientWaterIII>()
            .ActivateOnEnter<Tankbusters>()
            .ActivateOnEnter<DarkFlame>()
            .ActivateOnEnter<DarkFreeze>()
            .ActivateOnEnter<ForkedLightning>()
            .ActivateOnEnter<MegiddoFlame>()
            .ActivateOnEnter<AncientEruption>()
            .ActivateOnEnter<Captivity>()
            .ActivateOnEnter<WailOfTheLost>()
            .ActivateOnEnter<StreamAndPurgation>()
            .ActivateOnEnter<QuadrastrikeTowers>()
            .ActivateOnEnter<Quadrastrike>()
            .ActivateOnEnter<CometTowers>()
            .ActivateOnEnter<Brands>()
            .ActivateOnEnter<AncientCircle>()
            .ActivateOnEnter<CursedShriek>()
            .ActivateOnEnter<DarkCurrent>()
            .Raw.Update = () =>
            {
                var finals = module.Enemies((uint)OID.HadesFinal);
                for (var i = 0; i < finals.Count; ++i)
                {
                    if (finals[i].IsDead)
                        return true;
                }
                return false;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 693, NameID = 8352, PlanLevel = 80)]
public sealed class Ex3Hades(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f))
{
    private static readonly uint[] _enemies = [(uint)OID.HadesGiant, (uint)OID.HadesFinal, (uint)OID.NabrialesShade, (uint)OID.LahabreasShade, (uint)OID.IgeyorhmsShade,
        (uint)OID.AscianPrimesShade, (uint)OID.AetherialGaol];

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            if (e.Actor.OID == (uint)OID.AetherialGaol)
                e.Priority = 2;
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, _enemies);
    }
}
