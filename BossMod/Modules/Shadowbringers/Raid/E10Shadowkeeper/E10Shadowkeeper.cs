namespace BossMod.Shadowbringers.Raid.E10Shadowkeeper;

public enum OID : uint
{
    Boss = 0x3062, // R6.300, Shadowkeeper
    Helper = 0x233C, // R0.500
    ShadowSlash = 0x3061, // R6.300, left/right shadow
    ShadowImplosion = 0x3063, // R6.300, forward/backward shadow
    ShadowClone = 0x3064, // R4.500, warrior / barbs / cloak
    Voidgate = 0x3065, // R1.000
}

public enum AID : uint
{
    AutoAttack = 23305, // Boss->player, no cast, single-target
    BossMove = 23304, // Boss->location, no cast, single-target

    ForwardImplosion = 22196, // Boss->self, 6.0s cast, range 70 180-degree cone
    ForwardShadowImplosion = 22197, // ShadowImplosion->self, 6.0s cast, range 70 180-degree cone
    ForwardShadowImplosionVis = 22198, // Boss->self, 6.0s cast, single-target
    BackwardImplosion = 22199, // Boss->self, 6.0s cast, range 70 180-degree cone
    BackwardShadowImplosion = 22200, // ShadowImplosion->self, 6.0s cast, range 70 180-degree cone
    BackwardShadowImplosionVis = 22201, // Boss->self, 6.0s cast, single-target
    ForwardShadowImplosionVis2 = 22208, // Boss->self, 6.0s cast, single-target
    ForwardShadowImplosionHit2 = 22209, // Boss->self, no cast, single-target

    DeepshadowNova = 22245, // Boss->self, 5.0s cast, single-target
    DeepshadowNovaAOE = 22247, // Helper->self, 5.5s cast, range 80 circle

    SpawnShadow = 22230, // Boss->self, 4.0s cast, single-target
    ShadowJump = 22231, // ShadowClone->location, no cast, single-target
    ShadowJump2 = 22232, // ShadowClone->location, no cast, single-target
    ShadowWarrior = 22242, // Boss->self, 3.0s cast, single-target

    BarbsOfAgonyVis = 22339, // ShadowClone->self, 5.0s cast, single-target
    BarbsOfAgony = 22233, // ShadowClone->self, 6.0s cast, range 70 cone
    BarbsOfAgonyShadow = 23334, // ShadowImplosion->self, 6.0s cast, range 60 180-degree cone

    FadeToShadow = 22234, // Boss->self, 4.0s cast, single-target
    CloakOfShadows = 22235, // ShadowClone->self, 5.0s cast, single-target
    CloakOfShadowsAOE = 23313, // Helper->self, 7.5s cast, range 20 circle

    ThroneOfShadow = 22215, // Helper->self, 6.5s cast, range 16 circle
    DistantScream = 22214, // Helper->self, 5.5s cast, range 80 circle (knockback)

    UmbraSmash = 23467, // Boss->player, 4.0s cast, single-target
    UmbraSmashHit = 22228, // Boss->player, no cast, single-target
    ShadowsEdgeVis = 23307, // Boss->self, 4.0s cast, single-target
    ShadowsEdge = 22229, // Boss->player, no cast, range 50 width 8 rect

    LeftGigaSlash = 22193, // Boss->self, 6.0s cast, range 70 180-degree cone
    RightGigaSlash = 22190, // Boss->self, 6.0s cast, range 70 180-degree cone
    LeftShadowSlash = 22194, // ShadowSlash->self, 6.0s cast, range 70 180-degree cone
    RightShadowSlash = 22191, // ShadowSlash->self, 6.0s cast, range 70 180-degree cone
    LeftShadowSlashVis = 22195, // Boss->self, 6.0s cast, single-target
    LeftShadowSlashVis2 = 22205, // Boss->self, 6.0s cast, single-target
    LeftShadowSlashHit2 = 22206, // Boss->self, no cast, single-target
    RightShadowSlashVis = 22192, // Boss->self, 6.0s cast, single-target
    RightShadowSlashVis2 = 22202, // Boss->self, 6.0s cast, single-target
    RightShadowSlashHit2 = 22203, // Boss->self, no cast, single-target

    Voidgate = 22237, // Boss->self, 4.0s cast, single-target
    VoidPulse = 22238, // Helper->self, 4.0s cast, range 100 circle
    VoidgateResolve = 22220, // Helper->self, no cast
    VoidgateResolve2 = 23309, // Helper->self, no cast

