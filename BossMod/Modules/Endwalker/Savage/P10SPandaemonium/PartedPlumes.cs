namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class PartedPlumes : Components.SimpleAOEs
{
    public PartedPlumes(BossModule module) : base(module, (uint)AID.PartedPlumes, new AOEShapeCone(50f, 10f.Degrees()), 16) { MaxDangerColor = 2; }
}

class PartedPlumesVoidzone(BossModule module) : Components.GenericAOEs(module, default, "GTFO from voidzone!")
{
    private readonly AOEShapeCircle _shape = new(8f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return new AOEInstance[1] { new(_shape, new WPos(100f, 100f)) };
    }
}
