namespace BossMod.Endwalker.Savage.P9SKokytos;

sealed class DualityOfDeath(BossModule module) : Components.GenericBaitAway(module, (uint)AID.DualityOfDeathFire, centerAtTarget: true)
{
    private ulong _firstFireTarget;

    private static readonly AOEShapeCircle _shape = new(6);

    private bool ClippedByAny(Actor actor)
    {
        var baits = ActiveBaits;
        for (var i = 0; i < baits.Count; ++i)
        {
            var b = baits[i];
            if (IsClippedBy(actor, ref b))
                return true;
        }
        return false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (IsBaitTarget(actor))
        {
            if (Raid.WithoutSlot(false, true, true).AnyInRadiusExcluding(actor, _shape.Radius))
                hints.Add("GTFO from raid!");
            if (Module.PrimaryActor.TargetID == _firstFireTarget)
                hints.Add(actor.InstanceID != _firstFireTarget ? "Taunt!" : "Pass aggro!");
        }
        else if (ClippedByAny(actor))
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
