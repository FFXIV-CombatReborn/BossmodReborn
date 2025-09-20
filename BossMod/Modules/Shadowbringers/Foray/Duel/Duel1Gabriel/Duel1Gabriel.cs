namespace BossMod.Shadowbringers.Foray.Duel.Duel1Gabriel;

sealed class MagitekMissile(BossModule module) : Components.Voidzone(module, 1.3f, GetMissiles, 5f)
{
    private static List<Actor> GetMissiles(BossModule module) => module.Enemies((uint)OID.MagitekMissile);
}

sealed class MissileLauncher(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MissileLauncher, 4f);

sealed class InfraredHomingMissile(BossModule module) : Components.SimpleAOEs(module, (uint)AID.InfraredHomingMissile, 15f);
sealed class InfraredHomingMissileBait(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private static readonly AOEShapeCircle circle = new(15f);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Prey)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, circle, status.ExpireAt));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.InfraredHomingMissile)
            CurrentBaits.Clear();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Count != 0)
            hints.Add("Bait away!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentBaits.Count != 0)
            hints.AddForbiddenZone(new SDCircle(Arena.Center, 13.5f));
    }
}

sealed class DynamicSensoryJammer(BossModule module) : Components.StayMove(module, 3f)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.ExtremeCaution && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = new(Requirement.Stay, status.ExpireAt);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.ExtremeCaution && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = default;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.BozjaDuel, GroupID = 735, NameID = 4, PlanLevel = 80)]
public sealed class Gabriel(WorldState ws, Actor primary) : BossModule(ws, primary, new WPos(631f, 687f).Quantized(), new ArenaBoundsCircle(15f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 15f);
}
