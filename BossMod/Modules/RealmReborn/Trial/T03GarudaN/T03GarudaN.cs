namespace BossMod.RealmReborn.Trial.T03GarudaN;

public enum OID : uint
{
    Boss = 0xEF, // x1
    Monolith = 0xED, // x4
    EyeOfTheStormHelper = 0x622, // x1
    RazorPlumeP1 = 0xEE, // spawn during fight
    RazorPlumeP2 = 0x2B0, // spawn during fight
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Friction = 656, // Boss->players, no cast, range 5 circle at random target
    Downburst = 657, // Boss->self, no cast, range 10+1.7 ?-degree cone cleave
    WickedWheel = 658, // Boss->self, no cast, range 7+1.7 circle cleave
    Slipstream = 659, // Boss->self, 2.5s cast, range 10+1.7 ?-degree cone interruptible aoe
    MistralSongP1 = 667, // Boss->self, 4.0s cast, range 30+1.7 LOSable raidwide
    AerialBlast = 662, // Boss->self, 4.0s cast, raidwide
    EyeOfTheStorm = 664, // EyeOfTheStormHelper->self, 3.0s cast, range 12-25 donut
    MistralSongP2 = 660, // Boss->self, 4.0s cast, range 30+1.7 ?-degree cone aoe
    MistralShriek = 661, // Boss->self, 4.0s cast, range 23+1.7 circle aoe
    Featherlance = 665, // RazorPlumeP1/RazorPlumeP2->self, no cast, range 8 circle, suicide attack if not killed in ~25s
}

// disallow clipping monoliths
class Friction(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.PrimaryActor.CastInfo == null) // don't forbid standing near monoliths while boss is casting to allow avoiding aoes
        {
            var monoliths = Module.Enemies((uint)OID.Monolith);
            var count = monoliths.Count;
            for (var i = 0; i < count; ++i)
                hints.AddForbiddenZone(new SDCircle(monoliths[i].Position, 5));
        }
    }
}

class Downburst(BossModule module) : Components.Cleave(module, (uint)AID.Downburst, new AOEShapeCone(11.7f, 60f.Degrees()));
class Slipstream(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Slipstream, new AOEShapeCone(11.7f, 45f.Degrees()));

class MistralSongP1(BossModule module) : Components.CastLineOfSightAOE(module, (uint)AID.MistralSongP1, 31.7f, true)
{
    public override ReadOnlySpan<Actor> BlockerActors() => CollectionsMarshal.AsSpan(Module.Enemies((uint)OID.Monolith));
}

// actual casts happen every ~6s after aerial blast cast
class EyeOfTheStorm(BossModule module) : Components.GenericAOEs(module, (uint)AID.AerialBlast)
{
    private readonly AOEShapeDonut _shape = new(12f, 25f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (NumCasts > 0)
        {
            var eyes = Module.Enemies((uint)OID.EyeOfTheStormHelper);
            var count = eyes.Count;
            var aoes = new AOEInstance[count];
            for (var i = 0; i < count; ++i)
                aoes[i] = new(_shape, eyes[i].Position);
            return aoes;
        }
        return [];
    }
}

class MistralSongP2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MistralSongP2, new AOEShapeCone(31.7f, 60f.Degrees()));
class MistralShriek(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MistralShriek, 24.7f);

class T03GarudaNStates : StateMachineBuilder
{
    public T03GarudaNStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Friction>()
            .ActivateOnEnter<Downburst>()
            .ActivateOnEnter<Slipstream>()
            .ActivateOnEnter<MistralSongP1>()
            .ActivateOnEnter<EyeOfTheStorm>()
            .ActivateOnEnter<MistralSongP2>()
            .ActivateOnEnter<MistralShriek>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 58, NameID = 1644)]
public class T03GarudaN(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsCircle(21f))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.RazorPlumeP1 or (uint)OID.RazorPlumeP2 => 2,
                (uint)OID.Boss => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Monolith), Colors.Danger, true);
    }
}
