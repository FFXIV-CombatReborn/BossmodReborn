namespace BossMod.Endwalker.Savage.P9SKokytos;

class DualityOfDeath(BossModule module) : Components.GenericBaitAway(module, (uint)AID.DualityOfDeathFire, centerAtTarget: true)
{
    private ulong _firstFireTarget;

    private static readonly AOEShapeCircle _shape = new(6);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveBaitsOn(actor).Count != 0)
        {
            if (Raid.WithoutSlot(false, true, true).InRadiusExcluding(actor, _shape.Radius).Any())
                hints.Add("GTFO from raid!");
            if (Module.PrimaryActor.TargetID == _firstFireTarget)
                hints.Add(actor.InstanceID != _firstFireTarget ? "Taunt!" : "Pass aggro!");
        }
        else if (ActiveBaits.Any(b => IsClippedBy(actor, ref b)))
        {
            hints.Add("GTFO from tanks!");
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.DualityOfDeath)
        {
            CurrentBaits.Add(new(Module.PrimaryActor, actor, _shape));
            _firstFireTarget = Module.PrimaryActor.TargetID;
        }
    }
}
