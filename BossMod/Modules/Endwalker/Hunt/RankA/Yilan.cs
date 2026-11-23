namespace BossMod.Endwalker.Hunt.RankA.Yilan;

public enum OID : uint
{
    Boss = 0x35BF // R5.400, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    Soundstorm = 27230, // Boss->self, 5.0s cast, range 30 circle, applies march debuffs
    MiniLight = 27231, // Boss->self, 6.0s cast, range 18 circle
    Devour = 27232, // Boss->self, 1.0s cast, range 10 ?-degree cone, kills seduced and deals very small damage otherwise
    BogBomb = 27233, // Boss->location, 4.0s cast, range 6 circle
    BrackishRain = 27234 // Boss->self, 4.0s cast, range 10 90-degree cone
}

public enum SID : uint
{
    None = 0,
    ForwardMarch = 1958,
    AboutFace = 1959,
    LeftFace = 1960,
    RightFace = 1961
}

class Soundstorm(BossModule module) : Components.StatusDrivenForcedMarch(module, 2, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace)
{
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => MiniLight.Shape.Check(pos, Module.PrimaryActor);

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (Module.PrimaryActor.CastInfo?.IsSpell(AID.Soundstorm) ?? false)
            hints.Add("Apply march debuffs");
    }
}

class MiniLight(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;

    public static readonly AOEShapeCircle Shape = new(18);

    private readonly AOEInstance[] _oneAOEInstance = new AOEInstance[1];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
        {
            _oneAOEInstance[0] = new(Shape, Module.PrimaryActor.Position, default, _activation);
            return _oneAOEInstance;
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var activation = spell.Action.ID switch
        {
            (uint)AID.Soundstorm => Module.CastFinishAt(spell, 12.1f), // timing varies, have seen delays between 17.2s and 17.8s, but 2nd AID should correct any incorrectness
            (uint)AID.MiniLight => Module.CastFinishAt(spell),
            _ => default
        };
        if (activation != default)
            _activation = activation;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MiniLight)
            _activation = default;
    }
}

class Devour(BossModule module) : Components.CastHint(module, (uint)AID.Devour, "Harmless unless you got minimized by the previous mechanic");
class BogBomb(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BogBomb, 6f);
class BrackishRain(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BrackishRain, new AOEShapeCone(10f, 45f.Degrees()));

class YilanStates : StateMachineBuilder
{
    public YilanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Soundstorm>()
            .ActivateOnEnter<MiniLight>()
            .ActivateOnEnter<Devour>()
            .ActivateOnEnter<BogBomb>()
            .ActivateOnEnter<BrackishRain>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 10625)]
public class Yilan(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
