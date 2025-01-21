﻿namespace BossMod.Heavensward.Quest.MSQ.Heliodrome;

public enum OID : uint
{
    Boss = 0x195E,
    Helper = 0x233C,
    GrynewahtP2 = 0x195F, // R0.500, x0 (spawn during fight)
    ImperialColossus = 0x1966, // R3.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AugmentedUprising = 7608, // Boss->self, 3.0s cast, range 8+R 120-degree cone
    AugmentedSuffering = 7607, // Boss->self, 3.5s cast, range 6+R circle
    Heartstopper = 866, // _Gen_ImperialEques->self, 2.5s cast, range 3+R width 3 rect
    Overpower = 720, // _Gen_ImperialLaquearius->self, 2.1s cast, range 6+R 90-degree cone
    GrandSword = 7615, // _Gen_ImperialColossus->self, 3.0s cast, range 18+R 120-degree cone
    MagitekRay = 7617, // _Gen_ImperialColossus->location, 3.0s cast, range 6 circle
    GrandStrike = 7616, // _Gen_ImperialColossus->self, 2.5s cast, range 45+R width 4 rect
    ShrapnelShell = 7614, // GrynewahtP2->location, 2.5s cast, range 6 circle
    MagitekMissiles = 7612, // GrynewahtP2->location, 5.0s cast, range 15 circle

}

class MagitekMissiles(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MagitekMissiles), 15);
class ShrapnelShell(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ShrapnelShell), 6);
class Firebomb(BossModule module) : Components.PersistentVoidzone(module, 4, m => m.Enemies(0x1E86DF).Where(e => e.EventState != 7));

class Uprising(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AugmentedUprising), new AOEShapeCone(8.5f, 60.Degrees()));
class Suffering(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AugmentedSuffering), 6.5f);
class Heartstopper(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Heartstopper), new AOEShapeRect(3.5f, 1.5f));
class Overpower(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Overpower), new AOEShapeCone(6, 45.Degrees()));
class GrandSword(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.GrandSword), new AOEShapeCone(21, 60.Degrees()));
class MagitekRay(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MagitekRay), 6);
class GrandStrike(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.GrandStrike), new AOEShapeRect(48, 2));

class Adds(BossModule module) : Components.AddsMulti(module, [0x1960, 0x1961, 0x1962, 0x1963, 0x1964, 0x1965, 0x1966])
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = (OID)e.Actor.OID == OID.ImperialColossus ? 5 : e.Actor.TargetID == actor.InstanceID ? 1 : 0;
    }
}

class Bounds(BossModule module) : BossComponent(module)
{
    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID == 0x10000002)
            Arena.Bounds = new ArenaBoundsCircle(20);
    }
}

class ReaperAI(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor.MountId == 103 && WorldState.Actors.Find(actor.TargetID) is var target && target != null)
        {
            var aid = (OID)target.OID == OID.ImperialColossus ? Roleplay.AID.DiffractiveMagitekCannon : Roleplay.AID.MagitekCannon;
            hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, ActionQueue.Priority.High, targetPos: target.PosRot.XYZ());
        }
    }
}

class GrynewahtStates : StateMachineBuilder
{
    public GrynewahtStates(BossModule module) : base(module)
    {
        State build(uint id) => SimpleState(id, 10000, "Enrage")
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<Uprising>()
            .ActivateOnEnter<Suffering>()
            .ActivateOnEnter<Overpower>()
            .ActivateOnEnter<Heartstopper>()
            .ActivateOnEnter<GrandSword>()
            .ActivateOnEnter<MagitekRay>()
            .ActivateOnEnter<GrandStrike>()
            .ActivateOnEnter<ShrapnelShell>()
            .ActivateOnEnter<Firebomb>()
            .ActivateOnEnter<MagitekMissiles>();

        SimplePhase(1, id => build(id).ActivateOnEnter<Bounds>(), "P1")
            .Raw.Update = () => module.Enemies(OID.GrynewahtP2).Count != 0;
        DeathPhase(0x100, id => build(id).ActivateOnEnter<ReaperAI>().OnEnter(() =>
        {
            module.Arena.Bounds = Grynewaht.CircleBounds;
        }));
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 222, NameID = 5576)]
public class Grynewaht(WorldState ws, Actor primary) : BossModule(ws, primary, default, hexBounds)
{
    private static readonly ArenaBoundsComplex hexBounds = new([new Polygon(default, 10.675f, 6, 30.Degrees())]);
    public static readonly ArenaBoundsComplex CircleBounds = new([new Polygon(default, 20, 20)]);

    protected override bool CheckPull() => Raid.Player()!.InCombat;
}