namespace BossMod.Heavensward.Dungeon.D17BaelsarsWall.D173TheGriffin;

public enum OID : uint
{
    Boss = 0x193D, // R1.5
    RestraintCollar = 0x193E, // R1.0
    BladeOfTheGriffin = 0x193F, // R2.0
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Teleport = 7366, // Boss->location, no cast, single-target
    DullBlade = 7361, // Boss->player, no cast, single-target

    BeakOfTheGriffin = 7363, // Boss->self, 3.0s cast, range 80+R circle, raidwide
    FlashPowder = 7364, // Boss->self, 3.0s cast, range 80+R circle, gaze

    SanguineBlade = 7365, // Boss->self, 4.5s cast, range 40+R 180-degree cone
    ClawOfTheGriffin = 7362, // Boss->player, 4.0s cast, single-target, tankbuster
    GullDive = 7371, // BladeOfTheGriffin->self, no cast, range 80+R circle, tiny raidwides
    Lionshead = 7370, // Boss->location, 9.0s cast, range 80 circle, raidwide
    BigBoot = 7367, // Boss->player, 3.0s cast, single-target, knockback 15, away from source
    Corrosion = 7372, // BladeOfTheGriffin->self, 20.0s cast, range 9 circle
    RestraintCollarVisual = 7368, // Boss->player, 3.0s cast, single-target
    RestraintCollar = 7369 // RestraintCollar->self, 15.0s cast, ???
}

public enum IconID : uint
{
    Tankbuster = 198, // player
    Knockback = 22, // player
    Fetters = 1 // player
}

public enum SID : uint
{
    Fetters = 3421 // none->3DBA, extra=0xEC4
}

class RestraintCollar(BossModule module) : BossComponent(module)
{
    private bool chained;
    private bool chainsactive;
    private Actor? chaintarget;
    private bool casting;

    public override void Update()
    {
        var fetters = chaintarget?.FindStatus((uint)SID.Fetters) != null;
        if (fetters)
            chainsactive = true;
        if (fetters && !chained)
            chained = true;
        if (chaintarget != null && !fetters && !casting)
        {
            chained = false;
            chaintarget = null;
            chainsactive = false;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (chaintarget != null && !chainsactive)
            hints.Add($"{chaintarget.Name} is about to be fettered!");
        else if (chaintarget != null && chainsactive)
            hints.Add($"Destroy fetters on {chaintarget.Name}!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var collar = Module.Enemies((uint)OID.RestraintCollar);
        var ironchain = collar.Count != 0 ? collar[0] : null;
        if (ironchain != null && ironchain.IsTargetable && !ironchain.IsDead)
            hints.AddForbiddenZone(new SDInvertedCircle(ironchain.Position, ironchain.HitboxRadius + 2.6f));
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Fetters)
        {
            chaintarget = actor;
            casting = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RestraintCollar)
            casting = false;
    }
}

class BigBoot(BossModule module) : Components.GenericKnockback(module, (uint)AID.BigBoot, stopAtWall: true)
{
    private Actor? _target;
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_target != null && _target == actor)
            return new Knockback[1] { new(Module.PrimaryActor.Position, 15f, ignoreImmunes: true) };
        return [];
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Knockback)
            _target = actor;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.BigBoot)
            _target = null;
    }
}

class Corrosion(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(9f);
    private readonly List<AOEInstance> _aoes = new(9);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var blades = Module.Enemies((uint)OID.BladeOfTheGriffin);
        var countB = blades.Count;
        if (countB == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        if (countB < 9)
            return aoes;
        for (var i = 0; i < countB; ++i)
        {
            if (blades[i].IsDead)
                return aoes;
        }

        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Corrosion)
            _aoes.Add(new(circle, spell.LocXZ, default, Module.CastFinishAt(spell), actorID: caster.InstanceID));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Corrosion)
        {
            var count = _aoes.Count;
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

class SanguineBlade(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SanguineBlade, new AOEShapeCone(41.5f, 90f.Degrees()));
class ClawOfTheGriffin(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ClawOfTheGriffin);
class BeakOfTheGriffin(BossModule module) : Components.RaidwideCast(module, (uint)AID.BeakOfTheGriffin);
class Lionshead(BossModule module) : Components.RaidwideCast(module, (uint)AID.Lionshead);
class FlashPowder(BossModule module) : Components.CastGaze(module, (uint)AID.FlashPowder);

class D173TheGriffinStates : StateMachineBuilder
{
    public D173TheGriffinStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RestraintCollar>()
            .ActivateOnEnter<BigBoot>()
            .ActivateOnEnter<Corrosion>()
            .ActivateOnEnter<SanguineBlade>()
            .ActivateOnEnter<ClawOfTheGriffin>()
            .ActivateOnEnter<BeakOfTheGriffin>()
            .ActivateOnEnter<FlashPowder>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 219, NameID = 5564)]
public class D173TheGriffin(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(352.07f, 372.3f), new(358.25f, 373.34f), new(363.71f, 376.21f), new(367.87f, 380.41f), new(370.04f, 386.28f),
    new(370.04f, 396.84f), new(367.89f, 403.16f), new(367.58f, 403.64f), new(363.52f, 407.71f), new(357.95f, 410.5f),
    new(351.78f, 411.41f), new(345.79f, 410.4f), new(340.31f, 407.53f), new(336.34f, 403.55f), new(336.04f, 403.15f),
    new(333.33f, 397.71f), new(332.43f, 391.68f), new(333.47f, 385.6f), new(336.27f, 380.21f), new(340.49f, 376),
    new(346.04f, 373.23f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.RestraintCollar), Colors.Vulnerable);
        Arena.Actors(Enemies((uint)OID.BladeOfTheGriffin));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID == (uint)OID.BladeOfTheGriffin ? e.Actor.Position.AlmostEqual(Arena.Center, 5f) ? 2 : AIHints.Enemy.PriorityPointless
            : e.Actor.OID == (uint)OID.RestraintCollar ? 2 : 1;
        }
    }
}
