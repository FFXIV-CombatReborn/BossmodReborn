namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

sealed class Aethertithe(BossModule module) : Components.GenericAOEs(module)
{
    public AOEInstance[] AOE = [];

    private static readonly AOEShapeCone _shape = new(100f, 35f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOE;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x00)
        {
            return;
        }
        Angle? dir = state switch
        {
            0x04000100u => -55f.Degrees(),
            0x08000100u => (Angle)default,
            0x10000100u => 55f.Degrees(),
            _ => null
        };
        if (dir != null)
        {
            AOE = [new(_shape, Module.PrimaryActor.Position, dir.Value, WorldState.FutureTime(5.1d))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AethertitheAOER or (uint)AID.AethertitheAOEC or (uint)AID.AethertitheAOEL)
        {
            AOE = [];
            ++NumCasts;
        }
    }
}

sealed class Retribute : Components.GenericWildCharge
{
    public Retribute(BossModule module) : base(module, 4f, (uint)AID.RetributeAOE, 60f)
    {
        Source = module.PrimaryActor;
        foreach (var (i, p) in module.Raid.WithSlot(true, true, true))
            PlayerRoles[i] = p.Role == Role.Healer ? PlayerRole.Target : PlayerRole.Share;
    }
}
