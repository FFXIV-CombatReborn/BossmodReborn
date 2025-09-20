namespace BossMod.Endwalker.VariantCriterion.C02AMR.C020Trash1;

public enum OID : uint
{
    NRaiko = 0x3F94, // R4.500, x1
    NFurutsubaki = 0x3F95, // R1.440, x2
    NFuko = 0x3F96, // R4.500, x1
    NPenghou = 0x3F97, // R1.680, x2
    NYuki = 0x3FA0, // R2.000, x1

    SRaiko = 0x3F9A, // R4.500, x1
    SFurutsubaki = 0x3F9B, // R1.440, x2
    SFuko = 0x3F9C, // R4.500, x1
    SPenghou = 0x3F9D, // R1.680, x2
    SYuki = 0x3FA2, // R2.000, x1

    ResetHelper = 0x1E8FB8 // R2.000, x3, EventObj type
}

public enum AID : uint
{
    AutoAttack = 31320, // *Raiko/*Furutsubaki/*Fuko/*Penghou/*Yuki->player, no cast, single-target
    // raiko
    NBarrelingSmash = 34387, // NRaiko->player, 4.0s cast, width 7 rect charge
    NHowl = 34388, // NRaiko->self, 4.0s cast, range 60 circle
    NMasterOfLevin = 34389, // NRaiko->self, 4.0s cast, range 5-30 donut
    NDisciplesOfLevin = 34390, // NRaiko->self, 4.0s cast, range 10 circle
    NBloodyCaress = 34391, // NFurutsubaki->self, 3.0s cast, range 12 120-degree cone
    SBarrelingSmash = 34405, // SRaiko->player, 4.0s cast, width 7 rect charge
    SHowl = 34406, // SRaiko->self, 4.0s cast, range 60 circle
    SMasterOfLevin = 34407, // SRaiko->self, 4.0s cast, range 5-30 donut
    SDisciplesOfLevin = 34408, // SRaiko->self, 4.0s cast, range 10 circle
    SBloodyCaress = 34409, // SFurutsubaki->self, 3.0s cast, range 12 120-degree cone
    // fuko
    NTwister = 34392, // NFuko->players, 5.0s cast, range 8 circle stack
    NCrosswind = 34393, // NFuko->self, 4.0s cast, range 60 circle knockback 25
    NScytheTail = 34394, // NFuko->self, 4.0s cast, range 10 circle
    NTornado = 34395, // NPenghou->location, 3.0s cast, range 6 circle
    STwister = 34410, // SFuko->players, 5.0s cast, range 8 circle stack
    SCrosswind = 34411, // SFuko->self, 4.0s cast, range 60 circle knockback 25
    SScytheTail = 34412, // SFuko->self, 4.0s cast, range 10 circle
    STornado = 34413, // SPenghou->location, 3.0s cast, range 6 circle
    // yuki
    NRightSwipe = 34437, // NYuki->self, 4.0s cast, range 60 180-degree cone
    NLeftSwipe = 34438, // NYuki->self, 4.0s cast, range 60 180-degree cone
    SRightSwipe = 34440, // SYuki->self, 4.0s cast, range 60 180-degree cone
    SLeftSwipe = 34441 // SYuki->self, 4.0s cast, range 60 180-degree cone
}

abstract class BloodyCaress(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(12f, 60f.Degrees()))
{
    public override bool KeepOnPhaseChange => true;
}
sealed class NBloodyCaress(BossModule module) : BloodyCaress(module, (uint)AID.NBloodyCaress);
sealed class SBloodyCaress(BossModule module) : BloodyCaress(module, (uint)AID.SBloodyCaress);

abstract class DisciplesOfLevin(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 10f)
{
    public override bool KeepOnPhaseChange => true;
}
sealed class NDisciplesOfLevin(BossModule module) : DisciplesOfLevin(module, (uint)AID.NDisciplesOfLevin);
sealed class SDisciplesOfLevin(BossModule module) : DisciplesOfLevin(module, (uint)AID.SDisciplesOfLevin);

