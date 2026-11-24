namespace BossMod.Shadowbringers.Quest.Job.Gunbreaker.SteelAgainstSteel;

public enum OID : uint
{
    Boss = 0x2A45,
    Fustuarium = 0x2AD8, // R0.5
    CullingBlade = 0x2AD3, // R0.5
    IndustrialForce = 0x2BCE, // R0.5
    TerminusEst = 0x2A46, // R1.0
    CaptiveBolt = 0x2AD7, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    CullingBlade1 = 17553, // CullingBlade->self, 3.5s cast, range 60 30-degree cone
    TheOrder = 17568, // Boss->self, 4.0s cast, single-target
    TerminusEst1 = 17567, // TerminusEst->self, no cast, range 40+R width 4 rect
    CaptiveBolt = 17561, // CaptiveBolt->self, 7.0s cast, range 50+R width 10 rect
    AetherochemicalGrenado = 17575, // 2A47->location, 4.0s cast, range 8 circle
    Exsanguination1 = 17563, // 2AD4->self, 5.0s cast, range 2-7 180-degree donut segment
    Exsanguination2 = 17564, // 2AD5->self, 5.0s cast, range 7-12 180-degree donut segment
    Exsanguination3 = 17565, // 2AD6->self, 5.0s cast, range 12-17 180-degree donut segment
    DiffractiveLaser = 17574, // 2A48->self, 3.0s cast, range 45+R width 4 rect
    SnakeShot = 17569, // Boss->self, 4.0s cast, range 20 240-degree cone
    ScaldingTank1 = 17558, // Fustuarium->2A4A, 6.0s cast, range 6 circle
    ToTheSlaughter = 17559, // Boss->self, 4.0s cast, range 40 180-degree cone
}

class ScaldingTank(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.ScaldingTank1, 6f);
class ToTheSlaughter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ToTheSlaughter, new AOEShapeCone(40f, 90f.Degrees()));
class Exsanguination1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Exsanguination1, new AOEShapeDonutSector(2f, 7f, 90f.Degrees()));
class Exsanguination2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Exsanguination2, new AOEShapeDonutSector(7f, 12f, 90f.Degrees()));
class Exsanguination3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Exsanguination3, new AOEShapeDonutSector(12f, 17f, 90f.Degrees()));
class CaptiveBolt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CaptiveBolt, new AOEShapeRect(50f, 5f), 4);
class AetherochemicalGrenado(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AetherochemicalGrenado, 8f);
class DiffractiveLaser(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DiffractiveLaser, new AOEShapeRect(45f, 2f));
class SnakeShot(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SnakeShot, new AOEShapeCone(20f, 120f.Degrees()));
class CullingBlade(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CullingBlade1, new AOEShapeCone(60f, 15f.Degrees()));

class TerminusEst(BossModule module) : Components.GenericAOEs(module)
{
    private bool casting;
    private readonly List<Actor> casters = [];
    private static readonly AOEShapeRect rect = new(40f, 2f);

    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (casting)
        {
            var count = casters.Count;
            var aoesLen = count;
            if (_aoesBuffer.Length < aoesLen)
                _aoesBuffer = new AOEInstance[aoesLen];
            var aoes = _aoesBuffer;
            for (var i = 0; i < count; ++i)
            {
                var c = casters[i];
                aoes[i] = new(rect, c.Position, c.Rotation, Module.CastFinishAt(Module.PrimaryActor.CastInfo));
            }
            return aoes.AsSpan(0, aoesLen);
        }
        return [];
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.TerminusEst)
            casters.Add(actor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // check if we already have terminuses out, because he can use this spell for a diff mechanic
        if (casters.Count != 0 && spell.Action.ID == (uint)AID.TheOrder)
            casting = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.TerminusEst1)
        {
            casters.Remove(caster);
            // reset for next iteration
            if (casters.Count == 0)
                casting = false;
        }
    }
}

class VitusQuoMessallaStates : StateMachineBuilder
{
    public VitusQuoMessallaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CullingBlade>()
            .ActivateOnEnter<TerminusEst>()
            .ActivateOnEnter<CaptiveBolt>()
            .ActivateOnEnter<AetherochemicalGrenado>()
            .ActivateOnEnter<DiffractiveLaser>()
            .ActivateOnEnter<SnakeShot>()
            .ActivateOnEnter<Exsanguination1>()
            .ActivateOnEnter<Exsanguination2>()
            .ActivateOnEnter<Exsanguination3>()
            .ActivateOnEnter<ToTheSlaughter>()
            .ActivateOnEnter<ScaldingTank>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68802, NameID = 8872)]
public class VitusQuoMessalla(WorldState ws, Actor primary) : BossModule(ws, primary, new(-266f, -507f), new ArenaBoundsCircle(19.5f));
