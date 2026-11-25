
namespace BossMod.Shadowbringers.Raid.E05Ramuh.E05Ramuh;

public enum OID : uint
{
    Ramuh = 0x2D02, // R4.180, x1
    Helper = 0x233C, // R0.500, x32, Helper type
    _Gen_Actor1e8536 = 0x1E8536, // R2.000, x1, EventObj type
    SurgeOrb = 0x1EAF6B, // R0.500, x0 (spawn during fight), EventObj type
    WillOfRamuh = 0x2D05, // R3.800, x0 (spawn during fight)
    WillOfIxion = 0x2D06, // R3.000, x0 (spawn during fight)
    Stormcloud = 0x2D03, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_ = 19362, // 2D02->player, no cast, single-target
    CripplingBlow = 19363, // 2D02->player, 5.0s cast, single-target
    _Weaponskill_ = 19403, // 2D02->location, no cast, single-target
    _Weaponskill_StratospearSummons = 19341, // 2D02->self, 4.0s cast, single-target
    Impact = 20026, // 233C->location, 2.5s cast, range 3 circle
    _Weaponskill_JudgmentJolt = 19342, // 2D02->self, 6.0s cast, single-target
    JudgmentJolt = 19343, // 233C->self, 7.0s cast, range 22 circle
    _Weaponskill_JudgmentVolts = 19352, // 2D02->self, 6.0s cast, single-target
    JudgmentVolts = 19353, // 233C->self, 6.5s cast, range 100 circle
    _Weaponskill_FurysBolt = 19344, // 2D02->self, 3.0s cast, single-target
    _Weaponskill_DivineJudgmentVolts = 20066, // 2D02->self, 6.0s cast, single-target
    DivineJudgmentVolts = 19354, // 233C->self, 6.5s cast, range 100 circle
    _Weaponskill_TribunalSummons = 19345, // 2D02->self, 4.0s cast, single-target
    _Weaponskill_DeadlyDischarge = 19346, // 2D05->self, 4.0s cast, single-target
    DeadlyDischarge = 19347, // 233C->self, 4.5s cast, range 40 width 40 rect
    _Weaponskill_Gallop = 19350, // 2D06->self, 4.0s cast, single-target
    Gallop = 19351, // 233C->self, 4.5s cast, range 50 width 5 rect
    _Weaponskill_Thunderstorm = 19360, // 2D02->self, 5.0s cast, single-target
    ShockStrike = 19361, // 233C->location, 2.5s cast, range 3 circle
    VoltStrike = 19698, // 233C->location, 2.5s cast, range 5 circle
    _Weaponskill_StormcloudSummons = 19355, // 2D02->self, 3.0s cast, single-target
    LightningBolt = 19356, // 2D03->self, no cast, range 8 circle
}

public enum SID : uint
{
    SurgeProtection = 2228, // none->player, extra=0x1/0x2/0x3
    FurysBolt = 2231, // Ramuh->Ramuh, extra=0x1
    StaticCondensation = 2229, // none->player, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_m0372trg_t0j = 110, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_sinentai01p = 102, // 2D05/2D06->Ramuh
}

sealed class CripplingBlow(BossModule module) : Components.SingleTargetCast(module, (uint)AID.CripplingBlow);
sealed class Impact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Impact, 3f);
sealed class JudgmentJolt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.JudgmentJolt, 22f);
sealed class JudgmentVolts(BossModule module) : Components.RaidwideCast(module, (uint)AID.JudgmentVolts);
sealed class DivineJudgmentVolts(BossModule module) : Components.RaidwideCast(module, (uint)AID.DivineJudgmentVolts);
sealed class DeadlyDischarge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeadlyDischarge, new AOEShapeRect(40f, 4f))
{
    // too lazy to add proper knockback
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var aoes = ActiveAOEs(slot, actor);
        if (aoes.Length != 0)
        {
            var aoe = aoes[0];
            hints.GoalZones.Add(AIHints.GoalRectangle(aoe.Origin, aoe.Rotation.ToDirection(), 6f, 40f));
        }
    }
}
sealed class DeadlyDischargeKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.DeadlyDischarge, 10f)
{
    private Knockback[] _knockbackBuffer = [];
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (Casters.Count == 0)
        {
            return [];
        }

