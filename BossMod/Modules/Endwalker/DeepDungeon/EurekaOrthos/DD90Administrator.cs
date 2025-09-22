namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.DD90Administrator;

public enum OID : uint
{
    Boss = 0x3D23, // R5.950
    EggInterceptor = 0x3D24, // R2.3
    SquareInterceptor = 0x3D25, // R2.3
    OrbInterceptor = 0x3D26, // R2.3
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 31457, // Boss->player, no cast, single-target

    AetherochemicalLaserCone = 31451, // EggInterceptor->self, 6.0s cast, range 50 120-degree cone
    AetherochemicalLaserDonut = 31453, // OrbInterceptor->self, 6.0s cast, range 8-40 donut
    AetherochemicalLaserLine = 31452, // SquareInterceptor->self, 6.0s cast, range 40 width 5 rect

    AetherochemicalLaserCone2 = 32832, // EggInterceptor->self, 8.0s cast, range 50 120-degree cone
    AetherochemicalLaserLine2 = 32833, // SquareInterceptor->self, 8.0s cast, range 40 width 5 rect

    CrossLaser = 31448, // Boss->self, 6.0s cast, range 60 width 10 cross

    PeripheralLasersModelChange1 = 31458, // Boss->self, no cast, single-target
    PeripheralLasersModelChange2 = 31459, // Boss->self, no cast, single-target
    PeripheralLasers = 31447, // Boss->self, 6.0s cast, range 5-60 donut

    HomingLaserVisual = 31460, // Boss->self, no cast, single-target
    HomingLaser = 31461, // Helper->location, 3.0s cast, range 6 circle

    Laserstream = 31456, // Boss->self, 4.0s cast, range 60 circle, raidwide
    ParallelExecution = 31454, // Boss->self, 3.0s cast, range 60 circle, starts action combo

    SalvoScript = 31455, // Boss->self, 3.0s cast, range 60 circle
    SupportSystems = 31449, // Boss->self, 3.0s cast, single-target

    InterceptionSequence = 31450 // Boss->self, 3.0s cast, range 60 circle, starts action combo
}

public enum IconID : uint
{
    Icon1 = 390,
    Icon2 = 391,
    Icon3 = 392
}

class AetheroChemicalLaserCombo(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCone(50f, 60f.Degrees()), new AOEShapeDonut(8f, 60f), new AOEShapeRect(40f, 2.5f),
    new AOEShapeCross(60f, 5f), new AOEShapeDonut(5f, 60f)];
    public readonly List<AOEInstance> AOEs = new(6);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(AOEs);
        var act0 = aoes[0].Activation;
        var deadline1 = act0.AddSeconds(1d);
        var deadline2 = act0.AddSeconds(5d);
        var isNotLastSet = aoes[^1].Activation > deadline1;
        var color = Colors.Danger;
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (aoe.Activation > deadline2)
                break;
            if (aoe.Activation < deadline1)
            {
                if (isNotLastSet)
                    aoe.Color = color;
                aoe.Risky = true;
            }
            else
                aoe.Risky = false;
            ++index;
        }
        return aoes[..index];
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var shapeIndex = actor.OID switch
        {
            (uint)OID.SquareInterceptor => 2,
            (uint)OID.OrbInterceptor => 1,
            (uint)OID.EggInterceptor => 0,
            _ => -1
        };

        if (shapeIndex == -1)
            return;

        var delay = iconID switch
        {
            (uint)IconID.Icon1 => 7d,
            (uint)IconID.Icon2 => 10.5d,
            (uint)IconID.Icon3 => 14d,
            _ => default
        };

        AOEs.Add(new(_shapes[shapeIndex], actor.Position.Quantized(), actor.OID == (uint)OID.OrbInterceptor ? default : actor.Rotation, WorldState.FutureTime(delay)));
        if (AOEs.Count == 6)
            AOEs.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = spell.Action.ID switch
        {
            (uint)AID.PeripheralLasers => _shapes[4],
            (uint)AID.CrossLaser => _shapes[3],
            _ => null
        };

        if (shape != null)
        {
            AOEs.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.AetherochemicalLaserCone:
            case (uint)AID.AetherochemicalLaserLine:
            case (uint)AID.AetherochemicalLaserDonut:
            case (uint)AID.PeripheralLasers:
            case (uint)AID.CrossLaser:
                if (AOEs.Count != 0)
                {
                    AOEs.RemoveAt(0);
                }
                break;
        }
    }
}

class AetherLaserLine : Components.SimpleAOEs
{
    private readonly AetheroChemicalLaserCombo _aoe;

    public AetherLaserLine(BossModule module) : base(module, (uint)AID.AetherochemicalLaserLine, new AOEShapeRect(40f, 2.5f), 4)
    {
        MaxDangerColor = 2;
        MaxRisky = 2;
        _aoe = module.FindComponent<AetheroChemicalLaserCombo>()!;
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Count != 0 && _aoe.AOEs.Count == 0 ? base.ActiveAOEs(slot, actor) : [];
}

class AetherLaserLine2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AetherochemicalLaserLine2, new AOEShapeRect(40f, 2.5f));
class AetherLaserCone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AetherochemicalLaserCone2, new AOEShapeCone(50f, 60f.Degrees()));
class HomingLasers(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HomingLaser, 6f);
class Laserstream(BossModule module) : Components.RaidwideCast(module, (uint)AID.Laserstream);

class DD90AdministratorStates : StateMachineBuilder
{
    public DD90AdministratorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AetheroChemicalLaserCombo>()
            .ActivateOnEnter<AetherLaserLine>()
            .ActivateOnEnter<AetherLaserLine2>()
            .ActivateOnEnter<AetherLaserCone>()
            .ActivateOnEnter<Laserstream>()
            .ActivateOnEnter<HomingLasers>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "legendoficeman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 905, NameID = 12102)]
public class DD90Administrator(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300f, -300f), new ArenaBoundsSquare(20f));
