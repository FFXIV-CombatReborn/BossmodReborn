namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

sealed class Footprint(BossModule module) : Components.GenericKnockback(module, (uint)AID.Footprint)
{
    private readonly Knockback[] _oneKnockback = new Knockback[1];
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        {
            _oneKnockback[0] = new(Module.PrimaryActor.Position, 20f); // TODO: activation
            return _oneKnockback;
        }
    }
}
