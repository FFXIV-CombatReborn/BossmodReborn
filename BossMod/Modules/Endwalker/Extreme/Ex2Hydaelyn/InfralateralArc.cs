namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

// component for infralateral arc mechanic (role stacks)
sealed class InfralateralArc(BossModule module) : Components.CastCounter(module, (uint)AID.InfralateralArcAOE)
{
    private readonly Angle _coneHalfAngle = 45f.Degrees();

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var pcRole = EffectiveRole(actor);
        var boss = Module.PrimaryActor.Position;
        var pcDir = Angle.FromDirection(actor.Position - boss);
        var party = Raid.WithoutSlot(false, true, true);
        for (var i = 0; i < party.Length; ++i)
        {
            var a = party[i];
            if (EffectiveRole(a) != pcRole && a.Position.InCone(boss, pcDir, _coneHalfAngle))
            {
                hints.Add("Spread by roles!");
                return;
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var pcRole = EffectiveRole(pc);
        var pcDir = Angle.FromDirection(pc.Position - Module.PrimaryActor.Position);
        var party = Raid.WithoutSlot(false, true, true);
        for (var i = 0; i < party.Length; ++i)
        {
            var actor = party[i];
            if (EffectiveRole(actor) == pcRole)
            {
                continue;
            }
            var isincone = actor.Position.InCone(Module.PrimaryActor.Position, pcDir, _coneHalfAngle);
            Arena.Actor(actor, isincone ? Colors.Danger : Colors.PlayerGeneric, drawWorld: isincone ? true : null);
        }
    }

    private static Role EffectiveRole(Actor a) => a.Role == Role.Ranged ? Role.Melee : a.Role;
}
