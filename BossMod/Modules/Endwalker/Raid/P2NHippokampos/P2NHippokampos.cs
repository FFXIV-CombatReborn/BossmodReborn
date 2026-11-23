namespace BossMod.Endwalker.Raid.P2NHippokampos;

public enum OID : uint
{
    Boss = 0x3599, // R9.002, x1
    Helper = 0x233C, // R0.500, x24, Helper type
    CataractHead = 0x359A, // R8.002, x1
    DissociatedHead = 0x3869, // R9.002, x1
    Actor1EA1A1 = 0x1EA1A1, // R2.000, x1, EventObj type
    Exit = 0x1E850B, // R0.500, x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    SewageDeluge = 26614, // Boss->self, 5.0s cast, range 60 circle
    SpokenCataractHead3 = 26615, // CataractHead->self, 7.0s cast, range 60 180-degree cone
    SpokenCataractHead1 = 26616, // CataractHead->self, 7.0s cast, range 60 180-degree cone
    SpokenCataractHead2 = 26617, // CataractHead->self, 7.0s cast, range 60 180-degree cone
    SpokenCataractBoss = 26621, // Boss->self, 7.0s cast, range 100 width 15 rect
    SpokenCataractBoss2 = 26622, // Boss->self, 7.0s cast, range 100 width 15 rect (same as 26621 in the sheet; not seen in any log yet)
    CoherenceFlare = 26624, // CataractHead->location, no cast, range 60 circle with falloff
    Coherence = 26625, // Boss->self, 5.0s cast, single-target
    CoherenceRay = 26626, // Boss->self, no cast, range 60 width 6 rect, shared
    CoherenceEnd = 26627, // Boss->self, no cast, single-target
    Dissociation = 26628, // Boss->self, 4.0s cast, single-target
    DissociationEnd = 26629, // Boss->self, no cast, single-target
    DissociationAOE = 26630, // DissociatedHead->self, 8.0s cast, range 50 width 20 rect
    Shockwave = 26631, // Boss->location, 6.0s cast, range 60 circle, knockback
    TaintedFlood = 26632, // Boss->self, 3.0s cast, single-target
    TaintedFloodAOE = 26633, // Helper->players, 5.2s cast, range 6 circle
    PredatorySight = 26634, // Boss->self, 4.0s cast, single-target
    PredatorySightMark = 26635, // Helper->player, 5.0s cast, single-target, pair stack
    SewageEruptionVisual = 26636, // Boss->self, 4.0s cast, single-target
    SewageEruption = 26637, // Helper->location, 4.0s cast, range 6 circle
    DoubledImpact = 26638, // Boss->players, 5.0s cast, range 6 circle
    MurkyDepths = 26639, // Boss->self, 5.0s cast, range 60 circle
    Teleport = 26677, // Boss->location, no cast, single-target
    CoherenceRayTarget = 27924, // Helper->player, no cast, single-target
}

public enum SID : uint
{
    MarkOfEasyPrey = 2767, // Helper->player, extra=0x0
}

public enum IconID : uint
{
    PredatorySight = 55, // player
    CoherenceFlare = 87, // player
    DoubledImpact = 259, // player
    TaintedFlood = 310, // player
}

sealed class SpokenCataractHead3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpokenCataractHead3, new AOEShapeCone(60, 90.Degrees()));
sealed class SpokenCataractHead1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpokenCataractHead1, new AOEShapeCone(60, 90.Degrees()));
sealed class SpokenCataractHead2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpokenCataractHead2, new AOEShapeCone(60, 90.Degrees()));
sealed class SpokenCataractBoss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpokenCataractBoss, new AOEShapeRect(100, 7.5f));
sealed class SpokenCataractBoss2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpokenCataractBoss2, new AOEShapeRect(100, 7.5f));
sealed class DissociationAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DissociationAOE, new AOEShapeRect(50, 10));
sealed class TaintedFlood(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.TaintedFloodAOE, 6);
sealed class DoubledImpact(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.DoubledImpact, 6);

