namespace BossMod.Shadowbringers.Ultimate.TEA;

sealed class P2SuperJump(TEA module) : Components.GenericBaitAway(module, (uint)AID.SuperJumpAOE, centerAtTarget: true)
{
    private readonly AOEShapeCircle _shape = new(10f);

    public override void Update()
    {
        CurrentBaits.Clear();
        var source = module.BruteJustice();
        var target = source != null ? Raid.WithoutSlot(false, true, true).FarthestFrom(source.Position) : null;
        if (source != null && target != null)
            CurrentBaits.Add(new(source, target, _shape));
    }
}
