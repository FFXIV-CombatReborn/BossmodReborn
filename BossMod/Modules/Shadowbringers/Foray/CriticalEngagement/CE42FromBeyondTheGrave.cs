namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE42FromBeyondTheGrave;

public enum OID : uint
{
    Boss = 0x2E35, // R8.25
    Deathwall = 0x1EB173, // R0.5
    DeathwallHelper = 0x2EE8, // R0.50
    ShockSphere = 0x3232, // R1.0
    WarWraith = 0x3233, // R1.8
    HernaisTheTenacious = 0x3234, // R0.5
    DyunbuTheAccursed = 0x3235, // R0.5
    LlofiiTheForthright = 0x3236, // R0.5
    Monoceros = 0x3237, // R1.8
    LivingCorpseSpawn = 0x1EB07A, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttackBoss = 24692, // Boss->player, no cast, single-target
    AutoAttackHernais = 6497, // HernaisTheTenacious->player, no cast, single-target
    AutoAttackWraith = 6498, // WarWraith->player, no cast, single-target

    DevourSoul = 24093, // Boss->player, 5.0s cast, single-target, tankbuster
    Blight = 24094, // Boss->self, 5.0s cast, single-target, visual
    BlightAOE = 24095, // Helper->self, 5.0s cast, ???, raidwide
    GallowsMarch = 24096, // Boss->self, 3.0s cast, single-target, visual (applies doom and forced march)
    LivingCorpse = 24097, // Boss->self, 3.0s cast, single-target, visual
    ChainMagick = 24098, // Boss->self, 3.0s cast, single-target, applies dualcast for next soul purge
    SoulPurgeCircle = 24099, // Boss->self, 5.0s cast, range 10 circle
    SoulPurgeCircleDual = 24100, // Boss->self, no cast, range 10 circle
    SoulPurgeDonut = 24101, // Boss->self, 5.0s cast, range 10-30 donut
    SoulPurgeDonutDual = 24102, // Boss->self, no cast, range 10-30 donut
    CrimsonBlade = 24103, // HernaisTheTenacious->self, 8.0s cast, range 50 180-degree cone aoe
    BloodCyclone = 24104, // HernaisTheTenacious->self, 3.0s cast, range 5 circle
    Aethertide = 24105, // DyunbuTheAccursed->self, 8.0s cast, single-target, visual
    AethertideAOE = 24106, // Helper->players, 8.0s cast, range 8 circle spread
    MarchingBreath = 24107, // DyunbuTheAccursed->self, 8.0s cast, interruptible, heals all allies by 20% of max health (raidwide)
    TacticalStone = 24108, // DyunbuTheAccursed->player, 2.5s cast, single-target, autoattack
    TacticalAero = 24109, // DyunbuTheAccursed->self, 3.0s cast, range 40 width 8 rect
    Enrage = 24110, // DyunbuTheAccursed->self, 3.0s cast, applies Dmg up and haste to self
    EntropicFlame = 24111, // WarWraith->self, 4.0s cast, range 60 width 8 rect
    DarkFlare = 24112, // WarWraith->location, 5.0s cast, range 8 circle
    SoulSacrifice = 24113, // WarWraith->Boss, 6.0s cast, interruptible, WarWraith sacrifices to give Dmg Up to Boss

    DeadlyToxin = 24699, // DeathwallHelper->self, no cast, range 25-30 donut, deathwall
    Shock = 24114, // ShockSphere->self, no cast, range 7 circle aoe around sphere

    AutoAttackMonoceros = 871, // Monoceros->Boss, no cast, single-target
    PurifyingLight = 24115, // Monoceros->location, 11.0s cast, range 12 circle, visual
    PurifyingLightAOE = 24116, // Helper->location, no cast, range 12 circle, cleanse doom
    Ruin = 24119, // LlofiiTheForthright->Boss, 2.5s cast, single-target, autoattack
    Cleanse = 24969, // LlofiiTheForthright->location, 5.0s cast, range 6 circle, damages boss
    SoothingGlimmer = 24970 // LlofiiTheForthright->self, 2.5s cast, single-target, heal
}

public enum SID : uint
{
    ForwardMarch = 2161, // Boss->player, extra=0x0
    AboutFace = 2162, // Boss->player, extra=0x0
    LeftFace = 2163, // Boss->player, extra=0x0
    RightFace = 2164 // Boss->player, extra=0x0
}

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(25f, 30f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GallowsMarch && Arena.Bounds.Radius > 25f)
        {
            _aoe = [new(donut, Arena.Center, default, Module.CastFinishAt(spell, 0.8d))];
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Deathwall)
        {
            Arena.Bounds = new ArenaBoundsCircle(25f); // default arena got no extra collision, just a donut aoe
            Arena.Center = Arena.Center.Quantized();
            _aoe = [];
        }
    }
}

