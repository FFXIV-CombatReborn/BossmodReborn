﻿namespace BossMod.Endwalker.VariantCriterion.C02AMR.C020Trash1;

abstract class BloodyCaress(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(12, 60.Degrees()));
class NBloodyCaress(BossModule module) : BloodyCaress(module, AID.NBloodyCaress);
class SBloodyCaress(BossModule module) : BloodyCaress(module, AID.SBloodyCaress);

abstract class DisciplesOfLevin(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 10);
class NDisciplesOfLevin(BossModule module) : DisciplesOfLevin(module, AID.NDisciplesOfLevin);
class SDisciplesOfLevin(BossModule module) : DisciplesOfLevin(module, AID.SDisciplesOfLevin);

abstract class BarrelingSmash(BossModule module, AID aid) : Components.BaitAwayChargeCast(module, ActionID.MakeSpell(aid), 3.5f);
class NBarrelingSmash(BossModule module) : BarrelingSmash(module, AID.NBarrelingSmash);
class SBarrelingSmash(BossModule module) : BarrelingSmash(module, AID.SBarrelingSmash);

abstract class Howl(BossModule module, AID aid) : Components.RaidwideCast(module, ActionID.MakeSpell(aid));
class NHowl(BossModule module) : Howl(module, AID.NHowl);
class SHowl(BossModule module) : Howl(module, AID.SHowl);

abstract class MasterOfLevin(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeDonut(5, 30));
class NMasterOfLevin(BossModule module) : MasterOfLevin(module, AID.NMasterOfLevin);
class SMasterOfLevin(BossModule module) : MasterOfLevin(module, AID.SMasterOfLevin);

class C020RaikoStates : StateMachineBuilder
{
    private readonly bool _savage;

    public C020RaikoStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<NBloodyCaress>(!savage)
            .ActivateOnEnter<NDisciplesOfLevin>(!savage)
            .ActivateOnEnter<NBarrelingSmash>(!savage)
            .ActivateOnEnter<NHowl>(!savage)
            .ActivateOnEnter<NMasterOfLevin>(!savage)
            .ActivateOnEnter<SBloodyCaress>(savage)
            .ActivateOnEnter<SDisciplesOfLevin>(savage)
            .ActivateOnEnter<SBarrelingSmash>(savage)
            .ActivateOnEnter<SHowl>(savage)
            .ActivateOnEnter<SMasterOfLevin>(savage)
            .ActivateOnEnter<NRightSwipe>(!savage) // for yuki
            .ActivateOnEnter<NLeftSwipe>(!savage)
            .ActivateOnEnter<SRightSwipe>(savage)
            .ActivateOnEnter<SLeftSwipe>(savage);
    }

    private void SinglePhase(uint id)
    {
        DisciplesOfLevin(id, 5.3f);
        BarrelingSmashHowl(id + 0x10000, 6.1f);
        MasterOfLevin(id + 0x20000, 7.6f);
        BarrelingSmashHowl(id + 0x30000, 6.5f);
        SimpleState(id + 0xFF0000, 10, "???");
    }

    private void DisciplesOfLevin(uint id, float delay)
    {
        Cast(id, _savage ? AID.SDisciplesOfLevin : AID.NDisciplesOfLevin, delay, 4, "Out");
    }

    private void BarrelingSmashHowl(uint id, float delay)
    {
        Cast(id, _savage ? AID.SBarrelingSmash : AID.NBarrelingSmash, delay, 4, "Charge");
        Cast(id + 0x1000, _savage ? AID.SHowl : AID.NHowl, 2.1f, 4, "Raidwide");
    }

    private void MasterOfLevin(uint id, float delay)
    {
        Cast(id, _savage ? AID.SMasterOfLevin : AID.NMasterOfLevin, delay, 4, "In");
    }
}
class C020NRaikoStates(BossModule module) : C020RaikoStates(module, false);
class C020SRaikoStates(BossModule module) : C020RaikoStates(module, true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NRaiko, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 946, NameID = 12422, SortOrder = 1)]
public class C020NRaiko(WorldState ws, Actor primary) : C020Trash1(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.NFurutsubaki));
        Arena.Actors(Enemies(OID.NYuki));
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SRaiko, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 947, NameID = 12422, SortOrder = 1)]
public class C020SRaiko(WorldState ws, Actor primary) : C020Trash1(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.SFurutsubaki));
        Arena.Actors(Enemies(OID.SYuki));
    }
}
