namespace BossMod.Stormblood.Quest.MSQ.ARequiemForHeroes;

class StormUnbound(BossModule module) : Components.Exaflare(module, 5)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TheStormUnboundCast)
            Lines.Add(new(caster.Position, 5f * caster.Rotation.ToDirection(), Module.CastFinishAt(spell), 1d, 4, 2));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.TheStormUnboundCast or (uint)AID.TheStormUnboundRepeat)
        {
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                        Lines.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

class LightlessSpark2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightlessSparkAdds, new AOEShapeCone(40f, 45f.Degrees()));

class ArtOfTheStorm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArtOfTheStorm, 8f);
class EntropicFlame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EntropicFlame, new AOEShapeRect(50f, 4f));

class FloodOfDarkness(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FloodOfDarkness, 6f);
class VeinSplitter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VeinSplitter, 10f);
class LightlessSpark(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightlessSpark, new AOEShapeCone(40f, 45f.Degrees()));
class SwellUnbound(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheSwellUnbound, new AOEShapeDonut(8f, 20f));
class Swell(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.ArtOfTheSwell, 8f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
            hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center, 8f));
    }
}

abstract class ArtOfTheSword(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeRect(40f, 3f));
class ArtOfTheSword1(BossModule module) : ArtOfTheSword(module, (uint)AID.ArtOfTheSword1);
class ArtOfTheSword2(BossModule module) : ArtOfTheSword(module, (uint)AID.ArtOfTheSword2);
class ArtOfTheSword3(BossModule module) : ArtOfTheSword(module, (uint)AID.ArtOfTheSword3);

class DarkAether(BossModule module) : Components.Voidzone(module, 1.5f, GetVoidzones, 3f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.DarkAether);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (!z.IsDead)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

class Adds(BossModule module) : Components.AddsMulti(module, [(uint)OID.TheStorm, (uint)OID.TheSwell, (uint)OID.AmeNoHabakiri]);

public class ZenosP2States : StateMachineBuilder
{
    public ZenosP2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FloodOfDarkness>()
            .ActivateOnEnter<VeinSplitter>()
            .ActivateOnEnter<LightlessSpark>()
            .ActivateOnEnter<LightlessSpark2>()
            .ActivateOnEnter<SwellUnbound>()
            .ActivateOnEnter<Swell>()
            .ActivateOnEnter<ArtOfTheSword1>()
            .ActivateOnEnter<ArtOfTheSword2>()
            .ActivateOnEnter<ArtOfTheSword3>()
            .ActivateOnEnter<ArtOfTheStorm>()
            .ActivateOnEnter<EntropicFlame>()
            .ActivateOnEnter<DarkAether>()
            .ActivateOnEnter<StormUnbound>()
            .ActivateOnEnter<Adds>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68721, NameID = 6039, PrimaryActorOID = (uint)OID.BossP2)]
public class ZenosP2(WorldState ws, Actor primary) : BossModule(ws, primary, new(233, -93.25f), new ArenaBoundsCircle(20f));
