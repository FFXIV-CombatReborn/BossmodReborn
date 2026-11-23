namespace BossMod.Endwalker.Savage.P10SPandaemonium;

sealed class HarrowingHell(BossModule module) : BossComponent(module)
{
    public int NumCasts;
    private BitMask _closestTargets;

    public override void Update()
    {
        // boss always points to (0,1) => offset dot dir == z + const
        // the two players with the lowest z
        _closestTargets = default;
        int first = -1, second = -1;
        float z1 = float.MaxValue, z2 = float.MaxValue;
        foreach (var (i, p) in Raid.WithSlot(false, true, true))
        {
            var z = p.PosRot.Z;
            if (z < z1)
            {
                (second, z2) = (first, z1);
                (first, z1) = (i, z);
            }
            else if (z < z2)
            {
                (second, z2) = (i, z);
            }
        }
        if (first >= 0)
            _closestTargets.Set(first);
        if (second >= 0)
            _closestTargets.Set(second);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var soaking = _closestTargets[slot];
        var shouldSoak = actor.Role == Role.Tank;
        if (soaking != shouldSoak)
            hints.Add(shouldSoak ? "Stay in front of the raid!" : "Go behind tanks!");
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.HarrowingHellAOE1 or (uint)AID.HarrowingHellAOE2 or (uint)AID.HarrowingHellAOE3 or (uint)AID.HarrowingHellAOE4 or (uint)AID.HarrowingHellAOE5 or (uint)AID.HarrowingHellAOE6 or (uint)AID.HarrowingHellAOE7 or (uint)AID.HarrowingHellAOE8 or (uint)AID.HarrowingHellKnockback)
            ++NumCasts;
    }
}
