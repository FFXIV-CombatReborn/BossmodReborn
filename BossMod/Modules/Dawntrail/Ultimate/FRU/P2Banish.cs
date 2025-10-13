namespace BossMod.Dawntrail.Ultimate.FRU;

abstract class P2Banish(BossModule module) : Components.UniformStackSpread(module, 5f, 5f, 2, 2)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BanishStack:
                // TODO: this can target either supports or dd
                AddStacks(Raid.WithoutSlot(true, true, true).Where(p => p.Class.IsSupport()), Module.CastFinishAt(spell, 0.1d));
                break;
            case (uint)AID.BanishSpread:
                AddSpreads(Raid.WithoutSlot(true, true, true), Module.CastFinishAt(spell, 0.1d));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BanishStackAOE:
                Stacks.Clear();
                break;
            case (uint)AID.BanishSpreadAOE:
                Spreads.Clear();
                break;
        }
    }
}
