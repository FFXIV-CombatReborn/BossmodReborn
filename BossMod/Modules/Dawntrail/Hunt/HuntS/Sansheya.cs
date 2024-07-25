﻿namespace BossMod.Dawntrail.Hunt.RankS.Sansheya;

public enum OID : uint
{
    Boss = 0x43DD, // R4.0
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/4379->player, no cast, single-target
    CullingBlade = 39295, // Boss->self, 4.0s cast, range 80 circle
    PyreOfRebirth = 39288, // Boss->self, 4.0s cast, range 32 circle, status effect boiling, turns into pyretic
    FiresDomain = 39285, // Boss->players, 5.0s cast, width 6 rect charge
    TwinscorchedVeil1 = 39292, // Boss->self, 5.0s cast, range 40 180-degree cone, visual, right then left then circle
    TwinscorchedVeil2 = 39290, // Boss->self, 5.0s cast, range 40 180-degree cone, visual left then right then circle
    TwinscorchedHalo1 = 39289, // Boss->self, 5.0s cast, range 40 180-degree cone, visual, left then right then donut
    TwinscorchedHalo2 = 39291, // Boss->self, 5.0s cast, range 40 180-degree cone, visual, right then left then donut
    ScorchingLeft1 = 39528, // Boss->self, 1.0s cast, range 40 180-degree cone
    ScorchingLeft2 = 39557, // Boss->self, no cast, range 40 180-degree cone
    ScorchingRight1 = 39558, // Boss->self, no cast, range 40 180-degree cone
    ScorchingRight2 = 39529, // Boss->self, 1.0s cast, range 40 180-degree cone
    VeilOfHeat = 39293, // Boss->self, 1.0s cast, range 15 circle
    CaptiveBolt = 39296, // Boss->players, 5.0s cast, range 6 circle, stack
    HaloOfHeat = 39294, // Boss->self, 1.0s cast, range 10-40 donut
}

public enum SID : uint
{
    Boiling = 4140, // Boss->player, extra=0x0
    Pyretic = 960, // none->player, extra=0x0
}

class Boiling(BossModule module) : Components.StayMove(module)
{
    private bool pausedAI;
    private DateTime expiresAt;
    private bool expired;
    private BitMask _pyretic;
    private BitMask _boiling;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < Requirements.Length)
        {
            if ((SID)status.ID == SID.Boiling)
            {
                Requirements[slot] = Requirement.Stay;
                _boiling.Set(Raid.FindSlot(actor.InstanceID));
                if (actor == Module.Raid.Player()!)
                    expiresAt = status.ExpireAt;
            }
            else if ((SID)status.ID == SID.Pyretic)
            {
                _pyretic.Set(Raid.FindSlot(actor.InstanceID));
                Requirements[slot] = Requirement.Stay;
            }
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < Requirements.Length)
        {
            if ((SID)status.ID == SID.Boiling)
            {
                _boiling.Clear(Raid.FindSlot(actor.InstanceID));
                Requirements[slot] = Requirement.None;
            }
            else if ((SID)status.ID == SID.Pyretic)
            {
                Requirements[slot] = Requirement.None;
                expired = true;
                expiresAt = default;
                _pyretic.Clear(Raid.FindSlot(actor.InstanceID));
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_boiling[slot])
            hints.Add($"Boiling on you in {(actor.FindStatus(SID.Boiling)!.Value.ExpireAt - Module.WorldState.CurrentTime).TotalSeconds:f1}s. (Pyretic!)");
        else if (_pyretic[slot])
            hints.Add("Pyretic on you! STOP everything!");
    }

    public override void Update()
    {
        if (expiresAt != default && AI.AIManager.Instance?.Beh != null && expiresAt.AddSeconds(-0.1f) <= Module.WorldState.CurrentTime)
        {
            AI.AIManager.Instance?.SwitchToIdle();
            pausedAI = true;
        }
        else if (expired && pausedAI)
        {
            AI.AIManager.Instance?.SwitchToFollow(Service.Config.Get<AI.AIConfig>().FollowSlot);
            pausedAI = false;
            expired = false;
        }
    }
}

class TwinscorchedHaloVeil(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(40, 90.Degrees());
    private static readonly AOEShapeDonut donut = new(10, 40);
    private static readonly AOEShapeCircle circle = new(15);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            yield return _aoes[0] with { Color = ArenaColor.Danger };
        if (_aoes.Count > 1)
            yield return _aoes[1] with { Risky = false };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TwinscorchedVeil1:
            case AID.TwinscorchedVeil2:
                AddAOEs(circle, spell);
                break;
            case AID.TwinscorchedHalo1:
            case AID.TwinscorchedHalo2:
                AddAOEs(donut, spell);
                break;
        }
    }

    private void AddAOEs(AOEShape secondaryShape, ActorCastInfo spell)
    {
        var position = Module.PrimaryActor.Position;
        _aoes.Add(new(cone, position, spell.Rotation, Module.CastFinishAt(spell)));
        _aoes.Add(new(cone, position, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell, 2.3f)));
        _aoes.Add(new(secondaryShape, position, default, Module.CastFinishAt(spell, 4.5f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count == 0)
            return;
        switch ((AID)spell.Action.ID)
        {
            case AID.HaloOfHeat:
            case AID.VeilOfHeat:
            case AID.ScorchingLeft1:
            case AID.ScorchingLeft2:
            case AID.ScorchingRight1:
            case AID.ScorchingRight2:
                _aoes.RemoveAt(0);
                break;
        }
    }
}

class FiresDomain(BossModule module) : Components.BaitAwayChargeCast(module, ActionID.MakeSpell(AID.FiresDomain), 3);
class CaptiveBolt(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.CaptiveBolt), 6, 8);
class PyreOfRebirth(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.PyreOfRebirth));
class CullingBlade(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.CullingBlade));

class SansheyaStates : StateMachineBuilder
{
    public SansheyaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TwinscorchedHaloVeil>()
            .ActivateOnEnter<FiresDomain>()
            .ActivateOnEnter<PyreOfRebirth>()
            .ActivateOnEnter<CullingBlade>()
            .ActivateOnEnter<CaptiveBolt>()
            .ActivateOnEnter<Boiling>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 11399)]
public class Sansheya(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);