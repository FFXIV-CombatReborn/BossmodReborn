namespace BossMod.Heavensward.Dungeon.D06AetherochemicalResearchFacility.D064AscianPrime;

public enum OID : uint
{
    Boss = 0x3DA7, // R3.8
    LahabreasShade = 0x3DAB, // R3.5
    IgeyorhmsShade = 0x3DAA, // R3.5
    FrozenStar = 0x3DA8, // R1.5
    BurningStar = 0x3DA9, // R1.5
    ArcaneSphere = 0x3DAC, // R7.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 871, // Boss->player, no cast, single-target

    AncientCircle = 31901, // Helper->self, no cast, range 10-20 donut, player targeted donut AOE, kind of a stack
    AncientDarkness = 31903, // Helper->self, no cast, range 6 circle

    AncientEruptionVisual = 31908, // Boss->self, 5.0s cast, single-target
    AncientEruption = 31909, // Helper->location, 5.0s cast, range 5 circle

    AncientFrost = 31904, // Helper->players, no cast, range 6 circle

    Annihilation = 31927, // Boss->location, no cast, single-target
    AnnihilationAOE = 33024, // Helper->self, 6.3s cast, range 40 circle
    AnnihilationEnrage = 31928, // Boss->location, no cast, single-target, if Arcane Sphere doesn't get destroyed in time
    AnnihilationEnrageAOE = 33025, // Helper->self, 6.3s cast, range 40 circle

    ArcaneRevelation1 = 31912, // Boss->location, no cast, single-target, teleport
    ArcaneRevelation2 = 31913, // Boss->self, 3.0s cast, single-target
    BurningChains = 31905, // Helper->player, no cast, single-target

    ChillingCrossVisual = 31922, // IgeyorhmsShade->self, 6.0s cast, single-target
    ChillingCross1 = 31923, // Helper->self, 6.0s cast, range 40 width 5 cross
    ChillingCross2 = 31924, // Helper->self, 6.0s cast, range 40 width 5 cross

    CircleOfIcePrimeVisual1 = 31898, // FrozenStar->self, no cast, single-target
    CircleOfIcePrimeVisual2 = 31899, // FrozenStar->self, no cast, single-target
    CircleOfIcePrime = 33021, // Helper->self, 2.0s cast, range 5-40 donut
    FireSpherePrimeVisual1 = 31896, // BurningStar->self, no cast, single-target
    FireSpherePrimeVisual2 = 31897, // BurningStar->self, no cast, single-target
    FireSpherePrime = 33022, // Helper->self, 2.0s cast, range 16 circle

    DarkBlizzardIIIVisual = 31914, // IgeyorhmsShade->self, 6.0s cast, single-target
    DarkBlizzardIII1 = 31915, // Helper->self, 6.0s cast, range 41 20-degree cone
    DarkBlizzardIII2 = 31916, // Helper->self, 6.0s cast, range 41 20-degree cone
    DarkBlizzardIII3 = 31917, // Helper->self, 6.0s cast, range 41 20-degree cone
    DarkBlizzardIII4 = 31918, // Helper->self, 6.0s cast, range 41 20-degree cone
    DarkBlizzardIII5 = 31919, // Helper->self, 6.0s cast, range 41 20-degree cone

    DarkFireIIVisual = 31920, // LahabreasShade->self, 6.0s cast, single-target
    DarkFireII = 31921, // Helper->players, 6.0s cast, range 6 circle

    Dualstar = 31894, // Boss->self, 4.0s cast, single-target

    EntropicFlameMarker = 31907, // Helper->player, no cast, single-target
    EntropicFlameVisual1 = 32126, // Boss->self, no cast, single-target
    EntropicFlameVisual2 = 31906, // Boss->self, 5.0s cast, single-target
    EntropicFlame = 32555, // Helper->self, no cast, range 50 width 8 rect, line stack

    FusionPrime = 31895, // Boss->self, 3.0s cast, single-target

    HeightOfChaos = 31911, // Boss->player, 5.0s cast, range 5 circle

    ShadowFlare1 = 31910, // Boss->self, 5.0s cast, range 40 circle
    ShadowFlare2 = 31925, // IgeyorhmsShade->self, 5.0s cast, range 40 circle
    ShadowFlare3 = 31926, // LahabreasShade->self, 5.0s cast, range 40 circle

    UniversalManipulationTeleport = 31419, // Boss->location, no cast, single-target
    UniversalManipulation = 31900, // Boss->self, 5.0s cast, range 40 circle
    UniversalManipulation2 = 33044 // Boss->player, no cast, single-target
}

public enum IconID : uint
{
    AncientCircle = 384, // player
    DarkWhispers = 139, // player
    AncientFrost = 161 // player
}

public enum TetherID : uint
{
    BurningChains = 9 // player->player
}

class AncientCircle(BossModule module) : Components.DonutStack(module, (uint)AID.AncientCircle, (uint)IconID.AncientCircle, 10f, 20f, 8d, 4, 4);

