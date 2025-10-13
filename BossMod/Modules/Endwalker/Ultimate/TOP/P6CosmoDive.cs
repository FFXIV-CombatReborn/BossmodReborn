namespace BossMod.Endwalker.Ultimate.TOP;

sealed class P6CosmoDive(BossModule module) : Components.UniformStackSpread(module, 6f, 8f, 6, 6)
{
    private Actor? _source;
    private DateTime _activation;

    public override void Update()
    {
        Spreads.Clear();
        Stacks.Clear();
        if (_source != null)
        {
            BitMask forbidden = default;
            foreach (var (slot, actor) in Raid.WithSlot(false, true, true).SortedByRange(_source.Position).Take(2))
            {
                AddSpread(actor, _activation);
                forbidden.Set(slot);
            }
            var farthest = Raid.WithoutSlot(false, true, true).Farthest(_source.Position);
            if (farthest != null)
            {
                AddStack(farthest, _activation, forbidden);
            }
        }
        base.Update();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CosmoDive)
        {
            _source = caster;
            _activation = Module.CastFinishAt(spell, 2.5d);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.CosmoDiveTankbuster or (uint)AID.CosmoDiveStack)
        {
            _source = null;
            Spreads.Clear();
            Stacks.Clear();
        }
    }
}
