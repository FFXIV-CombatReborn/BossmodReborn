namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

sealed class NorthernCross(BossModule module) : Components.GenericAOEs(module)
{
    public AOEInstance[] AOE = [];
    private ChillingCataclysm? _aoe;
    private static readonly AOEShapeRect _shape = new(25f, 30f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoe ??= Module.FindComponent<ChillingCataclysm>(); // prevent NotherCross from hiding the safespot
        return _aoe?.AOEs.Count == 0 ? AOE : [];
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x03)
            return;
        var offset = state switch
        {
            0x00200010u => -90f.Degrees(),
            0x00020001u => 90f.Degrees(),
            _ => default
        };
        if (offset != default)
        {
            AOE = [new(_shape, Arena.Center, -126.875f.Degrees() + offset, WorldState.FutureTime(9.2d))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NorthernCrossL or (uint)AID.NorthernCrossR)
        {
            ++NumCasts;
            AOE = [];
        }
    }
}