// TODO: flare radius is estimated from one cast, 21.5k at 8y against about 10k past 17y
sealed class CoherenceFlare(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.CoherenceFlare, (uint)AID.CoherenceFlare, 15, 5.8f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.CoherenceFlare)
            Spreads.Clear();
    }
}

sealed class CoherenceRay(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.CoherenceRayTarget, (uint)AID.CoherenceRay, 8.3d, 60f, 3f)
{
    private ulong _flareTarget;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.CoherenceFlare)
            _flareTarget = actor.InstanceID;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        // the flare target must stay out of the line stack
        if (spell.Action.ID == AidMarker && Raid.FindSlot(_flareTarget) is var slot && slot >= 0)
        {
            var count = CurrentBaits.Count;
            for (var i = 0; i < count; ++i)
                CurrentBaits.Ref(i).Forbidden.Set(slot);
        }
    }
}

sealed class PredatorySight(BossModule module) : Components.StackTogether(module, (uint)IconID.PredatorySight, 4.9f, radius: 3);

// TODO: distance is estimated, players near the landing spot ended 13-16y away
sealed class Shockwave(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Shockwave, 15);
sealed class SewageEruption(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SewageEruption, 6);
sealed class SewageDeluge(BossModule module) : Components.RaidwideCast(module, (uint)AID.SewageDeluge);

sealed class SewageDelugeArena(BossModule module) : BossComponent(module)
{
    private static readonly ArenaBoundsRect _square = P2NHippokampos.DefaultBounds;
    private static readonly ArenaBoundsCustom _flooded = BuildFlooded();

    private static ArenaBoundsCustom BuildFlooded()
    {
        static Rectangle rect(float x, float z, float hx, float hz) => new(new(100f + x, 100f + z), hx, hz);
        Rectangle[] pieces =
        [
            rect(9.625f, 9.625f, 4.375f, 4.375f), rect(-9.625f, 9.625f, 4.375f, 4.375f), rect(9.625f, -9.625f, 4.375f, 4.375f), rect(-9.625f, -9.625f, 4.375f, 4.375f),
            rect(0, 9.5f, 5.5f, 2), rect(0, -9.5f, 5.5f, 2), rect(9.5f, 0, 2, 5.5f), rect(-9.5f, 0, 2, 5.5f),
        ];
        return new(pieces);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0)
            return;
        switch (state)
        {
            case 0x00200010:
                Arena.Bounds = _flooded;
                break;
            case 0x00080004:
            case 0x00040004:
                Arena.Bounds = _square;
                break;
        }
    }
}
sealed class MurkyDepths(BossModule module) : Components.RaidwideCast(module, (uint)AID.MurkyDepths);

sealed class P2NHippokamposStates : StateMachineBuilder
{
    public P2NHippokamposStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SpokenCataractHead3>()
            .ActivateOnEnter<SpokenCataractHead1>()
            .ActivateOnEnter<SpokenCataractHead2>()
            .ActivateOnEnter<SpokenCataractBoss>()
            .ActivateOnEnter<SpokenCataractBoss2>()
            .ActivateOnEnter<DissociationAOE>()
            .ActivateOnEnter<TaintedFlood>()
            .ActivateOnEnter<DoubledImpact>()
            .ActivateOnEnter<CoherenceFlare>()
            .ActivateOnEnter<CoherenceRay>()
            .ActivateOnEnter<SewageDeluge>()
            .ActivateOnEnter<PredatorySight>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<SewageEruption>()
            .ActivateOnEnter<SewageDelugeArena>()
            .ActivateOnEnter<MurkyDepths>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Nonu (ported from upstream and modified by LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 810, NameID = 10348)]
public sealed class P2NHippokampos(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), DefaultBounds)
{
    public static readonly ArenaBoundsRect DefaultBounds = new(17.5f, 22.5f);
}
