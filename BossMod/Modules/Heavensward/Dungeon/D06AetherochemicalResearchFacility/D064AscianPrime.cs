﻿namespace BossMod.Heavensward.Dungeon.D06AetherochemicalResearchFacility.D064AscianPrime;

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

    EntropicFlame = 31907, // Helper->player, no cast, single-target
    EntropicFlame1 = 32126, // Boss->self, no cast, single-target
    EntropicFlame2 = 31906, // Boss->self, 5.0s cast, single-target
    EntropicFlame3 = 32555, // Helper->self, no cast, range 50 width 8 rect, line stack

    FusionPrime = 31895, // Boss->self, 3.0s cast, single-target

    HeightOfChaos = 31911, // Boss->player, 5.0s cast, range 5 circle

    ShadowFlare1 = 31910, // Boss->self, 5.0s cast, range 40 circle
    ShadowFlare2 = 31925, // IgeyorhmsShade->self, 5.0s cast, range 40 circle
    ShadowFlare3 = 31926, // LahabreasShade->self, 5.0s cast, range 40 circle

    UniversalManipulationTeleport = 31419, // Boss->location, no cast, single-target
    UniversalManipulation = 31900, // Boss->self, 5.0s cast, range 40 circle
    UniversalManipulation2 = 33044 // Boss->player, no cast, single-target
}

public enum SID : uint
{
    AncientCircle = 3534, // none->player, extra=0x0 Player targeted donut AOE
    AncientFrost = 3506, // none->player, extra=0x0 Stack marker
    Bleeding = 2088, // Boss->player, extra=0x0
    BurningChains1 = 3505, // none->player, extra=0x0
    BurningChains2 = 769, // none->player, extra=0x0
    DarkWhispers = 3535, // none->player, extra=0x0 Spread marker
    Untargetable = 2056 // Boss->Boss, extra=0x231, before limitbreak phase
}

public enum IconID : uint
{
    Tankbuster = 343, // player
    AncientCircle = 384, // player
    DarkWhispers = 139, // player
    AncientFrost = 161, // player
    BurningChains = 97, // player
    DarkFire = 311 // player
}

public enum TetherID : uint
{
    StarTether = 110, // FrozenStar/BurningStar->FrozenStar/BurningStar
    BurningChains = 9, // player->player
    ArcaneSphere = 197 // ArcaneSphere->Boss
}

class AncientCircle(BossModule module) : Components.DonutStack(module, ActionID.MakeSpell(AID.AncientCircle), (uint)IconID.AncientCircle, 10, 20, 8, 4, 4);

class DarkWhispers(BossModule module) : Components.UniformStackSpread(module, 0, 6, alwaysShowSpreads: true)
{
    // regular spread component won't work because this is self targeted
    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.DarkWhispers)
            AddSpread(actor, WorldState.FutureTime(5));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.AncientDarkness)
            Spreads.Clear();
    }
}

class AncientFrost(BossModule module) : Components.StackWithIcon(module, (uint)IconID.AncientFrost, ActionID.MakeSpell(AID.AncientFrost), 6, 5, 4, 4);
class ShadowFlare(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ShadowFlare1));
class ShadowFlareLBPhase(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ShadowFlare2), "Raidwide x2");
class Annihilation(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AnnihilationAOE));
class UniversalManipulation(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.UniversalManipulation), "Raidwide + Apply debuffs for later");

class HeightOfChaos(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.HeightOfChaos), new AOEShapeCircle(5), true)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Tankbuster cleave");
    }
}

class AncientEruption(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.AncientEruption), 5);

class ChillingCross(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCross(40, 2.5f));
class ChillingCross1(BossModule module) : ChillingCross(module, AID.ChillingCross1);
class ChillingCross2(BossModule module) : ChillingCross(module, AID.ChillingCross2);

class DarkBlizzard(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(41, 10.Degrees()));
class DarkBlizzardIIIAOE1(BossModule module) : DarkBlizzard(module, AID.DarkBlizzardIII1);
class DarkBlizzardIIIAOE2(BossModule module) : DarkBlizzard(module, AID.DarkBlizzardIII2);
class DarkBlizzardIIIAOE3(BossModule module) : DarkBlizzard(module, AID.DarkBlizzardIII3);
class DarkBlizzardIIIAOE4(BossModule module) : DarkBlizzard(module, AID.DarkBlizzardIII4);
class DarkBlizzardIIIAOE5(BossModule module) : DarkBlizzard(module, AID.DarkBlizzardIII5);