    ShadowyEruption = 22239, // Boss->self, 3.0s cast, single-target
    ShadowyEruptionAlt = 22240, // Boss->self, 3.0s cast, single-target
    ShadowyEruptionAOE = 22241, // Helper->self, 3.0s cast, range 6 circle
}

public enum TetherID : uint
{
    ShadowWarrior = 131, // Boss<->ShadowClone
}

sealed class DeepshadowNova(BossModule module) : Components.RaidwideCast(module, (uint)AID.DeepshadowNovaAOE);
sealed class VoidPulse(BossModule module) : Components.RaidwideCast(module, (uint)AID.VoidPulse);
sealed class DistantScream(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.DistantScream, 12);
sealed class ThroneOfShadow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThroneOfShadow, 16);
sealed class CloakOfShadows(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CloakOfShadowsAOE, 20);
sealed class ShadowyEruption(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ShadowyEruptionAOE, 6);
sealed class UmbraSmash(BossModule module) : Components.SingleTargetCast(module, (uint)AID.UmbraSmash);

sealed class Implosions(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ForwardImplosion, (uint)AID.BackwardImplosion, (uint)AID.ForwardShadowImplosion, (uint)AID.BackwardShadowImplosion], new AOEShapeCone(70, 90.Degrees()));
sealed class GigaSlash(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LeftGigaSlash, (uint)AID.RightGigaSlash, (uint)AID.LeftShadowSlash, (uint)AID.RightShadowSlash], new AOEShapeCone(70, 90.Degrees()));
sealed class BarbsOfAgony(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BarbsOfAgony, (uint)AID.BarbsOfAgonyShadow], new AOEShapeCone(70, 90.Degrees()));

sealed class ShadowsEdge(BossModule module) : Components.GenericBaitAway(module, (uint)AID.ShadowsEdgeVis)
{
    private static readonly AOEShapeRect _shape = new(50, 4);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction && WorldState.Actors.Find(Module.PrimaryActor.TargetID) is { } target)
            CurrentBaits.Add(new(caster, target, _shape, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            for (var i = CurrentBaits.Count - 1; i >= 0; --i)
            {
                if (CurrentBaits[i].Source == caster)
                    CurrentBaits.RemoveAt(i);
            }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ShadowsEdge)
            CurrentBaits.Clear();
    }
}

sealed class Voidgate(BossModule module) : Components.GenericTowers(module, (uint)AID.VoidPulse)
{
    public override void OnMapEffect(byte index, uint state)
    {
        var pos = TowerPos(index);
        if (pos == default)
            return;
        if (state == 0x00020001)
            Towers.Add(new(pos, 2.5f, minSoakers: 2, maxSoakers: 2, activation: WorldState.FutureTime(14.5f)));
        else if (state is 0x00080004 or 0x40000004)
            for (var i = Towers.Count - 1; i >= 0; --i)
            {
                if (Towers[i].Position.AlmostEqual(pos, 1f))
                    Towers.RemoveAt(i);
            }
    }

    // these stupid things are mapeffects. they're about 8.5y from the middle and are indeces of a clock
    private WPos TowerPos(byte index)
    {
        if (index is < 2 or > 7)
            return default;
        var start = Module.Center + new WDir(0, -8.5f);
        return Module.Center + (start - Module.Center).Rotate((-45f * index).Degrees());
    }
}

sealed class E10ShadowkeeperStates : StateMachineBuilder
{
    public E10ShadowkeeperStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DeepshadowNova>()
            .ActivateOnEnter<Implosions>()
            .ActivateOnEnter<GigaSlash>()
            .ActivateOnEnter<BarbsOfAgony>()
            .ActivateOnEnter<CloakOfShadows>()
            .ActivateOnEnter<ShadowyEruption>()
            .ActivateOnEnter<ThroneOfShadow>()
            .ActivateOnEnter<DistantScream>()
            .ActivateOnEnter<UmbraSmash>()
            .ActivateOnEnter<ShadowsEdge>()
            .ActivateOnEnter<Voidgate>()
            .ActivateOnEnter<VoidPulse>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "croizat (ported from upstream and modified by LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 747, NameID = 9769)]
public sealed class E10Shadowkeeper(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.ShadowClone), Colors.Object);
        Arena.Actors(Enemies((uint)OID.ShadowImplosion), Colors.Object);
        Arena.Actors(Enemies((uint)OID.ShadowSlash), Colors.Object);
    }
}
