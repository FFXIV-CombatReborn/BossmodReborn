namespace BossMod.Endwalker.Unreal.Un2Sephirot;

sealed class P3Earthshaker(BossModule module) : Components.GenericAOEs(module, (uint)AID.EarthShakerAOE)
{
    private BitMask _targets;

    public bool Active => _targets.Any() && NumCasts < 2;

    private readonly AOEShape _shape = new AOEShapeCone(60f, 15f.Degrees());

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var origin = Module.Enemies((uint)OID.BossP3).FirstOrDefault();
        if (origin == null)
            return [];

        // TODO: timing...
        var aoes = _aoesBuffer;
        aoes.Clear();
        foreach (var target in Raid.WithSlot(true, true, true).IncludedInMask(_targets))
            aoes.Add(new(_shape, origin.Position, Angle.FromDirection(target.Item2.Position - origin.Position)));
        return CollectionsMarshal.AsSpan(aoes);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _targets[playerSlot] ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Earthshaker)
            _targets[Raid.FindSlot(actor.InstanceID)] = true;
    }
}
