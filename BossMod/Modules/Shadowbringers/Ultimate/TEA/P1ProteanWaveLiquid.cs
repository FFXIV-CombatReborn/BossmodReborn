namespace BossMod.Shadowbringers.Ultimate.TEA;

sealed class P1ProteanWaveLiquid(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ProteanWaveLiquidVisBoss, (uint)AID.ProteanWaveLiquidVisHelper], new AOEShapeCone(40f, 15f.Degrees()));

// single protean ("shadow") that fires in the direction the boss is facing
sealed class P1ProteanWaveLiquidInvisFixed(BossModule module) : Components.GenericAOEs(module, (uint)AID.ProteanWaveLiquidInvisBoss)
{
    private readonly AOEShapeCone cone = new(40f, 15f.Degrees());

    private readonly AOEInstance[] _oneAOEInstance = new AOEInstance[1];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        {
            _oneAOEInstance[0] = new(cone, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation);
            return _oneAOEInstance;
        }
    }
}

// proteans baited on 4 closest targets
sealed class P1ProteanWaveLiquidInvisBaited(BossModule module) : Components.GenericBaitAway(module, (uint)AID.ProteanWaveLiquidInvisHelper)
{
    private readonly AOEShapeCone cone = new(40f, 15f.Degrees());

    public override void Update()
    {
        CurrentBaits.Clear();
        foreach (var target in Raid.WithoutSlot(false, true, true).SortedByRange(Module.PrimaryActor.Position).Take(4))
            CurrentBaits.Add(new(Module.PrimaryActor, target, cone));
    }
}
