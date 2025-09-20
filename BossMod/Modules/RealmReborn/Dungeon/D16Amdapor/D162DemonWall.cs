namespace BossMod.RealmReborn.Dungeon.D16Amdapor.D162DemonWall;

public enum OID : uint
{
    Helper = 0x19A, // x3
    Boss = 0x283, // x1
    Pollen = 0x1E86B1 // x1, EventObj type
}

public enum AID : uint
{
    MurderHole = 1044, // Boss->player, no cast, range 6 circle cleaving autoattack at random target
    LiquefyCenter = 1045, // Helper->self, 3.0s cast, range 50+R width 8 rect
    LiquefySides = 1046, // Helper->self, 2.0s cast, range 50+R width 7 rect
    Repel = 1047 // Boss->self, 3.0s cast, range 40+R 180?-degree cone knockback 20 (non-immunable)
}

class LiquefyCenter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LiquefyCenter, new AOEShapeRect(50f, 4f));
class LiquefySides(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LiquefySides, new AOEShapeRect(50f, 3.5f));

class Repel(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Repel, 20f, true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // custom hint: stay in narrow zone in center
        if (Casters.Count != 0)
        {
            hints.AddForbiddenZone(new SDInvertedRect(Module.PrimaryActor.Position, new WDir(default, 1f), 50f, -2f, 1f));
        }
    }
}

class ForbiddenZones(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _shape = new(50, 10);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = new List<AOEInstance>(2)
        {
            new(_shape, Module.PrimaryActor.Position, 180f.Degrees()) // area behind boss
        };

        var pollens = Module.Enemies((uint)OID.Pollen);
        var pollen = pollens.Count != 0 ? pollens[0] : null;
        if (pollen != null && pollen.EventState == 0)
            aoes.Add(new(_shape, new(200f, -122f)));
        return CollectionsMarshal.AsSpan(aoes);
    }
}

class D162DemonWallStates : StateMachineBuilder
{
    public D162DemonWallStates(BossModule module) : base(module)
    {
        // note: no component for Murder Hole - there's not enough space to spread properly, and this hits for small damage
        TrivialPhase()
            .ActivateOnEnter<LiquefyCenter>()
            .ActivateOnEnter<LiquefySides>()
            .ActivateOnEnter<Repel>()
            .ActivateOnEnter<ForbiddenZones>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 14, NameID = 1694)]
public class D162DemonWall(WorldState ws, Actor primary) : BossModule(ws, primary, new(200f, -131f), new ArenaBoundsRect(10f, 21f));
