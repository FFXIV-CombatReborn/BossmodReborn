namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE54NeverCryWolf;

public enum OID : uint
{
    Boss = 0x319C, // R9.996, x1
    IceSprite = 0x319D, // R0.800, spawn during fight
    Icicle = 0x319E, // R3.000, spawn during fight
    Imaginifer = 0x319F, // R0.500, spawn during fight
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target

    IcePillar = 23581, // Boss->self, 3.0s cast, single-target, viusal
    IcePillarAOE = 23582, // Icicle->self, 3.0s cast, range 4 circle aoe (pillar drop)
    PillarPierce = 23583, // Icicle->self, 3.0s cast, range 80 width 4 rect aoe (pillar fall)
    Shatter = 23584, // Icicle->self, 3.0s cast, range 8 circle aoe (pillar explosion after lunar cry)
    Tramontane = 23585, // Boss->self, 3.0s cast, single-target, visual
    BracingWind = 23586, // IceSprite->self, 9.0s cast, range 60 width 12 rect, visual
    BracingWindAOE = 24787, // Helper->self, no cast, range 60 width 12 rect knock-forward 40
    LunarCry = 23588, // Boss->self, 14.0s cast, range 80 circle LOSable aoe

    ThermalGust = 23589, // Imaginifer->self, 2.0s cast, range 60 width 4 rect aoe (when adds appear)
    GlaciationEnrage = 22881, // Boss->self, 20.0s cast, single-target, visual
    GlaciationEnrageAOE = 23625, // Helper->self, no cast, ???, raidwide (deadly if adds aren't killed)
    AgeOfEndlessFrostFirstCW = 23590, // Boss->self, 5.0s cast, single-target, visual
    AgeOfEndlessFrostFirstCCW = 23591, // Boss->self, 5.0s cast, single-target, visual
    AgeOfEndlessFrostFirstAOE = 23592, // Helper->self, 5.0s cast, range 40 20-degree cone
    AgeOfEndlessFrostRest = 22883, // Boss->self, no cast, single-target
    AgeOfEndlessFrostRestAOE = 23593, // Helper->self, 0.5s cast, range 40 20-degree cone

    StormWithout = 23594, // Boss->self, 5.0s cast, single-target
    StormWithoutAOE = 23595, // Helper->self, 5.0s cast, range 10-40 donut
    StormWithin = 23596, // Boss->self, 5.0s cast, single-target
    StormWithinAOE = 23597, // Helper->self, 5.0s cast, range 10 circle
    AncientGlacier = 23600, // Boss->self, 3.0s cast, single-target, visual
    AncientGlacierAOE = 23601, // Helper->location, 3.0s cast, range 6 circle puddle
    Glaciation = 23602, // Boss->self, 5.0s cast, single-target, visual
    GlaciationAOE = 23603, // Helper->self, 5.6s cast, ???, raidwide

    TeleportBoss = 23621, // Boss->location, no cast, teleport
    TeleportImaginifer = 23622, // Imaginifer->location, no cast, ???, teleport
    ActivateImaginifer = 23623 // Imaginifer->self, no cast, single-target, visual
}

sealed class IcePillar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IcePillarAOE, 4f);
sealed class PillarPierce(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PillarPierce, new AOEShapeRect(80f, 2f));
sealed class Shatter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shatter, 8f);

sealed class BracingWind(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.BracingWind, 40f, false, 1, new AOEShapeRect(60f, 6f), Kind.DirForward)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        const float length = 24f * 2f; // casters are at the border, orthogonal to borders
        var count = Casters.Count;
        if (count == 0)
        {
            return;
        }
        var casters = CollectionsMarshal.AsSpan(Casters);
        for (var i = 0; i < count; ++i)
        {
            ref readonly var c = ref casters[i];
            hints.AddForbiddenZone(new SDRect(c.Origin, c.Direction, length, Distance - length, 6), c.Activation);
        }
    }
}

sealed class LunarCry(BossModule module) : Components.CastLineOfSightAOE(module, (uint)AID.LunarCry, 80f)
{
    private readonly List<Actor> _safePillars = [];
    private readonly BracingWind? _knockback = module.FindComponent<BracingWind>();

    public override ReadOnlySpan<Actor> BlockerActors() => CollectionsMarshal.AsSpan(_safePillars);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_knockback?.Casters.Count > 0)
            return; // resolve knockbacks first
        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Icicle)
            _safePillars.Add(actor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == (uint)AID.PillarPierce)
            _safePillars.Remove(caster);
    }
}

// this AOE only got 2s cast time, but the actors already spawn 4.5s earlier, so we can use that to our advantage
sealed class ThermalGust(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(60f, 2f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Imaginifer)
            _aoes.Add(new(_shape, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(6.5d)));

    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ThermalGust)
            _aoes.RemoveAt(0);
    }
}

sealed class AgeOfEndlessFrost(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle _increment;
    private DateTime _activation;
    private WPos _pos;
    private readonly List<Angle> _rotation = new(6);

    private static readonly AOEShapeCone _shape = new(40f, 10f.Degrees());

    private void InitIfReady()
    {
        if (_rotation.Count == 6 && _increment != default)
        {
            for (var i = 0; i < 6; ++i)
                Sequences.Add(new(_shape, _pos, _rotation[i], _increment, _activation, 2f, 7));
            _rotation.Clear();
            _increment = default;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.AgeOfEndlessFrostFirstCW:
                _increment = -40f.Degrees();
                InitIfReady();
                break;
            case (uint)AID.AgeOfEndlessFrostFirstCCW:
                _increment = 40f.Degrees();
                InitIfReady();
                break;
            case (uint)AID.AgeOfEndlessFrostFirstAOE:
                _rotation.Add(spell.Rotation);
                _activation = Module.CastFinishAt(spell);
                _pos = spell.LocXZ;
                InitIfReady();
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.AgeOfEndlessFrostFirstAOE or (uint)AID.AgeOfEndlessFrostRestAOE)
            AdvanceSequence(spell.LocXZ, spell.Rotation, WorldState.CurrentTime);
    }
}

sealed class StormWithout(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StormWithout, new AOEShapeDonut(10f, 40f));
sealed class StormWithin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StormWithin, 10f);
sealed class AncientGlacier(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AncientGlacierAOE, 6f);
sealed class Glaciation(BossModule module) : Components.RaidwideCast(module, (uint)AID.Glaciation);

sealed class CE54NeverCryWolfStates : StateMachineBuilder
{
    public CE54NeverCryWolfStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IcePillar>()
            .ActivateOnEnter<PillarPierce>()
            .ActivateOnEnter<Shatter>()
            .ActivateOnEnter<BracingWind>()
            .ActivateOnEnter<LunarCry>()
            .ActivateOnEnter<ThermalGust>()
            .ActivateOnEnter<AgeOfEndlessFrost>()
            .ActivateOnEnter<StormWithout>()
            .ActivateOnEnter<StormWithin>()
            .ActivateOnEnter<AncientGlacier>()
            .ActivateOnEnter<Glaciation>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 778, NameID = 25)] // bnpcname=9941
public sealed class CE54NeverCryWolf(WorldState ws, Actor primary) : BossModule(ws, primary, new(-830f, 190f), new ArenaBoundsSquare(24f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies((uint)OID.Imaginifer));
    }

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InSquare(Arena.Center, 24f);
}