abstract class BarrelingSmash(BossModule module, uint aid) : Components.BaitAwayChargeCast(module, aid, 3.5f)
{
    public override bool KeepOnPhaseChange => true;
}
sealed class NBarrelingSmash(BossModule module) : BarrelingSmash(module, (uint)AID.NBarrelingSmash);
sealed class SBarrelingSmash(BossModule module) : BarrelingSmash(module, (uint)AID.SBarrelingSmash);

abstract class Howl(BossModule module, uint aid) : Components.RaidwideCast(module, aid)
{
    public override bool KeepOnPhaseChange => true;
}
sealed class NHowl(BossModule module) : Howl(module, (uint)AID.NHowl);
sealed class SHowl(BossModule module) : Howl(module, (uint)AID.SHowl);

abstract class MasterOfLevin(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeDonut(5f, 30f))
{
    public override bool KeepOnPhaseChange => true;
}
sealed class NMasterOfLevin(BossModule module) : MasterOfLevin(module, (uint)AID.NMasterOfLevin);
sealed class SMasterOfLevin(BossModule module) : MasterOfLevin(module, (uint)AID.SMasterOfLevin);

abstract class Swipe(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(60f, 90f.Degrees()))
{
    public override bool KeepOnPhaseChange => true;
}
sealed class NRightSwipe(BossModule module) : Swipe(module, (uint)AID.NRightSwipe);
sealed class SRightSwipe(BossModule module) : Swipe(module, (uint)AID.SRightSwipe);
sealed class NLeftSwipe(BossModule module) : Swipe(module, (uint)AID.NLeftSwipe);
sealed class SLeftSwipe(BossModule module) : Swipe(module, (uint)AID.SLeftSwipe);

abstract class Tornado(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 6f)
{
    public override bool KeepOnPhaseChange => true;
}
sealed class NTornado(BossModule module) : Tornado(module, (uint)AID.NTornado);
sealed class STornado(BossModule module) : Tornado(module, (uint)AID.STornado);

abstract class ScytheTail(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 10f)
{
    public override bool KeepOnPhaseChange => true;
}
sealed class NScytheTail(BossModule module) : ScytheTail(module, (uint)AID.NScytheTail);
sealed class SScytheTail(BossModule module) : ScytheTail(module, (uint)AID.SScytheTail);

abstract class Twister(BossModule module, uint aid) : Components.StackWithCastTargets(module, aid, 8f, 4, 4)
{
    public override bool KeepOnPhaseChange => true;
}
sealed class NTwister(BossModule module) : Twister(module, (uint)AID.NTwister);
sealed class STwister(BossModule module) : Twister(module, (uint)AID.STwister);

abstract class Crosswind(BossModule module, uint aid) : Components.SimpleKnockbacks(module, aid, 25f)
{
    public override bool KeepOnPhaseChange => true;
}
sealed class NCrosswind(BossModule module) : Crosswind(module, (uint)AID.NCrosswind);
sealed class SCrosswind(BossModule module) : Crosswind(module, (uint)AID.SCrosswind);

sealed class C020NTrash1States(Trash1Arena module) : C020Trash1States(module, false);
sealed class C020STrash1States(Trash1Arena module) : C020Trash1States(module, true);

abstract class C020Trash1States : StateMachineBuilder
{
    private readonly bool _savage;
    private readonly Trash1Arena _module;

    public C020Trash1States(Trash1Arena module, bool savage) : base(module)
    {
        _savage = savage;
        _module = module;
        SimplePhase(0, Raiko, "")
            .ActivateOnEnter<NTornado>(!savage) // fuko
            .ActivateOnEnter<NScytheTail>(!savage)
            .ActivateOnEnter<NTwister>(!savage)
            .ActivateOnEnter<NCrosswind>(!savage)
            .ActivateOnEnter<STornado>(savage)
            .ActivateOnEnter<SScytheTail>(savage)
            .ActivateOnEnter<STwister>(savage)
            .ActivateOnEnter<SCrosswind>(savage)
            .ActivateOnEnter<NBloodyCaress>(!savage) // raiko
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
            .ActivateOnEnter<SLeftSwipe>(savage)
            .Raw.Update = () => (_module.Raiko()?.IsDeadOrDestroyed ?? true) || _module.PrimaryActor.IsDeadOrDestroyed;
        DeathPhase(1, Fuko)
            .Raw.Update = () => AllDeadOrDestroyed(savage ? Trash1Arena.TrashSavage : Trash1Arena.TrashNormal);
    }