class DevourSoul(BossModule module) : Components.SingleTargetCast(module, (uint)AID.DevourSoul);
class Blight(BossModule module) : Components.RaidwideCast(module, (uint)AID.Blight);

class GallowsMarch(BossModule module) : Components.StatusDrivenForcedMarch(module, 3f, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace)
{
    private readonly PurifyingLight _aoe = module.FindComponent<PurifyingLight>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (!aoe.Check(pos))
            {
                return true;
            }
        }
        return false;
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (Module.PrimaryActor.CastInfo?.IsSpell(AID.GallowsMarch) ?? false)
            hints.Add("Apply doom & march debuffs");
    }
}

class ShockSphere(BossModule module) : Components.Voidzone(module, 7f, GetSpheres)
{
    private static List<Actor> GetSpheres(BossModule module) => module.Enemies((uint)OID.ShockSphere);
}

class SoulPurge(BossModule module) : Components.GenericAOEs(module)
{
    private bool dualCast;
    private readonly List<AOEInstance> _aoes = [with(2)];

    private static readonly AOEShapeCircle circle = new(10f);
    private static readonly AOEShapeDonut donut = new(10f, 30f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (i == 0)
            {
                if (count > 1)
                    aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
            else
                aoe.Risky = false;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ChainMagick:
                dualCast = true;
                break;
            case (uint)AID.SoulPurgeDonut:
                if (!dualCast)
                    AddAOE(donut);
                else
                    AddAOEs(donut, circle);
                break;
            case (uint)AID.SoulPurgeCircle:
                if (!dualCast)
                    AddAOE(circle);
                else
                    AddAOEs(circle, donut);
                break;
        }
        void AddAOE(AOEShape shape, float delay = default) => _aoes.Add(new(shape, spell.LocXZ, default, Module.CastFinishAt(spell, delay)));
        void AddAOEs(AOEShape shape1, AOEShape shape2)
        {
            AddAOE(shape1);
            AddAOE(shape2, 2.1f);
            dualCast = false;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.SoulPurgeCircle or (uint)AID.SoulPurgeCircleDual or (uint)AID.SoulPurgeDonut or (uint)AID.SoulPurgeDonutDual)
            _aoes.RemoveAt(0);
    }
}

class CrimsonBlade(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CrimsonBlade, new AOEShapeCone(50f, 90f.Degrees()));
class BloodCyclone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BloodCyclone, 5f);
class Aethertide(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.AethertideAOE, 8f);
class MarchingBreath(BossModule module) : Components.CastInterruptHint(module, (uint)AID.MarchingBreath, showNameInHint: true); // heals all allies by 20% of max health (raidwide)
class TacticalAero(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TacticalAero, new AOEShapeRect(40f, 4f));
class EntropicFlame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EntropicFlame, new AOEShapeRect(60f, 4f));
class DarkFlare(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkFlare, 8f);
class SoulSacrifice(BossModule module) : Components.CastInterruptHint(module, (uint)AID.SoulSacrifice, showNameInHint: true); // WarWraith sacrifices itself to give boss a damage buff

class PurifyingLight : Components.SimpleAOEs
{
    public PurifyingLight(BossModule module) : base(module, (uint)AID.PurifyingLight, 12f)
    {
        Color = Colors.SafeFromAOE;
        Risky = false;
    }
}

class CE42FromBeyondTheGraveStates : StateMachineBuilder
{
    public CE42FromBeyondTheGraveStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<DevourSoul>()
            .ActivateOnEnter<Blight>()
            .ActivateOnEnter<PurifyingLight>()
            .ActivateOnEnter<GallowsMarch>()
            .ActivateOnEnter<ShockSphere>()
            .ActivateOnEnter<SoulPurge>()
            .ActivateOnEnter<CrimsonBlade>()
            .ActivateOnEnter<BloodCyclone>()
            .ActivateOnEnter<Aethertide>()
            .ActivateOnEnter<MarchingBreath>()
            .ActivateOnEnter<TacticalAero>()
            .ActivateOnEnter<EntropicFlame>()
            .ActivateOnEnter<DarkFlare>()
            .ActivateOnEnter<SoulSacrifice>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 778, NameID = 20)] // bnpcname=9931
public class CE42FromBeyondTheGrave(WorldState ws, Actor primary) : BossModule(ws, primary, startingArena.Center, startingArena)
{
    private static readonly ArenaBoundsCustom startingArena = new([new Polygon(new(-60f, 800f), 29.5f, 32)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies((uint)OID.WarWraith));
        Arena.Actors(Enemies((uint)OID.HernaisTheTenacious));
        Arena.Actors(Enemies((uint)OID.DyunbuTheAccursed));
    }

    protected override bool CheckPull() => base.CheckPull() && Raid.Player() is Actor player && player.Position.InCircle(Arena.Center, 30f);
}