class DarkFireII(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.DarkFireII), 6);
class BurningChains(BossModule module) : Components.Chains(module, (uint)TetherID.BurningChains, ActionID.MakeSpell(AID.BurningChains));
class EntropicFlame(BossModule module) : Components.LineStack(module, ActionID.MakeSpell(AID.EntropicFlame), ActionID.MakeSpell(AID.EntropicFlame3), 5.2f);

class Stars(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(5, 40);
    private static readonly AOEShapeCircle circle = new(16);

    private readonly List<AOEInstance> _aoesLongTether = [];
    private readonly List<AOEInstance> _aoesShortTether = [];
    private static readonly WPos _frozenStarShortTether = new(230, 86);
    private static readonly WPos _frozenStarLongTether = new(230, 92);
    private static readonly WPos _donut = new(230, 79);
    private static readonly WPos _circle1 = new(241, 79);
    private static readonly WPos _circle2 = new(219, 79);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = _aoesShortTether.Count > 0 ? _aoesShortTether : _aoesLongTether;
        foreach (var aoe in aoes)
            yield return new AOEInstance(aoe.Shape, aoe.Origin, default, aoe.Activation);
    }

    public override void OnActorCreated(Actor actor)
    {
        var activation1 = WorldState.FutureTime(11.8f);
        var activation2 = WorldState.FutureTime(14.8f);
        if ((OID)actor.OID == OID.FrozenStar)
        {
            if (actor.Position == _frozenStarLongTether)
            {
                _aoesShortTether.Add(new(circle, _circle1, default, activation1));
                _aoesShortTether.Add(new(circle, _circle2, default, activation1));
                _aoesLongTether.Add(new(donut, _donut, default, activation2));
            }
            else if (actor.Position == _frozenStarShortTether)
            {
                _aoesShortTether.Add(new(donut, _donut, default, activation1));
                _aoesLongTether.Add(new(circle, _circle1, default, activation2));
                _aoesLongTether.Add(new(circle, _circle2, default, activation2));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CircleOfIcePrime or AID.FireSpherePrime)
        {
            NumCasts++;
            if (_aoesShortTether.Count != 0)
                _aoesShortTether.RemoveAt(0);
            else
                _aoesLongTether.Clear();
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
            .ActivateOnEnter<DarkBlizzardIIIAOE1>()
            .ActivateOnEnter<DarkBlizzardIIIAOE2>()
            .ActivateOnEnter<DarkBlizzardIIIAOE3>()
            .ActivateOnEnter<DarkBlizzardIIIAOE4>()
            .ActivateOnEnter<DarkBlizzardIIIAOE5>()
            .ActivateOnEnter<AncientEruption>()
            .ActivateOnEnter<ChillingCross1>()
            .ActivateOnEnter<ChillingCross2>()
            .ActivateOnEnter<Stars>()
            .ActivateOnEnter<DarkFireII>()
            .ActivateOnEnter<UniversalManipulation>()
            .ActivateOnEnter<EntropicFlame>()
            .ActivateOnEnter<BurningChains>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 38, NameID = 3823)]
public class D064AscianPrime(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(230.1f, 58.66f), new(234.87f, 59.29f), new(235.46f, 59.42f), new(240.04f, 61.32f), new(240.54f, 61.66f),
    new(244.09f, 64.38f), new(244.52f, 64.77f), new(247.63f, 68.83f), new(249.53f, 73.43f), new(249.69f, 74),
    new(250.3f, 78.68f), new(250.33f, 79.19f), new(249.72f, 83.76f), new(249.62f, 84.35f), new(247.73f, 88.91f),
    new(247.47f, 89.37f), new(244.53f, 93.2f), new(244.15f, 93.57f), new(240.69f, 96.23f), new(240.26f, 96.52f),
    new(239.77f, 96.74f), new(235.7f, 98.28f), new(235.4f, 98.77f), new(222.57f, 98.77f), new(222.38f, 98.2f),
    new(222.04f, 97.81f), new(219.9f, 96.66f), new(215.6f, 93.36f), new(212.6f, 89.45f), new(212.28f, 88.91f),
    new(210.39f, 84.36f), new(210.29f, 83.78f), new(209.66f, 79.02f), new(210.34f, 73.83f), new(210.54f, 73.27f),
    new(212.22f, 69.21f), new(212.47f, 68.7f), new(215.59f, 64.64f), new(219.87f, 61.36f), new(224.41f, 59.48f),
    new(224.99f, 59.31f), new(229.86f, 58.67f)];
    public static readonly ArenaBounds arena = new ArenaBoundsComplex([new PolygonCustom(vertices)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.ArcaneSphere));
    }
}