        var knockbackLen = 1;
        if (_knockbackBuffer.Length < knockbackLen)
            _knockbackBuffer = new Knockback[knockbackLen];
        var knockback = _knockbackBuffer;
        var left = actor.Position.X < 100f;
        ref var kb = ref Casters.Ref(0);
        knockback[0] = new(kb.Origin, kb.Distance, kb.Activation, kb.Shape, kb.Direction, left ? Kind.DirLeft : Kind.DirRight, kb.MinDistance, kb.SafeWalls, kb.ActorID, kb.IgnoreImmunes, kb.ArenaProjectionLayer, kb.RestrictToArenaProjectionLayer);
        return knockback.AsSpan(0, knockbackLen);
    }
}
sealed class Gallop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Gallop, new AOEShapeRect(50f, 2.5f));
sealed class ShockStrike(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ShockStrike, 3f);
sealed class VoltStrike(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoltStrike, 5f);
sealed class Stormcloud(BossModule module) : Components.Voidzone(module, 8f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.Stormcloud);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}
sealed class SurgeOrb(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _orbs = [];
    private BitMask _avoid = new();
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.SurgeOrb)
        {
            _orbs.Add(actor);
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008)
        {
            _orbs.Remove(actor);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.SurgeProtection or (uint)SID.StaticCondensation)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (status.ID == (uint)SID.StaticCondensation)
            {
                _avoid.Set(slot);
            }
            else
            {
                var extra = status.Extra;
                if (extra >= 2)
                {
                    _avoid.Set(slot);
                }
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.SurgeProtection or (uint)SID.StaticCondensation)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            _avoid.Clear(slot);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var orbs = CollectionsMarshal.AsSpan(_orbs);
        var count = orbs.Length;
        for (var i = 0; i < count; ++i)
        {
            ref var orb = ref orbs[i];
            var position = orb.Position;

            var avoid = _avoid[pcSlot];
            if (avoid)
            {
                Arena.ZoneCircle(position, 1f, default);
            }
            else
            {
                Arena.ZoneCircleOutline(position, 1f, Colors.Safe);
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var orbs = CollectionsMarshal.AsSpan(_orbs);
        var count = orbs.Length;

        if (count != 0)
        {
            var avoid = _avoid[slot];
            for (var i = 0; i < count; ++i)
            {
                ref var orb = ref orbs[i];
                var position = orb.Position;

                if (avoid)
                {
                    var sd = new SDCircle(position, 2f);
                    hints.ForbiddenZones.Add((sd, default, default));
                }
                else
                {
                    hints.GoalZones.Add(AIHints.GoalSingleTarget(position, 1f));
                }
            }
        }
    }
}

sealed class RamuhStates : StateMachineBuilder
{
    public RamuhStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CripplingBlow>()
            .ActivateOnEnter<SurgeOrb>()
            .ActivateOnEnter<Impact>()
            .ActivateOnEnter<JudgmentJolt>()
            .ActivateOnEnter<JudgmentVolts>()
            .ActivateOnEnter<DivineJudgmentVolts>()
            .ActivateOnEnter<DeadlyDischarge>()
            .ActivateOnEnter<Gallop>()
            .ActivateOnEnter<ShockStrike>()
            .ActivateOnEnter<VoltStrike>()
            .ActivateOnEnter<Stormcloud>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.Ramuh, Contributors = "gynorhino", Expansion = BossModuleInfo.Expansion.Shadowbringers, Category = BossModuleInfo.Category.Raid,
GroupType = BossModuleInfo.GroupType.CFC, GroupID = 715u, NameID = 9281u, SortOrder = 1)]
public sealed class Ramuh(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsRect(19.5f, 14.5f));
