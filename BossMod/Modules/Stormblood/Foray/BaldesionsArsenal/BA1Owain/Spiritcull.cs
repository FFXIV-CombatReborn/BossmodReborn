namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA1Owain;

class PiercingLight1(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.PiercingLight1), 6);
class PiercingLight2(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.PiercingLight2), 6);

class Spiritcull(BossModule module) : Components.GenericStackSpread(module)
{
    private readonly List<Actor> targets = new(6);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.DoritoStack)
        {
            targets.Add(actor);
            if (Stacks.Count == 0)
                Stacks.Add(new(actor, 1.5f, 24, 24, activation: WorldState.FutureTime(5.1f)));
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.BloodSacrifice)
        {
            targets.Clear();
            Stacks.Clear();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (targets.Contains(actor))
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (targets.Contains(actor))
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Stacks.Count != 0 && targets.Contains(pc))
        {

            Actor? actor = null;
            var minDistanceSq = float.MaxValue;

            for (var i = 0; i < targets.Count; ++i)
            {
                var target = targets[i];
                if (target == pc)
                    continue;
                var distanceSq = (pc.Position - target.Position).LengthSq();
                if (distanceSq < minDistanceSq)
                {
                    minDistanceSq = distanceSq;
                    actor = target;
                }
            }
            Stacks[0] = Stacks[0] with { Target = actor ?? pc };

            base.DrawArenaForeground(pcSlot, pc);
        }
    }
}