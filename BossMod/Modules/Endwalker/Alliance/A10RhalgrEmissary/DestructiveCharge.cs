namespace BossMod.Endwalker.Alliance.A10RhalgrEmissary;

class DestructiveCharge(BossModule module) : Components.GenericAOEs(module)
{
    public List<AOEInstance> AOEs = new(2);

    private static readonly AOEShapeCone _shape = new(25f, 45f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x25)
            return;
        // 00020001 = anim start
        // 00080004 = -45/+135
        // 00100004 = +45/-135
        var dir = state switch
        {
            0x00080004u => -45f.Degrees(),
            0x00100004u => 45f.Degrees(),
            _ => default
        };
        if (dir != default)
        {
            var pos = Module.PrimaryActor.Position.Quantized();
            var act = WorldState.FutureTime(16.1d);
            AOEs.Add(new(_shape, pos, dir, act));
            AOEs.Add(new(_shape, pos, dir + 180f.Degrees(), act));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.DestructiveChargeAOE)
        {
            ++NumCasts;
            AOEs.Clear();
        }
    }
}
