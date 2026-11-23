namespace BossMod.Endwalker.Dungeon.D05Aitiascope.D051Livia;

public enum OID : uint
{
    Boss = 0x3469, // R7.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 24771, // Boss->player, no cast, single-target

    AglaeaBite = 25673, // Boss->self/player, 5.0s cast, range 9 90-degree cone, tankbuster 

    AglaeaClimb1 = 25666, // Boss->self, 7.0s cast, single-target
    AglaeaClimb2 = 25667, // Boss->self, 7.0s cast, single-target
    AglaeaClimbAOE = 25668, // Helper->self, 7.0s cast, range 20 90-degree cone

    AglaeaShotVisual = 25669, // Boss->self, 3.0s cast, single-target
    AglaeaShot1 = 25670, // 346A->location, 3.0s cast, range 20 width 4 rect
    AglaeaShot2 = 25671, // 346A->location, 1.0s cast, range 40 width 4 rect

    Disparagement = 25674, // Boss->self, 5.0s cast, range 40 120-degree cone

    Frustration = 25672, // Boss->self, 5.0s cast, range 40 circle, raidwide

    IgnisAmoris = 25676, // Helper->location, 4.0s cast, range 6 circle
    IgnisOdi = 25677, // Helper->players, 5.0s cast, range 6 circle

    OdiEtAmo = 25675 // Boss->self, 3.0s cast, single-target
}

class AglaeaBite(BossModule module) : Components.BaitAwayCast(module, (uint)AID.AglaeaBite, new AOEShapeCone(9f, 60f.Degrees()), endsOnCastEvent: true, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

class AglaeaShot(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(8)];
    private static readonly AOEShapeRect rect = new(20f, 3f);
    private readonly List<Actor> casters = [with(8)];
    private DateTime activation;

    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count != 0)
            return CollectionsMarshal.AsSpan(_aoes);
        if ((activation - WorldState.CurrentTime).TotalSeconds < 5d)
        {
            var count = casters.Count;
            var aoesLen = count;
            if (_aoesBuffer.Length < aoesLen)
                _aoesBuffer = new AOEInstance[aoesLen];
            var aoes = _aoesBuffer;
            for (var i = 0; i < count; ++i)
            {
                var c = casters[i];
                aoes[i] = new(rect, c.Position, c.Rotation, activation);
            }
            return aoes.AsSpan(0, aoesLen);
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.AglaeaShot1 or (uint)AID.AglaeaShot2)
        {
            _aoes.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            casters.Clear();
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
        {
            if (spell.Action.ID == (uint)AID.AglaeaShot1)
            {
                activation = WorldState.FutureTime(10d);
                _aoes.RemoveAt(0);
                casters.Add(caster);
            }
            else if (spell.Action.ID == (uint)AID.AglaeaShot2)
                _aoes.RemoveAt(0);
        }
    }
}

class AglaeaClimbAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AglaeaClimbAOE, new AOEShapeCone(20f, 45f.Degrees()));
class Disparagement(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Disparagement, new AOEShapeCone(40f, 60f.Degrees()));

class IgnisOdi(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.IgnisOdi, 6f, 4, 4);
class IgnisAmoris(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IgnisAmoris, 6f);
class Frustration(BossModule module) : Components.RaidwideCast(module, (uint)AID.Frustration);

class D051LiviaStates : StateMachineBuilder
{
    public D051LiviaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AglaeaShot>()
            .ActivateOnEnter<AglaeaClimbAOE>()
            .ActivateOnEnter<Disparagement>()
            .ActivateOnEnter<IgnisOdi>()
            .ActivateOnEnter<IgnisAmoris>()
            .ActivateOnEnter<Frustration>()
            .ActivateOnEnter<AglaeaBite>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 786, NameID = 10290)]
public class D051Livia(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(-6f, 471), 19.5f * CosPI.Pi36th, 36)], [new Rectangle(new(-6f, 491.025f), 20f, 1.25f)]);
}
