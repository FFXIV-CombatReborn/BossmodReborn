namespace BossMod.Shadowbringers.Extreme.Ex1Titania;

public enum OID : uint
{
    Boss = 0x27E3,
    Helper = 0x233C,
    Mustardseed = 0x27E4,
    Peaseblossom = 0x27E5,
    Puck = 0x27E6,
    SpiritOfDew = 0x27E7,
    SpiritOfFlame = 0x27E8,
    MustardseedGiant = 0x27E9,
    PeaseblossomGiant = 0x27EA,
    PuckGiant = 0x27EB,
    SpiritOfWood = 0x27EC,
    RootGrowth = 0x1EABE4,
    WaterPuddles = 0x1EABE5,
    WaterPuddlesBurst = 0x1E9E3C
}

public enum AID : uint
{
    AutoAttack = 872,
    AutoAttackPeaseblossomGiant = 18058,
    AutoAttackPuckGiant = 18059,

    ThunderRune = 15657,
    ThunderRuneFinal = 16170,
    FrostRuneVisual = 15658,
    FrostRune = 15659,
    FaeLight = 15660,
    FaeLightRepeat = 15661,
    GrowthRune = 15662,
    WoodsEmbrace = 15663,
    MidsummerNightsDream = 15664,
    BeingMortalVisual = 15666,
    BeingMortal = 15667,
    Peasebomb = 15668,
    Pease = 15669,
    HardSwipe = 15670,
    Pummel = 15671,
    LeafstormVisual = 15672,
    Leafstorm = 15673,
    PucksCaprice = 15674,
    Wallop = 15675,
    LoveInIdleness = 15677,
    LeafstormGiantVisual = 15678,
    PeasebombGiant = 15679,
    WarAndPease = 15680,
    PucksBreath = 15681,
    PucksRebukeVisual = 15682,
    PucksRebukeCenter = 15683,
    PucksRebuke = 15684,
    MistRune = 15685,
    SpringShower = 15686,
    FlameRune = 15687,
    FlameHammer = 15688,
    LeafstormGiant = 15689,
    DivinationRune = 15690,
    BrightSabbath = 15691,
    PhantomRuneCircle = 15692,
    PhantomRuneDonut = 15693,
    GentleBreeze = 16258,
    WhisperingWind = 16610,
    Uplift = 16926
}

public enum IconID : uint
{
    FlameRune = 62,
    Pease = 139
}

public enum TetherID : uint
{
    ChainOfBrambles = 12,
    Vulnerability = 14,
    ThunderRune = 84
}

sealed class BrightSabbath(BossModule module) : Components.RaidwideCast(module, (uint)AID.BrightSabbath);
sealed class BeingMortal(BossModule module) : Components.RaidwideCast(module, (uint)AID.BeingMortal);
sealed class PucksCaprice(BossModule module) : Components.RaidwideCast(module, (uint)AID.PucksCaprice);
sealed class SpringShower(BossModule module) : Components.RaidwideCast(module, (uint)AID.SpringShower);
sealed class WhisperingWind(BossModule module) : Components.RaidwideCast(module, (uint)AID.WhisperingWind);
sealed class PhantomRuneCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PhantomRuneCircle, 10f);
sealed class PhantomRuneDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PhantomRuneDonut, new AOEShapeDonut(5f, 60f));
sealed class FrostRune(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FrostRune, 10f, 4);
sealed class Uplift(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Uplift, 6f);
sealed class DivinationRune(BossModule module) : Components.BaitAwayCast(module, (uint)AID.DivinationRune, new AOEShapeCone(60f, 37.5f.Degrees()), tankbuster: true);
sealed class FaeLight(BossModule module) : Components.CastHint(module, (uint)AID.FaeLight, "Shared tankbuster x3");
sealed class Tankbusters(BossModule module) : Components.SingleTargetCasts(module, [(uint)AID.Pummel, (uint)AID.HardSwipe]);
sealed class Leafstorm(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Leafstorm, (uint)AID.LeafstormGiant], new AOEShapeCone(50f, 10f.Degrees()));
sealed class GentleBreeze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GentleBreeze, new AOEShapeRect(60f, 2f));
sealed class Wallop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Wallop, new AOEShapeRect(20f, 2.5f));
sealed class Pease(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Pease, 6f);
sealed class WarAndPease(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.WarAndPease, 10f);
sealed class PucksBreath(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.PucksBreath, 6f);
sealed class PucksRebukeCenter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PucksRebukeCenter, 5f);
sealed class PucksRebuke(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.PucksRebuke, 10f);
sealed class FlameRune(BossModule module) : Components.StackWithIcon(module, (uint)IconID.FlameRune, (uint)AID.FlameHammer, 6f, 5.3d, 3, 4, 2);
sealed class ThunderRune(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCircle(6f), (uint)TetherID.ThunderRune, (uint)AID.ThunderRune, centerAtTarget: true);