class DarkWhispers(BossModule module) : Components.UniformStackSpread(module, default, 6f)
{
    // regular spread component won't work because this is self targeted
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.DarkWhispers)
        {
            AddSpread(actor, WorldState.FutureTime(5d));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Spreads.Count != 0 && spell.Action.ID == (uint)AID.AncientDarkness)
        {
            Spreads.Clear();
        }
    }
}

class AncientFrost(BossModule module) : Components.StackWithIcon(module, (uint)IconID.AncientFrost, (uint)AID.AncientFrost, 6f, 5d, 4, 4);
class ShadowFlare(BossModule module) : Components.RaidwideCast(module, (uint)AID.ShadowFlare1);
class ShadowFlareLBPhase(BossModule module) : Components.RaidwideCast(module, (uint)AID.ShadowFlare2, "Raidwide x2");
class Annihilation(BossModule module) : Components.RaidwideCast(module, (uint)AID.AnnihilationAOE);
class UniversalManipulation(BossModule module) : Components.RaidwideCast(module, (uint)AID.UniversalManipulation, "Raidwide + Apply debuffs for later");

class HeightOfChaos(BossModule module) : Components.BaitAwayCast(module, (uint)AID.HeightOfChaos, 5f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

class AncientEruption(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AncientEruption, 5f);

class ChillingCross(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ChillingCross1, (uint)AID.ChillingCross2], new AOEShapeCross(40f, 2.5f));

class DarkBlizzardIII(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.DarkBlizzardIII1, (uint)AID.DarkBlizzardIII2, (uint)AID.DarkBlizzardIII3,
(uint)AID.DarkBlizzardIII4, (uint)AID.DarkBlizzardIII5], new AOEShapeCone(41f, 10f.Degrees()));

class DarkFireII(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.DarkFireII, 6f);
class BurningChains(BossModule module) : Components.Chains(module, (uint)TetherID.BurningChains, (uint)AID.BurningChains);
class EntropicFlame(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.EntropicFlameMarker, (uint)AID.EntropicFlame, 5.2d);

class Stars(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(5f, 40f);
    private static readonly AOEShapeCircle circle = new(16f);
    private readonly List<AOEInstance> _aoes = new(3);
    private static readonly WPos _frozenStarShortTether = new(230f, 86f), _frozenStarLongTether = new(230f, 92f);
    private static readonly WPos _donut = new(230f, 79f), _circle1 = new(241f, 79f), _circle2 = new(219f, 79f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var deadline = aoes[0].Activation.AddSeconds(1d);

        var index = 0;
        while (index < count)
        {
            ref var aoe = ref aoes[index];
            if (aoe.Activation >= deadline)
            {
                break;
            }
            ++index;
        }

        return aoes[..index];
    }

    public override void OnActorCreated(Actor actor)
    {
        void AddAOEs(bool reverse)
        {
            AddAOE(circle, _circle1, !reverse);
            AddAOE(circle, _circle2, !reverse);
            AddAOE(donut, _donut, reverse);
            if (reverse)
                _aoes.Reverse();
            void AddAOE(AOEShape shape, WPos pos, bool first) => _aoes.Add(new(shape, pos.Quantized(), default, WorldState.FutureTime(first ? 11.8d : 14.8d)));
        }

        if (actor.OID == (uint)OID.FrozenStar)
        {
            if (actor.Position == _frozenStarLongTether)
            {
                AddAOEs(false);
            }
            else if (actor.Position == _frozenStarShortTether)
            {
                AddAOEs(true);
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.CircleOfIcePrime or (uint)AID.FireSpherePrime)
        {
            _aoes.RemoveAt(0);
        }
    }
}

class D064AscianPrimeStates : StateMachineBuilder
{
    public D064AscianPrimeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AncientCircle>()
            .ActivateOnEnter<DarkWhispers>()
            .ActivateOnEnter<AncientFrost>()
            .ActivateOnEnter<ShadowFlare>()
            .ActivateOnEnter<ShadowFlareLBPhase>()
            .ActivateOnEnter<Annihilation>()
            .ActivateOnEnter<HeightOfChaos>()
            .ActivateOnEnter<DarkBlizzardIII>()
            .ActivateOnEnter<AncientEruption>()
            .ActivateOnEnter<ChillingCross>()
            .ActivateOnEnter<Stars>()
            .ActivateOnEnter<DarkFireII>()
            .ActivateOnEnter<UniversalManipulation>()
            .ActivateOnEnter<EntropicFlame>()
            .ActivateOnEnter<BurningChains>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 38, NameID = 3823, SortOrder = 11)]
public class D064AscianPrime(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public static readonly ArenaBoundsCustom arena = new([new Polygon(new(230f, 79f), 20.26f, 24)], [new Rectangle(new(230f, 98f), 5, 1.5f)], [new Rectangle(new(228.95f, 97f), 6.55f, 1.7f)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.ArcaneSphere));
    }
}
