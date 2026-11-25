using BossMod.Dawntrail.Alliance.A12Fafnir;

namespace BossMod.Shadowbringers.Foray.Duel.Duel5Menenius;

sealed class SpiralScourge(BossModule module) : Components.SingleTargetCast(module, (uint)AID.SpiralScourge, "Use Manawall, Excellence, or Invuln.");
sealed class CallousCrossfire(BossModule module) : Components.SingleTargetCast(module, (uint)AID.CallousCrossfire, "Use Light Curtain / Reflect.");

sealed class ReactiveMunition(BossModule module) : Components.StayMove(module)
{
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.AccelerationBomb)
        {
            if (Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
                PlayerStates[slot] = new(Requirement.Stay, status.ExpireAt);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.AccelerationBomb)
        {
            if (Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
                PlayerStates[slot] = default;
        }
    }
}

sealed class SenseWeakness(BossModule module) : Components.StayMove(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SenseWeakness)
        {
            if (Raid.FindSlot(caster.TargetID) is var slot && slot >= 0)
                PlayerStates[slot] = new(Requirement.Move, Module.CastFinishAt(spell));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SenseWeakness)
        {
            if (Raid.FindSlot(caster.TargetID) is var slot && slot >= 0)
                PlayerStates[slot] = default;
        }
    }
}

sealed class MagitekImpetus(BossModule module) : Components.StatusDrivenForcedMarch(module, 3f, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace, activationLimit: 1);
sealed class ProactiveMunition(BossModule module) : Components.StandardChasingAOEs(module, 6f, (uint)AID.ProactiveMunitionTrackingStart, (uint)AID.ProactiveMunitionTrackingMove, 6, 1d, 5, true, (uint)IconID.ProactiveMunition);

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "SourP", GroupType = BossModuleInfo.GroupType.BozjaDuel, GroupID = 778, NameID = 23)] // bnpcname=9695
public sealed class Duel5Menenius(WorldState ws, Actor primary) : BossModule(ws, primary, new(-810f, 520f), new ArenaBoundsSquare(20f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player() is Actor player && player.Position.InSquare(Arena.Center, 20f);
}
