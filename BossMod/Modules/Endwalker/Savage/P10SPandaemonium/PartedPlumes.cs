namespace BossMod.Endwalker.Savage.P10SPandaemonium;

sealed class PartedPlumes : Components.SimpleAOEs
{
    public PartedPlumes(BossModule module) : base(module, (uint)AID.PartedPlumesAOE, new AOEShapeCone(50f, 10f.Degrees()), 16) { MaxDangerColor = 2; }
}

sealed class PartedPlumesVoidzone(BossModule module) : Components.GenericAOEs(module, default, "GTFO from voidzone!")
{
    private readonly AOEShapeCircle _shape = new(8f);

    private readonly AOEInstance[] _oneAOEInstance = new AOEInstance[1];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _oneAOEInstance[0] = new(_shape, new WPos(100f, 100f));
        return _oneAOEInstance;
    }
}