sealed class ChainOfBrambles(BossModule module) : BossComponent(module)
{
    private const float BreakDistance = 20f;
    private readonly List<(Actor, Actor)> _chains = [];

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.ChainOfBrambles && WorldState.Actors.Find(tether.Target) is Actor target)
            _chains.Add((source, target));
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID != (uint)TetherID.ChainOfBrambles)
            return;
        for (var i = _chains.Count - 1; i >= 0; --i)
        {
            if (_chains[i].Item1 == source)
                _chains.RemoveAt(i);
        }
    }

    private Actor? Partner(Actor actor)
    {
        for (var i = 0; i < _chains.Count; ++i)
        {
            var (a, b) = _chains[i];
            if (a == actor)
                return b;
            if (b == actor)
                return a;
        }
        return null;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Partner(actor) is Actor p)
            hints.Add("Run away from your partner to break the chain!", (p.Position - actor.Position).LengthSq() < BreakDistance * BreakDistance);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Partner(actor) is Actor p)
            hints.AddForbiddenZone(new SDCircle(p.Position, BreakDistance));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Partner(pc) is Actor p)
            Arena.AddLine(pc.Position, p.Position, Colors.Danger);
    }
}

sealed class WoodsEmbrace(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID != (uint)OID.RootGrowth)
            return;
        if (state == 0x00010002u)
        {
            CurrentBaits.Add(new(actor, actor, new AOEShapeCross(10f, 3f)));
            CurrentBaits.Add(new(actor, actor, new AOEShapeCross(10f, 3f), customRotation: 45f.Degrees()));
        }
        else if (state == 0x00100020u)
        {
            CurrentBaits.Add(new(actor, actor, new AOEShapeCross(40f, 3f)));
            CurrentBaits.Add(new(actor, actor, new AOEShapeCross(40f, 3f), customRotation: 45f.Degrees()));
        }
    }

    public override void Update()
    {
        if (CurrentBaits.Count == 0)
            return;
        var roots = Module.Enemies((uint)OID.RootGrowth);
        for (var i = 0; i < roots.Count; ++i)
        {
            if (!roots[i].IsDead)
                return;
        }
        CurrentBaits.Clear();
    }
}

sealed class WaterTowers(BossModule module) : Components.GenericTowers(module)
{
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.WaterPuddles)
            Towers.Add(new(actor.Position, 5f, 1, 2));
        else if (actor.OID == (uint)OID.WaterPuddlesBurst)
            Towers.Clear();
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MistRune)
            Towers.Clear();
    }
}

sealed class Ex1TitaniaStates : StateMachineBuilder
{
    public Ex1TitaniaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BrightSabbath>()
            .ActivateOnEnter<BeingMortal>()
            .ActivateOnEnter<PucksCaprice>()
            .ActivateOnEnter<SpringShower>()
            .ActivateOnEnter<WhisperingWind>()
            .ActivateOnEnter<PhantomRuneCircle>()
            .ActivateOnEnter<PhantomRuneDonut>()
            .ActivateOnEnter<FrostRune>()
            .ActivateOnEnter<Uplift>()
            .ActivateOnEnter<DivinationRune>()
            .ActivateOnEnter<FaeLight>()
            .ActivateOnEnter<Tankbusters>()
            .ActivateOnEnter<Leafstorm>()
            .ActivateOnEnter<GentleBreeze>()
            .ActivateOnEnter<Wallop>()
            .ActivateOnEnter<Pease>()
            .ActivateOnEnter<WarAndPease>()
            .ActivateOnEnter<PucksBreath>()
            .ActivateOnEnter<PucksRebukeCenter>()
            .ActivateOnEnter<PucksRebuke>()
            .ActivateOnEnter<FlameRune>()
            .ActivateOnEnter<ThunderRune>()
            .ActivateOnEnter<ChainOfBrambles>()
            .ActivateOnEnter<WoodsEmbrace>()
            .ActivateOnEnter<WaterTowers>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 658, NameID = 8361, PlanLevel = 80)]
public sealed class Ex1Titania(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f))
{
    private static readonly uint[] _adds = [(uint)OID.Mustardseed, (uint)OID.Puck, (uint)OID.Peaseblossom, (uint)OID.MustardseedGiant, (uint)OID.PuckGiant,
        (uint)OID.PeaseblossomGiant, (uint)OID.SpiritOfDew];

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Mustardseed or (uint)OID.MustardseedGiant => 3,
                (uint)OID.Puck or (uint)OID.PuckGiant => 2,
                (uint)OID.Peaseblossom or (uint)OID.PeaseblossomGiant or (uint)OID.SpiritOfDew => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, _adds);
    }
}