    private void Raiko(uint id)
    {
        DisciplesOfLevin(id, 5.3f);
        BarrelingSmashHowl(id + 0x10000, 6.1f);
        MasterOfLevin(id + 0x20000, 7.6f);
        BarrelingSmashHowl(id + 0x30000, 6.5f);
        SimpleState(id + 0xFF0000, 10f, "???");
    }

    private void DisciplesOfLevin(uint id, float delay)
    {
        ActorCast(id, _module.Raiko, _savage ? (uint)AID.SDisciplesOfLevin : (uint)AID.NDisciplesOfLevin, delay, 4f, false, "Out");
    }

    private void BarrelingSmashHowl(uint id, float delay)
    {
        ActorCast(id, _module.Raiko, _savage ? (uint)AID.SBarrelingSmash : (uint)AID.NBarrelingSmash, delay, 4f, false, "Charge");
        ActorCast(id + 0x1000, _module.Raiko, _savage ? (uint)AID.SHowl : (uint)AID.NHowl, 2.1f, 4, false, "Raidwide");
    }

    private void MasterOfLevin(uint id, float delay)
    {
        ActorCast(id, _module.Raiko, _savage ? (uint)AID.SMasterOfLevin : (uint)AID.NMasterOfLevin, delay, 4f, false, "In");
    }

    private void Fuko(uint id)
    {
        ScytheTail(id, 5.7f);
        Twister(id + 0x10000, 2.1f);
        Crosswind(id + 0x20000, 12.0f);
        ScytheTail(id + 0x30000, 5.8f);
        Twister(id + 0x40000, 2.1f);
        Crosswind(id + 0x50000, 10.4f);
        ScytheTail(id + 0x60000, 6.0f);
        Twister(id + 0x70000, 4.1f);
        SimpleState(id + 0xFF0000, 10, "???");
    }

    private void ScytheTail(uint id, float delay)
    {
        ActorCast(id, _module.Fuko, _savage ? (uint)AID.SScytheTail : (uint)AID.NScytheTail, delay, 4f, false, "Out");
    }

    private void Twister(uint id, float delay)
    {
        ActorCast(id, _module.Fuko, _savage ? (uint)AID.STwister : (uint)AID.NTwister, delay, 5f, false, "Stack");
    }

    private void Crosswind(uint id, float delay)
    {
        ActorCast(id, _module.Raiko, _savage ? (uint)AID.SCrosswind : (uint)AID.NCrosswind, delay, 4f, false, "Knockback");
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.NYuki, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 946, NameID = 12425, SortOrder = 1)]
public sealed class C020NTrash1(WorldState ws, Actor primary) : Trash1Arena(ws, primary, false);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.SYuki, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 947, NameID = 12425, SortOrder = 1)]
public sealed class C020STrash1(WorldState ws, Actor primary) : Trash1Arena(ws, primary, true);

public abstract class Trash1Arena(WorldState ws, Actor primary, bool savage) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Square(default, 19.5f), new Rectangle(new(default, -20f), 4.5f, 9.5f)]);
    public static readonly uint[] TrashNormal = [(uint)OID.NYuki, (uint)OID.NFuko, (uint)OID.NFurutsubaki, (uint)OID.NRaiko, (uint)OID.NPenghou];
    public static readonly uint[] TrashSavage = [(uint)OID.SYuki, (uint)OID.SFuko, (uint)OID.SFurutsubaki, (uint)OID.SRaiko, (uint)OID.SPenghou];

    protected override bool CheckPull() => !Raid.Player()!.IsDead && IsAnyActorInCombat(savage ? TrashSavage : TrashNormal);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, savage ? TrashSavage : TrashNormal);
    }

    private Actor? _raiko;
    private Actor? _fuko;
    public Actor? Raiko() => _raiko;
    public Actor? Fuko() => _fuko;

    protected override void UpdateModule()
    {
        _raiko ??= GetActor(savage ? (uint)OID.SRaiko : (uint)OID.NRaiko);
        _fuko ??= GetActor(savage ? (uint)OID.SFuko : (uint)OID.NFuko);
    }
}
