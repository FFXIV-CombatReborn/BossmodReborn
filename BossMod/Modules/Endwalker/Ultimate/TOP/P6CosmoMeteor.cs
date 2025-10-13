namespace BossMod.Endwalker.Ultimate.TOP;

sealed class P6CosmoMeteorPuddles(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CosmoMeteorAOE, 10f);

sealed class P6CosmoMeteorAddComet(BossModule module) : Components.Adds(module, (uint)OID.CosmoComet);

sealed class P6CosmoMeteorAddMeteor(BossModule module) : Components.Adds(module, (uint)OID.CosmoMeteor);

sealed class P6CosmoMeteorSpread : Components.UniformStackSpread
{
    public int NumCasts;

    public P6CosmoMeteorSpread(BossModule module) : base(module, default, 5f)
    {
        AddSpreads(Raid.WithoutSlot(true, true, true));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.CosmoMeteorSpread)
            ++NumCasts;
    }
}

sealed class P6CosmoMeteorFlares(BossModule module) : Components.UniformStackSpread(module, 6f, 20f, 5) // TODO: verify flare falloff
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.OptimizedMeteor)
        {
            AddSpread(actor, WorldState.FutureTime(8.1d));
            if (Spreads.Count == 3)
            {
                // TODO: how is the stack target selected?
                var stackTarget = Raid.WithoutSlot(false, true, true).FirstOrDefault(p => !IsSpreadTarget(p));
                if (stackTarget != null)
                    AddStack(stackTarget, WorldState.FutureTime(8.1f));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.CosmoMeteorStack or (uint)AID.CosmoMeteorFlare)
        {
            Spreads.Clear();
            Stacks.Clear();
        }
    }
}
