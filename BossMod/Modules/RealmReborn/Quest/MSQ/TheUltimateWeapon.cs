namespace BossMod.RealmReborn.Quest.MSQ.TheUltimateWeapon;

public enum OID : uint
{
    Boss = 0x3933, // R1.75
    SeaOfPitch = 0x1EB738, // R0.5
    Firesphere = 0x3934, // R1.0
}

public enum AID : uint
{
    AncientFireIII = 29327, // Boss->self, 4.0s cast, range 40 circle
    DarkThunder = 29329, // Lahabrea->self, 4.0s cast, range 1 circle
    EndOfDays = 29331, // Boss->self, 4.0s cast, range 60 width 8 rect
    EndOfDaysAdds = 29762, // PhantomLahabrea->self, 4.0s cast, range 60 width 8 rect
    Nightburn = 29340, // Boss->player, 4.0s cast, single-target
    FiresphereSummon = 29332, // Boss->self, 4.0s cast, single-target
    Burst = 29333, // Firesphere->self, 3.0s cast, range 8 circle
    AncientEruption = 29335, // Lahabrea->self, 4.0s cast, range 6 circle
    FluidFlare = 29760, // Lahabrea->self, 4.0s cast, range 40 60-degree cone
    AncientCross = 29756, // Lahabrea->self, 4.0s cast, range 6 circle
    BurstFlare = 29758, // Lahabrea->self, 5.0s cast, range 60 circle
    GripOfNight = 29337, // Boss->self, 6.0s cast, range 40 150-degree cone
}

class BurstFlare(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.BurstFlare, 10f)
{
    private readonly FireSphere _aoe = module.FindComponent<FireSphere>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // don't add any hints if Burst hasn't gone off yet, it tends to spook AI mode into running into deathwall
        if (_aoe.ActiveAOEs(slot, actor).Length != 0)
            return;
        if (Casters.Count != 0)
            hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center, 5f), Casters.Ref(0).Activation);
    }
}

class GripOfNight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GripOfNight, new AOEShapeCone(40, 75f.Degrees()));
class AncientCross(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AncientCross, 6f, 8);
class AncientEruption(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AncientEruption, 6f);
class FluidFlare(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FluidFlare, new AOEShapeCone(40f, 30f.Degrees()));

class FireSphere(BossModule module) : Components.GenericAOEs(module, (uint)AID.Burst)
{
    private DateTime? _predictedCast;
    private static readonly AOEShapeCircle circle = new(8f);

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FiresphereSummon)
            _predictedCast = WorldState.CurrentTime.AddSeconds(12d);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Burst)
            _predictedCast = Module.CastFinishAt(spell);
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_predictedCast is DateTime dt && dt > WorldState.CurrentTime)
        {
            var spheres = Module.Enemies((uint)OID.Firesphere);
            var count = spheres.Count;
            var aoes = new AOEInstance[count];
            for (var i = 0; i < count; ++i)
                aoes[i] = new(circle, spheres[i].Position.Quantized(), default, dt);
            return aoes;
        }
        return [];
    }
}

class Nightburn(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Nightburn);
class AncientFire(BossModule module) : Components.RaidwideCast(module, (uint)AID.AncientFireIII);

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private bool completed;
    private static readonly AOEShapeDonut donut = new(15f, 20f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AncientFireIII && !completed)
        {
            _aoe = [new(donut, Arena.Center, default, Module.CastFinishAt(spell, 0.7d))];
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x00 && state == 0x00020001u)
        {
            Arena.Bounds = Lahabrea.SmallerBounds;
            completed = true;
        }
    }
}

class DarkThunder(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkThunder, 1f);
class SeaOfPitch(BossModule module) : Components.Voidzone(module, 4f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.SeaOfPitch);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

abstract class EoD(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeRect(60f, 4f));
class EndOfDays(BossModule module) : EoD(module, (uint)AID.EndOfDays);
class EndOfDaysAdds(BossModule module) : EoD(module, (uint)AID.EndOfDaysAdds);

class LahabreaStates : StateMachineBuilder
{
    public LahabreaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<DarkThunder>()
            .ActivateOnEnter<SeaOfPitch>()
            .ActivateOnEnter<AncientFire>()
            .ActivateOnEnter<EndOfDays>()
            .ActivateOnEnter<Nightburn>()
            .ActivateOnEnter<FireSphere>()
            .ActivateOnEnter<AncientEruption>()
            .ActivateOnEnter<FluidFlare>()
            .ActivateOnEnter<AncientCross>()
            .ActivateOnEnter<BurstFlare>()
            .ActivateOnEnter<GripOfNight>()
            .ActivateOnEnter<EndOfDaysAdds>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70058, NameID = 2143)]
public class Lahabrea(WorldState ws, Actor primary) : BossModule(ws, primary, new(-704f, 480f), new ArenaBoundsCircle(20f))
{
    public static readonly ArenaBoundsCircle SmallerBounds = new(15);
}

